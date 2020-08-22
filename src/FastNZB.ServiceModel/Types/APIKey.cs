using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class APIKey {

        private int _id = 0;
        private int _userId = 0;
        private int _requests = 0;
        private int _requestLimit = 0;
        private string _key = String.Empty;

        [AutoIncrementAttribute]
        [PrimaryKey]
        public int Id { get { return _id; } set { _id = value; } }      
        public int UserId { get { return _userId; } set { _userId = value; } }  
        public int Requests { get { return _requests; } set { _requests = value;} }
        public int RequestLimit { get { return _requestLimit; } set { _requestLimit = value; } }
        public string Key { get { return _key; } set { _key = value; } }
    }
}