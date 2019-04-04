using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;
using NSN.GNOCN.Tools.Logs;

namespace NSN.GNOCN.Tools.AWE.NSS.DCMCSProcess
{
    public class clsBackupRestore
    {
        public string ErrorMsg = string.Empty;
        public string FileError = string.Empty;
        private clsExceptionLogs oExceptionLog;
        private clsFileLogs oFileLog;
        public MySqlConnection conn = new MySqlConnection(ConfigurationManager.AppSettings["CSDBConstring"].ToString()); // for Production
        // public MySqlConnection conn = new MySqlConnection("datasource=93.183.30.156;database=awe_dcm;user id=root;password=tiger;"); // For Test

        public clsBackupRestore()
        {
            oExceptionLog = new clsExceptionLogs("Bharti_DCM_BACKUP");
            oFileLog = new clsFileLogs("Bharti_DCM_BACKUP_FileLogs");
        }
        public void Manage_HLR_MSC_ASSO_DEST_Backup(string circleName)
        {
            try
            {
                List<MySqlParameter> pList = new List<MySqlParameter>();
                MySqlParameter p = new MySqlParameter();
                p.ParameterName = "@circle";
                p.Value = circleName;
                pList.Add(p);
                string procedureName = string.Empty;
                procedureName = "Manage_HLR_MSC_ASSO_DEST_Backup";
                if (ExecuteMySqlStoredProcedure(procedureName, pList) == false)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ":SP of Updation to Backup got failed.";
                    oExceptionLog.WriteExceptionErrorToFile("Manage_HLR_MSC_ASSO_DEST_Backup()", ErrorMsg, "", ref FileError);
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":" + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("Manage_HLR_MSC_ASSO_DEST_Backup()", ErrorMsg, "", ref FileError);
            }

        }


        public void Restore_HLR_MSC_ASSO_DEST_From_Backup(string circleName)
        {
            try
            {
                List<MySqlParameter> pList = new List<MySqlParameter>();
                MySqlParameter p = new MySqlParameter();
                p.ParameterName = "@circle";
                p.Value = circleName;
                pList.Add(p);
                string procedureName = string.Empty;
                procedureName = "Restore_HLR_MSC_ASSO_DEST_From_Backup";
                if (ExecuteMySqlStoredProcedure(procedureName, pList)==false)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ":SP of Restoration got failed.";
                    oExceptionLog.WriteExceptionErrorToFile("Restore_HLR_MSC_ASSO_DEST_From_Backup()", ErrorMsg, "", ref FileError);
                }

            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":" + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("Restore_HLR_MSC_ASSO_DEST_From_Backup()", ErrorMsg, "", ref FileError);
            }

        }

        public bool ExecuteMySqlStoredProcedure(string ProcedureName, List<MySqlParameter> pList)
        {
            int retrycount = 0;
            MySqlCommand mMySqlCommand = new MySqlCommand();
            mMySqlCommand.CommandText = ProcedureName;
            mMySqlCommand.CommandType = CommandType.StoredProcedure;

            bool result = false;

            foreach (MySqlParameter p in pList)
            {
                mMySqlCommand.Parameters.Add(p);
            }
        retry:
            mMySqlCommand.Connection = conn;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            try
            {

                if (retrycount <= 4)
                {
                    mMySqlCommand.ExecuteNonQuery();
                }
                conn.Close();
                result = true;
            }
            catch (MySqlException ex)
            {
                retrycount = retrycount + 1;
                if (retrycount == 5)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString() + ".SP:" + ProcedureName;
                    oExceptionLog.WriteExceptionErrorToFile("ExecuteSQLQueryBC()", ErrorMsg, "", ref FileError);
                    result = false;

                }
                goto retry;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return result;
        }


    }
}
