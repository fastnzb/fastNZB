using System;
using ServiceStack.DataAnnotations;
namespace FastNZB.ServiceModel.Types
{
    public class Title
    {
        private int _id = 0;
        private string _name = String.Empty;
        private int _imdbId = 0;
        private int _tvEpisodeId = 0;
        private int _videoId = 0;
        private string _name1 = String.Empty;
        private string _name2 = String.Empty;

        [AutoIncrementAttribute]
        [PrimaryKey]
        public int Id { get { return _id; } set { _id = value; } }
        public string Name { get { return _name; } set { _name = value; } }  
        [Index]
        public int ImdbId { get { return _imdbId; } set { _imdbId = value; } }     
        [Index]
        public int TVEpisodeId { get { return _tvEpisodeId; } set { _tvEpisodeId = value; } }
        [Index]
        public int VideoId { get { return _videoId; } set { _videoId = value; } }
        public string Name1 { get { return _name1; } set { _name1 = value; } }
        public string Name2 { get { return _name2; } set { _name2 = value; } }        
    }
}