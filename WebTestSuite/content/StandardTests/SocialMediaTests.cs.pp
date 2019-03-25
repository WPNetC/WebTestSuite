using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using $rootnamespace$.Common;

namespace $rootnamespace$.StandardTests
{
    [TestClass]
    public class SocialMediaTests : WebTestBase
    {
        /// <summary>
        /// This is the address the tests will be run against.
        /// <para>Edit this to alter the page being tested.</para>
        /// </summary>
        private static string _testingUrl = $"{PathsAndPorts.BASE_ADDR}/";

        /// <summary>
        /// The set of possible Open Graph tags to test for.
        /// <para>Setting the value to false will skip the test</para>
        /// </summary>
        private static readonly Dictionary<string, bool> _oGTagsToTest = new Dictionary<string, bool>
        {
            { "description", true },
            { "image", true },
            { "site_name", true },
            { "title", true },
            { "type", true },
            { "updated_time", true },
            { "url", true }
        };

        /// <summary>
        /// The set of possible Twitter tags to test for.
        /// <para>Setting the value to false will skip the test</para>
        /// </summary>
        private static readonly Dictionary<string, bool> _twitterTagsToTest = new Dictionary<string, bool>
        {
            { "card", true },
            { "creator", true },
            { "description", true },
            { "image", true },
            { "title", true },
            { "site", true },
            { "url", true }
        };

        [TestMethod]
        public void HasOpenGraphTags()
        {
            var results = _oGTagsToTest.Where(p => p.Value == true).ToDictionary(k => k.Key, v => v.Value);

            foreach (var item in _oGTagsToTest)
            {
                if (item.Value == false)
                    continue;

                try
                {
                    TestSingleNode(_testingUrl, $"//meta[@property=\"og:{item.Key}\"]", $"Open Graph Tag: {item.Key}", true, "content");
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    results[item.Key] = false;
                }
            }

            // Check all results are true or report failed tests.
            Assert.IsTrue(results.All(p => p.Value == true),
                $"Failed to get following Open Graph tags: {string.Join(", ", results.Where(p => p.Value == false).Select(p => p.Key))}. See debug output for details.");
        }

        [TestMethod]
        public void HasTwitterCards()
        {
            var results = _twitterTagsToTest
                .Where(p => p.Value == true)
                .ToDictionary(k => k.Key, v => v.Value);

            foreach (var item in _twitterTagsToTest)
            {
                if (item.Value == false)
                    continue;

                try
                {
                    TestSingleNode(_testingUrl, $"//meta[@name=\"twitter:{item.Key}\"]", $"Twitter Tag: {item.Key}", true, "content");
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    results[item.Key] = false;
                }
            }

            // Check all results are true or report failed tests.
            Assert.IsTrue(results.All(p => p.Value == true),
                $"Failed to get following Twitter tags: {string.Join(", ", results.Where(p => p.Value == false).Select(p => p.Key))}. See debug output for details.");
        }

        private void LogException(Exception ex, [CallerMemberName] string name = "")
        {
            var msg = ex.Message.Split('.')[2];
            Debug.WriteLine($"TEST FAILED: {name} | {msg}");
        }
    }
}
