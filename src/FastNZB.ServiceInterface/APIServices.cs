using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Data;
using System.Text;    
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Data;
using ServiceStack.Configuration;
using FastNZB.ServiceModel.Types;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Serialization;
using FastNZB.ServiceModel.Rss;
using MySql.Data.MySqlClient;

using FastNZB.ServiceModel;

/// <summary>
/// 
/// </summary>
namespace FastNZB.ServiceInterface
{ 
    [ConnectionInfo(NamedConnection="ReadOnly")]
    public class APIService : Service {

        public IDbConnectionFactory ConnectionFactory { get; set; }                      
        private TextFileSettings _settings { get; set; }

        public object Any(FastNZB.ServiceModel.APIRequest req) {
                        
            if (req.o == "json")
                throw new HttpError(500, "JSON Not supported");
            
            if (req.t != "g" && req.t != "get")
                base.Request.ResponseContentType = MimeTypes.Xml;

            var function = "";
            IEnumerable<NZBResult> results = new List<NZBResult>();
            int total = 0;

            switch (req.t) {
                case "caps":
                    function = "c";
                    break;
                case "g":
                case "get":
                    function = "g";
                    break;
                case "s":
                case "search":
                    function = "s";
                    break;                
                case "tv":
                case "tvsearch":
                    function = "tv";
                    break;
                case "m":
                case "movie":
                    function = "m";
                    break;
                default:
                    throw new HttpError(202, "No such function");
            }
            
            if (function != "c" && !Db.Exists<APIKey>(q=>q.Key == req.apikey))
                return new HttpError(101, "API Key not present or invalid");
            
            // api rate limit to 60/minute
            APIKey key = null;
            if (function != "c") {
                key = Db.Single<APIKey>(q=>q.Key == req.apikey);
                var cacheKey = String.Format("apikey-rate-{0}", key.Id);                                
                var requests = Cache.Get<int>(cacheKey);
                if (requests == 0 || DateTime.Now > Cache.Get<DateTime>(cacheKey+"-expires")) {
                    requests = 1;
                    Cache.Set<int>(cacheKey, requests);
                    Cache.Set<DateTime>(cacheKey+"-expires", DateTime.Now.AddMinutes(1));
                }                
                else {
                    Cache.Set<int>(cacheKey, requests + 1);
                }

                if (requests > 60)
                    return new HttpError(500, "Request limit reached, please try again in one minute");
            }

            // deter scrapers
            if (req.offset > 1000) req.offset = 0;

