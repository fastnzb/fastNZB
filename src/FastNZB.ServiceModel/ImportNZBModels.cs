using System;
using System.Collections.Generic;
using ServiceStack;
using FastNZB.ServiceModel.Types;
using System.Runtime.Serialization;
using ServiceStack.Web;

namespace FastNZB.ServiceModel
{
    [Route("/api/import", Verbs="POST")]      
    public class ImportNZB
    {
        public string Key { get; set; }
        public int ReleaseId { get; set; }
        public int ImdbId { get; set; }
        public int VideoId { get; set; }
        public string Name { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public int Parts { get; set; }
        public long Size { get; set; }
        public string Guid { get; set; }
        public int GroupId { get; set; }
        public string NzbGuid { get; set; }
        public int TVEpisodeId { get; set; }
        public DateTime PostDate { get; set; }
        public string Data { get; set; }
        public int CategoryId { get; set; }
        public Video Video { get; set; }
        public TVEpisode TVEpisode { get; set; }
        public DateTime AddDate { get; set; }
    }

    /*[Route("/api/import", Verbs="POST")]
    public class ImportNZBs : IReturn<ImportNZBResponse>
    {
      public List<ImportNZB> NZBs { get; set; }
    }*/

    public class ImportNZBResponse {
    }

    public class UpdateNZB {
        public List<ImportNZB> NZBs { get; set; }        
    }
}