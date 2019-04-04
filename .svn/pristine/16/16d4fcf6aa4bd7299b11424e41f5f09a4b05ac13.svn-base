Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Diagnostics
Imports System.Data.Common
Imports MySql.Data.MySqlClient
Imports NSN.GNOCN.Tools.Logs

Namespace NSN.GNOCN.Tools.SQL.DbLayer


    Public Class DbConnectLayer
        Inherits MarshalByRefObject

        'Declare globle Variables
        Private oExceptionLog As clsExceptionLogs
        Private _strConnectionString As String = String.Empty
        Private DbType As DbType
        Private objFact As DbFactory

        ''private MySqlConnection _conPsgSQL = new MySqlConnection();
#Region "Constructors"
        Sub New(ByVal ConnectionString As String, ByVal _DbType As DbType)
            objFact = New DbFactory()
            DbType = _DbType
            _strConnectionString = ConnectionString
            oExceptionLog = New clsExceptionLogs("DCM_PACO")
        End Sub
        Sub New(ByVal ConnectionString As String, ByVal _DbType As String)
            Select Case (_DbType)
                Case "MYSQL" : DbType = DbType.MySqlType
                    Exit Select
                Case "ORACLE" : DbType = DbType.OracleType
                    Exit Select
                Case "SQL" : DbType = DbType.OracleType
                    Exit Select
                Case Else : Throw New InvalidCastException()

            End Select
            _strConnectionString = ConnectionString
        End Sub
