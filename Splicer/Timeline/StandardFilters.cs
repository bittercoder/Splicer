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
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Properties;
using Splicer.Utilities;
using Splicer.Utilities.Audio;
using Splicer.WindowsMedia;

namespace Splicer.Timeline
{
    public static class StandardFilters
    {
        /// <summary>
        /// This is the identified for the WavDest filter (a sample included with the Platform SDK for direct show)
        /// </summary>
        public static readonly Guid WavDestinationFilterId = new Guid("3C78B8E2-6C4D-11D1-ADE2-0000F8754B99");

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter RenderFileDestination(DisposalCleanup dc, IGraphBuilder graph, string outputFile)
        {
            if (dc == null) throw new ArgumentNullException("dc");
            if (graph == null) throw new ArgumentNullException("graph");
            if (string.IsNullOrEmpty(outputFile)) throw new ArgumentNullException("outputFile");

            int hr = 0;

            var fileFilter = (IBaseFilter) new FileWriter();

            hr = ((IFileSinkFilter) fileFilter).SetFileName(outputFile, null);
            DsError.ThrowExceptionForHR(hr);

            hr = graph.AddFilter(fileFilter, Resources.DefaultFileDestinationName);
            DsError.ThrowExceptionForHR(hr);

            dc.Add(fileFilter);

            return fileFilter;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter RenderWavDestination(DisposalCleanup dc, IGraphBuilder graph)
        {
            if (dc == null) throw new ArgumentNullException("dc");
            if (graph == null) throw new ArgumentNullException("graph");

            IBaseFilter wavDest =
                FilterGraphTools.AddFilterFromClsid(graph, WavDestinationFilterId, Resources.DefaultWavDestinationName);
            dc.Add(wavDest);

            return wavDest;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter RenderAsfWriterWithProfile(DisposalCleanup dc, IGraphBuilder graph, string profileData,
                                                             string outputFile)
        {
            if (dc == null) throw new ArgumentNullException("dc");
            if (graph == null) throw new ArgumentNullException("graph");
            if (string.IsNullOrEmpty(profileData)) throw new ArgumentNullException("profileData");
            if (string.IsNullOrEmpty(outputFile)) throw new ArgumentNullException("outputFile");

            int hr = 0;

            var asfWriterFilter = (IBaseFilter) new WMAsfWriter();
            dc.Add(asfWriterFilter);
            hr = graph.AddFilter(asfWriterFilter, Resources.DefaultAsfWriterName);
            DsError.ThrowExceptionForHR(hr);

            // Create an appropriate IWMProfile from the data
            IWMProfileManager profileManager = ProfileManager.CreateInstance();
            dc.Add(profileManager);

            IntPtr wmProfile = profileManager.LoadProfileByData(profileData);
            dc.Add(wmProfile);

            // Set the profile on the writer
            var configWriter = (IConfigAsfWriter2) asfWriterFilter;
            configWriter.ConfigureFilterUsingProfile(wmProfile);

            hr = ((IFileSinkFilter) asfWriterFilter).SetFileName(outputFile, null);
            DsError.ThrowExceptionForHR(hr);

            return asfWriterFilter;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter CreateAudioCompressor(DisposalCleanup dc, IGraphBuilder graph, IPin outPin,
                                                        AudioFormat settings)
        {
            if (dc == null) throw new ArgumentNullException("dc");
            if (graph == null) throw new ArgumentNullException("graph");
            if (outPin == null) throw new ArgumentNullException("outPin");
            if (settings == null) throw new ArgumentNullException("settings");

            int hr = 0;

            using (AudioCompressor compressor = AudioCompressorFactory.Create(settings))
            {
                IBaseFilter compressorFilter = compressor.Filter;
                dc.Add(compressorFilter);

                hr = graph.AddFilter(compressorFilter, settings.AudioCompressor);
                DsError.ThrowExceptionForHR(hr);

                FilterGraphTools.ConnectFilters(graph, outPin, compressorFilter, true);

                // set the media type on the output pin of the compressor
                if (compressor.MediaType != null)
                {
                    FilterGraphTools.SetFilterFormat(compressor.MediaType, compressorFilter);
                }

                return compressorFilter;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter RenderAviDestination(DisposalCleanup dc, ICaptureGraphBuilder2 graphBuilder,
                                                       string outputFile)
        {
            if (dc == null) throw new ArgumentNullException("dc");
            if (graphBuilder == null) throw new ArgumentNullException("graphBuilder");
            if (string.IsNullOrEmpty(outputFile)) throw new ArgumentNullException("outputFile");

            int hr = 0;

            // Create the file writer
            IBaseFilter multiplexer;
            IFileSinkFilter filter = null;
            try
            {
                hr = graphBuilder.SetOutputFileName(MediaSubType.Avi, outputFile, out multiplexer, out filter);
                dc.Add(multiplexer);
                DESError.ThrowExceptionForHR(hr);
            }
            finally
            {
                if (filter != null) Marshal.ReleaseComObject(filter);
            }

            return multiplexer;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter RenderNull(DisposalCleanup dc, IGraphBuilder graph)
        {
            if (dc == null) throw new ArgumentNullException("dc");
            if (graph == null) throw new ArgumentNullException("graph");

            var filter = (IBaseFilter) new NullRenderer();
            dc.Add(filter);

            graph.AddFilter(filter, Resources.DefaultNullRendererName);

            return filter;
        }
    }
}