            // TODO, store these caps values somewhere else
            if (function == "c") {

                var caps = new Caps() {
                    Server = new Server() {
                        Title = "fastNZB",
                        Email = "admin@fastnzb.com",
                        Image = "https://fastnzb.com/assets/img/nzb.png",
                        Appversion = "1.0.0",
                        Version = "0.1",
                        Strapline = "A great usenet indexer",
                        Meta = "usenet,nzbs,cms,community",
                    },
                    Limits = new Limits() {
                        Max = "100",
                        Default = "100"
                    },
                    Registration = new Registration() {
                        Open = "yes",
                        Available = "yes"
                    },
                    Searching = new Searching() {
                        Search = new Search() { Available = "yes", SupportedParams = "q" },
                        Tvsearch = new Tvsearch() {
                            Available = "yes",
                            SupportedParams = "q,vid,tvdbid,traktid,rid,tvmazeid,imdbid,tmdbid,season,ep",
                        },
                        Moviesearch = new Moviesearch() { Available = "yes", SupportedParams = "q,imdbid" },
                        Audiosearch = new Audiosearch() { Available = "no", SupportedParams = "" },
                    },
                    Categories = new Categories() {
                        Category = new List<FastNZB.ServiceModel.Category>() {
                            new FastNZB.ServiceModel.Category() {
                                Id = "2000",
                                Name = "Movies",
                                Subcat = new List<Subcat>() {
                                    new Subcat() { Id = "2050", Name = "3D" },
                                    new Subcat() { Id = "2060", Name = "BluRay" },
                                    new Subcat() { Id = "2070", Name = "DVD" },
                                    new Subcat() { Id = "2010", Name = "Foreign" },
                                    new Subcat() { Id = "2040", Name = "HD" },
                                    new Subcat() { Id = "2999", Name = "Other" },
                                    new Subcat() { Id = "2030", Name = "SD" },
                                    new Subcat() { Id = "2045", Name = "UHD" },
                                    new Subcat() { Id = "2080", Name = "WEBDL" },
                                }
                            },
                            new FastNZB.ServiceModel.Category() {
                                Id = "5000",
                                Name = "TV",
                                Subcat = new List<Subcat>() {
                                    new Subcat() { Id = "5080", Name = "Documentary" },
                                    new Subcat() { Id = "5040", Name = "HD" },
                                    new Subcat() { Id = "5030", Name = "SD" },
                                    new Subcat() { Id = "5010", Name = "WEB-DL" },
                                }
                            }
                        },
                    }
                };
                
                var ser = new XmlSerializer(typeof(Caps));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                using (StringWriter textWriter = new Utf8StringWriter()) {
                    ser.Serialize(textWriter, caps, ns);

                    return new HttpResult(textWriter.ToString(), "text/xml;charset=UTF-8");
                }

            }
            else if (function == "s") /* ------------------------------------------------------- */
            {

                var catsrch = GetCatSearch(Db, req.cat);
                
                var whereSql = String.Format(String.Concat(
                    "WHERE 1=1 ",
                    "{0} {1} {2} {3} "),
                    !String.IsNullOrEmpty(req.q) ? GetSphinxSearch(req.q) : "",
                    catsrch,
                    req.maxage > 0 ? String.Format("AND r.PostDate > NOW() - INTERVAL {0} DAY", req.maxage) : "",
                    req.minsize > 0 ? String.Format("AND r.Size >= {0}'", req.minsize) : ""
                );

                var baseSql = String.Format(String.Concat(
                    "SELECT r.*, ",
                        "concat(cp.title, ' > ', c.title) AS category_name, ",                            
                        "g.name AS group_name ",                        
                    "FROM NZB r ",
                    String.IsNullOrEmpty(req.q) ? "" :  "INNER JOIN NZBSearch rse ON rse.id = r.Id ",
                    "LEFT JOIN `Group` g ON g.id = r.GroupId ",
                    "LEFT JOIN Category c ON c.id = r.CategoryId ",                        
                    "LEFT JOIN Category cp ON cp.id = c.parentid "
                ));


                if (!String.IsNullOrEmpty(req.q) && (req.q.Contains("blade") || req.q.Contains("sniper")))
                    whereSql += "AND 0=1 ";

                var sql = String.Format(String.Concat(
                    "{0} ",
                    "{1} ORDER BY r.PostDate DESC LIMIT {2} OFFSET {3} "
                ), baseSql, whereSql, req.limit > 0 ? (req.limit > 100 ? 100 : req.limit) : 100, req.offset);                    

                if (String.IsNullOrEmpty(req.q))
                {
                    total = Db.Scalar<int>(String.Format(String.Concat(
                        "SELECT SUM(Count) ",
                        "FROM Count r WHERE 1=1 ",
                        "{0} "), catsrch));
                }
                else
                    total = Db.Scalar<int>(String.Format(String.Concat(
                        "SELECT count(*) ",
                        "FROM NZB r INNER JOIN NZBSearch rse ON rse.id = r.Id ",
                        "{0} "), whereSql));

                results = Db.Select<NZBResult>(sql);                                    
        
            }
            else if (function == "m") /* ------------------------------------------------------- */
            {                
                int imdbId = -1;

                // don't return anything if it's an invalid id
                if (!string.IsNullOrEmpty(req.imdbid) && !int.TryParse(req.imdbid, out imdbId))
                    return getResults(results, total, req.offset, key, req);

                string catsrch = GetCatSearch(Db, req.cat);

                var whereSql = String.Format(String.Concat(
                    "WHERE 1=1 ",
                    "{0} {1} {2} {3} {4} "),
                    !String.IsNullOrEmpty(req.q) ? GetSphinxSearch(req.q) : "",
                    (imdbId != -1 ? String.Format(" AND imdbid = {0} ", imdbId.ToString().PadLeft(7, '0')) : ""),
                    catsrch,
                    req.maxage > 0 ? String.Format("AND r.PostDate > NOW() - INTERVAL {0} DAY", req.maxage) : "",
                    req.minsize > 0 ? String.Format("AND r.Size >= {0}'", req.minsize) : ""
                );

                var baseSql = String.Format(String.Concat(
                    "SELECT r.*, ",
                        "concat(cp.title, ' > ', c.title) AS category_name, ",                            
                        "g.name AS group_name ",                        
                    "FROM NZB r ",
                    String.IsNullOrEmpty(req.q) ? "" : "INNER JOIN NZBSearch rse ON rse.id = r.Id ",
                    "LEFT JOIN `Group` g ON g.id = r.GroupId ",
                    "LEFT JOIN Category c ON c.id = r.CategoryId ",                        
                    "LEFT JOIN Category cp ON cp.id = c.parentid "
                ));

                var sql = String.Format(String.Concat(
                    "{0} ",
                    "{1} ORDER BY r.PostDate DESC LIMIT {2} OFFSET {3} "
                ), baseSql, whereSql, req.limit > 0 ? (req.limit > 100 ? 100 : req.limit) : 100, req.offset);

                if (String.IsNullOrEmpty(req.q) && imdbId == -1)
                {
                    total = Db.Scalar<int>(String.Format(String.Concat(
                        "SELECT SUM(Count) ",
                        "FROM Count r WHERE 1=1 ",
                        "{0} "), catsrch));
                }
                else
                    total = Db.Scalar<int>(String.Format(String.Concat(
                        "SELECT count(z.id) FROM (SELECT r.id FROM NZB r INNER JOIN NZBSearch rse ON rse.id = r.Id {0} LIMIT 125000) z "), whereSql));

                results = Db.Select<NZBResult>(sql);                                
            }
            else if (function == "tv") /* ------------------------------------------------------- */
            {
            
                var airdate = "";
                var series = "";
                var episode = "";
                // Process season only queries or Season and Episode/Airdate queries
                if (!String.IsNullOrEmpty(req.season) && !String.IsNullOrEmpty(req.ep))
                {

                    if (Regex.IsMatch(req.season, @"(19|20)\d{2}") && req.ep.Contains(@"/"))
                    {
                        var regex = new Regex(@"(19|20)\d{2}").Match(req.season);
                        airdate = (regex.Groups[0] + "-" + req.ep).Replace('/', '-');//str_replace('/', '-', $year[0] . '-' . $_GET['ep']);
                    }
                    else
                    {
                        series = req.season;
                        episode = req.ep;
                    }
                }
                else if (!String.IsNullOrEmpty(req.season))
                {
                    series = req.season;
                    episode = (!String.IsNullOrEmpty(req.ep) ? req.ep : "");
                }


                var query = "";
                var siteSQL = new List<string>();
                var showSql = "";
                bool validShowIds = true;

                var siteIdArr = new Dictionary<string, string>() {
                    {"id", !String.IsNullOrEmpty(req.vid) ? req.vid : "0" },
                    {"tvdb", !String.IsNullOrEmpty(req.tvdbid) ? req.tvdbid : "0" },
                    {"trakt", !String.IsNullOrEmpty(req.traktid) ? req.traktid : "0" },
                    {"tvrage", !String.IsNullOrEmpty(req.rid) ? req.rid : "0" },
                    {"tvmaze", !String.IsNullOrEmpty(req.tvmazeid) ? req.tvmazeid : "0" },
                    {"imdb", !String.IsNullOrEmpty(req.imdbid) ? req.imdbid : "0" },
                    {"tmdb", !String.IsNullOrEmpty(req.tmdbid) ? req.tmdbid : "0" }
                };

                foreach (var val in siteIdArr)
                {
                    int id = 0;

                    if (!int.TryParse(val.Value, out id))
                        validShowIds = false;

                    if (id > 0)
                        siteSQL.Add(String.Format("{0} = {1} ", val.Key, id));
                    //siteSQL += String.Format("OR {0} = {1} ", val.Key, id);
                }

                if (siteSQL.Count > 0)
                {

                    int episodeInt = 0;
                    int seriesInt = 0;
                    int.TryParse(new Regex(@"^s0*", RegexOptions.IgnoreCase).Replace(series, ""), out seriesInt);
                    int.TryParse(new Regex(@"^e0*", RegexOptions.IgnoreCase).Replace(episode, ""), out episodeInt);

                    var showQuery = String.Format(String.Concat(
                        "SELECT ",
                        "v.id AS video, ",
                        "CAST(GROUP_CONCAT(tve.id SEPARATOR ',') AS CHAR) AS episodes ",
                        "FROM Video v ",
                        "LEFT JOIN TVEpisode tve ON v.id = tve.videos_id ",
                        "WHERE ({0}) {1} {2} {3} ",
                        "GROUP BY v.id "),
                        siteSQL.Join(" OR "),
                        !String.IsNullOrEmpty(series) ? String.Format("AND tve.series = {0}", seriesInt) : "",
                        !String.IsNullOrEmpty(episode) ? String.Format("AND tve.episode = {0}", episodeInt) : "",
                        !String.IsNullOrEmpty(airdate) ? String.Format("AND DATE(tve.firstaired) = '{0}'", esc(airdate)) : ""
                    );
                    //return new HttpResult(showQuery);
                    var show = Db.Single<ShowNZB>(showQuery);
                    if (show != null)
                    {
                        if ((!String.IsNullOrEmpty(series) || !String.IsNullOrEmpty(episode) || !String.IsNullOrEmpty(airdate)) && show.episodes.Length > 0)
                        {
                            showSql = String.Format(" AND r.TVEpisodeId IN ({0}) ", show.episodes);
                        }
                        else if (show.video > 0)
                        {
                            showSql = " AND r.VideoId = " + show.video + " ";
                            // If $series is set but episode is not, return Season Packs only
                            if (!String.IsNullOrEmpty(series) && String.IsNullOrEmpty(episode))
                            {
                                showSql += " AND r.tv_episodes_id = 0 ";
                            }
                        }
                        else
                        {
                            // If we were passed Episode Info and no match was found, do not run the query
                            return getResults(results, total, req.offset, key, req);
                        }
                    }
                    else
                    {
                        // If we were passed Site ID Info and no match was found, do not run the query
                        return getResults(results, total, req.offset, key, req);
                    }
                }
                // no valid ids found, return nil
                else if (siteSQL.Count == 0 && !validShowIds)
                {
                    return getResults(results, total, req.offset, key, req);
                }

                // If $name is set it is a fallback search, add available SxxExx/airdate info to the query
                if (!String.IsNullOrEmpty(req.q) && showSql == "")
                {
                    int seriesInt = 0;
                    int.TryParse(series, out seriesInt);
                    if (!String.IsNullOrEmpty(series) && seriesInt < 1900)
                    {
                        req.q += String.Format(" S{0}", series.PadLeft(2, '0'));
                        if (!String.IsNullOrEmpty(episode) && !episode.Contains(@"/"))
                        {
                            req.q += String.Format("E{0}", episode.PadLeft(2, '0'));
                        }
                    }
                    else if (!String.IsNullOrEmpty(airdate))
                    {
                        req.q += String.Format(" {0}",
                            airdate.Replace(@"/", " ")
                            .Replace("-", " ")
                            .Replace(".", " ")
                            .Replace("_", " "));
                    }
                }


                string catsrch = GetCatSearch(Db, req.cat);

                string whereSql = String.Format(String.Concat(
                    "WHERE 1=1 ",
                    "{0} ",
                    "{1} {2} {3} {4} "),
                    !String.IsNullOrEmpty(req.q) ? GetSphinxSearch(req.q) : "",
                    catsrch,
                    showSql,
                    req.maxage > 0 ? String.Format("AND r.PostDate > NOW() - INTERVAL {0} DAY", req.maxage) : "",
                    req.minsize > 0 ? String.Format("AND r.Size >= {0}'", req.minsize) : ""
                );

                query = String.Format(String.Concat( 
                    "SELECT r.*,",
                    "v.title, v.countries_id, v.started, v.tvdb, v.trakt,",
                    "    v.imdb, v.tmdb, v.tvmaze, v.tvrage, v.source,",                        
                    "tve.series, tve.episode, tve.se_complete, tve.title, tve.firstaired, tve.summary, ",
                    "CONCAT(cp.title, ' > ', c.title) AS category_name, ",                        
                    "Group.name AS group_name ",                                        
                "FROM NZB r ",
                String.IsNullOrEmpty(req.q) ? "" : "INNER JOIN NZBSearch rse ON rse.id = r.Id ",
                "LEFT OUTER JOIN Video v ON r.VideoId = v.id AND v.type = 0 ",                    
                "LEFT OUTER JOIN TVEpisode tve ON r.TVEpisodeId = tve.id ",
                "LEFT JOIN Category c ON c.id = r.CategoryId ",
                "LEFT JOIN Category cp ON cp.id = c.parentid ",
                "LEFT JOIN `Group` ON `Group`.id = r.GroupId ",                                        
                "{0} ORDER BY PostDate DESC LIMIT {1} OFFSET {2} "),
                whereSql, req.limit > 0 ? (req.limit > 100 ? 100 : req.limit) : 100, req.offset);
                                    
                results = Db.Select<NZBResult>(query);

                if (String.IsNullOrEmpty(req.q) && String.IsNullOrEmpty(showSql))
                {
                    total = Db.Scalar<int>(String.Format(String.Concat(
                        "SELECT SUM(Count) ",
                        "FROM Count r WHERE 1=1 ",
                        "{0} "), catsrch));
                }
                else
                    total = Db.Scalar<int>(String.Format(String.Concat(
                        "SELECT count(z.id) FROM (SELECT r.id FROM NZB r ",
                        "INNER JOIN NZBSearch rse ON rse.id = r.Id ",
                        "LEFT OUTER JOIN Video v ON r.VideoId = v.id AND v.type = 0 ",
                        "LEFT OUTER JOIN TVEpisode tve ON r.TVEpisodeId = tve.id {0} LIMIT 125000) z "
                        ), whereSql));
            

            } 
            else if (function == "g") /* ------------------------------------------------------- */
            {
                
                var nzb = Db.Single<NZB>(q=>q.Guid == req.id);
            
                using (var db = ConnectionFactory.OpenDbConnection())
                {
                    db.Insert<Download>(new Download() {
                        NZBId = nzb.Id,
                        APIKeyId = key.Id,
                        IPAddress = Request.Headers["X-Forwarded-For"],
                    });          
                }

                var nzbpath = String.Format("nzb/{0}/{1}/{2}/{3}/",
                    req.id[0],
                    req.id[1],
                    req.id[2],
                    req.id[3]);     
                var filename = String.Format("{0}.nzb.gz",req.id);
                var path = _settings.Get<string>("FSPath");

                Stream stream = null;
                
                // check if nzb is stored in cache, else get from virt files (S3)
                if (!Directory.Exists(path+nzbpath) || !File.Exists(path+filename)) {
                    var file = VirtualFiles.GetFile(nzbpath+filename);    
                    Directory.CreateDirectory(path+nzbpath);
                    File.WriteAllBytes(path+nzbpath+filename, file.ReadAllBytes());                        
                }
                stream = File.OpenRead(path+nzbpath+filename);
                
                Response.AddHeader("Content-Disposition", "attachment; filename="+nzb.Name+".nzb");

                // decompress gzip
                MemoryStream output = new MemoryStream();

                using (Stream originalFileStream = stream)
                {                                                
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(output);                        
                    }
                }
                
