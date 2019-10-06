﻿using CarRentalUi.Models.ViewModels;
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
        // GET: Rental
        public async Task<ActionResult> Index()
        {
            RentalRepository repo = new RentalRepository();

            var viewModel = new RentalIndexViewModel()
            {
                RegistrationNumbers = await repo.GetUniqueRegistrationNumber(),
                Rentals = await repo.GetAll()
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
            decimal tolls = result.Sum(a => a.Tolls);
            decimal excessMileage = result.Sum(a => a.ExcessMileage);
            decimal totalAmountExTax = amountExTax + excessMileage;
            int totalKm = result.Sum(a => a.Mileage);
            decimal rentalDays = GetDays(result);

            return new AggregatedData()
            {
                TotalAmountExTax = amountExTax + excessMileage,
                AverageAmountPerDays = amountExTax/rentalDays,
                AverageDaysPerRent = rentalDays/result.Count(),
                AverageMilagePerDay = totalKm/rentalDays,
                AverageMilagePerRent = (decimal)totalKm/result.Count(),
                AveragePricePerKilometer = totalAmountExTax/totalKm,
                NumberOfBookings = result.Count(),
                NumberOfDaysRented = rentalDays,
                TotalAmount = result.Sum(a => a.Amount)           
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
    }
}