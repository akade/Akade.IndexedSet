using Microsoft.CodeAnalysis.Testing;
using System.IO;

namespace Akade.IndexedSet.Analyzers.Test;

internal static class NetRefAssemblies
{
    public static ReferenceAssemblies Current { get; } =
#if NET8_0
        ReferenceAssemblies.Net.Net80;
#elif NET9_0
        ReferenceAssemblies.Net.Net90;
#elif NET10_0
        new(targetFramework: "net10.0",
            referenceAssemblyPackage: new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
            referenceAssemblyPath: Path.Combine("ref", "net10.0"));
#endif
}
