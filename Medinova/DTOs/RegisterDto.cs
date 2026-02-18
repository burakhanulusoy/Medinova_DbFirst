using System.ComponentModel.DataAnnotations;

namespace Medinova.DTOs
{
    public class RegisterDto
    {

        [Required(ErrorMessage ="*Ad alanı boş bırakılamaz")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "*Soyad alanı boş bırakılamaz")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "*Mail adres alanı boş bırakılamaz")]

        public string Email { get; set; }
        [Required(ErrorMessage ="*Telefon numarası alanı boş bırakılamaz")]

        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "*Kullanıcı adı boş bırakılamaz")]

        public string UserName { get; set; }

        [Required(ErrorMessage = "*Şifre boş bırakılamaz.")]
        [MinLength(6, ErrorMessage = "*Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        // Aşağıdaki Regex: En az 1 küçük harf, 1 büyük harf, 1 rakam ve 1 özel karakter ister
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
              ErrorMessage = "*Şifreniz en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter (.,+_@ gibi) içermelidir.")]
        public string Password { get; set; }

        // ŞİFRE TEKRAR ALANI
        [Required(ErrorMessage = "*Şifre tekrarı boş bırakılamaz.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "*Girdiğiniz şifreler birbiriyle uyuşmuyor.")]
        public string ConfirmPassword { get; set; }



    }
}