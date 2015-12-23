using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

//File Operation
using System.IO;

//TFS Dlls - looks for reference 
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Lab.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;


namespace LM
{
    class Program
    {
        static void Main(string[] args)
        {
            //set value for TFS URI
            Uri serverURI;
            String URI = "http://tfs2012d:8080/tfs";
            serverURI = new Uri(URI);

            TfsConfigurationServer serverConnection = new TfsConfigurationServer(serverURI); //setup TFS connection using the server URI
            var catalogService = serverConnection.GetService<ICatalogService>();
            TfsTeamProjectCollection tfsServer = new TfsTeamProjectCollection(TfsTeamProjectCollection.GetFullyQualifiedUriForName("http://tfs2012d:8080/tfs"));     
            LabService labService = (LabService)tfsServer.GetService(typeof(LabService));

            //Get the details from the existing lab setup and store it to a text file
            //Get the list of team project collections on the TFS server
            ReadOnlyCollection<CatalogNode> projectCollections = serverConnection.CatalogNode.QueryChildren(new[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);
            using (System.IO.StreamWriter file = new StreamWriter(@"C:\test\LE.txt", true)) //create a new text file to write the lab details.
            {
                foreach (CatalogNode collection in projectCollections)
                {
                    Guid collectionGuid = new Guid(collection.Resource.Properties["InstanceId"]);
                    TfsTeamProjectCollection collectionObject = serverConnection.GetTeamProjectCollection(collectionGuid);
                    //Get list of team projects in the current collection
                    ReadOnlyCollection<CatalogNode> teamProjects = collection.QueryChildren(new[] { CatalogResourceTypes.TeamProject }, false, CatalogQueryOptions.None);
                    file.WriteLine(collectionObject.Name + "\t" + teamProjects.Count);

                    //Iterate thorugh each of the team projects and write the associated lab deatils to the text file
                    foreach (CatalogNode project in teamProjects)
                    {
                        //Create a lab query spec which will look for Lab Environments which are associated with the current team project.
                        LabEnvironmentQuerySpec querySpec = new LabEnvironmentQuerySpec();
                        querySpec.Project = project.Resource.DisplayName;

                        //create a list of Lab Environments associated with the team project
                        ICollection<LabEnvironment> envList = labService.QueryLabEnvironments(querySpec);

                        file.WriteLine(project.Resource.DisplayName + "\t" + envList.Count);
                        if (envList.Count == 0)
                            continue;

                        //Write details of each env to the text file.
                        foreach (LabEnvironment env in envList)
                        {
                            file.WriteLine(env.Name + "\t" + env.LabSystems.Count + "\t" + env.Description + "\t" + env.FullName + "\t" + env.TestControllerName + "\t" + env.StatusInfo + "\t" + env.RequiresIsolation + "\t" + env.CreatedBy);
                            foreach (LabSystem labSystem in env.LabSystems)
                            {
                                file.WriteLine(labSystem.ComputerName + "\t" + labSystem.VMGuid + "\t" + labSystem.VMName + "\t" + labSystem.Configuration + "\t" + labSystem.ConnectNames + "\t" + labSystem.CustomProperties + "\t" + labSystem.DisplayName + "\t" + labSystem.Environment + "\t" + labSystem.SubStatus +  "\t" + labSystem.Roles);
                            }
                            
                           // env.Destroy(); //This is included for testing purpose, delete the environment after getting the details, next steps will recreat the same environment.

                        }   
                     }
                }
            }
            


            char[] delimiterChars = { '\t' };
            String input;
            String[] inputsplit;
            
            TeamProjectHostGroupQuerySpec hostgroupspec = new TeamProjectHostGroupQuerySpec();

            //Read the data from the text file, set tab as the delimiter.
            using (System.IO.StreamReader file = new StreamReader(@"C:\test\LE.txt", true))
            {
                input = file.ReadLine();
                inputsplit = input.Split(delimiterChars);
                
                String collectionName = inputsplit[0];
                int projectCount = Int32.Parse(inputsplit[1]);
                Console.WriteLine(collectionName + "\t" + projectCount);

                for (int i = 0; i < projectCount; i++ ) //Loop through team projects
                {
                    input = file.ReadLine();
                    inputsplit = input.Split(delimiterChars);
                    
                    String projectName = inputsplit[0];
                    int envCount = Int32.Parse(inputsplit[1]);
                    Console.WriteLine(projectName + "\t" + envCount);

                    hostgroupspec.Project = projectName;
                    ICollection<TeamProjectHostGroup> hostgroupCollection = labService.QueryTeamProjectHostGroups(hostgroupspec); //get the list of host groups.
                    TeamProjectHostGroup[] hostgroup = hostgroupCollection.ToArray();
                    Console.WriteLine(hostgroup[1].Name);

                    VirtualMachineQuerySpec VMSpec = new VirtualMachineQuerySpec(); //Query all the machines on the required hot group
                    VMSpec.Location = hostgroup[1].Uri;
                    ICollection<VirtualMachine> ListofMachines = labService.QueryVirtualMachines(VMSpec); //get the list of machines available on the host group
                    VirtualMachine[] TotalMachine = ListofMachines.ToArray(); 

                    for (int j = 0; j < envCount; j++) //iterate through each environment for the team proejct
                    {
                        
                        input = file.ReadLine();
                        inputsplit = input.Split(delimiterChars);

                        //set the environment variables to create the environments with the same values.
                        String envName = inputsplit[0];
                        int machineCount = Int32.Parse(inputsplit[1]);
                        String description = inputsplit[2];
                        String controllerName = inputsplit[4];
                        String ownerName = inputsplit[7];
                        Console.WriteLine(envName + "\t" + machineCount);
                        
                        List<LabSystemDefinition> sysdef = new List<LabSystemDefinition>(); //create a list of lab system definitions 
                        for (int z = 0; z < machineCount; z++)
                        {
                            input = file.ReadLine();
                            inputsplit = input.Split(delimiterChars);

                            //Set the values for each of the machines within the environment.
                            String computerName = inputsplit[0];
                            String vmName = inputsplit[2];
                            String role = inputsplit[9];
                            String displayName = inputsplit[6];
                            Console.WriteLine(displayName + "\t" + role);
                               
                            foreach(VirtualMachine machine in TotalMachine) //iterate throught the machines available on the host group and get the one with the same name.
                            {
                                Console.WriteLine(displayName + "\t" + machine.VMName);
                                Console.WriteLine(machine.VMName.Equals(displayName));
                                if(machine.VMName.Equals(displayName)) //match VM name, set the sepcs and add it to the list of machines to be part of the env
                                {
                                    LabSystemDefinition firstsysdef = new LabSystemDefinition(machine, machine.VMName, role);
                                    firstsysdef.SourceType = Microsoft.TeamFoundation.Lab.Client.LabSystemSourceType.VirtualMachine;
                                    firstsysdef.ConfigurationComplete = true;
                                    firstsysdef.SourceIdentifier = TotalMachine[0].VMGuid.ToString(); 
                                    sysdef.Add(firstsysdef); //Add the machine to the Lab System
                                }
                            }    
                        }
                        
                        //create new LabEnvDef from the Lab system definition already created.
                        LabEnvironmentDefinition labdef = new LabEnvironmentDefinition(envName, description, sysdef);
                        labdef.TestControllerName = controllerName;

                        //create the lab env.
                        LabEnvironment Le = hostgroup[1].CreateLabEnvironment(labdef, null, null);    
                    }
                }     
            }
        
            Console.ReadLine();


        }

    }
}
