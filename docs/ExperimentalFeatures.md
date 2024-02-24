# Experimental Features

The new (.NET 8.0) [Experimental Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.experimentalattribute) is used to mark features that are more likely to change in the future.
**They are usually production-ready**, but we want to gather feedback from the community before we finalize them.

> :warning: In .NET 6 & 7, the `ExperimentalAttribute` is not available and we polyfill it but there is no compiler warning / error that will guide you!

## AkadeIndexedSetEXP0001 - Multi-key string indices
The multi-key string indices API is considered to be experimental because the API is allocation-heavy and might change to a `Span`-based API in the future.

The Feature itself, though, is production-ready, supported and is here to stay.
