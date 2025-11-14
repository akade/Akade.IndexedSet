# Experimental Features

The new (since .NET 8.0) [Experimental Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.experimentalattribute) is used to mark features that are more likely to change in the future.
**They are usually production-ready and the use-case is here to stay**, unless stated differently. However, changes, such as
small differences in the API will not be considered breaking changes in future releases.

## AkadeIndexedSetEXP0001 
:information_source: **Multi-key string indices**

The multi-key string indices API is considered to be experimental because the API is allocation-heavy and might change to a `Span`-based API in the future.

However, the feature itself is production-ready, supported and is here to stay.

## AkadeIndexedSetEXP0002 
:information_source: **Intersection-Queries**

The query method `Intersects` currently accepts `min` and `max` defining the range of the intersection.
Currently, the entire API is point-based in the same way even though the underlying index actually works
on much more generalized axis-aligned bounding boxes. Therefore, the API might change in the future to accept
a bounding box instead of min/max points.

However, the feature itself is production-ready, supported and is here to stay.

## AkadeIndexedSetEXP0003 
:information_source: **Vector index (approximate nearest neighbor search)**

The vector index provides approximate nearest neighbor (ANN) search for high-dimensional numeric vectors. It uses an in-memory graph-based index to efficiently find similar vectors and is optimized for read-heavy workloads.

The feature is considered experimental because the public API and configuration options (distance metrics, normalization, indexing parameters) may change.

However, the feature (an ANN index) itself is here to stay.
