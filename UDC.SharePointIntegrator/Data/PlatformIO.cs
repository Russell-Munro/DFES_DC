using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;

using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

using UDC.Common;
using UDC.Common.Data.Models.Configuration;

namespace UDC.SharePointIntegrator.Data
{
    //Create Wrappers / Type Converters for our Native SharePoint.Client -> Dictionary Conversions..
    public class PlatformIO
    {
        public String EndPointURL { get; set; }
        public String ServiceUsername { get; set; }
        public String ServicePassword { get; set; }
        public String ServiceDomain { get; set; }

        public PlatformIO()
        {
            LoadSettings();
        }
        public PlatformIO(String endPointURL, String serviceUsername, String servicePassword, String serviceDomain)
        {
            this.EndPointURL = endPointURL;
            this.ServiceUsername = serviceUsername;
            this.ServicePassword = servicePassword;
            this.ServiceDomain = serviceDomain;
        }
        public PlatformIO(PlatformCfg cfg)
        {
            if(cfg != null)
            {
                this.EndPointURL = cfg.EndPointURL;
                this.ServiceUsername = cfg.ServiceUsername;
                this.ServicePassword = cfg.ServicePassword;
                this.ServiceDomain = cfg.ServiceDomain;
            }
            else
            {
                LoadSettings();
            }
        }

        private void LoadSettings()
        {
            this.EndPointURL = AppSettings.GetValue("SharePoint:EndPointURL");
            this.ServiceUsername = AppSettings.GetValue("SharePoint:ServiceUsername");
            this.ServicePassword = AppSettings.GetValue("SharePoint:ServicePassword");
            this.ServiceDomain = AppSettings.GetValue("SharePoint:ServiceDomain");
        }
        private Boolean ValidateSettings()
        {
            Boolean blnRetVal = true;
            if(String.IsNullOrEmpty(this.EndPointURL))
            {
                blnRetVal = false;
            }
            if (String.IsNullOrEmpty(this.ServiceUsername))
            {
                blnRetVal = false;
            }
            if (String.IsNullOrEmpty(this.ServicePassword))
            {
                blnRetVal = false;
            }
            if (String.IsNullOrEmpty(this.ServiceDomain))
            {
                blnRetVal = false;
            }
            return blnRetVal;
        }

        public ClientContext GetClientContext()
        {
            ClientContext context = null;
            
            if (ValidateSettings())
            {
                NetworkCredential objCredentials = new NetworkCredential(this.ServiceUsername, this.ServicePassword, this.ServiceDomain);

                context = new ClientContext(this.EndPointURL);
                context.Credentials = objCredentials;
            }
            else
            {
                throw new Exception("Platform not configured! Please check settings and try again.");
            }

            return context;
        }

        public List<Dictionary<String, Object>> GetLists()
        {
            List<Dictionary<String, Object>> arrRetVal = null;
            List<List> arrSrcLists = null;
            ClientContext context = GetClientContext();

            context.Load(context.Web.Lists, 
                lists => lists.Include(
                    list => list.Title, 
                    list => list.Id, 
                    list => list.Fields));
            context.ExecuteQueryAsync().Wait();

            if (context.Web.Lists != null)
            {
                arrSrcLists = context.Web.Lists.ToList();
            }
            if(arrSrcLists != null)
            {
                arrRetVal = new List<Dictionary<String, Object>>();
                foreach (List objSrcLst in arrSrcLists)
                {
                    Dictionary<String, Object> objDestList = new Dictionary<String, Object>();
                    TypeConverters.ConvertList(objSrcLst, ref objDestList);
                    arrRetVal.Add(objDestList);
                }
            }

            arrSrcLists = null;
            context = null;

            return arrRetVal;
        }
        public Dictionary<String, Object> GetList(Guid listId)
        {
            Dictionary<String, Object> objRetVal = null;
            ClientContext context = GetClientContext();
            List objSrcLst = context.Web.Lists.GetById(listId);

            context.Load(objSrcLst, 
                obj => obj.Id, 
                obj => obj.Title, 
                obj => obj.Fields);
            context.ExecuteQueryAsync().Wait();

            if (objSrcLst != null)
            {
                objRetVal = new Dictionary<String, Object>();
                TypeConverters.ConvertList(objSrcLst, ref objRetVal);
            }

            context = null;

            return objRetVal;
        }
        public Dictionary<String, Object> GetList(String listName)
        {
            Dictionary<String, Object> objRetVal = null;
            ClientContext context = GetClientContext();
            List objSrcLst = context.Web.Lists.GetByTitle(listName);

            context.Load(objSrcLst,
                obj => obj.Id,
                obj => obj.Title,
                obj => obj.Fields);
            context.ExecuteQueryAsync().Wait();

            if (objSrcLst != null)
            {
                objRetVal = new Dictionary<String, Object>();
                TypeConverters.ConvertList(objSrcLst, ref objRetVal);
            }

            context = null;

            return objRetVal;
        }
        //public Dictionary<String, Object> GetListByUrl(String url)
        //{
        //    Dictionary<String, Object> objRetVal = null;
        //    ClientContext context = GetClientContext();
        //    List objSrcLst = context.Web.GetList(url);

