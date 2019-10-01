using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using Bytescout.PDFExtractor;
using PdfParser.Db;
using File = System.IO.File;

namespace PdfParser
{
    internal class Program
    {
        private static readonly D11PdfDataEntities Db = new D11PdfDataEntities();
        private static void Main()
        {
            var sw = default(StreamWriter);
            try
            {
                var fileDir = new DirectoryInfo(ConfigurationManager.AppSettings["DirLocation"] + @"\Log\");
                if (!fileDir.Exists)
                    fileDir.Create();

                sw = File.Exists(fileDir.FullName + "TrackLog.txt")
                    ? File.AppendText(fileDir.FullName + "TrackLog.txt")
                    : File.CreateText(fileDir.FullName + "TrackLog.txt");
                sw.WriteLine("======================= Schedular Started, Dated : " + DateTime.Now + " ==========================");

                ReadDownloadedFile(sw);
            }
            catch (Exception ex)
            {
                if (sw != null)
                {
                    sw.WriteLine("Main - " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    sw.WriteLine(ex.InnerException?.Message ?? ex.Message + " - " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                }
            }
            finally
            {
                if (sw != null)
                {
                    sw.WriteLine("======================= SchedularEnded, Dated : " + DateTime.Now + " ==========================");
                    sw.Close();
                    sw.Dispose();
                }
            }
        }


        /// <summary>
        /// Read downloaded files and insert records in database
        /// </summary>
        /// <param name="sw">Stream Writer</param>
        /// <returns>Void</returns>
        public static void ReadDownloadedFile(StreamWriter sw)
        {
            var path = new DirectoryInfo(ConfigurationManager.AppSettings["DirLocation"] + @"\teampdfs\");
            var downloadDirectory = new DirectoryInfo(path.ToString());
            if (downloadDirectory.Exists)
            {
                var files = downloadDirectory.GetFiles();

                foreach (var file in files)
                {
                    try
                    {
                        var fInfo = new FileInfo(file.ToString());
                        var newFile = ParsePdf(file, sw);
                        if (!newFile)
                        {
                            sw.WriteLine("Error in File  Processed : " + fInfo.Name);
                            Console.WriteLine("Error in File  Processed : " + fInfo.Name);

                        }
                        else
                        {
                            sw.WriteLine("File  Processed : " + fInfo.Name);
                            Console.WriteLine("File  Processed : " + fInfo.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine(" ReadDownloadedFile  - " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        sw.WriteLine(ex.Message + " - " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        Console.WriteLine(ex.Message + " - " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    }

                }
            }
        }

        /// <summary>
        /// Parse Pdf and save into xlsx format
        /// </summary>
        /// <param name="file">File info for parse</param>
        /// <param name="sw">stream writer </param>
        /// <returns>File info of parsed file</returns>
        public static bool ParsePdf(FileInfo file, StreamWriter sw)
        {
            try
            {
                var names = file.Name.Replace(".pdf", "").Split('-');
                string teamName, value, matchName = names[0], matchId = names[1];
                int columnCount, rowCount, teamNo = 0, count = 0;

                var extractor = new StructuredExtractor
                {
                    RegistrationName = "demo",
                    RegistrationKey = "demo"
                };

                // Load sample PDF document
                extractor.LoadDocumentFromFile(file.FullName);

                for (var pageIndex = 0; pageIndex < extractor.GetPageCount(); pageIndex++)
                {
                    Console.WriteLine("Starting extraction from page #" + pageIndex);
                    extractor.PrepareStructure(pageIndex);
                    rowCount = extractor.GetRowCount(pageIndex);

                    for (var row = 0; row < rowCount; row++)
                    {
                        if (row < 2) continue;
                        try
                        {

                            teamName = "";
                            columnCount = extractor.GetColumnCount(pageIndex, row);

                            for (var col = 0; col < columnCount; col++)
                            {
                                value = extractor.GetCellValue(pageIndex, row, col);
                                if (string.IsNullOrEmpty(value))
                                    continue;
                                if (value.Contains("(") && value.Contains(")") && col == 0)
                                {
                                    teamName = value.Substring(0, value.LastIndexOf("("));
                                    teamNo = value.Substring(value.IndexOf("(") + 1, value.Length - (value.IndexOf(")"))).ToInt();
                                    break;
                                }

                                if (col != 0) continue;
                                while (!value.Contains("(") && !value.Contains(")") && count < 5)
                                {
                                    value += $" {extractor.GetCellValue(pageIndex, ++row, col)}";
                                    value = value.Replace("  ", " ");
                                    count++;
                                }
                                count = 0;
                                teamName = value.Substring(0, value.LastIndexOf("(")).Trim();
                                teamNo = value.Substring(value.IndexOf("(") + 1, value.Length - (value.IndexOf(")"))).ToInt();
                                break;
                            }

                            Db.MatchDatas.Add(new MatchData()
                            {
                                LeagueId = matchId.ToInt(),
                                MatchName = matchName,
                                TeamName = teamName,
                                TeamNo = teamNo
                            });
                            Db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            sw.WriteLine("--------------------------------------------------------------------------------------");
                            sw.WriteLine(extractor.GetCellValue(pageIndex, row, 0));
                            sw.WriteLine("Error in file roe no :"+row+"--------" + file.Name);
                            sw.WriteLine(e.GetBaseException().Message);
                            sw.WriteLine("--------------------------------------------------------------------------------------");
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetBaseException().Message);
                sw.WriteLine(e.GetBaseException().Message);
            }
            return false;
        }
    }
}
