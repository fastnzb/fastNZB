using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace FastNZB.ServiceModel.Rss
{
	public class Feed
	{		
		private string replace = "_123456789_"; // needle for search/replace used later
		public string Description { get; set; }
		public Uri Link { get; set; }
		public string Title { get; set; }
		public string Copyright { get; set; }
		public long Total { get; set; }
		public int Offset { get; set; }

		public ICollection<Item> Items { get; set; } = new List<Item>();

		public string Serialize()
		{
			var doc = new XDocument(new XElement("rss"));
			doc.Root.Add(new XAttribute("version", "2.0"));
			doc.Root.Add(new XAttribute("xmlns" + replace + "atom", "http://www.w3.org/2005/Atom"));
			doc.Root.Add(new XAttribute("xmlns" + replace + "newznab", "http://www.newznab.com/DTD/2010/feeds/attributes/"));
			doc.Root.Add(new XAttribute("encoding", "utf-8"));

			var channel = new XElement("channel");			
			channel.Add(new XElement("atom"+replace+"link",new XAttribute("href", "https://fastnzb.com/api"), new XAttribute("rel","self"), new XAttribute("type", "application/rss+xml")));
			channel.Add(new XElement("title", this.Title));
			channel.Add(new XElement("description", ""));
			channel.Add(new XElement("link", this.Link.AbsoluteUri));
			channel.Add(new XElement("language", "en-gb"));
			channel.Add(new XElement("webMaster", "admin@fastnzb.com"));
			channel.Add(new XElement("category", "usenet,nzbs,cms,community"));
			channel.Add(new XElement("generator", "nZEDb"));
			channel.Add(new XElement("ttl", "10"));
			channel.Add(new XElement("docs", "https://fastnzb.com"));
			channel.Add(new XElement("image",
				new XAttribute("url", "https://fastnzb.com/assets/img/nzb.png"),
				new XAttribute("title", "fastNZB"),
				new XAttribute("link", "https://fastnzb.com"),
				new XAttribute("description", "fastNZB")));

			channel.Add(new XElement("description", this.Description));			
			channel.Add(new XElement("newznab" + replace + "response", new XAttribute("offset", this.Offset), new XAttribute("total", this.Total)));

			doc.Root.Add(channel);

			foreach (var item in Items)
			{
				var itemElement = new XElement("item");
				
				itemElement.Add(new XElement("title", item.Title));
				
				if (!string.IsNullOrWhiteSpace(item.Permalink)) 
					itemElement.Add(new XElement("guid", item.Permalink, new XAttribute("isPermaLink", "true")));
				
				itemElement.Add(new XElement("link", item.Link.AbsoluteUri));
				
				if (item.Comments != null) 
					itemElement.Add(new XElement("comments", item.Comments.AbsoluteUri));
				
				var pubdateFmt = item.PublishDate.ToString("r");
				pubdateFmt = pubdateFmt.Substring(0, pubdateFmt.Length - 3);
				pubdateFmt += item.PublishDate.ToString("zzz").Replace(":", "");
				itemElement.Add(new XElement("pubDate", pubdateFmt));
				
				if (!String.IsNullOrWhiteSpace(item.Category))
					itemElement.Add(new XElement("category", item.Category));
				
				itemElement.Add(new XElement("description", item.Body));

				if (item.Author != null)
					itemElement.Add(new XElement("author", $"{item.Author.Email} ({item.Author.Name})"));

				foreach (var c in item.Categories)
					itemElement.Add(new XElement("category", c));

				itemElement.Add(new XElement("enclosure", new XAttribute("url", item.Link.AbsoluteUri), new XAttribute("length", item.Length), new XAttribute("type", "application/x-nzb")));
				itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "category"), new XAttribute("value", item.CategoryId)));
				itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "size"), new XAttribute("value", item.Length)));
				itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "files"), new XAttribute("value", "0")));
				itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "poster"), new XAttribute("value", "")));
				itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "grabs"), new XAttribute("value", "0")));
				itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "comments"), new XAttribute("value", "0")));
				itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "password"), new XAttribute("value", "0")));
				
				if (item.Result != null)
				{
					var dateFmt = item.Result.PostDate.ToString("r");
					dateFmt = dateFmt.Substring(0, dateFmt.Length - 3);
					dateFmt += item.Result.PostDate.ToString("zzz").Replace(":", "");
					
					itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "usenetdate"), new XAttribute("value", dateFmt)));

					if (item.Result.GroupName != null)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "group"), new XAttribute("value", item.Result.GroupName)));
					
					if (item.Result.Season!=0)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "season"), new XAttribute("value", item.Result.Season)));
					
					if (item.Result.Episode!=0)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "episode"), new XAttribute("value", item.Result.Episode)));
					
					if (item.Result.TVAirDate!=DateTime.MinValue)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "tvairdate"), new XAttribute("value", item.Result.TVAirDate.ToString("yyyy-MM-dd"))));
					
					if (item.Result.TvdbId!=0)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "tvdbid"), new XAttribute("value", item.Result.TvdbId)));
					
					if (item.Result.TvRageId!=0)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "tvrageid"), new XAttribute("value", item.Result.TvRageId)));
					
					if (item.Result.TvRageId!=0)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "rageid"), new XAttribute("value", item.Result.TvRageId)));
					
					if (item.Result.TvMazeId!=0)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "tvmazeid"), new XAttribute("value", item.Result.TvMazeId)));
					
					if (item.Result.ImdbId!=0)
						itemElement.Add(new XElement("newznab" + replace + "attr", new XAttribute("name", "imdbid"), new XAttribute("value", item.Result.ImdbId)));
				}
				channel.Add(itemElement);
			}

			StringWriter textWriter = new Utf8StringWriter();
			doc.Save(textWriter, SaveOptions.None);

			// replace our needle with ":" since this library doesn't support the ":" char in an attr name
			return textWriter.ToString().Replace(replace, ":");
		}
	}
	public class Utf8StringWriter : StringWriter
	{
		public override Encoding Encoding => Encoding.UTF8;
	}
}