Imports NSN.GNOCN.Tools.SQL.DbLayer
Imports NSN.GNOCN.Tools.AWE.NSS

Namespace NSN.GNOCN.Tools.AWE.NSS

    Public Class AWE_NSS_DCM_PS

        Private objSourcedbPS As Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer
        Private objPSprocess As New clsProcess(System.Configuration.ConfigurationSettings.AppSettings("PSDBConstring").ToString())
        Private PSConstring As String = System.Configuration.ConfigurationSettings.AppSettings("PSDBConstring").ToString()
        Private WithEvents tmrPACO As System.Timers.Timer
        Private WithEvents tmrSQLUploadPS As System.Timers.Timer
        Private WithEvents tmrSQLUploadCS As System.Timers.Timer
        Private objSourcedbCS As Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer
        Private CSConstring As String = System.Configuration.ConfigurationSettings.AppSettings("CSDBConstring").ToString()


        Protected Overrides Sub OnStart(ByVal args() As String)
            Try
                LogEvent("AWE DCM PACO Sercice getting started...", EventLogEntryType.Information)

                'Debugger.Launch()
                objSourcedbPS = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(PSConstring, DbType.MySqlType)
                objSourcedbCS = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(CSConstring, DbType.MySqlType)

                'tmrPACO = New System.Timers.Timer

                'tmrPACO.Interval = 10000

                'tmrPACO.AutoReset = True

                'tmrPACO.Enabled = True

                'tmrPACO.Start()

                'tmrSQLUploadPS = New System.Timers.Timer

                'tmrSQLUploadPS.Interval = 10000

                'tmrSQLUploadPS.AutoReset = True

                'tmrSQLUploadPS.Enabled = True

                'tmrSQLUploadPS.Start()

                tmrSQLUploadCS = New System.Timers.Timer

                tmrSQLUploadCS.Interval = 10000

                tmrSQLUploadCS.AutoReset = True

                tmrSQLUploadCS.Enabled = True

                tmrSQLUploadCS.Start()

                LogEvent("AWE DCM PACO Upload service started successfully...", EventLogEntryType.Information)
            Catch ex As Exception
                LogEvent("AWE DCM PACO Upload service [Error while starting the service: " & ex.Message & "]", EventLogEntryType.Error)
            End Try
        End Sub

        Protected Overrides Sub OnStop()
            Try
                LogEvent("AWE DCM PACO Upload service stoping...", EventLogEntryType.Information)
                LogEvent("AWE DCM PACO Upload service stoped successfully...", EventLogEntryType.Information)
            Catch ex As Exception
                LogEvent("AWE DCM PACO Upload service [Error while stoping the service: " & ex.Message & "]", EventLogEntryType.Error)
            End Try
        End Sub
        Public Sub LogEvent(ByVal sMessage As String, ByVal EntryType As System.Diagnostics.EventLogEntryType)
            Try
                Dim oEventLog As EventLog = New EventLog("DCM_Upload")
                If Not Diagnostics.EventLog.SourceExists("DCM_Upload") Then
                    Diagnostics.EventLog.CreateEventSource("DCM_Upload", "DCM_Upload")
                End If
                Diagnostics.EventLog.WriteEntry("DCM_Upload", sMessage, EntryType)
            Catch e As Exception
            End Try
        End Sub
        Private Sub tmrSQLUploadPS_Elapsed(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrSQLUploadPS.Elapsed
            Dim errormsg As String = String.Empty
            Try
                tmrSQLUploadPS.Enabled = False
                LogEvent("Timer of PACO Upload process is starting...", EventLogEntryType.Information)
                If (System.DateTime.Now.ToString("HH:mm") = System.Configuration.ConfigurationManager.AppSettings("UploadPSToSQLStartTime").ToString()) Then


                    objSourcedbPS.ExecuteNonQuery("Update tblProcessUpdate Set starttime='" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "',EndTime='0000-00-00 00:00:00',Status='NOT COMPLETED'  Where DCMProcess='NSS' ")

                    LogEvent("PACO Upload process has been started...", EventLogEntryType.Information)
                    SQLUploaPSdMainFunction(errormsg)
                    LogEvent("PACO process has been ended...", EventLogEntryType.Information)
                    If (errormsg = String.Empty) Then
                        objSourcedbPS.ExecuteNonQuery("Update tblProcessUpdate Set EndTime='" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "',Status='COMPLETED',ErrorMsg='" & errormsg & "' Where DCMProcess='NSS' ")
                    Else
                        objSourcedbPS.ExecuteNonQuery("Update tblProcessUpdate Set EndTime='" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "',Status='NOT COMPLETED',ErrorMsg='" & errormsg & "' Where DCMProcess='NSS' ")
                    End If
                    If errormsg.Length > 0 Then
                        LogEvent("Error while Uploading AWE DCM PACO data (Error in tmrSQLUpload)... ERROR: " & errormsg, EventLogEntryType.Error)
                    End If
                End If
            Catch ex As Exception
                LogEvent("Error while Uploading AWE DCM PACO data (Error in tmrSQLUpload)... ERROR: " & ex.Message, EventLogEntryType.Error)
            Finally
                tmrSQLUploadPS.Enabled = True
            End Try
        End Sub
        Private Sub tmrSQLUploadCS_Elapsed(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrSQLUploadCS.Elapsed
            Dim errormsg As String = String.Empty
            Try
                tmrSQLUploadCS.Enabled = False
                LogEvent("Timer of CS Upload process is starting...", EventLogEntryType.Information)
                Dim dtcircle As DataTable = Nothing
                Dim dt As DataTable = Nothing
                dtcircle = objSourcedbCS.ExecuteSQL("SELECT distinct circle FROM tblprocessupdate")
                If dtcircle IsNot Nothing Then
                    If dtcircle.Rows.Count > 0 Then
                        For Each dr As DataRow In dtcircle.Rows
                            dt = Nothing
                            dt = objSourcedbCS.ExecuteSQL("SELECT CIRCLE FROM tblprocessupdate where status='NOT COMPLETED' and circle = '" & dr("Circle").ToString.Trim & "'")
                            If dt IsNot Nothing Then
                                If dt.Rows.Count > 0 Then
                                    objSourcedbCS.ExecuteNonQuery("Update tblprocessupdate Set status='In Progress',starttime='" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "',EndTime='0000-00-00 00:00:00' Where circle='" & dr("Circle").ToString.Trim & "'")
                                    SQLUploaCSdMainFunction(errormsg, dr("Circle").ToString.Trim)
                                    objSourcedbCS.ExecuteNonQuery("Update tblprocessupdate Set status='COMPLETED',EndTime='" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "' Where circle='" & dr("Circle").ToString.Trim & "'")
                                End If
                            End If
                        Next
                    End If
                End If
                Threading.Thread.Sleep(300000)
                If errormsg.Length > 0 Then
                    LogEvent("Error while Uploading AWE DCM CS data (Error in tmrSQLUploadCS)... ERROR: " & errormsg, EventLogEntryType.Error)
                End If
            Catch ex As Exception
                LogEvent("Error while Uploading AWE DCM CS data (Error in tmrSQLUpload)... ERROR: " & ex.Message, EventLogEntryType.Error)
            Finally
                tmrSQLUploadCS.Enabled = True
            End Try
        End Sub


        Public Sub SQLUploaPSdMainFunction(ByRef errormsg As String)
            Try
                clsTransferToNSSFromPS.dcm_update()
            Catch ex As Exception
                LogEvent("Error while Uploading AWE DCM PACO data (Error in SQLUploadMainFunction)... ERROR: " & ex.Message, EventLogEntryType.Error)
            End Try
        End Sub
        Public Sub SQLUploaCSdMainFunction(ByRef errormsg As String, ByVal circle As String)
            Try
                clsTransferToNSSFromCS.DCMDataMigration(Circle)
            Catch ex As Exception
                LogEvent("Error while Uploading AWE DCM PACO data (Error in SQLUploadMainFunction)... ERROR: " & ex.Message, EventLogEntryType.Error)
            End Try
        End Sub
    End Class
End Namespace
