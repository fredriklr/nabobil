﻿using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarRentalUi.Models.ViewModels
{
    public class RentalDetailsYearViewModel
    {
        public string RegistrationNumber { get; set; }
        public Dictionary<int,string> Month { get; set; }
        public int Year { get; set; }
        public AggregatedData AggregatedData { get; set; }
        public IEnumerable<Rental> Rentals { get; set; }
    }
}