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
     *  Class       DataService
     *  Author      Moonseok Kang 
     *  Create      01/03/2011
     *  Desc        Database Connection, Transaction 등의 Service관련
     *
     *  Program     (PrintLabel) F21.DataService\DataService.cs
     *  Modify      
     *              Moonseok Kang   :   2011/01/03  : Multi Gift Card
     *              Moonseok Kang   :   2011/01/12  : Tracking Order
     ******************************************************************************************/
    public enum DBCatalog
    {
        NewWarehouse,
        SCM21
          
    }

    public class DataService
    {
        //db 커넥션
        internal SqlConnection cn;
        internal SqlTransaction transaction;
        
        public DataService()
        {
            
        }

        /// <summary>
        /// 커넥션 열기
        /// </summary>				
        /// <returns>void</returns>
        private void CnOpen(DBCatalog db)
        {
            if (transaction == null)
            {
                Initialize(db);                    
            }

            if (cn.State == System.Data.ConnectionState.Closed)
            {
                cn.Open();
                System.Diagnostics.Debug.WriteLine("Connection -> " + db.ToString());
            }
            
        }
        /// <summary>
        /// 커넥션 닫기
        /// </summary>				
        /// <returns>void</returns>
        private void CnClose()
        {
            if (transaction == null)
            {
                if (cn.State == System.Data.ConnectionState.Open)
                    cn.Close();
            }
        }
        /// <summary>
        /// DB커넥션 생성, 연결스트링 셋팅
        /// </summary>				
        /// <returns>void</returns>
        private void Initialize(DBCatalog db)
        {
            this.cn = new SqlConnection();
            
            string dbName = "";

            //if (Basic.GetRegistryKey("TestMode") == "YES")
            //    dbName = db.ToString() + "_TEST";
            //else
            //    dbName = db.ToString();

            dbName = db.ToString();

            this.cn.ConnectionString
                        = Encryption.Decrypt(ConfigurationManager.ConnectionStrings[dbName].ToString(), true);
        }

        public String GetDBConnecitonString(DBCatalog db)
        {
            String result = String.Empty;

            using (SqlConnection con = new SqlConnection(Encryption.Decrypt(ConfigurationManager.ConnectionStrings["Common"].ToString(), true, "Forever21MX")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("spConfigValue;1", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@AppId", SqlDbType.VarChar, 4);
                cmd.Parameters.Add("@ConfigKey", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@Production", SqlDbType.Bit);

                cmd.Parameters["@AppId"].Value = ConfigManager.GetAppSetting2("AppId");
                cmd.Parameters["@ConfigKey"].Value = db.ToString();
                cmd.Parameters["@Production"].Value = (ConfigManager.GetAppSetting2("mode").Trim().Equals("production") ? true : false);

                SqlDataReader read = cmd.ExecuteReader();
                if (read.HasRows)
                {
                    read.Read();
                    result = read.GetString(0);
                }
                read.Close();
                read.Dispose();

                cmd.Dispose();
                con.Close();
            }

            return result;
        }

        public void BeginTransaction(DBCatalog db)
        {
            if (transaction != null)
                return;

            try
            {
                CnOpen(db);
                transaction = cn.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch
            {
                CnClose();
                throw;
            }
        }

        public void Commit()
        {
            if (transaction == null)
                return;

            try
            {
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                CnClose();
                transaction = null;
            }
        }

        public void Rollback()
        {
            if (transaction == null)
                return;

            try
            {
                transaction.Rollback();
            }
            catch { }
            finally
            {
                CnClose();
                transaction = null;
            }
        }

        #region DataService

        public DataTable SelectSql(string sql, DBCatalog db)
        {
            DataSet ds = new DataSet();
            DataTable table = new DataTable();
            ds.Tables.Add(table);
            
            try
            {
                CnOpen(db);
                SqlCommand cmd = new SqlCommand(sql, cn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds.Tables[0]);
                CnClose();

            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[SelectSql] " + sql + "\n[SelectSql Error] " + ex.Message);

                CnClose();
                throw ex;
            }
            return ds.Tables[0];

        }

        public string SelectScalarSql(string sql, DBCatalog db)
        {
            object retVal = string.Empty;

            try
            {
                CnOpen(db);
                SqlCommand cmd = new SqlCommand(sql, cn);
                retVal = cmd.ExecuteScalar();
                CnClose();

                if (retVal == null)
                    retVal = "";

            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[SelectSql] " + sql + "\n[SelectSql Error] " + ex.Message);

                CnClose();
                throw ex;
            }
            return retVal.ToString();

        }

        public DataTable SelectSql(string sql, NameValueCollection nvParam, DBCatalog db)
        {
            DataTable dt = null;

            try
            {
                CnOpen(db);

                dt = SqlHelper.SqlSearchDataTable(cn, sql, nvParam);
                nvParam.Clear();
                nvParam = null;
                CnClose();

            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[SelectSql] " + sql +"\n[SelectSql Error] " + ex.Message);
                CnClose();
                throw ex;
            }
            return dt;

        }


        public string ExecuteSql(string sql, DBCatalog db)
        {
            string retVal = "";

            try
            {
                CnOpen(db);
                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.CommandTimeout = 0;
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
                this.CnClose();

            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[ExecuteSql] " + sql + "[ExecuteSql Error] " + ex.Message);

                this.CnClose();
                throw ex;
            }

            return retVal;
        }

        public string ExecuteSql(string sql, NameValueCollection nvParam, DBCatalog db)
        {
            string retVal = "";

            try
            {
                CnOpen(db);
                SqlHelper.SqlExecuteNonQuery(cn, sql, nvParam,transaction);
                nvParam.Clear();
                nvParam = null;
                this.CnClose();

            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[ExecuteSql] " + sql +"\n[ExecuteSql Error] " + ex.Message);

                this.CnClose();
                throw ex;
            }

            return retVal;
        }

        public string ExecuteSpNonQuery(string spName, NameValueCollection nvcParam, DBCatalog db)
        {

            string retVal = "";

            try
            {
                CnOpen(db);
                SqlHelper.SqlExecuteSpNonQuery(cn, spName, nvcParam,transaction);
                nvcParam.Clear();
                nvcParam = null;
                CnClose();
            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[ExecuteSpNonQuery] " + spName + "\n[ExecuteSpNonQuery Error] " + ex.Message);

                CnClose();
                throw ex;
            }

            return retVal;

        }

        public DataSet ExecuteSpDataSet(string spName, NameValueCollection nvcParam, DBCatalog db)
        {
            DataSet ds = null;

            try
            {
                CnOpen(db);

                ds = SqlHelper.SqlExecuteSpDataSet(cn, spName, nvcParam);
                nvcParam.Clear();
                nvcParam = null;
                CnClose();

            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[ExecuteSpDataSet] " + spName + "\n[ExecuteSpDataSet Error] " + ex.Message);

                CnClose();
                throw ex;
            }
            return ds;

        }

        public DataTable ExecuteSpDataTable(string spName, NameValueCollection nvcParam, DBCatalog db)
        {
            DataTable dt = null;

            try
            {
                CnOpen(db);

                dt = SqlHelper.SqlExecuteSpDataTable(cn, spName, nvcParam);
                nvcParam.Clear();
                nvcParam = null;
                CnClose();

            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[ExecuteSpDataTable] " + spName + "\n[ExecuteSpDataTable Error] " + ex.Message);

                CnClose();
                throw ex;
            }
            return dt;

        }

        public string ExecuteSpScalar(string spName, NameValueCollection nvcParam, DBCatalog db)
        {

            string retVal = "";

            try
            {
                CnOpen(db);

                retVal = SqlHelper.SqlExecuteSpScalar(cn, spName, nvcParam,transaction);
                nvcParam.Clear();
                nvcParam = null;
                CnClose();
            }
            catch (Exception ex)
            {
                TextManager.WriteErrorLog("[ExecuteSpScalar] " + spName + "[ExecuteSpScalar Error] " + ex.Message);

                CnClose();
                throw ex;
            }
            return retVal;

        }
        #endregion

    }//END class
}//END namespace
