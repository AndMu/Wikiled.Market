using System;
using Wikiled.Market.Analysis;
using Wikiled.Twitter.Security;

namespace Wikiled.Market.Console.Logic
{
    public class ConfigurationValidator
    {
        public ConfigurationValidator(IAuthentication cred, Credentials credentials)
        {
            if (cred == null)
            {
                throw new ArgumentNullException(nameof(cred));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            var twitterCredentials = cred.Authenticate();
            if (string.IsNullOrWhiteSpace(twitterCredentials.AccessToken) ||
                string.IsNullOrWhiteSpace(twitterCredentials.AccessTokenSecret))
            {
                throw new ArgumentNullException("Access token not found");
            }

            if (string.IsNullOrWhiteSpace(credentials.QuandlKey))
            {
                throw new ArgumentNullException("QuandlKey not found");
            }
        }
    }
}
