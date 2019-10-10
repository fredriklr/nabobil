using CarRentalUi.Models.ViewModels;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CarRentalUi.Controllers
{
    public class RentalController : Controller
    {
        private static readonly decimal TAX_EXPENSE_PER_KM = 2.8m;
        private static readonly decimal TAX_PERCENTAGE = 0.22m;

        // GET: Rental
        public async Task<ActionResult> Index()
        {
            RentalRepository repo = new RentalRepository();

            var viewModel = new RentalIndexViewModel()
            {
                RegistrationNumbers = await repo.GetUniqueRegistrationNumber(),
                Rentals = await repo.GetAll(),
                MonthAndYearOfLastXMonths = GetMonthAndYearDictionaryOfLastMonths(4)
            };

            return View(viewModel);
        }
        
        // GET: Rental/Details/5
        public async Task<ActionResult> Details(string id)
        {
            RentalRepository repo = new RentalRepository();
            IEnumerable<Rental> result = await repo.GetAllByRegistrationNumber(id);

            var viewModel = new RentalDetailsViewModel()
            {
                RegistrationNumber = id,
                Year = result.Select(a => a.Year).Distinct().OrderBy(a => a),
                AggregatedData = GetAggregatedData(result),
                Rentals = result
            };

            return View(viewModel);
        }

        public async Task<ActionResult> DetailsByYear(string id, int year)
        {
            RentalRepository repo = new RentalRepository();
            IEnumerable<Rental> result = await repo.GetAllByRegistrationNumberAndYear(id,year);

            var viewModel = new RentalDetailsYearViewModel()
            {
                RegistrationNumber = id,
                Month = result.Select(a => a.Month).Distinct().OrderBy(a => a).ToDictionary(c => c, c=> CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(c)), 
                Year = year,
                AggregatedData = GetAggregatedData(result),
                Rentals = result
            };

            return View(viewModel);
        }

        public async Task<ActionResult> DetailsByYearAndMonth(string id, int year, int month)
        {
            RentalRepository repo = new RentalRepository();
            IEnumerable<Rental> result = await repo.GetAllByRegistrationNumberAndYearAndMonth(id, year, month);

            var viewModel = new RentalDetailsYearMonthViewModel()
            {
                RegistrationNumber = id,
                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                Year = year,
                AggregatedData = GetAggregatedData(result),
                Rentals = result.OrderBy(a => a.Pickup)
            };

            return View(viewModel);
        }


        private AggregatedData GetAggregatedData(IEnumerable<Rental> result)
        {
            decimal amountExTax = result.Sum(a => a.AmountNet);
            decimal amountIncTax = result.Sum(a => a.Amount);
            decimal tolls = result.Sum(a => a.Tolls);
            decimal excessMileage = result.Sum(a => a.ExcessMileage);
            decimal excessMileageIncTax = excessMileage / 0.75m;
            decimal totalAmountExTax = amountExTax + excessMileage;
            int totalKm = result.Sum(a => a.Mileage);
            decimal rentalDays = GetDays(result);
            decimal totalTaxAbleAmount = totalAmountExTax - (totalKm * TAX_EXPENSE_PER_KM);
            decimal totalTaxResult = totalTaxAbleAmount * TAX_PERCENTAGE;

            return new AggregatedData()
            {
                TotalAmountExTax = totalAmountExTax,
                AverageAmountPerDays = amountExTax/rentalDays,
                AverageDaysPerRent = rentalDays/result.Count(),
                AverageMilagePerDay = totalKm/rentalDays,
                AverageMilagePerRent = (decimal)totalKm/result.Count(),
                AveragePricePerKilometer = totalAmountExTax/totalKm,
                NumberOfBookings = result.Count(),
                NumberOfDaysRented = rentalDays,
                TotalAmount = amountIncTax + excessMileageIncTax,
                TotalTaxResult = totalTaxResult
            };
        }

        private decimal GetDays(IEnumerable<Rental> result)
        {
            return (decimal)result.Select(b =>
            {
                var days = (b.Delivery - b.Pickup).TotalDays;
                var daysRounded = Math.Floor(days);
                var diff = days - daysRounded;
                var extraDays = diff >= 0.125 ? 1 : 0;
                return extraDays + daysRounded;
            }).Sum(c => c);
        }

        private Dictionary<int, int> GetMonthAndYearDictionaryOfLastMonths(int numberOfMonths)
        {
            Dictionary<int, int> months = new Dictionary<int, int>();
            DateTime currentDate = DateTime.Today;

            for(int i = 0; i < numberOfMonths; i++)
            {
                months.Add(currentDate.Month, currentDate.Year);
                currentDate = currentDate.AddMonths(-1);
            }

            return months;
        }
    }
}
