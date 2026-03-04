using System.ComponentModel.DataAnnotations;

namespace NotikaEmail_Identity.Models
{
    public class CloseAccountViewModel
    {
        [Required(ErrorMessage = "Boş Bıraklıamaz")]
        public string Code { get; set; }
        
    }
}
