using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack;
using FastNZB.ServiceModel;
using FastNZB.ServiceModel.Types;
using ServiceStack.OrmLite;
using System.Data;
using ServiceStack.Data;
using ServiceStack.Configuration;

namespace FastNZB.ServiceInterface
{
    public class ImportServices : Service
    {
        public IDbConnectionFactory ConnectionFactory { get; set; }
        private TextFileSettings _settings { get; set; }

        public object Post(UpdateNZB request) {

            var nzbItemsToSave = new List<NZB>();
            foreach (var nzb in request.NZBs) {
                var nzbItem = Db.Single<NZB>(q=> q.ReleaseId == nzb.ReleaseId);
                nzbItem.CategoryId = nzb.CategoryId;
                nzbItemsToSave.Add(nzbItem);                
            }

            Db.SaveAll<NZB>(nzbItemsToSave);

            return true;
        }

        public object Post(ImportNZB nzb)
        {
            if (nzb.Key != _settings.GetString("ApiKey"))
                throw new HttpError(401, "Unauthorized");

            if (Db.Exists<NZB>(new { ReleaseId = nzb.ReleaseId }))
                throw new HttpError(500, "NZB already imported");

            if (Request.Files.Length == 0 && !String.IsNullOrEmpty(nzb.Data))
                throw new HttpError(500, "No file present");

            if (Request.Files.Length == 0 && !String.IsNullOrEmpty(nzb.Data))
            {
                byte[] data = Convert.FromBase64String(nzb.Data);

                using (var stream = new MemoryStream(data))
                {

                    var file = String.Format("nzb/{0}/{1}/{2}/{3}/{4}.nzb.gz",
                        nzb.Guid[0],
                        nzb.Guid[1],
                        nzb.Guid[2],
                        nzb.Guid[3],
                        nzb.Guid);

                    VirtualFiles.WriteFile(file, stream);
                }
            }

            Title title;     

            var catCount = Db.Single<Count>(q=>q.CategoryId == nzb.CategoryId);   

            bool titleExists = 
                (nzb.TVEpisodeId != 0 && Db.Exists<Title>(q=>q.TVEpisodeId == nzb.TVEpisodeId)) ||
                (nzb.ImdbId != 0 && Db.Exists<Title>(q=>q.ImdbId == nzb.ImdbId));
            
            using (var db = ConnectionFactory.OpenDbConnection()) {

                if (nzb.TVEpisodeId != 0 && !titleExists)
                {                        
                    db.Save<Title>(new Title() {
                        TVEpisodeId = nzb.TVEpisodeId,   
                        VideoId = nzb.VideoId,
                        Name = nzb.Name1 +" "+ nzb.Name2,
                        Name1 = nzb.Name1,
                        Name2 = nzb.Name2,                                
                    });
                }
                else if (nzb.ImdbId != 0 && !titleExists)
                {                
                    db.Save<Title>(new Title() {
                        ImdbId = nzb.ImdbId,   
                        Name = nzb.Name1,
                        Name1 = nzb.Name1,                                              
                    });
                }

                if (nzb.ImdbId != 0)
                    title = db.Single<Title>(q=>q.ImdbId == nzb.ImdbId);
                else
                    title = db.Single<Title>(q=> q.TVEpisodeId == nzb.TVEpisodeId);

                bool saveVideo = nzb.Video != null && !db.Exists<Video>(q=> q.Id == nzb.VideoId);
                bool saveEpisode = nzb.TVEpisode != null && !db.Exists<TVEpisode>(q=> q.Id == nzb.TVEpisodeId);                

                //if (saveEpisode)
                    //nzb.TVEpisode.Summary = "";//System.Net.WebUtility.UrlDecode(nzb.TVEpisode.Summary);

                using (var trans = db.OpenTransaction(IsolationLevel.ReadCommitted)) {

                    try {

                        var n = new NZB() {
                            TitleId = title.Id,
                            ImdbId = nzb.ImdbId,
                            VideoId = nzb.VideoId,
                            Name = nzb.Name,
                            Parts = nzb.Parts,
                            Size = nzb.Size,
                            GroupId = nzb.GroupId,
                            Guid = nzb.Guid,
                            //NzbGuid = nzb.NzbGuid,
                            ReleaseId = nzb.ReleaseId,
                            TVEpisodeId = nzb.TVEpisodeId,
                            PostDate = nzb.PostDate,  
                            ImportDate = DateTime.Now,
                            CategoryId = nzb.CategoryId,
                            Added = nzb.AddDate
                        };
                    
                        if (saveVideo) {
                            nzb.Video.Id = nzb.VideoId;
                            db.Save<Video>(nzb.Video);
                        }
                                    
                        if (saveEpisode) {
                            nzb.TVEpisode.Id = nzb.TVEpisodeId;
                            db.Save<TVEpisode>(nzb.TVEpisode);
                        }

                        db.Save<NZB>(n);                        

                        catCount.Total++;
                        db.Save<Count>(catCount);
                        
                        trans.Commit();                

                    } catch (Exception e) {                    

                        trans.Rollback();
                        throw new HttpError(500, "Database error: "+e.Message);

                    } // catch
                
                } // trans
                
            } // db

            foreach(var file in Request.Files)
            {
                var filename = String.Format("nzb/{0}/{1}/{2}/{3}/{4}",
                        file.FileName[0],
                        file.FileName[1],
                        file.FileName[2],
                        file.FileName[3],
                        file.FileName); 

                file.SaveTo(VirtualFiles, filename);
            }

            return true;

        }
    }
}