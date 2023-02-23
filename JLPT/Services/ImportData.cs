
using ExcelDataReader;
using JLPT.Interfaces;
using System.Data;
using System.Net;

namespace JLPT.Services
{
    public class ImportData : IImportData
    {
        private readonly string PRIMARYSHEET = "Orders";
        private readonly string PIVOTCOLUMN = "Email";
        private readonly string ORDERSTATUS = "Stripe Paid";
        public ImportData()
        {
        }
        public HttpStatusCode PopulateMasterData(IFormFile file)
        {
            try
            {

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

                                //save to the db
                            }

                        }
                    }
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

        public HttpStatusCode AddReportFormTemplate(Guid entityTemplateId, string reportType)
        {
            throw new NotImplementedException();
        }
    }
}
