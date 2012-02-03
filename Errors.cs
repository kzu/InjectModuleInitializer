/* 
InjectModuleInitializer

Command line program to inject a module initializer into a .NET assembly.

Copyright (C) 2009-2012 Einar Egilsson
http://einaregilsson.com/module-initializers-in-csharp/

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

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
    internal static class Errors
    {
        internal static string AssemblyDoesNotExist(string assembly)
        {
            return String.Format("Assembly '{0}' does not exist", assembly);
        }

        internal static string NoModuleInitializerTypeFound()
        {
            return "Found no type named 'ModuleInitializer', this type must exist or the ModuleInitializer parameter must be used";
        }

        internal static string InvalidFormatForModuleInitializer()
        {
            return "Invalid format for ModuleInitializer parameter, use Full.Type.Name::MethodName";
        }
        
        internal static string TypeNameDoesNotExist(string typeName)
        {
            return string.Format("No type named '{0}' exists in the given assembly!", typeName);
        }

        internal static string MethodNameDoesNotExist(string typeName, string methodName)
        {
            return string.Format("No method named '{0}' exists in the type '{0}'", methodName, typeName);
        }

        internal static string KeyFileDoesNotExist(string keyfile)
        {
            return string.Format("The key file'{0}' does not exist", keyfile);
        }
        
        internal static string ModuleInitializerMayNotBePrivate()
        {
            return "Module initializer method may not be private or protected, use public or internal instead";
        }
        
        internal static string ModuleInitializerMustBeVoid()
        {
            return "Module initializer method must have 'void' as return type";
        }

        internal static string ModuleInitializerMayNotHaveParameters()
        {
            return "Module initializer method must not have any parameters";
        }

        internal static string ModuleInitializerMustBeStatic()
        {
            return "Module initializer method must be static";
        }
    }
}
