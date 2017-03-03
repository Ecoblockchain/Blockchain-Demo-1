

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlockChainWebApplication.Models
{
    public class UserMetaData
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Please enter a value for Email")]
        [MaxLength(100, ErrorMessage = "Email should not have more than 100 characters")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Gender")]      
        [MaxLength(10, ErrorMessage = "Gender should not have more than 10 characters")]
        public string Gender { get; set; }

        [Display(Name = "Phone")]
        [MaxLength(50, ErrorMessage = "Phone should not have more than 50 characters")]
        public string Phone { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Preferred Name")]
        public string NickName { get; set; }

        [Display(Name = "Last Updated On")]
        public DateTime LastUpdatedOn { get; set; }

        [Display(Name = "Deleted On")]
        public DateTime? DeletedOn { get; set; }

        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please enter a value for Username")]
        [MaxLength(50, ErrorMessage = "Username should not have more than 50 characters")]
        public string Username { get; set; }

        [Display(Name = "Allow My Profile to be Searched")]      
        public bool IsSearchable { get; set; }

        [Display(Name = "Personal Value")]
        public Nullable<decimal> PV { get; set; }

        [Display(Name = "Criteria")]
        public string Criteria { get; set; }

        [Display(Name = "Criteria Value")]
        public Nullable<decimal> CriteriaValue { get; set; }
    }

    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

    }
}


