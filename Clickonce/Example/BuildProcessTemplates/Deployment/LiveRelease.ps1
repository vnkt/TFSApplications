# this is how Script Parameters defined in the DeploymentMappings.xml are accessed in PowerShell:

if ($ProductionServerName -eq "DPS980") { Write-Output "Script parameter received!" ; }
