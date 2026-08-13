using System;
using System.Reflection;

using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests.Service.Mapping
{
    internal static class MappingMethodInvoker
    {
        internal static TResult Invoke<TParameter, TResult>(
            string mappingTypeName,
            string methodName,
            TParameter parameter)
        {
            Type mappingType = typeof(PlayerService).Assembly.GetType(mappingTypeName);
            MethodInfo mappingMethod = mappingType.GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                [typeof(TParameter)],
                null);

            return (TResult)mappingMethod.Invoke(null, [parameter]);
        }
    }
}