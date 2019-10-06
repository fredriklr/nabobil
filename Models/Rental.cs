using ITDS.Core.Repository.CosmosDb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Rental: AbstractCosmosDbEntity
    {
        [JsonProperty("id")]
        public override string Id { get; set; }
        [JsonProperty("registrationNumber")]
        public string RegistrationNumber { get; set; }
        [JsonProperty("pickup")]
        public DateTime Pickup { get; set; }
        [JsonProperty("delivery")]
        public DateTime Delivery { get; set; }
        [JsonProperty("month")]
        public int Month => Delivery.Month;
        [JsonProperty("year")]
        public int Year => Delivery.Year;
        [JsonProperty("tolls")]
        public decimal Tolls { get; set; }
        [JsonProperty("mileage")]
        public int Mileage => MileageDelivery - MileagePickup;
        [JsonProperty("mileagePickup")]
        public int MileagePickup { get; set; }
        [JsonProperty("mileageDelivery")]
        public int MileageDelivery { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("amountNet")]
        public decimal AmountNet { get; set; }
        [JsonProperty("excessMileage")]
        public decimal ExcessMileage { get; set; }
        [JsonProperty("gasoline")]
        public decimal Gasoline { get; set; }
        [JsonProperty("extra")]
        public decimal Extra { get; set; }
    }
}
