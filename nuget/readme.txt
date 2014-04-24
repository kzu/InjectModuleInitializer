********************************************************************************
_________ _______  ______   _______    
\__   __/(  ___  )(  __  \ (  ___  )   
   ) (   | (   ) || (  \  )| (   ) | _ 
   | |   | |   | || |   ) || |   | |(_)
   | |   | |   | || |   | || |   | |   
   | |   | |   | || |   ) || |   | | _ 
   | |   | (___) || (__/  )| (___) |(_)
   )_(   (_______)(______/ (_______)   
                                       

1) Open ModuleInitializer.cs.
2) Add initialization logic to the Run method's body.

The InjectModuleInitializer nuget package modifies your project file, adding
an "AfterBuild" target. Upon a successful build of your project, the "After
Build" target runs InjectModuleInitializer.exe. This modifies your compiled
assembly so that, when loaded, the ModuleInitializer.Run() is called.

********************************************************************************

********************************************************************************
 _       _________ _______  _______  _        _______  _______ 
( \      \__   __/(  ____ \(  ____ \( (    /|(  ____ \(  ____ \
| (         ) (   | (    \/| (    \/|  \  ( || (    \/| (    \/
| |         | |   | |      | (__    |   \ | || (_____ | (__    
| |         | |   | |      |  __)   | (\ \) |(_____  )|  __)   
| |         | |   | |      | (      | | \   |      ) || (      
| (____/\___) (___| (____/\| (____/\| )  \  |/\____) || (____/\
(_______/\_______/(_______/(_______/|/    )_)\_______)(_______/
                                                               
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

********************************************************************************