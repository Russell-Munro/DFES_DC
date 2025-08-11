using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UDC.Common.Database.Data.Models.Database
{
    [Table("equ_dc_DataConnectorLog")]
    public class DataConnectorLog
    {
        [Key]
        [Column("dataConnectorLogID")]
        public Int64 Id { get; set; }
        public Int64? connectionRuleID { get; set; }

        public Int32 LogType { get; set; }
        public Int32 Action { get; set; }
        public Int32 Result { get; set; }

        public String Source { get; set; }
        public String Message { get; set; }
        public String Data { get; set; }

        public DateTime? DateCreated { get; set; }

        public ConnectionRule ConnectionRule { get; set; }
    }
}