using System;
using System.Collections.Generic;
using System.Text;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       EmpInfo
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        사용자 정보 클레스
     *
     *  Program     (PrintLabel) Biz.UPS\EmpInfo.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     ******************************************************************************************/
    /// <summary>
    /// 사용자 정보 클래스 입니다.
    /// </summary>
    public static class EmpInfo
    {
        static string empName = string.Empty;
        static string empId = string.Empty;
        static string groupId = string.Empty;
        static DateTime loginTime = new DateTime();

        /// <summary>
        /// 사용자 이름
        /// </summary>
        public static string EmpName
        {
            get { return empName; }
            set { empName = value; }
        }

        /// <summary>
        /// 사용자 아이디
        /// </summary>
        public static string EmpId
        {
            get 
            {                
                return empId; 
            }
            set 
            {
                //DB에서 사원 아이디 관련정보를 가져와 셋팅한다.
                empId = value; 
            }
        }

        /// <summary>
        /// 사용자 그룹
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
        /// 로그인 시간
        /// </summary>
        public static DateTime LoginTime
        {
            get { return loginTime; }
            set { loginTime = value; }
        }

        /// <summary>
        /// 사원번호 유효성 체크
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        public static bool ValidEmpId(string empId)
        {
            return true;
        }


    }//END class
}
