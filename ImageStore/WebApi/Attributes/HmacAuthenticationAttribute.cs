using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace WebApi.Attributes
{
    // http://bitoftech.net/2014/12/15/secure-asp-net-web-api-using-api-key-authentication-hmac-authentication/

    public class HmacAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        private static readonly Dictionary<string, string> AllowedApps = new Dictionary<string, string>();
        private const ulong RequestMaxAgeInSeconds = 300; // 5 min
        private const string AuthenticationScheme = "amx";

        public HmacAuthenticationAttribute()
        {
            if (AllowedApps.Count == 0)
            {
                AllowedApps.Add("4d53bce03ec34c0a911182d4c228ee6c", "A93reRTUJHsCuQSHR+L3GxqOJyDmQpCgps102ciuabc=");
            }
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var req = context.Request;

            if (req.Headers.Authorization != null &&
                AuthenticationScheme.Equals(req.Headers.Authorization.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                var rawAuthzHeader = req.Headers.Authorization.Parameter;

                var autherizationHeaderArray = GetAutherizationHeaderValues(rawAuthzHeader);

                if (autherizationHeaderArray != null)
                {
                    var appId = autherizationHeaderArray[0];
                    var incomingBase64Signature = autherizationHeaderArray[1];
                    var nonce = autherizationHeaderArray[2];
                    var requestTimeStamp = autherizationHeaderArray[3];

                    var isValid = IsValidRequest(req, appId, incomingBase64Signature, nonce, requestTimeStamp);

                    if (isValid.Result)
                    {
                        var currentPrincipal = new GenericPrincipal(new GenericIdentity(appId), null);
                        context.Principal = currentPrincipal;
                    }
                    else
                    {
                        context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                    }
                }
                else
                {
                    context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                }
            }
            else
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ResultWithChallenge(context.Result);
            return Task.FromResult(0);
        }

        public bool AllowMultiple => false;

        private static string[] GetAutherizationHeaderValues(string rawAuthzHeader)
        {
            var credArray = rawAuthzHeader.Split(':');
            return credArray.Length == 4 ? credArray : null;
        }

        private static async Task<bool> IsValidRequest(HttpRequestMessage req, string appId, string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            var requestContentBase64String = "";
            var requestUri = HttpUtility.UrlEncode(req.RequestUri.AbsoluteUri.ToLower());
            var requestHttpMethod = req.Method.Method;

            if (!AllowedApps.ContainsKey(appId))
            {
                return false;
            }

            var sharedKey = AllowedApps[appId];

            if (IsReplayRequest(nonce, requestTimeStamp))
            {
                return false;
            }

            var hash = await ComputeHash(req.Content);

            if (hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }

            var data = $"{appId}{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentBase64String}";
            var secretKeyBytes = Convert.FromBase64String(sharedKey);
            var signature = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(secretKeyBytes))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                return incomingBase64Signature.Equals(Convert.ToBase64String(signatureBytes), StringComparison.Ordinal);
            }

        }

        private static bool IsReplayRequest(string nonce, string requestTimeStamp)
        {
            if (MemoryCache.Default.Contains(nonce))
            {
                return true;
            }

            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            var currentTs = DateTime.UtcNow - epochStart;
            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

            if (serverTotalSeconds - requestTotalSeconds > RequestMaxAgeInSeconds)
            {
                return true;
            }

            MemoryCache.Default.Add(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(RequestMaxAgeInSeconds));
            return false;
        }

        private static async Task<byte[]> ComputeHash(HttpContent httpContent)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = null;
                var content = await httpContent.ReadAsByteArrayAsync();
                if (content.Length != 0)
                {
                    hash = md5.ComputeHash(content);
                }
                return hash;
            }
        }
    }

    public class ResultWithChallenge : IHttpActionResult
    {
        private const string AuthenticationScheme = "amx";
        private readonly IHttpActionResult _next;

        public ResultWithChallenge(IHttpActionResult next)
        {
            _next = next;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await _next.ExecuteAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(AuthenticationScheme));
            }
            return response;
        }
    }
}