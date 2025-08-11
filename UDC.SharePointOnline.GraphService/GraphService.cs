using UDC.Common;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Equ.SharePoint.GraphService
{
    public class GraphService : IGraphService
    {
        public GraphServiceClient client;
        private string driveId;

        public GraphService(
            string tenantId,
            string clientId,
            string clientSecret,
            string siteDomain,
            string sitePath,
            string driveName)
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();

            var authProvider = new ClientCredentialProvider(confidentialClientApplication);
            client = new GraphServiceClient(authProvider);

            AsyncHelper.RunSync(async () =>
            {
                var site = await client.Sites.GetByPath(sitePath, siteDomain).Request().GetAsync().ConfigureAwait(false);
                if (site == null)
                {
                    throw new Exception($"SharePoint site '{sitePath}' at '{siteDomain}' not found.");
                }

                var drives = await client.Sites[site.Id]
                                         .Drives
                                         .Request()
                                         .GetAsync()
                                         .ConfigureAwait(false);

                var drive = drives.FirstOrDefault(d => d.Name == driveName);
                driveId = drive?.Id ?? throw new Exception($"Drive '{driveName}' not found in site '{sitePath}'.");
            });
        }

        public Task<IEnumerable<Dictionary<string, object>>> GetListsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, object>> GetListAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Dictionary<string, object>>> GetDocumentsAsync(Guid listId, IEnumerable<string> fields)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Dictionary<string, object>>> GetTermSetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Dictionary<string, object>>> GetTermsAsync(Guid termSetId)
        {
            throw new NotImplementedException();
        }
    }
}
