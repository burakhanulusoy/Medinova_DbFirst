using System.ComponentModel.DataAnnotations;

namespace Medinova.DTOs
{
    public class ChangePassword
    {
        [Required(ErrorMessage ="Boş Bırakılamaz")]
        public string UserName { get; set; }
     
       

        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string OldPassword { get; set; }



        [MinLength(6, ErrorMessage = "Şifre minumum 6 karakterli olabilir")]

        [MaxLength(100, ErrorMessage = "Şifre maximum 100 karakterli olabilir")]

        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string NewPassword { get; set; }


        [Required(ErrorMessage = "Boş Bırakılamaz")]

        [Compare(nameof(NewPassword),ErrorMessage ="Şifreler birbiri ile uyumlu değil")]
        public string ConfirmPassword { get; set; }









    }
}