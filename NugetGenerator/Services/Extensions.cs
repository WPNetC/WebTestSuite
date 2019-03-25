using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetGenerator.Services
{
    public static class Extensions
    {
        public static string RelativePath(this FileInfo file, string solutionPath)
        {
            if (string.IsNullOrWhiteSpace(solutionPath))
                throw new ArgumentNullException(nameof(solutionPath));

            return file.FullName.Replace(solutionPath, "");
        }
    }
}
