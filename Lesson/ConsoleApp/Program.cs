using System.Net.Mail;
using System.Text;

namespace ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        MailMessage post = new MailMessage();
        post.From = new MailAddress("pantera197618@gmail.com");
        post.To.Add("pantera197618@gmail.com");
        post.Subject = "Hello World!";
        post.IsBodyHtml = false;
        post.Body = "Hello World!!!!!!!!!";
        
        Attachment attachment = new Attachment("../../../Program.cs");
        post.Attachments.Add(attachment);
        
        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.EnableSsl = true;
        smtp.Credentials = new System.Net.NetworkCredential("pantera197618@gmail.com", "fbwl jrfc rmth ahwt");
        
        smtp.Send(post);
    }
}