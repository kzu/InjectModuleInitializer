/* 
InjectModuleInitializer

MSBuild task and command line program to inject a module initializer
into a .NET assembly

Copyright (C) 2009 Einar Egilsson
http://tech.einaregilsson.com/2009/12/08/module-initializers-in-csharp/

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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.CSharp;
using NUnit.Framework;

#if DEBUG
namespace EinarEgilsson.Utilities.InjectModuleInitializer.Tests
{

    [TestFixture]
    public class InjectModuleInitializerTest
    {

        class LogMessageCollector
        {
            public readonly List<string> Messages = new List<string>();
            public void Log(string msg, params object[] args)
            {
                Messages.Add(string.Format(msg, args));
            }
        }

        [Test]
        public void TestAssemblyDoesNotExist()
        {
            const string name = "thiswontexist.no.it.doesnt";
            ExpectFailure(Errors.AssemblyDoesNotExist(name), name, null, null);
        }

        [Test]
        public void TestNoModuleInitializerTypeFound()
        {
            ExpectFailure(Errors.NoModuleInitializerTypeFound(), null, null,
                @"
            namespace Foo.Bar {
                class Baz {
                    static void Main(){}
                }
            }
");
        }

        [Test]
        public void TestInvalidFormatForModuleInitializer()
        {
            ExpectFailure(Errors.InvalidFormatForModuleInitializer(), null, "foo.foo.no.method",
            @"
            namespace Foo.Bar {
                class Baz {
                    static void Main(){}
                }
            }
            ");
        }

        [Test]
        public void TestTypeNameDoesNotExist()
        {
            ExpectFailure(Errors.TypeNameDoesNotExist("Foo.Bar.NotExist"), null, "Foo.Bar.NotExist::Run",
            @"
            namespace Foo.Bar {
                class Baz {
                    static void Main(){}
                }
            }
            ");
        }

        [Test]
        public void TestNoSuitableMethodFoundInType()
        {
            ExpectFailure(Errors.NoSuitableMethodFoundInType("Baz", "Foo.Bar"), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    public static void Baz(int x){}
                }
            }
            ");
            ExpectFailure(Errors.NoSuitableMethodFoundInType("Baz", "Foo.Bar"), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                }
            }
            ");
        }

        [Test]
        public void TestModuleInitializerMayNotBePrivate()
        {
            ExpectFailure(Errors.ModuleInitializerMayNotBePrivate(), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    private static void Baz(){}
                }
            }
            ");
            ExpectFailure(Errors.ModuleInitializerMayNotBePrivate(), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    protected static void Baz(){}
                }
            }
            ");
            ExpectFailure(Errors.ModuleInitializerMayNotBePrivate(), null, null,
            @"
            namespace Foo {
                class ModuleInitializer {
                    static void Main(){}
                    protected static void Run(){}
                }
            }
            ");
            ExpectFailure(Errors.ModuleInitializerMayNotBePrivate(), null, null,
            @"
            namespace Foo {
                class ModuleInitializer {
                    static void Main(){}
                    private static void Run(){}
                }
            }
            ");
        }

        [Test]
        public void TestModuleInitializerMustBeVoid()
        {
            ExpectFailure(Errors.ModuleInitializerMustBeVoid(), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    public static int Baz(){return 0;}
                }
            }
            ");
            ExpectFailure(Errors.ModuleInitializerMustBeVoid(), null, null,
            @"
            namespace Foo {
                class ModuleInitializer {
                    static void Main(){}
                    public static int Run(){return 0;}
                }
            }
            ");
        }

        [Test]
        public void TestExeExplicitInitializer()
        {
            TestExe(@"
            class Program {
                static void Main(){ System.Console.Write(""<MainMethod>""); }
                public static void Initializer() {System.Console.Write(""<ModuleInitializer>"");}
            }
            ", "Program::Initializer");
        }

        [Test]
        public void TestExeImplicitInitializer()
        {
            TestExe(@"
            class Program {
                static void Main(){ System.Console.Write(""<MainMethod>""); }
            }

            namespace Foo.Bar {
                class ModuleInitializer {
                    public static void Run() {System.Console.Write(""<ModuleInitializer>"");}
                }
            }
            ", null);
        }

        [Test]
        public void TestDllSuccess()
        {
            string dll = CompileAssembly(@"
                public class Empty {
                    public static string NeverSet {get; set;}
                }

                public class ModuleInitializer {
                    public static void Run() {
                        Empty.NeverSet = ""SetByModuleInitializer"";
                    }
                }   

            ", false);
            var injector = new InjectModuleInitializerImpl { AssemblyName = dll };
            Assert.IsTrue(injector.Execute(), "Injection failed");
            Assembly ass = Assembly.Load(File.ReadAllBytes(dll));
            Type t = ass.GetType("Empty");
            string value = (string)t.GetProperty("NeverSet").GetGetMethod().Invoke(null, null);
            Assert.AreEqual("SetByModuleInitializer", value);
            File.Delete(dll);
        }

        private static void TestExe(string source, string initializer)
        {
            string exe = CompileAssembly(source, true);

            var injector = new InjectModuleInitializerImpl { AssemblyName = exe, ModuleInitializer = initializer };
            Assert.IsTrue(injector.Execute(), "Injection failed");
            var info = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var proc = Process.Start(info);
            Assert.IsNotNull(proc);
            string output = proc.StandardOutput.ReadToEnd();
            Assert.AreEqual("<ModuleInitializer><MainMethod>", output);
            proc.WaitForExit();
            Thread.Sleep(200);
            File.Delete(exe);
        }

        private static string CompileAssembly(string source, bool isExe)
        {
            string filename = Path.GetTempFileName() + (isExe ? ".exe" : ".dll");
            var csc = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, filename, false);
            parameters.GenerateExecutable = isExe;
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, source);
            Assert.AreEqual(0, results.Errors.Count, "Invalid source code passed");
            return filename;
        }

        private static void ExpectFailure(string expectedMessage, string assemblyName, string moduleInitializer, string source)
        {
            Assert.IsTrue(assemblyName == null || source == null,
                          "Either source or assembly name should be passed as null");
            LogMessageCollector logger = new LogMessageCollector();
            var injector = new InjectModuleInitializerImpl();
            injector.LogError = logger.Log;
            injector.ModuleInitializer = moduleInitializer;
            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = CompileAssembly(source, true);
            }
            injector.AssemblyName = assemblyName;
            injector.Execute();
            Assert.Greater(logger.Messages.Count, 0, "No messages collected");
            Assert.AreEqual(expectedMessage, logger.Messages[0]);
            if (File.Exists(assemblyName))
            {
                File.Delete(assemblyName);
            }
        }
    }
}
#endif