using DataConnectorUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using UDC.Common;
using UDC.Common.Data.Models;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.Controllers.Filters
{
    public class AdminAuthFilter : IActionFilter
    {
        private AuthSessionService _authSessionService;

        public AdminAuthFilter(AuthSessionService authSessionService)
        {
            _authSessionService = authSessionService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            String strTokenAuthVal = "";
            Int32 intClientTZOffsetMins = 0;
            Boolean isAuthorised = false;


            if (context.HttpContext.Request.HasFormContentType)
            {
                strTokenAuthVal = GeneralHelpers.parseString(context.HttpContext.Request.Form["AuthToken"]);
                intClientTZOffsetMins = GeneralHelpers.parseInt32(context.HttpContext.Request.Form["client_tz_mins"]);
            }
            if (!String.IsNullOrEmpty(strTokenAuthVal))
            {
                UIUser authResult = _authSessionService.Authenticate(strTokenAuthVal, intClientTZOffsetMins);

                if (authResult != null && (authResult.IsConnectionAdmin || authResult.IsSyncManager))
                {
                    authResult = null;
                    context.HttpContext.Response.Redirect("/");
                }

                authResult = null;
            }
            else
            {
                isAuthorised = _authSessionService.IsAuthenticated();
            }
            if (!isAuthorised)
            {
                if(context.Controller is UIController)
                {
                    context.Result = new ViewResult
                    {
                        ViewName = "Unauthorized"
                    };
                }
                else
                {
                    context.Result = new JsonResult(new APIResponse(1, "Unauthorized"));
                }
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}