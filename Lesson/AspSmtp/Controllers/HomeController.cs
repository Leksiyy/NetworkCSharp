using EmailService;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IEmailSender _emailSender;

    public HomeController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [HttpGet]
    public IActionResult Email()
    {
        var message = new Message(new string[] { "pantera197618@gmail.com" }, "Test email", "This is the content from our email.");
        _emailSender.SendEmail(message);
        return Content("Email отправлен!");
    }

    public IActionResult Index()
    {
        return View();
    }
}