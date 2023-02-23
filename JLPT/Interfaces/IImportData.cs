using Microsoft.AspNetCore.Mvc;

namespace JLPT.Interfaces
{
    public interface IImportData
    {
        public System.Net.HttpStatusCode PopulateMasterData(IFormFile file);

        public System.Net.HttpStatusCode AddReportFormTemplate(Guid entityTemplateId, string reportType);
    }
}
