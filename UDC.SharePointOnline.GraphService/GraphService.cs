using UDC.Common;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using GraphList = Microsoft.Graph.List;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Equ.SharePoint.GraphService
{
    public class GraphService : IGraphService
    {
        public GraphServiceClient client;
        private string driveId;
        private string siteId;

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

        public async Task<IEnumerable<Dictionary<string, object>>> GetListsAsync()
        {
            var results = new List<Dictionary<string, object>>();

            var list = await client.Drives[driveId]
                                    .List
                                    .Request()
                                    .Expand("Columns")
                                    .GetAsync()
                                    .ConfigureAwait(false);

            if (list != null)
            {
                results.Add(ConvertList(list));
            }

            return results;
        }

        public async Task<Dictionary<string, object>> GetListAsync(Guid id)
        {
            var list = await client.Sites[siteId]
                                   .Lists[id.ToString()]
                                   .Request()
                                   .Expand("Columns")
                                   .GetAsync()
                                   .ConfigureAwait(false);

            if (list == null)
            {
                return null;
            }

            var result = ConvertList(list);

            var root = await client.Drives[driveId]
                                    .Root
                                    .Request()
                                    .GetAsync()
                                    .ConfigureAwait(false);

            var contents = await GetFolderContentsAsync(root.Id);
            result.Add("Folders", contents["Folders"]);
            result.Add("Documents", contents["Documents"]);

            return result;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetDocumentsAsync(Guid listId, IEnumerable<string> fields)
        {
            var items = await client.Sites[siteId]
                                     .Lists[listId.ToString()]
                                     .Items
                                     .Request()
                                     .Expand("Fields,DriveItem")
                                     .GetAsync()
                                     .ConfigureAwait(false);

            var results = new List<Dictionary<string, object>>();

            if (items != null)
            {
                foreach (var item in items.Where(i => i?.DriveItem != null))
                {
                    var dict = ConvertFile(item.DriveItem);

                    if (fields != null && item.Fields != null && item.Fields.AdditionalData != null)
                    {
                        foreach (var field in fields)
                        {
                            if (item.Fields.AdditionalData.ContainsKey(field))
                            {
                                dict[field] = item.Fields.AdditionalData[field];
                            }
                            else
                            {
                                dict[field] = null;
                            }
                        }
                    }

                    results.Add(dict);
                }
            }

            return results;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetTermSetsAsync()
        {
            var results = new List<Dictionary<string, object>>();

            var baseUrl = client.Sites[siteId].Request().RequestUrl;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/termStore/sets");
            await client.AuthenticationProvider.AuthenticateRequestAsync(request).ConfigureAwait(false);
            var response = await client.HttpProvider.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(content);
            var sets = json["value"] as JArray;
            if (sets != null)
            {
                foreach (var set in sets)
                {
                    var dict = new Dictionary<string, object>
                    {
                        {"Id", (string)set["id"]},
                        {"Name", (string)set["localizedNames"]?.First?["name"]}
                    };
                    results.Add(dict);
                }
            }

            return results;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetTermsAsync(Guid termSetId)
        {
            var results = new List<Dictionary<string, object>>();

            var baseUrl = client.Sites[siteId].Request().RequestUrl;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/termStore/sets/{termSetId}/terms?$expand=children");
            await client.AuthenticationProvider.AuthenticateRequestAsync(request).ConfigureAwait(false);
            var response = await client.HttpProvider.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(content);
            var terms = json["value"] as JArray;
            if (terms != null)
            {
                foreach (var term in terms)
                {
                    results.Add(ConvertTerm(term));
                }
            }

            return results;
        }

        private Dictionary<string, object> ConvertList(GraphList list)
        {
            var dest = new Dictionary<string, object>();
            dest.Add("Id", list.Id);
            dest.Add("Title", list.DisplayName);

            if (list.Columns != null)
            {
                var fields = new List<Dictionary<string, object>>();
                foreach (var column in list.Columns)
                {
                    fields.Add(ConvertField(column));
                }
                dest.Add("Fields", fields);
            }
            else
            {
                dest.Add("Fields", null);
            }

            return dest;
        }

        private Dictionary<string, object> ConvertField(ColumnDefinition column)
        {
            var dest = new Dictionary<string, object>();
            dest.Add("Id", column.Id);
            dest.Add("InternalName", column.Name);
            dest.Add("Title", column.DisplayName);
            dest.Add("Type", GetColumnType(column));
            dest.Add("ClrType", column.GetType().ToString());
            dest.Add("TermSetId", null);
            return dest;
        }

        private string GetColumnType(ColumnDefinition column)
        {
            if (column.AdditionalData != null)
            {
                if (column.AdditionalData.ContainsKey("columnType"))
                {
                    return column.AdditionalData["columnType"]?.ToString();
                }
                if (column.AdditionalData.ContainsKey("odata.type"))
                {
                    return column.AdditionalData["odata.type"]?.ToString();
                }
            }
            return null;
        }

        private Dictionary<string, object> ConvertFolder(DriveItem item)
        {
            return new Dictionary<string, object>
            {
                {"Id", item.Id},
                {"ParentId", item.ParentReference != null ? item.ParentReference.Id : null},
                {"Title", item.Name},
                {"LastModified", item.LastModifiedDateTime}
            };
        }

        private Dictionary<string, object> ConvertFile(DriveItem item)
        {
            return new Dictionary<string, object>
            {
                {"Id", item.Id},
                {"FolderId", item.ParentReference != null ? item.ParentReference.Id : null},
                {"Title", item.Name},
                {"Name", item.Name},
                {"Extension", Path.GetExtension(item.Name)},
                {"TotalSize", item.Size},
                {"Uploaded", item.File != null},
                {"LastModified", item.LastModifiedDateTime},
                {"DateCreated", item.CreatedDateTime}
            };
        }

        private async Task<Dictionary<string, object>> GetFolderContentsAsync(string folderId)
        {
            var retVal = new Dictionary<string, object>();
            List<Dictionary<string, object>> folders = null;
            List<Dictionary<string, object>> documents = null;

            var children = await client.Drives[driveId]
                                       .Items[folderId]
                                       .Children
                                       .Request()
                                       .GetAsync()
                                       .ConfigureAwait(false);

            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child.Folder != null)
                    {
                        var folder = ConvertFolder(child);
                        var sub = await GetFolderContentsAsync(child.Id);
                        folder.Add("Folders", sub["Folders"]);
                        folder.Add("Documents", sub["Documents"]);
                        if (folders == null) folders = new List<Dictionary<string, object>>();
                        folders.Add(folder);
                    }
                    else if (child.File != null)
                    {
                        if (documents == null) documents = new List<Dictionary<string, object>>();
                        documents.Add(ConvertFile(child));
                    }
                }
            }

            retVal.Add("Folders", folders);
            retVal.Add("Documents", documents);
            return retVal;
        }

        private Dictionary<string, object> ConvertTerm(JToken term)
        {
            var dest = new Dictionary<string, object>
            {
                {"Id", (string)term["id"]},
                {"Name", (string)term["labels"]?.First?["name"]}
            };

            var parentId = term["parent"]?["id"]?.ToString();
            if (parentId != null)
            {
                dest.Add("parentId", parentId);
            }

            var children = term["children"] as JArray;
            if (children != null && children.Count > 0)
            {
                var childList = new List<Dictionary<string, object>>();
                foreach (var child in children)
                {
                    childList.Add(ConvertTerm(child));
                }
                dest.Add("Terms", childList);
            }

            return dest;
        }
    }
}
