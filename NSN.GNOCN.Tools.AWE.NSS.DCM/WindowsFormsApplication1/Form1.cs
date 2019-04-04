using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NSN.GNOCN.Tools.AWE.NSS;
using System.Configuration;
using System.IO;
using MySql.Data.MySqlClient;
using NSN.GNOCN.Tools.AWE.NSS.DCMCSProcess;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String Circle = String.Empty;
            String Vendor = String.Empty;
            String Group = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
            //objCSprocess.StartMain();
            //CS_timer.Elapsed += new System.Timers.ElapsedEventHandler(CS_timer_Elapsed);
            //CS_timer.Interval = 2000;
            //CS_timer.Enabled = true;
            clsTransferToNSSFromCS.DCMDataMigration(Circle);


        }
        System.Timers.Timer Paco_timer = new System.Timers.Timer();
        System.Timers.Timer CS_timer = new System.Timers.Timer();



        clsCSprocess objCSprocess = new clsCSprocess();






        private void button2_Click(object sender, EventArgs e)
        {

            clsCSGetRawProcess obj = new clsCSGetRawProcess();
            obj.nodeTypeMain = new string[3];
            obj.nodeTypeMain[0] = "MSS";
            obj.nodeTypeMain[1] = "GCS";
            obj.nodeTypeMain[2] = "MGW";
            obj.groupName = "group3";
            obj.StartRawGeneration();

            String Circle = String.Empty;
            String Vendor = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper();
            Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();

            clsBackupRestore objManageBackup = new clsBackupRestore();
            objManageBackup.Manage_HLR_MSC_ASSO_DEST_Backup(Circle); // For Transfer data of MSC, HLR, Assosication Info and Destination Info table into their respective backup tables.

            // For Deleting files from drive.
            //try
            //{
            //    string rootFolderPath = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["folderPathForDelete"]);
            //    string days = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["DaysForDeleteRawFile"]);
            //    FolderAndFileOperations obj = new FolderAndFileOperations();
            //    obj.DeleteAllFilesAndFolders(rootFolderPath, days);
            //}
            //catch
            //{
            //}


            // For Starting Raw Generation
            objCSprocess.StartMain();

            //clsTransferToNSSFromCS.DCMDataMigration("UPW");
            //string Errordir = @"D:\PACO\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            //if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["PSStartTime"].ToString())
            //{

            //    if (!Directory.Exists(Errordir))
            //    {
            //        Directory.CreateDirectory(Errordir);

            //    }
            //    if ((!System.IO.File.Exists(Errordir + "\\" + "AWE_NSS_PACO.txt")))
            //    {
            //        File.AppendAllText(Errordir + "\\" + "AWE_NSS_PACO.txt", "DCM PACO process has been started at " + System.DateTime.Now.ToString() + "\r\n");

            //        //Paco_timer.Enabled = false;
            //        //objPSprocess.StartMain();
            //        ////Paco movement
            //        //clsTransferToNSSFromPS.dcm_update();
            //        System.IO.File.Delete(Errordir + "\\" + "AWE_NSS_PACO.txt");
            //    }



            //}
            //if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["PSStartTime"].ToString())
            //{
            //    if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsPACO"].ToString().ToLower()) == true)
            //    {

            //        System.Threading.Thread.Sleep(60 * 1000 * 2);
            //    }
            //}


            clsCSGetRawProcess obj5 = new clsCSGetRawProcess();
            obj5.nodeTypeMain = new string[2];
            obj5.nodeTypeMain[0] = "MSS";
            obj5.nodeTypeMain[1] = "MGW";

            obj5.groupName = "group2";

            obj5.StartRawGeneration();

            clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            objParse1.group_name = "group1";
            objParse1.NeMap = new string[1];
            objParse1.NeMap[0] = "MSS MAP";
            objParse1.StartParsing();
            if (System.DateTime.Now.ToString("hh:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
            { 
            
            }

        }
        String Constring = System.Configuration.ConfigurationSettings.AppSettings["CSDBConstring"].ToString();
        private void button3_Click(object sender, EventArgs e)
        {
            //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            //objParse1.group_name = "group1";
            //objParse1.NeMap = new string[1];
            //objParse1.NeMap[0] = "MSS MAP";
            //objParse1.StartParsing();
            //clsCSprocess obj = new clsCSprocess();
            //obj.StartMain();

            String Circle = String.Empty;
            String Vendor = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper();
            Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
            clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            objParse1.group_name = "group5";
            objParse1.NeMap = new string[1];
            objParse1.NeMap[0] = "MSS MAP";
            objParse1.StartParsing();
            clsCSprocess obj = new clsCSprocess();
            obj.StartMain();

            //clsCSParsingProcess objParse2 = new clsCSParsingProcess();
            //objParse2.group_name = "HLR_MSC";
            //objParse2.NeMap = new string[4];
            //objParse2.NeMap[0] = "MSC MAP";
            //objParse2.NeMap[1] = "HLR MAP";
            //objParse2.NeMap[2] = "ASSOCIATION INFO";
            //objParse2.NeMap[3] = "DESTINATION INFO";

            //objParse2.StartParsing();

            //clsCSGetRawProcess obj = new clsCSGetRawProcess();
            //obj.nodeTypeMain = new string[3];
            //obj.nodeTypeMain[0] = "MSS";
            //obj.nodeTypeMain[1] = "GCS";
            //obj.nodeTypeMain[2] = "MGW";
            //obj.groupName = "group1";
            //obj.StartRawGeneration();
            //obj.groupName = "HLR_MSC";
            //obj.nodeTypeMain = new string[2];
            //obj.nodeTypeMain[0] = "MSC";
            //obj.nodeTypeMain[1] = "HLR";
            //obj.StartRawGeneration();

            try
            {


                //CS_timer.Enabled = false;
                //objCSprocess.StartMain();







            }
            catch (Exception ex)
            {

                ExecuteNonQuery("Update tblProcessUpdate Set EndTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Status='NOT COMPLETED',ErrorMsg='" + ex.Message.ToString() + "' Where DCMProcess='CS' and Circle='" + Circle + "'  and Vendor='" + Vendor + "'");
            }
            finally
            {
                CS_timer.Enabled = true;
            }

        }
        private void CS_timer_Elapsed(object sender, EventArgs e)
        {
            String Circle = String.Empty;
            String Vendor = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper();
            Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
            try
            {


                CS_timer.Enabled = false;
                objCSprocess.StartMain();







            }
            catch (Exception ex)
            {

                ExecuteNonQuery("Update tblProcessUpdate Set EndTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Status='NOT COMPLETED',ErrorMsg='" + ex.Message.ToString() + "' Where DCMProcess='CS' and Circle='" + Circle + "'  and Vendor='" + Vendor + "'");
            }
            finally
            {
                CS_timer.Enabled = true;
            }
        }


        //private void LogEvent(string sMessage, System.Diagnostics.EventLogEntryType EntryType)
        //{
        //    try
        //    {
        //        EventLog oEventLog = new EventLog("AWE_NSS_DCM");
        //        if (!System.Diagnostics.EventLog.SourceExists("AWE_NSS_DCM"))
        //        {
        //            System.Diagnostics.EventLog.CreateEventSource("AWE_NSS_DCM", "AWE_NSS_DCM");
        //        }
        //        System.Diagnostics.EventLog.WriteEntry("AWE_NSS_DCM", sMessage, EntryType);
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
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
            catch (Exception ex)
            {
                //throw ex;
                return null;
            }
        }
        private Int32 ExecuteNonQuery(String Query)
        {
            MySqlConnection Conn = null;
            MySqlCommand Command = null;
            int result = 0;
            try
            {
                Conn = new MySqlConnection(Constring);
                // Command.CommandTimeout = 20000;
                Command = new MySqlCommand(Query, Conn);
                Command.CommandTimeout = 20000;
                Conn.Open();
                result = Command.ExecuteNonQuery();
                Conn.Close();
                return result;
            }
            catch (Exception ex)
            {
                //throw ex;
                return result;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                Command.Dispose();
                Conn.Dispose();
                if (Conn != null)
                {
                    Conn = null;
                }

                if (Command != null)
                {
                    Command = null;
                }

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //CS_timer.Elapsed += new System.Timers.ElapsedEventHandler(CS_timer_Elapsed);
            //CS_timer.Interval = 10000;
            //CS_timer.Enabled = true;
            ////clsCSGetRawProcess obj = new clsCSGetRawProcess();
            ////obj.nodeTypeMain = new string[2];
            ////obj.nodeTypeMain[0] = "MSS";
            ////obj.nodeTypeMain[1] = "MGW";
            ////obj.groupName = "group1";
            ////obj.StartRawGeneration();
        }

        private void button4_Click(object sender, EventArgs e)
        {            
            clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            objParse1.group_name = "group3";
            objParse1.NeMap = new string[1];
            objParse1.NeMap[0] = "MSS MAP";
            objParse1.StartParsing();  // Check all point for when debugging





           
            
            
            
            
            //clsCSGetRawProcess obj=new clsCSGetRawProcess();
            //obj.groupName = "HLR_MSC";
            //obj.nodeTypeMain = new string[2];
            //obj.nodeTypeMain[0] = "MSC";
            //obj.nodeTypeMain[1] = "HLR";
            //obj.StartRawGeneration();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            //if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["CSStartTime"].ToString())
            //{
            //    MessageBox.Show("Hi");
            //}
            //string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //int d1 = Int32.Parse(date1.Substring(11, 2));

            //String Circle = String.Empty;
            //String Vendor = String.Empty;
            //String Group = String.Empty;
            //Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
            //Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
            //Group = Circle + "_group" + 1;
            //int startTime =Int32.Parse( System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
            //int rangeTime = startTime + 4;
            //bool isProcessStart = false;
            //if (d1 >= startTime && d1 <= rangeTime)
            //{
            //    if (isProcessStart == false)
            //    {
            //        DataTable dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "'");
            //        if (dt != null)
            //        {
            //            if (dt.Rows.Count > 0)
            //            {
            //                if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
            //                {
            //                    isProcessStart = true;


            //                    isProcessStart = false;
            //                }
            //            }
            //        }
            //    }
            //}
            //clsProcess objPSprocess = new clsProcess(System.Configuration.ConfigurationSettings.AppSettings["PSDBConstring"].ToString());
            //objPSprocess.StartMain();\


            string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int d1 = Int32.Parse(date1.Substring(11, 2));

            String Circle = String.Empty;
            String Vendor = String.Empty;
            String Group = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
            Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
            Group = Circle + "_group" + 1;
            int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
            int rangeTime = startTime + 4;
            bool isProcessStart = false;
            if (d1 >= startTime && d1 <= rangeTime)
            {
                if (isProcessStart == false)
                {
                    DataTable dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                    if (dt != null)
                    {
                        if (dt.Rows.Count == 0)
                        {
                            //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                            //{
                            isProcessStart = true;
                            clsCSParsingProcess objParse1 = new clsCSParsingProcess();

                            objParse1.group_name = "group1";
                            objParse1.NeMap = new string[1];
                            objParse1.NeMap[0] = "MSS MAP";
                            objParse1.StartParsing();


                            objParse1 = null;

                            isProcessStart = false;
                            //}
                        }
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ExecuteNonQuery("update parsing_completion_details set `STATUS` = 'COMPLETED', `START_TIME`='0000-00-00 00:00:00',`END_TIME`='0000-00-00 00:00:00' where circle like 'bihar_group%'");

            string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int d1 = Int32.Parse(date1.Substring(11, 2));

            String Circle = String.Empty;
            String Vendor = String.Empty;
            String Group = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
            Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
            Group = Circle + "_HLR_MSC";
            int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
            int rangeTime = startTime + 4;
            bool isProcessStart = false;
            if (d1 >= startTime && d1 <= rangeTime)
            {
                if (isProcessStart == false)
                {
                    //For checking in all groups for completion of parsing of MSS,MGW.
                    DataTable dt = ExecuteSQL("Select * from parsing_completion_details WHERE CIRCLE Like '" + Circle + "%' and CIRCLE not Like '%hlr_msc'  and Status='NOT COMPLETED'");
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
                                    dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                    if (dt != null)
                                    {
                                        if (dt.Rows.Count == 0)
                                        {
                                            //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                            //{
                                            isProcessStart = true;
                                            
                                            clsCSParsingProcess objParse2 = new clsCSParsingProcess();
                                            objParse2.group_name = "HLR_MSC";
                                            objParse2.NeMap = new string[4];
                                            objParse2.NeMap[0] = "MSC MAP";
                                            objParse2.NeMap[1] = "HLR MAP";
                                            objParse2.NeMap[2] = "ASSOCIATION INFO";
                                            objParse2.NeMap[3] = "DESTINATION INFO";
                                            objParse2.StartParsing();
                                            clsTransferToNSSFromCS.DCMDataMigration(Circle);//Added By Deepak on 14-03-2013
                                            //clsCSParsingProcess objParse3 = new clsCSParsingProcess();
                                            //objParse3.group_name = "HLR_MSC";
                                            //objParse3.NeMap = new string[1];
                                            //objParse3.NeMap[0] = "HLR MAP";
                                            //objParse3.StartParsing();

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
                                         
                                            objParse2 = null;
                                            //objParse4 = null;
                                            //objParse5 = null;

                                            isProcessStart = false;
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnManageBackup_Click(object sender, EventArgs e)
        {
            clsBackupRestore obj = new clsBackupRestore();
            obj.Manage_HLR_MSC_ASSO_DEST_Backup("GUJ");
        }

        private void btnRestoreBackup_Click(object sender, EventArgs e)
        {
            clsBackupRestore obj = new clsBackupRestore();
            obj.Restore_HLR_MSC_ASSO_DEST_From_Backup("GUJ");
        }

        private void btnRawGeneration_Click(object sender, EventArgs e)
        {
                           ////string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ////int d1 = Int32.Parse(date1.Substring(11, 2));
            //clsCSGetRawProcess obj = new clsCSGetRawProcess();
            //obj.nodeTypeMain = new string[3];
            //obj.nodeTypeMain[0] = "MSS";
            //obj.nodeTypeMain[1] = "GCS";
            //obj.nodeTypeMain[2] = "MGW";
            //obj.groupName = "group1";
            //obj.StartRawGeneration();
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

                                    objParse1.group_name = "group2";
                                    objParse1.NeMap = new string[1];
                                    objParse1.NeMap[0] = "MSS MAP";
                                    objParse1.StartParsing();

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
    }
}
