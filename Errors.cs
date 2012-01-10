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

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
    internal static class Errors
    {
        public static string AssemblyDoesNotExist(string assembly)
        {
            return String.Format("Assembly '{0}' does not exist", assembly);
        }

        public static string NoModuleInitializerTypeFound()
        {
            return "Found no type named 'ModuleInitializer', this type must exist or the ModuleInitializer parameter must be used";
        }

        public static string InvalidFormatForModuleInitializer()
        {
            return "Invalid format for ModuleInitializer parameter, use Full.Type.Name::MethodName";
        }
        
        public static string TypeNameDoesNotExist(string typeName)
        {
            return string.Format("No type named '{0}' exists in the given assembly!", typeName);
        }
        
        public static string NoSuitableMethodFoundInType(string methodName, string typeName)
        {
            return string.Format("No suitable method named '{0}' found in type '{1}'", methodName, typeName);
        }
        
        public static string ModuleInitializerMayNotBePrivate()
        {
            return "Module initializer may not be private or protected, use public or internal instead";
        }
        
        public static string ModuleInitializerMustBeVoid()
        {
            return "Module initializer must have 'void' as return type";
        }
    }
}
