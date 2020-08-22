using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Data;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Data;
using ServiceStack.Configuration;

using FastNZB.ServiceModel.Types;

using FastNZB.ServiceModel;

namespace FastNZB.ServiceInterface
{    
    [ConnectionInfo(NamedConnection="ReadOnly")]
    public class NZBServices : Service
    {      
        public IDbConnectionFactory ConnectionFactory { get; set; }

        public IAutoQueryDb AutoQuery { get; set; }

        public TextFileSettings _settings { get; set; }

        [Authenticate]
        public object Any(ListNZBs request)
        {                                    
            var nzbs = Db.LoadSelect<NZB>(q=>q.TitleId == request.Id).OrderBy(q=>q._Days).ThenBy(q=>q._Votes);
            var response = new List<ListNZBResult>();

            foreach(var n in nzbs) {
                response.Add(new ListNZBResult().PopulateWith(n));
            }

            return response;        
        }

        public object Any(SubmitVote request)
        {            
            var session = SessionAs<FastNZBUserSession>();

            Vote vote = null;

            if (session.IsAuthenticated && Db.Exists<Vote>(q => q.UserId == int.Parse(session.UserAuthId) && q.NZBId == request.Id))
                vote = Db.Single<Vote>(q => q.UserId == int.Parse(session.UserAuthId) && q.NZBId == request.Id);
            else if (!session.IsAuthenticated && Request.Headers["X-Forwarded-For"] != null && Db.Exists<Vote>(q => q.IPAddress == Request.Headers["X-Forwarded-For"] && q.NZBId == request.Id))
                vote = Db.Single<Vote>(q => q.IPAddress == Request.Headers["X-Forwarded-For"] && q.NZBId == request.Id);

            if (request.val < 0) request.val = -1;
            if (request.val > 0) request.val = 1;

            using (var db = ConnectionFactory.OpenDbConnection())
            {
                if (vote != null)
                {
                    if (request.val == vote.Value)
                        db.Delete<Vote>(q => q.Id == vote.Id);
                    else
                    {
                        vote.Value = request.val;
                        db.Save<Vote>(vote);
                    }
                }
                else
                {                    
                    db.Insert<Vote>(new Vote()
                    {
                        NZBId = request.Id,
                        IPAddress = Request.Headers["X-Forwarded-For"],
                        Value = request.val,
                        UserId = !session.IsAuthenticated ? 0 : int.Parse(session.UserAuthId)
                    });
                }
            }

            return Db.Select<Vote>(q => q.NZBId == request.Id).Sum(q => q.Value);
        }

        public object Any(GetNZBDetail request) {
            var n = Db.Single<NZB>(q=>q.Guid == request.Id);
            Db.LoadReferences<NZB>(n);
            return new ListNZBResult().PopulateWith(n);
        }

        [Authenticate]
        public object Any(GetNZB request)
        {
            var nzb = Db.Single<NZB>(q=>q.Guid == request.Id);
            
            using (var db = ConnectionFactory.OpenDbConnection())
            {
                db.Insert<Download>(new Download() {
                    NZBId = nzb.Id,
                    IPAddress = Request.Headers["X-Forwarded-For"],
                });          
            }

            var nzbpath = String.Format("nzb/{0}/{1}/{2}/{3}/",
                request.Id[0],
                request.Id[1],
                request.Id[2],
                request.Id[3]);     
            var filename = String.Format("{0}.nzb.gz",request.Id);
            var path = _settings.Get<string>("FSPath");

            Stream stream = null;                
            if (!Directory.Exists(path+nzbpath) || !File.Exists(path+filename)) {
                var file = VirtualFiles.GetFile(nzbpath+filename);    
                Directory.CreateDirectory(path+nzbpath);
                File.WriteAllBytes(path+nzbpath+filename, file.ReadAllBytes());                        
            }
            stream = File.OpenRead(path+nzbpath+filename);          

            Response.AddHeader("Content-Disposition", "attachment; filename="+nzb.Name+".nzb");
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

        public object Any(QueryTitles query)
        {            
            var q = AutoQuery.CreateQuery(query, base.Request);            
            var words = query.NameQuery.Split(' ');
            var count = 0;
            foreach (string word in words)
            {
                var w = word;                

                if (w.Contains("^") && count == 0)
                    q.And(q2 => q2.Name.StartsWith(w.Substring(1)));
                else if (w.Contains("--"))
                    q.And(q2 => q2.Name.Contains(w.Substring(2)));
                else
                    q.And(q2 => q2.Name.Contains(w));

                count++;
            }
            
            return AutoQuery.Execute(new QueryTitles(), q);
        }
    }

    [Authenticate]
    [ConnectionInfo(NamedConnection="ReadOnly")]
    [Route("/api/query/titles")]      
    public class QueryTitles : QueryDb<Title>
    {
        public string NameQuery { get; set; }
    }

    [Authenticate]
    [ConnectionInfo(NamedConnection="ReadOnly")]
    [Route("/api/query/nzbs")]    
    public class QueryNZBs : QueryDb<NZB> { }
}