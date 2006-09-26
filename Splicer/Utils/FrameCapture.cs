using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;
using DirectShowLib.DES;

namespace Splicer.Utils
{
    /// <summary> Summary description for MainForm. </summary>
    public class FrameCapture : ISampleGrabberCB, IDisposable
    {
        #region Member variables

        /// <summary> graph builder interface. </summary>
        private IFilterGraph2 m_graphBuilder = null;

        private IMediaControl m_mediaCtrl = null;
        private IMediaEvent m_MediaEvent = null;
        private IMediaSeeking m_MediaSeeking = null;

        /// <summary> Dimensions of the image, calculated once in constructor. </summary>
        private int m_videoWidth;

        private int m_videoHeight;
        private int m_stride;
        public int m_Count = 0;
        public int m_Blacks = 0;
        public Stack<double> m_thresholds = new Stack<double>();

#if DEBUG
        // Allow you to "Connect to remote graph" from GraphEdit
        private DsROTEntry m_rot = null;
#endif

        #endregion

        #region API

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source,
                                              [MarshalAs(UnmanagedType.U4)] uint Length);

        #endregion

        /// <summary>
        /// Return the length of the media file in seconds
        /// </summary>
        private double GetLength(string fileName)
        {
            int hr;
            double d;

            IMediaDet imd = (IMediaDet) new MediaDet();

            // Set the name
            hr = imd.put_Filename(fileName);
            DESError.ThrowExceptionForHR(hr);

            // Read from stream zero
            hr = imd.put_CurrentStream(0);
            DESError.ThrowExceptionForHR(hr);

            // Get the length in seconds
            hr = imd.get_StreamLength(out d);
            DESError.ThrowExceptionForHR(hr);

            Marshal.ReleaseComObject(imd);

            return d;
        }

