using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using AsmResolver.PE;
using AsmResolver.PE.File;
using AsmResolver.PE.Win32Resources.Icon;
using Velopack.NuGet;
using Velopack.Packaging.Windows;

namespace Velopack.Packaging.Tests;

public class ResourceEditTests
{
    private readonly ITestOutputHelper _output;

    public ResourceEditTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private void CreateTestPEFileWithoutRsrc(string tempFile)
    {
        var peBuilder = new ManagedPEBuilder(
            PEHeaderBuilder.CreateExecutableHeader(),
            new MetadataRootBuilder(new MetadataBuilder()),
            ilStream: new BlobBuilder());
        var peImageBuilder = new BlobBuilder();
        peBuilder.Serialize(peImageBuilder);

        using var fs = File.OpenWrite(tempFile);
        fs.Write(peImageBuilder.ToArray());
    }

    [Fact]
    public void CopyResourcesWithoutRsrc()
    {
        using var logger = _output.BuildLoggerFor<ResourceEditTests>();

        using var _1 = Utility.GetTempFileName(out var tempFile);
        CreateTestPEFileWithoutRsrc(tempFile);

        var edit = new ResourceEdit(tempFile, logger);
        edit.CopyResourcesFrom(PathHelper.GetFixture("atom.exe"));
        edit.Commit();

        var versionInfo = FileVersionInfo.GetVersionInfo(tempFile);
        Assert.Equal("0.16.2", versionInfo.FileVersion);
        Assert.Equal("0.132.0-f4b23b8", versionInfo.ProductVersion);
        Assert.StartsWith("Copyright (C) 2014 GitHub", versionInfo.LegalCopyright);
        Assert.Equal("Atom", versionInfo.ProductName);
        Assert.Equal("Atom", versionInfo.FileDescription);
        Assert.Equal("GitHub, Inc.", versionInfo.CompanyName);
    }

    [Fact]
    public void CopyResourcesWithPreExistingRsrc()
    {
        using var logger = _output.BuildLoggerFor<ResourceEditTests>();

        using var _1 = Utility.GetTempFileName(out var tempFile);
        var exe = PathHelper.GetFixture("SquirrelAwareTweakedNetCoreApp.exe");
        File.Copy(exe, tempFile);

        var edit = new ResourceEdit(tempFile, logger);
        edit.CopyResourcesFrom(PathHelper.GetFixture("atom.exe"));
        edit.Commit();

        var versionInfo = FileVersionInfo.GetVersionInfo(tempFile);
        Assert.Equal("0.16.2", versionInfo.FileVersion);
        Assert.Equal("0.132.0-f4b23b8", versionInfo.ProductVersion);
        Assert.StartsWith("Copyright (C) 2014 GitHub", versionInfo.LegalCopyright);
        Assert.Equal("Atom", versionInfo.ProductName);
        Assert.Equal("Atom", versionInfo.FileDescription);
        Assert.Equal("GitHub, Inc.", versionInfo.CompanyName);
    }

    [Fact]
    public void SetIconWithPreExistingRsrc()
    {
        using var logger = _output.BuildLoggerFor<ResourceEditTests>();

        using var _1 = Utility.GetTempFileName(out var tempFile);
        var exe = PathHelper.GetFixture("atom.exe");
        File.Copy(exe, tempFile);

        var beforeRsrc = PEImage.FromFile(PEFile.FromFile(tempFile)).Resources;
        Assert.NotNull(beforeRsrc);
        var beforeIcon = IconResource.FromDirectory(beforeRsrc);
        Assert.Single(beforeIcon.GetIconGroups());
        Assert.Equal(6, beforeIcon.GetIconGroups().ToList()[0].GetIconEntries().Count());

        var edit = new ResourceEdit(tempFile, logger);
        edit.SetExeIcon(PathHelper.GetFixture("clowd.ico"));
        edit.Commit();

        var afterRsrc = PEImage.FromFile(PEFile.FromFile(tempFile)).Resources;
        Assert.NotNull(afterRsrc);
        var afterIcon = IconResource.FromDirectory(afterRsrc);
        Assert.Single(afterIcon.GetIconGroups());
        Assert.Equal(7, afterIcon.GetIconGroups().ToList()[0].GetIconEntries().Count());
    }

    [Fact]
    public void SetIconWithoutRsrc()
    {
        using var logger = _output.BuildLoggerFor<ResourceEditTests>();

        using var _1 = Utility.GetTempFileName(out var tempFile);
        CreateTestPEFileWithoutRsrc(tempFile);

        var beforeRsrc = PEImage.FromFile(PEFile.FromFile(tempFile)).Resources;
        Assert.Null(beforeRsrc);

        var edit = new ResourceEdit(tempFile, logger);
        edit.SetExeIcon(PathHelper.GetFixture("clowd.ico"));
        edit.Commit();

        var afterRsrc = PEImage.FromFile(PEFile.FromFile(tempFile)).Resources;
        Assert.NotNull(afterRsrc);
        var afterIcon = IconResource.FromDirectory(afterRsrc);
        Assert.Single(afterIcon.GetIconGroups());
        Assert.Equal(7, afterIcon.GetIconGroups().ToList()[0].GetIconEntries().Count());
    }

    [Fact]
    public void SetVersionInfoWithPreExistingRsrc()
    {
        using var logger = _output.BuildLoggerFor<ResourceEditTests>();
        using var _1 = Utility.GetTempFileName(out var tempFile);
        var exe = PathHelper.GetFixture("atom.exe");
        File.Copy(exe, tempFile);

        var nuspec = PathHelper.GetFixture("FullNuspec.nuspec");
        var manifest = PackageManifest.ParseFromFile(nuspec);
        var pkgVersion = manifest.Version!;

        var edit = new ResourceEdit(tempFile, logger);
        edit.SetVersionInfo(manifest);
        edit.Commit();

        var versionInfo = FileVersionInfo.GetVersionInfo(tempFile);
        Assert.Equal(pkgVersion.ToFullString(), versionInfo.FileVersion);
        Assert.Equal(pkgVersion.ToFullString(), versionInfo.ProductVersion);
        Assert.Equal(manifest.ProductCopyright, versionInfo.LegalCopyright);
        Assert.Equal(manifest.ProductName, versionInfo.ProductName);
        Assert.Equal(manifest.ProductDescription, versionInfo.FileDescription);
        Assert.Equal(manifest.ProductCompany, versionInfo.CompanyName);
    }

    [Fact]
    public void SetVersionInfoWithoutRsrc()
    {
        using var logger = _output.BuildLoggerFor<ResourceEditTests>();
        using var _1 = Utility.GetTempFileName(out var tempFile);
        CreateTestPEFileWithoutRsrc(tempFile);

        var nuspec = PathHelper.GetFixture("FullNuspec.nuspec");
        var manifest = PackageManifest.ParseFromFile(nuspec);
        var pkgVersion = manifest.Version!;

        var edit = new ResourceEdit(tempFile, logger);
        edit.SetVersionInfo(manifest);
        edit.Commit();

        var versionInfo = FileVersionInfo.GetVersionInfo(tempFile);
        Assert.Equal(pkgVersion.ToFullString(), versionInfo.FileVersion);
        Assert.Equal(pkgVersion.ToFullString(), versionInfo.ProductVersion);
        Assert.Equal(manifest.ProductCopyright, versionInfo.LegalCopyright);
        Assert.Equal(manifest.ProductName, versionInfo.ProductName);
        Assert.Equal(manifest.ProductDescription, versionInfo.FileDescription);
        Assert.Equal(manifest.ProductCompany, versionInfo.CompanyName);
    }
}
