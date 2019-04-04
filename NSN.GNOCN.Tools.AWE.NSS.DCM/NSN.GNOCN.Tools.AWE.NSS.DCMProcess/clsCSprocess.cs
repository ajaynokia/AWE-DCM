using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;


namespace NSN.GNOCN.Tools.AWE.NSS
{
    public class clsCSprocess
    {
        System.Timers.Timer TimerGroup1;
        System.Timers.Timer TimerGroup2;
        System.Timers.Timer TimerGroup3;
        System.Timers.Timer TimerGroup4;
        System.Timers.Timer TimerGroup5;



        public void StartMain()
        {
            try
            {

                if (TimerGroup1 == null)
                {
                    TimerGroup1 = new System.Timers.Timer();
                    TimerGroup1.Elapsed += new ElapsedEventHandler(OnTimedEventGroup1_Elapsed);
                    TimerGroup1.Interval = 10000;
                    TimerGroup1.Enabled = true;
                }
                else
                {
                    if (TimerGroup1.Enabled == false) TimerGroup1.Enabled = true;
                }




                if (TimerGroup2 == null)
                {
                    TimerGroup2 = new System.Timers.Timer();
                    TimerGroup2.Elapsed += new ElapsedEventHandler(OnTimedEventGroup2_Elapsed);
                    TimerGroup2.Interval = 10000;
                    TimerGroup2.Enabled = true;
                }
                else
                {
                    if (TimerGroup2.Enabled == false) TimerGroup2.Enabled = true;
                }




                if (TimerGroup3 == null)
                {
                    TimerGroup3 = new System.Timers.Timer();
                    TimerGroup3.Elapsed += new ElapsedEventHandler(OnTimedEventGroup3_Elapsed);
                    TimerGroup3.Interval = 10000;
                    TimerGroup3.Enabled = true;
                }
                else
                {
                    if (TimerGroup3.Enabled == false) TimerGroup3.Enabled = true;
                }



                if (TimerGroup4 == null)
                {
                    TimerGroup4 = new System.Timers.Timer();
                    TimerGroup4.Elapsed += new ElapsedEventHandler(OnTimedEventGroup4_Elapsed);
                    TimerGroup4.Interval = 10000;
                    TimerGroup4.Enabled = true;
                }
                else
                {
                    if (TimerGroup4.Enabled == false) TimerGroup4.Enabled = true;
                }

                if (TimerGroup5 == null)
                {
                    TimerGroup5 = new System.Timers.Timer();
                    TimerGroup5.Elapsed += new ElapsedEventHandler(OnTimedEventGroup5_Elapsed);
                    TimerGroup5.Interval = 10000;
                    TimerGroup5.Enabled = true;
                }
                else
                {
                    if (TimerGroup5.Enabled == false) TimerGroup5.Enabled = true;
                }



            }
            catch (Exception exc)
            {
                LogEvent("Exception in StartMain().Error: " + exc.Message, EventLogEntryType.Information);
                //throw exc;
            }
            finally
            {
                System.Threading.Thread.Sleep(60 * 1000 * 2);
            }
        }
        private void OnTimedEventGroup1_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //TimerGroup1.Enabled = false;  
                //string errorFolderPath = @"D:\DP\";
                //if (!System.IO.Directory.Exists(errorFolderPath))
                //{
                //    System.IO.Directory.CreateDirectory(errorFolderPath);
                //}
                //System.IO.File.AppendAllText(errorFolderPath + "\\" + "_error_log.txt", "Test file" + DateTime.Now + "\n\r");
                //if (System.DateTime.Now.ToShortTimeString() == "00:01")
                //{
                ////LogEvent("TimerGroup1 disabled", EventLogEntryType.Information);


                TimerGroup1.Enabled = false;
                clsCSGetRawProcess obj = new clsCSGetRawProcess();
                obj.nodeTypeMain = new string[3];
                obj.nodeTypeMain[0] = "MSS";
                obj.nodeTypeMain[1] = "GCS";
                obj.nodeTypeMain[2] = "MGW";                
                obj.groupName = "group1";
                obj.StartRawGeneration();
                LogEvent("TimerGroup1 Raw Generation Complete for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                //objParse1.group_name = "group1";
                //objParse1.NeMap = new string[1];
                //objParse1.NeMap[0] = "MSS MAP";
                //objParse1.StartParsing();

                obj = null;
                //objParse1 = null;

            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in TimerGroup1.Error in " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + ": " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                //TimerGroup1.Enabled = true;
            }
        }

        private void OnTimedEventGroup2_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                TimerGroup2.Enabled = false;
                clsCSGetRawProcess obj = new clsCSGetRawProcess();

