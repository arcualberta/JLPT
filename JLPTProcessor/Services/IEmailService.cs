namespace JLPT.Services
{
    public interface IEmailService
    {
        void SendEmail(UserInfo user, string body);
        void SendTestEmail(UserInfo user, string body);
        public string ReadEmailBody();
    }
}
