
using Catfish.API.Repository.Constants;
using Catfish.API.Repository.DTO;
using Catfish.API.Repository.Interfaces;
using Catfish.API.Repository.Models;
using Catfish.API.Repository.Models.Entities;
using Catfish.API.Repository.Models.Forms;
using ExcelDataReader;
using JLPT.Interfaces;
using System.Data;
using System.Net;

namespace JLPT.Services
{
    public class ImportData : IImportData
    {
        private readonly IEntityTemplateService _entityTemplateService;
        private readonly JLPTDbContext _context;
        private readonly string PRIMARYSHEET = "Orders";
        private readonly string PIVOTCOLUMN = "Email";
        private readonly string ORDERSTATUS = "Stripe Paid";
        public ImportData(JLPTDbContext context,IEntityTemplateService entityTemplateService)
        {
            _entityTemplateService = entityTemplateService;
            _context = context;
        }
        public HttpStatusCode PopulateMasterData(Guid templateId, IFormFile file)
        {
            try
            {
                EntityTemplate template = _entityTemplateService.GetEntityTemplate(templateId);
                if (template == null)
                    return HttpStatusCode.NotFound;

                FormTemplate primaryFormTemplate = template.Forms.Where(f => f.Id == template!.EntityTemplateSettings!.PrimaryFormId).FirstOrDefault();

                if (primaryFormTemplate == null)
                    return HttpStatusCode.NotFound;

               

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                //JLPT only care for "Attendees, Questions and Orders tabs on the worksheets
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;


                    using (var excelDataReader = ExcelReaderFactory.CreateReader(stream))
                    {
                        //  IExcelDataReader excelDataReader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);
                        var conf = new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = a => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                        };
                        DataSet dataSet = excelDataReader.AsDataSet(conf);


                        DataRowCollection rows = dataSet!.Tables[PRIMARYSHEET]!.Rows;
                        int statusIndex = GetOrderStatusIndex(dataSet);


                        for (int i = 0; i < rows.Count; i++)//foreach (DataRow row in rows)
                        {
                            //TODO:
                            //1. check if the order's status == "Stripe Paid"
                            if (rows[i].ItemArray![statusIndex]!.ToString() == ORDERSTATUS)
                            {
                                // var cols = rows[i].ItemArray; //get all cols in that row
                                EntityData entityData = CreateEntityData(templateId, primaryFormTemplate.Id, dataSet, template.Forms.ToList(), rows[i], eEntityType.Item);

                                //save to the db
                                _context!.Entities!.Add(entityData);
                            }

                        }
                        _context.SaveChanges();
                    }
                }

