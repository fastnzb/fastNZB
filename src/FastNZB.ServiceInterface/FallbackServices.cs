using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Data;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Data;
using ServiceStack.Configuration;

using FastNZB.ServiceModel.Types;

using FastNZB.ServiceModel;

namespace FastNZB.ServiceInterface
{    
    [FallbackRoute("/{Path*}")]
    [Route("/index.html")]
    public class Fallback
    {
        public string Path { get; set; }
    }

    public class FallBackService : Service
    {
        public TextFileSettings Settings { get; set; }
        public object Any(Fallback request)
        {            
            return new HttpResult(new FileInfo("wwwroot/index.html")) { ContentType = "text/html" };
        }
    }
}