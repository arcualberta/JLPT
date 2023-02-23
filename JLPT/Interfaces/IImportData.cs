using Microsoft.AspNetCore.Mvc;

namespace JLPT.Interfaces
{
    public interface IImportData
    {
        public bool PopulateMasterData(Guid templateId);
    }
}
