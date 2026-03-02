using System.ComponentModel.DataAnnotations;

namespace NotikaEmail_Identity.Models
{
    public class ResetPasswordViewModel
    {
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Şifreler aynı olmak zorunda")]
        public string ConfirmPassword { get; set; }

    }
}
