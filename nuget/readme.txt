********************************************************************************
_________ _______  ______   _______    
\__   __/(  ___  )(  __  \ (  ___  )   
   ) (   | (   ) || (  \  )| (   ) | _ 
   | |   | |   | || |   ) || |   | |(_)
   | |   | |   | || |   | || |   | |   
   | |   | |   | || |   ) || |   | | _ 
   | |   | (___) || (__/  )| (___) |(_)
   )_(   (_______)(______/ (_______)   
                                       
1) Open ModuleInitializer.cs. Note the documentation at the top.
2) Add initialization logic to the Run method's body.

********************************************************************************

********************************************************************************
 _______  ______   _______          _________
(  ___  )(  ___ \ (  ___  )|\     /|\__   __/
| (   ) || (   ) )| (   ) || )   ( |   ) (   
| (___) || (__/ / | |   | || |   | |   | |   
|  ___  ||  __ (  | |   | || |   | |   | |   
| (   ) || (  \ \ | |   | || |   | |   | |   
| )   ( || )___) )| (___) || (___) |   | |   
|/     \||/ \___/ (_______)(_______)   )_(   
                                             
The InjectModuleInitializer nuget package modifies your project file, adding
an "AfterBuild" target. The target is configured such that, upon a successful
build of your project, it runs the InjectModuleInitializer.exe program. This
program modifies your compiled assembly so that, when it is loaded for the
first time, ModuleInitializer.Run() is called.

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

ASCII art courtesy of patorjk.com/software/taag/#p=display&f=Epic&t=patorjk.com