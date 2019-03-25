using NugetGenerator.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetGenerator
{
    class Program
    {
        const string basePath = @"H:\Web\NetC\WebTestSuite";
        const string nugetSolutionName = "WebTestSuite";
        const string testSolutionName = "WTS.Tests";
        static readonly string[] excludes =
        {
            "bin",
            "obj",
            ".csproj",
            "packages.config",
            "AssemblyInfo.cs"
        };
        static readonly Dictionary<string, string> targets = new Dictionary<string, string>
        {
            {".NETFramework4.5","net45" },
            {".NETFramework4.6","net46" },
            {".NETFramework4.7","net47" },
            {".NETStandard2.0","netstandard2.0" }
        };

        static void Main(string[] args)
        {
            var nugetSolutionPath = Path.Combine(basePath, nugetSolutionName);
            var testsSolutionPath = Path.Combine(basePath, testSolutionName);

            // Get all files in test solution
            var files = SolutionService.GetFilesFromTestSolution(testsSolutionPath, excludes);

            // Create list for succeeded file writes
            var succeededFilePaths = new List<string>();


            // Ensure content folder exists and is empty
            var contentFolderPath = Path.Combine(nugetSolutionPath, "content");
            if (Directory.Exists(contentFolderPath))
                Directory.Delete(contentFolderPath, true);
            Directory.CreateDirectory(contentFolderPath);

            // Ensure contentFiles folder exists and is empty
            var contentFilesFolderPath = Path.Combine(nugetSolutionPath, "contentFiles");
            if (Directory.Exists(contentFilesFolderPath))
                Directory.Delete(contentFilesFolderPath, true);
            Directory.CreateDirectory(contentFilesFolderPath);

            // Write tests to nuget solution
            foreach (var file in files)
            {
                var ppText = NugetService.AddReplacementPlaceholdersToFile(file, testSolutionName);
                var relativePath = SolutionService.WriteFileToNugetSolution(file, ppText, nugetSolutionPath, testsSolutionPath);
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    Console.WriteLine($"Could not write the file {file.FullName} to nuget solution");
                    continue;
                }
                succeededFilePaths.Add(relativePath);
            }

            // Update .nuspec file
            if(!NugetService.AddContentFilesToNuspec(nugetSolutionPath, succeededFilePaths, targets.Values))
            {
                Console.WriteLine("Failed to write .nuspecfile");
            }
            else
                Console.WriteLine("Nuget project and .nuspec file successfully updated");

            // TODO: Update .sln and .csproj file

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
