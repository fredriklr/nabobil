namespace CarRentalUi.Models.ViewModels
{
    public class AggregatedData
    {
        public decimal NumberOfDaysRented { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountExTax { get; set; }
        public int NumberOfBookings { get; set; }
        public decimal AverageDaysPerRent { get; set; }
        public decimal AverageAmountPerDays { get; set; }
        public decimal AverageMilagePerRent { get; set; }
        public decimal AverageMilagePerDay { get; set; }
        public decimal AveragePricePerKilometer { get; set; }
        public decimal TotalTaxResult { get; set; }
    }
}