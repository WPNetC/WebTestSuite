using System.Configuration;

namespace WTS.Tests.Common
{
    /// <summary>
    /// Base paths and ports to use for tests
    /// </summary>
    internal static class PathsAndPorts
    {
        /*
         * Don't use new syntax to maximise compatibility
         */
        public static string BASE_ADDR { get { return ConfigurationManager.AppSettings["BASE_ADDR"]; } }
        public static int HTTP_PORT
        {
            get
            {
                int port = 80;
                int.TryParse(ConfigurationManager.AppSettings["HTTP_PORT"], out port);
                return port;

            }
        }
        public static int HTTPS_PORT {
            get
            {
                int port = 443;
                int.TryParse(ConfigurationManager.AppSettings["HTTPS_PORT"], out port);
                return port;
            }
        }
        public static string ROBOTS_ADR { get { return ConfigurationManager.AppSettings["ROBOTS_ADR"]; } }
        public static string SITEMAP_ADR { get { return ConfigurationManager.AppSettings["SITEMAP_ADR"]; } }
    }
}
