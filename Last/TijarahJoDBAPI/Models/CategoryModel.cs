using System;

namespace Models
{
	    public class CategoryModel
	    {
	        public CategoryModel(int? categoryid, string categoryname, DateTime createdat, bool isdeleted)
	        {
	            this.CategoryID = categoryid;
	            this.CategoryName = categoryname;
	            this.CreatedAt = createdat;
	            this.IsDeleted = isdeleted;
	        }

	        public int? CategoryID { get; set; }
	        public string CategoryName { get; set; }
	        public DateTime CreatedAt { get; set; }
	        public bool IsDeleted { get; set; }
	    }
}
