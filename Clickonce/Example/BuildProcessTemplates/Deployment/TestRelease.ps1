# this script merely demonstrates the information available to a script run by Tfs Deployer

# This is the TFS 2008 build information structure:
$TfsDeployerBuildDetail | Format-List -Property * ;
#Get-ChildItem Variable:TfsDeployer*


# This is the old TFS 2005 obsoleted structure (still supported):
#$TfsDeployerBuildData | Format-List -Property * ;



