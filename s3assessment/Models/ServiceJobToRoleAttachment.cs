using System;
using System.Collections.Generic;
using System.Text;

namespace S3Assessment.Models
{
    public class ServiceJobToRoleAttachment
    {
        public string JobId { get; set; }
        public string RoleArn { get; set; }
    }
}
