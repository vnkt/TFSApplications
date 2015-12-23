function UpdateVersion
{
	param([int]$SystemId, [string]$newversion)
	
	Write-Output "Version is $newversion"

	#Connect to the database
	# Load the ODP assembly 
	$oracle=[Reflection.Assembly]::LoadFile("D:\oracle\product\10.2.0\client_x86\odp.net\bin\2.x\Oracle.DataAccess.dll")

	#connect to Oracle 
	$constr = "User Id=draxsystems;Password=draxsystems;Data Source=drxtrp.draxpower.com"
	$conn= New-Object Oracle.DataAccess.Client.OracleConnection($constr)
	$result=$conn.Open()

	# Create a datareader for a SQL statement 
	$sql="SELECT version, version_status_id, tfs_uri FROM app_version WHERE VERSION = (SELECT max(version) FROM  app_version WHERE app_id = $SystemId AND version >= '$newversion') AND app_id = $SystemId"
	$command = New-Object Oracle.DataAccess.Client.OracleCommand($sql,$conn)
	$reader=$command.ExecuteReader()

	switch ($TfsDeployerBuildDetail.Quality)
	{
		"Testing" {
			$versionstatus = 4
		}
		"Released" {
			$versionstatus = 1
		}
		default {
			$versionstatus = 3
		}
	}

	$tfs_uri = $TfsDeployerBuildDetail.Uri
	if ($reader.read()) {
		$dbversion=$reader.GetString(0)  
		$dbversionstatus=$reader.GetDecimal(1)
		$dbtfs_uri=$reader.GetString(2)
		
		#version check - See if there are later version already being released
		if ($dbversion -gt $newversion) {
			throw (new-object System.Exception("Later version has already being created. This version = $newversion, latest created version = $dbversion"))
		}
		#build check - See if this version is already associated with a different TFS build
		if ($dbtfs_uri -ne $null) {
			if (($tfs_uri -ne $dbtfs_uri) -and ($dbversion -eq $newversion)) {
				throw (New-Object System.Exception("This version is already linked to a different TFS Build. This build = $tfs_uri, existing build is $dbtfs_uri"))
			}
		}
		#status check - See if this version has already been released into live
		if (($dbversionstatus -eq 1) -and ($versionstatus -ne 1)) {
			#Revert it back to Live via rejected to stop a re-release
			$TfsDeployerBuildDetail.Quality = "Rejected"
			$TfsDeployerBuildDetail.Save()
			
			$TfsDeployerBuildDetail.Quality = "Released"
			$TfsDeployerBuildDetail.Save()
			
			throw (new-object System.Exception("This version has already been released, can't change it's status back to anything else"))
		}
		
		$sql="UPDATE app_version SET VERSION_STATUS_ID = $versionstatus, tfs_uri = '$tfs_uri' WHERE APP_ID = $SystemId AND version = '$newversion'"
	} else {
		$sql="INSERT INTO app_version (VERSION, VERSION_STATUS_ID, APP_ID, tfs_uri) VALUES ('$newversion', $versionstatus, $SystemId, '$tfs_uri')"
	}
	$reader.Close()

	$command = New-Object Oracle.DataAccess.Client.OracleCommand( $sql,$conn)
	$result=$command.ExecuteNonQuery()
}

function UpdateClickOnceVersion
{
	param ([int]$SystemId)

	#Locate all .application files
	cd $TfsDeployerBuildDetail.DropLocation

	$appfiles = Get-ChildItem -recurse -include *.application
	if ($appfiles -eq $Null)
	{
		Write-Output "No Application files found in deployment "
	} else {
		#Look in each config file for a system id
		foreach ($appfile in $appfiles)
		{	
			#Load Config File into Variable
			$config = [xml] (get-content $appfile.fullname)

			#Get the system id
			$newversion = $config.assembly.assemblyIdentity.GetAttribute("version") 
			
			UpdateVersion -SystemId $SystemId -newversion $newversion
		}
	}
}

