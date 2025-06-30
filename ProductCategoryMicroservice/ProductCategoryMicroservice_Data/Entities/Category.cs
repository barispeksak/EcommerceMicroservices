using System;
using System.Collections.Generic;

namespace ProductCategoryMicroservice_Data.Entities
{
    public class Category
    {
        public int Id { get; set; }                          // PK
        public int? ParentCategoryId { get; set; }           // Self-FK (nullable)
        public string CategoryName { get; set; } = null!;    // varchar

        // Navigation
        public Category? ParentCategory { get; set; }
        public ICollection<Category>? SubCategories { get; set; }
    }
}
