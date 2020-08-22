using System.Collections.Generic;
using System.IO;
using Funq;
using System.Linq;
using ServiceStack;
using ServiceStack.IO;
using ServiceStack.VirtualPath;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.Data;
using ServiceStack.Text;
using System.Configuration;
using ServiceStack.Redis;
using ServiceStack.Auth;
using ServiceStack.Caching;

using FastNZB.ServiceInterface;
using FastNZB.ServiceModel.Types;

namespace FastNZB
{
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Configure your ServiceStack AppHost singleton instance:
        /// Call base constructor with App Name and assembly where Service classes are located
        /// </summary>
        public AppHost() : base("FastNZB", typeof(SearchServices).Assembly)
        {
            AppSettings = new MultiAppSettings(
                new TextFileSettings("appsettings.txt"),
                new EnvironmentVariableSettings(),
                new AppSettings());                
        }

        public override void Configure(Container container)
        {            
            // ServiceStack config
            SetConfig(new HostConfig() {
                //HandlerFactoryPath = "/api",                
                DebugMode = true,
                DefaultRedirectPath = "index.html",
                StrictMode = false
            });
            
            // register app settings in container for use later            
            container.Register<TextFileSettings>(new TextFileSettings("appsettings.txt"));

            // master db            
            var dbFactory = new OrmLiteConnectionFactory(AppSettings.Get<string>("MySQL"), MySqlDialect.Provider);
            container.Register<IDbConnectionFactory>(dbFactory);

            // read-only db
            dbFactory.RegisterConnection("ReadOnly", AppSettings.Get<string>("MySQLRead"), MySqlDialect.Provider);
            
            // init tables if they don't exist
            using (var db = dbFactory.OpenDbConnection())
            {
                db.CreateTableIfNotExists<Title>();
                db.CreateTableIfNotExists<NZB>();                
                db.CreateTableIfNotExists<Download>();                
                db.CreateTableIfNotExists<Vote>();
                db.CreateTableIfNotExists<APIKey>();
                db.CreateTableIfNotExists<APIRequest>();
                db.CreateTableIfNotExists<Category>();
                db.CreateTableIfNotExists<Count>();
                db.CreateTableIfNotExists<TVEpisode>();
                db.CreateTableIfNotExists<Video>();
            }

            // auto query is used for paging/querying titles
            Plugins.Add(new AutoQueryFeature { MaxLimit = 100 });

            // configure redis as cache
            string redisHost = AppSettings.Get("Redis", defaultValue: "localhost");            
            var redisClient = new PooledRedisClientManager(new string[] { redisHost });            
            var cacheClient = new RedisClientManagerCacheClient(redisClient);            
            container.Register<ICacheClient>(cacheClient);
            cacheClient.InitSchema();

            // register auth repo
            var authRepo = new FastNZBOrmLiteAuthRepository(dbFactory);
            container.Register<IUserAuthRepository>(authRepo);
            authRepo.InitSchema();

            Plugins.Add(new AuthFeature(() => new FastNZBUserSession(),
            new IAuthProvider[] {                 
                new CredentialsAuthProvider(), //HTML Form post of UserName/Password credentials
            })
            {                
                HtmlRedirect = null,
                IncludeRegistrationService = false,
                MaxLoginAttempts = 5,//appSettings.Get("MaxLoginAttempts", 5),
                IncludeAssignRoleServices = false,
                ValidateUniqueEmails = false,
                ValidateUniqueUserNames = false,                                                
            });

            Plugins.Add(new RegistrationFeature() {
                AtRestPath = "/api/register"
            });            
        }
    }
}