function ProcessSystemId
{
	param ([int]$SystemId, [int]$Version)

	Write-Output "SystemId is $SystemId "

	#Connect to the database
	# Load the ODP assembly 
	$oracle=[Reflection.Assembly]::LoadFile("D:\oracle\product\10.2.0\client_x86\odp.net\bin\2.x\Oracle.DataAccess.dll")

	#connect to Oracle 
	$constr = "User Id=draxsystems;Password=draxsystems;Data Source=drxtrp.draxpower.com"
	$conn= New-Object Oracle.DataAccess.Client.OracleConnection($constr)
	$conn.Open()

	# Create a datareader for a SQL statement 
	switch ($TfsDeployerBuildDetail.Quality)
	{
		"Testing" {
			$Environment = 3
			$EnvironmentName = "Test"
		}
		"Released"{
			$Environment = 1
			$EnvironmentName = "Live"
		}
	}
	$sql="SELECT D.TYPE, d.location FROM app_deployment d WHERE d.app_id = $SystemId AND d.environment_id = $Environment"
	$command = New-Object Oracle.DataAccess.Client.OracleCommand( $sql,$conn)
	$reader=$command.ExecuteReader()

	# Write out the results 
	if ($reader.HasRows -eq $FALSE)
	{
		Write-Output "No release configured for the $EnvironmentName environment for system $SystemId"
		throw (new-object System.Exception("No release configured for the $EnvironmentName environment for system $SystemId"))
	}
	while ($reader.read()) {
		$DeployType=[int]$reader.GetDecimal(0)
		$Location=$reader.GetString(1)

		Write-Output "Releasing to the $EnvironmentName Environment"

		switch ($DeployType)
		{
			#ClickOnce
			1 {
				Write-Output "Click Once Deployment"

				UpdateClickOnceVersion -SystemId $SystemId
				
				$Source = Join-Path -Path $TfsDeployerBuildDetail.DropLocation -ChildPath Publish\*

				Write-Output "Releasing files to $Location"
				$Directory = New-Item -ItemType Directory -Path $Location -Force

				try { 
					Copy-Item $Source $Location -Recurse -Force
				} catch {
					#Just ignore the error
				}
			
				Write-Output "Release Complete"
			}

			#File Copy
			2 {
				Write-Output "File Copy"
			
				UpdateVersion -SystemId $SystemId -newversion $Version 
				
				$Source = Join-Path -Path $TfsDeployerBuildDetail.DropLocation -ChildPath *

				Write-Output "Releasing files to $Location"
				$Directory = New-Item -ItemType Directory -Path $Location -Force

				try { 
					Copy-Item $Source $Location -Recurse -Force
				} catch {
					#Just ignore the error
				}

			}
			default {
				Write-Output "Unrecognised"
			}
		}

		if ($TfsDeployerBuildDetail.Quality -eq "Released")
		{
			#Save the build
			if ($TfsDeployerBuildDetail -ne $null)
			{
				if ($TfsDeployerBuildDetail.KeepForever -ne $TRUE)
				{
					Write-Output "As this is a live release, the Build has been marked as Keep Forever."
					$TfsDeployerBuildDetail.KeepForever = $TRUE
					$TfsDeployerBuildDetail.Save()
				}
			}
		} 
	}
}
#Locate all .config files
cd $TfsDeployerBuildDetail.DropLocation

$files = Get-ChildItem -recurse -include *.config.deploy
if ($files -eq $Null)
{
	Write-Output "No Config files found in deployment "
} else {
	#Look in each config file for a system id
	foreach ($file in $files)
	{	
		$checkFile = $file.name
		Write-Output "Checking file $checkFile"

		#Load Config File into Variable
		$config = [xml] (get-content $file.fullname)

		#Get the system id
		$config.selectNodes('//setting') | % {set $_.name $_.value}
		if ($SystemId -eq $Null)
		{
			Write-Output "No SystemId found in the config file"
		} else {
			ProcessSystemId -SystemId $SystemId
		}
	}
}

#Locate all .xlsx files
cd $TfsDeployerBuildDetail.DropLocation

$files = Get-ChildItem -recurse -include *.xlsx
if ($files -eq $Null)
{
	Write-Output "No Excel files found in deployment "
} else {
	#Look in each config file for a system id
	foreach ($file in $files)
	{	
		$checkFile = $file.name
		Write-Output "Checking file $checkFile"

		#Load Config File into Variable
		try {
			$excel = new-object -comobject Excel.Application
			$excelfile = $excel.Workbooks.Open($file)

			$excelsheet = $excelfile.Worksheets.Item("System")
			$SystemId = $excelsheet.Cells.Item(2,3).Value2
			$Version = $excelsheet.Cells.Item(3,3).Value2
		} finally {		
			$excelsheet = $null
			if ($excelfile -ne $null) {
				$excelfile.Close()
				$excelfile = $null
			}
			if ($excel -ne $null) {
				$excel.Quit()
				$excel = $null
			}
		}
		
		#Get the system id
		if ($SystemId -eq $Null)
		{
			Write-Output "No SystemId found in the spreadsheet file"
		} else {
			ProcessSystemId -SystemId $SystemId -Version $Version
		}
	}
}


