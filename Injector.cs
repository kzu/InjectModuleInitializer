/* 
InjectModuleInitializer

MSBuild task and command line program to inject a module initializer
into a .NET assembly

Copyright (C) 2009 Einar Egilsson
http://einaregilsson.com/2009/12/16/module-initializers-in-csharp/

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Collections.Generic;

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
    internal class Injector
    {
        public delegate void ErrorLogger(string msg, params object[] args);
        public string AssemblyFile { get; set; }
        public string ModuleInitializer { get; set; }
        public ErrorLogger LogError { get; set; }

        static Injector()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadEmbeddedAssembly;
        }

        static System.Reflection.Assembly LoadEmbeddedAssembly(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Substring(0, args.Name.IndexOf(','));
            string resourceName = typeof(Injector).Namespace + ".lib." + name + ".dll";
            Stream stream = typeof(Injector).Assembly.GetManifestResourceStream(resourceName);
            if (stream == null) {
                return null;
            }
            using (stream) {
                byte[] buf = new byte[stream.Length];
                stream.Read(buf,0, buf.Length);
                return System.Reflection.Assembly.Load(buf);
            }
        }


        private AssemblyDefinition Assembly { get; set; }

        private string PdbFile
        {
            get
            {
                Debug.Assert(AssemblyFile != null);
                string path = Path.ChangeExtension(AssemblyFile, ".pdb");
                if (File.Exists(path))
                {
                    return path;
                }
                return null;
            }
        }


        public bool Execute()
        {
            try
            {
                if (LogError == null)
                {
                    LogError = (msg, args) => Console.Error.WriteLine("ERROR: " + msg, args);
                }
                if (!File.Exists(AssemblyFile))
                {
                    LogError(Errors.AssemblyDoesNotExist(AssemblyFile));
                    return false;
                }
                
                ReadAssembly();

                MethodReference callee = GetCalleeMethod();
                if (callee == null)
                {
                    return false;
                }

                InjectInitializer(callee);

                WriteAssembly();

                return true;
            }
            catch (Exception ex)
            {
                if (LogError != null)
                {
                    LogError(ex.Message);
                }
                return false;
            }
        }

        private void InjectInitializer(MethodReference callee)
        {
            Debug.Assert(Assembly != null);
            TypeReference voidRef = Assembly.MainModule.Import(callee.ReturnType);
            const MethodAttributes attributes = MethodAttributes.Static
                                                | MethodAttributes.SpecialName
                                                | MethodAttributes.RTSpecialName;
            var cctor = new MethodDefinition(".cctor", attributes, voidRef);
            ILProcessor il = cctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Call, callee));
            il.Append(il.Create(OpCodes.Ret));

            TypeDefinition moduleClass = Find(Assembly.MainModule.Types, t => t.Name == "<Module>");
            moduleClass.Methods.Add(cctor);

            Debug.Assert(moduleClass != null, "Found no module class!");
        }

        private void WriteAssembly()
        {
            Debug.Assert(Assembly != null);
            var writeParams = new WriterParameters();
            if (PdbFile != null)
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new PdbWriterProvider();
            }
            Assembly.Write(AssemblyFile, writeParams);
        }

        private void ReadAssembly()
        {
            Debug.Assert(Assembly == null);
            var readParams = new ReaderParameters(ReadingMode.Immediate);
            if (PdbFile != null)
            {
                readParams.ReadSymbols = true;
                readParams.SymbolReaderProvider = new PdbReaderProvider();
            }
            Assembly = AssemblyDefinition.ReadAssembly(AssemblyFile, readParams);
        }

        private MethodReference GetCalleeMethod()
        {
            Debug.Assert(Assembly != null);
            ModuleDefinition module = Assembly.MainModule;
            string methodName;
            TypeDefinition moduleInitializerClass;
            if (string.IsNullOrEmpty(ModuleInitializer))
            {
                methodName = "Run";
                moduleInitializerClass = Find(module.Types, t => t.Name == "ModuleInitializer");
                if (moduleInitializerClass == null)
                {
                    LogError(Errors.NoModuleInitializerTypeFound());
                    return null;
                }
            }
            else
            {
                if (!ModuleInitializer.Contains("::"))
                {
                    LogError(Errors.InvalidFormatForModuleInitializer());
                    return null;
                }
                string typeName = ModuleInitializer.Substring(0, ModuleInitializer.IndexOf("::"));
                methodName = ModuleInitializer.Substring(typeName.Length + 2);
                moduleInitializerClass = Find(module.Types, t => t.FullName == typeName);
                if (moduleInitializerClass == null)
                {
                    LogError(Errors.TypeNameDoesNotExist(typeName));
                    return null;
                }
            }

            MethodDefinition callee = Find(moduleInitializerClass.Methods, m => m.Name == methodName && m.Parameters.Count == 0);
            if (callee == null)
            {
                LogError(Errors.NoSuitableMethodFoundInType(methodName, moduleInitializerClass.FullName));
                return null;
            }
            if (callee.IsPrivate || callee.IsFamily)
            {
                LogError(Errors.ModuleInitializerMayNotBePrivate());
                return null;
            }
            if (!callee.ReturnType.FullName.Equals("System.Void")) //Comparing the objects themselves doesn't work as of Mono.Cecil 0.9 for some reason...
            {
                LogError(Errors.ModuleInitializerMustBeVoid());
                return null;
            }
            return callee;
        }

        //No LINQ, since we want to target 2.0
        private static T Find<T>(Collection<T> objects, Predicate<T> condition) where T:class
        {
            foreach (T obj in objects)
            {
                if (condition(obj))
                {
                    return obj;
                }
            }
            return null;
        }
    }
}
