﻿param(
	# Getting the control percentage as an argument
	[int] $desiredCodeCoveragePercent = 95
)

Write-Host "Desired Code Coverage Percent is " -nonewline; Write-Host $desiredCodeCoveragePercent

# Load Assemblies we use. Make sure you change the version as per need. 
[Reflection.Assembly]::Load(“Microsoft.TeamFoundation.Client, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a”) | Out-Null  
[Reflection.Assembly]::Load(“Microsoft.TeamFoundation.TestManagement.Client, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a”) | Out-Null
[Reflection.Assembly]::Load(“Microsoft.TeamFoundation.TestManagement.Common, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a”) | Out-Null
[Reflection.Assembly]::Load("Microsoft.TeamFoundation.Build.Client,Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a”) | Out-Null

# Getting a few environment variables we need
[String] $CollectionUrl = "$env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI"
[String] $BuildUrl = "$env:BUILD_BUILDURI"
[String] $project = "$env:SYSTEM_TEAMPROJECT"

[int] $coveredBlocks = 0
[int] $skippedBlocks = 0
[int] $totalBlocks = 0
[int] $codeCoveragePercent = 0


# Get the Team Project Collection, then test and build service
$teamProjectCollection = [Microsoft.TeamFoundation.Client.TfsTeamProjectCollectionFactory]::GetTeamProjectCollection($CollectionUrl)
$tcmService = $teamProjectCollection.GetService([Microsoft.TeamFoundation.TestManagement.Client.ITestManagementService])
$buildServer = $teamProjectCollection.GetService([Microsoft.TeamFoundation.Build.Client.IBuildServer])
[Microsoft.TeamFoundation.TestManagement.Client.ITestManagementTeamProject] $tcmProject = $tcmService.GetTeamProject($project);


# Getting all the test run as part of the build 
$totalRuns = $tcmProject.TestRuns.ByBuild($BuildUrl)

# Getting Code coverage class details
[Microsoft.TeamFoundation.TestManagement.Client.ICoverageAnalysisManager] $coverageAnalysisManager = $tcmProject.CoverageAnalysisManager

# Looping through all the results
 foreach ($currentRun in $totalRuns)
 {
	 $sourceBlocks = $coverageAnalysisManager.QueryTestRunCoverage($currentRun.Id, 1)
	 foreach ($currentBlock in $sourceBlocks)
	 {
		 $modules = $currentBlock.Modules
		 foreach($module in $modules)
		 {
				$coveredBlocks += $module.Statistics.BlocksCovered
				$skippedBlocks += $module.Statistics.BlocksNotCovered  
		 }

	 }
 }

 $totalBlocks = $coveredBlocks + $skippedBlocks;

 if ($totalBlocks -eq 0)
{
	$codeCoveragePercent = 0
	Write-Host $codeCoveragePercent -nonewline; Write-Host " is the Code Coverage. Failing the build"
	exit -1

}

$codeCoveragePercent = $coveredBlocks * 100.0 / $totalBlocks
Write-Host "Code Coverage percentage is " -nonewline; Write-Host $codeCoveragePercent


if ($codeCoveragePercent -le $desiredCodeCoveragePercent)
{
	Write-Host "Failing the build as CodeCoverage limit not met"
	exit -1
}
Write-Host "CodeCoverage limit met"