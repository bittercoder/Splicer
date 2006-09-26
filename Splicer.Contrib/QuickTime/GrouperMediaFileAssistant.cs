using System;
using System.IO;
using Splicer.Timeline;

namespace Splicer.Contrib.QuickTime
{
    public class GrouperMediaFileAssistant : IMediaFileAssistant
    {
        public bool WillAssist(MediaFile file)
        {
            string extension = Path.GetExtension(file.FileName);
            if (extension.StartsWith(".")) extension = extension.Substring(1);

            if (GrouperQuickTimeFileRegistration.IsQTAudioExtension(extension) ||
                GrouperQuickTimeFileRegistration.IsQTVideoExtension(extension))
            {
                return true;
            }

            return false;
        }

        public IDisposable Assist(MediaFile file)
        {
            return new GrouperQuickTimeFileRegistration();
        }
    }
}