using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Services;

namespace UDC.SitefinityContextPlugin.Controllers.API
{
    public class ForceSSLFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            Boolean allowRequest = false;
            Boolean sslOffloadingEnabled = false;
            ConfigManager objManager = ConfigManager.GetManager();
            SystemConfig objSystemConfig = objManager.GetSection<SystemConfig>();

            if (objSystemConfig != null && objSystemConfig.SslOffloadingSettings != null)
            {
                if (objSystemConfig.SslOffloadingSettings.EnableSslOffloading)
                {
                    if (!String.IsNullOrEmpty(objSystemConfig.SslOffloadingSettings.HttpHeaderFieldName) && !String.IsNullOrEmpty(objSystemConfig.SslOffloadingSettings.HttpHeaderFieldValue))
                    {
                        sslOffloadingEnabled = true;

                        IEnumerable<String> arrHeaderValues = filterContext.Request.Headers.GetValues(objSystemConfig.SslOffloadingSettings.HttpHeaderFieldName);
                        String strHeaderValue = arrHeaderValues.FirstOrDefault();

                        if (strHeaderValue == objSystemConfig.SslOffloadingSettings.HttpHeaderFieldValue)
                        {
                            allowRequest = true;
                        }

                        arrHeaderValues = null;
                    }
                }
            }
            if (!sslOffloadingEnabled)
            {
                if (filterContext.Request.RequestUri.Scheme == Uri.UriSchemeHttps)
                {
                    allowRequest = true;
                }
            }
            if (!allowRequest)
            {
                filterContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }

            objSystemConfig = null;
            objManager = null;
        }
    }
}