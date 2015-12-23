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

        public void codeCoverageOutsideBuildScope()
        {

            
        }

        static void Main(string[] args)
        {
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri("http://vnkt-server:8080/tfs/tfs2015"));
            ITestManagementService tms = tfs.GetService<ITestManagementService>();

            IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            IBuildDetailSpec buildSpec = buildServer.CreateBuildDetailSpec("CodeRepository", "CodeCoverage");
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


                        ITestRunCoverage[] sourceBlocks = coverageAnalysisManager.QueryTestRunCoverage(current.Id, CoverageQueryFlags.Modules);
                        foreach (ITestRunCoverage currentBlock in sourceBlocks)
                        {
                            IModuleCoverage[] modules = currentBlock.Modules;
                            foreach (IModuleCoverage module in modules)
                            {
                                num += module.Statistics.BlocksCovered;
                                num2 += module.Statistics.BlocksNotCovered;

                            }
                        }

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
