using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.IdentityManagement.Model;

namespace S3Assessment
{
    public class PoliciesProcessor
    {
        private static readonly Dictionary<string, ManagedPolicyData> _policies = new Dictionary<string, ManagedPolicyData>();

        public void ProcessInlinePolicies(Dictionary<string, List<string>> inlineRolePolicies)
        {
            foreach (var inlineRolePolicy in inlineRolePolicies)
            {
                Helper.Show($"Analyzing role: {inlineRolePolicy.Key}", ConsoleColor.Gray);

                foreach (var inlinePolicy in inlineRolePolicy.Value)
                {
                    Helper.Show($"    |- analyzing attached policy: {inlinePolicy}", ConsoleColor.DarkGray);

                    string policyDocument = AWS.IamClient.Do(c => c.GetRolePolicyAsync(new GetRolePolicyRequest
                    {
                        PolicyName = inlinePolicy,
                        RoleName = RolesProcessor.Roles[inlineRolePolicy.Key].RoleName
                    })).PolicyDocument;

                    var policyData = new ManagedPolicyData
                    {
                        Document = policyDocument,
                        Name = inlinePolicy,
                        RolesArn = new List<string> { inlineRolePolicy.Key }
                    };

                    _policies.Add($"{inlineRolePolicy.Key}_{inlinePolicy}", policyData);
                }
            }
        }

        public void ProcessAttachedPolicies(Dictionary<string, List<string>> attachedRolePolicies)
        {
            foreach (var roleWithPolicies in attachedRolePolicies)
            {
                Helper.Show($"Analyzing role: {roleWithPolicies.Key}", ConsoleColor.Gray);

                foreach (string policyArn in roleWithPolicies.Value)
                {
                    Helper.Show($"    |- analyzing attached policy: {policyArn}", ConsoleColor.DarkGray);

                    ManagedPolicyData managedPolicy;

                    if (!_policies.ContainsKey(policyArn))
                    {
                        var policy = AWS.IamClient.Do(c => c.GetPolicyAsync(new GetPolicyRequest
                        {
                            PolicyArn = policyArn
                        })).Policy;

                        managedPolicy = new ManagedPolicyData
                        {
                            DefaultVersionId = policy.DefaultVersionId,
                            Arn = policy.Arn,
                            Name = policy.PolicyName,
                            RolesArn = new List<string> { roleWithPolicies.Key }
                        };

                        managedPolicy.Document = AWS.IamClient.Do(c => c.GetPolicyVersionAsync(
                                new GetPolicyVersionRequest
                                {
                                    PolicyArn = policyArn,
                                    VersionId = managedPolicy.DefaultVersionId
                                }))
                            .PolicyVersion
                            .Document;

                        _policies.Add(policyArn, managedPolicy);
                    }
                    else
                    {
                        _policies[policyArn].RolesArn.Add(roleWithPolicies.Key);
                    }
                }
            }
        }

        public Dictionary<string, List<string>> GetPoliciesGrantingS3Access(IEnumerable<string> rolesUsedS3Service)
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var roleAccordingToS3 in rolesUsedS3Service)
            {
                Helper.Show($"Checking {roleAccordingToS3} policies", ConsoleColor.DarkMagenta);

                List<PolicyGrantingServiceAccess> policiesGrantingServiceAccess = AWS.IamClient.Do(c => c.ListPoliciesGrantingServiceAccessAsync(
                    new ListPoliciesGrantingServiceAccessRequest
                    {
                        ServiceNamespaces = new List<string> { "s3" },
                        Arn = roleAccordingToS3
                    }))
                    .PoliciesGrantingServiceAccess
                    .First()
                    .Policies;

                foreach (var policyGrantingServiceAccess in policiesGrantingServiceAccess)
                {
                    string policyKey = policyGrantingServiceAccess.PolicyArn;

                    if (string.IsNullOrEmpty(policyGrantingServiceAccess.PolicyArn))
                    {
                        policyKey = $"{roleAccordingToS3}_{policyGrantingServiceAccess.PolicyName}";
                    }

                    Helper.Show($"Analyzing {policyKey} policy", ConsoleColor.DarkCyan);

                    var policy = _policies[policyKey];
                    if (result.ContainsKey(roleAccordingToS3))
                    {
                        result[roleAccordingToS3].Add(policy.Name);
                    }
                    else
                    {
                        result.Add(roleAccordingToS3, new List<string> { policy.Name });
                    }
                }
            }

            return result;
        }

        public IEnumerable<string> GetRolesArnThatHasS3Policies()
        {
            return _policies
                .Where(p => Helper.CheckIfPolicyRelatesToS3(Uri.UnescapeDataString(p.Value.Document)))
                .SelectMany(i => i.Value.RolesArn)
                .Distinct()
                .ToHashSet();
        }

        public IEnumerable<string> GetRolesWithFullAccessPolicy()
        {
            return _policies
                .Where(p => Helper.CheckIfPolicyIsFullAccess(p.Value))
                .SelectMany(i => i.Value.RolesArn)
                .Distinct()
                .ToHashSet();
        }
    }
}
