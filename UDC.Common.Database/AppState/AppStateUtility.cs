using System;
using System.Collections.Generic;
using System.Linq;

using UDC.Common.Data;
using UDC.Common.Database.Data;
using UDC.Common.Database.Data.Models.Database;

namespace UDC.Common.Database.AppState
{
    public class AppStateUtility
    {
        public static Boolean GetMetaDataOnlyOverride(Int64 ruleID, DatabaseContext objDB)
        {
            Boolean retVal = false;

            
            ApplicationState objState = objDB.ApplicationStates.Where(obj => obj.Key.ToLower() == ("metadataonlyoverride_" + ruleID.ToString()).ToLower()).OrderByDescending(obj => obj.LastUpdated).FirstOrDefault();

            if (objState != null)
            {
                retVal = GeneralHelpers.parseBool(objState.Value);
            }

       

            return retVal;
        }
        public static void SetMetaDataOnlyOverride(Int64 ruleID, Boolean value)
        {
            ApplicationState objState = null;

            using (DatabaseContext objDB = new DatabaseContext())
            {
                DateTime dtUtcNow = DateTime.UtcNow;

                objState = objDB.ApplicationStates.Where(obj => obj.Key.ToLower() == ("metadataonlyoverride_" + ruleID.ToString()).ToLower()).OrderByDescending(obj => obj.LastUpdated).FirstOrDefault();
                if (objState == null)
                {
                    objState = new ApplicationState();
                    objState.Key = "metadataonlyoverride_" + ruleID.ToString();
                    objState.DateCreated = dtUtcNow;
                    objDB.ApplicationStates.Add(objState);
                }
                objState.LastUpdated = dtUtcNow;
                objState.Value = value.ToString();

                objDB.SaveChanges();
            }

            objState = null;
        }
        public static void DeleteMetaDataOnlyOverride(Int64 ruleID)
        {
            using (DatabaseContext objDB = new DatabaseContext())
            {
                List<ApplicationState> arrStates = objDB.ApplicationStates.Where(obj => obj.Key.ToLower() == ("metadataonlyoverride_" + ruleID.ToString()).ToLower()).ToList();
                if (arrStates != null)
                {
                    objDB.ApplicationStates.RemoveRange(arrStates);
                    objDB.SaveChanges();
                }
                arrStates = null;
            }
        }
    }
}