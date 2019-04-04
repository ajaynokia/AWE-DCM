using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NSN.GNOCN.Tools.AWE.NSS;
using System.Configuration;
using System.IO;
using MySql.Data.MySqlClient;
using System.Timers;
using NSN.GNOCN.Tools.AWE.NSS.DCMCSProcess;
using NSN.GNOCN.Tools.Logs;



namespace NSN.GNOCN.Tools.AWE.NSS.DCM
{
    public partial class AWE_NSS_DCM : ServiceBase
    {

        //System.Timers.Timer Paco_timer = new System.Timers.Timer();tmrhlrmsc

        Timer CS_timer = new System.Timers.Timer();
        Timer tmrGroup1 = new System.Timers.Timer();
        Timer tmrGroup2 = new System.Timers.Timer();
        Timer tmrGroup3 = new System.Timers.Timer();
        Timer tmrGroup4 = new System.Timers.Timer();
        Timer tmrGroup5 = new System.Timers.Timer();
        Timer tmrhlrmsc = new System.Timers.Timer();
        private MySqlConnection conn;
        private string ErrorMsg;
        private string FileError;
        private clsExceptionLogs oExceptionLog;
        private clsFileLogs oFileLog;

        //clsProcess objPSprocess = new clsProcess(System.Configuration.ConfigurationSettings.AppSettings["PSDBConstring"].ToString());

        clsCSprocess objCSprocess = new clsCSprocess();

        //clsTransferToNSSFromPS objTransferData = new clsTransferToNSSFromPS();

        String Constring = System.Configuration.ConfigurationManager.AppSettings["CSDBConstring"].ToString();

        public AWE_NSS_DCM()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {


                //Debugger.Launch();
                LogEvent("AWE_NSS_DCM Sercice for circle " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + " getting started...", EventLogEntryType.Information);

                //Paco_timer.Elapsed += new System.Timers.ElapsedEventHandler(Paco_timer_Elapsed);
                //Paco_timer.Interval = 10000;
                //Paco_timer.Enabled = true;
                //Paco_timer.AutoReset = true;
                conn = new MySqlConnection(Constring);
                oExceptionLog = new clsExceptionLogs("Bharti_DCM_Parse_" + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper());
                oFileLog = new clsFileLogs("Bharti_DCM_Parse_" + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper());

                CS_timer.Elapsed += new System.Timers.ElapsedEventHandler(CS_timer_Elapsed);
                CS_timer.Interval = 10000;
                CS_timer.Enabled = true;
                CS_timer.AutoReset = true;

                tmrGroup1.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerEventGroup1_Elapsed);
                tmrGroup1.Interval = 10000;
                tmrGroup1.Enabled = true;
                tmrGroup1.AutoReset = true;

                tmrGroup2.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerEventGroup2_Elapsed);
                tmrGroup2.Interval = 10000;
                tmrGroup2.Enabled = true;
                tmrGroup2.AutoReset = true;

