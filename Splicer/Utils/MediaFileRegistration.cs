using System;
using System.Collections.Generic;

namespace Splicer.Utils
{
    public class MediaFileRegistration : IDisposable
    {
        private List<RegsiteredMediaExtension> _registrations = new List<RegsiteredMediaExtension>();

        public void Add(RegsiteredMediaExtension rme)
        {
            _registrations.Add(rme);
        }

        public void Dispose()
        {
            if (_registrations != null)
            {
                foreach (RegsiteredMediaExtension registration in _registrations)
                {
                    registration.Dispose();
                }

                _registrations = null;
            }
        }
    }
}