Imports System.IO
Imports System.Windows.Forms

Namespace NSN.GNOCN.Tools.Logs

    Public Class clsExceptionLogs
        Shared _strFileNameRemark As String
        Public Sub New(ByVal strFileNameRemark As String)
            _strFileNameRemark = strFileNameRemark
        End Sub

        Public Sub WriteExceptionErrorToFile(ByVal pProcedureNameString As String, _
                                        ByVal pWriteErrorMessageString As String, _
                                        ByVal pWriteCustomErrorMessage As String, _
                                        ByRef pErrorMessageString As String)
            Try
                Dim pFileNamePathString As String = Application.StartupPath & "\" & "log" & "\" & "Exception_" & Now().ToString("yyyy_MM_dd") & "_" & _strFileNameRemark & ".log"
                WriteExceptionErrorToFile(pFileNamePathString, pProcedureNameString, pWriteErrorMessageString, pWriteCustomErrorMessage, pErrorMessageString)
            Catch ex As Exception
                '' not able to handel this exception...
            End Try
        End Sub

        Private Sub WriteExceptionErrorToFile(ByVal pFileNamePathString As String, _
                                            ByVal pProcedureNameString As String, _
                                            ByVal pWriteErrorMessageString As String, _
                                            ByVal pWriteCustomErrorMessage As String, _
                                            ByRef pErrorMessageString As String)

            Static intRecCount As Integer = 0

            If intRecCount > 2 Then
                Exit Sub
            Else
                intRecCount = intRecCount + 1
            End If

            Dim ExceptionMessageString As String
            If Not System.IO.Directory.Exists(Path.GetDirectoryName(pFileNamePathString)) Then
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(pFileNamePathString))
            End If
            Try
                Dim ObjectFileStream As New FileStream(pFileNamePathString, FileMode.Append, FileAccess.Write, FileShare.None)
                Dim ObjectStreamWriter As New StreamWriter(ObjectFileStream)
                ExceptionMessageString = "[" & Now().ToString & "] - " & _
                                            pWriteCustomErrorMessage & _
                                            "Procedure:[" & pProcedureNameString & "] - " & _
                                            "Error Message:[" & pWriteErrorMessageString & "]."
                ObjectStreamWriter.WriteLine(ExceptionMessageString)
                If Not ObjectStreamWriter Is Nothing Then
                    ObjectStreamWriter.Flush()
                    ObjectStreamWriter.Close()
                End If
                If Not ObjectFileStream Is Nothing Then
                    ObjectFileStream.Close()
                End If
            Catch exError As Exception
                pErrorMessageString = "Unable to write to Application Log File. Contact your Application Administrator." & exError.Message
                Dim pFileNamePathStringError As String = Application.StartupPath & "\" & "log" & "\" & "Exception_" & Now().ToString("yyyy_MM_dd") & "_" & _strFileNameRemark & "_" & TimeOfDay.Ticks & " .log"
                WriteExceptionErrorToFile(pFileNamePathStringError, pProcedureNameString, pWriteErrorMessageString, pErrorMessageString)
            End Try
        End Sub

    End Class

End Namespace
