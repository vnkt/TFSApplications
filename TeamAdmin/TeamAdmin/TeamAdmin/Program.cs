using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Server;
using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.Server.Core;

namespace TeamAdmin
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to the TFS server and get the TfsTeamProjectCollection project URI.
            TfsTeamProjectCollection collection = new TfsTeamProjectCollection(new Uri("http://vnkt-server:8080/tfs/TFS2015"));
            

            ITeamFoundationTeamService team = collection.GetService<ITeamFoundationTeamService>();
            team.QueryTeams()

  
            
            /*// Retrieve the default team.
            TfsTeamService teamService = collection.GetService<TfsTeamService>();
            TeamFoundationTeam defaultTeam = teamService.GetDefaultTeam(projectUri, null);

            // Get security namespace for the project collection.
            ISecurityService securityService = collection.GetService<ISecurityService>();
            SecurityNamespace securityNamespace = securityService.GetSecurityNamespace(FrameworkSecurity.IdentitiesNamespaceId);

            // Use reflection to retrieve a security token for the team.
            MethodInfo mi = typeof(IdentityHelper).GetMethod("CreateSecurityToken", BindingFlags.Static | BindingFlags.NonPublic);
            string token = mi.Invoke(null, new object[] { defaultTeam.Identity }) as string;

            // Retrieve an ACL object for all the team members.
            var allMembers = defaultTeam.GetMembers(collection, MembershipQuery.Expanded).Where(m => !m.IsContainer);
            AccessControlList acl = securityNamespace.QueryAccessControlList(token, allMembers.Select(m => m.Descriptor), true);

            // Retrieve the team administrator SIDs by querying the ACL entries.
            var entries = acl.AccessControlEntries;
            var admins = entries.Where(e => (e.Allow & 15) == 15).Select(e => e.Descriptor.Identifier);

            // Finally, retrieve the actual TeamFoundationIdentity objects from the SIDs.
            var adminIdentities = allMembers.Where(m => admins.Contains(m.Descriptor.Identifier));
            */
        }
    }
}
