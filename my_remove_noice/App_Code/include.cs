using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Reflection;
/*
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="webservice.aspx.cs" Inherits="WaterRegion.Search.webservice.webservice"  %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
*/

namespace utility
{
    public class myinclude
    {
        private Random rnd = new Random(DateTime.Now.Millisecond);

        public myinclude()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }
        public string str_replace(string r, string t, string data)
        {
            return data.Replace(r, t);
        }
        public int rand(int min, int max)
        {
            return rnd.Next(min, max);
        }
        public string pwd()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //return dirname(System.Web.HttpContext.Current.Server.MapPath("~/"));
        }
        public bool is_dir(string path)
        {
            return Directory.Exists(path);
        }
        public bool is_file(string filepath)
        {
            return File.Exists(filepath);
        }
        public void myLog(String data)
        {
            myLog(data, false);
        }
        public void myLog(String data, bool isWriteFile = false)
        {
            Console.WriteLine(data);
            if (isWriteFile)
            {
                string path = pwd() + "\\Log";
                if (!is_dir(path))
                {
                    mkdir(path);
                }
                string filename = String.Format("{0}.txt", date("Y-m-d"));
                string fn = String.Format("{0}\\{1}", path, filename);
                if (!is_file(fn))
                {
                    touch(fn);
                }
                file_put_contents(fn, string.Format("\r\n\r\n{0} -\r\n{1}", date("Y-m-d H:i:s"), data), true);
            }
        }
        public void touch(string fileName)
        {
            FileStream myFileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            myFileStream.Close();
            myFileStream.Dispose();
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow);
        }

        public string get_between(string data, string s_begin, string s_end)
        {
            //http://stackoverflow.com/questions/378415/how-do-i-extract-a-string-of-text-that-lies-between-two-parenthesis-using-net
            //string a = "abcdefg";
            //MessageBox.Show(my.get_between(a, "cde", "g"));
            //return f;
            string s = data;
            int start = s.IndexOf(s_begin);
            string new_s = data.Substring(start + s_begin.Length);
            int end = new_s.IndexOf(s_end);
            return s.Substring(start + s_begin.Length, end);
        }
        public bool is_string_like(string data, string find_string)
        {
            return (data.IndexOf(find_string) == -1) ? false : true;
        }
        public bool is_string_like_new(string data, string find_string)
        {
            /*
              is_string_like($data,$fine_string)

              $mystring = "Hi, this is good!";
              $searchthis = "%thi% goo%";

              $resp = string_like($mystring,$searchthis);


              if ($resp){
                 echo "milike = VERDADERO";
              } else{
                 echo "milike = FALSO";
              }

              Will print:
              milike = VERDADERO

              and so on...

              this is the function:
            */
            bool tieneini = false;
            if (find_string == "") return true;
            var vi = explode("%", find_string);
            int offset = 0;
            for (int n = 0, max_n = vi.Count(); n < max_n; n++)
            {
                if (vi[n] == "")
                {
                    if (vi[0] == "")
                    {
                        tieneini = true;
                    }
                }
                else
                {
                    //newoff =  strpos(data,vi[$n],offset);
                    int newoff = data.IndexOf(vi[n], offset);
                    if (newoff != -1)
                    {
                        if (!tieneini)
                        {
                            if (offset != newoff)
                            {
                                return false;
                            }
                        }
                        if (n == max_n - 1)
                        {
                            if (vi[n] != data.Substring(data.Length - vi[n].Length, vi[n].Length))
                            {
                                return false;
                            }

                        }
                        else
                        {
                            offset = newoff + vi[n].Length;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool is_istring_like(string data, string find_string)
        {
            return (data.ToUpper().IndexOf(find_string.ToUpper()) == -1) ? false : true;
        }

        //大小寫
        public string strtoupper(string input)
        {
            return input.ToUpper();
        }
        public string strtolower(string input)
        {
            return input.ToLower();
        }
        public string UTF8toBig5(string strInput)
        {
            byte[] strut8 = System.Text.Encoding.Unicode.GetBytes(strInput);
            byte[] strbig5 = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.Default, strut8);
            return System.Text.Encoding.Default.GetString(strbig5);
        }

        //UnixTimeToDateTime
        public DateTime UnixTimeToDateTime(string text)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            // Add the number of seconds in UNIX timestamp to be converted.            
            dateTime = dateTime.AddSeconds(Convert.ToDouble(text));
            return dateTime;
        }
        //仿php的date
        public string time()
        {
            return strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        public string date()
        {
            return date("Y-m-d H:i:s", strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        public string date(string format)
        {
            return date(format, strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        public string date(string format, string unixtimestamp)
        {
            DateTime tmp = UnixTimeToDateTime(unixtimestamp);
            tmp = tmp.AddHours(+8);
            switch (format)
            {
                case "Y-m-d H:i:s":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss");
                case "Y/m/d":
                    return tmp.ToString("yyyy/MM/dd");
                case "Y/m/d H:i:s":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss");
                case "Y/m/d H:i:s.fff":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss.fff");
                case "Y-m-d_H_i_s":
                    return tmp.ToString("yyyy-MM-dd_HH_mm_ss");
                case "Y-m-d":
                    return tmp.ToString("yyyy-MM-dd");
                case "H:i:s":
                    return tmp.ToString("HH:mm:ss");
                case "H:i":
                    return tmp.ToString("HH:mm");
                case "Y-m-d H:i":
                    return tmp.ToString("yyyy-MM-dd HH:mm");
                case "Y_m_d_H_i_s":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss");
                case "Y_m_d_H_i_s_fff":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                case "w":
                    //回傳week, sun =0 , sat = 6, mon=1.....
                    return Convert.ToInt16(tmp.DayOfWeek).ToString();
                case "Y":
                    return tmp.ToString("yyyy");
                case "m":
                    return tmp.ToString("MM");
                case "d":
                    return tmp.ToString("dd");
                case "H":
                    return tmp.ToString("HH");
                case "i":
                    return tmp.ToString("mm");
                case "s":
                    return tmp.ToString("ss");
                case "Y-m-d H:i:s.fff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                case "Y-m-d H:i:s.ffffff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                case "H:i:s.fff":
                    return tmp.ToString("HH:mm:ss.fff");
                case "H:i:s.ffffff":
                    return tmp.ToString("HH:mm:ss.ffffff");
                case "N":
                    //回傳星期1~星期日 (1~7)
                    Dictionary<string, string> w = new Dictionary<string, string>();
                    w["Monday"] = "1";
                    w["Tuesday"] = "2";
                    w["Wednesday"] = "3";
                    w["Thursday"] = "4";
                    w["Friday"] = "5";
                    w["Saturday"] = "6";
                    w["Sunday"] = "7";
                    return w[tmp.DayOfWeek.ToString()];
            }
            return "";
        }
        //strtotime 轉換成 Unix time
        public string strtotime(string value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (Convert.ToDateTime(value) - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            if (is_string_like(value, "."))
            {
                //有小數點               
                double sec = span.Ticks / (TimeSpan.TicksPerMillisecond / 1000.0) / 1000000.0;
                return sec.ToString();
            }
            else
            {
                return span.TotalSeconds.ToString();
            }
        }
        public string strtotime(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return span.TotalSeconds.ToString();
        }
        //javascript用的吐js資料
        public string jsAddSlashes(string value)
        {
            value = value.Replace("\\", "\\\\");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\r", "\\r");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("&", "\\x26");
            value = value.Replace("<", "\\x3C");
            value = value.Replace(">", "\\x3E");
            return value;
        }
        public bool in_array(string find_key, List<string> arr)
        {
            return arr.Contains(find_key);
        }
        public bool in_array(string find_key, string[] arr)
        {
            return arr.Contains(find_key);
        }
        public bool in_array(string find_key, char[] arr)
        {
            string[] o = new string[arr.Count()];
            for (int i = 0; i < arr.Count(); i++)
            {
                o[i] = arr[i].ToString();
            }
            return in_array(find_key, o);
        }
        public bool in_array(string find_key, ArrayList arr)
        {
            return arr.Contains(find_key);
        }

        public byte[] file_get_contents(string url)
        {
            if (url.ToLower().IndexOf("http:") > -1 || url.ToLower().IndexOf("https:") > -1)
            {
                // URL                 
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                byte[] byteData = null;

                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 60000;
                request.Proxy = null;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36";
                //request.Referer = getSystemKey("HTTP_REFERER");
                response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                byteData = ReadStream(stream, 32765);
                response.Close();
                stream.Close();
                return byteData;
            }
            else
            {
                byte[] data;
                using (var fs = new FileStream(url, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    data = ReadStream(fs, 8192);
                    fs.Close();
                };
                return data;
            }
        }

        public void file_put_contents(string filepath, string input)
        {
            file_put_contents(filepath, s2b(input), false);
        }
        public void file_put_contents(string filepath, byte[] input)
        {
            file_put_contents(filepath, input, false);
        }
        public void file_put_contents(string filepath, string input, bool isFileAppend)
        {
            file_put_contents(filepath, s2b(input), isFileAppend);
        }
        public void file_put_contents(string filepath, byte[] input, bool isFileAppend)
        {

            switch (isFileAppend)
            {
                case true:
                    {
                        FileMode FM = new FileMode();
                        if (!is_file(filepath))
                        {
                            FM = FileMode.Create;
                            using (FileStream myFile = File.Open(@filepath, FM, FileAccess.Write))
                            {
                                myFile.Seek(myFile.Length, SeekOrigin.Begin);
                                myFile.Write(input, 0, input.Length);
                                myFile.Dispose();
                            }
                        }
                        else
                        {
                            FM = FileMode.Append;
                            using (FileStream myFile = File.Open(@filepath, FM, FileAccess.ReadWrite))
                            {
                                myFile.Seek(myFile.Length, SeekOrigin.Begin);
                                myFile.Write(input, 0, input.Length);
                                myFile.Dispose();
                            }
                        }
                    }
                    break;
                case false:
                    {
                        using (FileStream myFile = File.Open(@filepath, FileMode.Create, FileAccess.Write))
                        {
                            myFile.Write(input, 0, input.Length);
                            myFile.Dispose();
                        };
                    }
                    break;
            }
        }

        public string b2s(byte[] input)
        {
            return System.Text.Encoding.UTF8.GetString(input);
        }
        public byte[] s2b(string input)
        {
            return System.Text.Encoding.UTF8.GetBytes(input);
        }
        private byte[] ReadStream(Stream stream, int initialLength)
        {
            if (initialLength < 1)
            {
                initialLength = 32768;
            }
            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] bytes = new byte[read];
            Array.Copy(buffer, bytes, read);
            return bytes;
        }


        public string implode(string keyword, string[] arrays)
        {
            return string.Join(keyword, arrays);
        }
        public string implode(string keyword, List<string> arrays)
        {
            return string.Join<string>(keyword, arrays);
        }
        public string implode(string keyword, Dictionary<int, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (int k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, Dictionary<string, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (string k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, ArrayList arrays)
        {
            string[] tmp = new String[arrays.Count];
            for (int i = 0; i < arrays.Count; i++)
            {
                tmp[i] = arrays[i].ToString();
            }
            return string.Join(keyword, tmp);
        }
        public string[] explode(string keyword, string data)
        {
            return data.Split(new string[] { keyword }, StringSplitOptions.None);
        }
        public string[] explode(string keyword, object data)
        {
            return data.ToString().Split(new string[] { keyword }, StringSplitOptions.None);
        }
        public string[] explode(string[] keyword, string data)
        {
            return data.Split(keyword, StringSplitOptions.None);
        }
        public string size_hum_read_v2(string _size)
        {
            return size_hum_read_v2(Convert.ToInt64(_size));
        }
        public string size_hum_read_v2(long _size)
        {
            if (_size != 0)
            {
                List<string> unit = new List<string>();
                unit.Add("B");
                unit.Add("KB");
                unit.Add("MB");
                unit.Add("GB");
                unit.Add("TB");
                unit.Add("PB");
                int i = Convert.ToInt32(Math.Floor(Math.Log(_size, 1024)));
                return string.Format("{0:0.00}", Math.Round(Convert.ToDouble(_size) / Convert.ToDouble(Math.Pow(1024, i)), 2)) + " " + unit[i];
            }
            else
            {
                return "0 B";
            }
        }

        public string EscapeUnicode(string input)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            foreach (char ch in input)
            {
                if (ch <= 0x7f)
                    sb.Append(ch);
                else
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)ch);
            }
            return sb.ToString();
        }
        public string unEscapeUnicode(string input)
        {
            return Regex.Unescape(input);
        }
        public string json_encode(object input, bool isUtf8 = false)
        {
            if (isUtf8 == false)
            {
                return EscapeUnicode(JsonConvert.SerializeObject(input, Formatting.None));
            }
            else
            {
                return json_format(EscapeUnicode(JsonConvert.SerializeObject(input, Formatting.None)));
            }
        }
        public string json_format(string input)
        {
            JArray jdod = json_decode(input);
            return EscapeUnicode(JsonConvert.SerializeObject(jdod, Formatting.Indented));
        }
        public string json_format_utf8(string input)
        {
            JArray jdod = json_decode(input);
            return JsonConvert.SerializeObject(jdod, Formatting.Indented);
        }
        public string trim(string input)
        {
            return input.Trim();
        }
        public JArray json_decode(string input)
        {
            input = trim(input);
            if (input.Length != 0)
            {
                if (input.Substring(1, 1) != "[")
                {
                    input = "[" + input + "]";

                    return (JArray)JsonConvert.DeserializeObject<JArray>(input, new JsonSerializerSettings
                    {
                        DateParseHandling = DateParseHandling.None
                    });
                }
                else
                {
                    return (JArray)JsonConvert.DeserializeObject<JArray>(input, new JsonSerializerSettings
                    {
                        DateParseHandling = DateParseHandling.None
                    });
                }
            }
            else
            {
                return null;
            }
        }
        public string basename(string path)
        {
            return Path.GetFileName(path);
        }
        public string mainname(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        public string subname(string path)
        {
            //return Path.GetExtension(path);
            return Path.GetExtension(path).TrimStart('.');
        }
        public long filesize(string path)
        {
            FileInfo f = new FileInfo(path);
            return f.Length;
        }
        public long filemtime(string filename)
        {
            if (!is_file(filename))
            {
                return -1;
            }
            DateTime dt = File.GetLastWriteTime(filename);
            return Convert.ToInt64(strtotime(dt.ToString("yyyy-MM-dd HH:mm:ss")));
        }
        public string size_hum_read(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int i = 0;
            double dblSByte = Convert.ToDouble(bytes);
            if (bytes > 1024)
                for (i = 0; (bytes / 1024) > 0; i++, bytes /= 1024)
                    dblSByte = bytes / 1024.0;
            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        public string[] glob(string path)
        {
            //string[] test = my.glob("c:\\tmp");
            //my.echo(my.pre_print_r(test));
            return Directory.GetFiles(path);
        }
        public string[] glob(string path, string patten)
        {
            //string[] test = my.glob("c:\\tmp");
            //my.echo(my.pre_print_r(test));
            return Directory.GetFiles(path, patten);
        }
        public void mkdir(string path)
        {
            Directory.CreateDirectory(path);
        }
        public void copy(string sourceFile, string destFile)
        {
            System.IO.File.Copy(sourceFile, destFile, true);
        }
        public string dirname(string path)
        {
            return Directory.GetParent(path).FullName;
        }


        public string system(string command)
        {
            StringBuilder sb = new StringBuilder();
            string version = System.Environment.OSVersion.VersionString;//读取操作系统版本  
            if (version.Contains("Windows"))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "cmd.exe";

                    p.StartInfo.UseShellExecute = false;//是否指定操作系统外壳进程启动程序  
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;//不显示dos命令行窗口  

                    p.Start();//启动cmd.exe  
                    p.StandardInput.WriteLine(command);//输入命令  
                    p.StandardInput.WriteLine("exit");//退出cmd.exe  
                    p.WaitForExit();//等待执行完了，退出cmd.exe  

                    using (StreamReader reader = p.StandardOutput)//截取输出流  
                    {
                        string line = reader.ReadLine();//每次读取一行  
                        while (!reader.EndOfStream)
                        {
                            sb.Append(line).Append("<br />");//在Web中使用<br />换行  
                            line = reader.ReadLine();
                        }
                        p.WaitForExit();//等待程序执行完退出进程  
                        p.Close();//关闭进程  
                        reader.Close();//关闭流  
                    }
                }
            }
            return sb.ToString();
        }

        public string microtime()
        {
            System.DateTime dt = DateTime.Now;
            System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan span = dt - UnixEpoch;
            return string.Format("{0:0.0000}", Convert.ToDouble((span.Ticks / (TimeSpan.TicksPerMillisecond / 1000))) / 1000000.0);
        }

        public string implode(string keyword, ConcurrentDictionary<int, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (int k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }

        public string implode(string keyword, ConcurrentDictionary<string, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (string k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }

        public bool isAllowChars(string data, string allowCharsString)
        {
            var m = allowCharsString.ToCharArray();
            //echo(json_encode(m));
            for (int i = 0, max_i = data.Length; i < max_i; i++)
            {
                if (!in_array(data[i].ToString(), m))
                {
                    return false;
                }
            }
            return true;
        }

        public bool isFileLocked(string fileName)
        {
            try
            {
                FileStream fs = File.OpenWrite(fileName);
                fs.Close();
                return false;
            }

            catch (Exception) { return true; }
        }
        public string system_background(string command)
        {
            return system_background(command, 0);
        }
        public bool deltree(string targetDir)
        {
            try
            {
                // 如果目標目錄不存在，直接返回 true，表示已成功刪除
                if (!Directory.Exists(targetDir))
                    return true;

                // 遞迴刪除所有文件和子目錄
                string[] files = Directory.GetFiles(targetDir);
                string[] dirs = Directory.GetDirectories(targetDir);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    if (!deltree(dir))
                        return false; // 如果刪除子目錄失敗，則返回 false
                }

                // 最後刪除目標目錄
                Directory.Delete(targetDir);

                // 如果上面的所有操作都成功完成，返回 true
                return true;
            }
            catch
            {
                // 在捕獲異常時返回 false，表示刪除失敗
                return false;
            }
        }
        public float[] wavToFloatArr(string wavFile)
        {
            var audioFileReader = new AudioFileReader(wavFile);
            var sourceProvider = audioFileReader.ToWaveProvider();

            // 计算要读取的样本数
            int totalSamples = (int)audioFileReader.Length / (audioFileReader.WaveFormat.BitsPerSample / 8);

            // 创建一个 float 数组来存储音频数据
            byte[] audioData = new byte[totalSamples];

            int bytesRead = 0;
            int offset = 0;

            while (offset < totalSamples)
            {
                // 从音频文件中读取数据
                int samplesToRead = Math.Min(sourceProvider.WaveFormat.SampleRate, totalSamples - offset);
                bytesRead = sourceProvider.Read(audioData, offset, samplesToRead);

                if (bytesRead == 0)
                    break;

                offset += bytesRead;
            }

            // 关闭文件读取器
            audioFileReader.Dispose();


            int sampleCount = audioData.Length / 4; // 假设每个浮点数占用 4 个字节

            float[] floatArray = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                floatArray[i] = BitConverter.ToSingle(audioData, i * 4);
            }
            return floatArray;
        }
        public string system_background(string command, int timeout)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                string version = System.Environment.OSVersion.VersionString;//读取操作系统版本  
                if (version.Contains("Windows"))
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.UseShellExecute = false;//是否指定操作系统外壳进程启动程序  
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardOutput = false;
                        p.StartInfo.CreateNoWindow = true;//不显示dos命令行窗口  
                        p.Start();//启动cmd.exe  
                        p.StandardInput.WriteLine(command);//输入命令                      
                        if (timeout == 0)
                        {
                            p.StandardInput.WriteLine("exit");//退出cmd.exe 
                            p.WaitForExit();//等待执行完了，退出cmd.exe  
                        }
                        else if (timeout == -1)
                        {
                            //wont wait
                            //wont close
                        }
                        else
                        {
                            p.StandardInput.WriteLine("exit");//退出cmd.exe 
                            p.WaitForExit(timeout);//等待执行完了，退出cmd.exe  
                        }
                    }
                }
            }
            catch
            {
            }
            return sb.ToString();
        }
        public double arduino_map(double x, double inMin, double inMax, double outMin, double outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }

}