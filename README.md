# Akade.IndexedSet

![.Net Version](https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=version&query=%2F%2FTargetFrameworks%5B1%5D&url=https://raw.githubusercontent.com/akade/Akade.IndexedSet/main/Akade.IndexedSet/Akade.IndexedSet.csproj&logo=.net)
[![CI Build](https://github.com/akade/Akade.IndexedSet/actions/workflows/ci-build.yml/badge.svg?branch=main)](https://github.com/akade/Akade.IndexedSet/actions/workflows/ci-build.yml)
[![NuGet version (Akade.IndexedSet)](https://img.shields.io/nuget/v/Akade.IndexedSet.svg)](https://www.nuget.org/packages/Akade.IndexedSet/)
[![MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/akade/Akade.IndexedSet#readme)

A convenient data structure supporting efficient in-memory indexing and querying, including range queries and fuzzy string matching.
In a nutshell, it allows you to write LINQ-like queries *without* enumerating through the entire list. If you are currently completely enumerating
through your data, expect huge [speedups](docs/Benchmarks.md) and much better scalability!

<!--TOC-->
  - [Overview](#overview)
    - [Design Goals](#design-goals)
    - [Performance and Operation-Support of the different indices:](#performance-and-operation-support-of-the-different-indices)
      - [General queries](#general-queries)
      - [String queries](#string-queries)
      - [Spatial queries](#spatial-queries)
  - [Features](#features)
    - [Unique index](#unique-index)
    - [Non-unique index](#non-unique-index)
    - [Range index](#range-index)
    - [String indices and fuzzy matching](#string-indices-and-fuzzy-matching)
    - [Multi-key indices: All indices can be used with multiple keys](#multi-key-indices-all-indices-can-be-used-with-multiple-keys)
    - [Computed or compound key](#computed-or-compound-key)
    - [Spatial index](#spatial-index)
    - [Vector index](#vector-index)
    - [Concurrency and Thread-Safety](#concurrency-and-thread-safety)
    - [No reflection and no expressions - convention-based index naming](#no-reflection-and-no-expressions-convention-based-index-naming)
  - [FAQs](#faqs)
    - [How do I use multiple index types for the same property?](#how-do-i-use-multiple-index-types-for-the-same-property)
    - [How do I update key values if the elements are already in the set?](#how-do-i-update-key-values-if-the-elements-are-already-in-the-set)
    - [How do I do case-insensitve (fuzzy) string matching (Prefix, FullTextIndex)?](#how-do-i-do-case-insensitve-fuzzy-string-matching-prefix-fulltextindex)
  - [Roadmap](#roadmap)
<!--/TOC-->

## Overview

A sample showing different indices and queries:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Overview(importer:cs?body-only=true) -->
```cs
var data = new Purchase[] {
    new(Id: 1, ProductId: 1, Amount: 1, UnitPrice: 5),
    new(Id: 2, ProductId: 1, Amount: 2, UnitPrice: 5),
    new(Id: 6, ProductId: 4, Amount: 3, UnitPrice: 12),
    new(Id: 7, ProductId: 4, Amount: 8, UnitPrice: 10) // discounted price
    };

IndexedSet<int, Purchase> set = data.ToIndexedSet(x => x.Id)
                                    .WithIndex(x => x.ProductId)
                                    .WithRangeIndex(x => x.Amount)
                                    .WithRangeIndex(x => x.UnitPrice)
                                    .WithRangeIndex(x => x.Amount * x.UnitPrice)
                                    .WithIndex(x => (x.ProductId, x.UnitPrice))
                                    .Build();

// efficient queries on configured indices
_ = set.Where(x => x.ProductId, 4);
_ = set.Range(x => x.Amount, 1, 3, inclusiveStart: true, inclusiveEnd: true);
_ = set.GreaterThanOrEqual(x => x.UnitPrice, 10);
_ = set.MaxBy(x => x.Amount * x.UnitPrice);
_ = set.Where(x => (x.ProductId, x.UnitPrice), (4, 10));
```
<!-- end-snippet -->

### Design Goals
- Much faster solution than (naive) LINQ-based full-enumeration
- Syntax close to LINQ-Queries
- Easy to use with a fluent builder API
- Reflection & Expression-free to be AOT & Trimming friendly (for example for Blazor/WebASM)
- It's not a db - in-memory only

### Performance and Operation-Support of the different indices:

Below, you find runtime complexities. Benchmarks can be found [here](docs/Benchmarks.md)

#### General queries

- n: total number of elements
- m: number of elements in the return set
- ✔: Supported
- ⚠: Supported but throws if not exactly 1 item was found
- ❌: Not-supported

| Query     | Unique-Index | NonUnique-Index | Range-Index     |
| --------- | ------------ | --------------- | --------------- |
| Single    | ⚠ O(1)      | ⚠ O(1)         | ⚠ O(log n)    |
| Where     | ✔ O(1)       | ✔ O(m)         | ✔ O(log n + m) |
| Range     | ❌           | ❌             | ✔ O(log n + m)  |
| < / <=    | ❌           | ❌             | ✔ O(log n + m)  |
| > / >=    | ❌           | ❌             | ✔ O(log n + m)  |
| OrderBy   | ❌           | ❌             | ✔ O(m)          |
| Max/Min   | ❌           | ❌             | ✔ O(1)          |

#### String queries

- w: length of query word
- D: maximum distance in fuzzy query
- r: number of items in result set

| Query           | Prefix-Index | FullText-Index |
| ----------------| ------------ | -------------- |
| StartWith       | ✔ O(w+r)     | ✔ O(w+r)     |
| Contains        | ❌           | ✔ O(w+r)      |
| Fuzzy StartWith | ✔ O(w+D+r)   | ✔ O(w+D+r)   |
| Fuzzy Contains  | ❌           | ✔ O(w+D+r)    |

> ℹ FullText indices use a lot more memory than prefix indices and are more expensive to construct. Only
use FullText indices if you really require it.

#### Spatial queries

The spatial index is based on an R*Tree. Runtime complexity for search of kNN is ~O(max height of tree),
if we assume a low degree of overlap of the bounding boxes. Worst case in theory is O(n), but in practice
the performance is usually pretty decent. 

> ℹ The performance of spatial queries depends heavily on the distribution of the data. Consider using
bulk-loading (i.e. specify the data at indexedset creation) for best performance.

#### Vector queries

Vector indices use approximate nearest neighbor (ANN) search based on a Fresh Vamana graph. This provides very fast
similarity search for high-dimensional vectors, commonly used for similarity search or semantic search with embeddings.
The runtime complexity for Fresh Vamana-ANN depends on the dataset, construction etc, but in practice is sub-linear and scales well to large datasets.

## Features

### Unique index
Dictionary-based, O(1), access on keys:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_UniqueIndex(importer:cs?body-only=true) -->
```cs
IndexedSet<int, Data> set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
                                                   .WithUniqueIndex(x => x.SecondaryKey)
                                                   .Build();

_ = set.Add(new(PrimaryKey: 1, SecondaryKey: 5));

// fast access via primary key
Data data = set[1];

// fast access via secondary key
data = set.Single(x => x.SecondaryKey, 5);
```
<!-- end-snippet -->

> ℹ Entities do not require a primary key. `IndexedSet<TPrimaryKey, TData>` inherits from `IndexedSet<TData>`
but provides convenient access to the automatically added unique index: `set[primaryKey]` instead 
of `set.Single(x => x.PrimaryKey, primaryKey)`.


### Non-unique index
Dictionary-based, O(1), access on keys (single value) with multiple values (multiple keys):

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_NonUniqueIndex_SingleKey(importer:cs?body-only=true) -->
```cs
IndexedSet<int, Data>? set = new Data[] { new(PrimaryKey: 1, SecondaryKey: 5), new(PrimaryKey: 2, SecondaryKey: 5) }
    .ToIndexedSet(x => x.PrimaryKey)
    .WithIndex(x => x.SecondaryKey)
    .Build();

// fast access via secondary key
IEnumerable<Data> data = set.Where(x => x.SecondaryKey, 5);
```
<!-- end-snippet -->

### Range index
Binary-heap based O(log(n)) access for range based, smaller than (or equals) or bigger than (or equals) and orderby queries. Also useful to do paging sorted on exactly one index.

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_RangeIndex(importer:cs?body-only=true) -->
```cs
IndexedSet<Data> set = IndexedSetBuilder.Create(new Data[] { new(1, SecondaryKey: 3), new(2, SecondaryKey: 4) })
                                        .WithRangeIndex(x => x.SecondaryKey)
                                        .Build();

// fast access via range query
IEnumerable<Data> data = set.Range(x => x.SecondaryKey, 1, 5);

// fast max & min key value or elements
int maxKey = set.Max(x => x.SecondaryKey);
data = set.MaxBy(x => x.SecondaryKey);

// fast larger or smaller than
data = set.LessThan(x => x.SecondaryKey, 4);

// fast ordering & paging
data = set.OrderBy(x => x.SecondaryKey, skip: 10).Take(10); // second page of 10 elements
```
<!-- end-snippet -->

### String indices and fuzzy matching
Prefix- & Suffix-Trie based indices for efficient StartWith & String-Contains queries including support
for fuzzy matching.

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_StringQueries(importer:cs?body-only=true) -->
```cs
IndexedSet<Type> data = typeof(object).Assembly.GetTypes()
                                               .ToIndexedSet()
                                               .WithPrefixIndex(x => x.Name)
                                               .WithFullTextIndex(x => x.FullName!)
                                               .Build();

// fast prefix or contains queries via indices
_ = data.StartsWith(x => x.Name, "Int");
_ = data.Contains(x => x.FullName!, "Int");

// fuzzy searching is supported by prefix and full text indices
// the following will also match "String"
_ = data.FuzzyStartsWith(x => x.Name, "Strang", 1);
_ = data.FuzzyContains(x => x.FullName!, "Strang", 1);
```
<!-- end-snippet -->

### Multi-key indices: All indices can be used with multiple keys
There are overloads for all indices that allow to use multiple keys. 

You can have a unique index where each element can have multiple keys:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_UniqueIndex_MultipleKeys(importer:cs?body-only=true) -->
```cs
IndexedSet<int, Data> set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
                                           .WithUniqueIndex(x => x.AlternativeKeys) // Where AlternativeKeys returns an IEnumerable<int>
                                           .Build();

_ = set.Add(new(PrimaryKey: 1, SecondaryKey: 2) { AlternativeKeys = [3, 4] });
_ = set.Single(x => x.AlternativeKeys, contains: 3); // returns above element
```
<!-- end-snippet -->

The same applies for most other index types, for example for non-unique indices:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_NonUniqueIndex_MultipleKeys(importer:cs?body-only=true) -->
```cs
IndexedSet<int, GraphNode> set = IndexedSetBuilder<GraphNode>.Create(a => a.Id)
                                                             .WithIndex(x => x.ConnectsTo) // Where ConnectsTo returns an IEnumerable<int>
                                                             .Build();

//   1   2
//   |\ /
//   | 3
//    \|
//     4

_ = set.Add(new(Id: 1, ConnectsTo: [3, 4]));
_ = set.Add(new(Id: 2, ConnectsTo: [3]));
_ = set.Add(new(Id: 3, ConnectsTo: [1, 2, 3]));
_ = set.Add(new(Id: 4, ConnectsTo: [1, 3]));

// For readability, it is recommended to write the name for the parameter contains
IEnumerable<GraphNode> nodesThatConnectTo1 = set.Where(x => x.ConnectsTo, contains: 1); // returns nodes 3 & 4
IEnumerable<GraphNode> nodesThatConnectTo3 = set.Where(x => x.ConnectsTo, contains: 1); // returns nodes 1 & 2 & 3

// Non-optimized Where(x => x.Contains(...)) query:
nodesThatConnectTo1 = set.FullScan().Where(x => x.ConnectsTo.Contains(1)); // returns nodes 3 & 4, but enumerates through the entire set
```
<!-- end-snippet -->

> :information_source: For range queries, this introduces a small overhead as the results are filtered to be distinct: 
> i.e. `O(log n + m log m)` instead of `O(log n + m)`.

> :information_source: Multi-key string indices are marked experimental. Read more at [Experimental Features](docs/ExperimentalFeatures.md#AkadeIndexedSetEXP0001)


### Computed or compound key

The data structure also allows to use computed or compound keys:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_ComputedOrCompoundKey(importer:cs?body-only=true) -->
```cs
var data = new RangeData[] { new(Start: 2, End: 10) };
IndexedSet<RangeData> set = data.ToIndexedSet()
                                .WithIndex(x => (x.Start, x.End))
                                .WithIndex(x => x.End - x.Start)
                                .WithIndex(ComputedKey.SomeStaticMethod)
                                .Build();
// fast access via indices
IEnumerable<RangeData> result = set.Where(x => (x.Start, x.End), (2, 10));
result = set.Where(x => x.End - x.Start, 8);
result = set.Where(ComputedKey.SomeStaticMethod, 42);
```
<!-- end-snippet -->
> ℹ For more samples, take a look at the unit tests.

### Spatial index
R*Tree based spatial indices for efficient 2D/3D intersection & kNN queries.

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_SpatialIndex(importer:cs?body-only=true) -->
```cs
var locations = new Vector2[]
{
    new(100, 150),  // A point inside our query area
    new(250, 200),  // Another point inside our query area
    new(400, 500),  // A point outside our query area
    new(50, 75)     // A point outside our query area
};

IndexedSet<Vector2> spatialSet = locations.ToIndexedSet()
                                          .WithSpatialIndex(x => x)
                                          .Build();

Vector2 searchCenter = new(200, 200);
Vector2 areaMin = new(150, 150);
Vector2 areaMax = new(300, 250);

// fast spatial range queries
IEnumerable<Vector2> pointsInArea = spatialSet.Intersects(x => x, areaMin, areaMax);

// fast nearest neighbor queries
IEnumerable<Vector2> nearestPoints = spatialSet.NearestNeighbors(x => x, searchCenter).Take(2);
```
<!-- end-snippet -->

> ⚠ For best performance, consider bulk-loading data at index creation time.
> ℹ Spatial indices work with Vector2 & Vector3 at the moment. The tree implementation is generic and might be extended to support other types or opened up for custom types in the future.

### Vector index
(.NET 9+ only) High-performance approximate nearest neighbor search based on a Fresh Vamana graph.

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_VectorIndex(importer:cs?body-only=true) -->
```cs
// Sample document vectors (simplified for example)
var documents = new Document[]
{
    new("Machine Learning Basics", new float[] { 0.1f, 0.8f, 0.3f, 0.9f }),
    new("Deep Learning Guide", new float[] { 0.2f, 0.9f, 0.4f, 0.8f }),
    new("Cooking Recipes", new float[] { 0.9f, 0.1f, 0.8f, 0.2f }),
    new("Travel Guide", new float[] { 0.3f, 0.2f, 0.9f, 0.1f })
};

IndexedSet<Document> vectorSet = documents.ToIndexedSet()
                                          .WithVectorIndex(x => x.Embedding.Span)
                                          .Build();

// query vector for "AI and machine learning"
ReadOnlySpan<float> queryVector = new float[] { 0.15f, 0.85f, 0.35f, 0.85f };

// fast approximate nearest neighbor search
IEnumerable<Document> similarDocuments = vectorSet.ApproximateNearestNeighbors(x => x.Embedding.Span, queryVector, k: 2);
```
<!-- end-snippet -->

> ⚠ Vector indices use approximate algorithms and don't support exact key lookup operations like `Where()` or `Single()`. 
They are specifically designed for similarity search scenarios.

> ℹ For more samples, take a look at the unit tests.

### Concurrency and Thread-Safety

The "normal" indexedset is not thread-safe, however, a ReaderWriterLock-based implementation is available.
Just call `BuildConcurrent()` instead of `Build()`:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs Features_Concurrency(importer:cs?body-only=true) -->
```cs
ConcurrentIndexedSet<Data> set = IndexedSetBuilder<Data>.Create()
                                                        .WithIndex(x => x.SecondaryKey)
                                                        .BuildConcurrent();
```
<!-- end-snippet -->

> ⚠ The concurrent implementation needs to materialize all query results.<br />
> `OrderBy` and `OrderByDescending` take an additional `count` parameter to avoid unnecessary materialization.
> You can judge the overhead [here](docs/Benchmarks.md#ConcurrentSet)
### No reflection and no expressions - convention-based index naming

We are using the [CallerArgumentExpression](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute)-Feature 
of .Net 6/C# 10 to provide convention-based naming of the indices:
- `set.Where(x => (x.Prop1, x.Prop2), (1, 2))` tries to use an index named `"x => (x.Prop1, x.Prop2)"""
- `set.Where(ComputedKeys.NumberOfDays, 5)` tries to use an index named `"ComputedKeys.NumberOfDays"`
- **Hence, be careful what you pass in. 
> :information_source: The following naming conventions are recommended:
> - Use x as parameter name in any lambdas that determines an index name.
> - Do not use parentheses in any lambda that determines an index name.
> - Do not use block bodied in any lambda that determines an index name. 
> - For complex indices, use a static method.
> [C# Analyzers](./Analyzers/Readme.md) are shipped with the package to spot incorrect index names.

Reasons
- Simple and yet effective:
  - Allows computed, compound, custom values etc. to be indexed without adding complexity...
- Performance: No reflection at work and no (runtime) code-gen necessary
- AOT-friendly including full trimming support

## FAQs

### How do I use multiple index types for the same property?

Use "named" indices by using static methods:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs FAQ_MultipleIndicesForSameProperty(importer:cs?body-only=true) -->
```cs
IndexedSet<int, Data> set = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey)
                                                   .WithUniqueIndex(DataIndices.UniqueIndex)
                                                   .WithRangeIndex(x => x.SecondaryKey)
                                                   .Build();
_ = set.Add(new(1, 4));
// querying unique index:
Data data = set.Single(DataIndices.UniqueIndex, 4); // Uses the unique index
Data data2 = set.Single(x => x.SecondaryKey, 4); // Uses the range index
IEnumerable<Data> inRange = set.Range(x => x.SecondaryKey, 1, 10); // Uses the range index
```
<!-- end-snippet -->

> ℹ We recommend using the lambda syntax for "simple" properties and static methods for more complicated ones. It's easy to read, resembles "normal" LINQ-Queries and all the magic strings are compiler generated.

### How do I update key values if the elements are already in the set?
**The implementation requires any keys of any type to never change the value while the instance is within the set**.
You can manually remove, update and add an object. However, there are some helper methods for that - which is especially
useful for the concurrent variant as it provides thread-safe serialized access.

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs FAQ_UpdatingKeys(importer:cs?body-only=true) -->
```cs
IndexedSet<Data> set = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey).Build();
ConcurrentIndexedSet<Data> concurrentSet = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey).BuildConcurrent();
Data dataElement = new(1, 4);

// updating a mutable property
_ = set.Update(dataElement, e => e.MutableProperty = 7);
// updating an immutable property
_ = set.Update(dataElement, e => e with { SecondaryKey = 12 });
// be careful, the second time will do an add as dataElement still refers to the "old" record
_ = set.Update(dataElement, e => e with { SecondaryKey = 12 });

// updating in an concurrent set
concurrentSet.Update(set =>
{
    // serialized access to the inner IndexedSet, where you can safely use above update methods
    // in an multi-threaded environment
});
```
<!-- end-snippet -->

### How do I do case-insensitve (fuzzy) string matching (Prefix, FullTextIndex)?
While you can use whatever index expression that you want (i.e. `.ToLowerInvariant()`), 
using a comparer is recommended:

<!-- begin-snippet: Akade.IndexedSet.Tests/Samples/Readme.cs FAQ_CaseInsensitiveFuzzyMatching(importer:cs?body-only=true) -->
```cs
IndexedSet<Data> set = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey)
                                              .WithFullTextIndex(x => x.Text, CharEqualityComparer.OrdinalIgnoreCase)
                                              .Build();
IEnumerable<Data> matches = set.FuzzyContains(x => x.Text, "Search", maxDistance: 2);
```
<!-- end-snippet -->

## Roadmap
Potential features (not ordered):
- [x] Thread-safe version
- [x] Easier updating of keys
- [x] More index types (Trie)
- [x] Range insertion and corresponding `.ToIndexedSet().WithIndex(x => ...).[...].Build()`
- [x] Refactoring to allow a primarykey-less set: this was an artificial restriction that is not necessary
- [x] Benchmarks
- [x] Simplification of string indices, i.e. Span/String based overloads to avoid `AsMemory()`...
- [x] Analyzers to help with best practices
- [x] Multi-key everything: All index types can be used with multiple keys per element.
- [ ] Tree-based range index for better insertion performance
- [ ] Aggregates (i.e. sum or average: interface based on state & add/removal state update functions)
- [ ] Custom (equality) comparer for indices
- [ ] Helper functions for search scenarios (Searching in multiple properties, text-reprocessing & result merging)
- [ ] Becnhmark vs elastic search

If you have any suggestion or found a bug / unexpected behavior, open an issue! I will also review PRs and integrate them if they fit the project.

