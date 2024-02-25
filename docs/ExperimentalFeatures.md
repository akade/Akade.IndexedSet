# Experimental Features

The new (.NET 8.0) [Experimental Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.experimentalattribute) is used to mark features that are more likely to change in the future.
**They are usually production-ready**, but changes in future releases will not be considered breaking changes.

> :warning: In .NET 6 & 7, the `ExperimentalAttribute` is not available and we polyfill it but there is no compiler warning / error that will guide you!

## AkadeIndexedSetEXP0001 
:information_source: **Multi-key string indices**

The multi-key string indices API is considered to be experimental because the API is allocation-heavy and might change to a `Span`-based API in the future.

However, the feature itself is production-ready, supported and is here to stay.
