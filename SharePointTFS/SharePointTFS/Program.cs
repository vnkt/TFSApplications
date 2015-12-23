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
            SPWeb newWeb = null;
            SPSite objSiteCollection = null;
            String projectName = null, serverURL = null, spURL = null;
            Guid ownedWeb = new Guid();
            Guid projectId = new Guid();

            projectName = Console.ReadLine();
            serverURL = "http://localhost:8080/tfs";

            try 
            { 
                SPSecurity.RunWithElevatedPrivileges( 
                delegate 
                { 
                    string webApplicationUrl = "http://localhost/"; //web Application URL 
                    string parentSiteUrl = "/sites/Defaultcollection"; //URL under which to create sub site 

                    spURL = "/sites/Defaultcollection/"+projectName; //new sub site to add 
                    string siteTitle = projectName; 
                    string siteDescription = "test team project"; 
                    uint languageCode = 1033; //1033 is the code for english 
                    string siteTemplate = "Microsoft.TeamFoundation.SharePoint.Dashboards#0"; 
 
                    parentSiteUrl = parentSiteUrl.TrimStart('/'); 
                    string requestUrl = String.Concat(webApplicationUrl, parentSiteUrl);

                    //creating the new site
                    using (objSiteCollection = new SPSite(requestUrl)) 
                    {
                        newWeb = objSiteCollection.AllWebs.Add(spURL, siteTitle, siteDescription, languageCode, siteTemplate, false, false);
                       ownedWeb = newWeb.ID;
                    } 
                 }
                ); 
            } 
            catch (Exception ex) 
            {
                 Console.WriteLine(ex);
             }

            //adding the new site to navigation pane on the root
            if (newWeb != null)
            {
                newWeb.Navigation.UseShared = true;
                SPNavigationNode node = new SPNavigationNode(newWeb.Title, newWeb.ServerRelativeUrl);
                bool parentInheritsTopNav = newWeb.ParentWeb.Navigation.UseShared;

                if (parentInheritsTopNav)
                    objSiteCollection.RootWeb.Navigation.TopNavigationBar.AddAsLast(node);
                else
                    newWeb.ParentWeb.Navigation.TopNavigationBar.AddAsLast(node);
             
                newWeb.Dispose();
            }

             //Getting project details
            Uri serverURI;
            try
            {
                String URI;
                URI = "http://localhost:8080/tfs";
                serverURI = new Uri(URI);
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
                    foreach (CatalogNode project in teamProjects)
                    {
                       Console.WriteLine(project.Resource.DisplayName);
                       if (project.Resource.DisplayName == projectName)
                       {
                           projectId = project.Resource.Identifier;
                           var projectPortalNodes = project.QueryChildren(new Guid[] { CatalogResourceTypes.ProjectPortal }, true, CatalogQueryOptions.ExpandDependencies);
                           CatalogNode projectPortalNode = null;

                           if (projectPortalNodes.Count > 0)
                               projectPortalNode = projectPortalNodes[0];
                           else
                               projectPortalNode = project.CreateChild(CatalogResourceTypes.ProjectPortal, "Project Portal");
              
                           projectPortalNode.Resource.Properties["ResourceSubType"] = "WssSite";
                           projectPortalNode.Resource.Properties["RelativePath"] = spURL;
                           projectPortalNode.Resource.Properties["OwnedWebIdentifier"] = ownedWeb.ToString();

                           projectPortalNode.Dependencies.SetSingletonDependency("ReferencedResource", sharepointWebAppResources[0].NodeReferences[0]);

                           catalogService.SaveNode(projectPortalNode);
                       }
                    }
                    Console.WriteLine(" Entereted ");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("exception" + e);
            }
        }
    }
}
