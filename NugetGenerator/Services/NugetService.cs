using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace NugetGenerator.Services
{
    public class NugetService
    {
        public static string AddReplacementPlaceholdersToFile(FileInfo file, string testsNamespace)
        {
            if (!file.Exists)
                return null;

            var origText = File.ReadAllText(file.FullName);
            if (file.Extension.ToLower() != ".cs")
                return origText;

            var namespaced = Regex.Replace(origText, testsNamespace, "$rootnamespace$", RegexOptions.IgnoreCase);

            return namespaced;
        }

        public static bool AddContentFilesToNuspec(string nugetSolutionPath, IEnumerable<string> filePaths, IEnumerable<string> targets, string nuspecFileName = "")
        {
            // Check solution folder exists and we have files to add
            if (!Directory.Exists(nugetSolutionPath) || filePaths?.Any() == false)
                return false;

            var nuspecFiles = Directory.EnumerateFiles(nugetSolutionPath, "*.nuspec", SearchOption.AllDirectories).ToArray();

            // Check we either only have 1 nuspec file or have been given a filename
            if (nuspecFiles?.Any() == false
                || (nuspecFiles.Length > 1 && string.IsNullOrWhiteSpace(nuspecFileName)))
                return false;

            var nuspecFile = nuspecFiles.Length == 1 ? nuspecFiles.First() : nuspecFiles.FirstOrDefault(p => p.Contains(nuspecFileName));

            // Check we found the nuspec file
            if (nuspecFile == null || !File.Exists(nuspecFile))
                return false;

            try
            {
                // Load nuspec file as xml doc
                var doc = new System.Xml.XmlDocument();
                doc.Load(nuspecFile);

                // Get metadata node
                var metaNode = doc.DocumentElement.FirstChild;
                if (metaNode is null)
                {
                    return false;
                }

                // Get or create version and contentFiles nodes
                XmlNode versionNode = null;
                XmlNode contentFilesNode = null;

                var enu = metaNode.ChildNodes.GetEnumerator();
                while (enu.MoveNext())
                {
                    if (enu.Current is XmlNode childNode)
                    {
                        if (childNode.Name == "version")
                        {
                            versionNode = childNode;
                            if (contentFilesNode is null)
                                continue;
                            else
                                break;
                        }
                        if (childNode.Name == "contentFiles")
                        {
                            contentFilesNode = childNode;
                            if (versionNode is null)
                                continue;
                            else
                                break;
                        }
                    }
                }

                if (versionNode is null)
                    versionNode = metaNode.AppendChild(doc.CreateElement("version"));
                if (contentFilesNode is null)
                    contentFilesNode = metaNode.AppendChild(doc.CreateElement("contentFiles"));

                // Clear current contentFiles children
                contentFilesNode.RemoveAll();

                // Add new contentFiles nodes
                foreach (var file in filePaths)
                {
                    foreach (var target in targets)
                    {
                        var cfNode = doc.CreateElement("files");
                        cfNode.SetAttribute("include", $"any/{target}/contentFiles/{file}");
                        cfNode.SetAttribute("buildAction", "Content");
                        contentFilesNode.AppendChild(cfNode);
                    }
                }

                // Add new content nodes
                foreach (var file in filePaths)
                {
                    foreach (var target in targets)
                    {
                        var cfNode = doc.CreateElement("files");
                        cfNode.SetAttribute("include", $"any/{target}/content/{file}");
                        cfNode.SetAttribute("buildAction", "Content");
                        contentFilesNode.AppendChild(cfNode);
                    }
                }

                // Update version
                var versionText = string.IsNullOrWhiteSpace(versionNode.InnerText) ? "0.0.0.0" : versionNode.InnerText;
                var oldVersion = Version.Parse(versionText);
                var version = new Version(oldVersion.Major, oldVersion.Minor, oldVersion.Build, oldVersion.Revision + 1);
                versionNode.InnerText = version.ToString();

                doc.Save(nuspecFile);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