                return HttpStatusCode.OK;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
      /*  private HttpStatusCode ImportDataFromExcel(Guid templateId, string pivotColumnName, IFormFile file)
        {

            EntityTemplate entityTemplate = _entityTemplateService.GetEntityTemplate(templateId);
            if (entityTemplate == null)
                return HttpStatusCode.NotFound;

            FormTemplate primaryFormTemplate = entityTemplate.Forms.Where(f => f.Id == entityTemplate!.EntityTemplateSettings!.PrimaryFormId).FirstOrDefault();

            if (primaryFormTemplate == null)
                return HttpStatusCode.NotFound;

            string primarySheetName = primaryFormTemplate!.Name!;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;


                using (var excelDataReader = ExcelReaderFactory.CreateReader(stream))
                {
                    //  IExcelDataReader excelDataReader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);
                    var conf = new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = a => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };
                    DataSet dataSet = excelDataReader.AsDataSet(conf);


                    DataRowCollection rows = dataSet!.Tables[primarySheetName]!.Rows;

                    //Check if the number of fields in the templates is same with the number of column on the sheet
                    if (primaryFormTemplate.Fields!.Count != rows[0].ItemArray!.Count())
                        return HttpStatusCode.BadRequest;

                    for (int i = 0; i < rows.Count; i++)//foreach (DataRow row in rows)
                    {
                        // var cols = rows[i].ItemArray; //get all cols in that row
                        EntityData entityData = CreateEntityData(templateId, primaryFormTemplate.Id, dataSet, entityTemplate.Forms.ToList(), rows[i], eEntityType.Item, pivotColumnName);

                        //save to the db
                        _context!.Entities!.Add(entityData);


                        //DEBUG  ONLY!!!!
                        //   if (i == 1)
                        //     break;//ONLY PROCESS 2 ROWS
                    }
                    _context.SaveChanges();
                }
            }

            return HttpStatusCode.OK;
        }
      */
        private EntityTemplate ImportEntityTemplateSchema(string templateName, string primaryFormName, IFormFile file)
        {
            try
            {
                EntityTemplate template;

                //Reaad the excel file.
                // DataSet dataSet = GetSheetData(file);

                //assuming the haeder is in 1st row
                template = CreateEntityTemplate(templateName, primaryFormName, file);

                _context.EntityTemplates!.Add(template);
                _context.SaveChanges();
                return template;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        private FormTemplate CreateFormTemplate(DataRow headerRow, string currentSheetName)
        {
            FormTemplate template = new FormTemplate();

            template.Id = Guid.NewGuid();
            template.Name = currentSheetName;
            template.Description = "This form template was created from excel sheet '" + currentSheetName + "'";
            template.Created = DateTime.Now;
            template.Updated = DateTime.Now;

            IList<Field> fields = new List<Field>();
            foreach (string colValue in headerRow.ItemArray.ToList())
            {
                Field field = new Field();
                field.Id = Guid.NewGuid();
                field.Type = FieldType.ShortAnswer;
                TextCollection textCol = new TextCollection();
                textCol.Id = Guid.NewGuid();
                Text txtVal = new Text() { Id = Guid.NewGuid(), Lang = "en", Value = colValue, TextType = eTextType.ShortAnswer };
                textCol.Values = new Text[1];
                textCol.Values[0] = txtVal;
                field.Title = textCol;

                fields.Add(field);
            }

            template.Fields = fields;
            return template;
        }
        private EntityTemplate CreateEntityTemplate(string templateName, string primarySheetName, IFormFile file)
        {
            //Create a new FormTemplate
            EntityTemplate template = new EntityTemplate();
            template.Id = Guid.NewGuid();
            template.Name = templateName;
            template.Description = "Entity template created from excel workbook";
            template.Created = DateTime.Now;
            template.Updated = DateTime.Now;
            template.State = eState.Draft;
            template.Forms = new List<FormTemplate>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            //DataRowCollection rows;
            DataSet dataSet;
            EntityTemplateSettings templateSettings = new EntityTemplateSettings();
            templateSettings.DataForms = new FormEntry[] { };
            List<FormEntry> dataForms = new List<FormEntry>();
            List<FormTemplate> formTemplates = new List<FormTemplate>();


            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;

                using (var excelDataReader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = a => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = false //need the header
                        }
                    };
                    dataSet = excelDataReader.AsDataSet(conf);
                    for (int i = 0; i < excelDataReader.ResultsCount; i++)
                    {
                        var sheetName = excelDataReader.Name;

                        DataRowCollection rows = dataSet!.Tables[sheetName]!.Rows;

                        //assuming the 1st row is the header
                        FormTemplate formTemplate = CreateFormTemplate(rows[0], sheetName);

                        FormEntry entry = new FormEntry();
                        entry.Id = formTemplate.Id;
                        entry.IsRequired = sheetName.ToLower() == primarySheetName.ToLower() ? true : false;
                        entry.Name = formTemplate!.Name!;
                        entry.State = formTemplate.Status;
                        dataForms.Add(entry);

                        //STILL NEED FIELD MAPPING!!!!

                        templateSettings.TitleField = new FieldEntry() { FieldId = formTemplate.Fields![0].Id, FormId = formTemplate.Id };
                        templateSettings.DescriptionField = new FieldEntry() { FieldId = formTemplate.Fields![1].Id, FormId = formTemplate.Id };
                        if (formTemplate!.Name!.ToLower() == primarySheetName.ToLower())
                            templateSettings.PrimaryFormId = formTemplate.Id;

                        formTemplate.EntityTemplates.Add(template);
                        _context.Forms!.Add(formTemplate);


                        formTemplates.Add(formTemplate);

                        //read next sheet
                        excelDataReader.NextResult();

                    }
                }
            }
            templateSettings.DataForms = dataForms.ToArray();

            template.EntityTemplateSettings = templateSettings;
            template.Forms = formTemplates;
            return template;
        }

