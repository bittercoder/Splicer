// Copyright 2006-2008 Splicer Project - http://www.codeplex.com/splicer/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Splicer.Renderer
{
    /// <summary>
    /// Logs progress to the console
    /// </summary>
    public class ConsoleProgressParticipant : AbstractProgressParticipant
    {
        private readonly int _framesPerLog;

        public ConsoleProgressParticipant(int framesPerLog)
        {
            _framesPerLog = framesPerLog;
        }

        public ConsoleProgressParticipant()
            : this(15)
        {
        }

        protected override void HandleProgress(double sampleTime, int bufferLength)
        {
            if ((FrameCount%_framesPerLog) == 0)
            {
                Console.WriteLine("Bytes: {0}, MediaTime: {1}, Elapsed: {2}", TotalBytes, LastSampleTime,
                                  DateTime.Now.Subtract(StartTime));
            }
        }
    }
}