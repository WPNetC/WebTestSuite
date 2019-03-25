using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Linq;
using System.Net;
using $rootnamespace$.Common;

namespace $rootnamespace$.StandardTests
{
    [TestClass]
    public class ValidationTests : WebTestBase
    {
        /// <summary>
        /// This is the address the tests will be run against.
        /// <para>Edit this to alter the page being tested.</para>
        /// </summary>
        private static string _testingUrl = $"http://{PathsAndPorts.BASE_ADDR}/";

        [TestMethod]
        public void PagePassesW3CValidation()
        {
            var html = "";
            var url = _testingUrl;
            using (var wc = new WebClient())
                html = wc.DownloadString(url);


            var client = new RestClient("https://validator.w3.org/nu/?out=json");
            var request = new RestRequest(Method.POST);
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "text/html; charset=utf-8");
            request.AddParameter("", html, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                Assert.Inconclusive("Could not reach validator api");

            JObject jObj = null;
            try
            {
                jObj = JObject.Parse(response.Content);
            }
            catch
            {
                Assert.Inconclusive("Could not parse validator response");
            }

            var errors = jObj["messages"]?.Children().Where(p => p["subType"].Value<string>() == "error");
            if (errors?.Any() == true)
                Assert.Fail($"{errors.Count()} errors found on {url}");

            var warns = jObj["messages"]?.Children().Where(p => p["subType"].Value<string>() == "warning");
            if (warns?.Any() == true)
                Assert.Inconclusive($"{warns.Count()} warnings found on {url}");
        }
    }
}
