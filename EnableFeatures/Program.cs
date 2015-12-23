using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Integration.Server;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Server.WebAccess.WorkItemTracking.Common;


namespace FeatureEnablement
{
    class Program
    {
        static void Main(string[] args)
        {
            string urlToCollection = @"http://vnkt-server:8080/tfs/TFS2015";
            Guid instanceId;

            // Get the TF Request Context
            using (DeploymentServiceHost deploymentServiceHost = GetDeploymentServiceHost(urlToCollection, out instanceId))
            {
                using (TeamFoundationRequestContext context = GetContext(deploymentServiceHost, instanceId))
                {
                    // For each team project in the collection
                    CommonStructureService css = context.GetService<CommonStructureService>();
                    foreach (var project in css.GetWellFormedProjects(context))
                    {
                        // Run the 'Configuration Features Wizard'
                        ProvisionProjectFeatures(context, project);
                    }
                }
            }
        }

        private static DeploymentServiceHost GetDeploymentServiceHost(string urlToCollection, out Guid instanceId)
        {
            using (var teamProjectCollection = new TfsTeamProjectCollection(new Uri(urlToCollection), CredentialCache.DefaultCredentials))
            {
                const string connectionStringPath = "/Configuration/Database/Framework/ConnectionString";
                var registry = teamProjectCollection.ConfigurationServer.GetService<Microsoft.TeamFoundation.Framework.Client.ITeamFoundationRegistry>();
                string connectionString = registry.GetValue(connectionStringPath);
                instanceId = teamProjectCollection.InstanceId;

                // Get the system context
                TeamFoundationServiceHostProperties deploymentHostProperties = new TeamFoundationServiceHostProperties();
                deploymentHostProperties.ConnectionString = connectionString;
                deploymentHostProperties.HostType = TeamFoundationHostType.Application | TeamFoundationHostType.Deployment;
                return new DeploymentServiceHost(deploymentHostProperties, false);
            }
        }

        private static TeamFoundationRequestContext GetContext(DeploymentServiceHost deploymentServiceHost, Guid instanceId)
        {
            using (TeamFoundationRequestContext deploymentRequestContext = deploymentServiceHost.CreateSystemContext())
            {
                // Get the identity for the tf request context
                TeamFoundationIdentityService ims = deploymentRequestContext.GetService<TeamFoundationIdentityService>();
                TeamFoundationIdentity identity = ims.ReadRequestIdentity(deploymentRequestContext);

                // Get the tf request context
                TeamFoundationHostManagementService hostManagementService = deploymentRequestContext.GetService<TeamFoundationHostManagementService>();

                return hostManagementService.BeginUserRequest(deploymentRequestContext, instanceId, identity.Descriptor);
            }
        }

        private static void ProvisionProjectFeatures(TeamFoundationRequestContext context, CommonStructureProjectInfo project)
        {
            // Get the Feature provisioning service ("Configure Features")
            ProjectFeatureProvisioningService projectFeatureProvisioningService = context.GetService<ProjectFeatureProvisioningService>();

            if (projectFeatureProvisioningService.GetFeatures(context, project.Uri.ToString()).Where(f => (f.State == ProjectFeatureState.NotConfigured && !f.IsHidden)).Any())
            {
                // When the team project is already fully or partially configured, report it
                Console.WriteLine("{0}: Project is up to date.", project.Name);
            }
            else
            {
                // find the valid process templates
                IEnumerable<IProjectFeatureProvisioningDetails> projectFeatureProvisioningDetails = projectFeatureProvisioningService.ValidateProcessTemplates(context, project.Uri);

                int validProcessTemplateCount = projectFeatureProvisioningDetails.Where(d => d.IsValid).Count();

                if (validProcessTemplateCount == 0)
                {
                    // when there are no valid process templates found
                    Console.WriteLine("{0}: No valid process template found!");
                }
                else if (validProcessTemplateCount == 1)
                {
                    // at this point, only one process template without configuration errors is found
                    // configure the features for this team project
                    IProjectFeatureProvisioningDetails projectFeatureProvisioningDetail = projectFeatureProvisioningDetails.ElementAt(0);
                    projectFeatureProvisioningService.ProvisionFeatures(context, project.Uri.ToString(), projectFeatureProvisioningDetail.ProcessTemplateId);

                    Console.WriteLine("{0}: Configured using settings from {1}.", project.Name, projectFeatureProvisioningDetail.ProcessTemplateName);
                }
                else if (validProcessTemplateCount > 1)
                {
                    // when multiple process templates found that closely match, report it
                    Console.WriteLine("{0}: Multiple valid process templates found!", project.Name);
                }
            }
        }
    }
}
