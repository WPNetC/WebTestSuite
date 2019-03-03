using System;
using System.Net;
using System.Net.Http;

namespace $rootnamespace$.StandardTests
{
    public abstract class WebTestBase : IDisposable
    {
        protected static HttpClient httpClient;

        protected static bool TryUri(System.Uri uri)
        {
            try
            {
                var client = httpClient ?? (httpClient = new HttpClient());
                var response = httpClient.GetAsync(uri).Result;

                return HttpStatusCode.OK == response.StatusCode;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    httpClient?.Dispose();
                    httpClient = null;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
