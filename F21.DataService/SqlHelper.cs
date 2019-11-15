using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Specialized;
using System.Configuration;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       SqlHelper
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        SqlHelper
     *
     *  Program     (PrintLabel) F21.DataService\SqlHelper.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     ******************************************************************************************/
    class SqlHelper
    {
        public static DataTable SqlSearchDataTable(SqlConnection con, string sql, NameValueCollection nvcParam)
        {
            SqlCommand cmd = new SqlCommand(sql, con);

            try
            {
                if (nvcParam != null)
                {
                    foreach (string nvlKey in nvcParam.AllKeys)
                    {
                        try
                        {
                            cmd.Parameters.Add(nvlKey, SqlDbType.VarChar);
                            cmd.Parameters[nvlKey].Value = nvcParam[nvlKey];
                        }
                        catch (System.NullReferenceException ex)
                        {
                            throw new Exception("쿼리 [" + sql + "] 에" + nvlKey + "의 파라메터가 없습니다.\n\r" + ex.Message, ex);
                        }
                    }
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();

                da.Fill(table);

                return table;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SqlExecuteNonQuery(SqlConnection con, string sql, NameValueCollection nvcParam, SqlTransaction tran)
        {
            SqlCommand cmd = new SqlCommand(sql, con);

            System.Diagnostics.Debug.WriteLine("[" + sql + "]");

            string excuteSql = sql;

            try
            {
                foreach (string nvlKey in nvcParam.AllKeys)
                {
                    try
                    {
                        cmd.Parameters.Add(nvlKey, SqlDbType.VarChar);
                        cmd.Parameters[nvlKey].Value = nvcParam[nvlKey];
                        System.Diagnostics.Debug.WriteLine("[" + nvlKey + "] " + nvcParam[nvlKey].ToString());

                        excuteSql += " '" + nvcParam[nvlKey].ToString() + "',";

                    }
                    catch (System.NullReferenceException ex)
                    {
                        throw new Exception("쿼리 [" + sql + "] 에" + nvlKey + "의 파라메터가 없습니다.\n\r" + ex.Message, ex);
                    }
                }

                System.Diagnostics.Debug.WriteLine("[ExcuteSQL] " + excuteSql);
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SqlExecuteSpNonQuery(SqlConnection con, string spName, NameValueCollection nvcParam, SqlTransaction tran)
        {
            SqlCommand cmd = new SqlCommand(spName, con);
            cmd.CommandType = CommandType.StoredProcedure;

            System.Diagnostics.Debug.WriteLine("[" + spName + "]");
            string excuteSql = spName;

            try
            {
                cmd.Transaction = tran;

                SqlCommandBuilder.DeriveParameters(cmd);
                foreach (string nvlKey in nvcParam.AllKeys)
                {
                    try
                    {
                        cmd.Parameters[nvlKey].Value = nvcParam[nvlKey];
                        System.Diagnostics.Debug.WriteLine("[" + nvlKey + "] " + nvcParam[nvlKey].ToString());
                        excuteSql += "'" + nvcParam[nvlKey].ToString() + "',";

                    }
                    catch (System.NullReferenceException ex)
                    {
                        throw new Exception("프로시져 " + spName + " 에" + nvlKey + "의 파라메터가 없습니다.\n\r" + ex.Message, ex);
                    }
                }
                System.Diagnostics.Debug.WriteLine("[ExcuteSQL] " + excuteSql);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static DataSet SqlExecuteSpDataSet(SqlConnection con, string spName, NameValueCollection nvcParam)
        {
            SqlCommand cmd = new SqlCommand(spName, con);
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            DataSet ds = new DataSet();
            try
            {
                SqlCommandBuilder.DeriveParameters(cmd);
                if (nvcParam != null)
                {
                    foreach (string nvlKey in nvcParam.AllKeys)
                    {
                        try
                        {
                            cmd.Parameters[nvlKey].Value = nvcParam[nvlKey];
                        }
                        catch (System.NullReferenceException ex)
                        {
                            throw new Exception("프로시져" + spName + " 에" + nvlKey + "의 파라메터가 없습니다.\n\r" + ex.Message, ex);
                        }
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                //DataTable table = new DataTable();
                //da.Fill(table);
                //ds.Tables.Add(table);

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable SqlExecuteSpDataTable(SqlConnection con, string spName, NameValueCollection nvcParam)
        {
            SqlCommand cmd = new SqlCommand(spName, con);
            cmd.CommandTimeout = 0;

            cmd.CommandType = CommandType.StoredProcedure;

            System.Diagnostics.Debug.WriteLine("[" + spName + "]");
            string excuteSql = spName;

            try
            {
                SqlCommandBuilder.DeriveParameters(cmd);
                if (nvcParam != null)
                {
                    foreach (string nvlKey in nvcParam.AllKeys)
                    {
                        try
                        {
                            cmd.Parameters[nvlKey].Value = nvcParam[nvlKey];
                            System.Diagnostics.Debug.WriteLine("[" + nvlKey + "] " + nvcParam[nvlKey].ToString());
                            excuteSql += " '" + nvcParam[nvlKey].ToString() + "',";

                        }
                        catch (System.NullReferenceException ex)
                        {
                            throw new Exception("프로시져" + spName + " 에" + nvlKey + "의 파라메터가 없습니다.\n\r" + ex.Message, ex);
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[ExcuteSQL] " + excuteSql);


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();
                da.Fill(table);

                return table;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string SqlExecuteSpScalar(SqlConnection con, string spName, NameValueCollection nvcParam, SqlTransaction tran)
        {
            SqlCommand cmd = new SqlCommand(spName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            string retVal = "";

            System.Diagnostics.Debug.WriteLine("[" + spName + "]");
            string excuteSql = spName;

            try
            {
                cmd.Transaction = tran;

                SqlCommandBuilder.DeriveParameters(cmd);
                foreach (string nvlKey in nvcParam.AllKeys)
                {
                    try
                    {
                        cmd.Parameters[nvlKey].Value = nvcParam[nvlKey];
                        System.Diagnostics.Debug.WriteLine("[" + nvlKey + "] " + nvcParam[nvlKey].ToString());
                        excuteSql += " '" + nvcParam[nvlKey].ToString() + "',";

                    }
                    catch (System.NullReferenceException ex)
                    {
                        throw new Exception("프로시져 " + spName + " 에" + nvlKey + "의 파라메터가 없습니다.\n\r" + ex.Message, ex);
                    }
                }
                cmd.ExecuteNonQuery();

                foreach (SqlParameter sqlParam in cmd.Parameters)
                {
                    if (sqlParam.Direction == ParameterDirection.Output || sqlParam.Direction == ParameterDirection.InputOutput)
                    {
                        retVal = retVal + sqlParam.Value.ToString() + "|";
                    }
                }
                if (retVal.Length > 0)
                {
                    retVal = retVal.Substring(0, retVal.Length - 1);
                }

                System.Diagnostics.Debug.WriteLine("[retVal] " + retVal);

                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[ExcuteSQL] " + excuteSql);


                return retVal;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private const int ARGUMENT_NAME = 0, OVERLOAD = 1, POSITION = 2, LEVEL = 3, DATATYPE = 4, DEFAULT_VALUE = 5, IN_OUT = 6, LENGTH = 7, PRECISION = 8, SCALE = 9, RADIX = 10, SPARE = 11;

    }//END class
}//END namespace
