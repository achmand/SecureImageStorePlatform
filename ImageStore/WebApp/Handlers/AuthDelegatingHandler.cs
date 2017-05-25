using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace WebApp.Handlers
{
    // created custom delegating handler 
    public sealed class AuthDelegatingHandler : DelegatingHandler
    {
        private readonly string _applicationId = WebConfigurationManager.AppSettings["AppId"];
        private readonly string _secret = WebConfigurationManager.AppSettings["Secret"];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var reqContent = string.Empty;
            var requestUri = Uri.EscapeUriString(request.RequestUri.ToString().ToLowerInvariant());
            var timeStamp = DateTime.UtcNow.ToString("g");
            var nonce = Guid.NewGuid().ToString();

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsByteArrayAsync();
                using (var md5 = MD5.Create())
                {
                    var hashedContent = md5.ComputeHash(content);
                    reqContent = Convert.ToBase64String(hashedContent);
                }
            }

            var method = request.Method.Method;
            var signatureRaw = string.Format($"{_applicationId},{method},{requestUri},{timeStamp},{nonce},{reqContent}");

            var secret = Convert.FromBase64String(_secret);
            var signature = Encoding.UTF8.GetBytes(signatureRaw);

            using (var hmac = new HMACSHA256(secret))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                var requestSignature = Convert.ToBase64String(signatureBytes);
                request.Headers.Authorization = new AuthenticationHeaderValue("dmc", string.Format($"{_applicationId}|{requestSignature}|{nonce}|{timeStamp}"));
            }

            var httpResponse = await base.SendAsync(request, cancellationToken);
            return httpResponse;
        }
    }
}