using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JLPT.Services
{
    public class UserDataService : IUserDataInterface
    {
        private readonly JlptDbContext _context;
        private readonly IEmailService _emailService;
       
        public UserDataService(JlptDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public void PopulateUserData()
        {
            //read data in "master file"
            string folderRoot = Path.Combine("App_Data/", "Output");
            //var lines = (dynamic)null; 

            string sourceFile = Path.Combine(folderRoot, "2023_July_Edmonton_Master.csv");

            
            if (!File.Exists(sourceFile))
                File.Create(sourceFile).Close();

            string[] lines = File.ReadAllLines(sourceFile);

            string line = "";
           
            try
            {
                for (int i = 1; i < lines.Count(); i++)//while ((line = reader!.ReadLine()) != null)
                {
                    line = lines[i];
                    UserInfo userInfo = new UserInfo();
                    userInfo.TestLevel = getTestLevel(line).Replace("\"", "");
                    userInfo.Email = getEmail(line).Replace("\"", ""); ;
                    userInfo.FirstName = getFirstName(line).Replace("\"", ""); ;
                    userInfo.LastName = getLastName(line).Replace("\"", ""); ;
                    userInfo.SequenceNumber = getSquenceNo(line).Replace("\"", ""); ;
                    userInfo.DOB = getDOB(line).Replace("\"", ""); ;

                    _context.UsersInfo!.Add(userInfo);
                   
                }
            
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        private string getSquenceNo(string content)
        {
            return content.Split(",")[0];
        }
        private string getTestLevel(string content)
        {
            return content.Split(",")[1];
        }
        private string getFirstName(string content)
        {
            return content.Split(",")[2];
        }
        private string getLastName(string content)
        {
            return content.Split(",")[3];
        }
        private string getDOB(string content)
        {
            //Birthday: 1992-06-11
            string[] temps = content.Split(",");
            string bdate = temps[4] + "-" + temps[5] + "-" + temps[6];
            return bdate;
        }
        private string getEmail(string content)
        {
            return content.Split(",")[12];
        }

        public List<UserInfo> GetUserData(int maxItems)
        {
            return _context.UsersInfo!.Where(u=>u.IsEmailSent == false).Take(maxItems).ToList();
        }

        public bool SendEmail(string email, string body)
        {
            throw new NotImplementedException();
        }

        public bool SendEmails(List<int> selectedUserIds)
        {
            bool success = false;
            try
            {
                string body = _emailService.ReadEmailBody();
                List<UserInfo> users = GetSelectedUserData(selectedUserIds);
                foreach (var user in users)
                {
                    //send email one by one
                    _emailService.SendEmail(user, body);

                    //upon successful sending the email out -- set the "isSEndEmail" flag to "true"
                    user.IsEmailSent = true;
                    _context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                _context.SaveChanges();
                success = true; 
            }
            catch(Exception ex)
            {
                success = false;
            }
            return success;
        }

        public List<UserInfo> GetSelectedUserData(List<int> selectedUserIds)
        {
           return _context.UsersInfo!.Where(u=> selectedUserIds.Contains(u.Id)).ToList();
        }
    }
}
