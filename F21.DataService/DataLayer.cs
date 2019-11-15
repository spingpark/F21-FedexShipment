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
     *  Desc        Database Execute����
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
        /// DataTable�� ��ȯ�ϴ� ��������
        /// </summary>
        /// <param name="sql">����</param>
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
        /// ���ϰ��� �������� ���� ����
        /// </summary>
        /// <param name="sql">����</param>
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
        /// ��ȸ ��������
        /// </summary>
        /// <param name="sql">����</param>
        /// <param name="nvParam">�Ķ����</param>
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
        /// ���/����/���� ��������
        /// </summary>
        /// <param name="sql">����</param>
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
        /// ���/����/���� ��������
        /// </summary>
        /// <param name="sql">����</param>
        /// <param name="nvParam">�Ķ����</param>
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
        /// ���/����/���� ���ν������� 
        /// </summary>
        /// <param name="spName">���ν����̸�</param>
        /// <param name="nvParam">�Ķ����</param>
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
        /// ���ν��������� DataSet ��ȯ
        /// </summary>
        /// <param name="spName">���ν����̸�</param>
        /// <param name="nvParam">�Ķ����</param>
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
        /// ���ν��������� DataTable��ȯ
        /// </summary>
        /// <param name="spName">���ν����̸�</param>
        /// <param name="nvParam">�Ķ����</param>
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
