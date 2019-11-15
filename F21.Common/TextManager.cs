using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections;
using System.Windows.Forms;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       TextManager
     *  Author      Moonseok Kang 
     *  Create      9/25/2009
     *  Desc        Text 관련 기능 클레스
     *
     *  Program     (PrintLabel) Biz.UPS\TextManager.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     *              2011/03/16  : Moonseok Kang :   Text file write type Unicode 
     *              2011/03/29  : Moonseok Kang :   Function Overloading 
     ******************************************************************************************/
    public class TextManager
    {
        static string ErrorLogPath = Application.StartupPath + "\\Error.log";

        /// <summary>
        /// 텍스트파일로 저장
        /// </summary>
        /// <param name="logMessage"></param>
        public static void Write(string logMessage, string strPath)
        {
            //string strPath = @"C:\Log.txt";
            StreamWriter w = File.AppendText(strPath);
            //StreamWriter w = new StreamWriter(strPath, true, Encoding.Default);
            w.Write(logMessage);

            w.Flush();

            w.Close();
        }

        /// <summary>
        /// 텍스트파일로 저장
        /// </summary>
        /// <param name="logMessage"></param>
        public static void WriteLine(string logMessage, string strPath)
        {
            //string strPath = @"C:\Log.txt";
            //StreamWriter w = File.AppendText(strPath);

            StreamWriter w = new StreamWriter(strPath, true, Encoding.UTF8);

            w.WriteLine(logMessage);

            w.Flush();

            w.Close();
        }

        /// <summary>
        /// 텍스트파일로 저장
        /// </summary>
        /// <param name="logMessage"></param>
        public static void WriteLine(string logMessage, string strPath,Encoding encoding )
        {
            //string strPath = @"C:\Log.txt";
            //StreamWriter w = File.AppendText(strPath);

            //2011/03/16  : Moonseok Kang :   Text file write type Unicode 
            StreamWriter w = new StreamWriter(strPath, true, encoding);

            w.WriteLine(logMessage);

            w.Flush();

            w.Close();
        }

        public static void WriteLine(string logMessage, string strPath, Encoding encoding, bool IsAppend)
        {
            //string strPath = @"C:\Log.txt";
            //StreamWriter w = File.AppendText(strPath);

            //2011/03/16  : Moonseok Kang :   Text file write type Unicode 

            StreamWriter w = new StreamWriter(strPath, IsAppend, encoding);

            w.WriteLine(logMessage);

            w.Flush();

            w.Close();
        }

        /// <summary>
        /// 텍스트파일로 저장
        /// </summary>
        /// <param name="logMessage"></param>
        public static void WriteErrorLog(string logMessage)
        {
            //string strPath = @"C:\Log.txt";
            StreamWriter w = File.AppendText(ErrorLogPath);

            Console.WriteLine(logMessage);

            w.WriteLine(DateTime.Now.ToString() + " ============================================");
            w.WriteLine(logMessage);
            w.WriteLine("");


            w.Flush();

            w.Close();
        }

        public static void SaveLog(string strPath, string strStepName, string logMessage)
        {
            StreamWriter w = File.AppendText(strPath);

            w.WriteLine("Step Name\t:" + "\t" + strStepName);
            w.WriteLine("Working Time\t:" + "\t" + DateTime.Now.ToString());
            w.WriteLine("");
            w.WriteLine(logMessage + "\t" + GetStringByte("-",60));

            w.Flush();

            w.Close();
        }

        /// <summary>
        /// 텍스트 파일의 문자열을 줄단위로 읽어와 배열로 저장
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static ArrayList ReadLog(string strPath)
        {
            StreamReader r = File.OpenText(strPath);
            ArrayList arr = new ArrayList();

            while (!r.EndOfStream)
            {
                arr.Add(r.ReadLine());
            }

            r.Close();

            return arr;
        }

        /// <summary>
        /// 텍스트 파일의 문자열을 줄단위로 읽어와 배열로 저장
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string ReadText(string strPath)
        {
            StreamReader r = File.OpenText(strPath);
            string result = "";

            while (!r.EndOfStream)
            {
                result += r.ReadLine();
            }

            r.Close();

            return result;
        }

        /// <summary>
        /// string 을 byte길이많큼 잘라서 string 으로 반환
        /// </summary>
        /// <param name="StringValue"></param>
        /// <param name="ByteCnt"></param>
        /// <returns></returns>
        public static string GetStringByte(string StringValue, int ByteCnt)
        {
            string lsTmpStringValue = string.Empty;
            string lsReturnValue = string.Empty;
            int liByteCnt = 0;

            StringValue = StringValue.Replace("\t", " ");

            try
            {
                StringValue = StringValue.PadRight(ByteCnt, ' ').ToString();

                for (int liStartPoint = 0; liStartPoint < StringValue.Length; liStartPoint++)
                {
                    lsTmpStringValue = lsTmpStringValue + StringValue.Substring(liStartPoint, 1);
                    liByteCnt = Encoding.Default.GetByteCount(lsTmpStringValue);

                    if (liByteCnt == ByteCnt)
                    {
                        lsReturnValue = lsTmpStringValue.ToString();
                        break;
                    }
                    else if (liByteCnt > ByteCnt)
                    {
                        lsReturnValue = lsTmpStringValue.Substring(0, lsTmpStringValue.Length - 1);

                        lsReturnValue = lsReturnValue + string.Empty.PadRight(1, ' ').ToString();
                        break;
                    }
                }
                return lsReturnValue.ToString();

            }
            catch
            {
                return string.Empty.PadRight(ByteCnt, ' ').ToString();
            }
        }

        /// <summary>
        /// 문자를 인수에 맞게 붙여서 리턴한다.
        /// </summary>
        /// <param name="pText">대상 문자열</param>
        /// <param name="pCnt">문자열 반복갯수</param>
        /// <returns></returns>
        public static string SetStrAdd(string pText, int pCnt)
        {
            string retVal = string.Empty;

            for (int i = 1; i < pText.Length; i++)
            {
                retVal = retVal + pText;    
            }

            return retVal;
        }

    }//END class
}//END namespace
