using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Taxonomies.Model;

using UDC.Common;
using UDC.Common.Data.Models;
using UDC.SitefinityContextPlugin.Data;
using UDC.SitefinityContextPlugin.Models;

namespace UDC.SitefinityContextPlugin.Controllers.API
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [BasicAuthoriseFilter]
    public class SFContextAPIController : ApiController
    {
        #region Taxonomy Methods...
        [HttpGet]
        [Route("udc-api/sfcontext/GetAllHierarchicalTaxonomies/")]
        public Object GetAllHierarchicalTaxonomies()
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = null;
            
            try
            {
                List<HierarchicalTaxonomy> arrTaxonomies = CMSDataIO.GetAllHierarchicalTaxonomies();

                if(arrTaxonomies != null)
                {
                    arrRetVal = new List<Dictionary<String, Object>>();
                    foreach(HierarchicalTaxonomy objTaxonomy in arrTaxonomies)
                    {
                        Dictionary<String, Object> objDestTaxonomy = new Dictionary<String, Object>();
                        TypeConverters.ConvertTaxonomy(objTaxonomy, ref objDestTaxonomy);
                        arrRetVal.Add(objDestTaxonomy);
                    }
                }

                arrTaxonomies = null;

                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetAllHierarchicalTaxonomies", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetHierarchicalTaxonomyTree/")]
        public Object GetHierarchicalTaxonomyTree()
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            try
            {
                arrRetVal = CMSDataIO.GetHierarchicalTaxonomyTree();
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetHierarchicalTaxonomyTree", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetHierarchicalTaxonomyTree/")]
        public Object GetHierarchicalTaxonomyTree(String taxonomyGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            Dictionary<String, Object> objRetVal = new Dictionary<String, Object>();

            try
            {
                Guid taxonomyID = GeneralHelpers.parseGUID(taxonomyGuid);
                if (taxonomyID != Guid.Empty)
                {
                    objRetVal = CMSDataIO.GetHierarchicalTaxonomyTree(taxonomyID);
                }
                objResponse.data = objRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetHierarchicalTaxonomyTree", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetHierarchicalTaxonomy/")]
        public Object GetHierarchicalTaxonomy(String taxonomyGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            Dictionary<String, Object> objRetVal = null;

            try
            {
                Guid taxonomyID = GeneralHelpers.parseGUID(taxonomyGuid);
                if (taxonomyID != Guid.Empty)
                {
                    HierarchicalTaxonomy objSFTaxonomy = CMSDataIO.GetHierarchicalTaxonomy(taxonomyID);

                    if (objSFTaxonomy != null)
                    {
                        objRetVal = new Dictionary<String, Object>();
                        TypeConverters.ConvertTaxonomy(objSFTaxonomy, ref objRetVal);
                    }

                    objSFTaxonomy = null;
                }

                objResponse.data = objRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetHierarchicalTaxonomy", ref ex, ref objResponse);
            }

            return objResponse;
        }

        [HttpGet]
        [Route("udc-api/sfcontext/DeleteTaxon/")]
        public Object DeleteTaxon(String taxonGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid taxonID = GeneralHelpers.parseGUID(taxonGuid);
                if (taxonID != Guid.Empty)
                {
                    arrRetVal = CMSDataIO.DeleteTaxon(taxonID);
                }
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("DeleteTaxon", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpPost]
        [Route("udc-api/sfcontext/DeleteTaxons/")]
        public Object DeleteTaxons([FromBody]GetDocumentArgs formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                List<Guid> arrTaxonIDs = JsonConvert.DeserializeObject<List<Guid>>(formData.guidList);
                if (arrTaxonIDs != null)
                {
                    arrRetVal = CMSDataIO.DeleteTaxons(arrTaxonIDs);
                }
                objResponse.data = arrRetVal;

                arrTaxonIDs = null;
            }
            catch (Exception ex)
            {
                ParseException("DeleteTaxons", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/DeleteChildTaxons/")]
        public Object DeleteChildTaxons(String taxonomyGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid taxonomyId = GeneralHelpers.parseGUID(taxonomyGuid);
                if (taxonomyId != Guid.Empty)
                {
                    arrRetVal = CMSDataIO.DeleteChildTaxons(taxonomyId);
                }
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("DeleteChildTaxons", ref ex, ref objResponse);
            }

            return objResponse;
        }

        [HttpPost]
        [Route("udc-api/sfcontext/SaveTaxonomy/")]
        public Object SaveTaxonomy([FromBody]TaxonomyFormData formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid taxonomyID = GeneralHelpers.parseGUID(formData.taxonomyGuid);
                arrRetVal = CMSDataIO.SaveTaxonomy(taxonomyID, formData.Name, (CMSDataIO.TaxonTypes)formData.TaxonomyType);
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("SaveTaxonomy", ref ex, ref objResponse);
            }

            return objResponse;
        }
        public class TaxonomyFormData
        {
            public String taxonomyGuid { get; set; }
            public String Name { get; set; }
            public Int32 TaxonomyType { get; set; }
        }
        [HttpPost]
        [Route("udc-api/sfcontext/SaveTaxon/")]
        public Object SaveTaxon([FromBody]TaxonFormData formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid taxonID = GeneralHelpers.parseGUID(formData.taxonGuid);
                Guid parentTaxonomyID = GeneralHelpers.parseGUID(formData.parentTaxonomyGuid);
                Guid parentTaxonID = GeneralHelpers.parseGUID(formData.parentTaxonGuid);

                arrRetVal = CMSDataIO.SaveTaxon(taxonID, parentTaxonomyID, parentTaxonID, formData.Name, (CMSDataIO.TaxonTypes)formData.TaxonType);
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("SaveTaxon", ref ex, ref objResponse);
            }

            return objResponse;
        }
        public class TaxonFormData
        {
            public String taxonGuid { get; set; }
            public String parentTaxonomyGuid { get; set; }
            public String parentTaxonGuid { get; set; }
            public String Name { get; set; }
            public Int32 TaxonType { get; set; }
        }
        #endregion

        #region Library Methods...
        [HttpGet]
        [Route("udc-api/sfcontext/GetLibraryTreeStructure/")]
        public Object GetLibraryTreeStructure(String rootLibId)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            Dictionary<String, Object> objRetVal = null;

            try
            {
                Guid objGuid = Guid.Empty;
                if (!Guid.TryParse(rootLibId, out objGuid))
                {
                    objGuid = Guid.Empty;
                }
                objRetVal = CMSDataIO.GetLibraryTreeStructure(objGuid);

                objResponse.data = objRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetLibraryTreeStructure", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetLibraries/")]
        public Object GetLibraries()
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = null;

            try
            {
                List<DocumentLibrary> arrSFLibs = CMSDataIO.GetLibraries();

                if (arrSFLibs != null && arrSFLibs.Count > 0)
                {
                    arrRetVal = new List<Dictionary<String, Object>>();
                    foreach (DocumentLibrary objSrcLib in arrSFLibs)
                    {
                        Dictionary<String, Object> objDestLib = new Dictionary<String, Object>();
                        TypeConverters.ConvertDocumentLibrary(objSrcLib, ref objDestLib);
                        arrRetVal.Add(objDestLib);
                    }
                }

                arrSFLibs = null;

                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetLibraries", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetLibrary/")]
        public Object GetLibrary(String libGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            Dictionary<String, Object> objRetVal = null;

            try
            {
                Guid libID = GeneralHelpers.parseGUID(libGuid);
                if (libID != Guid.Empty)
                {
                    DocumentLibrary objSFLib = CMSDataIO.GetLibrary(libID);

                    if (objSFLib != null)
                    {
                        objRetVal = new Dictionary<String, Object>();
                        TypeConverters.ConvertDocumentLibrary(objSFLib, ref objRetVal);
                    }

                    objSFLib = null;
                }

                objResponse.data = objRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetLibrary", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/DeleteLibrary/")]
        public Object DeleteLibrary(String libGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid libID = GeneralHelpers.parseGUID(libGuid);
                if (libID != Guid.Empty)
                {
                    arrRetVal = CMSDataIO.DeleteLibrary(libID);
                }
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("DeleteLibrary", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpPost]
        [Route("udc-api/sfcontext/SaveLibrary/")]
        public Object SaveLibrary([FromBody]LibraryFormData formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid libID = GeneralHelpers.parseGUID(formData.LibGuid);
                arrRetVal = CMSDataIO.SaveLibrary(libID, formData.Title, null);
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("SaveLibrary", ref ex, ref objResponse);
            }

            return objResponse;
        }
        public class LibraryFormData
        {
            public String LibGuid { get; set; }
            public String Title { get; set; }
        }
        #endregion

        #region Folder Methods...
        [HttpGet]
        [Route("udc-api/sfcontext/GetFolders/")]
        public Object GetFolders(String rootFolderGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = null;

            try
            {
                List<IFolder> arrSFFolders = null;
                Guid rootFolderID = GeneralHelpers.parseGUID(rootFolderGuid);

                if (rootFolderID != Guid.Empty)
                {
                    arrSFFolders = CMSDataIO.GetFolders(rootFolderID);
                    if (arrSFFolders != null && arrSFFolders.Count > 0)
                    {
                        arrRetVal = new List<Dictionary<String, Object>>();
                        foreach (IFolder objSrcFolder in arrSFFolders)
                        {
                            Dictionary<String, Object> objDestLib = new Dictionary<String, Object>();
                            TypeConverters.ConvertDocumentLibrary(objSrcFolder, ref objDestLib);
                            arrRetVal.Add(objDestLib);
                        }
                    }
                }

                arrSFFolders = null;

                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetFolders", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetFolder/")]
        public Object GetFolder(String folderGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            Dictionary<String, Object> objRetVal = null;

            try
            {
                Guid folderID = GeneralHelpers.parseGUID(folderGuid);
                if (folderID != Guid.Empty)
                {
                    IFolder objSFFolder = CMSDataIO.GetFolder(folderID);

                    if (objSFFolder != null)
                    {
                        objRetVal = new Dictionary<String, Object>();
                        TypeConverters.ConvertDocumentLibrary(objSFFolder, ref objRetVal);
                    }

                    objSFFolder = null;
                }

                objResponse.data = objRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetFolder", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/DeleteFolder/")]
        public Object DeleteFolder(String folderGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid folderID = GeneralHelpers.parseGUID(folderGuid);
                if (folderID != Guid.Empty)
                {
                    arrRetVal = CMSDataIO.DeleteFolder(folderID);
                }
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("DeleteFolder", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpPost]
        [Route("udc-api/sfcontext/DeleteFolders/")]
        public Object DeleteFolders([FromBody]GetDocumentArgs formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                List<Guid> arrFolderIDs = JsonConvert.DeserializeObject<List<Guid>>(formData.guidList);
                if (arrFolderIDs != null)
                {
                    arrRetVal = CMSDataIO.DeleteFolders(arrFolderIDs);
                }
                objResponse.data = arrRetVal;

                arrFolderIDs = null;
            }
            catch (Exception ex)
            {
                ParseException("DeleteFolders", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpPost]
        [Route("udc-api/sfcontext/SaveFolder/")]
        public Object SaveFolder([FromBody]FolderFormData formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid folderID = GeneralHelpers.parseGUID(formData.folderGuid);
                Guid libID = GeneralHelpers.parseGUID(formData.libGuid);
                Guid parentFolderID = GeneralHelpers.parseGUID(formData.parentFolderGuid);

                if (libID != Guid.Empty)
                {
                    arrRetVal = CMSDataIO.SaveFolder(folderID, libID, parentFolderID, formData.Title, formData.Description);
                }
                
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("SaveFolder", ref ex, ref objResponse);
            }

            return objResponse;
        }
        public class FolderFormData
        {
            public String folderGuid { get; set; }
            public String libGuid { get; set; }
            public String parentFolderGuid { get; set; }

            public String Title { get; set; }
            public String Description { get; set; }
        }
        #endregion

        #region Document Methods...
        [HttpGet]
        [Route("udc-api/sfcontext/GetDocumentCustomFields/")]
        public Object GetDocumentCustomFields()
        {
            APIResponse objResponse = new APIResponse(0, "Success");

            try
            {
                objResponse.data = CMSDataIO.GetDocumentCustomFields();
            }
            catch (Exception ex)
            {
                ParseException("GetDocumentCustomFields", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetDocuments/")]
        public Object GetDocuments(String libGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            try
            {
                Guid libID = GeneralHelpers.parseGUID(libGuid);
                if (libID != Guid.Empty)
                {
                    List<Document> arrSFDocs = CMSDataIO.GetDocuments(libID);

                    if (arrSFDocs != null && arrSFDocs.Count > 0)
                    {
                        arrRetVal = new List<Dictionary<String, Object>>();
                        foreach (Document objSrcDoc in arrSFDocs)
                        {
                            Dictionary<String, Object> objDestDoc = new Dictionary<String, Object>();
                            TypeConverters.ConvertDocument(objSrcDoc, ref objDestDoc);
                            arrRetVal.Add(objDestDoc);
                        }
                    }

                    arrSFDocs = null;
                }

                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetDocuments", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetDocumentsByFolder/")]
        public Object GetDocumentsByFolder(String folderGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            try
            {
                Guid folderID = GeneralHelpers.parseGUID(folderGuid);
                if (folderID != Guid.Empty)
                {
                    List<Document> arrSFDocs = CMSDataIO.GetDocumentsByFolder(folderID);

                    if (arrSFDocs != null && arrSFDocs.Count > 0)
                    {
                        arrRetVal = new List<Dictionary<String, Object>>();
                        foreach (Document objSrcDoc in arrSFDocs)
                        {
                            Dictionary<String, Object> objDestDoc = new Dictionary<String, Object>();
                            TypeConverters.ConvertDocument(objSrcDoc, ref objDestDoc);
                            arrRetVal.Add(objDestDoc);
                        }
                    }

                    arrSFDocs = null;
                }

                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("GetDocumentsByFolder", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpPost]
        [Route("udc-api/sfcontext/GetDocuments/")]
        public Object GetDocuments([FromBody]GetDocumentArgs formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            try
            {
                List<Guid> arrDocIDs = JsonConvert.DeserializeObject<List<Guid>>(formData.guidList);
                if(arrDocIDs != null)
                {
                    List<Document> arrSFDocs = CMSDataIO.GetDocuments(arrDocIDs);
                    if (arrSFDocs != null && arrSFDocs.Count > 0)
                    {
                        arrRetVal = new List<Dictionary<String, Object>>();
                        foreach (Document objSrcDoc in arrSFDocs)
                        {
                            Dictionary<String, Object> objDestDoc = new Dictionary<String, Object>();
                            TypeConverters.ConvertDocument(objSrcDoc, ref objDestDoc);
                            arrRetVal.Add(objDestDoc);
                        }
                    }
                    arrSFDocs = null;
                }
                objResponse.data = arrRetVal;

                arrDocIDs = null;
            }
            catch (Exception ex)
            {
                ParseException("GetDocuments", ref ex, ref objResponse);
            }

            return objResponse;
        }
        public class GetDocumentArgs
        {
            public String guidList { get; set; }
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetDocument/")]
        public Object GetDocument(String Id)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            Dictionary<String, Object> objRetVal = new Dictionary<String, Object>();

            try
            {
                Guid docID = GeneralHelpers.parseGUID(Id);
                if (docID != Guid.Empty)
                {
                    Document objDoc = CMSDataIO.GetDocument(docID);

                    if (objDoc != null)
                    {
                        objRetVal = new Dictionary<String, Object>();
                        TypeConverters.ConvertDocument(objDoc, ref objRetVal);
                    }

                    objDoc = null;

                    objResponse.data = objRetVal;
                }
            }
            catch (Exception ex)
            {
                ParseException("GetDocument", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/GetDocumentBinary/")]
        public HttpResponseMessage GetDocumentBinary(String Id)
        {
            HttpResponseMessage objRetVal = new HttpResponseMessage(HttpStatusCode.OK);
            APIResponse objResponse = new APIResponse(0, "Success");
            Stream objStream = null;

            try
            {
                Guid docID = GeneralHelpers.parseGUID(Id);
                if (docID != Guid.Empty)
                {
                    objStream = CMSDataIO.GetDocumentStream(docID);
                }
                if (objStream != null)
                {
                    objRetVal.Content = new StreamContent(objStream);
                    objRetVal.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                }
                else
                {
                    objResponse.exitCode = 1;
                    objResponse.message = "Could not load document binary! Check that the document actually exists and the actual file is uploaded.";

                    objRetVal.Content = new StringContent(JsonConvert.SerializeObject(objResponse));
                    objRetVal.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
            }
            catch (Exception ex)
            {
                ParseException("GetDocumentBinary", ref ex, ref objResponse);

                objRetVal.Content = new StringContent(JsonConvert.SerializeObject(objResponse));
                objRetVal.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            objResponse = null;

            return objRetVal;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/DeleteDocument/")]
        public Object DeleteDocument(String docGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                Guid docID = GeneralHelpers.parseGUID(docGuid);
                if (docID != Guid.Empty)
                {
                    arrRetVal = CMSDataIO.DeleteDocument(docID);
                }
                objResponse.data = arrRetVal;
            }
            catch (Exception ex)
            {
                ParseException("DeleteDocument", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpPost]
        [Route("udc-api/sfcontext/DeleteDocuments/")]
        public Object DeleteDocuments([FromBody]GetDocumentArgs formData)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                List<Guid> arrDocIDs = JsonConvert.DeserializeObject<List<Guid>>(formData.guidList);
                if (arrDocIDs != null)
                {
                    arrRetVal = CMSDataIO.DeleteDocuments(arrDocIDs);
                }
                objResponse.data = arrRetVal;

                arrDocIDs = null;
            }
            catch (Exception ex)
            {
                ParseException("DeleteDocuments", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpPost]
        [Route("udc-api/sfcontext/SaveDocument/")]
        public Object SaveDocument()
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            APIActionResult arrRetVal = null;

            try
            {
                DocumentFormData formData = Request.Content.ReadAsAsync<DocumentFormData>().Result;

                if(formData != null)
                {
                    Guid docId = GeneralHelpers.parseGUID(formData.DocGuid);
                    Guid libraryId = GeneralHelpers.parseGUID(formData.LibGuid);
                    Guid folderId = GeneralHelpers.parseGUID(formData.FolderGuid);
                    Dictionary<String, Object> objFieldValues = null;

                    if(!String.IsNullOrEmpty(formData.FieldValues))
                    {
                        objFieldValues = JsonConvert.DeserializeObject<Dictionary<String, Object>>(formData.FieldValues);
                    }
                    if (libraryId != Guid.Empty)
                    {
                        arrRetVal = CMSDataIO.SaveDocument(docId, libraryId, folderId, formData.Title, formData.Filename, formData.DateCreated, formData.LastModified, objFieldValues, formData.FileData);
                    }

                    objFieldValues = null;

                    objResponse.data = arrRetVal;
                }

                formData = null;
            }
            catch (Exception ex)
            {
                ParseException("SaveDocument", ref ex, ref objResponse);
            }

            return objResponse;
        }
        public class DocumentFormData
        {
            public String DocGuid { get; set; }
            public String LibGuid { get; set; }
            public String FolderGuid { get; set; }

            public String Title { get; set; }
            public String Filename { get; set; }

            public String FieldValues { get; set; }

            public Byte[] FileData { get; set; }

            public DateTime DateCreated { get; set; }
            public DateTime LastModified { get; set; }
        }
        #endregion

        [HttpGet]
        [Route("udc-api/sfcontext/RunSearchIndex/")]
        public Object RunSearchIndex(String indexName)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            try
            {
                objResponse.data = CMSDataIO.RunSearchIndex(indexName);
            }
            catch (Exception ex)
            {
                ParseException("RunSearchIndex", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/PurgeDocumentRevisionHistories/")]
        public Object PurgeDocumentRevisionHistories(String libGuid)
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            try
            {
                Guid libID = GeneralHelpers.parseGUID(libGuid);
                if (libID != Guid.Empty)
                {
                    objResponse.data = CMSDataIO.PurgeDocumentRevisionHistories(libID);
                }
            }
            catch (Exception ex)
            {
                ParseException("PurgeDocumentRevisionHistories", ref ex, ref objResponse);
            }

            return objResponse;
        }
        [HttpGet]
        [Route("udc-api/sfcontext/PurgeOrhpanedSFChunks/")]
        public Object PurgeOrhpanedSFChunks()
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            try
            {
                objResponse.data = CMSDataIO.PurgeOrhpanedSFChunks();
            }
            catch (Exception ex)
            {
                ParseException("PurgeOrhpanedSFChunks", ref ex, ref objResponse);
            }

            return objResponse;
        }

        [HttpGet]
        [Route("udc-api/sfcontext/GetVersion/")]
        public Object GetVersion()
        {
            APIResponse objResponse = new APIResponse(0, "Success");
            
            try
            {
                Assembly objAssembly = null;
                String assemblyName = "";
                String versionInfo = "";

                try
                {
                    objAssembly = Assembly.Load("UDC.SitefinityContextPlugin");
                    if (objAssembly != null)
                    {
                        Version version = objAssembly.GetName().Version;
                        assemblyName = objAssembly.GetName().Name;
                        if (version != null)
                        {
                            versionInfo = version.ToString();
                        }
                        version = null;
                    }
                }
                catch (Exception ex) { assemblyName = "Failed to load assembly. " + ex.Message; }

                objResponse.data = assemblyName + " - " + versionInfo;

                versionInfo = null;
                assemblyName = null;
                objAssembly = null;
            }
            catch (Exception ex)
            {
                ParseException("GetVersion", ref ex, ref objResponse);
            }

            return objResponse;
        }

        private void ParseException(String source, ref Exception ex, ref APIResponse response)
        {
            response.exitCode = 1;
            if (ex is AggregateException aggregateException)
            {
                response.message = "A series of errors occurred while trying to serve the request! (" + source + ") \n";
                foreach (var innerEx in aggregateException.InnerExceptions)
                {
                    response.message += innerEx.Message + " -- " + innerEx.StackTrace + "\n\n";
                }
            }
            else
            {
                response.message = "An error occurred while trying to serve the request! (" + source + ") " + ex.Message + " -- " + ex.StackTrace;
            }
        }
    }
}