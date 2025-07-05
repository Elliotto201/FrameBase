using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MediaFoundation;
using MediaFoundation.Net;
using MediaFoundation.ReadWrite;

namespace FrameBase
{
    internal static class MediaLoader
    {
        public static void Initialize()
        {
            
        }

        public static (List<VideoFrame> Frames, float FrameRate) GetFramesFromPathMp4(string path)
        {
            var allFrames = new List<VideoFrame>();
            float frameRate = 0f;

            MFExtern.MFStartup(0x20070, MFStartup.Full);

            IMFSourceReader reader;
            MFExtern.MFCreateSourceReaderFromURL(path, null, out reader);

            // Disable all streams initially
            reader.SetStreamSelection((int)MF_SOURCE_READER.MF_SOURCE_READER_ALL_STREAMS, false);

            const int MaxStreams = 64;

            for (int i = 0; i < MaxStreams; i++)
            {
                IMFMediaType nativeType = null;

                int hr = reader.GetNativeMediaType(i, 0, out nativeType);
                if (hr != 0 || nativeType == null)
                    continue;

                Guid majorType;
                nativeType.GetGUID(MFConstants.MF_MT_MAJOR_TYPE, out majorType);

                if (majorType == MFMediaType.Video)
                {
                    // Enable stream
                    reader.SetStreamSelection(i, true);

                    // Set output type to RGB32
                    IMFMediaType outputType;
                    MFExtern.MFCreateMediaType(out outputType);
                    outputType.SetGUID(MFConstants.MF_MT_MAJOR_TYPE, MFMediaType.Video);
                    outputType.SetGUID(MFConstants.MF_MT_SUBTYPE, MFConstants.MFVideoFormat_RGB32);
                    reader.SetCurrentMediaType(i, IntPtr.Zero, outputType);

                    // Get dimensions
                    int width, height;
                    MFHelper.GetFrameSize(outputType, MFConstants.MF_MT_FRAME_SIZE, out width, out height);

                    // Get framerate from nativeType
                    long frameRateValue = 0;
                    int getFpsResult = nativeType.GetUINT64(MFConstants.MF_MT_FRAME_RATE, out frameRateValue);
                    if (getFpsResult == 0 && frameRateValue != 0)
                    {
                        int numerator = (int)(frameRateValue >> 32);
                        int denominator = (int)(frameRateValue & 0xFFFFFFFF);
                        if (denominator != 0)
                        {
                            frameRate = (float)numerator / denominator;
                        }
                    }

                    while (true)
                    {
                        IMFSample sample;
                        int streamIndex;
                        int flags;
                        long timestamp;

                        reader.ReadSample(i, 0, out streamIndex, out flags, out timestamp, out sample);

                        if ((flags & (int)MF_SOURCE_READER_FLAG.EndOfStream) != 0)
                            break;

                        if (sample == null)
                            continue;

                        IMFMediaBuffer buffer;
                        sample.ConvertToContiguousBuffer(out buffer);

                        buffer.Lock(out IntPtr pBuffer, out int _, out int currentLength);

                        byte[] frameData = new byte[currentLength];
                        Marshal.Copy(pBuffer, frameData, 0, currentLength);

                        buffer.Unlock();
                        Marshal.ReleaseComObject(buffer);
                        Marshal.ReleaseComObject(sample);

                        allFrames.Add(new VideoFrame
                        {
                            Pixels = frameData,
                            Width = width,
                            Height = height,
                            Timestamp = timestamp,
                            StreamIndex = i
                        });
                    }

                    Marshal.ReleaseComObject(outputType);
                    Marshal.ReleaseComObject(nativeType);

                    break;  // exit after first video stream
                }

                if (nativeType != null)
                    Marshal.ReleaseComObject(nativeType);
            }

            Marshal.ReleaseComObject(reader);
            MFExtern.MFShutdown();

            return (allFrames, frameRate);
        }
    }

    public static class MFConstants
    {
        public static readonly Guid MF_MT_MAJOR_TYPE = new Guid("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
        public static readonly Guid MF_MT_SUBTYPE = new Guid("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
        public static readonly Guid MFMediaType_Video = new Guid("73646976-0000-0010-8000-00AA00389B71");
        public static readonly Guid MFVideoFormat_RGB32 = new Guid("00000016-0000-0010-8000-00AA00389B71");
        public static readonly Guid MF_MT_FRAME_SIZE = new Guid("1652c33d-d6b2-4012-b834-72030849a37d");
        public static readonly Guid MF_MT_FRAME_RATE = new Guid("c459a2e8-3d2c-4e44-b132-fee5156c7bb0");
    }

    public enum MF_SOURCE_READER
    {
        MF_SOURCE_READER_FIRST_VIDEO_STREAM = unchecked((int)0xFFFFFFFC),
        MF_SOURCE_READER_ALL_STREAMS = unchecked((int)0xFFFFFFFE)
    }

    [Flags]
    public enum MF_SOURCE_READER_FLAG
    {
        None = 0,
        Error = 0x00000001,
        EndOfStream = 0x00000002,
        NewStream = 0x00000004,
        NativeMediaTypeChanged = 0x00000010,
        CurrentMediaTypeChanged = 0x00000020,
        StreamTick = 0x00000100
    }

    public static class MFHelper
    {
        public static void GetFrameSize(IMFAttributes attr, Guid key, out int width, out int height)
        {
            long packed;
            attr.GetUINT64(key, out packed);
            width = (int)(packed >> 32);
            height = (int)(packed & 0xFFFFFFFF);
        }
    }
}