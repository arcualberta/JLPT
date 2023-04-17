using System.Net.Mail;

namespace JLPT.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly string _fromEmail;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        

        public EmailService(IConfiguration config)
        {
            _config = config;
            _fromEmail = _config.GetSection("EmailSettings:EmailSender").Value;
            _smtpServer = _config.GetSection("EmailServer:SmtpServer").Value;
            _smtpPort = _config.GetValue<int>("EmailServer:Port");
            
        }
        public void SendEmail(UserInfo user, string body)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_fromEmail);
                mailMessage.Subject = "Japanese Language Proficentcy Test";
                mailMessage.IsBodyHtml = true;

                body = body.Replace(" ##FNAME##", user.FirstName)
                           .Replace(" ##LNAME##", user.LastName)
                           .Replace(" ##DOB##", user.DOB)
                    .Replace(" ##SEQNUM##", user.SequenceNumber)
                    .Replace(" ##TESTLEVEL##", user.TestLevel);
                mailMessage.Body = body;

                //DEBUG ONLY
                mailMessage.To.Add("mruaini@ualberta.ca");
               // mailMessage.To.Add(user.Email);
                mailMessage.CC.Add("arcrcg@ualberta.ca");
                
                using (SmtpClient client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.UseDefaultCredentials = true;
                    client.EnableSsl = true;
                    client.Send(mailMessage);
                }

              

            }
            catch (Exception ex)
            {

            }
        }

        public string ReadEmailBody()
        {
            var body = System.IO.File.ReadAllText("VoucherTemplate.html");

            return body;
        }
    }
}
