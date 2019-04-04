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
using NSN.GNOCN.Tools.Logs;
namespace NSN.GNOCN.Tools.AWE.NSS
{
    public class clsCSParsingProcess
    {
        public string ErrorMsg = string.Empty;
        public string FileError = string.Empty;
        public string group_name = string.Empty;
        public string ConnString = string.Empty;
        public MySqlConnection conn;
        string _NEMAP = string.Empty;
        public string[] NeMap;
        public string _CircleName = string.Empty;
        public string _vendor = string.Empty;
        public string _NEType = string.Empty;
        public string _ne_name = string.Empty;

        public string folderPath = string.Empty;
        public string errorFolderPath = string.Empty;
        private string Errordir = "clsCSParsingProcess";
        private clsExceptionLogs oExceptionLog;
        private clsFileLogs oFileLog;
        public string ErrorQuery = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public void StartParsing()
        {

            try
            {


                // Reading configuration file (MasterDb.XML)               
                string masterDb = Convert.ToString(System.Configuration.ConfigurationSettings.AppSettings["XMLFile"]);
                DataSet ds = new DataSet();
                ds.ReadXml(masterDb);
                if (ds.Tables.Count > 0)
                {
                    ConnString = Convert.ToString(ds.Tables[0].Rows[0]["connectionString"]);
                    conn = new MySqlConnection(Convert.ToString(ds.Tables[0].Rows[0]["connectionString"]));
                    _CircleName = Convert.ToString(ds.Tables[0].Rows[0]["circle"]);
                    _vendor = Convert.ToString(ds.Tables[0].Rows[0]["vendor"]);
                    folderPath = Convert.ToString(ds.Tables[0].Rows[0]["folderPath"]);
                    errorFolderPath = Convert.ToString(ds.Tables[0].Rows[0]["errorFolderPath"]);
                }
                oExceptionLog = new clsExceptionLogs("Bharti_DCM_Parse_" + _CircleName);
                oFileLog = new clsFileLogs("Bharti_DCM_Parse_" + _CircleName);

                string parsing_flag = string.Empty;
                string raw_path = @folderPath + System.DateTime.Now.ToString("dd_MM_yyyy") + @"\";

                // Comment below two lines when debugging

                try
                {
                    if (!("mss_link_" + _CircleName + "_" + group_name).Contains("HLR_MSC"))
                    {
                        ExecuteSQLQuery(ref conn, "delete from mss_link_" + _CircleName + "_" + group_name);
                        ExecuteSQLQuery(ref conn, "delete from mss_mgw_map_" + _CircleName + "_" + group_name);

                    }
                }
                catch { }
                try
                {
                    if (!("mss_mgw_map_" + _CircleName + "_" + group_name).Contains("HLR_MSC"))
                    {
                        //ExecuteSQLQuery(ref conn, "delete from mss_link_" + _CircleName + "_" + group_name);
                        ExecuteSQLQuery(ref conn, "delete from mss_mgw_map_" + _CircleName + "_" + group_name);

                    }
                }
                catch { }

                DataTable RawDt = new DataTable();

            again: string query1 = "SELECT `STATUS` FROM raw_generation_details where `circle`= '" + _CircleName + "_" + group_name + "' ";

                SQLQuery(ref conn, ref RawDt, query1);

                if (RawDt != null) //Added By Deepak on 19-03-2013
                {
                    if (RawDt.Rows.Count > 0)//Added By Deepak on 19-03-2013
                    {
                        string Raw_status = RawDt.Rows[0][0].ToString().Trim();
                        if (Raw_status.ToUpper() == "COMPLETED")
                        {                            
                            // comment when debugging
                            ExecuteSQLQuery(ref conn, "UPDATE  parsing_completion_details SET `status` = 'NOT COMPLETED', `START_TIME` = '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', `END_TIME`= '0000-00-00 00:00:00' where `circle`= '" + _CircleName + "_" + group_name + "'");
                            for (int K = 0; K <= NeMap.Length - 1; K++)
                            {
                                _NEMAP = NeMap[K].ToString();
                                start();
                            }

                            parsing_flag = "true";
                            //pramod changes
                            missing_element_info(_CircleName, group_name);  // if some elements fails to get login while generating raw file then data from previous day is copied from backup tables
                            Inser_data_into_final_table(_CircleName + "_" + group_name);

                        }

                        else
                        {
                            System.Threading.Thread.Sleep(1000 * 60 * 2);
                            goto again;
                        }
                        if (parsing_flag == "true")
                        {
                            ExecuteSQLQuery(ref conn, "UPDATE  parsing_completion_details SET status = 'COMPLETED', END_TIME = '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'  where circle= '" + _CircleName + "_" + group_name + "'");
                            parsing_flag = "false";
                        }
                        //Moving data from DCM(MYSQL DB) to AWE NSS(MSSQL DB)
                        //clsTransferToNSSFromCS.DCMDataMigration(_CircleName);//Added By Deepak on 14-03-2013
                    }

                }






            }
            catch (Exception ex)
            {
                if (!Directory.Exists(errorFolderPath))
                {
                    Directory.CreateDirectory(errorFolderPath);
                }
                File.AppendAllText(errorFolderPath + "\\" + _CircleName + "_" + group_name + "_error_log.txt", ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("StartParsing()", ErrorMsg, "", ref FileError);

                // change date 09-04-2014, check this on 11-04-2014
                ExecuteSQLQuery(ref conn, "UPDATE  parsing_completion_details SET status = 'COMPLETED', END_TIME = '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'  where circle= '" + _CircleName + "_" + group_name + "'");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void start()
        {
            try
            {
                string[] Command;
                if (_NEMAP == "MSS MAP")
                {
                    for (int i = 1; i <= 6; i++)    // previous debugging
                    {
                        if (i == 1)
                        {
                            Command = null;
                            _NEType = "MSS";
                            //Command = new string[] { }; // for debugging
                            Command = new string[] { "ZWVI", "ZQNI", "ZQRI", "ZRCI_PRINT_5", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZJGI", "ZEDO", "ZJNI", "ZWTI","ZNES", "ZOYI", "ZW7I", "ZWTI_P" }; //new ("ZNSI_NA0", "ZNSI_NA1", "ZNES", "ZOYI", "ZW7I", "ZWTI_P" added by AJAY)
                            //Command = new string[] { "ZWVI", "ZQNI", "ZQRI", "ZRCI_PRINT_5", "ZJGI", "ZNRI_NA0", "ZNRI_NA1", "ZEDO", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZJNI", "ZWTI" };
                            ReadFromFile(_CircleName, _NEMAP, _NEType, Command);
                            update_mss_extra_et(_CircleName);
                            _NEType = "";
                            //"ZWVI", "ZQNI", "ZQRI", "ZRCI_PRINT_5", "ZJGI", "ZNRI_NA0", "ZNRI_NA1", "ZEDO", "ZNEL", "ZNSI_NA0", "ZNSI_NA1" 
                        }
                        else if (i == 2)
                        {
                            Command = null;
                            _NEType = "MGW";
                            //Command = new string[] { "ZJVI", "ZJVI_2" }; // for debugging
                            Command = new string[] { "ZJVI", "ZR2O", "ZW7N", "ZRCI_PRINT_4", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZJVI_2", "ZUSI_NIWU", "ZUSI_IWS1E", "ZUSI_IWSEP", "ZNES" }; //new
                            //Command = new string[] { "ZJVI", "ZR2O", "ZW7N", "ZNRI_NA0", "ZNRI_NA1", "ZJVI_2", "ZRCI_PRINT_4", "ZUSI_NIWU", "ZUSI_IWS1E", "ZUSI_IWSEP", "ZNEL", "ZNSI_NA0", "ZNSI_NA1" };
                            ReadFromFile(_CircleName, _NEMAP, _NEType, Command);
                            _NEType = "";
                            //"ZJVI", "ZR2O", "ZW7N", "ZNRI_NA0", "ZNRI_NA1", "ZJVI_2", "ZRCI_PRINT_4", "ZUSI_NIWU", "ZUSI_IWS1E", "ZUSI_IWSEP", "ZNEL", "ZNSI_NA0", "ZNSI_NA1" 
                        }
                        else if (i == 3)
                        {
                            Command = null;
                            _NEType = "MGW_extra_et";
                            //Command = new string[] { }; //for debugging
                            Command = new string[] { "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZUSI_NIWU", "ZUSI_IWS1E", "ZUSI_IWSEP" };
                            ReadFromFile(_CircleName, _NEMAP, _NEType, Command);
                            _NEType = "";
                            //"ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZUSI_NIWU", "ZUSI_IWS1E"
                        }
                        else if (i == 4)
                        {
                            Command = null;
                            _NEType = "MGW_ATCA";
                            //New Code for ATCA..... 
                            Command = new string[] { "VMGW_MGW", "TDM_ATER", "LICENSE_TARGET", "SIGNALING_SS7_OWN", "TDMMGU", "SIGNALING_SS7_LINK", "TDM_CIRCUITGROUP", "SIGNALING_SS7_LINKSET", "SIGNALING_SS7_ROUTE", "SIGNALING_SCCP_SUBSYS_ALL", "SHOW_TDM_CIRCUITGROUP_ALL", "ParseData_show_has_functional_unit_info_show_mode_verbose", "ParseData_SHOWSIGNALLING_ASSOCIATION_ALL" };
                            //Command = new string[] { "VMGW_MGW", "SIGNALING_SS7_LINK", "SIGNALING_SS7_LINKSET" };
                            ReadFromFile(_CircleName, _NEMAP, _NEType, Command);
                            _NEType = "";

                        }
                        else if (i == 5)
                        {
                            // for sheet MSS-LINK-DATA

                            Command = null;
                            _NEType = "MSS";
                            _NEMAP = "MSS_LINKS";
                            //Command = new string[] { "ZNEL" };//for debugging
                            Command = new string[] { "ZQNI", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZNES" };  //"ZQNI", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1"
                            ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command);
                            _NEType = "";
                        }
                        else if (i == 6)
                        {
                            try
                            {
                                if (!("mss_mgw_map_" + _CircleName + "_" + group_name).Contains("HLR_MSC"))
                                {
                                    ExecuteSQLQuery(ref conn, "delete FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where  MSS_SPC is null and VMGW_ID is null and VMGW is null and VMGW_CTRL_SIGU is null and VMGW_SOURCE_IP is null and VMGW_DEST_IP is null and CGR is null and NCGR is null and CGR_SPC is null and GENERIC_NAME is null and CGR_NET is null and CGR_TYPE is null and CGR_UPART is null and TERM_ID is null and CIC is null and BSC_NAME is null and BSC_STATE is null and BSC_NUMBER is null and LAC is null and MGW_NAME is null and MGW_Source_Map_IP is null and MGW_C_NO is null and MGW_SPC is null and MGW_VMGW_ID is null and MGW_VMGW is null and MGW_VMGW_CTRL_ISU is null and MGW_CGR is null and MGW_NCGR is null and UNIT is null and STER is null and ETGR_VETGR is null and ET is null and APCM is null and LINK is null and LINKSET is null and SLC is null EXTRA_ET_FLAG is null and SPC_TYPE is null and ATCA_FLAG is null and ROUTER_ID is null and PAR_SET is null and PRIO is null and SP_TYPE is null and SS7_STAND is null and SUB_FIELD_INFO_COUNT is null and SUB_FIELD_INFO_LENGTHS is null and STATE is null and SUB_FIELD_INFO_BIT is null and NBCRCT is null and REGISTRATION_STATUS is null and VMGW_SECONDARY_IP is null and SCCP_SUBSYS_STATUS is null and SCCP_IDENTIFIER is null and SCCP_SUBSYS is null and TF is null and EXTERN_PCM_TSL is null and INT_PCM_TSL is null and BIT_RATE is null and ASSOCIATION_SET is null and MGW_NBCRCT is null and LINK_SPC is null and LINK_NODE is null and LINK_ADMIN_STATE is null and LINK_RATE is null and LINK_STATUS is null and HCLBRG is null and LINK_DEST_POINTCODE is null and circle = '" + _CircleName + "' and VENDOR = '" + _vendor + "';");
                                    update_Generic_name();
                                }
                            }
                            catch { }
                        }
                    }
                }
                else if (_NEMAP == "MSC MAP")
                {

                    for (int i = 1; i <= 3; i++)
                    {
                        if (i == 1)
                        {
                            Command = null;
                            _NEType = "MSC";
                            Command = new string[] { "ZWVI", "ZQNI", "ZQRI", "ZRCI", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZEDO", "ZNES" };
                            // "ZWVI", "ZQNI", "ZQRI", "ZRCI", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZEDO"
                            ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command);
                            _NEType = "";
                        }

                        else if (i == 2)
                        {
                            _NEType = "MSC_EXTRA_ET";
                            Command = new string[] { "ZNSI_NA0", "ZNSI_NA1" };
                            ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command);
                            update_msc_extra_et(_CircleName.Trim());
                            _NEType = "";
                        }
                        else if (i == 3)
                        {
                            ExecuteSQLQuery(ref conn, "delete from msc_" + _CircleName + " where MSS_SPC is null and  CGR is null and  NCGR is null and  CGR_SPC is null and  GENERIC_NAME is null and  CGR_NET is null and  CGR_TYPE is null and  CGR_UPART is null and  CIC is null and  BSC_NAME is null and  BSC_STATE is null and  BSC_NUMBER is null and  LAC is null and  NE_Type2 is null and  ET is null and  LINK is null and  LINKSET is null and  SLC is null and  SPC_TYPE is null ;");
                            update_Generic_name();
                        }
                    }
                }
                else if (_NEMAP == "HLR MAP")
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 1)
                        {
                            Command = null;
                            _NEType = "HLR";
                            Command = new string[] { "ZQNI", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1", "ZNES" };  //"ZQNI", "ZNEL", "ZNSI_NA0", "ZNSI_NA1", "ZNRI_NA0", "ZNRI_NA1"
                            ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command);
                            _NEType = "";
                        }
                        else if (i == 2)
                        {
                            ExecuteSQLQuery(ref conn, "delete  FROM hlr_" + _CircleName + " where  HLR_SPC is null and CGR_SPC is null and CGR_NET is null and NODE_Type is null and ET is null and LINK is null and LINKSET is null and  SLC is null;");
                            update_Generic_name();
                        }
                    }
                }
                else if (_NEMAP == "ASSOCIATION INFO")
                {
                    string tempGroup = group_name;

                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 1)
                        {
                            Command = null;
                            _NEType = "MSS";
                            //Command = new string[] { "ZRCI", "ZNSI_NA1", "ZNSI_NA0", "ZOYI" }; //new
                            Command = new string[] { "ZNSI_NA1", "ZNSI_NA0", "ZOYI", "ZRCI" };     // "ZNSI_NA1", "ZNSI_NA0", "ZOYI", "ZRCI"                       

                            string[] group = { "group_1", "group_2", "group_3", "Group_4", "group_5" };


                            for (int y = 0; y < group.Length; y++)
                            {

                                group_name = group[y].Replace("_", "");
                                ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command, group[y].ToString());

                            }
                            _NEType = "";

                        }
                        if (i == 2)
                        {
                            Command = null;
                            _NEType = "MGW";
                            //Command = new string[] { "ZRCI", "ZNSI_NA1", "ZNSI_NA0", "ZOYI", "ZNES" }; //new
                            Command = new string[] { "ZNSI_NA1", "ZNSI_NA0", "ZOYI", "ZRCI" };     //"ZNSI_NA1", "ZNSI_NA0", "ZOYI", "ZRCI"
                            string[] group = { "group_1", "group_2", "group_3", "Group_4", "group_5" };

                            for (int y = 0; y < group.Length; y++)
                            {
                                group_name = group[y].Replace("_", "");
                                ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command, group[y].ToString());

                            }
                            _NEType = "";
                        }
                    }
                    group_name = tempGroup;
                }
                else if (_NEMAP == "DESTINATION INFO")
                {
                    string tempGroup = group_name;

                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 1)
                        {
                            Command = null;
                            _NEType = "MSS";
                            Command = new string[] { "ZRRI", "ZRNI", "ZRIL" };  //"ZRRI", "ZRNI", "ZRIL"
                            string[] group = { "group_1", "group_2", "group_3", "Group_4", "group_5" };

                            for (int y = 0; y < group.Length; y++)
                            {
                                group_name = group[y].Replace("_", "");
                                ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command, group[y].ToString());

                            }
                            _NEType = "";
                        }
                        if (i == 2)
                        {
                            Command = null;
                            _NEType = "MSC";
                            Command = new string[] { "ZRRI", "ZRNI", "ZRIL" };  //"ZRRI", "ZRNI", "ZRIL" 
                            group_name = "MSC";
                            ReadFromFile(_CircleName.Trim(), _NEMAP, _NEType, Command);
                            _NEType = "";
                        }
                    }
                    group_name = tempGroup;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ErrorMsg + "[" + ex.Message.ToString() + "]";
                oExceptionLog.WriteExceptionErrorToFile("Start()", ErrorMsg, "", ref FileError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CircleName"></param>
        /// <param name="NE_MAP"></param>
        /// <param name="NE_Type"></param>
        /// <param name="CommandList"></param>
        /// <param name="group"></param>
        public void ReadFromFile(string CircleName, string NE_MAP, string NE_Type, string[] CommandList, string group)
        {
            try
            {
                string file_name = string.Empty;
                string Data = string.Empty;
                string sCommandFolderPath = string.Empty;
                string sFile = string.Empty;
                string sCommandName = string.Empty;
                string[] s;
                string sPath = string.Empty;
                string temp = string.Empty;
                int i = 0;
                StreamReader sr;
                string[] sName;
                if (NE_Type == "MGW_extra_et")
                {
                    NE_Type = "MGW";
                }
                if (NE_Type == "MSC_EXTRA_ET")
                {
                    NE_Type = "MSC";
                }

                if (NE_Type == "MGW" || NE_Type == "MSS")
                {
                    sPath = @folderPath + System.DateTime.Now.ToString("dd_MM_yyyy") + "/" + _CircleName + "/" + NE_Type + "/" + group;
                }

                else
                {
                    sPath = @folderPath + System.DateTime.Now.ToString("dd_MM_yyyy") + "/" + _CircleName + "/" + NE_Type;
                }

                if (Directory.Exists(sPath))
                {
                    for (int j = 0; j <= CommandList.Length - 1; j++)
                    {
                        try
                        {
                            sFile = CommandList[j].ToString();
                            if (sFile.Contains("ZRCI"))
                            {
                                sFile = "ZRCI";
                            }
                            if (sFile.Trim() == "ZJVI_2" || sFile.Trim() == "ZJVI")
                            {
                                sCommandFolderPath = sPath + "/" + "ZJVI";
                            }
                            else if (sFile.Trim() == "ZR2O_update")
                            {
                                sCommandFolderPath = sPath + "/" + "ZR2O";
                            }
                            else
                            {

                                sCommandFolderPath = sPath + "/" + sFile;
                            }
                            sCommandName = sFile;

                        }
                        catch (Exception ex)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ReadFromFile_sub1()", ErrorMsg, "", ref FileError);
                            continue;

                        }

                        try
                        {
                            foreach (string fNEName in Directory.GetFiles(sCommandFolderPath))
                            {

                                string cmnd = string.Empty;
                                cmnd = sCommandName;
                                if (cmnd.Contains("ZJVI_2") || cmnd.Contains("ZJVI_1"))
                                {
                                    cmnd = "ZJVI";
                                }
                                temp = "";
                                sr = new StreamReader(fNEName);
                                file_name = fNEName.Trim().ToString();
                                file_name = Regex.Replace(file_name, "/", "$");
                                s = Regex.Split(file_name, cmnd);

                                _ne_name = s[1].ToString().Replace("\\", "$");
                                sName = Regex.Split(_ne_name.ToString(), ".txt");
                                //s = null;
                                //s = Regex.Split(sName[1].ToString().Trim(), ".txt");
                                _ne_name = sName[0].ToString().TrimStart('$');
                                Data = sr.ReadToEnd();
                                ParsefromTextFile(Data, CircleName, NE_MAP, _NEType, sCommandName);
                                sr.Close();
                            }

                        }

                        catch (Exception ex)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ReadFromFile_sub2()", ErrorMsg, "", ref FileError);
                            continue;
                            //File.WriteAllText(@"E:\CS_DCM\DCM_RAW\" + DateTime.Now.ToString("dd_MM_yyyy") + @"\ParsingError.txt", "Circle : " + CircleName + " Element : " + NE_MAP + " Type : " + NE_Type + "\r\n" + ex + "\r\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ReadFromFile()", ErrorMsg, "", ref FileError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CircleName"></param>
        /// <param name="NE_MAP"></param>
        /// <param name="NE_Type"></param>
        /// <param name="CommandList"></param>
        public void ReadFromFile(string CircleName, string NE_MAP, string NE_Type, string[] CommandList)
        {
            //  group_name = "group_2";
            try
            {
                string group_name1 = group_name;

                group_name1 = group_name1.Insert(group_name1.Length - 1, "_");

                string file_name = string.Empty;
                string Data = string.Empty;
                string sCommandFolderPath = string.Empty;
                string sFile = string.Empty;
                string sCommandName = string.Empty;
                string[] s;
                string sPath = string.Empty;
                string temp = string.Empty;
                int i = 0;
                StreamReader sr;
                string[] sName;
                if (NE_Type == "MGW_extra_et")
                {
                    NE_Type = "MGW";
                }
                if (NE_Type == "MSC_EXTRA_ET")
                {
                    NE_Type = "MSC";
                }

                if (NE_Type == "MGW" || NE_Type == "MSS" || NE_Type == "MGW_ATCA")
                {
                    sPath = (NE_Type == "MGW_ATCA" ? @folderPath + System.DateTime.Now.ToString("dd_MM_yyyy") + "/" + _CircleName + "/ATCA/" + NE_Type + "/" + group_name1 : @folderPath + System.DateTime.Now.ToString("dd_MM_yyyy") + "/" + _CircleName + "/" + NE_Type + "/" + group_name1);
                    //sPath = "E:/CS_DCM/" + "DCM_RAW" + "/" + System.DateTime.Now.ToString("dd_MM_yyyy") + "/" + _CircleName + "/" + NE_Type + "/" + group_name1;
                }
                // what for association and destination 
                else
                {
                    sPath = @folderPath + System.DateTime.Now.ToString("dd_MM_yyyy") + "/" + _CircleName + "/" + NE_Type;
                }

                if (Directory.Exists(sPath))
                {
                    for (int j = 0; j <= CommandList.Length - 1; j++)
                    {
                        try
                        {
                            sFile = CommandList[j].ToString();
                            if (sFile.Contains("ZRCI"))
                            {
                                sFile = "ZRCI";
                            }
                            if (sFile.Trim() == "ZJVI_2" || sFile.Trim() == "ZJVI")
                            {
                                sCommandFolderPath = sPath + "/" + "ZJVI";
                            }
                            else if (sFile.Trim() == "ZR2O_update")
                            {
                                sCommandFolderPath = sPath + "/" + "ZR2O";
                            }
                            else
                            {

                                sCommandFolderPath = sPath + "/" + sFile;
                            }
                            sCommandName = sFile;

                        }
                        catch (Exception ex)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ReadFromFile_ATCA_SUB1()", ErrorMsg, "", ref FileError);
                            continue;

                        }

                        try
                        {
                            foreach (string fNEName in Directory.GetFiles(sCommandFolderPath))
                            {

                                string cmnd = string.Empty;
                                cmnd = sCommandName;
                                if (cmnd.Contains("ZJVI_2") || cmnd.Contains("ZJVI_1"))
                                {
                                    cmnd = "ZJVI";
                                }
                                temp = "";
                                sr = new StreamReader(fNEName);
                                file_name = fNEName.Trim().ToString();
                                file_name = Regex.Replace(file_name, "/", "$");
                                s = Regex.Split(file_name, cmnd);

                                _ne_name = s[1].ToString().Replace("\\", "$");
                                sName = Regex.Split(_ne_name.ToString(), ".txt");
                                //s = null;
                                //s = Regex.Split(sName[1].ToString().Trim(), ".txt");
                                _ne_name = sName[0].ToString().TrimStart('$');
                                Data = sr.ReadToEnd();
                                //Added by Deepak on 20-06-2014
                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '' where Circle = '" + CircleName + "' and Element='" + _ne_name + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);


                                ParsefromTextFile(Data, CircleName, NE_MAP, _NEType, sCommandName);

                                sr.Close();
                            }

                        }

                        catch (Exception ex)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ReadFromFile_ATCA_SUB2()", ErrorMsg, "", ref FileError);
                            continue;
                            //File.WriteAllText(@"E:\CS_DCM\DCM_RAW\" + DateTime.Now.ToString("dd_MM_yyyy") + @"\ParsingError.txt", "Circle : " + CircleName + " Element : " + NE_MAP + " Type : " + NE_Type + "\r\n" + ex + "\r\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ReadFromFile_ATCA()", ErrorMsg, "", ref FileError);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtData"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_MAP"></param>
        /// <param name="NE_Type"></param>
        /// <param name="CommandName"></param>
        public void ParsefromTextFile(string txtData, string CircleName, string NE_MAP, string NE_Type, string CommandName)
        {
            try
            {
                switch (NE_MAP)
                {
                    #region[MSS_LINKS NE_MAP]
                    case "MSS_LINKS":
                        NE_Type = "MSS_LINKS";
                        if (NE_Type == "MSS_LINKS")
                        {
                            if (CommandName == "ZQNI")
                            {
                                Update_MSS_LINK_Details();
                                string[] GIGBtxt_ZQNI = Regex.Split(txtData, "\n");
                                ParseData_ZQNI(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNRI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNRI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ExecuteSQLQuery(ref conn, "delete FROM mss_link_" + CircleName + "_" + group_name + " where  CGR_SPC is null and GENERIC_NAME is null and CGR_NET is null and ET is null and LINK is null and LINKSET is null and SLC is null");
                                ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                            }
                            if (CommandName == "ZNEL")
                            {
                                string[] GIGBtxt_ZNEL = Regex.Split(txtData, "\n");
                                ParseData_ZNEL(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                            }
                        }
                        break;
                    #endregion

                    #region[MSS MAP NE_MAP]
                    case "MSS MAP":
                        if (NE_Type == "MSS")
                        {
                            if (CommandName == "ZWVI")
                            {
                                ParseData_ZWVI(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZW7I") //Added by AJAY
                            {
                                ParseData_ZW7I_LICENCECAPACITY(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZW7I") //Added by AJAY
                            {
                                ParseData_ZW7I_LICENCEEXCEEDED(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZWTI_P") //ADDED by AJAY
                            {
                                ParseData_ZWTI_P(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZQNI")
                            {
                                ParseData_ZQNI(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZQRI")
                            {
                                ParseData_ZQRI(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZRCI")
                            {
                                ParseData_ZRCI_PRINT_5(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZJGI")
                            {
                                ParseData_ZJGI(txtData, CircleName, NE_Type);
                            }

                            //if (CommandName == "ZJVI") //Added by AJAY
                            //{
                            //    ParseData_ZJVI_1(txtData, CircleName, NE_Type);                                
                            //}

                            //if (CommandName == "ZJVI_2") //Added by AJAY
                            //{
                            //    ParseData_ZJVI_2(txtData, CircleName, NE_Type);
                            //}
                            if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNRI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNRI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                            }
                            if (CommandName == "ZEDO")
                            {
                                ParseData_ZEDO(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNEL")
                            {
                                ParseData_mss_ZNEL(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_mss_ZNSI(txtData, CircleName, NE_Type, NA);
                            }

                            if (CommandName == "ZOYI") //Added by AJAY
                            {
                                association_ZOYI(txtData, CircleName);
                            }

                            if (CommandName == "ZJNI")
                            {
                                ParseData_ZJNI_FQDN(txtData, CircleName, _NEType);
                            }
                            //code added on 24-08-2018 by Rahul
                            if (CommandName == "ZWTI")
                            {
                                ParseData_mss_ZWTI(txtData, CircleName, NE_Type);
                            }
                        }
                        else if (NE_Type == "MGW_extra_et")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                MGW_EXTRA_ET_ZNSI(txtData, CircleName, "MGW", NA);
                            }

                            if (CommandName == "ZNRI_NA0")
                            {
                                string[] GIGBtxt_ZNRI_NA0 = Regex.Split(txtData, "\n");
                                ZNRI_MGW_EXTRA_ET(GIGBtxt_ZNRI_NA0, CircleName, "MGW");
                            }

                            if (CommandName == "ZNRI_NA1")
                            {
                                string[] GIGBtxt_ZNRI_NA1 = Regex.Split(txtData, "\n");
                                ZNRI_MGW_EXTRA_ET(GIGBtxt_ZNRI_NA1, CircleName, "MGW");
                            }

                            if (CommandName == "ZW7N")
                            {
                                string[] GIGBtxt_ZW7N = Regex.Split(txtData, "\n");
                                ParseData_ZW7N_Extra_et(GIGBtxt_ZW7N, CircleName, "MGW");
                            }

                            if (CommandName == "ZUSI_IWS1E")
                            {
                                ZUSI_IWS1E_EXTRA_ET(txtData, CircleName, "MGW");
                            }

                            if (CommandName == "ZUSI_IWSEP")
                            {
                                ZUSI_IWS1E_EXTRA_ET(txtData, CircleName, "MGW");
                            }

                            if (CommandName == "ZUSI_NIWU")
                            {
                                ZUSI_NIWU_EXTRA_ET(txtData, CircleName, "MGW");
                            }
                        }
                        else if (NE_Type == "MGW")
                        {
                            if (CommandName == "ZJVI")
                            {

                                ParseData_ZJVI_1(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZWFI_PS") //ADDED by AJAY
                            {
                                ParseData_ZWFI_PS(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZR2O")
                            {
                                ParseData_ZR2O(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZW7I") //Added by AJAY
                            {
                                ParseData_ZW7I_LICENCECAPACITY(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZW7I") //Added by AJAY
                            {
                                ParseData_ZW7I_LICENCEEXCEEDED(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZW7N")
                            {
                                string[] GIGBtxt_ZW7N = Regex.Split(txtData, "\n");
                                ParseData_ZW7N(GIGBtxt_ZW7N, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNRI_NA0")
                            {

                                string[] GIGBtxt_ZNRI_NA0 = Regex.Split(txtData, "\n");

                                ParseData_ZNRI_MGW(GIGBtxt_ZNRI_NA0, CircleName, NE_Type, "NA0");

                            }

                            if (CommandName == "ZNRI_NA1")
                            {
                                string[] GIGBtxt_ZNRI_NA1 = Regex.Split(txtData, "\n");
                                ParseData_ZNRI_MGW(GIGBtxt_ZNRI_NA1, CircleName, NE_Type, "NA1");
                            }

                            if (CommandName == "ZJVI_2")
                            {
                                ParseData_ZJVI_2(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZRCI")
                            {
                                //ParseData_ZRCI_PRINT_4(txtData, CircleName, NE_Type); // As of now function is not required

                                ParseData_ZRCI_PRINT_2(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZUSI_NIWU")
                            {
                                ParseData_ZUSI_NIWU(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZUSI_IWS1E")
                            {
                                ParseData_ZUSI_IWS1E(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZNEL")
                            {
                                ParseData_ZNEL(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZUSI_IWSEP")
                            {
                                ParseData_ZUSI_IWS1E(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                            }
                            //if (CommandName == "ZRC_PRINT_2")
                            //{

                            //    ParseData_ZRCI_PRINT_2(txtData, CircleName, NE_Type);
                            //}

                        }
                        else if (NE_Type == "MGW_ATCA")
                        {
                            // For ATCA                         
                            if (CommandName == "VMGW_MGW")
                            {
                                ParseData_VMGW_MGW(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "TDM_ATER")
                            {
                                ParseData_TDM_ATER(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "LICENSE_TARGET")
                            {
                                ParseData_LICENSE_TARGET(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "SIGNALING_SS7_OWN")
                            {
                                ParseData_SIGNALING_SS7_OWN(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "TDMMGU")
                            {
                                //Parsing logic not shared by operation team.
                            }
                            else if (CommandName == "SIGNALING_SS7_LINK")
                            {
                                ParseData_SIGNALING_SS7_LINK(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "TDM_CIRCUITGROUP")
                            {
                                ParseData_TDM_CIRCUITGROUP(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "SIGNALING_SS7_LINKSET") //Extra ET
                            {
                                ParseData_SIGNALING_SS7_LINK(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "SIGNALING_SS7_ROUTE")
                            {
                                ParseData_SIGNALING_SS7_ROUTE(txtData, CircleName, NE_Type);
                            }

                            else if (CommandName == "show_license_feature_mgmt_all") // ADDED by AJAY
                            {
                                ParseData_show_license_feature_mgmt_all(txtData, CircleName, NE_Type);
                            }

                            else if (CommandName == "SIGNALLING_ASSOCIATION_ALL") // ADDED by AJAY

                            {
                                ParseData_SIGNALLING_ASSOCIATION_ALL(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "SIGNALING_SCCP_SUBSYS_ALL")
                            {
                                ParseData_SIGNALING_SCCP_SUBSYS_ALL(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "SHOW_TDM_CIRCUITGROUP_ALL")
                            {
                                ParseData_SHOW_TDM_CIRCUITGROUP_ALL(txtData, CircleName, NE_Type);
                            }
                            // END ATCA COMMANDs  
                        }
                        break;
                    #endregion

                    #region[MSC MAP NE_MAP]
                    case "MSC MAP":
                        if (NE_Type == "MSC_EXTRA_ET")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_ZNSI_MSC_extra_et(txtData, CircleName, NE_Type, NA);
                            }
                        }
                        else
                        {
                            if (CommandName == "ZWVI")
                            {
                                ParseData_ZWVI(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZQNI")
                            {
                                ParseData_ZQNI(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZQRI")
                            {
                                ParseData_ZQRI(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNRI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNRI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                            }
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                            }
                            if (CommandName == "ZNEL")
                            {
                                ParseData_ZNEL(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZRCI")
                            {
                                ParseData_MSC_ZRCI_PRINT_5(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZEDO")
                            {
                                ParseData_ZEDO(txtData, CircleName, NE_Type);
                            }
                        }
                        break;
                    #endregion

                    #region[HLR MAP NE_MAP]
                    case "HLR MAP":
                        if (CommandName == "ZQNI")
                        {
                            UpdateHLRDetails();
                            string[] GIGBtxt_ZQNI = Regex.Split(txtData, "\n");
                            ParseData_ZQNI(txtData, CircleName, NE_Type);
                        }
                        if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                        {
                            string NA = string.Empty;
                            if (CommandName == "ZNRI_NA0")
                            {
                                NA = "NA0";
                            }
                            else if (CommandName == "ZNRI_NA1")
                            {
                                NA = "NA1";
                            }
                            ExecuteSQLQuery(ref conn, "delete FROM hlr_" + CircleName + " where  CGR_SPC is null and GENERIC_NAME is null and CGR_NET is null and ET is null and LINK is null and LINKSET is null and SLC is null");
                            ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                        }
                        if (CommandName == "ZNEL")
                        {
                            string[] GIGBtxt_ZNEL = Regex.Split(txtData, "\n");
                            ParseData_ZNEL(txtData, CircleName, NE_Type);
                        }
                        if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                        {
                            string NA = string.Empty;
                            if (CommandName == "ZNSI_NA0")
                            {
                                NA = "NA0";
                            }
                            else if (CommandName == "ZNSI_NA1")
                            {
                                NA = "NA1";
                            }
                            ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                        }
                        break;
                    #endregion

                    #region[DESTINATION INFO NE_MAP]
                    case "DESTINATION INFO":
                        if (_NEType == "MSS" || _NEType == "MSC")
                        {
                            if (CommandName == "ZRRI")
                            {
                                parse_ZRRI(txtData, _CircleName);
                            }

                            if (CommandName == "ZRNI")
                            {
                                parse_ZRNI(txtData, _CircleName);
                            }

                            if (CommandName == "ZRIL")
                            {
                                parse_ZRIL(txtData, _CircleName);
                            }
                        }
                        break;
                    #endregion

                    #region[ASSOCIATION INFO NE_MAP]
                    case "ASSOCIATION INFO":
                        if (_NEType == "MSS")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                association_ZNSI_NA1(txtData, CircleName, NA);
                            }
                            if (CommandName == "ZOYI")
                            {
                                association_ZOYI(txtData, _CircleName);
                                ExecuteSQLQuery(ref conn, "delete from association_info_" + CircleName + " where circle = '" + _CircleName + "' and node = '" + _ne_name + "' and `SOURCE_IP_1` IS NULL ");
                            }

                            if (CommandName == "ZRCI")
                            {
                                ASSOCIATION_ZRCI(txtData, _CircleName);
                            }
                        }
                        else if (_NEType == "MGW")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                association_ZNSI_NA1(txtData, _CircleName, NA);
                            }
                            else if (CommandName == "ZRCI")
                            {
                                string[] GIGBtxt_ZRCI_PRINT_4 = Regex.Split(txtData, "\n");
                                ASSOCIATION_ZRCI(txtData, _CircleName);
                            }
                            else if (CommandName == "ZOYI")
                            {
                                association_ZOYI(txtData, _CircleName);
                                ExecuteSQLQuery(ref conn, "delete from association_info_" + CircleName + " where circle = '" + _CircleName + "' and node = '" + _ne_name + "' and `SOURCE_IP_1` IS NULL ");
                            }
                        }
                        break;
                        #endregion
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseFromTextFile()", ErrorMsg, "", ref FileError);
                //File.WriteAllText(@"E:\CS_DCM\DCM_RAW\" + @"PARSING_ERROR\" + DateTime.Now.ToString("dd_MM_yyyy") + @"\ParsingError.txt", "Circle : " + _CircleName + " Element : " + _ne_name.Trim() + " Type : " + NE_Type + "\r\n" + ex + "\r\n");
            }
        }

        // MGW Parsing Functions
        #region[MGW Parsing Functions]
        
        /// <summary>
        /// 
        /// </summary>
        public void UpdateHLRDetails()
        {
            try
            {
                DataTable dt = new DataTable();
                string SqlStr = string.Empty;
                string ELEMENT_IP = string.Empty;
                SqlStr = "Select NE_IP from NE_Master where VENDOR='" + _vendor + "' and CIRCLE='" + _CircleName + "' and NE_NAME='" + _ne_name + "' and Node_TYPE='" + _NEType + "'";
                SQLQuery(ref conn, ref dt, SqlStr);
                ELEMENT_IP = dt.Rows[0]["NE_IP"].ToString();

                SqlStr = "Insert into hlr_" + _CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, HLR, ELEMENT_IP) Values('" + _vendor + "','" + _CircleName + "','" + _ne_name + "','" + _NEType + "','" + _ne_name + "','" + ELEMENT_IP + "')";
                ExecuteSQLQuery(ref conn, SqlStr);
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("UpdateHLRDetails()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Update_MSS_LINK_Details()
        {
            try
            {
                DataTable dt = new DataTable();
                string SqlStr = string.Empty;
                string ELEMENT_IP = string.Empty;
                SqlStr = "select node_type from NE_Master where VENDOR='" + _vendor + "' and CIRCLE='" + _CircleName + "' and NE_NAME='" + _ne_name + "'";
                SQLQuery(ref conn, ref dt, SqlStr);
                string type_of_node = dt.Rows[0]["node_type"].ToString();
                dt.Clear();
                SqlStr = "Select NE_IP from NE_Master where VENDOR='" + _vendor + "' and CIRCLE='" + _CircleName + "' and NE_NAME='" + _ne_name + "' and Node_TYPE='" + type_of_node + "'";
                SQLQuery(ref conn, ref dt, SqlStr);
                ELEMENT_IP = dt.Rows[0]["NE_IP"].ToString();

                SqlStr = "Insert into mss_link_" + _CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MSS, ELEMENT_IP) Values('" + _vendor + "','" + _CircleName + "','" + _ne_name + "','MSS_LINKS','" + _ne_name + "','" + ELEMENT_IP + "')";
                ExecuteSQLQuery(ref conn, SqlStr);
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("Update_MSS_LINK_Details()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ZR20_update_ET(string output, string CircleName, string NE_Type)
        {

            string query = string.Empty;
            string et = string.Empty;
            string apcm = string.Empty;
            string value = string.Empty;
            string[] data;
            string previous_apcm = string.Empty;
            string[] temp;
            string[] temp1;

            try
            {
                temp = Regex.Split(output.Trim(), "APCM - TSL");

                for (int i = 1; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (output.Contains("APCM - TSL") && output.Trim().Contains("COMMAND EXECUTED"))
                        {
                            data = Regex.Split(temp[i].Trim(), "\r\n");

                            for (int j = 0; j < data.Length - 1; j++)
                            {
                                if (data[j].Trim().Contains("COMMAND EXECUTED"))
                                {
                                    break;
                                }

                                if (data[j].Trim() != "" && !data[j].Trim().Contains("APCM NOT FOUND") && !data[j].Trim().Contains("COMMAND EXECUTED") && !data[j].Trim().Contains("APCM") && !data[j].Trim().Contains("ET"))
                                {

                                    value = Regex.Replace(data[j].Trim(), " {2,}", "~");
                                    temp1 = Regex.Split(value.Trim(), "~");
                                    apcm = temp1[0].TrimEnd('-');
                                    if (apcm.Contains("-"))
                                    {
                                        apcm = apcm.Substring(0, apcm.IndexOf('-'));
                                    }
                                    et = temp1[2].Trim();
                                    apcm = apcm.Trim();


                                    if (apcm != "" && et != "")
                                    {
                                        query = "update mss_mgw_map_" + CircleName + "_" + group_name + " set et = '" + et + "' where apcm = '" + apcm.Trim() + "' and  Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "'";
                                        ExecuteSQLQuery(ref conn, query);
                                        apcm = "";
                                        et = "";
                                        j = data.Length;

                                    }

                                }



                            }


                        }

                        else
                        {
                            break;
                        }


                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function zr2o_update_et ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ZR20_update_ET_SUBWithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ZR20_update_ET()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        /// <param name="net"></param>
        public void ParseData_ZNSI(string output, string CircleName, string NE_Type, string net) 
        {
            DataTable dt = new DataTable();
            string temp_data = string.Empty;
            string slc = string.Empty;
            string[] value;
            string[] temp;
            string query = string.Empty; ;
            string query1 = string.Empty;
            string link = string.Empty;
            string cgr_spc = string.Empty;
            string cgr_net = string.Empty;
            string linkset = string.Empty;
            string SPCCDODEHD = string.Empty;
            string LSSTATE = string.Empty;
            string ASSOCSTATE = string.Empty;
            try
            {
                string[] cmnd_output = Regex.Split(output.Trim(), "COMMAND EXECUTED");
                string data = cmnd_output[0].Trim();
                //if (data.Contains("SLC"))
                if (data.Contains("LS STATE  LINK SLC"))
                {
                    cmnd_output = Regex.Split(data, "LS STATE  LINK SLC");
                    if (cmnd_output.Length == 1)
                    {
                        data = cmnd_output[0].Trim().ToString();
                    }
                    else
                    {
                        data = cmnd_output[1].Trim().ToString();
                    }
                }
                else if (data.Contains("ASSOCIATION SET"))
                {
                    cmnd_output = Regex.Split(data, "LS    IP");
                    if (cmnd_output.Length == 1)
                    {
                        data = cmnd_output[0].Trim().ToString();
                    }
                    else
                    {
                        data = cmnd_output[1].Trim().ToString();
                    }
                }
                else if (data.Contains("REMOTE LINKS"))
                {
                    cmnd_output = Regex.Split(data, "SLC");
                    if (cmnd_output.Length == 1)
                    {
                        data = cmnd_output[0].Trim().ToString();
                    }
                    else
                    {
                        data = cmnd_output[1].Trim().ToString();
                    }
                }                
                temp = Regex.Split(data, "\r\n");
                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    if (i >= temp.Length - 1)
                    {
                        break;
                    }
                    try
                    {
                        if (!temp[i].Trim().Contains("----------") && temp[i] != "" && !temp[i].Trim().Contains("LS    IP") && !temp[i].Trim().Contains("NET SP CODE H/D        LINK SET              ASSOCIATION SET       STATE LINK") && !temp[i].Trim().Contains("LINK TEST NOT ALLOWED"))
                        {
                            while (temp[i] != "COMMAND EXECUTED" && i <= temp.Length - 1 && temp[i].Trim() != "")
                            {
                                if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                {
                                    temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                    value = Regex.Split(temp_data.ToString().Trim(), "~");

                                    if (value.Length > 3)
                                    {
                                        cgr_net = value[0].ToString();
                                        cgr_spc = value[1].ToString();
                                        linkset = value[2].ToString();
                                        link = value[4].ToString();
                                        slc = value[5].ToString();

                                    }
                                    if (value.Length == 2)
                                    {
                                        slc = value[1].ToString();
                                        link = value[0].ToString();
                                    }
                                    if (value.Length == 3 && !temp_data.Trim().Contains("REMOTE LINKS:~NET/SP~SLC"))
                                    {
                                        cgr_net = value[0].ToString();
                                        cgr_spc = value[1].ToString();                                        
                                        slc = value[2].ToString();

                                    }
                                    if ((slc != "" && linkset != "" && cgr_net != "" && cgr_spc != "" && link != "") || (slc != "" && cgr_net != "" && cgr_spc != ""))
                                    {
                                        query = "select distinct CGR_SPC,CGR_NET,LINK,LINKSET,SLC from mss_mgw_map_" + CircleName + "_" + group_name + " where CGR_SPC = '" + cgr_spc.Trim() + "' AND CGR_NET='" + cgr_net + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' and link is null and linkset is null";
                                        SQLQuery(ref conn, ref dt, query);                                 
                                        if (dt.Rows.Count >= 1)
                                        {
                                            query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set SLC = '" + slc.Trim() + "', LINK = '" + link.Trim() + "', LINKSET = '" + linkset.Trim() + "' where CGR_SPC = '" + cgr_spc.Trim() + "' and CGR_NET='" + cgr_net + "' AND MGW_NAME = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' AND NODE_TYPE = '" + _NEType + "' and link is null and linkset is null";

                                            //query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set SLC = '" + slc.Trim() + "', CGR_NET = '" + cgr_net.Trim() + "', cgr_spc = '" + cgr_spc.Trim() + "', LINK = '" + link.Trim() + "' where LINKSET = '" + linkset.Trim() + "' AND ne_name = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and EXTRA_ET_FLAG = '1' ";
                                            ExecuteSQLQuery(ref conn, query1);                                            
                                        }
                                        else
                                        {
                                            query = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,ifnull(NE_NAME,'') as NE_NAME ,ifnull(MCC,'') as MCC,ifnull(MNC,'') as MNC,ifnull(MSS,'') as MSS,ifnull(ELEMENT_IP,'') as ELEMENT_IP,ifnull(CGR_NET,'') as CGR_NET,ifnull(MGW_C_NO,'') as MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + net + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                            //query = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                            SQLQuery(ref conn, ref dt, query);
                                            if (dt.Rows.Count >= 1)
                                            {
                                                query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_SPC,CGR_NET,LINK,LINKSET,SLC) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + cgr_spc.Trim() + "', '" + cgr_net.Trim() + "', '" + link.Trim() + "', '" + linkset.Trim() + "', '" + slc.Trim() + "')";
                                                ExecuteSQLQuery(ref conn, query1);                                                
                                            }
                                            else if (dt.Rows.Count <= 0)
                                            {
                                                query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,CGR_SPC,CGR_NET,LINK,LINKSET,SLC) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + cgr_spc.Trim() + "', '" + cgr_net.Trim() + "', '" + link.Trim() + "', '" + linkset.Trim() + "', '" + slc.Trim() + "')";
                                                ExecuteSQLQuery(ref conn, query1);                                                
                                            }
                                            //query1 = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (CGR_SPC,CGR_NET,LINK,LINKSET,SLC) values (' " + cgr_spc.Trim() + " ', ' " + cgr_net.Trim() + " ', ' " + link.Trim() + " ', ' " + linkset.Trim() + " ', ' " + slc.Trim() + " ') ";                                            
                                        }
                                    }
                                    i = i + 1;
                                    if (i > temp.Length - 1)
                                    {
                                        break;
                                    }
                                }
                            }
                            slc = linkset = cgr_net = cgr_spc = link = "";
                        }

                        else if (temp[i].Trim().Contains("ASSOCIATION SET") || temp[i].Trim().Contains("LINK TEST NOT ALLOWED"))
                        {
                            i = i + 2;
                            if (i > temp.Length - 1)
                            {
                                break;
                            }
                            if (!temp[i].Trim().Contains("----------") && temp[i] != "")
                            {
                                //while (temp[i] != "COMMAND EXECUTED" && i <= temp.Length - 1 && temp[i].Trim() != "")
                                // {
                                if (temp[i].Trim().Contains("LINK TEST NOT ALLOWED"))
                                {
                                   
                                }
                                else
                                {
                                    if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                    {
                                        temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                        value = Regex.Split(temp_data.ToString().Trim(), "~");
                                        if (value.Length > 3)
                                        {
                                            cgr_net = value[0].ToString();
                                            cgr_spc = value[1].ToString();
                                            linkset = value[2].ToString();
                                            ASSOCSTATE = value[3].ToString();
                                            LSSTATE = value[4].ToString();
                                            link = value[5].ToString();
                                        }
                                        if (value.Length == 3 && temp_data.Trim().Contains("REMOTE LINKS:"))
                                        {
                                            cgr_net = "";
                                            cgr_spc = "";
                                            slc = "";

                                        }

                                        if (cgr_spc != "")
                                        {
                                            query = "select distinct CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK from mss_mgw_map_" + CircleName + "_" + group_name + " where CGR_SPC = '" + cgr_spc.Trim() + "' and CGR_NET='" + cgr_net + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' and LINKSET is null and ASSOCIATION_SET is null and STATE is null and LINK is null";

                                            //query = "select distinct CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK from mss_mgw_map_" + CircleName + "_" + group_name + " where circle = '" + CircleName + "' and NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' ";                                                
                                            SQLQuery(ref conn, ref dt, query);

                                            if (dt.Rows.Count > 0)
                                            {
                                                query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK = '" + link.Trim() + "', STATE = '" + LSSTATE.Trim() + "', ASSOCIATION_SET = '" + ASSOCSTATE.Trim() + "', LINKSET = '" + linkset.Trim() + "' where CGR_SPC='" + cgr_spc + "' and CGR_NET='" + cgr_net + "' AND MGW_NAME = '" + _ne_name.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' AND NODE_TYPE = '" + _NEType + "' and LINKSET is null and ASSOCIATION_SET is null and STATE is null and LINK is null";

                                                //query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK = '" + link.Trim() + "', STATE = '" + LSSTATE.Trim() + "', ASSOCIATION_SET = '" + ASSOCSTATE.Trim() + "',CGR_NET='" + cgr_net + "',CGR_SPC='" + cgr_spc + "' where LINKSET = '" + linkset.Trim() + "' AND ne_name = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and EXTRA_ET_FLAG = '1' ";
                                                ExecuteSQLQuery(ref conn, query1);
                                                //ASSOCSTATE = LSSTATE = linkset = cgr_net = cgr_spc = link = "";
                                            }
                                            else
                                            {
                                                query = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,ifnull(NE_NAME,'') as NE_NAME ,ifnull(MCC,'') as MCC,ifnull(MNC,'') as MNC,ifnull(MSS,'') as MSS,ifnull(ELEMENT_IP,'') as ELEMENT_IP,ifnull(CGR_NET,'') as CGR_NET,ifnull(MGW_C_NO,'') as MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + net + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                                //query = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where MGW_NAME='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                                SQLQuery(ref conn, ref dt, query);
                                                if (dt.Rows.Count >= 1)
                                                {
                                                    query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + cgr_net.Trim() + "', '" + cgr_spc.Trim() + "', '" + linkset.Trim() + "', '" + ASSOCSTATE.Trim() + "', '" + LSSTATE.Trim() + "', '" + link.Trim() + "')";
                                                    ExecuteSQLQuery(ref conn, query1);
                                                }
                                                else if (dt.Rows.Count <= 0)
                                                {
                                                    query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + cgr_net.Trim() + "', '" + cgr_spc.Trim() + "', '" + linkset.Trim() + "', '" + ASSOCSTATE.Trim() + "', '" + LSSTATE.Trim() + "', '" + link.Trim() + "')";
                                                    ExecuteSQLQuery(ref conn, query1);
                                                    //ASSOCSTATE = LSSTATE = linkset = cgr_net = cgr_spc = link = "";
                                                }
                                                //query1 = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values (' " + cgr_net.Trim() + " ', ' " + cgr_spc.Trim() + " ', ' " + linkset.Trim() + " ', ' " + ASSOCSTATE.Trim() + " ', ' " + LSSTATE.Trim() + " ', ' " + link.Trim() + " ') ";
                                            }
                                        }
                                        link = LSSTATE = ASSOCSTATE = linkset = cgr_spc = cgr_net = "";                                                                                                
                                    }
                                }
                            }
                        }
                        //if (i > temp.Length - 1)
                        //{
                        //    break;
                        //}
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_mss_ZNSI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNSI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNSI_SUBwithConinue()", ErrorMsg, "", ref FileError);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZWTI_P(string output, string CircleName, string NE_Type) //ADDED by AJAY
        {
            DataTable dt = new DataTable();
            string Newquery = string.Empty;
            string VENDOR = string.Empty;
            string CIRCLE = string.Empty;
            string NE_NAME = string.Empty;
            string NODE_TYPE = string.Empty;
            string FEATURE_CODE = string.Empty;
            string FEATURE_NAME = string.Empty;
            string FEATURE_STATE = string.Empty;
            string FEATURE_CAPACITY = string.Empty;
            string SUBTRACK_STATUS = string.Empty;
            string CP_VALUE = string.Empty;
            string POWERCARD_STATUS = string.Empty;
            string query = string.Empty;
            string query1 = string.Empty;
            int flag = 1;
            string data = string.Empty;
            string[] temp;
            string cmnd_output = output.Trim();

            try
            {
                temp = Regex.Split(cmnd_output, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (temp[i].Contains("FEATURE CODE"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_CODE = data.Remove(0, data.LastIndexOf("."));
                            FEATURE_CODE = FEATURE_CODE.Replace(".", " ");
                            FEATURE_CODE = FEATURE_CODE.Trim();

                        }

                        if (temp[i].Contains("FEATURE NAME"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_NAME = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_NAME = FEATURE_NAME.Replace("..", " ");
                            FEATURE_NAME = FEATURE_NAME.Trim();
                        }

                        if (temp[i].Contains("FEATURE STATE"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_STATE = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_STATE = FEATURE_STATE.Replace("..", " ");
                            FEATURE_STATE = FEATURE_STATE.Trim();
                        }

                        if (temp[i].Contains("FEATURE CAPACITY"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_CAPACITY = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_CAPACITY = FEATURE_CAPACITY.Replace("..", " ");
                            FEATURE_CAPACITY = FEATURE_CAPACITY.Trim();
                        }


                        if (FEATURE_CODE != "" && FEATURE_NAME != "" && FEATURE_STATE != "" && FEATURE_CAPACITY != "" && _NEType != "")
                        {
                            //if (flag == 1)
                            //{
                            query = "select VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_STATE,FEATURE_CAPACITY from licence_capacity_warning_" + CircleName + " where circle = '" + CircleName + "' and NE_NAME = '" + _ne_name + "' and FEATURE_CODE = '" + FEATURE_CODE + "' AND NODE_TYPE = '" + _NEType + "' ";
                            SQLQuery(ref conn, ref dt, query);

                            //for (int p = 0; p <= dt.Rows.Count - 1; p++)
                            //{
                            if (dt.Rows.Count > 0)
                            {
                                query1 = "";

                                query1 = " update licence_capacity_warning_" + CircleName + "  set   `FEATURE_NAME`='" + FEATURE_NAME + "',";
                                query1 += " `FEATURE_STATE`='" + FEATURE_STATE + "', `FEATURE_CAPACITY`='" + FEATURE_CAPACITY + "' where NE_NAME = '" + _ne_name + "' FEATURE_CODE = '" + FEATURE_CODE + "'  AND ";
                                query1 += " circle = '" + _CircleName + "' ";

                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_STATE = "";
                                FEATURE_CAPACITY = "";
                            }
                            //}
                            // }
                            else
                            {
                                query1 = "insert into licence_capacity_warning_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_STATE,FEATURE_CAPACITY) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "', '" + _ne_name.Trim() + "', '" + _NEType.Trim() + "','" + FEATURE_CODE.Trim() + "','" + FEATURE_NAME.Trim() + "','" + FEATURE_STATE.Trim() + "','" + FEATURE_CAPACITY.Trim() + "') ";
                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_STATE = "";
                                FEATURE_CAPACITY = "";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZWTI_P_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_LICENCECAPACITY()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZWFI_PS(string output, string CircleName, string NE_Type) //ADDED by AJAY
        {
            string temp_data = string.Empty;
            string slc = string.Empty;
            string[] value;
            string[] temp;
            DataTable dt = new DataTable();
            string query1 = string.Empty;
            string CHMS = string.Empty;
            string SHMS = string.Empty;
            string PPA = string.Empty;
            string PIU = string.Empty;
            string HW_CONF_STATE = string.Empty;
            string HW_STATE = string.Empty;
            string unit = string.Empty;
            string query = string.Empty; ;
            try
            {
                string[] cmnd_output = Regex.Split(output.Trim(), "COMMAND EXECUTED");
                string data = cmnd_output[0].Trim();
                cmnd_output = Regex.Split(data, "READING DATA FROM DATABASE ...");

                if (cmnd_output.Length == 1)
                {
                    data = cmnd_output[1].Trim().ToString();
                }
                else
                {
                    data = cmnd_output[1].Trim().ToString();
                }


                temp = Regex.Split(data, "\r\n");
                for (int i = 2; i <= temp.Length - 1; i++)
                {
                    try
                    {

                        if (!temp[i].Trim().Contains("----------") && temp[i] != "" && _NEType == "MGW")
                        {
                            while (temp[i] != "COMMAND EXECUTED" && i <= temp.Length - 1 && temp[i].Trim() != "")
                            {
                                if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                {
                                    temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                    value = Regex.Split(temp_data.ToString().Trim(), "~");

                                    if (value.Length <= 7)
                                    {
                                        HW_STATE = value[value.Length - 1].ToString();
                                        HW_CONF_STATE = value[value.Length - 2].ToString();
                                        unit = value[value.Length - 3].ToString();
                                        PIU = value[value.Length - 4].ToString();
                                        PPA = value[value.Length - 5].ToString();
                                        SHMS = value[value.Length - 6].ToString();
                                        CHMS = value[value.Length - 7].ToString();
                                    }

                                    if (HW_STATE != "" && HW_CONF_STATE != "" && unit != "" && PIU != "" && PPA != "" && SHMS != "" && CHMS != "" && NE_Type == "MGW")
                                    {

                                        query = "select distinct VENDOR, CIRCLE, NE_NAME, NODE_TYPE,CHMS,SHMS,PPA,PIU,UNIT,HW_CONF_STATE,HW_STATE from redundancy_subtrack_mgw_guj  where HW_STATE = '" + HW_STATE + "' and UNIT = '" + unit + "' and PIU = '" + PIU + "' and PPA = '" + PPA + "' and SHMS = '" + SHMS + "' and CHMS = '" + CHMS + "' and NODE_TYPE = '" + NE_Type + "' ";
                                        SQLQuery(ref conn, ref dt, query);

                                        if (dt.Rows.Count > 0)
                                        {
                                            query = "Update redundancy_subtrack_mgw_guj  set CHMS = '" + CHMS.Trim() + "', SHMS = '" + SHMS.Trim() + "', PPA = '" + PPA.Trim() + "', PIU = '" + PIU.Trim() + "', UNIT = '" + unit.Trim() + "', HW_CONF_STATE = '" + HW_CONF_STATE.Trim() + "', HW_STATE = '" + HW_STATE.Trim() + "'  where NODE_TYPE = '" + NE_Type + "' and NE_NAME = '" + _ne_name.Trim() + "'  and VENDOR='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' ";
                                            ExecuteSQLQuery(ref conn, query);

                                        }
                                        else
                                        {
                                            query1 = "insert into redundancy_subtrack_mgw_guj (VENDOR, CIRCLE, NE_NAME, NODE_TYPE,CHMS,SHMS,PPA,PIU,UNIT,HW_CONF_STATE,HW_STATE) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "', '" + _ne_name.Trim() + "', '" + _NEType.Trim() + "', '" + CHMS.Trim() + "', '" + SHMS.Trim() + "', '" + PPA.Trim() + "', '" + PIU.Trim() + "', '" + unit.Trim() + "', '" + HW_CONF_STATE.Trim() + "', '" + HW_STATE.Trim() + "') ";
                                            ExecuteSQLQuery(ref conn, query1);
                                        }
                                    }
                                }

                                ExecuteSQLQuery(ref conn, query);

                                i = i + 1;
                                if (i > temp.Length - 1)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNSI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZWFI_PS_SUBwithConinue()", ErrorMsg, "", ref FileError);
            }

        }

        //public void ParseData_ZNSI(string output, string CircleName, string NE_Type, string net)
        //{
        //    try
        //    {
        //        string[] cmnd_output = Regex.Split(output.Trim(), "COMMAND EXECUTED");
        //        string data = cmnd_output[0].Trim();
        //        cmnd_output = Regex.Split(data, "SLC");

        //        if (cmnd_output.Length == 1)
        //        {
        //            data = cmnd_output[0].Trim().ToString();
        //        }
        //        else
        //        {
        //            data = cmnd_output[1].Trim().ToString();
        //        }
        //        string temp_data = string.Empty;
        //        string slc = string.Empty;
        //        string[] value;
        //        string[] temp;
        //        string query = string.Empty;
        //        string cgr_spc = string.Empty;
        //        string cgr_net = string.Empty;
        //        string link = string.Empty;
        //        string LinkSet = string.Empty;

        //        temp = Regex.Split(data, "\r\n");


        //        for (int i = 0; i <= temp.Length - 1; i++)
        //        {
        //            try
        //            {
        //                if (!temp[i].Trim().Contains("----------") && temp[i].Trim().ToString() != "")
        //                {
        //                    while (i < temp.Length && temp[i] != "COMMAND EXECUTED" && temp[i].Trim() != "")
        //                    {
        //                        if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
        //                        {
        //                            temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
        //                            value = Regex.Split(temp_data.ToString().Trim(), "~");

        //                            if (value.Length >= 2)
        //                            {
        //                                slc = value[value.Length - 1].ToString();
        //                                link = value[value.Length - 2].ToString();
        //                            }

        //                            if (value.Length > 3)
        //                            {
        //                                cgr_net = value[0].Trim();
        //                                cgr_spc = value[1].Trim();
        //                                LinkSet = value[2].Trim();
        //                            }


        //                            if (cgr_net != "" || cgr_spc != "")
        //                            {
        //                                if (cgr_net.Contains("NA0") || cgr_net.Contains("NA1"))
        //                                {
        //                                    if (NE_Type == "HLR")
        //                                    {
        //                                        query = "Update hlr_" + CircleName + " set SLC = '" + slc.Trim() + "', CGR_NET = '" + cgr_net + "', cgr_spc = '" + cgr_spc + "'   where link = '" + link.Trim() + "' AND NE_NAME = '" + _ne_name + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  ";
        //                                        cgr_net = "";
        //                                        cgr_spc = "";

        //                                    }
        //                                    else if (NE_Type == "MSS_LINKS")
        //                                    {
        //                                        //changed where condition from link to et on 2015-02-06
        //                                        //DataTable dt = null;
        //                                        //SQLQuery(ref conn, ref dt, query);
        //                                        //query = "Select * from mss_link_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and LINKSET='" + LinkSet.Trim() + "' ";

        //                                        query = "Update mss_link_" + CircleName + "_" + group_name + " set SLC = '" + slc.Trim() + "', CGR_NET = '" + cgr_net + "', cgr_spc = '" + cgr_spc + "'   where  NE_NAME = '" + _ne_name + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and LINKSET='" + LinkSet.Trim() + "' ";
        //                                        cgr_net = "";
        //                                        cgr_spc = "";

        //                                    }
        //                                    else if (NE_Type == "MSC")
        //                                    {
        //                                        query = "Update msc_" + CircleName + " set SLC = '" + slc.Trim() + "' where link = '" + link.Trim() + "' AND NE_NAME = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and CGR_NET = '" + net + "' ";
        //                                        cgr_net = "";
        //                                        cgr_spc = "";
        //                                    }
        //                                    else
        //                                    {
        //                                        query = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set SLC = '" + slc.Trim() + "' where link = '" + link.Trim() + "' AND MGW_NAME = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and CGR_NET = '" + net + "' ";
        //                                        cgr_net = "";
        //                                        cgr_spc = "";
        //                                    }
        //                                    ExecuteSQLQuery(ref conn, query);
        //                                }
        //                            }
        //                        }
        //                        i = i + 1;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
        //                //if (!Directory.Exists(parsing_error_path))
        //                //{
        //                //    Directory.CreateDirectory(parsing_error_path);
        //                //}

        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZNSI ");
        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        //                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNSI_SUBwithContinue()", ErrorMsg, "", ref FileError);

        //                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //                ExecuteSQLQuery(ref conn, ErrorQuery);
        //                continue;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNSI()", ErrorMsg, "", ref FileError);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZNEL(string command_output, string CircleName, string NE_Type)
        {
            String ErrorMsg = string.Empty;
            DataTable dt = new DataTable();
            string output = command_output;
            string[] temp;
            string temp1 = string.Empty;
            string temp2 = string.Empty;
            string text = string.Empty;
            string data = string.Empty;
            string[] value;
            string li = string.Empty;
            string LINK = "";
            string LINK_SET = string.Empty;
            string LINK_STATE = string.Empty;
            string UNIT = string.Empty;
            string query = string.Empty;
            string temp_query = string.Empty;
            string temp_value = string.Empty;
            string QueryTemp = string.Empty;
            string nextQuery = string.Empty;
            DataTable newdt = new DataTable();
            DataTable dtmsc = new DataTable();
            DataTable tempdt = new DataTable();
            DataTable linkdt = new DataTable();
            string[] temp_val2;
            string PCM_TSL = string.Empty;
            string LOG_TERM = string.Empty;
            string ETGR = string.Empty;
            string TF = string.Empty;
            string EXTERN_PCM_TSL = string.Empty;
            string BIT_RATE = string.Empty;
            string ASSOCIATION_SET = string.Empty;
            try
            {
                if (output.Contains("RATE")) // for all nodes first it will check whether rate and M3UA Based is present or not ** modified by Deepak & Asif
                {
                    temp1 = output.Remove(0, output.IndexOf("RATE"));
                    if (temp1.Contains("M3UA BASED LINKS"))
                    {
                        temp = Regex.Split(temp1, "M3UA BASED LINKS");// Data under  "M3UA BASED LINKS" is not required.Data will be parser only if it contains data under "AV" or "UA"[Pramod]
                        temp1 = temp[0];
                        temp2 = temp[1];
                    }
                }
                else
                {
                    //temp1 = output;
                    if (output.Contains("M3UA BASED LINKS"))
                    {
                        temp = Regex.Split(output, "M3UA BASED LINKS");// Data under  "M3UA BASED LINKS" is not required.Data will be parser only if it contains data under "AV" or "UA"[Pramod]
                        temp2 = temp[1];
                    }
                }

                #region[TDM BASED LINKS]
                temp = Regex.Split(temp1, "\r\n");
                if (!string.IsNullOrEmpty(temp1))
                {
                    for (int i = 2; i <= temp.Length - 1; i++)
                    {
                        try
                        {
                            if (temp[i].Contains("AV-") || (temp[i].Contains("UA-")))
                            {
                                dt.Clear();
                                dtmsc.Clear();
                                newdt.Clear();
                                text = "";
                                data = "";
                                data = temp[i].Trim();
                                string[] data_split = Regex.Split(data, " ");
                                for (int j = 0; j <= data_split.Length - 1; j++)
                                {
                                    if (data_split[j] != "")
                                        text = text + data_split[j].Trim() + ";";
                                }
                                if (text != "")
                                {
                                    value = Regex.Split(text, ";");
                                    LINK = value[0];
                                    LINK_SET = value[1] + " " + value[2];
                                    LINK_STATE = value[3];
                                    UNIT = value[4];
                                    LOG_TERM = value[5];
                                    ETGR = value[6];
                                    TF = value[7];
                                    EXTERN_PCM_TSL = value[8];
                                    BIT_RATE = value[9];

                                    if (NE_Type == "MSC")
                                    {
                                        if (value.Length > 7)      // important to check as sometimes there are rows that have no pcm_tsl values.
                                        {
                                            temp_val2 = Regex.Split(value[7].ToString().Trim(), "-");
                                            PCM_TSL = temp_val2[0];
                                        }
                                    }
                                    else
                                    {
                                        if (value.Length == 6)
                                        {
                                            temp_val2 = Regex.Split(value[4].ToString().Trim(), "-");
                                        }
                                        else
                                        {
                                            temp_val2 = Regex.Split(value[8].ToString().Trim(), "-");
                                        }
                                        PCM_TSL = temp_val2[0];
                                    }
                                    text = "";
                                    if (NE_Type == "HLR")
                                    {

                                        tempdt.Clear();
                                        query = " select VENDOR, CIRCLE, NE_NAME, NODE_TYPE, HLR, ELEMENT_IP, HLR_C_NO from hlr_" + CircleName + " where `ET` is null and `LINK` is null and `LINKSET` is null and `VENDOR` = '" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and NODE_Type='" + NE_Type + "' and NE_NAME = '" + _ne_name.Trim() + "' ";
                                        SQLQuery(ref conn, ref newdt, query);
                                        for (int l = 0; l <= newdt.Rows.Count - 1; l++)
                                        {
                                            nextQuery = "insert into hlr_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, HLR, ELEMENT_IP, HLR_C_NO,ET,LINK,LINKSET) values ('" + newdt.Rows[l]["VENDOR"] + "','" + newdt.Rows[l]["CIRCLE"] + "','" + newdt.Rows[l]["NE_NAME"] + "','" + newdt.Rows[l]["NODE_TYPE"] + "','" + newdt.Rows[l]["HLR"] + "','" + newdt.Rows[l]["ELEMENT_IP"] + "','" + newdt.Rows[l]["HLR_C_NO"] + "','" + PCM_TSL.Trim() + "','" + LINK.Trim() + "','" + LINK_SET.Trim() + "')";
                                            ExecuteSQLQuery(ref conn, nextQuery);
                                        }
                                    }
                                    else if (NE_Type == "MSS_LINKS")
                                    {
                                        tempdt.Clear();
                                        query = " select VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MSS, ELEMENT_IP, MSS_C_NO from mss_link_" + CircleName + "_" + group_name + "  where `ET` is null and `LINK` is null and `LINKSET` is null and `VENDOR` = '" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and NODE_Type='" + NE_Type + "' and NE_NAME = '" + _ne_name.Trim() + "' ";
                                        SQLQuery(ref conn, ref newdt, query);
                                        for (int l = 0; l <= newdt.Rows.Count - 1; l++)
                                        {
                                            nextQuery = "insert into mss_link_" + CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MSS, ELEMENT_IP, MSS_C_NO,ET,LINK,LINKSET) values ('" + newdt.Rows[l]["VENDOR"] + "','" + newdt.Rows[l]["CIRCLE"] + "','" + newdt.Rows[l]["NE_NAME"] + "','" + newdt.Rows[l]["NODE_TYPE"] + "','" + newdt.Rows[l]["MSS"] + "','" + newdt.Rows[l]["ELEMENT_IP"] + "','" + newdt.Rows[l]["MSS_C_NO"] + "','" + PCM_TSL.Trim() + "','" + LINK.Trim() + "','" + LINK_SET.Trim() + "')";
                                            ExecuteSQLQuery(ref conn, nextQuery);
                                        }
                                    }
                                    else if (NE_Type == "MSC")
                                    {
                                        SQLQuery(ref conn, ref dtmsc, "select distinct * from msc_" + CircleName + " where ET = '" + PCM_TSL.Trim() + "'   and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and NE_NAME = '" + _ne_name.Trim() + "'");
                                        if (dtmsc.Rows.Count > 0)
                                        {
                                            tempdt.Clear();
                                            SQLQuery(ref conn, ref tempdt, "select distinct LINK from msc_" + CircleName + " where ET = '" + PCM_TSL.Trim() + "'   and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and NE_NAME = '" + _ne_name.Trim() + "' and link is not null and link not in ('')  ");
                                            // this will check if already there is a link defined under this et
                                            if (tempdt.Rows.Count > 0)
                                            {
                                                linkdt.Clear();
                                                temp_query = "SELECT * FROM (select VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, if(CIC is null,'',CIC) AS CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, NE_Type2, ET, LINK, LINKSET, SLC, SPC_TYPE, EXTRA_ET_FLAG from msc_" + CircleName + " where ET = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and NE_NAME = '" + _ne_name.Trim() + "') a GROUP BY a.CIC";

                                                SQLQuery(ref conn, ref linkdt, temp_query);

                                                for (int k = 0; k <= linkdt.Rows.Count - 1; k++)
                                                {
                                                    string qur = string.Empty;

                                                    temp_query = "insert into msc_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, NE_Type2, ET, LINK, LINKSET, SLC, SPC_TYPE, EXTRA_ET_FLAG) values ('" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" + linkdt.Rows[k]["NE_NAME"] + "','" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "', '" + linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "', '" + linkdt.Rows[k]["MSS_C_NO"] + "', '" + linkdt.Rows[k]["MSS_SPC"] + "', '" + linkdt.Rows[k]["CGR"] + "', NCGR, '" + linkdt.Rows[k]["CGR_SPC"] + "', '" + linkdt.Rows[k]["GENERIC_NAME"] + "', '" + linkdt.Rows[k]["CGR_NET"] + "',  '" + linkdt.Rows[k]["CGR_TYPE"] + "', '" + linkdt.Rows[k]["CGR_UPART"] + "', '" + linkdt.Rows[k]["CIC"] + "', '" + linkdt.Rows[k]["BSC_NAME"] + "', '" + linkdt.Rows[k]["BSC_STATE"] + "' , '" + linkdt.Rows[k]["BSC_NUMBER"] + "' ,'" + linkdt.Rows[k]["LAC"] + "' ,'" + linkdt.Rows[k]["NE_Type2"] + "', '" + PCM_TSL + "', '" + LINK + "','" + LINK_SET + "', '" + linkdt.Rows[k]["SLC"] + "', '" + linkdt.Rows[k]["SPC_TYPE"] + "','" + linkdt.Rows[k]["EXTRA_ET_FLAG"] + "' )";
                                                    ExecuteSQLQuery(ref conn, temp_query);
                                                }
                                            }
                                            else
                                            {
                                                query = "Update msc_" + CircleName + " set LINK = '" + LINK.Trim() + "' , LINKSET = '" + LINK_SET.Trim() + "' where ET = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and NE_NAME = '" + _ne_name.Trim() + "' ";
                                                ExecuteSQLQuery(ref conn, query);
                                            }
                                        }
                                        else
                                        {
                                            query = "insert into msc_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE,ET, LINK, LINKSET,EXTRA_ET_FLAG) values ('" + _vendor.Trim() + "','" + CircleName.Trim() + "','" + _ne_name.Trim() + "','MSC','" + PCM_TSL.Trim() + "', '" + LINK.Trim() + "','" + LINK_SET.Trim() + "','1')";
                                            ExecuteSQLQuery(ref conn, query);
                                        }
                                    }
                                    else
                                    {
                                        // for mgw
                                        SQLQuery(ref conn, ref dt, "select distinct * from mss_mgw_map_" + CircleName + "_" + group_name + "  where (ET = '" + PCM_TSL.Trim() + "' OR APCM = '" + PCM_TSL.Trim() + "')  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' ");
                                        if (dt.Rows.Count > 0)
                                        {
                                            if (PCM_TSL.Trim().Length <= 4)
                                            { // pramod
                                                tempdt.Clear();
                                                SQLQuery(ref conn, ref tempdt, "select distinct  LINK from mss_mgw_map_" + CircleName + "_" + group_name + "  where ET = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and link is not null and link not in ('') ");
                                                if (tempdt.Rows.Count > 0)
                                                {
                                                    // here we have checked that if this ET already has a link defined in it or not, if yes then a duplicate row is need to be inserted
                                                    // this means that there is already a link defined and now we have to insert this link under this ET
                                                    linkdt.Clear();
                                                    temp_query = "select * from(select VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP,VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, if(CIC is null,'',CIC) CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME,MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID,MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE from mss_mgw_map_" + CircleName + "_" + group_name + " where ET = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "') A group by cic";
                                                    SQLQuery(ref conn, ref linkdt, temp_query);
                                                    for (int k = 0; k <= linkdt.Rows.Count - 1; k++)
                                                    {
                                                        string qur = string.Empty;
                                                        string et_flag = linkdt.Rows[k]["EXTRA_ET_FLAG"].ToString().Trim();
                                                        if (et_flag == "2")
                                                        {
                                                            qur = "";
                                                            qur = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP, VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME, MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID, MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE) values( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" + linkdt.Rows[k]["NE_NAME"] + "', '" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" + linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" + linkdt.Rows[k]["MSS_C_NO"] + "','" + linkdt.Rows[k]["MSS_SPC"] + "','" + linkdt.Rows[k]["VMGW_ID"] + "','" + linkdt.Rows[k]["VMGW"] + "','" + linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" + linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" + linkdt.Rows[k]["VMGW_DEST_IP"] + "','" + linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" + linkdt.Rows[k]["CGR_SPC"] + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" + linkdt.Rows[k]["CGR_TYPE"] + "','" + linkdt.Rows[k]["CGR_UPART"] + "','" + linkdt.Rows[k]["TERM_ID"] + "','" + linkdt.Rows[k]["CIC"] + "','" + linkdt.Rows[k]["BSC_NAME"] + "','" + linkdt.Rows[k]["BSC_STATE"] + "','" + linkdt.Rows[k]["BSC_NUMBER"] + "','" + linkdt.Rows[k]["LAC"] + "','" + linkdt.Rows[k]["MGW_NAME"] + "','" + linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" + linkdt.Rows[k]["MGW_C_NO"] + "','" + linkdt.Rows[k]["MGW_SPC"] + "','" + linkdt.Rows[k]["MGW_VMGW_ID"] + "','" + linkdt.Rows[k]["MGW_VMGW"] + "', '" + linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" + linkdt.Rows[k]["MGW_CGR"] + "','" + linkdt.Rows[k]["MGW_NCGR"] + "', '" + linkdt.Rows[k]["UNIT"] + "', '" + linkdt.Rows[k]["STER"] + "', '" + linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" + linkdt.Rows[k]["APCM"] + "', '" + LINK.Trim().ToString() + "', '" + LINK_SET.Trim().ToString() + "', '" + linkdt.Rows[k]["SLC"] + "', '2', '" + linkdt.Rows[k]["SPC_TYPE"] + "')";
                                                            ExecuteSQLQuery(ref conn, qur);
                                                        }
                                                        else
                                                        {
                                                            qur = "";
                                                            qur = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP, VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME, MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID, MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE) values( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" + linkdt.Rows[k]["NE_NAME"] + "', '" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" + linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" + linkdt.Rows[k]["MSS_C_NO"] + "','" + linkdt.Rows[k]["MSS_SPC"] + "','" + linkdt.Rows[k]["VMGW_ID"] + "','" + linkdt.Rows[k]["VMGW"] + "','" + linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" + linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" + linkdt.Rows[k]["VMGW_DEST_IP"] + "','" + linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" + linkdt.Rows[k]["CGR_SPC"] + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" + linkdt.Rows[k]["CGR_TYPE"] + "','" + linkdt.Rows[k]["CGR_UPART"] + "','" + linkdt.Rows[k]["TERM_ID"] + "','" + linkdt.Rows[k]["CIC"] + "','" + linkdt.Rows[k]["BSC_NAME"] + "','" + linkdt.Rows[k]["BSC_STATE"] + "','" + linkdt.Rows[k]["BSC_NUMBER"] + "','" + linkdt.Rows[k]["LAC"] + "','" + linkdt.Rows[k]["MGW_NAME"] + "','" + linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" + linkdt.Rows[k]["MGW_C_NO"] + "','" + linkdt.Rows[k]["MGW_SPC"] + "','" + linkdt.Rows[k]["MGW_VMGW_ID"] + "','" + linkdt.Rows[k]["MGW_VMGW"] + "', '" + linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" + linkdt.Rows[k]["MGW_CGR"] + "','" + linkdt.Rows[k]["MGW_NCGR"] + "', '" + linkdt.Rows[k]["UNIT"] + "', '" + linkdt.Rows[k]["STER"] + "', '" + linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" + linkdt.Rows[k]["APCM"] + "', '" + LINK.Trim().ToString() + "', '" + LINK_SET.Trim().ToString() + "', '" + linkdt.Rows[k]["SLC"] + "', '0', '" + linkdt.Rows[k]["SPC_TYPE"] + "')";
                                                            ExecuteSQLQuery(ref conn, qur);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK='" + LINK.Trim().ToString() + "', LINKSET = '" + LINK_SET.Trim().ToString() + "' where ET = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and (link is null OR link='') ");
                                                }
                                            }
                                            else
                                            {
                                                tempdt.Clear();
                                                SQLQuery(ref conn, ref tempdt, "select distinct LINK from mss_mgw_map_" + CircleName + "_" + group_name + "  where APCM = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and link is not null and link not in ('') ");
                                                if (tempdt.Rows.Count > 0)
                                                {
                                                    // here we have checked that if this ET already has a link defined in it or not, if yes then a duplicate row is need to be inserted
                                                    // this means that there is already a link defined and now we have to insert this link under this ET
                                                    linkdt.Clear();
                                                    temp_query = "select * from (select VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP,VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, if(CIC is null,'',CIC) CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME,MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID,MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE from mss_mgw_map_" + CircleName + "_" + group_name + "  where ET = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "')A group by CIC ";
                                                    SQLQuery(ref conn, ref linkdt, temp_query);
                                                    for (int k = 0; k <= linkdt.Rows.Count - 1; k++)
                                                    {
                                                        string qur = string.Empty;
                                                        string et_flag = linkdt.Rows[k]["EXTRA_ET_FLAG"].ToString().Trim();
                                                        if (et_flag == "2")
                                                        {
                                                            qur = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP, VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME, MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID, MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE)  values( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" + linkdt.Rows[k]["NE_NAME"] + "', '" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" + linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" + linkdt.Rows[k]["MSS_C_NO"] + "','" + linkdt.Rows[k]["MSS_SPC"] + "','" + linkdt.Rows[k]["VMGW_ID"] + "','" + linkdt.Rows[k]["VMGW"] + "','" + linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" + linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" + linkdt.Rows[k]["VMGW_DEST_IP"] + "','" + linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" + linkdt.Rows[k]["CGR_SPC"] + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" + linkdt.Rows[k]["CGR_TYPE"] + "','" + linkdt.Rows[k]["CGR_UPART"] + "','" + linkdt.Rows[k]["TERM_ID"] + "','" + linkdt.Rows[k]["CIC"] + "','" + linkdt.Rows[k]["BSC_NAME"] + "','" + linkdt.Rows[k]["BSC_STATE"] + "','" + linkdt.Rows[k]["BSC_NUMBER"] + "','" + linkdt.Rows[k]["LAC"] + "','" + linkdt.Rows[k]["MGW_NAME"] + "','" + linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" + linkdt.Rows[k]["MGW_C_NO"] + "','" + linkdt.Rows[k]["MGW_SPC"] + "','" + linkdt.Rows[k]["MGW_VMGW_ID"] + "','" + linkdt.Rows[k]["MGW_VMGW"] + "', '" + linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" + linkdt.Rows[k]["MGW_CGR"] + "', '" + linkdt.Rows[k]["MGW_NCGR"] + "', '" + linkdt.Rows[k]["UNIT"] + "', '" + linkdt.Rows[k]["STER"] + "', '" + linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" + linkdt.Rows[k]["APCM"] + "', '" + LINK.Trim().ToString() + "', '" + LINK_SET.Trim().ToString() + "', '" + linkdt.Rows[k]["SLC"] + "', '2', '" + linkdt.Rows[k]["SPC_TYPE"] + "')";
                                                            ExecuteSQLQuery(ref conn, qur);
                                                        }
                                                        else
                                                        {
                                                            qur = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP, VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME, MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID, MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE) values( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" + linkdt.Rows[k]["NE_NAME"] + "', '" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" + linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" + linkdt.Rows[k]["MSS_C_NO"] + "','" + linkdt.Rows[k]["MSS_SPC"] + "','" + linkdt.Rows[k]["VMGW_ID"] + "','" + linkdt.Rows[k]["VMGW"] + "','" + linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" + linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" + linkdt.Rows[k]["VMGW_DEST_IP"] + "','" + linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" + linkdt.Rows[k]["CGR_SPC"] + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" + linkdt.Rows[k]["CGR_TYPE"] + "','" + linkdt.Rows[k]["CGR_UPART"] + "','" + linkdt.Rows[k]["TERM_ID"] + "','" + linkdt.Rows[k]["CIC"] + "','" + linkdt.Rows[k]["BSC_NAME"] + "','" + linkdt.Rows[k]["BSC_STATE"] + "','" + linkdt.Rows[k]["BSC_NUMBER"] + "','" + linkdt.Rows[k]["LAC"] + "','" + linkdt.Rows[k]["MGW_NAME"] + "','" + linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" + linkdt.Rows[k]["MGW_C_NO"] + "','" + linkdt.Rows[k]["MGW_SPC"] + "','" + linkdt.Rows[k]["MGW_VMGW_ID"] + "','" + linkdt.Rows[k]["MGW_VMGW"] + "', '" + linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" + linkdt.Rows[k]["MGW_CGR"] + "', '" + linkdt.Rows[k]["MGW_NCGR"] + "', '" + linkdt.Rows[k]["UNIT"] + "', '" + linkdt.Rows[k]["STER"] + "', '" + linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" + linkdt.Rows[k]["APCM"] + "', '" + LINK.Trim().ToString() + "', '" + LINK_SET.Trim().ToString() + "', '" + linkdt.Rows[k]["SLC"] + "', '0', '" + linkdt.Rows[k]["SPC_TYPE"] + "')";
                                                            ExecuteSQLQuery(ref conn, qur);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK='" + LINK.Trim().ToString() + "', LINKSET = '" + LINK_SET.Trim().ToString() + "' where APCM = '" + PCM_TSL.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and (link is null OR link='') ");
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // these are extra ET's
                                            if (PCM_TSL.Trim().Length <= 4)
                                            {
                                                //query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + "  (VENDOR, CIRCLE,NODE_TYPE, MGW_NAME,ET,LINK,LINKSET, EXTRA_ET_FLAG) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType + "','" + _ne_name.Trim() + "','" + PCM_TSL.Trim() + "', '" + LINK.Trim() + "','" + LINK_SET.Trim() + "','2')";
                                                query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + "  (VENDOR, CIRCLE,NODE_TYPE, MGW_NAME,ET,LINK,LINKSET, EXTRA_ET_FLAG,TERM_ID,TF,ETGR_VETGR,EXTERN_PCM_TSL,BIT_RATE) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType + "','" + _ne_name.Trim() + "','" + PCM_TSL.Trim() + "', '" + LINK.Trim() + "','" + LINK_SET.Trim() + "','2','" + LOG_TERM + "','" + TF + "','" + ETGR + "','" + EXTERN_PCM_TSL + "','" + BIT_RATE + "')";
                                                ExecuteSQLQuery(ref conn, query);

                                                PCM_TSL = LINK = LINK_SET = LOG_TERM = TF = ETGR = EXTERN_PCM_TSL = BIT_RATE = "";
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                            //if (!Directory.Exists(parsing_error_path))
                            //{
                            //    Directory.CreateDirectory(parsing_error_path);
                            //}

                            //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZNEL ");
                            //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNEL_SUBwithContinue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            continue;
                        }
                    }
                }
                #endregion

                #region[M3UA BASED LINKS]
                if (NE_Type == "MGW")
                {
                    temp_val2 = Regex.Split(temp2, "\r\n");
                    if (!string.IsNullOrEmpty(temp2))
                    {
                        for (int i = 0; i <= temp_val2.Length - 1; i++)
                        {
                            try
                            {
                                if (temp_val2[i].Contains("AV-") || temp_val2[i].Contains("UA-"))
                                {
                                    newdt.Clear();
                                    text = "";
                                    data = "";
                                    data = temp_val2[i].Trim();
                                    string[] data_split = Regex.Split(data, " ");
                                    for (int j = 0; j <= data_split.Length - 1; j++)
                                    {
                                        if (data_split[j] != "")
                                        {
                                            text = text + data_split[j].Trim() + ";";
                                        }
                                    }
                                    if (text != "")
                                    {
                                        value = Regex.Split(text, ";");
                                        LINK = value[0];
                                        LINK_SET = value[1] + " " + value[2];
                                        ASSOCIATION_SET = value[3];
                                        LINK_STATE = value[4];
                                        text = "";
                                        nextQuery = "select ET from mss_mgw_map_" + CircleName + "_" + group_name + "  where NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' AND circle = '" + CircleName + "' and ET = '" + PCM_TSL + "' ";
                                        SQLQuery(ref conn, ref dt, nextQuery);

                                        //if (dt.Rows.Count <= 0)
                                        //code change by rahul
                                        if (dt.Rows.Count >= 0)
                                        {
                                            query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + "  (`VENDOR`,`CIRCLE`,`MGW_NAME`,`NODE_TYPE`,`EXTRA_ET_FLAG`,`LINK`,`LINKSET`,`ET`,`STATE`,`ASSOCIATION_SET`) values ('" + _vendor + "','" + CircleName + "','" + _ne_name + "','" + _NEType + "','1','" + LINK + "','" + LINK_SET + "','" + PCM_TSL + "','" + LINK_STATE + "','" + ASSOCIATION_SET + "')";
                                            ExecuteSQLQuery(ref conn, query);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNEL_SUBwithConitnue()", ErrorMsg, "", ref FileError);

                                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                                ExecuteSQLQuery(ref conn, ErrorQuery);
                                continue;
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNEL()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZR2O(string output, string CircleName, string NE_Type)
        {
            string ErrorMsg = string.Empty;
            string Vendor = string.Empty;
            string Circle = string.Empty;
            string query = string.Empty;
            try
            {
                query = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set ET=Case when (Length(Term_id)<=4) Then Term_id End,APCM=Case when (Length(Term_id)>4) Then Term_id End where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "'  and MGW_NAME ='" + _ne_name.Trim() + "'";
                ExecuteSQLQuery(ref conn, query);

                ZR20_update_ET(output, _CircleName, NE_Type);
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZR2O()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZUSI_NIWU(string command_output, string CircleName, string NE_Type)
        {
            string[] temp;
            string[] value;
            string[] value1;
            string[] value2;
            string ET = string.Empty;
            string ETGR = string.Empty;

            string UNIT_NIWU = string.Empty;
            string temp_output = string.Empty;
            string query = string.Empty;
            string output = command_output.Trim();
            string temp_value = string.Empty;
            int i = 0;
            try
            {
                temp = Regex.Split(output, "\r\n");

                int size = temp.Length - 1;

            endhere: for (; i <= size; i++)
                {
                    try
                    {
                        if (temp[i].Contains("NIWU") && !temp[i].Contains("ZUSI:NIWU"))  // condition to ignore all previous values before "IWS1E" in the output 
                        {
                            while (temp[i].ToString() != "COMMAND EXECUTED")
                            {



                            nextIWS1E: if (temp[i].Contains("NIWU"))
                                {

                                    value = Regex.Split(temp[i].ToString().Trim(), " ");
                                    UNIT_NIWU = value[0].Trim();
                                }


                            comehere: if (temp[i].Contains("ETGR"))
                                {

                                    value1 = Regex.Split(temp[i].ToString().Trim(), " ");
                                    value1 = Regex.Split(value1[0], "-");
                                    ETGR = value1[1];
                                    i = i + 1;


                                    if (temp[i].Contains("ET"))
                                    {
                                        while (temp[i] != "ETGR")
                                        {

                                            if (temp[i].Contains("NIWU"))
                                            {
                                                goto nextIWS1E;
                                            }

                                            if (temp[i].Contains("COMMAND EXECUTED"))
                                            {
                                                i = size;
                                                goto endhere;
                                            }

                                            if (temp[i].Contains("ET") && !temp[i].Contains("SET"))
                                            {
                                                value2 = Regex.Split(temp[i].ToString().Trim(), " ");
                                                value2 = Regex.Split(value2[0], "-");
                                                ET = value2[1];



                                                if (ET.Length <= 4)
                                                {
                                                    query = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set UNIT = '" + UNIT_NIWU + "' , ETGR_VETGR = '" + ETGR + "' where ET = '" + ET + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
                                                }
                                                else
                                                {
                                                    query = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set UNIT = '" + UNIT_NIWU + "' , ETGR_VETGR = '" + ETGR + "' where APCM = '" + ET + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
                                                }

                                                ExecuteSQLQuery(ref conn, query);

                                                if (temp[i].Contains("ETGR"))
                                                {
                                                    goto comehere;
                                                }

                                                if (temp[i].Contains("COMMAND EXECUTED"))
                                                {
                                                    i = size;
                                                    goto endhere;
                                                }

                                            }
                                            i = i + 1;

                                        }

                                    }


                                    i = i + 1;

                                }

                                i = i + 1;

                            }

                        }


                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZUSI_NIWU ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZUSI_NIWU_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }


                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZUSI_NIWU()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZUSI_IWS1E(string command_output, string CircleName, string NE_Type)
        {
            string[] temp;
            string[] value;
            string[] value1;
            string[] value2;
            string ET = string.Empty;
            string VETGR = string.Empty;
            string STER = string.Empty;
            string UNIT_IWS1E = string.Empty;
            string temp_output = string.Empty;
            string[] temp1;
            string query = string.Empty;
            string output = command_output.Trim();
            string temp_value = string.Empty;
            int i = 0;

            try
            {

                temp = Regex.Split(output, "\r\n");

                int size = temp.Length - 1;

            endhere: for (; i <= size; i++)
                {
                    try
                    {
                        if ((temp[i].Contains("IWS1E") || temp[i].Contains("IWSEP")) && (!temp[i].Contains("ZUSI:IWS1E::FULL") && !temp[i].Contains("ZUSI:IWSEP::FULL")))
                        {


                            while (temp[i].ToString() != "COMMAND EXECUTED")
                            {
                            nextIWS1E: if (temp[i].Contains("IWS1E") || temp[i].Contains("IWSEP"))
                                {
                                    value = Regex.Split(temp[i].ToString().Trim(), " ");
                                    UNIT_IWS1E = value[0].Trim();
                                }

                                if (temp[i].Contains("STER") || temp[i].Contains("NPUP"))
                                {
                                    temp1 = Regex.Split(temp[i].ToString().Trim(), " ");
                                    temp1 = Regex.Split(temp1[0], "-");
                                    STER = temp1[1];
                                }


                            comehere: if (temp[i].Contains("VETGR") || temp[i].Contains("LETGR"))
                                {
                                    value1 = Regex.Split(temp[i].ToString().Trim(), " ");
                                    value1 = Regex.Split(value1[0], "-");
                                    VETGR = value1[1];
                                    i = i + 1;

                                    if (temp[i].Contains("ET"))
                                    {
                                        while (temp[i] != "LETGR" || temp[i] != "VETGR")
                                        {
                                            if (temp[i].Contains("IWS1E") || temp[i].Contains("IWSEP"))
                                            {
                                                goto nextIWS1E;
                                            }

                                            if (temp[i].Contains("COMMAND EXECUTED"))
                                            {
                                                i = size;
                                                goto endhere;
                                            }

                                            if (temp[i].Contains("ET") && !temp[i].Contains("SET") && !temp[i].Contains("NPUP"))
                                            {
                                                value2 = Regex.Split(temp[i].ToString().Trim(), " ");
                                                value2 = Regex.Split(value2[0], "-");
                                                ET = value2[1];



                                                if (ET.Length <= 4)
                                                {
                                                    query = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set UNIT = '" + UNIT_IWS1E + "' ,STER = '" + STER + "', ETGR_VETGR = '" + VETGR + "' where ET = '" + ET + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
                                                }
                                                else
                                                {
                                                    query = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set UNIT = '" + UNIT_IWS1E + "' ,STER = '" + STER + "', ETGR_VETGR = '" + VETGR + "' where APCM = '" + ET + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
                                                }


                                                ExecuteSQLQuery(ref conn, query);

                                                if (temp[i].Contains("VETGR") || temp[i].Contains("LETGR"))
                                                {
                                                    goto comehere;
                                                }



                                            }

                                            i = i + 1;
                                        }
                                    }


                                }



                                i = i + 1;
                            }

                        }



                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZUSI_IW1SE ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZUSI_IWSIE_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZUSI_IWSIE()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZRCI_PRINT_4(string command_output, string CircleName, string NE_Type)
        {
            string[] output = Regex.Split(command_output.Trim(), "\r\n");
            int size = output.Length;
            string[] temp;
            string[] value;
            string[] temp1;
            string temp_data = string.Empty;
            string SqlStr = string.Empty;
            string PCM_TSL = string.Empty;
            string NCGR = string.Empty;
            string CGR = string.Empty;

            DataTable dt = new DataTable();

            for (int i = 0; i <= size - 1; i++)
            {
                try
                {
                end_of_file: if (output[i].Contains("CGR") && !output[i].Contains("ZRCI") && !output[i].Contains("COMMAND EXECUTED"))
                    {
                        while (!output[i].Contains("COMMAND EXECUTED"))
                        {
                        comehere: if (output[i].Contains("CGR"))
                            {
                                temp = Regex.Split(output[i], "    ");

                                for (int j = 0; j < temp.Length; j++)
                                {
                                    if (temp[j].Contains("CGR") && !temp[j].Contains("NCGR"))
                                    {
                                        j = j + 1;
                                        temp1 = Regex.Split(temp[j].ToString(), ":");
                                        CGR = temp1[1].Trim();
                                    }

                                    else if (temp[j].Contains("NCGR"))
                                    {
                                        j = j + 1;
                                        temp1 = Regex.Split(temp[j].ToString(), ":");

                                        NCGR = temp1[1].Trim();

                                    }
                                }
                            }


                            //if (output[i].Contains("PCM-TSL") && output[i].Contains("STATE") && !output[i].Contains("HGR"))
                            if (output[i].Contains("PCM-TSL") && output[i].Contains("STATE"))
                            {
                                while (!output[i].Contains("CGR :") && !output[i].Contains("COMMAND EXECUTED"))
                                {
                                    if (output[i].Trim() != "" && !output[i].Contains("HGR"))   //if (output[i].Trim() != "" && output[i].Contains("HGR"))
                                    {
                                        if (output[i].Trim() != "")
                                        {
                                            value = Regex.Split(output[i].ToString(), "  ");
                                            value = Regex.Split(value[0].ToString(), "-");
                                            PCM_TSL = value[0];

                                            if (PCM_TSL.Length <= 4)
                                            {
                                                SQLQuery(ref conn, ref dt, "select MGW_CGR,MGW_NCGR,ET from mss_mgw_map_" + CircleName + "_" + group_name + "  where ET = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + CircleName + "' ");
                                            }
                                            else
                                            {
                                                SQLQuery(ref conn, ref dt, "select MGW_CGR,MGW_NCGR,ET from mss_mgw_map_" + CircleName + "_" + group_name + "  where APCM = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + CircleName + "' ");
                                            }

                                            if (dt.Rows.Count > 0)
                                            {
                                                if (PCM_TSL.Length <= 4)
                                                {
                                                    if (CGR.ToString() != "247")
                                                    {
                                                        SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_CGR='" + CGR + "', MGW_NCGR= '" + NCGR + "' Where ET = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + CircleName + "' ";
                                                    }
                                                }

                                                else
                                                {
                                                    if (CGR.ToString() != "247")
                                                    {
                                                        SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_CGR='" + CGR + "', MGW_NCGR= '" + NCGR + "' Where apcm = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + _CircleName + "' ";
                                                    }

                                                }

                                                if (SqlStr != "")
                                                {
                                                    ExecuteSQLQuery(ref conn, SqlStr);
                                                    SqlStr = "";
                                                }

                                            }


                                        }
                                    }

                                    i = i + 1;
                                }

                                if (output[i].Contains("CGR :"))
                                {
                                    goto comehere;
                                }

                                if (output[i].Contains("COMMAND EXECUTED"))
                                {
                                    goto end_of_file;
                                }

                            }



                            i = i + 1;
                        }


                    }

                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZRCI_PRINT_4 ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_4_withContinue()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);
                    continue;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZJVI_2(string command_output, string CircleName, string NE_Type)
        {

            string output = command_output.Trim();
            int size = output.Length;
            string[] temp = { };
            string[] temp2;
            string temp1 = string.Empty;


            string[] value1;

            string element_name = string.Empty;
            string value = string.Empty;
            string value2 = string.Empty;
            string VMGW_ID = string.Empty;
            string CGR_GROUP = string.Empty;
            string VMGW_NAME = string.Empty;
            string VMGW_CTRL_ISU = string.Empty;
            string STATUS = string.Empty;
            string query = string.Empty;
            string query2 = string.Empty;
            string query3 = string.Empty;
            DataTable dt;

            string own_address = string.Empty;

            try
            {
                if (output.Contains("VIRTUAL MGW DATA"))
                {
                    temp = Regex.Split(output, "VIRTUAL MGW DATA");
                }

                if (temp.Length >= 2)
                {
                    temp1 = temp[1];
                }
                temp2 = Regex.Split(temp1.ToString(), "\n");


                for (int i = 0; i < temp2.Length; i++)
                {
                    try
                    {
                        if (temp2[i].Contains("VMGW ID"))
                        {
                            value = temp2[i];
                            value1 = Regex.Split(value, ":");
                            VMGW_ID = value1[1].Trim();

                        }

                        if (temp2[i].Contains("VMGW NAME"))
                        {
                            value = temp2[i];
                            value1 = Regex.Split(value, ":");
                            value2 = value1[1].Trim();
                            VMGW_NAME = value2;
                        }

                        if (temp2[i].Contains("MASTER ISU"))
                        {
                            value = temp2[i];
                            value1 = Regex.Split(value, ":");
                            value2 = value1[1].Trim();
                            VMGW_CTRL_ISU = value2;

                        }

                        if (temp2[i].Contains("OWN ADDRESS"))
                        {
                            value = temp2[i];
                            value1 = Regex.Split(value, ":");
                            own_address = value1[1].Trim();
                        }

                        if (temp2[i].Contains("CIRCUIT GROUP"))
                        {
                            value = temp2[i];
                            value1 = Regex.Split(value, ":");
                            value2 = value1[1].Trim();
                            value1 = Regex.Split(value2, " ");

                            foreach (string element in value1)
                            {
                                if (element != "")
                                {
                                    CGR_GROUP = CGR_GROUP.Trim();
                                    CGR_GROUP = "'" + element + "'," + CGR_GROUP;
                                }
                            }

                            CGR_GROUP = CGR_GROUP.Remove(CGR_GROUP.Length - 1, 1).ToString().Trim();
                        }

                        if (temp2[i].Contains("STATUS")) //Added by AJAY
                        {
                            value = temp2[i];
                            value1 = Regex.Split(value, " ........ ");
                            STATUS = value1[1].Trim();
                        }

                        if (VMGW_CTRL_ISU != "" && VMGW_NAME != "" && CGR_GROUP != "" && VMGW_ID != "" && own_address != "" && STATUS != "")
                        {

                            #region New Code for Insert Query
                            dt = new DataTable();
                            query3 = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,`MGW_VMGW_ID`,`MGW_VMGW`,`MGW_VMGW_CTRL_ISU`,`REGISTRATION_STATUS` from  mss_mgw_map_" + _CircleName + "_" + group_name + " where MGW_NAME='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                            SQLQuery(ref conn, ref dt, query3);
                            if (dt.Rows.Count >= 1)
                            {
                                query3 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,`MGW_VMGW_ID`,`MGW_VMGW`,`MGW_VMGW_CTRL_ISU`,`REGISTRATION_STATUS`) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + VMGW_ID.Trim() + "', '" + VMGW_NAME.Trim() + "', '" + VMGW_CTRL_ISU.Trim() + "', '" + STATUS.Trim() + "')";
                                ExecuteSQLQuery(ref conn, query3);
                            }
                            else if (dt.Rows.Count <= 0)
                            {
                                query3 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,`MGW_VMGW_ID`,`MGW_VMGW`,`MGW_VMGW_CTRL_ISU`,`REGISTRATION_STATUS`) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + VMGW_ID.Trim() + "', '" + VMGW_NAME.Trim() + "', '" + VMGW_CTRL_ISU.Trim() + "', '" + STATUS.Trim() + "')";
                                ExecuteSQLQuery(ref conn, query3);
                            }
                            #endregion
                            else
                            {
                                query = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_VMGW = '" + VMGW_NAME + "' ,  MGW_VMGW_CTRL_ISU = '" + VMGW_CTRL_ISU + "' ,  REGISTRATION_STATUS = '" + STATUS + "' where VMGW_DEST_IP = '" + own_address + "' AND circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
                                query2 = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_VMGW_ID = '" + VMGW_ID + "' where VMGW_DEST_IP = '" + own_address + "' And CGR in (" + CGR_GROUP + ") AND circle = '" + CircleName + "' AND MGW_NAME = '" + _ne_name + "' ";
                                ExecuteSQLQuery(ref conn, query);
                                ExecuteSQLQuery(ref conn, query2);
                            }

                            VMGW_ID = "";
                            CGR_GROUP = "";
                            VMGW_NAME = "";
                            VMGW_CTRL_ISU = "";
                            own_address = "";
                            STATUS = "";
                        }

                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZJVI_2 ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJVI_2_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJVI_2()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }
        //public void ParseData_ZJVI_2(string command_output, string CircleName, string NE_Type)
        //{

        //    string output = command_output.Trim();
        //    int size = output.Length;
        //    string[] temp = { };
        //    string[] temp2;
        //    string temp1 = string.Empty;


        //    string[] value1;

        //    string element_name = string.Empty;
        //    string value = string.Empty;
        //    string value2 = string.Empty;
        //    string VMGW_ID = string.Empty;
        //    string CGR_GROUP = string.Empty;

        //    string VMGW_NAME = string.Empty;
        //    string VMGW_CTRL_ISU = string.Empty;
        //    string query = string.Empty;
        //    string query2 = string.Empty;

        //    string own_address = string.Empty;

        //    try
        //    {
        //        if (output.Contains("VIRTUAL MGW DATA"))
        //        {
        //            temp = Regex.Split(output, "VIRTUAL MGW DATA");
        //        }

        //        if (temp.Length >= 2)
        //        {
        //            temp1 = temp[1];
        //        }
        //        temp2 = Regex.Split(temp1.ToString(), "\n");


        //        for (int i = 0; i < temp2.Length; i++)
        //        {
        //            try
        //            {
        //                if (temp2[i].Contains("VMGW ID"))
        //                {
        //                    value = temp2[i];
        //                    value1 = Regex.Split(value, ":");
        //                    VMGW_ID = value1[1].Trim();

        //                }

        //                if (temp2[i].Contains("VMGW NAME"))
        //                {
        //                    value = temp2[i];
        //                    value1 = Regex.Split(value, ":");
        //                    value2 = value1[1].Trim();
        //                    VMGW_NAME = value2;

        //                }

        //                if (temp2[i].Contains("MASTER ISU"))
        //                {
        //                    value = temp2[i];
        //                    value1 = Regex.Split(value, ":");
        //                    value2 = value1[1].Trim();
        //                    VMGW_CTRL_ISU = value2;

        //                }

        //                if (temp2[i].Contains("OWN ADDRESS"))
        //                {
        //                    value = temp2[i];
        //                    value1 = Regex.Split(value, ":");
        //                    own_address = value1[1].Trim();


        //                }

        //                if (temp2[i].Contains("CIRCUIT GROUP"))
        //                {
        //                    value = temp2[i];
        //                    value1 = Regex.Split(value, ":");
        //                    value2 = value1[1].Trim();
        //                    value1 = Regex.Split(value2, " ");

        //                    foreach (string element in value1)
        //                    {
        //                        if (element != "")
        //                        {
        //                            CGR_GROUP = CGR_GROUP.Trim();
        //                            CGR_GROUP = "'" + element + "'," + CGR_GROUP;
        //                        }
        //                    }

        //                    CGR_GROUP = CGR_GROUP.Remove(CGR_GROUP.Length - 1, 1).ToString().Trim();
        //                }


        //                if (VMGW_CTRL_ISU != "" && VMGW_NAME != "" && CGR_GROUP != "" && VMGW_ID != "" && own_address != "")
        //                {
        //                    query = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_VMGW = '" + VMGW_NAME + "' ,  MGW_VMGW_CTRL_ISU = '" + VMGW_CTRL_ISU + "' where VMGW_DEST_IP = '" + own_address + "' AND circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
        //                    query2 = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_VMGW_ID = '" + VMGW_ID + "' where VMGW_DEST_IP = '" + own_address + "' And CGR in (" + CGR_GROUP + ") AND circle = '" + CircleName + "' AND MGW_NAME = '" + _ne_name + "' ";
        //                    ExecuteSQLQuery(ref conn, query);
        //                    ExecuteSQLQuery(ref conn, query2);
        //                    VMGW_ID = "";
        //                    CGR_GROUP = "";
        //                    VMGW_NAME = "";
        //                    VMGW_CTRL_ISU = "";
        //                    own_address = "";

        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
        //                //if (!Directory.Exists(parsing_error_path))
        //                //{
        //                //    Directory.CreateDirectory(parsing_error_path);
        //                //}

        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZJVI_2 ");
        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        //                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJVI_2_SUBwithContinue()", ErrorMsg, "", ref FileError);

        //                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //                ExecuteSQLQuery(ref conn, ErrorQuery);
        //                continue;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJVI_2()", ErrorMsg, "", ref FileError);

        //        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //        ExecuteSQLQuery(ref conn, ErrorQuery);
        //    }

        //}

        /// <summary>
        /// Parsing Data Of Both ZNRI:NA0; and ZNRI:NA1; Commands For MGW
        /// </summary>
        /// <param name="GIGBtxt"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        /// <param name="NET"></param>
        public void ParseData_ZNRI_MGW(string[] GIGBtxt, string _CircleName, string NE_Type, string NET)
        {
            string[] temp;
            string[] temp1;
            string SqlStr = string.Empty;
            string MGW_spc = string.Empty;
            string link_name = string.Empty;
            string SP_Type = string.Empty;
            string SS7_Stand = string.Empty;
            string SubField_Count = string.Empty;
            string SubField_Lnths = string.Empty;
            string SubField_Bit = string.Empty;
            string STATE = string.Empty;
            string PRIO = string.Empty;
            string PAR_SET = string.Empty;
            string CGR_NET = string.Empty;
            DataTable dt = new DataTable();

            for (int i = 0; i < GIGBtxt.Length; i++)
            {
                try
                {
                    //code change on 13-07-2018 by Rahul Kumar
                    #region [output contains Routes]
                    if (GIGBtxt[i].Trim().ToString().Contains("ROUTES:"))
                    {
                        i = i + 2;
                        while (GIGBtxt[i] != "COMMAND EXECUTED" && i <= GIGBtxt.Length - 1 && GIGBtxt[i].Trim() != "")
                        {
                            temp = GIGBtxt[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            MGW_spc = temp[0].Trim().ToString();
                            link_name = temp[1].Trim().ToString();
                            STATE = temp[2].Trim().ToString();
                            PRIO = temp[3].Trim().ToString();
                            //if (MGW_spc != "" || MGW_spc != null)
                            if (!string.IsNullOrEmpty(MGW_spc))
                            {
                                SqlStr = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,ifnull(NE_NAME,'') as NE_NAME ,ifnull(MCC,'') as MCC,ifnull(MNC,'') as MNC,ifnull(MSS,'') as MSS,ifnull(ELEMENT_IP,'') as ELEMENT_IP,ifnull(CGR_NET,'') as CGR_NET,ifnull(MGW_C_NO,'') as MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                //SqlStr = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                SQLQuery(ref conn, ref dt, SqlStr);
                                if (dt.Rows.Count <= 0)
                                {
                                    SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,MGW_SPC,LINKSET,STATE,PRIO) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + MGW_spc + "','" + link_name + "','" + STATE + "','" + PRIO + "')";
                                    
                                    ExecuteSQLQuery(ref conn, SqlStr);
                                    link_name = STATE = PRIO = "";
                                }
                                else if (dt.Rows.Count >= 1)
                                {
                                    SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO,MGW_SPC,LINKSET,STATE,PRIO) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["MGW_C_NO"] + "','" + MGW_spc + "','" + link_name + "','" + STATE + "','" + PRIO + "')";
                                    ExecuteSQLQuery(ref conn, SqlStr);
                                    link_name = STATE = PRIO = "";
                                }
                                i = i + 1;
                                if (i > GIGBtxt.Length - 1)
                                {
                                    break;
                                }
                            }
                            MGW_spc = "";
                        }
                    }
                    #endregion

                    #region [output data contains SP CODE H/D && NET]
                    if (GIGBtxt[i].ToString().Trim().Contains("SP CODE H/D") && GIGBtxt[i].ToString().Trim().Contains("NET"))
                    {
                        i = i + 2;
                        #region [output data contains NET && !OWN SP]
                        if (GIGBtxt[i].Contains(NET) && !GIGBtxt[i].Contains("OWN SP"))
                        {
                            temp = GIGBtxt[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            CGR_NET= temp[0].Trim().ToString();
                            MGW_spc = temp[1].Trim().ToString();
                            link_name = temp[2].Trim().ToString();
                            STATE = temp[3].Trim().ToString();
                            PAR_SET = temp[4].Trim().ToString();
                            if (!string.IsNullOrEmpty(MGW_spc))
                            {
                                SqlStr = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,ifnull(NE_NAME,'') as NE_NAME ,ifnull(MCC,'') as MCC,ifnull(MNC,'') as MNC,ifnull(MSS,'') as MSS,ifnull(ELEMENT_IP,'') as ELEMENT_IP,ifnull(CGR_NET,'') as CGR_NET,ifnull(MGW_C_NO,'') as MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                //SqlStr = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                SQLQuery(ref conn, ref dt, SqlStr);
                                if (dt.Rows.Count <= 0)
                                {                                    
                                    SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,CGR_NET,MGW_SPC,LINKSET,STATE,PAR_SET) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + CGR_NET + "','" + MGW_spc + "','" + link_name + "','" + STATE + "','" + PAR_SET + "')";
                                    //SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO,CGR_SPC,LINKSET,STATE,PAR_SET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["MGW_C_NO"] + "','" + MGW_spc + "','" + link_name + "','" + STATE + "','" + PAR_SET + "')";
                                    ExecuteSQLQuery(ref conn, SqlStr);
                                    link_name = STATE = CGR_NET= PAR_SET = "";
                                }
                                else if (dt.Rows.Count >= 1)
                                {                                    
                                    SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO,MGW_SPC,LINKSET,STATE,PAR_SET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["MGW_C_NO"] + "','" + MGW_spc + "','" + link_name + "','" + STATE + "','" + PAR_SET + "')";
                                    ExecuteSQLQuery(ref conn, SqlStr);
                                    link_name = STATE = CGR_NET = PAR_SET = "";
                                }
                            }
                            MGW_spc = "";
                        }
                        #endregion

                        #region [output data contains NET && OWN SP]
                        if (GIGBtxt[i].Contains(NET) && GIGBtxt[i].Contains("OWN SP"))
                        {
                            temp1 = GIGBtxt[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            CGR_NET = temp1[0].Trim().ToString();
                            MGW_spc = temp1[1].Trim().ToString();
                            link_name = temp1[2].Trim().ToString();
                            SP_Type = temp1[3].Trim().ToString();
                            SS7_Stand = temp1[4].Trim().ToString();
                            SubField_Count = temp1[5].Trim().ToString();
                            SubField_Bit = temp1[6].Trim().ToString();
                            SubField_Lnths = temp1[7].Trim().ToString();
                            if (!string.IsNullOrEmpty(MGW_spc))
                            {
                                //SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name + " set MGW_SPC='" + MGW_spc.Trim() + "' LINKSET='" + link_name + "', SP_TYPE = '" + SP_Type + "', SS7_STAND='" + SS7_Stand + "', SUB_FIELD_INFO_COUNT='" + SubField_Count + "', SUB_FIELD_INFO_BIT ='" + SubField_Bit + "', SUB_FIELD_INFO_LENGTHS ='" + SubField_Lnths + "' Where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";

                                //SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name + " set MGW_SPC='" + MGW_spc.Trim() + "' LINKSET='" + link_name + "', SP_TYPE ='" + SP_Type + "',";
                                //SqlStr += "SS7_STAND='" + SS7_Stand + "', SUB_FIELD_INFO_COUNT='" + SubField_Count + "', SUB_FIELD_INFO_BIT ='" + SubField_Bit + "', SUB_FIELD_INFO_LENGTHS ='" + SubField_Lnths + "',";
                                //SqlStr += " Where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                                //ExecuteSQLQuery(ref conn, SqlStr);


                                SqlStr = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,ifnull(NE_NAME,'') as NE_NAME ,ifnull(MCC,'') as MCC,ifnull(MNC,'') as MNC,ifnull(MSS,'') as MSS,ifnull(ELEMENT_IP,'') as ELEMENT_IP,ifnull(CGR_NET,'') as CGR_NET,ifnull(MGW_C_NO,'') as MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";

                                //SqlStr = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                SQLQuery(ref conn, ref dt, SqlStr);
                                if (dt.Rows.Count <= 0)
                                {

                                    SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,CGR_NET,MGW_SPC,LINKSET,SP_TYPE,SS7_STAND,SUB_FIELD_INFO_COUNT,SUB_FIELD_INFO_BIT,SUB_FIELD_INFO_LENGTHS) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + CGR_NET + "','" + MGW_spc + "','" + link_name + "','" + SP_Type + "','" + SS7_Stand + "','" + SubField_Count + "','" + SubField_Bit + "','" + SubField_Lnths + "')";                                    
                                    ExecuteSQLQuery(ref conn, SqlStr);
                                    link_name = SP_Type = SS7_Stand = SubField_Count = SubField_Bit = CGR_NET= SubField_Lnths = "";
                                }
                                else if (dt.Rows.Count >= 1)
                                {                                    
                                    SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO,MGW_SPC,LINKSET,SP_TYPE,SS7_STAND,SUB_FIELD_INFO_COUNT,SUB_FIELD_INFO_BIT,SUB_FIELD_INFO_LENGTHS) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["MGW_C_NO"] + "','" + MGW_spc + "','" + link_name + "','" + SP_Type + "','" + SS7_Stand + "','" + SubField_Count + "','" + SubField_Bit + "','" + SubField_Lnths + "')";
                                    ExecuteSQLQuery(ref conn, SqlStr);
                                    link_name = SP_Type = SS7_Stand = SubField_Count = SubField_Bit = CGR_NET = SubField_Lnths = "";
                                }
                            }

                            MGW_spc = "";

                        }
                        #endregion
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZNRI_MGW ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNRI_MGW_withContinue()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);
                    continue;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GIGBtxt"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZW7N(string[] GIGBtxt, string CircleName, string NE_Type)
        {
            string ErrorMsg = string.Empty;
            string Vendor = string.Empty;
            string Circle = string.Empty;
            string MGW_C_NO = string.Empty;
            string SqlStr = string.Empty; ;
            string[] FindResult;

            for (int m = 0; m <= GIGBtxt.Length - 1; m++)
            {
                try
                {
                    if (GIGBtxt[m].Trim().ToString().IndexOf("TARGET IDENTIFIER") != -1)
                    {
                        FindResult = GIGBtxt[m].Split(':');
                        MGW_C_NO = FindResult[1].Trim().ToString();

                        if (MGW_C_NO != "")
                        {
                            SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_C_NO='" + MGW_C_NO.Trim() + "' Where MGW_Name='" + _ne_name.Trim() + "' and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' ";
                            ExecuteSQLQuery(ref conn, SqlStr);
                        }
                        MGW_C_NO = "";
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZW7N ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7N-WithContinue()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);
                    continue;
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZJVI_1(string output, string CircleName, string NE_Type)
        {
            string ErrorMsg = string.Empty;
            string Vendor = string.Empty;
            string Circle = string.Empty;
            // string pattern = "\\\\b";
            string MGW_SOURCEIP = string.Empty;
            string VMGW_ID = string.Empty;
            string VMGW = string.Empty;
            string SqlStr = string.Empty;
            string date1 = System.DateTime.Now.ToString("yyyy_MM_dd");
            string[] FindResult;
            string[] temp;
            string query = string.Empty;
            DataTable dt;

            try
            {
                string[] cmnd_log = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);


                for (int i = 0; i <= cmnd_log.Length - 1; i++)
                {
                    try
                    {

                        if (cmnd_log[i].Contains("COMMAND EXECUTED"))
                        {
                            break;
                        }
                        if (!cmnd_log[i].Contains("COMMAND EXECUTED"))
                        {
                            if (cmnd_log[i].Contains("MGW") && cmnd_log[i].Contains(System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")))
                            {
                                FindResult = null;
                                FindResult = cmnd_log[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                _ne_name = FindResult[1].Trim().ToString();
                            }

                            if (cmnd_log[i].Contains("OWN ADDRESS"))
                            {
                                temp = null;
                                temp = cmnd_log[i].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                                MGW_SOURCEIP = temp[1].ToString().Trim();
                            }

                            if (MGW_SOURCEIP != "" && _ne_name != "")
                            {

                                #region New Code for Insert Query
                                dt = new DataTable();
                                query = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,`MGW_Source_Map_IP` from  mss_mgw_map_" + _CircleName + "_" + group_name + " where MGW_NAME='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                SQLQuery(ref conn, ref dt, query);
                                if (dt.Rows.Count >= 1)
                                {
                                    query = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,`MGW_Source_Map_IP`) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + MGW_SOURCEIP.Trim() + "')";
                                    ExecuteSQLQuery(ref conn, query);
                                }
                                else if (dt.Rows.Count <= 0)
                                {
                                    query = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NODE_TYPE,`MGW_Source_Map_IP`) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + MGW_SOURCEIP.Trim() + "')";
                                    ExecuteSQLQuery(ref conn, query);
                                }
                                #endregion
                                else
                                {
                                    SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set MGW_NAME='" + _ne_name.Trim() + "', MGW_Source_Map_IP='" + MGW_SOURCEIP.Trim() + "' Where VMGW_DEST_IP='" + MGW_SOURCEIP.Trim() + "'  and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                                    ExecuteSQLQuery(ref conn, SqlStr);
                                }                                
                                MGW_SOURCEIP = "";
                                VMGW = "";

                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZJVI_1 ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJVI_1_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJVI_1()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }
        #endregion

        // MSS Parsing Function
        #region [MSS Parsing Functions]
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZEDO(string output, string CircleName, string NE_Type)
        {
            string cgr_spc = "";
            int lac_count;
            string previous_lac = string.Empty;
            string mss_lac = string.Empty;
            string newquery = string.Empty;
            string LAC_VALUE = string.Empty;
            string BSC_NAME = string.Empty;
            string[] temp1;
            string data = string.Empty;
            string temp_data = string.Empty;
            string BSC_NUMBER = string.Empty;
            string BSC_OPERATIONAL_STATE = string.Empty;
            try
            {
                if (output.Contains("BSC NAME"))
                {
                    data = output.Remove(0, output.IndexOf("BSC NAME"));
                }

                data = data.Trim();
                string[] temp;
                string[] cmnd_data = Regex.Split(data.Trim(), "BSC NAME");

                for (int i = 0; i <= cmnd_data.Length - 1; i++)
                {
                    try
                    {
                        BSC_NAME = "";
                        BSC_NUMBER = "";
                        BSC_OPERATIONAL_STATE = "";
                        cgr_spc = "";
                        lac_count = 0;
                        LAC_VALUE = "";

                        if (cmnd_data[i] != " " && cmnd_data[i] != "")
                        {
                            temp = Regex.Split(cmnd_data[i].Trim(), "\r\n");

                            for (int j = 0; j <= temp.Length - 1; j++)
                            {
                                if (BSC_NAME == "")
                                {
                                    temp1 = Regex.Split(temp[0].Trim(), ":");
                                    BSC_NAME = temp1[1].Trim();
                                }

                                if (temp[j].Contains("BSC NUMBER"))
                                {
                                    temp1 = Regex.Split(temp[j].Trim(), ":");
                                    BSC_NUMBER = temp1[1].Trim();
                                }

                                if (temp[j].Contains("BSC OPERATIONAL STATE"))
                                {
                                    temp1 = Regex.Split(temp[j].Trim(), ":");
                                    BSC_OPERATIONAL_STATE = temp1[1].Trim();
                                }


                                if (temp[j].Contains("SIGNALLING POINT CODE"))
                                {
                                    temp1 = Regex.Split(temp[j].Trim(), ":");
                                    cgr_spc = temp1[1].Trim();
                                    if (cgr_spc != "-")
                                        cgr_spc = cgr_spc.Substring(0, 4).ToString().Trim();   // some times cgr spc conains "(HEX)" that should be removed
                                }


                                if (temp[j].Contains("BTSs UNDER BSC"))
                                {
                                    j = j + 1;
                                endhere: while (j < temp.Length)
                                    {
                                        if (temp[j].Contains("COMMAND EXECUTED"))
                                        {
                                            j = temp.Length;
                                            goto endhere;
                                        }

                                        if (temp[j].Trim() != "" && !temp[j].Trim().Contains("BTS NAME") && !temp[j].Trim().Contains("LAC") && !temp[j].Trim().Contains("-----"))
                                        {
                                            temp_data = Regex.Replace(temp[j].Trim(), " {2,}", "~");
                                            temp1 = Regex.Split(temp_data.Trim(), "~");

                                            if (temp1.Length > 2)
                                            {
                                                mss_lac = temp1[2].Trim().ToString();
                                            }


                                            if (mss_lac != previous_lac)
                                            {
                                                LAC_VALUE = mss_lac + "," + LAC_VALUE;
                                                lac_count = 1;
                                            }


                                            previous_lac = mss_lac;


                                            if (BSC_NAME != "" && BSC_NUMBER != "" && BSC_OPERATIONAL_STATE != "" && lac_count == 1 && cgr_spc != "")
                                            {
                                                LAC_VALUE = LAC_VALUE.Trim().TrimEnd(',');
                                                if (NE_Type == "MSS")
                                                {
                                                    newquery = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set BSC_NAME = '" + BSC_NAME + "', BSC_STATE = '" + BSC_OPERATIONAL_STATE + "', BSC_NUMBER = '" + BSC_NUMBER + "', LAC = '" + LAC_VALUE + "' where NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + CircleName + "' AND CGR_SPC like '%" + cgr_spc + "/%'";
                                                }

                                                else if (NE_Type == "MSC")
                                                {
                                                    newquery = "update msc_" + CircleName + " set BSC_NAME = '" + BSC_NAME + "', BSC_STATE = '" + BSC_OPERATIONAL_STATE + "', BSC_NUMBER = '" + BSC_NUMBER + "', LAC = '" + LAC_VALUE + "' where NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + CircleName + "' AND CGR_SPC like '%" + cgr_spc + "/%'";
                                                }
                                                ExecuteSQLQuery(ref conn, newquery);

                                                BSC_NAME = "";
                                                BSC_NUMBER = "";
                                                BSC_OPERATIONAL_STATE = "";
                                                cgr_spc = "";
                                                lac_count = 0;
                                            }

                                        }

                                        j = j + 1;

                                    }

                                }

                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZEDO ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZEDO1_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZEDO1()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZRCI_PRINT_5(string output, string CircleName, string NE_Type)
        {
            #region ZRCI_PRINT_5
            //DataTable dt = new DataTable();
            //string temp1 = string.Empty; ;
            //string temp_data = string.Empty; ;
            //string[] value;
            //string[] term_id;
            //string[] temp_value;
            //string[] temp;
            //string[] value1;
            //string pattern = "\\bCGR\\b";
            //string cmnd_output = Regex.Replace(output, pattern, "CGR_VALUE");
            //string[] data = Regex.Split(cmnd_output, "CGR_VALUE");
            //string query = string.Empty; ;
            //string CGR = string.Empty; ;
            //string CGR_UPART = string.Empty; ;
            //string CGR_NET = string.Empty; ;

            //string CGR_SPC = string.Empty; ;
            //string NCGR = string.Empty; ;
            //string T_ID = string.Empty; ;
            //string CGR_TYPE = string.Empty; ;
            //string vmgw = string.Empty; ;
            //// string T_ID = string.Empty; ;

            //string CIC = string.Empty; ;


            //string check1 = string.Empty;
            //string check2 = string.Empty;

            //try
            //{
            //    for (int i = 2; i <= data.Length - 1; i++)
            //    {
            //        CGR = "";
            //        CGR_NET = "";
            //        CGR_SPC = "";
            //        CGR_TYPE = "";
            //        NCGR = "";
            //        vmgw = "";
            //        T_ID = "";
            //        CIC = "";
            //        CGR_UPART = "";
            //        check1 = "";
            //        check2 = "";

            //        #region [data not contain SPE]

            //        if (!data[i].Contains("SPE"))
            //        {

            //            temp1 = data[i].Trim().ToString();
            //            temp = Regex.Split(temp1, "\r\n");

            //            for (int j = 0; j <= temp.Length - 1; j++)
            //            {
            //                try
            //                {
            //                    if (temp[j].Contains("NCGR") && temp[j].Trim() != "")
            //                    {
            //                        temp_data = temp[j].Trim();
            //                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
            //                        temp_value = Regex.Split(temp_data, "~");
            //                        value = Regex.Split(temp_value[0].Trim(), ":");
            //                        value1 = Regex.Split(temp_value[2].Trim(), ":");
            //                        CGR = value[1].Trim();
            //                        NCGR = value1[1].Trim();
            //                    }

            //                    if (temp[j].Contains("TYPE") && temp[j].Contains("STATE"))
            //                    {
            //                        temp_data = temp[j].Trim().ToString();
            //                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
            //                        temp_value = Regex.Split(temp_data, "~");
            //                        CGR_TYPE = temp_value[1].Trim();
            //                        CGR_TYPE = CGR_TYPE.Replace(":", " ");
            //                        CGR_TYPE = CGR_TYPE.Trim();

            //                    }
            //                    if (temp[j].Contains("CGR      : 990             NCGR     : MZHRS2"))
            //                    {
            //                        temp_data = temp[j].Trim();
            //                    }
            //                    if (temp[j].Contains("SPC(H/D)") && temp[j].Contains("NET"))
            //                    {
            //                        temp_data = temp[j].Trim();
            //                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
            //                        temp_value = Regex.Split(temp_data, "~");

            //                        CGR_NET = temp_value[3].Trim();
            //                        CGR_NET = CGR_NET.Replace(":", " ");
            //                        CGR_NET = CGR_NET.Trim();

            //                        value = Regex.Split(temp_value[temp_value.Length - 1].Trim(), ":");
            //                        CGR_SPC = value[1].Trim().ToString();

            //                    }

            //                    if (temp[j].Contains("UPART"))
            //                    {
            //                        //temp_data = temp[j].Trim();
            //                        //temp_data = Regex.Replace(temp_data, " {2,}", "~");
            //                        //temp_value = Regex.Split(temp_data, "~");
            //                        //CGR_UPART = temp_value[3].Trim();

            //                        //CGR_UPART = CGR_UPART.Replace(":", " ");
            //                        //CGR_UPART = CGR_UPART.Trim();

            //                        temp_data = temp[j].Trim();
            //                        temp_value = Regex.Split(temp_data, ":");
            //                        if (temp_value.Length > 1)
            //                        {
            //                            CGR_UPART = temp_value[1].Trim();
            //                            CGR_UPART = CGR_UPART.Trim();
            //                        }
            //                    }

            //                    if (temp[j].Contains("MGW") && !temp[j].Contains("NCGR"))
            //                    {
            //                        temp_data = temp[j].Trim();
            //                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
            //                        temp_value = Regex.Split(temp_data, "~");
            //                        vmgw = temp_value[1].Replace(":", " ");
            //                        vmgw = vmgw.Trim();
            //                    }


            //                    if (temp[j].Contains("TERMID") && temp[j].Contains("HGR") && temp[j].Contains("UNIT"))
            //                    {
            //                        j = j + 1;

            //                    comehere: while (j != temp.Length) //&& j != temp.Length - 1)
            //                        {
            //                            if (temp[j].Trim() != "" && !temp[j].Trim().Contains("COMMAND EXECUTED") && !temp[j].Trim().Contains("MSCi"))
            //                            {
            //                                temp_data = temp[j].Trim();
            //                                temp_data = Regex.Replace(temp_data, " {2,}", "~");
            //                                temp_value = Regex.Split(temp_data, "~");
            //                                term_id = Regex.Split(temp_value[0].Trim(), "-");
            //                                T_ID = term_id[0].Trim();
            //                                CIC = temp_value[5].Trim().ToString();
            //                            }

            //                            if (temp[j].Contains("COMMAND EXECUTED"))
            //                            {
            //                                j = temp.Length;
            //                                goto comehere;
            //                            }

            //                            if (T_ID != check1 && CIC != check2)
            //                            {
            //                                check1 = T_ID;
            //                                check2 = CIC;

            //                                if (CGR != "" && CGR_NET != "" && CGR_SPC != "" && CGR_TYPE != "" && CGR_UPART != "" && NCGR != "" && T_ID != " " && CIC != " " && vmgw != " ")
            //                                {
            //                                    SQLQuery(ref conn, ref dt, "select distinct * from mss_mgw_map_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and cgr_net is null and cgr_spc is null and ncgr is null and cic is null");

            //                                    for (int l = 0; l <= dt.Rows.Count - 1; l++)
            //                                    {
            //                                        if (vmgw != "")
            //                                        {
            //                                            check1 = T_ID;
            //                                        }
            //                                        query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`,`CGR_NET`, `CGR_TYPE`, `CGR_UPART`, `TERM_ID`, `CIC`) values ";
            //                                        query += "('" + dt.Rows[l][0].ToString() + "','" + dt.Rows[l][1].ToString() + "','" + dt.Rows[l][2].ToString() + "','" + dt.Rows[l][3].ToString() + "','" + dt.Rows[l][4].ToString() + "','" + dt.Rows[l][5].ToString() + "','" + dt.Rows[l][6].ToString() + "','" + dt.Rows[l][7].ToString() + "','" + dt.Rows[l][8].ToString() + "','" + dt.Rows[l][9].ToString() + "','" + dt.Rows[l][10].ToString() + "','" + vmgw + "','" + dt.Rows[l][12].ToString() + "','" + dt.Rows[l][13].ToString() + "','" + dt.Rows[l][14].ToString() + "', '" + CGR + "', '" + NCGR + "', '" + CGR_SPC + "', '" + CGR_NET + "', '" + CGR_TYPE + "', '" + CGR_UPART + "', '" + T_ID + "','" + CIC + "')";

            //                                        ExecuteSQLQuery(ref conn, query);


            //                                    }
            //                                }

            //                            }
            //                            j = j + 1;
            //                        }
            //                    }
            //                }
            //                catch (Exception ex)
            //                {
            //                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
            //                    //if (!Directory.Exists(parsing_error_path))
            //                    //{
            //                    //    Directory.CreateDirectory(parsing_error_path);
            //                    //}

            //                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZRCI_PRINT5 ");
            //                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            //                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
            //                    oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_5_SUBwithConinue()", ErrorMsg, "", ref FileError);

            //                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
            //                    ExecuteSQLQuery(ref conn, ErrorQuery);
            //                    continue;
            //                }
            //            }
            //        }
            //        #endregion

            //    }

            //}
            //catch (Exception ex)
            //{
            //    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
            //    oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_5()", ErrorMsg, "", ref FileError);

            //    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
            //    ExecuteSQLQuery(ref conn, ErrorQuery);
            //}
            #endregion

            #region [ZRCI_PRINT_2]

            ParseData_ZRCI_PRINT_2(output, CircleName, NE_Type);

            #endregion
        }

        /// <summary>
        /// Parsing data for alarm 2087
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZRCI_PRINT_2(string output, string CircleName, string NE_Type)
        {
            DataTable dt;
            string temp1 = string.Empty; ;
            string temp_data = string.Empty; ;
            string[] value;
            string[] temp_value;
            string[] temp;
            string[] value1;
            string pattern = "\\bCGR\\b";
            string cmnd_output = Regex.Replace(output, pattern, "CGR_VALUE");
            string[] data = Regex.Split(cmnd_output, "CGR_VALUE");
            string query = string.Empty; ;
            string CGR = string.Empty; ;
            string CGR_UPART = string.Empty; ;
            string CGR_NET = string.Empty; ;
            string CGR_SPC = string.Empty; ;
            string NCGR = string.Empty; ;
            string CGR_TYPE = string.Empty; ;
            string vmgw = string.Empty; ;
            string NBCRCT = string.Empty; ;
            string FQDN = string.Empty; ;
            DataTable dt1;
            DataTable dt2;

            try
            {
                for (int i = 2; i <= data.Length - 1; i++)
                {
                    CGR = "";
                    CGR_NET = "";
                    CGR_SPC = "";
                    CGR_TYPE = "";
                    NCGR = "";
                    vmgw = "";
                    NBCRCT = "";
                    FQDN = "";

                    if (data[i] != null)
                    {
                        try
                        {

                            temp1 = data[i].Trim().ToString();
                            temp = Regex.Split(temp1, "\r\n");

                            #region[getting NCGR,NBCRT,CGR,CGR_SPC,vmgw Values from output]
                            for (int j = 0; j <= temp.Length - 1; j++)
                            {
                                try
                                {
                                    if (temp[j].Contains("NCGR") && temp[j].Trim() != "")
                                    {
                                        temp_data = temp[j].Trim();
                                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                        temp_value = Regex.Split(temp_data, "~");
                                        value = Regex.Split(temp_value[0].Trim(), ":");
                                        value1 = Regex.Split(temp_value[2].Trim(), ":");
                                        CGR = value[1].Trim();
                                        NCGR = value1[1].Trim();
                                    }

                                    if (temp[j].Contains("NBCRCT") && !temp[j].Contains("TREE"))
                                    {
                                        temp_data = temp[j].Trim().ToString();
                                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                        temp_value = Regex.Split(temp_data, "~");
                                        NBCRCT = temp_value[5].Trim();
                                        NBCRCT = NBCRCT.Replace(":", " ");
                                        NBCRCT = NBCRCT.Trim();
                                        if (NBCRCT.Contains("-"))
                                            NBCRCT = NBCRCT.Replace("-", " ");

                                    }
                                    else if(temp[j].Contains("NBCRCT") && temp[j].Contains("TREE"))
                                    {                                       
                                            temp_data = temp[j].Trim().ToString();
                                            temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                            temp_value = Regex.Split(temp_data, "~");
                                            NBCRCT = temp_value[1].Trim();
                                            NBCRCT = NBCRCT.Replace(":", " ");
                                            NBCRCT = NBCRCT.Trim();
                                            if (NBCRCT.Contains("-"))
                                                NBCRCT = NBCRCT.Replace("-", " ");
                                    }

                                    if (temp[j].Contains("SPC(H/D)") && temp[j].Contains("NET"))
                                    {
                                        temp_data = temp[j].Trim();
                                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                        temp_value = Regex.Split(temp_data, "~");

                                        CGR_NET = temp_value[3].Trim();
                                        CGR_NET = CGR_NET.Replace(":", " ");
                                        CGR_NET = CGR_NET.Trim();
                                        if (CGR_NET.Contains("-"))
                                            CGR_NET = CGR_NET.Replace("-", " ");

                                        value = Regex.Split(temp_value[temp_value.Length - 1].Trim(), ":");
                                        CGR_SPC = value[1].Trim().ToString();
                                        if (CGR_SPC.Contains("-"))
                                            CGR_SPC = CGR_SPC.Replace("-", " ");

                                    }

                                    if (temp[j].Contains("MGW") && !temp[j].Contains("ASI"))
                                    {
                                        temp_data = temp[j].Trim();
                                        temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                        temp_value = Regex.Split(temp_data, "~");
                                        vmgw = temp_value[1].Replace(":", " ");
                                        vmgw = vmgw.Trim();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                                    //if (!Directory.Exists(parsing_error_path))
                                    //{
                                    //    Directory.CreateDirectory(parsing_error_path);
                                    //}

                                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZRCI_PRINT5 ");
                                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                                    oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_2_MGW_SUBwithConinue()", ErrorMsg, "", ref FileError);

                                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                                    ExecuteSQLQuery(ref conn, ErrorQuery);
                                    continue;
                                }
                            }
                            #endregion

                            #region[inserting NCGR,CGR,CGR_SPC,NBCRT,vmgw Values into table for MGW and MSS]
                            if (NE_Type == "MGW")
                            {
                                if (NCGR != "" && CGR != "" && NBCRCT != "")
                                //if (CGR != "")
                                {
                                    dt = new DataTable();
                                   // SQLQuery(ref conn, ref dt, "select DISTINCT `VENDOR`, `CIRCLE`, `MGW_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `MGW_CGR`, `MGW_NCGR`,`CGR_SPC`,`CGR_NET`, `NBCRCT` from mss_mgw_map_" + CircleName + "_" + group_name + " where MGW_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and MGW_CGR is null and MGW_NCGR is null and CGR_SPC is null and NBCRCT is null and CGR_NET is null");
                                    SQLQuery(ref conn, ref dt, "select DISTINCT MGW_NAME,MGW_CGR,MGW_NCGR,CGR_NET,CGR,MGW_NBCRCT from mss_mgw_map_" + CircleName + "_" + group_name + "  where CGR = '" + CGR + "' and MGW_CGR is null and MGW_NCGR is null and MGW_NBCRCT is null and circle = '" + CircleName + "' ");
                                    if (dt.Rows.Count >= 1)
                                    {
                                        query = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_NAME = '" + _ne_name + "', MGW_CGR='" + CGR + "', MGW_NCGR= '" + NCGR + "', CGR_NET='" + CGR_NET + "', MGW_NBCRCT='" + NBCRCT + "' Where CGR = '" + CGR + "' and MGW_CGR is null and MGW_NCGR is null and MGW_NBCRCT is null and circle = '" + CircleName + "' and Vendor='" + _vendor.Trim() + "' ";
                                        ExecuteSQLQuery(ref conn, query);
                                        NCGR = CGR = CGR_NET = NBCRCT = "";
                                    }
                                    else
                                    {
                                        query = "select distinct * from (select VENDOR,CIRCLE,NODE_TYPE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where MGW_NAME='" + _ne_name.Trim() + "' and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                        SQLQuery(ref conn, ref dt, query);
                                        if (dt.Rows.Count >= 0)
                                        {
                                            query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`,`NODE_TYPE`,`MGW_NAME`,`MGW_CGR`,`MGW_NCGR`,`CGR_SPC`,`CGR_NET`,`MGW_NBCRCT`) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + NE_Type + "','" + _ne_name.Trim() + "','" + CGR + "', '" + NCGR + "','" + CGR_SPC + "','" + CGR_NET + "', '" + NBCRCT + "') ";
                                            ExecuteSQLQuery(ref conn, query);
                                            NCGR = CGR = CGR_NET = NBCRCT = "";
                                        }

                                        //SQLQuery(ref conn, ref dt, "select DISTINCT `VENDOR`, `CIRCLE`, `MGW_NAME`, `NODE_TYPE`, ifnull(`MCC`,''), ifnull(`MNC`,''), ifnull(`MSS`,''), ifnull(`ELEMENT_IP`,''), ifnull(`MSS_C_NO`,''), `MSS_SPC`, `VMGW_ID`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `MGW_CGR`, `MGW_NCGR`,`CGR_SPC`,`CGR_NET`, `MGW_NBCRCT` from mss_mgw_map_" + CircleName + "_" + group_name + " where MGW_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and Vendor='" + _vendor.Trim() + "' and MGW_CGR is null and MGW_NCGR is null and CGR_SPC is null and MGW_NBCRCT is null and CGR_NET is null");
                                        //if (dt.Rows.Count >= 0)
                                        //{
                                        //    for (int k = 0; k <= dt.Rows.Count - 1; k++)
                                        //    {
                                        //        query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `MGW_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `MGW_CGR`, `MGW_NCGR`,`CGR_SPC`,`CGR_NET`, `MGW_NBCRCT`) values ";
                                        //        query += "('" + dt.Rows[k][0].ToString() + "','" + dt.Rows[k][1].ToString() + "','" + dt.Rows[k][2].ToString() + "','" + dt.Rows[k][3].ToString() + "','" + dt.Rows[k][4].ToString() + "','" + dt.Rows[k][5].ToString() + "','" + dt.Rows[k][6].ToString() + "','" + dt.Rows[k][7].ToString() + "','" + dt.Rows[k][8].ToString() + "','" + dt.Rows[k][9].ToString() + "','" + dt.Rows[k][10].ToString() + "','" + dt.Rows[k][12].ToString() + "','" + dt.Rows[k][13].ToString() + "','" + dt.Rows[k][14].ToString() + "', '" + CGR + "', '" + NCGR + "','" + CGR_SPC + "','" + CGR_NET + "', '" + NBCRCT + "')";
                                        //        ExecuteSQLQuery(ref conn, query);
                                        //        NCGR = CGR = NBCRCT = "";
                                        //    }
                                        //}

                                    }
                                    #region Logic Commented for MGW

                                    //SQLQuery(ref conn, ref dt, "select * from mss_mgw_map_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and CGR is null and ncgr is null and NBCRCT is null and CGR_NET is null");

                                    //for (int k = 0; k <= dt.Rows.Count - 1; k++)
                                    //{
                                    //    query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`,`CGR_NET`, `NBCRCT`) values ";
                                    //    query += "('" + dt.Rows[k][0].ToString() + "','" + dt.Rows[k][1].ToString() + "','" + dt.Rows[k][2].ToString() + "','" + dt.Rows[k][3].ToString() + "','" + dt.Rows[k][4].ToString() + "','" + dt.Rows[k][5].ToString() + "','" + dt.Rows[k][6].ToString() + "','" + dt.Rows[k][7].ToString() + "','" + dt.Rows[k][8].ToString() + "','" + dt.Rows[k][9].ToString() + "','" + dt.Rows[k][10].ToString() + "','" + dt.Rows[k][12].ToString() + "','" + dt.Rows[k][13].ToString() + "','" + dt.Rows[k][14].ToString() + "', '" + CGR + "', '" + NCGR + "','" + CGR_NET + "', '" + NBCRCT + "')";
                                    //    ExecuteSQLQuery(ref conn, query);
                                    //    NCGR = CGR = NBCRCT = "";
                                    //}
                                    #endregion
                                }

                            }
                            else
                            {
                                //if (NCGR != "" && CGR != "" && CGR_SPC != "" && NBCRCT != "" && vmgw == "")
                                if (CGR != "" && vmgw == "")
                                {
                                    dt1 = new DataTable();
                                    SQLQuery(ref conn, ref dt1, "select distinct `VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`, `CGR_NET`, `NBCRCT` from mss_mgw_map_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and cgr_spc is null and ncgr is null and NBCRCT is null and CGR is null and CGR_NET is null");
                                    if (dt1.Rows.Count >= 1)
                                    {
                                        for (int k = 0; k <= dt1.Rows.Count - 1; k++)
                                        {
                                            query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`, `CGR_NET`, `NBCRCT`) values ";
                                            query += "('" + dt1.Rows[k][0].ToString() + "','" + dt1.Rows[k][1].ToString() + "','" + dt1.Rows[k][2].ToString() + "','" + dt1.Rows[k][3].ToString() + "','" + dt1.Rows[k][4].ToString() + "','" + dt1.Rows[k][5].ToString() + "','" + dt1.Rows[k][6].ToString() + "','" + dt1.Rows[k][7].ToString() + "','" + dt1.Rows[k][8].ToString() + "','" + dt1.Rows[k][9].ToString() + "','" + dt1.Rows[k][10].ToString() + "','" + dt1.Rows[k][12].ToString() + "','" + dt1.Rows[k][13].ToString() + "','" + dt1.Rows[k][14].ToString() + "', '" + CGR + "', '" + NCGR + "', '" + CGR_SPC + "', '" + CGR_NET + "', '" + NBCRCT + "')";
                                            ExecuteSQLQuery(ref conn, query);
                                            NCGR = CGR = CGR_SPC = CGR_NET = NBCRCT = "";
                                        }
                                    }
                                }
                                //else if (NCGR != "" && CGR != "" && CGR_SPC != "" && vmgw != "" && NBCRCT != "")
                                else if (CGR != "" && vmgw != "")
                                {
                                    dt2 = new DataTable();
                                    SQLQuery(ref conn, ref dt2, "select distinct `VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`, `CGR_NET`, `NBCRCT` from mss_mgw_map_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and cgr_spc is null and ncgr is null and NBCRCT is null and vmgw is null and CGR is null and CGR_NET is null");
                                    if (dt2.Rows.Count >= 1)
                                    {
                                        for (int k = 0; k <= dt2.Rows.Count - 1; k++)
                                        {
                                            query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`,`CGR_NET`, `NBCRCT`) values ";
                                            query += "('" + dt2.Rows[k][0].ToString() + "','" + dt2.Rows[k][1].ToString() + "','" + dt2.Rows[k][2].ToString() + "','" + dt2.Rows[k][3].ToString() + "','" + dt2.Rows[k][4].ToString() + "','" + dt2.Rows[k][5].ToString() + "','" + dt2.Rows[k][6].ToString() + "','" + dt2.Rows[k][7].ToString() + "','" + dt2.Rows[k][8].ToString() + "','" + dt2.Rows[k][9].ToString() + "','" + dt2.Rows[k][10].ToString() + "','" + vmgw + "','" + dt2.Rows[k][12].ToString() + "','" + dt2.Rows[k][13].ToString() + "','" + dt2.Rows[k][14].ToString() + "', '" + CGR + "', '" + NCGR + "', '" + CGR_SPC + "','" + CGR_NET + "', '" + NBCRCT + "')";
                                            ExecuteSQLQuery(ref conn, query);
                                            NCGR = CGR = CGR_SPC = CGR_NET = NBCRCT = vmgw = "";
                                        }
                                    }
                                }

                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                            //if (!Directory.Exists(parsing_error_path))
                            //{
                            //    Directory.CreateDirectory(parsing_error_path);
                            //}

                            //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZRCI_PRINT5 ");
                            //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_2_MGW_SUBwithConinue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_2()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        //code added by rahul 13-08-2018
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZJNI_FQDN(string output, string CircleName, string NE_Type)
        {
            //    DataTable dt = new DataTable();
            //    string temp1 = string.Empty; ;
            //    string temp_data = string.Empty; ;
            //    string[] value;
            //    string[] temp_value;
            //    string[] temp;
            //    string[] value1;
            //    //string pattern = "\\bROOT\\b";
            //    //string cmnd_output = Regex.Replace(output, pattern, "ROOT_VALUE");
            //    string[] data = Regex.Split(output, "ROOT");
            //    string query = string.Empty; ;
            //    string CGR = string.Empty; ;
            //    string CGR_UPART = string.Empty; ;
            //    string CGR_NET = string.Empty; ;
            //    string CGR_SPC = string.Empty; ;
            //    string NCGR = string.Empty; ;
            //    string CGR_TYPE = string.Empty; ;
            //    string vmgw = string.Empty; ;
            //    string NBCRCT = string.Empty; ;
            //    string FQDN = string.Empty; ;

            //    try
            //    {
            //        //for (int i = 2; i <= data.Length - 1; i++)
            //        //{
            //        CGR = "";
            //        CGR_NET = "";
            //        CGR_SPC = "";
            //        CGR_TYPE = "";
            //        NCGR = "";
            //        vmgw = "";
            //        NBCRCT = "";
            //        FQDN = "";

            //        //if (data[i] != null)
            //        //{

            //        temp1 = data[1].Trim().ToString();
            //        temp = Regex.Split(temp1, "\r\n");

            //        for (int j = 0; j <= temp.Length - 1; j++)
            //        {
            //            try
            //            {
            //                if (temp[j].Contains("FQDN"))
            //                {
            //                    temp_data = temp[j].Trim();
            //                    temp_value = Regex.Split(temp_data, ":");
            //                    FQDN = temp_value[1].Trim();
            //                }
            //                if (temp[j].Contains("ASSIGNED HOST(S):"))
            //                {
            //                    j = j + 2;

            //                repeat: while (j != temp.Length) //&& j != temp.Length - 1)
            //                    {
            //                        temp_data = temp[j].Trim();
            //                        if (temp[j].Contains("COMMAND EXECUTED"))
            //                        {
            //                            j = temp.Length;
            //                            goto repeat;
            //                        }
            //                        j = j + 1;
            //                    }
            //                }
            //                if (NE_Type == "MGW")
            //                {
            //                    //SQLQuery(ref conn, ref dt, "select * from mss_mgw_map_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and cgr_net is null and cgr_spc is null and ncgr is null and cic is null");

            //                    //query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`,`CGR_NET`, `CGR_TYPE`) values ";
            //                    //query += "('" + dt.Rows[0][0].ToString() + "','" + dt.Rows[0][1].ToString() + "','" + dt.Rows[0][2].ToString() + "','" + dt.Rows[0][3].ToString() + "','" + dt.Rows[0][4].ToString() + "','" + dt.Rows[0][5].ToString() + "','" + dt.Rows[0][6].ToString() + "','" + dt.Rows[0][7].ToString() + "','" + dt.Rows[0][8].ToString() + "','" + dt.Rows[0][9].ToString() + "','" + dt.Rows[0][10].ToString() + "','" + vmgw + "','" + dt.Rows[l][12].ToString() + "','" + dt.Rows[l][13].ToString() + "','" + dt.Rows[l][14].ToString() + "', '" + CGR + "', '" + NCGR + "', '" + CGR_SPC + "', '" + CGR_NET + "', '" + CGR_TYPE + "')";
            //                    //ExecuteSQLQuery(ref conn, query);
            //                }
            //                else
            //                {
            //                    //SQLQuery(ref conn, ref dt, "select * from mss_mgw_map_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and cgr_net is null and cgr_spc is null and ncgr is null and cic is null");

            //                    //query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`,`CGR_NET`, `CGR_TYPE`) values ";
            //                    //query += "('" + dt.Rows[0][0].ToString() + "','" + dt.Rows[0][1].ToString() + "','" + dt.Rows[0][2].ToString() + "','" + dt.Rows[0][3].ToString() + "','" + dt.Rows[0][4].ToString() + "','" + dt.Rows[0][5].ToString() + "','" + dt.Rows[0][6].ToString() + "','" + dt.Rows[0][7].ToString() + "','" + dt.Rows[0][8].ToString() + "','" + dt.Rows[0][9].ToString() + "','" + dt.Rows[0][10].ToString() + "','" + vmgw + "','" + dt.Rows[l][12].ToString() + "','" + dt.Rows[l][13].ToString() + "','" + dt.Rows[l][14].ToString() + "', '" + CGR + "', '" + NCGR + "', '" + CGR_SPC + "', '" + CGR_NET + "', '" + CGR_TYPE + "')";
            //                    //ExecuteSQLQuery(ref conn, query);
            //                }

            //                // if (CGR != "" || CGR_SPC != "" || NBCRCT != ""  || NCGR != "" || vmgw != " ")
            //                // {
            //                //SQLQuery(ref conn, ref dt, "select * from mss_mgw_map_" + CircleName + "_" + group_name + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + CircleName + "' and cgr_net is null and cgr_spc is null and ncgr is null and cic is null");

            //                //query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`, `VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`,`CGR_NET`, `CGR_TYPE`) values ";
            //                //query += "('" + dt.Rows[0][0].ToString() + "','" + dt.Rows[0][1].ToString() + "','" + dt.Rows[0][2].ToString() + "','" + dt.Rows[0][3].ToString() + "','" + dt.Rows[0][4].ToString() + "','" + dt.Rows[0][5].ToString() + "','" + dt.Rows[0][6].ToString() + "','" + dt.Rows[0][7].ToString() + "','" + dt.Rows[0][8].ToString() + "','" + dt.Rows[0][9].ToString() + "','" + dt.Rows[0][10].ToString() + "','" + vmgw + "','" + dt.Rows[l][12].ToString() + "','" + dt.Rows[l][13].ToString() + "','" + dt.Rows[l][14].ToString() + "', '" + CGR + "', '" + NCGR + "', '" + CGR_SPC + "', '" + CGR_NET + "', '" + CGR_TYPE + "')";
            //                //ExecuteSQLQuery(ref conn, query);
            //                // }                                                                                                              
            //            }
            //            catch (Exception ex)
            //            {
            //                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
            //                //if (!Directory.Exists(parsing_error_path))
            //                //{
            //                //    Directory.CreateDirectory(parsing_error_path);
            //                //}

            //                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZRCI_PRINT5 ");
            //                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            //                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
            //                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_2_MSS_SUBwithConinue()", ErrorMsg, "", ref FileError);

            //                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
            //                ExecuteSQLQuery(ref conn, ErrorQuery);
            //                continue;
            //            }
            //        }
            //        //}

            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
            //        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_5()", ErrorMsg, "", ref FileError);

            //        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
            //        ExecuteSQLQuery(ref conn, ErrorQuery);
            //    }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZJGI(string output, string CircleName, string NE_Type) //Updated by AJAY
        {
            string Newquery = string.Empty;
            string MGW_ID = string.Empty;
            string VMGW = string.Empty;
            string REGISTRATION_STATUS = string.Empty; //Added by AJAY
            string VMGW_CTRL_SIGU = string.Empty;
            string VMGW_SOURCE_IP = string.Empty;
            string VMGW_DEST_IP = string.Empty;
            string VMGW_SECONDARY_IP = string.Empty; // Added by AJAY
            string PAR_SET = string.Empty;
            string query1 = string.Empty;
            string query = string.Empty;
            string data = string.Empty;
            string[] temp;
            DataTable dt;
            string cmnd_output = output.Trim();

            try
            {
                temp = Regex.Split(cmnd_output, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (temp[i].Contains("MGW ID"))
                        {
                            data = temp[i].Trim().ToString();
                            MGW_ID = data.Remove(0, data.LastIndexOf("."));
                            MGW_ID = MGW_ID.Replace(".", " ");
                            MGW_ID = MGW_ID.Trim();
                        }

                        if (temp[i].Contains("MGW NAME"))
                        {
                            data = temp[i].Trim().ToString();
                            VMGW = data.Remove(0, data.LastIndexOf(".."));
                            VMGW = VMGW.Replace("..", " ");
                            VMGW = VMGW.Trim();
                        }

                        if (temp[i].Contains("MGW ADDRESS"))
                        {
                            data = temp[i].Trim().ToString();
                            VMGW_DEST_IP = data.Remove(0, data.LastIndexOf(".."));
                            VMGW_DEST_IP = VMGW_DEST_IP.Replace("..", " ");
                            VMGW_DEST_IP = VMGW_DEST_IP.Trim();
                        }

                        if (temp[i].Contains("UNIT INDEX"))
                        {
                            data = temp[i].Trim().ToString();
                            VMGW_CTRL_SIGU = data.Remove(0, data.LastIndexOf(".."));
                            VMGW_CTRL_SIGU = VMGW_CTRL_SIGU.Replace("..", " ");
                            VMGW_CTRL_SIGU = VMGW_CTRL_SIGU.Trim();
                        }

                        if (temp[i].Contains("PRIMARY CTRL ADDRESS")) // CTRL ADDRESS modified to PRIMARY CTRL ADDRESS
                        {
                            data = temp[i].Trim().ToString();
                            VMGW_SOURCE_IP = data.Remove(0, data.LastIndexOf(".."));
                            VMGW_SOURCE_IP = VMGW_SOURCE_IP.Replace("..", " ");
                            VMGW_SOURCE_IP = VMGW_SOURCE_IP.Trim();
                        }

                        if (temp[i].Contains("SECONDARY CTRL ADDRESS")) //  added by AJAY
                        {
                            data = temp[i].Trim().ToString();
                            VMGW_SECONDARY_IP = data.Remove(0, data.LastIndexOf(".."));
                            VMGW_SECONDARY_IP = VMGW_SECONDARY_IP.Replace("..", " ");
                            VMGW_SECONDARY_IP = VMGW_SECONDARY_IP.Trim();
                        }

                        if (temp[i].Contains("DEFAULT PARAMETER SET")) // added by AJAY
                        {
                            data = temp[i].Trim().ToString();
                            PAR_SET = data.Remove(0, data.LastIndexOf(".."));
                            PAR_SET = PAR_SET.Replace("..", " ");
                            PAR_SET = PAR_SET.Trim();
                        }

                        if (temp[i].Contains("REGISTRATION STATUS"))
                        {
                            data = temp[i].Trim().ToString();
                            REGISTRATION_STATUS = data.Remove(0, data.LastIndexOf(".."));
                            REGISTRATION_STATUS = REGISTRATION_STATUS.Replace("..", " ");
                            REGISTRATION_STATUS = REGISTRATION_STATUS.Trim();
                        }

                        if (MGW_ID != "" && VMGW != "" && VMGW_DEST_IP != "" && VMGW_CTRL_SIGU != "" && VMGW_SOURCE_IP != "" && REGISTRATION_STATUS != "" && VMGW_SECONDARY_IP != "" && PAR_SET != "")
                        {
                            

                            #region New Code for Insert Query
                            dt = new DataTable();
                            query = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,`VMGW_ID`,`VMGW`,`VMGW_CTRL_SIGU`,`VMGW_SOURCE_IP`,`VMGW_DEST_IP`, `REGISTRATION_STATUS`,`VMGW_SECONDARY_IP`,`PAR_SET` from  mss_mgw_map_" + _CircleName + "_" + group_name + " where NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                SQLQuery(ref conn, ref dt, query);
                            if (dt.Rows.Count >= 1)
                            {
                                query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,`VMGW_ID`,`VMGW`,`VMGW_CTRL_SIGU`,`VMGW_SOURCE_IP`,`VMGW_DEST_IP`,`REGISTRATION_STATUS`,`VMGW_SECONDARY_IP`,`PAR_SET`) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + MGW_ID.Trim() + "','" + VMGW.Trim() + "','" + VMGW_CTRL_SIGU.Trim() + "', '" + VMGW_SOURCE_IP.Trim() + "', '" + VMGW_DEST_IP.Trim() + "', '" + REGISTRATION_STATUS.Trim() + "', '" + VMGW_SECONDARY_IP.Trim() + "', '" + PAR_SET.Trim() + "')";
                                ExecuteSQLQuery(ref conn, query1);
                            }
                            else if (dt.Rows.Count <= 0)
                            {
                                query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,`VMGW_ID`,`VMGW`,`VMGW_CTRL_SIGU`,`VMGW_SOURCE_IP`,`VMGW_DEST_IP`, `REGISTRATION_STATUS`,`VMGW_SECONDARY_IP`,`PAR_SET`) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + MGW_ID.Trim() + "','" + VMGW.Trim() + "','" + VMGW_CTRL_SIGU.Trim() + "', '" + VMGW_SOURCE_IP.Trim() + "', '" + VMGW_DEST_IP.Trim() + "', '" + REGISTRATION_STATUS.Trim() + "', '" + VMGW_SECONDARY_IP.Trim() + "', '" + PAR_SET.Trim() + "')";
                                ExecuteSQLQuery(ref conn, query1);
                            }
                            else
                            {

                                query1 = "";

                                query1 = " update mss_mgw_map_" + CircleName + "_" + group_name + "  set `VMGW_ID`='" + MGW_ID + "',  `VMGW_CTRL_SIGU`='" + VMGW_CTRL_SIGU + "',";
                                query1 += "   `VMGW_SOURCE_IP`='" + VMGW_SOURCE_IP + "', `VMGW_DEST_IP`='" + VMGW_DEST_IP + "', `REGISTRATION_STATUS` = '" + REGISTRATION_STATUS + "', `VMGW_SECONDARY_IP` = '" + VMGW_SECONDARY_IP + "', `PAR_SET` = '" + PAR_SET + "'   where NE_NAME = '" + _ne_name + "'  AND ";
                                query1 += " VMGW_CTRL_SIGU IS NULL  AND circle = '" + _CircleName + "' AND `VMGW`='" + VMGW + "' "; //Added REGISTRATION_STATUS and VMGW_SECONDARY_IP column by AJAY

                                ExecuteSQLQuery(ref conn, query1);

                            }
                            #endregion

                            MGW_ID = "";
                            VMGW = "";
                            VMGW_CTRL_SIGU = "";
                            VMGW_DEST_IP = "";
                            VMGW_SOURCE_IP = "";
                            REGISTRATION_STATUS = "";
                            VMGW_SECONDARY_IP = "";
                            PAR_SET = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZJGI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJGI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJGI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }
        //public void ParseData_ZJGI(string output, string CircleName, string NE_Type)
        //{
        //    string Newquery = string.Empty;
        //    string MGW_ID = string.Empty;
        //    string VMGW = string.Empty;

        //    string VMGW_CTRL_SIGU = string.Empty;
        //    string VMGW_SOURCE_IP = string.Empty;
        //    string VMGW_DEST_IP = string.Empty;
        //    string query1 = string.Empty;
        //    string data = string.Empty;
        //    string[] temp;
        //    string cmnd_output = output.Trim();

        //    try
        //    {
        //        temp = Regex.Split(cmnd_output, "\r\n");

        //        for (int i = 0; i <= temp.Length - 1; i++)
        //        {
        //            try
        //            {
        //                if (temp[i].Contains("MGW ID"))
        //                {
        //                    data = temp[i].Trim().ToString();
        //                    MGW_ID = data.Remove(0, data.LastIndexOf("."));
        //                    MGW_ID = MGW_ID.Replace(".", " ");
        //                    MGW_ID = MGW_ID.Trim();

        //                }

        //                if (temp[i].Contains("MGW NAME"))
        //                {
        //                    data = temp[i].Trim().ToString();
        //                    VMGW = data.Remove(0, data.LastIndexOf(".."));
        //                    VMGW = VMGW.Replace("..", " ");
        //                    VMGW = VMGW.Trim();
        //                }

        //                if (temp[i].Contains("MGW ADDRESS"))
        //                {
        //                    data = temp[i].Trim().ToString();
        //                    VMGW_DEST_IP = data.Remove(0, data.LastIndexOf(".."));
        //                    VMGW_DEST_IP = VMGW_DEST_IP.Replace("..", " ");
        //                    VMGW_DEST_IP = VMGW_DEST_IP.Trim();
        //                }

        //                if (temp[i].Contains("UNIT INDEX"))
        //                {
        //                    data = temp[i].Trim().ToString();
        //                    VMGW_CTRL_SIGU = data.Remove(0, data.LastIndexOf(".."));
        //                    VMGW_CTRL_SIGU = VMGW_CTRL_SIGU.Replace("..", " ");
        //                    VMGW_CTRL_SIGU = VMGW_CTRL_SIGU.Trim();
        //                }

        //                if (temp[i].Contains("CTRL ADDRESS"))
        //                {
        //                    data = temp[i].Trim().ToString();
        //                    VMGW_SOURCE_IP = data.Remove(0, data.LastIndexOf(".."));
        //                    VMGW_SOURCE_IP = VMGW_SOURCE_IP.Replace("..", " ");
        //                    VMGW_SOURCE_IP = VMGW_SOURCE_IP.Trim();

        //                }

        //                if (MGW_ID != "" && VMGW != "" && VMGW_DEST_IP != "" && VMGW_CTRL_SIGU != "" && VMGW_SOURCE_IP != "")
        //                {
        //                    query1 = "";

        //                    query1 = " update mss_mgw_map_" + CircleName + "_" + group_name + "  set `VMGW_ID`='" + MGW_ID + "',  `VMGW_CTRL_SIGU`='" + VMGW_CTRL_SIGU + "',";
        //                    query1 += "   `VMGW_SOURCE_IP`='" + VMGW_SOURCE_IP + "', `VMGW_DEST_IP`='" + VMGW_DEST_IP + "' where NE_NAME = '" + _ne_name + "'  AND ";
        //                    query1 += " VMGW_CTRL_SIGU = ''  AND circle = '" + _CircleName + "' AND `VMGW`='" + VMGW + "'";

        //                    //query1 = " update mss_mgw_map_" + CircleName + "_" + group_name + "  set `VMGW_ID`='" + MGW_ID + "',  `VMGW_CTRL_SIGU`='" + VMGW_CTRL_SIGU + "',";
        //                    //query1 += "   `VMGW_SOURCE_IP`='" + VMGW_SOURCE_IP + "', `VMGW_DEST_IP`='" + VMGW_DEST_IP + "',`VMGW`='" + VMGW + "' where NE_NAME = '" + _ne_name + "'  AND ";
        //                    //query1 += " VMGW_CTRL_SIGU = ''  AND circle = '" + _CircleName + "'";

        //                    ExecuteSQLQuery(ref conn, query1);


        //                    MGW_ID = "";
        //                    VMGW = "";
        //                    VMGW_CTRL_SIGU = "";
        //                    VMGW_DEST_IP = "";
        //                    VMGW_SOURCE_IP = "";


        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
        //                //if (!Directory.Exists(parsing_error_path))
        //                //{
        //                //    Directory.CreateDirectory(parsing_error_path);
        //                //}

        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZJGI ");
        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        //                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJGI_SUBwithConinue()", ErrorMsg, "", ref FileError);

        //                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //                ExecuteSQLQuery(ref conn, ErrorQuery);
        //                continue;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZJGI()", ErrorMsg, "", ref FileError);

        //        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //        ExecuteSQLQuery(ref conn, ErrorQuery);
        //    }
        //}

        /// <summary>
        /// Parsing Data Of Both ZNRI:NA0; and ZNRI:NA1; Commands For MSS
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        /// <param name="NET"></param>
        public void ParseData_ZNRI_MSS(string output, string _CircleName, string NE_Type, string NET)   // change here for root set 
        {
            string link_name = string.Empty;
            string MSS_SPC = string.Empty;
            string temp = string.Empty;
            string[] tebihar_local;
            string[] temp1;
            string SqlStr = string.Empty;

            string PRIO = string.Empty;
            string STATE = string.Empty;
            string PAR_SET = string.Empty;
            string SP_Type = string.Empty;
            string SS7_Stand = string.Empty;
            string SubField_Count = string.Empty;
            string SubField_Lnths = string.Empty;
            string SubField_Bit = string.Empty;
            string CGR_NET = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                string[] cmnd_data = Regex.Split(output, "\r\n");

                for (int i = 0; i <= cmnd_data.Length - 1; i++)
                {
                    try
                    {
                        //code change on 13-07-2018 by Rahul Kumar
                        #region [output contains Routes]
                        if (cmnd_data[i].ToString().Trim().Contains("ROUTES:"))
                        {
                            temp = cmnd_data[i].Trim();
                            i = i + 2;
                            while (cmnd_data[i] != "COMMAND EXECUTED" && i <= cmnd_data.Length - 1 && cmnd_data[i].Trim() != "")
                            {
                                tebihar_local = cmnd_data[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                MSS_SPC = tebihar_local[0].Trim().ToString();
                                link_name = tebihar_local[1].Trim().ToString();
                                STATE = tebihar_local[2].Trim().ToString();
                                PRIO = tebihar_local[3].Trim().ToString();
                                if (!string.IsNullOrEmpty(MSS_SPC))
                                {
                                    if (NE_Type == "HLR")
                                    {
                                        SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,ELEMENT_IP,CGR_NET,HLR_C_NO from  hlr_" + _CircleName + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        dt.Clear();
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count >= 1)
                                        {

                                            SqlStr = "insert into hlr_" + _CircleName + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,ELEMENT_IP,CGR_NET,HLR_C_NO,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','"+ NE_Type +"','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["HLR_C_NO"] + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                       else if (dt.Rows.Count <= 0)
                                        {

                                            SqlStr = "insert into hlr_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                        i = i + 1;
                                        if (i > temp.Length - 1)
                                        {
                                            break;
                                        }
                                    }
                                    else if (NE_Type == "MSS_LINKS")
                                    {
                                        if (NET == "NA0")
                                        {
                                            SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MSS_C_NO,MSS,ELEMENT_IP from  mss_link_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        }
                                        else
                                        {
                                            SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MSS_C_NO,MSS,ELEMENT_IP from  mss_link_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        }
                                        //SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MSS,ELEMENT_IP,CGR_NET from  mss_link_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' )A";
                                        dt.Clear();
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count <= 0)
                                        {
                                            SqlStr = "insert into mss_link_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";                 
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                       else if (dt.Rows.Count >= 1)
                                        {
                                            SqlStr = "insert into mss_link_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MSS,MSS_C_NO,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["MSS_C_NO"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = NET = MSS_SPC = "";
                                        }
                                        i = i + 1;
                                        if (i > temp.Length - 1)
                                        {
                                            break;
                                        }
                                    }
                                    else if (NE_Type == "MSC")
                                    {
                                        SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET from  msc_" + _CircleName + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count >= 1)
                                        {

                                            SqlStr = "insert into msc_" + _CircleName + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                       else if (dt.Rows.Count <= 0)
                                        {
                                            SqlStr = "insert into msc_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                        i = i + 1;
                                        if (i > temp.Length - 1)
                                        {
                                            break;
                                        }

                                    }
                                    else
                                    {
                                        // for mss
                                        SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                        //SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count <= 0)
                                        {
                                            SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET,STATE,PRIO) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "','" + STATE + "','" + PRIO + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = STATE = PRIO = "";
                                        }
                                        else if (dt.Rows.Count >= 1)
                                        {                                           
                                            SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET,STATE,PRIO) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + NET + "','" + MSS_SPC + "','" + link_name + "','" + STATE + "','" + PRIO + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = STATE = PRIO = "";
                                        }
                                        i = i + 1;
                                        if (i > cmnd_data.Length - 1)
                                        {
                                            break;
                                        }
                                    }
                                }
                                MSS_SPC = "";
                            }
                        }
                        #endregion

                        #region [output contains SP CODE H/D && NET]
                        if (cmnd_data[i].ToString().Trim().Contains("SP CODE H/D") && cmnd_data[i].ToString().Trim().Contains("NET"))
                        {
                            i = i + 2;

                            #region [output contains OWN SP]
                            if (cmnd_data[i].Contains(NET) && cmnd_data[i].Contains("OWN SP"))
                            {
                                temp = cmnd_data[i].Trim();
                                temp = Regex.Replace(temp.ToString().Trim(), " {2,}", "~");
                                temp1 = Regex.Split(temp, "~");
                                //CGR_NET = temp1[0].Trim().ToString();
                                MSS_SPC = temp1[1].Trim().ToString();
                                link_name = temp1[2].Trim().ToString();
                                SP_Type = temp1[3].Trim().ToString();
                                SS7_Stand = temp1[4].Trim().ToString();
                                SubField_Count = temp1[5].Trim().ToString();
                                SubField_Bit = temp1[6].Trim().ToString();
                                SubField_Lnths = temp1[7].Trim().ToString();
                                if (!string.IsNullOrEmpty(MSS_SPC))
                                {
                                    if (NE_Type == "HLR")
                                    {

                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        ExecuteSQLQuery(ref conn, "Update hlr_" + _CircleName + " set HLR_SPC='" + MSS_SPC + "'  where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + _CircleName + "' and Vendor='" + _vendor.Trim() + "' AND CGR_NET = '" + NET + "'");
                                    }
                                    else if (NE_Type == "MSS_LINKS")
                                    {
                                        SqlStr = "Select distinct * from(Select VENDOR,CIRCLE,NE_NAME,ELEMENT_IP,CGR_NET,MSS_SPC,LINKSET from mss_link_" + _CircleName + "_" + group_name + " where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + _CircleName + "' and Vendor='" + _vendor.Trim() + "' AND CGR_NET = '" + NET + "')A";
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt != null)
                                        {
                                            if (dt.Rows.Count <= 0)
                                            {
                                                ExecuteSQLQuery(ref conn, "insert into mss_link_" + _CircleName + "_" + group_name + "(Vendor,Circle,NODE_Type,NE_NAME,CGR_NET,MSS_SPC,LINKSET) values('" + _vendor.Trim() + "','" + _CircleName + "','" + NE_Type + "','" + _ne_name.Trim() + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')");
                                            }
                                            else if (dt.Rows.Count >= 1)
                                            {
                                                SqlStr = "insert into mss_link_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,ELEMENT_IP,CGR_NET,MSS_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + MSS_SPC + "','" + link_name + "')";
                                                ExecuteSQLQuery(ref conn, SqlStr);
                                                link_name = "";
                                            }
                                        }

                                        ExecuteSQLQuery(ref conn, "Update mss_link_" + _CircleName + "_" + group_name + " set MSS_SPC='" + MSS_SPC + "'  where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + _CircleName + "' and Vendor='" + _vendor.Trim() + "' AND CGR_NET = '" + NET + "'");                         
                                    }
                                    else if (NE_Type == "MSC")
                                    {

                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        ExecuteSQLQuery(ref conn, "update msc_" + _CircleName + " set MSS_SPC = '" + MSS_SPC + "' where NE_NAME = '" + _ne_name.Trim() + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + _CircleName + "' AND CGR_NET = '" + NET + "' and Vendor='" + _vendor.Trim() + "' ");
                                    }
                                    else
                                    // for mss
                                    {
                                        SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count >= 1)
                                        {
                                            //SqlStr = "update mss_mgw_map_" + _CircleName + "_" + group_name + " set MSS_SPC = '" + MSS_SPC + "', LINKSET='" + link_name + "', SP_TYPE = '" + SP_Type + "',SS7_STAND='" + SS7_Stand + "', SUB_FIELD_INFO_COUNT='" + SubField_Count + "', SUB_FIELD_INFO_BIT ='" + SubField_Bit + "', SUB_FIELD_INFO_LENGTHS ='" + SubField_Lnths + "' where NE_NAME = '" + _ne_name.Trim() + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + _CircleName + "' AND CGR_NET = '" + NET + "' and Vendor='" + _vendor.Trim() + "' ";
                                            SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MSS_SPC,LINKSET,SP_TYPE,SS7_STAND,SUB_FIELD_INFO_COUNT,SUB_FIELD_INFO_BIT,SUB_FIELD_INFO_LENGTHS) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + MSS_SPC + "','" + link_name + "','" + SP_Type + "','" + SS7_Stand + "','" + SubField_Count + "','" + SubField_Bit + "','" + SubField_Lnths + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = SP_Type = SS7_Stand = SubField_Count = SubField_Bit = SubField_Lnths = "";
                                        }
                                        else if (dt.Rows.Count <= 0)
                                        {                                          
                                            SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,MSS_SPC,LINKSET,SP_TYPE,SS7_STAND,SUB_FIELD_INFO_COUNT,SUB_FIELD_INFO_BIT,SUB_FIELD_INFO_LENGTHS) values ('" + _vendor.Trim() + "','" + _CircleName + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "','" + SP_Type + "','" + SS7_Stand + "','" + SubField_Count + "','" + SubField_Bit + "','" + SubField_Lnths + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = SP_Type = SS7_Stand = SubField_Count = SubField_Bit = SubField_Lnths = "";
                                        }
                                    }
                                }
                                MSS_SPC = "";
                            }
                            #endregion

                            #region[output contains SP CODE H/D && NET]
                            else
                            {
                                tebihar_local = cmnd_data[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                MSS_SPC = tebihar_local[1].Trim().ToString();
                                link_name = tebihar_local[2].Trim().ToString();
                                STATE = tebihar_local[3].Trim().ToString();
                                PAR_SET = tebihar_local[4].Trim().ToString();
                                if (!string.IsNullOrEmpty(MSS_SPC))
                                {
                                    if (NE_Type == "HLR")
                                    {
                                        SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,ELEMENT_IP,CGR_NET,HLR_C_NO from  hlr_" + _CircleName + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        dt.Clear();
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count >= 1)
                                        {

                                            SqlStr = "insert into hlr_" + _CircleName + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,ELEMENT_IP,CGR_NET,HLR_C_NO,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["HLR_C_NO"] + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                        else if (dt.Rows.Count <= 0)
                                        {

                                            SqlStr = "insert into hlr_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET) values ('" + _vendor.Trim() + "','" + _CircleName + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }

                                    }
                                    else if (NE_Type == "MSS_LINKS")
                                    {
                                        if (NET == "NA0")
                                        {
                                            SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MSS_C_NO,MSS,ELEMENT_IP from  mss_link_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        }
                                        else
                                        {
                                            SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MSS_C_NO,MSS,ELEMENT_IP from  mss_link_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        }
                                        //SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MSS,ELEMENT_IP,CGR_NET from  mss_link_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' )A";
                                        dt.Clear();
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count >= 1)
                                        {

                                            SqlStr = "insert into mss_link_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MSS,MSS_C_NO,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["MSS_C_NO"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                        else if (dt.Rows.Count <= 0)
                                        {

                                            SqlStr = "insert into mss_link_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET) values ('" + _vendor.Trim() + "','" + _CircleName + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }


                                    }
                                    else if (NE_Type == "MSC")
                                    {
                                        SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET from  msc_" + _CircleName + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count >= 1)
                                        {

                                            SqlStr = "insert into msc_" + _CircleName + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }
                                        else if (dt.Rows.Count <= 0)
                                        {

                                            SqlStr = "insert into msc_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET) values ('" + _vendor.Trim() + "','" + _CircleName + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = "";
                                        }


                                    }
                                    else
                                    {
                                        // for mss
                                        SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                        SQLQuery(ref conn, ref dt, SqlStr);
                                        if (dt.Rows.Count >= 1)
                                        {
                                            SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET,STATE,PAR_SET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + NET + "','" + MSS_SPC + "','" + link_name + "','" + STATE + "','" + PAR_SET + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = STATE = PAR_SET = "";
                                        }
                                        else if (dt.Rows.Count <= 0)
                                        {
                                            SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET,STATE,PAR_SET) values ('" + _vendor.Trim() + "','" + _CircleName + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + NET + "','" + MSS_SPC + "','" + link_name + "','" + STATE + "','" + PAR_SET + "')";
                                            ExecuteSQLQuery(ref conn, SqlStr);
                                            link_name = STATE = PAR_SET = "";
                                        }
                                    }
                                }
                                MSS_SPC = "";
                            }
                            #endregion
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZNRI_MSS ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNRI_MSS_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNRI_MSS()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public string CheckForErrorinFile(string output)
        {
            try
            {
                string Msg = string.Empty;
                if (output.Contains("Unknown command"))
                {
                    Msg = "% Unknown command or computer name, or unable to find computer address.";

                }
                else if (output.Contains("USER AUTHORIZATION FAILURE"))
                {
                    Msg = "USER AUTHORIZATION FAILURE.";

                }
                else if (output.Contains("DELAY"))
                {
                    Msg = "DELAY.";

                }
                return Msg;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZQRI(string output, string CircleName, string NE_Type)
        {
            string sqlstr = string.Empty;
            string element_ip = string.Empty;
            string[] temp1;
            string required_output = string.Empty;
            string value = string.Empty;
            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: ";
            try
            {
                ErrorMsg = ErrorMsg + CheckForErrorinFile(output);
                string data = output.Remove(0, output.IndexOf("-------"));
                string[] temp = Regex.Split(data, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {

                        value = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                        temp1 = Regex.Split(value, "~");

                        for (int j = 0; j <= temp1.Length - 1; j++)
                        {
                            if (temp1[j].Trim() == "L")
                            {
                                if (j == 0)
                                {
                                    element_ip = temp1[j + 1];   // for atca mss
                                }
                                else
                                {
                                    element_ip = temp1[j - 1];
                                }

                                break;
                            }

                        }

                        if (element_ip != "")
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZQRI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = ErrorMsg + "[" + ex.Message.ToString() + "]";
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZQRI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }


                }

                if (_NEType == "MSC")
                {
                    sqlstr = "Update msc_" + CircleName + " Set ELEMENT_IP='" + element_ip.Trim() + "' Where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                    ExecuteSQLQuery(ref conn, sqlstr);
                }
                else
                {
                    sqlstr = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  Set ELEMENT_IP='" + element_ip.Trim() + "' Where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                    ExecuteSQLQuery(ref conn, sqlstr);
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZQRI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZWVI(string output, string CircleName, string NE_Type)
        {
            string element = _ne_name.Trim();
            string TYPE = NE_Type;
            string circle = _CircleName;
            string temp = string.Empty;
            string MCC = string.Empty;
            string MNC = string.Empty;
            int flag2;
            int flag1;
            string query = string.Empty;

            try
            {
                string[] cmnd_data = Regex.Split(output, "\r\n");

                for (int i = 0; i <= cmnd_data.Length - 1; i++)
                {
                    try
                    {
                        if (cmnd_data[i].Contains("MOBILE COUNTRY CODE"))
                        {
                            i = i + 1;
                            flag1 = 0;
                            while (flag1 != 1)
                            {
                                if (cmnd_data[i].Trim() != "")
                                {
                                    MCC = cmnd_data[i].Trim().ToString();
                                    flag1 = 1;
                                }
                                i = i + 1;
                            }
                        }

                        if (cmnd_data[i].Contains("MOBILE NETWORK CODE"))
                        {
                            i = i + 1;
                            flag2 = 0;

                            while (flag2 != 1)
                            {
                                if (cmnd_data[i].Trim() != "")
                                {
                                    MNC = cmnd_data[i].Trim().ToString();
                                    flag2 = 1;
                                }
                                i = i + 1;
                            }

                        }

                        if (cmnd_data[i].Contains("COMMAND EXECUTED"))
                        {
                            break;
                        }

                        if (MCC != "" && MNC != "")
                        {
                            if (_NEType == "MSC")
                            {
                                query = "insert into msc_" + CircleName + " (`VENDOR`,`CIRCLE`,`NE_NAME`,`NODE_TYPE`,`MCC`, `MNC`) values ('" + _vendor + "','" + circle + "', '" + element + "', '" + TYPE + "', '" + MCC + "', '" + MNC + "') ";
                                ExecuteSQLQuery(ref conn, query);
                            }

                            else
                            {
                                query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + "  (`VENDOR`,`CIRCLE`,`NE_NAME`,`NODE_TYPE`,`MCC`, `MNC`) values ('" + _vendor + "','" + circle + "', '" + element + "', '" + TYPE + "', '" + MCC + "', '" + MNC + "') ";
                                ExecuteSQLQuery(ref conn, query);
                            }

                            i = cmnd_data.Length;
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZWVI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZWVI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZWVI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZQNI(string output, string CircleName, string NE_Type)
        {
            DataTable dt = new DataTable();
            string MSS_C_NO = string.Empty;
            string MSS = string.Empty;
            string query = String.Empty;
            int flag1;
            try
            {
                string[] cmnd_output = Regex.Split(output, "\r\n");   //string.Split(output, "\r\n");


                for (int i = 0; i <= cmnd_output.Length - 1; i++)
                {
                    try
                    {

                        if (cmnd_output[i].Contains("C-NUM"))
                        {
                            flag1 = 0;
                            i = i + 1;

                            while (flag1 != 1)
                            {
                                if (cmnd_output[i].Contains(" "))
                                {
                                    while (cmnd_output[i] == " ")
                                    {
                                        i = i + 1;
                                    }
                                }
                                string check = cmnd_output[i].Trim().ToString();
                                if (check.Trim().Length > 5)
                                {
                                    string temp = Regex.Replace(cmnd_output[i].ToString().Trim(), " {2,}", "~");
                                    string[] temp1 = Regex.Split(temp, "~");
                                    MSS_C_NO = temp1[3].Trim().ToString();
                                    MSS = temp1[4].Trim().ToString();
                                }

                                if (MSS != "" && MSS_C_NO != "")
                                {
                                    if (NE_Type == "HLR")
                                    {
                                        query = "Select * from hlr_" + CircleName + " Where NE_NAME='" + _ne_name.Trim() + "' and node_Type='" + NE_Type + "' and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                                        SQLQuery(ref conn, ref dt, query);
                                        if (dt.Rows.Count > 0)
                                        {
                                            query = "Update hlr_" + CircleName + " Set HLR='" + MSS.Trim() + "',HLR_C_NO='" + MSS_C_NO.Trim() + "' Where NE_NAME='" + _ne_name.Trim() + "' and node_Type='" + NE_Type + "' and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                                        }

                                        ExecuteSQLQuery(ref conn, query);

                                    }

                                    else if (NE_Type == "MSS_LINKS")
                                    {
                                        query = "update mss_link_" + CircleName + "_" + group_name + "  set mss = '" + MSS + "', mss_c_no = '" + MSS_C_NO + "' where circle = '" + CircleName + "' and node_type = '" + NE_Type + "' and ne_name = '" + _ne_name + "'   ";
                                        ExecuteSQLQuery(ref conn, query);

                                    }
                                    else if (_NEType == "MSC")
                                    {
                                        query = "update msc_" + CircleName + " set mss = '" + MSS + "', mss_c_no = '" + MSS_C_NO + "' where circle = '" + CircleName + "' and node_type = '" + NE_Type + "' and ne_name = '" + _ne_name + "'   ";
                                        ExecuteSQLQuery(ref conn, query);
                                    }

                                    else
                                    {
                                        query = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set mss = '" + MSS + "', mss_c_no = '" + MSS_C_NO + "' where circle = '" + CircleName + "' and node_type = '" + NE_Type + "' and ne_name = '" + _ne_name + "'   ";
                                        ExecuteSQLQuery(ref conn, query);
                                    }

                                    flag1 = 1;
                                    i = cmnd_output.Length;

                                }

                                i = i + 1;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZQNI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZQNI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZQNI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        //code added by rahul on 24-08-2018
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_mss_ZWTI(string output, string CircleName, string NE_Type)
        {
            string Unit = string.Empty;
            string Loc = string.Empty;
            string Power = string.Empty;
            string CP = string.Empty;
            string[] temp1;
            string[] values;
            string data = string.Empty;
            string temp_data = string.Empty;
            string newquery = string.Empty;
            string Track = string.Empty;
            try
            {
                if (output.Contains("READING DATA FROM DATABASE ..."))
                {
                    data = output.Remove(0, output.IndexOf("READING DATA FROM DATABASE ..."));
                }

                data = data.Trim();
                string[] temp;
                string[] cmnd_data = Regex.Split(data.Trim(), "READING DATA FROM DATABASE ...");

                for (int i = 0; i <= cmnd_data.Length - 1; i++)
                {
                    try
                    {
                        Unit = "";
                        Loc = "";
                        Power = "";
                        CP = "";

                        if (cmnd_data[i] != " " && cmnd_data[i] != "")
                        {
                            temp = Regex.Split(cmnd_data[1].Trim(), "\r\n");

                            for (int j = 0; j <= temp.Length - 1; j++)
                            {
                                if (temp[j].Contains("IN LOC"))
                                {
                                    temp_data = Regex.Replace(temp[j].ToString().Trim(), " {2,}", "~");
                                    values = Regex.Split(temp_data, "~");
                                    Unit = values[0].ToString().Trim();
                                    Loc = values[2].ToString().Trim();
                                }

                                if (temp[j].Contains("TRACK") && Power == "")
                                {
                                    temp_data = Regex.Replace(temp[j].ToString().Trim(), " {2,}", "~");
                                    values = Regex.Split(temp_data, "~");
                                    Power = values[0].ToString().Trim();
                                }

                                if (temp[j].Contains("CP"))
                                {
                                    temp_data = Regex.Replace(temp[j].ToString().Trim(), " {3,}", "~");
                                    CP = temp_data.Substring(0, 12).ToString().Trim();
                                }

                                if (Unit != "" && Loc != "" && Power != "" && CP != "")
                                {
                                    //newquery = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set CGR = '" + circuit_group_number + "', NCGR = '" + circuit_group_name + "' where NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + CircleName + "' AND CGR_SPC like '%" + cgr_spc + "/%'";
                                    //newquery = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set CGR = '" + circuit_group_number + "', NCGR = '" + circuit_group_name + "' where MGW_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + CircleName + "'";

                                    newquery = "insert into redundancy_subtrack_mss_" + _CircleName + "(VENDOR,CIRCLE,NE_NAME,NODE_TYPE,UNIT,LOC,POWER_CARD,CP,ATCA_FLAG) Values('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + _NEType.Trim() + "','" + Unit.Trim() + "','" + Loc.Trim() + "','" + Power + "','" + CP + "','1')";
                                    ExecuteSQLQuery(ref conn, newquery);
                                    Unit = "";
                                    Loc = "";
                                    Power = "";
                                    CP = "";
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZEDO ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZWTI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZWTI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }



        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="command_output"></param>
        ///// <param name="CircleName"></param>
        ///// <param name="NE_Type"></param>
        //public void ParseData_ZCEL(string command_output, string CircleName, string NE_Type)
        //{
        //    string[] output = Regex.Split(command_output.Trim(), "\r\n");
        //    int size = output.Length;
        //    string[] temp;
        //    string[] value;
        //    string[] temp1;
        //    string temp_data = string.Empty;
        //    string SqlStr = string.Empty;
        //    string PCM_TSL = string.Empty;
        //    string NCGR = string.Empty;
        //    string CGR = string.Empty;

        //    DataTable dt = new DataTable();

        //    for (int i = 0; i <= size - 1; i++)
        //    {
        //        try
        //        {
        //        end_of_file: if (output[i].Contains("CGR") && !output[i].Contains("ZRCI") && !output[i].Contains("COMMAND EXECUTED"))
        //            {
        //                while (!output[i].Contains("COMMAND EXECUTED"))
        //                {
        //                comehere: if (output[i].Contains("CGR"))
        //                    {
        //                        temp = Regex.Split(output[i], "    ");

        //                        for (int j = 0; j < temp.Length; j++)
        //                        {
        //                            if (temp[j].Contains("CGR") && !temp[j].Contains("NCGR"))
        //                            {
        //                                temp1 = Regex.Split(temp[j].ToString(), ":");
        //                                CGR = temp1[1].Trim();
        //                            }

        //                            else if (temp[j].Contains("NCGR"))
        //                            {
        //                                temp1 = Regex.Split(temp[j].ToString(), ":");

        //                                NCGR = temp1[1].Trim();

        //                            }
        //                        }
        //                    }


        //                    //if (output[i].Contains("PCM-TSL") && output[i].Contains("STATE") && !output[i].Contains("HGR"))
        //                    if (output[i].Contains("PCM-TSL") && output[i].Contains("STATE"))
        //                    {
        //                        while (!output[i].Contains("CGR :") && !output[i].Contains("COMMAND EXECUTED"))
        //                        {
        //                            //if (output[i].Trim() != "" && !output[i].Contains("HGR"))   //if (output[i].Trim() != "" && output[i].Contains("HGR"))
        //                            //{
        //                                if (output[i].Trim() != "")
        //                                {
        //                                    value = Regex.Split(output[i].ToString(), "  ");
        //                                    value = Regex.Split(value[0].ToString(), "-");
        //                                    //PCM_TSL = value[0];

        //                                    //if (PCM_TSL.Length <= 4)
        //                                    //{
        //                                      //  SQLQuery(ref conn, ref dt, "select MGW_CGR,MGW_NCGR,ET from mss_mgw_map_" + CircleName + "_" + group_name + "  where ET = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + CircleName + "' ");
        //                                    //}
        //                                    //else
        //                                    //{
        //                                      //  SQLQuery(ref conn, ref dt, "select MGW_CGR,MGW_NCGR,ET from mss_mgw_map_" + CircleName + "_" + group_name + "  where APCM = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + CircleName + "' ");
        //                                    //}

        //                                    if (dt.Rows.Count > 0)
        //                                    {
        //                                        //if (PCM_TSL.Length <= 4)
        //                                        //{
        //                                            if (CGR.ToString() != "247")
        //                                            {
        //                                                SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_CGR='" + CGR + "', MGW_NCGR= '" + NCGR + "' Where ET = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + CircleName + "' ";
        //                                            }
        //                                        //}

        //                                        //else
        //                                        //{
        //                                           // if (CGR.ToString() != "247")
        //                                            //{
        //                                              //  SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set MGW_CGR='" + CGR + "', MGW_NCGR= '" + NCGR + "' Where apcm = '" + PCM_TSL + "' and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + _CircleName + "' ";
        //                                            //}

        //                                        }

        //                                        if (SqlStr != "")
        //                                        {
        //                                            ExecuteSQLQuery(ref conn, SqlStr);
        //                                            SqlStr = "";
        //                                        }

        //                                    }


        //                                }
        //                            //}

        //                            i = i + 1;
        //                       // }

        //                        if (output[i].Contains("CGR :"))
        //                        {
        //                            goto comehere;
        //                        }

        //                        if (output[i].Contains("COMMAND EXECUTED"))
        //                        {
        //                            goto end_of_file;
        //                        }

        //                    }



        //                    i = i + 1;
        //                }


        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
        //            //if (!Directory.Exists(parsing_error_path))
        //            //{
        //            //    Directory.CreateDirectory(parsing_error_path);
        //            //}

        //            //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZRCI_PRINT_4 ");
        //            //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        //            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //            oExceptionLog.WriteExceptionErrorToFile("ParseData_ZRCI_PRINT_4_withContinue()", ErrorMsg, "", ref FileError);

        //            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //            ExecuteSQLQuery(ref conn, ErrorQuery);
        //            continue;
        //        }
        //    }

        //}

        // FOR MSS EXTRA ET

        //public void ParseData_mss_ZNSI(string output, string CircleName, string NE_Type, string net)
        //{
        //    string temp_data = string.Empty;
        //    string slc = string.Empty;
        //    string[] value;
        //    string[] temp;
        //    string query = string.Empty; ;
        //    string cgr_spc = string.Empty;
        //    string cgr_net = string.Empty;
        //    string link = string.Empty;
        //    try
        //    {
        //        string[] cmnd_output = Regex.Split(output.Trim(), "COMMAND EXECUTED");
        //        string data = cmnd_output[0].Trim();
        //        cmnd_output = Regex.Split(data, "SLC");

        //        if (cmnd_output.Length == 1)
        //        {
        //            data = cmnd_output[0].Trim().ToString();
        //        }
        //        else
        //        {
        //            data = cmnd_output[1].Trim().ToString();
        //        }


        //        temp = Regex.Split(data, "\r\n");

        //        for (int i = 0; i <= temp.Length - 1; i++)
        //        {
        //            try
        //            {

        //                if (!temp[i].Trim().Contains("----------") && temp[i] != "")
        //                {
        //                    while (temp[i] != "COMMAND EXECUTED" && i <= temp.Length - 1 && temp[i].Trim() != "")
        //                    {
        //                        if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
        //                        {
        //                            temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
        //                            value = Regex.Split(temp_data.ToString().Trim(), "~");

        //                            if (value.Length >= 2)
        //                            {
        //                                slc = value[value.Length - 1].ToString();
        //                                link = value[value.Length - 2].ToString();
        //                            }

        //                            if (value.Length > 3)
        //                            {
        //                                cgr_net = value[0].Trim();
        //                                cgr_spc = value[1].Trim();
        //                            }

        //                            query = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set SLC = '" + slc.Trim() + "', CGR_NET = '" + cgr_net + "', cgr_spc = '" + cgr_spc + "'   where link = '" + link.Trim() + "' AND ne_name = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and EXTRA_ET_FLAG = '1'  ";

        //                        }

        //                        ExecuteSQLQuery(ref conn, query);

        //                        i = i + 1;
        //                        if (i > temp.Length - 1)
        //                        {
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
        //                //if (!Directory.Exists(parsing_error_path))
        //                //{
        //                //    Directory.CreateDirectory(parsing_error_path);
        //                //}

        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_mss_ZNSI ");
        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        //                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //                oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNSI_SUBwithConinue()", ErrorMsg, "", ref FileError);

        //                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //                ExecuteSQLQuery(ref conn, ErrorQuery);
        //                continue;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //        oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNSI_SUBwithConinue()", ErrorMsg, "", ref FileError);
        //    }

        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        /// <param name="net"></param>
        public void ParseData_mss_ZNSI(string output, string CircleName, string NE_Type, string net)
        {
            DataTable dt = new DataTable();
            string temp_data = string.Empty;
            string slc = string.Empty;
            string[] value;
            string[] temp;
            string query = string.Empty; ;
            string query1 = string.Empty;
            string link = string.Empty;
            string cgr_spc = string.Empty;
            string cgr_net = string.Empty;
            string linkset = string.Empty;
            string SPCCDODEHD = string.Empty;
            string LSSTATE = string.Empty;
            string ASSOCSTATE = string.Empty;
            try
            {
                string[] cmnd_output = Regex.Split(output.Trim(), "COMMAND EXECUTED");
                string data = cmnd_output[0].Trim();
                if (data.Contains("SLC"))
                {
                    cmnd_output = Regex.Split(data, "SLC");
                    if (cmnd_output.Length == 1)
                    {
                        data = cmnd_output[0].Trim().ToString();
                    }
                    else
                    {
                        data = cmnd_output[1].Trim().ToString();
                    }
                }
                else
                {
                    cmnd_output = Regex.Split(data, "LS    IP");
                    if (cmnd_output.Length == 1)
                    {
                        data = cmnd_output[0].Trim().ToString();
                    }
                    else
                    {
                        data = cmnd_output[1].Trim().ToString();
                    }                    
                }
                temp = Regex.Split(data, "\r\n");
                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    if (i >= temp.Length - 1)
                    {
                        break;
                    }
                    try
                    {
                        if (!temp[i].Trim().Contains("----------") && temp[i] != "" && !temp[i].Trim().Contains("LS    IP") && !temp[i].Trim().Contains("NET SP CODE H/D        LINK SET              ASSOCIATION SET       STATE LINK") && !temp[i].Trim().Contains("LINK TEST NOT ALLOWED"))
                        {
                            while (temp[i] != "COMMAND EXECUTED" && i <= temp.Length - 1 && temp[i].Trim() != "")
                            {
                                if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                {
                                    temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                    value = Regex.Split(temp_data.ToString().Trim(), "~");

                                    if (value.Length > 3)
                                    {
                                        cgr_net = value[0].ToString();
                                        cgr_spc = value[1].ToString();
                                        linkset = value[2].ToString();
                                        link = value[4].ToString();
                                        slc = value[5].ToString();

                                    }
                                    if (value.Length == 2)
                                    {
                                        slc = value[1].ToString();
                                        link = value[0].ToString();
                                    }
                                    if (slc != "" && linkset != "" && cgr_net != "" && cgr_spc != "" && link != "")
                                    {
                                        query = "select distinct CGR_SPC,CGR_NET,LINK,LINKSET,SLC from mss_mgw_map_" + CircleName + "_" + group_name + " where CGR_SPC = '" + cgr_spc.Trim() + "' AND CGR_NET='" + cgr_net + "' and circle = '" + CircleName + "' and NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' and link is null and linkset is null";
                                        SQLQuery(ref conn, ref dt, query);

                                        if (dt.Rows.Count >= 1)
                                        {
                                            query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set SLC = '" + slc.Trim() + "', LINK = '" + link.Trim() + "', LINKSET = '" + linkset.Trim() + "' where CGR_SPC = '" + cgr_spc.Trim() + "' and CGR_NET='" + cgr_net + "' AND MGW_NAME = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' AND NODE_TYPE = '" + _NEType + "' and link is null and linkset is null";

                                            //query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set SLC = '" + slc.Trim() + "', CGR_NET = '" + cgr_net.Trim() + "', cgr_spc = '" + cgr_spc.Trim() + "', LINK = '" + link.Trim() + "' where LINKSET = '" + linkset.Trim() + "' AND ne_name = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and EXTRA_ET_FLAG = '1' ";
                                            ExecuteSQLQuery(ref conn, query1);
                                        }
                                        else
                                        {
                                            query = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                            SQLQuery(ref conn, ref dt, query);
                                            if (dt.Rows.Count >= 1)
                                            {
                                                query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_SPC,CGR_NET,LINK,LINKSET,SLC) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + cgr_spc.Trim() + "', '" + cgr_net.Trim() + "', '" + link.Trim() + "', '" + linkset.Trim() + "', '" + slc.Trim() + "')";
                                                ExecuteSQLQuery(ref conn, query1);
                                            }
                                            else if (dt.Rows.Count <= 0)
                                            {
                                                query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_SPC,CGR_NET,LINK,LINKSET,SLC) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + cgr_spc.Trim() + "', '" + cgr_net.Trim() + "', '" + link.Trim() + "', '" + linkset.Trim() + "', '" + slc.Trim() + "')";
                                                ExecuteSQLQuery(ref conn, query1);
                                            }
                                            //query1 = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (CGR_SPC,CGR_NET,LINK,LINKSET,SLC) values (' " + cgr_spc.Trim() + " ', ' " + cgr_net.Trim() + " ', ' " + link.Trim() + " ', ' " + linkset.Trim() + " ', ' " + slc.Trim() + " ') ";                                            
                                        }

                                    }
                                    i = i + 1;
                                    if (i > temp.Length - 1)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        
                        else if (temp[i].Trim().Contains("ASSOCIATION SET") || temp[i].Trim().Contains("LINK TEST NOT ALLOWED"))
                        {                            
                            i = i + 2;
                            if (i > temp.Length - 1)
                            {
                                break;
                            }
                            if (!temp[i].Trim().Contains("----------") && temp[i] != "")
                            {
                                //while (temp[i] != "COMMAND EXECUTED" && i <= temp.Length - 1 && temp[i].Trim() != "")
                               // {
                                    if (temp[i].Trim().Contains("LINK TEST NOT ALLOWED"))
                                    {
                                        //i = i + 2;
                                        //if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                        //{
                                        //    temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                        //    value = Regex.Split(temp_data.ToString().Trim(), "~");

                                        //    cgr_net = value[0].ToString();
                                        //    cgr_spc = value[1].ToString();
                                        //    linkset = value[2].ToString();
                                        //    ASSOCSTATE = value[3].ToString();
                                        //    LSSTATE = value[4].ToString();
                                        //    link = value[5].ToString();

                                        //    if (cgr_spc != "")
                                        //    {
                                        //        query = "select distinct CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK from mss_mgw_map_" + CircleName + "_" + group_name + " where CGR_SPC = '" + cgr_spc.Trim() + "' AND CGR_NET='" + cgr_net + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' and LINKSET is null and ASSOCIATION_SET is null and STATE is null and LINK is null";
                                        //        SQLQuery(ref conn, ref dt, query);

                                        //        if (dt.Rows.Count > 0)
                                        //        {
                                        //            query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK = '" + link.Trim() + "', STATE = '" + LSSTATE.Trim() + "', ASSOCIATION_SET = '" + ASSOCSTATE.Trim() + "',LINKSET = '" + linkset.Trim() + "' where CGR_SPC='" + cgr_spc + "' and CGR_NET='" + cgr_net + "' AND MGW_NAME = '" + _ne_name.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' AND NODE_TYPE = '" + _NEType + "' and LINKSET is null and ASSOCIATION_SET is null and STATE is null and LINK is null";

                                        //            //query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK = '" + link.Trim() + "', STATE = '" + LSSTATE.Trim() + "', ASSOCIATION_SET = '" + ASSOCSTATE.Trim() + "',CGR_NET='" + cgr_net + "',CGR_SPC='" + cgr_spc + "' where LINKSET = '" + linkset.Trim() + "' AND ne_name = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and EXTRA_ET_FLAG = '1' ";
                                        //            ExecuteSQLQuery(ref conn, query1);
                                        //        }
                                        //        else
                                        //        {

                                        //            query = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                        //            SQLQuery(ref conn, ref dt, query);
                                        //            if (dt.Rows.Count >= 1)
                                        //            {
                                        //                query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + cgr_net.Trim() + "', '" + cgr_spc.Trim() + "', '" + linkset.Trim() + "', '" + ASSOCSTATE.Trim() + "', '" + LSSTATE.Trim() + "', '" + link.Trim() + "')";
                                        //                ExecuteSQLQuery(ref conn, query1);
                                        //            }
                                        //            //query1 = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values (' " + cgr_net.Trim() + " ', ' " + cgr_spc.Trim() + " ', ' " + linkset.Trim() + " ', ' " + ASSOCSTATE.Trim() + " ', ' " + LSSTATE.Trim() + " ', ' " + link.Trim() + " ') ";
                                        //        }
                                        //    }
                                        //    i = i + 1;
                                        //    if (i > temp.Length - 1)
                                        //    {
                                        //        break;
                                        //    }

                                        //}
                                        //}
                                        //}
                                    }
                                    else
                                    {
                                        if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                        {                                            
                                            temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                            value = Regex.Split(temp_data.ToString().Trim(), "~");

                                            cgr_net = value[0].ToString();
                                            cgr_spc = value[1].ToString();
                                            linkset = value[2].ToString();
                                            ASSOCSTATE = value[3].ToString();
                                            LSSTATE = value[4].ToString();
                                            link = value[5].ToString();

                                            if (cgr_spc != "")
                                            {
                                                query = "select distinct CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK from mss_mgw_map_" + CircleName + "_" + group_name + " where CGR_SPC = '" + cgr_spc.Trim() + "' and CGR_NET='" + cgr_net + "' and circle = '" + CircleName + "' and MGW_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' and LINKSET is null and ASSOCIATION_SET is null and STATE is null and LINK is null";

                                                //query = "select distinct CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK from mss_mgw_map_" + CircleName + "_" + group_name + " where circle = '" + CircleName + "' and NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' ";                                                
                                                SQLQuery(ref conn, ref dt, query);

                                                if (dt.Rows.Count > 0)
                                                {
                                                    query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK = '" + link.Trim() + "', STATE = '" + LSSTATE.Trim() + "', ASSOCIATION_SET = '" + ASSOCSTATE.Trim() + "', LINKSET = '" + linkset.Trim() + "' where CGR_SPC='" + cgr_spc + "' and CGR_NET='" + cgr_net + "' AND MGW_NAME = '" + _ne_name.Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' AND NODE_TYPE = '" + _NEType + "' and LINKSET is null and ASSOCIATION_SET is null and STATE is null and LINK is null";

                                                    //query1 = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  set LINK = '" + link.Trim() + "', STATE = '" + LSSTATE.Trim() + "', ASSOCIATION_SET = '" + ASSOCSTATE.Trim() + "',CGR_NET='" + cgr_net + "',CGR_SPC='" + cgr_spc + "' where LINKSET = '" + linkset.Trim() + "' AND ne_name = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and EXTRA_ET_FLAG = '1' ";
                                                    ExecuteSQLQuery(ref conn, query1);
                                                }
                                                else
                                                {
                                                    query = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP from  mss_mgw_map_" + _CircleName + "_" + group_name + " where NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                                    SQLQuery(ref conn, ref dt, query);
                                                    if (dt.Rows.Count >= 1)
                                                    {
                                                        query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + NE_Type + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + cgr_net.Trim() + "', '" + cgr_spc.Trim() + "', '" + linkset.Trim() + "', '" + ASSOCSTATE.Trim() + "', '" + LSSTATE.Trim() + "', '" + link.Trim() + "')";
                                                        ExecuteSQLQuery(ref conn, query1);
                                                    }
                                                if (dt.Rows.Count <= 0)
                                                {
                                                    query1 = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,NODE_TYPE,CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _ne_name.Trim() + "','" + NE_Type + "','" + cgr_net.Trim() + "', '" + cgr_spc.Trim() + "', '" + linkset.Trim() + "', '" + ASSOCSTATE.Trim() + "', '" + LSSTATE.Trim() + "', '" + link.Trim() + "')";
                                                    ExecuteSQLQuery(ref conn, query1);
                                                }
                                                //query1 = "insert into mss_mgw_map_" + CircleName + "_" + group_name + " (CGR_NET,CGR_SPC,LINKSET,ASSOCIATION_SET,STATE,LINK) values (' " + cgr_net.Trim() + " ', ' " + cgr_spc.Trim() + " ', ' " + linkset.Trim() + " ', ' " + ASSOCSTATE.Trim() + " ', ' " + LSSTATE.Trim() + " ', ' " + link.Trim() + " ') ";
                                            }
                                            cgr_net =cgr_spc = linkset = ASSOCSTATE = LSSTATE = link = "";
                                        }                                           
                                        }
                                    }                                                                
                            }
                        }                        
                        //if (i > temp.Length - 1)
                        //{
                        //    break;
                        //}
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_mss_ZNSI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNSI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNSI_SUBwithConinue()", ErrorMsg, "", ref FileError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        public void update_mss_extra_et(string circle)
        {

            DataTable dt = new DataTable();
            DataTable dtnew = new DataTable();
            string mss_c_no = string.Empty;
            string mss_spc_na0 = string.Empty;
            string mss_spc_na1 = string.Empty;
            string MSS_MCC = string.Empty;
            string MSS_mnc = string.Empty;
            string MSS = string.Empty;
            string element_ip = string.Empty;
            string node_name = string.Empty;
            try
            {
                SQLQuery(ref conn, ref dtnew, "select distinct ne_name FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where  circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and node_type = 'MSS'");
                for (int i = 0; i < dtnew.Rows.Count; i++)
                {
                    node_name = dtnew.Rows[i]["ne_name"].ToString();

                    string query = "SELECT distinct mss_c_no FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where ne_name ='" + node_name + "' and node_type = '" + _NEType + "' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "';";
                    SQLQuery(ref conn, ref dt, query);
                    if (dt.Rows.Count > 0)
                        mss_c_no = dt.Rows[0][0].ToString();

                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + "  set `MSS_C_NO` = '" + mss_c_no + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1' ");
                    dt.Clear();

                    query = "SELECT distinct `mss_spc` FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where ne_name ='" + node_name + "' and node_type = '" + _NEType + "' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and cgr_net = 'NA0';";
                    SQLQuery(ref conn, ref dt, query);

                    if (dt.Rows.Count > 0)
                        mss_spc_na0 = dt.Rows[0][0].ToString();

                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + "  set `mss_spc` = '" + mss_spc_na0 + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1' and cgr_net = 'NA0' ");
                    dt.Clear();

                    query = "SELECT distinct `mss_spc` FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where ne_name ='" + node_name + "' and node_type = '" + _NEType + "' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and cgr_net = 'NA1';";
                    SQLQuery(ref conn, ref dt, query);

                    if (dt.Rows.Count > 0)
                        mss_spc_na1 = dt.Rows[0][0].ToString();

                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + "  set `mss_spc` = '" + mss_spc_na1 + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1' and cgr_net = 'NA1' ");
                    dt.Clear();

                    // VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP, VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME, MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID, MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE
                    query = "SELECT distinct `MCC` FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where ne_name ='" + node_name + "' and node_type = '" + _NEType + "' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);

                    if (dt.Rows.Count > 0)
                        MSS_MCC = dt.Rows[0][0].ToString();

                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + "  set `MCC` = '" + MSS_MCC + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();

                    query = "SELECT distinct `MNC` FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where ne_name ='" + node_name + "' and node_type = '" + _NEType + "' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);

                    if (dt.Rows.Count > 0)
                        MSS_mnc = dt.Rows[0][0].ToString();

                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + "  set `MNC` = '" + MSS_mnc + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();

                    query = "SELECT distinct `MSS` FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where ne_name ='" + node_name + "' and node_type = '" + _NEType + "' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);

                    if (dt.Rows.Count > 0)
                        MSS = dt.Rows[0][0].ToString();

                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + "  set `MSS` = '" + MSS + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();

                    query = "SELECT distinct `element_ip` FROM mss_mgw_map_" + _CircleName + "_" + group_name + "  where ne_name ='" + node_name + "' and node_type = '" + _NEType + "' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);

                    if (dt.Rows.Count > 0)
                        element_ip = dt.Rows[0][0].ToString();

                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + "  set `element_ip` = '" + element_ip + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + node_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("update_mss_extra_et()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_mss_ZNEL(string command_output, string CircleName, string NE_Type)
        {
            DataTable dt = new DataTable();
            string output = command_output;
            string temp1 = string.Empty; ;
            string temp2 = string.Empty;
            string text = string.Empty; ;
            string[] value;
            string[] temp;
            string[] temp_val2;
            string query = string.Empty; ;
            string data = string.Empty; ;
            string cgr_net = string.Empty; ;
            string LINK = string.Empty; ;
            string LINK_SET = string.Empty; ;
            string LINK_STATE = string.Empty; ;
            string UNIT = string.Empty; ;
            string nextQuery = string.Empty; ;
            string PCM_TSL = string.Empty; ;
            string TERM = string.Empty;
            string TF = string.Empty;
            string EXTERN_PCM_TSL = string.Empty;
            string INT_PCM_TSL = string.Empty;
            string BIT_RATE = string.Empty;
            string ASSOCIATION_SET = string.Empty;
            DataTable newdt = new DataTable();
            try
            {
                if (output.Contains("RATE"))
                {
                    if (output.Contains("SIGNALLING LINK NOT IN SIGNALLING LINK SET"))
                    {
                        //temp1 = output;
                        if (output.Contains("M3UA BASED LINKS"))
                        {
                            temp = Regex.Split(output, "M3UA BASED LINKS");
                            temp2 = temp[1];
                        }
                    }
                    else
                    {
                        temp1 = output.Remove(0, output.IndexOf("RATE"));
                        if (temp1.Contains("M3UA BASED LINKS"))
                        {
                            temp = Regex.Split(temp1, "M3UA BASED LINKS");
                            temp1 = temp[0];
                            temp2 = temp[1];
                        }
                    }
                }
                else
                {
                    //temp1 = output;
                    if (output.Contains("M3UA BASED LINKS"))
                    {
                        temp = Regex.Split(output, "M3UA BASED LINKS");
                        temp2 = temp[1];
                    }
                }

                //if (output.Contains("RATE"))
                //{
                //    temp1 = output.Remove(0, output.IndexOf("RATE"));
                //    temp = Regex.Split(temp1, "M3UA BASED LINKS");

                //    temp1 = temp[0];
                //}
                //else if (temp1.Contains("M3UA BASED LINKS"))
                //{
                //    temp = Regex.Split(temp1, "M3UA BASED LINKS");
                //    //temp1 = temp[0];
                //    temp1 = temp[1];
                //}

                #region[SS7 with RATE]
                temp = Regex.Split(temp1, "\r\n");
                if (!string.IsNullOrEmpty(temp1))
                {
                    for (int i = 0; i <= temp.Length - 1; i++)
                    {
                        try
                        {
                            #region[ SS7 ]
                            if (temp[i].Contains("AV-") || temp[i].Contains("UA-"))
                            {
                                newdt.Clear();
                                text = "";
                                data = "";
                                data = temp[i].Trim();
                                string[] data_split = Regex.Split(data, " ");
                                for (int j = 0; j <= data_split.Length - 1; j++)
                                {
                                    if (data_split[j] != "")
                                    {
                                        text = text + data_split[j].Trim() + ";";
                                    }
                                }
                                if (text != "")
                                {
                                    value = Regex.Split(text, ";");
                                    LINK = value[0];
                                    LINK_SET = value[1] + " " + value[2];
                                    LINK_STATE = value[3];
                                    UNIT = value[4];
                                    TERM = value[5];
                                    TF = value[6];
                                    EXTERN_PCM_TSL = value[7];
                                    INT_PCM_TSL = value[8];
                                    BIT_RATE = value[9];

                                    if (value.Length == 10)
                                    {
                                        temp_val2 = Regex.Split(value[8].ToString().Trim(), "-");
                                        PCM_TSL = temp_val2[0];
                                    }
                                    else if (value.Length == 6)
                                    {
                                        temp_val2 = Regex.Split(value[4].ToString().Trim(), "-");
                                        PCM_TSL = temp_val2[0];
                                    }
                                    text = "";
                                    nextQuery = "select distinct ET from mss_mgw_map_" + CircleName + "_" + group_name + "  where NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' AND circle = '" + CircleName + "' and ET = '" + PCM_TSL + "' ";
                                    SQLQuery(ref conn, ref dt, nextQuery);

                                    //if (dt.Rows.Count <= 0)
                                    //code change by rahul
                                    if (dt.Rows.Count >= 0)
                                    {
                                        query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + "  (`VENDOR`,`CIRCLE`,`NE_NAME`,`NODE_TYPE`,`EXTRA_ET_FLAG`,`LINK`,`LINKSET`,`ET`,`STATE`,`TERM_ID`,`TF`,`EXTERN_PCM_TSL`,`INT_PCM_TSL`,`BIT_RATE`) values ('" + _vendor + "','" + CircleName + "','" + _ne_name + "','" + _NEType + "','1','" + LINK + "','" + LINK_SET + "','" + PCM_TSL + "','" + LINK_STATE + "','" + TERM + "','" + TF + "','" + EXTERN_PCM_TSL + "','" + INT_PCM_TSL + "','" + BIT_RATE + "')";
                                        ExecuteSQLQuery(ref conn, query);
                                    }
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                            //if (!Directory.Exists(parsing_error_path))
                            //{
                            //    Directory.CreateDirectory(parsing_error_path);
                            //}

                            //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_mss_ZNEL ");
                            //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNEL_SUBwithConitnue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            continue;
                        }
                    }
                }
                #endregion

                #region[M3UA with Association Set]
                temp_val2 = Regex.Split(temp2, "\r\n");
                if (!string.IsNullOrEmpty(temp2))
                {
                    for (int i = 0; i <= temp_val2.Length - 1; i++)
                    {
                        try
                        {
                            #region[M3UA]

                            if (temp_val2[i].Contains("AV-") || temp_val2[i].Contains("UA-"))
                            {
                                newdt.Clear();
                                text = "";
                                data = "";
                                data = temp_val2[i].Trim();
                                string[] data_split = Regex.Split(data, " ");
                                for (int j = 0; j <= data_split.Length - 1; j++)
                                {
                                    if (data_split[j] != "")
                                    {
                                        text = text + data_split[j].Trim() + ";";
                                    }
                                }
                                if (text != "")
                                {
                                    value = Regex.Split(text, ";");
                                    LINK = value[0];
                                    LINK_SET = value[1] + " " + value[2];
                                    ASSOCIATION_SET = value[3];
                                    LINK_STATE = value[4];
                                    text = "";
                                    nextQuery = "select distinct ET from mss_mgw_map_" + CircleName + "_" + group_name + "  where NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + _NEType + "' AND circle = '" + CircleName + "' and ET = '" + PCM_TSL + "' ";
                                    SQLQuery(ref conn, ref dt, nextQuery);

                                    //if (dt.Rows.Count <= 0)
                                    //code change by rahul
                                    if (dt.Rows.Count >= 0)
                                    {
                                        query = "insert into mss_mgw_map_" + CircleName + "_" + group_name + "  (`VENDOR`,`CIRCLE`,`NE_NAME`,`NODE_TYPE`,`EXTRA_ET_FLAG`,`LINK`,`LINKSET`,`ET`,`STATE`,`ASSOCIATION_SET`) values ('" + _vendor + "','" + CircleName + "','" + _ne_name + "','" + _NEType + "','1','" + LINK + "','" + LINK_SET + "','" + PCM_TSL + "','" + LINK_STATE + "','" + ASSOCIATION_SET + "')";
                                        ExecuteSQLQuery(ref conn, query);
                                    }
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                            //if (!Directory.Exists(parsing_error_path))
                            //{
                            //    Directory.CreateDirectory(parsing_error_path);
                            //}

                            //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_mss_ZNEL ");
                            //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNEL_SUBwithConitnue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            continue;
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_mss_ZNEL()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_SIGNALLING_ASSOCIATION_ALL(string output, string _CircleName, string NE_Type) //Added by AJAY
        {
            DataTable dt = new DataTable();
            string temp1 = string.Empty;
            string temp_data = string.Empty;
            string association_id = string.Empty;
            string primary_local_ip_address = string.Empty;
            string secondary_local_ip_address = string.Empty;
            string node = string.Empty;
            string remote_name = string.Empty;
            string primary_remote_ip_address = string.Empty;
            string secondary_remote_ip_address = string.Empty;
            string sctp_profile = string.Empty;
            string status = string.Empty;
            string query = string.Empty;
            string query1 = string.Empty;
            string[] temp;
            string data = string.Empty;
            string cmnd_output = output.Trim();

            try
            {
                temp = Regex.Split(cmnd_output, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (temp[i].Contains("association id"))
                        {
                            data = temp[i].Trim().ToString();
                            association_id = data.Remove(0, data.LastIndexOf(":"));
                            association_id = association_id.Replace(":", " ");
                            association_id = association_id.Trim();

                        }

                        if (temp[i].Contains("primary-local-ip-addr"))
                        {
                            data = temp[i].Trim().ToString();
                            primary_local_ip_address = data.Remove(0, data.LastIndexOf(":"));
                            primary_local_ip_address = primary_local_ip_address.Replace(":", " ");
                            primary_local_ip_address = primary_local_ip_address.Trim();
                        }

                        if (temp[i].Contains("secondary-local-ip-addr"))
                        {
                            data = temp[i].Trim().ToString();
                            secondary_local_ip_address = data.Remove(0, data.LastIndexOf(":"));
                            secondary_local_ip_address = secondary_local_ip_address.Replace(":", " ");
                            secondary_local_ip_address = secondary_local_ip_address.Trim();
                        }

                        if (temp[i].Contains("node"))
                        {
                            data = temp[i].Trim().ToString();
                            node = data.Remove(0, data.LastIndexOf(":"));
                            node = node.Replace(":", " ");
                            node = node.Trim();
                        }

                        if (temp[i].Contains("remote-as-name"))
                        {
                            data = temp[i].Trim().ToString();
                            remote_name = data.Remove(0, data.LastIndexOf(":"));
                            remote_name = remote_name.Replace(":", " ");
                            remote_name = remote_name.Trim();
                        }

                        if (temp[i].Contains("primary-remote-ip-addr"))
                        {
                            data = temp[i].Trim().ToString();
                            primary_remote_ip_address = data.Remove(0, data.LastIndexOf(":"));
                            primary_remote_ip_address = primary_remote_ip_address.Replace(":", " ");
                            primary_remote_ip_address = primary_remote_ip_address.Trim();
                        }

                        if (temp[i].Contains("secondary-remote-ip-addr"))
                        {
                            data = temp[i].Trim().ToString();
                            secondary_remote_ip_address = data.Remove(0, data.LastIndexOf(":"));
                            secondary_remote_ip_address = secondary_remote_ip_address.Replace(":", " ");
                            secondary_remote_ip_address = secondary_remote_ip_address.Trim();
                        }

                        if (temp[i].Contains("sctp-profile"))
                        {
                            data = temp[i].Trim().ToString();
                            sctp_profile = data.Remove(0, data.LastIndexOf(":"));
                            sctp_profile = sctp_profile.Replace(":", " ");
                            sctp_profile = sctp_profile.Trim();
                        }

                        if (temp[i].Contains("status"))
                        {
                            data = temp[i].Trim().ToString();
                            status = data.Remove(0, data.LastIndexOf(":"));
                            status = status.Replace(":", " ");
                            status = status.Trim();
                        }

                        if (association_id != "" && primary_local_ip_address != "" && secondary_local_ip_address != "" && node != "" && remote_name != "" && primary_remote_ip_address != "" && secondary_remote_ip_address != "" && sctp_profile != "" && status != "" && _NEType == "MGW_ATCA")
                        {
                            query = "select NODE,LINK_SET,IP_LINK,ASSOCIATION_SET_ID,ASSOCIATION_SET,ID,UNIT,SOURCE_IP_1,SOURCE_IP_2,DESTINATION_IP_1,DESTINATION_IP_2,CGR_NAME,CGR_NUMBER,NA,SPC,DESTINATION_IP,DESTINATION_NAME,CIRCLE,NODE_TYPE,SCTP_USER,ROLE,STATUS from association_info_" + _CircleName + " where circle = '" + _CircleName + "' and NODE_TYPE = ' " + NE_Type + " '";
                            SQLQuery(ref conn, ref dt, query);

                            if (dt.Rows.Count > 0)
                            {
                                query1 = "";

                                query1 = " update association_info_" + _CircleName + "  set   `ASSOCIATION_SET_ID`='" + association_id + "',";
                                query1 += " `SOURCE_IP_1`='" + primary_local_ip_address + "', `SOURCE_IP_2`='" + secondary_local_ip_address + "', `DESTINATION_IP_1`='" + primary_remote_ip_address + "' ";
                                query1 += " `DESTINATION_IP_2`='" + secondary_remote_ip_address + "', `NODE_TYPE`='" + node + "', `DESTINATION_NAME`='" + remote_name + "', `SCTP_USER`='" + sctp_profile + "', `STATUS`='" + status + "' ";
                                query1 += " where NODE = '" + _ne_name + "' AND ";
                                query1 += " circle = '" + _CircleName + "' ";

                                ExecuteSQLQuery(ref conn, query1);


                                primary_local_ip_address = "";
                                secondary_local_ip_address = "";
                                primary_remote_ip_address = "";
                                secondary_remote_ip_address = "";
                            }
                            else
                            {
                                query1 = "insert into association_info_" + _CircleName + " (`NODE`,`ASSOCIATION_SET_ID`,`SOURCE_IP_1`,`SOURCE_IP_2`,`DESTINATION_IP_1`,`DESTINATION_IP_2`,`NODE_TYPE`,`DESTINATION_NAME`,`SCTP_USER`,`STATUS`) values ('" + _ne_name + "','" + association_id + "','" + primary_local_ip_address + "','" + primary_remote_ip_address + "','" + secondary_local_ip_address + "','" + secondary_remote_ip_address + "','" + NE_Type + "','" + remote_name + "','" + sctp_profile + "','" + status + "')";
                                ExecuteSQLQuery(ref conn, query1);


                                primary_local_ip_address = "";
                                secondary_local_ip_address = "";
                                primary_remote_ip_address = "";
                                secondary_remote_ip_address = "";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_LICENCECAPACITY_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_LICENCECAPACITY()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZW7I_LICENCECAPACITY(string output, string CircleName, string NE_Type) //ADDED by AJAY
        {
            DataTable dt = new DataTable();
            string Newquery = string.Empty;
            string VENDOR = string.Empty;
            string CIRCLE = string.Empty;
            string NE_NAME = string.Empty;
            string NODE_TYPE = string.Empty;
            string FEATURE_CODE = string.Empty;
            string FEATURE_NAME = string.Empty;
            string FEATURE_STATE = string.Empty;
            string FEATURE_CAPACITY = string.Empty;
            string VMGW_SECONDARY_IP = string.Empty;
            string query = string.Empty;
            string query1 = string.Empty;
            int flag = 1;
            string data = string.Empty;
            string[] temp;
            string cmnd_output = output.Trim();

            try
            {
                temp = Regex.Split(cmnd_output, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (temp[i].Contains("FEATURE CODE"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_CODE = data.Remove(0, data.LastIndexOf("."));
                            FEATURE_CODE = FEATURE_CODE.Replace(".", " ");
                            FEATURE_CODE = FEATURE_CODE.Trim();

                        }

                        if (temp[i].Contains("FEATURE NAME"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_NAME = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_NAME = FEATURE_NAME.Replace("..", " ");
                            FEATURE_NAME = FEATURE_NAME.Trim();
                        }

                        if (temp[i].Contains("FEATURE STATE"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_STATE = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_STATE = FEATURE_STATE.Replace("..", " ");
                            FEATURE_STATE = FEATURE_STATE.Trim();
                        }

                        if (temp[i].Contains("FEATURE CAPACITY"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_CAPACITY = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_CAPACITY = FEATURE_CAPACITY.Replace("..", " ");
                            FEATURE_CAPACITY = FEATURE_CAPACITY.Trim();
                        }


                        if (FEATURE_CODE != "" && FEATURE_NAME != "" && FEATURE_STATE != "" && FEATURE_CAPACITY != "" && _NEType != "")
                        {
                            //if (flag == 1)
                            //{
                            query = "select VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_STATE,FEATURE_CAPACITY from licence_capacity_warning_" + CircleName + " where circle = '" + CircleName + "' and NE_NAME = '" + _ne_name + "' and FEATURE_CODE = '" + FEATURE_CODE + "' AND NODE_TYPE = '" + _NEType + "' ";
                            SQLQuery(ref conn, ref dt, query);

                            //for (int p = 0; p <= dt.Rows.Count - 1; p++)
                            //{
                            if (dt.Rows.Count > 0)
                            {
                                query1 = "";

                                query1 = " update licence_capacity_warning_" + CircleName + "  set   `FEATURE_NAME`='" + FEATURE_NAME + "',";
                                query1 += " `FEATURE_STATE`='" + FEATURE_STATE + "', `FEATURE_CAPACITY`='" + FEATURE_CAPACITY + "' where NE_NAME = '" + _ne_name + "' FEATURE_CODE = '" + FEATURE_CODE + "'  AND ";
                                query1 += " circle = '" + _CircleName + "' ";

                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_STATE = "";
                                FEATURE_CAPACITY = "";
                            }
                            //}
                            // }
                            else
                            {
                                query1 = "insert into licence_capacity_warning_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_STATE,FEATURE_CAPACITY) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "', '" + _ne_name.Trim() + "', '" + _NEType.Trim() + "','" + FEATURE_CODE.Trim() + "','" + FEATURE_NAME.Trim() + "','" + FEATURE_STATE.Trim() + "','" + FEATURE_CAPACITY.Trim() + "') ";
                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_STATE = "";
                                FEATURE_CAPACITY = "";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_LICENCECAPACITY_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_LICENCECAPACITY()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZW7I_LICENCEEXCEEDED(string output, string CircleName, string NE_Type) //ADDED by AJAY
        {
            DataTable dt = new DataTable();
            string Newquery = string.Empty;
            string VENDOR = string.Empty;
            string CIRCLE = string.Empty;
            string NE_NAME = string.Empty;
            string NODE_TYPE = string.Empty;
            string FEATURE_CODE = string.Empty;
            string FEATURE_NAME = string.Empty;
            string FEATURE_STATE = string.Empty;
            string FEATURE_CAPACITY = string.Empty;
            string VMGW_SECONDARY_IP = string.Empty;
            string query = string.Empty;
            string query1 = string.Empty;
            int flag = 1;
            string data = string.Empty;
            string[] temp;
            string cmnd_output = output.Trim();

            try
            {
                temp = Regex.Split(cmnd_output, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (temp[i].Contains("FEATURE CODE"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_CODE = data.Remove(0, data.LastIndexOf("."));
                            FEATURE_CODE = FEATURE_CODE.Replace(".", " ");
                            FEATURE_CODE = FEATURE_CODE.Trim();

                        }

                        if (temp[i].Contains("FEATURE NAME"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_NAME = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_NAME = FEATURE_NAME.Replace("..", " ");
                            FEATURE_NAME = FEATURE_NAME.Trim();
                        }

                        if (temp[i].Contains("FEATURE STATE"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_STATE = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_STATE = FEATURE_STATE.Replace("..", " ");
                            FEATURE_STATE = FEATURE_STATE.Trim();
                        }

                        if (temp[i].Contains("FEATURE CAPACITY"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_CAPACITY = data.Remove(0, data.LastIndexOf(".."));
                            FEATURE_CAPACITY = FEATURE_CAPACITY.Replace("..", " ");
                            FEATURE_CAPACITY = FEATURE_CAPACITY.Trim();
                        }


                        if (FEATURE_CODE != "" && FEATURE_NAME != "" && FEATURE_STATE != "" && FEATURE_CAPACITY != "" && _NEType != "")
                        {
                            //if (flag == 1)
                            //{
                            query = "select VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_STATE,FEATURE_CAPACITY from licence_capacity_exceeded_" + CircleName + " where circle = '" + CircleName + "' and NE_NAME = '" + _ne_name + "' and FEATURE_CODE = '" + FEATURE_CODE + "' AND NODE_TYPE = '" + _NEType + "' ";
                            SQLQuery(ref conn, ref dt, query);

                            //for (int p = 0; p <= dt.Rows.Count - 1; p++)
                            //{
                            if (dt.Rows.Count > 0)
                            {
                                query1 = "";

                                query1 = " update licence_capacity_exceeded_" + CircleName + "  set   `FEATURE_NAME`='" + FEATURE_NAME + "',";
                                query1 += " `FEATURE_STATE`='" + FEATURE_STATE + "', `FEATURE_CAPACITY`='" + FEATURE_CAPACITY + "' where NE_NAME = '" + _ne_name + "' `FEATURE_CODE` = '" + FEATURE_CODE + "'  AND ";
                                query1 += " circle = '" + _CircleName + "' ";

                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_STATE = "";
                                FEATURE_CAPACITY = "";
                            }
                            //}
                            // }
                            else
                            {
                                query1 = "insert into licence_capacity_exceeded_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_STATE,FEATURE_CAPACITY) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "', '" + _ne_name.Trim() + "', '" + _NEType.Trim() + "','" + FEATURE_CODE.Trim() + "','" + FEATURE_NAME.Trim() + "','" + FEATURE_STATE.Trim() + "','" + FEATURE_CAPACITY.Trim() + "') ";
                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_STATE = "";
                                FEATURE_CAPACITY = "";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_LICENCEEXCEEDED_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_LICENCEEXCEEDED()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_show_license_feature_mgmt_all(string output, string CircleName, string NE_Type) //ADDED by AJAY
        {
            DataTable dt = new DataTable();
            string Newquery = string.Empty;
            string VENDOR = string.Empty;
            string CIRCLE = string.Empty;
            string NE_NAME = string.Empty;
            string NODE_TYPE = string.Empty;
            string FEATURE_CODE = string.Empty;
            string FEATURE_NAME = string.Empty;
            string FEATURE_DESCRIPTION = string.Empty;
            string VMGW_SECONDARY_IP = string.Empty;
            string ATCA_FLAG = string.Empty;
            string query = string.Empty;
            string query1 = string.Empty;
            int flag = 1;
            string data = string.Empty;
            string[] temp;
            string cmnd_output = output.Trim();

            try
            {
                temp = Regex.Split(cmnd_output, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (temp[i].Contains("Feature Name"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_NAME = data.Remove(0, data.LastIndexOf(":"));
                            FEATURE_NAME = FEATURE_NAME.Replace(":", " ");
                            FEATURE_NAME = FEATURE_NAME.Trim();

                        }

                        if (temp[i].Contains("Feature Code"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_CODE = data.Remove(0, data.LastIndexOf(":"));
                            FEATURE_CODE = FEATURE_CODE.Replace(":", " ");
                            FEATURE_CODE = FEATURE_CODE.Trim();
                        }

                        if (temp[i].Contains("Feature Description"))
                        {
                            data = temp[i].Trim().ToString();
                            FEATURE_DESCRIPTION = data.Remove(0, data.LastIndexOf(":"));
                            FEATURE_DESCRIPTION = FEATURE_DESCRIPTION.Replace(":", " ");
                            FEATURE_DESCRIPTION = FEATURE_DESCRIPTION.Trim();
                        }


                        if (FEATURE_CODE != "" && FEATURE_NAME != "" && FEATURE_DESCRIPTION != "" && _NEType == "MGW_ATCA")
                        {
                            //if (flag == 1)
                            //{
                            query = "select VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_STATE,FEATURE_CAPACITY,ATCA_FLAG,FEATURE_DESCRIPTION from licence_capacity_warning_" + CircleName + " where circle = '" + CircleName + "' and NE_NAME = '" + _ne_name + "' and FEATURE_CODE = '" + FEATURE_CODE + "' AND NODE_TYPE = '" + _NEType + "' ";
                            SQLQuery(ref conn, ref dt, query);

                            //for (int p = 0; p <= dt.Rows.Count - 1; p++)
                            //{
                            if (dt.Rows.Count > 0)
                            {
                                query1 = "";

                                query1 = " update licence_capacity_warning_" + CircleName + "  set   `FEATURE_NAME`='" + FEATURE_NAME + "',";
                                query1 += " `FEATURE_DESCRIPTION`='" + FEATURE_DESCRIPTION + "' where NE_NAME = '" + _ne_name + "' FEATURE_CODE = '" + FEATURE_CODE + "'  AND ";
                                query1 += " circle = '" + _CircleName + "' ATCA_FLAG = '" + _NEType + "' ";

                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_DESCRIPTION = "";
                            }
                            //}
                            // }
                            else
                            {
                                query1 = "insert into licence_capacity_warning_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE,FEATURE_CODE,FEATURE_NAME,FEATURE_DESCRIPTION,ATCA_FLAG) values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "', '" + _ne_name.Trim() + "', '" + _NEType.Trim() + "','" + FEATURE_CODE.Trim() + "','" + FEATURE_NAME.Trim() + "','" + FEATURE_DESCRIPTION.Trim() + "','" + 1 + "') ";
                                ExecuteSQLQuery(ref conn, query1);


                                FEATURE_CODE = "";
                                FEATURE_NAME = "";
                                FEATURE_DESCRIPTION = "";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7I()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }
        #endregion

        // for msc extra et
        #region[for msc extra et]
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        /// <param name="net"></param>
        public void ParseData_ZNSI_MSC_extra_et(string output, string CircleName, string NE_Type, string net)
        {
            string temp_data = string.Empty;
            string slc = string.Empty;
            string[] value;
            string[] temp;
            string query = string.Empty;
            string cgr_spc = string.Empty;
            string cgr_net = string.Empty;
            string link = string.Empty;

            try
            {
                string[] cmnd_output = Regex.Split(output.Trim(), "COMMAND EXECUTED");
                string data = cmnd_output[0].Trim();
                cmnd_output = Regex.Split(data, "SLC");

                if (cmnd_output.Length == 1)
                {
                    data = cmnd_output[0].Trim().ToString();
                }
                else
                {
                    data = cmnd_output[1].Trim().ToString();
                }

                temp = Regex.Split(data, "\r\n");


                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (!temp[i].Trim().Contains("----------") && temp[i].Trim().ToString() != "")
                        {
                            while (i < temp.Length && temp[i] != "COMMAND EXECUTED" && temp[i].Trim() != "")
                            {
                                if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                {
                                    temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                    value = Regex.Split(temp_data.ToString().Trim(), "~");

                                    if (value.Length >= 2)
                                    {
                                        slc = value[value.Length - 1].ToString();
                                        link = value[value.Length - 2].ToString();
                                    }

                                    if (value.Length > 3)
                                    {
                                        cgr_net = value[0].Trim();
                                        cgr_spc = value[1].Trim();
                                    }





                                    query = "Update msc_" + CircleName + " set SLC = '" + slc.Trim() + "', CGR_NET = '" + net + "',cgr_spc='" + cgr_spc + "'  where link = '" + link.Trim() + "' AND NE_NAME = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + CircleName.Trim() + "' and EXTRA_ET_FLAG='1' ";



                                }


                                ExecuteSQLQuery(ref conn, query);
                                i = i + 1;


                            }

                        }



                    }

                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZNSI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNSI_MSC_extra_et_SUBwithConitnue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_ZNSI_MSC_extra_et()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circleName"></param>
        public void update_msc_extra_et(string circleName)
        {
            string query = string.Empty;
            DataTable dt = new DataTable();
            string MSS_MCC = string.Empty;
            string MSS_mnc = string.Empty;
            string MSS = string.Empty;
            string element_ip = string.Empty;
            DataTable dtnew = new DataTable();

            try
            {
                SQLQuery(ref conn, ref dtnew, "select distinct ne_name FROM msc_" + circleName + " where  circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and node_type = 'MSC'");
                for (int i = 0; i < dtnew.Rows.Count; i++)
                {
                    string node_name = dtnew.Rows[i]["ne_name"].ToString();


                    query = "SELECT distinct `MCC` FROM msc_" + circleName + " where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);
                    MSS_MCC = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update msc_" + circleName + " set `MCC` = '" + MSS_MCC + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();

                    query = "SELECT distinct `MNC` FROM msc_" + circleName + " where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);
                    MSS_mnc = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update msc_" + circleName + " set `MNC` = '" + MSS_mnc + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();

                    query = "SELECT distinct `MSS` FROM msc_" + circleName + " where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);
                    MSS = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update msc_" + circleName + " set `MSS` = '" + MSS + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();

                    query = "SELECT distinct `element_ip` FROM msc_" + circleName + " where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);
                    element_ip = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update msc_" + circleName + " set `element_ip` = '" + element_ip + "' where ne_name ='" + node_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='1'");
                    dt.Clear();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("update_msc_extra_et()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }
        #endregion

        //ASSOCIATION parsing functions
        #region[ASSOCIATION parsing functions]
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="Circle"></param>
        /// <param name="net"></param>
        public void association_ZNSI_NA1(string command_output, string Circle, string net)
        {
            string node = string.Empty;
            string[] value;
            string[] value1;
            string LINK_SET = string.Empty;
            string IP_LINK = string.Empty;
            string ASSOCIATION_SET_ID = string.Empty;
            string ASSOCIATION_SET = string.Empty;
            string SPC = string.Empty;
            string NA = string.Empty;
            string[] temporary;
            string output = string.Empty;
            string QUERY = string.Empty;
            //string searcCharacter = "\\bIP\\b";

            try
            {
                string[] temp = Regex.Split(command_output, "\r\n");
                if (command_output.Contains("LS    IP"))
                    output = command_output.Remove(0, command_output.Trim().IndexOf("LS    IP"));
                value = Regex.Split(output, "\r\n");

                for (int j = 0; j < value.Length; j++)
                {
                    try
                    {

                        if (value[j].ToString().Trim().Contains(net))
                        {
                            while (!value[j].ToString().Contains("COMMAND EXECUTED"))
                            {
                                if (value[j].ToString().Contains(net))
                                {
                                    string temp_data = Regex.Replace(value[j].ToString().Trim(), " {2,}", "~");
                                    value1 = Regex.Split(temp_data, "~");
                                    if (value1.Length > 3)
                                    {
                                        NA = value1[0].ToString().Trim();
                                        SPC = value1[1].ToString().Trim();
                                        LINK_SET = value1[2].ToString().Trim();
                                        IP_LINK = value1[5].ToString().Trim();
                                        temporary = Regex.Split(value1[3].ToString().Trim(), " ");
                                        ASSOCIATION_SET_ID = temporary[0];
                                        ASSOCIATION_SET = temporary[1];
                                    }
                                    if (NA != "" && SPC != "" && LINK_SET != "" && IP_LINK != "" && ASSOCIATION_SET != "" && ASSOCIATION_SET_ID != "")
                                    {
                                        QUERY = "insert into association_info_" + Circle + " (`NODE`,`LINK_SET`,`IP_LINK`,`ASSOCIATION_SET_ID`,`ASSOCIATION_SET`,`NA`,`SPC`,`circle`,`NODE_TYPE`) values ('" + _ne_name + "','" + LINK_SET + "','" + IP_LINK + "','" + ASSOCIATION_SET_ID + "', '" + ASSOCIATION_SET + "','" + NA + "','" + SPC + "','" + Circle + "', '" + _NEType + "')";
                                        ExecuteSQLQuery(ref conn, QUERY);
                                        NA = "";
                                        SPC = "";
                                        LINK_SET = "";
                                        IP_LINK = "";
                                        ASSOCIATION_SET = "";
                                        ASSOCIATION_SET_ID = "";
                                    }
                                }
                                j = j + 1;
                            }
                            if (value[j].ToString().Contains("COMMAND EXECUTED"))
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function association_ZNSI_NA1 ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("association_ZNSI_NA1_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("association_ZNSI_NA1()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }
        //public void association_ZNSI_NA1(string command_output, string Circle, string net)
        //{
        //    string node = string.Empty;
        //    string[] value;
        //    string[] value1;
        //    string LINK_SET = string.Empty;
        //    string IP_LINK = string.Empty;
        //    string ASSOCIATION_SET_ID = string.Empty;
        //    string ASSOCIATION_SET = string.Empty;
        //    string SPC = string.Empty;
        //    string NA = string.Empty;
        //    string[] temporary;
        //    string output = string.Empty;
        //    string QUERY = string.Empty;
        //    //string searcCharacter = "\\bIP\\b";

        //    try
        //    {
        //        string[] temp = Regex.Split(command_output, "\r\n");
        //        if (command_output.Contains("LS    IP"))
        //            output = command_output.Remove(0, command_output.Trim().IndexOf("LS    IP"));
        //        value = Regex.Split(output, "\r\n");

        //        for (int j = 0; j < value.Length; j++)
        //        {
        //            try
        //            {

        //                if (value[j].ToString().Trim().Contains(net))
        //                {
        //                    while (!value[j].ToString().Contains("COMMAND EXECUTED"))
        //                    {
        //                        if (value[j].ToString().Contains(net))
        //                        {
        //                            string temp_data = Regex.Replace(value[j].ToString().Trim(), " {2,}", "~");
        //                            value1 = Regex.Split(temp_data, "~");
        //                            NA = value1[0].ToString().Trim();
        //                            SPC = value1[1].ToString().Trim();
        //                            LINK_SET = value1[2].ToString().Trim();
        //                            IP_LINK = value1[5].ToString().Trim();
        //                            temporary = Regex.Split(value1[3].ToString().Trim(), " ");
        //                            ASSOCIATION_SET_ID = temporary[0];
        //                            ASSOCIATION_SET = temporary[1];
        //                            if (NA != "" && SPC != "" && LINK_SET != "" && IP_LINK != "" && ASSOCIATION_SET != "" && ASSOCIATION_SET_ID != "")
        //                            {
        //                                QUERY = "insert into association_info_" + Circle + " (`NODE`,`LINK_SET`,`IP_LINK`,`ASSOCIATION_SET_ID`,`ASSOCIATION_SET`,`NA`,`SPC`,`circle`,`NODE_TYPE`) values ('" + _ne_name + "','" + LINK_SET + "','" + IP_LINK + "','" + ASSOCIATION_SET_ID + "', '" + ASSOCIATION_SET + "','" + NA + "','" + SPC + "','" + Circle + "', '" + _NEType + "')";
        //                                ExecuteSQLQuery(ref conn, QUERY);
        //                                NA = "";
        //                                SPC = "";
        //                                LINK_SET = "";
        //                                IP_LINK = "";
        //                                ASSOCIATION_SET = "";
        //                                ASSOCIATION_SET_ID = "";
        //                            }
        //                        }
        //                        j = j + 1;
        //                    }
        //                    if (value[j].ToString().Contains("COMMAND EXECUTED"))
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
        //                //if (!Directory.Exists(parsing_error_path))
        //                //{
        //                //    Directory.CreateDirectory(parsing_error_path);
        //                //}

        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function association_ZNSI_NA1 ");
        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        //                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //                oExceptionLog.WriteExceptionErrorToFile("association_ZNSI_NA1_SUBwithContinue()", ErrorMsg, "", ref FileError);

        //                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //                ExecuteSQLQuery(ref conn, ErrorQuery);
        //                continue;

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //        oExceptionLog.WriteExceptionErrorToFile("association_ZNSI_NA1()", ErrorMsg, "", ref FileError);

        //        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //        ExecuteSQLQuery(ref conn, ErrorQuery);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="Circle"></param>
        public void association_ZOYI(string command_output, string Circle) //Modified by AJAY 05thDec2018
        {
            string[] value;
            string temp_value = string.Empty;
            string[] data_temp;
            string temp_data = string.Empty;
            string ID = string.Empty;
            string assc_id = string.Empty;
            string UNIT = string.Empty;
            string value_of_output = string.Empty;
            string output_value = string.Empty;
            string[] outputValue;
            string[] temp;
            int flag;
            int size;
            string query = string.Empty;
            string query1 = string.Empty;
            DataTable dt = new DataTable();
            string SOURCE_IP1 = string.Empty;
            string SOURCE_IP2 = string.Empty;
            string DESTINATION_IP1 = string.Empty;
            string DESTINATION_IP2 = string.Empty;
            //////Added new parameters by AJAY
            string SCTP_USER = string.Empty;
            string ROLE = string.Empty;
            string SOURCE_PORT = string.Empty;
            string DESTINATION_PORT = string.Empty;
            string PARAMETER_SET_NAME = string.Empty;
            string STATE = string.Empty;
            string DATA_STREAM_COUNT = string.Empty;
            string assc_name = string.Empty;
            try
            {
                string[] command_data = Regex.Split(command_output, "COMMAND EXECUTED");
                command_output = command_data[0].Trim();
                command_output = command_output + Environment.NewLine + "COMMAND EXECUTED";
                value = Regex.Split(command_output, "ASSOCIATION SET NAME");
                //value = Regex.Split(command_output, "SCTP USER"); // Added by AJAY
                //value = Regex.Split(command_output, "ROLE"); // Added by AJAY

                for (int j = 1; j <= value.Length - 1; j++)
                {
                    try
                    {

                        data_temp = Regex.Split(value[j].ToString().Trim(), "\r\n");
                        for (int k = 1; k <= data_temp.Length - 1; k++)
                        {
                            if (!data_temp[k].Trim().Contains("-----------") && data_temp[k] != "")
                            {
                                temp_data = Regex.Replace(data_temp[k].Trim(), " {2,}", "~");
                                temp = Regex.Split(temp_data, "~");
                                assc_name = temp[0].Trim();
                                assc_id = temp[1];
                                SCTP_USER = temp[2];
                                ROLE = temp[3];
                                if (assc_id != "")
                                {
                                    k = data_temp.Length;
                                }
                            }
                        }
                        output_value = value[j].ToString().Trim();
                        outputValue = Regex.Split(output_value, "STATE");
                        for (int y = 1; y <= outputValue.Length - 1; y++)
                        {

                            data_temp = Regex.Split(outputValue[y].Trim(), "\r\n");
                            size = data_temp.Length - 1;
                            flag = 1;
                            for (int m = 0; m <= size; m++)
                            {
                                if (!data_temp[m].ToString().Trim().Contains("--------------") && data_temp[m].Trim() != "")
                                {
                                    temp_data = Regex.Replace(data_temp[m].ToString().Trim(), " {1,}", "~");
                                    temp = Regex.Split(temp_data, "~");
                                    UNIT = temp[1];
                                    ID = temp[0].Trim();
                                    PARAMETER_SET_NAME = temp[3].Trim(); //Added by AJAY
                                    STATE = temp[4].Trim(); //Added by AJAY
                                    m = m + 1;
                                    if (data_temp[m].Trim() == "")
                                    {
                                        while (string.IsNullOrWhiteSpace(data_temp[m]))
                                        {
                                            m = m + 1;
                                        }
                                    }
                                    if (data_temp[m].ToString() != " ")
                                    {
                                        while (m != size + 1)
                                        {
                                            if (data_temp[m].Trim().Contains("SOURCE ADDRESS 1"))
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                SOURCE_IP1 = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("SOURCE ADDRESS 2"))
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                SOURCE_IP2 = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("PRIMARY DEST"))
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                temp = Regex.Split(temp_data, "/");
                                                temp_data = temp[0].Trim();
                                                DESTINATION_IP1 = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("SECONDARY DEST"))
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                temp = Regex.Split(temp_data, "/");
                                                temp_data = temp[0].Trim();
                                                DESTINATION_IP2 = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("SOURCE PORT")) // Added by AJAY 06DEC2018
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                SOURCE_PORT = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("DESTINATION PORT")) // Added by AJAY 06DEC2018
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                DESTINATION_PORT = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("PARAMETER SET NAME")) // Added by AJAY 06DEC2018
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                PARAMETER_SET_NAME = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("STATE")) // Added by AJAY 06DEC2018
                                            {
                                                temp = Regex.Split(data_temp[m], "--------------------");
                                                temp_data = temp[1].Trim();
                                                STATE = temp_data;
                                            }
                                            if (data_temp[m].Trim().Contains("DATA STREAM COUNT")) // Added by AJAY 06DEC2018
                                            {
                                                temp = Regex.Split(data_temp[m], ":");
                                                temp_data = temp[1].Trim();
                                                DATA_STREAM_COUNT = temp_data;
                                            }

                                            if (DESTINATION_IP1 != "" && DESTINATION_IP2 != "" && SOURCE_IP1 != "" && SOURCE_IP2 != "" && SOURCE_PORT != "" && DESTINATION_PORT != "" && PARAMETER_SET_NAME != "" && STATE != "" && DATA_STREAM_COUNT != "")
                                            {
                                                #region old Code
                                                //if (flag == 1)
                                                //{
                                                //    query = "select NODE, LINK_SET, IP_LINK, ASSOCIATION_SET_ID, ASSOCIATION_SET,NA,SPC,SCTP_USER,ROLE,STATUS,PARAMETER_SET_NAME,SOURCE_PORT,DESTINATION_PORT,STATE,DATA_STREAM_COUNT from association_info_" + Circle + " where circle = '" + Circle + "' and node = '" + _ne_name + "' AND ASSOCIATION_SET_ID = '" + assc_id + "'  AND SOURCE_IP_1 IS NULL";
                                                //    SQLQuery(ref conn, ref dt, query);

                                                //    for (int p = 0; p <= dt.Rows.Count - 1; p++)
                                                //    {
                                                //        query1 = "insert into association_info_" + Circle + " (`NODE`, `LINK_SET`, `IP_LINK`, `ASSOCIATION_SET_ID`, `ASSOCIATION_SET`,`NA`,`SPC`, `ID`, `UNIT`, `SOURCE_IP_1`, `SOURCE_IP_2`, `DESTINATION_IP_1`, `DESTINATION_IP_2`,`CIRCLE`,`NODE_TYPE`,`SCTP_USER`,`ROLE`,`STATE`,`PARAMETER_SET_NAME`,`SOURCE_PORT`,`DESTINATION_PORT`,`DATA_STREAM_COUNT`) values ('" + dt.Rows[p][0].ToString() + "','" + dt.Rows[p][1].ToString() + "','" + dt.Rows[p][2].ToString() + "','" + dt.Rows[p][3].ToString() + "','" + dt.Rows[p][4].ToString() + "','" + dt.Rows[p][5].ToString() + "','" + dt.Rows[p][6].ToString() + "','" + ID + "', '" + UNIT + "', '" + SOURCE_IP1 + "', '" + SOURCE_IP2 + "', '" + DESTINATION_IP1 + "', '" + DESTINATION_IP2 + "', '" + Circle + "', '" + _NEType + "', '" + SCTP_USER + "', '" + ROLE + "', '" + STATE + "', '" + PARAMETER_SET_NAME + "', '" + SOURCE_PORT + "', '" + DESTINATION_PORT + "', '" + DATA_STREAM_COUNT + "' )";
                                                //        ExecuteSQLQuery(ref conn, query1);
                                                //    }
                                                //    dt.Clear();
                                                //}
                                                #endregion

                                                #region updated Code
                                                if (flag == 1)
                                                {
                                                    query = "select NODE, LINK_SET, IP_LINK, ASSOCIATION_SET_ID, ASSOCIATION_SET,NA,SPC,SCTP_USER,ROLE,STATUS,PARAMETER_SET_NAME,SOURCE_PORT,DESTINATION_PORT,STATE,DATA_STREAM_COUNT from association_info_" + Circle + " where circle = '" + Circle + "' and node = '" + _ne_name + "' AND ASSOCIATION_SET_ID = '" + assc_id + "'  AND SOURCE_IP_1 IS NULL";
                                                    SQLQuery(ref conn, ref dt, query);
                                                    if (dt.Rows.Count >= 1)
                                                    {
                                                        for (int p = 0; p <= dt.Rows.Count - 1; p++)
                                                        {
                                                            query1 = "insert into association_info_" + Circle + " (`NODE`, `LINK_SET`, `IP_LINK`, `ASSOCIATION_SET_ID`, `ASSOCIATION_SET`,`NA`,`SPC`, `ID`, `UNIT`, `SOURCE_IP_1`, `SOURCE_IP_2`, `DESTINATION_IP_1`, `DESTINATION_IP_2`,`CIRCLE`,`NODE_TYPE`,`SCTP_USER`,`ROLE`,`STATE`,`PARAMETER_SET_NAME`,`SOURCE_PORT`,`DESTINATION_PORT`,`DATA_STREAM_COUNT`) values ('" + dt.Rows[p][0].ToString() + "','" + dt.Rows[p][1].ToString() + "','" + dt.Rows[p][2].ToString() + "','" + dt.Rows[p][3].ToString() + "','" + dt.Rows[p][4].ToString() + "','" + dt.Rows[p][5].ToString() + "','" + dt.Rows[p][6].ToString() + "','" + ID + "', '" + UNIT + "', '" + SOURCE_IP1 + "', '" + SOURCE_IP2 + "', '" + DESTINATION_IP1 + "', '" + DESTINATION_IP2 + "', '" + Circle + "', '" + _NEType + "', '" + SCTP_USER + "', '" + ROLE + "', '" + STATE + "', '" + PARAMETER_SET_NAME + "', '" + SOURCE_PORT + "', '" + DESTINATION_PORT + "', '" + DATA_STREAM_COUNT + "' )";
                                                            ExecuteSQLQuery(ref conn, query1);
                                                        }
                                                        dt.Clear();
                                                    }
                                                    else if (dt.Rows.Count <= 0)
                                                    {

                                                        query1 = "insert into association_info_" + Circle + " (`NODE`,ASSOCIATION_SET_ID, `ASSOCIATION_SET`, `ID`, `UNIT`, `SOURCE_IP_1`, `SOURCE_IP_2`, `DESTINATION_IP_1`, `DESTINATION_IP_2`,`CIRCLE`,`NODE_TYPE`,`SCTP_USER`,`ROLE`,`STATE`,`PARAMETER_SET_NAME`,`SOURCE_PORT`,`DESTINATION_PORT`,`DATA_STREAM_COUNT`) values ('" + _ne_name.ToString() + "','" + assc_id + "', '" + assc_name + "', '" + ID + "', '" + UNIT + "', '" + SOURCE_IP1 + "', '" + SOURCE_IP2 + "', '" + DESTINATION_IP1 + "', '" + DESTINATION_IP2 + "', '" + Circle + "', '" + _NEType + "', '" + SCTP_USER + "', '" + ROLE + "', '" + STATE + "', '" + PARAMETER_SET_NAME + "', '" + SOURCE_PORT + "', '" + DESTINATION_PORT + "', '" + DATA_STREAM_COUNT + "' )";
                                                        ExecuteSQLQuery(ref conn, query1);
                                                    }
                                                }
                                                #endregion

                                                flag = 0;
                                                DESTINATION_IP1 = "";
                                                DESTINATION_IP2 = "";
                                                SOURCE_IP1 = "";
                                                SOURCE_IP2 = "";
                                                PARAMETER_SET_NAME = "";
                                                SOURCE_PORT = "";
                                                DESTINATION_PORT = "";
                                                STATE = "";
                                                DATA_STREAM_COUNT = "";
                                            }

                                            m = m + 1;
                                        }
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function association_ZOYI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("association_ZOYI_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("association_ZOYI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        //public void association_ZOYI(string command_output, string Circle)
        //{
        //    string[] value;
        //    string temp_value = string.Empty;
        //    string[] data_temp;
        //    string temp_data = string.Empty;
        //    string ID = string.Empty;
        //    string assc_id = string.Empty;
        //    string UNIT = string.Empty;
        //    string value_of_output = string.Empty;
        //    string output_value = string.Empty;
        //    string[] outputValue;
        //    string[] temp;
        //    int flag;
        //    int size;
        //    string query = string.Empty;
        //    string query1 = string.Empty;
        //    DataTable dt = new DataTable();
        //    string SOURCE_IP1 = string.Empty;
        //    string SOURCE_IP2 = string.Empty;
        //    string DESTINATION_IP1 = string.Empty;
        //    string DESTINATION_IP2 = string.Empty;
        //    try
        //    {
        //        string[] command_data = Regex.Split(command_output, "COMMAND EXECUTED");
        //        command_output = command_data[0].Trim();
        //        command_output = command_output + Environment.NewLine + "COMMAND EXECUTED";
        //        value = Regex.Split(command_output, "ASSOCIATION SET NAME");


        //        for (int j = 1; j <= value.Length - 1; j++)
        //        {
        //            try
        //            {

        //                data_temp = Regex.Split(value[j].ToString().Trim(), "\r\n");
        //                for (int k = 1; k <= data_temp.Length - 1; k++)
        //                {
        //                    if (!data_temp[k].Trim().Contains("-----------") && data_temp[k] != "")
        //                    {
        //                        temp_data = Regex.Replace(data_temp[k].Trim(), " {2,}", "~");
        //                        temp = Regex.Split(temp_data, "~");
        //                        assc_id = temp[1];
        //                        if (assc_id != "")
        //                        {
        //                            k = data_temp.Length;
        //                        }
        //                    }
        //                }
        //                output_value = value[j].ToString().Trim();
        //                outputValue = Regex.Split(output_value, "STATE");
        //                for (int y = 1; y <= outputValue.Length - 1; y++)
        //                {

        //                    data_temp = Regex.Split(outputValue[y].Trim(), "\r\n");
        //                    size = data_temp.Length - 1;
        //                    flag = 1;
        //                    for (int m = 0; m <= size; m++)
        //                    {
        //                        if (!data_temp[m].ToString().Trim().Contains("--------------") && data_temp[m].Trim() != "")
        //                        {
        //                            temp_data = Regex.Replace(data_temp[m].ToString().Trim(), " {2,}", "~");
        //                            temp = Regex.Split(temp_data, "~");
        //                            UNIT = temp[1];
        //                            ID = temp[0].Trim();
        //                            m = m + 1;
        //                            if (data_temp[m].Trim() == "")
        //                            {
        //                                while (string.IsNullOrWhiteSpace(data_temp[m]))
        //                                {
        //                                    m = m + 1;
        //                                }
        //                            }
        //                            if (data_temp[m].ToString() != " ")
        //                            {
        //                                while (m != size + 1)
        //                                {
        //                                    if (data_temp[m].Trim().Contains("SOURCE ADDRESS 1"))
        //                                    {
        //                                        temp = Regex.Split(data_temp[m], ":");
        //                                        temp_data = temp[1].Trim();
        //                                        SOURCE_IP1 = temp_data;
        //                                    }
        //                                    if (data_temp[m].Trim().Contains("SOURCE ADDRESS 2"))
        //                                    {
        //                                        temp = Regex.Split(data_temp[m], ":");
        //                                        temp_data = temp[1].Trim();
        //                                        SOURCE_IP2 = temp_data;
        //                                    }
        //                                    if (data_temp[m].Trim().Contains("PRIMARY DEST"))
        //                                    {
        //                                        temp = Regex.Split(data_temp[m], ":");
        //                                        temp_data = temp[1].Trim();
        //                                        temp = Regex.Split(temp_data, "/");
        //                                        temp_data = temp[0].Trim();
        //                                        DESTINATION_IP1 = temp_data;
        //                                    }
        //                                    if (data_temp[m].Trim().Contains("SECONDARY DEST"))
        //                                    {
        //                                        temp = Regex.Split(data_temp[m], ":");
        //                                        temp_data = temp[1].Trim();
        //                                        temp = Regex.Split(temp_data, "/");
        //                                        temp_data = temp[0].Trim();
        //                                        DESTINATION_IP2 = temp_data;
        //                                    }
        //                                    if (DESTINATION_IP1 != "" && DESTINATION_IP2 != "" && SOURCE_IP1 != "" && SOURCE_IP2 != "")
        //                                    {
        //                                        if (flag == 1)
        //                                        {
        //                                            query = "select NODE, LINK_SET, IP_LINK, ASSOCIATION_SET_ID, ASSOCIATION_SET,NA,SPC from association_info_" + Circle + " where circle = '" + Circle + "' and node = '" + _ne_name + "' AND ASSOCIATION_SET_ID = '" + assc_id + "'  AND SOURCE_IP_1 IS NULL";
        //                                            SQLQuery(ref conn, ref dt, query);

        //                                            for (int p = 0; p <= dt.Rows.Count - 1; p++)
        //                                            {
        //                                                query1 = "insert into association_info_" + Circle + " (`NODE`, `LINK_SET`, `IP_LINK`, `ASSOCIATION_SET_ID`, `ASSOCIATION_SET`,`NA`,`SPC`, `ID`, `UNIT`, `SOURCE_IP_1`, `SOURCE_IP_2`, `DESTINATION_IP_1`, `DESTINATION_IP_2`,`CIRCLE`,`NODE_TYPE`) values ('" + dt.Rows[p][0].ToString() + "','" + dt.Rows[p][1].ToString() + "','" + dt.Rows[p][2].ToString() + "','" + dt.Rows[p][3].ToString() + "','" + dt.Rows[p][4].ToString() + "','" + dt.Rows[p][5].ToString() + "','" + dt.Rows[p][6].ToString() + "','" + ID + "', '" + UNIT + "', '" + SOURCE_IP1 + "', '" + SOURCE_IP2 + "', '" + DESTINATION_IP1 + "','" + DESTINATION_IP2 + "', '" + Circle + "', '" + _NEType + "' )";
        //                                                ExecuteSQLQuery(ref conn, query1);

        //                                            }
        //                                            dt.Clear();
        //                                        }

        //                                        flag = 0;
        //                                        DESTINATION_IP1 = "";
        //                                        DESTINATION_IP2 = "";
        //                                        SOURCE_IP1 = "";
        //                                        SOURCE_IP2 = "";

        //                                    }

        //                                    m = m + 1;
        //                                }
        //                            }

        //                        }
        //                    }
        //                }



        //            }
        //            catch (Exception ex)
        //            {
        //                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
        //                //if (!Directory.Exists(parsing_error_path))
        //                //{
        //                //    Directory.CreateDirectory(parsing_error_path);
        //                //}

        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function association_ZOYI ");
        //                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        //                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //                oExceptionLog.WriteExceptionErrorToFile("association_ZOYI_SUBwithContinue()", ErrorMsg, "", ref FileError);

        //                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //                ExecuteSQLQuery(ref conn, ErrorQuery);
        //                continue;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //        oExceptionLog.WriteExceptionErrorToFile("association_ZOYI()", ErrorMsg, "", ref FileError);

        //        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //        ExecuteSQLQuery(ref conn, ErrorQuery);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="Circle"></param>
        public void ASSOCIATION_ZRCI(string command_output, string Circle)
        {
            string output = command_output.Trim();
            string[] temp = null;
            DataTable dt = new DataTable();
            int flag1 = 0;
            string query1 = string.Empty;
            int flag2 = 0;
            string temp_data = string.Empty;
            int flag = 0;
            string CGR = string.Empty;
            string NCGR = string.Empty;
            string value = string.Empty;
            string[] grep = null;
            string query = string.Empty;
            int current_location;
            string SPC = string.Empty;
            query1 = "select distinct SPC from association_info_" + Circle + " where circle = '" + _CircleName + "' AND NODE = '" + _ne_name + "' ";
            SQLQuery(ref conn, ref dt, query1);
            for (int j = 0; j <= dt.Rows.Count - 1; j++)
            {
                try
                {
                    SPC = dt.Rows[j]["SPC"].ToString();
                    flag = 1;
                    temp = Regex.Split(output, "\r\n");

                    for (int i = 0; i <= temp.Length - 1; i++)
                    {
                        if (temp[i].Trim().Contains(SPC))
                        {
                            flag = 1;
                            flag1 = 1;
                            flag2 = 1;
                            current_location = i;
                            while (flag != 0)
                            {
                                if (temp[current_location].ToString().Trim().Contains("CGR"))
                                {
                                    value = temp[current_location].ToString().Trim();              // now the whole work from here is for greping the "cgr" and "ncgr" value
                                    grep = Regex.Split(value, " ");

                                    for (int n = 0; n <= grep.Length - 1; n++)
                                    {
                                        if (grep[n] == "CGR")
                                        {
                                            n = n + 1;
                                            while (flag1 != 0)
                                            {
                                                if (grep[n].Trim() != "" && grep[n].Trim() != ":")
                                                {
                                                    CGR = grep[n].Trim();
                                                    if (CGR != "")
                                                    {
                                                        flag1 = 0;
                                                    }
                                                }
                                                n = n + 1;
                                            }
                                        }
                                        if (grep[n] == "NCGR")
                                        {
                                            n = n + 1;

                                            while (flag2 != 0)
                                            {
                                                if (grep[n].Trim() != "" && grep[n].Trim() != ":")
                                                {
                                                    NCGR = grep[n].Trim();

                                                    if (NCGR != "")
                                                    {
                                                        flag2 = 0;
                                                    }
                                                }
                                                n = n + 1;
                                            }

                                        }

                                    }
                                    if (CGR != "" && NCGR != "")
                                    {
                                        query = "update association_info_" + Circle + " SET CGR_NAME = '" + CGR + "' , `CGR_NUMBER` = '" + NCGR + "' where `SPC` = '" + SPC + "' AND NODE = '" + _ne_name + "'";
                                        ExecuteSQLQuery(ref conn, query);
                                        flag = 0;

                                        NCGR = "";
                                        CGR = "";
                                    }
                                }
                                current_location = current_location - 1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function association_ZRCI ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ASSOCIATION_ZRCI_withcontinue()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);
                    continue;
                }
            }


        }
        #endregion

        // DESTINATION INFO PARSING FUNCTIONS
        #region[DESTINATION INFO PARSING FUNCTIONS]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="Circle"></param>
        public void parse_ZRRI(string command_output, string Circle)
        {
            string query = string.Empty;
            DataTable dt = new DataTable();
            string ne_ncgr = string.Empty;
            string temp_data = string.Empty;
            string[] output;
            string ROU = string.Empty;
            string query1 = string.Empty;
            string[] temp = Regex.Split(command_output, "ROU");
            string ne_cgr = string.Empty;

            if (group_name == "MSC")
            {
                query = "SELECT distinct ncgr, cgr FROM mss_mgw_map_" + Circle + " where ne_name = '" + _ne_name + "' and `circle` = '" + _CircleName + "' and `VENDOR` = '" + _vendor + "' group by ncgr";
            }
            else
            {
                query = "SELECT distinct ncgr, cgr FROM mss_mgw_map_" + Circle + "_" + group_name + " where ne_name = '" + _ne_name + "' and `circle` = '" + _CircleName + "' and `VENDOR` = '" + _vendor + "' group by ncgr";
            }

            SQLQuery(ref conn, ref dt, query);

            for (int i = 0; i <= dt.Rows.Count - 1; i++)        //loop to go through various ncgrs found under a single element
            {
                try
                {
                    ne_ncgr = dt.Rows[i][0].ToString().Trim();     // "ne_ncgr" = ncgr of the respective element
                    ne_cgr = dt.Rows[i][1].ToString().Trim();     // "ne_cgr" = cgr of the respective element

                    for (int j = 0; j <= temp.Length - 1; j++)
                    {
                        temp_data = temp[j].Trim();
                        if (temp_data.Contains(ne_ncgr))
                        {
                            output = Regex.Split(temp_data.Trim(), "\r\n");
                            output = Regex.Split(output[1].Trim(), " ");
                            ROU = output[0].Trim();                            //value of "ROU" is fetched here according to the respective ncgr

                            query1 = "insert into destination_info_" + Circle + " (`VENDOR`, `CIRCLE`, `NE_NAME`, `NE_TYPE`, `ROU`,`CGR`, `NCGR`) values  ('" + _vendor + "', '" + _CircleName + "','" + _ne_name + "','" + _NEType + "','" + ROU + "','" + ne_cgr + "','" + ne_ncgr + "')";
                            ExecuteSQLQuery(ref conn, query1);

                            if (temp_data.Contains("COMMAND EXECUTED"))
                            {
                                break;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function parse_ZRRI ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("parse_ZRRI_withcontinue()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);
                    continue;
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="Circle"></param>
        public void parse_ZRNI(string command_output, string Circle)
        {
            string query = string.Empty;
            DataTable dt = new DataTable();
            string ne_ROU = string.Empty;
            string temp_data = string.Empty;
            string[] output;
            string ROU = string.Empty;
            string query1 = string.Empty;
            string SPR = string.Empty;

            try
            {
                string[] temp = Regex.Split(command_output, "SPR");

                query = "select distinct `ROU` from destination_info_" + Circle + " where ne_name = '" + _ne_name + "' and `circle` = '" + _CircleName + "' and `VENDOR` = '" + _vendor + "'";
                SQLQuery(ref conn, ref dt, query);

                for (int i = 0; i <= dt.Rows.Count - 1; i++)
                {
                    try
                    {
                        ne_ROU = dt.Rows[i][0].ToString().Trim().ToString();
                        if (ne_ROU != "")
                        {
                            for (int j = 2; j <= temp.Length - 1; j++)
                            {
                                if (temp_data.Contains("\\b" + ne_ROU + "\\b"))
                                {
                                    output = Regex.Split(temp_data.Trim(), "\r\n");
                                    output = Regex.Split(output[1].Trim(), " ");
                                    SPR = output[0].Trim();
                                    query1 = "update destination_info_" + Circle + " SET SPR ='" + SPR + "' WHERE ROU='" + ne_ROU + "' AND CIRCLE='" + Circle + "'AND NE_NAME='" + _ne_name + "'AND VENDOR='" + _vendor + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    if (temp_data.Contains("COMMAND EXECUTED"))
                                    {
                                        break;
                                    }

                                }
                            }
                        }


                        if (temp_data.Contains("COMMAND EXECUTED"))
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function parse_ZRNI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("parse_ZRNI_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("parse_ZRNI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="Circle"></param>
        public void parse_ZRIL(string command_output, string Circle)
        {
            string query = string.Empty;
            DataTable dt = new DataTable();
            string ne_ROU = string.Empty;
            string temp_data = string.Empty;
            string ROU = string.Empty;
            string query1 = string.Empty;
            string rou = string.Empty;
            string SPR = string.Empty;
            string nsdest = string.Empty;
            string ndest = string.Empty;
            string alt = string.Empty;
            string rt = string.Empty;

            try
            {
                string[] temp = Regex.Split(command_output, "TCR");

                query = "select distinct `SPR` from destination_info_" + Circle + " where ne_name = '" + _ne_name + "' and `circle` = '" + _CircleName + "' and `VENDOR` = '" + _vendor + "'";

                SQLQuery(ref conn, ref dt, query);
                string temp1 = string.Empty;

                if (dt.Rows.Count == 1)
                {
                    temp1 = dt.Rows[0][0].ToString();     // it checks for empty spr's
                }

                if (dt.Rows.Count <= 0 || temp1 == "")
                {

                    dt.Columns.Clear();
                    query = "select distinct `ROU` from destination_info_" + Circle + " where ne_name = '" + _ne_name + "' and `circle` = '" + _CircleName + "' and `VENDOR` = '" + _vendor + "'";
                    SQLQuery(ref conn, ref dt, query);

                    rt = "ROU";
                    for (int i = 0; i <= temp.Length - 1; i++)
                    {
                        try
                        {
                            string[] nData = Regex.Split(temp[i], "\r\n");

                            for (int j = 0; j <= nData.Length - 1; j++)
                            {
                                if (nData[j].Contains("ROU"))
                                {
                                    //foreach (string rou in dt.Rows.ToString())
                                    for (int k = 0; k <= dt.Rows.Count - 1; k++)
                                    {
                                        rou = dt.Rows[k][0].ToString().Trim();
                                        if (rou != "")
                                        {
                                            if (nData[j].Contains(rou))
                                            {
                                                string[] ns = Regex.Split(nData[j].Trim(), "     ");

                                                if (ns.Length != 1)
                                                {
                                                    nsdest = ns[1].ToString();
                                                }

                                                else
                                                {
                                                    ns = Regex.Split(nData[j].Trim(), "  ");
                                                    nsdest = ns[1].ToString();

                                                    if (nsdest == "" || nsdest == " ")
                                                    {
                                                        nsdest = ns[2].ToString();

                                                        if (nsdest.Contains("ROU"))
                                                        {
                                                            ns = Regex.Split(nsdest.Trim(), "ROU");
                                                            nsdest = ns[0].ToString();
                                                        }
                                                    }
                                                }


                                                if (nsdest == "" || nsdest == " " || ns.Length < 3)
                                                {
                                                    string[] ns1 = Regex.Split(ns[0].Trim(), " ");
                                                    nsdest = ns1[ns1.Length - 1].ToString();
                                                }

                                                string[] al = Regex.Split(nData[j - 3].Trim(), "   ");
                                                alt = al[0].ToString();
                                                string[] nd = Regex.Split(temp[i - 1].Trim(), "\r\n");
                                                nd = Regex.Split(nd[nd.Length - 1].Trim(), "  ");
                                                ndest = nd[1].ToString().Trim();

                                                if (ndest == "")
                                                {
                                                    ndest = nd[2].Trim().ToString();
                                                }

                                                if (ndest.Contains(" "))
                                                {
                                                    string[] nd1 = Regex.Split(ndest, " ");
                                                    ndest = nd1[0].ToString();

                                                }


                                                query1 = "update destination_info_" + Circle + " SET NDEST ='" + ndest + "',NSDEST='" + nsdest + "',ALT='" + alt + "',RT='" + rt + "' WHERE ROU='" + rou + "' AND CIRCLE='" + Circle + "'AND NE_NAME='" + _ne_name + "'AND VENDOR='" + _vendor + "'";

                                                ExecuteSQLQuery(ref conn, query1);


                                            }


                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                            //if (!Directory.Exists(parsing_error_path))
                            //{
                            //    Directory.CreateDirectory(parsing_error_path);
                            //}

                            //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function parse_ZRIL ");
                            //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("parse_ZRIL_SUB1withContinue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            continue;
                        }
                    }



                }
                else
                {
                    string rous = string.Empty;
                    rt = "SPR";

                    for (int i = 0; i <= temp.Length - 1; i++)
                    {
                        try
                        {
                            string[] nData = Regex.Split(temp[i], "\r\n");

                            for (int j = 0; j <= nData.Length - 1; j++)
                            {
                                if (nData[j].Contains("SPR"))
                                {
                                    //foreach (string rous in dt.Rows)
                                    for (int n = 0; i <= dt.Rows.Count - 1; n++)
                                    {
                                        rous = dt.Rows[n][0].ToString().Trim();
                                        if (rous != "")
                                        {
                                            if (nData[j].Contains(rous))
                                            {
                                                string[] ns = Regex.Split(nData[j].Trim(), "     ");

                                                if (ns.Length != 1)
                                                {
                                                    nsdest = ns[1].ToString();
                                                }

                                                else
                                                {
                                                    ns = Regex.Split(nData[j].Trim(), "  ");
                                                    nsdest = ns[1].ToString();
                                                    if (nsdest == "" || nsdest == " ")
                                                    {
                                                        nsdest = ns[2].ToString();

                                                        if (nsdest.Contains("SPR"))
                                                        {
                                                            ns = Regex.Split(nsdest.Trim(), "SPR");
                                                            nsdest = ns[0].ToString();
                                                        }
                                                    }
                                                }

                                                if (nsdest == "" || nsdest == " " || ns.Length < 3)
                                                {
                                                    string[] ns1 = Regex.Split(ns[0].Trim(), " ");
                                                    nsdest = ns1[ns1.Length - 1].ToString();
                                                }

                                                string[] al = Regex.Split(nData[j - 3].Trim(), "   ");
                                                alt = al[0].ToString();
                                                string[] nd = Regex.Split(temp[i - 1].Trim(), "\r\n");
                                                nd = Regex.Split(nd[nd.Length - 1].Trim(), "  ");
                                                ndest = nd[1].ToString().Trim();

                                                if (ndest == "")
                                                {
                                                    ndest = nd[2].Trim().ToString();
                                                }

                                                if (ndest.Contains(" "))
                                                {
                                                    string[] nd1 = Regex.Split(ndest, " ");
                                                    ndest = nd1[0].ToString();

                                                }
                                                query1 = "update destination_info_" + Circle + " SET NDEST ='" + ndest + "',NSDEST='" + nsdest + "',ALT='" + alt + "',RT='" + rt + "' WHERE SPR='" + rous.Trim().ToString() + "' AND CIRCLE='" + _CircleName + "'AND NE_NAME='" + _ne_name + "'AND VENDOR='" + _vendor + "'";
                                                ExecuteSQLQuery(ref conn, query1);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                            //if (!Directory.Exists(parsing_error_path))
                            //{
                            //    Directory.CreateDirectory(parsing_error_path);
                            //}

                            //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function parse_ZRIL ");
                            //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("parse_ZRIL_SUB2withContinue()", ErrorMsg, "", ref FileError);

                            ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                            ExecuteSQLQuery(ref conn, ErrorQuery);
                            continue;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("parse_ZRIL()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }
        #endregion

        //MSC PARSING FUNCTIONS
        #region[MSC PARSING FUNCTIONS]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_MSC_ZRCI_PRINT_5(string output, string CircleName, string NE_Type)
        {
            string[] temp;
            string temp1 = string.Empty;
            string temp_data = string.Empty;
            string[] temp_value;
            string[] value;
            string CGR = string.Empty;
            string CGR_UPART = string.Empty;
            string CGR_NET = string.Empty;
            string CGR_SPC = string.Empty;
            string NCGR = string.Empty;
            string T_ID = string.Empty;
            string vmgw = string.Empty;
            string CGR_TYPE = string.Empty;
            string[] term_id;
            string CIC = string.Empty;
            string[] value1;
            string pattern = "\\bCGR\\b";
            string check1 = string.Empty;
            string check2 = string.Empty;
            DataTable dt = new DataTable();
            string query = string.Empty;

            try
            {
                string cmnd_output = Regex.Replace(output, pattern, "CGR_VALUE");
                string[] data = Regex.Split(cmnd_output, "CGR_VALUE");

                for (int i = 2; i <= data.Length - 1; i++)
                {
                    try
                    {
                        CGR = "";
                        CGR_NET = "";
                        CGR_SPC = "";
                        CGR_TYPE = "";
                        NCGR = "";
                        vmgw = "";
                        T_ID = "";
                        CIC = "";
                        CGR_UPART = "";
                        check1 = "";
                        check2 = "";

                        if (!data[i].Contains("SPE"))
                        {
                            temp1 = data[i].Trim().ToString();
                            temp = Regex.Split(temp1, "\r\n");

                            for (int j = 0; j <= temp.Length - 1; j++)
                            {
                                if (temp[j].Contains("COMMAND EXECUTED"))
                                {
                                    break;
                                }

                                if (temp[j].Contains("NCGR") && temp[j].Trim() != "")
                                {
                                    temp_data = temp[j].Trim();
                                    temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                    temp_value = Regex.Split(temp_data, "~");
                                    value = Regex.Split(temp_value[0].Trim(), ":");
                                    value1 = Regex.Split(temp_value[2].Trim(), ":");
                                    CGR = value[1].Trim();
                                    NCGR = value1[1].Trim();

                                }
                                if (temp[j].Contains("TYPE") && temp[j].Contains("STATE"))
                                {
                                    temp_data = temp[j].Trim().ToString();
                                    temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                    temp_value = Regex.Split(temp_data, "~");
                                    CGR_TYPE = temp_value[1].Trim();
                                    CGR_TYPE = CGR_TYPE.Replace(":", " ");
                                    CGR_TYPE = CGR_TYPE.Trim();
                                }

                                if (temp[j].Contains("SPC(H/D)") && temp[j].Contains("NET"))
                                {
                                    temp_data = temp[j].Trim();
                                    temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                    temp_value = Regex.Split(temp_data, "~");

                                    CGR_NET = temp_value[3].Trim();
                                    CGR_NET = CGR_NET.Replace(":", " ");
                                    CGR_NET = CGR_NET.Trim();

                                    value = Regex.Split(temp_value[temp_value.Length - 1].Trim(), ":");
                                    CGR_SPC = value[1].Trim().ToString();

                                }

                                if (temp[j].Contains("UPART"))
                                {
                                    temp_data = temp[j].Trim();
                                    temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                    temp_value = Regex.Split(temp_data, "~");
                                    CGR_UPART = temp_value[3].Trim();
                                    CGR_UPART = CGR_UPART.Replace(":", " ");
                                    CGR_UPART = CGR_UPART.Trim();

                                }

                                if (temp[j].Contains("PCM-TSL") && temp[j].Contains("HGR") && (temp[j].Contains("CCSPCM")))      // ccspcm is neccessary 
                                {
                                    j = j + 1;

                                    while (j <= temp.Length - 1)
                                    {

                                        if (temp[j].Trim() != "" && !temp[j].Trim().Contains("COMMAND EXECUTED"))
                                        {
                                            temp_data = temp[j].Trim();
                                            temp_data = Regex.Replace(temp_data, " {2,}", "~");
                                            temp_value = Regex.Split(temp_data, "~");
                                            term_id = Regex.Split(temp_value[0].Trim(), "-");
                                            T_ID = term_id[0].Trim();

                                            if (temp_value.Length > 4)
                                            {
                                                CIC = temp_value[5].Trim().ToString();
                                            }

                                            if (temp[j].Contains("COMMAND EXECUTED"))
                                            {
                                                break;
                                            }

                                            if (T_ID != check1 && CIC != check2)
                                            {
                                                check1 = T_ID;
                                                check2 = CIC;

                                                if (CGR != "" && CGR_NET != "" && CGR_SPC != "" && CGR_TYPE != "" && CGR_UPART != "" && NCGR != "" && T_ID != " " && CIC != " " && vmgw != " ")
                                                {
                                                    SQLQuery(ref conn, ref dt, "select * from msc_" + CircleName + " where NE_NAME = '" + _ne_name + "' and NODE_TYPE = '" + NE_Type + "' and circle = '" + _CircleName + "' and cgr_net is null and cgr_spc is null and ncgr is null and cic is null");
                                                    for (int l = 0; l <= dt.Rows.Count - 1; l++)
                                                    {
                                                        query = "insert into msc_" + CircleName + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, CGR, NCGR, CGR_SPC, CGR_NET, CGR_TYPE, CGR_UPART,ET, CIC) values ";
                                                        query += "('" + dt.Rows[l][0].ToString() + "','" + dt.Rows[l][1].ToString() + "','" + dt.Rows[l][2].ToString() + "','" + dt.Rows[l][3].ToString() + "','" + dt.Rows[l][4].ToString() + "','" + dt.Rows[l][5].ToString() + "','" + dt.Rows[l][6].ToString() + "','" + dt.Rows[l][7].ToString() + "','" + dt.Rows[l][8].ToString() + "', '" + CGR + "', '" + NCGR + "', '" + CGR_SPC + "', '" + CGR_NET + "', '" + CGR_TYPE + "', '" + CGR_UPART + "', '" + T_ID + "','" + CIC + "')";

                                                        ExecuteSQLQuery(ref conn, query);


                                                    }

                                                    dt.Clear();
                                                }

                                            }

                                        }
                                        j = j + 1;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_MSC_ZRCI_PRINT_5 ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_MSC_ZRCI_PRINT_5_SUB2withContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_MSC_ZRCI_PRINT_5()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }
        #endregion

        //mgw EXTRA_ET PARSING FUNCTIONS
        #region[MGW EXTRA_ET PARSING FUNCTIONS]
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        /// <param name="net"></param>
        public void MGW_EXTRA_ET_ZNSI(string output, string CircleName, string NE_Type, string net)
        {
            string temp_data = string.Empty; ;
            string slc = string.Empty; ;
            string[] value;
            string[] temp;
            string query = string.Empty; ;
            string cgr_spc = string.Empty; ;
            string cgr_net = string.Empty; ;
            string link = string.Empty; ;

            try
            {
                string[] cmnd_output = Regex.Split(output.Trim(), "COMMAND EXECUTED");
                string data = cmnd_output[0].Trim();
                cmnd_output = Regex.Split(data, "SLC");

                if (cmnd_output.Length == 1)
                {
                    data = cmnd_output[0].Trim().ToString();
                }
                else
                {
                    data = cmnd_output[1].Trim().ToString();
                }
                temp = Regex.Split(data, "\r\n");


                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {
                        if (!temp[i].Trim().Contains("----------") && temp[i] != "")
                        {
                            while (temp[i] != "COMMAND EXECUTED" && i <= temp.Length && temp[i].Trim() != "")
                            {
                                if (temp[i].Trim() != "" && !temp[i].Trim().Contains("----------"))
                                {
                                    temp_data = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                                    value = Regex.Split(temp_data.ToString().Trim(), "~");

                                    if (value.Length >= 2)
                                    {
                                        slc = value[value.Length - 1].ToString();
                                        link = value[value.Length - 2].ToString();
                                    }

                                    if (value.Length > 3)
                                    {
                                        cgr_net = value[0].Trim();
                                        cgr_spc = value[1].Trim();
                                    }
                                    query = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set SLC = '" + slc.Trim() + "', CGR_NET = '" + cgr_net + "', cgr_spc = '" + cgr_spc + "'   where link = '" + link.Trim() + "' AND mgw_name = '" + _ne_name.Trim() + "'  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "' and EXTRA_ET_FLAG = '2'  ";
                                    ExecuteSQLQuery(ref conn, query);
                                }

                                i = i + 1;
                                if (i == temp.Length)
                                {
                                    break;
                                }
                            }
                        }

                    }

                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function MGW_EXTRA_ET_ZNSI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("MGW_EXTRA_ET_ZNSI_SUB2withContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("MGW_EXTRA_ET_ZNSI()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GIGBtxt"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ZNRI_MGW_EXTRA_ET(string[] GIGBtxt, string CircleName, string NE_Type)
        {
            string ErrorMsg = string.Empty;
            string Vendor = string.Empty;
            string Circle = string.Empty;
            string MGW_SPC = string.Empty;
            string SqlStr = string.Empty;
            string[] FindResult;
            int cnt = 0;

            try
            {
                for (int m = 0; m <= GIGBtxt.Length - 1; m++)
                {
                    try
                    {
                        if (GIGBtxt[0].Trim().ToString().IndexOf("ZNRI:NA0") != -1)
                        {
                            if (GIGBtxt[m].Trim().ToString().IndexOf("") != -1)
                            {
                                if (GIGBtxt[m].Trim().ToString().IndexOf("SP CODE H/D") != -1)
                                {
                                    if (GIGBtxt[m + 2].Trim().ToString().IndexOf("OWN SP") != -1)
                                    {
                                        cnt = 0;
                                        FindResult = null;
                                        FindResult = Regex.Split(GIGBtxt[m + 2].Trim(), " ");

                                        for (int k = 0; k <= FindResult.Length - 1; k++)
                                        {
                                            if (FindResult[k].Trim().ToString() != "")
                                            {
                                                cnt += 1;
                                                if (cnt == 2)
                                                {
                                                    MGW_SPC = FindResult[k].Trim().ToString();

                                                    if (MGW_SPC != "")
                                                    {
                                                        SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set MGW_SPC='" + MGW_SPC.Trim() + "' Where CGR_NET='NA0' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and EXTRA_ET_FLAG = '2'";
                                                        ExecuteSQLQuery(ref conn, SqlStr);
                                                    }

                                                    MGW_SPC = "";
                                                    break;
                                                }
                                            }
                                        }
                                        ;
                                    }
                                }
                            }

                        }

                        else if (GIGBtxt[0].Trim().ToString().IndexOf("ZNRI:NA1") != -1)
                        {
                            if (GIGBtxt[m].Trim().ToString().IndexOf("") != -1)
                            {
                                if (GIGBtxt[m].Trim().ToString().IndexOf("SP CODE H/D") != -1)
                                {
                                    if (GIGBtxt[m + 2].Trim().ToString().IndexOf("OWN SP") != -1)
                                    {
                                        cnt = 0;
                                        FindResult = null;
                                        FindResult = Regex.Split(GIGBtxt[m + 2].Trim(), " ");

                                        for (int k = 0; k <= FindResult.Length - 1; k++)
                                        {
                                            if (FindResult[k].Trim().ToString() != "")
                                            {
                                                cnt += 1;
                                                if (cnt == 2)
                                                {
                                                    MGW_SPC = FindResult[k].Trim().ToString();

                                                    if (MGW_SPC != "")
                                                    {
                                                        SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set MGW_SPC='" + MGW_SPC.Trim() + "' Where CGR_NET='NA1' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'and EXTRA_ET_FLAG = '1'";
                                                        ExecuteSQLQuery(ref conn, SqlStr);
                                                    }

                                                    MGW_SPC = "";
                                                    break;
                                                }
                                            }
                                        }
                                        ;
                                    }
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ZNRI_MGW_EXTRA_ET ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ZNRI_MGW_EXTRA_ET_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ZNRI_MGW_EXTRA_ET()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GIGBtxt"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_ZW7N_Extra_et(string[] GIGBtxt, string CircleName, string NE_Type)
        {
            string ErrorMsg = string.Empty;
            string Vendor = string.Empty;
            string Circle = string.Empty;
            string MGW_C_NO = string.Empty;
            string SqlStr = string.Empty; ;
            string[] FindResult;

            for (int m = 0; m <= GIGBtxt.Length - 1; m++)
            {
                try
                {
                    if (GIGBtxt[m].Trim().ToString().IndexOf("") !=
                        -1)
                    {
                        if (GIGBtxt[m].Trim().ToString().IndexOf("TARGET IDENTIFIER") != -1)
                        {
                            FindResult = GIGBtxt[m].Split(':');
                            MGW_C_NO = FindResult[1].Trim().ToString();

                            if (MGW_C_NO != "")
                            {
                                SqlStr = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set MGW_C_NO='" + MGW_C_NO.Trim() + "' Where MGW_Name='" + _ne_name.Trim() + "' and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and EXTRA_ET_FLAG = '2' ";
                                ExecuteSQLQuery(ref conn, SqlStr);
                            }
                            MGW_C_NO = "";
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZW7N_Extra_et( ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                    oExceptionLog.WriteExceptionErrorToFile("ParseData_ZW7N_Extra_et_withContinue()", ErrorMsg, "", ref FileError);

                    ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                    ExecuteSQLQuery(ref conn, ErrorQuery);
                    continue;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ZUSI_IWS1E_EXTRA_ET(string command_output, string CircleName, string NE_Type)
        {
            string[] temp;
            string[] value;
            string[] value1;
            string[] value2;
            string ET = string.Empty;
            string VETGR = string.Empty;
            string STER = string.Empty;
            string UNIT_IWS1E = string.Empty;
            string temp_output = string.Empty;
            string[] temp1;
            string query = string.Empty;
            string output = command_output.Trim();
            string temp_value = string.Empty;
            int i = 0;

            try
            {
                temp = Regex.Split(output, "\r\n");
                int size = temp.Length - 1;

            endhere: for (; i <= size - 1; i++)
                {
                    try
                    {
                        if ((temp[i].Contains("IWS1E") || temp[i].Contains("IWSEP")) && (!temp[i].Contains("ZUSI:IWS1E::FULL") && !temp[i].Contains("ZUSI:IWSEP::FULL")))
                        {
                            while (temp[i].ToString() != "COMMAND EXECUTED")
                            {
                            nextIWS1E: if (temp[i].Contains("IWS1E") || temp[i].Contains("IWSEP"))
                                {
                                    value = Regex.Split(temp[i].ToString().Trim(), " ");
                                    UNIT_IWS1E = value[0].Trim();
                                }

                                if (temp[i].Contains("STER") || temp[i].Contains("NPUP"))
                                {
                                    temp1 = Regex.Split(temp[i].ToString().Trim(), " ");
                                    temp1 = Regex.Split(temp1[0], "-");
                                    STER = temp1[1];
                                }


                            comehere: if (temp[i].Contains("VETGR") || temp[i].Contains("LETGR"))
                                {
                                    value1 = Regex.Split(temp[i].ToString().Trim(), " ");
                                    value1 = Regex.Split(value1[0], "-");
                                    VETGR = value1[1];
                                    i = i + 1;

                                    if (temp[i].Contains("ET"))
                                    {
                                        while (temp[i] != "LETGR" || temp[i] != "VETGR")
                                        {

                                            if (temp[i].Contains("IWS1E") || temp[i].Contains("IWSEP"))
                                            {
                                                goto nextIWS1E;
                                            }

                                            if (temp[i].Contains("COMMAND EXECUTED"))
                                            {
                                                i = size;
                                                goto endhere;
                                            }


                                            if (temp[i].Contains("ET") && !temp[i].Contains("SET") && !temp[i].Contains("NPUP"))
                                            {
                                                value2 = Regex.Split(temp[i].ToString().Trim(), " ");
                                                value2 = Regex.Split(value2[0], "-");
                                                ET = value2[1];



                                                query = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set UNIT = '" + UNIT_IWS1E + "' ,STER = '" + STER + "', ETGR_VETGR = '" + VETGR + "' where ET = '" + ET + "' and circle = '" + _CircleName + "' and MGW_NAME = '" + _ne_name + "' and extra_et_flag ='2'";
                                                ExecuteSQLQuery(ref conn, query);

                                                if (temp[i].Contains("VETGR") || temp[i].Contains("LETGR"))
                                                {
                                                    goto comehere;
                                                }



                                                if (temp[i].Contains("COMMAND EXECUTED"))
                                                {
                                                    i = size;
                                                    goto endhere;
                                                }

                                            }

                                            i = i + 1;
                                        }
                                    }
                                }
                                i = i + 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ZUSI_IWS1E_EXTRA_ET ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ZUSI_IWS1E_EXTRA_ET_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }

                update_extra_et(_CircleName, _NEType, _ne_name);
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ZUSI_IWS1E_EXTRA_ET()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ZUSI_NIWU_EXTRA_ET(string command_output, string CircleName, string NE_Type)
        {
            string[] temp;
            string[] value;
            string[] value1;
            string[] value2;
            string ET = string.Empty;
            string ETGR = string.Empty;
            int i = 0;
            string UNIT_NIWU = string.Empty;
            string temp_output = string.Empty;
            string query = string.Empty;
            string output = command_output.Trim();
            string temp_value = string.Empty;

            try
            {
                temp = Regex.Split(output, "\r\n");

                int size = temp.Length - 1;

            endhere: for (; i <= size - 1; i++)
                {
                    try
                    {

                        if (temp[i].Contains("NIWU") && !temp[i].Contains("ZUSI:NIWU"))  // condition to ignore all previous values before "IWS1E" in the output 
                        {
                            while (temp[i].ToString() != "COMMAND EXECUTED")
                            {

                            nextIWS1E: if (temp[i].Contains("NIWU"))
                                {
                                    value = Regex.Split(temp[i].ToString().Trim(), " ");
                                    UNIT_NIWU = value[0].Trim();
                                }


                            comehere: if (temp[i].Contains("ETGR"))
                                {
                                    value1 = Regex.Split(temp[i].ToString().Trim(), " ");
                                    value1 = Regex.Split(value1[0], "-");
                                    ETGR = value1[1];
                                    i = i + 1;


                                    if (temp[i].Contains("ET"))
                                    {
                                        while (temp[i] != "ETGR")
                                        {
                                            if (temp[i].Contains("NIWU"))
                                            {
                                                goto nextIWS1E;
                                            }

                                            if (temp[i].Contains("COMMAND EXECUTED"))
                                            {
                                                i = size;
                                                goto endhere;
                                            }

                                            if (temp[i].Contains("ET") && !temp[i].Contains("SET"))
                                            {
                                                value2 = Regex.Split(temp[i].ToString().Trim(), " ");
                                                value2 = Regex.Split(value2[0], "-");
                                                ET = value2[1];

                                                query = "Update mss_mgw_map_" + CircleName + "_" + group_name + " set UNIT = '" + UNIT_NIWU + "' , ETGR_VETGR = '" + ETGR + "' where ET = '" + ET + "' and circle = '" + _CircleName + "' and MGW_NAME = '" + _ne_name + "' and extra_et_flag ='2' ";
                                                ExecuteSQLQuery(ref conn, query);

                                                if (temp[i].Contains("ETGR"))
                                                {
                                                    goto comehere;
                                                }


                                                if (temp[i].Contains("COMMAND EXECUTED"))
                                                {
                                                    i = size;
                                                    goto endhere;
                                                }


                                            }
                                            i = i + 1;
                                        }
                                    }

                                }


                                i = i + 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ZUSI_NIWU_EXTRA_ET ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ZUSI_NIWU_EXTRA_ET_SUBwithContinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }


                update_extra_et(_CircleName, _NEType, _ne_name);
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ZUSI_NIWU_EXTRA_ET()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="ne_type"></param>
        /// <param name="ne_name"></param>
        public void update_extra_et(string circle, string ne_type, string ne_name)
        {
            DataTable dt = new DataTable();
            string mss_c_no = string.Empty;
            string MNC = string.Empty;
            string MCC = string.Empty;
            string MSS = string.Empty;
            string element_ip = string.Empty;
            string mss_spc_na0 = string.Empty;
            string mss_spc_na1 = string.Empty;

            try
            {
                string query = "SELECT distinct mss_c_no FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where mgw_name ='" + _ne_name + "' and node_type = 'MGW' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "';";
                SQLQuery(ref conn, ref dt, query);
                if (dt.Rows.Count > 0)
                {
                    mss_c_no = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set `MSS_C_NO` = '" + mss_c_no + "' where mgw_name ='" + _ne_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='2' ");
                    dt.Clear();
                }


                query = "SELECT distinct `MNC` FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where mgw_name ='" + _ne_name + "' and node_type = 'MGW' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "';";
                SQLQuery(ref conn, ref dt, query);
                if (dt.Rows.Count > 0)
                {
                    MNC = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set `MNC` = '" + MNC + "' where mgw_name ='" + _ne_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='2' ");
                    dt.Clear();
                }





                query = "SELECT distinct `MCC` FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where mgw_name ='" + _ne_name + "' and node_type = 'MGW' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "';";
                SQLQuery(ref conn, ref dt, query);
                if (dt.Rows.Count > 0)
                {
                    MCC = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set `MCC` = '" + MCC + "' where mgw_name ='" + _ne_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='2' ");
                    dt.Clear();
                }




                query = "SELECT distinct `MSS` FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where mgw_name ='" + _ne_name + "' and node_type = 'MGW' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "';";
                SQLQuery(ref conn, ref dt, query);
                if (dt.Rows.Count > 0)
                {
                    MSS = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set `MSS` = '" + MSS + "' where mgw_name ='" + _ne_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='2' ");
                    dt.Clear();

                }




                query = "SELECT distinct `element_ip` FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where mgw_name ='" + _ne_name + "' and node_type = 'MGW' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "';";
                SQLQuery(ref conn, ref dt, query);
                if (dt.Rows.Count > 0)
                {
                    element_ip = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set `element_ip` = '" + MSS + "' where mgw_name ='" + _ne_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='2' ");
                    dt.Clear();

                }



                query = "SELECT distinct `mss_spc` FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where mgw_name ='" + _ne_name + "' and node_type = 'MGW' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and cgr_net = 'NA0';";
                SQLQuery(ref conn, ref dt, query);
                if (dt.Rows.Count > 0)
                {
                    mss_spc_na0 = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set `mss_spc` = '" + mss_spc_na0 + "' where mgw_name ='" + _ne_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='2' and cgr_net = 'NA0' ");
                    dt.Clear();


                }



                query = "SELECT distinct `mss_spc` FROM mss_mgw_map_" + _CircleName + "_" + group_name + " where mgw_name ='" + _ne_name + "' and node_type = 'MGW' and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and cgr_net = 'NA1';";
                SQLQuery(ref conn, ref dt, query);
                if (dt.Rows.Count > 0)
                {
                    mss_spc_na1 = dt.Rows[0][0].ToString();
                    ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set `mss_spc` = '" + mss_spc_na1 + "' where mgw_name ='" + _ne_name + "'  and circle = '" + _CircleName + "' and vendor = '" + _vendor + "' and extra_et_flag ='2' and cgr_net = 'NA1' ");
                    dt.Clear();

                }

            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("update_extra_et()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }

        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void update_Generic_name()
        {
            DataTable dt = new DataTable();
            String generic_name = string.Empty; ;
            String cgr_net = string.Empty; ;
            String spc = string.Empty; ;
            String SPC_TYPE = string.Empty; ;
            String query1 = string.Empty; ;
            String newquery = string.Empty; ;
            String query = string.Empty; ;

            try
            {

                query = "SELECT * FROM mss_mgw_map_generic_name where circle = '" + _CircleName + "'";
                SQLQuery(ref conn, ref dt, query);


                for (int i = 0; i < dt.Rows.Count - 1; i++)
                {
                    SPC_TYPE = dt.Rows[i][0].ToString();
                    generic_name = dt.Rows[i][1].ToString();
                    cgr_net = dt.Rows[i][2].ToString();
                    spc = dt.Rows[i][3].ToString();


                    if (_NEMAP == "MSS MAP")
                    {
                        query1 = "update mss_mgw_map_" + _CircleName + "_" + group_name + " set GENERIC_NAME = '" + generic_name + "', SPC_TYPE = '" + SPC_TYPE + "' where  CGR_SPC  like '%" + spc + "/%' and CGR_NET = '" + cgr_net + "' and CIRCLE = '" + _CircleName + "'";
                        ExecuteSQLQuery(ref conn, query1);

                    }

                    else if (_NEMAP == "MSC MAP")
                    {
                        query1 = "update msc_" + _CircleName + " set GENERIC_NAME = '" + generic_name + "', SPC_TYPE = '" + SPC_TYPE + "'  where CGR_SPC like '%" + spc + "/%'and CGR_NET = '" + cgr_net + "' and CIRCLE = '" + _CircleName + "'";
                        ExecuteSQLQuery(ref conn, query1);

                    }

                    else if (_NEMAP == "HLR MAP")
                    {
                        query1 = "update hlr_" + _CircleName + " set GENERIC_NAME = '" + generic_name + "', SPC_TYPE = '" + SPC_TYPE + "' where  CGR_SPC  like '%" + spc + "/%' and CGR_NET = '" + cgr_net + "' and CIRCLE = '" + _CircleName + "'";
                        ExecuteSQLQuery(ref conn, query1);

                    }
                }


                dt.Clear();
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("update_Generic_name()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }




        }

        // for updtae queries
        //public bool ExecuteSQLQuery(ref MySqlConnection MYSQL_connection, string querystr)
        //{
        //    MySqlCommand mMySqlCommand = new MySqlCommand();

        //    mMySqlCommand.CommandText = querystr;
        //    mMySqlCommand.CommandType = CommandType.Text;
        //    mMySqlCommand.Connection = MYSQL_connection;


        //    if (MYSQL_connection.State != ConnectionState.Open)
        //    {
        //        conn.Open();
        //    }

        //    if (MYSQL_connection.State != ConnectionState.Open)
        //    {
        //        conn.Open();
        //    }
        //    try
        //    {
        //        mMySqlCommand.Connection = MYSQL_connection;
        //        mMySqlCommand.ExecuteNonQuery();
        //        MYSQL_connection.Close();
        //        return true;
        //    }
        //    catch (MySqlException ex)
        //    {
        //        return false;
        //    }
        //}
        //// for select1 queries
        //public bool SQLQuery(ref MySqlConnection MYSQL_connection, ref DataTable DTCombo, string querystr)
        //{
        //    DTCombo = new DataTable();
        //    MySqlDataAdapter ADAPTCombo = new MySqlDataAdapter();
        //    MySqlCommand mMySqlCommand = new MySqlCommand();


        //    mMySqlCommand.CommandText = querystr;
        //    mMySqlCommand.CommandType = CommandType.Text;
        //    mMySqlCommand.Connection = MYSQL_connection;

        //    if (MYSQL_connection.State != ConnectionState.Open)
        //    {
        //        conn.Open();
        //    }

        //    if (MYSQL_connection.State != ConnectionState.Open)
        //    {
        //        conn.Open();
        //    }
        //    ADAPTCombo.ReturnProviderSpecificTypes = true;
        //    mMySqlCommand.Connection = MYSQL_connection;
        //    ADAPTCombo.SelectCommand = mMySqlCommand;
        //    try
        //    {
        //        ADAPTCombo.Fill(DTCombo);
        //        MYSQL_connection.Close();
        //        conn.Close();
        //        return true;

        //    }
        //    catch (MySqlException ex)
        //    {
        //        conn.Close();
        //        return false;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MYSQL_connection"></param>
        /// <param name="DT"></param>
        /// <param name="querystr"></param>
        /// <returns></returns>
        public bool SQLQuery(ref MySqlConnection MYSQL_connection, ref DataTable DT, string querystr)
        {
            int retrycount = 0;
            DT = new DataTable();
            MySqlDataAdapter ADAPTCombo = new MySqlDataAdapter();
            MySqlCommand mMySqlCommand = new MySqlCommand();

            mMySqlCommand.CommandText = querystr;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = MYSQL_connection;
            mMySqlCommand.CommandTimeout = 900000;

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
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString() + ".Query:" + querystr;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MYSQL_connection"></param>
        /// <param name="querystr"></param>
        /// <returns></returns>
        public bool ExecuteSQLQuery(ref MySqlConnection MYSQL_connection, string querystr)
        {
            int retrycount = 0;
            MySqlCommand mMySqlCommand = new MySqlCommand();

            mMySqlCommand.CommandText = querystr;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = MYSQL_connection;
            mMySqlCommand.CommandTimeout = 900000;


        //if (MYSQL_connection.State != ConnectionState.Open)
        //{
        //    conn.Open();
        //}
        retry:
            try
            {

                if (MYSQL_connection.State != ConnectionState.Open)
                {
                    conn.Open();
                }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        public void Inser_data_into_final_table(string circle)
        {

            try
            {

                if (circle.ToLower().Contains("group"))
                {
                    ExecuteSQLQuery(ref conn, "insert into  mss_mgw_map_" + _CircleName + "  ( select distinct * from mss_mgw_map_" + _CircleName + "_" + group_name + ")");



                    ExecuteSQLQuery(ref conn, "insert into  mss_link_" + _CircleName + "  ( select distinct * from mss_link_" + _CircleName + "_" + group_name + ")");
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("Inser_data_into_final_table()", ErrorMsg, "", ref FileError);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        public static void manage_backup(string circle)
        {
            string query = string.Empty;

            ////  for MSS MGW COMMENT THIS WHOLE FUNCTION 
            //query = "delete from association_info_bck_" + circle ;
            //ExecuteSQLQuery(ref conn, query);
            //query = "insert into association_info_bck_" + circle + " SELECT * FROM association_info_" + circle;
            //ExecuteSQLQuery(ref conn, query);
            //query = "delete from association_info_" + circle ;
            //ExecuteSQLQuery(ref conn, query);

            //////query = "delete from mss_mgw_map_bck_kol";
            //////ExecuteSQLQuery(ref conn, query);
            //////query = "insert into mss_mgw_map_bck_kol  SELECT * FROM mss_mgw_map_kol where circle = '" + circle + "'";
            //////ExecuteSQLQuery(ref conn, query);



            ////// change here pramod for hlr_msc part and comment above lines

            //query = "delete from destination_info_bck_" + circle ;
            //ExecuteSQLQuery(ref conn, query);
            //query = "insert into destination_info_bck_" + circle + " SELECT * FROM destination_info_" + circle;
            //ExecuteSQLQuery(ref conn, query);
            //query = "delete from destination_info_" + circle ;

            //query = "delete from hlr_bck_" + circle ;
            //ExecuteSQLQuery(ref conn, query);
            //query = "insert into hlr_bck_" + circle + "  SELECT * FROM hlr_" + circle ;
            //ExecuteSQLQuery(ref conn, query);
            //query = "delete from hlr_" + circle ;


            //query = "delete from msc_bck_" + circle;
            //ExecuteSQLQuery(ref conn, query);
            //query = "insert into msc_bck_" + circle + "  SELECT * FROM msc_" + circle;
            //ExecuteSQLQuery(ref conn, query);
            //query = "delete from msc_" + circle;


        }
        // check for missing

        /// <summary>
       /// 
       /// </summary>
       /// <param name="cir"></param>
       /// <param name="GroupName"></param>
        public void missing_element_info(string cir, string GroupName)
        {
            try
            {
                string group_name1 = GroupName;
                string NotfoundTable = null;

                group_name1 = group_name1.Insert(group_name1.Length - 1, "_");

                if (group_name1.Contains("HLR") || group_name1.Contains("MSC"))
                {
                    group_name1 = "HLR_MSC";
                }

                bool updateflag = false;
                string element = string.Empty;
                string TYPE_NODE = string.Empty;
                string query1 = string.Empty;
                DateTime d1 = System.DateTime.Now.AddDays(-1);
                string CurrentDate = d1.ToString("yyyy-MM-dd");
                string query = "select `ELEMENT`, `NODE_TYPE` FROM dcm_node_failure_details where circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";
                DataTable dt = new DataTable();
                DataTable dt1 = null;
                SQLQuery(ref conn, ref dt, query);

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i <= dt.Rows.Count - 1; i++)
                    {
                        TYPE_NODE = dt.Rows[i][1].ToString().Trim();
                        element = dt.Rows[i][0].ToString().Trim();
                        NotfoundTable = "";
                        dt1 = null;

                        if ((TYPE_NODE == "MSS") || (TYPE_NODE == "GCS"))
                        {
                            // ............ FOR MSS MGW MAP..................
                            query1 = "Select ne_name FROM mss_mgw_map_bck_" + cir + " where ne_name = '" + element + "'";
                            SQLQuery(ref conn, ref dt1, query1);
                            if (dt1 != null)
                            {
                                if (dt1.Rows.Count > 0)
                                {
                                    dt1 = null;
                                    query1 = "Select ne_name FROM destination_info_bck_" + cir + " where ne_name = '" + element + "'";
                                    SQLQuery(ref conn, ref dt1, query1);
                                    if (dt1 != null)
                                    {
                                        if (dt1.Rows.Count > 0)
                                        {
                                            dt1 = null;
                                            query1 = "Select node FROM association_info_bck_" + cir + " where node = '" + element + "'";
                                            SQLQuery(ref conn, ref dt1, query1);
                                            if (dt1 != null)
                                            {
                                                if (dt1.Rows.Count > 0)
                                                {
                                                    try
                                                    {
                                                        if (!("mss_mgw_map_" + cir + "_" + group_name1).Contains("HLR_MSC"))
                                                        {
                                                            query1 = "delete FROM mss_mgw_map_" + cir + "_" + group_name1 + " where ne_name = '" + element + "'";
                                                            ExecuteSQLQuery(ref conn, query1);

                                                            query1 = "insert into mss_mgw_map_" + cir + " select * from mss_mgw_map_bck_" + cir + " where ne_name = '" + element + "'";
                                                            ExecuteSQLQuery(ref conn, query1);

                                                        }
                                                    }
                                                    catch { }


                                                    query1 = "delete FROM destination_info_" + cir + " where ne_name = '" + element + "'";
                                                    ExecuteSQLQuery(ref conn, query1);

                                                    query1 = "insert into destination_info_" + cir + " select * from destination_info_bck_" + cir + " where ne_name = '" + element + "'";
                                                    ExecuteSQLQuery(ref conn, query1);

                                                    query1 = "delete FROM association_info_" + cir + " where node = '" + element + "'";
                                                    ExecuteSQLQuery(ref conn, query1);

                                                    query1 = "insert into association_info_" + cir + " select * from association_info_bck_" + cir + " where node = '" + element + "'";
                                                    ExecuteSQLQuery(ref conn, query1);

                                                    updateflag = true;

                                                }
                                                else
                                                {
                                                    NotfoundTable = "association_info_bck";
                                                }
                                            }
                                            else
                                            {
                                                NotfoundTable = "association_info_bck";
                                            }
                                        }
                                        else
                                        {
                                            NotfoundTable = "destination_info_bck";
                                        }
                                    }
                                    else
                                    {
                                        NotfoundTable = "destination_info_bck";
                                    }
                                }
                                else
                                {
                                    NotfoundTable = "mss_mgw_map_bck";
                                }

                            }
                            else
                            {
                                NotfoundTable = "mss_mgw_map_bck";
                            }

                            if (updateflag == true)
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Uploaded from Backup.' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";
                            }
                            else
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Element is not found in Backup Table:" + NotfoundTable + "' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";

                            }
                            ExecuteSQLQuery(ref conn, query1);
                            updateflag = false;

                        }

                        if (TYPE_NODE == "MGW")
                        {
                            // ............ FOR MSS MGW MAP..................
                            query1 = "Select ne_name FROM mss_mgw_map_bck_" + cir + " where MGW_Name = '" + element + "'";
                            SQLQuery(ref conn, ref dt1, query1);
                            if (dt1 != null)
                            {
                                if (dt1.Rows.Count > 0)
                                {
                                    dt1 = null;
                                    //query1 = "Select ne_name FROM destination_info_bck_" + cir + " where ne_name = '" + element + "'";
                                    //SQLQuery(ref conn, ref dt1, query1);
                                    //if (dt1 != null)
                                    //{
                                    //    if (dt1.Rows.Count > 0)
                                    //    {
                                    //        dt1 = null;
                                    query1 = "Select Node FROM association_info_bck_" + cir + " where node = '" + element + "'";
                                    SQLQuery(ref conn, ref dt1, query1);
                                    if (dt1 != null)
                                    {
                                        if (dt1.Rows.Count > 0)
                                        {
                                            try
                                            {
                                                if (!("mss_mgw_map_" + cir + "_" + group_name1).Contains("HLR_MSC"))
                                                {
                                                    query1 = "delete FROM mss_mgw_map_" + cir + "_" + group_name1 + " where ne_name = '" + element + "'";
                                                    ExecuteSQLQuery(ref conn, query1);

                                                    query1 = "insert into mss_mgw_map_" + cir + " select * from mss_mgw_map_bck_" + cir + " where ne_name = '" + element + "'";
                                                    ExecuteSQLQuery(ref conn, query1);

                                                }
                                            }
                                            catch { }

                                            query1 = "delete FROM destination_info_" + cir + " where ne_name = '" + element + "'";
                                            ExecuteSQLQuery(ref conn, query1);

                                            query1 = "insert into destination_info_" + cir + " select * from destination_info_bck_" + cir + " where ne_name = '" + element + "'";
                                            ExecuteSQLQuery(ref conn, query1);

                                            query1 = "delete FROM association_info_" + cir + " where node = '" + element + "'";
                                            ExecuteSQLQuery(ref conn, query1);

                                            query1 = "insert into association_info_" + cir + " select * from association_info_bck_" + cir + " where node = '" + element + "'";
                                            ExecuteSQLQuery(ref conn, query1);

                                            updateflag = true;
                                        }
                                        {
                                            NotfoundTable = "association_info_bck";
                                        }
                                    }
                                    {
                                        NotfoundTable = "association_info_bck";
                                    }
                                    //    }
                                    //    else
                                    //    {
                                    //        NotfoundTable = "destination_info_bck";
                                    //    }

                                    //}
                                    //else
                                    //{
                                    //    NotfoundTable = "destination_info_bck";
                                    //}
                                }
                                else
                                {
                                    NotfoundTable = "mss_mgw_map_bck";
                                }
                            }
                            else
                            {
                                NotfoundTable = "mss_mgw_map_bck";
                            }

                            if (updateflag == true)
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Uploaded from Backup.' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";
                            }
                            else
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Element is not found in Backup Table:" + NotfoundTable + "' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";

                            }
                            ExecuteSQLQuery(ref conn, query1);
                            updateflag = false;

                        }


                        // change here pramod for hlr_msc part and comment above lines

                        if (TYPE_NODE == "MSC")
                        {

                            query1 = "Select distinct ne_name FROM msc_bck_" + cir + " where ne_name = '" + element + "'";
                            SQLQuery(ref conn, ref dt1, query1);
                            if (dt1 != null)
                            {
                                if (dt1.Rows.Count > 0)
                                {
                                    dt1 = null;
                                    //query1 = "Select ne_name FROM destination_info_bck_" + cir + " where ne_name = '" + element + "'";
                                    //SQLQuery(ref conn, ref dt1, query1);
                                    //if (dt1 != null)
                                    //{
                                    //    if (dt1.Rows.Count > 0)
                                    //    {
                                    //        dt1 = null;
                                    //        query1 = "Select node FROM association_info_bck_" + cir + " where node = '" + element + "'";
                                    //        SQLQuery(ref conn, ref dt1, query1);
                                    //        if (dt1 != null)
                                    //        {
                                    //            if (dt1.Rows.Count > 0)
                                    //            {
                                    query1 = "delete FROM msc_" + cir + " where ne_name = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    query1 = "insert into msc_" + cir + " select * from msc_bck_" + cir + " where ne_name = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    query1 = "delete FROM destination_info_" + cir + " where ne_name = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    query1 = "insert into destination_info_" + cir + " select * from destination_info_bck_" + cir + " where ne_name = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    query1 = "delete FROM association_info_" + cir + " where node = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    query1 = "insert into association_info_" + cir + " select * from association_info_bck_" + cir + " where node = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    updateflag = true;
                                    //            }
                                    //            else
                                    //            {
                                    //                NotfoundTable = "association_info_bck";
                                    //            }
                                    //        }
                                    //        else
                                    //        {
                                    //            NotfoundTable = "association_info_bck";
                                    //        }

                                    //    }
                                    //    else
                                    //    {
                                    //        NotfoundTable = "destination_info_bck";
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    NotfoundTable = "destination_info_bck";
                                    //}
                                }
                                else
                                {
                                    NotfoundTable = "msc_bck";
                                }
                            }
                            else
                            {
                                NotfoundTable = "msc_bck";
                            }

                            if (updateflag == true)
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Uploaded from Backup.' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";
                            }
                            else
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Element is not found in Backup Table:" + NotfoundTable + "' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";

                            }
                            ExecuteSQLQuery(ref conn, query1);
                            updateflag = false;
                        }


                        if (TYPE_NODE == "HLR")
                        {
                            query1 = "Select ne_name FROM hlr_bck_" + cir + " where ne_name = '" + element + "'";
                            SQLQuery(ref conn, ref dt1, query1);
                            if (dt1 != null)
                            {
                                if (dt1.Rows.Count > 0)
                                {

                                    query1 = "delete FROM hlr_" + cir + " where ne_name = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    query1 = "insert into hlr_" + cir + " select * from hlr_bck_" + cir + " where ne_name = '" + element + "'";
                                    ExecuteSQLQuery(ref conn, query1);

                                    updateflag = true;
                                }
                                else
                                {
                                    NotfoundTable = "hlr_bck";
                                }
                            }
                            else
                            {
                                NotfoundTable = "hlr_bck";
                            }
                            if (updateflag == true)
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Uploaded from Backup.' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";
                            }
                            else
                            {
                                query1 = "Update dcm_node_failure_details Set Status='Element is not found in Backup Table:" + NotfoundTable + "' where Element='" + element + "' and circle = '" + cir + "' and Circle_Group='" + group_name1 + "' and Time_Stamp like '" + CurrentDate + "%'";

                            }
                            ExecuteSQLQuery(ref conn, query1);
                            updateflag = false;
                        }


                    }

                    //query1 = "delete from dcm_node_failure_details where circle = '" + cir + "' and Group='" + GroupName + "' and Time_Stamp like '" + CurrentDate + "%'";
                    //ExecuteSQLQuery(ref conn, query1);
                }

            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("missing_element_info()", ErrorMsg, "", ref FileError);
            }
        }

        // for ATCA (MGW) Parsing
        #region[ATCA (MGW) Parsing Functions]
        //public void ParseData_VMGW_MGW(string output, string _CircleName, string NE_Type)
        //{
        //    //Console.WriteLine("ParseData_VMGW_MGW");


        //    string[] temp;
        //    string[] temp1;
        //    string t = string.Empty;
        //    string[] columnName = { "MGW_NAME", "MGW_VMGW_CTRL_ISU", "MGW_VMGW_ID", "MGW_VMGW", "MGW_SOURCE_MAP_IP", "CGR_GROUP" };
        //    DataTable dt = CreateDataTable(columnName);
        //    try
        //    {
        //        if (output != null)
        //        {
        //            temp = Regex.Split(output.Trim(), "\r\n");
        //            t = temp[2].Split('@')[0];
        //            string MGW_NAME = t.Substring(0, t.IndexOf(' '));
        //            //string MGW_NAME = "";
        //            string MGW_VMGW_CTRL_ISU = "";
        //            string MGW_VMGW_ID = "";
        //            string MGW_VMGW = "";
        //            string MGW_SOURCE_MAP_IP = "";
        //            string CGR_GROUP = "";

        //            for (int i = 1; i < temp.Length; i++)
        //            {
        //                if (temp[i].Contains(':'))
        //                {
        //                    temp1 = Regex.Split(temp[i], ":");

        //                    if (temp1[0].Contains("SISU rg(s)") || temp1[0].Contains("--More--SISU rg(s)"))
        //                    {
        //                        MGW_VMGW_CTRL_ISU = temp1[1];
        //                    }
        //                    else if (temp1[0].Contains("virtual media gateway identifier") || temp1[0].Contains("--More--virtual media gateway identifier"))
        //                    {
        //                        MGW_VMGW_ID = temp1[1];
        //                    }
        //                    else if (temp1[0].Contains("virtual media gateway name") || temp1[0].Contains("--More--virtual media gateway name"))
        //                    {
        //                        MGW_VMGW = temp1[1];
        //                    }
        //                    else if (temp1[0].Contains("own primary IP address") || temp1[0].Contains("--More--own primary IP address"))
        //                    {
        //                        MGW_SOURCE_MAP_IP = temp1[1];
        //                    }
        //                    else if (temp1[0].Contains("circuit group") || temp1[0].Contains("--More--circuit group"))
        //                    {
        //                        CGR_GROUP = temp1[1];
        //                    }



        //                    //MGW_VMGW_CTRL_ISU = (temp1[0].Contains("SISU rg(s)") || temp1[0].Contains("--More--SISU rg(s)") ? temp1[1] : "");
        //                    //MGW_VMGW_ID = (temp1[0].Contains("virtual media gateway identifier") || temp1[0].Contains("--More--virtual media gateway identifier") ? temp1[1] : "");
        //                    //MGW_VMGW = (temp1[0].Contains("virtual media gateway name") || temp1[0].Contains("--More--virtual media gateway name") ? temp1[1] : "");
        //                    //MGW_SOURCE_MAP_IP = (temp1[0].Contains("own primary IP address") || temp1[0].Contains("--More--own primary IP address") ? temp1[1] : "");

        //                    if (MGW_VMGW_CTRL_ISU != "" && MGW_VMGW_ID != "" && MGW_VMGW != "" && MGW_SOURCE_MAP_IP != "" && CGR_GROUP != "")
        //                    {
        //                        dt.Rows.Add(MGW_NAME, MGW_VMGW_CTRL_ISU, MGW_VMGW_ID, MGW_VMGW, MGW_SOURCE_MAP_IP, CGR_GROUP);
        //                        MGW_VMGW_CTRL_ISU = MGW_VMGW_ID = MGW_VMGW = MGW_SOURCE_MAP_IP = CGR_GROUP = "";
        //                    }
        //                }
        //            }

        //            foreach (DataRow dr in dt.Rows)
        //            {

        //                // From ZJVI
        //                //string SqlStr = "Update mss_mgw_map_wb set MGW_Name='" + _ne_name.Trim() + "',MGW_Source_Map_IP='" + MGW_SOURCEIP.Trim() + "' Where VMGW_DEST_IP='" + MGW_SOURCEIP.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
        //                //ExecuteSQLQuery(ref conn, SqlStr);

        //                //from ZJVI_2
        //                //query = "update mss_mgw_map_wb set MGW_VMGW = '" + VMGW_NAME + "' ,  MGW_VMGW_CTRL_ISU = '" + VMGW_CTRL_ISU + "' where VMGW_DEST_IP = '" + own_address + "' AND circle = '" + _CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
        //                //query2 = "update mss_mgw_map_wb set MGW_VMGW_ID = '" + VMGW_ID + "' where VMGW_DEST_IP = '" + own_address + "' And CGR in (" + CGR_GROUP + ") AND circle = '" + _CircleName + "' AND MGW_NAME = '" + _ne_name + "' ";


        //                string SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name +
        //                                        " set MGW_Name='" + Convert.ToString(dr["MGW_NAME"]).Trim() + "', " +
        //                                        " MGW_Source_Map_IP='" + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]).Trim() + "', " +
        //                                        " MGW_VMGW_CTRL_ISU ='" + Convert.ToString(dr["MGW_VMGW_CTRL_ISU"]) + "', " +
        //                                        " MGW_VMGW ='" + Convert.ToString(dr["MGW_VMGW"]) + "', ATCA_FLAG=1" +
        //                                " Where VMGW_DEST_IP='" + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]).Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
        //                ExecuteSQLQuery(ref conn, SqlStr);



        //                //string[] cgrGroup = Convert.ToString(dr["CGR_GROUP"]).Split(',');                        
        //                //for (int i=0 ;i< cgrGroup.Length;i++)
        //                //{
        //                //    cgrGroup[i] = cgrGroup[i].Trim(); // for removing extra space fro each string.
        //                //}
        //                if (Convert.ToString(dr["CGR_GROUP"]).Trim() != "-")
        //                {
        //                    string cgr = Convert.ToString(dr["CGR_GROUP"]).Trim().TrimEnd(',');
        //                    SqlStr = "update mss_mgw_map_" + _CircleName + "_" + group_name +
        //                                    " set MGW_VMGW_ID = '" + Convert.ToString(dr["MGW_VMGW_ID"]).Trim() + "' " +
        //                              " where VMGW_DEST_IP = '" + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]).Trim() + "' " +
        //                                     " And MGW_CGR in (" + cgr + ") AND circle = '" + _CircleName + "' AND MGW_NAME = '" + _ne_name + "' ";
        //                    ExecuteSQLQuery(ref conn, SqlStr);
        //                }

        //                //Console.WriteLine("virtual media gateway name  : " + Convert.ToString(dr["MGW_NAME"]));
        //                //Console.WriteLine("SISU rg(s)  : " + Convert.ToString(dr["MGW_VMGW_CTRL_ISU"]));
        //                //Console.WriteLine("virtual media gateway identifier  : " + Convert.ToString(dr["MGW_VMGW_ID"]));
        //                //Console.WriteLine("virtual media gateway name  : " + Convert.ToString(dr["MGW_VMGW"]));
        //                //Console.WriteLine("own primary IP address  : " + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]));
        //                //Console.ReadLine();
        //            }
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
        //        //if (!Directory.Exists(parsing_error_path))
        //        //{
        //        //    Directory.CreateDirectory(parsing_error_path);
        //        //}

        //        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_VMGW_MGW ");
        //        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message.ToString());
        //        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
        //        oExceptionLog.WriteExceptionErrorToFile("ParseData_VMGW_MGW()", ErrorMsg, "", ref FileError);

        //        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
        //        ExecuteSQLQuery(ref conn, ErrorQuery);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_VMGW_MGW(string output, string _CircleName, string NE_Type)
        {
            //Console.WriteLine("ParseData_VMGW_MGW");
            string[] temp;
            string[] temp1;
            string t = string.Empty;
            string[] columnName = { "MGW_NAME", "MGW_VMGW_CTRL_ISU", "MGW_VMGW_ID", "MGW_VMGW", "MGW_SOURCE_MAP_IP", "CGR_GROUP", "HCLBRG" };
            DataTable dt = CreateDataTable(columnName);
            try
            {
                if (output != null)
                {
                    temp = Regex.Split(output.Trim(), "\r\n");
                    t = temp[1].Split('@')[1];
                    string MGW_NAME = t.Substring(0, t.IndexOf(' '));
                    string MGW_VMGW_CTRL_ISU = "";
                    string MGW_VMGW_ID = "";
                    string MGW_VMGW = "";
                    string MGW_SOURCE_MAP_IP = "";
                    string CGR_GROUP = "";
                    string HCLBRG = "";

                    for (int i = 1; i < temp.Length; i++)
                    {
                        if (temp[i].Contains(':'))
                        {
                            temp1 = Regex.Split(temp[i], ":");

                            if (temp1[0].Contains("SISU rg(s)") || temp1[0].Contains("--More--SISU rg(s)"))
                            {
                                MGW_VMGW_CTRL_ISU = temp1[1];
                            }
                            else if (temp1[0].Contains("virtual media gateway identifier") || temp1[0].Contains("--More--virtual media gateway identifier"))
                            {
                                MGW_VMGW_ID = temp1[1];
                            }
                            else if (temp1[0].Contains("virtual media gateway name") || temp1[0].Contains("--More--virtual media gateway name"))
                            {
                                MGW_VMGW = temp1[1];
                            }
                            else if (temp1[0].Contains("own primary IP address") || temp1[0].Contains("--More--own primary IP address"))
                            {
                                MGW_SOURCE_MAP_IP = temp1[1];
                            }
                            else if (temp1[0].Contains("circuit group") || temp1[0].Contains("--More--circuit group"))
                            {
                                CGR_GROUP = temp1[1];
                            }
                            else if (temp1[0].Contains("HCLB rg") || temp1[0].Contains("--More--HCLB rg"))
                            {
                                HCLBRG = temp1[1];
                            }

                            //MGW_VMGW_CTRL_ISU = (temp1[0].Contains("SISU rg(s)") || temp1[0].Contains("--More--SISU rg(s)") ? temp1[1] : "");
                            //MGW_VMGW_ID = (temp1[0].Contains("virtual media gateway identifier") || temp1[0].Contains("--More--virtual media gateway identifier") ? temp1[1] : "");
                            //MGW_VMGW = (temp1[0].Contains("virtual media gateway name") || temp1[0].Contains("--More--virtual media gateway name") ? temp1[1] : "");
                            //MGW_SOURCE_MAP_IP = (temp1[0].Contains("own primary IP address") || temp1[0].Contains("--More--own primary IP address") ? temp1[1] : "");

                            if (MGW_VMGW_CTRL_ISU != "" && MGW_VMGW_ID != "" && MGW_VMGW != "" && MGW_SOURCE_MAP_IP != "" && CGR_GROUP != "" && HCLBRG != "")
                            {
                                dt.Rows.Add(MGW_NAME, MGW_VMGW_CTRL_ISU, MGW_VMGW_ID, MGW_VMGW, MGW_SOURCE_MAP_IP, CGR_GROUP, HCLBRG);
                                HCLBRG = MGW_VMGW_CTRL_ISU = MGW_VMGW_ID = MGW_VMGW = MGW_SOURCE_MAP_IP = CGR_GROUP = "";
                            }
                        }
                    }

                    foreach (DataRow dr in dt.Rows)
                    {

                        // From ZJVI
                        //string SqlStr = "Update mss_mgw_map_wb set MGW_Name='" + _ne_name.Trim() + "',MGW_Source_Map_IP='" + MGW_SOURCEIP.Trim() + "' Where VMGW_DEST_IP='" + MGW_SOURCEIP.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                        //ExecuteSQLQuery(ref conn, SqlStr);

                        //from ZJVI_2
                        //query = "update mss_mgw_map_wb set MGW_VMGW = '" + VMGW_NAME + "' ,  MGW_VMGW_CTRL_ISU = '" + VMGW_CTRL_ISU + "' where VMGW_DEST_IP = '" + own_address + "' AND circle = '" + _CircleName + "' and MGW_NAME = '" + _ne_name + "' ";
                        //query2 = "update mss_mgw_map_wb set MGW_VMGW_ID = '" + VMGW_ID + "' where VMGW_DEST_IP = '" + own_address + "' And CGR in (" + CGR_GROUP + ") AND circle = '" + _CircleName + "' AND MGW_NAME = '" + _ne_name + "' ";


                        string SqlStr = "update mss_mgw_map_" + _CircleName + "_" + group_name +
                                                " set MGW_Name='" + Convert.ToString(dr["MGW_NAME"]).Trim() + "', " +
                                                " MGW_Source_Map_IP='" + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]).Trim() + "', " +
                                                " MGW_VMGW_CTRL_ISU ='" + Convert.ToString(dr["MGW_VMGW_CTRL_ISU"]) + "', " +
                                                " MGW_VMGW ='" + Convert.ToString(dr["MGW_VMGW"]) + "', " +
                                                " HCLBRG ='" + Convert.ToString(dr["HCLBRG"]) + "'," +
                                                " ATCA_FLAG = 1 " +
                                        " Where VMGW_DEST_IP='" + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]).Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                        ExecuteSQLQuery(ref conn, SqlStr);



                        //string[] cgrGroup = Convert.ToString(dr["CGR_GROUP"]).Split(',');                        
                        //for (int i=0 ;i< cgrGroup.Length;i++)
                        //{
                        //    cgrGroup[i] = cgrGroup[i].Trim(); // for removing extra space fro each string.
                        //}
                        if (Convert.ToString(dr["CGR_GROUP"]).Trim() != "-")
                        {
                            string cgr = Convert.ToString(dr["CGR_GROUP"]).Trim().TrimEnd(',');
                            SqlStr = "update mss_mgw_map_" + _CircleName + "_" + group_name +
                                            " set MGW_VMGW_ID = '" + Convert.ToString(dr["MGW_VMGW_ID"]).Trim() + "' " +
                                      " where VMGW_DEST_IP = '" + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]).Trim() + "' " +
                                             " And CGR in (" + cgr + ") AND circle = '" + _CircleName + "' AND MGW_NAME = '" + _ne_name + "' ";
                            ExecuteSQLQuery(ref conn, SqlStr);
                        }

                        //Console.WriteLine("virtual media gateway name  : " + Convert.ToString(dr["MGW_NAME"]));
                        //Console.WriteLine("SISU rg(s)  : " + Convert.ToString(dr["MGW_VMGW_CTRL_ISU"]));
                        //Console.WriteLine("virtual media gateway identifier  : " + Convert.ToString(dr["MGW_VMGW_ID"]));
                        //Console.WriteLine("virtual media gateway name  : " + Convert.ToString(dr["MGW_VMGW"]));
                        //Console.WriteLine("own primary IP address  : " + Convert.ToString(dr["MGW_SOURCE_MAP_IP"]));
                        //Console.ReadLine();
                    }
                }
            }

            catch (Exception ex)
            {
                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                //if (!Directory.Exists(parsing_error_path))
                //{
                //    Directory.CreateDirectory(parsing_error_path);
                //}

                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_VMGW_MGW ");
                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_VMGW_MGW()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_TDM_ATER(string output, string _CircleName, string NE_Type)
        {
            //Console.WriteLine("ParseData_TDM_ATER");
            string[] temp;
            string[] temp1;
            string[] columnName = { "APCM", "ET" };
            DataTable dt = CreateDataTable(columnName);
            try
            {
                if (output != null)
                {
                    temp = Regex.Split(output.Trim(), "COMMAND EXECUTED"); // means where command executed is done in the output file.
                    temp = Regex.Split(output.Trim(), "\r\n");

                    for (int i = 1; i < temp.Length; i++)
                    {
                        temp1 = Regex.Split(temp[i], " ");
                        try
                        {
                            string t = Regex.Split(temp1[0], "--More--\r").Length > 1 ? Regex.Split(temp1[0], "--More--\r")[1] : temp1[0];
                            int apcm = Convert.ToInt32(t);
                            int et = Convert.ToInt32(temp1[5]);
                            dt.Rows.Add(apcm, et);
                        }
                        catch (Exception ex)
                        {
                            ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                            oExceptionLog.WriteExceptionErrorToFile("ParseData_TDM_ATER_Sub()", ErrorMsg, "", ref FileError);
                        }
                    }
                }

                // Old ZR2O
                //query = "Update mss_mgw_map_wb set ET=Case when (Length(Term_id)<=4) Then Term_id End,APCM=Case when (Length(Term_id)>4) Then Term_id End where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME ='" + _ne_name.Trim() + "'";                
                string SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name + " set ET=Case when (Length(Term_id)<=4) Then Term_id End,APCM=Case when (Length(Term_id)>4) Then Term_id End where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME ='" + _ne_name.Trim() + "'";
                ExecuteSQLQuery(ref conn, SqlStr);

                foreach (DataRow dr in dt.Rows)
                {

                    //Old ZR2O et Update
                    // query = "update mss_mgw_map_wb set et = '" + et + "' where apcm = '" + apcm.Trim() + "' and  Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "'";
                    SqlStr = "update mss_mgw_map_" + _CircleName + "_" + group_name + " set et = '" + Convert.ToString(dr["ET"]) + "'" +
                                     " where apcm = '" + Convert.ToString(dr["APCM"]) + "' and  Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "'";
                    ExecuteSQLQuery(ref conn, SqlStr);

                    //SqlStr = "Update mss_mgw_map_" + _CircleName +
                    //                        " set ET=" + Convert.ToString(dr["ET"]) + ", " +
                    //                        " APCM=" + Convert.ToString(dr["APCM"]) + ", ATCA_FLAG=1 " +
                    //                " where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME ='" + _ne_name.Trim() + "'";

                    //  ExecuteSQLQuery(ref conn, SqlStr);

                    //Console.WriteLine("APCM  : " + Convert.ToString(dr["APCM"]));
                    //Console.WriteLine("ET  : " + Convert.ToString(dr["ET"]));
                    //Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                //if (!Directory.Exists(parsing_error_path))
                //{
                //    Directory.CreateDirectory(parsing_error_path);
                //}

                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_TDM_ATER ");
                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_TDM_ATER()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_LICENSE_TARGET(string output, string _CircleName, string NE_Type)
        {
            //Console.WriteLine("ParseData_LICENSE_TARGET");

            string[] temp;
            string[] temp1;
            string[] columnName = { "MGW_C_NO" };
            DataTable dt = CreateDataTable(columnName);
            try
            {
                if (output != null)
                {
                    temp = Regex.Split(output.Trim(), "\r\n");
                    string MGW_C_NO = "";
                    for (int i = 1; i < temp.Length; i++)
                    {
                        if (temp[i].Contains(':'))
                        {
                            temp1 = Regex.Split(temp[i], ":");

                            if (temp1[0].Contains("Target ID") || temp1[0].Contains("--More--Target ID"))
                            {
                                MGW_C_NO = temp1[1];
                            }

                            if (MGW_C_NO != "")
                            {
                                dt.Rows.Add(MGW_C_NO);
                                MGW_C_NO = "";
                            }
                        }
                    }
                }

                foreach (DataRow dr in dt.Rows)
                {
                    // Old for ZW7N

                    //SqlStr = "Update mss_mgw_map_wb set MGW_C_NO='" + MGW_C_NO.Trim() + "' Where MGW_Name='" + _ne_name.Trim() + "' and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' ";

                    string SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name +
                                            "  set MGW_C_NO='" + Convert.ToString(dr["MGW_C_NO"]).Trim() + "', ATCA_FLAG=1 " +
                                    " Where MGW_Name='" + _ne_name.Trim() + "' and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' ";
                    ExecuteSQLQuery(ref conn, SqlStr);

                    //Console.WriteLine("circuit group name  : " + Convert.ToString(dr["MGW_C_NO"]));
                    //Console.ReadLine();
                }
            }

            catch (Exception ex)
            {
                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                //if (!Directory.Exists(parsing_error_path))
                //{
                //    Directory.CreateDirectory(parsing_error_path);
                //}

                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_LICENSE_TARGET ");
                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_LICENSE_TARGET()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_SIGNALING_SS7_OWN(string output, string _CircleName, string NE_Type)
        {
            //Console.WriteLine("ParseData_SIGNALING_SS7_OWN");


            string[] temp;
            string[] temp1;
            string[] columnName = { "MGW_SPC", "SAP_NAME" };
            DataTable dt = CreateDataTable(columnName);
            try
            {
                if (output != null)
                {
                    temp = Regex.Split(output.Trim(), "\r\n");
                    string MGW_SPC = ""; // for code
                    string SAP_NAME = ""; // for name
                    for (int i = 1; i < temp.Length; i++)
                    {
                        if (temp[i].Contains(':'))
                        {
                            temp1 = Regex.Split(temp[i], ":");

                            if (temp1[0].Contains("sap-name") || temp1[0].Contains("--More--sap-name"))
                            {
                                SAP_NAME = temp1[1];
                            }

                            if (temp1[0].Contains("self-point-code") || temp1[0].Contains("--More--self-point-code"))
                            {
                                MGW_SPC = temp1[1];
                            }

                            if (SAP_NAME != "" && MGW_SPC != "")
                            {

                                dt.Rows.Add(MGW_SPC, SAP_NAME);
                                MGW_SPC = SAP_NAME = "";

                            }
                        }
                    }
                }

                foreach (DataRow dr in dt.Rows)
                {
                    // Old for ZNRI
                    //SqlStr = "Update mss_mgw_map_wb set MGW_SPC='" + MGW_SPC.Trim() + "' Where CGR_NET='NA0' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";

                    string SqlStr = string.Empty;
                    bool done = false;
                    if (Convert.ToString(dr["SAP_NAME"]).Trim().ToUpper() == "SAPNA0")
                    {
                        SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name +
                                        " set MGW_SPC='" + Convert.ToString(dr["MGW_SPC"]).Trim() + "', ATCA_FLAG=1 " +
                                 " Where CGR_NET='NA0' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                        done = ExecuteSQLQuery(ref conn, SqlStr);
                    }
                    else if (Convert.ToString(dr["SAP_NAME"]).Trim().ToUpper() == "SAPNA1")
                    {
                        SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name +
                                        " set MGW_SPC='" + Convert.ToString(dr["MGW_SPC"]).Trim() + "', ATCA_FLAG=1 " +
                                 " Where CGR_NET='NA1' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                        done = ExecuteSQLQuery(ref conn, SqlStr);
                    }

                    //Console.WriteLine("SAP Name  : " + Convert.ToString(dr["SAP_NAME"]));
                    //Console.WriteLine("Self Point Code  : " + Convert.ToString(dr["MGW_SPC"]));
                    //Console.WriteLine("Done  : " + done);
                    //Console.ReadLine();
                }
            }

            catch (Exception ex)
            {
                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                //if (!Directory.Exists(parsing_error_path))
                //{
                //    Directory.CreateDirectory(parsing_error_path);
                //}

                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_SIGNALING_SS7_OWN ");
                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_SIGNALING_SS7_OWN()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_SIGNALING_SS7_ROUTE(string output, string _CircleName, string NE_Type)
        {
            string[] temp;
            string[] temp1;
            DataTable dt = null;
            string routeId = string.Empty;
            string routeName = string.Empty;
            int cnt;
            string SqlStr = string.Empty;
            try
            {
                output = output.Replace("--More--", "");
                temp = Regex.Split(output.Trim(), "\r\n");
                //SQLQuery(ref conn, ref dt, "select distinct * from mss_mgw_map_" + _CircleName + "_" + group_name + " where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and Node_Type='MGW_ATCA' order by LinkSet");
                for (int i = 1; i < temp.Length - 1; i++)
                {
                    if (temp[i].Contains("route-name"))
                    {
                        temp1 = null;
                        temp1 = Regex.Split(temp[i], " ");
                        cnt = 0;
                        for (int j = 0; j < temp1.Length; j++)
                        {
                            if (temp1[j].ToString().Trim() != "")
                            {
                                if (cnt == 2)
                                {
                                    routeName = temp1[j].ToString();
                                    break;
                                }
                                cnt = cnt + 1;
                            }
                        }

                    }
                    if (temp[i].Contains("route-id"))
                    {
                        temp1 = null;
                        temp1 = Regex.Split(temp[i], " ");
                        cnt = 0;
                        for (int j = 0; j < temp1.Length; j++)
                        {
                            if (temp1[j].ToString().Trim() != "")
                            {
                                if (cnt == 2)
                                {
                                    routeId = temp1[j].ToString();
                                    break;
                                }
                                cnt = cnt + 1;
                            }
                        }

                    }
                    if (routeId != "" && routeName != "")
                    {
                        //SQLQuery(ref conn, ref dt, "select distinct * from mss_mgw_map_" + _CircleName + "_" + group_name + " where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and Node_Type='MGW_ATCA' and LINKSET='" + routeName + "' and ROUTER_ID='" + routeId + "'");
                        SQLQuery(ref conn, ref dt, "select distinct * from mss_mgw_map_" + _CircleName + "_" + group_name + " where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and Node_Type='MGW_ATCA' and LINKSET='" + routeName + "'");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name + " Set ROUTER_ID='" + routeId.Trim() + "' Where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and Node_Type='MGW_ATCA' and LINKSET='" + routeName + "'";
                                ExecuteSQLQuery(ref conn, SqlStr);

                            }
                            else
                            {
                                SqlStr = "Insert into mss_mgw_map_" + _CircleName + "_" + group_name + "(VENDOR,CIRCLE,NODE_TYPE,MGW_NAME,ROUTER_ID,LINKSET,ATCA_FLAG) Values('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType.Trim() + "','" + _ne_name.Trim() + "','" + routeId.Trim() + "','" + routeName.Trim() + "','1')";
                                ExecuteSQLQuery(ref conn, SqlStr);
                            }
                        }
                        else
                        {
                            SqlStr = "Insert into mss_mgw_map_" + _CircleName + "_" + group_name + "(VENDOR,CIRCLE,NODE_TYPE,MGW_NAME,ROUTER_ID,LINKSET,ATCA_FLAG) Values('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType.Trim() + "','" + _ne_name.Trim() + "','" + routeId.Trim() + "','" + routeName.Trim() + "','1')";
                            ExecuteSQLQuery(ref conn, SqlStr);
                        }
                        routeId = "";
                        routeName = "";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_SIGNALING_SS7_ROUTE()", ErrorMsg, "", ref FileError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_TDM_CIRCUITGROUP(string output, string _CircleName, string NE_Type)
        {
            //Console.WriteLine("ParseData_TDM_CIRCUITGROUP");



            string[] temp;
            string[] temp1;
            string[] columnName = { "MGW_CGR", "MGW_NCGR", "PCM_TSL" };
            DataTable dt = CreateDataTable(columnName);
            try
            {
                if (output != null)
                {
                    temp = Regex.Split(output.Trim(), "\r\n");
                    string MGW_CGR = "";
                    string MGW_NCGR = "";
                    string PCM_TSL = "";

                    for (int i = 1; i < temp.Length - 1; i++)
                    {
                        if (temp[i].Contains(':'))
                        {
                            temp1 = Regex.Split(temp[i], ":");

                            if (temp1[0].Contains("NCGR") || temp1[0].Contains("--More--NCGR"))
                            {
                                MGW_NCGR = temp1[1].Split(' ')[0];
                                MGW_CGR = temp1[2];
                            }
                        }
                        if (MGW_NCGR != "" && MGW_CGR != "")
                        {
                            for (int k = i + 1; k < temp.Length - 1; k++)
                            {
                                temp1 = Regex.Split(temp[k], ":");

                                if (temp1[0].Contains("NCGR") || temp1[0].Contains("--More--NCGR"))
                                {
                                    i = k - 1;
                                    MGW_CGR = MGW_NCGR = PCM_TSL = "";
                                    break;
                                }

                                if (temp[k].Contains('-'))
                                {
                                    int j = 0;
                                    int space = 0;
                                    foreach (char ch in temp[k])
                                    {
                                        if (ch == '>' || ch == 'P') // for checking this line contains heading like ...(PCM-TSL  ORD  admin_state oper_state usage_state) or >>                                            
                                            break;
                                        if (ch == 32)   // for space
                                            space = j;

                                        j++;

                                        if (ch >= 47 && ch <= 57)
                                        {
                                            for (j = space + 1; j <= temp[k].Length - 1; j++)
                                            {
                                                if (temp[k][j] == ' ')
                                                {
                                                    j = 0;
                                                    break;
                                                }
                                                PCM_TSL = PCM_TSL + temp[k][j];
                                            }
                                        }
                                        if (j == 0)
                                            break;
                                    }
                                }
                                if (MGW_CGR != "" && MGW_NCGR != "" && PCM_TSL != "")
                                {
                                    dt.Rows.Add(MGW_CGR, MGW_NCGR, PCM_TSL);
                                    PCM_TSL = "";
                                }
                                i = k;
                            }
                        }
                    }
                }

                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {

                        string pcmTSL = Convert.ToString(Convert.ToInt32(Convert.ToString(dr["PCM_TSL"]).Split('-')[0].Trim()));
                        string SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name + " set MGW_CGR='" + Convert.ToString(dr["MGW_CGR"]).Trim() + "', MGW_NCGR= '" + Convert.ToString(dr["MGW_NCGR"]).Trim() + "' , atca_flag=1 Where (ET = '" + pcmTSL + "' or APCM='" + pcmTSL + "') and MGW_CGR is null and MGW_NAME = '" + _ne_name + "' and circle = '" + _CircleName + "' ";
                        bool done = ExecuteSQLQuery(ref conn, SqlStr);
                        //Console.WriteLine("MGW_CGR  : " + Convert.ToString(dr["MGW_CGR"] + " MGW_NCGR  : " + Convert.ToString(dr["MGW_NCGR"]) + " PCL-TSL : " + Convert.ToString(dr["PCM_TSL"]) + " Done : " + done));                    
                        //Console.WriteLine(" Done : " + done);
                        //Console.Read();
                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_TDM_CIRCUITGROUP_SUBwithContinue()", ErrorMsg, "", ref FileError);
                        continue;

                    }
                }
            }

            catch (Exception ex)
            {
                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                //if (!Directory.Exists(parsing_error_path))
                //{
                //    Directory.CreateDirectory(parsing_error_path);
                //}

                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_TDM_CIRCUITGROUP ");
                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_TDM_CIRCUITGROUP()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_SIGNALING_SS7_LINK(string output, string _CircleName, string NE_Type)
        {
            //This is common function for link all and linset all should be execute in sequence of commands like "LINK all","LINKSET all"
            string tempQuery = string.Empty;
            string[] temp;
            string[] temp1;
            string[] columnName = { "LINK_ID", "LINKSET_ID", "DESTINATION_POINT_CODE", "DESTINATION_POINT_CODE_NAME", "SLC", "PCM_ID", "LINK_SPC", "LINK_NODE", "LINK_ADMIN_STATE", "LINK_RATE", "LINK_STATUS" };
            DataTable dt = CreateDataTable(columnName);
            DataTable dt_PCM;
            try
            {
                if (output != null)
                {
                    temp = Regex.Split(output.Trim(), "\r\n");
                    string LINK_ID = "";
                    string LINKSET_ID = "";
                    string DESTINATION_POINT_CODE = "";
                    string DESTINATION_POINT_CODE_NAME = "";
                    string LINK_STATUS = "";
                    string LINK_ADMIN_STATE = string.Empty;
                    string LINK_NODE = string.Empty;
                    string LINK_SPC = string.Empty;
                    string LINK_RATE = string.Empty;
                    string SLC = "";
                    string PCM_ID = "";

                    for (int i = 1; i < temp.Length; i++)
                    {
                        if (temp[i].Contains(':'))
                        {
                            temp1 = Regex.Split(temp[i], ":");
                            if (temp1[0].Contains("link-id") || temp1[0].Contains("--More--link-id"))
                            {
                                LINK_ID = temp1[1].ToString().Trim();
                            }
                            if (temp1[0].Contains("destination-point-code-name") || temp1[0].Contains("--More--\rdestination-point-code-name"))
                            {
                                DESTINATION_POINT_CODE_NAME = temp1[1].ToString().Trim();
                            }
                            if ((temp1[0].ToString().Trim() == ("destination-point-code")) || (temp1[0].ToString().Trim() == ("--More--\rdestination-point-code")))
                            {
                                DESTINATION_POINT_CODE = temp1[1].ToString().Trim();
                            }
                            if ((temp1[0].ToString().Trim() == ("linkset-id")) || (temp1[0].ToString().Trim() == ("--More--\rlinkset-id")))
                            {
                                LINKSET_ID = temp1[1].ToString().Trim();
                            }
                            if (temp1[0].Contains("slc") || temp1[0].Contains("--More--slc"))
                            {
                                SLC = temp1[1].ToString().Trim();
                            }
                            if (temp1[0].Contains("PCM Id") || temp1[0].Contains("--More--PCM Id"))
                            {
                                PCM_ID = temp1[1].ToString().Trim();
                            }                                                       
                            if (temp1[0].Contains("self-point-code") || temp1[0].Contains("--More--\rself-point-code"))
                            {
                                LINK_SPC = temp1[1].ToString().Trim();
                            }
                            if (temp1[0].Contains("node") || temp1[0].Contains("--More--\rnode"))
                            {
                                LINK_NODE = temp1[1].ToString().Trim();
                            }
                            if (temp1[0].Contains("link-admin-state") || temp1[0].Contains("--More--\rlink-admin-state"))
                            {
                                LINK_ADMIN_STATE = temp1[1].ToString().Trim();
                            }
                            if (temp1[0].Contains("Link Rate") || temp1[0].Contains("--More--\rLink Rate"))
                            {
                                LINK_RATE = temp1[1].ToString().Trim();
                            }
                            if (temp1[0].Contains("link-status") || temp1[0].Contains("--More--\rlink-status"))
                            {
                                LINK_STATUS = temp1[1].ToString().Trim();
                            }
                            
                            if (LINK_ID != "" && DESTINATION_POINT_CODE_NAME != "")
                            {
                                #region new code for LINKSETALL
                                if (PCM_ID == "" && LINKSET_ID != "") //Cursor will enter only for LINKSET all command
                                {                                    
                                    try
                                    {
                                        string[] str = LINK_ID.Split(',');
                                        if (str.Length == 1)
                                        {
                                            if (LINKSET_ID == "")
                                            {
                                                LINKSET_ID = str[0].ToString().Trim();
                                            }
                                            dt.Rows.Add(str[0].ToString().Trim(), LINKSET_ID, DESTINATION_POINT_CODE, DESTINATION_POINT_CODE_NAME, SLC, PCM_ID, LINK_SPC, LINK_NODE, LINK_ADMIN_STATE, LINK_RATE, LINK_STATUS);
                                        }
                                        else
                                        {
                                            for (int k = 0; k < str.Length; k++)
                                            {
                                                if (LINKSET_ID == "")
                                                {
                                                    LINKSET_ID = str[k].ToString().Trim();
                                                }
                                                dt.Rows.Add(str[k].ToString().Trim(), LINKSET_ID, DESTINATION_POINT_CODE, DESTINATION_POINT_CODE_NAME, SLC, PCM_ID, LINK_SPC, LINK_NODE, LINK_ADMIN_STATE, LINK_RATE, LINK_STATUS);
                                            }
                                        }

                                        DESTINATION_POINT_CODE = LINK_ID = LINKSET_ID = SLC = PCM_ID = LINK_SPC = LINK_NODE = LINK_ADMIN_STATE = LINK_RATE = LINK_STATUS = "";
                                    }
                                    catch
                                    {

                                        
                                    }
                                }
                                #endregion

                                #region old code for LINKSETALL
                                //if (PCM_ID == "" && LINKSET_ID != "") //Cursor will enter only for LINKSET all command
                                //{
                                //    dt_PCM = new DataTable();
                                //    tempQuery = "Select distinct ET from mss_mgw_map_" + _CircleName + "_" + group_name + " where LINK ='" + LINKSET_ID + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' LIMIT 1";
                                //    SQLQuery(ref conn, ref dt_PCM, tempQuery);
                                //    try
                                //    {
                                //        PCM_ID = dt_PCM.Rows[0]["ET"].ToString();
                                //    }
                                //    catch
                                //    {

                                //        string[] str = LINK_ID.Split(',');
                                //        if (str.Length == 1)
                                //        {
                                //            if (LINKSET_ID == "")
                                //            {
                                //                LINKSET_ID = str[0].ToString().Trim();
                                //            }
                                //            dt.Rows.Add(str[0].ToString().Trim(), DESTINATION_POINT_CODE, LINKSET_ID, DESTINATION_POINT_CODE_NAME, SLC, PCM_ID, LINK_SPC, LINK_NODE, LINK_ADMIN_STATE, LINK_RATE, LINK_STATUS);
                                //        }
                                //        else
                                //        {
                                //            for (int k = 0; k < str.Length; k++)
                                //            {
                                //                if (LINKSET_ID == "")
                                //                {
                                //                    LINKSET_ID = str[k].ToString().Trim();
                                //                }
                                //                dt.Rows.Add(str[k].ToString().Trim(), DESTINATION_POINT_CODE, LINKSET_ID, DESTINATION_POINT_CODE_NAME, SLC, PCM_ID, LINK_SPC, LINK_NODE, LINK_ADMIN_STATE, LINK_RATE, LINK_STATUS);
                                //            }
                                //        }

                                //        DESTINATION_POINT_CODE = LINK_ID = LINKSET_ID = SLC = PCM_ID = LINK_SPC = LINK_NODE = LINK_ADMIN_STATE= LINK_RATE= LINK_STATUS = "";
                                //    }
                                //}
                                #endregion

                                #region code for LINKALL
                                //if (PCM_ID != "" && LINK_SPC != "" && LINK_ADMIN_STATE !="")
                                if (LINK_SPC != "" && LINK_ADMIN_STATE != "")
                                    {
                                    string[] str = LINK_ID.Split(',');
                                    if (str.Length == 1)
                                    {
                                        //if (LINKSET_ID == "")
                                        //{
                                          //  LINKSET_ID = str[0].ToString().Trim();
                                        //}
                                        dt.Rows.Add(str[0].ToString().Trim(), LINKSET_ID, DESTINATION_POINT_CODE, DESTINATION_POINT_CODE_NAME, SLC, PCM_ID, LINK_SPC, LINK_NODE, LINK_ADMIN_STATE, LINK_RATE, LINK_STATUS);
                                    }
                                    else
                                    {
                                        for (int k = 0; k < str.Length; k++)
                                        {
                                            //if (LINKSET_ID == "")
                                            //{
                                              //  LINKSET_ID = str[k].ToString().Trim();
                                           //}
                                            dt.Rows.Add(str[k].ToString().Trim(), LINKSET_ID, DESTINATION_POINT_CODE, DESTINATION_POINT_CODE_NAME, SLC, PCM_ID, LINK_SPC, LINK_NODE, LINK_ADMIN_STATE, LINK_RATE, LINK_STATUS);
                                        }
                                    }

                                    DESTINATION_POINT_CODE = LINK_ID = LINKSET_ID = SLC = PCM_ID = LINK_SPC = LINK_NODE = LINK_ADMIN_STATE= LINK_RATE= LINK_STATUS = "";
                                }
                                #endregion
                            }
                        }
                    }
                }
                DataTable dtet;
                DataTable tempdt;
                DataTable linkdt;
                foreach (DataRow dr in dt.Rows)
                {
                    // for mgw
                    tempdt = null;
                    linkdt = null;
                    dtet = null;

                    tempdt = new DataTable();
                    linkdt = new DataTable();
                    dtet = new DataTable();

                    SQLQuery(ref conn, ref dtet, "select distinct * from mss_mgw_map_" + _CircleName + "_" + group_name + " where (ET = '" + Convert.ToString(dr["PCM_ID"]).Trim() + "' OR APCM = '" + Convert.ToString(dr["PCM_ID"]).Trim() + "')  and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' ");
                    if (dtet.Rows.Count > 0)
                    {
                        if (Convert.ToString(dr["PCM_ID"]).Trim().Length <= 4)
                        { // pramod
                            tempdt.Clear();
                            SQLQuery(ref conn, ref tempdt, "select distinct LINK from mss_mgw_map_" + _CircleName + "_" + group_name + " where ET = '" + Convert.ToString(dr["PCM_ID"]).Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and link is not null and link not in ('') ");
                            if (tempdt.Rows.Count > 0)
                            {
                                // here we have checked that if this ET already has a link defined in it or not, if yes then a duplicate row is need to be inserted                                            
                                // this means that there is already a link defined and now we have to insert this link under this ET

                                linkdt.Clear();
                                tempQuery = "select distinct * from(select VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP,VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, if(CIC is null,'',CIC) CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME,MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID,MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE,ATCA_FLAG,ROUTER_ID,LINK_SPC,LINK_NODE,LINK_ADMIN_STATE,LINK_RATE,LINK_STATUS,LINK_DEST_POINTCODE from mss_mgw_map_" + _CircleName + "_" + group_name + " where ET = '" + Convert.ToString(dr["PCM_ID"]).Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "') A group by cic";
                                SQLQuery(ref conn, ref linkdt, tempQuery);

                                for (int k = 0; k <= linkdt.Rows.Count - 1; k++)
                                {

                                    string et_flag = linkdt.Rows[k]["EXTRA_ET_FLAG"].ToString().Trim();
                                    if (et_flag == "2")
                                    {
                                        tempQuery = "insert into `awe_dcm_vodafone`.`mss_mgw_map_" + _CircleName + "_" + group_name +
                                                    "` (`VENDOR`, `CIRCLE`, `NE_NAME`, `NODE_TYPE`, `MCC`, `MNC`, `MSS`, `ELEMENT_IP`, `MSS_C_NO`, `MSS_SPC`, `VMGW_ID`, `VMGW`, `VMGW_CTRL_SIGU`, `VMGW_SOURCE_IP`,`VMGW_DEST_IP`, `CGR`, `NCGR`, `CGR_SPC`, `GENERIC_NAME`, `CGR_NET`, `CGR_TYPE`, `CGR_UPART`, `TERM_ID`, `CIC`, `BSC_NAME`, `BSC_STATE`, `BSC_NUMBER`, `LAC`, `MGW_NAME`, `MGW_Source_Map_IP`, `MGW_C_NO`, `MGW_SPC`, `MGW_VMGW_ID`, `MGW_VMGW`, `MGW_VMGW_CTRL_ISU`, `MGW_CGR`, `MGW_NCGR`, `UNIT`, `STER`, `ETGR_VETGR`, `ET`, `APCM`, `LINK`, `LINKSET`, `SLC`, `EXTRA_ET_FLAG`, `SPC_TYPE`, `ATCA_FLAG`, `ROUTER_ID`, `LINK_SPC`,`LINK_NODE`,`LINK_ADMIN_STATE`,`LINK_RATE`,`LINK_STATUS`,`LINK_DEST_POINTCODE`) values ( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" + linkdt.Rows[k]["NE_NAME"] + "', '" +
                                                    linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" + linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] +
                                                    "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" + linkdt.Rows[k]["MSS_C_NO"] + "','" + linkdt.Rows[k]["MSS_SPC"] + "','" +
                                                    linkdt.Rows[k]["VMGW_ID"] + "','" + linkdt.Rows[k]["VMGW"] + "','" + linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" +
                                                    linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" + linkdt.Rows[k]["VMGW_DEST_IP"] + "','" + linkdt.Rows[k]["CGR"] + "','" +
                                                    linkdt.Rows[k]["NCGR"] + "','" + linkdt.Rows[k]["CGR_SPC"] + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" +
                                                    //linkdt.Rows[k]["NCGR"] + "','" + Convert.ToString(dr["LINKSET_ID"]).Trim() + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" +
                                                    linkdt.Rows[k]["CGR_NET"] + "','" + linkdt.Rows[k]["CGR_TYPE"] + "','" + linkdt.Rows[k]["CGR_UPART"] + "','" +
                                                    linkdt.Rows[k]["TERM_ID"] + "','" + linkdt.Rows[k]["CIC"] + "','" + linkdt.Rows[k]["BSC_NAME"] + "','" +
                                                    linkdt.Rows[k]["BSC_STATE"] + "','" + linkdt.Rows[k]["BSC_NUMBER"] + "','" + linkdt.Rows[k]["LAC"] + "','" +
                                                    linkdt.Rows[k]["MGW_NAME"] + "','" + linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" + linkdt.Rows[k]["MGW_C_NO"] + "','" +
                                                    linkdt.Rows[k]["MGW_SPC"] + "','" + linkdt.Rows[k]["MGW_VMGW_ID"] + "','" + linkdt.Rows[k]["MGW_VMGW"] + "', '" +
                                                    linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" + linkdt.Rows[k]["MGW_CGR"] + "','" + linkdt.Rows[k]["MGW_NCGR"] + "', '" +
                                                    linkdt.Rows[k]["UNIT"] + "', '" + linkdt.Rows[k]["STER"] + "', '" + linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" +
                                                    linkdt.Rows[k]["APCM"] + "', '" + Convert.ToString(dr["LINK_ID"]).Trim() + "', '" + Convert.ToString(dr["DESTINATION_POINT_CODE_NAME"]).Trim() + "', '" +
                                                    linkdt.Rows[k]["SLC"] + "', '2', '" +
                                                    linkdt.Rows[k]["SPC_TYPE"] + "',1,'" + dr["LINKSET_ID"] + "','" + Convert.ToString(dr["LINK_SPC"]).Trim() + "','" + Convert.ToString(dr["LINK_NODE"]).Trim() + "','" + Convert.ToString(dr["LINK_ADMIN_STATE"]).Trim() + "','" + Convert.ToString(dr["LINK_RATE"]).Trim() + "', '" + Convert.ToString(dr["LINK_STATUS"]).Trim() + "','" + dr["DESTINATION_POINT_CODE"] + "')";
                                        ExecuteSQLQuery(ref conn, tempQuery);
                                    }
                                    else
                                    {

                                        tempQuery = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP,VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME,MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID,MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE,ATCA_FLAG,ROUTER_ID,LINK_SPC,LINK_NODE,LINK_ADMIN_STATE,LINK_RATE,LINK_STATUS,LINK_DEST_POINTCODE) values( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" +
                                                    linkdt.Rows[k]["NE_NAME"] + "', '" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" + linkdt.Rows[k]["MNC"] + "', '" +
                                                    linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" + linkdt.Rows[k]["MSS_C_NO"] + "','" +
                                                    linkdt.Rows[k]["MSS_SPC"] + "','" + linkdt.Rows[k]["VMGW_ID"] + "','" + linkdt.Rows[k]["VMGW"] + "','" +
                                                    linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" + linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" + linkdt.Rows[k]["VMGW_DEST_IP"] + "','" +
                                                    //linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" + Convert.ToString(dr["LINKSET_ID"]).Trim() + "','" +
                                                    linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" + linkdt.Rows[k]["CGR_SPC"] + "','" +
                                                    linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" + linkdt.Rows[k]["CGR_TYPE"] + "','" +
                                                    linkdt.Rows[k]["CGR_UPART"] + "','" + linkdt.Rows[k]["TERM_ID"] + "','" + linkdt.Rows[k]["CIC"] + "','" +
                                                    linkdt.Rows[k]["BSC_NAME"] + "','" + linkdt.Rows[k]["BSC_STATE"] + "','" + linkdt.Rows[k]["BSC_NUMBER"] + "','" +
                                                    linkdt.Rows[k]["LAC"] + "','" + linkdt.Rows[k]["MGW_NAME"] + "','" + linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" +
                                                    linkdt.Rows[k]["MGW_C_NO"] + "','" + linkdt.Rows[k]["MGW_SPC"] + "','" + linkdt.Rows[k]["MGW_VMGW_ID"] + "','" +
                                                    linkdt.Rows[k]["MGW_VMGW"] + "', '" + linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" + linkdt.Rows[k]["MGW_CGR"] + "','" +
                                                    linkdt.Rows[k]["MGW_NCGR"] + "', '" + linkdt.Rows[k]["UNIT"] + "', '" + linkdt.Rows[k]["STER"] + "', '" +
                                                    linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" + linkdt.Rows[k]["APCM"] + "', '" +
                                                    Convert.ToString(dr["LINK_ID"]).Trim() + "', '" + Convert.ToString(dr["DESTINATION_POINT_CODE_NAME"]).Trim() + "', '" + linkdt.Rows[k]["SLC"] + "', '0', '" +
                                                    linkdt.Rows[k]["SPC_TYPE"] + "',1,'" + dr["LINKSET_ID"] + "', '" + Convert.ToString(dr["LINK_SPC"]).Trim() + "','" + Convert.ToString(dr["LINK_NODE"]).Trim() + "','" + Convert.ToString(dr["LINK_ADMIN_STATE"]).Trim() + "','" + Convert.ToString(dr["LINK_RATE"]).Trim() + "', '" + Convert.ToString(dr["LINK_STATUS"]).Trim() + "','" + dr["DESTINATION_POINT_CODE"] + "')";
                                        ExecuteSQLQuery(ref conn, tempQuery);
                                    }

                                }
                            }
                            else
                            {
                                ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set LINK='" + Convert.ToString(dr["LINK_ID"]).Trim() +
                                                 "', LINKSET = '" + Convert.ToString(dr["DESTINATION_POINT_CODE_NAME"]).Trim() + "', ROUTER_ID='" + dr["LINKSET_ID"] + "', SLC = '" + Convert.ToString(dr["SLC"]).Trim() + "', ATCA_FLAG=1, LINK_SPC = '" + Convert.ToString(dr["LINK_SPC"]).Trim() + "', LINK_NODE = '" + Convert.ToString(dr["LINK_NODE"]).Trim() + "', LINK_ADMIN_STATE = '" + Convert.ToString(dr["LINK_ADMIN_STATE"]).Trim() + "', LINK_RATE = '" + Convert.ToString(dr["LINK_RATE"]).Trim() + "', LINK_STATUS = '" + Convert.ToString(dr["LINK_STATUS"]).Trim() + "', LINK_DEST_POINTCODE='" + dr["DESTINATION_POINT_CODE"] + "' " +
                                                 " where ET = '" + Convert.ToString(dr["PCM_ID"]).Trim() + "' and Vendor='" + _vendor.Trim() +
                                                 "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and (link is null OR link='') ");
                            }

                        }
                        else
                        {
                            tempdt.Clear();
                            SQLQuery(ref conn, ref tempdt, "select distinct LINK from mss_mgw_map_" + _CircleName + "_" + group_name + " where APCM = '" + Convert.ToString(dr["PCM_ID"]).Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and link is not null and link not in ('') ");
                            if (tempdt.Rows.Count > 0)
                            {

                                // here we have checked that if this ET already has a link defined in it or not, if yes then a duplicate row is need to be inserted                                            
                                // this means that there is already a link defined and now we have to insert this link under this ET

                                linkdt.Clear();
                                tempQuery = "select distinct * from (select VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP,VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME,MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID,MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE,ATCA_FLAG,ROUTER_ID,LINK_SPC,LINK_NODE,LINK_ADMIN_STATE,LINK_RATE,LINK_STATUS,LINK_DEST_POINTCODE from mss_mgw_map_" + _CircleName + "_" + group_name + " where ET = '" + Convert.ToString(dr["PCM_ID"]).Trim() + "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "')A group by CIC ";
                                SQLQuery(ref conn, ref linkdt, tempQuery);

                                for (int k = 0; k <= linkdt.Rows.Count - 1; k++)
                                {
                                    string qur = string.Empty;
                                    string et_flag = linkdt.Rows[k]["EXTRA_ET_FLAG"].ToString().Trim();
                                    if (et_flag == "2")
                                    {
                                        qur = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP,VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME,MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID,MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE,ATCA_FLAG,ROUTER_ID,LINK_SPC,LINK_NODE,LINK_ADMIN_STATE,LINK_RATE,LINK_STATUS,LINK_DEST_POINTCODE) values( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" +
                                                linkdt.Rows[k]["NE_NAME"] + "', '" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" +
                                                linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" +
                                                linkdt.Rows[k]["MSS_C_NO"] + "','" + linkdt.Rows[k]["MSS_SPC"] + "','" + linkdt.Rows[k]["VMGW_ID"] + "','" +
                                                linkdt.Rows[k]["VMGW"] + "','" + linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" + linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" +
                                                linkdt.Rows[k]["VMGW_DEST_IP"] + "','" + linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" +
                                                //Convert.ToString(dr["LINKSET_ID"]).Trim() + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" +
                                                linkdt.Rows[k]["CGR_SPC"] + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" +
                                                linkdt.Rows[k]["CGR_TYPE"] + "','" + linkdt.Rows[k]["CGR_UPART"] + "','" + linkdt.Rows[k]["TERM_ID"] + "','" +
                                                linkdt.Rows[k]["CIC"] + "','" + linkdt.Rows[k]["BSC_NAME"] + "','" + linkdt.Rows[k]["BSC_STATE"] + "','" +
                                                linkdt.Rows[k]["BSC_NUMBER"] + "','" + linkdt.Rows[k]["LAC"] + "','" + linkdt.Rows[k]["MGW_NAME"] + "','" +
                                                linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" + linkdt.Rows[k]["MGW_C_NO"] + "','" + linkdt.Rows[k]["MGW_SPC"] + "','" +
                                                linkdt.Rows[k]["MGW_VMGW_ID"] + "','" + linkdt.Rows[k]["MGW_VMGW"] + "', '" + linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" +
                                                linkdt.Rows[k]["MGW_CGR"] + "', '" + linkdt.Rows[k]["MGW_NCGR"] + "', '" + linkdt.Rows[k]["UNIT"] + "', '" +
                                                linkdt.Rows[k]["STER"] + "', '" + linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" +
                                                linkdt.Rows[k]["APCM"] + "', '" + Convert.ToString(dr["LINK_ID"]).Trim() + "', '" + Convert.ToString(dr["DESTINATION_POINT_CODE_NAME"]).Trim() + "', '" +
                                                linkdt.Rows[k]["SLC"] + "', '2', '" + linkdt.Rows[k]["SPC_TYPE"] + "',1,'" + dr["LINKSET_ID"] + "','" + Convert.ToString(dr["LINK_SPC"]).Trim() + "','" + Convert.ToString(dr["LINK_NODE"]).Trim() + "','" + Convert.ToString(dr["LINK_ADMIN_STATE"]).Trim() + "','" + Convert.ToString(dr["LINK_RATE"]).Trim() + "', '" + Convert.ToString(dr["LINK_STATUS"]).Trim() + "','" + Convert.ToString(dr["DESTINATION_POINT_CODE"]).Trim() + "')";
                                        ExecuteSQLQuery(ref conn, qur);
                                    }
                                    else
                                    {
                                        qur = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + "(VENDOR, CIRCLE, NE_NAME, NODE_TYPE, MCC, MNC, MSS, ELEMENT_IP, MSS_C_NO, MSS_SPC, VMGW_ID, VMGW, VMGW_CTRL_SIGU, VMGW_SOURCE_IP,VMGW_DEST_IP, CGR, NCGR, CGR_SPC, GENERIC_NAME, CGR_NET, CGR_TYPE, CGR_UPART, TERM_ID, CIC, BSC_NAME, BSC_STATE, BSC_NUMBER, LAC, MGW_NAME,MGW_Source_Map_IP, MGW_C_NO, MGW_SPC, MGW_VMGW_ID,MGW_VMGW, MGW_VMGW_CTRL_ISU, MGW_CGR, MGW_NCGR, UNIT, STER, ETGR_VETGR, ET, APCM, LINK, LINKSET, SLC, EXTRA_ET_FLAG, SPC_TYPE,ATCA_FLAG,ROUTER_ID,LINK_SPC,LINK_NODE,LINK_ADMIN_STATE,LINK_RATE,LINK_STATUS,LINK_DEST_POINTCODE) values( '" + linkdt.Rows[k]["VENDOR"] + "','" + linkdt.Rows[k]["CIRCLE"] + "','" +
                                                linkdt.Rows[k]["NE_NAME"] + "', '" + linkdt.Rows[k]["NODE_TYPE"] + "','" + linkdt.Rows[k]["MCC"] + "','" +
                                                linkdt.Rows[k]["MNC"] + "', '" + linkdt.Rows[k]["MSS"] + "', '" + linkdt.Rows[k]["ELEMENT_IP"] + "','" +
                                                linkdt.Rows[k]["MSS_C_NO"] + "','" + linkdt.Rows[k]["MSS_SPC"] + "','" + linkdt.Rows[k]["VMGW_ID"] + "','" +
                                                linkdt.Rows[k]["VMGW"] + "','" + linkdt.Rows[k]["VMGW_CTRL_SIGU"] + "','" + linkdt.Rows[k]["VMGW_SOURCE_IP"] + "','" +
                                                linkdt.Rows[k]["VMGW_DEST_IP"] + "','" + linkdt.Rows[k]["CGR"] + "','" + linkdt.Rows[k]["NCGR"] + "','" +
                                                // Convert.ToString(dr["LINKSET_ID"]).Trim() + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" +
                                                linkdt.Rows[k]["CGR_SPC"] + "','" + linkdt.Rows[k]["GENERIC_NAME"] + "','" + linkdt.Rows[k]["CGR_NET"] + "','" +
                                                linkdt.Rows[k]["CGR_TYPE"] + "','" + linkdt.Rows[k]["CGR_UPART"] + "','" + linkdt.Rows[k]["TERM_ID"] + "','" +
                                                linkdt.Rows[k]["CIC"] + "','" + linkdt.Rows[k]["BSC_NAME"] + "','" + linkdt.Rows[k]["BSC_STATE"] + "','" +
                                                linkdt.Rows[k]["BSC_NUMBER"] + "','" + linkdt.Rows[k]["LAC"] + "','" + linkdt.Rows[k]["MGW_NAME"] + "','" +
                                                linkdt.Rows[k]["MGW_Source_Map_IP"] + "','" + linkdt.Rows[k]["MGW_C_NO"] + "','" + linkdt.Rows[k]["MGW_SPC"] + "','" +
                                                linkdt.Rows[k]["MGW_VMGW_ID"] + "','" + linkdt.Rows[k]["MGW_VMGW"] + "', '" + linkdt.Rows[k]["MGW_VMGW_CTRL_ISU"] + "', '" +
                                                linkdt.Rows[k]["MGW_CGR"] + "', '" + linkdt.Rows[k]["MGW_NCGR"] + "', '" + linkdt.Rows[k]["UNIT"] + "', '" +
                                                linkdt.Rows[k]["STER"] + "', '" + linkdt.Rows[k]["ETGR_VETGR"] + "', '" + linkdt.Rows[k]["ET"] + "', '" +
                                                linkdt.Rows[k]["APCM"] + "', '" + Convert.ToString(dr["LINK_ID"]).Trim() + "', '" + Convert.ToString(dr["DESTINATION_POINT_CODE_NAME"]).Trim() + "', '" +
                                                linkdt.Rows[k]["SLC"] + "', '0', '" + linkdt.Rows[k]["SPC_TYPE"] + "',1,'" + dr["LINKSET_ID"] + "','" + Convert.ToString(dr["LINK_SPC"]).Trim() + "','" + Convert.ToString(dr["LINK_NODE"]).Trim() + "','" + Convert.ToString(dr["LINK_ADMIN_STATE"]).Trim() + "','" + Convert.ToString(dr["LINK_RATE"]).Trim() + "','" + Convert.ToString(dr["LINK_STATUS"]).Trim() + "','" + Convert.ToString(dr["DESTINATION_POINT_CODE"]).Trim() + "')";
                                        ExecuteSQLQuery(ref conn, qur);
                                    }
                                }
                            }

                            else
                            {
                                ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set LINK='" + Convert.ToString(dr["LINK_ID"]).Trim() + "', LINKSET = '" +
                                                    Convert.ToString(dr["DESTINATION_POINT_CODE_NAEM"]).Trim() + "',ROUTER_ID='" + dr["LINKSET_ID"] + "', SLC = '" + Convert.ToString(dr["SLC"]).Trim() + "', ATCA_FLAG=1, LINK_SPC = '" + Convert.ToString(dr["LINK_SPC"]).Trim() + "', LINK_NODE = '" + Convert.ToString(dr["LINK_NODE"]).Trim() + "', LINK_ADMIN_STATE = '" + Convert.ToString(dr["LINK_ADMIN_STATE"]).Trim() + "', LINK_RATE = '" + Convert.ToString(dr["LINK_RATE"]).Trim() + "', LINK_STATUS = '" + Convert.ToString(dr["LINK_STATUS"]).Trim() + "', LINK_DEST_POINTCODE='" + dr["DESTINATION_POINT_CODE"] + "' where APCM = '" + Convert.ToString(dr["PCM_ID"]).Trim() +
                                                    "' and Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() +
                                                    "' and (link is null OR link='') ");
                            }
                        }
                    }

                    else
                    {
                        // these are extra ET's
                        if ((Convert.ToString(dr["PCM_ID"]).Trim().Length <= 4) ||(Convert.ToString(dr["PCM_ID"]).Trim().Length == 42))
                        {
                            tempQuery = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR, CIRCLE,NODE_TYPE, MGW_NAME,ET,LINK,LINKSET,ROUTER_ID, EXTRA_ET_FLAG, ATCA_FLAG, SLC,LINK_SPC,LINK_NODE,LINK_ADMIN_STATE,LINK_RATE,LINK_STATUS,LINK_DEST_POINTCODE) " +
                                        " values ('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType + "','" + _ne_name.Trim() + "','" +
                                        Convert.ToString(dr["PCM_ID"]).Trim() + "', '" + Convert.ToString(dr["LINK_ID"]).Trim() + "','" + Convert.ToString(dr["DESTINATION_POINT_CODE_NAME"]).Trim() + "','" + Convert.ToString(dr["LINKSET_ID"]).Trim() + "','2',1, '" + Convert.ToString(dr["SLC"]).Trim() + "','" + Convert.ToString(dr["LINK_SPC"]).Trim() + "','" + Convert.ToString(dr["LINK_NODE"]).Trim() + "','" + Convert.ToString(dr["LINK_ADMIN_STATE"]).Trim() + "','" + Convert.ToString(dr["LINK_RATE"]).Trim() + "','" + Convert.ToString(dr["LINK_STATUS"]).Trim() + "','" + Convert.ToString(dr["DESTINATION_POINT_CODE"]).Trim() + "')";
                            ExecuteSQLQuery(ref conn, tempQuery);
                        }
                    }

                    //Console.WriteLine("Link ID  : " + Convert.ToString(dr["LINK_ID"]));
                    //Console.WriteLine("Destination Point Code  : " + Convert.ToString(dr["DESTINATION_POINT_CODE"]));
                    //Console.WriteLine("SLC  : " + Convert.ToString(dr["SLC"]));
                    //Console.WriteLine("PCM ID  : " + Convert.ToString(dr["PCM_ID"]));
                    //Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                //if (!Directory.Exists(parsing_error_path))
                //{
                //    Directory.CreateDirectory(parsing_error_path);
                //}

                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_SIGNALING_SS7_LINK ");
                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_SIGNALING_SS7_LINK()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_SHOW_TDM_CIRCUITGROUP_ALL(string output, string CircleName, string NE_Type)
        {
            string cgr_spc = "";
            string previous_lac = string.Empty;
            string mss_lac = string.Empty;
            string newquery = string.Empty;
            string LAC_VALUE = string.Empty;
            string circuit_group_name = string.Empty;
            string[] temp1;
            string data = string.Empty;
            string temp_data = string.Empty;
            string circuit_group_number = string.Empty;
            try
            {
                if (output.Contains("CIRCUIT GROUP:"))
                {
                    data = output.Remove(0, output.IndexOf("CIRCUIT GROUP:"));
                }

                data = data.Trim();
                string[] temp;
                string[] cmnd_data = Regex.Split(data.Trim(), "CIRCUIT GROUP:");

                for (int i = 0; i <= cmnd_data.Length - 1; i++)
                {
                    try
                    {
                        circuit_group_name = "";
                        circuit_group_number = "";
                        cgr_spc = "";

                        if (cmnd_data[i] != " " && cmnd_data[i] != "")
                        {
                            temp = Regex.Split(cmnd_data[i].Trim(), "\r\n");

                            for (int j = 0; j <= temp.Length - 1; j++)
                            {
                                if (temp[j].Contains("circuit group name"))
                                {
                                    temp1 = Regex.Split(temp[j].Trim(), ":");
                                    circuit_group_name = temp1[1].Trim();
                                }

                                if (temp[j].Contains("circuit group number"))
                                {
                                    temp1 = Regex.Split(temp[j].Trim(), ":");
                                    circuit_group_number = temp1[1].Trim();
                                }

                                if (circuit_group_name != "" && circuit_group_number != "")
                                {
                                    //newquery = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set CGR = '" + circuit_group_number + "', NCGR = '" + circuit_group_name + "' where NE_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + CircleName + "' AND CGR_SPC like '%" + cgr_spc + "/%'";
                                    //newquery = "update mss_mgw_map_" + CircleName + "_" + group_name + "  set CGR = '" + circuit_group_number + "', NCGR = '" + circuit_group_name + "' where MGW_NAME = '" + _ne_name + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + CircleName + "'";

                                    newquery = "Insert into mss_mgw_map_" + _CircleName + "_" + group_name + "(VENDOR,CIRCLE,NODE_TYPE,MGW_NAME,MGW_CGR,MGW_NCGR,ATCA_FLAG) Values('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType.Trim() + "','" + _ne_name.Trim() + "','" + circuit_group_number.Trim() + "','" + circuit_group_name.Trim() + "','1')";
                                    ExecuteSQLQuery(ref conn, newquery);
                                    circuit_group_name = "";
                                    circuit_group_number = "";
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZEDO ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        oExceptionLog.WriteExceptionErrorToFile("ParseData_SHOW_TDM_CIRCUITGROUP_ALL_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_SHOW_TDM_CIRCUITGROUP_ALL()", ErrorMsg, "", ref FileError);

                ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'", "") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="_CircleName"></param>
        /// <param name="NE_Type"></param>
        public void ParseData_SIGNALING_SCCP_SUBSYS_ALL(string output, string _CircleName, string NE_Type)
        {

            string[] temp;
            string[] temp1;
            string[] temp_SCCP;
            DataTable dt = null;
            //DataTable dt_SCCP = null;
            string SCCP_STATUS = string.Empty;
            string SCCP_SubSystem = string.Empty;
            string SubSys_FullName = string.Empty;
            string SqlStr = string.Empty;
            string SCCP_Identifier = string.Empty;
            string SCCP_SUBSYS_NUM = string.Empty;
            try
            {
                output = output.Replace("--More--", "");
                temp = Regex.Split(output.Trim(), "\r\n");
                for (int i = 1; i < temp.Length - 1; i++)
                {
                    if (temp[i].Contains("SCCP SubSystem") && !temp[i].Contains("SCCP SubSystem Identifier") && !temp[i].Contains("SCCP SubSystem Number"))
                    {
                        temp1 = null;
                        temp1 = Regex.Split(temp[i], ":");
                        temp_SCCP = Regex.Split(temp1[1].ToString(), "_");
                        SCCP_SubSystem = temp_SCCP[1].ToString();
                        SubSys_FullName = temp1[1].ToString();
                    }
                    if (temp[i].Contains("SCCP SubSystem Identifier"))
                    {
                        temp1 = null;
                        temp1 = Regex.Split(temp[i], ":");
                        SCCP_Identifier = temp1[1].ToString();
                    }
                    if (temp[i].Contains("SCCP SubSystem Number"))
                    {
                        temp1 = null;
                        temp1 = Regex.Split(temp[i], ":");
                        SCCP_SUBSYS_NUM = temp1[1].ToString();
                    }
                    if (temp[i].Contains("Status"))
                    {
                        temp1 = null;
                        temp1 = Regex.Split(temp[i], ":");
                        SCCP_STATUS = temp1[1].ToString();
                    }
                    if (SCCP_SubSystem != "" && SCCP_STATUS != "" && SCCP_Identifier != "" && SubSys_FullName != "")
                    {
                        dt = new DataTable();
                        SQLQuery(ref conn, ref dt, "select distinct * from mss_mgw_map_" + _CircleName + "_" + group_name + " where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and Node_Type='MGW_ATCA' and LINKSET='" + SCCP_SubSystem + "'");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name + " Set SCCP_SUBSYS_STATUS ='" + SCCP_STATUS.Trim() + "', SCCP_IDENTIFIER='" + SCCP_Identifier + "', SCCP_SUBSYS ='" + SubSys_FullName.Trim() + "' Where Vendor='" + _vendor.Trim() + "' and CIRCLE = '" + _CircleName.Trim() + "'  and MGW_NAME = '" + _ne_name.Trim() + "' and Node_Type='MGW_ATCA' and LINKSET='" + SCCP_SubSystem + "'";
                                ExecuteSQLQuery(ref conn, SqlStr);
                            }
                            //else
                            //{
                            //    SqlStr = "Insert into mss_mgw_map_" + _CircleName + "_" + group_name + "(VENDOR,CIRCLE,NODE_TYPE,MGW_NAME,SCCP_SUBSYS_STATUS,LINKSET,ATCA_FLAG) Values('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType.Trim() + "','" + _ne_name.Trim() + "','" + SCCP_STATUS.Trim() + "','" + SCCP_SubSystem.Trim() + "','1')";
                            //    ExecuteSQLQuery(ref conn, SqlStr);
                            //}
                        }
                        //else
                        //{
                        //    SqlStr = "Insert into mss_mgw_map_" + _CircleName + "_" + group_name + "(VENDOR,CIRCLE,NODE_TYPE,MGW_NAME,SCCP_SUBSYS_STATUS,LINKSET,ATCA_FLAG) Values('" + _vendor.Trim() + "','" + _CircleName.Trim() + "','" + _NEType.Trim() + "','" + _ne_name.Trim() + "','" + SCCP_STATUS.Trim() + "','" + SCCP_SubSystem.Trim() + "','1')";
                        //    ExecuteSQLQuery(ref conn, SqlStr);
                        //}
                        SCCP_Identifier = "";
                        SubSys_FullName = "";
                        SCCP_SUBSYS_NUM = "";
                        SCCP_STATUS = "";
                        SCCP_SubSystem = "";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("ParseData_SIGNALING_SCCP_SUBSYS_ALL()", ErrorMsg, "", ref FileError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public DataTable CreateDataTable(string[] columnName)
        {

            DataTable dataTable = new DataTable();
            try
            {

                for (int i = 0; i < columnName.Length; i++)
                {
                    dataTable.Columns.Add(columnName[i], typeof(string));
                }
            }
            catch (Exception ex)
            {

                //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + _NEType + @"\";
                //if (!Directory.Exists(parsing_error_path))
                //{
                //    Directory.CreateDirectory(parsing_error_path);
                //}

                //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function CreateDataTable ");
                //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> " + ex.Message.ToString());
                ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("CreateDataTable()", ErrorMsg, "", ref FileError);

            }
            return dataTable;

        }
        // End ATCA Parsing
        #endregion


    }
}