                tmrGroup3.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerEventGroup3_Elapsed);
                tmrGroup3.Interval = 10000;
                tmrGroup3.Enabled = true;
                tmrGroup3.AutoReset = true;

                tmrGroup4.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerEventGroup4_Elapsed);
                tmrGroup4.Interval = 10000;
                tmrGroup4.Enabled = true;
                tmrGroup4.AutoReset = true;

                tmrGroup5.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerEventGroup5_Elapsed);
                tmrGroup5.Interval = 10000;
                tmrGroup5.Enabled = true;
                tmrGroup5.AutoReset = true;

                tmrhlrmsc.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerEventHLRMSC_Elapsed);
                tmrhlrmsc.Interval = 1000 * 60 * 15;
                tmrhlrmsc.Enabled = true;
                tmrhlrmsc.AutoReset = true;
                LogEvent("AWE_NSS_DCM service for circle " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + " started successfully...", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                LogEvent(e.Message, EventLogEntryType.Error);
            }
        }


        //private void Paco_timer_Elapsed(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Paco_timer.Enabled = false;
        //        if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["PSStartTime"].ToString())
        //        {
        //            if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsPACO"].ToString().ToLower()) == true)
        //            {

        //                LogEvent("Timer of PACO for circle " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + " disabled...", EventLogEntryType.Information);
        //                objPSprocess.StartMain();
        //                //Paco movement
        //                clsTransferToNSSFromPS.dcm_update();
        //                Paco_timer.Enabled = true;
        //                LogEvent("Timer of PACO for circle " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + " enabled...", EventLogEntryType.Information);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogEvent("Timer for PACO [Error while starting the service: " + ex.Message + "]", EventLogEntryType.Error);
        //    }
        //    finally
        //    {
        //        Paco_timer.Enabled = true;
        //    }
        //}
        private void CS_timer_Elapsed(object sender, EventArgs e)
        {
            String Circle = String.Empty;
            String Vendor = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper();
            Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
            try
            {
                CS_timer.Enabled = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["CSStartTime"].ToString())
                {
                    //DataTable dt = ExecuteSQL("Select ifnull(Status,'') as Status from tblProcessUpdate Where DCMProcess='CS' and Circle='" + Circle + "'  and Vendor='" + Vendor + "' ");
                    //if (dt.Rows.Count > 0)
                    //{
                    //    if ((dt.Rows[0]["Status"].ToString() == "") || (dt.Rows[0]["Status"].ToString().Contains("COMPLETED")))
                    //    {
                    //if (ExecuteNonQuery("Update tblProcessUpdate Set StartTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',EndTime='0000-00-00 00:00:00',Status='IN PROGRESS' Where DCMProcess='CS' and Circle='" + Circle + "'  and Vendor='" + Vendor + "'") == 1)
                    //{
                    //ExecuteNonQuery("Update tblProcessUpdate Set StartTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',EndTime='0000-00-00 00:00:00',Status='IN PROGRESS' Where DCMProcess='CS' and Circle='" + Circle + "'  and Vendor='" + Vendor + "'");

                    LogEvent("Timer of CS for circle " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + " started...", EventLogEntryType.Information);

                    clsBackupRestore objManageBackup = new clsBackupRestore();
                    objManageBackup.Manage_HLR_MSC_ASSO_DEST_Backup(Circle); // For Transfer data of MSC, HLR, Assosication Info and Destination Info table into their respective backup tables.

                    // For Deleting files from drive.
                    try
                    {
                        string rootFolderPath = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["folderPathForDelete"]);
                        string days = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["DaysForDeleteRawFile"]);
                        FolderAndFileOperations obj = new FolderAndFileOperations();
                        obj.DeleteAllFilesAndFolders(rootFolderPath, days);
                    }
                    catch
                    {
                    }


                    // For Starting Raw Generation
                    objCSprocess.StartMain();


                    //ExecuteNonQuery("Update tblProcessUpdate Set EndTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Status='COMPLETED' Where DCMProcess='CS' and Circle='" + Circle + "' and Vendor='" + Vendor + "'");
                    LogEvent("Timer of CS for circle " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + " end...", EventLogEntryType.Information);

                    //}
                    //    }

                    //}
                }


            }
            catch (Exception ex)
            {
                LogEvent("Timer for CS [Error while starting the service: " + ex.Message + "]", EventLogEntryType.Error);
                ExecuteSQLQuery(ref conn, "Update tblProcessUpdate Set EndTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Status='NOT COMPLETED',ErrorMsg='" + ex.Message.ToString() + "' Where DCMProcess='CS' and Circle='" + Circle + "'  and Vendor='" + Vendor + "'");
            }
            finally
            {
                CS_timer.Enabled = true;
            }
        }
        protected override void OnStop()
        {
            try
            {
                LogEvent("NSS_CS_DCM service stoping...", EventLogEntryType.Information);
                LogEvent("NSS_CS_DCM service stoped successfully...", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                LogEvent(e.Message, EventLogEntryType.Error);
            }
        }
        private void LogEvent(string sMessage, System.Diagnostics.EventLogEntryType EntryType)
        {
            try
            {
                EventLog oEventLog = new EventLog("NSS_CS_DCM");
                if (!System.Diagnostics.EventLog.SourceExists("NSS_CS_DCM"))
                {
                    System.Diagnostics.EventLog.CreateEventSource("NSS_CS_DCM", "NSS_CS_DCM");
                }
                System.Diagnostics.EventLog.WriteEntry("NSS_CS_DCM", sMessage, EntryType);
            }
            catch
            {
                //throw e;
            }
        }
        private System.Data.DataTable ExecuteSQL(String Query)
        {
            MySqlDataAdapter da = null;
            System.Data.DataTable Dt = null;
            try
            {
                if (Dt == null)
                {
                    Dt = new DataTable();
                }
                da = new MySqlDataAdapter(Query, Constring);
                da.Fill(Dt);
                return Dt;
            }
            catch
            {
                //throw ex;
                return null;
            }
        }
        public bool ExecuteSQLQuery(ref MySqlConnection MYSQL_connection, string querystr)
        {
            int retrycount = 0;
            MySqlCommand mMySqlCommand = new MySqlCommand();

            mMySqlCommand.CommandText = querystr;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = MYSQL_connection;
            mMySqlCommand.CommandTimeout = 900000;

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
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString() + ".Query:" + querystr;
                    oExceptionLog.WriteExceptionErrorToFile("ExecuteSQLQueryMain()", ErrorMsg, "", ref FileError);
                    return false;

                }
                goto retry;
            }
            finally
            {
                conn.Close();
            }


        }

        private void OnTimerEventGroup1_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                tmrGroup1.Enabled = false;
                ////string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ////int d1 = Int32.Parse(date1.Substring(11, 2));

                String Circle = String.Empty;
                String Vendor = String.Empty;
                String Group = String.Empty;
                Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
                Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
                Group = Circle + "_group" + 1;
                ////int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
                ////int rangeTime = startTime + 4;
                bool isProcessStart = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
                {
                    if (isProcessStart == false)
                    {
                        DataTable dt = ExecuteSQL("Select * from parsing_completion_details Where circle='" + Group + "' and Status='NOT COMPLETED' and START_TIME='0000-00-00 00:00:00' ");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                dt = null;
                                dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                if (dt != null)
                                {
                                    if (dt.Rows.Count == 0)
                                    {
                                        //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                        //{
                                        isProcessStart = true;
                                        clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                                        LogEvent("tmrGroup1 instantiating start  for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1.group_name = "group1";
                                        objParse1.NeMap = new string[1];
                                        objParse1.NeMap[0] = "MSS MAP";
                                        objParse1.StartParsing();
                                        LogEvent("tmrGroup1 process end for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);

                                        objParse1 = null;

                                        isProcessStart = false;
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in tmrGroup1.Error: " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                tmrGroup1.Enabled = true;
            }
        }

        private void OnTimerEventGroup2_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                tmrGroup2.Enabled = false;
                //string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //int d1 = Int32.Parse(date1.Substring(11, 2));

                String Circle = String.Empty;
                String Vendor = String.Empty;
                String Group = String.Empty;
                Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
                Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
                Group = Circle + "_group" + 2;
                //int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
                //int rangeTime = startTime + 4;
                bool isProcessStart = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
                {
                    if (isProcessStart == false)
                    {

                        DataTable dt = ExecuteSQL("Select * from parsing_completion_details Where circle='" + Group + "' and Status='NOT COMPLETED' and START_TIME='0000-00-00 00:00:00' ");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                dt = null;
                                dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                if (dt != null)
                                {
                                    if (dt.Rows.Count == 0)
                                    {
                                        //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                        //{
                                        isProcessStart = true;

                                        clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                                        LogEvent("tmrGroup2 instantiating start for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1.group_name = "group2";
                                        objParse1.NeMap = new string[1];
                                        objParse1.NeMap[0] = "MSS MAP";
                                        objParse1.StartParsing();
                                        LogEvent("tmrGroup2 process end for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1 = null;

                                        isProcessStart = false;
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in tmrGroup2.Error: " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                tmrGroup2.Enabled = true;
            }
        }

        private void OnTimerEventGroup3_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                tmrGroup3.Enabled = false;
                //string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //int d1 = Int32.Parse(date1.Substring(11, 2));

                String Circle = String.Empty;
                String Vendor = String.Empty;
                String Group = String.Empty;
                Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
                Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
                Group = Circle + "_group" + 3;
                //int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
                //int rangeTime = startTime + 4;
                bool isProcessStart = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
                {
                    if (isProcessStart == false)
                    {
                        DataTable dt = ExecuteSQL("Select * from parsing_completion_details Where circle='" + Group + "' and Status='NOT COMPLETED' and START_TIME='0000-00-00 00:00:00' ");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                dt = null;
                                dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                if (dt != null)
                                {
                                    if (dt.Rows.Count == 0)
                                    {
                                        //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                        //{
                                        isProcessStart = true;

                                        clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                                        LogEvent("tmrGroup3 instantiating start for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1.group_name = "group3";
                                        objParse1.NeMap = new string[1];
                                        objParse1.NeMap[0] = "MSS MAP";
                                        objParse1.StartParsing();
                                        LogEvent("tmrGroup3 process end for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1 = null;

                                        isProcessStart = false;
                                        //}
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in tmrGroup3.Error: " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                tmrGroup3.Enabled = true;
            }
        }

        public void OnTimerEventGroup4_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                tmrGroup4.Enabled = false;
                ////string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ////int d1 = Int32.Parse(date1.Substring(11, 2));

                String Circle = String.Empty;
                String Vendor = String.Empty;
                String Group = String.Empty;
                Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
                Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
                Group = Circle + "_group" + 4;
                //int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
                //int rangeTime = startTime + 4;
                bool isProcessStart = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
                {
                    if (isProcessStart == false)
                    {
                        DataTable dt = ExecuteSQL("Select * from parsing_completion_details Where circle='" + Group + "' and Status='NOT COMPLETED' and START_TIME='0000-00-00 00:00:00' ");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                dt = null;
                                dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                if (dt != null)
                                {
                                    if (dt.Rows.Count == 0)
                                    {
                                        //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                        //{
                                        isProcessStart = true;

                                        clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                                        LogEvent("tmrGroup4 instantiating start for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1.group_name = "group4";
                                        objParse1.NeMap = new string[1];
                                        objParse1.NeMap[0] = "MSS MAP";
                                        objParse1.StartParsing();

                                        //clsCSParsingProcess objParse2 = new clsCSParsingProcess();
                                        //objParse2.group_name = "HLR_MSC";
                                        //objParse2.NeMap = new string[2];
                                        //objParse2.NeMap[0] = "MSC MAP";
                                        //objParse2.NeMap[1] = "HLR MAP";
                                        //objParse2.StartParsing();

                                        ////clsCSParsingProcess objParse3 = new clsCSParsingProcess();
                                        ////objParse3.group_name = "HLR_MSC";
                                        ////objParse3.NeMap = new string[1];
                                        ////objParse3.NeMap[0] = "HLR MAP";
                                        ////objParse3.StartParsing();

                                        //clsCSParsingProcess objParse4 = new clsCSParsingProcess();
                                        //objParse4.group_name = "";
                                        //objParse4.NeMap = new string[1];
                                        //objParse4.NeMap[0] = "ASSOCIATION INFO";
                                        //objParse4.StartParsing();

                                        //clsCSParsingProcess objParse5 = new clsCSParsingProcess();
                                        //objParse5.group_name = "";
                                        //objParse5.NeMap = new string[1];
                                        //objParse5.NeMap[0] = "DESTINATION INFO";
                                        //objParse5.StartParsing();
                                        LogEvent("tmrGroup4 process end for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1 = null;

                                        isProcessStart = false;
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in tmrGroup4.Error: " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                tmrGroup4.Enabled = true;
            }
        }
        public void OnTimerEventHLRMSC_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                tmrhlrmsc.Enabled = false;
                ////string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ////int d1 = Int32.Parse(date1.Substring(11, 2));


                String Circle = String.Empty;
                String Vendor = String.Empty;
                String Group = String.Empty;
                Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
                Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
                Group = Circle + "_HLR_MSC";
                ////int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
                ////int rangeTime = startTime + 4;
                bool isProcessStart = false;
                //if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
                //{
                if (isProcessStart == false)
                {

                    DataTable dt = ExecuteSQL("Select * from raw_generation_details Where circle Like '" + Circle + "%' and Status='NOT COMPLETED'");
                    if (dt != null)
                    {
                        if (dt.Rows.Count == 0)
                        {
                            dt = null;
                            //For checking in all groups for completion of parsing of MSS,MGW.
                            dt = ExecuteSQL("Select * from parsing_completion_details WHERE CIRCLE Like '" + Circle + "%' and CIRCLE not Like '%hlr_msc'  and Status='NOT COMPLETED'");
                            if (dt != null)
                            {
                                if (dt.Rows.Count == 0)
                                {
                                    dt = null;
                                    //For checking current group has not been started parsing.
                                    dt = ExecuteSQL("Select * from parsing_completion_details Where circle='" + Group + "' and Status='NOT COMPLETED' and START_TIME='0000-00-00 00:00:00' ");
                                    if (dt != null)
                                    {
                                        if (dt.Rows.Count > 0)
                                        {
                                            dt = null;
                                            //For checking raw generation of current group has not been completed.
                                            //dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                            //if (dt != null)
                                            //{
                                            //    if (dt.Rows.Count == 0)
                                            //    {
                                            //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                            //{
                                            isProcessStart = true;

                                            clsCSParsingProcess objParse2 = new clsCSParsingProcess();
                                            LogEvent("HLR_MSC instantiating start for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                            objParse2.group_name = "HLR_MSC";
                                            objParse2.NeMap = new string[4];
                                            objParse2.NeMap[0] = "MSC MAP";
                                            objParse2.NeMap[1] = "HLR MAP";
                                            objParse2.NeMap[2] = "ASSOCIATION INFO";
                                            objParse2.NeMap[3] = "DESTINATION INFO";

                                            objParse2.StartParsing();
                                            LogEvent("HLR_MSC process end for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);

                                            LogEvent("Restore_HLR_MSC_ASSO_DEST_From_Backup process start for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                            clsBackupRestore objManageBackup = new clsBackupRestore();
                                            objManageBackup.Restore_HLR_MSC_ASSO_DEST_From_Backup(Circle); // For restore data from MSC, HLR, Assosication Info and Destination Info backup table table to their respective tables.
                                            // Stored procedure also check if there is no rows in table(MSC, HLR,Association info, Destination info) then Restore data from their backup tables.
                                            LogEvent("Restore_HLR_MSC_ASSO_DEST_From_Backup process end for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);

                                            //clsTransferToNSSFromCS.DCMDataMigration(Circle);
                                            ExecuteSQLQuery(ref conn, "UPDATE  tblprocessupdate SET `status` = 'NOT COMPLETED', `STARTTIME` = '0000-00-00 00:00:00', `ENDTIME`= '0000-00-00 00:00:00' where `circle`= '" + Circle + "'");
                                             
                                            objParse2 = null;
                                            //clsCSParsingProcess objParse2 = new clsCSParsingProcess();
                                            //objParse2.group_name = "HLR_MSC";
                                            //objParse2.NeMap = new string[2];
                                            //objParse2.NeMap[0] = "MSC MAP";
                                            //objParse2.NeMap[1] = "HLR MAP";
                                            //objParse2.StartParsing();

                                            ////clsCSParsingProcess objParse3 = new clsCSParsingProcess();
                                            ////objParse3.group_name = "HLR_MSC";
                                            ////objParse3.NeMap = new string[1];
                                            ////objParse3.NeMap[0] = "HLR MAP";
                                            ////objParse3.StartParsing();

                                            //clsCSParsingProcess objParse4 = new clsCSParsingProcess();
                                            //objParse4.group_name = "";
                                            //objParse4.NeMap = new string[1];
                                            //objParse4.NeMap[0] = "ASSOCIATION INFO";
                                            //objParse4.StartParsing();

                                            //clsCSParsingProcess objParse5 = new clsCSParsingProcess();
                                            //objParse5.group_name = "";
                                            //objParse5.NeMap = new string[1];
                                            //objParse5.NeMap[0] = "DESTINATION INFO";
                                            //objParse5.StartParsing();
                                            //LogEvent("tmrGroup4 process end...", EventLogEntryType.Information);
                                            //objParse2 = null;
                                            //objParse4 = null;
                                            //objParse5 = null;

                                            isProcessStart = false;
                                            //}
                                            //    }
                                            //}
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in tmrGroup4.Error: " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                tmrhlrmsc.Enabled = true;
            }
        }
        private void OnTimerEventGroup5_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                tmrGroup5.Enabled = false;
                //string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //int d1 = Int32.Parse(date1.Substring(11, 2));

                String Circle = String.Empty;
                String Vendor = String.Empty;
                String Group = String.Empty;
                Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
                Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
                Group = Circle + "_group" + 5;
                ////int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
                ////int rangeTime = startTime + 4;
                bool isProcessStart = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
                {
                    if (isProcessStart == false)
                    {
                        DataTable dt = ExecuteSQL("Select * from parsing_completion_details Where circle='" + Group + "' and Status='NOT COMPLETED' and START_TIME='0000-00-00 00:00:00' ");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                dt = null;
                                dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                if (dt != null)
                                {
                                    if (dt.Rows.Count == 0)
                                    {
                                        //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                        //{
                                        isProcessStart = true;

                                        clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                                        LogEvent("tmrGroup5 instantiating start for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1.group_name = "group5";
                                        objParse1.NeMap = new string[1];
                                        objParse1.NeMap[0] = "MSS MAP";
                                        objParse1.StartParsing();
                                        LogEvent("tmrGroup5 process end for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                                        objParse1 = null;

                                        isProcessStart = false;
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in tmrGroup5.Error: " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                tmrGroup5.Enabled = true;
            }
        }

    }
}
