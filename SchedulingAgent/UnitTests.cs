using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Database.Logging;
using UDC.Common.Data.Models;
using UDC.Common.Interfaces;

using UDC.DataConnectorCore.Models;
using UDC.DataConnectorCore;

using SP = UDC.SharePointIntegrator.Data;
using SF = UDC.SitefinityIntegrator.Data;

using static UDC.Common.Constants;

namespace UDC.SchedulingAgent
{
    public static class UnitTests
    {
        #region Test Set Up Database...
        public static void TestSetupDatabase()
        {
            //using (DatabaseContext objDB = new DatabaseContext())
            //{
            //    Console.WriteLine("Ensuring DB Schema Exists...");
            //    objDB.CreateDatabaseSchema();

            //    List<Connection> arrConnections = objDB.Connections.ToList();
            //    if (arrConnections != null)
            //    {
            //        Console.WriteLine("arrConnections has: " + arrConnections.Count + " items...");
            //    }
            //    else
            //    {
            //        Console.WriteLine("arrConnections is null...");
            //    }
            //    arrConnections = null;
            //}
        }
        #endregion
        #region Test Cryptor...
        public static void TestCryptor()
        {
            //String appSecret = AppSettings.GetValue("SharedKey");
            //String targetText = "I am a string to encrypt...";
            //String encrypted = Common.Cryptor.Encrypt(targetText, appSecret);

            //Console.WriteLine("encrypted:" + encrypted);
            //Console.WriteLine("decrypted:" + Common.Cryptor.Decrypt(encrypted, appSecret));
        }
        #endregion

