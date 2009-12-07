/* 
InjectModuleInitializer

MSBuild task and command line program to inject a module initializer
into a .NET assembly

Copyright (C) 2009 Einar Egilsson
http://tech.einaregilsson.com/2009/12/07/module-initializers-in-csharp/

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

$Author$
$Revision$
$HeadURL$ 
*/
using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
    internal class InjectModuleInitializerImpl
    {
        public delegate void ErrorLogger(string msg, params object[] args);
        public string AssemblyName { get; set; }
        public string ModuleInitializer { get; set; }
        public ErrorLogger LogError { get; set; }

        public bool Execute()
        {
            try
            {
                if (LogError == null)
                {
                    LogError = (msg, args) => Console.Error.WriteLine("ERROR: " + msg, args);
                }
                if (!File.Exists(AssemblyName))
                {
                    LogError(Errors.AssemblyDoesNotExist(AssemblyName));
                    return false;
                }
                AssemblyDefinition assembly = AssemblyFactory.GetAssembly(AssemblyName);

                MethodReference callee = GetCalleeMethod(assembly);
                if (callee == null)
                {
                    return false;
                }

                TypeReference voidRef = assembly.MainModule.Import(typeof(void));
                const MethodAttributes attributes = MethodAttributes.Static
                                                    | MethodAttributes.SpecialName
                                                    | MethodAttributes.RTSpecialName;
                var cctor = new MethodDefinition(".cctor", attributes, voidRef);

                cctor.Body.CilWorker.Append(cctor.Body.CilWorker.Create(OpCodes.Call, callee));
                cctor.Body.CilWorker.Append(cctor.Body.CilWorker.Create(OpCodes.Ret));
                assembly.MainModule.Inject(cctor, assembly.MainModule.Types["<Module>"]);
                AssemblyFactory.SaveAssembly(assembly, AssemblyName);
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

        private MethodReference GetCalleeMethod(AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.MainModule;
            string typeName = null, methodName;
            if (string.IsNullOrEmpty(ModuleInitializer))
            {
                methodName = "Run";
                foreach (TypeDefinition t in module.Types)
                {
                    if (t.Name == "ModuleInitializer")
                    {
                        typeName = t.FullName;
                        break;
                    }
                }
                if (typeName == null)
                {
                    LogError(Errors.NoModuleInitializerTypeFound());
                    return null;
                }
            } else
            {
                if (!ModuleInitializer.Contains("::"))
                {
                    LogError(Errors.InvalidFormatForModuleInitializer());
                    return null;
                }
                typeName = ModuleInitializer.Substring(0, ModuleInitializer.IndexOf("::"));
                methodName = ModuleInitializer.Substring(typeName.Length + 2);
                if (!module.Types.Contains(typeName))
                {
                    LogError(Errors.TypeNameDoesNotExist(typeName));
                    return null;
                }
            }

            TypeDefinition type = module.Types[typeName];
            MethodDefinition callee = type.Methods.GetMethod(methodName, new Type[] { });
            if (callee == null)
            {
                LogError(Errors.NoSuitableMethodFoundInType(methodName, typeName));
                return null;
            }
            if (callee.IsPrivate || callee.IsFamily)
            {
                LogError(Errors.ModuleInitializerMayNotBePrivate());
                return null;
            }
            TypeReference voidRef = module.Import(typeof(void));
            if (!callee.ReturnType.ReturnType.Equals(voidRef))
            {
                LogError(Errors.ModuleInitializerMustBeVoid());
                return null;
            }
            return callee;
        }
    }
}
