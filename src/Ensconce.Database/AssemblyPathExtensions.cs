using System;
using System.IO;
using System.Reflection;

namespace Ensconce.Database
{
    public static class AssemblyPathExtensions
    {
        public static string Directory(this Assembly assembly)
        {
            var exePath = new Uri(assembly.CodeBase).LocalPath;
            return Path.GetDirectoryName(exePath);
        }
    }
}
