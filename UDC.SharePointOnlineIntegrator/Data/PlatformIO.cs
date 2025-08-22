using System;
using System.Collections.Generic;
using System.Linq;
using UDC.Common;
using UDC.Common.Data.Models.Configuration;
using Equ.SharePoint.GraphService;

namespace UDC.SharePointOnlineIntegrator.Data
{
    public class PlatformIO
    {
        private readonly IGraphService _graphService;
        public PlatformCfg PlatformConfig { get; set; }

        public PlatformIO(IGraphService graphService)
        {
            _graphService = graphService;
        }

        public PlatformIO(PlatformCfg cfg, IGraphService graphService)
        {
            PlatformConfig = cfg;
            _graphService = graphService;
        }

        public List<Dictionary<string, object>> GetLists()
        {
            return AsyncHelper.RunSync(() => _graphService.GetListsAsync())?.ToList();
        }

        public Dictionary<string, object> GetList(Guid listGuid)
        {
            return AsyncHelper.RunSync(() => _graphService.GetListAsync(listGuid));
        }

        public Dictionary<string, object> GetListTreeStructure(Guid listGuid)
        {
            return AsyncHelper.RunSync(() => _graphService.GetListAsync(listGuid));
        }

        public List<Dictionary<string, object>> GetDocuments(Guid listGuid, bool includeBinary, List<string> fields)
        {
            return AsyncHelper.RunSync(() => _graphService.GetDocumentsAsync(listGuid, fields))?.ToList();
        }

        public List<Dictionary<string, object>> GetDocuments(List<string> docIds, bool includeBinary, List<string> fields)
        {
            var arrSrcFiles = AsyncHelper.RunSync(() => _graphService.GetDocumentsAsync(Guid.Empty, fields))?.ToList();
            if (docIds != null && arrSrcFiles != null)
            {
                arrSrcFiles = arrSrcFiles.Where(f => docIds.Contains(GeneralHelpers.parseString(f["Id"]))).ToList();
            }
            return arrSrcFiles;
        }

        public Dictionary<string, object> GetDocument(string id, bool includeBinary, List<string> fields)
        {
            var arrDocs = GetDocuments(new List<string>() { id }, includeBinary, fields);
            if (arrDocs != null && arrDocs.Count > 0)
            {
                return arrDocs[0];
            }
            return null;
        }

        public List<Dictionary<string, object>> GetTermSets(bool includeTerms)
        {
            var arrTermSets = AsyncHelper.RunSync(() => _graphService.GetTermSetsAsync())?.ToList();
            if (includeTerms && arrTermSets != null)
            {
                foreach (var termSet in arrTermSets)
                {
                    Guid termSetGuid = GeneralHelpers.parseGUID(termSet["Id"]);
                    termSet["Terms"] = AsyncHelper.RunSync(() => _graphService.GetTermsAsync(termSetGuid))?.ToList();
                }
            }
            return arrTermSets;
        }

        public Dictionary<string, object> GetTermSet(Guid termSetGuid, bool includeTerms)
        {
            var arrTermSets = AsyncHelper.RunSync(() => _graphService.GetTermSetsAsync())?.ToList();
            var objTermSet = arrTermSets?.FirstOrDefault(ts => GeneralHelpers.parseGUID(ts["Id"]) == termSetGuid);
            if (objTermSet != null && includeTerms)
            {
                objTermSet["Terms"] = AsyncHelper.RunSync(() => _graphService.GetTermsAsync(termSetGuid))?.ToList();
            }
            return objTermSet;
        }
    }
}
