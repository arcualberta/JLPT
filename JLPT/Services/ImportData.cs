
using Catfish.API.Repository.Interfaces;
using Catfish.API.Repository.Models;
using Catfish.API.Repository.Models.Entities;
using JLPT.Interfaces;

namespace JLPT.Services
{
    public class ImportData : IImportData
    {
        private readonly IEntityTemplateService _entityTemplateService;

        public ImportData(IEntityTemplateService entityTemplateService)
        {
            _entityTemplateService = entityTemplateService;
        }
        public bool PopulateMasterData(Guid templateId)
        {
            try
            {
                EntityTemplate template = _entityTemplateService.GetEntityTemplate(templateId);
                return true;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
