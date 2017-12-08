using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ClPropsPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var vswhereTargetPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString("N") + ".ClPropsPatcher.vswhere.exe");
            using (var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ClPropsPatcher.vswhere.exe") as UnmanagedMemoryStream)
            using (var memStream = new MemoryStream())
            {
                resStream.CopyTo(memStream);
                File.WriteAllBytes(vswhereTargetPath, memStream.ToArray());
            }
            var vswhereOutput = Process.Start(new ProcessStartInfo
            {
                FileName = vswhereTargetPath,
                Arguments = "-property installationPath",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }).StandardOutput;
            while (!vswhereOutput.EndOfStream)
            {
                var vsPath = Path.Combine(vswhereOutput.ReadLine(), @"Common7\IDE\VC\VCTargets\Microsoft.Cl.Common.props");
                var props = File.ReadAllText(vsPath);
                props = props.Replace("MultiThreadedDebugDll", "MultiThreadedDebug");
                props = props.Replace("MultiThreadedDll", "MultiThreaded");
                props = props.Replace("MaxSpeed</Optimization>", "Full</Optimization>");
                File.WriteAllText(vsPath, props);
            }
        }
    }
}
