// Copyright 2004-2006 Castle Project - http://www.castleproject.org/
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
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Utils;
using Splicer.Utils.Audio;
using Splicer.WindowsMedia;

namespace Splicer.Timeline
{
    public static class StandardFilters
    {
        public static readonly Guid WavDestFilterId = new Guid("3C78B8E2-6C4D-11D1-ADE2-0000F8754B99");

        public static IBaseFilter RenderFileDestination(DisposalCleanup dc, IGraphBuilder graph, string outputFile)
        {
            int hr = 0;

            IBaseFilter fileFilter = (IBaseFilter) new FileWriter();

            hr = ((IFileSinkFilter) fileFilter).SetFileName(outputFile, null);
            DsError.ThrowExceptionForHR(hr);

            hr = graph.AddFilter(fileFilter, "Output File");
            DsError.ThrowExceptionForHR(hr);

            dc.Add(fileFilter);

            return fileFilter;
        }

        public static IBaseFilter RenderWavDest(DisposalCleanup dc, IGraphBuilder graph)
        {
            IBaseFilter wavDest = FilterGraphTools.AddFilterFromClsid(graph, WavDestFilterId, "Wav DEST");
            dc.Add(wavDest);

            return wavDest;
        }

        public static IBaseFilter RenderAsfWriterWithProfile(DisposalCleanup dc, IGraphBuilder graph, string profileData,
                                                             string outputFile)
        {
            int hr = 0;

            IBaseFilter asfWriterFilter = (IBaseFilter) new WMAsfWriter();
            dc.Add(asfWriterFilter);
            hr = graph.AddFilter(asfWriterFilter, "ASF Writer");
            DsError.ThrowExceptionForHR(hr);

            // Create an appropriate IWMProfile from the data
            IWMProfileManager profileManager = ProfileManager.CreateInstance();
            dc.Add(profileManager);

            IntPtr wmProfile = profileManager.LoadProfileByData(profileData);
            dc.Add(wmProfile);

            // Set the profile on the writer
            IConfigAsfWriter2 configWriter = (IConfigAsfWriter2) asfWriterFilter;
            configWriter.ConfigureFilterUsingProfile(wmProfile);

            hr = ((IFileSinkFilter) asfWriterFilter).SetFileName(outputFile, null);
            DsError.ThrowExceptionForHR(hr);

            return asfWriterFilter;
        }

        public static IBaseFilter CreateAudioCompressor(DisposalCleanup dc, IGraphBuilder graph, IPin outPin,
                                                        AudioFormat settings)
        {
            int hr = 0;

            AudioCompressor compressor = AudioCompressorFactory.Create(settings);
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

        public static IBaseFilter RenderAviDest(DisposalCleanup dc, ICaptureGraphBuilder2 icgb, string outputFile)
        {
            int hr = 0;

            // Create the file writer
            IBaseFilter pMux;
            IFileSinkFilter pFilter = null;
            try
            {
                hr = icgb.SetOutputFileName(MediaSubType.Avi, outputFile, out pMux, out pFilter);
                dc.Add(pMux);
                DESError.ThrowExceptionForHR(hr);
            }
            finally
            {
                if (pFilter != null) Marshal.ReleaseComObject(pFilter);
            }

            return pMux;
        }

        public static IBaseFilter RenderNull(DisposalCleanup dc, IGraphBuilder graph)
        {
            IBaseFilter filter = (IBaseFilter) new NullRenderer();
            dc.Add(filter);

            graph.AddFilter(filter, "Null Renderer");

            return filter;
        }
    }
}