using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public void step2FileToWAV(string sourceFile, string targetFile)
        {
            my.myLog("step2FileToWAV: \r\nsourceFile: " + sourceFile + "\r\ntargetFile: " + targetFile);
            string dn = my.dirname(targetFile);
            string mn = my.mainname(targetFile);
            string CMD = my.pwd() + "\\binary\\ffmpeg.exe -y -i \"" + sourceFile + "\" -ac 2 -ar 44100 -acodec pcm_s16le  \"" + dn + "\\" + mn + ".wav\" && exit";
            my.myLog(CMD);
            my.system_background(CMD);
            my.myLog("");
        }
        public static float[] tempFloat = null;
        public void step3FixNoise(string tmpWAVPath) //修正爆聲問題
        {
            string dn = my.dirname(tmpWAVPath);
            string fixedWAV_UP = dn + "\\fixedWAV_UP.wav";
            string fixedWAV = dn + "\\fixedWAV.wav";

            //先調大聲
            using (var audioFileReader = new AudioFileReader(tmpWAVPath))
            {
                // Create a SimpleCompressor with desired settings
                // Adjust the threshold and ratio as needed
                float boostFactor = 1.6f; // Adjust this value as needed
                var boostEffect = new HighFrequencyBoostEffect(audioFileReader, boostFactor);

                //buffer[i] = (byte)lowPassFilter.Transform(buffer[i]);

                // Create a WaveFileWriter for the output
                // Convert the effect to IWaveProvider using WaveProviderToWaveStream                
                WaveFileWriter.CreateWaveFile(fixedWAV_UP, boostEffect.ToWaveProvider());
            }
            //my.myLog(my.json_encode(tempFloat));
            //然後找到尖波，刪掉
            //using (var audioFileReader = new AudioFileReader(fixedWAV_UP))
            //{
            //}
            my.copy(fixedWAV_UP, tmpWAVPath);
        }

        class HighFrequencyBoostEffect : ISampleProvider
        {
            private ISampleProvider sourceProvider;
            private float boostFactor;

            public WaveFormat WaveFormat => sourceProvider.WaveFormat;

            public HighFrequencyBoostEffect(ISampleProvider sourceProvider, float boostFactor)
            {
                this.sourceProvider = sourceProvider;
                this.boostFactor = boostFactor;
            }

            public byte[] ConvertFloatArrayToByteArray(float[] floatArray)
            {
                byte[] byteArray = new byte[floatArray.Length * 4]; // Each float is 4 bytes

                for (int i = 0; i < floatArray.Length; i++)
                {
                    byte[] floatBytes = BitConverter.GetBytes(floatArray[i]);

                    // Assuming little-endian architecture (most common)
                    if (BitConverter.IsLittleEndian)
                    {
                        // Reverse the byte order to match little-endian format
                        Array.Reverse(floatBytes);
                    }

                    // Copy the bytes to the byte array
                    Array.Copy(floatBytes, 0, byteArray, i * 4, 4);
                }

                return byteArray;
            }


            public int Read(float[] buffer, int offset, int count)
            {

                // 創建壓縮器效果
                //int samplesRead = sourceProvider.Read(buffer, offset, count);                
                //sourceProvider.ToMono();
                int samplesRead = sourceProvider.Read(buffer, offset, count);


                //人說話聲音範圍 500~4000
                //From : 
                var HighPassFilter = BiQuadFilter.HighPassFilter(44100, 400f, 1.0f);
                var LowPassFilter = BiQuadFilter.LowPassFilter(44100, 4000.0f, 1.0f);

                //var NoisePassFilter = BiQuadFilter.(44100, 100.0f, 1.0f);

                float[] buffer_find_point = new float[buffer.Length];

                //Array.Copy(buffer, buffer_find_point, buffer.Length);

                for (int n = 1; n < samplesRead; n++)
                {
                    if (Math.Abs(buffer[n] - buffer[n - 1]) > 0.006)
                    {
                        while (Math.Abs(buffer[n] - buffer[n - 1]) > 0.003)
                        {
                            float mm = Math.Abs(buffer[n] - buffer[n - 1]);
                            //max_minus = (mm > max_minus) ? mm : max_minus;
                            //if (mm >= 0.02)
                            {
                                buffer[n] = (buffer[n] + buffer[n - 1]) / 2.0f;
                            }
                        }
                    }
                }
                float maxM = 0;
                float minM = 0;
                for (int n = 0; n < samplesRead; n++)
                {
                    maxM = (buffer[n] > maxM) ? buffer[n] : maxM;
                    minM = (buffer[n] < minM) ? buffer[n] : minM;
                    //buffer[n] = HighPassFilter.Transform(buffer[n]);
                    //buffer[n] = LowPassFilter.Transform(buffer[n]);

                }
                Console.WriteLine("minM: " + minM);
                Console.WriteLine("maxM: " + maxM);
                for (int n = 1; n < samplesRead; n++)
                {
                    if (Math.Abs(buffer[n]) < 0.1 && buffer[n] > 0)
                    {
                        buffer[n] = (float)my.arduino_map(buffer[n], 0.0f, 0.1f, 0f, 0.1f, 1.4f);
                        //buffer[n] *= 0.8f;
                    }
                    else
                    {
                        //buffer[n] = (float)my.arduino_map(buffer[n], 0.0f, minM, 0.0f, -0.8f, 1.2f);
                        //buffer[n] *= 0.8f;
                    }
                    //buffer[n] = HighPassFilter.Transform(buffer[n]);
                    //buffer[n] = LowPassFilter.Transform(buffer[n]);
                }
                
                //tempFloat = new float[buffer.Length];
                //Array.Copy(buffer, tempFloat, buffer.Length);
                return samplesRead;
            }


            public float[] ExponentialSmoothing(float[] data, float alpha)
            {
                float[] smoothedData = new float[data.Length];
                smoothedData[0] = data[0];

                for (int i = 1; i < data.Length; i++)
                {
                    smoothedData[i] = alpha * data[i] + (1 - alpha) * smoothedData[i - 1];
                }

                return smoothedData;
            }
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
                            CMD = my.pwd() + "\\binary\\ffmpeg.exe -y -i \"" + noSoundVideo + "\" -i \"" + setting.tempWAV + "\" -c:v copy -c:a aac -strict experimental -shortest \"" + setting.outputFile + "\" && exit";
                            my.system_background(CMD);
                            my.myLog("CMD: " + CMD);
                            break;
                        case false:
                            //輸入檔不是 mp4
                            //輸出的 mp4 即是 wav 而已
                            my.copy(setting.tempWAV, setting.outputFile);
                            break;
                    }
                    my.deltree(setting.tempPath);
                    break;
                case false:
                    //copy tempWAV to outputFile
                    my.copy(setting.tempWAV, setting.outputFile);
                    my.deltree(setting.tempPath);
                    break;
            }
            my.myLog("");
        }
    }

}
