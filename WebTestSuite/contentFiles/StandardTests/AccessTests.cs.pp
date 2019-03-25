using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using $rootnamespace$.Common;

namespace $rootnamespace$.StandardTests
{
    [TestClass]
    public class AccessTests : WebTestBase
    {
        [TestMethod]
        public void PageIsHTML5()
        {
            var url = $"http://{PathsAndPorts.BASE_ADDR}";
            var html = GetHtml(url);
            Assert.IsTrue(Regex.IsMatch(html, "<!doctype html>", RegexOptions.IgnoreCase),
                $"Page at {url} is not HTML5 doc type");
        }

        [TestMethod]
        public void CanAccessSiteHttp()
        {
            var uri = new System.Uri($"http://{PathsAndPorts.BASE_ADDR}");
            Assert.IsTrue(TryUri(uri), $"Cannot access site on http at: {uri.AbsoluteUri}");
        }

        [TestMethod]
        public void CanAccessSiteHttps()
        {
            var uri = new System.Uri($"https://{PathsAndPorts.BASE_ADDR}");
            using (var httpClientHandler = new HttpClientHandler())
            {
                // This is to ignore SSL cert errors that are likely on development machines
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    var response = client.GetAsync(uri).Result;
                    Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, $"Cannot access site on https at: {uri.AbsoluteUri}");
                }
            }
        }

        [TestMethod]
        public void CanAccessRobotsTxt()
        {
            var uri = new System.Uri($"http://{PathsAndPorts.BASE_ADDR}{PathsAndPorts.ROBOTS_ADR}");
            Assert.IsTrue(TryUri(uri), $"Cannot access robots.txt at: {uri.AbsoluteUri}");
        }

        [TestMethod]
        public void CanAccessSitemap()
        {
            var uri = new System.Uri($"http://{PathsAndPorts.BASE_ADDR}{PathsAndPorts.SITEMAP_ADR}");
            Assert.IsTrue(TryUri(uri), $"Cannot access sitemap at: {uri.AbsoluteUri}");
        }
    }
}
