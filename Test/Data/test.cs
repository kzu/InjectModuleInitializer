using System;
using System.Reflection;

class Program {

	public static void Main(){
        Assembly ass = Assembly.GetExecutingAssembly();
		System.Console.WriteLine("<Main>");
        System.Console.WriteLine("ClrVersion: " + System.Environment.Version);
        System.Console.WriteLine("AssemblyRuntime: " + ass.ImageRuntimeVersion);
        string key = "";
        foreach (byte b in new AssemblyName(ass.FullName).GetPublicKeyToken()) {
            key += b.ToString("x");
        }
        System.Console.WriteLine("PublicKey: " + key);
	}
}

public class ModuleInitializer {
	public static void Run() {
		System.Console.Write("<ModuleInit>");
	}

    public static int NotVoid()
    {
        return 0;
    }

    public static void Parameters(int x, double s) { }
    public void NotStatic() { }
    private static void Private() { }
    protected static void Protected() { }

}

namespace NS
{
    public class SomeOtherClass
    {
        public static void SomeOtherMethod()
        {
            System.Console.Write("<ModuleInit>");
        }
    }
}
