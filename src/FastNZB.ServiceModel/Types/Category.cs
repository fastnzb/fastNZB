using System;
using ServiceStack.DataAnnotations;

namespace FastNZB.ServiceModel.Types
{
    public class Category
    {    
        private int _id = 0;
        private string _title = String.Empty;
        private int _parentId = 0;
                        
        [PrimaryKey]
        [Alias("id")]
        public int Id { get { return _id; } set { _id = value; } }
        [Alias("title")]
        public string Title { get { return _title; } set { _title = value; } }
        [Alias("parentid")]
        public int ParentId { get { return _parentId; } set { _parentId = value; } }

        public bool IsParent() {
            return this.ParentId == 0;
        }
    }
}