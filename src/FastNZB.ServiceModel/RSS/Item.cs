using System;
using System.Collections.Generic;
using FastNZB.ServiceModel.Types;

namespace FastNZB.ServiceModel.Rss
{
  public class Item
  {
    public Author Author { get; set; }
    public string Body { get; set; }
    public ICollection<string> Categories { get; set; } = new List<string>();
    public Uri Comments { get; set; }
    public Uri Link { get; set; }
    public string Permalink { get; set; }
    public DateTime PublishDate { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Length { get; set; }    
    public string CategoryId { get; set; }
    public NZBResult Result { get; set; }

  }
}