using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using S3Assessment.Processors;

namespace S3Assessment
{
    class Program
    {
        static void Main(string[] args)
        {
            var rolesProcessor = new RolesProcessor();
            var jobs = rolesProcessor.InitiateJobsToRolesAccessAnalysis();
            rolesProcessor.ReceiveRolesPoliciesNames();

            // Grab all inline and attached IAM policies
            var policiesProcessor = new PoliciesProcessor();
            policiesProcessor.ProcessAttachedPolicies(rolesProcessor.AttachedPolicies);
            policiesProcessor.ProcessInlinePolicies(rolesProcessor.InlinePolicies);

            var jobsProcessor = new JobsProcessor();
            var rolesUsingS3Service = jobsProcessor.GetRolesUsingS3Service(jobs);

            var rolesHavingS3Permissions = policiesProcessor.GetRolesArnThatHasS3Policies();
            var policiesUsingS3Buckets = policiesProcessor.GetPoliciesGrantingS3Access(rolesUsingS3Service);

            // Finding overprivileged roles
            Helper.Show("********************** Roles that have S3 permission but don't use S3 *********************************");
            var unusedRoles = rolesHavingS3Permissions.Except(rolesUsingS3Service);
            if (unusedRoles.Count() > 0) 
            {
                Array.ForEach(unusedRoles.ToArray(), (role) => Helper.Show(role, ConsoleColor.Red));
            }
            else
            {
                Helper.Show("No roles found!");
            }

            // Finding administrative roles
            Helper.Show("********************** Roles that don't have S3 permission but use S3 *********************************");
            var implicitRoles = rolesUsingS3Service.Except(rolesHavingS3Permissions);
            if (implicitRoles.Count() > 0)
            {
                Array.ForEach(implicitRoles.ToArray(), (role) => Helper.Show(role, ConsoleColor.Red));
            }
            else
            {
                Helper.Show("No roles found!");
            }

            Helper.Show("********************** Roles that use S3 buckets *************************");
            foreach (var policiesUsedS3Bucket in policiesUsingS3Buckets)
            {
                Helper.Show(policiesUsedS3Bucket.Key, ConsoleColor.DarkBlue);
                foreach (var policy in policiesUsedS3Bucket.Value)
                {
                    Helper.Show($"    |-{policy}", ConsoleColor.DarkBlue);
                }
            }

            Helper.Show("********************** Roles that have S3 Full Access policies *************************");
            foreach (var roleHavingFullAccess in policiesProcessor.GetRolesWithFullAccessPolicy())
            {
                Helper.Show(roleHavingFullAccess, ConsoleColor.DarkGreen);
            }
        }
    }
}