using System.Net.Http;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class BaseController : Controller
    {
        private HttpClient _httpClient;
        public const string ApiUri = "http://localhost:8836/api/";

        public BaseController()
        {
            _httpClient = HttpClientFactory.Create();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
        }

        public BaseController(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

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