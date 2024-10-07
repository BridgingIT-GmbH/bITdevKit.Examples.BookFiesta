using System;
using System.IO;
using System.Linq;
using Nuke.Common;
// using Nuke.Common.CI;
// using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;

// ReSharper disable All
#pragma warning disable CS0618 // Type or member is obsolete

// dotnet tool install Nuke.GlobalTool --global
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.All);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Solution file to build")]
    readonly string SolutionFile;

    [Solution] Solution Solution;

    [Parameter("SonarQube server URL")]
    readonly string SonarServerUrl = "http://localhost:9000";

    [Parameter("SonarQube Docker image")]
    readonly string SonarQubeDockerImage = "sonarqube:latest";

    [Parameter("SonarQube Docker container name")]
    readonly string SonarQubeContainerName = "sonarqube_container_bookfiesta";

    [Parameter("Host path for SonarQube data")]
    readonly AbsolutePath SonarQubeHostPath = RootDirectory / "tools" / "sonarqube" / "data";

    [Parameter("Source directory containing .cs and .md files")]
    readonly AbsolutePath SourceDirectory = RootDirectory / "src";

    [Parameter("Output file path for concatenated files")]
    readonly AbsolutePath DumpCodeOutputFile = RootDirectory / "CodeDump.txt";

    // Default local admin credentials
    readonly string SonarLogin = "admin";
    readonly string SonarPassword = "admin";

    // Use the solution name for both the SonarQube Project Name and Key
    string SonarProjectName => Solution.Name;
    string SonarProjectKey => Solution.Name.Replace(" ", "_").ToLower();

    protected override void OnBuildInitialized()
    {
        if (string.IsNullOrEmpty(SolutionFile))
        {
            Console.WriteLine("Using default solution file.");
        }
        else
        {
            Console.WriteLine($"Using specified solution file: {SolutionFile}");
            Solution = ProjectModelTasks.ParseSolution(SolutionFile);
        }
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            // EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target EnsureSonarQubeContainer => _ => _
        .Executes(() =>
        {
            var isRunning = DockerPs(settings => settings
                .SetQuiet(true)
                .SetFilter($"name={SonarQubeContainerName}"))
                .Any();

            if (!isRunning)
            {
                SonarQubeHostPath.CreateDirectory();

                DockerRun(settings => settings
                    .SetImage(SonarQubeDockerImage)
                    .SetName(SonarQubeContainerName)
                    .SetPublish("9000:9000"));
                    //.SetVolume($"{SonarQubeHostPath}:/opt/sonarqube/data")
                    //.SetDetach(true)
                    //.SetNetwork("host"));

                System.Threading.Thread.Sleep(30000);
            }
            else
            {
                Console.WriteLine("SonarQube container is already running.");
            }
        });

    Target SonarBegin => _ => _
        .DependsOn(EnsureSonarQubeContainer)
        .Before(Compile)
        .Executes(() =>
        {
            SonarScannerTasks.SonarScannerBegin(s => s
                .SetProjectKey(SonarProjectKey)
                .SetName(SonarProjectName)
                .SetServer(SonarServerUrl)
                .SetLogin(SonarLogin)
                .SetPassword(SonarPassword)
                .SetFramework("net5.0"));
        });

    Target SonarEnd => _ => _
        .After(Test)
        .Executes(() =>
        {
            SonarScannerTasks.SonarScannerEnd(s => s
                .SetLogin(SonarLogin)
                .SetPassword(SonarPassword));
        });

    Target SonarAnalysis => _ => _
        .DependsOn(SonarBegin, Test, SonarEnd)
        .Executes(() =>
        {
            Console.WriteLine($"SonarQube analysis complete for project '{SonarProjectName}'.");
            Console.WriteLine($"Project Key: {SonarProjectKey}");
            Console.WriteLine($"Solution analyzed: {Solution.Path}");
            Console.WriteLine($"You can view the results at {SonarServerUrl}");
            Console.WriteLine("Default login credentials are admin/admin. Please change them after first login.");
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild()
                .EnableCollectCoverage()
                .SetCoverletOutputFormat(CoverletOutputFormat.opencover));
                // .SetResultsDirectory(TestResultsDirectory));
        });

    Target DumpCode => _ => _
    .Executes(() =>
    {
        var files = SourceDirectory.GlobFiles("**/*.cs", "**/*.md")
            .Where(file =>
                !file.Name.Equals("GlobalSuppressions.cs", StringComparison.OrdinalIgnoreCase) &&
                !file.Name.Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) &&
                !file.Name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) &&
                !file.ToString().Contains("Migrations") &&
                !file.ToString().Contains(".UnitTests") &&
                !file.ToString().Contains(".IntegrationTests"))
            .ToList();

        var totalFiles = files.Count;
        var processedFiles = 0;
        var csFiles = 0;
        var mdFiles = 0;
        var totalLines = 0;

        Console.WriteLine($"Starting to process {totalFiles} files (.cs and .md)...");

        using (var writer = new StreamWriter(DumpCodeOutputFile))
        {
            foreach (var file in files)
            {
                processedFiles++;
                var fileName = file.Name;
                var extension = Path.GetExtension(fileName).ToLower();

                Console.WriteLine($"Processing: {fileName} ({processedFiles} of {totalFiles})");

                writer.WriteLine($"// File: {file}");
                writer.WriteLine("// ----------------------------------------");

                var lines = File.ReadAllLines(file);
                var contentStartIndex = FindContentStartIndex(lines);
                totalLines += lines.Length - contentStartIndex;

                for (int i = contentStartIndex; i < lines.Length; i++)
                {
                    writer.WriteLine(lines[i]);
                }

                writer.WriteLine();

                if (extension == ".cs")
                    csFiles++;
                else if (extension == ".md")
                    mdFiles++;
            }
        }

        Console.WriteLine($"All files have been concatenated into {DumpCodeOutputFile}");
        Console.WriteLine($"Total files processed: {totalFiles}");
        Console.WriteLine($"C# files: {csFiles}");
        Console.WriteLine($"Markdown files: {mdFiles}");
        Console.WriteLine($"Total lines: {totalLines}");
    });

private int FindContentStartIndex(string[] lines)
{
    for (var i = 0; i < lines.Length; i++)
    {
        if (!string.IsNullOrWhiteSpace(lines[i]) && !lines[i].TrimStart().StartsWith("//"))
        {
            return i;
        }
    }
    return 0;
}

    Target All => _ => _
        .DependsOn(SonarAnalysis, DumpCode)
        .Executes(() =>
        {
            Console.WriteLine("All tasks completed successfully.");
        });
}