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
using System.Text.RegularExpressions;

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
    internal class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            //I only have VS Express at home and this is the easiest way to debug
            //the unit tests since there's no test runner and I can't attach to NUnit.
            if (args.Length == 1 && args[0] == "/runtests")
            {
                return TestRunner.RunTests();
            }
#endif
            var task = new Injector();
            if (args.Length == 0 || args.Length > 2 || Regex.IsMatch(args[0], @"^((/|--?)(\?|h|help))$"))
            {
                PrintHelp();
                return 1;
            }

            Console.WriteLine("InjectModuleInitializer v1.1");
            Console.WriteLine("");
            
            task.AssemblyFile = args[args.Length - 1];
            if (args.Length == 2)
            {
                var match = Regex.Match(args[0], "^(/m:|/ModuleInitializer:)(.+)", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    Console.Error.WriteLine("ERROR: Invalid argument '{0}', type InjectModuleInitializer /? for help", args[0]);
                    return 1;
                }
                task.ModuleInitializer = match.Groups[2].Value;
            }
                
            int result = task.Execute() ? 0 : 1;
            if (result == 0)
            {
                Console.WriteLine("Module Initializer successfully injected in assembly " + task.AssemblyFile);
            }
            return result;
        }
        
        static void PrintHelp()
        {
            Console.Error.WriteLine(@"
InjectModuleInitializer.exe [/m:<method>] filename

/m:<method>                   Specify the method to be run as the module  
/moduleinitializer:<method>   initializer. Written as full name of containing
                              type, followed by :: and then the method name,
                              e.g. Namespace.ClassName::Method. Method is
                              required to be a public or internal static
                              method that takes no parameters and returns void.
                              If this parameter is omitted the program will 
                              look for a type name ModuleInitializer (in any
                              namespace) and look for a method named Run in
                              that type.

/?                            Prints this help screen.

Additional information about this program, including how to use it as
a MSBuild task, can be found at the url:

  http://einaregilsson.com/2009/12/16/module-initializers-in-csharp
");
        }

    }
}
