using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using s3assessment.Processors;

namespace s3assessment
{
    class Program
    {
        static void Main(string[] args)
        {
            var rolesProcessor = new RolesProcessor();

            var jobs = rolesProcessor.InitiateJobsToRolesAccessAnalysis();
            rolesProcessor.ReceiveRolesPoliciesNames();

            var policiesProcessor = new PoliciesProcessor();

            policiesProcessor.ProcessAttachedPolicies(rolesProcessor.AttachedPolicies);
            policiesProcessor.ProcessInlinePolicies(rolesProcessor.InlinePolicies);

            var jobsProcessor = new JobsProcessor();

            var rolesUsedS3Service = jobsProcessor.GetRolesUsingS3Service(jobs).ToList();
            var rolesHasS3Policies = policiesProcessor.GetRolesArnThatHasS3Policies();

            var policiesHavingProductionBucketsAccess = policiesProcessor.GetPoliciesGrantingS3Access(rolesUsedS3Service);


            Helper.Show("**********************Roles that have S3 permission but don't use it*********************************");

            foreach (var rolesHasS3Policy in rolesHasS3Policies)
            {
                if (!rolesUsedS3Service.Contains(rolesHasS3Policy))
                {
                    Helper.Show(rolesHasS3Policy, ConsoleColor.Red);
                }
            }

            Helper.Show("**********************Roles that don't have S3 permission but use it*********************************");

            foreach (var roleUsedS3 in rolesUsedS3Service)
            {
                if (!rolesHasS3Policies.Contains(roleUsedS3))
                {
                    Helper.Show(roleUsedS3, ConsoleColor.Red);
                }
            }

            Helper.Show("********************** Roles that use S3 production buckets *************************");

            foreach (var policiesUsedS3Bucket in policiesHavingProductionBucketsAccess)
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

            Console.ReadLine();
        }
    }
}