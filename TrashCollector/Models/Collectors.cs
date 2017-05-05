using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TrashCollector.Models
{
    public class Collectors
    {
        [Key, ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Display(Name = "Zip Code")]
        [Required(ErrorMessage = "Zip code is required.")]
        [RegularExpression(@"\d{5}$", ErrorMessage = "Invalid Zip Code")]
        public int ZipCode { get; set; }
        [Required]
        public int EmployeeId { get; set; }
    }
}