using System;

namespace UDC.SitefinityContextModels.Documents
{
    public class DocumentLibrary
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public Int64 TotalSize { get; set; }
        public Int32 Status { get; set; }
        public DateTime LastModified { get; set; }

        public DocumentLibrary() { }
    }
}