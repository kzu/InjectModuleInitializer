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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
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
        public void TestNoModuleConstructorTypeFound()
        {
            ExpectFailure(Errors.NoModuleConstructorTypeFound(), null, null, 
                @"
            namespace Foo.Bar {
                class Baz {
                    static void Main(){}
                }
            }
");
        }

        [Test]
        public void TestInvalidFormatForModuleConstructor()
        {
            ExpectFailure(Errors.InvalidFormatForModuleConstructor(), null, "foo.foo.no.method",
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
        public void TestModuleConstructorMayNotBePrivate()
        {
            ExpectFailure(Errors.ModuleConstructorMayNotBePrivate(), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    private static void Baz(){}
                }
            }
            ");
            ExpectFailure(Errors.ModuleConstructorMayNotBePrivate(), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    protected static void Baz(){}
                }
            }
            ");
            ExpectFailure(Errors.ModuleConstructorMayNotBePrivate(), null, null,
            @"
            namespace Foo {
                class ModuleConstructor {
                    static void Main(){}
                    protected static void Run(){}
                }
            }
            ");
            ExpectFailure(Errors.ModuleConstructorMayNotBePrivate(), null, null,
            @"
            namespace Foo {
                class ModuleConstructor {
                    static void Main(){}
                    private static void Run(){}
                }
            }
            ");
        }

        [Test]
        public void TestModuleConstructorMustBeVoid()
        {
            ExpectFailure(Errors.ModuleConstructorMustBeVoid(), null, "Foo.Bar::Baz",
            @"
            namespace Foo {
                class Bar {
                    static void Main(){}
                    public static int Baz(){return 0;}
                }
            }
            ");
            ExpectFailure(Errors.ModuleConstructorMustBeVoid(), null, null,
            @"
            namespace Foo {
                class ModuleConstructor {
                    static void Main(){}
                    public static int Run(){return 0;}
                }
            }
            ");
        }

        private string CompileAssembly(string source)
        {
            string filename = Path.GetTempFileName() + ".exe";
            var csc = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, filename, false);
            parameters.GenerateExecutable = true;
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, source);
            Assert.AreEqual(0, results.Errors.Count, "Invalid source code passed");
            return filename;
        }

        private void ExpectFailure(string expectedMessage, string assemblyName, string moduleConstructor, string source)
        {
            Assert.IsTrue(assemblyName == null || source == null,
                          "Either source or assembly name should be passed as null");
            LogMessageCollector logger = new LogMessageCollector();
            var injector = new InjectModuleInitializerImpl();
            injector.LogError = logger.Log;
            injector.ModuleConstructor = moduleConstructor;
            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = CompileAssembly(source);
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