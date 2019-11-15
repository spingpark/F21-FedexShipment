using System;
using System.Collections.Generic;
using System.Text;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       EmpInfo
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        ����� ���� Ŭ����
     *
     *  Program     (PrintLabel) Biz.UPS\EmpInfo.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     ******************************************************************************************/
    /// <summary>
    /// ����� ���� Ŭ���� �Դϴ�.
    /// </summary>
    public static class EmpInfo
    {
        static string empName = string.Empty;
        static string empId = string.Empty;
        static string groupId = string.Empty;
        static DateTime loginTime = new DateTime();

        /// <summary>
        /// ����� �̸�
        /// </summary>
        public static string EmpName
        {
            get { return empName; }
            set { empName = value; }
        }

        /// <summary>
        /// ����� ���̵�
        /// </summary>
        public static string EmpId
        {
            get 
            {                
                return empId; 
            }
            set 
            {
                //DB���� ��� ���̵� ���������� ������ �����Ѵ�.
                empId = value; 
            }
        }

        /// <summary>
        /// ����� �׷�
        /// </summary>
        public static string GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        /// <summary>
        /// �α��� �ð�
        /// </summary>
        public static DateTime LoginTime
        {
            get { return loginTime; }
            set { loginTime = value; }
        }

        /// <summary>
        /// �����ȣ ��ȿ�� üũ
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        public static bool ValidEmpId(string empId)
        {
            return true;
        }


    }//END class
}
