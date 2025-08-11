using System;

namespace UDC.Common.Data.Models
{
    public class APIActionResult
    {
        public enum APIActions
        {
            NoAction = 0,
            Created = 1,
            Updated = 2,
            Deleted = 3,
        }

        public Object Id { get; set; }
        public Object DataObject { get; set; }
        public APIActions APIAction { get; set; }

        public APIActionResult() { }
        public APIActionResult(Guid id, APIActions apiAction)
        {
            this.Id = id;
            this.APIAction = apiAction;
        }
        public APIActionResult(Guid id, Object dataObject, APIActions apiAction)
        {
            this.Id = id;
            this.DataObject = dataObject;
            this.APIAction = apiAction;
        }
    }
}