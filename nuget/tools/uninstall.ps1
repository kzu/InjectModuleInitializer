param($installPath, $toolsPath, $package, $project)

Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
$buildProject = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1
$projectRoot = $buildProject.Xml;

foreach ($target in $projectRoot.Targets)
{
	if ($target.Name -eq "InjectModuleInitializer")
	{
		$projectRoot.RemoveChild($target);
	}
}

$project.Save()