using System.Net;

namespace Client;

class Program
{
    static void Main(string[] args)
    {
        WebClient client = new WebClient();
        string fileCopy = "../../../copy.png",
            urlString = "https://www.google.com/images/branding/googlelogo/2x/googlelogo_light_color_272x92dp.png";
        client.DownloadFile(urlString, fileCopy);
    }
}