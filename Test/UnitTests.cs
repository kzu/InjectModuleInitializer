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

        [Test]
        public void AssemblyDoesNotExist()
        {
            const string name = "thiswontexist.no.it.doesnt";
            try
            {
                new Injector().Inject(name);
                Assert.Fail();
            }
            catch (InjectionException ex)
            {
                Assert.AreEqual(Errors.AssemblyDoesNotExist(name), ex.Message);
            }
        }

        [Test]
        public void KeyFileDoesNotExist()
        {
            string keyfile = "notexist";
            try
            {
                new Injector().Inject(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath, null, keyfile);
                Assert.Fail();
            }
            catch (InjectionException ex)
            {
                Assert.AreEqual(Errors.KeyFileDoesNotExist(keyfile), ex.Message);
            }
        }

        [Test]
        public void NoModuleInitializerTypeFound()
        {
            BuildAndFailInject(Errors.NoModuleInitializerTypeFound(), "NoModuleInitializerTypeFound", codefile:"NoModuleInitializerTypeFound.cs");
        }

        [Test]
        public void InvalidFormatForModuleInitializer()
        {
            BuildAndFailInject(Errors.InvalidFormatForModuleInitializer(), "InvalidFormatForModuleInitializer", "foo.foo.no.method");
        }

        [Test]
        public void TypeNameDoesNotExist()
        {
            const string type = "Does.Not.Exist";
            BuildAndFailInject(Errors.TypeNameDoesNotExist(type), "TypeNameDoesNotExist", type+"::Method");
        }

        [Test]
        public void MethodNameDoesNotExist()
        {
            const string type = "ModuleInitializer";
            const string method = "NotExist";
            BuildAndFailInject(Errors.MethodNameDoesNotExist(type, method), "MethodNameDoesNotExist", type + "::" + method);
        }

        [Test]
        public void InitializerMethodMayNotBePrivate()
        {
            BuildAndFailInject(Errors.ModuleInitializerMayNotBePrivate(), "InitializerMethodMayNotBePrivate", "ModuleInitializer::Private");
        }

        [Test]
        public void InitializerMethodMayNotBeProtected()
        {
            BuildAndFailInject(Errors.ModuleInitializerMayNotBePrivate(), "InitializerMethodMayNotBeProtected", "ModuleInitializer::Protected");
        }

        [Test]
        public void InitializerMethodMustBeStatic()
        {
            BuildAndFailInject(Errors.ModuleInitializerMustBeStatic(), "InitializerMethodMustBeStatic", "ModuleInitializer::NotStatic");
        }

        [Test]
        public void InitializerMethodMustBeVoid()
        {
            BuildAndFailInject(Errors.ModuleInitializerMustBeVoid(), "InitializerMethodMustBeVoid", "ModuleInitializer::NotVoid");
        }

        [Test]
        public void InitializerMethodMayNotHaveParameters()
        {
            BuildAndFailInject(Errors.ModuleInitializerMayNotHaveParameters(), "InitializerMethodMayNotHaveParameters", "ModuleInitializer::Parameters");
        }
      
        private void BuildAndFailInject(string errorMsg, string assemblyName, string moduleInitializer = null, string keyfile = null, string codefile = "test.cs")
        {
            var result = Build(assemblyName, codefile);
            Assert.AreEqual(0, result.Result);
            try
            {
                new Injector().Inject(string.Format(@"Test\Data\{0}.exe",assemblyName) , moduleInitializer, keyfile);
                Assert.Fail("Should have thrown exception");
            }
            catch (InjectionException ex)
            {
                Assert.AreEqual(errorMsg, ex.Message);
            }
        }

        [Test]
        public void ExeImplicitInitializer()
        {
            var result = Build("ExeImplicitInitializer");
            Assert.AreEqual(0, result.Result);
            new Injector().Inject(@"Test\Data\ExeImplicitInitializer.exe", keyfile: @"Test\Data\testkey.snk");
            ExecResult execResult = Exec(@"Test\Data\ExeImplicitInitializer.exe", "");
            Assert.AreEqual(0, execResult.Result);
            Assert.IsTrue(execResult.StdOut.Contains("<ModuleInit><Main>"));
            Assert.IsTrue(execResult.StdOut.Contains("ClrVersion: " + RuntimeVersion));
            Assert.IsTrue(execResult.StdOut.Contains("AssemblyRuntime: v" + RuntimeVersion));
            Assert.IsTrue(execResult.StdOut.Contains("PublicKey: 6dd84ac5c69bf74"));
        }

        protected virtual string RuntimeVersion {
            get { return "2.0.50727"; }
        }

//        [Test]
//        public void TestExeImplicitInitializer()
//        {
//            TestExe(@"
//            class Program {
//                static void Main(){ System.Console.Write(""<MainMethod>""); }
//            }
//
//            namespace Foo.Bar {
//                class ModuleInitializer {
//                    public static void Run() {System.Console.Write(""<ModuleInitializer>"");}
//                }
//            }
//            ", null);
//        }

//        [Test]
//        public void TestDllSuccess()
//        {
//            string dll = CompileAssembly(@"
//                public class Empty {
//                    public static string NeverSet;
//                }
//
//                public class ModuleInitializer {
//                    public static void Run() {
//                        Empty.NeverSet = ""SetByModuleInitializer"";
//                    }
//                }   
//
//            ", false);
//            var injector = new Injector();
//            injector.Inject(dll, null);
//            Assembly ass = Assembly.Load(File.ReadAllBytes(dll));
//            Type t = ass.GetType("Empty");
//            string value = (string)t.GetField("NeverSet").GetValue(null);
//            Assert.AreEqual("SetByModuleInitializer", value);
//            File.Delete(dll);
//        }

        protected virtual string MSBuild
        {
            get { return @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe"; }
        }

        private class ExecResult
        {
            public int Result { get; set; }
            public string StdOut { get; set; }
            public string StdErr { get; set; }
        }

        private ExecResult Exec(string program, string args)
        {
            var info = new ProcessStartInfo
            {
                FileName = program,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Arguments = args
            };
            Process p = Process.Start(info);
            p.WaitForExit();
            return new ExecResult { Result = p.ExitCode, StdErr = p.StandardError.ReadToEnd(), StdOut = p.StandardOutput.ReadToEnd() };
        }

        private ExecResult Build(string outputName, string codefile="test.cs")
        {
            return Exec(MSBuild, string.Format(@"/p:AssemblyName={0};CodeFile={1} Test\Data\test.build", outputName, codefile));
        }

    }

    public class InjectModuleInitializerTest_4_0 : InjectModuleInitializerTest
    {
        protected override string MSBuild
        {
            get { return @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"; }
        }
    }

}
#endif