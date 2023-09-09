using my_remove_noice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static my_remove_noice.Program;

namespace utility
{
    public class myapp
    {
        public void step1CreatePath(string tmpPath)
        {
            my.myLog("step1CreatePath: Create Temp Path: " + tmpPath);
            if (!my.is_dir(tmpPath))
            {
                my.mkdir(tmpPath);
            }
            my.myLog("");
        }
        public void step2FileToMp3(string sourceFile, string targetFile)
        {
            my.myLog("step2FileToMp3: \r\nsourceFile: " + sourceFile + "\r\ntargetFile: " + targetFile);
            string dn = my.dirname(targetFile);
            string mn = my.mainname(targetFile);
            string CMD = my.pwd() + "\\binary\\ffmpeg.exe -y -i \"" + sourceFile + "\" -vn \"" + dn + "\\" + mn + ".mp3\" && exit";
            my.myLog(CMD);
            my.system_background(CMD);
            my.myLog("");
        }

        public void step4Output()
        {
            //輸出結果
            my.myLog("輸出結果");
            switch (setting.isOutputMP4)
            {
                case true:
                    switch (setting.isInputMP4)
                    {
                        case true:
                            //先產無聲的影片
                            string noSoundVideo = setting.tempPath + "\\noSound.mp4";
                            string CMD = my.pwd() + "\\binary\\ffmpeg.exe -y -i \"" + setting.sourceFile + "\" -an \"" + noSoundVideo + "\" && exit";
                            my.system_background(CMD);
                            my.myLog("CMD: " + CMD);
                            //合併聲音
                            CMD = my.pwd() + "\\binary\\ffmpeg.exe -y -i \"" + noSoundVideo + "\" -i \"" + setting.tempMP3 + "\" -c:v copy -c:a aac -strict experimental -shortest \"" + setting.outputFile + "\" && exit";
                            my.system_background(CMD);
                            my.myLog("CMD: "+CMD);
                            break;
                        case false:
                            //輸入檔不是 mp4
                            //輸出的 mp4 即是 mp3 而已
                            my.copy(setting.tempMP3, setting.outputFile);
                            break;
                    }
                    my.deltree(setting.tempPath);
                    break;
                case false:
                    //copy tempMp3 to outputFile
                    my.copy(setting.tempMP3, setting.outputFile);
                    my.deltree(setting.tempPath);
                    break;
            }
            my.myLog("");
        }
    }
}
