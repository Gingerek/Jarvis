namespace Jarvis.Structure.Tests;

public sealed class StructureBoundaryTests
{
    [Fact]
    public void Core_Project_Has_No_ProjectReferences()
    {
        var root = FindRepositoryRoot();
        var coreProject = Path.Combine(root, "src", "Jarvis.Core", "Jarvis.Core.csproj");
        var content = File.ReadAllText(coreProject);

        Assert.DoesNotContain("ProjectReference", content, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jarvis.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Jarvis repository root was not found.");
    }
}
