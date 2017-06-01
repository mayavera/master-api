﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Constants;
using MasterApi.Core.Enums;
using MasterApi.Core.Services;
using MasterApi.Web.Filters;
using MasterApi.Core.Infrastructure.Caching;
using MasterApi.Core.Common;
using MasterApi.Core.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using MasterApi.Core.Account.Enums;

namespace MasterApi.Web.Controllers.v1
{
    [Route("api/{version}/[controller]")]
    public class GeoController : BaseController
    {
        private readonly IGeoService _geoService;
        private readonly IMemoryCache _cache;
        private static MemoryCacheEntryOptions _cacheOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoController" /> class.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <param name="geoService">The country service.</param>
        public GeoController(IUserInfo userInfo, IGeoService geoService, IMemoryCache cache) : base(userInfo)
        {
            _geoService = geoService;
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove,
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <param name="page">Index of the page.</param>
        /// <param name="size">Size of the page.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("countries")]
        [ProducesResponseType(typeof(CountryOutput), 200)]
        public async Task<IActionResult> GetCountries([FromUri] int page = 0, int size = 0)
        {
            PackedList<CountryOutput> result;
            if (page > 0 && size > 0)
            {
                result = await _geoService.GetCountries(page, size);
            } else
            {
                if (!_cache.TryGetValue(DataCacheKey.Countries, out result))
                {
                    result = await _geoService.GetCountries(page, size);
                    _cache.Set(DataCacheKey.Countries, result, _cacheOptions);
                }
            }

            AddHeader("X-TOTAL-RECORDS", result.Total);

            return Ok(result.Data);            
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <param name="page">Index of the page.</param>
        /// <param name="size">Size of the page.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("languages")]
        [ProducesResponseType(typeof(LanguageOutput), 200)]  
        public async Task<IActionResult> GetLanguages([FromUri] int page = 0, int size = 0)
        {
            var result = await _geoService.GetLanguages(page, size);
            AddHeader("X-TOTAL-RECORDS", result.Total);
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <param name="page">Index of the page.</param>
        /// <param name="size">Size of the page.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("countries/enabled")]
        [ProducesResponseType(typeof(CountryEnabledOutput), 200)]
        public async Task<IActionResult> GetEnabledCountries([FromUri] int page = 0, int size = 0)
        {
            var result = await _geoService.GetEnabledCountries(page, size);
            AddHeader("X-TOTAL-RECORDS", result.Total);
            return Ok(result.Data);
        }
        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <param name="page">Index of the page.</param>
        /// <param name="size">Size of the page.</param>
        /// <param name="iso2">The iso2.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("states")]        
        [ProducesResponseType(typeof(ProvinceStateOutput), 200)]
        public async Task<IActionResult> GetProvinceStates([FromUri] int page = 0, int size = 0, string iso2 = null)
        {
            var result = await _geoService.GetStates(page, size, iso2);
            AddHeader("X-TOTAL-RECORDS", result.Total);
            return Ok(result.Data);
        }

        /// <summary>
        /// Patches the specified identifier.
        /// </summary>
        /// <param name="iso2">The iso2.</param>
        /// <returns></returns>
        [HttpPatch("country/{iso2}/enable")]
        [ProducesResponseType(typeof(IActionResult), 200)]
        [ClaimsAuthorize(ClaimTypes.Role, UserAccessLevel.Admin)]
        public async Task<IActionResult> PatchEnable(string iso2)
        {
            return await EnableDisableCounty(iso2, true);
        }

        /// <summary>
        /// Patches the specified identifier.
        /// </summary>
        /// <param name="iso2">The iso2.</param>
        /// <returns></returns>        
        [HttpPatch("country/{iso2}/disable")]
        [ClaimsAuthorize(ClaimTypes.Role, UserAccessLevel.Admin)]
        public async Task<IActionResult> PatchDisable(string iso2)
        {
            return await EnableDisableCounty(iso2, false);
        }

        private async Task<IActionResult> EnableDisableCounty(string iso2, bool enable)
        {
            if (string.IsNullOrEmpty(iso2)) { return BadRequest(); }
            await _geoService.EnableDisableCountry(iso2, UserInfo.UserId, enable);
            Module = ModelType.EnabledCountry;
            return Ok(ModelAction.Update, EventStatus.Success);
        }

        /// <summary>
        /// Patches the specified identifier.
        /// </summary>
        /// <param name="iso2">The iso2.</param>
        /// <param name="languageCode">The language code.</param>
        /// <param name="main">if set to <c>true</c> [main].</param>
        /// <returns></returns>        
        [HttpPatch("country/{iso2}/lang/{languageCode}")]
        [ClaimsAuthorize(ClaimTypes.Role, UserAccessLevel.Admin)]
        public async Task<IActionResult> PatchCountyLanguage(string iso2, string languageCode, [FromUri] bool main = false)
        {
            if (string.IsNullOrEmpty(iso2) || string.IsNullOrEmpty(languageCode))
            {
                return BadRequest(AppConstants.InformationMessages.InvalidRequestParameters);
            }

            iso2 = iso2.ToUpper();
            languageCode = languageCode.ToLower();
            await _geoService.SetCountryLanguage(iso2, languageCode, main);
            Module = ModelType.CountryLanguage;
            return Ok(ModelAction.Update, EventStatus.Success);
        }
    }

}