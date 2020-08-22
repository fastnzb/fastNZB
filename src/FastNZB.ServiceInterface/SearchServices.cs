using System.Threading.Tasks;
using System.Collections.Generic;
using ServiceStack;
using FastNZB.ServiceModel;
using ServiceStack.Redis;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using FastNZB.ServiceModel.Types;

namespace FastNZB.ServiceInterface
{
    [Authenticate]
    public class SearchServices : Service
    {
        public object Any(FindNZB request)
        {
            return Db.Select<Title>("SELECT * FROM Title WHERE MATCH(Name) AGAINST(@term)", new { term = request.text });
        }
    }
}