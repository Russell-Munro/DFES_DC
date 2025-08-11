using System;
using System.Collections.Generic;

using UDC.Common;
using UDC.Common.Data.Models;

namespace UDC.SitefinityIntegrator.Data
{
    public class TypeConverters
    {
        #region Abstract Type Converters...
        public static void ConvertSyncContainer(Dictionary<String, Object> src, ref SyncContainer dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.parentId = GeneralHelpers.parseString(src["ParentId"]);
            dest.Name = GeneralHelpers.parseString(src["Title"]);
        }
        public static void ConvertSyncContainer(SyncContainer src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "ParentId", src.parentId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Name", src.Name);
        }

        public static void ConvertSyncObject(Dictionary<String, Object> src, ref SyncObject dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.containerId = GeneralHelpers.parseString(src["FolderId"]);
            dest.Name = GeneralHelpers.parseString(src["Title"]);

            dest.Title = GeneralHelpers.parseString(src["Title"]);
            dest.FileName = GeneralHelpers.parseString(src["FileName"]);

            dest.SizeBytes = GeneralHelpers.parseInt64(src["Size"]);

            dest.DateCreated = GeneralHelpers.parseDate(src["DateCreated"]);
            dest.LastUpdated = GeneralHelpers.parseDate(src["LastModified"]);
        }
        public static void ConvertSyncObject(SyncObject src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "FolderId", src.containerId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", src.Name);

            GeneralHelpers.addUpdateDictionary(ref dest, "Title", src.Title);
            GeneralHelpers.addUpdateDictionary(ref dest, "FileName", src.FileName);

            GeneralHelpers.addUpdateDictionary(ref dest, "Size", src.SizeBytes);

            GeneralHelpers.addUpdateDictionary(ref dest, "DateCreated", src.DateCreated);
            GeneralHelpers.addUpdateDictionary(ref dest, "LastModified", src.LastUpdated);
        }

        public static void ConvertSyncTag(Dictionary<String, Object> src, ref SyncTag dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.Name = GeneralHelpers.parseString(src["Name"]);

            if(src.ContainsKey("ParentId"))
            {
                dest.parentId = GeneralHelpers.parseString(src["ParentId"]);
            }
        }
        public static void ConvertSyncTag(SyncTag src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "ParentId", src.parentId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Name", src.Name);
        }

        public static void ConvertSyncField(Dictionary<String, Object> src, ref SyncField dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            dest.Key = GeneralHelpers.parseString(src["FieldName"]);
            dest.Title = GeneralHelpers.parseString(src["Title"]);
            dest.NativeType = GeneralHelpers.parseString(src["ClrType"]);
            dest.LinkedLookupId = GeneralHelpers.parseString(src["TaxonomyId"]);

            if (dest.NativeType.IndexOf("System.String") > -1 || dest.NativeType.IndexOf("Telerik.Sitefinity.Model.Lstring") > -1)
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.String;
            }
            else if (dest.NativeType.IndexOf("System.Guid") > -1)
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.Guid;
            }
            else if (dest.NativeType.IndexOf("System.Int32") > -1 || dest.NativeType.IndexOf("System.Int64") > -1)
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.Integer;
            }
            else if (dest.NativeType.IndexOf("System.DateTime") > -1)
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.DateTime;
            }
            else if(!String.IsNullOrEmpty(dest.LinkedLookupId) && GeneralHelpers.parseGUID(dest.LinkedLookupId) != Guid.Empty)
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.Taxonomy;
            }
            else
            {
                dest.FieldDataType = Common.Constants.FieldDataTypes.String;
            }
        }
        public static void ConvertSyncField(SyncField src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "FieldName", src.Key);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", src.Title);
            GeneralHelpers.addUpdateDictionary(ref dest, "ClrType", src.NativeType);
            GeneralHelpers.addUpdateDictionary(ref dest, "TaxonomyId", src.LinkedLookupId);
        }
        #endregion
    }
}