        //    context.Load(objSrcLst,
        //        obj => obj.Id,
        //        obj => obj.Title,
        //        obj => obj.Fields);
        //    context.ExecuteQueryAsync().Wait();

        //    if (objSrcLst != null)
        //    {
        //        objRetVal = new Dictionary<String, Object>();
        //        TypeConverters.ConvertList(objSrcLst, ref objRetVal);
        //    }

        //    context = null;

        //    return objRetVal;
        //}

        public List<Dictionary<String, Object>> GetTermSets(Boolean includeTerms)
        {
            List<Dictionary<String, Object>> arrRetVal = null;
            ClientContext context = GetClientContext();
            TaxonomySession objTaxonSession = TaxonomySession.GetTaxonomySession(context);
            TermStore objTermStore = objTaxonSession.GetDefaultSiteCollectionTermStore();

            context.Load(objTermStore);
            context.Load(objTermStore.Groups);
            context.ExecuteQueryAsync().Wait();

            if (objTermStore != null)
            {
                TermGroupCollection arrSrcTermGroups = objTermStore.Groups;

                context.Load(arrSrcTermGroups);
                context.ExecuteQueryAsync().Wait();

                if (arrSrcTermGroups != null)
                {
                    foreach (TermGroup objSrcTGrp in arrSrcTermGroups)
                    {
                        context.Load(objSrcTGrp.TermSets);
                        context.ExecuteQueryAsync().Wait();

                        if (objSrcTGrp != null)
                        {
                            TermSetCollection arrSrcTermSets = objSrcTGrp.TermSets;

                            context.Load(arrSrcTermSets);
                            context.ExecuteQueryAsync().Wait();

                            if (arrSrcTermSets != null)
                            {
                                if(arrRetVal == null)
                                {
                                    arrRetVal = new List<Dictionary<String, Object>>();
                                }
                                foreach (TermSet objSrcTermSet in arrSrcTermSets)
                                {
                                    List<Dictionary<String, Object>> arrDestTerms = null;
                                    Dictionary<String, Object> objDestTermSet = new Dictionary<String, Object>();
                                    TermCollection arrSrcTerms = objSrcTermSet.Terms;

                                    objDestTermSet.Add("Id", objSrcTermSet.Id);
                                    objDestTermSet.Add("Name", objSrcTermSet.Name);
                                    objDestTermSet.Add("Description", objSrcTermSet.Description);

                                    if(includeTerms)
                                    {
                                        context.Load(arrSrcTerms,
                                        tc => tc.Include(
                                            term => term.Id,
                                            term => term.Name,
                                            term => term.Description,
                                            term => term.TermsCount,
                                            term => term.Terms
                                        ));
                                        context.ExecuteQueryAsync().Wait();

                                        if (arrSrcTerms != null)
                                        {
                                            arrDestTerms = new List<Dictionary<String, Object>>();
                                            foreach (Term objSrcTerm in arrSrcTerms)
                                            {
                                                Dictionary<String, Object> objDestTerm = new Dictionary<String, Object>();
                                                TypeConverters.ConvertTerm(objSrcTerm, ref objDestTerm);
                                                objDestTerm["parentId"] = Guid.Empty;
                                                if (objSrcTerm.TermsCount > 0)
                                                {
                                                    Dictionary<String, Object> arrRecursiveCall = RecursiveGetTerms(objSrcTerm, ref context);
                                                    GeneralHelpers.appendDictionary(ref objDestTerm, ref arrRecursiveCall);
                                                }
                                                arrDestTerms.Add(objDestTerm);
                                            }
                                        }
                                        objDestTermSet.Add("Terms", arrDestTerms);
                                    }
                                    arrRetVal.Add(objDestTermSet);

                                    arrSrcTerms = null;
                                }
                            }

                            arrSrcTermSets = null;
                        }
                    }
                }

                arrSrcTermGroups = null;
            }

            objTermStore = null;
            objTaxonSession = null;
            context = null;

            return arrRetVal;
        }
        public Dictionary<String, Object> GetTermSet(Guid id, Boolean includeTerms)
        {
            Dictionary<String, Object> objRetVal = null;
            ClientContext context = GetClientContext();
            TaxonomySession objTaxonSession = TaxonomySession.GetTaxonomySession(context);
            TermStore objTermStore = objTaxonSession.GetDefaultSiteCollectionTermStore();

            context.Load(objTermStore);
            context.ExecuteQueryAsync().Wait();

            if (objTermStore != null)
            {
                TermSet objSrcTermSet = objTermStore.GetTermSet(id);

                objRetVal = new Dictionary<String, Object>();
                context.Load(objSrcTermSet);
                context.ExecuteQueryAsync().Wait();

                objRetVal.Add("Id", objSrcTermSet.Id);
                objRetVal.Add("Name", objSrcTermSet.Name);
                objRetVal.Add("Description", objSrcTermSet.Description);

                if (includeTerms)
                {
                    TermCollection arrSrcTerms = objSrcTermSet.Terms;
                    List<Dictionary<String, Object>> arrDestTerms = null;

                    context.Load(arrSrcTerms,
                    tc => tc.Include(
                        term => term.Id,
                        term => term.Name,
                        term => term.Description,
                        term => term.TermsCount,
                        term => term.Terms
                    ));
                    context.ExecuteQueryAsync().Wait();

                    if (arrSrcTerms != null)
                    {
                        arrDestTerms = new List<Dictionary<String, Object>>();
                        foreach (Term objSrcTerm in arrSrcTerms)
                        {
                            Dictionary<String, Object> objDestTerm = new Dictionary<String, Object>();
                            TypeConverters.ConvertTerm(objSrcTerm, ref objDestTerm);
                            objDestTerm["parentId"] = Guid.Empty;
                            if (objSrcTerm.TermsCount > 0)
                            {
                                Dictionary<String, Object> arrRecursiveCall = RecursiveGetTerms(objSrcTerm, ref context);
                                GeneralHelpers.appendDictionary(ref objDestTerm, ref arrRecursiveCall);
                            }
                            arrDestTerms.Add(objDestTerm);
                        }
                    }
                    objRetVal.Add("Terms", arrDestTerms);

                    arrSrcTerms = null;
                }

                objSrcTermSet = null;
            }

            objTermStore = null;
            objTaxonSession = null;
            context = null;

            return objRetVal;
        }
        private Dictionary<String, Object> RecursiveGetTerms(Term parentTerm, ref ClientContext context)
        {
            Dictionary<String, Object> objRetVal = new Dictionary<String, Object>();
            List<Dictionary<String, Object>> arrDestTerms = null;
            TermCollection arrTerms = parentTerm.Terms;

            context.Load(arrTerms,
                tc => tc.Include(
                    term => term.Id,
                    term => term.Name,
                    term => term.Description,
                    term => term.TermsCount,
                    term => term.Terms
                ));
            context.ExecuteQueryAsync().Wait();

            if (arrTerms != null)
            {
                arrDestTerms = new List<Dictionary<String, Object>>();
                foreach (Term objTerm in arrTerms)
                {
                    Dictionary<String, Object> objDestTerm = new Dictionary<String, Object>();
                    TypeConverters.ConvertTerm(objTerm, ref objDestTerm);
                    objDestTerm["parentId"] = parentTerm.Id;
                    if (objTerm.TermsCount > 0)
                    {
                        Dictionary<String, Object> arrRecursiveCall = RecursiveGetTerms(objTerm, ref context);
                        GeneralHelpers.appendDictionary(ref objRetVal, ref arrRecursiveCall);
                    }
                    arrDestTerms.Add(objDestTerm);
                }
            }

            objRetVal.Add("Terms", arrDestTerms);

            return objRetVal;
        }

