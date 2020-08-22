using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace FastNZB.Import
{

    // nZEDb tables

    [Alias("releases")]
    public class NZEDBRelease
    {
        public int id { get; set; }
        public string searchname { get; set; }
        public int totalpart { get; set; }
        public int groups_id { get; set; }
        public Int64 size { get; set; }
        public string guid { get; set; }
        public string tv_title { get; set; }
        public string se_complete { get; set; }
        public string movie_title { get; set; }
        public int imdbid { get; set; }
        public int videos_id { get; set; }
        public int tv_episodes_id { get; set; }
        public DateTime postdate { get; set; }
        public bool exported { get; set; }
        public int failedexport { get; set; }
        public int categories_id { get; set; }
        public DateTime adddate { get; set; }
    }

    [Alias("videos")]
    public class NZEDBVideo
    {

        private int _id = 0;
        private int _type = 0;
        private string _title = String.Empty;
        private string _countriesId = String.Empty;
        private DateTime _started = DateTime.MinValue;
        private int _anidb = 0;
        private int _imdb = 0;
        private int _tmdb = 0;
        private int _trakt = 0;
        private int _tvdb = 0;
        private int _tvmaze = 0;
        private int _tvrage = 0;
        private int _source = 0;


        [AutoIncrementAttribute]
        [PrimaryKey]
        [Alias("id")]
        public int Id { get { return _id; } set { _id = value; } }
        [Alias("type")]
        public int Type { get { return _type; } set { _type = value; } }
        [Alias("title")]
        public string Title { get { return _title; } set { _title = value; } }
        [Alias("country_id")]
        public string CountriesId { get { return _countriesId; } set { _countriesId = value; } }
        [Alias("started")]
        public DateTime Started { get { return _started; } set { _started = value; } }
        [Alias("anidb")]
        public int AniDB { get { return _anidb; } set { _anidb = value; } }
        [Alias("imdb")]
        public int Imdb { get { return _imdb; } set { _imdb = value; } }
        [Alias("tmdb")]
        public int Tmdb { get { return _tmdb; } set { _tmdb = value; } }
        [Alias("trakt")]
        public int Trakt { get { return _trakt; } set { _trakt = value; } }
        [Alias("tvdb")]
        public int Tvdb { get { return _tvdb; } set { _tvdb = value; } }
        [Alias("tvmaze")]
        public int TvMaze { get { return _tvmaze; } set { _tvmaze = value; } }
        [Alias("tvrage")]
        public int TvRage { get { return _tvrage; } set { _tvrage = value; } }
        [Alias("source")]
        public int Source { get { return _source; } set { _source = value; } }
    }
    [Alias("tv_episodes")]
    public class NZEDBTVEpisode
    {

        private int _id = 0;
        private int _videosId = 0;
        private int _series = 0;
        private int _episode = 0;
        private string _secomplete = String.Empty;
        private string _title = String.Empty;
        private DateTime _firstaired = DateTime.MinValue;
        private string _summary = String.Empty;

        [AutoIncrementAttribute]
        [PrimaryKey]
        [Alias("id")]
        public int Id { get { return _id; } set { _id = value; } }
        [Alias("videos_id")]
        public int VideosId { get { return _videosId; } set { _videosId = value; } }
        [Alias("series")]
        public int Series { get { return _series; } set { _series = value; } }
        [Alias("episode")]
        public int Episode { get { return _episode; } set { _episode = value; } }
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
