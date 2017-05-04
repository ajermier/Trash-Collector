using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TrashCollector.Models
{
    public class CustomerAddress
    {
        [Key, ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Display(Name = "Address 1")]
        [Required(ErrorMessage = "Address is Required.")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Display(Name = "City")]
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }
        [Display(Name = "State")]
        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }
        [Display(Name = "Zip Code")]
        [Required(ErrorMessage = "Zip code is required.")]
        [RegularExpression(@"\d{5}$", ErrorMessage = "Invalid Zip Code")]
        public int Zip { get; set; }
    }
    public class CreateViewModel
    {
        [Display(Name = "Address 1")]
        [Required(ErrorMessage = "Address is Required.")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Display(Name = "City")]
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }
        [Display(Name = "State")]
        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }
        [Display(Name = "Zip Code")]
        [Required(ErrorMessage = "Zip code is required.")]
        [RegularExpression(@"\d{5}$", ErrorMessage = "Invalid Zip Code")]
        public int Zip { get; set; }
    }
}