                return new HttpResult(output, "application/x-nzb");
            }

            return getResults(results, total, req.offset, key, req);
        }

        private HttpResult getResults(IEnumerable<NZBResult> results, long total, int offset, APIKey key, FastNZB.ServiceModel.APIRequest req)
        {
            var settings = _settings;
            var response = new Feed() {
                Title = "fastNZB",
                Link = new Uri(settings.Get<string>("BaseUrl")),   
                Total = total,
                Offset = offset        
            };

            var items = new List<Item>();            
            foreach (var nzb in results) {                
                items.Add(new Item() {
                    Title = nzb.Name,
                    Link = new Uri(nzb.GetLink(settings.Get<string>("BaseUrl"), key.Key, "1")),
                    PublishDate = nzb.Added,
                    Permalink = settings.Get<string>("BaseUrl") + "/details/" +nzb.Guid,
                    Body = nzb.Name,
                    Length = nzb.Size.ToString(),
                    Category = nzb.CategoryName,
                    CategoryId = nzb.CategoryId.ToString(),
                    Comments = new Uri(settings.Get<string>("BaseUrl")),  
                    Result = nzb == null ? new NZBResult() : nzb
                });
            }

            response.Items = items;

            using (var db = ConnectionFactory.OpenDbConnection())
            {
                key.Requests++;
                db.Save<APIKey>(key);

                var request = new FastNZB.ServiceModel.Types.APIRequest {
                    UserId = key.UserId,
                    APIKeyId = key.Id,   
                    Date = DateTime.Now,
                    results = total
                }.PopulateWith(req);

                db.Save<FastNZB.ServiceModel.Types.APIRequest>(request);
            }

            return new HttpResult(response.Serialize(), "text/xml;charset=UTF-8");
        }

