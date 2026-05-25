using System.Runtime.CompilerServices;
using APTPackageDependenciesResolver;

public class Program
{

    public static void Main()
    {
        DebianPackageReader reader = new DebianPackageReader();
        List<DebianPackage> installedPackages = reader.GetAllInstalledPackages();

        foreach (var pkg in installedPackages)
        {
            Console.WriteLine(pkg.Name);
        }
    }
}