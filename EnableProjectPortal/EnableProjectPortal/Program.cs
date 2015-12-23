# Query the host groups 
$hostGroupQuerySpec = New-Object Microsoft.TeamFoundation.Lab.Client.TeamProjectHostGroupQuerySpec; 
$hostGroupQuerySpec.Project = $projectName; 
$hostGroupList = $labService.QueryTeamProjectHostGroups($hostGroupQuerySpec);

# Pick the first host group 
$hostGroup = $hostGroupList[0];

# Query the virtual machines 
$machineQuerySpec = New-Object Microsoft.TeamFoundation.Lab.Client.VirtualMachineQuerySpec; 
$machineQuerySpec.Location = $hostGroup.Uri; 
$machineQuerySpec.FilterAlreadyImportedVirtualMachines = $TRUE; 
$virtualMachines = $labService.QueryVirtualMachines($machineQuerySpec);

# Pick the first virtual machine 
$virtualMachine = $virtualMachines[0];

# Create lab system definition 
$lsName = "machine1"; 
$roleName = "webRole"; 
$labSystemDefinition = New-Object Microsoft.TeamFoundation.Lab.Client.LabSystemDefinition($lsName,$lsName,$roleName); 
$labSystemDefinition.SourceIdentifier = $virtualMachine.VMGuid.ToString(); 
$labSystemDefinition.SourceType = [Microsoft.TeamFoundation.Lab.Client.LabSystemSourceType]::VirtualMachine; 
$labSystemDefinition.ConfigurationComplete = $TRUE;

# Create lab environment definition 
$leName = "aseemEnvironment"; 
$leDescription = "This is Aseem's environment"; 
$labSystemDefinitions = [array]$labSystemDefinition 
$labEnvironmentDefinition = New-Object Microsoft.TeamFoundation.Lab.Client.LabEnvironmentDefinition -ArgumentList @(,$leName, $leDescription, $labSystemDefinitions); 
$labEnvironmentDefinition.TestControllerName = $testControllerName;

# Create the environment 
$hostGroup.CreateLabEnvironment($labEnvironmentDefinition, $NULL, $NULL);