        public Dictionary<String, Object> GetListTreeStructure(Guid listId)
        {
            Dictionary<String, Object> objRetVal = null;
            ClientContext context = GetClientContext();
            List objSrcLst = context.Web.Lists.GetById(listId);

            context.Load(objSrcLst);
            context.Load(objSrcLst.Fields);
            context.Load(objSrcLst.RootFolder, f => f.Name, f => f.ServerRelativeUrl, f => f.Folders, f => f.Files);
            context.ExecuteQueryAsync().Wait();

            //objSrcLst.GetItemById
            //objSrcLst.GetItemByUniqueId
            //objSrcLst.GetItems(CAML)

            if (objSrcLst != null)
            {
                objRetVal = new Dictionary<String, Object>();

                Dictionary<String, Object> objContents = GetFolderContents(objSrcLst.RootFolder.ServerRelativeUrl, ref context);
                TypeConverters.ConvertList(objSrcLst, ref objRetVal);
                GeneralHelpers.appendDictionary(ref objRetVal, ref objContents);
                objContents = null;
            }
            
            context = null;

            return objRetVal;
        }
        public Dictionary<String, Object> GetListTreeStructure(String listName)
        {
            Dictionary<String, Object> objRetVal = null;
            ClientContext context = GetClientContext();
            List objSrcLst = context.Web.Lists.GetByTitle(listName);

            context.Load(objSrcLst);
            context.Load(objSrcLst.Fields);
            context.Load(objSrcLst.RootFolder, f => f.Name, f => f.ServerRelativeUrl, f => f.Folders, f => f.Files);
            context.ExecuteQueryAsync().Wait();

            if (objSrcLst != null)
            {
                objRetVal = new Dictionary<String, Object>();

                Dictionary<String, Object> objContents = GetFolderContents(objSrcLst.RootFolder.ServerRelativeUrl, ref context);
                TypeConverters.ConvertList(objSrcLst, ref objRetVal);
                GeneralHelpers.appendDictionary(ref objRetVal, ref objContents);
                objContents = null;
            }

            context = null;

            return objRetVal;
        }
        private Dictionary<String, Object> GetFolderContents(String parentFolderRelativeUrl, ref ClientContext context)
        {
            Dictionary<String, Object> retVal = new Dictionary<String, Object>();
            Folder objSrcFolder = context.Web.GetFolderByServerRelativeUrl(parentFolderRelativeUrl);
            
            List<Dictionary<String, Object>> arrDestFolders = null;
            List<Dictionary<String, Object>> arrDestItems = null;

            context.Load(objSrcFolder,
                f => f.Name,
                f => f.ServerRelativeUrl,
                //f => f.TimeLastModified,
                f => f.Folders,
                f => f.Files
            );
            context.ExecuteQueryAsync().Wait();

            if (objSrcFolder != null)
            {
                if (objSrcFolder.Folders != null && objSrcFolder.Folders.Count > 0)
                {
                    arrDestFolders = new List<Dictionary<String, Object>>();
                    foreach (Folder objFolder in objSrcFolder.Folders)
                    {
                        Dictionary<String, Object> objDestFolder = new Dictionary<String, Object>();
                        TypeConverters.ConvertFolder(objFolder, parentFolderRelativeUrl, ref objDestFolder);

                        //Recurse to next level...
                        Dictionary<String, Object> arrRecursiveCall = GetFolderContents(objFolder.ServerRelativeUrl, ref context);
                        GeneralHelpers.appendDictionary(ref objDestFolder, ref arrRecursiveCall);

                        arrDestFolders.Add(objDestFolder);
                    }
                }
                if (objSrcFolder.Files != null && objSrcFolder.Files.Count > 0)
                {
                    arrDestItems = new List<Dictionary<String, Object>>();

                    foreach (File objFile in objSrcFolder.Files)
                    {
                        Dictionary<String, Object> objDestItem = new Dictionary<String, Object>();
                        TypeConverters.ConvertFile(objFile, parentFolderRelativeUrl, ref objDestItem);
                        arrDestItems.Add(objDestItem);
                    }
                }
            }

            retVal.Add("Folders", arrDestFolders);
            retVal.Add("Documents", arrDestItems);

            return retVal;
        }

