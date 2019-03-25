using Microsoft.VisualStudio.TestTools.UnitTesting;
using $rootnamespace$.Common;

namespace $rootnamespace$.StandardTests
{
    [TestClass]
    public class SEOTests : WebTestBase
    {
        /// <summary>
        /// This is the address the tests will be run against.
        /// <para>Edit this to alter the page being tested.</para>
        /// </summary>
        private static string _testingUrl = $"{PathsAndPorts.BASE_ADDR}/";

        [TestMethod]
        public void PageHasTitleTag()
        {
            TestSingleNode(_testingUrl, "//title", "title tag", true);
        }

        [TestMethod]
        public void PageHasShortcutIcon()
        {
            TestSingleNode(_testingUrl, "//link[@rel=\"shortcut icon\"]", "shortcut icon", true, "href");
        }

        [TestMethod]
        public void PageHasMetaKeywords()
        {
            TestSingleNode(_testingUrl, "//meta[@name=\"keywords\"]", "meta keywords", true, "content");
        }

        [TestMethod]
        public void PageHasMetaDescription()
        {
            TestSingleNode(_testingUrl, "//meta[@name=\"description\"]", "meta description", true, "content");
        }
    }
}
