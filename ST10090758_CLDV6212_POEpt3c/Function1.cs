using System;
using System.IO;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.WebJobs;

using Microsoft.Azure.WebJobs.Extensions.Http;

using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Microsoft.Azure.WebJobs.Extensions.Tables;

using Microsoft.Azure.Cosmos.Table;

using Microsoft.Azure.Documents;

using Microsoft.Extensions.Primitives;

using System.Net;

using System.Net.Http;

using System.Collections.ObjectModel;

namespace ST10090758_CLDV6212_POEpt3c
{
    public static class Function1
    {
        [FunctionName("ID")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Queue("partc-outqueue"), StorageAccount("azurewebjobsstorage")] ICollector<string> msg,
            [Table("vaccinationData"), StorageAccount("azurewebjobsstorage")] IAsyncCollector<VaxData> table
            /*[Table(tableName: "vaccinationTable")] CloudTable table*/, ILogger log)
        {
            // parallel arrays to store dummy info for specific id numbers
            string[] validId = { "1234567891011", "1213141516171", "8192021222324" };
            string[] vaccine = { "1st Dose", "1st Dose\n\t 2nd Dose", "1st Dose" };
            string[] manufacturer = { "Pfizer", "Johnsons&Johnsons", "Zymergen" };
            string[] clinicSite = { "Helen Joseph", "Flora Clinic", "Dischem" };

            //string message = "123:dhsjkhd:23678:jdak";
            //var m = message.Split(':');

            log.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id = id ?? data?.id;

            string responseMessage = null;

            //if the string is null i.e., no id is entered then a prompt notifies the user to enter an ID
            if (string.IsNullOrEmpty(id) == true)
            {
                responseMessage = "This HTTP triggered function executed successfully. Pass a valid ID number in the query string to view vaccination data.";

                return new OkObjectResult(responseMessage);//return the string that has been updated to be displayed to user
            }
            else// if the string is not null then the entered id is searched against an array that stores valid IDs
            {
                for (int i = 0; i < validId.Length; i++)//iterate through the array to find a valid id
                {
                    if (id.Equals(validId[i]))
                    {

                        //if a match is found then display the vaccination data for that id number
                        responseMessage = $"Vaccination data for ID {id}:\nVaccine: {vaccine[i]}\nProductName/Manufacturer Lot number: {manufacturer[i]}\nDate: {DateTime.Now}\nHealthcare Professional/Clinic Site: {clinicSite[i]}";
                        msg.Add($"{responseMessage}");

                        var vaxObj = new VaxData()
                        {
                            PartitionKey = Guid.NewGuid().ToString(),
                            RowKey = Guid.NewGuid().ToString(),
                            SA_Id = validId[i],
                            Vaccine_Dose = vaccine[i],
                            Manufacturer = manufacturer[i],
                            Clinic_Site = clinicSite[i]

                        };


                        await table.AddAsync(vaxObj);
                        //table storage code. Insert
                        //var vax = new VaxData { PartitionKey = id, RowKey = responseMessage };
                        //var op = TableOperation.Insert(vax);


                        i = validId.Length;

                    }
                    else
                    {
                        //if no match is found then notify the user
                        responseMessage = $"ID number {id} is invalid";
                    }
                }
                return new OkObjectResult(responseMessage);//return the string that has been updated to be displayed to user
            }




        }
    }

    public class VaxData : TableEntity
    {
        public string SA_Id { get; set; }
        public string Vaccine_Dose { get; set; }
        public string Manufacturer { get; set; }
        public string Clinic_Site { get; set; }

    }

}
