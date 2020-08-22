using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class Video {

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


        //[AutoIncrementAttribute]
        [PrimaryKey]
        [Alias("id")]
        public int Id { get { return _id; } set { _id = value; } }      
        [Alias("type")]        
        public int Type { get { return _type; } set { _type = value; } }  
        [Alias("title")]
        public string Title { get { return _title; } set { _title = value; } }
        [Alias("countries_id")]
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
}