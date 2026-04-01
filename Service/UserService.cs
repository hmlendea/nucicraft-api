using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

using NuciDAL.Repositories;
using NuciLog.Core;

namespace NuciCraft.API.Service
{
    public class UserService(
        IFileRepository<UserEntity> usersRepository,
        ILogger logger) : IUserService
    {
        public void Register(RegisterUserRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, request.Username),
                new(MyLogInfoKey.OnlineUUID, request.OnlineUUID),
                new(MyLogInfoKey.CreatedDT, request.CreatedDT),
                new(MyLogInfoKey.IpAddress, request.IpAddress),
                new(MyLogInfoKey.SkinUrl, request.SkinUrl)
            ];

            logger.Info(
                MyOperation.RegisterUser,
                OperationStatus.Started,
                logInfos);

            try
            {
                User user = new()
                {
                    Username = request.Username,
                    OfflineUUID = GetOfflineUuid(request.Username),
                    OnlineUUID = request.OnlineUUID,
                    CreatedDT = request.CreatedDT != null ? DateTimeOffset.Parse(request.CreatedDT) : DateTimeOffset.Now,
                    Password = request.Password,
                    IpAddress = request.IpAddress,
                    SkinUrl = request.SkinUrl
                };

                logInfos = logInfos
                    .Append(new(MyLogInfoKey.OfflineUUID, user.OfflineUUID))
                    .Append(new(MyLogInfoKey.CreatedDT, user.CreatedDT));

                usersRepository.Add(user.ToDataObject());
                usersRepository.SaveChanges();

                logger.Info(
                    MyOperation.RegisterUser,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.RegisterUser,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        public User Get(string username)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, username)
            ];

            logger.Info(
                MyOperation.GetUser,
                OperationStatus.Started,
                logInfos);

            try
            {
                User user = usersRepository
                    .Get(username)
                    .ToDomainModel();

                logger.Info(
                    MyOperation.GetUser,
                    OperationStatus.Success,
                    logInfos);

                return user;
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.GetUser,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        static string GetOfflineUuid(string username)
        {
            string input = $"OfflinePlayer:{username}";

            // Compute MD5 hash
            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert to hex string
            StringBuilder hexBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hexBuilder.Append(b.ToString("x2"));
            }

            string byteArray = hexBuilder.ToString();

            // Modify specific bytes (UUID v3 format adjustments)
            int byte6 = (Convert.ToInt32(byteArray.Substring(6, 2), 16) & 0x0f) | 0x30;
            int byte8 = (Convert.ToInt32(byteArray.Substring(8, 2), 16) & 0x3f) | 0x80;

            byteArray =
                byteArray.Substring(0, 6) +
                byte6.ToString("x2") +
                byteArray.Substring(8, 2) +
                byte8.ToString("x2") +
                byteArray.Substring(10);

            // Format as UUID
            return
                $"{byteArray.Substring(0, 8)}-" +
                $"{byteArray.Substring(8, 4)}-" +
                $"{byteArray.Substring(12, 4)}-" +
                $"{byteArray.Substring(16, 4)}-" +
                $"{byteArray.Substring(20)}";
        }
    }
}