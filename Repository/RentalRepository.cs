using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITDS.Core.CosmosDb;
using ITDS.Core.Repository.CosmosDb;
using Models;

namespace Repository
{
    public class RentalRepository
    {
        private static ICosmosDbRepository<Rental> _repository = null;

        public RentalRepository()
        {
            if(_repository == null)
            {
                _repository = CosmosDbRepositoryFactory.Create<Rental>("https://nabobil.documents.azure.com:443/", "MQX99W0Yw612NIB34NlXoUmEY9VVd34pAKn8UTzlhygrvSOFTrXCOpKABiasnEuG3vCRKFDSavirC1WOtfbYBA==","Nabobil","Rental",isPartitioned:true);
            }
        }
        public Task<IEnumerable<Rental>> GetAll()
        {
            return _repository.GetAsync<Rental>(a => a.Id != null);
        }
        public Task<IEnumerable<string>> GetUniqueRegistrationNumber()
        {
            return _repository.Query<string>("SELECT DISTINCT VALUE (c.registrationNumber) FROM c");
        }

        public Task<IEnumerable<Rental>> GetAllByRegistrationNumber(string regNr)
        {
            return _repository.GetAsync<Rental>(a => a.RegistrationNumber == regNr);
        }
        public Task<IEnumerable<Rental>> GetAllByRegistrationNumberAndYear(string regNr, int year)
        {
            return _repository.GetAsync<Rental>(a => a.RegistrationNumber == regNr && a.Year == year);
        }

        public Task<IEnumerable<Rental>> GetAllByRegistrationNumberAndYearAndMonth(string regNr, int year, int month)
        {
            return _repository.GetAsync<Rental>(a => a.RegistrationNumber == regNr && a.Year == year && a.Month == month);
        }

    }
}