        #region Test Sitefinity Integration...
        public static void TestSitefinityIntegrator()
        {
            APIResponse objAPIResponse = null;
            SF.PlatformIO objSFIO = new SF.PlatformIO();

            //Will still need to Test Save Routines once we have them...
            //[X] objAPIResponse = objSFIO.GetHierarchicalTaxonomyTree();
            //[X] objAPIResponse = objSFIO.GetHierarchicalTaxonomy(new Guid("f5714de2-afa1-441e-b869-58b541cb1d4d"));
            //[X] objAPIResponse = objSFIO.DeleteTaxon(new Guid("844a60d7-47e6-4ecf-a69b-408150dd936e"));
            //[X] objAPIResponse = objSFIO.DeleteChildTaxons(new Guid("965a9536-ccec-46cc-a184-40ec8bb4077a"));
            //[X] SaveTaxonomy
            //SF.PlatformIO.TaxonomyFormData objTaxonomy = new SF.PlatformIO.TaxonomyFormData();
            //objTaxonomy.taxonomyGuid = Guid.Empty.ToString();
            //objTaxonomy.Name = "TestApiTaxonomy222";
            //objTaxonomy.TaxonomyType = 1;
            //objAPIResponse = objSFIO.SaveTaxonomy(objTaxonomy);
            //[X] SaveTaxon
            //SF.PlatformIO.TaxonFormData objTaxon = new SF.PlatformIO.TaxonFormData();
            //objTaxon.taxonGuid = Guid.Empty.ToString();
            //objTaxon.parentTaxonomyGuid = new Guid("4e3be3b8-87ba-432a-8c85-096b49297eec").ToString();
            //objTaxon.parentTaxonGuid = Guid.Empty.ToString();
            //objTaxon.Name = "Sub Taxon 1";
            //objTaxon.TaxonType = 1;
            //objAPIResponse = objSFIO.SaveTaxon(objTaxon);
            //
            //[X] 
            objAPIResponse = objSFIO.GetLibraryTreeStructure(new Guid("41109a1d-de8e-4032-be56-8f7591dd819a"));
            //[X] objAPIResponse = objSFIO.GetLibraries();
            //[X] objAPIResponse = objSFIO.GetLibrary(new Guid("41109a1d-de8e-4032-be56-8f7591dd819a"));
            //[X] objAPIResponse = objSFIO.DeleteLibrary(new Guid("0988b2d5-b6ec-4e62-abd9-60ff4dcf8793"));
            //[X] SaveLibrary??
            //SF.PlatformIO.LibraryFormData objLibrary = new SF.PlatformIO.LibraryFormData();
            //objLibrary.LibGuid = Guid.Empty.ToString();
            //objLibrary.Title = "TestApiDocumentLibrary";
            //objAPIResponse = objSFIO.SaveLibrary(objLibrary);
            // 
            //[X] objAPIResponse = objSFIO.GetFolders(new Guid("ef90470d-23a4-4461-8c58-444e1286807d"));
            //[X] objAPIResponse = objSFIO.GetFolder(new Guid("fc5cab2d-3baa-46c1-ab69-41e628385bd9"));
            //[X] objAPIResponse = objSFIO.DeleteFolder(new Guid("1a6b021f-936c-4e6e-9aeb-7e8588aec2cb"));
            //[X] SaveFolder??
            //SF.PlatformIO.FolderFormData objFolder = new SF.PlatformIO.FolderFormData();
            //objFolder.folderGuid = Guid.Empty.ToString();
            //objFolder.libGuid = new Guid("7c5b7f09-f647-43cc-8325-99c0d1cc1436").ToString();
            //objFolder.parentFolderGuid = Guid.Empty.ToString();
            //objFolder.Title = "TestApiFolder1";
            //objFolder.Description = "This folder was magically created by the API...";
            //objAPIResponse = objSFIO.SaveFolder(objFolder);
            //
            //[X] objAPIResponse = objSFIO.GetDocumentCustomFields();
            //[X] objAPIResponse = objSFIO.GetDocuments(new Guid("41109a1d-de8e-4032-be56-8f7591dd819a"));
            //[X] objAPIResponse = objSFIO.GetDocumentsByFolder(new Guid("fc5cab2d-3baa-46c1-ab69-41e628385bd9"));
            //[X] objAPIResponse = objSFIO.GetDocument(new Guid("a1130f6a-d8fe-4bb4-9b91-70b38f816f23"));
            //[X] objAPIResponse = objSFIO.GetDocumentBinary(new Guid("a1130f6a-d8fe-4bb4-9b91-70b38f816f23"));
            //[X] objAPIResponse = objSFIO.DeleteDocument(new Guid("5e69f6fc-f569-4ab2-86ad-5b177052d2c6"));
            //[X] SaveDocument??
            //SF.PlatformIO.DocumentFormData objDocument = new SF.PlatformIO.DocumentFormData();
            //Dictionary<String, Object> objFieldValues = new Dictionary<String, Object>();
            //objDocument.DocGuid = Guid.Empty.ToString();
            //objDocument.LibGuid = new Guid("7c5b7f09-f647-43cc-8325-99c0d1cc1436").ToString();
            //objDocument.FolderGuid = new Guid("2fbb2867-5cf4-4888-92dc-16e65f18e0af").ToString();
            //objDocument.Title = "SharePointConnectorSolutionTopology";
            //objDocument.Filename = "SharePointConnectorSolutionTopology_Render.pdf";
            //objDocument.FieldValues = JsonConvert.SerializeObject(objFieldValues);
            //objDocument.FileData = null;
            //if(File.Exists("D:\\_NTCodeLabsTemp\\Jobs_Client\\Equilibrium\\SharePointConnectorSolutionTopology\\SharePointConnectorSolutionTopology_Render.pdf"))
            //{
            //    objDocument.FileData = File.ReadAllBytes("D:\\_NTCodeLabsTemp\\Jobs_Client\\Equilibrium\\SharePointConnectorSolutionTopology\\SharePointConnectorSolutionTopology_Render.pdf");
            //}
            //objAPIResponse = objSFIO.SaveDocument(objDocument);

            Console.WriteLine(JsonConvert.SerializeObject(objAPIResponse));

            objAPIResponse = null;
            objSFIO = null;
        }
        #endregion

