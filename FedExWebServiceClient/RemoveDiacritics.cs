using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using System.Runtime.Serialization.Formatters.Soap;

namespace F21.Service
{
    public static class RemoveDiacritics
    {
        public static string RemoveAccent(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }

    public static class Serialize
    {
        #region ### SerializeToSoap
        public static byte[] serializeToSoap(object graph)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                SoapFormatter formatter = new SoapFormatter();

                //BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, graph);
                //
                byte[] soapBytes = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(soapBytes, 0, Convert.ToInt32(stream.Length));
                //
                stream.Close();
                return soapBytes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return new byte[0];
        }
        #endregion
    }
}
