using System;
using System.Collections.Generic;
using System.Text;
using Amazon.IdentityManagement;

namespace S3Assessment
{
    public static class AWS
    { 
        private static Lazy<AmazonIdentityManagementServiceClient> _iamClient = new Lazy<AmazonIdentityManagementServiceClient>
        (
            () => new AmazonIdentityManagementServiceClient(),
            true
        );

        public static AmazonIdentityManagementServiceClient IamClient
        {
            get
            {
                return _iamClient.Value;
            }
        }
    }
}
