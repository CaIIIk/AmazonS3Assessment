using System;
using System.Collections.Generic;
using System.Text;
using Amazon.IdentityManagement;

namespace s3assessment
{
    public static class AWS
    {
        private static AmazonIdentityManagementServiceClient _iamClient;

        public static AmazonIdentityManagementServiceClient IamClient
        {
            get
            {
                if (_iamClient == null)
                    _iamClient = new AmazonIdentityManagementServiceClient();

                return _iamClient;
            }
        }
    }
}
