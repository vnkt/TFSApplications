using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Navigation;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System.Collections.ObjectModel;

namespace TFSsite
{
    class Program
    {
        static void Main(string[] args)
        {
           
            try 
            {
                SPSecurity.RunWithElevatedPrivileges(
                delegate
                {

                    SPWebCollection SPList = null;
                    Guid ownedWeb = new Guid();
                    Guid projectId = new Guid();

                    foreach (SPWebApplication webApp in SPWebService.ContentService.WebApplications)
                    {
                        foreach (SPSite siteCollection in webApp.Sites)
                        {
                           SPList = siteCollection.RootWeb.GetSubwebsForCurrentUser();

                           foreach (SPWeb web in SPList)
                               Console.WriteLine("hello " + web.Url + " " + web.ServerRelativeUrl);
                        }
                    }

                    String URI;
                    URI = "http://shr-tfs2013-5:8080/tfs";
                    Uri serverURI = new Uri(URI);
                    TfsConfigurationServer serverConnection = new TfsConfigurationServer(serverURI);
                    var catalogService = serverConnection.GetService<ICatalogService>();

                    // Get SP WebApp Resource, so that we can add the dependency later
                    var sharepointWebAppResources = catalogService.QueryResourcesByType(new Guid[] { CatalogResourceTypes.SharePointWebApplication }, CatalogQueryOptions.None);

                    ReadOnlyCollection<CatalogNode> projectCollections = serverConnection.CatalogNode.QueryChildren(new[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);
                    foreach (CatalogNode collection in projectCollections)
                    {
                        Guid collectionGuid = new Guid(collection.Resource.Properties["InstanceId"]);
                        TfsTeamProjectCollection collectionObject = serverConnection.GetTeamProjectCollection(collectionGuid);

                        ReadOnlyCollection<CatalogNode> teamProjects = collection.QueryChildren(new[] { CatalogResourceTypes.TeamProject }, false, CatalogQueryOptions.None);
                        int found = 0;
                        String URL = null;

                        foreach (CatalogNode project in teamProjects)
                        {

                            foreach (SPWeb oWebsite in SPList)
                            {
                                if (oWebsite.Url.Contains(project.Resource.DisplayName))
                                {
                                    found = 1;
                                    URL = oWebsite.ServerRelativeUrl;
                                    ownedWeb = oWebsite.ID;
                                    break;
                                }
                            }

                            if (found == 0)
                                continue;

                            Console.WriteLine(project.Resource.DisplayName);
                            projectId = project.Resource.Identifier;
                            var projectPortalNodes = project.QueryChildren(new Guid[] { CatalogResourceTypes.ProjectPortal }, true, CatalogQueryOptions.ExpandDependencies);
                            CatalogNode projectPortalNode = null;

                            if (projectPortalNodes.Count > 0)
                                projectPortalNode = projectPortalNodes[0];
                            else
                                projectPortalNode = project.CreateChild(CatalogResourceTypes.ProjectPortal, "Project Portal");

                            projectPortalNode.Resource.Properties["ResourceSubType"] = "WssSite";
                            projectPortalNode.Resource.Properties["RelativePath"] = URL;
                            projectPortalNode.Resource.Properties["OwnedWebIdentifier"] = ownedWeb.ToString();

                            projectPortalNode.Dependencies.SetSingletonDependency("ReferencedResource", sharepointWebAppResources[0].NodeReferences[0]);

                            catalogService.SaveNode(projectPortalNode);

                        }
                        Console.WriteLine(" Entereted ");
                    }
                }
                ); 
            }
            catch (Exception e)
            {
                Console.WriteLine("exception" + e);
            }
          
        } 
    }
}