        #region SharePointIntegrator Tests...
        public static void TestSharePointIntegrator()
        {
            SP.PlatformIO objSPIO = new SP.PlatformIO();

            //Lists (Gets List Fields Too)
            //List<Dictionary<String, Object>> arrLists = objSPIO.GetLists();
            //if(arrLists != null)
            //{
            //    Console.WriteLine(JsonConvert.SerializeObject(arrLists));
            //}
            //arrLists = null;

            //Get Single List
            //Dictionary<String, Object> objList = objSPIO.GetList(new Guid("94156236-d65f-480b-9aac-fee68fe5a20c"));
            //Dictionary<String, Object> objList = objSPIO.GetList("VH-Process");
            //if (objList != null)
            //{
            //    Console.WriteLine(JsonConvert.SerializeObject(objList));
            //}
            //objList = null;

            //TreeStruct
            ////Dictionary<String, Object> objTreeStruct = objSPIO.GetListTreeStructure(new Guid("94156236-d65f-480b-9aac-fee68fe5a20c"));
            //Dictionary<String, Object> objTreeStruct = objSPIO.GetListTreeStructure("VH-Process");
            //Console.WriteLine("\n\n");
            //Console.WriteLine(JsonConvert.SerializeObject(objTreeStruct));
            //objTreeStruct = null;

            //TermSets / Taxons
            //List<Dictionary<String, Object>> arrTermSets = objSPIO.GetTermSets();
            //if(arrTermSets != null)
            //{
            //    Console.WriteLine(JsonConvert.SerializeObject(arrTermSets));
            //}
            //arrTermSets = null;

            //Get some Actual Documents and their Binary Payloads...
            //Dictionary<String, Object> objDoc = objSPIO.GetDocument("/VPProcess/Workers-Compensation-and-Injury-Management/Presumptive-Cancer-Claims/Presumptive-Legislation-for-Volunteers-for-Prescribed-Cancers.PDF", false,
            //    new List<String>() {
            //        "DocumentType",
            //        "Topic",
            //        "Audience1",
            //        "VHDescription",
            //        "Service1",
            //        "VHKeywords0",
            //        "CircularTypes",
            //        "Region",
            //        "Author"
            //    });
            //if (objDoc != null)
            //{
            //    Console.WriteLine(JsonConvert.SerializeObject(objDoc));
            //}
            //objDoc = null;


            objSPIO = null;
        }
        #endregion

        #region Test resolution of Platforms / Integrators...
        public static void TestPlatformProviderResolution()
        {
            //List<IPlatform> arrSupportedPlatforms = ProviderHelpers.GetPlatformInstances();

            //if (arrSupportedPlatforms != null)
            //{
            //    Console.WriteLine("\n\nSupported Platforms: ");
            //    foreach (IPlatform platform in arrSupportedPlatforms)
            //    {
            //        Console.WriteLine("* " + platform.Name + " (" + platform.PlatformID + ")");

            //        List<IIntegrator> arrSrcIntegrators = ProviderHelpers.GetSrcIntegrators(platform);
            //        List<IIntegrator> arrDestIntegrators = ProviderHelpers.GetDestIntegrators(platform);

            //        if (arrSrcIntegrators != null)
            //        {
            //            Console.WriteLine("\tSources:");
            //            foreach (IIntegrator src in arrSrcIntegrators)
            //            {
            //                Console.WriteLine("\t\t" + src.Name + " (" + src.IntegratorID + ")");
            //            }
            //        }
            //        if (arrDestIntegrators != null)
            //        {
            //            Console.WriteLine("\tDestinations:");
            //            foreach (IIntegrator dst in arrDestIntegrators)
            //            {
            //                Console.WriteLine("\t\t" + dst.Name + " (" + dst.IntegratorID + ")");
            //            }
            //        }
            //        arrDestIntegrators = null;
            //        arrSrcIntegrators = null;
            //    }
            //}
            //arrSupportedPlatforms = null;

            //Console.WriteLine("\n\n");

            ////Try Getting a Single Integrator By ID...
            //Console.WriteLine("Single Integrator:");
            //IIntegrator objSFDocsIntegrator = ProviderHelpers.GetIntegrator(new Guid("5f49ff89-604f-4e29-935e-bd463dff354e"));
            //if (objSFDocsIntegrator != null)
            //{
            //    Console.WriteLine(objSFDocsIntegrator.Name);
            //}
            //objSFDocsIntegrator = null;

            //Console.WriteLine("\n\n");
        }
        #endregion

