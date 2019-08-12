using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.IdentityManagement;
using Newtonsoft.Json;

namespace S3Assessment
{
    public static class Helper
    {
        public static bool CheckIfPolicyIsFullAccess(ManagedPolicyData policyData, bool checkPolicyName = true)
        {
            if (checkPolicyName)
            {
                if (policyData.Name.Contains("AmazonS3FullAccess"))
                {
                    return true;
                }
            }

            return Uri.UnescapeDataString(policyData.Document)
                       .Replace("\n", string.Empty)
                       .IndexOf("\"Action\": \"s3:*\"", StringComparison.OrdinalIgnoreCase) > 0;
        }

        public static bool CheckIfPolicyRelatesToS3(string document)
        {
            document = Uri.UnescapeDataString(document);

            if (document.Contains("\"Action\": \"s3:*\""))
                return true;
            if (document.Contains("\"s3:"))
                return true;
            if (document.Contains("\"arn:aws:s3:::"))
                return true;

            return false;
        }

        public static T Do<T>(this AmazonIdentityManagementServiceClient amazonClient, Func<AmazonIdentityManagementServiceClient, Task<T>> action)
        {
            return action(amazonClient).Result;
        }

        public static void Show(string data, ConsoleColor? color = null)
        {
            if (color != null)
                Console.ForegroundColor = color.Value;

            Console.WriteLine(data);
            Console.ResetColor();
        }
    }
}
