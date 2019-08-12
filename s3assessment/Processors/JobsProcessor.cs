using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.IdentityManagement.Model;
using S3Assessment.Models;

namespace S3Assessment.Processors
{
    public class JobsProcessor
    {

        private IEnumerable<string> DoGetRolesUsingS3(IEnumerable<ServiceJobToRoleAttachment> serviceJobToRoleAttachments) 
        {
            foreach (var rolesJobId in serviceJobToRoleAttachments)
            {
                Helper.Show($"Processing Job: {rolesJobId.JobId}");

                List<string> servicesUsedByRole = AWS.IamClient.Do(c => c.GetServiceLastAccessedDetailsAsync(
                        new GetServiceLastAccessedDetailsRequest
                        {
                            JobId = rolesJobId.JobId
                        })).ServicesLastAccessed
                    .Select(s => s.ServiceName)
                    .ToList();

                if (servicesUsedByRole.Contains("Amazon S3"))
                {
                    yield return rolesJobId.RoleArn;
                }

                Helper.Show($"Role {rolesJobId.RoleArn}, services count {servicesUsedByRole.Count}", ConsoleColor.DarkYellow);
            }
        }

        public IEnumerable<string> GetRolesUsingS3Service(List<ServiceJobToRoleAttachment> serviceJobToRoleAttachments)
        {
            return DoGetRolesUsingS3(serviceJobToRoleAttachments).ToHashSet();
        }
    }
}
