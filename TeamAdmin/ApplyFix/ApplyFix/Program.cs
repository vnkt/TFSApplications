using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System.Collections.ObjectModel;

namespace ApplyFix
{
    class Program
    {

        public static void ExecuteCommand(string command)
        {
            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process process;

            //path where you have the batch file
            ProcessInfo = new ProcessStartInfo("C:\\Users\\venkatn\\Desktop\\Gabriel\\PreFix.bat", command);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            //path where you have the batch file
            ProcessInfo.WorkingDirectory = "C:\\Users\\venkatn\\Desktop\\Gabriel\\";
            // *** Redirect the output ***
            ProcessInfo.RedirectStandardError = true;
            ProcessInfo.RedirectStandardOutput = true;

            process = Process.Start(ProcessInfo);
            process.WaitForExit();

            // *** Read the streams ***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            ExitCode = process.ExitCode;


            process.Close();
        }


        static void Main(string[] args)
        {
            String URI = "http://vnkt-server:8080/tfs";
            Uri serverURI = new Uri(URI);

            TfsConfigurationServer serverConnection = new TfsConfigurationServer(serverURI); //setup TFS connection using the server URI
            var catalogService = serverConnection.GetService<ICatalogService>();

            ReadOnlyCollection<CatalogNode> projectCollections = serverConnection.CatalogNode.QueryChildren(new[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);
            foreach (CatalogNode collection in projectCollections)
            {

                Guid collectionGuid = new Guid(collection.Resource.Properties["InstanceId"]);
                TfsTeamProjectCollection collectionObject = serverConnection.GetTeamProjectCollection(collectionGuid);

                if (collection.Resource.DisplayName != "Customer")
                // choose the name of the collection you want this run against
                    continue;

                ReadOnlyCollection<CatalogNode> teamProjects = collection.QueryChildren(new[] { CatalogResourceTypes.TeamProject }, false, CatalogQueryOptions.None);

                foreach (CatalogNode project in teamProjects)
                {
                    string arguments  = collectionObject.Uri + " " + project.Resource.DisplayName;
                    ExecuteCommand(arguments);
                }
            }
        }
    }
}