        /// <summary> File name to scan</summary>
        public FrameCapture(string FileName)
        {
            try
            {
                double length = GetLength(FileName);

                m_thresholds.Push((0.8)*length);
                m_thresholds.Push((0.5)*length);
                m_thresholds.Push((0.2)*length);

                // Set up the capture graph
                SetupGraph(FileName);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary> release everything. </summary>
        public void Dispose()
        {
            CloseInterfaces();
        }

        // Destructor
        ~FrameCapture()
        {
            CloseInterfaces();
        }


        /// <summary> capture the next image </summary>
        public void Start()
        {
            int hr =
                m_MediaSeeking.SetPositions(10, AMSeekingSeekingFlags.SeekToKeyFrame, 1,
                                            AMSeekingSeekingFlags.IncrementalPositioning);
            DsError.ThrowExceptionForHR(hr);

            hr = m_mediaCtrl.Run();
            DsError.ThrowExceptionForHR(hr);
        }


        public void WaitUntilDone()
        {
            int hr;
            EventCode evCode;
            const int E_Abort = unchecked((int) 0x80004004);

            do
            {
                Application.DoEvents();
                hr = m_MediaEvent.WaitForCompletion(100, out evCode);
            } while (hr == E_Abort);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary> build the capture graph for grabber. </summary>
        private void SetupGraph(string FileName)
        {
            int hr;

            ISampleGrabber sampGrabber = null;
            IBaseFilter baseGrabFlt = null;
            IBaseFilter capFilter = null;
            IBaseFilter nullrenderer = null;

            // Get the graphbuilder object
            m_graphBuilder = new FilterGraph() as IFilterGraph2;
            m_mediaCtrl = m_graphBuilder as IMediaControl;
            m_MediaEvent = m_graphBuilder as IMediaEvent;
            m_MediaSeeking = m_graphBuilder as IMediaSeeking;

            IMediaFilter mediaFilt = m_graphBuilder as IMediaFilter;

            try
            {
#if DEBUG
                m_rot = new DsROTEntry(m_graphBuilder);
#endif

                // Add the video source
                hr = m_graphBuilder.AddSourceFilter(FileName, "Ds.NET FileFilter", out capFilter);
                DsError.ThrowExceptionForHR(hr);

                // Get the SampleGrabber interface
                sampGrabber = new SampleGrabber() as ISampleGrabber;
                baseGrabFlt = sampGrabber as IBaseFilter;

                ConfigureSampleGrabber(sampGrabber);

                // Add the frame grabber to the graph
                hr = m_graphBuilder.AddFilter(baseGrabFlt, "Ds.NET Grabber");
                DsError.ThrowExceptionForHR(hr);

                // ---------------------------------
                // Connect the file filter to the sample grabber

                // Hopefully this will be the video pin, we could check by reading it's mediatype
                IPin iPinOut = DsFindPin.ByDirection(capFilter, PinDirection.Output, 0);

                // Get the input pin from the sample grabber
                IPin iPinIn = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Input, 0);

                hr = m_graphBuilder.Connect(iPinOut, iPinIn);
                DsError.ThrowExceptionForHR(hr);

                // Add the null renderer to the graph
                nullrenderer = new NullRenderer() as IBaseFilter;
                hr = m_graphBuilder.AddFilter(nullrenderer, "Null renderer");
                DsError.ThrowExceptionForHR(hr);

                // ---------------------------------
                // Connect the sample grabber to the null renderer

                iPinOut = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Output, 0);
                iPinIn = DsFindPin.ByDirection(nullrenderer, PinDirection.Input, 0);

                hr = m_graphBuilder.Connect(iPinOut, iPinIn);
                DsError.ThrowExceptionForHR(hr);

                // Turn off the clock.  This causes the frames to be sent
                // thru the graph as fast as possible
                hr = mediaFilt.SetSyncSource(null);
                DsError.ThrowExceptionForHR(hr);

                // Read and cache the image sizes
                SaveSizeInfo(sampGrabber);
            }
            finally
            {
                if (capFilter != null)
                {
                    Marshal.ReleaseComObject(capFilter);
                    capFilter = null;
                }
                if (sampGrabber != null)
                {
                    Marshal.ReleaseComObject(sampGrabber);
                    sampGrabber = null;
                }
                if (nullrenderer != null)
                {
                    Marshal.ReleaseComObject(nullrenderer);
                    nullrenderer = null;
                }
            }
        }

        /// <summary> Read and store the properties </summary>
        private void SaveSizeInfo(ISampleGrabber sampGrabber)
        {
            int hr;

            // Get the media type from the SampleGrabber
            AMMediaType media = new AMMediaType();
            hr = sampGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            // Grab the size info
            VideoInfoHeader videoInfoHeader =
                (VideoInfoHeader) Marshal.PtrToStructure(media.formatPtr, typeof (VideoInfoHeader));
            m_videoWidth = videoInfoHeader.BmiHeader.Width;
            m_videoHeight = videoInfoHeader.BmiHeader.Height;
            m_stride = m_videoWidth*(videoInfoHeader.BmiHeader.BitCount/8);

            DsUtils.FreeAMMediaType(media);
            media = null;
        }

        /// <summary> Set the options on the sample grabber </summary>
        private void ConfigureSampleGrabber(ISampleGrabber sampGrabber)
        {
            AMMediaType media;
            int hr;

            // Set the media type to Video/RBG24
            media = new AMMediaType();
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.RGB24;
            media.formatType = FormatType.VideoInfo;
            hr = sampGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
            media = null;

            // Choose to call BufferCB instead of SampleCB
            hr = sampGrabber.SetCallback(this, 1);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary> Shut down capture </summary>
        private void CloseInterfaces()
        {
            int hr;

            try
            {
                if (m_mediaCtrl != null)
                {
                    // Stop the graph
                    hr = m_mediaCtrl.Stop();
                    m_mediaCtrl = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

#if DEBUG
            if (m_rot != null)
            {
                m_rot.Dispose();
            }
#endif

            if (m_graphBuilder != null)
            {
                Marshal.ReleaseComObject(m_graphBuilder);
                m_graphBuilder = null;
            }
            GC.Collect();
        }

        /// <summary> sample callback, NOT USED. </summary>
        int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
        {
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        /// <summary> buffer callback, COULD BE FROM FOREIGN THREAD. </summary>        
        int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            if ((m_thresholds.Count > 0) && (SampleTime > m_thresholds.Peek()))
            {
                m_thresholds.Pop();

                using (Bitmap bitmap = new Bitmap(m_videoWidth, m_videoHeight, PixelFormat.Format24bppRgb))
                {
                    Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                    BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                    CopyMemory(data.Scan0, pBuffer, (uint) BufferLen);

                    bitmap.UnlockBits(data);

                    bitmap.Save(string.Format("output{0}.jpg", m_Count));
                }
                m_Count++;
            }

            return 0;
        }
    }
}