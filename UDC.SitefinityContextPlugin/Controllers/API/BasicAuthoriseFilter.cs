using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Collections.Specialized;

using Microsoft.Owin;

using UDC.SitefinityContextPlugin.Data;

namespace UDC.SitefinityContextPlugin.Controllers.API
{
    public class BasicAuthoriseFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            IOwinContext context = filterContext.Request.GetOwinContext();
            NameValueCollection headers = new NameValueCollection();
            Boolean blnAuthorised = false;

            foreach (var key in context.Request.Headers.Keys)
            {
                headers.Add(key, context.Request.Headers[key]);
            }
            if (headers["Authorization"] != null)
            {
                String strAuthHeader = context.Request.Headers["Authorization"];
                if (!String.IsNullOrEmpty(strAuthHeader) && strAuthHeader.StartsWith("Basic"))
                {
                    String strAuth = strAuthHeader.Substring("Basic ".Length).Trim();
                    Encoding objEncoding = Encoding.GetEncoding("UTF-8");
                    String strCreds = objEncoding.GetString(Convert.FromBase64String(strAuth));

                    if (strCreds.IndexOf(":") > -1)
                    {
                        String strUsr = strCreds.Split(':')[0];
                        String strPass = strCreds.Split(':')[1];

                        if (!String.IsNullOrEmpty(strUsr) && !String.IsNullOrEmpty(strPass))
                        {
                            blnAuthorised = CMSDataIO.AuthenticateUser(strUsr, strPass, true);
                        }
                        strUsr = null;
                        strPass = null;
                    }
                    strAuth = null;
                    objEncoding = null;
                    strCreds = null;
                }
                strAuthHeader = null;
            }

            headers = null;

            if (!blnAuthorised)
            {
                HttpResponseMessage objResponse = new HttpResponseMessage()
                {
                    Content = new StringContent("Not Authorised")
                };
                objResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                objResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                filterContext.Response = objResponse;
            }
        }
    }
}