using System;
using System.Text;
using System.Net.Http;
using System.IO;

using Newtonsoft.Json;

using UDC.Common.Data.Models;

namespace UDC.Common.Data
{
    public class WebAPIClient
    {
        public enum HttpMethod
        {
            GET = 1,
            POST = 2
        }
        public APIResponse SendRequest(String url, String user, String pass, Boolean ignoreCertErrors, Object postParams, HttpMethod method = HttpMethod.GET)
        {
            APIResponse objAPIResponse = null;
            HttpClient objHttpClient = null;
            Byte[] arrAuthHeader = Encoding.ASCII.GetBytes(user + ":" + pass);
            HttpResponseMessage objResponse = null;
            HttpContent objContent = null;
            String strData = "";

            if (ignoreCertErrors)
            {
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
                objHttpClient = new HttpClient(handler);
            }
            else
            {
                objHttpClient = new HttpClient();
            }
            
            objHttpClient.Timeout = TimeSpan.FromMinutes(999);
            objHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(arrAuthHeader));

            switch(method)
            {
                case HttpMethod.GET:
                    objResponse = objHttpClient.GetAsync(url).Result;
                    break;
                case HttpMethod.POST:
                    String postData = "";
                    if(postParams != null)
                    {
                        postData = JsonConvert.SerializeObject(postParams);
                    }
                    StringContent obj = new StringContent(postData, Encoding.UTF8, "application/json");
                    objResponse = objHttpClient.PostAsync(url, obj).Result;
                    break;
            }

            objContent = objResponse.Content;

            strData = objContent.ReadAsStringAsync().Result;

            if (objResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                switch (objContent.Headers.ContentType.MediaType)
                {
                    case "text/html":
                        //Likely Sitefinity Initialising or Authentication Error...
                        objAPIResponse = new APIResponse(2, "The Sitefinity Intsance returned text/html which likely means the instance is still initialising, or there is an authentication error for the SitefinityContextPlugin...", strData);
                        break;
                    case "application/json":
                        //Likely a valid payload if parsed correctly or API Error...
                        try
                        {
                            objAPIResponse = JsonConvert.DeserializeObject<APIResponse>(strData);
                        }
                        catch (Exception ex)
                        {
                            objAPIResponse = new APIResponse(1, "Response was not in the expected JSON format...", strData);
                        }
                        break;
                    case "application/octet-stream":
                        //Document Binary Stream...
                        Stream objBinaryPayload = objContent.ReadAsStreamAsync().Result;
                        objAPIResponse = new APIResponse(0, "Recieved binary payload...", GeneralHelpers.parseBinaryStream(objBinaryPayload));
                        objBinaryPayload = null;
                        break;
                }
            }
            else
            {
                objAPIResponse = new APIResponse(1, "An error occurred on the remote server... [" + objResponse.StatusCode.ToString() + "]", strData);
            }

            strData = null;
            objContent = null;
            objResponse = null;
            arrAuthHeader = null;
            objHttpClient = null;

            return objAPIResponse;
        }
    }
}