﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        [AllowAnonymous]
        [HttpPost("reopen")]
        [ModelStateValidator]
        public async Task<HttpResponse> ReopenAccount(LoginInput model)
        {
            await _userAccountService.ReopenAccountAsync(model.Username, model.Password);
            var redirectTo = new Uri(string.Format("{0}{1}", _appSettings.Urls.Web, _appSettings.Urls.LoginPage));
            Response.Redirect(redirectTo.ToString());
            return Response;
        }
       
    }

}