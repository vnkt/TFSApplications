using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Server = Microsoft.TeamFoundation.Server.WebAccess.WorkItemTracking.Common;

namespace features4tfs
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> keyValueArguments;

            if (args == null || args.Length == 0 || args[0].Equals("/?") || args[0].Equals("-?") || !Parse(args, out keyValueArguments))
            {
                PrintUsage();
                return;
            }

            try
            {
                Console.WriteLine();

                string tpcUrl = keyValueArguments["tpcUrl"];
                TfsTeamProjectCollection tfsCollection = new TfsTeamProjectCollection(new Uri(tpcUrl));

                string configDbConnectionString = string.Empty;

                if (keyValueArguments.ContainsKey("webConfig"))
                    configDbConnectionString = GetConfigDbConnectionStringFromWebConfig(Path.Combine(keyValueArguments["webConfig"]));
                if (keyValueArguments.ContainsKey("db"))
                    configDbConnectionString = keyValueArguments["db"];

                Console.WriteLine("Loading team project list...");
                WorkItemStore witStore = tfsCollection.GetService<WorkItemStore>();

                if (witStore.Projects.Count == 0)
                {
                    Console.WriteLine("No projects found.");
                    return;
                }

                using (DeploymentServiceHost deploymentServiceHost = CreateDeploymentServiceHost(configDbConnectionString))
                {
                    // For each team project in the collection
                    foreach (Project project in witStore.Projects)
                    {
                        RunFeatureEnablement(deploymentServiceHost, project, tfsCollection.InstanceId);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static DeploymentServiceHost CreateDeploymentServiceHost(string configDbConnectionString)
        {            
            Console.WriteLine("Creating deployment service host...");

            TeamFoundationServiceHostProperties deploymentHostProperties = new TeamFoundationServiceHostProperties();
            deploymentHostProperties.HostType = TeamFoundationHostType.Deployment | TeamFoundationHostType.Application;
            return new DeploymentServiceHost(deploymentHostProperties, SqlConnectionInfoFactory.Create(configDbConnectionString, null, null), true);
        }

        private static TeamFoundationRequestContext CreateServicingContext(DeploymentServiceHost deploymentServiceHost, Guid instanceId)
        {
            Console.WriteLine("\tCreating servicing context...");

            using (TeamFoundationRequestContext requestContext = deploymentServiceHost.CreateSystemContext(true))
            {
                TeamFoundationHostManagementService host = requestContext.GetService<TeamFoundationHostManagementService>();
                return host.BeginRequest(requestContext, instanceId, RequestContextType.ServicingContext);
            }
        }
        
        private static string GetConfigDbConnectionStringFromWebConfig(string pathWebConfig)
        {
            Console.WriteLine("Loading web.config...");

            if (!File.Exists(pathWebConfig))
            {
                throw new FileNotFoundException("web.config not found.", pathWebConfig);
            }

            XmlDocument webConfig = new XmlDocument();
            webConfig.Load(pathWebConfig);
            XmlNode dbNode = webConfig.SelectSingleNode("/configuration/appSettings/add[@key='applicationDatabase']/@value");
            if (dbNode == null || string.IsNullOrEmpty(dbNode.Value))
            {
                throw new InvalidDataException(string.Format("{0} doesn't have applicationDatabase or value is not set. This is a invalid web.config", pathWebConfig));
            }

            return dbNode.Value;
        }

        private static void RunFeatureEnablement(DeploymentServiceHost deploymentServiceHost, Project project, Guid instanceId)
        {
            try
            {
                Console.WriteLine("Running feature enablement for '{0}'", project.Name);

                using (TeamFoundationRequestContext context = CreateServicingContext(deploymentServiceHost, instanceId))
                {
                    // Run the 'Configuration Features Wizard'  
                    ProvisionProjectFeatures(context, project);
                }

                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Feature enablement failed for project '{0}': {1}", project.Name, ex);
                Console.ReadLine();
            }
        }

        private static void ProvisionProjectFeatures(TeamFoundationRequestContext context, Project project)
        {
            // Get the Feature provisioning service ("Configure Features")
            var projectFeatureProvisioningService = context.GetService<Server.ProjectFeatureProvisioningService>();
            

             Console.WriteLine("1");

            /*if (!projectFeatureProvisioningService.GetFeatures(context, project.Uri.ToString()).Where(f => (f.State == Server.ProjectFeatureState.NotConfigured && !f.IsHidden)).Any())
            {
                Console.WriteLine("true");
                // When the team project is already fully or partially configured, report it
                Console.WriteLine("\t{0}: Project is up to date.", project.Name);
            }
            else
            {*/
                Console.WriteLine("false");
                Console.WriteLine("2");
                // Find valid process templates
                var projectFeatureProvisioningDetails = projectFeatureProvisioningService.ValidateProcessTemplates(context, project.Uri.ToString());
                var validProcessTemplateDetails = projectFeatureProvisioningDetails.Where(d => d.IsValid);

                Console.WriteLine("3");
                switch (validProcessTemplateDetails.Count())
                {
                    case 0:
                        Console.WriteLine("\t{0}: No valid process templates found.", project.Name);
                        break;
                    case 1:                        
                        var projectFeatureProvisioningDetail = projectFeatureProvisioningDetails.ElementAt(0);

                        Console.WriteLine("\tConfiguring using settings from '{0}'", projectFeatureProvisioningDetail.ProcessTemplateDescriptorName);

                        projectFeatureProvisioningService.ProvisionFeatures(context, project.Uri.ToString(), projectFeatureProvisioningDetail.ProcessTemplateDescriptorId);
                        Console.WriteLine("\t{0}: Done.", project.Name);
                        break;
                    default:
                        // When multiple process templates are found, report it
                        Console.WriteLine("\t{0}: Multiple valid process templates found.", project.Name);

                        foreach (var templateDetails in validProcessTemplateDetails)
                        {
                            Console.WriteLine("\t{0}", templateDetails.ProcessTemplateDescriptorName);
                        }
                        
                        break;
                }
           //}
        }

        private static bool Parse(string[] args, out Dictionary<string, string> keyValueArguments)
        {
            keyValueArguments = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("/c:", StringComparison.OrdinalIgnoreCase))
                {
                    keyValueArguments["tpcUrl"] = args[i].Substring(3);
                }
                else if (args[i].StartsWith("/w:", StringComparison.OrdinalIgnoreCase))
                {
                    keyValueArguments["webConfig"] = args[i].Substring(3);
                }
                else if (args[i].StartsWith("/d:", StringComparison.OrdinalIgnoreCase))
                {
                    keyValueArguments["db"] = args[i].Substring(3);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static void PrintUsage()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("features4tfs");
            sb.AppendLine();
            sb.AppendLine("This tool allows you to run Configure Features wizard for the whole");
            sb.AppendLine("team project collection at once instead of doing it manually for each");
            sb.AppendLine("individual project.");
            sb.AppendLine();
            sb.AppendLine("Usage: features4tfs /c:<collectionUrl> [/d:<connectionString>] [/w:<webConfigPath>]");
            sb.AppendLine();
            sb.AppendLine("/c:\t\tTFS collection Url.");
            sb.AppendLine("/d:\t\tTFS configuration DB connection string.");
            sb.AppendLine("/w:\t\tPath to TFS AT web.config.");
            sb.AppendLine();
            sb.AppendLine(@"Examples:");
            sb.AppendLine(@"(TFS 2012)      features4tfs /c:http://localhost:8080/tfs/DefaultCollection");
            sb.AppendLine(@"(TFS 2013/2015) features4tfs /c:http://localhost:8080/tfs/DefaultCollection /d:""Data Source=.;Initial Catalog=Tfs_Configuration;Integrated Security=True""");

            Console.Write(sb.ToString());
        }
    }
}
