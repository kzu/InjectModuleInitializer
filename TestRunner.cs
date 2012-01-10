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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
#if DEBUG
    public static class TestRunner
    {
        public static int RunTests()
        {
            int success=0, fail=0;
            foreach (Type t in new[]{typeof(InjectModuleInitializerTest), typeof(InjectModuleInitializerTest_4_0)}) 
            {
                Console.WriteLine("\r\n" + t.Name);
                foreach (var method in t.GetMethods())
                {
                    if (method.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), true).Length > 0)
                    {
                        Console.Write("    "+ method.Name);
                        try
                        {
                            method.Invoke(new InjectModuleInitializerTest(), new object[0]);
                            WriteColored("\r    " + method.Name, ConsoleColor.Green);
                            success++;
                        }
                        catch (Exception ex)
                        {
                            WriteColored("\r    " + method.Name, ConsoleColor.Red);
                            Console.WriteLine(ex.Message);
                            fail++;
                        }
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("RESULTS: {0} Succeeded, {1} Failed", success, fail);
            Console.ReadKey();
            return 0;
        }
        static void WriteColored(string msg, ConsoleColor color)
        {
            ConsoleColor normal = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = normal;
        }

    }
#endif
}