        private int GetPivotColumnIndex(DataSet data, string sheetName, string pivotColumn)
        {
            int? ind = data!.Tables[sheetName]!.Columns[pivotColumn] == null ? -1 : data!.Tables[sheetName]!.Columns[pivotColumn]?.Ordinal;

            return ind.Value;
        }
        private int GetOrderStatusIndex(DataSet data)
        {
            int? ind = data!.Tables["Orders"]!.Columns["status"]!.Ordinal;

            return ind.Value;
        }
        private EntityData CreateEntityData(Guid templateId, Guid primaryFormId, DataSet dataSet, List<FormTemplate> forms, DataRow primaryRow, eEntityType eEntityType)
        {
            EntityData entity = new EntityData();
            entity.Id = Guid.NewGuid();
            entity.TemplateId = templateId;
            entity.EntityType = eEntityType;

            FormTemplate primaryForm = forms.Where(f => f.Id == primaryFormId).FirstOrDefault();
            int pivotColumIndex = GetPivotColumnIndex(dataSet, primaryForm!.Name!, PIVOTCOLUMN);

            entity.Title = primaryRow!.ItemArray[0].ToString();// primaryRow!.ItemArray[pivotColumIndex]!.ToString();
            entity.Description = primaryRow!.ItemArray[pivotColumIndex].ToString();//pivot column should not be empty
            entity.State = eState.Active; //??
            entity.Created = DateTime.Now;
            entity.Updated = DateTime.Now;

            List<FormData> formData = new List<FormData>();

            //get primary form content
            FormData data = new FormData();
            data.FormId = primaryFormId; //primaryFormId
            data.Created = DateTime.Now;
            data.Updated = DateTime.Now;
            data.Id = Guid.NewGuid();


            string pivotColumnValue = "";
            pivotColumnValue = GetPivotColumnValue(primaryRow, primaryForm!.Name!, pivotColumIndex);
            data.FieldData = CreateFieldData(primaryForm!.Fields!.ToList(), primaryRow);
            formData.Add(data);


            //Get child form data
            foreach (FormTemplate form in forms)
            {
                if (form.Id != primaryFormId)
                {
                    //Check if the sheet contain the pivot colum -- if not don't import the data
                    // if no pivot column found, it will return -1
                    pivotColumIndex = GetPivotColumnIndex(dataSet, form.Name!, PIVOTCOLUMN);//have recheck in case the pivot column not in the same position in different sheet

                    if (pivotColumIndex > -1)
                    {
                        var selectedRows = GetChildFormRows(dataSet, form.Name!, pivotColumIndex, pivotColumnValue);

                        foreach (DataRow row in selectedRows)
                        {
                            FormData dt = new FormData();
                            dt.FormId = form.Id;
                            dt.Created = DateTime.Now;
                            dt.Updated = DateTime.Now;
                            dt.Id = Guid.NewGuid();

                            dt.FieldData = CreateFieldData(form.Fields!.ToList(), row);

                            formData.Add(dt);
                        }
                    }
                }
            }

            entity.Data = formData;
            return entity;

        }

        private string GetPivotColumnValue(DataRow row, string sheetName, int pivotColumnIndex)
        {
            string val = "";
            val = row.ItemArray[pivotColumnIndex]!.ToString();

            return val;
        }
        private DataRow? GetChildFormRow(DataSet dataset, string sheetName, string pivotColumnValue)
        {

            DataRowCollection rows = dataset.Tables[sheetName]!.Rows;
            object[] condition = new object[1];
            condition[0] = pivotColumnValue;

            DataRow selectedRow = rows.Find(condition[0]);
            return selectedRow;
        }

        private IEnumerable<DataRow> GetChildFormRows(DataSet dataset, string sheetName, int pivotColumIndex, string pivotColumnValue)
        {

            DataRowCollection rows = (dataset.Tables[sheetName]!.Rows);

            var selectedRows = rows.Cast<DataRow>().Where(r => r[pivotColumIndex].ToString().ToLower() == pivotColumnValue.ToLower());
            return selectedRows;
        }
        private List<FieldData> CreateFieldData(List<Field> fields, DataRow row)
        {
            List<FieldData> dataList = new List<FieldData>();

            for (int i = 0; i < fields.Count; i++)
            {
                FieldData fldData = new FieldData();
                fldData.Id = Guid.NewGuid();
                fldData.FieldId = fields[i].Id;

                string colValue = row!.ItemArray[i]!.ToString();
                TextCollection textCol = new TextCollection();
                textCol.Id = Guid.NewGuid();
                Text txtVal = new Text() { Id = Guid.NewGuid(), Lang = "en", Value = colValue, TextType = eTextType.ShortAnswer };
                textCol.Values = new Text[1];
                textCol.Values[0] = txtVal;
                fldData.MultilingualTextValues = new TextCollection[1];

                fldData.MultilingualTextValues[0] = textCol;

                dataList.Add(fldData);
            }


            return dataList;
        }
        private string getRegistrationReportHeaders()
        {
            return "\"Test Level\",\"Year Code\",\"Test Site\",\"Test Level\",\"Sequence No.\",\"Full Name\",\"Sex\",\"DOB Year\",\"DOB Month\",\"DOB Day\",\"Password\",\"Native Language\",\"Learning Place\",\"Reason\",\"Occupation\",\"Occupation Details\",\"Media\",	\"With Teachers\",	\"With Friends\",\"With Family\",\"With Supervisor\",\"With Colleagues\",\"With Customers\",\"Times Taking N1\",\"Times Taking N2\",\"Times Taking N3\",\"Times Taking N4\",\"Times Taking N5\",\"Latest N1 Result\",\"Latest N2 Result\",\"Latest N3 Result\",\"Latest N4 Result\",\"Latest N5 Result\"";
        }

