using UDC.Common;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Equ.SharePoint.GraphService
{
    // Your interface is already good, assuming it's correctly defined elsewhere:
    // public interface IGraphService
    // {
    //     Task<bool> ExistsAsync(string path);
    //     Task DeleteAsync(string path);
    //     Task<Stream> DownloadAsync(string path);
    //     Task UploadAsync(string path, Stream stream);
    // }

    public class GraphService : IGraphService
    {
        public GraphServiceClient client;
        private string driveId;

        // RECOMMENDATION: Make the constructor async, and call InitializeAsync from it
        // Or, have a separate async initialization method that must be called after construction.
        // For simplicity and to avoid changing the constructor signature immediately,
        // we'll move the async initialization to a separate method, and use AsyncHelper in constructor
        // as a temporary measure IF the constructor MUST remain synchronous.
        // **The ideal scenario is an async factory method for GraphService or an async Init method.**

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

            // RESOLUTION: Use AsyncHelper to avoid deadlocks in the constructor.
            // This still blocks the calling thread of the constructor, but it prevents the specific deadlock
            // due to SynchronizationContext.
            AsyncHelper.RunSync(async () =>
            {
                // Retrieve Site
                var site = await client.Sites.GetByPath(sitePath, siteDomain).Request().GetAsync().ConfigureAwait(false);
                if (site == null)
                {
                    throw new Exception($"SharePoint site '{sitePath}' at '{siteDomain}' not found.");
                }

                // Get Drives for the site
                var drives = await client.Sites[site.Id]
                                         .Drives
                                         .Request()
                                         .GetAsync()
                                         .ConfigureAwait(false);

                // Find the specific drive by name
                var drive = drives.FirstOrDefault(d => d.Name == driveName);
                driveId = drive?.Id ?? throw new Exception($"Drive '{driveName}' not found in site '{sitePath}'.");
            });
        }

    }
}