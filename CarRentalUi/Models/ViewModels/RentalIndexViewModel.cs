using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarRentalUi.Models.ViewModels
{
    public class RentalIndexViewModel
    {
        public IEnumerable<string> RegistrationNumbers { get; set; }
        public IEnumerable<Rental> Rentals { get; set; }
    }
}