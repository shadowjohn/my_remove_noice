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
            string CMD = my.pwd() + "\\binary\\ffmpeg.exe -y -i \"" + sourceFile + "\" -ac 1 -ar 44100 -acodec pcm_s16le  \"" + dn + "\\" + mn + ".wav\" && exit";
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
            public static float[] ConvertByteArrayToFloatArray(byte[] byteArray)
            {
                int floatCount = byteArray.Length / 4; // Each float is 4 bytes
                float[] floatArray = new float[floatCount];

                for (int i = 0; i < floatCount; i++)
                {
                    byte[] floatBytes = new byte[4];

                    // Copy 4 bytes from the byteArray to the floatBytes array
                    Array.Copy(byteArray, i * 4, floatBytes, 0, 4);

                    // Reverse the byte order if necessary (assuming little-endian)
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(floatBytes);
                    }

                    // Convert the byte array back to a float
                    floatArray[i] = BitConverter.ToSingle(floatBytes, 0);
                }

                return floatArray;
            }

            public int Read(float[] buffer, int offset, int count)
            {

                // 創建壓縮器效果
                //int samplesRead = sourceProvider.Read(buffer, offset, count);                
                sourceProvider.ToMono();
                int samplesRead = sourceProvider.Read(buffer, offset, count);

                /*
                {
                    Attack = 0.01f, // 調整攻擊時間
                    Release = 0.1f, // 調整釋放時間
                    Threshold = -12.0f, // 調整閾值
                    Ratio = 4.0f, // 調整壓縮比率
                    MakeUpGain = 6.0f // 調整增益
                };
                */
                //load noise.wav
                string noiseFile = my.pwd() + "\\noise.wav";
                // 噪音檔讀入
                float[] noiseFloat = my.wavToFloatArr(noiseFile);

                //人說話聲音範圍 500~4000
                //From : 
                var HighPassFilter = BiQuadFilter.HighPassFilter(44100, 500f, 1.0f);
                var LowPassFilter = BiQuadFilter.LowPassFilter(44100, 4000.0f, 1.0f);

                //var NoisePassFilter = BiQuadFilter.(44100, 100.0f, 1.0f);

                float[] buffer_find_point = new float[buffer.Length];

                //Array.Copy(buffer, buffer_find_point, buffer.Length);
                float max_minus = 0;
                for (int n = 1; n < samplesRead - 1; n++)
                {
                    while (Math.Abs(buffer[n] - buffer[n - 1]) > 0.01)
                    {
                        float mm = Math.Abs(buffer[n] - buffer[n - 1]);
                        //max_minus = (mm > max_minus) ? mm : max_minus;
                        //if (mm >= 0.02)
                        {
                            buffer[n] = (buffer[n] + buffer[n - 1]) / 2.0f;
                        }
                    }
                }
                for (int n = 1; n < samplesRead - 1; n++)
                {
                    //buffer[n] = HighPassFilter.Transform(buffer[n]);
                    buffer[n] = LowPassFilter.Transform(buffer[n]);
                }
                //Console.WriteLine("max_minus: " + max_minus);

                if (false)
                {
                    for (int n = 99999999; n < samplesRead; n++)
                    {
                        //buffer[n] = HighPassFilter.Transform(buffer[n]);
                        //buffer[n] = LowPassFilter.Transform(buffer[n]);                   
                        //buffer[n] = HighPassFilter.Transform(buffer[n]);
                        //buffer[n] = LowPassFilter.Transform(buffer[n]);
                        //buffer[n] = (float)my.arduino_map(buffer[n], 0.0, 0.001, 0.0, 1.0);
                        //buffer[n] = (float)my.arduino_map(buffer[n], 0.1, 1.0, 0.3, 1.0);
                        if (buffer[n] > 0)
                        {
                            buffer[n] *= (float)0.1;
                        }
                        else if (buffer[n] < 0)
                        {
                            buffer[n] *= (float)0.1;
                        }
                        else
                        {
                            //buffer[n] = 0;
                        }
                        buffer[n] = (float)my.arduino_map(buffer[n], 0.01, 0.1, 0.95, 1.0);
                        buffer[n] = HighPassFilter.Transform(buffer[n]);
                        buffer[n] = LowPassFilter.Transform(buffer[n]);
                        if ((buffer[n] > 0.01 || buffer[n] < -0.01) && n - 1 >= 2)
                        {
                            //Console.WriteLine(buffer[n]);
                            //buffer[n] = buffer_find_point[n];
                            buffer[n] = buffer_find_point[n - 2];
                            //buffer[n] = 0;
                        }
                        else
                        {
                            buffer[n] = buffer_find_point[n];
                        }
                    }
                }

                //buffer_find_point[n] = HighPassFilter.Transform(buffer_find_point[n]);
                //buffer_find_point[n] = LowPassFilter.Transform(buffer_find_point[n]);

                ////buffer[n] = LowPassFilter.Transform(buffer[n]);
                //buffer[n] /= 1.6f;

                //buffer[n] = LowPassFilter.Transform(buffer[n]);
                //buffer_find_point[offset + n] = (float)Math.Pow(buffer_find_point[offset + n], 2);

                for (int n = 0; n < samplesRead; n++)
                {
                    // buffer[n] *= 1.6f;
                }

                //tempFloat = new float[buffer.Length];
                //Array.Copy(buffer, tempFloat, buffer.Length);
                return samplesRead;
            }

            public List<int> FindChangePoints(float[] audioData, float threshold)
            {
                List<int> changePoints = new List<int>();

                for (int i = 1; i < audioData.Length; i++)
                {
                    // 計算相鄰樣本的差異
                    float difference = Math.Abs(audioData[i] - audioData[i - 1]);

                    // 如果差異超過閾值，則視為變化點
                    if (difference > threshold)
                    {
                        //Console.WriteLine(difference);
                        changePoints.Add(i);
                    }
                }

                return changePoints;
            }

            public float CalculateCorrelation(float[] segment, float[] audioData)
            {
                int segmentLength = segment.Length;
                int audioDataLength = audioData.Length;

                // 确保两个信号的长度相同
                if (segmentLength != audioDataLength)
                {
                    throw new ArgumentException("信号长度不相等");
                }

                // 计算两个信号的平均值
                float segmentMean = segment.Average();
                float audioDataMean = audioData.Average();

                float numerator = 0.0f; // 相关性的分子部分
                float denominatorSegment = 0.0f; // segment 的方差
                float denominatorAudioData = 0.0f; // audioData 的方差

                for (int i = 0; i < segmentLength; i++)
                {
                    float segmentDeviation = segment[i] - segmentMean;
                    float audioDataDeviation = audioData[i] - audioDataMean;

                    numerator += segmentDeviation * audioDataDeviation;
                    denominatorSegment += segmentDeviation * segmentDeviation;
                    denominatorAudioData += audioDataDeviation * audioDataDeviation;
                }

                // 计算相关性系数
                float correlation = numerator / (float)Math.Sqrt(denominatorSegment * denominatorAudioData);

                return correlation;
            }


            public float[] MovingAverage(float[] data, int windowSize)
            {
                if (data.Length < windowSize)
                {
                    throw new ArgumentException("Window size must be less than or equal to the data length.");
                }

                float[] smoothedData = new float[data.Length];

                for (int i = 0; i < data.Length; i++)
                {
                    // 計算窗口內數值的平均值
                    int start = Math.Max(0, i - windowSize + 1);
                    int end = Math.Min(data.Length - 1, i + 1);
                    float sum = 0;

                    for (int j = start; j <= end; j++)
                    {
                        sum += data[j];
                    }

                    smoothedData[i] = sum / (end - start + 1);
                }

                return smoothedData;
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
