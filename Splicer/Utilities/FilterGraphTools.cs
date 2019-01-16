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

// this class is largely derived from a public domain class that is released
// as part of the DirectShow.Net samples.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Properties;

namespace Splicer.Utilities
{
    /// <summary>
    /// A collection of methods to do common DirectShow tasks.
    /// </summary>
    public sealed class FilterGraphTools
    {
        public static readonly Guid AudioCompressorCategoryClassId = new Guid("33d9a761-90c8-11d0-bd43-00a0c911ce86");
        public static readonly Guid IBaseFilterInterfaceId = new Guid("56a86895-0ad4-11ce-b03a-0020af0ba770");
        public static readonly Guid IPropertyBagInterfaceId = new Guid("55272A00-42CB-11CE-8135-00AA004BB851");

        private FilterGraphTools()
        {
        }

        /// <summary>
        /// Add a filter to a DirectShow Graph using its CLSID
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="clsid">a valid CLSID. This object must implement IBaseFilter</param>
        /// <param name="name">the name used in the graph (may be null)</param>
        /// <returns>an instance of the filter if the method successfully created it, null if not</returns>
        /// <remarks>
        /// You can use <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> to check is the CLSID is valid before calling this method
        /// </remarks>
        /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
        /// <code>
        /// Guid nvidiaVideoDecClsid = new Guid("71E4616A-DB5E-452B-8CA5-71D9CC7805E9");
        /// 
        /// if (FilterGraphTools.IsThisComObjectInstalled(nvidiaVideoDecClsid))
        /// {
        ///   filter = FilterGraphTools.AddFilterFromClsid(graphBuilder, nvidiaVideoDecClsid, "NVIDIA Video Decoder");
        /// }
        /// else
        /// {
        ///   // use another filter...
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IsThisComObjectInstalled"/>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter AddFilterFromClsid(IGraphBuilder graphBuilder, Guid clsid, string name)
        {
            int hr = 0;
            IBaseFilter filter = null;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                Type type = Type.GetTypeFromCLSID(clsid);
                filter = (IBaseFilter) Activator.CreateInstance(type);

                hr = graphBuilder.AddFilter(filter, name);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException)
            {
                if (filter != null)
                {
                    Marshal.ReleaseComObject(filter);
                    filter = null;
                }
            }

            return filter;
        }

