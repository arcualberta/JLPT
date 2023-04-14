using JLPT.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JLPTProcessor.Pages
{
    public class ImportDataModel : PageModel
    {
        //private readonly ILogger<VoucherModel> _logger;
        private readonly IUserDataInterface _userData;

        public ImportDataModel( IUserDataInterface userData)
        {
            _userData = userData;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string TestLevel { get; set; }

        public string Email { get; set; }
        public string SequenceNumber { get; set; }
        public bool IsEmailSent { get; set; } = false;
        public void OnGet()
        {
        }
        public void OnPostPopulateUserData()
        {
            _userData.PopulateUserData();
  

        }

    }
}