                obj.nodeTypeMain = new string[3];
                obj.nodeTypeMain[0] = "MSS";
                obj.nodeTypeMain[1] = "GCS";
                obj.nodeTypeMain[2] = "MGW";
                obj.groupName = "group2";
                obj.StartRawGeneration();
                LogEvent("TimerGroup2 Raw Generation Complete for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);
                //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                //objParse1.group_name = "group2";
                //objParse1.NeMap = new string[1];
                //objParse1.NeMap[0] = "MSS MAP";
                //objParse1.StartParsing();

                obj = null;
                ////objParse1 = null;
                //  }

            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in TimerGroup2.Error for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + ": " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                //TimerGroup2.Enabled = true;
            }
        }

        private void OnTimedEventGroup3_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                TimerGroup3.Enabled = false;
                clsCSGetRawProcess obj = new clsCSGetRawProcess();

                obj.nodeTypeMain = new string[3];
                obj.nodeTypeMain[0] = "MSS";
                obj.nodeTypeMain[1] = "GCS";
                obj.nodeTypeMain[2] = "MGW";
                obj.groupName = "group3";
                obj.StartRawGeneration();


                LogEvent("TimerGroup3 Raw Generation Complete for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);




                //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                //objParse1.group_name = "group3";
                //objParse1.NeMap = new string[1];
                //objParse1.NeMap[0] = "MSS MAP";
                //objParse1.StartParsing();

                obj = null;
                
                //objParse1 = null;


            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in TimerGroup3.Error for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + ": " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                //TimerGroup3.Enabled = true;
            }
        }

        public void OnTimedEventGroup4_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                TimerGroup4.Enabled = false;
                clsCSGetRawProcess obj = new clsCSGetRawProcess();

                obj.nodeTypeMain = new string[3];
                obj.nodeTypeMain[0] = "MSS";
                obj.nodeTypeMain[1] = "GCS";
                obj.nodeTypeMain[2] = "MGW";
                obj.groupName = "group4";
                obj.StartRawGeneration();
                // for calling msc_hlr commands

                clsCSGetRawProcess obj1 = new clsCSGetRawProcess();

                obj1.groupName = "HLR_MSC";
                obj1.nodeTypeMain = new string[2];
                obj1.nodeTypeMain[0] = "MSC";
                obj1.nodeTypeMain[1] = "HLR";
                obj1.StartRawGeneration();
                LogEvent("TimerGroup4 and HLRMSC Raw Generation Complete for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);

                //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                //objParse1.group_name = "group4";
                //objParse1.NeMap = new string[1];
                //objParse1.NeMap[0] = "MSS MAP";
                //objParse1.StartParsing();

                //clsCSParsingProcess objParse2 = new clsCSParsingProcess();
                //objParse2.group_name = "";
                //objParse2.NeMap = new string[1];
                //objParse2.NeMap[0] = "MSC MAP";
                //objParse2.StartParsing();

                //clsCSParsingProcess objParse3 = new clsCSParsingProcess();
                //objParse3.group_name = "";
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


                obj = null;
                obj1 = null;
                //objParse1 = null;
                //objParse2 = null;
                //objParse3 = null;
                //objParse4 = null;
                //objParse5 = null;
                //}




            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in TimerGroup4.Error for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + ": " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                //TimerGroup4.Enabled = true;
            }
        }
        private void OnTimedEventGroup5_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                TimerGroup5.Enabled = false;
                clsCSGetRawProcess obj = new clsCSGetRawProcess();

                obj.nodeTypeMain = new string[3];
                obj.nodeTypeMain[0] = "MSS";
                obj.nodeTypeMain[1] = "GCS";
                obj.nodeTypeMain[2] = "MGW";
                obj.groupName = "group5";
                obj.StartRawGeneration();
                LogEvent("TimerGroup5 Raw Generation Complete for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + "...", EventLogEntryType.Information);

                //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
                //objParse1.group_name = "group5";
                //objParse1.NeMap = new string[1];
                //objParse1.NeMap[0] = "MSS MAP";
                //objParse1.StartParsing();

                obj = null;
                //objParse1 = null;
                //}

            }
            catch (Exception ex)
            {
                //throw ex;
                LogEvent("Error in TimerGroup5.Error for " + System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString() + ": " + ex.Message.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                //    TimerGroup5.Enabled = true;
            }
        }

        //public void test()
        //{
        //    try
        //    {
        //        //if (System.DateTime.Now.ToShortTimeString() == "00:15")
        //        //{
        //        //TimerGroup4.Enabled = false;
        //        clsCSGetRawProcess obj = new clsCSGetRawProcess();

        //        obj.nodeTypeMain = new string[2];
        //        obj.nodeTypeMain[0] = "MSS";
        //        obj.nodeTypeMain[1] = "MGW";

        //        obj.groupName = "group4";
        //        obj.StartRawGeneration();
        //        // for calling msc_hlr commands

        //        obj.groupName = "HLR_MSC";
        //        obj.nodeTypeMain = new string[2];
        //        obj.nodeTypeMain[0] = "MSC";
        //        obj.nodeTypeMain[1] = "HLR";
        //        obj.StartRawGeneration();

        //        clsCSParsingProcess objParse1 = new clsCSParsingProcess();
        //        objParse1.group_name = "group4";
        //        objParse1.NeMap = new string[1];
        //        objParse1.NeMap[0] = "MSS MAP";
        //        objParse1.StartParsing();

        //        clsCSParsingProcess objParse2 = new clsCSParsingProcess();
        //        objParse2.group_name = "HLR_MSC";
        //        objParse2.NeMap = new string[2];
        //        objParse2.NeMap[0] = "MSC";
        //        objParse2.NeMap[1] = "HLR";
        //        objParse2.StartParsing();

        //        obj = null;
        //        objParse1 = null;
        //        //}




        //    }
        //    catch (Exception ex)
        //    {
        //        //throw ex;
        //    }
        //    finally
        //    {
        //        //TimerGroup4.Enabled = true;
        //    }
        //}


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
            catch (Exception e)
            {
                //throw e;
            }
        }
    }
}
