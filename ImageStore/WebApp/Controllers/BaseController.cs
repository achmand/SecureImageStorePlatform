using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;
using WebApp.Handlers;

namespace WebApp.Controllers
{
    public class BaseController : Controller
    {
        #region properties & variables 

        private HttpClient _httpClient;
        public const string ApiUri = "http://localhost:8836/api";

        public HttpClient HttpClient
        {
            get
            {
                return _httpClient ?? new HttpClient();
            }
            private set
            {
                _httpClient = value;
            }
        }


        #endregion

        #region ctors 

        public BaseController()
        {
            var authDelegating = new AuthDelegatingHandler();
            _httpClient = HttpClientFactory.Create(authDelegating);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public BaseController(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        #endregion
     
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}