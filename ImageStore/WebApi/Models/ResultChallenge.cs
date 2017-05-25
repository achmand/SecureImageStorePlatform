using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.Models
{
    public sealed class ResultChallenge : IHttpActionResult
    {
        #region properties & variables

        private readonly IHttpActionResult _next;

        #endregion

        #region ctors

        public ResultChallenge(IHttpActionResult next)
        {
            _next = next;
        }

        #endregion

        #region public methods

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await _next.ExecuteAsync(cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("hmac"));
            }

            return response;
        }

        #endregion
    }
}