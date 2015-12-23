using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace CodeCoverageCheck
{
	[BuildActivity(HostEnvironmentOption.All)] //specifying the scope of the activity - I have put it all so that this can execute in all the scopes (build, agent, controller) etc
	public sealed class CodeCoverageCheckActivity : CodeActivity<int> //returns integer
	{
		public InArgument<IBuildDetail> BuildDetail { get; set; } //getting the currrent build context. 
		//BuildDetail will be the variable that will habve all the information about the current build
		//It is a InArgument hence going to be an input to the activity
		
		protected override int Execute(CodeActivityContext context)
		{

			IBuildDetail currentBuildDetail = context.GetValue<IBuildDetail>(this.BuildDetail); //Creating a local variable to copy the BuildDetail we get as input

			ITestManagementService service = currentBuildDetail.BuildServer.TeamProjectCollection.GetService<ITestManagementService>(); //Initializing the Test Management Service

			ITestManagementTeamProject teamProject = service.GetTeamProject(currentBuildDetail.TeamProject); //Getting the team project associated with the build
			System.Collections.Generic.IEnumerable<ITestRun> totalRuns = teamProject.TestRuns.ByBuild(currentBuildDetail.Uri); //Getting the Total tests run 
			ICoverageAnalysisManager coverageAnalysisManager = teamProject.CoverageAnalysisManager; //Getting the Class to retrieve code coverage

			int coveredBlocks = 0;
			int skippedBlocks = 0;
			int codeCoveragePercent = 0;

			foreach (ITestRun currentRun in totalRuns) //Looping through all the test runs
			{

				ITestRunCoverage[] sourceBlocks = coverageAnalysisManager.QueryTestRunCoverage(currentRun.Id, CoverageQueryFlags.Modules);
				 foreach (ITestRunCoverage currentBlock in sourceBlocks)
				 {
					IModuleCoverage[] modules = currentBlock.Modules;
					 foreach (IModuleCoverage module in modules)
					 {
							coveredBlocks += module.Statistics.BlocksCovered;
							skippedBlocks += module.Statistics.BlocksNotCovered;

					 }
				 }
			}

			int totalBlocks = coveredBlocks + skippedBlocks;

			if (totalBlocks == 0)
			{
				codeCoveragePercent = 0;
				return codeCoveragePercent;
			}

			codeCoveragePercent = (int)((double)coveredBlocks * 100.0 / (double)totalBlocks);

			currentBuildDetail.Status = BuildStatus.Failed;
			return codeCoveragePercent;
		}
	}
}
