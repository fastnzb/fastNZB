using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class Download
    {
        private int _id  = 0;
        private int _apiKeyId = 0;
        private int _nzbId = 0;
        private string _ipAddress = String.Empty;

        [AutoIncrementAttribute]
        [PrimaryKey]
        public int Id { get { return _id; } set { _id = value; } }        
        public int APIKeyId { get { return _apiKeyId; } set { _apiKeyId = value; } }        
        public int NZBId { get { return _nzbId; } set { _nzbId = value; } }
        public string IPAddress { get { return _ipAddress; } set { _ipAddress = value; } }
    }
}