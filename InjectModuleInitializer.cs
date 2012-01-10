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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace EinarEgilsson.Utilities.InjectModuleInitializer
{
    public class InjectModuleInitializer : Task
    {
        [Required]
        public string AssemblyFile { get; set; }
        public string ModuleInitializer { get; set; }
        private readonly Injector injector = new Injector();
        
        public override bool Execute()
        {
            injector.LogError = Log.LogError;
            injector.AssemblyFile = AssemblyFile;
            injector.ModuleInitializer = ModuleInitializer;
            return injector.Execute();
        }
    }
}
