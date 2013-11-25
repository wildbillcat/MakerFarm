using System;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    public class PrintErrorType
    {
        public long PrintErrorTypeId { get; set; } /* Generic Id for the Error Type */

        [Display(Name = "Error Name", Description = "The Name/Short Description of the error")]
        public string PrintErrorName { get; set; } /* Name of the error type */

        [Display(Name = "User Error?", Description = "This Denotes if it is an error deemed to be fault of the user")]
        public bool UserError { get; set; } /* There are two types of errors in this world, User Errors and Non-User Errors */
    }
}