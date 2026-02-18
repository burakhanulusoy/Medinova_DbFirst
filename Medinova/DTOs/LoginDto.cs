using System.ComponentModel.DataAnnotations;

namespace Medinova.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage ="*Kullanıcı adı boş geçilemez.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "*Şifre boş geçilemez.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "*Şifreler uyumlu değil kontrol ediniz.")]
        public string ConfirmPassword { get; set; }


    }
}