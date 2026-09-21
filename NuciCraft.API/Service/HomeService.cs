using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Helpers;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Mappings;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public sealed class HomeService(
        IFileRepository<HomeDataObject> repository,
        IPlayerService playerService,
        ILogger logger) : IHomeService
    {
        private readonly object persistenceLock = new();

        public Home Add(AddHomeRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, request?.Player)
            ];

            return Execute(
                MyOperation.AddHome,
                logInfos,
                () =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    ArgumentException.ThrowIfNullOrWhiteSpace(request.Player);
                    ValidateLocation(request.Location);

                    Player player = playerService.Get(new GetPlayerRequest { Username = request.Player });
                    Home home = new()
                    {
                        Identifier = Guid.NewGuid().ToString(),
                        CreatedDT = DateTimeOffset.UtcNow,
                        Name = request.Name,
                        Player = player.Identifier,
                        Location = request.Location
                    };
                    ArgumentNullException.ThrowIfNull(home.Name);
                    HomeDataObject dataObject = home.ToDataObject();
                    ValidateUniqueName(dataObject);
                    repository.Add(dataObject);
                    repository.SaveChanges();

                    return dataObject.ToServiceModel();
                });
        }

        public Home Get(string homeIdentifier) => Execute(MyOperation.GetHome, () =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(homeIdentifier);

            return repository.Get(homeIdentifier).ToServiceModel();
        });

        public Home Get(string playerIdentifier, string name) => Execute(MyOperation.GetHome, () =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(playerIdentifier);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            HomeDataObject home = repository.GetAll().FirstOrDefault(candidate =>
                string.Equals(candidate.Player, playerIdentifier, StringComparison.Ordinal) &&
                GetNames(candidate.Name).Contains(name.Trim(), StringComparer.OrdinalIgnoreCase));

            if (home is null)
            {
                throw new KeyNotFoundException("No home exists for the supplied player and name.");
            }

            return home.ToServiceModel();
        });

        public IEnumerable<Home> GetAll() => Execute(MyOperation.GetAllHomes, () =>
            repository.GetAll().Select(home => home.ToServiceModel()).ToArray());

        public IEnumerable<Home> GetAllByPlayer(string playerIdentifier)
            => Execute(MyOperation.GetAllHomes, () =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(playerIdentifier);

                return repository.GetAll()
                    .Where(home => string.Equals(home.Player, playerIdentifier, StringComparison.Ordinal))
                    .Select(home => home.ToServiceModel())
                    .ToArray();
            });

        public Home Update(PatchHomeRequest request) => Execute(MyOperation.UpdateHome, () =>
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Identifier);
            HomeDataObject home = repository.Get(request.Identifier).ToServiceModel().ToDataObject();
            ApplyPatch(request, home);
            ValidateUniqueName(home);
            home.UpdatedDT = TimestampFormats.GetCurrentUtcTimestamp();
            repository.Update(home);
            repository.SaveChanges();

            return home.ToServiceModel();
        });

        private void ApplyPatch(PatchHomeRequest request, HomeDataObject home)
        {
            if (request.Name is not null)
            {
                home.Name = home.Name.MergeWith(request.Name.ToDataObject());
            }

            if (request.Player is not null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(request.Player);
                home.Player = playerService.Get(new GetPlayerRequest
                {
                    Identifier = request.Player
                }).Identifier;
            }

            if (request.Location is not null)
            {
                ValidateLocation(request.Location);
                home.Location = request.Location.ToDataObject();
            }
        }

        private void ValidateUniqueName(HomeDataObject home)
        {
            IEnumerable<string> names = GetNames(home.Name).ToArray();

            if (!names.Any())
            {
                throw new ArgumentException("At least one non-whitespace home name must be provided.");
            }

            bool duplicateExists = repository.GetAll().Any(candidate =>
                !string.Equals(candidate.Id, home.Id, StringComparison.Ordinal) &&
                string.Equals(candidate.Player, home.Player, StringComparison.Ordinal) &&
                GetNames(candidate.Name).Intersect(names, StringComparer.OrdinalIgnoreCase).Any());

            if (duplicateExists)
            {
                throw new ArgumentException("The player has a home with one of the supplied names.");
            }
        }

        private static IEnumerable<string> GetNames(LocalisedStringDataObject name)
        {
            string[] names =
            [
                name.Default, name.Chinese, name.Dacian, name.English, name.French,
                name.German, name.Italian, name.Japanese, name.Latin, name.Nucian, name.Romanian
            ];

            return names
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim());
        }

        private static void ValidateLocation(Coordinates location)
        {
            ArgumentNullException.ThrowIfNull(location);
            ArgumentException.ThrowIfNullOrWhiteSpace(location.World);

            if (!float.IsFinite(location.X) || !float.IsFinite(location.Y) ||
                !float.IsFinite(location.Z) || !float.IsFinite(location.Pitch) ||
                !float.IsFinite(location.Yaw))
            {
                throw new ArgumentException("Home coordinates and orientation must be finite.");
            }
        }

        private TResult Execute<TResult>(Operation operation, Func<TResult> action)
            => Execute(operation, [], action);

        private TResult Execute<TResult>(
            Operation operation,
            IEnumerable<LogInfo> logInfos,
            Func<TResult> action)
        {
            lock (persistenceLock)
            {
                logger.Info(operation, OperationStatus.Started, logInfos);

                try
                {
                    TResult result = action();
                    logger.Info(operation, OperationStatus.Success, logInfos);

                    return result;
                }
                catch (Exception exception)
                {
                    logger.Error(operation, OperationStatus.Failure, exception, logInfos);

                    throw;
                }
            }
        }
    }
}