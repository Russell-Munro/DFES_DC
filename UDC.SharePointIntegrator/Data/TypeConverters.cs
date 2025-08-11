using System;
using System.Collections.Generic;

using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

using UDC.Common;
using UDC.Common.Data.Models;

namespace UDC.SharePointIntegrator.Data
{
    public static class TypeConverters
    {
        #region Platform Native Type Converters...
        public static void ConvertList(List src, ref Dictionary<String, Object> dest)
        {
            List<Dictionary<String, Object>> arrDestFlds = null;

            dest.Add("Id", src.Id);
            dest.Add("Title", src.Title);

            if (src.Fields != null)
            {
                arrDestFlds = new List<Dictionary<String, Object>>();
                foreach (Field objSrcFld in src.Fields)
                {
                    Dictionary<String, Object> objDestFld = new Dictionary<String, Object>();
                    ConvertField(objSrcFld, ref objDestFld);
                    arrDestFlds.Add(objDestFld);
                }
            }
            dest.Add("Fields", arrDestFlds);
        }
        public static void ConvertField(Field src, ref Dictionary<String, Object> dest)
        {
            dest.Add("Id", src.Id);
            dest.Add("InternalName", src.InternalName);
            dest.Add("Title", src.Title);
            dest.Add("FieldTypeKind", src.FieldTypeKind);
            dest.Add("FieldTypeKindStr", src.FieldTypeKind.ToString());
            dest.Add("TypeAsString", src.TypeAsString);
            dest.Add("ClrType", src.GetType().ToString());
            dest.Add("TermSetId", null);

            if (src.TypeAsString == "TaxonomyFieldType" || src.TypeAsString == "TaxonomyFieldTypeMulti")
            {
                TaxonomyField objTaxonomyField = src.Context.CastTo<TaxonomyField>(src);
                
                src.Context.Load(objTaxonomyField, obj => obj.TermSetId);
                src.Context.ExecuteQueryAsync().Wait();
                dest["TermSetId"] = objTaxonomyField.TermSetId.ToString();

                objTaxonomyField = null;
            }
        }
        public static void ConvertTerm(Term src, ref Dictionary<String, Object> dest)
        {
            dest.Add("Id", src.Id);
            dest.Add("parentId", null);
            dest.Add("Name", GeneralHelpers.getRenderableString(src.Name));
            dest.Add("Description", GeneralHelpers.getRenderableString(src.Description));
            dest.Add("Terms", null);
        }
        public static void ConvertFolder(Folder src, String parentFolderRelativeUrl, ref Dictionary<String, Object> dest)
        {
            dest.Add("Id", src.ServerRelativeUrl);
            dest.Add("ParentId", parentFolderRelativeUrl);
            dest.Add("Title", src.Name);
            //dest.Add("LastModified", src.TimeLastModified); //Doesn't Exist in SP Instance...
        }
        public static void ConvertFile(File src, String parentFolderRelativeUrl, ref Dictionary<String, Object> dest)
        {
            dest.Add("Id", src.ServerRelativeUrl);
            dest.Add("FolderId", parentFolderRelativeUrl);
            dest.Add("Title", src.Title);
            dest.Add("Name", src.Name);
            dest.Add("Extension", System.IO.Path.GetExtension(src.Name));
            dest.Add("TotalSize", src.Length);
            dest.Add("Uploaded", src.Exists);
            dest.Add("LastModified", src.TimeLastModified);
            dest.Add("DateCreated", src.TimeCreated);
        }
        #endregion

        #region Abstract Type Converters...
        public static void ConvertSyncContainer(Dictionary<String, Object> src, ref SyncContainer dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.Name = GeneralHelpers.parseString(src["Title"]);
            if (src.ContainsKey("ParentId"))
            {
                dest.parentId = GeneralHelpers.parseString(src["ParentId"]);
            }
        }
        public static void ConvertSyncContainer(SyncContainer src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "ParentId", src.parentId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", src.Name);
        }

        public static void ConvertSyncObject(Dictionary<String, Object> src, ref SyncObject dest, List<String> fields)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.containerId = GeneralHelpers.parseString(src["FolderId"]);
            dest.Title = GeneralHelpers.parseString(src["Title"]);
            dest.Name = GeneralHelpers.parseString(src["Name"]);

