
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Program
{
    private const string DefaultAdministrationPath = "/var/lib/dpkg";

    public static void Main()
    {
/*
        var stream = new StreamReader(process.OpenStandardOutputStream());
        var cachedPackages = new Dictionary<string, DebianPackageInformation>();

        while (true)
        {
            var line = stream.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                break;
            }

            if (line.StartsWith(DebianPackageInformation.NameProperty))
            {
                ref DebianPackageInformation information = ref CollectionsMarshal.GetValueRefOrAddDefault(cachedPackages, null!, out bool existed)!;

                if (!existed)
                {
                    information = new DebianPackageInformation
                    {
                        Name = line.Substring(DebianPackageInformation.NameProperty.Length),
                    };
                }
            }
        }
*/
    }
}