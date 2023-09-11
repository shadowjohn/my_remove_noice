using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utility;
namespace my_remove_noice
{
    internal class Program
    {
        public class settingEntity
        {
            public string sourceFile;
            public string outputFile;
            public bool isInputMP4 = false;
            public bool isOutputMP4 = false;
            public string tempPath = string.Empty;
            public string tempWAV = string.Empty;
        };
        public static myinclude my = new myinclude();
        public static myapp App = new myapp();

        static List<string> fileFilters = new List<string>() { "mp4", "mp3", "wav" };
        static string usageMessage = @"

This program can remove mp4 movie or mp3 file noise.

Author: FeatherMountain (https://3wa.tw/)

Version: 0.01
my_remove_noice.exe [sourceFile] -o [targetFile] [options]

File Filters: mp4 mp3 wav

options:    
    -h 或 -? (顯示此說明)        
    -o [Output file]
";
        public static void exit()
        {
            Environment.Exit(0);
        }
        public static settingEntity setting = new settingEntity();
        static void Main(string[] args)
        {

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-o" && i + 1 < args.Length)
                {
                    setting.outputFile = args[i + 1];
                    i++; // Skip the next argument as it has been processed as the value of -s_srs.
                    string sn = my.subname(setting.outputFile).ToLower();
                    if (!my.in_array(sn, fileFilters))
                    {
                        my.myLog("outputFile: " + setting.outputFile + " not in " + my.implode(",", fileFilters));
                        Environment.Exit(0);
                    }
                    if (sn == "mp4")
                    {
                        setting.isOutputMP4 = true;
                    }

                }
                else
                {
                    setting.sourceFile = args[i];
                    string sn = my.subname(setting.sourceFile).ToLower();
                    if (!my.in_array(sn, fileFilters))
                    {
                        my.myLog("sourceFile: " + setting.sourceFile + " not in " + my.implode(",", fileFilters));
                        Environment.Exit(0);
                    }
                    if (sn == "mp4")
                    {
                        setting.isInputMP4 = true;
                    }
                }
            }

            setting.tempPath = my.pwd() + "\\temp\\" + my.time();
            setting.tempWAV = setting.tempPath + "\\" + my.time() + ".wav";

            //開始
            //step1. 建立暫存目錄
            App.step1CreatePath(setting.tempPath);

            //step2. 變 temp wav
            App.step2FileToWAV(setting.sourceFile, setting.tempWAV);

            //step3. 處影聲音爆聲問題
            App.step3FixNoise(setting.tempWAV);

            //step4. 輸出結果
            App.step4Output();
        }
    }

}
