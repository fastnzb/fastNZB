using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Data;

using FastNZB.ServiceModel.Types;

namespace FastNZB.ServiceInterface
{
    [DataContract]
    public class FastNZBUserSession : AuthUserSession
    {
        [DataMember]
        public string APIKey { get; set; }

        [DataMember]
        public long APIRequests { get; set; }
    }
}
