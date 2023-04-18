namespace JLPT.Services
{
    public interface IUserDataInterface
    {
        public void PopulateUserData();
        public List<UserInfo> GetUserData(int maxItems);
        public List<UserInfo> GetSelectedUserData(List<int> selectedUserIds);

        public bool SendEmails(List<int> selectedUserIds, bool testEmail=false);
    }
}
