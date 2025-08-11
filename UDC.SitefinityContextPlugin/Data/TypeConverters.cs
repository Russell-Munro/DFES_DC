using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Model;

using UDC.Common;

namespace UDC.SitefinityContextPlugin.Data
{
    public static class TypeConverters
    {
        #region Platform Native Type Converters...
        public static String ConvertLString(Lstring src)
        {
            return (src != null ? src.Value : "");
        }
        public static Lstring ConvertLString(String src)
        {
            return new Lstring(src);
        }
        public static String ConvertUrlSafe(String src)
        {
            return Regex.Replace(src.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
        }

        public static void ConvertDocumentLibrary(Telerik.Sitefinity.IFolder src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "ParentId", src.ParentId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", ConvertLString(src.Title));
            GeneralHelpers.addUpdateDictionary(ref dest, "Description", ConvertLString(src.Description));
            GeneralHelpers.addUpdateDictionary(ref dest, "LastModified", src.LastModified);
        }
        public static void ConvertDocument(Telerik.Sitefinity.Libraries.Model.Document src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "FolderId", src.FolderId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", ConvertLString(src.Title));
            GeneralHelpers.addUpdateDictionary(ref dest, "Description", ConvertLString(src.Description));
            GeneralHelpers.addUpdateDictionary(ref dest, "FileName", ConvertLString(src.Title));
            GeneralHelpers.addUpdateDictionary(ref dest, "FileExtension", src.Extension);
            GeneralHelpers.addUpdateDictionary(ref dest, "Size", src.TotalSize);
            GeneralHelpers.addUpdateDictionary(ref dest, "Status", (Int32)src.Status);
            GeneralHelpers.addUpdateDictionary(ref dest, "Uploaded", src.Uploaded);
            GeneralHelpers.addUpdateDictionary(ref dest, "DateCreated", src.DateCreated);
            GeneralHelpers.addUpdateDictionary(ref dest, "LastModified", src.LastModified);
        }
        public static void ConvertMetaField(MetaField src, ref Dictionary<String, Object> dest)
        {
            //objFld.Id
            //objFld.FieldName
            //objFld.Title
            //objFld.Description

            //objFld.ClrType
            //objFld.ColumnName
            //objFld.DefaultValue

            //objFld.Required
            //objFld.IsSingleTaxon
            //objFld.TaxonomyId
            //objFld.TaxonomyProvider

            dest.Add("Id", src.Id);
            dest.Add("FieldName", src.FieldName);
            dest.Add("Title", src.Title);
            dest.Add("Description", src.Description);
            dest.Add("ClrType", src.ClrType);
            dest.Add("Required", src.Required);
            dest.Add("TaxonomyId", src.TaxonomyId);
            dest.Add("IsSingleTaxon", src.IsSingleTaxon);
        }
        public static void ConvertTaxonomy(Telerik.Sitefinity.Taxonomies.Model.HierarchicalTaxonomy src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "Name", src.Name);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", ConvertLString(src.Title));
            GeneralHelpers.addUpdateDictionary(ref dest, "Description", ConvertLString(src.Description));
        }
        public static void ConvertTaxon(Telerik.Sitefinity.Taxonomies.Model.Taxon src, ref Dictionary<String, Object> dest)
        {
            GeneralHelpers.addUpdateDictionary(ref dest, "Id", src.Id);
            GeneralHelpers.addUpdateDictionary(ref dest, "ParentId", src.ParentId);
            GeneralHelpers.addUpdateDictionary(ref dest, "Name", src.Name);
            GeneralHelpers.addUpdateDictionary(ref dest, "Title", ConvertLString(src.Title));
            GeneralHelpers.addUpdateDictionary(ref dest, "Description", ConvertLString(src.Description));
            GeneralHelpers.addUpdateDictionary(ref dest, "UrlName", ConvertLString(src.UrlName));
            GeneralHelpers.addUpdateDictionary(ref dest, "Status", (Int32)src.Status);
        }
        #endregion
    }
}