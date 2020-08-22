using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class Vote
    {
        private int _id = 0;
        private int _nzbId = 0;
        private string _ipAddress = String.Empty;
        private int _value = 0;
        private int _userId = 0;

        [AutoIncrementAttribute]
        [PrimaryKey]
        public int Id { get { return _id; } set { _id = value; } }                

        [References(typeof(NZB))]
        public int NZBId { get { return _nzbId; } set { _nzbId = value; } }
        public string IPAddress { get { return _ipAddress; } set { _ipAddress = value; } }
        public int Value { get { return _value; } set { _value = value; } }
        public int UserId { get { return _userId; } set { _userId = value; } }
    }
}