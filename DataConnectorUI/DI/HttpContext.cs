using Microsoft.AspNetCore.Http;

namespace DataConnectorUI.DI
{
    public static class HttpContext
    {
        private static IHttpContextAccessor ContextAccessor;

        public static Microsoft.AspNetCore.Http.HttpContext Current => ContextAccessor.HttpContext;

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }
    }
}