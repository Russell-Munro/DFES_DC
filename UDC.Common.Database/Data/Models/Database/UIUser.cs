using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UDC.Common.Database.Data.Models.Database
{
    [Table("equ_dc_UIUser")]
    public class UIUser
    {
        [Key]
        [Column("uiUserID")]
        public Int64 Id { get; set; }
        public String sessionID { get; set; }
        public String remoteUserId { get; set; }
        public String Username { get; set; }
        public String Email { get; set; }
        public Int32 TZOffsetMins { get; set; }

        public Boolean IsConnectionAdmin { get; set; }
        public Boolean IsSyncManager { get; set; }

        public String ReferringApplication { get; set; }
        public String ReferringApplicationURL { get; set; }

        public DateTime LastAccessed { get; set; }
    }
}