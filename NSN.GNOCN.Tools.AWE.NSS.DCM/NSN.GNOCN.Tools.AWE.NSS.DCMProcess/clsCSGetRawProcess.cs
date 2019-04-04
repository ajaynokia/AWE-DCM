using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using NSN.GNOCN.Tools.Logs;

namespace NSN.GNOCN.Tools.AWE.NSS
{

    public class clsCSGetRawProcess
    {
        private string Router_IP = string.Empty;
        private string Router_ID = string.Empty;
        private int Router_Port = 0;
        private string Router_UID = string.Empty;
        private string Router_PWD = string.Empty;
        private int Router_TimeOut = 0;

        public string ErrorMsg = string.Empty;
        public string FileError = string.Empty;
        public string Errordir = string.Empty; // @"D:/CS_DCM/DCM_RAW/" + System.DateTime.Now.ToString("dd_MM_yyyy") + @"/ErrorLogs/";
        public MySqlConnection conn; // new MySqlConnection("datasource=93.183.30.156;database=awe_dcm;user id=root;password=tiger");
        public string logData = string.Empty;
        public ScriptingSSH.ScriptingSSH TelnetSession = null;

        public string groupName = string.Empty; // "group1";  // increase at 26 02 2014
        public string circleName = string.Empty;
        public string vendor = string.Empty;
        public string folderPath = string.Empty;
        public string errorFolderPath = string.Empty;
        private clsExceptionLogs oExceptionLog;
        private clsFileLogs oFileLog;

