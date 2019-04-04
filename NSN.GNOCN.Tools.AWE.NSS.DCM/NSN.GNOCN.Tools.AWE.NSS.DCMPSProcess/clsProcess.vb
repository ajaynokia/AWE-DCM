Imports NSN.GNOCN.Tools.SQL.DbLayer

Namespace NSN.GNOCN.Tools.AWE.NSS

    Public Class clsProcess

        Private _client As String = String.Empty
        Private _CircleName As String = String.Empty
        Private _vendor As String = String.Empty
        Private _NEType As String = String.Empty
        Private objSourcedb As Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer
        Dim objProcess As clsPSProcess
        Sub New(ByVal ConnectionString)
            objSourcedb = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(ConnectionString, DbType.MySqlType)
            objProcess = New clsPSProcess(ConnectionString)
        End Sub
        Public Sub StartMain()
            Try


                _CircleName = String.Empty
                _vendor = System.Configuration.ConfigurationManager.AppSettings("Vendor").ToString()
                _client = System.Configuration.ConfigurationManager.AppSettings("Client").ToString()

                Dim dtClient As DataTable = Nothing
                Dim _clientConString As String = System.Configuration.ConfigurationManager.AppSettings("PSDBConstring").ToString()
                'Dim _clientConString As String = System.Configuration.ConfigurationManager.AppSettings("ClientConString").ToString()
                'Dim _clientConString As String = "Server=93.183.30.216;Port=3306;Database=awe_paco_dcm;Uid=root;Pwd=tiger"

                Dim DTrouter As DataTable = Nothing
                Dim Type() As String = {"SGSN", "GGSN", "CMD"}
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.circle_master set Connection_String='', PT4_Profile='', Task_Profile=''")
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='',ERRORMSG='',REMARKS='' where Rtr_ActiveFlag is true")
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='',ERRORMSG='',REMARKS='' where NE_ActiveFlag is true")
                objSourcedb.ExecuteNonQuery("delete from awe_paco_dcm.flag_update")
                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.flag_update (START_TIME, END_TIME, BOOL, CMD_SCANNER) values('" + System.DateTime.Now.ToString() + "','NULL','NULL','NULL')")
                '********************************************************DATA BACKUP********************************************************************************************
                'BACKUP data deletion
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_backup ")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblsgsn_nsvci_backup")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_update_backup")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_up_apndata_backup")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblnd111_backup")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblmaster_details_backup")
                'data insertion for BACKUP
                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblcoremap_backup select * FROM awe_paco_dcm.tblcoremap")
                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblsgsn_nsvci_backup select * FROM awe_paco_dcm.tblsgsn_nsvci")
                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblcoremap_update_backup select * FROM awe_paco_dcm.tblcoremap_update")
                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblcoremap_up_apndata_backup select * FROM awe_paco_dcm.tblcoremap_up_apndata;")
                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblnd111_backup select * from awe_paco_dcm.tblnd111")
                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblmaster_details_backup select * from awe_paco_dcm.tblmaster_details")
                'data deletion for new data
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap ")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblsgsn_nsvci")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_update")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_up_apndata")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblnd111")
                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblmaster_details")
                '********************************************************DATA BACKUP********************************************************************************************

                For i As Integer = 0 To Type.Length - 1
                    dtClient = objSourcedb.ExecuteSQL("SELECT distinct circle FROM awe_paco_dcm.router_master where domain='PS' and node_type='" + Type(i).ToString() + "'")
                    _NEType = Type(i).ToString()
                    For Each dr As DataRow In dtClient.Rows
                        _CircleName = dr("circle")
                        DTrouter = objSourcedb.ExecuteSQL(" select distinct Rtr_Name from awe_paco_dcm.ne_master where  Rtr_Name<>'' and node_type='" & _NEType & "' and circle='" & _CircleName & "'")
                        objProcess.NE_Type = _NEType
                        objProcess.CircleName = _CircleName
                        objProcess.Vendor = _vendor

                        For Each drRouter As DataRow In DTrouter.Rows
                            objProcess.RouterName = drRouter("Rtr_Name")
                            objProcess.AutoUpdateNSEIMapping()
                            'FetchData(_CircleName, "", routerName)
                            System.Threading.Thread.Sleep(1000)
                        Next
                        System.Threading.Thread.Sleep(1000)
                    Next
                    System.Threading.Thread.Sleep(1000)
                Next

                'changing by manish 
                '-----------------------------------------Here Call ND111 function with oracal connectivity----------------------------------
                'Note :- Use for ND111 
                System.Threading.Thread.Sleep(1000)
                objProcess.ND111()

                '------------------------------------------End One Circle and Start another circle-------------------------------------------- 
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.flag_update set  END_TIME='" + System.DateTime.Now.ToString() + "', BOOL='True', CMD_SCANNER='True'")

                System.Threading.Thread.Sleep(5000)
            Catch ex As Exception
                'Throw ex
            End Try
        End Sub
    End Class
End Namespace