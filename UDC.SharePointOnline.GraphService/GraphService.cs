using UDC.Common;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Equ.SharePoint.GraphService
{
    public class GraphService : IGraphService
    {
        private readonly GraphServiceClient client;
        private readonly string siteId;
        private readonly string driveId;

        public GraphService(
            string tenantId,
            string clientId,
            string clientSecret,
            string siteDomain,
            string sitePath,
            string driveName)
        {
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
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

                siteId = site.Id;

                var drives = await client.Sites[site.Id]
                                         .Drives
                                         .Request()
                                         .GetAsync()
                                         .ConfigureAwait(false);

                var drive = drives.FirstOrDefault(d => d.Name == driveName);
                driveId = drive?.Id ?? throw new Exception($"Drive '{driveName}' not found in site '{sitePath}'.");
            });
        }

        private static Dictionary<string, object> SimplifyDriveItem(DriveItem item) => new()
        {
            ["id"] = item.Id,
            ["name"] = item.Name,
            ["size"] = item.Size,
            ["isFolder"] = item.Folder != null,
            ["webUrl"] = item.WebUrl,
            ["createdDateTime"] = item.CreatedDateTime,
            ["lastModifiedDateTime"] = item.LastModifiedDateTime
        };

        private static Exception WrapServiceException(ServiceException ex, string resource) => ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new UnauthorizedAccessException("Authentication to Microsoft Graph failed.", ex),
            HttpStatusCode.NotFound => new KeyNotFoundException($"{resource} not found.", ex),
            _ => new Exception($"Microsoft Graph request for {resource} failed.", ex)
        };

        public async Task<Dictionary<string, object>> GetItemAsync(string itemId)
        {
            try
            {
                var item = await client.Sites[siteId].Drives[driveId].Items[itemId]
                    .Request()
                    .GetAsync()
                    .ConfigureAwait(false);

                return SimplifyDriveItem(item);
            }
            catch (ServiceException ex)
            {
                throw WrapServiceException(ex, $"item '{itemId}'");
            }
        }

        public async Task<IList<Dictionary<string, object>>> GetChildrenAsync(string itemId = null)
        {
            try
            {
                IDriveItemChildrenCollectionPage items;

                if (string.IsNullOrEmpty(itemId))
                {
                    items = await client.Sites[siteId].Drives[driveId].Root.Children
                        .Request()
                        .GetAsync()
                        .ConfigureAwait(false);
                }
                else
                {
                    items = await client.Sites[siteId].Drives[driveId].Items[itemId].Children
                        .Request()
                        .GetAsync()
                        .ConfigureAwait(false);
                }

                return items.Select(SimplifyDriveItem).ToList();
            }
            catch (ServiceException ex)
            {
                var resource = string.IsNullOrEmpty(itemId) ? "drive root" : $"item '{itemId}'";
                throw WrapServiceException(ex, resource);
            }
        }
    }
}

