using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;

namespace UDC.SitefinityContextPlugin.Handlers
{
    public abstract class BaseHandler : IHttpHandler, IReadOnlySessionState, IRouteHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }
        public BaseHandler()
        {

        }

        public virtual void ProcessRequest(HttpContext context)
        {
            this.ExecMain(context);
        }
        abstract protected void ExecMain(HttpContext context);
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        public void RedirectWithData(Dictionary<String, String> parameters, String url)
        {
            HttpResponse objResponse = HttpContext.Current.Response;
            StringBuilder objSB = new StringBuilder();
            String strJS = "<script language=\"javascript\">\n" +
                            "function submitForm()\n" +
                            "{\n" +
                            "	var dt = new Date();\n" +
                            "	document.getElementById(\"client_tz_mins\").value = dt.getTimezoneOffset();\n" +
                            "	document.forms[\"form\"].submit();\n" +
                            "}\n" +
                            "</script>\n";

            objSB.Append("<html>");
            objSB.Append("<head>");
            objSB.Append(strJS);
            objSB.Append("</head>");
            objSB.AppendFormat("<body onload='submitForm();'>");
            objSB.AppendFormat("<form id='form' name='form' action='{0}' method='post'>", url);
            objSB.AppendFormat("<input type='hidden' id='client_tz_mins' name='client_tz_mins' value='' />");

            foreach (String key in parameters.Keys)
            {
                objSB.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", key, parameters[key]);
            }

            objSB.Append("</form></body></html>");

            objResponse.Clear();
            objResponse.Write(objSB.ToString());

            objSB = null;

            objResponse.End();
        }
    }
}