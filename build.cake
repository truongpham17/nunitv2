#tool "nuget:?package=WiX.Toolset"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// CHANGE TO SET PACKAGE AND ASSEMBLY VERSIONS
//////////////////////////////////////////////////////////////////////

var version = "2.6.6";
var modifier = ""; // for example "-beta2"

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

var DBG_SUFFIX = configuration == "Debug" ? "-dbg" : "";
var PACKAGE_VERSION = version + modifier + DBG_SUFFIX;
var PACKAGE_BASE_NAME = "NUnit-" + PACKAGE_VERSION;

var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var SOLUTION_FILE = PROJECT_DIR + "nunitv2.sln";

var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";
var NUNIT_CONSOLE = BIN_DIR + "nunit-console.exe";

var LIB_DIR = PROJECT_DIR + "lib/";

var INSTALL_DIR = PROJECT_DIR + "install/";

var NUGET_DIR = PROJECT_DIR + "nuget/";

var PACKAGE_DIR = PROJECT_DIR + "packages/";
var PACKAGE_WORK_DIR = PACKAGE_DIR + PACKAGE_BASE_NAME + "/";
var PACKAGE_BIN_DIR = PACKAGE_WORK_DIR + "bin/";
var PACKAGE_LIB_DIR = PACKAGE_WORK_DIR + "lib/";

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Deletes all files in the BIN directory")
    .Does(() =>
    {
        CleanDirectory(BIN_DIR);
    });

Task("CleanPackageWorkDir")
	.Description("Deletes all files in the package work directory")
	.Does(() =>
	{
		CleanDirectory(PACKAGE_WORK_DIR);
	});

//////////////////////////////////////////////////////////////////////
// Build
//////////////////////////////////////////////////////////////////////

Task("Build")
	.Description("Builds the Solution")
	.Does(() =>
	{
		// Copy down library files 
		// TODO: Replace with packages
		CopyFilesToDirectory(LIB_DIR + "**/*", BIN_DIR + "lib/");

		MSBuild(SOLUTION_FILE, new MSBuildSettings()
			.SetConfiguration(configuration)
			.SetVerbosity(Verbosity.Minimal));

		// Extra copy of pnunit.framework
		CopyFileToDirectory(BIN_DIR + "pnunit.framework.dll", BIN_DIR + "framework/");
		CopyFile(PROJECT_DIR + "NUnitTests.v2.nunit", BIN_DIR + "NUnitTests.nunit");
		CopyFile(PROJECT_DIR + "NUnitTests.config", BIN_DIR + "NUnitTests.config");
	});

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("BasicTests")
	.Description("Runs the tests")
	.IsDependentOn("Build")
	.Does(() =>
	{
		int rc = StartProcess(
			NUNIT_CONSOLE, 
			new ProcessSettings()
			{
				WorkingDirectory = BIN_DIR,
				Arguments = "NUnitTests.nunit"
			});

		if (rc > 0)
			throw new Exception(string.Format("{0} tests failed", rc));
		else if (rc < 0)
			throw new Exception(string.Format("Console returned rc = {0}", rc));
	});

Task("Net45Tests")
	.Description("Runs the .NET 4.5 tests")
	.IsDependentOn("Build")
	.Does(() =>
	{
		int rc = StartProcess(
			NUNIT_CONSOLE,
			new ProcessSettings()
			{
				WorkingDirectory = BIN_DIR + "tests",
				Arguments = "nunit.core.tests.net45.dll nunit.framework.tests.net45.dll -noxml -framework:net-4.5"
			});

		if (rc > 0)
			throw new Exception(string.Format("{0} tests failed", rc));
		else if (rc < 0)
			throw new Exception(string.Format("Console returned rc = {0}", rc));
	});

//////////////////////////////////////////////////////////////////////
// Package
//////////////////////////////////////////////////////////////////////

Task("CreatePackageDir")
	.Description("Creates the package directory")
	.Does(() =>
	{
		CreateDirectory(PACKAGE_DIR);
	});

Task("PackageSource")
	.Description("Create Source Package")
	.IsDependentOn("CreatePackageDir")
	.Does(() =>
	{
		string zipOutput = PACKAGE_DIR + PACKAGE_BASE_NAME + "-src.zip";
		int rc = StartProcess("git", "archive --format=zip --output=" + zipOutput + " HEAD");
	});

