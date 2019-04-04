using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.IO;
using NSN.GNOCN.Tools.Logs;

namespace NSN.GNOCN.Tools.AWE.NSS
{
    public class clsTransferToNSSFromCS
    {
        private static clsExceptionLogs oExceptionLog;
        static string errorFolderPath = @"D:\CS_DCM\DCM_RAW\PARSING_ERROR\";

        private static string GetNSS_Circle(string GroupWiseName)
        {
            try
            {
                string circle = string.Empty;
                switch (GroupWiseName)
                {
                    case "AP":
                        circle = "AP";
                        break;
                    case "GUJ":
                        circle = "GUJARAT";
                        break;
                    case "CHN":
                        circle = "CHENNAI";
                        break;
                    case "HAR":
                        circle = "HARYANA";
                        break;
                    case "KERALA":
                        circle = "KERALA";
                        break;
                    case "MAH":
                        circle = "MAHARASHTRA";
                        break;
                    case "RAJ":
                        circle = "RAJASTHAN";
                        break;
                    case "ROB":
                        circle = "ROB";
                        break;
                    case "KAR":
                        circle = "KARNATAKA";
                        break;
                    case "ROTN":
                        circle = "ROTN";
                        break;
                    case "UPE":
                        circle = "UPE";
                        break;
                    case "UPW":
                        circle = "UPw";
                        break;
                }

                return circle;
            }
            catch
            {
                return "";
            }
        }
        public static void DCMDataMigration(string CircleName)
        {
            string ErrorMsg = string.Empty;
            string FileError = string.Empty;
            DataTable dtTemp = null;
            try
            {
                oExceptionLog = new clsExceptionLogs("Bharti_DCM_Parse_" + CircleName);
                //MySqlConnection mysqlConn = new MySqlConnection("Data Source = 93.183.30.156; Database=Awe_dcm; user id=root; password=tiger; allow zero datetime=true;connection timeout=1000");
                MySqlConnection mysqlConn = new MySqlConnection(System.Configuration.ConfigurationSettings.AppSettings["CSDBConstring"].ToString());
                SqlConnection sqlConn = new SqlConnection(System.Configuration.ConfigurationSettings.AppSettings["SQLDBConstring"].ToString());

                DateTime start = DateTime.Now;
                string dateString = string.Empty;
                DataTable dtCircle = new DataTable();
                ExecuteMySqlQuery(ref mysqlConn, "update `awe_dcm_vodafone`.`dcm_updation_completion` set time_Stamp='" + System.DateTime.Now.ToString("yyyy-MM-dd") + "', status='not completed' Where circle='" + CircleName + "'");
                MySqlQuery(ref mysqlConn, ref dtCircle, "SELECT distinct circle FROM `awe_dcm_vodafone`.`dcm_updation_completion` where time_stamp regexp '" + System.DateTime.Now.ToString("yyyy-MM-dd") + "' and status='Not Completed' and circle='" + CircleName + "';");
                if (dtCircle.Rows.Count > 0)
                {
                    for (int i = 0; i < dtCircle.Rows.Count; i++)
                    {
                        DataTable dtGroups = new DataTable();
                        MySqlQuery(ref mysqlConn, ref dtGroups, "SELECT distinct `group_Circle` FROM `awe_dcm_vodafone`.`group_info` where circle='" + dtCircle.Rows[i][0].ToString() + "'");
                        if (dtGroups.Rows.Count > 0)
                        {
                            string CircleGroupName = string.Empty;
                            for (int j = 0; j < dtGroups.Rows.Count; j++)
                            {
                                CircleGroupName += "'" + dtGroups.Rows[j][0].ToString() + "',";
                            }

                            DataTable dtc = new DataTable();
                            int Month = Convert.ToInt32(System.DateTime.Now.ToString("MM"));
                            int Day = Convert.ToInt32(System.DateTime.Now.ToString("dd"));
                            int Year = Convert.ToInt32(System.DateTime.Now.ToString("yyyy"));

                            //dateString = Month.ToString() + "/" + Day.ToString() + "/" + Year.ToString();

                            dateString = System.DateTime.Now.ToString("yyyy-MM-dd");

                            MySqlQuery(ref mysqlConn, ref dtc, "SELECT CIRCLE FROM parsing_completion_details where start_time regexp '" + dateString + "' and circle in (" + CircleGroupName.TrimEnd(',') + ") and `status` = 'completed'");
                            string date = System.DateTime.Now.ToString("MM/dd/yyyy");
                            if (dtc.Rows.Count == dtGroups.Rows.Count)
                            {
                                //Console.WriteLine("DCM Data is updating in AWE SQL DB for Circle :" + dtCircle.Rows[i][0].ToString());

                                //foreach (DataRow drc in dtc.Rows)
                                //{
                                //Console.WriteLine("Circle " + dtCircle.Rows[i][0].ToString() + " data is transfering.... Do not Close exe");
                                DataTable dta = new DataTable();
                                DataTable dtd = new DataTable();
                                DataTable dth = new DataTable();
                                DataTable dtmsc = new DataTable();
                                DataTable dtmss = new DataTable();
                                DataTable dtne = new DataTable();
                                DataTable dtMssLink = new DataTable();
                                DataTable dtMssSubtrack = new DataTable(); //code add by rahul

                                MySqlQuery(ref mysqlConn, ref dta, "SELECT distinct ifnull(`NODE`,''),ifnull(`LINK_SET`,''),ifnull(`IP_LINK`,''),ifnull(`ASSOCIATION_SET_ID`,''),ifnull(`ASSOCIATION_SET`,''),ifnull(`ID`,''),ifnull(`UNIT`,''),ifnull(`SOURCE_IP_1`,''),ifnull(`SOURCE_IP_2`,''),ifnull(`DESTINATION_IP_1`,''),ifnull(`DESTINATION_IP_2`,''),ifnull(`CGR_NAME`,''),ifnull(`CGR_NUMBER`,''),ifnull(`NA`,''),ifnull(`SPC`,''),ifnull(`DESTINATION_IP`,''),ifnull(`DESTINATION_NAME`,''),ifnull(`CIRCLE`,''),ifnull(`NODE_TYPE`,''),ifnull(`SCTP_USER`,''),ifnull(`ROLE`,''),ifnull(`STATUS`,''),ifnull(`PARAMETER_SET_NAME`,''),ifnull(`SOURCE_PORT`,''),ifnull(`DESTINATION_PORT`,''),ifnull(`STATE`,''),ifnull(`DATA_STREAM_COUNT`,'') FROM association_info_" + dtCircle.Rows[i][0].ToString() + " where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                MySqlQuery(ref mysqlConn, ref dtd, "SELECT distinct * FROM `awe_dcm_vodafone`.`destination_info_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                MySqlQuery(ref mysqlConn, ref dth, "SELECT distinct * FROM `awe_dcm_vodafone`.`hlr_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                MySqlQuery(ref mysqlConn, ref dtmsc, "SELECT distinct * FROM `awe_dcm_vodafone`.`msc_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                MySqlQuery(ref mysqlConn, ref dtmss, "SELECT distinct ifnull(`VENDOR`,''), ifnull(`CIRCLE`,''), ifnull(`NE_NAME`,''),ifnull(`NODE_TYPE`,''),ifnull(`MCC`,''),ifnull(`MNC`,''),ifnull(`MSS`,''),ifnull(`ELEMENT_IP`,''),ifnull(`MSS_C_NO`,''),ifnull(`MSS_SPC`,''),ifnull(`VMGW_ID`,''),ifnull(`VMGW`,''),ifnull(`VMGW_CTRL_SIGU`,''),ifnull(`VMGW_SOURCE_IP`,''),ifnull(`VMGW_DEST_IP`,''),ifnull(`CGR`,''),ifnull(`NCGR`,''),ifnull(`CGR_SPC`,''),ifnull(`GENERIC_NAME`,''),ifnull(`CGR_NET`,''),ifnull(`CGR_TYPE`,''),ifnull(`CGR_UPART`,''),ifnull(`TERM_ID`,''),ifnull(`CIC`,''),ifnull(`BSC_NAME`,''),ifnull(`BSC_STATE`,''),ifnull(`BSC_NUMBER`,''),ifnull(`LAC`,''),ifnull(`MGW_NAME`,''),ifnull(`MGW_Source_Map_IP`,''),ifnull(`MGW_C_NO`,''),ifnull(`MGW_SPC`,''),ifnull(`MGW_VMGW_ID`,''),ifnull(`MGW_VMGW`,''),ifnull(`MGW_VMGW_CTRL_ISU`,''),ifnull(`MGW_CGR`,''),ifnull(`MGW_NCGR`,''),ifnull(`UNIT`,''),ifnull(`STER`,''),ifnull(`ETGR_VETGR`,''),ifnull(`ET`,''),ifnull(`APCM`,''),ifnull(`LINK`,''),ifnull(`LINKSET`,''),ifnull(`SLC`,''),ifnull(`EXTRA_ET_FLAG`, 0),ifnull(`SPC_TYPE`,''),`ATCA_FLAG`,ifnull(`ROUTER_ID`,''),ifnull(`PAR_SET`,''),ifnull(`PRIO`,''),ifnull(`SP_TYPE`,''),ifnull(`SS7_STAND`,''),ifnull(`SUB_FIELD_INFO_COUNT`,''),ifnull(`SUB_FIELD_INFO_LENGTHS`,''),ifnull(`STATE`,''),ifnull(`SUB_FIELD_INFO_BIT`,''),ifnull(`NBCRCT`,''),ifnull(`REGISTRATION_STATUS`,''),ifnull(`VMGW_SECONDARY_IP`,''),ifnull(`SCCP_SUBSYS_STATUS`,''),ifnull(`SCCP_IDENTIFIER`,''),ifnull(`SCCP_SUBSYS`,''),ifnull(`TF`,''),ifnull(`EXTERN_PCM_TSL`,''),ifnull(`INT_PCM_TSL`,''),ifnull(`BIT_RATE`,''),ifnull(`ASSOCIATION_SET`,''),ifnull(`MGW_NBCRCT`,''),ifnull(`LINK_SPC`,''),ifnull(`LINK_NODE`,''),ifnull(`LINK_ADMIN_STATE`,''),ifnull(`LINK_RATE`,''),ifnull(`LINK_STATUS`,''),ifnull(`HCLBRG`,''),ifnull(`LINK_DEST_POINTCODE`,'') FROM `awe_dcm_vodafone`.`mss_mgw_map_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");

                                MySqlQuery(ref mysqlConn, ref dtMssLink, "SELECT distinct ifnull(`VENDOR`,''),ifnull(`CIRCLE`,''),ifnull(`NE_NAME`,''),ifnull(`NODE_TYPE`,''),ifnull(`MSS`,''),ifnull(`ELEMENT_IP`,''),ifnull(`MSS_C_NO`,''),ifnull(`MSS_SPC`,''),ifnull(`CGR_SPC`,''),ifnull(`GENERIC_NAME`,''),ifnull(`CGR_NET`,''),ifnull(`ET`,''),ifnull(`LINK`,''),ifnull(`LINKSET`,''),ifnull(`SLC`,'') FROM `awe_dcm_vodafone`.`mss_link_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");

                                MySqlQuery(ref mysqlConn, ref dtMssSubtrack, "SELECT distinct ifnull(`VENDOR`,''),ifnull(`CIRCLE`,''),ifnull(`NE_NAME`,''),ifnull(`NODE_TYPE`,''),ifnull(`UNIT`,''),ifnull(`LOC`,''),ifnull(`POWER_CARD`,''),ifnull(`CP`,''),ifnull(`ATCA_FLAG`,'') FROM `awe_dcm_vodafone`.`redundancy_subtrack_mss_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");

                                if (dta != null)
                                {
                                    if (dta.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving Association Data in to Temp Table in Sql");
                                        Insert_intoTempTable(ref sqlConn, dta, "association_info_temp", GetNSS_Circle(dtCircle.Rows[i][0].ToString()));
                                        //Console.WriteLine("Moving Association Data from Temp to Main Table in SQL");
                                        ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[association_info] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                        ExecuteSqlQuery(ref sqlConn, "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[association_info] SELECT * FROM [AWE_db_VFI_CS_DCM].[dbo].[association_info_temp] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                    }
                                }
                                if (dtd != null)
                                {
                                    if (dtd.Rows.Count > 0)
                                    {

                                        //Console.WriteLine("Moving Destination Data in to Temp Table in Sql");
                                        Insert_intoTempTable(ref sqlConn, dtd, "destination_info_temp", GetNSS_Circle(dtCircle.Rows[i][0].ToString()));
                                        //Console.WriteLine("Moving Destination Data from Temp to Main Table in SQL");
                                        ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[destination_info] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                        ExecuteSqlQuery(ref sqlConn, "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[destination_info] SELECT * FROM [AWE_db_VFI_CS_DCM].[dbo].[destination_info_temp] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                    }
                                }
                                if (dth != null)
                                {
                                    if (dth.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving HLR Data in to Temp Table in Sql");
                                        Insert_intoTempTable(ref sqlConn, dth, "HLR_temp", GetNSS_Circle(dtCircle.Rows[i][0].ToString()));
                                        //Console.WriteLine("Moving HLR Data from Temp to Main Table in SQL");
                                        ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[HLR] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                        ExecuteSqlQuery(ref sqlConn, "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[HLR] SELECT * FROM [AWE_db_VFI_CS_DCM].[dbo].[HLR_temp] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                    }
                                }
                                if (dtmsc != null)
                                {
                                    if (dtmsc.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving MSC Data in to Temp Table in Sql");
                                        Insert_intoTempTable(ref sqlConn, dtmsc, "MSC_temp", GetNSS_Circle(dtCircle.Rows[i][0].ToString()));
                                        //Console.WriteLine("Moving MSC Data from Temp to Main Table in SQL");
                                        ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[MSC] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                        ExecuteSqlQuery(ref sqlConn, "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[MSC] SELECT * FROM [AWE_db_VFI_CS_DCM].[dbo].[MSC_temp] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                    }
                                }
                                if (dtmss != null)
                                {
                                    if (dtmss.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving MSS MGW Data in to Temp Table in Sql");
                                        Insert_intoTempTable(ref sqlConn, dtmss, "MSS_MGW_MAPPING_temp", GetNSS_Circle(dtCircle.Rows[i][0].ToString()));
                                        //Console.WriteLine("Moving MGW Data from Temp to Main Table in SQL");
                                        ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[MSS_MGW_MAPPING] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                        ExecuteSqlQuery(ref sqlConn, "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[MSS_MGW_MAPPING] SELECT * FROM [AWE_db_VFI_CS_DCM].[dbo].[MSS_MGW_MAPPING_temp] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                    }
                                }
                                if (dtMssLink != null)
                                {
                                    if (dtMssLink.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving MSS Link Data in to Temp Table in Sql");
                                        Insert_intoTempTable(ref sqlConn, dtMssLink, "mss_link_temp", GetNSS_Circle(dtCircle.Rows[i][0].ToString()));
                                        //Console.WriteLine("Moving MSS Link Data from Temp to Main Table in SQL");
                                        ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[MSS_LINK] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                        ExecuteSqlQuery(ref sqlConn, "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[MSS_LINK] SELECT * FROM [AWE_db_VFI_CS_DCM].[dbo].[MSS_LINK_TEMP] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                    }
                                }

                                //code added by rahul on 27-08-2018
                                if (dtMssSubtrack != null)
                                {
                                    if (dtMssSubtrack.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving MSS Link Data in to Temp Table in Sql");
                                        Insert_intoTempTable(ref sqlConn, dtMssSubtrack, "redundancy_subtrack_mss_temp", GetNSS_Circle(dtCircle.Rows[i][0].ToString()));
                                        //Console.WriteLine("Moving MSS Link Data from Temp to Main Table in SQL");
                                        ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[redundancy_subtrack_mss] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                        ExecuteSqlQuery(ref sqlConn, "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[redundancy_subtrack_mss] SELECT * FROM [AWE_db_VFI_CS_DCM].[dbo].[redundancy_subtrack_mss_temp] WHERE CIRCLE = '" + GetNSS_Circle(dtCircle.Rows[i][0].ToString()) + "'");
                                    }
                                }

                                //Console.WriteLine("Moving mss_mgw_Mapping Data to Mss_mgw_map_bck table for circle :" + dtCircle.Rows[i][0].ToString());
                                dtTemp = new DataTable();
                                MySqlQuery(ref mysqlConn, ref dtTemp, "select * FROM `awe_dcm_vodafone`.`mss_mgw_map_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                if (dtTemp != null)
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        if (!ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`mss_mgw_map_bck_" + dtCircle.Rows[i][0].ToString() + "`"))
                                        {
                                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ": Could not truncate table `awe_dcm_vodafone`.`mss_mgw_map_bck_" + dtCircle.Rows[i][0].ToString() + "`";
                                            oExceptionLog.WriteExceptionErrorToFile("DCMDataMigration()", ErrorMsg, "", ref FileError);
                                        }
                                        if (!ExecuteMySqlQuery(ref mysqlConn, "insert into `awe_dcm_vodafone`.`mss_mgw_map_bck_" + dtCircle.Rows[i][0].ToString() + "` select * FROM `awe_dcm_vodafone`.`mss_mgw_map_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'"))
                                        {
                                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ": Could not insert  into table `awe_dcm_vodafone`.`mss_mgw_map_bck_" + dtCircle.Rows[i][0].ToString() + "`";
                                            oExceptionLog.WriteExceptionErrorToFile("DCMDataMigration()", ErrorMsg, "", ref FileError);
                                        }
                                        if (!ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`mss_mgw_map_" + dtCircle.Rows[i][0].ToString() + "`"))
                                        {
                                            ErrorMsg = "@" + System.DateTime.Now.ToString() + ": Could not truncate table `awe_dcm_vodafone`.`mss_mgw_map_" + dtCircle.Rows[i][0].ToString() + "`";
                                            oExceptionLog.WriteExceptionErrorToFile("DCMDataMigration()", ErrorMsg, "", ref FileError);
                                        }
                                    }

                                }
                                dtTemp = null;

                                dtTemp = new DataTable();
                                MySqlQuery(ref mysqlConn, ref dtTemp, "select * FROM `awe_dcm_vodafone`.`mss_link_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                if (dtTemp != null)
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving mss_link Data to Mss_link_bck table for circle :" + dtCircle.Rows[i][0].ToString());
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`mss_link_bck_" + dtCircle.Rows[i][0].ToString() + "`");
                                        ExecuteMySqlQuery(ref mysqlConn, "insert into `awe_dcm_vodafone`.`mss_link_bck_" + dtCircle.Rows[i][0].ToString() + "` select * FROM `awe_dcm_vodafone`.`mss_link_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`mss_link_" + dtCircle.Rows[i][0].ToString() + "`");
                                    }
                                }
                                dtTemp = null;


                                dtTemp = new DataTable();
                                MySqlQuery(ref mysqlConn, ref dtTemp, "select * FROM `awe_dcm_vodafone`.`association_info_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                if (dtTemp != null)
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving mss_link Data to Mss_link_bck table for circle :" + dtCircle.Rows[i][0].ToString());
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`association_info_bck_" + dtCircle.Rows[i][0].ToString() + "`");
                                        ExecuteMySqlQuery(ref mysqlConn, "insert into `awe_dcm_vodafone`.`association_info_bck_" + dtCircle.Rows[i][0].ToString() + "` select * FROM `awe_dcm_vodafone`.`association_info_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`association_info_" + dtCircle.Rows[i][0].ToString() + "`");
                                    }
                                }
                                dtTemp = null;

                                dtTemp = new DataTable();
                                MySqlQuery(ref mysqlConn, ref dtTemp, "select * FROM `awe_dcm_vodafone`.`destination_info_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                if (dtTemp != null)
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving mss_link Data to Mss_link_bck table for circle :" + dtCircle.Rows[i][0].ToString());
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`destination_info_bck_" + dtCircle.Rows[i][0].ToString() + "`");
                                        ExecuteMySqlQuery(ref mysqlConn, "insert into `awe_dcm_vodafone`.`destination_info_bck_" + dtCircle.Rows[i][0].ToString() + "` select * FROM `awe_dcm_vodafone`.`destination_info_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`destination_info_" + dtCircle.Rows[i][0].ToString() + "`");
                                    }
                                }
                                dtTemp = null;

                                dtTemp = new DataTable();
                                MySqlQuery(ref mysqlConn, ref dtTemp, "select * FROM `awe_dcm_vodafone`.`hlr_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                if (dtTemp != null)
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving mss_link Data to Mss_link_bck table for circle :" + dtCircle.Rows[i][0].ToString());
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`hlr_" + dtCircle.Rows[i][0].ToString() + "`");
                                        ExecuteMySqlQuery(ref mysqlConn, "insert into `awe_dcm_vodafone`.`hlr_" + dtCircle.Rows[i][0].ToString() + "` select * FROM `awe_dcm_vodafone`.`hlr_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`hlr_" + dtCircle.Rows[i][0].ToString() + "`");
                                    }
                                }
                                dtTemp = null;

                                dtTemp = new DataTable();
                                MySqlQuery(ref mysqlConn, ref dtTemp, "select * FROM `awe_dcm_vodafone`.`msc_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                if (dtTemp != null)
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        //Console.WriteLine("Moving mss_link Data to Mss_link_bck table for circle :" + dtCircle.Rows[i][0].ToString());
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`msc_" + dtCircle.Rows[i][0].ToString() + "`");
                                        ExecuteMySqlQuery(ref mysqlConn, "insert into `awe_dcm_vodafone`.`msc_" + dtCircle.Rows[i][0].ToString() + "` select * FROM `awe_dcm_vodafone`.`msc_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`msc_" + dtCircle.Rows[i][0].ToString() + "`");
                                    }
                                }
                                dtTemp = null;

                                //code added by rahul on 28-08-2018
                                dtTemp = new DataTable();
                                MySqlQuery(ref mysqlConn, ref dtTemp, "select * FROM `awe_dcm_vodafone`.`redundancy_subtrack_mss_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                if (dtTemp != null)
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`redundancy_subtrack_mss_bck_" + dtCircle.Rows[i][0].ToString() + "`");
                                        ExecuteMySqlQuery(ref mysqlConn, "insert into `awe_dcm_vodafone`.`redundancy_subtrack_mss_bck_" + dtCircle.Rows[i][0].ToString() + "` select * FROM `awe_dcm_vodafone`.`redundancy_subtrack_mss_" + dtCircle.Rows[i][0].ToString() + "` where circle = '" + dtCircle.Rows[i][0].ToString() + "'");
                                        ExecuteMySqlQuery(ref mysqlConn, "truncate table `awe_dcm_vodafone`.`redundancy_subtrack_mss_" + dtCircle.Rows[i][0].ToString() + "`");
                                    }
                                }

                                //Console.WriteLine("Circle " + dtCircle.Rows[i][0].ToString() + " data transfering is Completed !!!\r\n");
                                ExecuteMySqlQuery(ref mysqlConn, "update `awe_dcm_vodafone`.`dcm_updation_completion` set time_stamp='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', status='completed' where circle='" + dtCircle.Rows[i][0].ToString() + "'");


                            }
                        }
                        TimeSpan ElapsedTime = DateTime.Now.Subtract(start);

                    }
                    dtCircle.Clear();
                    // Thread.Sleep(10 * 60 * 1000);
                    //   MySqlQuery(ref mysqlConn, ref dtCircle, "SELECT distinct circle FROM `awe_dcm_vodafone`.`dcm_updation_completion` where time_stamp regexp '" + System.DateTime.Now.ToString("yyyy-MM-dd") + "' and status='Not completed';");
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "@" + System.DateTime.Now.ToString() + ":" + ex.Message.ToString();
                oExceptionLog.WriteExceptionErrorToFile("DCMDataMigration()", ErrorMsg, "", ref FileError);
            }
        }

        public static void Insert_intoTempTable(ref SqlConnection sqlConn, DataTable data, String Table, string Circle)
        {
            try
            {
                string QueryHead = string.Empty;
                string QueryTail = string.Empty;
                string MGWATCA = string.Empty;
                int count = 0;
                int mgw_atca_flag = 0;
                int mss_atca_flag = 0;
                ExecuteSqlQuery(ref sqlConn, "DELETE FROM [AWE_db_VFI_CS_DCM].[dbo].[" + Table + "] WHERE CIRCLE = '" + Circle + "'");
                QueryHead = "INSERT INTO [AWE_db_VFI_CS_DCM].[dbo].[" + Table + "] values ";///Convert.ToInt32(IIf(a = "", 0, a))

                foreach (DataRow dr in data.Rows)
                {
                    if (Table.Contains("association"))
                    {
                        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
                        if (!regex.IsMatch(dr[5].ToString()))
                        {
                            continue;
                        }
                        //if (dr[2].ToString() == "78")
                        //{
                        //    QueryTail += "('" + dr[0].ToString() + "','" + dr[1].ToString() + "','" + dr[2].ToString() + "','" + dr[3].ToString() + "','" + dr[4].ToString() + "','" + dr[5].ToString()  + "','" + dr[6].ToString() + "','" + dr[7].ToString() + "','" + dr[8].ToString() + "','" + dr[9].ToString() + "','" + dr[10].ToString() + "','" + dr[11].ToString() + "','" + dr[12].ToString() + "','" + dr[13].ToString() + "','" + dr[14].ToString() + "','" + dr[15].ToString() + "','" + dr[16].ToString() + "','" + dr[17].ToString() + "','" + dr[18].ToString() + "'),";
                        //}
                        QueryTail += "('" + dr[0].ToString() + "','" + dr[1].ToString() + "','" + Convert.ToInt32(dr[2].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[2].ToString().Trim())) + "','" + Convert.ToInt32(dr[3].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[3].ToString().Trim())) + "','" + dr[4].ToString() + "','" + Convert.ToInt32(dr[5].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[5].ToString().Trim())) + "','" + dr[6].ToString() + "','" + dr[7].ToString() + "','" + dr[8].ToString() + "','" + dr[9].ToString() + "','" + dr[10].ToString() + "','" + dr[11].ToString() + "','" + dr[12].ToString() + "','" + dr[13].ToString() + "','" + dr[14].ToString() + "','" + dr[15].ToString() + "','" + dr[16].ToString() + "','" + Circle.ToString() + "','" + dr[18].ToString() + "','" + dr[19].ToString() + "','" + dr[20].ToString() + "','" + dr[21].ToString() + "','" + dr[22].ToString() + "','" + dr[23].ToString() + "','" + dr[24].ToString() + "','" + dr[25].ToString() + "','" + dr[26].ToString() + "'),";
                    }
                    else if (Table.ToLower().Contains("destination"))
                    {
                        QueryTail += "('" + dr[0].ToString() + "','" + Circle.ToString() + "','" + dr[2].ToString() + "','" + dr[3].ToString() + "','" + dr[4].ToString() + "','" + dr[5].ToString() + "','" + dr[6].ToString() + "','" + dr[7].ToString() + "','" + dr[8].ToString() + "','" + dr[9].ToString() + "','" + dr[10].ToString() + "','" + dr[11].ToString() + "'),";
                    }
                    else if (Table.ToLower().Contains("hlr"))
                    {
                        QueryTail += "('" + dr[0].ToString() + "','" + Circle.ToString() + "','" + dr[2].ToString() + "','" + dr[3].ToString() + "','" + dr[4].ToString() + "','" + dr[5].ToString() + "','" + dr[6].ToString() + "','" + dr[7].ToString() + "','" + dr[8].ToString() + "','" + dr[9].ToString() + "','" + dr[10].ToString() + "','" + dr[11].ToString() + "','" + dr[12].ToString() + "','" + dr[13].ToString() + "','" + dr[14].ToString() + "'),";
                    }
                    else if (Table.ToLower().Contains("msc"))
                    {
                        QueryTail += "('" + dr[0].ToString() + "','" + Circle.ToString() + "','" + dr[2].ToString() + "','" + dr[3].ToString() + "','" + dr[4].ToString() + "','" + dr[5].ToString() + "','" + dr[6].ToString() + "','" + dr[7].ToString() + "','" + dr[8].ToString() + "','" + dr[9].ToString() + "','" + dr[10].ToString() + "','" + dr[11].ToString() + "','" + dr[12].ToString() + "','" + dr[13].ToString() + "','" + dr[14].ToString() + "','" + dr[15].ToString() + "','" + dr[16].ToString() + "','" + dr[17].ToString() + "','" + dr[18].ToString() + "','" + dr[19].ToString() + "','" + dr[20].ToString() + "','" + dr[21].ToString() + "','" + dr[22].ToString() + "','" + dr[23].ToString() + "','" + dr[24].ToString() + "','" + dr[25].ToString() + "','" + dr[26].ToString() + "','" + dr[27].ToString() + "'),";
                    }
                    else if (Table.ToLower().Contains("mgw"))
                    {
                        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");

                        if (Convert.ToBoolean(dr[47].ToString()))
                        {
                            mgw_atca_flag = 1;
                            //code add by rahul on 7-08-2018                            
                            MGWATCA = dr[3].ToString().Trim().Replace("MGW_ATCA", "MGW");
                        }
                        else
                        {
                            MGWATCA = dr[3].ToString().Trim();
                        }
                        if (!regex.IsMatch(dr[23].ToString()))
                        {
                            if (string.IsNullOrEmpty(dr[23].ToString().Trim()))
                            {
                                QueryTail += "('" + dr[0].ToString().Trim() + "','" + Circle.ToString().Trim() + "','" + dr[2].ToString().Trim() + "','" + MGWATCA + "','" + dr[4].ToString().Trim() + "','" + dr[5].ToString().Trim() + "','" + dr[6].ToString().Trim() + "','" + dr[7].ToString().Trim() + "','" + dr[8].ToString().Trim() + "','" + dr[9].ToString() + "','" + dr[10].ToString().Trim() + "','" + dr[11].ToString() + "','" + dr[12].ToString().Trim() + "','" + dr[13].ToString() + "','" + dr[14].ToString() + "','" + dr[15].ToString().Trim() + "','" + dr[16].ToString().Trim() + "','" + dr[17].ToString().Trim() + "','" + dr[18].ToString().Trim() + "','" + dr[19].ToString().Trim() + "','" + dr[20].ToString().Trim() + "','" + dr[21].ToString().Trim() + "','" + dr[22].ToString().Trim() + "','" + dr[23].ToString().Trim() + "','" + dr[24].ToString().Trim() + "','" + dr[25].ToString().Trim() + "','" + dr[26].ToString().Trim() + "','" + dr[27].ToString().Trim() + "','" + dr[28].ToString().Trim() + "','" + dr[29].ToString().Trim() + "','" + dr[30].ToString().Trim() + "','" + dr[31].ToString().Trim() + "','" + dr[32].ToString().Trim() + "','" + dr[33].ToString().Trim() + "','" + dr[34].ToString().Trim() + "','" + dr[35].ToString().Trim() + "','" + dr[36].ToString().Trim() + "','" + dr[37].ToString().Trim() + "','" + dr[38].ToString().Trim() + "','" + dr[39].ToString() + "','" + dr[40].ToString() + "','" + dr[41].ToString() + "','" + dr[42].ToString() + "','" + dr[43].ToString() + "','" + dr[44].ToString().Trim() + "','" + dr[45].ToString().Trim() + "','" + dr[46].ToString() + "','" + mgw_atca_flag + "','" + dr[48].ToString().Trim() + "', '" + dr[49].ToString().Trim() + "','" + dr[50].ToString().Trim() + "','" + dr[51].ToString().Trim() + "','" + dr[52].ToString().Trim() + "','" + dr[53].ToString().Trim() + "', '" + dr[54].ToString().Trim() + "','" + dr[55].ToString().Trim() + "','" + dr[56].ToString().Trim() + "','" + dr[57].ToString().Trim() + "','" + dr[58].ToString().Trim() + "','" + dr[59].ToString().Trim() + "','" + dr[60].ToString().Trim() + "','" + dr[61].ToString().Trim() + "','" + dr[62].ToString().Trim() + "','" + dr[63].ToString().Trim() + "','" + dr[64].ToString().Trim() + "','" + dr[65].ToString().Trim() + "','" + dr[66].ToString().Trim() + "','" + dr[67].ToString().Trim() + "','" + dr[68].ToString().Trim() + "','" + dr[69].ToString().Trim() + "','" + dr[70].ToString().Trim() + "','" + dr[71].ToString().Trim() + "','" + dr[72].ToString().Trim() + "','" + dr[73].ToString().Trim() + "','" + dr[74].ToString().Trim() + "','" + dr[75].ToString().Trim() + "'),";


                                //QueryTail += "('" + dr[0].ToString().Trim() + "','" + Circle.ToString().Trim() + "','" + dr[2].ToString().Trim() + "','" + MGWATCA + "','" + Convert.ToInt32(dr[4].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[4].ToString().Trim())) + "','" + Convert.ToInt32(dr[5].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[5].ToString().Trim())) + "','" + dr[6].ToString().Trim() + "','" + dr[7].ToString().Trim() + "','" + Convert.ToInt32(dr[8].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[8].ToString().Trim())) + "','" + dr[9].ToString() + "','" + Convert.ToInt32(dr[10].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[10].ToString().Trim())) + "','" + dr[11].ToString() + "','" + Convert.ToInt32(dr[12].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[12].ToString().Trim())) + "','" + dr[13].ToString() + "','" + dr[14].ToString() + "','" + Convert.ToInt32(dr[15].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[15].ToString().Trim())) + "','" + dr[16].ToString().Trim() + "','" + dr[17].ToString().Trim() + "','" + dr[18].ToString().Trim() + "','" + dr[19].ToString().Trim() + "','" + dr[20].ToString().Trim() + "','" + dr[21].ToString().Trim() + "','" + Convert.ToInt32(dr[22].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[22].ToString().Trim())) + "','" + Convert.ToInt32(dr[23].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[23].ToString().Trim())) + "','" + dr[24].ToString().Trim() + "','" + dr[25].ToString().Trim() + "','" + dr[26].ToString().Trim() + "','" + dr[27].ToString().Trim() + "','" + dr[28].ToString().Trim() + "','" + dr[29].ToString().Trim() + "','" + dr[30].ToString().Trim() + "','" + dr[31].ToString().Trim() + "','" + dr[32].ToString().Trim() + "','" + dr[33].ToString().Trim() + "','" + dr[34].ToString().Trim() + "','" + dr[35].ToString().Trim() + "','" + dr[36].ToString().Trim() + "','" + dr[37].ToString().Trim() + "','" + dr[38].ToString().Trim() + "','" + dr[39].ToString() + "','" + dr[40].ToString() + "','" + dr[41].ToString() + "','" + dr[42].ToString() + "','" + dr[43].ToString() + "','" + dr[44].ToString().Trim() + "','" + Convert.ToInt32(dr[45].ToString().Trim() == "" ? 0 : Convert.ToInt32(dr[45].ToString().Trim())) + "','" + dr[46].ToString() + "','" + mgw_atca_flag + "','" + dr[48].ToString().Trim() + "', '" + dr[49].ToString().Trim() + "','" + dr[50].ToString().Trim() + "','" + dr[51].ToString().Trim() + "','" + dr[52].ToString().Trim() + "','" + dr[53].ToString().Trim() + "', '" + dr[54].ToString().Trim() + "','" + dr[55].ToString().Trim() + "','" + dr[56].ToString().Trim() + "','" + dr[57].ToString().Trim() + "','" + dr[58].ToString().Trim() + "','" + dr[59].ToString().Trim() + "','" + dr[60].ToString().Trim() + "','" + dr[61].ToString().Trim() + "','" + dr[62].ToString().Trim() + "','" + dr[63].ToString().Trim() + "','" + dr[64].ToString().Trim() + "','" + dr[65].ToString().Trim() + "','" + dr[66].ToString().Trim() + "','" + dr[67].ToString().Trim() + "'),";
                            }
                            else
                            {
                                QueryTail += "('" + dr[0].ToString().Trim() + "','" + Circle.ToString().Trim() + "','" + dr[2].ToString().Trim() + "','" + MGWATCA + "','" + dr[4].ToString().Trim() + "','" + dr[5].ToString().Trim() + "','" + dr[6].ToString().Trim() + "','" + dr[7].ToString().Trim() + "','" + dr[8].ToString().Trim() + "','" + dr[9].ToString().Trim() + "','" + dr[10].ToString().Trim() + "','" + dr[11].ToString().Trim() + "','" + dr[12].ToString().Trim() + "','" + dr[13].ToString().Trim() + "','" + dr[14].ToString().Trim() + "','" + dr[15].ToString().Trim() + "','" + dr[16].ToString().Trim() + "','" + dr[17].ToString().Trim() + "','" + dr[18].ToString().Trim() + "','" + dr[19].ToString().Trim() + "','" + dr[20].ToString().Trim() + "','" + dr[21].ToString().Trim() + "','" + dr[22].ToString().Trim() + "','" + dr[23].ToString().Trim() + "','" + dr[24].ToString().Trim() + "','" + dr[25].ToString().Trim() + "','" + dr[26].ToString().Trim() + "','" + dr[27].ToString().Trim() + "','" + dr[28].ToString().Trim() + "','" + dr[29].ToString().Trim() + "','" + dr[30].ToString().Trim() + "','" + dr[31].ToString().Trim() + "','" + dr[32].ToString().Trim() + "','" + dr[33].ToString().Trim() + "','" + dr[34].ToString().Trim() + "','" + dr[35].ToString().Trim() + "','" + dr[36].ToString().Trim() + "','" + dr[37].ToString().Trim() + "','" + dr[38].ToString().Trim() + "','" + dr[39].ToString().Trim() + "','" + dr[40].ToString().Trim() + "','" + dr[41].ToString().Trim() + "','" + dr[42].ToString().Trim() + "','" + dr[43].ToString().Trim() + "','" + dr[44].ToString().Trim() + "','" + dr[45].ToString().Trim() + "','" + dr[46].ToString().Trim() + "','" + mgw_atca_flag + "','" + dr[48].ToString().Trim() + "','" + dr[49].ToString().Trim() + "','" + dr[50].ToString().Trim() + "','" + dr[51].ToString().Trim() + "','" + dr[52].ToString().Trim() + "','" + dr[53].ToString().Trim() + "', '" + dr[54].ToString().Trim() + "','" + dr[55].ToString().Trim() + "','" + dr[56].ToString().Trim() + "','" + dr[57].ToString().Trim() + "','" + dr[58].ToString().Trim() + "','" + dr[59].ToString().Trim() + "','" + dr[60].ToString().Trim() + "','" + dr[61].ToString().Trim() + "','" + dr[62].ToString().Trim() + "','" + dr[63].ToString().Trim() + "','" + dr[64].ToString().Trim() + "','" + dr[65].ToString().Trim() + "','" + dr[66].ToString().Trim() + "','" + dr[67].ToString().Trim() + "','" + dr[68].ToString().Trim() + "','" + dr[69].ToString().Trim() + "','" + dr[70].ToString().Trim() + "','" + dr[71].ToString().Trim() + "','" + dr[72].ToString().Trim() + "','" + dr[73].ToString().Trim() + "','" + dr[74].ToString().Trim() + "','" + dr[75].ToString().Trim() + "'),";
                            }
                        }
                        else
                        {
                            QueryTail += "('" + dr[0].ToString().Trim() + "','" + Circle.ToString().Trim() + "','" + dr[2].ToString().Trim() + "','" + MGWATCA + "','" + dr[4].ToString().Trim() + "','" + dr[5].ToString().Trim() + "','" + dr[6].ToString().Trim() + "','" + dr[7].ToString().Trim() + "','" + dr[8].ToString().Trim() + "','" + dr[9].ToString().Trim() + "','" + dr[10].ToString().Trim() + "','" + dr[11].ToString() + "','" + dr[12].ToString().Trim() + "','" + dr[13].ToString().Trim() + "','" + dr[14].ToString().Trim() + "','" + dr[15].ToString().Trim() + "','" + dr[16].ToString().Trim() + "','" + dr[17].ToString().Trim() + "','" + dr[18].ToString().Trim() + "','" + dr[19].ToString().Trim() + "','" + dr[20].ToString().Trim() + "','" + dr[21].ToString().Trim() + "','" + dr[22].ToString().Trim() + "','" + dr[23].ToString().Trim() + "','" + dr[24].ToString().Trim() + "','" + dr[25].ToString().Trim() + "','" + dr[26].ToString().Trim() + "','" + dr[27].ToString().Trim() + "','" + dr[28].ToString().Trim() + "','" + dr[29].ToString().Trim() + "','" + dr[30].ToString().Trim() + "','" + dr[31].ToString().Trim() + "','" + dr[32].ToString().Trim() + "','" + dr[33].ToString().Trim() + "','" + dr[34].ToString().Trim() + "','" + dr[35].ToString().Trim() + "','" + dr[36].ToString().Trim() + "','" + dr[37].ToString().Trim() + "','" + dr[38].ToString().Trim() + "','" + dr[39].ToString().Trim() + "','" + dr[40].ToString().Trim() + "','" + dr[41].ToString().Trim() + "','" + dr[42].ToString().Trim() + "','" + dr[43].ToString().Trim() + "','" + dr[44].ToString().Trim() + "','" + dr[45].ToString().Trim() + "','" + dr[46].ToString().Trim() + "','" + mgw_atca_flag + "','" + dr[48].ToString().Trim() + "', '" + dr[49].ToString().Trim() + "','" + dr[50].ToString().Trim() + "','" + dr[51].ToString().Trim() + "','" + dr[52].ToString().Trim() + "','" + dr[53].ToString().Trim() + "', '" + dr[54].ToString().Trim() + "','" + dr[55].ToString().Trim() + "','" + dr[56].ToString().Trim() + "','" + dr[57].ToString().Trim() + "','" + dr[58].ToString().Trim() + "','" + dr[59].ToString().Trim() + "','" + dr[60].ToString().Trim() + "','" + dr[61].ToString().Trim() + "','" + dr[62].ToString().Trim() + "','" + dr[63].ToString().Trim() + "','" + dr[64].ToString().Trim() + "','" + dr[65].ToString().Trim() + "','" + dr[66].ToString().Trim() + "','" + dr[67].ToString().Trim() + "','" + dr[68].ToString().Trim() + "','" + dr[69].ToString().Trim() + "','" + dr[70].ToString().Trim() + "','" + dr[71].ToString().Trim() + "','" + dr[72].ToString().Trim() + "','" + dr[73].ToString().Trim() + "','" + dr[74].ToString().Trim() + "','" + dr[75].ToString().Trim() + "'),";
                        }
                        mgw_atca_flag = 0;
                    }
                    else if (Table.ToLower().Contains("mss_link"))
                    {
                        QueryTail += "('" + dr[0].ToString() + "','" + Circle.ToString() + "','" + dr[2].ToString() + "','" + dr[3].ToString() + "','" + dr[4].ToString() + "','" + dr[5].ToString() + "','" + dr[6].ToString() + "','" + dr[7].ToString() + "','" + dr[8].ToString() + "','" + dr[9].ToString() + "','" + dr[10].ToString() + "','" + dr[11].ToString() + "','" + dr[12].ToString() + "','" + dr[13].ToString() + "','" + dr[14].ToString() + "'),";
                    }
                    //code added by rahul on 27-08-2018
                    else if (Table.ToLower().Contains("subtrack_mss"))
                    {
                        QueryTail += "('" + dr[0].ToString() + "','" + Circle.ToString() + "','" + dr[2].ToString() + "','" + dr[3].ToString() + "','" + dr[4].ToString() + "','" + dr[5].ToString() + "','" + dr[6].ToString() + "','" + dr[7].ToString() + "','" + mss_atca_flag + "'),";
                    }
                    if (count == 999)
                    {
                        string Query = QueryHead + QueryTail.TrimEnd(',');
                        ExecuteSqlQuery(ref sqlConn, Query);
                        QueryTail = string.Empty;
                        count = 0;
                    }
                    else
                        count++;
                }
                if (QueryTail != string.Empty)
                {
                    string Query = QueryHead + QueryTail.TrimEnd(',');
                    ExecuteSqlQuery(ref sqlConn, Query);
                    QueryTail = string.Empty;
                    count = 0;
                }

            }
            catch (Exception ex)
            {
                if (!Directory.Exists(errorFolderPath))
                {
                    Directory.CreateDirectory(errorFolderPath);
                }
                File.AppendAllText(errorFolderPath + "\\" + Circle + "_error_log.txt", ex.Message.ToString());
            }
        }
        public static void MySqlQuery(ref MySqlConnection mySqlConn, ref DataTable dt, string Query)
        {
            int retrycount = 0;
            string ErrorMsg = "";
            string FileError = "";
            MySqlDataAdapter ADAPTCombo = new MySqlDataAdapter();
            MySqlCommand mMySqlCommand = new MySqlCommand();

            mMySqlCommand.CommandText = Query;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = mySqlConn;
            mMySqlCommand.CommandTimeout = 900000;






            //if (MYSQL_connection.State != ConnectionState.Open)
            //{
            //    return false;
            //}
            ADAPTCombo.SelectCommand = mMySqlCommand;
        retry:
            try
            {
                if (mySqlConn.State != ConnectionState.Open)
                {
                    mySqlConn.Open();

                }
                if (retrycount > 0)
                {
                    dt = null;
                    dt = new DataTable();
                }
                if (retrycount <= 4)
                {
                    ADAPTCombo.Fill(dt);
                }

            }
            catch (Exception ex)
            {
                retrycount = retrycount + 1;
                if (retrycount == 5)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString() + ".Query:" + Query;
                    oExceptionLog.WriteExceptionErrorToFile("MySqlQuery()", ErrorMsg, "", ref FileError);
                    return;
                }
                goto retry;
            }
            finally
            {
                if (mySqlConn.State == ConnectionState.Open)
                {
                    mySqlConn.Close();
                }
            }

        }

        public static void ExecuteSqlQuery(ref SqlConnection sqlConn, string Query)
        {
            SqlCommand scmd = new SqlCommand();
            SqlTransaction transaction;
            scmd.CommandType = CommandType.Text;
            scmd.CommandText = Query;
            scmd.CommandTimeout = 900000;
            scmd.Connection = sqlConn;

            string ErrorMsg = "";
            string FileError = "";
            int retrycount = 0;
        retry:
            try
            {
                if (sqlConn.State != ConnectionState.Open)
                {
                    sqlConn.Open();
                }
                transaction = sqlConn.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    scmd.Transaction = transaction;
                    if (retrycount <= 4)
                    {
                        scmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    //return true;
                }
                catch (Exception ex)
                {
                    retrycount = retrycount + 1;
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (SqlException e)
                    {
                        if (transaction.Connection != null)
                        {
                            ErrorMsg = "An exception of type " + e.GetType() + " was encountered while attempting to roll back the transaction.";
                            FileError = "";
                            oExceptionLog.WriteExceptionErrorToFile("ExecuteSQLQuery()", ErrorMsg, "", ref FileError);
                        }
                    }
                    if (retrycount == 5)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString() + ".Query:" + Query;
                        FileError = "";
                        oExceptionLog.WriteExceptionErrorToFile("ExecuteSQLQuery()", ErrorMsg, "", ref FileError);
                        return;
                    }
                    goto retry;
                }
                finally
                {
                    if (sqlConn.State == ConnectionState.Open)
                    {
                        sqlConn.Close();
                    }
                }
            }
            catch (SqlException e1)
            {
                if (retrycount == 5)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e1.Message.ToString() + ".Query:" + Query;
                    FileError = "";
                    oExceptionLog.WriteExceptionErrorToFile("ExecuteSQLQuery()", ErrorMsg, "", ref FileError);
                    return;
                }
                goto retry;
            }
        }


        public static bool ExecuteMySqlQuery(ref MySqlConnection sqlConn, string Query)
        {
            //MySqlCommand scmd = new MySqlCommand();
            //scmd.CommandType = CommandType.Text;
            //scmd.CommandText = Query;
            //scmd.CommandTimeout = 1000;
            //scmd.Connection = sqlConn;
            //try
            //{
            //    if (sqlConn.State != ConnectionState.Open)
            //        sqlConn.Open();
            //    scmd.ExecuteNonQuery();

            //}
            //catch (Exception exc)
            //{
            //    //throw exc;
            //}
            //finally
            //{
            //    sqlConn.Close();
            //}
            string ErrorMsg = "";
            string FileError = "";
            int retrycount = 0;
            MySqlCommand mMySqlCommand = new MySqlCommand();
            MySqlTransaction transaction;
            mMySqlCommand.CommandText = Query;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = sqlConn;
            mMySqlCommand.CommandTimeout = 900000;


        retry:
            try
            {
                if (sqlConn.State != ConnectionState.Open)
                {
                    sqlConn.Open();
                }
                transaction = sqlConn.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    if (retrycount <= 4)
                    {
                        mMySqlCommand.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    retrycount = retrycount + 1;
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (SqlException e)
                    {
                        if (transaction.Connection != null)
                        {
                            ErrorMsg = "An exception of type " + e.GetType() + " was encountered while attempting to roll back the transaction.";
                            FileError = "";
                            oExceptionLog.WriteExceptionErrorToFile("ExecuteMySqlQuery()", ErrorMsg, "", ref FileError);
                            return false;
                        }
                    }
                    if (retrycount == 5)
                    {
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString() + ".Query:" + Query;
                        FileError = "";
                        oExceptionLog.WriteExceptionErrorToFile("ExecuteMySqlQuery()", ErrorMsg, "", ref FileError);
                        //return false;
                        return false;

                    }
                    goto retry;
                }
                finally
                {
                    if (sqlConn.State == ConnectionState.Open)
                    {
                        sqlConn.Close();
                    }
                }
            }
            catch (Exception e1)
            {
                if (retrycount == 5)
                {
                    ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + e1.Message.ToString() + ".Query:" + Query;
                    FileError = "";
                    oExceptionLog.WriteExceptionErrorToFile("ExecuteMySqlQuery()", ErrorMsg, "", ref FileError);
                    //return false;
                    return false;

                }
                goto retry;
            }
        }
    }
}
