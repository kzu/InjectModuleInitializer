********************************************************************************
_________ _______  ______   _______    
\__   __/(  ___  )(  __  \ (  ___  )   
   ) (   | (   ) || (  \  )| (   ) | _ 
   | |   | |   | || |   ) || |   | |(_)
   | |   | |   | || |   | || |   | |   
   | |   | |   | || |   ) || |   | | _ 
   | |   | (___) || (__/  )| (___) |(_)
   )_(   (_______)(______/ (_______)   
                                       
1) Add the following class to your project:

internal static class ModuleInitializer
{
    internal static void Run()
    {
        // TODO: Add assembly initialization logic.
    }
}

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

This program is licensed under the MIT license: http://opensource.org/licenses/MIT

********************************************************************************

ASCII art courtesy of patorjk.com/software/taag/#p=display&f=Epic&t=patorjk.com