using System;

namespace UDC.SitefinityContextModels.Documents
{
    public class Document
    {
        public Guid Id { get; set; }
        public Guid? FolderId { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public String FileName { get; set; }
        public String FileExtension { get; set; }
        public Int64 Size { get; set; }
        public Int32 Status { get; set; }
        public DateTime LastModified { get; set; }

        public Document() { }
    }
}