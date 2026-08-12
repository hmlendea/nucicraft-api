using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Helpers;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public sealed class CountryService(
        IFileRepository<CountryDataObject> repository,
        ILogger logger) : ICountryService
    {
        public void Add(AddCountryRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier),
                new(MyLogInfoKey.Username, request.Leader)
            ];

            logger.Info(
                MyOperation.AddCountry,
                OperationStatus.Started,
                logInfos);

            try
            {
                CountryDataObject countryDataObject = new()
                {
                    Id = request.Identifier,
                    Name = request.Name,
                    LeaderTitle = request.LeaderTitle,
                    Leader = request.Leader,
                    CreatedDT = DateTimeOffset.UtcNow.ToString(
                        TimestampFormats.Full,
                        CultureInfo.InvariantCulture)
                };

                repository.Add(countryDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.AddCountry,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.AddCountry,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Country Get(string countryIdentifier)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, countryIdentifier)
            ];

            logger.Info(
                MyOperation.GetCountry,
                OperationStatus.Started,
                logInfos);

            try
            {
                Country country = repository.Get(countryIdentifier).ToServiceModel();

                logger.Info(
                    MyOperation.GetCountry,
                    OperationStatus.Success,
                    logInfos);

                return country;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetCountry,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public IEnumerable<Country> GetAll()
        {
            logger.Info(
                MyOperation.GetAllCountries,
                OperationStatus.Started);

            try
            {
                IEnumerable<Country> countries = repository.GetAll().ToServiceModels();

                logger.Info(
                    MyOperation.GetAllCountries,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.Count, countries.Count()));

                return countries;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetAllCountries,
                    OperationStatus.Failure,
                    exception);

                throw;
            }
        }

        public void Update(UpdateCountryRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.CountryIdentifier)
            ];

            logger.Info(
                MyOperation.UpdateCountry,
                OperationStatus.Started,
                logInfos);

            try
            {
                ValidatePatchSelector(request);

                CountryDataObject countryDataObject = repository.Get(request.CountryIdentifier);

                ApplyPatchValues(request, countryDataObject);

                countryDataObject.UpdatedDT = DateTimeOffset.UtcNow.ToString(
                    TimestampFormats.Full,
                    CultureInfo.InvariantCulture);

                repository.Update(countryDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdateCountry,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.UpdateCountry,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private static void ValidatePatchSelector(UpdateCountryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CountryIdentifier))
            {
                throw new ArgumentException("The country identifier must be provided.");
            }
        }

        private static void ApplyPatchValues(
            UpdateCountryRequest request,
            CountryDataObject countryDataObject)
        {
            if (request.Name is not null)
            {
                countryDataObject.Name = countryDataObject.Name.MergeWith(request.Name);
            }

            if (request.LeaderTitle is not null)
            {
                countryDataObject.LeaderTitle = countryDataObject.LeaderTitle.MergeWith(request.LeaderTitle);
            }

            if (request.Leader is not null)
            {
                countryDataObject.Leader = request.Leader;
            }
        }
    }
}
