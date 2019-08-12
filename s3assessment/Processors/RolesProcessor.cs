using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using S3Assessment;
using S3Assessment.Models;

public class RolesProcessor
{
    private static Dictionary<string, Role> _roles = new Dictionary<string, Role>();
    private readonly Dictionary<string, List<string>> _inlinePolicies = new Dictionary<string, List<string>>();
    private readonly Dictionary<string, List<string>> _attachedPolicies = new Dictionary<string, List<string>>();

    public static Dictionary<string, Role> Roles => _roles;
    public Dictionary<string, List<string>> InlinePolicies => _inlinePolicies;
    public Dictionary<string, List<string>> AttachedPolicies => _attachedPolicies;


    public RolesProcessor()
    {
        Helper.Show("Receiving roles...", ConsoleColor.DarkCyan);

        _roles = AWS.IamClient.Do(i => i.ListRolesAsync(new ListRolesRequest
        {
            MaxItems = 1000
        }))
        .Roles
        .ToDictionary(r => r.Arn);
    }

    public List<ServiceJobToRoleAttachment> InitiateJobsToRolesAccessAnalysis()
    {
        Helper.Show("Initiating jobs to get access info for AWS services...", ConsoleColor.DarkCyan);

        return _roles.Values.Select(i =>
        {
            string jobId = AWS.IamClient.Do(c => 
                c.GenerateServiceLastAccessedDetailsAsync(
                new GenerateServiceLastAccessedDetailsRequest
                {
                    Arn = i.Arn
                }))
                .JobId;

            return new ServiceJobToRoleAttachment
            {
                JobId = jobId,
                RoleArn = i.Arn
            };
        }).ToList();
    }

    public void ReceiveRolesPoliciesNames()
    {
        Helper.Show("Receiving roles and policies...", ConsoleColor.DarkCyan);

        foreach (Role role in _roles.Values)
        {
            var inlineRolePolicies = AWS.IamClient.Do(c => c.ListRolePoliciesAsync(new ListRolePoliciesRequest
                {
                    RoleName = role.RoleName
                }))
                .PolicyNames;

            _inlinePolicies.Add(role.Arn, inlineRolePolicies);

            var attachedRolePolicies = AWS.IamClient.Do(c => c.ListAttachedRolePoliciesAsync(
                    new ListAttachedRolePoliciesRequest
                    {
                        RoleName = role.RoleName
                    }))
                .AttachedPolicies
                .Select(p => p.PolicyArn)
                .ToList();

            _attachedPolicies.Add(role.Arn, attachedRolePolicies);
        }
    }
}