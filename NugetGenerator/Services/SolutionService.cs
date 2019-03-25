using NugetGenerator.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetGenerator
{
    public class SolutionService
    {

        public static List<FileInfo> GetFilesFromTestSolution(string solutionPath, string[] excludes)
        {
            if (!Directory.Exists(solutionPath))
                return null;

            var dInf = new DirectoryInfo(solutionPath);
            var files = dInf.EnumerateFiles("*", SearchOption.AllDirectories).ToArray();

            var result = new List<FileInfo>();
            foreach (var file in files)
            {
                if (excludes.Contains(file.Extension))
                    continue;
                if (excludes.Contains(file.Name))
                    continue;

                bool include = true;
                foreach (var exclude in excludes)
                {
                    if (file.FullName.Contains(exclude))
                    {
                        include = false;
                        break;
                    }
                }
                if (include)
                    result.Add(file);
            }

            return result;
        }

        public static string WriteFileToNugetSolution(FileInfo origFile, string fileText, string nugetSolutionPath, string testSolutionPath)
        {
            if (origFile == null || !Directory.Exists(nugetSolutionPath))
                return null;

            try
            {
                // Remove test solution path to get relative path
                var origRelativePath = origFile.RelativePath(testSolutionPath);

                // Create file relative path and add .pp extension if needed
                var nugetFileName = origFile.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase) ? origFile.Name + ".pp" : origFile.Name;
                var nugetFileRelativePath = origRelativePath.Replace(origFile.Name, nugetFileName).TrimStart('\\');

                // Ensure content folder exists and is empty
                var contentFolderPath = Path.Combine(nugetSolutionPath, "content");

                // Ensure contentFiles folder exists and is empty
                var contentFilesFolderPath = Path.Combine(nugetSolutionPath, "contentFiles");

                // Write content file
                var contentFilePath = Path.Combine(contentFolderPath, nugetFileRelativePath);
                var fi = new FileInfo(contentFilePath);
                CreateStructure(fi.Directory);
                File.WriteAllText(contentFilePath, fileText);

                // Write contentFiles file
                var contentFilesFilePath = Path.Combine(contentFilesFolderPath, nugetFileRelativePath);
                var fi2 = new FileInfo(contentFilesFilePath);
                CreateStructure(fi2.Directory);
                File.WriteAllText(contentFilesFilePath, fileText);

                return nugetFileRelativePath;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Create folder structure for a given directory.
        /// <para>This is because <see cref="System.IO.Directory"/>.CreateDirectory lies!</para>
        /// </summary>
        /// <param name="dInf"></param>
        private static void CreateStructure(DirectoryInfo dInf)
        {
            if (!dInf.Exists)
            {
                CreateStructure(dInf.Parent);
                Directory.CreateDirectory(dInf.FullName);
            }
        }
    }
}
