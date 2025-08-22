using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UDC.Common
{
    public class GeneralHelpers
    {
        public static CultureInfo getCurrentCulture(String value)
        {
            CultureInfo objCulture = new CultureInfo("en-AU");
            return objCulture;
        }

        public static Int32 parseInt32(String value)
        {
            Int32 retVal = 0;
            Int32.TryParse(value, out retVal);
            return retVal;
        }
        public static Int32 parseInt32(Object value)
        {
            Int32 retVal = 0;
            if (value != null)
            {
                Int32.TryParse(value.ToString(), out retVal);
            }
            return retVal;
        }
        public static Int32 parseInt32(int? value)
        {
            Int32 retVal = 0;
            if (value.HasValue)
            {
                retVal = value.Value;
            }
            return retVal;
        }

        public static Int64 parseInt64(String value)
        {
            Int64 retVal = 0;
            Int64.TryParse(value, out retVal);
            return retVal;
        }
        public static Int64 parseInt64(Object value)
        {
            Int64 retVal = 0;
            if (value != null)
            {
                Int64.TryParse(value.ToString(), out retVal);
            }
            return retVal;
        }
        public static Int64 parseInt64(Int64? value)
        {
            Int64 retVal = 0;
            if (value.HasValue)
            {
                retVal = value.Value;
            }
            return retVal;
        }

        public static Double parseDouble(String value)
        {
            Double retVal = 0;
            Double.TryParse(value, out retVal);
            return retVal;
        }
        public static Double parseDouble(Object value)
        {
            Double retVal = 0;
            if (value != null)
            {
                Double.TryParse(value.ToString(), out retVal);
            }
            return retVal;
        }
        public static Double parseDouble(Double? value)
        {
            Double retVal = 0;
            if (value.HasValue)
            {
                retVal = value.Value;
            }
            return retVal;
        }

        public static String parseString(String value)
        {
            String retVal = "";
            if (!String.IsNullOrEmpty(value))
            {
                retVal = value.ToString();
            }
            return retVal;
        }
        public static String parseString(Object value)
        {
            String retVal = "";
            if (value != null)
            {
                retVal = value.ToString();
            }
            return retVal;
        }

        public static Guid parseGUID(String value)
        {
            Guid objGuid = Guid.Empty;

            if (!Guid.TryParse(value, out objGuid))
            {
                objGuid = Guid.Empty;
            }

            return objGuid;
        }

        public static Boolean parseBool(String value)
        {
            Boolean retVal = false;
            if (!String.IsNullOrEmpty(value))
            {
                value = value.ToLower();
                if (value == "1" || value == "true" || value == "yes" || value == "on")
                {
                    retVal = true;
                }
            }
            return retVal;
        }
        public static Boolean parseBool(Object value)
        {
            Boolean retVal = false;
            if (value != null)
            {
                retVal = parseBool(parseString(value));
            }
            return retVal;
        }

        public static DateTime parseDate(String value)
        {
            DateTime objRetVal = DateTime.MinValue;
            DateTime.TryParse(value, out objRetVal);
            return objRetVal;
        }
        public static DateTime parseDate(Object value)
        {
            DateTime objRetVal = DateTime.MinValue;
            if (value != null)
            {
                DateTime.TryParse(parseString(value), out objRetVal);
            }
            return objRetVal;
        }
        public static Int64 toEpoch(DateTime target)
        {
            return (Int64)(target - new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static DateTime fromEpoch(Int64 target)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(target);
        }

        public static Byte[] parseBinaryStream(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static List<Int32> GetBytes(String src)
        {
            List<Int32> arr = new List<Int32>();
            for (var i = 0; i < src.Length; i++)
            {
                arr.Add((Int32)src[i]);
            }
            return arr;
        }
        private static String ToIntString(List<Int32> arr)
        {
            String str = "";
            for (var i = 0; i < arr.Count; i++)
            {
                str += arr[i] + ",";
            }
            if (str.EndsWith(","))
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
        private static Boolean StrEqual(String str1, String str2)
        {
            Boolean retVal = false;
            Int32[] arr1 = GetBytes(str1).ToArray();
            Int32[] arr2 = GetBytes(str2).ToArray();

            retVal = arr1.SequenceEqual(arr2);

            arr1 = null;
            arr2 = null;

            return retVal;
        }

        public static String getRenderableString(String value)
        {
            return Regex.Replace(value, @"[^\u001F-\u007E]", String.Empty);
        }
        public static String stripHtml(String value)
        {
            return Regex.Replace(value, @"<(.|\n)*?>", String.Empty);
        }
        public static String sanitizeString(String value)
        {
            return getRenderableString(stripHtml(value));
        }

        public static Dictionary<String, Object> objectToDictionary(Object src)
        {
            Dictionary<String, Object> arrRetVal = null;
            String str = JsonConvert.SerializeObject(src, Formatting.None,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });
            arrRetVal = JsonConvert.DeserializeObject<Dictionary<String, Object>>(str);

            return arrRetVal;
        }
        public static void addUpdateDictionary(ref Dictionary<String, Object> baseObj, String key, Object value)
        {
            if(baseObj != null)
            {
                if(!baseObj.ContainsKey(key))
                {
                    baseObj.Add(key, value);
                }
                else
                {
                    baseObj[key] = value;
                }
            }
        }
        public static void appendDictionary(ref Dictionary<String, Object> baseObj, ref Dictionary<String, Object> toAppend)
        {
            if (toAppend != null && baseObj != null)
            {
                foreach (String key in toAppend.Keys)
                {
                    if(!baseObj.ContainsKey(key))
                    {
                        baseObj.Add(key, toAppend[key]);
                    }
                    else
                    {
                        baseObj[key] = toAppend[key];
                    }
                }
            }
        }

        public static Dictionary<String, String> ParseAdditionalConfigs(String configs)
        {
            Dictionary<String, String> dict = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
            if (!String.IsNullOrWhiteSpace(configs))
            {
                String[] pairs = configs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String pair in pairs)
                {
                    String[] kv = pair.Split(new char[] { ':' }, 2);
                    if (kv.Length == 2)
                    {
                        dict[kv[0].Trim()] = kv[1].Trim();
                    }
                }
            }
            return dict;
        }

        public static List<String> toStringList(String SourceString, Char[] delimiter)
        {
            String[] arrParts = SourceString.Split(delimiter);
            List<String> arrRetVal = new List<String>();
            if (arrParts != null)
            {
                foreach (String strValue in arrParts)
                {
                    if (strValue.Length > 0)
                    {
                        arrRetVal.Add(parseString(strValue).Trim());
                    }
                }
            }
            return arrRetVal;
        }

        public static String stringListToCSV(List<String> TargetArray)
        {
            String strRetVal = "";
            if (TargetArray != null)
            {
                if (TargetArray.Count > 0)
                {
                    foreach (String strValue in TargetArray)
                    {
                        strRetVal += strValue + ",";
                    }
                }
            }
            if (strRetVal.EndsWith(","))
            {
                strRetVal = strRetVal.Substring(0, strRetVal.Length - 1);
            }
            return strRetVal;
        }

        public static String assembleString(List<String> parts, String delimeter)
        {
            String strRetVal = "";
            if (parts != null)
            {
                foreach (String strValue in parts)
                {
                    if (!String.IsNullOrWhiteSpace(strValue))
                    {
                        strRetVal += strValue + delimeter;
                    }
                }
            }
            if (strRetVal.EndsWith(delimeter))
            {
                strRetVal = strRetVal.Substring(0, strRetVal.Length - delimeter.Length);
            }

            return strRetVal;
        }
        public static String formattedAppendString(List<Tuple<String, String>> formattedAppends)
        {
            String strRetVal = "";
            if (formattedAppends != null && formattedAppends.Count > 0)
            {
                foreach (Tuple<String, String> append in formattedAppends)
                {
                    if (!String.IsNullOrWhiteSpace(append.Item2))
                    {
                        strRetVal += String.Format(append.Item1, append.Item2);
                    }
                }
            }

            return strRetVal;
        }

        public static String prettyDate(DateTime? value)
        {
            return (value != null && value.HasValue) ? prettyDate(value.Value) : "";
        }
        public static String prettyDate(DateTime value)
        {
            return value.ToString("dddd, dd MMMM yyyy");
        }
        public static String prettyDateTime(DateTime? value)
        {
            return (value != null && value.HasValue) ? prettyDateTime(value.Value) : "";
        }
        public static String prettyDateTime(DateTime value)
        {
            return prettyDate(value) + " " + getTimeString(value);
        }
        public static String uniDate(DateTime value)
        {
            return value.ToString("yyyy/MM/dd");
        }
        public static String getTimeString(DateTime value)
        {
            return value.ToString("HH:mm:ss");
        }
        public static String uniDateTime(DateTime value)
        {
            return uniDate(value) + " " + getTimeString(value);
        }

        public static String toTitleCase(String str)
        {
            var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }
        public static String formatEnum(String Src)
        {
            String strOutput = "";
            Int32 i = 0;
            foreach (Char objChar in Src)
            {
                if (Convert.ToInt32(objChar) > 64 && Convert.ToInt32(objChar) < 91 && i > 0)
                {
                    strOutput += ' ';
                    strOutput += objChar;
                }
                else
                {
                    strOutput += objChar;
                }
                i++;
            }
            return strOutput;
        }

        public static String prettyFileSize(Double fileSize)
        {
            fileSize = (fileSize / 1024) / 1024;
            return $"{fileSize:0.00} MB";
        }
        public static String prettyFileSize(String fileSize)
        {
            Double fileSizeValue = parseInt64(fileSize);
            String retValue = prettyFileSize(fileSizeValue);
            return retValue;
        }
    }
}