#End Region

        Sub New()
            ' TODO: Complete member initialization 
        End Sub

        ' <summary>
        ' Implement Funtion declared in IDBLayer for executing query 
        ' </summary>
        ' <param name="Query"></param>
        ' <returns>Dt</returns>
        ' <remarks></remarks>
        Public Function ExecuteSQL(ByVal Query As String) As System.Data.DataTable
            Dim da As DbDataAdapter = Nothing
            'MySqlDataAdapter da = default(MySqlDataAdapter);
            Dim Dt As System.Data.DataTable = Nothing
            Try
                If (Dt Is Nothing) Then
                    Dt = New DataTable()
                End If
                da = objFact.CreateInstanceDataAdapter(DbType, Query, _strConnectionString)
                da.Fill(Dt)
                Return Dt
            Catch ex As Exception
                Throw ex
            End Try
        End Function
        Public Function GetDataset(ByVal Query) As System.Data.DataSet
            Dim da As System.Data.SqlClient.SqlDataAdapter = Nothing
            Dim Ds As System.Data.DataSet = Nothing
            Dim con As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(_strConnectionString)
            Try
                If (con.State = ConnectionState.Closed) Then
                    con.Open()
                End If
                If Ds Is Nothing Then
                    Ds = New DataSet()
                End If
                Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand(Query, con)
                da = New System.Data.SqlClient.SqlDataAdapter(cmd)
                cmd.CommandTimeout = 1000 * 60 * 15
                da.Fill(Ds)
                Return Ds
            Catch ex As Exception
                Return Nothing
            Finally
                If (con.State = ConnectionState.Open) Then
                    con.Close()
                End If
            End Try
        End Function


          
        ' <summary>
        '
        '</summary>
        '<param name="Query"></param>
        '<returns>Dt</returns>
        '<remarks></remarks>
        Public Function ExecuteSQL(ByVal Query As String, ByRef ErrMsg As String) As System.Data.DataTable
            Dim da As DbDataAdapter = Nothing
            Dim Dt As System.Data.DataTable = Nothing
            Try
                If (Dt Is Nothing) Then
                    Dt = New DataTable()
                End If
                da = objFact.CreateInstanceDataAdapter(DbType, Query, _strConnectionString)
                da.Fill(Dt)
                Return Dt
            Catch ex As Exception
                ErrMsg = ex.Message

                Throw ex
            End Try
        End Function




        ' <summary>
        ' This function is used for Executing query with DataAdapter
        ' </summary>
        ' <param name="Query">Query as string</param>
        ' <param name="Dadp">Dadp as as Datadapter</param>
        ' <returns>It returns as DataTable</returns>
        ' <remarks></remarks>
        'public System.Data.DataTable ExecuteSQL(string Query, ref object Dadp)
        '{
        '//    MySqlDataAdapter da = default(MySqlDataAdapter);
        '//    System.Data.DataTable Dt = null;
        '//    try
        '//    {
        '//        if (Dt == null)
        '//        {
        '//            Dt = new DataTable();
        '//        }
        '//        da = new MySqlDataAdapter(Query, _strConnectionString);
        '//        da.Fill(Dt);
        '//        Dadp = da;
        '//        return Dt;

        '//    }
        '//    catch (Exception ex)
        '//    {
        '//        MiniOSSLog.WriteLine("ERROR: while executing MiniOSS Query:[ExecuteSQL] " + ex.Message + Constants.vbCrLf + Query);
        '//        throw ex;
        '//    }

        '//}

        '/// <summary>
        '/// This Funtion is update data from Postgre database 
        '/// </summary>
        '/// <param name="Dt">Dt as DataTable</param>
        '/// <param name="Da"> Da as DataAdapter</param>
        '/// <returns>It returns as Boolean</returns>
        '/// <remarks></remarks>
        '//public ReturnSuccessOrFailure UpdateDataToPostgre(System.Data.DataTable Dt, object Da,DbType DbType)
        '//{

        '//    try
        '//    {
        '//        dynamic oPostGreCmdBuilder = new MySqlCommandBuilder((MySqlDataAdapter)Da);
        '//        //' Dt.AcceptChanges()
        '//        ((MySqlDataAdapter)Da).Update(Dt);

        '//        return ReturnSuccessOrFailure.Success;


        '//    }
        '//    catch (Exception ex)
        '//    {
        '//        throw ex;

        '//    }

        '//}
        '<summary>
        'This Funtion is used for checking the existing database 
        '</summary>
        '<param name="Query">Query as string</param>
        '<returns>It returns as Object</returns>
        '<remarks></remarks>
        Public Function ExecuteScalar(ByVal Query As String) As Object
            Dim Conn As DbConnection = Nothing
            Dim Command As DbCommand = Nothing

            Dim obj As Object = Nothing
            Try
                Conn = objFact.CreateInstanceDbConnection(DbType, _strConnectionString)
                Command = objFact.CreateInstanceDbCommand(DbType, Query, Conn)
                Conn.Open()
                obj = Command.ExecuteScalar()
                Conn.Close()
                Return obj
            Catch ex As Exception
                Throw ex
            Finally
                If ((Conn.State = ConnectionState.Open)) Then
                    Conn.Close()
                End If
                Command.Dispose()
                Conn.Dispose()
                If (Not Conn Is Nothing) Then
                    Conn = Nothing
                End If
                If (Not Command Is Nothing) Then
                    Command = Nothing
                End If
            End Try
        End Function

        ' <summary>
        ' This Funtion is used for ExecuteNonQuery statement
        ' </summary>
        ' <param name="Query">Query as string</param>
        ' <returns>It returns as Boolean</returns>
        ' <remarks></remarks>
        Public Function ExecuteNonQuery(ByVal Query As String) As Int32
            Dim Conn As DbConnection = Nothing
            Dim Command As DbCommand = Nothing
            Dim result As Integer = 0
            Dim retrycount As Integer = 0
            Dim ErrorMsg As String = String.Empty
            Dim FileError As String = String.Empty
            Try
                Conn = objFact.CreateInstanceDbConnection(DbType, _strConnectionString)
                '// Command.CommandTimeout = 20000;
                Command = objFact.CreateInstanceDbCommand(DbType, Query, Conn)
                Command.CommandTimeout = 20000
                Conn.Open()
