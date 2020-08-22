using System;

using System.Data;
using System.Collections.Generic;
using System.Runtime;
using System.Linq;

using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.MySql;
using ServiceStack.DataAnnotations;
using ServiceStack.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using FastNZB.ServiceModel;
using FastNZB.ServiceModel.Types;

namespace FastNZB.Import
{
    class Program
    {
        private static TextFileSettings appSettings = new TextFileSettings("appsettings.txt");

        public static void Main(string[] args)
        {
            
            var factory = new OrmLiteConnectionFactory(appSettings.GetString("MySQL"), MySqlDialect.Provider);

            using (var db = factory.OpenDbConnection())
            {
                var count = db.SqlScalar<int>(String.Concat(
                    "select count(*) from releases r ",
                    "where ((r.videos_id != 0 and r.tv_episodes_id !=0) or(r.imdbid is not null and r.imdbid != 0)) and r.exported=0 and r.failedexport < 3 and r.haspreview=0 and r.passwordstatus <= 0;"));
                var size = 100;

                for (var i = 0; i < (count <= size ? 1 : count / size); i++)
                {
                    var sql = String.Format(String.Concat(
                        "select r.id, r.searchname, r.totalpart, r.groups_id, r.size, r.guid, v.title as tv_title, tv.se_complete, m.title as movie_title, r.videos_id, r.imdbid, r.tv_episodes_id, r.postdate, r.failedexport, r.categories_id, r.adddate from releases r ",
                        "left join videos v on v.id = r.videos_id ",
                        "left join tv_episodes tv on tv.id=r.tv_episodes_id ",
                        "left join movieinfo m on m.imdbid = r.imdbid ",
                        "where ((r.videos_id !=0 and r.tv_episodes_id !=0) or (r.imdbid is not null and r.imdbid != 0)) and r.haspreview=0 and r.passwordstatus <= 0 ",
                        "and r.exported=0 and r.failedexport < 3 ",
                        "group by r.id order by r.adddate DESC ",
                        "LIMIT {0} "), size);

                    var releases = db.Select<NZEDBRelease>(sql);
                    
                    Main(releases, db);

                    Console.WriteLine("Uploaded {0:N0} of {1:N0}", size * (i + 1), count);
                }
            }
        }

        public static void Main(List<NZEDBRelease> releases, IDbConnection db)
        {            
            var client = new JsonServiceClient(appSettings.GetString("BaseUrl"));
            var folder = appSettings.GetString("FSPath");
            var apiKey = appSettings.GetString("ApiKey");

            foreach (var release in releases)
            {
                
                if (release.guid == null || release.guid.Length < 5)
                {
                    Console.WriteLine("Invalid Guid");
                    continue;
                }

                var filename = String.Format("/{5}/{0}/{1}/{2}/{3}/{4}.nzb.gz",
                    release.guid[0],
                    release.guid[1],
                    release.guid[2],
                    release.guid[3],
                    release.guid, folder);
                
                var success = false;
                var nzb = new ImportNZB();
                var fileExists = true;

                if (File.Exists((filename)))
                {

                    NZEDBTVEpisode episode = null;

                    if (release.tv_episodes_id != 0)
                    {
                        episode = db.SingleById<NZEDBTVEpisode>(release.tv_episodes_id);
                        if (episode != null) episode.Summary = String.Empty;// System.Net.WebUtility.UrlEncode(episode.Summary);
                    }

                    NZEDBVideo video = null;

                    if (release.videos_id != 0)
                    {
                        video = db.SingleById<NZEDBVideo>(release.videos_id);
                    }

                    nzb = new ImportNZB()
                    {
                        Key = apiKey,
                        Name = release.searchname,
                        Name1 = String.IsNullOrEmpty(release.movie_title) ? release.tv_title : release.movie_title,
                        Name2 = String.IsNullOrEmpty(release.se_complete) ? String.Empty : release.se_complete,
                        Parts = release.totalpart,
                        GroupId = release.groups_id,
                        Size = release.size,
                        Guid = release.guid,                        
                        ImdbId = release.imdbid,
                        VideoId = release.videos_id,
                        ReleaseId = release.id,
                        TVEpisodeId = release.tv_episodes_id,
                        PostDate = release.postdate,
                        TVEpisode = new TVEpisode().PopulateWith(episode),
                        Video = new Video().PopulateWith(video),
                        CategoryId = release.categories_id,
                        AddDate = release.adddate
                    };

                    try
                    {                        
                        client.PostFileWithRequest<bool>(File.OpenRead(filename), String.Format("{0}.nzb.gz", release.guid), nzb);

                        success = true;
                    }
                    catch (WebServiceException e)
                    {
                        Console.WriteLine("Web Exception: {0}", e.Message);                        

                        if (e.Message == "NZB already imported")
                            success = true;

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);                        
                    }
       

                } else {
                    Console.WriteLine("File does not exists");
                    fileExists = false;
                    success = false;
                }

                // try one more time with base64
                if (success == false && fileExists)
                {
                    try
                    {
                        Console.WriteLine("\tRetrying...");
                        
                        nzb.Data = File.Exists(filename) ? Convert.ToBase64String(File.ReadAllBytes(filename)) : String.Empty;
                        client.Post(nzb);
                        success = true;
                    }
                    catch (WebServiceException e)
                    {
                        Console.WriteLine("\tWeb Exception: {0}", e.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\t{0}", e.Message);
                    }
                }                

                if (success)
                {
                    db.Update<NZEDBRelease>(new { exported = true }, q => q.id == release.id);
                    
                } else {
                    db.Update<NZEDBRelease>(new { failedexport = (release.failedexport + 1) }, q => q.id == release.id);
                }                

    		}

		}
    }
}
