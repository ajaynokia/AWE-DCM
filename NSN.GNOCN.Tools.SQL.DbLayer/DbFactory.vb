Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.Data.OracleClient
Imports System.Data.SqlClient
Imports System.Data.Common

Namespace NSN.GNOCN.Tools.SQL.DbLayer

    Public Class DbFactory
        Implements IDbFactory

        Public Function CreateInstanceDataAdapter(ByVal Dbtype As DbType) As System.Data.Common.DbDataAdapter Implements IDbFactory.CreateInstanceDataAdapter
            Try
                Dim objDtAda As DbDataAdapter = Nothing
                Select Case Dbtype
                    Case Dbtype.MySqlType
                        objDtAda = New MySqlDataAdapter()
                        Exit Select
                    Case Dbtype.OracleType
                        objDtAda = New OracleDataAdapter()
                        Exit Select
                    Case Dbtype.SqlType
                        objDtAda = New SqlDataAdapter()
                        Exit Select
                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))
                End Select
                Return objDtAda
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Function CreateInstanceDataAdapter(ByVal Dbtype As DbType, ByVal Query As String, ByVal conStr As String) As System.Data.Common.DbDataAdapter Implements IDbFactory.CreateInstanceDataAdapter
            Try

                Dim objDtAda As DbDataAdapter = Nothing
                Select Case Dbtype
                    Case Dbtype.MySqlType
                        objDtAda = New MySqlDataAdapter(Query, conStr)
                        Exit Select
                    Case Dbtype.OracleType
                        objDtAda = New OracleDataAdapter(Query, conStr)
                        Exit Select
                    Case Dbtype.SqlType
                        objDtAda = New SqlDataAdapter(Query, conStr)
                        Exit Select
                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))
                        'default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
                End Select
                Return objDtAda
            Catch ex As Exception
                Return Nothing
            Finally

            End Try
        End Function

        Public Function CreateInstanceDbCommand(ByVal Dbtype As DbType) As System.Data.Common.DbCommand Implements IDbFactory.CreateInstanceDbCommand
            Dim obj As DbCommand = Nothing
            Try
                Select Case Dbtype
                    Case Dbtype.MySqlType
                        obj = New MySqlCommand()
                        Exit Select
                    Case Dbtype.OracleType
                        obj = New OracleCommand()
                        Exit Select
                    Case Dbtype.SqlType
                        obj = New SqlCommand()
                        Exit Select
                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))
                        'default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
                End Select
                Return obj
            Catch ex As Exception
                Return obj
            Finally
            End Try
           
        End Function

        Public Function CreateInstanceDbCommand(ByVal Dbtype As DbType, ByVal cmdText As String, ByVal con As System.Data.Common.DbConnection) As System.Data.Common.DbCommand Implements IDbFactory.CreateInstanceDbCommand
            Dim obj As DbCommand = Nothing
            Try
                Select Case Dbtype
                    Case Dbtype.MySqlType
                        obj = New MySqlCommand(cmdText, con)
                        Exit Select
                    Case Dbtype.OracleType
                        obj = New OracleCommand(cmdText, con)
                        Exit Select
                    Case Dbtype.SqlType
                        obj = New SqlCommand(cmdText, con)
                        Exit Select
                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))
                        'default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
                End Select
                Return obj
            Catch ex As Exception
                Return obj
            Finally
            End Try
        End Function

        Public Function CreateInstanceDbCommandBuilder(ByVal Dbtype As DbType) As System.Data.Common.DbCommandBuilder Implements IDbFactory.CreateInstanceDbCommandBuilder

            Dim obj As DbCommandBuilder = Nothing
            Try
                Select Case Dbtype
                    Case Dbtype.MySqlType
                        obj = New MySqlCommandBuilder()
                        Exit Select
                    Case Dbtype.OracleType
                        obj = New OracleCommandBuilder()
                        Exit Select
                    Case Dbtype.SqlType
                        obj = New SqlCommandBuilder()
                        Exit Select
                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))
                        'default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
                End Select
                Return obj
            Catch ex As Exception
                Return obj
            Finally

            End Try
        End Function

        Public Function CreateInstanceDbCommandBuilder(ByVal Dbtype As DbType, ByVal da As System.Data.Common.DbDataAdapter) As System.Data.Common.DbCommandBuilder Implements IDbFactory.CreateInstanceDbCommandBuilder

            Dim obj As DbCommandBuilder = Nothing
            Try
                Select Case Dbtype
                    Case Dbtype.MySqlType
                        obj = New MySqlCommandBuilder(da)
                        Exit Select
                    Case Dbtype.OracleType
                        obj = New OracleCommandBuilder(da)
                        Exit Select
                    Case Dbtype.SqlType
                        obj = New SqlCommandBuilder(da)
                        Exit Select
                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))
                        ' default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
                End Select
                Return obj
            Catch ex As Exception
                Return obj
            End Try
        End Function

        Public Function CreateInstanceDbConnection(ByVal Dbtype As DbType) As System.Data.Common.DbConnection Implements IDbFactory.CreateInstanceDbConnection

            Dim obj As DbConnection = Nothing
            Try
                Select Case Dbtype

                    Case Dbtype.MySqlType

                        obj = New MySqlConnection()

                        Exit Select

                    Case Dbtype.OracleType

                        obj = New OracleConnection()

                        Exit Select

                    Case Dbtype.SqlType

                        obj = New SqlConnection()

                        Exit Select

                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))

                        'default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
                End Select
                Return obj
            Catch ex As Exception
                Return obj
            Finally

            End Try
        End Function

        Public Function CreateInstanceDbConnection(ByVal Dbtype As DbType, ByVal strConString As String) As System.Data.Common.DbConnection Implements IDbFactory.CreateInstanceDbConnection

            Dim obj As DbConnection = Nothing
            Try
                Select Case Dbtype
                    Case Dbtype.MySqlType

                        obj = New MySqlConnection(strConString)

                        Exit Select

                    Case Dbtype.OracleType

                        obj = New OracleConnection(strConString)

                        Exit Select

                    Case Dbtype.SqlType

                        obj = New SqlConnection(strConString)

                        Exit Select

                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))

                        'default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));

                End Select
                Return obj
            Catch ex As Exception
                Return obj
            Finally

            End Try
        End Function

        Public Function CreateInstanceDbParameter(ByVal Dbtype As DbType) As System.Data.Common.DbParameter Implements IDbFactory.CreateInstanceDbParameter

            Dim obj As DbParameter = Nothing
            Try
                Select Case Dbtype

                    Case Dbtype.MySqlType

                        obj = New MySqlParameter()

                        Exit Select

                    Case Dbtype.OracleType

                        obj = New OracleParameter()

                        Exit Select

                    Case Dbtype.SqlType

                        obj = New SqlParameter()

                        Exit Select
                    Case Else
                        Throw New ArgumentException(CType(" An database of type {0} cannot be found", DbType))

                        'default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
                End Select
                Return obj
            Catch ex As Exception
                Return obj
            Finally

            End Try
           
        End Function
    End Class
   

       



       

        






    '//public DbParameterCollection CreateInstanceDbParameterCollection(DbType Dbtype)
    '//{
    '//    DbParameterCollection obj = null;
    '//    try
    '//    {
    '//        switch (Dbtype)
    '//        {
    '//            case DbType.MySqlType:

    '//                obj = new MySqlParameterCollection();

    '//                break;

    '//            case DbType.OracleType:

    '//                obj = new OracleParameterCollection();

    '//                break;

    '//            case DbType.SqlType:

    '//                //obj = new SqlParameterCollection();

    '//                break;

    '//            default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
    '//        }
    '//        return obj;
    '//    }
    '//    catch (Exception ex)
    '//    {
    '//        return obj;
    '//    }
    '//    finally
    '//    {
    '//    }
    '//}



    '//    public DbTransaction CreateInstanceDbTransaction( DbType Dbtype)
    '//    {
    '//        DbTransaction obj= null;
    '//        try
    '//        {
    '//             try
    '//        {
    '//            switch (Dbtype)
    '//            {
    '//                case DbType.MySqlType:

    '//                    obj =  MySqlTransaction;

    '//                    break;

    '//                case DbType.OracleType:

    '//                    obj =  OracleTransaction();

    '//                    break;

    '//                case DbType.SqlType:

    '//                    obj =  SqlTransaction();

    '//                    break;

    '//                default: throw new ArgumentException(string.Format(" An database of type {0} cannot be found", Enum.GetName(typeof(DbType), Dbtype)));
    '//            }
    '//        }
    '//        catch (Exception ex)
    '//        {
    '//        }
    '//        finally
    '//        {
    '//        }
    '//    }
    '//}

End Namespace