        public List<Dictionary<String, Object>> GetDocuments(Guid listId, Boolean includeBinary, List<String> fields)
        {
            List<Dictionary<String, Object>> arrRetVal = null;
            ClientContext context = GetClientContext();
            List objSrcLst = context.Web.Lists.GetById(listId);

            context.Load(objSrcLst);
            context.ExecuteQueryAsync().Wait();

            if(objSrcLst != null)
            {
                ListItemCollection arrListItems = null;
                CamlQuery objCaml = new CamlQuery();

                objCaml.ViewXml = "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">0</Value></Eq></Where></Query></View>";
                arrListItems = objSrcLst.GetItems(objCaml);

                context.Load(arrListItems, arr => arr.Include(li => li.File));
                context.ExecuteQueryAsync().Wait();

                if(arrListItems != null)
                {
                    arrRetVal = new List<Dictionary<String, Object>>();

                    if(fields != null && fields.Count > 0)
                    {
                        //Bulk Load Fields over all files...
                        foreach (File objSrcFile in arrListItems.Where(obj => obj.File != null).Select(obj => obj.File))
                        {
                            foreach (String field in fields)
                            {
                                context.Load(objSrcFile, obj => obj.ListItemAllFields[field]);
                            }
                        }
                        context.ExecuteQueryAsync().Wait();
                    }

                    Int32 counter = 0;
                    foreach (File objSrcFile in arrListItems.Where(obj => obj.File != null).Select(obj => obj.File))
                    {
                        Dictionary<String, Object> objDestDoc = new Dictionary<String, Object>();
                        TypeConverters.ConvertFile(objSrcFile, "", ref objDestDoc);

                        if(counter < 3)
                        {
                            if (includeBinary && objSrcFile.Exists)
                            {
                                ClientResult<System.IO.Stream> objFI = objSrcFile.OpenBinaryStream();

                                context.Load(objSrcFile);
                                context.ExecuteQueryAsync().Wait();

                                if (objFI != null && objFI.Value != null)
                                {
                                    Byte[] arrBytes = GeneralHelpers.parseBinaryStream(objFI.Value);
                                    objDestDoc.Add("FileData", arrBytes);
                                }

                                objFI = null;
                            }
                        }
                        
                        arrRetVal.Add(objDestDoc);

                        counter++;
                    }
                }

                objCaml = null;
                arrListItems = null;
            }

            objSrcLst = null;
            context = null;

            return arrRetVal;
        }
        public List<Dictionary<String, Object>> GetDocuments(List<String> fileRelativeUrls, Boolean includeBinary, List<String> fields)
        {
            List<Dictionary<String, Object>> arrRetVal = null;

            if (fileRelativeUrls != null && fileRelativeUrls.Count > 0)
            {
                foreach (String fileUrl in fileRelativeUrls)
                {
                    Dictionary<String, Object> objDoc = GetDocument(fileUrl, includeBinary, fields);

                    if (objDoc != null)
                    {
                        if (arrRetVal == null)
                        {
                            arrRetVal = new List<Dictionary<String, Object>>();
                        }
                        arrRetVal.Add(objDoc);
                    }
                }
            }

            return arrRetVal;
        }
        public Dictionary<String, Object> GetDocument(String fileRelativeUrl, Boolean includeBinary, List<String> fields)
        {
            Dictionary<String, Object> objRetVal = null;
            List<String> arrErrors = new List<String>();
            ClientContext context = GetClientContext();
            File objSrcFile = context.Web.GetFileByServerRelativeUrl(fileRelativeUrl);
            
            context.Load(objSrcFile);
            context.Load(objSrcFile, obj => obj.ListItemAllFields);

            if (fields != null && fields.Count > 0)
            {
                foreach (String field in fields)
                {
                    context.Load(objSrcFile, obj => obj.ListItemAllFields[field]);
                }
            }
            context.ExecuteQueryAsync().Wait();

            if(objSrcFile != null)
            {
                List objSrcLst = null;

                context.Load(objSrcFile, obj => obj.ServerRelativeUrl);
                context.ExecuteQueryAsync().Wait();

                objSrcLst = context.Web.GetList("/" + objSrcFile.ServerRelativeUrl.Split('/')[1]);

                context.Load(objSrcLst, obj => obj.Fields);
                context.ExecuteQueryAsync().Wait();

                objRetVal = new Dictionary<String, Object>();
                TypeConverters.ConvertFile(objSrcFile, "", ref objRetVal);

                if (includeBinary && objSrcFile.Exists)
                {
                    ClientResult<System.IO.Stream> objFI = objSrcFile.OpenBinaryStream();

                    context.Load(objSrcFile);
                    context.ExecuteQueryAsync().Wait();

                    if (objFI != null && objFI.Value != null)
                    {
                        Byte[] arrBytes = GeneralHelpers.parseBinaryStream(objFI.Value);
                        objRetVal.Add("FileData", arrBytes);
                    }

                    objFI = null;
                }

                if (fields != null && fields.Count > 0 && objSrcLst != null && objSrcLst.Fields != null)
                {
                    foreach (String field in fields)
                    {
                        Dictionary<String, Object> objFieldValue = null;
                        String wrappedType = "";
                        String nativeType = "";
                        Object val = null;

                        try
                        {
                            if (objSrcFile.ListItemAllFields != null && objSrcFile.ListItemAllFields[field] != null)
                            {
                                objFieldValue = new Dictionary<String, Object>();
                                nativeType = objSrcFile.ListItemAllFields[field].GetType().ToString();
                                val = objSrcFile.ListItemAllFields[field];

                                if (objSrcFile.ListItemAllFields[field] is TaxonomyFieldValue || objSrcFile.ListItemAllFields[field] is TaxonomyFieldValueCollection)
                                {
                                    //Work with Taxonomy Values... Will treat as always collection of string guids for simplicity up the chain...
                                    List<String> arrTaxonIDs = new List<String>();

                                    wrappedType = "TaxonomyList";
                                    if (objSrcFile.ListItemAllFields[field] is TaxonomyFieldValue)
                                    {
                                        TaxonomyFieldValue objTaxonFld = (TaxonomyFieldValue)objSrcFile.ListItemAllFields[field];
                                        arrTaxonIDs.Add(GeneralHelpers.parseGUID(objTaxonFld.TermGuid).ToString());
                                        objTaxonFld = null;
                                    }
                                    else if (objSrcFile.ListItemAllFields[field] is TaxonomyFieldValueCollection)
                                    {
                                        TaxonomyFieldValueCollection arrTaxonFlds = (TaxonomyFieldValueCollection)objSrcFile.ListItemAllFields[field];
                                        foreach (TaxonomyFieldValue objSrcTaxon in arrTaxonFlds)
                                        {
                                            arrTaxonIDs.Add(GeneralHelpers.parseGUID(objSrcTaxon.TermGuid).ToString());
                                        }
                                        arrTaxonFlds = null;
                                    }
                                    val = arrTaxonIDs;

                                    arrTaxonIDs = null;
                                }
                                else if (objSrcFile.ListItemAllFields[field] is FieldUserValue)
                                {
                                    wrappedType = "String";
                                    val = GeneralHelpers.sanitizeString(((FieldUserValue)objSrcFile.ListItemAllFields[field]).LookupValue);
                                }
                                else if (objSrcFile.ListItemAllFields[field] is FieldGeolocationValue)
                                {
                                    wrappedType = "String";
                                    val = ((FieldGeolocationValue)objSrcFile.ListItemAllFields[field]).Latitude.ToString() + "," + ((FieldGeolocationValue)objSrcFile.ListItemAllFields[field]).Longitude.ToString();
                                }
                                else if (objSrcFile.ListItemAllFields[field] is FieldLookupValue)
                                {
                                    wrappedType = "String";
                                    val = GeneralHelpers.sanitizeString(((FieldLookupValue)objSrcFile.ListItemAllFields[field]).LookupValue);
                                }
                                else if (objSrcFile.ListItemAllFields[field] is FieldUrlValue)
                                {
                                    wrappedType = "String";
                                    val = GeneralHelpers.sanitizeString(((FieldUrlValue)objSrcFile.ListItemAllFields[field]).Url);
                                }
                                else if (objSrcFile.ListItemAllFields[field] is FieldStringValues)
                                {
                                    wrappedType = "Dictionary";
                                    val = ((FieldStringValues)objSrcFile.ListItemAllFields[field]).FieldValues;
                                }

                                objFieldValue.Add("NativeType", nativeType);
                                objFieldValue.Add("WrappedType", wrappedType);
                                objFieldValue.Add("Value", val);
                            }
                            if (!objRetVal.ContainsKey(field))
                            {
                                objRetVal.Add(field, objFieldValue);
                            }
                            else
                            {
                                if (objRetVal[field] == null && objFieldValue != null)
                                {
                                    objRetVal[field] = objFieldValue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            arrErrors.Add("An error occured while accessing ListItemAllFields on File from SharePoint. " + ex.Message);
                        }
                    }
                }
                
                objSrcLst = null;
            }

            objRetVal.Add("_errors", arrErrors);

            context = null;

            return objRetVal;
        }
    }
}