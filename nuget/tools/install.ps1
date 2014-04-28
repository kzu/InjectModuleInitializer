param($installPath, $toolsPath, $package, $project)

function Get-Relative-Path ($from, $to)
{
	$path = $NULL

	$fromPath = ""
	for ($fromDirectory = New-Object System.IO.DirectoryInfo([System.IO.Path]::GetDirectoryName($from));
		$fromDirectory -ne $NULL;
		$fromDirectory = $fromDirectory.Parent)
	{
		$toPath = ""
		for ($toDirectory = New-Object System.IO.DirectoryInfo($to);
			$toDirectory -ne $NULL;
			$toDirectory = $toDirectory.Parent)
		{
			If ($fromDirectory.FullName.ToLower() -eq $toDirectory.FullName.ToLower())
			{
				$path = $fromPath + $toPath
				break
			}
			
			$toPath = $toDirectory.Name + "\" + $toPath
		}
	
		if ($path -ne $NULL)
		{
			break
		}
		
		$fromPath = $fromPath + "..\"
	}
	
	$path
}

function Check-ModuleInitializer($buildProject)
{
	$moduleInitializers = $buildProject.Items | where {$_.Xml.ItemType -eq "Compile" -and $_.Xml.Include.EndsWith("ModuleInitializer.cs")}
	$count = ($moduleInitializers | measure).Count
	
	if ($count -gt 1)
	{
		Write-Host "ModuleInitializer.cs already exists in another location. Deleting template..."
		$project.ProjectItems.Item("ModuleInitializer.cs").Delete()
	}
}

$relativePath = Get-Relative-Path $project.FileName $toolsPath

Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
$buildProject = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

$target = $buildProject.Xml.AddTarget("InjectModuleInitializer")
$target.AfterTargets = "AfterBuild"
$task = $target.AddTask("Exec")
$task.SetParameter("Command", $relativePath + "InjectModuleInitializer.exe `"`$(TargetPath)`"")

Check-ModuleInitializer($buildProject)

$project.Save()