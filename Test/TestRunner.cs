/* 
InjectModuleInitializer

/* 
InjectModuleInitializer

Command line program to inject a module initializer into a .NET assembly.

Copyright (C) 2009-2016 Einar Egilsson
http://einaregilsson.com/module-initializers-in-csharp/

This program is licensed under the MIT license: http://opensource.org/licenses/MIT
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace EinarEgilsson.Utilities.InjectModuleInitializer.Test
{
#if DEBUG
    public static class TestRunner
    {
        public static int RunTests()
        {
            int success=0, fail=0;

            var baseType = typeof(InjectModuleInitializerTest);
            var testTypes = new List<Type>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(baseType))
                {
                    testTypes.Add(type);
                }
            }

            foreach (Type t in testTypes) 
            {
                Console.WriteLine("\r\n" + t.Name);
                foreach (var method in t.GetMethods())
                {
                    if (method.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), true).Length > 0)
                    {
                        Console.Write("    "+ method.Name);
                        try
                        {
                            method.Invoke(Activator.CreateInstance(t), new object[0]);
                            WriteColored("\r    " + method.Name, ConsoleColor.Green);
                            success++;
                        }
                        catch (TargetInvocationException ex)
                        {
                            WriteColored("\r    " + method.Name, ConsoleColor.Red);
                            Console.WriteLine(ex.InnerException.Message);
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
