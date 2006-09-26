using System;
using System.Runtime.Serialization;

namespace Splicer
{
    [Serializable]
    public class SplicerException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SplicerException()
        {
        }

        public SplicerException(string message) : base(message)
        {
        }

        public SplicerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SplicerException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}