retry:
                Try
                    If (retrycount <= 4) Then
                        result = Command.ExecuteNonQuery()
                    End If
                    Return result
                Catch ex As Exception
                    retrycount = retrycount + 1
                    If (retrycount = 5) Then
                        ErrorMsg = "@" + System.DateTime.Now.ToString() + ": " + ex.Message.ToString() + ".Query:" + Query
                        oExceptionLog.WriteExceptionErrorToFile("ExecuteSQLQuery()", ErrorMsg, "", FileError)
                        Return 0
                    End If
                    GoTo retry
                End Try
                Return result
            Catch ex As Exception
                Throw ex
                Return result
            Finally
                If (Conn.State = ConnectionState.Open) Then
                    Conn.Close()
                End If
                Command.Dispose()
                Conn.Dispose()
                If (Not Conn Is Nothing) Then
                    Conn = Nothing
                End If
                If (Not Command Is Nothing) Then
                    Command = Nothing
                End If
            End Try
        End Function
        Public Function UpdateExecuteNonQuery(ByVal dt As DataTable, ByVal Query As String) As Int32
            Dim Conn As DbConnection = Nothing
            Dim Command As DbCommand = Nothing
            Dim result As Integer = 0
            Try
                Conn = objFact.CreateInstanceDbConnection(DbType, _strConnectionString)
                '// Command.CommandTimeout = 20000;
                Conn.Open()
                Dim adap As New MySqlDataAdapter(Query, Conn)

                Dim CmdBuilder As New MySqlCommandBuilder(adap)
                adap.Update(dt)
                Conn.Close()
                Return result
            Catch ex As Exception
                Throw ex
                Return result
            Finally
                If (Conn.State = ConnectionState.Open) Then
                    Conn.Close()
                End If
                Command.Dispose()
                Conn.Dispose()
                If (Not Conn Is Nothing) Then
                    Conn = Nothing
                End If
                If (Not Command Is Nothing) Then
                    Command = Nothing
                End If
            End Try
        End Function


        '/// <summary>
        '/// This Funtion is used for ExecuteNonQuery statement
        '/// </summary>
        '/// <param name="Query">Query as string</param>
        '/// <returns>It returns as Boolean</returns>
        '/// <remarks></remarks>
        '//public ReturnSuccessOrFailure ExecuteNonQuery(string Query, ref int EffectedRows)
        '//{
        '//    MySqlConnection PgConn = new MySqlConnection();
        '//    MySqlCommand PgCommand = new MySqlCommand();
        '//    try
        '//    {
        '//        PgConn = new MySqlConnection(_strConnectionString);
        '//        PgCommand = new MySqlCommand(Query, PgConn);
        '//        PgConn.Open();
        '//        EffectedRows = PgCommand.ExecuteNonQuery();
        '//        PgConn.Close();
        '//        return ReturnSuccessOrFailure.Success;
        '//    }
        '//    catch (Exception ex)
        '//    {
        '//        MiniOSSLog.WriteLine("ERROR while executing MiniOSS Query:[ExecuteNonQuery] " + ex.Message + Constants.vbCrLf + Query);
        '//        //Throw ex
        '//        return ReturnSuccessOrFailure.Failure;
        '//    }
        '//    finally
        '//    {
        '//        if ((PgConn.State == ConnectionState.Open))
        '//        {
        '//            PgConn.Close();
        '//        }
        '//        PgCommand.Dispose();
        '//        PgConn.Dispose();
        '//        if ((PgConn != null))
        '//        {
        '//            PgConn = null;
        '//        }
        '//        if ((PgCommand != null))
        '//        {
        '//            PgConn = null;
        '//        }
        '//    }
        '//}

        '/// <summary>
        '/// This Funtion is used for ExecuteNonQuery statement
        '/// </summary>
        '/// <param name="Query">Query as string</param>
        '/// <returns>It returns as Boolean</returns>
        '///// <remarks></remarks>
        '//public ReturnSuccessOrFailure ExecuteNonQuery(string Query, ref string ErrMsg)
        '//{
        '//    MySqlConnection PgConn = new MySqlConnection();
        '//    MySqlCommand PgCommand = new MySqlCommand();
        '//    try
        '//    {
        '//        PgConn = new MySqlConnection(_strConnectionString);
        '//        PgCommand.CommandTimeout = 20000;
        '//        PgCommand = new MySqlCommand(Query, PgConn);
        '//        PgConn.Open();
        '//        PgCommand.ExecuteNonQuery();
        '//        PgConn.Close();

        '//        return ReturnSuccessOrFailure.Success;
        '//    }
        '//    catch (Exception ex)
        '//    {
        '//        ErrMsg = ex.Message;
        '//        MiniOSSLog.WriteLine("ERROR while executing MiniOSS Query:[ExecuteNonQuery] " + ex.Message + Constants.vbCrLf + Query);
        '//        //Throw ex
        '//        return ReturnSuccessOrFailure.Failure;
        '//    }
        '//    finally
        '//    {
        '//        if ((PgConn.State == ConnectionState.Open))
        '//        {
        '//            PgConn.Close();
        '//        }
        '//        PgCommand.Dispose();
        '//        PgConn.Dispose();
        '//        if ((PgConn != null))
        '//        {
        '//            PgConn = null;
        '//        }
        '//        if ((PgCommand != null))
        '//        {
        '//            PgConn = null;
        '//        }
        '//    }
        '//}


        '/// <summary>
        '/// This Funtion is used for ExecuteNonQuery statement
        '/// </summary>
        '/// <param name="Query">Query as string</param>
        '/// <returns>It returns as Boolean</returns>
        '/// <remarks></remarks>
        Public Sub ExecuteNonQuery(ByRef ErrMsg As String, ByVal arr As ArrayList, ByVal isTrans As Boolean)

            Dim Conn As DbConnection = objFact.CreateInstanceDbConnection(DbType)
            Dim Command As DbCommand = Nothing
            Dim Trans As DbTransaction = Nothing
            Try
                Conn = objFact.CreateInstanceDbConnection(DbType, _strConnectionString)
                Command.CommandTimeout = 20000
                Conn.Open()
                Trans = Conn.BeginTransaction()
                For i As Integer = 0 To arr.Count - 1
                    Command.Transaction = Trans
                    Command = objFact.CreateInstanceDbCommand(DbType, arr(i).ToString(), Conn)
                    Command.ExecuteNonQuery()
                Next
                Trans.Commit()

            Catch ex As Exception
                Trans.Rollback()
                ErrMsg = ex.Message
                '//Throw ex
            Finally
                If (Conn.State = ConnectionState.Open) Then
                    Conn.Close()
                End If
                Command.Dispose()
                Conn.Dispose()
                If (Not Conn Is Nothing) Then
                    Conn = Nothing
                End If
                If (Not Command Is Nothing) Then
                    Command = Nothing
                End If
            End Try

        End Sub


        Public Sub ExecuteNonQuery(ByRef ErrMsg As String, ByVal arr As ArrayList)
            Dim Conn As DbConnection = objFact.CreateInstanceDbConnection(DbType)
            Dim Command As DbCommand = objFact.CreateInstanceDbCommand(DbType)
            Try
                Conn = objFact.CreateInstanceDbConnection(DbType, _strConnectionString)
                Command.CommandTimeout = 20000
                Conn.Open()
                For i As Integer = 0 To arr.Count - 1
                    Command = objFact.CreateInstanceDbCommand(DbType, arr(i).ToString(), Conn)
                    Try
                        Command.ExecuteNonQuery()
                    Catch ex As Exception
                        'ExecuteNonQuery("insert into ErrLog  (Error_Occured,Alarm_Query ) values ( '" + ex.Message.Replace("'", "~") + "','" + arr[i].ToString().Replace("'","~") + "';")
                    Finally
                    End Try
                Next
            Catch ex As Exception
                ErrMsg = ex.Message
                '//Throw ex
            Finally
                If (Conn.State = ConnectionState.Open) Then
                    Conn.Close()
                End If
                Command.Dispose()
                Conn.Dispose()
                If Not Conn Is Nothing Then
                    Conn = Nothing
                End If
                If Not Command Is Nothing Then
                    Command = Nothing
                End If
            End Try
        End Sub

        Public Sub ExecuteProcedure(ByVal spName As String, ByVal parameters As ArrayList)
            Dim Conn As DbConnection = Nothing
            Dim Command As DbCommand = Nothing
            Try
                Conn = objFact.CreateInstanceDbConnection(DbType, _strConnectionString)
                Command = objFact.CreateInstanceDbCommand(DbType)
                Conn.Open()
                Command.Connection = Conn
                Command.CommandType = CommandType.StoredProcedure
                Command.CommandText = spName.ToString()
                Command.CommandTimeout = 300000
                If (Not parameters Is Nothing) Then
                    For i As Integer = 0 To i <= parameters.Count - 1
                        Command.Parameters.Add(parameters(i))
                    Next
                End If
                Command.ExecuteNonQuery()
            Catch ex As Exception
                Throw ex
            Finally
                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If
                Command.Dispose()
                Conn.Dispose()
                If Not Conn Is Nothing Then
                    Conn = Nothing
                End If
                If Not Command Is Nothing Then
                    Command = Nothing
                End If

            End Try
        End Sub

    End Class
End Namespace