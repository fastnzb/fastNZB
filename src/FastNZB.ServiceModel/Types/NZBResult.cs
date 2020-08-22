using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class NZBResult : NZB {   

        private int _season = 0;
        private int _episode = 0;
        private DateTime _tvAirDate = DateTime.MinValue;
        private int _tvdbId = 0;
        private int _tvRageId = 0;
        private int _tvMazeId = 0;
        private int _imdbId = 0;
        private string _categoryName = String.Empty;
        private string _groupName = String.Empty;

        [Alias("series")]
        public int Season { get { return _season; } set { _season = value; } }
        [Alias("episode")]
        public int Episode { get { return _episode; } set { _episode = value; } }
        [Alias("firstaired")]
        public DateTime TVAirDate { get { return _tvAirDate; } set { _tvAirDate = value; } }
        [Alias("tvdb")]
        public int TvdbId { get { return _tvdbId; } set { _tvdbId = value; } }
        [Alias("tvrage")]
        public int TvRageId { get { return _tvRageId; } set { _tvRageId = value;} }
        [Alias("tvmase")]
        public int TvMazeId { get { return _tvMazeId; } set { _tvMazeId = value; } }
        [Alias("imdb")]
        public new int ImdbId { get { return _imdbId; } set { _imdbId = value; } }
        [Alias("category_name")]
        public string CategoryName { get { return _categoryName; } set { _categoryName = value; } }
        [Alias("group_name")]
        public string GroupName { get { return _groupName; } set { _groupName = value; } }
    }
}