        #region Abstract Sync Methods Test...
        public static void PlatformAgnosticTests()
        {
            Int64 targetRuleID = 2;
            SyncService objSyncSvc = new SyncService(LogVerbosityLevels.All);
            SyncStatus objSyncStatus = null;
            String strOutput = "";

            objSyncSvc.SyncStateUpdated += ObjSyncSvc_SyncStateUpdated;
            objSyncStatus = objSyncSvc.ExecuteRuleSync(targetRuleID);

            strOutput += "TagsCreated: " + objSyncStatus.TagsCreated + "\n";
            strOutput += "TagsUpdated: " + objSyncStatus.TagsUpdated + "\n";
            strOutput += "TagsSkipped: " + objSyncStatus.TagsSkipped + "\n";
            strOutput += "TagsDeleted: " + objSyncStatus.TagsDeleted + "\n\n";
            strOutput += "ContainersCreated: " + objSyncStatus.ContainersCreated + "\n";
            strOutput += "ContainersUpdated: " + objSyncStatus.ContainersUpdated + "\n";
            strOutput += "ContainersSkipped: " + objSyncStatus.ContainersSkipped + "\n";
            strOutput += "ContainersDeleted: " + objSyncStatus.ContainersDeleted + "\n\n";
            strOutput += "ObjectsCreated: " + objSyncStatus.ObjectsCreated + "\n";
            strOutput += "ObjectsUpdated: " + objSyncStatus.ObjectsUpdated + "\n";
            strOutput += "ObjectsSkipped: " + objSyncStatus.ObjectsSkipped + "\n";
            strOutput += "ObjectsDeleted: " + objSyncStatus.ObjectsDeleted + "\n\n";
            strOutput += "SyncTimeElapsed: " + objSyncStatus.SyncTimeElapsed.ToString(@"hh\:mm\:ss") + "\n";

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(strOutput);

            try
            {
                Logger.Write(targetRuleID, LogTypes.Trace, LogActions.Stopped, LogResults.Success, "PlatformAgnosticTests()", "Sync Log Results. See data for details.", JsonConvert.SerializeObject(objSyncStatus));
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to write Sync Log to database. Check Database is configured correctly and the service is running. " + ex.Message);
            }
            objSyncStatus = null;
            objSyncSvc = null;

            Console.ResetColor();
        }
        private static void ObjSyncSvc_SyncStateUpdated(object sender, SyncService.SyncStateUpdatedEventArgs e)
        {
            switch(e.SyncState.LogType)
            {
                case Constants.LogTypes.Trace:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Constants.LogTypes.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Constants.LogTypes.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.Write(e.SyncState.TimeStamp.ToString() + " - ");
            if (e.SyncState.LogType != Constants.LogTypes.Trace)
            {
                Console.Write(e.SyncState.LogType.ToString() + ": ");
            }
            
            //[X] e.SyncState.TimeStamp
            //[ ] e.SyncState.Source
            //[ ] e.SyncState.SourceDesc
            //[X] e.SyncState.Msg

            //[ ] e.SyncState.Data
            //[ ] e.SyncState.Exception

            Console.WriteLine(e.SyncState.Msg);

            if (e.SyncState.LogType == Constants.LogTypes.Error)
            {
                Console.WriteLine(e.SyncState.Exception);
                Console.WriteLine(e.SyncState.Data);
            }

            Console.ResetColor();
        }

        #region Sitefinity
        public static void AbstractSyncMethods_Sitefinity()
        {
            IIntegrator objSFIntegrator = ProviderHelpers.GetIntegrator(new Guid("5f49ff89-604f-4e29-935e-bd463dff354e"));

            //* GetContainers Test
            //List<SyncContainer> arrContainers = objSFIntegrator.GetContainers();
            //Console.WriteLine(JsonConvert.SerializeObject(arrContainers));
            //arrContainers = null;

            //* GetContainerTree Test
            //SyncContainer objRootContainer = objSFIntegrator.GetContainerTree("41109a1d-de8e-4032-be56-8f7591dd819a");
            //Console.WriteLine(JsonConvert.SerializeObject(objRootContainer));
            //objRootContainer = null;

            //* Abstract Container -> Library / Folder Creation - Nested
            /*
            SyncContainer objMyNewLib = new SyncContainer();
            String libId = "";
            objMyNewLib.Name = "objMyNewLib_AbstractApiTest";
            libId = objSFIntegrator.SaveContainer(objMyNewLib, true, "");
            Console.WriteLine(libId);
            objMyNewLib = null;
            SyncContainer objMyNewFolder = new SyncContainer();
            String folderId = "";
            objMyNewFolder.Name = "SubFolder_1";
            folderId = objSFIntegrator.SaveContainer(objMyNewFolder, false, libId);
            Console.WriteLine(folderId);
            objMyNewFolder = null;
            SyncContainer objMyNewSubFolder = new SyncContainer();
            String subfolderId = "";
            objMyNewSubFolder.parentId = folderId;
            objMyNewSubFolder.Name = "SubFolder_2";
            subfolderId = objSFIntegrator.SaveContainer(objMyNewSubFolder, false, libId);
            Console.WriteLine(subfolderId);
            objMyNewSubFolder = null;
            */

            //* Get Object Fields Test
            //List<SyncField> arrFields = objSFIntegrator.GetFields(null);
            //Console.WriteLine(JsonConvert.SerializeObject(arrFields));
            //arrFields = null;

            //* Get Sync Tags (List) Test
            //List<SyncTag> arrTags = objSFIntegrator.GetMetaTagsList();
            //Console.WriteLine(JsonConvert.SerializeObject(arrTags));
            //arrTags = null;

            //* Get Tag and Hierarchical Tree Test
            //SyncTag objRootTag = objSFIntegrator.GetMetaTagTree("f5714de2-afa1-441e-b869-58b541cb1d4d");
            //Console.WriteLine(JsonConvert.SerializeObject(objRootTag));
            //objRootTag = null;

            //* Abstract Tag -> Taxonomy / Taxon Creation - Nested
            /*
            SyncTag objMyNewTaxonomy = new SyncTag();
            String taxonomyId = "";
            objMyNewTaxonomy.Name = "objMyNewTaxonomy_AbstractApiTest";
            taxonomyId = objSFIntegrator.SaveMetaTag(objMyNewTaxonomy, true, "");
            Console.WriteLine(taxonomyId);
            objMyNewTaxonomy = null;
            SyncTag objMyNewTaxon = new SyncTag();
            String taxonId = "";
            objMyNewTaxon.Name = "SubTaxon_1";
            taxonId = objSFIntegrator.SaveMetaTag(objMyNewTaxon, false, taxonomyId);
            Console.WriteLine(taxonId);
            objMyNewTaxon = null;
            SyncTag objMyNewSubTaxon = new SyncTag();
            String subtaxonId = "";
            objMyNewSubTaxon.parentId = taxonId;
            objMyNewSubTaxon.Name = "SubTaxon_2";
            subtaxonId = objSFIntegrator.SaveMetaTag(objMyNewSubTaxon, false, taxonomyId);
            Console.WriteLine(subtaxonId);
            objMyNewSubTaxon = null;
            */

            //* Test Save SyncObject / Document
            //...

            objSFIntegrator = null;
        }
        #endregion
        #region SharePoint
        public static void AbstractSyncMethods_SharePoint()
        {
            IIntegrator objSPIntegrator = ProviderHelpers.GetIntegrator(new Guid("fcbd7cf3-36dc-4a8e-97c0-eb9f3b0665cc"));

            //Test Read Only Methods... For now we are not writing back to SharePoint...
            //* GetContainers Test
            //List<SyncContainer> arrContainers = objSPIntegrator.GetContainers();
            //Console.WriteLine(JsonConvert.SerializeObject(arrContainers));
            //arrContainers = null;

            //* GetTreeStructure for Container....
            //SyncContainer objContainer = objSPIntegrator.GetContainerTree("956ba9da-8bd7-4d84-b681-d072048d3876");
            //Console.WriteLine(JsonConvert.SerializeObject(objContainer));
            //objContainer = null;

            //* Get Object Fields for Container....
            //List<SyncField> arrFields = objSPIntegrator.GetFields("956ba9da-8bd7-4d84-b681-d072048d3876");
            //Console.WriteLine(JsonConvert.SerializeObject(arrFields));
            //arrFields = null;

            //* Get Meta Tags....
            //List<SyncTag> arrTags = objSPIntegrator.GetMetaTagsList();
            //Console.WriteLine(JsonConvert.SerializeObject(arrTags));
            //arrTags = null;

            //* Get Meta Tag Hierarchy....
            //SyncTag objTag = objSPIntegrator.GetMetaTagTree("785f5c65-66ca-4f19-8406-5bcbfc6cef69");
            //Console.WriteLine(JsonConvert.SerializeObject(objTag));
            //objTag = null;

            //* Get Objects By Container (List)....
            //List<SyncObject> arrObjects = objSPIntegrator.GetObjects("956ba9da-8bd7-4d84-b681-d072048d3876");
            //Console.WriteLine(JsonConvert.SerializeObject(arrObjects));
            //arrObjects = null;

            //* Get Objects By IDs (Contains)....
            //SharePointIntegrator.Integrators.SharePointDocumentsProvider objSPIntegrator2 = new SharePointIntegrator.Integrators.SharePointDocumentsProvider();
            //List<String> arrIDs = new List<String>()
            //{
            //    "/VPProcess/Ops-Resources-Communications/Goldfields-Midlands/Goldfields-Midlands_Communication-Resources_A2.pdf",
            //    "/VPProcess/Ops-Resources-Communications/Goldfields-Midlands/Beverley_108.pdf",
            //    "/VPProcess/IM-TOOLBOX/ops-resources-operational-reference/DFES-IMToolbox-Midwest-Gascoyne-State-Out-of-Region-Incident-Support-Plan.pdf",
            //    "/VPProcess/IM-TOOLBOX/ops-resources-operational-reference/DFES-IMToolbox-Perth-Metropolitan-Regions-Resource-Request-Flowchart.pdf",
            //    "/VPProcess/Aviation/forms-and-resources/OBN-2017-02-Electronic-Archiving-of-Incident-Information.pdf",
            //    "/VPProcess/Doctrine/Field-Guide/FG-3.05---SES-VFES-Support-at-Bushfires.pdf"
            //};
            //List<SyncObject> arrObjects = objSPIntegrator2.GetObjects(arrIDs);
            //Console.WriteLine(JsonConvert.SerializeObject(arrObjects));
            //arrObjects = null;
            //objSPIntegrator2 = null;

            //* Get Object By ID...
            //SyncObject objObject = objSPIntegrator.GetObject("/VPProcess/Aviation/forms-and-resources/OBN-2017-02-Electronic-Archiving-of-Incident-Information.pdf");
            //Console.WriteLine(JsonConvert.SerializeObject(objObject));
            //objObject = null;

            objSPIntegrator = null;
        }
        #endregion
        #endregion
    }
}