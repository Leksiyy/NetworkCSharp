using System.Net.Mail;

namespace smtp;

class Program
{
    static void Main(string[] args)
    {
        MailMessage post = new MailMessage();
        while (true)
        {
            post.From = new MailAddress("your@gmail.com");
            
            post.To.Add("notyour@gmail.com");
            post.To.Add("notyour2197618@gmail.com");

            post.Subject = "Hello World!";
            post.IsBodyHtml = false;
            post.Body = "Hello World!!!!!!!!!";
            
            Attachment attachment = new Attachment("../../../Program.cs");
            post.Attachments.Add(attachment);
            
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("your@gmail.com", "your code for smtp");

            smtp.Send(post);

            break;
        }
        
        
    }
}