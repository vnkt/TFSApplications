using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.Server;

namespace TFSAPITEAMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TfsTeamProjectCollection tfs;
        private TfsTeamService teamService;
        private ProjectInfo projectInfo;
        private IGroupSecurityService gss;
        private List<TeamFoundationTeam> _teams;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            var tpp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            tpp.ShowDialog();

            if (tpp.SelectedTeamProjectCollection == null) return;

            tfs = tpp.SelectedTeamProjectCollection;
            teamService = tfs.GetService<TfsTeamService>();
            projectInfo = tpp.SelectedProjects[0];

            gss = (IGroupSecurityService)tfs.GetService(typeof(IGroupSecurityService));

            GetTeams();
            btnRemove.IsEnabled = btnCreateTeam.IsEnabled = true;
        }

        void GetTeams()
        {
            _teams = teamService.QueryTeams(projectInfo.Uri).ToList();
            listTeams.ItemsSource = _teams;
        }

        private void listTeams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listTeams.SelectedItem == null) return;
            listGroups.SelectedItem = null;
            var team = listTeams.SelectedItem as TeamFoundationTeam;

            listMembers.ItemsSource = team.GetMembers(tfs, MembershipQuery.Direct);


            
        }

        private void btnCreateTeam_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text)) { MessageBox.Show("Please enter title"); return; }
            teamService.CreateTeam(projectInfo.Uri, txtTitle.Text, txtDescription.Text, null);
            GetTeams();
        }

        private void listGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listGroups.SelectedItem == null) return;
            listTeams.SelectedItem = null;
            var group = listGroups.SelectedItem as Identity;

            Identity SIDS = gss.ReadIdentity(SearchFactor.Sid, group.Sid, QueryMembership.Direct);
            if (SIDS.Members.Length == 0)
            {
                listMembers.ItemsSource = null;
                return;

            }
            Identity[] UserIds = gss.ReadIdentities(SearchFactor.Sid, SIDS.Members, QueryMembership.None);
            listMembers.ItemsSource = UserIds;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (listTeams.SelectedItem != null)
            {
                var team = listTeams.SelectedItem as TeamFoundationTeam;
                var teamSid = team.Identity.Descriptor.Identifier;

                gss.DeleteApplicationGroup(teamSid);
                GetTeams();
            }
            else if (listGroups.SelectedItem != null)
            {
                var group = listGroups.SelectedItem as Identity;
                var groupSid = group.Sid;

                gss.DeleteApplicationGroup(groupSid);
            }
        }
    }
}