            dest.FileName = GeneralHelpers.parseString(src["Name"]);
            dest.SizeBytes = GeneralHelpers.parseInt64(src["TotalSize"]);
            dest.DateCreated = GeneralHelpers.parseDate(src["DateCreated"]);
            dest.LastUpdated = GeneralHelpers.parseDate(src["LastModified"]);

            if(src.ContainsKey("FileData"))
            {
                dest.BinaryPayload = (Byte[])src["FileData"];
            }
            if(fields != null)
            {
                dest.Properties = new Dictionary<String, Object>();
                foreach(String fldKey in fields)
                {
                    if(!dest.Properties.ContainsKey(fldKey)) //Multiple mappings could cause duplicates...
                    {
                        if (src.ContainsKey(fldKey) && src[fldKey] != null)
                        {
                            if (src[fldKey] is Dictionary<String, Object>)
                            {
                                dest.Properties.Add(fldKey, ((Dictionary<String, Object>)src[fldKey])["Value"]);
                            }
                            else
                            {
                                dest.Properties.Add(fldKey, src[fldKey]);
                            }

                        }
                        else
                        {
                            dest.Properties.Add(fldKey, null);
                        }
                    }
                }
            }
        }
        public static void ConvertSyncObject(SyncObject src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "FolderId", src.containerId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", src.Title);
            GeneralHelpers.addUpdateDictionary(ref dest, "Name", src.Name);

            GeneralHelpers.addUpdateDictionary(ref dest, "Name", src.FileName);
            GeneralHelpers.addUpdateDictionary(ref dest, "TotalSize", src.SizeBytes);
            GeneralHelpers.addUpdateDictionary(ref dest, "DateCreated", src.DateCreated);
            GeneralHelpers.addUpdateDictionary(ref dest, "LastModified", src.LastUpdated);

            GeneralHelpers.addUpdateDictionary(ref dest, "FileData", src.BinaryPayload);

            /////////////////////
            //src.Properties <--Would need to complete this if we were writing back to SharePoint... Later... ToDo:
            /////////////////////
        }

        public static void ConvertSyncTag(Dictionary<String, Object> src, ref SyncTag dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            if(src.ContainsKey("parentId"))
            {
                dest.parentId = GeneralHelpers.parseString(src["parentId"]);
            }
            dest.Name = GeneralHelpers.parseString(src["Name"]);
        }
        public static void ConvertSyncTag(SyncTag src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "parentId", src.parentId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Name", src.Name);
        }

        public static void ConvertSyncField(Dictionary<String, Object> src, ref SyncField dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.Key = GeneralHelpers.parseString(src["InternalName"]);
            dest.Title = GeneralHelpers.parseString(src["Title"]);
            dest.NativeType = GeneralHelpers.parseString(src["TypeAsString"]);
            dest.LinkedLookupId = GeneralHelpers.parseString(src["TermSetId"]);

            switch (dest.NativeType)
            {
                case "TaxonomyFieldTypeMulti":
                case "TaxonomyFieldType":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.Taxonomy;
                    break;
                case "ContentTypeId":
                case "Note":
                case "Text":
                case "Lookup":
                case "Choice":
                case "URL":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.String;
                    break;
                case "Boolean":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.Boolean;
                    break;
                case "DateTime":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.DateTime;
                    break;
                case "Counter":
                case "TypeAsString":
                case "User":
                case "Computed":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.String;
                    break;
                case "Number":
                case "Integer":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.Integer;
                    break;
                case "Guid":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.Guid;
                    break;
                default:
                    dest.FieldDataType = Common.Constants.FieldDataTypes.String;
                    break;
            }
            if(!String.IsNullOrEmpty(dest.LinkedLookupId))
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.Taxonomy;
            }
        }
        public static void ConvertSyncField(SyncField src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "InternalName", src.Key);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", src.Title);
            GeneralHelpers.addUpdateDictionary(ref dest, "TypeAsString", src.NativeType);
            GeneralHelpers.addUpdateDictionary(ref dest, "TermSetId", src.LinkedLookupId);
        }
        #endregion
    }
}