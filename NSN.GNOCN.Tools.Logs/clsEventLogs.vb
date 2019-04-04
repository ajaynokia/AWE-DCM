Namespace NSN.GNOCN.Tools.Logs

    Public Class EventLogs
        Public Sub LogEvent(ByVal sMessage As String, ByVal strSource As String, ByVal EntryType As System.Diagnostics.EventLogEntryType)
            Try
                Dim oEventLog As EventLog = New EventLog("NSN.GNOCN.Tools")
                If Not Diagnostics.EventLog.SourceExists(strSource) Then
                    Diagnostics.EventLog.CreateEventSource(strSource, "NSN.GNOCN.Tools")
                End If
                Diagnostics.EventLog.WriteEntry(strSource, sMessage, EntryType)
            Catch e As Exception
                Throw e
            End Try
        End Sub

        Public Sub TopicLogEvent(ByVal sMessage As String, ByVal strSource As String, ByVal EntryType As System.Diagnostics.EventLogEntryType)
            Try
                Dim oEventLog As EventLog = New EventLog("NSN.iHUB_TOPIC")
                If Not Diagnostics.EventLog.SourceExists(strSource) Then
                    Diagnostics.EventLog.CreateEventSource(strSource, "NSN.iHUB_TOPIC")
                End If
                Diagnostics.EventLog.WriteEntry(strSource, sMessage, EntryType)
            Catch e As Exception
                Throw e
            End Try
        End Sub
    End Class
End Namespace



''using System;
''using System.Collections.Generic;
''using System.Linq;
''using System.Text;
''using System.Diagnostics;


''Namespace NSN.GNOCN.TOOLS.PT4.Log
''{
''    public class clsLog:IDisposable 
''    {

''        EventLog oEventLog;
''        public void LogEvent(string sMessage, System.Diagnostics.EventLogEntryType EntryType)
''        {
''            try
''            {
''                oEventLog = new EventLog("NSN.GNOCN.Tools");
''                if (!System.Diagnostics.EventLog.SourceExists("AWEPT4Process"))
''                {
''                    System.Diagnostics.EventLog.CreateEventSource("AWEPT4Process", "NSN.GNOCN.Tools");
''                }
''                System.Diagnostics.EventLog.WriteEntry("AWEPT4Process", sMessage, EntryType);
''            }
''            catch (Exception ex)
''            {
''                throw ex;
''            }
''        }

''        public void TopicLogEvent(string sMessage, System.Diagnostics.EventLogEntryType EntryType)
''        {
''            try
''            {
''                oEventLog = new EventLog("PT4 Topic Reading Logs");
''                if (!System.Diagnostics.EventLog.SourceExists("Topic Updates Logs"))
''                {
''                    System.Diagnostics.EventLog.CreateEventSource("Topic Updates Logs", "PT4 Topic Reading Logs");
''                }
''                System.Diagnostics.EventLog.WriteEntry("Topic Updates Logs", sMessage, EntryType);
''            }
''            catch (Exception ex)
''            {
''                throw ex;
''            }
''        }


''        public void Dispose()
''        {
''            try
''            {

''                oEventLog.Dispose();
''                this.Dispose();
''            }
''            catch (Exception ex)
''            {
''                throw ex;
''            }

''        }
''    }
''}
