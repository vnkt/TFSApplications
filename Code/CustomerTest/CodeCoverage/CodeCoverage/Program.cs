using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Build.Client;

namespace CodeCoverage
{
    public class Program
    {
        public int getBuild(int a, int b)
        {
            Console.WriteLine(a + b);
            return a + b;

        }

        static void Main(string[] args)
        {
           
        }

        public void codeCoverageOutsideBuildScope()
        {

            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri("http://tfs2013d:8080/tfs/15_DefaultCollection"));
            ITestManagementService tms = tfs.GetService<ITestManagementService>();

            IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            IBuildDetailSpec buildSpec = buildServer.CreateBuildDetailSpec("KendallTest", "CUITCoverage");
            IBuildDetail[] valueArray = buildServer.QueryBuilds(buildSpec).Builds;


            try
            {
                foreach (IBuildDetail value in valueArray)
                {
                    ITestManagementService service = value.BuildServer.TeamProjectCollection.GetService<ITestManagementService>();
                    ITestManagementTeamProject teamProject = service.GetTeamProject(value.TeamProject);
                    System.Collections.Generic.IEnumerable<ITestRun> enumerable = teamProject.TestRuns.ByBuild(value.Uri);


                    ICoverageAnalysisManager coverageAnalysisManager = teamProject.CoverageAnalysisManager;
                    int num = 0;
                    int num2 = 0;
                    foreach (ITestRun current in enumerable)
                    {
                        ITestRunCoverage[] source = coverageAnalysisManager.QueryTestRunCoverage(current.Id, CoverageQueryFlags.Modules);
                        num += source.Sum((ITestRunCoverage c) => c.Modules.Sum((IModuleCoverage m) => m.Statistics.BlocksCovered));
                        num2 += source.Sum((ITestRunCoverage c) => c.Modules.Sum((IModuleCoverage m) => m.Statistics.BlocksNotCovered));
                    }
                    int num3 = num + num2;
                    if (num3 == 0)
                    {
                        Console.WriteLine(0);
                    }
                    Console.WriteLine((double)((double)num * 100.0 / (double)num3));
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
