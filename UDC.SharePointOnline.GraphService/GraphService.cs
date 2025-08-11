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

        // This adheres to your IGraphService interface.
        public async Task<bool> ExistsAsync(string path)
        {
            try
            {
                //check client and driveId are initialized
                if (client == null || string.IsNullOrEmpty(driveId))
                {
                    throw new InvalidOperationException("GraphServiceClient or DriveId is not initialized.");
                }

                //this will fail with "nullReference Exception if the file is not present in the drive - so just return false;
                var item = await client.Drives[driveId].Root.ItemWithPath(path).Request().GetAsync().ConfigureAwait(false);
                return item != null; // If GetAsync returns an item, it exists.
            }
            catch (ServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This is the expected way to handle a "not found" scenario in Graph API.
                return false;
            }
            catch (Exception ex)
            {
                //ItemWithPath -  will fail with "nullReference Exception if the file is not present in the drive - so just return false;
                return false;
            }
        }

        public async Task DeleteAsync(string path)
        {
            // Use ConfigureAwait(false)
            await client.Drives[driveId].Root.ItemWithPath(path).Request().DeleteAsync().ConfigureAwait(false);
        }

        public async Task<Stream> DownloadAsync(string path)
        {
            // Use ConfigureAwait(false)
            var stream = await client.Drives[driveId].Root.ItemWithPath(path).Content.Request().GetAsync().ConfigureAwait(false);
            return stream;
        }

        public async Task<Stream> DownloadByIdAsync(string itemId) // Not in IGraphService, consider adding or removing.
        {
            // Use ConfigureAwait(false)
            var stream = await client.Drives[driveId].Items[itemId].Content.Request().GetAsync().ConfigureAwait(false);
            return stream;
        }

        public async Task UploadAsync(string path, Stream stream)
        {
            stream.Position = 0; // Ensure stream is at the beginning for upload.
            // Use ConfigureAwait(false)
            await client.Drives[driveId].Root.ItemWithPath(path).Content.Request().PutAsync<DriveItem>(stream).ConfigureAwait(false);
        }
    }
}