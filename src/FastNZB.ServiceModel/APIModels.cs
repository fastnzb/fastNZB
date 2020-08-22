using System.Collections.Generic;
using ServiceStack;
using FastNZB.ServiceModel.Types;
using System.Runtime.Serialization;
using System;
using System.Xml.Serialization;
using System.Xml;

namespace FastNZB.ServiceModel
{
    [Route("/api")]
    [Route("/api/api")]
    public class APIRequest : IReturn<object>
    {        
        public string apikey { get; set; }
        public string t { get; set; }
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
        public string i { get; set; }
        public string extended { get; set; }
        public string password { get; set; }
    }

    public class ShowNZB
    {
        public int video { get; set; }
        public string episodes { get; set; }
    }

	[XmlRoot(ElementName="server")]
	public class Server {
		[XmlAttribute(AttributeName="appversion")]
		public string Appversion { get; set; }
		[XmlAttribute(AttributeName="version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName="title")]
		public string Title { get; set; }
		[XmlAttribute(AttributeName="strapline")]
		public string Strapline { get; set; }
		[XmlAttribute(AttributeName="email")]
		public string Email { get; set; }
		[XmlAttribute(AttributeName="meta")]
		public string Meta { get; set; }
		[XmlAttribute(AttributeName="url")]
		public string Url { get; set; }
		[XmlAttribute(AttributeName="image")]
		public string Image { get; set; }
	}

	[XmlRoot(ElementName="limits")]
	public class Limits {
		[XmlAttribute(AttributeName="max")]
		public string Max { get; set; }
		[XmlAttribute(AttributeName="default")]
		public string Default { get; set; }
	}

	[XmlRoot(ElementName="registration")]
	public class Registration {
		[XmlAttribute(AttributeName="available")]
		public string Available { get; set; }
		[XmlAttribute(AttributeName="open")]
		public string Open { get; set; }
	}

	[XmlRoot(ElementName="search")]
	public class Search {
		[XmlAttribute(AttributeName="available")]
		public string Available { get; set; }
		[XmlAttribute(AttributeName="supportedParams")]
		public string SupportedParams { get; set; }
	}

	[XmlRoot(ElementName="tv-search")]
	public class Tvsearch {
		[XmlAttribute(AttributeName="available")]
		public string Available { get; set; }
		[XmlAttribute(AttributeName="supportedParams")]
		public string SupportedParams { get; set; }
	}

	[XmlRoot(ElementName="movie-search")]
	public class Moviesearch {
		[XmlAttribute(AttributeName="available")]
		public string Available { get; set; }
		[XmlAttribute(AttributeName="supportedParams")]
		public string SupportedParams { get; set; }
	}

	[XmlRoot(ElementName="audio-search")]
	public class Audiosearch {
		[XmlAttribute(AttributeName="available")]
		public string Available { get; set; }
		[XmlAttribute(AttributeName="supportedParams")]
		public string SupportedParams { get; set; }
	}

	[XmlRoot(ElementName="searching")]
	public class Searching {
		[XmlElement(ElementName="search")]
		public Search Search { get; set; }
		[XmlElement(ElementName="tv-search")]
		public Tvsearch Tvsearch { get; set; }
		[XmlElement(ElementName="movie-search")]
		public Moviesearch Moviesearch { get; set; }
		[XmlElement(ElementName="audio-search")]
		public Audiosearch Audiosearch { get; set; }
	}

	[XmlRoot(ElementName="subcat")]
	public class Subcat {
		[XmlAttribute(AttributeName="id")]
		public string Id { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="category")]
	public class Category {
		[XmlElement(ElementName="subcat")]
		public List<Subcat> Subcat { get; set; }
		[XmlAttribute(AttributeName="id")]
		public string Id { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="categories")]
	public class Categories {
		[XmlElement(ElementName="category")]
		public List<Category> Category { get; set; }
	}

	[XmlRoot(ElementName="caps")]
	public class Caps {
		[XmlElement(ElementName="server")]
		public Server Server { get; set; }
		[XmlElement(ElementName="limits")]
		public Limits Limits { get; set; }
		[XmlElement(ElementName="registration")]
		public Registration Registration { get; set; }
		[XmlElement(ElementName="searching")]
		public Searching Searching { get; set; }
		[XmlElement(ElementName="categories")]
		public Categories Categories { get; set; }
	}    
}