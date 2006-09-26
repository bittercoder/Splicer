using System;

namespace Splicer.Renderer
{
    /// <summary>
    /// Logs progress to the console
    /// </summary>
    public class ConsoleProgressCallback : AbstractProgressCallback
    {
        private int _framesPerLog;

        public ConsoleProgressCallback(int framesPerLog)
        {
            _framesPerLog = framesPerLog;
        }

        public ConsoleProgressCallback()
            : this(15)
        {
        }

        protected override void HandleProgress(string filename, double sampleTime, IntPtr buffer, int bufferLength)
        {
            if ((FrameCount%_framesPerLog) == 0)
            {
                Console.WriteLine("Bytes: {0}, MediaTime: {1}, Elapsed: {2}", TotalBytes, LastSampleTime,
                                  DateTime.Now.Subtract(StartTime));
            }
        }
    }
}