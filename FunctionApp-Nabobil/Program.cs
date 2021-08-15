using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace ReadNabobilFile
{
    class Program
    {
        private static Dictionary<string, Rental> rentals;
        private static DocumentClient _client;
        private static string endpoint = "https://nabobil.documents.azure.com:443/";
        private static string authKey = "MQX99W0Yw612NIB34NlXoUmEY9VVd34pAKn8UTzlhygrvSOFTrXCOpKABiasnEuG3vCRKFDSavirC1WOtfbYBA==";

        public static async Task ImportFile(Stream stream, ILogger log)
        {
            rentals = new Dictionary<string, Rental>();

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            // Get temp file name
            var temp = Path.GetTempPath(); // Get %TEMP% path
            var file = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()); // Get random file name without extension
            var path = Path.Combine(temp, file + ".xlsx"); // Get random file path

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                // Write content of your memory stream into file stream
                memoryStream.WriteTo(fs);
            }

            log.LogInformation($"Path to temp stored file: {path}");

            FileInfo fileInfo = new FileInfo(path);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                //int ColCount = worksheet.Dimension.Columns;

                // Loop once to add all base rentals to rentals-list
                for (int i = 2; i <= rowCount; i++)
                {
                    string id = worksheet.Cells[i, 3].Value.ToString();
                    decimal amount = decimal.Parse(worksheet.Cells[i, 12].Value.ToString());
                    decimal payout = decimal.Parse(worksheet.Cells[i, 17].Value.ToString());

                    switch (worksheet.Cells[i, 11].Value.ToString())
                    {
                        case "base rental":
                            {
                                if(worksheet.Cells[i, 7].Value.ToString().StartsWith("deleted"))
                                    break;

                                int milagePickup = 1;
                                if (worksheet.Cells[i, 8].Value != null)
                                {
                                    Int32.TryParse(worksheet.Cells[i, 8].Value.ToString(), out milagePickup);
                                }

                                int milageDelivery = 2;
                                if (worksheet.Cells[i, 9].Value != null)
                                {
                                    Int32.TryParse(worksheet.Cells[i, 9].Value.ToString(), out milageDelivery);
                                }

                                rentals.Add(id,
                                    new Rental()
                                    {
                                        Amount = amount,
                                        Delivery = DateTime.Parse(worksheet.Cells[i, 5].Value.ToString()),
                                        Pickup = DateTime.Parse(worksheet.Cells[i, 4].Value.ToString()),
                                        Id = id,
                                        MileageDelivery = milageDelivery,
                                        MileagePickup = milagePickup,
                                        AmountNet = payout,
                                        RegistrationNumber = worksheet.Cells[i, 7].Value.ToString()
                                    });
                                break;
                            }
                    }
                }

                // Loop again to calculate all the other stuff except of base rental
                for (int i = 2; i <= rowCount; i++)
                {
                    string id = worksheet.Cells[i, 3].Value.ToString();
                    decimal amount = decimal.Parse(worksheet.Cells[i, 12].Value.ToString());
                    decimal payout = decimal.Parse(worksheet.Cells[i, 17].Value.ToString());

                    string description = worksheet.Cells[i, 11].Value.ToString().ToLower();

                    switch (description)
                    {
                        case "tolls":
                            {
                                if (rentals.ContainsKey(id))
                                    rentals[id].Tolls += payout;
                                break;
                            }
                        case "gasoline":
                            {
                                if (rentals.ContainsKey(id))
                                    rentals[id].Gasoline += payout;
                                break;
                            }
                        case "excess_mileage":
                            {
                                if (rentals.ContainsKey(id))
                                    rentals[id].ExcessMileage += payout;
                                break;
                            }
                        case "trip_fee":
                            {
                                if (rentals.ContainsKey(id))
                                    rentals[id].Amount += amount;
                                break;
                            }
                        case "risk_based_insurance":
                            {
                                if (rentals.ContainsKey(id))
                                    rentals[id].Amount += amount;
                                break;
                            }
                        case "avtale om kilometer":
                            {
                                if (rentals.ContainsKey(id))
                                    rentals[id].ExcessMileage += payout;
                                break;
                            }
                        default:
                            {
                                // Manuell utvidelse eller justering av leiepris som ikke reflekteres i base rental
                                if (rentals.ContainsKey(id) && (description.Contains("utvidelse") || description.Contains("utviden") || description.Contains("sen levering") || description.Contains("leiepris") || description.Contains("forsinket") || description.Contains("forlenge") || description.Contains("timer")))
                                {
                                    rentals[id].Amount += amount;
                                    rentals[id].AmountNet += payout;
                                }
                                else if (rentals.ContainsKey(id) && description != "base rental")
                                    rentals[id].Extra += payout;
                                break;
                            }
                    }
                }
            }

            log.LogInformation("Loaded Excel stuff");

            _client = new DocumentClient(new Uri(endpoint), authKey);

            string databaseId = "Nabobil";
            string collectionId = "Rental";
            Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            List<Task> tasks = new List<Task>();
            Console.WriteLine("Databaseexport start " + DateTime.UtcNow.ToLongTimeString());
            foreach (var rental in rentals.Values)
            {
                tasks.Add(Upsert<Rental>(rental, _client, collectionLink));
            }
            await Task.WhenAll(tasks);
        }

        public static async Task Upsert<T>(T entity, DocumentClient _client, Uri _collectionLink, string etag = null)
        {
            await _client.UpsertDocumentAsync(
                _collectionLink, entity, new RequestOptions
                {
                    AccessCondition = etag == null ? null : new AccessCondition
                    {
                        Condition = etag,
                        Type = AccessConditionType.IfMatch
                    }
                });
        }
    }
}