        public string[] nodeTypeMain;
        public string ErrorQuery = string.Empty;
        public int retryRouter = 0;
        public void StartRawGeneration()
        {
            oExceptionLog = new clsExceptionLogs("Bharti_DCM_Raw");
            oFileLog = new clsFileLogs("Bharti_DCM_Raw_FileLogs");

            ErrorMsg = "";
            string updateQuery = string.Empty;
            string masterDb = Convert.ToString(ConfigurationSettings.AppSettings["XMLFile"]);
            DataSet ds = new DataSet();
            ds.ReadXml(masterDb);

            if (ds.Tables.Count > 0)
            {
                conn = new MySqlConnection(Convert.ToString(ds.Tables[0].Rows[0]["connectionString"]));
                circleName = Convert.ToString(ds.Tables[0].Rows[0]["circle"]);
                vendor = Convert.ToString(ds.Tables[0].Rows[0]["vendor"]);
                folderPath = Convert.ToString(ds.Tables[0].Rows[0]["folderPath"]);
                Errordir = errorFolderPath = Convert.ToString(ds.Tables[0].Rows[0]["errorFolderPath"]);

                DataTable dt = new DataTable();
                DataTable dtcheck = new DataTable();

                string query = "Update raw_generation_details set `STATUS` = 'NOT COMPLETED', `START_TIME`='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', `END_TIME` = '0000-00-00 00:00:00' where circle = '" + circleName + "_" + groupName + "'";
                ExecuteSQLQuery(ref conn, query);

                if (groupName.Contains("HLR_MSC"))
                {
                    query = "Update ne_master set Ischecked='0' where circle = '" + circleName + "' and domain = 'CS' and `GROUP` = '" + groupName + "' ";
                }
                else
                {
                    query = "Update ne_master set Ischecked='0' where circle = '" + circleName + "' and domain = 'CS' and `GROUP` = '" + (groupName.Insert(groupName.Length - 1, "_")) + "' ";
                }

                ExecuteSQLQuery(ref conn, query);

                query = "SELECT * FROM router_master where  vendor = 'nsn' and  circle = '" + circleName + "' and domain ='CS' and Rtr_ActiveFlag is true";        // and Rtr_Name = 'mah_group1 RTR5'";
                SQLQuery(ref conn, ref dt, query);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        for (int k = 0; k < dt.Rows.Count; k++)
                        {
                            Router_IP = dt.Rows[k]["Rtr_IP"].ToString();
                            Router_ID = dt.Rows[k]["Router_id"].ToString();
                            Router_Port = 22;
                            Router_UID = dt.Rows[k]["Rtr_UserName"].ToString();
                            Router_PWD = dt.Rows[k]["Rtr_Password"].ToString();
                            Router_TimeOut = 350;
                            TelnetSession = null;
                            TelnetSession = new ScriptingSSH.ScriptingSSH(Router_IP, Router_Port, Router_UID, Router_PWD, Router_Port);
                            if (groupName.Contains("HLR_MSC"))
                            {
                                query = "SELECT * FROM ne_master where  circle = '" + circleName + "' and domain = 'CS' and `GROUP` = '" + groupName + "' and Ischecked is false and isEnable=1";        // and Rtr_Name = 'mah_group1 RTR5'";
                            }
                            else
                            {
                                query = "SELECT * FROM ne_master where  circle = '" + circleName + "' and domain = 'CS' and `GROUP` = '" + (groupName.Insert(groupName.Length - 1, "_")) + "' and Ischecked is false and isEnable=1";        // and Rtr_Name = 'mah_group1 RTR5'";
                            }
                            SQLQuery(ref conn, ref dtcheck, query);
                            if (dtcheck != null)
                            {
                                if (dtcheck.Rows.Count > 0)
                                {
                                    try
                                    {

                                    retry:

                                        retryRouter = retryRouter + 1;

                                        if (TelnetSession.Connect() == true)
                                        {
                                            string[] node_type = nodeTypeMain;
                                            if (node_type != null)
                                            {
                                                for (int i = 0; i < node_type.Length; i++)
                                                {
                                                    if (TelnetSession.IsConnected() == false)
                                                    {
                                                        TelnetSession = null;
                                                        TelnetSession = new ScriptingSSH.ScriptingSSH(Router_IP, Router_Port, Router_UID, Router_PWD, 300);
                                                        TelnetSession.Connect();
                                                    }
                                                    string nodeType = node_type[i].Trim().ToString();
                                                    EnterElementTOCollectDATA(ref TelnetSession, dt.Rows[k]["Circle"].ToString(), nodeType, Router_ID);
                                                }


                                                ////Comment for Testing
                                                //updateQuery = "update raw_generation_details set `STATUS` = 'COMPLETED', `END_TIME`='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where circle = '" + circleName + "_" + groupName + "'";
                                                //ExecuteSQLQuery(ref conn, updateQuery);

                                                //ExecuteSQLQuery(ref conn, "Update router_master Set ERRORMSG='',REMARKS='OK',STATUS='' Where Circle='" + circleName + "'");

                                                //updateQuery = "update parsing_completion_details set `STATUS` = 'NOT COMPLETED', `START_TIME`='0000-00-00 00:00:00',`END_TIME`='0000-00-00 00:00:00' where circle = '" + circleName + "_" + groupName + "'";
                                                //ExecuteSQLQuery(ref conn, updateQuery);
                                            }

                                            TelnetSession.Disconnect();
                                            ExecuteSQLQuery(ref conn, "Update router_master Set ERRORMSG='',REMARKS='OK',STATUS='Success' Where Circle='" + circleName + "' and Router_id='" + Router_ID + "'");
                                        }

                                        else
                                        {
                                            if (retryRouter == 3)
                                            {

                                                ErrorMsg = "Retry " + retryRouter + " times.Could not connect to Router of " + circleName + " at " + System.DateTime.Now.ToString() + ".";
                                                oExceptionLog.WriteExceptionErrorToFile("StartRawGeneration()", ErrorMsg, "", ref FileError);

                                                updateQuery = "update raw_generation_details set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "_" + groupName + "'";
                                                ExecuteSQLQuery(ref conn, updateQuery);

                                                ExecuteSQLQuery(ref conn, "Update router_master Set STATUS='Login Failed',ERRORMSG='" + ErrorMsg.Replace("'","") + "',REMARKS='NOT OK' Where Circle='" + circleName + "'");
                                                retryRouter = 0;

                                            }
                                            else
                                            {
                                                goto retry;
                                            }

                                            //if (!Directory.Exists(Errordir))
                                            //{
                                            //    Directory.CreateDirectory(Errordir);
                                            //}
                                            //File.AppendAllText(Errordir + "\\" + circleName + "_" + groupName + "_error_log.txt", "fail to enter " + circleName + " router");
                                        }


                                    }

                                    catch (Exception e)
                                    {
                                        //if (!Directory.Exists(Errordir))
                                        //{
                                        //    Directory.CreateDirectory(Errordir);
                                        //}
                                        //File.AppendAllText(Errordir, e.ToString());
                                        TelnetSession.Disconnect();
                                        ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                                        oExceptionLog.WriteExceptionErrorToFile("StartRawGeneration()", ErrorMsg, "", ref FileError);
                                        ExecuteSQLQuery(ref conn, "Update router_master Set STATUS='Login Failed',ERRORMSG='" + ErrorMsg.Replace("'","") + "',REMARKS='NOT OK' Where Circle='" + circleName + "' and Router_id='" + Router_ID + "'");

                                    }
                                    //------------------------------------------------------------
                                }
                            }
                        }
                        //Comment for Testing
                        updateQuery = "update raw_generation_details set `STATUS` = 'COMPLETED', `END_TIME`='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where circle = '" + circleName + "_" + groupName + "'";
                        ExecuteSQLQuery(ref conn, updateQuery);

                        updateQuery = "update parsing_completion_details set `STATUS` = 'NOT COMPLETED', `START_TIME`='0000-00-00 00:00:00',`END_TIME`='0000-00-00 00:00:00' where circle = '" + circleName + "_" + groupName + "'";
                        ExecuteSQLQuery(ref conn, updateQuery);

                    }
                }
            }
        }

        private string GRP_Name = string.Empty;
        public void EnterElementTOCollectDATA(ref ScriptingSSH.ScriptingSSH SessionName, string circle, string type, string Router_Id)
        {
            try
            {
                string element_name = string.Empty;
                string NEErrorLog = string.Empty;
                string group_name = (type != "HLR_MSC" ? groupName.Insert(groupName.Length - 1, "_") : type);
                if (group_name.Contains("HLR") || group_name.Contains("MSC"))
                {
                    group_name = "HLR_MSC";
                }
                GRP_Name = group_name;
                DataTable dt1 = new DataTable();
                string query1 = "SELECT NE_NAME, NE_IP, NE_UserName, NE_Password, `group` , atca_flag, DelayTime,Protocol FROM ne_master where circle = '" + circle + "' and domain = 'CS' and node_type = '" + type + "' and `GROUP` = '" + group_name + "' and Ischecked is false and isEnable is true;";

                //string query1 = "SELECT NE_NAME, NE_IP, NE_UserName, NE_Password, `group` , atca_flag, DelayTime,Router_id FROM ne_master where circle = '" + circle + "' and domain = 'CS' and node_type = '" + type + "' and `GROUP` = '" + group_name + "' and Router_Id='" + Router_Id + "'";

                SQLQuery(ref conn, ref dt1, query1);


                foreach (DataRow dr in dt1.Rows)
                {
                    //group_name = dr["group"].ToString();
                    try
                    {
                        element_name = dr["NE_NAME"].ToString();
                        ExecuteSQLQuery(ref conn, "Update ne_master Set STATUS='',ERRORMSG='',Remarks='' Where Circle='" + circle + "' and NE_NAME='" + element_name + "' ");

                        NEErrorLog = SessionName.SessionLog.ToString();
                       // oExceptionLog.WriteExceptionErrorToFile("Testing_Error", NEErrorLog, "", ref FileError);
                        Logs("Processing " + element_name + "[" + type + "]" + "[" + dr["NE_IP"].ToString() + "]...");
                        if (NEErrorLog.Contains("ERROR"))
                        {
                            TelnetSession.Disconnect();
                            TelnetSession = null;
                            TelnetSession = new ScriptingSSH.ScriptingSSH(Router_IP, Router_Port, Router_UID, Router_PWD, 300);
                            TelnetSession.Connect();
                        }
                        if (TelnetSession.IsConnected() == false)
                        {
                            TelnetSession.Disconnect();
                            TelnetSession = null;
                            TelnetSession = new ScriptingSSH.ScriptingSSH(Router_IP, Router_Port, Router_UID, Router_PWD, 300);
                            TelnetSession.Connect();
                        }
                        if (Convert.ToBoolean(dr["atca_flag"]) == true)
                        {
                            TelnetSession.Disconnect();
                            TelnetSession = null;
                            TelnetSession = new ScriptingSSH.ScriptingSSH(Router_IP, Router_Port, Router_UID, Router_PWD, 300);
                            TelnetSession.Connect();
                        }

                        SessionName.timeout = 500;
                        SessionName.ClearSessionLog();
                        SessionName.LOG = "";
                        NEErrorLog = "";
                        ErrorMsg = "";

                        try
                        {
                            //SessionName.SendAndWait("", "<|>|#|$", "|", false);

                            if (Convert.ToInt32(dr["DelayTime"].ToString()) != 0)
                            {
                                SessionName.timeout = Convert.ToInt32(dr["DelayTime"].ToString());
                            }

                            if (!dr["Protocol"].ToString().Contains("SSH"))
                            {
                                SessionName.SendAndWait(dr["NE_IP"].ToString(), "<|>|#|$", "|", false);
                            }
                            else
                            {
                                
                                SessionName.SendAndWait("ssh -l " + dr["NE_UserName"].ToString() + " " + dr["NE_IP"].ToString(), "<|>|#|$|Password:", "|", false);
                               
                            }

                            Logs("Firing command for "+ element_name + "[" + type + "]" + " -> ssh -l " + dr["NE_UserName"].ToString() + " " + dr["NE_IP"].ToString());
                            //SessionName.SendAndWait(dr["NE_IP"].ToString(), "<|>|#|$|~|--More--|:", "|", false);
                            //if (!dr["NE_IP"].ToString().Contains("ssh")) // Check for atca flag when we put ssh -l username NE_IP in database (field ne_ip for table ne_master) then we put this condtion else we run it normal.
                            //{
                            //    SessionName.SendAndWait(dr["NE_UserName"].ToString(), "<|>|#|$|~|--More--|:", "|", false);
                            //}
                            if (!dr["Protocol"].ToString().Contains("SSH"))
                            {
                                SessionName.SendAndWait(dr["NE_UserName"].ToString(), "<|>|#|$", "|", false);
                            }
                            
                            SessionName.SendAndWait(dr["NE_Password"].ToString(), "<|>|#|$", "|", false);

                            NEErrorLog = SessionName.SessionLog.ToString();
                            Logs("command output for " + element_name + "[" + type + "]-->" + NEErrorLog);
                            if (NEErrorLog.ToUpper().Contains("DELAY"))
                            {
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + "DELAY in NE";

                            }
                            else if (NEErrorLog.ToUpper().Contains("timeout expired"))
                            {

                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + "timeout expired!";
                            }
                            else if (NEErrorLog.ToUpper().Contains("USER AUTHORIZATION FAILURE"))
                            {

                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + "USER AUTHORIZATION FAILURE";
                            }
                            else if (NEErrorLog.ToUpper().Contains("USER ACCOUNT IS DISABLED"))
                            {

                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + "USER ACCOUNT IS DISABLED";
                            }
                            else if (NEErrorLog.ToUpper().Contains("Unknown command"))
                            {

                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + "% Unknown command or computer name, or unable to find computer address";
                            }
                            //else if (NEErrorLog.ToUpper().Contains("ENTER PASSWORD"))
                            //{

                            //    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + "User name or password is not correct.";
                            //}
                            else
                            {
                                NEErrorLog = "";
                                ErrorMsg = "";
                                string cmnd_to_fire = string.Empty;
                                cmnd_to_fire = "set cli built-in rows -1";
                                SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);                                    
                                Logs("Login Success for "+ element_name + "[" + type + "]");
                            }

                            //oExceptionLog.WriteExceptionErrorToFile("Testing_Error", SessionName.SessionLog.ToString(), "", ref FileError);
                            if (NEErrorLog != "")
                            {
                                Logs("Login Fail for "+ element_name + "[" + type + "]" + ".Error:" + NEErrorLog);
                                ExecuteSQLQuery(ref conn, "Update dcm_node_failure_details1 Set ErrorMsg='" + ErrorMsg.Replace("'","") + "' Where  ELEMENT='" + element_name + "' and Circle='" + circle + "'");
                                ExecuteSQLQuery(ref conn, "insert into dcm_node_failure_details (ELEMENT,CIRCLE,Circle_Group,NODE_TYPE,Time_Stamp,ErrorMsg) VALUES ('" + element_name + "','" + circle + "','" + group_name + "','" + type + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + ErrorMsg.Replace("'","") + "')");
                                ExecuteSQLQuery(ref conn, "Update ne_master Set STATUS='Login Failed',ERRORMSG='" + ErrorMsg.Replace("'","") + "',Remarks='NOT OK' Where Circle='" + circle + "' and NE_NAME='" + element_name + "' ");
                                SessionName.Disconnect();
                                SessionName.Connect();
                                continue;
                            }

                            ExecuteSQLQuery(ref conn, "Update ne_master Set STATUS='Success',ERRORMSG='',Remarks='OK' Where Circle='" + circle + "' and NE_NAME='" + element_name + "' ");
                            ExecuteSQLQuery(ref conn, "Update ne_master set Ischecked='1' where circle = '" + circle + "' and domain = 'CS' and node_type = '" + type + "' and `GROUP` = '" + group_name + "' and ne_name='" + element_name + "'");
                        }
                        catch (Exception ex)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString();
                            Logs("Login Fail for "+ element_name + "[" + type + "]" + ".Error:" + ErrorMsg);
                            oExceptionLog.WriteExceptionErrorToFile("EnterElementTOCollectDATA_Sub()", ErrorMsg, "", ref FileError);
                            //string loginfailurePath = @errorFolderPath + "DCM_RAW_nodeLoginFailure/" + System.DateTime.Now.ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + element_name + ".txt";
                            //Directory.CreateDirectory(loginfailurePath);
                            ExecuteSQLQuery(ref conn, "insert into dcm_node_failure_details (ELEMENT,CIRCLE,Circle_Group,NODE_TYPE,Time_Stamp,ErrorMsg) VALUES ('" + element_name + "','" + circle + "','" + group_name + "','" + type + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + ErrorMsg.Replace("'","") + "')");
                            ExecuteSQLQuery(ref conn, "Update dcm_node_failure_details1 Set ErrorMsg='" + ErrorMsg.Replace("'","") + "' Where  ELEMENT='" + element_name + "' and Circle='" + circle + "'");
                            ExecuteSQLQuery(ref conn, "Update ne_master Set STATUS='Login Failed',ERRORMSG='" + ErrorMsg.Replace("'","") + "' Where Circle='" + circle + "' and NE_NAME='" + element_name + "' ");
                            SessionName.Disconnect();
                            SessionName.Connect();
                            continue;
                        }

                        if (Convert.ToBoolean(dr["atca_flag"]) == false)
                        {
                            Logs("Getting command for "+ element_name + "[" + type + "]");
                            InElementCollectECDATA(ref SessionName, circle, element_name, type, group_name); // If atca flag is false means current selected node is not a atca node.
                        }
                        else
                        {
                            Logs("Getting command for "+ element_name + "[" + type + "]");
                            InElementCollectECDATA(ref SessionName, circle, element_name, type, group_name, Convert.ToBoolean(dr["atca_flag"])); // If atca flag is true means current selected node is not a atca node.
                        }

                        // Below lines are used for disconnecting current node and condtion based on ATCA and NON-ATCA. For ATCA 'exit'  and for Non-ATCA 'ZZZZ;' command is used. 
                        //int prompt = SessionName.SendAndWait("ZZZZ;", "<|>|#|$|~|--More--|:", "|", false);
                        int prompt = Convert.ToBoolean(dr["atca_flag"]) == false ? SessionName.SendAndWait("ZZZZ;", "<|>|#|$|~|--More--|:", "|", false) : SessionName.SendAndWait("exit", "<|>|#|$|~|--More--|:", "|", false);
                        if (prompt == 1)
                            prompt = Convert.ToBoolean(dr["atca_flag"]) == false ? SessionName.SendAndWait("ZZZZ;", "<|>|#|$|~|--More--|:", "|", false) : SessionName.SendAndWait("exit", "<|>|#|$|~|--More--|:", "|", false);
                        // -----------END-----------

                    }

                    catch (Exception e)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("EnterElementTOCollectDATA_subwithcontinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        if (TelnetSession.IsConnected() == false)
                        {
                            TelnetSession.Connect();
                        }
                        continue;

                    }

                }

            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("EnterElementTOCollectDATA()", ErrorMsg, "", ref FileError);
            }

        }
        private void Logs(string Data)
        {
            try
            {
                string Path = @"" + System.Configuration.ConfigurationManager.AppSettings["ErrorLogPath"].ToString() + System.DateTime.Now.ToString("yyyy-MM-dd");
                if (!System.IO.Directory.Exists(Path))
                {
                    System.IO.Directory.CreateDirectory(Path);
                }

                System.IO.StreamWriter s = new System.IO.StreamWriter(Path + "\\DCM_LOG_" + GRP_Name + ".log", true);
                s.Write("\r\n" + System.DateTime.Now.ToString("HH:mm") + ": " + Data);
                s.Close();
                s.Dispose();
            }
            catch
            {

            }
        }
        public void InElementCollectECDATA(ref ScriptingSSH.ScriptingSSH SessionName, string circle, string element, string type, string group_name)
        {
            try
            {
                if (type == "GCS")
                {
                    type = "MSS";
                }
                string query = "  SELECT distinct commands  FROM dcm_command_info where node_type = '" + type + "' and ISEnable=1";
                DataTable newdt = new DataTable();
                string command = string.Empty;
                SQLQuery(ref conn, ref newdt, query);
                string OUTPUT = string.Empty;
                SessionName.ClearSessionLog();
                SessionName.LOG = "";

                string cmnd_to_fire = string.Empty;

                foreach (DataRow dr1 in newdt.Rows)
                {
                    command = "";
                    command = dr1[0].ToString().Trim();
                    Logs("Firing command " + command +" in " + element + "[" + type + "]");
                    //if (command.Contains("ZNSI"))
                    //{
                    //    command = dr1[0].ToString().Trim();
                    //}

                    // first we have to fire zusi and then check that which command should be fired either zusi:iw1se or zusi:iwsep OR we have to fire both
                    #region FOR ZUSI COMMAND
                    if (command.Contains("ZUSI"))
                    {
                        cmnd_to_fire = "set cli built-in rows -1";
                        SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                        cmnd_to_fire = "";

                        cmnd_to_fire = command.Trim();
                        SessionName.ClearSessionLog();
                        SessionName.LOG = "";
                        OUTPUT = "";
                        // command execution
                        SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                        System.Threading.Thread.Sleep(3000);
                        OUTPUT = SessionName.SessionLog.ToString();

                        #region FOR ZUSI:IWSEP
                        if (OUTPUT.Contains("IWSEP"))
                        {
                            cmnd_to_fire = "set cli built-in rows -1";
                            SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            cmnd_to_fire = "";
                            logData = "";
                            command = "ZUSI_IWSEP";

                            string LOGDir = string.Empty;
                            if (group_name.ToUpper().Contains("HLR"))
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            }

                            else
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                            }

                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }

                            cmnd_to_fire = "ZUSI:IWSEP::FULL;";
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";

                            // command execution
                            try
                            {
                                SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            }

                            catch (Exception e)
                            {
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZUSI_IWSEPwithcontinue()", ErrorMsg, "", ref FileError);

                                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);
                                continue;
                            }

                            System.Threading.Thread.Sleep(3000);
                            logData = SessionName.SessionLog.ToString();

                            save_log(LOGDir, logData, element);
                        }

                        #endregion

                        #region FOR ZUSI:Iws1E
                        if (OUTPUT.Contains("IWS1E"))
                        {
                            cmnd_to_fire = "set cli built-in rows -1";
                            SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            cmnd_to_fire = "";
                            logData = "";
                            command = "ZUSI_IWS1E";
                            string LOGDir = string.Empty;
                            if (group_name.ToUpper().Contains("HLR"))
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            }

                            else
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                            }

                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }

                            cmnd_to_fire = "ZUSI:IWS1E::FULL;";
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";

                            // command execution
                            try
                            {

                                SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            }

                            catch (Exception e)
                            {
                                //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                                //if (!Directory.Exists(commandfilurepath))
                                //{
                                //    Directory.CreateDirectory(commandfilurepath);
                                //}
                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZUSI_IWS1Ewithcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                                continue;
                            }

                            System.Threading.Thread.Sleep(3000);
                            logData = SessionName.SessionLog.ToString();


                            save_log(LOGDir, logData, element);

                        }
                        #endregion

                        #region FOR ZUSI:NIWU
                        if (OUTPUT.Contains("NIWU"))
                        {
                            cmnd_to_fire = "set cli built-in rows -1";
                            SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            cmnd_to_fire = "";
                            logData = "";
                            command = "ZUSI_NIWU";
                            string LOGDir = string.Empty;
                            if (group_name.ToUpper().Contains("HLR"))
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            }

                            else
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                            }
                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }

                            cmnd_to_fire = "ZUSI:NIWU::FULL;";
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";

                            // command execution
                            try
                            {

                                SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            }

                            catch (Exception e)
                            {
                                //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                                //if (!Directory.Exists(commandfilurepath))
                                //{
                                //    Directory.CreateDirectory(commandfilurepath);
                                //}
                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZUSI_NIWUwithcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                                continue;
                            }

                            System.Threading.Thread.Sleep(3000);
                            logData = SessionName.SessionLog.ToString();


                            save_log(LOGDir, logData, element);

                        }
                        #endregion

                    }

                    #endregion

                    // FIRST WE HAVE TO EXECUTE "ZUSI:" SO AS TO KNOW EXACT NO. OF CGR's THE NODE IS HAVING 

                    #region FOR ZRCI COMMAND
                    else if (command.Contains("ZRCI"))
                    {                        
                        cmnd_to_fire = "set cli built-in rows -1";
                        SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                        cmnd_to_fire = "";
                        cmnd_to_fire = command.Trim();
                        SessionName.ClearSessionLog();
                        SessionName.LOG = "";
                        OUTPUT = "";
                        string cgr_max_value = "";
                        string[] temp;
                        // command execution
                        SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                        System.Threading.Thread.Sleep(3000);
                        OUTPUT = SessionName.SessionLog.ToString();
                        temp = OUTPUT.Split(new[] { "COMMAND EXECUTED" }, StringSplitOptions.RemoveEmptyEntries);
                        OUTPUT = temp[0].Trim().ToString();
                        temp = null;
                        temp = OUTPUT.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        OUTPUT = temp[temp.Length - 1].Trim();
                        temp = null;

                        temp = OUTPUT.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        OUTPUT = temp[0].Trim();
                        cgr_max_value = OUTPUT;

                        #region FOR MSS                        
                        if (type.ToLower() == "gcs" || type.ToLower() == "mss")
                        {
                            if (type == "gcs")
                            {
                                type = "mss";  // must be "mss" only 
                            }
                            logData = "";
                            command = "ZRCI";
                            string LOGDir = string.Empty;
                            if (group_name.ToUpper().Contains("HLR"))
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            }

                            else
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                            }

                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }

                            cmnd_to_fire = "ZRCI:SEA=3:CGR=1&&" + cgr_max_value + ":PRINT=2;";

                            //cmnd_to_fire = "ZRCI:SEA=3:CGR=1&&" + cgr_max_value + ":PRINT=5:;";
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";

                            // command execution
                            try
                            {
                                //SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                                SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~", "|", false);
                            }

                            catch (Exception e)
                            {
                                //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                                //if (!Directory.Exists(commandfilurepath))
                                //{
                                //    Directory.CreateDirectory(commandfilurepath);
                                //}
                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());

                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZRCIwithcontinue()", ErrorMsg, "", ref FileError);

                                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                continue;
                            }

                            System.Threading.Thread.Sleep(3000);
                            logData = SessionName.SessionLog.ToString();

                            save_log(LOGDir, logData, element);


                        }
                        #endregion

                        else if (type.ToLower() == "mgw")
                        {
                            #region FOR MGW
                            logData = "";
                            command = "ZRCI";

                            string LOGDir = string.Empty;
                            if (group_name.ToUpper().Contains("HLR"))
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            }

                            else
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                            }

                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }

                            cmnd_to_fire="ZRCI:SEA=3:CGR=1&&" + cgr_max_value + ":PRINT=2;";
                            //cmnd_to_fire = "ZRCI:SEA=3:CGR=1&&" + cgr_max_value + ":PRINT=4:;";
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";

                            // command execution
                            try
                            {
                                SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            }

                            catch (Exception e)
                            {
                                //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                                //if (!Directory.Exists(commandfilurepath))
                                //{
                                //    Directory.CreateDirectory(commandfilurepath);
                                //}
                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());

                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZRCIPRINT4withcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                                continue;
                            }

                            System.Threading.Thread.Sleep(3000);
                            logData = SessionName.SessionLog.ToString();

                            save_log(LOGDir, logData, element);



                            #endregion

                        }


                        else if (type.ToLower() == "msc")
                        {
                            #region FOR MSC

                            logData = "";
                            command = "ZRCI";

                            string LOGDir = string.Empty;
                            if (group_name.ToUpper().Contains("HLR"))
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            }

                            else
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                            }
                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }

                            cmnd_to_fire = "ZRCI:SEA=3:CGR=1&&" + cgr_max_value + ":PRINT=5:;";
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";

                            // command execution
                            try
                            {
                                SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            }

                            catch (Exception e)
                            {
                                //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                                //if (!Directory.Exists(commandfilurepath))
                                //{
                                //    Directory.CreateDirectory(commandfilurepath);
                                //}
                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZRCIPRINT5withcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                                continue;
                            }

                            System.Threading.Thread.Sleep(3000);
                            logData = SessionName.SessionLog.ToString();

                            save_log(LOGDir, logData, element);



                            #endregion

                        }

                    }
                    #endregion

                    #region FOR ZNSI:NA0/NA1 COMMAND

                    else if (command == "ZNSI")
                    {
                        cmnd_to_fire = "set cli built-in rows -1";
                        SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                        cmnd_to_fire = "";
                        cmnd_to_fire = command.Trim();
                        SessionName.ClearSessionLog();
                        SessionName.LOG = "";
                        OUTPUT = "";
                        int r = 0;
                        try
                        {

                            r = SessionName.SendAndWait(cmnd_to_fire + "\r", "<|>|#|$|~|:|...", "|", true);

                            if (r == 5)
                            {
                                SessionName.SendAndWait("\n", "<|>|#|$|~|:|...", "|", false);
                            }

                            OUTPUT = SessionName.SessionLog.ToString();
                            SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);  // to interupt

                            SessionName.ClearSessionLog();
                        }
                        catch (Exception e)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZRCIPRINT5withcontinue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);

                            SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                        }
                        #region to check if element can execute both ZNSI:NA0 AND ZNSI:NA1
                        if (OUTPUT.Contains("NA0") && OUTPUT.Contains("NA1"))
                        {
                            for (int i = 0; i <= 1; i++)
                            {
                                if (i == 0)
                                {
                                    ZNSI_COMMAND_execution(circle, ref SessionName, "NA0", element, type, group_name);
                                }

                                else
                                {
                                    ZNSI_COMMAND_execution(circle, ref SessionName, "NA1", element, type, group_name);
                                }
                            }
                        }
                        #endregion

                        #region to check if element can execute ZNSI:NA0 AND not ZNSI:NA1

                        else if (OUTPUT.Contains("NA0") && !OUTPUT.Contains("NA1"))
                        {
                            try
                            {
                                ZNSI_COMMAND_execution(circle, ref SessionName, "NA0", element, type, group_name);
                            }

                            catch (Exception e)
                            {
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZNSINA0withcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                continue;
                            }


                        }

                        #endregion

                        #region to check if element can execute ZNSI:NA1 AND not ZNSI:NA0

                        else if (OUTPUT.Contains("NA1") && !OUTPUT.Contains("NA0"))
                        {
                            try
                            {
                                ZNSI_COMMAND_execution(circle, ref SessionName, "NA1", element, type, group_name);
                            }

                            catch (Exception e)
                            {
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZNSINA1withcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);
                                continue;
                            }


                        }
                        #endregion

                    }
                    #endregion

                    #region FOR ZNRI:NA0/NA1 COMMAND
        
                    else if (command == "ZNRI")
                    {
                        cmnd_to_fire = "set cli built-in rows -1";
                        SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                        cmnd_to_fire = "";
                        cmnd_to_fire = command.Trim();
                        SessionName.ClearSessionLog();
                        SessionName.LOG = "";
                        OUTPUT = "";
                        int r = 0;
                        try
                        {
                            r = SessionName.SendAndWait(cmnd_to_fire + "\r", "<|>|#|$|~|:|...", "|", true);
                            if (r == 5)
                            {
                                SessionName.SendAndWait("\n", "<|>|#|$|~|:||...", "|", false);
                            }

                            OUTPUT = SessionName.SessionLog.ToString();
                            SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~", "|", false);  // to interupt

                            SessionName.ClearSessionLog();
                        }
                        catch (Exception e)
                        {

                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + cmnd_to_fire + "] " + e.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZNRI()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);

                            SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                        }
                        #region to check if element can execute both ZNRI:NA0 AND ZNRI:NA1
                        if (OUTPUT.Contains("NA0") && OUTPUT.Contains("NA1"))
                        {
                            for (int i = 0; i <= 1; i++)
                            {
                                if (i == 0)
                                {
                                    ZNRI_COMMAND_execution(circle, ref SessionName, "NA0", element, type, group_name);
                                }

                                else
                                {
                                    ZNRI_COMMAND_execution(circle, ref SessionName, "NA1", element, type, group_name);
                                }
                            }
                        }
                        #endregion

                        #region to check if element can execute ZNRI:NA0 AND not ZNRI:NA1

                        else if (OUTPUT.Contains("NA0") && !OUTPUT.Contains("NA1"))
                        {
                            try
                            {
                                ZNRI_COMMAND_execution(circle, ref SessionName, "NA0", element, type, group_name);
                            }

                            catch (Exception e)
                            {
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZNRINA0withcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                continue;
                            }

                        }

                        #endregion

                        #region to check if element can execute ZNRI:NA1 AND not ZNRI:NA0

                        else if (OUTPUT.Contains("NA1") && !OUTPUT.Contains("NA0"))
                        {
                            try
                            {
                                ZNRI_COMMAND_execution(circle, ref SessionName, "NA1", element, type, group_name);
                            }

                            catch (Exception e)
                            {
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZNRINA1withcontinue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);

                                continue;
                            }


                        }

                    }

                    #endregion

                    #endregion

                    //else if (command.Contains("ZWTI:P;"))
                    //{                        
                                               
                    //    command = "ZWTI";

                    //    string LOGDir = string.Empty;
                    //    if (group_name.ToUpper().Contains("HLR"))
                    //    {
                    //        LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                    //    }

                    //    else
                    //    {
                    //        LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                    //    }
                    //    Logs("Directory for command " + command + "-->" + LOGDir);
                    //    if (!Directory.Exists(LOGDir))
                    //    {
                    //        Directory.CreateDirectory(LOGDir);
                    //    }

                    //    //cmnd_to_fire = "ZWTI:P;";
                    //    SessionName.ClearSessionLog();
                    //    SessionName.LOG = "";
                    //    logData = "";
                    //    cmnd_to_fire = "";
                    //    string comnd = dr1[0].ToString().Trim();

                    //    Logs("Firing command " + comnd + " in " + element + "[" + type + "]");
                      
                    //    // command execution
                    //    try
                    //    {
                    //        int prompt = SessionName.SendAndWait(comnd, "<|>|#|$|~|--More--", "|", false);
                    //    }

                    //    catch (Exception e)
                    //    {
                    //        ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + comnd + "] " + e.Message.ToString();
                    //        oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ZWTIPwithcontinue()", ErrorMsg, "", ref FileError);

                    //        SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                    //        ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + comnd + "] " + e.Message.ToString();
                    //        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                    //        ExecuteSQLQuery(ref conn, ErrorQuery);
                    //        continue;
                    //    }

                    //    System.Threading.Thread.Sleep(3000);
                    //    logData = SessionName.SessionLog.ToString();
                    //    Logs("LOG of " + comnd + " in " + element + "[" + type + "]-->" + "\r\n" + logData);
                    //    save_log(LOGDir, logData, element);

                    //}

                    #region FOR OTHER COMMANDS
                    else
                    {
                        try
                        {
                            if (command.Contains("ZWTI"))
                            {
                                Logs("Firing command ZWTI:P; " + command + " in " + element + "[" + type + "]");
                                command = "ZWTI";

                            }
                            else if(command.Contains("ZJVI"))
                            {
                                command = "ZJVI";
                            }
                            else if (command.Contains("ZW7N"))
                            {
                                command = "ZW7N";
                            }

                            else if (command.Contains("ZR2O"))
                            {
                                command = "ZR2O";
                            }


                            else if (command.Contains("ZNEL"))
                            {
                                command = "ZNEL";
                            }


                            else if (command.Contains("ZOYI"))
                            {
                                command = "ZOYI";

                            }

                            else if (command.Contains("ZQRI:OMU"))
                            {
                                command = "ZQRI";

                            }
                            else if (command.Contains("ZEDO"))
                            {
                                command = "ZEDO";

                            }
                            else if (command.Contains("ZRRI:GSW"))
                            {
                                command = "ZRRI";

                            }
                            else if (command.Contains("ZRNI:SPR"))
                            {
                                command = "ZRNI";

                            }
                            else if (command.Contains("ZRIL:NDEST"))
                            {
                                command = "ZRIL";

                            }
                            else if (command.Contains("ZWVI"))
                            {
                                command = "ZWVI";

                            }
                            else if (command.Contains("ZQNI"))
                            {
                                command = "ZQNI";

                            }
                            else if (command.Contains("ZOYI"))
                            {
                                command = "ZOYI";

                            }
                            else if (command.Contains("ZOYI"))
                            {
                                command = "ZOYI";

                            }

                            else if (command.Contains("ZJGI:MODE=1:MSCID=0&&99:;"))
                            {
                                command = "ZJGI";
                            }

                            else if (command.Contains("ZJGI:MODE=1:MGWID=0&&99:;"))
                            {
                                command = "ZJGI";
                            }
                            //code added by rahul on 14-8-2018
                            else if (command.Contains("ZJNI"))
                            {
                                command = "ZJNI";
                            }
                            //else if (command.Contains("ZWTI"))
                            //{
                            //    Logs("Firing command ZWTI:P; " + command + " in " + element + "[" + type + "]");
                            //    command = "ZWTI";

                            //}

                            string LOGDir = string.Empty;
                            if (group_name.ToUpper().Contains("HLR"))
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            }

                            else
                            {
                                LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                            }
                            // for mss and mgw
                            // string LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;

                            // for hlr and msc
                            //  string LOGDir = "E:/CS_DCM/DCM_RAW/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                            Logs("Directory for command " + command + "-->"+ LOGDir);
                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";
                            logData = "";
                            //cmnd_to_fire = "set cli built-in rows -1";
                            //SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            //cmnd_to_fire = "";
                            string comnd = dr1[0].ToString().Trim();

                            Logs("Firing command " + comnd + " in " + element + "[" + type + "]");

                            //int prompt = SessionName.SendAndWait(comnd, "<|>|#|$|~|--More--", "|", false);
                            int prompt = SessionName.SendAndWait(comnd, "<|>|#|$|~|--More--", "|", false);
                            System.Threading.Thread.Sleep(3000);

                            logData = SessionName.SessionLog.ToString();
                            Logs("LOG of " + comnd + " in " + element + "[" + type + "]-->" + "\r\n" + logData);

                            save_log(LOGDir, logData, element);

                        }

                        catch (Exception e)
                        {
                            //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                            //if (!Directory.Exists(commandfilurepath))
                            //{
                            //    Directory.CreateDirectory(commandfilurepath);
                            //}
                            //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                            //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_subwithcontinue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                           //cmnd_to_fire = "set cli built-in rows -1";
                            //SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            //cmnd_to_fire = "";
                            //SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~", "|", false);

                            continue;
                        }


                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_ATCA()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        public void InElementCollectECDATA(ref ScriptingSSH.ScriptingSSH SessionName, string circle, string element, string type, string group_name, bool isAtca)
        {
            try
            {

                if (type == "mgw".ToUpper())
                {
                    type = "ATCA";
                }
                else if (type == "GCS")
                {
                    type = "MSS";
                }

                // for testing by arv  
                //string query = "  SELECT distinct commands  FROM dcm_command_info where node_type = '" + type + "' and commands='show signaling ss7 route all'";
                string query = "  SELECT distinct commands  FROM dcm_command_info where node_type = '" + type + "' and ISEnable=1";
                DataTable newdt = new DataTable();
                string command = string.Empty;
                SQLQuery(ref conn, ref newdt, query);
                string OUTPUT = string.Empty;
                SessionName.ClearSessionLog();
                SessionName.LOG = "";

                string cmnd_to_fire = string.Empty;

                foreach (DataRow dr1 in newdt.Rows)
                {
                    command = "";
                    command = dr1[0].ToString().Trim();

                    #region FOR SHOW_TDM_CIRCUITGROUP ALL / SHOW CIRCUITGROUP CRCT CGR CGR_NUM < CIRCUITGROUP_NUMBER >

                    if (command.Contains("tdm circuitgroup"))
                    {
                        try
                        {
                            string storeFolderName = group_name + "/TDM_CIRCUITGROUP";
                            TDM_CIRCUITGROUP_Execution(circle, ref SessionName, command, storeFolderName, element, type);
                        }
                        catch (Exception ex)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ":[" + command + "] " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_WithContinue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            continue;
                        }
                    }
                    #endregion

                    #region FOR OTHER COMMANDS
                    else
                    {
                        try
                        {

                            // for ATCA Commands 

                            if (command.Contains("vmgw mgw"))
                            {
                                command = "VMGW_MGW";    // show vmgw mgw mod 0
                            }                           
                            else if (command.Contains("tdm ater"))
                            {
                                command = "TDM_ATER";    // show tdm ater
                            }
                            else if (command.Contains("license target-id"))
                            {
                                command = "LICENSE_TARGET";    // show license target-id
                            }
                            else if (command.Contains("signaling ss7 own-point-code"))
                            {
                                command = "SIGNALING_SS7_OWN";    // show signaling ss7 own-point-code all
                            }

                            else if (command.Contains("functional-unit unit-info show-mode"))
                            {
                                command = "TDMMGU";    // show functional-unit unit-info show-mode verbose unit-type TDMMGU
                            }

                            else if (command.Contains("signaling ss7 link all"))
                            {
                                command = "SIGNALING_SS7_LINK";    // show signaling ss7 link it work for linkset command also
                            }
                            //code added by Rahul on 07-08-2018
                            else if (command.Contains("signaling ss7 linkset all"))
                            {
                                command = "SIGNALING_SS7_LINKSET";
                            }

                            else if (command.Contains("signaling ss7 route"))
                            {
                                command = "SIGNALING_SS7_ROUTE";    // show signaling ss7 route it work for routeset command also show signaling ss7 route all
                            }
                                //code added by Rahul on 03-08-2018
                            else if (command.Contains("show signaling sccp subsystem"))
                            {
                                command = "SIGNALING_SCCP_SUBSYS_ALL";
                            }
                            else if (command.Contains("show tdm circuitgroup all"))
                            {
                                command = "SHOW_TDM_CIRCUITGROUP_ALL";
                            }

                            string storeFolderName = group_name + "/" + command;
                            string LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/ATCA" + "/" + (type == "ATCA" ? "MGW_ATCA" : type) + "/" + storeFolderName;
                            if (!Directory.Exists(LOGDir))
                            {
                                Directory.CreateDirectory(LOGDir);
                            }
                            SessionName.ClearSessionLog();
                            SessionName.LOG = "";
                            logData = "";
                            //cmnd_to_fire = "set cli built-in rows -1";
                            //SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            //cmnd_to_fire = "";

                            string comnd = dr1[0].ToString().Trim(); // command

                            // old : int prompt_no = SessionName.SendAndWait(comnd, "<|>|#|$|~|--More--|:|--", "|", false);  // command executes here

                            if (command == "SIGNALING_SS7_LINK")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "SIGNALING_SS7_LINKSET")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "SHOW_TDM_CIRCUITGROUP_ALL")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "VMGW_MGW")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "TDM_ATER")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "LICENSE_TARGET")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "SIGNALING_SS7_OWN")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "SIGNALING_SS7_ROUTE")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "SIGNALING_SCCP_SUBSYS_ALL")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }
                            else if (command == "TDMMGU")
                            {
                                SessionName.SendAndWait("fsclish", "<|> |#|$|~|--More--", "|", false);  // command executes here
                            }                           
                            cmnd_to_fire = "set cli built-in rows -1";
                            SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            cmnd_to_fire = "";
                            int prompt_no = SessionName.SendAndWait(comnd, "<|> |#|$|~|--More--", "|", false);  // command executes here

                            while (prompt_no == 5 || prompt_no == 6)
                            {
                                //cmnd_to_fire = "set cli built-in rows -1";
                                //SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                                //cmnd_to_fire = "";
                                prompt_no = SessionName.SendAndWait("\r\n", "<|>|#|$|~|--More--", "|", false);
                            }

                            System.Threading.Thread.Sleep(3000);
                            logData = SessionName.SessionLog.ToString();

                            save_log(LOGDir, logData, element);

                        }

                        catch (Exception e)
                        {
                            //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                            //if (!Directory.Exists(commandfilurepath))
                            //{
                            //    Directory.CreateDirectory(commandfilurepath);
                            //}
                            //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                            //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());

                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_sub()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            cmnd_to_fire = "set cli built-in rows -1";
                            SessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                            cmnd_to_fire = "";
                            SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                            continue;
                        }


                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        public void ZNSI_COMMAND_execution(string circle, ref ScriptingSSH.ScriptingSSH sessionName, string state, string element, string type, string group_name)
        {

            try
            {
                string cmnd_fire = string.Empty;
                cmnd_fire = "set cli built-in rows -1";
                sessionName.SendAndWait(cmnd_fire, "<|>|#|$|~|--More--", "|", false);
                cmnd_fire = "";
                string command = "ZNSI_" + state.Trim();
                string LOGDir = string.Empty;
                if (group_name.ToUpper().Contains("HLR"))
                {
                    LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                }

                else
                {
                    LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                }
                if (!Directory.Exists(LOGDir))
                {
                    Directory.CreateDirectory(LOGDir);
                }

                sessionName.ClearSessionLog();
                sessionName.LOG = "";                
                string cmnd_to_fire = "ZNSI:" + state.Trim() + ":;"; ;


                // command execution
                try
                {

                    sessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--|:", "|", false);
                }

                catch (Exception e)
                {
                    //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                    //if (!Directory.Exists(commandfilurepath))
                    //{
                    //    Directory.CreateDirectory(commandfilurepath);
                    //}
                    //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                    //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ZNSI_COMMAND_execution_sub()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);

                    sessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);


                }

                System.Threading.Thread.Sleep(3000);
                logData = sessionName.SessionLog.ToString();

                save_log(LOGDir, logData, element);
            }
            catch (Exception e)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ZNSI_COMMAND_execution()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }


        }

        public void ZNRI_COMMAND_execution(string circle, ref ScriptingSSH.ScriptingSSH sessionName, string state, string element, string type, string group_name)
        {
            try
            {
                string cmnd_fire = string.Empty;
                cmnd_fire = "set cli built-in rows -1";
                sessionName.SendAndWait(cmnd_fire, "<|>|#|$|~|--More--", "|", false);
                cmnd_fire = "";
                string command = "ZNRI_" + state.Trim();
                string LOGDir = string.Empty;
                if (group_name.ToUpper().Contains("HLR"))
                {
                    LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + command;
                }

                else
                {
                    LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/" + type + "/" + group_name + "/" + command;
                }
                if (!Directory.Exists(LOGDir))
                {
                    Directory.CreateDirectory(LOGDir);
                }

                sessionName.ClearSessionLog();
                sessionName.LOG = "";
                string cmnd_to_fire = "ZNRI:" + state.Trim() + ":;"; ;


                // command execution
                try
                {
                    sessionName.SendAndWait(cmnd_to_fire, "<|>|#|$|~|--More--", "|", false);
                }

                catch (Exception e)
                {
                    //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                    //if (!Directory.Exists(commandfilurepath))
                    //{
                    //    Directory.CreateDirectory(commandfilurepath);
                    //}
                    //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");

                    //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ZNRI_COMMAND_execution_Sub()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);

                    sessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);

                }

                System.Threading.Thread.Sleep(3000);
                logData = sessionName.SessionLog.ToString();

                save_log(LOGDir, logData, element);
            }
            catch (Exception e)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ZNRI_COMMAND_execution()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        public void save_log(string logpath, string data, string element_name)
        {
            try
            {
            executeagain: if (data.Contains("MSC OBSERVATION REPORT"))
                {
                    string find = "END OF REPORT";
                    int find_length = find.Length;
                    int index = data.IndexOf("MSC OBSERVATION REPORT");
                    int ind = data.IndexOf("END OF REPORT");

                    if (index > ind)
                        data = data.Remove(ind, find_length).Insert(ind, " ");

                    else
                        data = data.Remove(index, ind - index);


                    if (data.Contains("MSC OBSERVATION REPORT"))
                    {
                        goto executeagain;
                    }

                }

                if (!data.Contains("MSC OBSERVATION REPORT"))
                {

                    data = data.Replace("END OF REPORT", "");

                    //File.WriteAllText(logpath + "/" + element_name + ".txt", data);
                    oFileLog.WriteLogDataToFile(logpath + "/" + element_name + ".txt", data);
                }
            }
            catch (Exception e)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("save_log()", ErrorMsg, "", ref FileError);

            }



        }

        public bool SQLQuery(ref MySqlConnection MYSQL_connection, ref DataTable DT, string querystr)
        {
            int retrycount = 0;
            DT = new DataTable();
            MySqlDataAdapter ADAPTCombo = new MySqlDataAdapter();
            MySqlCommand mMySqlCommand = new MySqlCommand();

            mMySqlCommand.CommandText = querystr;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = MYSQL_connection;
            mMySqlCommand.CommandTimeout = 1000 * 60 * 15;

            DT.Clear();


            if (MYSQL_connection.State != ConnectionState.Open)
            {
                MYSQL_connection.Open();

            }

            //if (MYSQL_connection.State != ConnectionState.Open)
            //{
            //    return false;
            //}
            ADAPTCombo.SelectCommand = mMySqlCommand;

        retry:
            try
            {

                if (retrycount <= 4)
                {
                    ADAPTCombo.Fill(DT);
                }
                return true;
            }
            catch (Exception ex)
            {
                retrycount = retrycount + 1;
                if (retrycount == 5)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("SQLQuery()", ErrorMsg, "", ref FileError);
                    return false;

                }
                goto retry;
            }
            finally
            {
                conn.Close();
            }


            //try
            //{
            //    ADAPTCombo.Fill(DT);
            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}

            //finally
            //{
            //    conn.Close();
            //}
        }

        public bool ExecuteSQLQuery(ref MySqlConnection MYSQL_connection, string querystr)
        {
            int retrycount = 0;
            MySqlCommand mMySqlCommand = new MySqlCommand();

            mMySqlCommand.CommandText = querystr;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = MYSQL_connection;
            mMySqlCommand.CommandTimeout = 1000 * 60 * 15;

        retry:
            if (MYSQL_connection.State != ConnectionState.Open)
            {
                conn.Open();
            }

            //if (MYSQL_connection.State != ConnectionState.Open)
            //{
            //    conn.Open();
            //}

            try
            {

                if (retrycount <= 4)
                {
                    mMySqlCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                retrycount = retrycount + 1;
                if (retrycount == 5)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ExecuteSQLQuery()", ErrorMsg, "", ref FileError);
                    return false;

                }
                goto retry;
            }
            finally
            {
                conn.Close();
            }

            //try
            //{
            //    mMySqlCommand.Connection = MYSQL_connection;
            //    mMySqlCommand.ExecuteNonQuery();
            //    MYSQL_connection.Close();
            //    conn.Close();
            //    return true;
            //}
            //catch (MySqlException ex)
            //{
            //    return false;
            //}
        }

        public void TDM_CIRCUITGROUP_Execution(string circle, ref ScriptingSSH.ScriptingSSH SessionName, string command, string folderName, string element, string type)
        {
            string cmnd_fire = string.Empty;
            cmnd_fire = "set cli built-in rows -1";
            SessionName.SendAndWait(cmnd_fire, "<|>|#|$|~|--More--", "|", false);
            cmnd_fire = "";
            // show tdm circuitgroup all
            string cmnd_to_fire = command;
            string OUTPUT = "";
            string LOGDir = @folderPath + "/" + System.DateTime.Now.AddDays(1).ToString("dd_MM_yyyy") + "/" + circle + "/ATCA" + "/" + (type == "ATCA" ? "MGW_ATCA" : type) + "/" + folderName;
            if (!Directory.Exists(LOGDir))
            {
                Directory.CreateDirectory(LOGDir);
            }
            // command execution
            try
            {
                SessionName.ClearSessionLog();
                SessionName.LOG = "";



                int r = SessionName.SendAndWait(cmnd_to_fire + "\r", "<|>|#|$|~|--More--", "|", true);
                while (r == 5 || r == 6)
                {
                    r = SessionName.SendAndWait("\n", "<|>|#|$|~|--More--", "|", true);
                }

                OUTPUT = SessionName.SessionLog.ToString();
                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);  // to interupt
                SessionName.ClearSessionLog();
                Dictionary<string, string> mgwCGRDictionary = new Dictionary<string, string>();  //  key = circuit group number, value = circuit group name
                if (OUTPUT != null)
                {
                    string[] temp = Regex.Split(OUTPUT.Trim(), "\r\n");
                    string MGW_CGR = "";
                    string MGW_NCGR = "";
                    string VMGW = "";
                    for (int i = 1; i < temp.Length; i++)
                    {
                        if (temp[i].Contains(':'))
                        {
                            string[] temp1 = Regex.Split(temp[i], ":");
                            if (temp1[0].Contains("circuit group name") || temp1[0].Contains("--More--circuit group name"))
                            {
                                MGW_NCGR = temp1[1];
                            }
                            if (temp1[0].Contains("circuit group number") || temp1[0].Contains("--More--circuit group number"))
                            {
                                MGW_CGR = temp1[1];
                            }
                            if (temp1[0].Contains("spe use") || temp1[0].Contains("--More--spe use"))
                            {
                                VMGW = temp1[1];
                            }
                            if (MGW_CGR != "" && MGW_NCGR != "" && VMGW != "")
                            {
                                if (VMGW.Trim() == "VMGW")
                                {
                                    mgwCGRDictionary.Add(MGW_CGR, MGW_NCGR);
                                }
                                MGW_CGR = MGW_NCGR = VMGW = "";
                            }
                        }
                    }
                }
                OUTPUT = "";
                foreach (var v in mgwCGRDictionary)
                {

                    OUTPUT = OUTPUT + "circuit group name  : " + Convert.ToString(v.Value) + "\r\n";
                    OUTPUT = OUTPUT + "circuit group number  : " + Convert.ToString(v.Key) + "\r\n";
                    cmnd_to_fire = "show tdm circuitgroup crct cgr cgr-num  " + Convert.ToString(v.Key);  // creating command for each circuit group number and get PCM_TSL
                    SessionName.ClearSessionLog();
                    SessionName.LOG = "";
                    r = SessionName.SendAndWait(cmnd_to_fire + "\r", "<|>|#|$|~|--More--", "|", true);
                    while (r == 6 || r == 5)
                    {
                        r = SessionName.SendAndWait("\n", "<|>|#|$|~|--More--", "|", true);
                    }
                    OUTPUT = OUTPUT + "\r\n";
                    OUTPUT = OUTPUT + SessionName.SessionLog.ToString();
                    SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~|--More--", "|", false);  // to interupt                    
                    OUTPUT = OUTPUT + "\r\n";
                }
            }

            catch (Exception e)
            {
                //string commandfilurepath = Errordir + @"\Command_failure\" + circle;
                //if (!Directory.Exists(commandfilurepath))
                //{
                //    Directory.CreateDirectory(commandfilurepath);
                //}
                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", command + "   " + element + "   " + type + " ...............................");
                //File.AppendAllText(commandfilurepath + @"\" + element + "_" + command + ".txt", e.ToString());

                ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("InElementCollectECDATA_subwithcontinue()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where circle = '" + circleName + "' and Element='" + element + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);

                SessionName.SendAndWait(char.ConvertFromUtf32(25), "<|>|#|$|~", "|", false);
            }
            System.Threading.Thread.Sleep(3000);
            save_log(LOGDir, OUTPUT, element);
        }
    }

}