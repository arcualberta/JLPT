using JLPT.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JLPTProcessor.Pages
{
    public class VoucherModel : PageModel
    {
        //private readonly ILogger<VoucherModel> _logger;
        private readonly IUserDataInterface _userData;

        public VoucherModel( IUserDataInterface userData)
        {
            _userData = userData;
        }
       /* public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string TestLevel { get; set; }

        public string Email { get; set; }
        public string SequenceNumber { get; set; }
        public bool IsEmailSent { get; set; } = false;*/
       public List<JLPT.UserInfo>Users { get; set; }

        [BindProperty]
        public List<int> SelectedUserIds { get; set; }

        [BindProperty(SupportsGet = true)]
        public int MaxItems { get; set; } = 25;
        public void OnGet()
        {
            Users = _userData.GetUserData(MaxItems);
        }
        public void OnPostPopulateUserData()
        {
            _userData.PopulateUserData();
        }
        public void OnPostSendEmail()
        {
            _userData.SendEmails(SelectedUserIds);

            Users = _userData.GetUserData(MaxItems);
        }

        public void OnPostSendTestEmail()
        {
            _userData.SendEmails(SelectedUserIds, true);//set true for "testEmail"

            Users = _userData.GetUserData(MaxItems);
        }
        public void SelectAll()
        {
            Users = _userData.GetUserData(MaxItems);

            foreach(var usr in Users)
            {
                SelectedUserIds.Add(usr.Id);
            }
        }
    }
}