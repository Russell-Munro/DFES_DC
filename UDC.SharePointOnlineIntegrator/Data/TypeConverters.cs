using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Graph;

using UDC.Common;
using UDC.Common.Data.Models;

namespace UDC.SharePointOnlineIntegrator.Data
{
    public static class TypeConverters
    {
        #region Platform Native Type Converters...
        public static void ConvertList(Microsoft.Graph.List src, ref Dictionary<String, Object> dest)
        {
            List<Dictionary<String, Object>> arrDestFlds = null;

            dest.Add("Id", src.Id);
            dest.Add("Title", src.DisplayName);

            if (src.Columns != null)
            {
                arrDestFlds = new List<Dictionary<String, Object>>();
                foreach (ColumnDefinition objSrcFld in src.Columns)
                {
                    Dictionary<String, Object> objDestFld = new Dictionary<String, Object>();
                    ConvertField(objSrcFld, ref objDestFld);
                    arrDestFlds.Add(objDestFld);
                }
            }
            dest.Add("Fields", arrDestFlds);
        }

        public static void ConvertField(ColumnDefinition src, ref Dictionary<String, Object> dest)
        {
            dest.Add("Id", src.Id);
            dest.Add("InternalName", src.Name);
            dest.Add("Title", src.DisplayName);
            dest.Add("Type", GetColumnType(src));
            dest.Add("ClrType", src.GetType().ToString());
            dest.Add("TermSetId", null);
        }

        private static string GetColumnType(ColumnDefinition column)
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

        public static void ConvertFolder(DriveItem src, ref Dictionary<String, Object> dest)
        {
            dest.Add("Id", src.Id);
            dest.Add("ParentId", src.ParentReference != null ? src.ParentReference.Id : null);
            dest.Add("Title", src.Name);
            dest.Add("LastModified", src.LastModifiedDateTime);
        }

        public static void ConvertFile(DriveItem src, ref Dictionary<String, Object> dest)
        {
            dest.Add("Id", src.Id);
            dest.Add("FolderId", src.ParentReference != null ? src.ParentReference.Id : null);
            dest.Add("Title", src.Name);
            dest.Add("Name", src.Name);
            dest.Add("Extension", Path.GetExtension(src.Name));
            dest.Add("TotalSize", src.Size);
            dest.Add("Uploaded", src.File != null);
            dest.Add("LastModified", src.LastModifiedDateTime);
            dest.Add("DateCreated", src.CreatedDateTime);
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

            if (fields != null)
            {
                dest.Properties = new Dictionary<String, Object>();
                foreach (String fldKey in fields)
                {
                    if (!dest.Properties.ContainsKey(fldKey))
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

            // Binary content intentionally omitted
        }

        public static void ConvertSyncField(Dictionary<String, Object> src, ref SyncField dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.Key = GeneralHelpers.parseString(src["InternalName"]);
            dest.Title = GeneralHelpers.parseString(src["Title"]);
            dest.NativeType = GeneralHelpers.parseString(src["Type"]);
            dest.LinkedLookupId = GeneralHelpers.parseString(src["TermSetId"]);

            switch (dest.NativeType)
            {
                case "Boolean":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.Boolean;
                    break;
                case "DateTime":
                    dest.FieldDataType = Common.Constants.FieldDataTypes.DateTime;
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
            if (!String.IsNullOrEmpty(dest.LinkedLookupId))
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.Taxonomy;
            }
        }
        public static void ConvertSyncField(SyncField src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "InternalName", src.Key);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", src.Title);
            GeneralHelpers.addUpdateDictionary(ref dest, "Type", src.NativeType);
            GeneralHelpers.addUpdateDictionary(ref dest, "TermSetId", src.LinkedLookupId);
        }
        #endregion
    }
}

