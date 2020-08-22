using System;
using ServiceStack.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace FastNZB.ServiceModel.Types
{
    public class NZB
    {
        private int _id = 0;
        private int _titleId = 0;
        private int _imdbId = 0;
        private int _videoId = 0;
        private string _name = String.Empty;
        private int _parts = 0;
        private long _size = 0;
        private int _groupId = 0;
        private string _guid = String.Empty;
        private string _nzbGuid = String.Empty;
        private int _releaseId = 0;
        private int _tvEpisodeId = 0;
        private int _categoryId = 0;
        private DateTime _importDate = DateTime.Now;
        private DateTime _postDate = DateTime.MinValue;
        private DateTime _added = DateTime.MinValue;

        [AutoIncrementAttribute]
        [PrimaryKey]
        public int Id { get { return _id; } set { _id = value; } }
        [References(typeof(Title))]
        [Index]
        public int TitleId { get { return _titleId; } set { _titleId = value; } }
        [Index]
        public int ImdbId { get { return _imdbId; } set { _imdbId = value; } }
        [Index]
        public int VideoId { get { return _videoId; } set { _videoId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public int Parts { get { return _parts; } set { _parts = value; } }
        public Int64 Size { get { return _size; } set { _size = value; } }
        [Index]
        public int GroupId { get { return _groupId; } set { _groupId = value; } }
        public string Guid { get { return _guid; } set { _guid = value; } }
        public string NzbGuid { get { return _nzbGuid; } set { _nzbGuid = value; } }
        [Index]
        public int ReleaseId { get { return _releaseId; } set { _releaseId = value; } }
        [Index]
        public int TVEpisodeId { get { return _tvEpisodeId; } set { _tvEpisodeId = value; } }
        [Index]
        public int CategoryId { get { return _categoryId; } set { _categoryId = value; } }
        public DateTime ImportDate { get { return _importDate; } set { _importDate = value; } }
        [Index]
        public DateTime PostDate { get { return _postDate; } set { _postDate = value; } }    
        public DateTime Added { get { return _added; } set { _added = value; } }
        [Reference]
        public List<Vote> Votes { get; set; }

        [Ignore]
        public int _Days { 
            get 
            {
                return PostDate != DateTime.MinValue ? (DateTime.Now - PostDate).Days : 0;
            }
        }

        [Ignore]
        public int _Votes {
            get
            {
                return this.Votes == null ? 0 : this.Votes.Sum(q=>q.Value);
            }
        }

        public string GetLink(string baseUrl, string apiKey, string i = "") {
            return String.Format("{0}/api?t=g&id={1}&apikey={2}&i={3}",
                baseUrl,
                this.Guid,
                apiKey,
                i
            );
        }
    }
}