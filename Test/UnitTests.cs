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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

#if DEBUG
namespace EinarEgilsson.Utilities.InjectModuleInitializer.Test
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
        public void TestModuleInitializerMustBeStatic()
        {
            ExpectFailure(Errors.ModuleInitializerMustBeStatic(), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    public void Baz(){}
                }
            }
            ");
            ExpectFailure(Errors.ModuleInitializerMustBeStatic(), null, null,
            @"
            namespace Foo {
                class ModuleInitializer {
                    static void Main(){}
                    public void Run(){}
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
                    public static string NeverSet;
                }

                public class ModuleInitializer {
                    public static void Run() {
                        Empty.NeverSet = ""SetByModuleInitializer"";
                    }
                }   

            ", false);
            var injector = new Injector { AssemblyFile = dll };
            Assert.IsTrue(injector.Execute(), "Injection failed");
            Assembly ass = Assembly.Load(File.ReadAllBytes(dll));
            Type t = ass.GetType("Empty");
            string value = (string) t.GetField("NeverSet").GetValue(null);
            Assert.AreEqual("SetByModuleInitializer", value);
            File.Delete(dll);
        }

        private void TestExe(string source, string initializer)
        {
            string exe = CompileAssembly(source, true);

            var injector = new Injector { AssemblyFile = exe, ModuleInitializer = initializer };
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

        protected virtual string Compiler
        {
            get { return @"C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe"; }
        }

        private string CompileAssembly(string source, bool isExe)
        {
            string filename = Path.GetTempFileName() + (isExe ? ".exe" : ".dll");
            string target = isExe ? "exe" : "library";
            string sourceFile = Path.GetTempFileName() + ".cs";
            File.WriteAllText(sourceFile, source);
            var info = new ProcessStartInfo
            {
                FileName = Compiler,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = string.Format("/out:\"{0}\" /t:{1} /debug /reference:mscorlib.dll \"{2}\"", filename, target,sourceFile)
            };
            Process p = Process.Start(info);
            p.WaitForExit();
            Assert.AreEqual(0, p.ExitCode, "Invalid source code passed");
            return filename;
        }

        private void ExpectFailure(string expectedMessage, string assemblyName, string moduleInitializer, string source)
        {
            Assert.IsTrue(assemblyName == null || source == null,
                          "Either source or assembly name should be passed as null");
            LogMessageCollector logger = new LogMessageCollector();
            var injector = new Injector();
            injector.LogError = logger.Log;
            injector.ModuleInitializer = moduleInitializer;
            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = CompileAssembly(source, true);
            }
            injector.AssemblyFile = assemblyName;
            injector.Execute();
            Assert.Greater(logger.Messages.Count, 0, "No messages collected");
            Assert.AreEqual(expectedMessage, logger.Messages[0]);
            if (File.Exists(assemblyName))
            {
                File.Delete(assemblyName);
            }
        }
    }

    public class InjectModuleInitializerTest_4_0 : InjectModuleInitializerTest
    {
        protected override string Compiler
        {
            get { return @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"; }
        }
    }

}
#endif