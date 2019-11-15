using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Specialized;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       DataLayer
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        Database Execute관련
     *
     *  Program     (PrintLabel) F21.DataService\DataLayer.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     ******************************************************************************************/
    public class DataLayer
    {
        static DataService ds = null;

        static DataLayer()
        {
            ds = new DataService();
        }

        #region DataService

        public static void BeginTransaction(DBCatalog db)
        {
            System.Diagnostics.Debug.WriteLine("[BeginTransaction] -> " + db.ToString());
            
            ds.BeginTransaction(db);
        }

        public static void Commit()
        {
            System.Diagnostics.Debug.WriteLine("[Commit]");
            ds.Commit();
        }

        public static void Rollback()
        {
            System.Diagnostics.Debug.WriteLine("[Rollback]");
            ds.Rollback();
        }

        /// <summary>
        /// DataTable을 반환하는 쿼리실행
        /// </summary>
        /// <param name="sql">쿼리</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static DataTable Search(string sql, DBCatalog db)
        {
            DataTable dt = null;
            try
            {
                dt = ds.SelectSql(sql, db);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Search Error] " + ex.Message);
            }
            return dt;

        }

        /// <summary>
        /// 단일값을 가져오는 쿼리 실행
        /// </summary>
        /// <param name="sql">쿼리</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static string SearchScalar(string sql, DBCatalog db)
        {
            string retVal = null;
            try
            {
                retVal = ds.SelectScalarSql(sql, db);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Search Error] " + ex.Message);
            }
            return retVal;

        }

        /// <summary>
        /// 조회 쿼리실행
        /// </summary>
        /// <param name="sql">쿼리</param>
        /// <param name="nvParam">파라메터</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static DataTable Search(string sql, NameValueCollection nvParam, DBCatalog db)
        {

            DataTable dt = null;
            try
            {
                dt = ds.SelectSql(sql, nvParam, db);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Search Error] " + ex.Message);
            }
            return dt;
        }

        
        /// <summary>
        /// 등록/수정/삭제 쿼리실행
        /// </summary>
        /// <param name="sql">쿼리</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static string ExecuteSql(string sql, DBCatalog db)
        {

            string strReturn = "";
            try
            {
                strReturn = ds.ExecuteSql(sql, db);
            }
            catch (Exception ex)
            {
                strReturn = ex.Message;
            }
            return strReturn;

        }

        /// <summary>
        /// 등록/수정/삭제 쿼리실행
        /// </summary>
        /// <param name="sql">쿼리</param>
        /// <param name="nvParam">파라메터</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static string ExecuteSql(string sql, NameValueCollection nvParam, DBCatalog db)
        {

            string strReturn = "";
            try
            {
                strReturn = ds.ExecuteSql(sql, nvParam, db);
            }
            catch (Exception ex)
            {
                strReturn = ex.Message;
            }
            return strReturn;

        }

        
        /// <summary>
        /// 등록/수정/삭제 프로시져실행 
        /// </summary>
        /// <param name="spName">프로시져이름</param>
        /// <param name="nvParam">파라메터</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static string ExecuteSpSql(string spName, NameValueCollection nvParam, DBCatalog db)
        {

            string strReturn = "";
            try
            {
                strReturn = ds.ExecuteSpScalar(spName.Trim(), nvParam, db);
            }
            catch (Exception ex)
            {
                strReturn = ex.Message;
            }
            return strReturn;

        }

        
        /// <summary>
        /// 프로시져실행후 DataSet 반환
        /// </summary>
        /// <param name="spName">프로시져이름</param>
        /// <param name="nvParam">파라메터</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static DataSet ExecuteSpDataSet(string spName, NameValueCollection nvParam, DBCatalog db)
        {
            
            DataSet dataset = null;
            try
            {
                dataset = ds.ExecuteSpDataSet(spName.Trim(), nvParam, db);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] " + ex.Message);
            }
            return dataset;

        }

        /// <summary>
        /// 프로시져실행후 DataTable반환
        /// </summary>
        /// <param name="spName">프로시져이름</param>
        /// <param name="nvParam">파라메터</param>
        /// <param name="db">DB</param>
        /// <returns></returns>
        public static DataTable ExecuteSpDataTable(string spName, NameValueCollection nvParam, DBCatalog db)
        {

            DataTable dt = null;
            try
            {
                dt = ds.ExecuteSpDataTable(spName.Trim(), nvParam, db);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] " + ex.Message);
            }
            return dt;

        }
                
        #endregion

    }//END class
}//END namespace
