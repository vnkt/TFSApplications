//Set the location manually
$Location = "\\dave\ClickOnce"
//Get the Source Path
$Source = Join-Path -Path $TfsDeployerBuildDetail.DropLocation -ChildPath Publish\*

//Create the Directory
$Directory = New-Item -ItemType Directory -Path $Location -Force

//Copy the directories
try { 
	Copy-Item $Source $Location -Recurse -Force
} catch {
	#Just ignore the error
}