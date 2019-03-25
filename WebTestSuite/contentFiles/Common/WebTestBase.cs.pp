using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;

namespace $rootnamespace$.Common
{
    public abstract class WebTestBase : IDisposable
    {
        protected static HttpClient httpClient;

        // Using local variables to save re-getting same page unnecessarily
        // but still allow a differnt page for differnt test classes
        protected string _pageHtml;
        protected string _pageUrl;
        protected HtmlDocument _doc;

        /// <summary>
        /// Checks a uri returns a 200 response
        /// </summary>
        /// <param name="uri">The <see cref="System.Uri"/> to test</param>
        /// <returns>True if a 200 response is received. False otherwise.</returns>
        protected bool TryUri(System.Uri uri)
        {
            try
            {
                if (!uri.IsWellFormedOriginalString())
                {
                    // TODO: Fix or throw on bad uri
                }

                var client = httpClient ?? (httpClient = new HttpClient());
                var response = httpClient.GetAsync(uri).Result;

                return HttpStatusCode.OK == response.StatusCode;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get the page HTML for a given url
        /// </summary>
        /// <param name="url">The url to try to get HTML text from</param>
        /// <returns>Page html text</returns>
        protected string GetHtml(string url)
        {
            try
            {
                // Ensure we have an absolute url
                if (!url.StartsWith("http"))
                {
                    url = url
                        .TrimStart('~')
                        .TrimStart('/');

                    url = $"http://{url}";
                }

                // Get page html
                var client = httpClient ?? (httpClient = new HttpClient());
                return httpClient.GetStringAsync(url).Result;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Try to get the response from a bad url as a <see cref="HtmlDocument"/>
        /// </summary>
        /// <param name="url">The url to try to get a response from.</param>
        /// <returns>The <see cref="HttpStatusCode"/> and response as a <see cref="HtmlDocument"/> if successful. <see langword="null"/> If fails.</returns>
        protected Tuple<HttpStatusCode, HtmlDocument> TryGetBadUrl(string url)
        {
            using (var wc = new WebClient())
            {
                try
                {
                    // This should cause a 404.
                    wc.OpenRead(url);
                }
                catch (WebException ex)
                {
                    // Check it was a 404 error that caused the exception
                    var status = ((HttpWebResponse)ex.Response)?.StatusCode;
                    if (status.HasValue)
                    {
                        try
                        {
                            // Get the response stream from the exception
                            using (var stream = ex.Response.GetResponseStream())
                            {
                                // Load stream as HTML doc
                                var doc = new HtmlDocument();
                                doc.Load(stream);

                                return new Tuple<HttpStatusCode, HtmlDocument>(status.Value, doc);
                            }
                        }
                        catch
                        {

                        }

                        return new Tuple<HttpStatusCode, HtmlDocument>(status.Value, null);
                    }
                }

                return new Tuple<HttpStatusCode, HtmlDocument>(HttpStatusCode.OK, null);
            }
        }

        /// <summary>
        /// Ensures we have the page we are testing.
        /// <para>All tests should fail if this returns false.</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected bool EnsureHavePage(string url)
        {
            if (string.IsNullOrWhiteSpace(_pageHtml) || url != _pageUrl)
            {
                _pageHtml = GetHtml(url);
                _pageUrl = url;
            }

            if (_doc is null)
            {
                _doc = new HtmlDocument();
                _doc.LoadHtml(_pageHtml);
            }

            return _doc != null;
        }

        /// <summary>
        /// Tests a single node exits and is the only instance of it.
        /// <para>Optionally will check for content in the node</para>
        /// </summary>
        /// <param name="url">The url to try to find the node on</param>
        /// <param name="xPath">The XPath to test the html against</param>
        /// <param name="name">The name to use un outut messages</param>
        /// <param name="checkContent">If true will also check for content in the node</param>
        /// <param name="attrName">If set will look for content in this attribute. Otherwise will look in inner HTML.</param>
        protected void TestSingleNode(string url, string xPath, string name, bool checkContent = false, string attrName = null)
        {
            if (!EnsureHavePage(url))
                Assert.Fail($"Could not get document at {url}");

            var nodes = _doc.DocumentNode.SelectNodes(xPath);
            if (nodes == null || nodes.Count == 0)
                Assert.Fail($"No {name} found on {url}");

            if (nodes.Count > 1)
                Assert.Fail($"More than 1 {name} found");

            if (checkContent)
            {
                if (attrName is null)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(nodes[0].InnerText), $"{name} contained no inner text");
                    return;
                }

                var attr = nodes[0].Attributes[attrName];
                if (attr is null)
                    Assert.Fail($"{name} does not contain an attribute named {attrName}");

                Assert.IsFalse(string.IsNullOrWhiteSpace(attr.Value), $"{name} contained no text in attribute {attrName}");
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
                    // TODO: Complete disposing
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
