using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JLPT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        public ActionResult PopulateMasterData(Guid id)
        {
            return Ok();
        }
    }
}