        public string GetCatSearch(IDbConnection  Db, string categoryList) {
            
            if (categoryList == null) return " AND 1=1 ";
            string categoryIDs = System.Net.WebUtility.UrlDecode(categoryList);
            
            // Append Web-DL category ID if HD present for SickBeard / Sonarr compatibility.
            if (categoryIDs.Contains("5040") &&
                !categoryIDs.Contains("5010")) {
                categoryIDs += "," + "5010";
            }
            
            var cats = new List<string>(categoryIDs.Split(','));
            var categories = new List<string>();            
            foreach (var cat in cats) {
                var c = cat;
                //if (cat == "5050") c = "5999";
                var ci = 0;
                if (!int.TryParse(c, out ci))                
                    continue;                
                var category = Db.Single<FastNZB.ServiceModel.Types.Category>(q=>q.Id == ci);
                if (category == null)
                    continue;                
                if (category.IsParent()) {
                    foreach(var subCat in Db.Select<FastNZB.ServiceModel.Types.Category>(q=>q.ParentId == category.Id)) {
                        categories.Add(subCat.Id.ToString());
                    }
                } else {
                    categories.Add(category.Id.ToString());
                }
            }

            int catCount = categories.Count;
            string catsrch = "";
            switch (catCount) {
                //No category constraint
                case 0:
                    // don't return on invalid cats
                    catsrch = cats.Count > 0 ? " AND 1=0 " : " AND 1=1 ";
                    break;
                // One category constraint
                case 1:
                    catsrch = " AND r.CategoryId = " + categories[0];
                    break;
                // Multiple category constraints
                default:
                    catsrch = " AND r.CategoryId IN (" + categories.Join(",") + ") ";
                    break;
            }              
                
            return catsrch;
        }


