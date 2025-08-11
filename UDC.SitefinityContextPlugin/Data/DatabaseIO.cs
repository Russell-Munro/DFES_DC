using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

using UDC.SitefinityContextPlugin.Configuration;

namespace UDC.SitefinityContextPlugin.Data
{
    public class DatabaseIO
    {
        private String ConStrKey = "";

        public DatabaseIO()
        {

        }
        public DatabaseIO(String conStrKey)
        {
            this.ConStrKey = conStrKey;
        }
        public DatabaseIO(Boolean getConStrFromCfg)
        {
            if (getConStrFromCfg)
            {
                DataConnectorConfig objDataConnectorConfig = Telerik.Sitefinity.Configuration.Config.Get<DataConnectorConfig>();
                this.ConStrKey = objDataConnectorConfig.ConnectionStringKey.Value;
                objDataConnectorConfig = null;
            }
        }

        public void UpdateSQL(String SQL)
        {
            UpdateSQL(SQL, ConfigurationManager.ConnectionStrings[this.ConStrKey].ConnectionString);
        }
        public void UpdateSQL(String SQL, params Tuple<String, Object>[] Parameters)
        {
            UpdateSQL(SQL, ConfigurationManager.ConnectionStrings[this.ConStrKey].ConnectionString, Parameters);
        }
        public void UpdateSQL(String SQL, String ConStr, params Tuple<String, Object>[] Parameters)
        {
            if (SQL.Length > 0)
            {
                SqlConnection objConn = new SqlConnection(ConStr);
                SqlCommand objCommand = new SqlCommand(SQL, objConn);

                if (Parameters != null && Parameters.Length > 0)
                {
                    foreach (Tuple<String, Object> objParam in Parameters)
                    {
                        if (objParam.Item2 != null)
                        {
                            objCommand.Parameters.Add(new SqlParameter(objParam.Item1, objParam.Item2));
                        }
                        else
                        {
                            objCommand.Parameters.Add(new SqlParameter(objParam.Item1, DBNull.Value));
                        }
                    }
                }

                objCommand.CommandTimeout = 0;
                objConn.Open();
                objCommand.ExecuteNonQuery();
                objConn.Close();

                objCommand.Dispose();
                objConn.Dispose();

                objConn = null;
                objCommand = null;
            }
        }
        public List<Dictionary<String, Object>> GetAnonymousListOfDictionary(String SQL, String[] ColumnValues, params Tuple<String, Object>[] Parameters)
        {
            return GetAnonymousListOfDictionary(SQL, ColumnValues, ConfigurationManager.ConnectionStrings[this.ConStrKey].ConnectionString, Parameters);
        }
        public List<Dictionary<String, Object>> GetAnonymousListOfDictionary(String SQL, String[] ColumnValues, String ConStr, params Tuple<String, Object>[] Parameters)
        {
            List<Dictionary<String, Object>> arrObjects = new List<Dictionary<String, Object>>();
            if (SQL.Length > 0 && ColumnValues != null)
            {
                SqlConnection objConn = new SqlConnection(ConStr);
                SqlCommand objCommand = new SqlCommand(SQL, objConn);
                SqlDataReader objReader = null;

                if (ColumnValues != null && ColumnValues.Length > 0)
                {
                    String strCols = "";
                    foreach (String strColName in ColumnValues)
                    {
                        strCols += "[" + strColName + "],";
                    }
                    if (strCols.EndsWith(","))
                    {
                        strCols = strCols.Substring(0, strCols.Length - 1);
                    }
                    SQL = String.Format(SQL, strCols);
                }
                if (Parameters != null && Parameters.Length > 0)
                {
                    foreach (Tuple<String, Object> objParam in Parameters)
                    {
                        objCommand.Parameters.Add(new SqlParameter(objParam.Item1, objParam.Item2));
                    }
                }
                objCommand.CommandText = SQL;
                objCommand.CommandTimeout = 0;
                objConn.Open();
                objReader = objCommand.ExecuteReader();

                while (objReader.Read())
                {
                    Dictionary<String, Object> objObject = new Dictionary<String, Object>();
                    foreach (String strColName in ColumnValues)
                    {
                        String strDestColName = strColName;
                        if (strDestColName.IndexOf(".") > -1)
                        {
                            strDestColName = strColName.Split('.')[1];
                        }
                        objObject.Add(strDestColName, objReader[strColName]);
                    }
                    arrObjects.Add(objObject);
                }

                objConn.Close();
                objCommand.Dispose();
                objConn.Dispose();

                objReader = null;
                objCommand = null;
                objConn = null;
            }
            return arrObjects;
        }
    }
}