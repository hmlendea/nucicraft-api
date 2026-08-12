using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

namespace NuciCraft.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class CountriesController(
        ICountryService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Add(
            [FromBody] AddCountryRequest request)
            => ProcessRequest(
                request,
                () => service.Add(request),
                authorisation);

        [HttpGet]
        [Route("{countryIdentifier}")]
        public ActionResult Get(
            string countryIdentifier)
            => ProcessRequest(
                new GetCountryRequest()
                {
                    Identifier = countryIdentifier
                },
                () => new GetResponse(service.Get(countryIdentifier)),
                authorisation);

        [HttpGet]
        public ActionResult GetAll()
            => ProcessRequest(
                new GetCountriesRequest(),
                () => new GetResponse(service.GetAll()),
                authorisation);

        [HttpPatch]
        [Route("{countryIdentifier}")]
        public ActionResult PatchByIdentifier(
            string countryIdentifier,
            [FromBody] PatchCountryRequest request)
        {
            request.Identifier = countryIdentifier;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }
    }
}
