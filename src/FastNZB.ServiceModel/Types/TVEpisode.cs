using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class TVEpisode {

        private int _id = 0;
        private int _videosId = 0;
        private int _series = 0;
        private int _episode = 0;
        private string _secomplete = String.Empty;
        private string _title = String.Empty;
        private DateTime _firstaired = DateTime.MinValue;
        private string _summary = String.Empty;

        //[AutoIncrementAttribute]
        [PrimaryKey]
        [Alias("id")]
        public int Id { get { return _id; } set { _id = value;} }      
        [Alias("videos_id")]
        [Index]
        public int VideosId { get { return _videosId; } set { _videosId = value; } }  
        [Alias("series")]
        [Index]
        public int Series { get { return _series; } set { _series = value; } }
        [Alias("episode")]
        [Index]
        public int Episode { get { return _episode; } set { _episode = value; } }
        [Index]
        [Alias("se_complete")]
        public string SeComplete { get { return _secomplete; } set { _secomplete = value; } }
        [Alias("title")]
        public string Title { get { return _title; } set { _title = value; } }
        [Alias("firstaired")]
        public DateTime FirstAired { get { return _firstaired; } set { _firstaired = value; } }
        [Alias("summary")]        
        public string Summary { get { return _summary; } set { _summary = value; } }
    }
}