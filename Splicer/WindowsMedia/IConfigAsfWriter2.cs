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

namespace Splicer.WindowsMedia
{
    /// <summary>Defines filter configuration parameters used in the IConfigAsfWriter2 GetParam and SetParam methods.</summary>
    public enum AmAsfWriterConfigParam
    {
        /// <summary>No parameters.</summary>
        None = 0,
        /// <summary>
        /// Indicates whether the WM ASF Writer should automatically create a temporal index 
        /// after it has completed encoding a file. Set this parameter to FALSE if you want 
        /// to create a frame-based index using the Windows Media Format SDK directly.
        /// </summary>
        AutoIndex = 1,
        /// <summary>
        /// Indicates whether the filter should operate in two-pass mode. In two-pass mode the filter 
        /// makes two passes through the file. In the first pass, the filter examines each media stream 
        /// in its entirety to determine the optimal encoding parameters for the file. The actual 
        /// encoding is performed in the second pass. Therefore, to create an ASF file in two-pass mode, 
        /// you must run the graph, wait for an EC_PREPROCESS_COMPLETE event, seek to the beginning of 
        /// the source file, and then run the graph a second time.
        /// </summary>
        Multipass = 2,
        /// <summary>
        /// Indicates that the WM ASF Writer will not attempt to compress the input streams.
        /// Use this flag to pack content that is not Windows Media–based into an ASF file.
        /// </summary>
        DontCompress = 3
    }

    /// <summary>
    /// The IConfigAsfWriter interface is implemented by the WM ASF Writer filter and provides methods for getting 
    /// and setting the Advanced Streaming Format (ASF) profiles the filter will use to write files.
    /// </summary>
    [ComImport]
    [Guid("7989CCAA-53F0-44f0-884A-F3B03F6AE066")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IConfigAsfWriter2
    {
        /// <summary>
        /// The ConfigureFilterUsingProfileId method configures the filter to write an ASF file 
        /// with a profile identifier (ID) index from the system profile list.
        /// </summary>
        /// <param name="dwProfileId">Profile ID as defined in the Windows Media Format SDK.</param>
        void ConfigureFilterUsingProfileId([In] uint dwProfileId);

        /// <summary>The GetCurrentProfileId method retrieves the current ASF profile ID.</summary>
        /// <returns>A variable of type DWORD that receives the current profile ID.</returns>
        uint GetCurrentProfileId();

        /// <summary>The ConfigureFilterUsingProfileGuid method configures the filter to write an ASF file based on the specified predefined profile.</summary>
        /// <param name="guidProfile">Profile GUID as defined in the Windows Media Format SDK header file Wmsysprf.h.</param>
        void ConfigureFilterUsingProfileGuid(ref Guid guidProfile);

        /// <summary>The GetCurrentProfileGuid method retrieves the current ASF profile ID.</summary>
        /// <param name="pProfileGuid">Pointer to a variable of type GUID that receives the profile ID.</param>
        void GetCurrentProfileGuid([Out] out Guid pProfileGuid);

        /// <summary>
        /// The ConfigureFilterUsingProfile method configures the filter to write an ASF file 
        /// based on the specified application-defined profile.
        /// </summary>
        /// <param name="pProfile">IWMProfile interface pointer to the application-defined profile.</param>
        void ConfigureFilterUsingProfile([In] IntPtr pProfile);

        /// <summary>The GetCurrentProfile method retrieves the application-defined ASF profile.</summary>
        /// <returns>Address of a pointer that receives the IWMProfile interface of the application-defined profile.</returns>
        IntPtr GetCurrentProfile();

        /// <summary>The SetIndexMode method enables the application to control whether the file will be indexed and therefore seekable.</summary>
        /// <param name="bIndexFile">Variable of type BOOL; TRUE specifies that the file will be indexed.</param>
        void SetIndexMode([In, MarshalAs(UnmanagedType.Bool)] bool bIndexFile);

        /// <summary>The GetIndexMode method retrieves the current index mode.</summary>
        /// <param name="pbIndexFile">
        /// Pointer to a variable of type BOOL that receives the index mode setting. A value of TRUE 
        /// indicates that the WM ASF Writer is configured to write indexed files.
        /// </param>
        void GetIndexMode([Out, MarshalAs(UnmanagedType.Bool)] out bool pbIndexFile);

        /// <summary>The StreamNumFromPin method retrieves the stream number associated with the specified input pin.</summary>
        /// <param name="pPin">Input pin.</param>
        /// <param name="pwStreamNum">Pointer that receives the pin number.</param>
        void StreamNumFromPin([In] IPin pPin, [In] short pwStreamNum);

        /// <summary>The SetParam method sets the value of the specified filter configuration parameter.</summary>
        /// <param name="dwParam">Specifies the parameter to set.</param>
        /// <param name="dwParam1">Specifies the value to assign to the dwParam parameter.</param>
        /// <param name="dwParam2">Not used; must be 0.</param>
        void SetParam([In] AmAsfWriterConfigParam dwParam, [In] uint dwParam1, [In] uint dwParam2);

        /// <summary>The GetParam method retrieves the current value of the specified filter configuration parameter.</summary>
        /// <param name="dwParam">Specifies the parameter to retrieve.</param>
        /// <param name="pdwParam1">Pointer to a variable that retrieves the value of the parameter specified in dwParam.</param>
        /// <param name="pdwParam2">Not used; must be 0.</param>
        void GetParam([In] AmAsfWriterConfigParam dwParam, [Out] out uint pdwParam1, [Out] out uint pdwParam2);

        /// <summary>
        /// The ResetMultiPassState method resets the filter when a preprocessing 
        /// encoding pass is canceled before it is completed.
        /// </summary>
        void ResetMultiPassState();
    }
}