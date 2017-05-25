using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using WebApi.Models;

namespace WebApi.Attributes
{
    // HMAC-Authentication, Changed most of the code found in the resource below 
    // RESOURCE: https://github.com/tjoudeh/WebApiHMACAuthentication
    public sealed class HmacAuthAttribute : Attribute, IAuthenticationFilter
    {
        #region propeties & variables

        private static readonly Dictionary<string, string> AuthenticatedApps = new Dictionary<string, string>();
        private const double TimeOutMinutes = 2;
        private const string Scheme = "dmc";

        public bool AllowMultiple => false;

        #endregion

        #region ctors

        public HmacAuthAttribute()
        {
            if (AuthenticatedApps.Count <= 0)
            {
                AuthenticatedApps.Add("FDB030466B5E4CDD86F4406214D9A9C7", "0AD1DC3EF5594E2786C886A5BD7AA34B");
            }
        }

        #endregion

        #region public methods

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var validReqAuth = request.Headers.Authorization != null;
            var validReqScheme = request.Headers.Authorization?.Scheme == Scheme;

            if (!validReqAuth || !validReqScheme)
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                return Task.FromResult(0);
            }

            var rawData = request.Headers.Authorization.Parameter;
            var authValues = rawData.Split('|');
            if (authValues.Length != 4)
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            var isValid = Validate(authValues, request);
            if (!isValid)
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            var applicationId = authValues[0];
            var principal = new GenericPrincipal(new GenericIdentity(applicationId), null);
            context.Principal = principal;

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ResultChallenge(context.Result);
            return Task.FromResult(0);
        }

        #endregion

        #region private methods

        private static bool ValidateTimeStamp(string timestamp)
        {
            var timeNow = DateTime.UtcNow;
            DateTime time;

            if (!DateTime.TryParse(timestamp, out time))
            {
                return false;
            }

            var minutes = timeNow.Subtract(time).TotalMinutes;
            return !(minutes > TimeOutMinutes);
        }

        private static bool Validate(IReadOnlyList<string> values, HttpRequestMessage httpRequestMessage)
        {
            var appId = values[0];
            if (!AuthenticatedApps.ContainsKey(appId))
            {
                return false;
            }

            var signature = values[1];
            var nonce = values[2];
            var timeStamp = values[3];

            var validTimeStamp = ValidateTimeStamp(timeStamp);
            if (!validTimeStamp)
            {
                return false;
            }

            var method = httpRequestMessage.Method.Method;
            var hashedContent = GetHash(httpRequestMessage.Content);
            var requestContent = string.Empty;

            if (hashedContent != null)
            {
                requestContent = Convert.ToBase64String(hashedContent);
            }

            var requestUri = Uri.EscapeUriString(httpRequestMessage.RequestUri.ToString().ToLowerInvariant());
            var rawData = string.Format($"{appId},{method},{requestUri},{timeStamp},{nonce},{requestContent}");
            var secret = AuthenticatedApps[appId];

            var secretBytes = Convert.FromBase64String(secret);
            var signatureEncoded = Encoding.UTF8.GetBytes(rawData);

            using (var hmac = new HMACSHA256(secretBytes))
            {
                var signatureBytes = hmac.ComputeHash(signatureEncoded);
                var signatureBase64 = Convert.ToBase64String(signatureBytes);
                var isValid = signature == signatureBase64;
                return isValid;
            }
        }

        private static byte[] GetHash(HttpContent httpContent)
        {
            using (var md5 = MD5.Create())
            {
                var content = httpContent.ReadAsByteArrayAsync().Result;
                if (content.Length == 0)
                {
                    return null;
                }

                var hash = md5.ComputeHash(content);
                return hash;
            }
        }

        #endregion
    }
}