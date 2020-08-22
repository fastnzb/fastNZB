using System;
using ServiceStack;
using ServiceStack.Auth;
using FastNZB.ServiceModel;
using FastNZB.ServiceModel.Types;
using System.Data;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Configuration;

namespace FastNZB.ServiceInterface
{
    [Authenticate]
    [ConnectionInfo(NamedConnection="ReadOnly")]
    public class SessionInfoServices : Service
    {
        public object Any(SessionInfo request)
        {
            var result = SessionAs<FastNZBUserSession>();
            var userId = int.Parse(result.UserAuthId);

            if (String.IsNullOrEmpty(result.APIKey)) {
                var apiKey = Db.Single<APIKey>(q=> q.UserId == userId);
                result.APIKey = apiKey != null ? apiKey.Key : "";
                Request.SaveSession(result);
            }            

            result.APIRequests = Db.Count<FastNZB.ServiceModel.Types.APIRequest>(q=>q.UserId == userId && q.Date > DateTime.Now.AddDays(-1));
    
            return result;
        }
    }
}
