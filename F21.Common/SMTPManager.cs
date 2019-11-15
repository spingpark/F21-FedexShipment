using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       SMTPManager
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        메일관련 클레스
     *
     *  Program     (PrintLabel) Biz.UPS\SMTPManager.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     ******************************************************************************************/
    public class SMTPManager
    {
        MailAddress mailFrom;
        MailAddress mailTo;
        MailMessage mailMessage;

        SmtpClient client;

        public SMTPManager(string from, string to)
        {
            client = new SmtpClient("10.204.12.55", 25);

            mailFrom = new MailAddress(from);
            mailTo = new MailAddress(to);

            mailMessage = new MailMessage(from, to);
        }

        public SMTPManager(string Server, int Port, string from, string to)
        {
            client = new SmtpClient(Server, Port);

            mailFrom = new MailAddress(from);
            mailTo = new MailAddress(to);

            mailMessage = new MailMessage(from, to);

        }

        public void AddCC(string emali)
        {
            MailAddress bcc1 = new MailAddress(emali);
            mailMessage.CC.Add(bcc1);
        }

        public bool SendMail(string Subject, string Body)
        {
            // Include credentials if the server requires them.
            client.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            mailMessage.Subject = Subject;
            mailMessage.Body = Body;
            mailMessage.IsBodyHtml = true;

            try
            {
                client.Send(mailMessage);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] " + ex.Message);
                return false;
            }

            return true;
        }
    }//END class
}//END namespace
