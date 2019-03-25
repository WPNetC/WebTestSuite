using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text.RegularExpressions;
using WTS.Tests.Common;

namespace WTS.Tests.StandardTests
{
    [TestClass]
    public class ContentTests : WebTestBase
    {
        /// <summary>
        /// This is the address the tests will be run against.
        /// <para>Edit this to alter the page being tested.</para>
        /// </summary>
        private static string _testingUrl = $"http://{PathsAndPorts.BASE_ADDR}/";

        [TestMethod]
        public void HasCustom404Page()
        {
            var url = $"{_testingUrl}hsjkfhkjdhfsfknsfnsfnskf";
            var response = TryGetBadUrl(url);            

            if(response == null || response.Item1 != HttpStatusCode.NotFound)
            {
                // If we did not receive a 404 status we count this as a fail.
                Assert.Fail($"Did not receive a 404 from {url}. Instead received {response.Item1}");
            }

            // Try to get title tag. Use outer html to help limit the regex.
            var title = response.Item2.DocumentNode.SelectSingleNode("//title")?.OuterHtml;
            if (!string.IsNullOrWhiteSpace(title))
            {
                // Compare title tag content to default IIS 404 content
                var rgx = "<title>IIS [\\s\\S]+? Error - 404[\\s\\S]+?</title>";
                Assert.IsFalse(Regex.IsMatch(title, rgx, RegexOptions.IgnoreCase), "Default IIS 404 page found");
            }
        }

        [TestMethod]
        public void HasCustom50xPage()
        {
            // TODO: Create test page that throws a 50x error!
            var url = $"{_testingUrl}hsjkfhkjdhfsfknsfnsfnskf";
            var response = TryGetBadUrl(url);

            if (response == null || (int)response.Item1 < 500 || (int)response.Item1 >= 600)
            {
                // If we did not receive a 404 status we count this as a inconclusive.
                Assert.Inconclusive($"Did not receive a 50x from {url}. Instead received {response.Item1}");
            }

            // Try to get title tag. Use outer html to help limit the regex.
            var title = response.Item2.DocumentNode.SelectSingleNode("//title")?.OuterHtml;
            if (!string.IsNullOrWhiteSpace(title))
            {
                // Compare title tag content to default IIS 50x content
                var rgx = "<title>50[\\s\\S]?.* server error[\\s\\S]?.*</title>";
                Assert.IsFalse(Regex.IsMatch(title, rgx, RegexOptions.IgnoreCase), "Default IIS 404 page found");
            }
        }
    }
}
