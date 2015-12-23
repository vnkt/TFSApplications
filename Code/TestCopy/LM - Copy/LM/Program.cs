using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using System.Collections.ObjectModel;


namespace ListOfProject
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TfsTeamProjectCollection collection = new TfsTeamProjectCollection(new Uri("http://sinjithtfs2013:8080/tfs"));

                ITestManagementService service = collection.GetService<ITestManagementService>();
                ITestManagementTeamProject testManagementProject = service.GetTeamProject("Test Project 2013");

                foreach (ITestPlan p in testManagementProject.TestPlans.Query("Select * From TestPlan"))
                {
                    ITestPlan plan = testManagementProject.TestPlans.Create();
                    plan.Name = p.Name;
                    plan.StartDate = p.StartDate;
                    plan.EndDate = p.EndDate;
                    plan.Save();

                    Console.WriteLine(p.RootSuite.Entries.Count());

                    foreach (ITestSuiteBase suite in p.RootSuite.SubSuites)
                    {
                        IStaticTestSuite newSuite = testManagementProject.TestSuites.CreateStatic();
                        newSuite.Title = suite.Title;
                        newSuite.Description = suite.Description;
                        Console.WriteLine(suite.TestSuiteType);
                        //newSuite.TestCases = suite.TestCases;
                        plan.RootSuite.Entries.Add(newSuite);
                    }

                    plan.Save();

                }

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception" + e);
                Console.ReadLine();
            }
        }
    }
}
