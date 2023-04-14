namespace JLPT
{
    public class UserInfo
    {
     
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string TestLevel { get; set; }

        public string Email { get; set; }
        public string SequenceNumber { get; set; }
        public bool IsEmailSent { get; set; } = false;
    }
}
