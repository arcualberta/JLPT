using Microsoft.AspNetCore.Mvc;

namespace JLPT.Services
{
    public interface IUserDataInterface
    {
        public void PopulateUserData();
        public List<UserInfo> GetUserData(int maxItems);
        public List<UserInfo> GetSelectedUserData(List<int> selectedUserIds);

        public bool SendEmails(List<int> selectedUserIds, bool testEmail=false);

        public bool IsFileExisted(string filePath);
        public FileContentResult GetFile(string filePath);
    }
}