        private string getMasterReportHeaders()
        {
            return "\"Sequence No.\",\"Level\",\" First Name\", \"Last Name\",\"DOB Year\",\"DOB Month\",\"DOB Day\",\"Street\",\"City\",\"Province\",\"Country\",\"Postal\",\"Email\",	\"Affiliation\",\" Registration Type\",\"Times Taking N1\",\"Times Taking N2\",\"Times Taking N3\",\"Times Taking N4\",\"Times Taking N5\",\"Latest N1 Result\",\"Latest N2 Result\",\"Latest N3 Result\",\"	Latest N4 Result\",\"Latest N5 Result\"";
        }
        private int getLangCode(string lang)
        {
            int code = 0;
            switch (lang)
            {
                case "Akan":
                    code = 602;
                    break;
                case "Amharic":
                    code = 602;
                    break;
                case "Arabic":
                    code = 701;
                    break;
                case "Ashanti":
                    code = 629;
                    break;
                case "Bambara":
                    code = 604;
                    break;
                case "Bemba":
                    code = 605;
                    break;
                case "Berber":
                    code = 606;
                    break;
                case "Chichewa":
                    code = 607;
                    break;
                case "Efik":
                    code = 608;
                    break;
                case "English":
                    code = 408;
                    break;
                case "Ewe":
                    code = 609;
                    break;
                case "French":
                    code = 411;
                    break;
                case "Fulani":
                    code = 610;
                    break;
                case "Ga":
                    code = 611;
                    break;
                case "Galla":
                    code = 612;
                    break;
                case "Hausa":
                    code = 613;
                    break;
                case "Ibo":
                    code = 614;
                    break;
                case "Kikongo":
                    code = 631;
                    break;
                case "Kikuyu":
                    code = 613;
                    break;
                case "Kinya Ruanda":
                    code = 632;
                    break;
                case "Kiswahili":
                    code = 616;
                    break;
                case "Lingala":
                    code = 617;
                    break;

                default:
                    code = 000;
                    break;

            }



            return code;
        }

        private string getSingleSelectedCode(string selected) // separator "-"
        {
            string code = "";
            if (selected.Contains("-"))
            {
                string[] temp = selected.Split('-');
                code = temp[0].Trim();
            }

            return code;

        }
        private string getMultiSelectedCodes(string selected, int arrLength/*total options in this question*/) // separator "-"
        {

            string selectedCodes = "";
            string[] temp = selected.Split(',');
            string[] codes = new string[arrLength];
            //padd the ansswers
            for (int i = 0; i < arrLength; i++)
            {
                //initialize with empty
                codes[i] = " ";
            }
            for (int i = 0; i < temp.Length; i++)
            {
                string strCode = getSingleSelectedCode(temp[i]);
                codes[Convert.ToInt16(strCode) - 1] = strCode;
            }
            selectedCodes = string.Join("", codes);
            return selectedCodes;

        }
        private FormTemplate CreateFormTemplate(string reportType)
        {
            FormTemplate template = new FormTemplate();

            template.Id = Guid.NewGuid();
            List<string> headers = new List<string>();

            if (reportType.Equals("master", StringComparison.CurrentCultureIgnoreCase))
            {
                template.Name = "Master Report";
                template.Description = "This form template was for JLPT Master Report  template";
                headers=(getMasterReportHeaders().Split(",")).ToList();
            }
            else
            {
                template.Name = "Registration Report";
                template.Description = "This form template was for JLPR Registration report template";
                headers = (getRegistrationReportHeaders().Split(",")).ToList();
            }

           
          
            template.Created = DateTime.Now;
            template.Updated = DateTime.Now;

            IList<Field> fields = new List<Field>();
            foreach (string colValue in headers)
            {
                Field field = new Field();
                field.Id = Guid.NewGuid();
                field.Type = FieldType.ShortAnswer;
                TextCollection textCol = new TextCollection();
                textCol.Id = Guid.NewGuid();
                Text txtVal = new Text() { Id = Guid.NewGuid(), Lang = "en", Value = colValue, TextType = eTextType.ShortAnswer };
                textCol.Values = new Text[1];
                textCol.Values[0] = txtVal;
                field.Title = textCol;

                fields.Add(field);
            }

            template.Fields = fields;
            return template;
        }
        public HttpStatusCode AddReportFormTemplate(Guid entityTemplateId, string reportType)
        {
            try
            {
                EntityTemplate template = _entityTemplateService.GetEntityTemplate(entityTemplateId);
                if (template == null)
                    return HttpStatusCode.NotFound;


                FormTemplate formTemplate = CreateFormTemplate(reportType);

                template.Forms.Add(formTemplate);

                _context.Entry(template).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();

                return HttpStatusCode.OK;
            }
            catch(Exception ex)
            {
                return HttpStatusCode.BadRequest;
            }
        }
    }
}
