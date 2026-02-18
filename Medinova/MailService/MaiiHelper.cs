using System;
using System.Net;
using System.Net.Mail;

namespace Medinova.MailService
{
    public class MaiiHelper
    {
        public static void SendVerificationCode(string toEmail, string code)
        {
            string fromEmail = "medinovahastaneleri@gmail.com";
            string fromPassword = "dusz pcpr nwmr amoj";


            //burada c# posta ilişkısı boyle yapılıyormus kod ve ııcndekı aynı olacka hep
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;//bağlatıya güvenecegmıyım
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(fromEmail, fromPassword);//kimlık kartı eposta için

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromEmail);
            mailMessage.To.Add(toEmail);//kime gidecek
            mailMessage.Subject = "Medinova Randevu Doğrulama Kodu";
            mailMessage.Body = $"Merhaba, randevunuzu onaylamak için doğrulama kodunuz: <h1>{code}</h1>";
            mailMessage.IsBodyHtml = true;

            client.Send(mailMessage);
        }

        public static void SendAppointmentConfirmation(string toEmail, string department, string doctor, DateTime date, string time)
        {
            // ... (Üstteki kimlik bilgileri ve smtp ayarları aynı) ...
            string fromEmail = "medinovahastaneleri@gmail.com";
            string fromPassword = "dusz pcpr nwmr amoj"; // Senin şifren

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(fromEmail, fromPassword);

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromEmail);
            mailMessage.To.Add(toEmail);

            // Konu ve İçerik Tasarımı
            mailMessage.Subject = "Medinova - Randevu Onayı";

            // HTML ile şık bir tablo yapalım
            mailMessage.Body = $@"
        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
            <h2 style='color: #0d6efd;'>Randevunuz Onaylandı ✅</h2>
            <p>Sayın Hastamız, randevu işleminiz başarıyla tamamlanmıştır. Detaylar aşağıdadır:</p>
            <hr>
            <p><strong>Bölüm:</strong> {department}</p>
            <p><strong>Doktor:</strong> {doctor}</p>
            <p><strong>Tarih:</strong> {date.ToString("dd MMMM yyyy")}</p>
            <p><strong>Saat:</strong> {time}</p>
            <hr>
            <p style='font-size: 12px; color: #777;'>Sağlıklı günler dileriz.<br>Medinova Hastaneleri</p>
        </div>";

            mailMessage.IsBodyHtml = true;

            client.Send(mailMessage);
        }


    }
}