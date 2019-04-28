using System;
using System.IO;

namespace Contoso.GameNetCore.GameSockets.ConformanceTest
{
    public class Helpers
    {
        public static string GetApplicationPath(string projectName)
        {
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, projectName));
        }
    }
}
