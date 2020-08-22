using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class Count
    {
        private int _categoryId = 0;
        private long _total = 0;

        [PrimaryKey]        
        public int CategoryId { get { return _categoryId; } set { _categoryId = value; } }  
        [Alias("Count")]
        public long Total { get { return _total; } set { _total = value; } }        
    }
}