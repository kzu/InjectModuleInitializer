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
$Url$ 
*/
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("InjectModuleInitializer v" + Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("");
            var task = new InjectModuleInitializerImpl();
            if (args.Length < 1 || args.Length > 2 || args[0] == "/?")
            {
                Help();
                return 1;
            }

            foreach (var arg in args)
            {
                Match match = Regex.Match(arg, "^(/a:|/assembly:)(.+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    task.AssemblyName = match.Groups[2].Value;
                    continue;
                }
                match = Regex.Match(arg, "^(/m:|/moduleconstructor:)(.+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    task.ModuleConstructor = match.Groups[2].Value;
                    continue;
                }
                Console.Error.WriteLine("ERROR: Invalid argument '{0}'. Type 'InjectModuleInitializer /? for help", arg);
                return 1;
            }
                
            return task.Execute() ? 0 : 1;
        }

        static void Help()
        {
            Console.WriteLine("HELP");
        }
    }
}


//string assemblyName = args[0];
//string[] parts = Regex.Split(args[1], "::");
//string typeName = parts[0];
//string methodName = parts[1];

//AssemblyDefinition assembly = AssemblyFactory.GetAssembly (assemblyName); 
//TypeReference voidRef = assembly.MainModule.Import(typeof(void));
//var attributes = MethodAttributes.Static
//                | MethodAttributes.SpecialName
//                | MethodAttributes.RTSpecialName;
//var cctor = new MethodDefinition( ".cctor", attributes, voidRef);

//TypeDefinition type = assembly.MainModule.Types[typeName];
//MethodReference methodRef = type.Methods.GetMethod(methodName,new Type[]{});
//cctor.Body.CilWorker.Append(cctor.Body.CilWorker.Create(OpCodes.Call, methodRef));
//cctor.Body.CilWorker.Append(cctor.Body.CilWorker.Create(OpCodes.Ret));
//assembly.MainModule.Inject(cctor, assembly.MainModule.Types["<Module>"]);
//AssemblyFactory.SaveAssembly(assembly, assemblyName);         

