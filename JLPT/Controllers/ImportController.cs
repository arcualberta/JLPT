using JLPT.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JLPT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IImportData _importDataSrv;

        public ImportController(IImportData importData)
        {
            _importDataSrv = importData;
        }

        [HttpPost]
        public ActionResult PopulateMasterData(Guid id)
        {
            return Ok();
        }

        
        [HttpPatch]
        public ActionResult AddReportTemplatePatch(Guid id, string reportType)
        {
            _importDataSrv.AddReportFormTemplate(id, reportType);
            return Ok();
        }
    }
}
