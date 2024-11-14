using System.ComponentModel.DataAnnotations;

namespace IdentityService.Pages.Account.Register
{
    public class RegisterViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string  Password { get; set; }

        [Required]
        public string Username { get; set; }    

        [Required]
        public string FullName { get; set; }

        /// <summary>
        /// After clicking it will get the url to return to the page
        /// </summary>
        public string  ReturnUrl { get; set; }

        public string  Button { get; set; }
    }
}