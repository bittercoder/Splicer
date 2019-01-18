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
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Splicer.Utilities
{
    /// <summary>
    /// Tool for registering or unregistering a dynamic link library - handy for registering filters when they're not available.
    /// </summary>
    public class LibraryRegistration
    {
        private const string DllRegisterServer = "DllRegisterServer";
        private const string DllUnregisterServer = "DllUnregisterServer";
        private static ModuleBuilder _moduleBuilder;
        private string _dllFile;
        private Type _generatedType;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryRegistration"/> class.
        /// </summary>
        /// <param name="dllFile">The DLL file.</param>
        public LibraryRegistration(string dllFile)
        {
            _dllFile = dllFile;
            GenerateType();
        }

        /// <summary>
        /// Registers this instance.
        /// </summary>
        public void Register()
        {
            InternalRegServer(false);
        }

        /// <summary>
        /// Unregisters this instance.
        /// </summary>
        public void Unregister()
        {
            InternalRegServer(true);
        }

        /// <summary>
        /// Internals the reg server.
        /// </summary>
        /// <param name="unregister">if set to <c>true</c> [unregister].</param>
        private void InternalRegServer(bool unregister)
        {
            string sMemberName = unregister ? DllUnregisterServer : DllRegisterServer;

            int hr = (int) _generatedType.InvokeMember(sMemberName, BindingFlags.InvokeMethod, null,
                                                       Activator.CreateInstance(_generatedType), null,
                                                       CultureInfo.InvariantCulture);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Generates the type.
        /// </summary>
        private void GenerateType()
        {
            if (_moduleBuilder == null)
            {
                AssemblyName assemblyName = new AssemblyName();
                assemblyName.Name = "DllRegServerAssembly" + Guid.NewGuid().ToString("N");

                AssemblyBuilder assemblyBuilder =
                    AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                _moduleBuilder = assemblyBuilder.DefineDynamicModule("DllRegServerModule");
            }

            TypeBuilder typeBuilder = _moduleBuilder.DefineType("DllRegServerClass" + Guid.NewGuid().ToString("N"));

            MethodBuilder methodBuilder;

            methodBuilder = typeBuilder.DefinePInvokeMethod("DllRegisterServer", _dllFile,
                                                            MethodAttributes.Public | MethodAttributes.Static |
                                                            MethodAttributes.PinvokeImpl,
                                                            CallingConventions.Standard, typeof (int), null,
                                                            CallingConvention.StdCall, CharSet.Auto);

            methodBuilder.SetImplementationFlags(MethodImplAttributes.PreserveSig |
                                                 methodBuilder.GetMethodImplementationFlags());

            methodBuilder = typeBuilder.DefinePInvokeMethod("DllUnregisterServer", _dllFile,
                                                            MethodAttributes.Public | MethodAttributes.Static |
                                                            MethodAttributes.PinvokeImpl,
                                                            CallingConventions.Standard, typeof (int), null,
                                                            CallingConvention.StdCall, CharSet.Auto);

            methodBuilder.SetImplementationFlags(MethodImplAttributes.PreserveSig |
                                                 methodBuilder.GetMethodImplementationFlags());
            _generatedType = typeBuilder.CreateType();
        }
    }
}