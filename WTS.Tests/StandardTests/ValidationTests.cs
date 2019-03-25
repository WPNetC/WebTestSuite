using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using WTS.Tests.Common;

namespace WTS.Tests.StandardTests
{
    [TestClass]
    public class ValidationTests : WebTestBase
    {
        private static readonly string[] _pagesToTest =
        {
            $"http://{PathsAndPorts.BASE_ADDR}/"
        };
        private static readonly string[] _styleSheets =
        {
            //TODO: Add in means to scrape project for .css files
        };


        [TestMethod]
        public void PagesPassW3CHTMLValidation()
        {
            var contentType = "text/html";

            var validationResults = new List<HTMLValidatorResonse>();
            for (int ii = 0; ii < _pagesToTest.Length; ii++)
            {
                var url = _pagesToTest[ii];
                try
                {
                    var html = "";
                    using (var wc = new WebClient())
                        html = wc.DownloadString(url);
                    validationResults.AddRange(ValidateWithW3C(contentType, html, url));
                }
                catch (Exception ex)
                {

                }

                // This is a free API and requests a 1 sec delay between checks
                if (ii + 1 < _pagesToTest.Length)
                    Thread.Sleep(1000);
            }

            ProcessValidationResults(validationResults);
        }

        [TestMethod]
        public void StylesPassW3CValidation()
        {
            var contentType = "text/css";

            var validationResults = new List<HTMLValidatorResonse>();
            for (int ii = 0; ii < _styleSheets.Length; ii++)
            {
                var filePath = _styleSheets[ii];
                var fileText = File.ReadAllText(filePath);
                try
                {
                    validationResults.AddRange(ValidateWithW3C(contentType, fileText, filePath));
                }
                catch (Exception ex)
                {

                }

                // This is a free API and requests a 1 sec delay between checks
                if (ii + 1 < _pagesToTest.Length)
                    Thread.Sleep(1000);
            }

            ProcessValidationResults(validationResults);
        }


        private void ProcessValidationResults(List<HTMLValidatorResonse> validationResults)
        {
            // Get errors and warnings
            var errors = validationResults
                .Where(p => p.Type == "error")
                .OrderBy(p => p.LastLine)
                .ToList();

            var warns = validationResults
                .Where(p => p.Type == "info")
                .OrderBy(p => p.LastLine)
                .ToList();

            // If have errors we will fail.
            if (errors?.Any() == true)
            {
                // Add warnings so all messages are delivered
                errors.AddRange(warns);
                var output = errors.GroupBy(p => p.PathOrUrl);

                string errorString = "";
                foreach (var group in output)
                {
                    errorString += FormatValidatorErrorMessages(group, group.Key);
                }

                if (int.TryParse(ConfigurationManager.AppSettings["W3CH_HTML_ERROR_FAIL_COUNT"], out int failCount))
                {
                    // If we have set a fail count we check that before failing.
                    if (errors.Count() > failCount)
                        Assert.Fail(errorString);
                }
                else
                {
                    // If not fail count we will fail on any errors.
                    Assert.Fail(errorString);
                }
            }

            // If have warnings the test is inconclusive
            if (warns?.Any() == true)
            {
                var output = warns.GroupBy(p => p.PathOrUrl);

                string warningsString = "";
                foreach (var group in output)
                {
                    warningsString += FormatValidatorErrorMessages(group, group.Key);
                }

                if (int.TryParse(ConfigurationManager.AppSettings["W3CH_HTML_WARN_FAIL_COUNT"], out int failCount))
                {
                    // If we have set a fail count we check that before failing.
                    if (warns.Count() > failCount)
                        Assert.Fail(warningsString);
                }

                // If no fail count we consider warnings as inconclusive
                Assert.Inconclusive(warningsString);
            }
        }

        private HTMLValidatorResonse[] ValidateWithW3C(string contentType, string content, string pathOrUrl)
        {
            // Parse response to object array
            HTMLValidatorResonse[] validationResults = null;

            // Create REST request
            var client = new RestClient("https://validator.w3.org/nu/?out=json");
            var request = new RestRequest(Method.POST);
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", $"{contentType}; charset=utf-8");
            request.AddParameter("", content, ParameterType.RequestBody);

            // Get resonse
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    validationResults = JObject.Parse(response.Content)["messages"]
                        .ToObject<HTMLValidatorResonse[]>();

                    // We need this to know where we got the errors
                    foreach (var item in validationResults)
                    {
                        item.PathOrUrl = pathOrUrl;
                    }
                }
                catch
                {

                }
            }

            return validationResults;
        }

        private string FormatValidatorErrorMessages(IEnumerable<HTMLValidatorResonse> tokens, string url)
        {
            var line1 = "Message: {0}" + Environment.NewLine;
            var line2 = "Extract: {0}" + Environment.NewLine;
            var line3 = "From line {0}, column {1}; to line {2}, column {3}" + Environment.NewLine;

            var formatted = string.Join(Environment.NewLine,
                tokens.Select(p =>
                string.Format(line1, p.Message) +
                string.Format(line2, p.Extract) +
                string.Format(line3, p.FirstLine ?? p.LastLine, p.FirstColumn, p.LastLine, p.LastColumn)
                ));

            return $"{Environment.NewLine + Environment.NewLine}{tokens.Count()} {tokens.First().SubType ?? tokens.First().Type}s found on {url}.{Environment.NewLine + Environment.NewLine}{formatted}";
        }

        internal class HTMLValidatorResonse
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("subType")]
            public string SubType { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("extract")]
            public string Extract { get; set; }
            [JsonProperty("firstLine")]
            public int? FirstLine { get; set; }
            [JsonProperty("firstColumn")]
            public int FirstColumn { get; set; }
            [JsonProperty("lastLine")]
            public int LastLine { get; set; }
            [JsonProperty("lastColumn")]
            public int LastColumn { get; set; }
            [JsonProperty("hiliteStart")]
            public int HiliteStart { get; set; }
            [JsonProperty("hiliteLength")]
            public int HiliteLength { get; set; }
            [JsonIgnore]
            public string PathOrUrl { get; set; }
        }
    }
}
