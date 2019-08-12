using System;
using System.Collections.Generic;
using System.Text;

namespace S3Assessment
{
    public class ManagedPolicyData
    {
        public string Arn { get; set; }
        public string DefaultVersionId { get; set; }
        public string Document { get; set; }
        public string Name { get; set; }
        public List<string> RolesArn { get; set; }
    }
}
