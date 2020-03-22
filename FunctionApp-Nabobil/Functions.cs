using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ReadNabobilFile;

namespace FunctionApp_Nabobil
{
    public static class Functions
    {
        [FunctionName("ExcelfilesBlobTrigger")]
        public static async void Run([BlobTrigger("excelfiles-container/{name}", Connection = "NabobilStorageConnection")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            await Program.ImportFile(myBlob, log);
            log.LogInformation($"Completed processing");
        }

        //[FunctionName("ExcelfilesQueueTrigger")]
        //public static void Run([QueueTrigger("uploaded-excelfiles", Connection = "NabobilStorageConnection")]string blobname, ILogger log)
        //{
        //    log.LogInformation($"C# Queue trigger function processed: {blobname}");
        //}
    }
}