        /// <summary>
        /// Add a filter to a DirectShow Graph using its name
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="deviceCategory">the filter category (see DirectShowLib.FilterCategory)</param>
        /// <param name="friendlyName">the filter name (case-sensitive)</param>
        /// <returns>an instance of the filter if the method successfully created it, null if not</returns>
        /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
        /// <code>
        /// filter = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.LegacyAmFilterCategory, "NVIDIA Video Decoder");
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>
        public static IBaseFilter AddFilterByName(IGraphBuilder graphBuilder, Guid deviceCategory, string friendlyName)
        {
            int hr = 0;
            IBaseFilter filter = null;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            DsDevice[] devices = DsDevice.GetDevicesOfCat(deviceCategory);

            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].Name.Equals(friendlyName))
                    continue;

                hr =
                    (graphBuilder as IFilterGraph2).AddSourceFilterForMoniker(devices[i].Mon, null, friendlyName,
                                                                              out filter);
                DsError.ThrowExceptionForHR(hr);

                break;
            }

            return filter;
        }

        /// <summary>
        /// Add a filter to a DirectShow Graph using its Moniker's device path
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="devicePath">a moniker path</param>
        /// <param name="name">the name to use for the filter in the graph</param>
        /// <returns>an instance of the filter if the method successfully creates it, null if not</returns>
        /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
        /// <code>
        /// string devicePath = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{71E4616A-DB5E-452B-8CA5-71D9CC7805E9}";
        /// filter = FilterGraphTools.AddFilterByDevicePath(graphBuilder, devicePath, "NVIDIA Video Decoder");
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter AddFilterByDevicePath(IGraphBuilder graphBuilder, string devicePath, string name)
        {
            int hr = 0;
            IBaseFilter filter = null;

            IBindCtx bindCtx = null;
            IMoniker moniker = null;

            int eaten;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                hr = NativeMethods.CreateBindCtx(0, out bindCtx);
                Marshal.ThrowExceptionForHR(hr);

                hr = NativeMethods.MkParseDisplayName(bindCtx, devicePath, out eaten, out moniker);
                Marshal.ThrowExceptionForHR(hr);

                hr = (graphBuilder as IFilterGraph2).AddSourceFilterForMoniker(moniker, bindCtx, name, out filter);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException)
            {
                // An error occur. Just returning null...
            }
            finally
            {
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
                if (moniker != null) Marshal.ReleaseComObject(moniker);
            }

            return filter;
        }

        /// <summary>
        /// Find a filter in a DirectShow Graph using its name
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="filterName">the filter name to find (case-sensitive)</param>
        /// <returns>an instance of the filter if found, null if not</returns>
        /// <seealso cref="FindFilterByClsid"/>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter FindFilterByName(IGraphBuilder graphBuilder, string filterName)
        {
            int hr = 0;
            IBaseFilter filter = null;
            IEnumFilters enumFilters = null;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            hr = graphBuilder.EnumFilters(out enumFilters);
            if (hr == 0)
            {
                var filters = new IBaseFilter[1];
                IntPtr fetched = IntPtr.Zero;

                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    FilterInfo filterInfo;

                    hr = filters[0].QueryFilterInfo(out filterInfo);
                    if (hr == 0)
                    {
                        if (filterInfo.pGraph != null)
                            Marshal.ReleaseComObject(filterInfo.pGraph);

                        if (filterInfo.achName.Equals(filterName))
                        {
                            filter = filters[0];
                            break;
                        }
                    }

                    Marshal.ReleaseComObject(filters[0]);
                }
                Marshal.ReleaseComObject(enumFilters);
            }

            return filter;
        }

        /// <summary>
        /// Find a filter in a DirectShow Graph using its CLSID
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="filterClsid">the CLSID to find</param>
        /// <returns>an instance of the filter if found, null if not</returns>
        /// <seealso cref="FindFilterByName"/>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IBaseFilter FindFilterByClsid(IGraphBuilder graphBuilder, Guid filterClsid)
        {
            int hr = 0;
            IBaseFilter filter = null;
            IEnumFilters enumFilters = null;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            hr = graphBuilder.EnumFilters(out enumFilters);
            if (hr == 0)
            {
                var filters = new IBaseFilter[1];
                IntPtr fetched = IntPtr.Zero;

                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    Guid clsid;

                    hr = filters[0].GetClassID(out clsid);

                    if ((hr == 0) && (clsid == filterClsid))
                    {
                        filter = filters[0];
                        break;
                    }

                    Marshal.ReleaseComObject(filters[0]);
                }
                Marshal.ReleaseComObject(enumFilters);
            }

            return filter;
        }

        /// <summary>
        /// Render a filter's pin in a DirectShow Graph
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="source">the filter containing the pin to render</param>
        /// <param name="pinName">the pin name</param>
        /// <returns>true if rendering is a success, false if not</returns>
        /// <example>
        /// <code>
        /// hr = graphBuilder.AddSourceFilter(@"foo.avi", "Source Filter", out filter);
        /// DsError.ThrowExceptionForHR(hr);
        /// 
        /// if (!FilterGraphTools.RenderPin(graphBuilder, filter, "Output"))
        /// {
        ///   // Something went wrong...
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder or source is null</exception>
        /// <remarks>This method assumes that the filter is part of the given graph</remarks>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static bool RenderPin(IGraphBuilder graphBuilder, IBaseFilter source, string pinName)
        {
            int hr = 0;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (source == null)
                throw new ArgumentNullException("source");

            IPin pin = DsFindPin.ByName(source, pinName);

            if (pin != null)
            {
                hr = graphBuilder.Render(pin);
                Marshal.ReleaseComObject(pin);

                return (hr >= 0);
            }

            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void RenderOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter)
        {
            if (graphBuilder == null) throw new ArgumentNullException("graphBuilder");
            if (filter == null) throw new ArgumentNullException("filter");

            int hr = 0;

            if (filter == null)
                throw new ArgumentNullException("filter");

            IEnumPins enumPins;
            var pins = new IPin[1];
            IntPtr fetched = IntPtr.Zero;

            hr = filter.EnumPins(out enumPins);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                while (enumPins.Next(pins.Length, pins, fetched) == 0)
                {
                    try
                    {
                        PinDirection direction;

                        pins[0].QueryDirection(out direction);

                        if (direction == PinDirection.Output)
                        {
                            hr = graphBuilder.Render(pins[0]);
                            DsError.ThrowExceptionForHR(hr);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(pins[0]);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumPins);
            }
        }

        /// <summary>
        /// Disconnect all pins on a given filter
        /// </summary>
        /// <param name="filter">the filter on which to disconnect all the pins</param>
        /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occurred during the disconnection process</exception>
        /// <remarks>Both input and output pins are disconnected</remarks>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void DisconnectPins(IBaseFilter filter)
        {
            int hr = 0;

            if (filter == null)
                throw new ArgumentNullException("filter");

            IEnumPins enumPins;
            var pins = new IPin[1];
            IntPtr fetched = IntPtr.Zero;

            hr = filter.EnumPins(out enumPins);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                while (enumPins.Next(pins.Length, pins, fetched) == 0)
                {
                    try
                    {
                        hr = pins[0].Disconnect();
                        DsError.ThrowExceptionForHR(hr);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(pins[0]);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumPins);
            }
        }

        /// <summary>
        /// Disconnect pins of all the filters in a DirectShow Graph
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if the method can't enumerate its filters</exception>
        /// <remarks>This method doesn't throw an exception if an error occurs during pin disconnections</remarks>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void DisconnectAllPins(IGraphBuilder graphBuilder)
        {
            int hr = 0;
            IEnumFilters enumFilters;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            hr = graphBuilder.EnumFilters(out enumFilters);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                var filters = new IBaseFilter[1];
                IntPtr fetched = IntPtr.Zero;

                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    try
                    {
                        DisconnectPins(filters[0]);
                    }
                    catch (COMException)
                    {
                    }
                    Marshal.ReleaseComObject(filters[0]);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumFilters);
            }
        }

        /// <summary>
        /// Remove and release all filters from a DirectShow Graph
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if the method can't enumerate its filters</exception>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void RemoveAllFilters(IGraphBuilder graphBuilder)
        {
            int hr = 0;
            IEnumFilters enumFilters;
            var filtersArray = new ArrayList();

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            hr = graphBuilder.EnumFilters(out enumFilters);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                var filters = new IBaseFilter[1];
                IntPtr fetched = IntPtr.Zero;

                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    filtersArray.Add(filters[0]);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumFilters);
            }

            foreach (IBaseFilter filter in filtersArray)
            {
                hr = graphBuilder.RemoveFilter(filter);
                Marshal.ReleaseComObject(filter);
            }
        }

        /// <summary>
        /// Save a DirectShow Graph to a GRF file
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="fileName">the file to be saved</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur during the file creation</exception>
        /// <seealso cref="LoadGraphFile"/>
        /// <remarks>This method overwrites any existing file</remarks>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void SaveGraphFile(IGraphBuilder graphBuilder, string fileName)
        {
            int hr = 0;
            IStorage storage = null;
#if USING_NET11
            UCOMIStream stream = null;
#else
            IStream stream = null;
#endif

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                hr = NativeMethods.StgCreateDocfile(
                    fileName,
                    STGM.Create | STGM.Transacted | STGM.ReadWrite | STGM.ShareExclusive,
                    0,
                    out storage
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = storage.CreateStream(
                    @"ActiveMovieGraph",
                    STGM.Write | STGM.Create | STGM.ShareExclusive,
                    0,
                    0,
                    out stream
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = (graphBuilder as IPersistStream).Save(stream, true);
                Marshal.ThrowExceptionForHR(hr);

                hr = storage.Commit(STGC.Default);
                Marshal.ThrowExceptionForHR(hr);
            }
            finally
            {
                if (stream != null)
                    Marshal.ReleaseComObject(stream);
                if (storage != null)
                    Marshal.ReleaseComObject(storage);
            }
        }

        /// <summary>
        /// Load a DirectShow Graph from a file
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="fileName">the file to be loaded</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the given file is not a valid graph file</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur during loading</exception>
        /// <seealso cref="SaveGraphFile"/>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void LoadGraphFile(IGraphBuilder graphBuilder, string fileName)
        {
            int hr = 0;
            IStorage storage = null;
#if USING_NET11
			UCOMIStream stream = null;
#else
            IStream stream = null;
#endif

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                if (NativeMethods.StgIsStorageFile(fileName) != 0)

                    throw new ArgumentException(fileName);

                hr = NativeMethods.StgOpenStorage(
                    fileName,
                    null,
                    STGM.Transacted | STGM.Read | STGM.ShareDenyWrite,
                    IntPtr.Zero,
                    0,
                    out storage
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = storage.OpenStream(
                    @"ActiveMovieGraph",
                    IntPtr.Zero,
                    STGM.Read | STGM.ShareExclusive,
                    0,
                    out stream
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = (graphBuilder as IPersistStream).Load(stream);
                Marshal.ThrowExceptionForHR(hr);
            }
            finally
            {
                if (stream != null)
                    Marshal.ReleaseComObject(stream);
                if (storage != null)
                    Marshal.ReleaseComObject(storage);
            }
        }

        /// <summary>
        /// Check if a DirectShow filter can display Property Pages
        /// </summary>
        /// <param name="filter">A DirectShow Filter</param>
        /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
        /// <seealso cref="ShowFilterPropertyPage"/>
        /// <returns>true if the filter has Property Pages, false if not</returns>
        /// <remarks>
        /// This method is intended to be used with <see cref="ShowFilterPropertyPage">ShowFilterPropertyPage</see>
        /// </remarks>
        public static bool HasPropertyPages(IBaseFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            return ((filter as ISpecifyPropertyPages) != null);
        }

        /// <summary>
        /// Display Property Pages of a given DirectShow filter
        /// </summary>
        /// <param name="filter">A DirectShow Filter</param>
        /// <param name="parent">A hwnd handle of a window to contain the pages</param>
        /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
        /// <seealso cref="HasPropertyPages"/>
        /// <remarks>
        /// You can check if a filter supports Property Pages with the <see cref="HasPropertyPages">HasPropertyPages</see> method.<br/>
        /// <strong>Warning</strong> : This method is blocking. It only returns when the Property Pages are closed.
        /// </remarks>
        /// <example>This sample shows how to check if a filter supports Property Pages and displays them
        /// <code>
        /// if (FilterGraphTools.HasPropertyPages(myFilter))
        /// {
        ///   FilterGraphTools.ShowFilterPropertyPage(myFilter, myForm.Handle);
        /// }
        /// </code>
        /// </example>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void ShowFilterPropertyPage(IBaseFilter filter, IntPtr parent)
        {
            int hr = 0;
            FilterInfo filterInfo;
            DsCAUUID caGuid;
            object[] objs;

            if (filter == null)
                throw new ArgumentNullException("filter");

            if (HasPropertyPages(filter))
            {
                hr = filter.QueryFilterInfo(out filterInfo);
                DsError.ThrowExceptionForHR(hr);

                if (filterInfo.pGraph != null)
                    Marshal.ReleaseComObject(filterInfo.pGraph);

                hr = (filter as ISpecifyPropertyPages).GetPages(out caGuid);
                DsError.ThrowExceptionForHR(hr);

                try
                {
                    objs = new object[1];
                    objs[0] = filter;

                    NativeMethods.OleCreatePropertyFrame(
                        parent, 0, 0,
                        filterInfo.achName,
                        objs.Length, objs,
                        caGuid.cElems, caGuid.pElems,
                        0, 0,
                        IntPtr.Zero
                        );
                }
                finally
                {
                    Marshal.FreeCoTaskMem(caGuid.pElems);
                }
            }
        }

        /// <summary>
        /// Check if a COM Object is available
        /// </summary>
        /// <param name="clsid">The CLSID of this object</param>
        /// <example>This sample shows how to check if the MPEG-2 Demultiplexer filter is available
        /// <code>
        /// if (FilterGraphTools.IsThisComObjectInstalled(typeof(MPEG2Demultiplexer).GUID))
        /// {
        ///   // Use it...
        /// }
        /// </code>
        /// </example>
        /// <returns>true if the object is available, false if not</returns>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static bool IsThisComObjectInstalled(Guid clsid)
        {
            bool retval = false;

            try
            {
                Type type = Type.GetTypeFromCLSID(clsid);
                object o = Activator.CreateInstance(type);
                retval = true;
                Marshal.ReleaseComObject(o);
            }
            catch (COMException)
            {
            }

            return retval;
        }

        /// <summary>
        /// Check if the Video Mixing Renderer 9 Filter is available
        /// <seealso cref="IsThisComObjectInstalled"/>
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> internally
        /// </remarks>
        /// <returns>true if VMR9 is present, false if not</returns>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static bool IsVMR9Present()
        {
            return IsThisComObjectInstalled(typeof (VideoMixingRenderer9).GUID);
        }

        /// <summary>
        /// Check if the Video Mixing Renderer 7 Filter is available
        /// <seealso cref="IsThisComObjectInstalled"/>
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> internally
        /// </remarks>
        /// <returns>true if VMR7 is present, false if not</returns>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static bool IsVMR7Present()
        {
            return IsThisComObjectInstalled(typeof (VideoMixingRenderer).GUID);
        }

        /// <summary>
        /// Connects the first output pin of the upfilter to the first input pin of the downfilter.
        /// </summary>
        /// <param name="graphBuilder"></param>
        /// <param name="sourcePin"></param>
        /// <param name="downFilter"></param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void ConnectFilters(IGraphBuilder graphBuilder, IPin sourcePin, IBaseFilter downFilter,
                                          bool useIntelligentConnect)
        {
            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (sourcePin == null)
                throw new ArgumentNullException("sourcePin");

            if (downFilter == null)
                throw new ArgumentNullException("downFilter");

            IPin destPin;

            destPin = DsFindPin.ByDirection(downFilter, PinDirection.Input, 0);

            if (destPin == null)
                throw new ArgumentException("The downstream filter has no input pin");

            try
            {
                ConnectFilters(graphBuilder, sourcePin, destPin, useIntelligentConnect);
            }
            finally
            {
                Marshal.ReleaseComObject(destPin);
            }
        }

        /// <summary>
        /// Connects the first output pin of the upfilter to the first input pin of the downfilter.
        /// </summary>
        /// <param name="graphBuilder"></param>
        /// <param name="upFilter"></param>
        /// <param name="downFilter"></param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void ConnectFilters(IGraphBuilder graphBuilder, IBaseFilter upFilter, IBaseFilter downFilter,
                                          bool useIntelligentConnect)
        {
            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (upFilter == null)
                throw new ArgumentNullException("upFilter");

            if (downFilter == null)
                throw new ArgumentNullException("downFilter");

            IPin sourcePin, destPin;

            sourcePin = DsFindPin.ByDirection(upFilter, PinDirection.Output, 0);

            if (sourcePin == null)
                throw new ArgumentException("The source filter has no output pin");

            destPin = DsFindPin.ByDirection(downFilter, PinDirection.Input, 0);

            if (destPin == null)
                throw new ArgumentException("The downstream filter has no input pin");

            try
            {
                ConnectFilters(graphBuilder, sourcePin, destPin, useIntelligentConnect);
            }
            finally
            {
                Marshal.ReleaseComObject(sourcePin);
                Marshal.ReleaseComObject(destPin);
            }
        }

        /// <summary>
        /// Connect pins from two filters
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="upFilter">the upstream filter</param>
        /// <param name="sourcePinName">the upstream filter pin name</param>
        /// <param name="downFilter">the downstream filter</param>
        /// <param name="destinationPinName">the downstream filter pin name</param>
        /// <param name="useIntelligentConnect">indicate if the method should use DirectShow's Intelligent Connect</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder, upFilter or downFilter are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if pin names are not found in filters</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if pins can't connect</exception>
        /// <remarks>
        /// If useIntelligentConnect is true, this method can add missing filters between the two pins.<br/>
        /// If useIntelligentConnect is false, this method works only if the two media types are compatible.
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void ConnectFilters(IGraphBuilder graphBuilder, IBaseFilter upFilter, string sourcePinName,
                                          IBaseFilter downFilter, string destinationPinName, bool useIntelligentConnect)
        {
            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (upFilter == null)
                throw new ArgumentNullException("upFilter");

            if (downFilter == null)
                throw new ArgumentNullException("downFilter");

            IPin sourcePin, destPin;

            sourcePin = DsFindPin.ByName(upFilter, sourcePinName);
            if (sourcePin == null)
                throw new ArgumentException("The source filter has no pin called : " + sourcePinName, sourcePinName);

            destPin = DsFindPin.ByName(downFilter, destinationPinName);
            if (destPin == null)
                throw new ArgumentException("The downstream filter has no pin called : " + destinationPinName,
                                            destinationPinName);

            try
            {
                ConnectFilters(graphBuilder, sourcePin, destPin, useIntelligentConnect);
            }
            finally
            {
                Marshal.ReleaseComObject(sourcePin);
                Marshal.ReleaseComObject(destPin);
            }
        }

        /// <summary>
        /// Connect pins from two filters
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="sourcePin">the source (upstream / output) pin</param>
        /// <param name="destinationPin">the destination (downstream / input) pin</param>
        /// <param name="useIntelligentConnect">indicates if the method should use DirectShow's Intelligent Connect</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder, sourcePin or destinationPin are null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if pins can't connect</exception>
        /// <remarks>
        /// If useIntelligentConnect is true, this method can add missing filters between the two pins.<br/>
        /// If useIntelligentConnect is false, this method works only if the two media types are compatible.
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void ConnectFilters(IGraphBuilder graphBuilder, IPin sourcePin, IPin destinationPin,
                                          bool useIntelligentConnect)
        {
            int hr = 0;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (sourcePin == null)
                throw new ArgumentNullException("sourcePin");

            if (destinationPin == null)
                throw new ArgumentNullException("destinationPin");

            if (useIntelligentConnect)
            {
                hr = graphBuilder.Connect(sourcePin, destinationPin);
                DsError.ThrowExceptionForHR(hr);
            }
            else
            {
                hr = graphBuilder.ConnectDirect(sourcePin, destinationPin, null);
                DsError.ThrowExceptionForHR(hr);
            }
        }

        /// <summary>
        /// Determine whether a specified pin is audio or video
        /// </summary>
        /// <param name="pin">Pin to check</param>
        /// <returns>True if pin is video</returns>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static bool IsVideo(IPin pin)
        {
            if (pin == null) throw new ArgumentNullException("pin");

            int hr;
            bool bRet = false;
            var pmt = new AMMediaType[1];
            IEnumMediaTypes ppEnum;
            IntPtr i = IntPtr.Zero;

            // Walk the MediaTypes for the pin
            hr = pin.EnumMediaTypes(out ppEnum);
            DESError.ThrowExceptionForHR(hr);

            try
            {
                // Just read the first one
                hr = ppEnum.Next(1, pmt, i);
                DESError.ThrowExceptionForHR(hr);

                bRet = pmt[0].majorType == MediaType.Video;
            }
            finally
            {
                Marshal.ReleaseComObject(ppEnum);
            }
            DsUtils.FreeAMMediaType(pmt[0]);

            return bRet;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static IList<PinQueryInfo> EnumeratePins(IBaseFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            int hr = 0;

            var pinsInfo = new List<PinQueryInfo>();

            IEnumPins pinsEnum = null;
            try
            {
                hr = filter.EnumPins(out pinsEnum);
                DsError.ThrowExceptionForHR(hr);

                if (pinsEnum == null) throw new InvalidOperationException("pinsEnum is null");

                var pins = new IPin[1];

                while (true)
                {
                    try
                    {
                        int fetched = 0;

                        IntPtr pcFetched = Marshal.AllocCoTaskMem(4);

                        try
                        {
                            hr = pinsEnum.Next(pins.Length, pins, pcFetched);
                            DsError.ThrowExceptionForHR(hr);
                            fetched = Marshal.ReadInt32(pcFetched);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pcFetched);
                        }

                        if (fetched == 1)
                        {
                            // we have something
                            IPin pin = pins[0];

                            string queryId;
                            hr = pin.QueryId(out queryId);
                            DsError.ThrowExceptionForHR(hr);

                            PinInfo pinInfo;
                            hr = pin.QueryPinInfo(out pinInfo);
                            DsError.ThrowExceptionForHR(hr);

                            pinsInfo.Add(new PinQueryInfo(pinInfo.dir, pinInfo.name, queryId));
                        }
                        else
                        {
                            break;
                        }
                    }
                    finally
                    {
                        if (pins[0] != null) Marshal.ReleaseComObject(pins[0]);
                        pins[0] = null;
                    }
                }
            }
            finally
            {
                if (pinsEnum != null) Marshal.ReleaseComObject(pinsEnum);
            }

            return pinsInfo;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void SetFilterFormat(AMMediaType streamFormat, IBaseFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            int hr;

            IEnumPins pinsEnum = null;
            try
            {
                hr = filter.EnumPins(out pinsEnum);
                DsError.ThrowExceptionForHR(hr);

                if (pinsEnum == null) throw new SplicerException(Resources.ErrorPinsEnumeratorIsNull);

                var pins = new IPin[1];

                while (true)
                {
                    try
                    {
                        int fetched = 0;
                        IntPtr pcFetched = Marshal.AllocCoTaskMem(4);
                        try
                        {
                            hr = pinsEnum.Next(pins.Length, pins, pcFetched);
                            DsError.ThrowExceptionForHR(hr);
                            fetched = Marshal.ReadInt32(pcFetched);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pcFetched);
                        }

                        if (fetched == 1)
                        {
                            // we have something
                            IPin pin = pins[0];

                            string queryId;
                            hr = pin.QueryId(out queryId);
                            DsError.ThrowExceptionForHR(hr);

                            PinInfo pinInfo;
                            hr = pin.QueryPinInfo(out pinInfo);
                            DsError.ThrowExceptionForHR(hr);

                            if (pinInfo.dir != PinDirection.Output) continue;
                            var streamConfig = (IAMStreamConfig) pin;

                            hr = streamConfig.SetFormat(streamFormat);
                            DsError.ThrowExceptionForHR(hr);
                        }
                        else
                        {
                            break;
                        }
                    }
                    finally
                    {
                        if (pins[0] != null) Marshal.ReleaseComObject(pins[0]);
                        pins[0] = null;
                    }
                }
            }
            finally
            {
                if (pinsEnum != null) Marshal.ReleaseComObject(pinsEnum);
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public static IPin FindPinForMajorType(IBaseFilter filter, PinDirection direction, Guid majorType)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            int hr = 0;

            IEnumPins pinsEnum = null;
            try
            {
                hr = filter.EnumPins(out pinsEnum);
                DsError.ThrowExceptionForHR(hr);

                var pins = new IPin[1];

                int numberFetched = 1;
                while (numberFetched > 0)
                {
                    IntPtr pcFetched = Marshal.AllocCoTaskMem(4);
                    try
                    {
                        hr = pinsEnum.Next(1, pins, pcFetched);
                        DsError.ThrowExceptionForHR(hr);
                        numberFetched = Marshal.ReadInt32(pcFetched);
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pcFetched);
                    }

                    if (numberFetched > 0)
                    {
                        PinDirection currentPinDirection;
                        hr = pins[0].QueryDirection(out currentPinDirection);
                        DsError.ThrowExceptionForHR(hr);

                        if (currentPinDirection != direction) continue;

                        IEnumMediaTypes mediaTypesEnum = null;
                        try
                        {
                            var mediaTypes = new AMMediaType[1];
                            pins[0].EnumMediaTypes(out mediaTypesEnum);


                            int numberFetched2 = 1;

                            while (numberFetched2 > 0)
                            {
                                IntPtr fetched2 = IntPtr.Zero;
                                try
                                {
                                    hr = mediaTypesEnum.Next(1, mediaTypes, fetched2);
                                    DsError.ThrowExceptionForHR(hr);
                                    numberFetched2 = Marshal.ReadInt32(fetched2);
                                }
                                finally
                                {
                                    Marshal.FreeCoTaskMem(fetched2);
                                }

                                if (numberFetched2 > 0)
                                {
                                    if (mediaTypes[0].majorType == majorType)
                                    {
                                        // success, return the pin
                                        return pins[0];
                                    }
                                }

                                Marshal.ReleaseComObject(pins[0]);
                            }
                        }
                        finally
                        {
                            if (mediaTypesEnum != null) Marshal.ReleaseComObject(mediaTypesEnum);
                        }
                    }
                }
            }
            finally
            {
                if (pinsEnum != null) Marshal.ReleaseComObject(pinsEnum);
            }

            return null;
        }
    }
}