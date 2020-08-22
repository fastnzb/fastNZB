using System.Collections.Generic;
using ServiceStack;
using FastNZB.ServiceModel.Types;
namespace FastNZB.ServiceModel
{
    [Route("/api/search/{text}")]
    public class FindNZB : IReturn<object>
    {        
        public string text { get; set; }
    }

    [Route("/api/nzb/{Id}")]
    public class GetNZB
    {
        public string Id { get; set; }
    }

    [Route("/api/nzb/detail/{Id}")]
    public class GetNZBDetail {
        public string Id { get; set; }
    }

    [Route("/api/title/{Id}")]
    public class ListNZBs : IReturn<List<ListNZBResult>>
    {        
        public int Id { get; set; }
    }

    public class ListNZBResult {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string Guid { get; set; }
        public int _Days { get; set; }
        public int _Votes { get; set; }
    }

    [Route("/api/vote/{Id}")]
    public class SubmitVote {
        public int Id { get; set; }
        public int val { get; set; }
    }
}