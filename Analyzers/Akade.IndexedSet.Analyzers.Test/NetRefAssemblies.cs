using Microsoft.CodeAnalysis.Testing;
using System.IO;

namespace Akade.IndexedSet.Analyzers.Test;

internal static class NetRefAssemblies
{
    public static ReferenceAssemblies Current { get; } =
#if NET10_0
        new(targetFramework: "net10.0",
            referenceAssemblyPackage: new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
            referenceAssemblyPath: Path.Combine("ref", "net10.0"));
#elif NET11_0
        new(targetFramework: "net11.0",
            referenceAssemblyPackage: new PackageIdentity("Microsoft.NETCore.App.Ref", "11.0.0-rc.2.26504.105"),
            referenceAssemblyPath: Path.Combine("ref", "net11.0"));
#endif
}
