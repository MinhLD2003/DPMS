using DPMS_WebAPI.Exceptions;
using DPMS_WebAPI.Utils;
using FlexCel.Render;
using FlexCel.Report;
using FlexCel.XlsAdapter;
using System.Data;
using System.Reflection;

namespace UNI.Utils
{
    public enum ReportType
    {
        pdf,
        xlsx,
        docx
    }

    public class FlexcelUtils
    {
        public Stream CreateReport(String template, ReportType type, DataTable table, Dictionary<String, Object> parameters = null)
        {
            return CreateReport(File.ReadAllBytes(template), type, table, parameters);
        }
        public Stream CreateReport(String template, ReportType type, DataSet ds, Dictionary<String, Object> parameters = null)
        {
            return CreateReport(File.ReadAllBytes(template), type, ds, parameters);
        }
        public Stream CreateReport(byte[] template, ReportType type, DataTable table, Dictionary<String, Object> parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();

                FlexCelReport report = new FlexCelReport(true);
                report.AddTable("Data", table);
                report.AddTable("Data2", table);

                //Parametters
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }

                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;

                templateStream.Dispose();
                report.Dispose();

                if (type == ReportType.xlsx)
                {
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);

                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);

                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();

                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }

        public Stream CreateReport(byte[] template, ReportType type, DataSet ds, Dictionary<String, Object>? parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                // Set data theo số datatable lấy ra từ store
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        report.AddTable("Data" + i, ds.Tables[i]);
                    }
                }
                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, p.Value);
                    }
                }
                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    //SaveStreamToFile(@"G:\TestReport\" + Guid.NewGuid().ToString() + ".xlsx", outStream);
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }

        public Stream CreateReport(byte[] template, ReportType type, DataSet ds, Dictionary<String, Object> parameters = null, bool validateEmpty = false)
        {
            ValidateDataSet(ds, validateEmpty);
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                // Set data theo số datatable lấy ra từ store
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        report.AddTable("Data" + i, ds.Tables[i]);
                    }
                }
                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }

                report.SetUserFunction("SanitizeInput", new ExcelSecurityHelper());
                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    //SaveStreamToFile(@"G:\TestReport\" + Guid.NewGuid().ToString() + ".xlsx", outStream);
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                if (ex.Message.Contains("not visible sheets on the file") == true)
                {
                    throw new ReportEmptyException();
                }
                throw;
            }
        }

        private static void ValidateDataSet(DataSet data, bool validateEmpty = false)
        {
            if (validateEmpty && (data == null || data.Tables.Count == 0))
            {
                throw new ReportEmptyException();
            }

            //loop through all tables in dataset if all tables are empty then throw exception
            var isEmpty = true;
            var indexValid = -1; // index of table contains valid and message column

            for (var i = 0; i < data.Tables.Count; i++)
            {
                var table = data.Tables[i];
                if (indexValid < 0 && table.Columns?.Count == 2 && table.Columns.Contains("valid") &&
                    (table.Columns.Contains("messages") || table.Columns.Contains("message")) && table.Rows.Count > 0)
                {
                    indexValid = i;
                }
                else if (table.Rows.Count > 0)
                {
                    isEmpty = false;
                }
            }
            // check table in dataset has rows and columns with name valid and message
            // if valid is 0 then throw exception with message

            if (indexValid >= 0)
            {
                var validTable = data.Tables[indexValid];
                var row = validTable.Rows[0];
                if (row["valid"].ToString() == "0")
                {
                    var message = row["messages"].ToString();
                    if (string.IsNullOrEmpty(message))
                    {
                        message = row["message"].ToString();
                    }
                    throw new ReportException(message);
                }
                if (validateEmpty && isEmpty)
                {
                    throw new ReportEmptyException();
                }
            }
            else if (validateEmpty && isEmpty)
            {
                throw new ReportEmptyException();
            }
        }
        public Stream CreateMergeTableReport(byte[] template, ReportType? type, DataSet ds,
            Dictionary<String, Object> parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                var fromValue = parameters != null && parameters.TryGetValue("FromValue", out var outFromValue)
                    ? Convert.ToInt32(outFromValue)
                    : 0;
                var toValue = parameters != null && parameters.TryGetValue("ToValue", out var outToValue)
                    ? Convert.ToInt32(outToValue)
                    : 0;

                //Số cột cố định đầu tiên làm key để merge dòng giữa các bảng
                var fixedColumnCount =
                    parameters != null && parameters.TryGetValue("FixedColumnCount", out var outFixedColumnCount)
                        ? Convert.ToInt32(outFixedColumnCount)
                        : 1;

                //Vị trí của bảng đầu tiên chứa dữ liệu trong DataSet
                var indexFirstDataTable =
                    parameters != null && parameters.TryGetValue("IndexFirstDataTable", out var outIndexFirstDataTable)
                        ? Convert.ToInt32(outIndexFirstDataTable)
                        : 0;
                // Vị trí của  bảng đầu tiên chứa dữ liệu trong DataSet
                var indexHeaderDataTable = parameters != null && parameters.TryGetValue("IndexHeaderTable", out var outIndexHeaderTable)
                    ? Convert.ToInt32(outIndexHeaderTable)
                    : -1; ;

                var delta = (toValue - fromValue) + 1;

                // Set data theo số datatable lấy ra từ store
                if (ds.Tables.Count > 1)
                {
                    if (indexHeaderDataTable >= 0)
                    {
                        //Nhân số cột của bảng header theo fromValue và toValue
                        //Tạo thêm cột để chứa giá trị của từng value
                        var originHeader = ds.Tables[indexHeaderDataTable];
                        originHeader.Columns.Add("DynamicValue", typeof(int));
                        var headerData = originHeader.Clone();
                        foreach (DataRow row in originHeader.Rows)
                        {
                            for (var j = fromValue; j <= toValue; j++)
                            {
                                row["DynamicValue"] = j;
                                headerData.ImportRow(row);
                            }
                        }
                        report.AddTable("Data0", headerData);
                    }

                    //Bảng chứa dữ liệu merge của các bảng qua từng giai đoạn
                    //Khởi tạo cấu trúc cho bảng merge
                    //Xoá hết cột động, chỉ dữ lại các cột cố định đầu tiên
                    var mergedDataTable = ds.Tables[indexFirstDataTable].Clone();
                    while (mergedDataTable.Columns.Count > fixedColumnCount)
                    {
                        mergedDataTable.Columns.RemoveAt(mergedDataTable.Columns.Count - 1);
                    }

                    //Tạo cột động dựa trên tổng số bảng data riêng lẻ
                    var dataColumns = ds.Tables[indexFirstDataTable].Columns;
                    for (var i = fixedColumnCount; i < dataColumns.Count; i++)
                    {
                        var dynamicColumnName = dataColumns[i].ColumnName;
                        for (var j = fromValue; j <= toValue; j++)
                        {
                            var newColumn = new DataColumn(dynamicColumnName + "_" + j);
                            mergedDataTable.Columns.Add(newColumn);
                        }
                    }
                    // for each table from index 1 to the end of the list
                    // copy the data to the new table, if first 3 columns are the same then merge the data

                    for (var i = indexFirstDataTable; i < ds.Tables.Count; i++)
                    {
                        var currentTable = ds.Tables[i];
                        var currentTableIndex = i - indexFirstDataTable;
                        foreach (DataRow row in currentTable.Rows)
                        {
                            var newRow = mergedDataTable.NewRow();

                            //Chép dữ liệu của những cột đầu cố định
                            for (var j = 0; j < fixedColumnCount; j++)
                            {
                                newRow[j] = row[j];
                            }

                            //Chép dữ liệu của những cột động
                            for (var j = fixedColumnCount; j < currentTable.Columns.Count; j++)
                            {
                                newRow[fixedColumnCount + (j - fixedColumnCount) * delta + currentTableIndex] = row[j];
                            }

                            var found = false;
                            foreach (DataRow existsRow in mergedDataTable.Rows)
                            {
                                var same = true;
                                for (var j = 0; j < fixedColumnCount; j++)
                                {
                                    if (existsRow[j].Equals(newRow[j])) continue;
                                    same = false;
                                    break;
                                }

                                if (!same) continue;

                                for (var j = fixedColumnCount; j < currentTable.Columns.Count; j++)
                                {
                                    var updateIndex = fixedColumnCount + (j - fixedColumnCount) * delta + currentTableIndex;
                                    existsRow[updateIndex] = newRow[updateIndex];
                                }

                                found = true;
                                break;
                            }

                            if (!found)
                            {
                                mergedDataTable.Rows.Add(newRow);
                            }
                        }
                    }
                    report.AddTable("Data1", mergedDataTable);
                }

                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }

                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == null || type == ReportType.xlsx)
                {
                    //SaveStreamToFile(@"G:\TestReport\" + Guid.NewGuid().ToString() + ".xlsx", outStream);
                    return outStream;
                }

                XlsFile xls = new XlsFile(true);
                xls.Open(outStream);
                FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                Stream pdfStream = new MemoryStream();
                pdf.Export(pdfStream);
                pdfStream.Position = 0;
                outStream.Dispose();
                pdf.Dispose();
                return pdfStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }
        public Stream CreateReportPDF(byte[] template, ReportType type, DataTable dt1, Dictionary<String, String> parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                report.AddTable("Data0", dt1);
                // Set data theo số datatable lấy ra từ store
                //if (ds.Tables.Count > 0)
                //{
                //    for (int i = 0; i < ds.Tables.Count; i++)
                //    {
                //        report.AddTable("Data" + i.ToString(), ds.Tables[i]);
                //    }
                //}
                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }
                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);

                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }

        public Stream CreateReportPDF_chuky(byte[] template, ReportType type, DataTable dt1, Dictionary<String, String> parameters = null, byte[] template_chuky = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                report.AddTable("Data0", dt1);
                // Set data theo số datatable lấy ra từ store
                //if (ds.Tables.Count > 0)
                //{
                //    for (int i = 0; i < ds.Tables.Count; i++)
                //    {
                //        report.AddTable("Data" + i.ToString(), ds.Tables[i]);
                //    }
                //}
                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }

                report.SetValue("chuky", template_chuky);

                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }

        public Stream CreateStreamPDF(byte[] template, ReportType type, DataTable dt1, Dictionary<String, String> parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }
                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }

        public static ReportType ReportByType(String type)
        {
            switch (type)
            {
                case "pdf":
                    return ReportType.pdf;
                case "docx":
                    return ReportType.docx;
                default:
                    return ReportType.xlsx;
            }
        }
        public string GetTypeFile(ReportType reportType)
        {
            switch (reportType)
            {
                case ReportType.pdf:
                    return "application/pdf";
                case ReportType.xlsx:
                    return "application/vnd.ms-excel";
                default:
                    return "application/vnd.ms-word";
            }
        }

        public Stream ExportCourseYearByPosition(byte[] template,
                                                 ReportType type,
                                                 DataSet ds,
                                                 Dictionary<String, Object> parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                // Set data theo số datatable lấy ra từ store
                if (ds.Tables.Count > 0)
                {
                    //report.AddTable("Data0", ds.Tables[0]);
                    report.AddTable("Data1", ds.Tables[0]);
                    //report.AddTable("Data2", ds.Tables[2]);
                }
                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }
                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    //SaveStreamToFile(@"G:\TestReport\"+Guid.NewGuid().ToString()+".xlsx", outStream);
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }

        public Stream ExportCourseYearByPosition1(byte[] template,
                                                ReportType type,
                                                DataSet ds,
                                                Dictionary<String, Object> parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                // Set data theo số datatable lấy ra từ store
                //if (ds.Tables.Count > 0)
                //{
                //    //report.AddTable("Data0", ds.Tables[0]);
                //    report.AddTable("Data0", ds.Tables[0]);
                //    //report.AddTable("Data2", ds.Tables[2]);
                //}
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        report.AddTable("Data" + i.ToString(), ds.Tables[i]);
                    }
                }

                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }
                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    //SaveStreamToFile(@"c:\TestReport\" + Guid.NewGuid().ToString() + ".xlsx", outStream);
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }

        public void SaveStreamToFile(string fileFullPath, Stream stream)
        {
            if (stream.Length == 0) return;

            // Create a FileStream object to write a stream to a file
            using (FileStream fileStream = System.IO.File.Create(fileFullPath, (int)stream.Length))
            {
                // Fill the bytes[] array with the stream data
                byte[] bytesInStream = new byte[stream.Length];
                stream.Read(bytesInStream, 0, (int)bytesInStream.Length);

                // Use FileStream object to write to the specified file
                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
            }
        }

        private static List<T> ReadToObject<T>(Stream file, int fromRow = 2) where T : new()
        {
            var lst = new List<T>();
            var props = typeof(T).GetProperties();
            file.Position = 0;
            var xls = new XlsFile(file, false)
            {
                IgnoreFormulaText = true
            };
            for (var i = fromRow; i <= xls.RowCount; i++)
            {
                var row = new T();
                var pIdx = 0;
                foreach (var p in props)
                {
                    pIdx++;
                    var attr = ((ExcelAttribute)p.GetCustomAttributes(typeof(ExcelAttribute), false).FirstOrDefault());
                    if (attr?.Ignore == true) continue;
                    var col = attr?.ExcelCol;
                    var val = attr?.IsSequence == true ? i - fromRow + 1 : string.IsNullOrEmpty(col) ? xls.GetCellValue(i, pIdx) : xls.GetCellValue($"{col}{i}");

                    var value = val.ChangeType(p.PropertyType);
                    p.SetValue(row, value);
                }
                lst.Add(row);
            }

            return lst;
        }

        public static List<T> ReadToObject<T>(byte[] file, int fromRow = 2) where T : new()
        {
            using (var mm = new MemoryStream(file))
            {
                return ReadToObject<T>(mm, fromRow);
            }
        }

        public static List<T> ReadToObject1<T>(byte[] file, int fromRow = 2) where T : new()
        {
            using (var mm = new MemoryStream(file))
            {
                return ReadToObject1<T>(mm, fromRow);
            }
        }

        private static List<T> ReadToObject1<T>(Stream file, int fromRow = 2) where T : new()
        {
            var lst = new List<T>();
            var props = typeof(T).GetProperties();
            file.Position = 0;
            var xls = new XlsFile(file, false)
            {
                IgnoreFormulaText = true
            };

            var objectcv = new T();

            var i_row = fromRow;
            var i_col = 1;
            foreach (var p in props)
            {
                var attr = ((ExcelAttribute)p.GetCustomAttributes(typeof(ExcelAttribute), false).FirstOrDefault());
                if (attr?.Ignore == true) continue;
                var valuefind = attr?.ExcelValue;
                if (GetPositionValue(valuefind, xls, ref i_row, ref i_col) == true)
                {
                    var val = attr?.IsColNext == true ? xls.GetCellValue(i_row, i_col + 1) : attr?.IsRowNext == true ? xls.GetCellValue(i_row + 1, i_col) : null;
                    var value = val.ChangeType(p.PropertyType);
                    p.SetValue(objectcv, value);
                }
            }
            lst.Add(objectcv);
            return lst;
        }

        private static bool GetPositionValue(string find, XlsFile xls, ref int rowfind, ref int colfind)
        {
            for (var i = rowfind; i <= xls.RowCount; i++)
            {
                for (int col = 1; col < xls.ColCount; col++)
                {
                    string valuestr = Convert.ToString(xls.GetCellValue(i, col));
                    if (string.Compare(valuestr, find, StringComparison.OrdinalIgnoreCase) == 0 & valuestr != null)
                    {
                        //Console.WriteLine($"Value found at Row: {row + 1}, Column: {col + 1}");
                        rowfind = i;
                        colfind = col;
                        //result = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
    public class ExcelAttribute : Attribute
    {
        public string ExcelCol { get; set; }
        public bool Ignore { get; set; } = false;
        public bool IsSequence { get; set; } = false;
        public string ExcelValue { get; set; }
        public bool IsRowNext { get; set; } = false;
        public bool IsColNext { get; set; } = false;
    }
    public class FlexcellUtils<T>
    {
        public Stream CreateReport(byte[] template, ReportType type, List<T> list, Dictionary<String, Object> parameters = null)
        {
            Stream templateStream = new MemoryStream(template);
            try
            {
                templateStream.Position = 0;
                Stream outStream = new MemoryStream();
                FlexCelReport report = new FlexCelReport(true);
                // Set data theo số datatable lấy ra từ store

                report.AddTable("Data0", list);

                //Parametters 
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        report.SetValue(p.Key, ExcelSecurityHelper.SanitizeInput(p.Value));
                    }
                }
                templateStream.Position = 0;
                report.Run(templateStream, outStream);
                outStream.Position = 0;
                templateStream.Dispose();
                report.Dispose();
                if (type == ReportType.xlsx)
                {
                    //SaveStreamToFile(@"G:\TestReport\" + Guid.NewGuid().ToString() + ".xlsx", outStream);
                    return outStream;
                }
                else
                {
                    XlsFile xls = new XlsFile(true);
                    xls.Open(outStream);
                    FlexCelPdfExport pdf = new FlexCelPdfExport(xls);
                    Stream pdfStream = new MemoryStream();
                    pdf.Export(pdfStream);
                    pdfStream.Position = 0;
                    outStream.Dispose();
                    pdf.Dispose();
                    return pdfStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                templateStream.Dispose();
                throw;
            }
        }
    }

    //Hàm bảo mật cho excel kiểm tra dữ liệu xem có phải là công thức/macro hay không tự động thêm dấu ' vào trước để tránh thực thi
    public class ExcelSecurityHelper : TFlexCelUserFunction
    {
        public static object SanitizeInput(object input)
        {
            if (input is string stringInput)
            {
                if (string.IsNullOrEmpty(stringInput))
                {
                    return stringInput;
                }

                var firstChar = stringInput[0];
                if (firstChar == '=' || firstChar == '@' || firstChar == '+' || firstChar == '-')
                {
                    return "'" + stringInput;
                }

                return stringInput;
            }

            return input;
        }

        public override object Evaluate(object[] parameters)
        {
            return SanitizeInput(parameters[0]);
        }
    }
}