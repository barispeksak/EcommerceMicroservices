using System;
using System.ComponentModel.DataAnnotations;

namespace trendyolApi.Models{
    public class User{
        public int Id { get; set; }

        public string? Fname { get; set; }
        public string? Lname { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public DateTime Dob { get; set; }
    }
}
