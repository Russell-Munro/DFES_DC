using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UDC.Common.Database.Data.Models.Database
{
    [Table("equ_dc_ApplicationState")]
    public class ApplicationState
    {
        [Key]
        [Column("applicationStateID")]
        public Int64 Id { get; set; }
        public String Key { get; set; }
		public String Value { get; set; }
        public Byte[] ValueBinary { get; set; }
		public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}