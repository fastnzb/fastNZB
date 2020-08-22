using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class APIRequest {

        // no default values here simply because i'm lazy
        [AutoIncrementAttribute]
        [PrimaryKey]
        public long Id { get; set; }      
        public int UserId { get; set; }  
        public long APIKeyId { get; set; }   
        public DateTime Date { get; set; }     
        public string t { get; set; }        
        [Alias("id_a")]
        public string id { get; set; }
        public string q { get; set; }
        public string vid { get; set; }
        public string tvdbid { get; set; }
        public string traktid { get; set; }
        public string rid { get; set; }
        public string tvmazeid { get; set; }
        public string imdbid { get; set; }
        public string tmdbid { get; set; }
        public string season { get; set; }
        public string ep { get; set; }
        public int maxage { get; set; }
        public int minsize { get; set; }
        public string cat { get; set; }
        public string group { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }     
        public string o { get; set; }     
        public long results { get; set; }   
    }
}