        /// <summary>
        /// Get SQL for Sphinx search, expects rse as table alias
        /// </summary>
        /// <param name="q">Search terms</param>
        /// <returns>SQL beginning with AND</returns>
        public string GetSphinxSearch(string q)
        {            
            var r = "";            
            var words = q.Split(' ');            
            var searchWords = "";
            
            foreach (string word in words)
            {
                var w = word
                    .Trim(new char[] { '\n', '\t', '\r', '\0', '\x0B', '-', ' ' })                        
                    .Replace("'", "\\'")
                    .Replace(";", "\\;");

                if (!String.IsNullOrEmpty(w))
                    searchWords += esc(w) +" ";                    
            }

            r = String.Format("AND (rse.query = '@@relaxed @searchname {0};limit=10000;maxmatches=10000;sort=relevance;mode=extended')", searchWords);
            
            return r;
        }
        
        /// <summary>
        /// Full text saerch is disabled, always falls back to slower LIKE method
        /// You should really just use sphinx in production
        /// </summary>
        /// <param name="q">Search terms</param>
        /// <returns>SQL always beginning with AND</returns>
        public string GetFullTextSearch(string q)
        {            
            var r = "";            

            // At least 1 search term needs to be mandatory.
            var words = q.Split(' ');            
            
            var count = 0;
            foreach (string word in words)
            {
                var w = word
                    .Trim(new char[] { '\n', '\t', '\r', '\0', '\x0B', '-', ' ' })                        
                    .Replace("'", "\\'");                        

                if (w.Contains("^") && count == 0)
                    r += String.Format(" AND r.Name LIKE '{0}%' ", esc(w.Substring(1)));
                else if (w.Contains("--"))
                    r += String.Format(" AND r.Name NOT LIKE '%{0}%' ", esc(w.Substring(2)));
                else
                    r += String.Format("AND r.Name LIKE '%{0}%' ", esc(w));
                    
                count++;
            }
            
            return r;
        }

        public string esc(string str)
        {
            return MySqlHelper.EscapeString(str);
        }

    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }


}