Task("BuildInstallImage")
	.Description("Build the install image for zip or msi")
	.IsDependentOn("CreatePackageDir")
	.IsDependentOn("CleanPackageWorkDir")
	.Does(() =>
	{
		CopyFiles(
			new FilePath[] {
				"license.txt",
				"src/GuiRunner/nunit-gui/Logo.ico"
			},
			PACKAGE_WORK_DIR);

		// TODO: Create a method to handle wildcard directories
		CopyFilesToDirectory(BIN_DIR + "*", PACKAGE_BIN_DIR);
		CopyFilesToDirectory(BIN_DIR + "lib/*", PACKAGE_BIN_DIR + "lib/");
		CopyFilesToDirectory(BIN_DIR + "lib/Images/*", PACKAGE_BIN_DIR + "lib/Images/");
		CopyFilesToDirectory(BIN_DIR + "lib/Images/Tree/Circles/*", PACKAGE_BIN_DIR + "lib/Images/Tree/Circles/");
		CopyFilesToDirectory(BIN_DIR + "lib/Images/Tree/Classic/*", PACKAGE_BIN_DIR + "lib/Images/Tree/Classic/");
		CopyFilesToDirectory(BIN_DIR + "lib/Images/Tree/Default/*", PACKAGE_BIN_DIR + "lib/Images/Tree/Default/");
		CopyFilesToDirectory(BIN_DIR + "lib/Images/Tree/Visual Studio/*", PACKAGE_BIN_DIR + "lib/Images/Tree/Visual Studio/");
		CopyFilesToDirectory(BIN_DIR + "tests/*", PACKAGE_BIN_DIR + "tests/");
		CopyFilesToDirectory(BIN_DIR + "framework/*", PACKAGE_BIN_DIR + "framework/");
	});

Task("PackageZip")
	.Description("Create Binary Zip Package")
	.IsDependentOn("BuildInstallImage")
	.Does(() =>
	{
		var zipOutput = PACKAGE_DIR + PACKAGE_BASE_NAME + ".zip";
		Zip(PACKAGE_WORK_DIR, zipOutput);
	});

Task("PackageMsi")
	.Description("Create the MSI Installer")
	.IsDependentOn("BuildInstallImage")
	.Does(() =>
	{
		WiXCandle(
			INSTALL_DIR + "*.wxs",

			new CandleSettings()
			{
				Defines = new Dictionary<string, string>()
				{
					{"ProductVersion", version},
					{"NominalVersion", PACKAGE_VERSION},
					{"TargetRuntime", "net-3.5"},
					{"InstallImage", PACKAGE_WORK_DIR}
				},

				OutputDirectory = PACKAGE_WORK_DIR
			});

		WiXLight(
			PACKAGE_WORK_DIR + "*.wixobj", 

			new LightSettings()
			{
				Extensions = new [] { "WixUiExtension" },

				OutputFile = PACKAGE_DIR + PACKAGE_BASE_NAME + ".msi"
			});
	});

Task("PackageNuGet")
	.Description("Create the nuget packages")
	.IsDependentOn("BuildInstallImage")
	.Does(() =>
	{
		foreach (var nuspecFile in GetFiles(NUGET_DIR + "*.nuspec"))
			NuGetPack(nuspecFile, new NuGetPackSettings()
			{
				Version = PACKAGE_VERSION,
				BasePath = PACKAGE_WORK_DIR,
				OutputDirectory = PACKAGE_DIR,
				NoPackageAnalysis = true
			});
	});

//////////////////////////////////////////////////////////////////////
// HELPER METHODS
//////////////////////////////////////////////////////////////////////

void CopyFilesToDirectory(string pattern, string toDir)
{
	if (!DirectoryExists(toDir))
		CreateDirectory(toDir);

	CopyFiles(pattern, toDir);
}

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Rebuild")
	.IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("Test")
	.IsDependentOn("BasicTests")
	.IsDependentOn("Net45Tests");

Task("Package")
	.IsDependentOn("PackageSource")
	.IsDependentOn("PackageZip")
	.IsDependentOn("PackageMsi")
	.IsDependentOn("PackageNuGet");

Task("AppVeyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

Task("Default")
	.IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
