# Akade.IndexedSet

[![CI Build](https://github.com/akade/Akade.IndexedSet/actions/workflows/ci-build.yml/badge.svg?branch=master)](https://github.com/akade/Akade.IndexedSet/actions/workflows/ci-build.yml)
[![NuGet version (Akade.IndexedSet)](https://img.shields.io/nuget/v/Akade.IndexedSet.svg)](https://www.nuget.org/packages/Akade.IndexedSet/)

Provides an In-Memory data structure, the IndexedSet, that allows to easily add indices to allow efficient querying. Based on often seeing inefficient usage of 
`.FirstOrDefault`, `.Where`, `.Single` etc... and implementing data-structures to improve those queries for every project I'm on.

## Overview

A sample showing different queries as you might want do for a report:

```csharp
// typically, you would query this from the db
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

### Performance / Operation-Support of the different indices:

- n: total number of elements
- m: number of elements in the return set
- ✔: Supported
- ⚠: Supported but throws if not exactly 1 item was found
- ❌: Not-supported

#### General queries

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

| Query           | Prefix-Index | FullText-Index |
| ----------------| ------------ | ---------------|
| StartWith       | ⚠ O(w)      | ⚠ O(w)       |
| Contains        | ❌           | ✔ O(w)        |
| Fuzzy StartWith | ⚠ O(w+D)    | ⚠ O(w+D)     |
| Fuzzy Contains  | ❌           | ✔ O(w+D)      |

> ℹ FullText indices use a lot more memory than prefix indices and are more expensive to construct. Only
use FullText indices if you really require it.


## Features
This project aims to provide a data structure (*it's not a DB!*) that allows to easily setup fast access on different properties:
### Unique index (single entity, single key)
Dictionary-based, O(1), access on keys:

```csharp
IndexedSet<int, Data> set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
                                                   .WithUniqueIndex(x => x.SecondaryKey)
                                                   .Build();

_ = set.Add(new(PrimaryKey: 1, SecondaryKey: 5));

// fast access via primary key
Data data = set[1];

// fast access via secondary key
data = set.Single(x => x.SecondaryKey, 5);
```

> ℹ Entities do not require a primary key. `IndexedSet<TPrimaryKey, TData>` inherits from `IndexedSet<TData>`
but provides convenient access to the automatically added unique index: `set[primaryKey]` instead 
of `set.Single(x => x.PrimaryKey, primaryKey)`.


### Non-unique index (multiple entities, single key)
Dictionary-based, O(1), access on keys (single value) with multiple values (multiple keys):

```csharp
IndexedSet<int, Data> set = new Data[] { new(PrimaryKey: 1, SecondaryKey: 5), new(PrimaryKey: 2, SecondaryKey: 5) }
        .ToIndexedSet(x => x.PrimaryKey)
        .WithIndex(x => x.SecondaryKey)
        .Build();

// fast access via secondary key
IEnumerable<Data> data = set.Where(x => x.SecondaryKey, 5);
```

### Non-unique index (multiple entities, multiple keys)
Dictionary-based, O(1), access on denormalized keys i.e. multiple keys for multiple entities:
```csharp

IndexedSet<int, GraphNode> set = IndexedSetBuilder<GraphNode>.Create(a => a.Id)
                                                                .WithIndex(x => x.ConnectsTo) // Where ConnectsTo returns an IEnumerable<int>
                                                                .Build();

//   1   2
//   |\ /
//   | 3
//    \|
//     4

_ = set.Add(new(Id: 1, ConnectsTo: new[] { 3, 4 }));
_ = set.Add(new(Id: 2, ConnectsTo: new[] { 3 }));
_ = set.Add(new(Id: 3, ConnectsTo: new[] { 1, 2, 3 }));
_ = set.Add(new(Id: 4, ConnectsTo: new[] { 1, 3 }));

// For readability, it is recommended to write the name for the parameter contains
IEnumerable<GraphNode> nodesThatConnectTo1 = set.Where(x => x.ConnectsTo, contains: 1); // returns nodes 3 & 4
IEnumerable<GraphNode> nodesThatConnectTo3 = set.Where(x => x.ConnectsTo, contains: 1); // returns nodes 1 & 2 & 3

// Non-optimized Where(x => x.Contains(...)) query:
nodesThatConnectTo1 = set.FullScan().Where(x => x.ConnectsTo.Contains(1)); // returns nodes 3 & 4, but enumerates through the entire set
```

### Range index
Binary-heap based O(log(n)) access for range based, smaller than (or equals) or bigger than (or equals) and orderby queries. Also useful to do paging sorted on exactly one index.

```csharp
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

### String indices & fuzzy matching
Prefix- & Suffix-Trie based indices for efficient StartWith & String-Contains queries including support
for fuzzy matching.

```csharp
IndexedSet<Type> data = typeof(object).Assembly.GetTypes()
                                               .ToIndexedSet()
                                               .WithPrefixIndex(x => x.Name.AsMemory())
                                               .WithFullTextIndex(x => x.FullName.AsMemory())
                                               .Build();

// fast prefix or contains queries via indices
_ = data.StartsWith(x => x.Name.AsMemory(), "Int".AsMemory());
_ = data.Contains(x => x.FullName.AsMemory(), "Int".AsMemory());

// fuzzy searching is supported by prefix and full text indices
// the following will also match "String"
_ = data.FuzzyStartsWith(x => x.Name.AsMemory(), "Strang".AsMemory(), 1);
_ = data.FuzzyContains(x => x.FullName.AsMemory(), "Strang".AsMemory(), 1);
```

### Computed or compound key

The data structure also allows to use computed or compound keys:

```csharp
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
> ℹ For more samples, take a look at the unit tests.

### Reflection- & expression-free - convention-based index naming

We are using the [CallerArgumentExpression](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute)-Feature 
of .Net 6/C# 10 to provide convention-based naming of the indices:
- `set.Where(x => (x.Prop1, x.Prop2), (1, 2))` tries to use an index named `"x => (x.Prop1, x.Prop2)"`
- `set.Where(ComputedKeys.NumberOfDays, 5)` tries to use an index named `"ComputedKeys.NumberOfDays"`
- **Hence, be careful what you pass in. The convention is to always use a lambda with x as variable name or use static methods.**

Reasons
- Simple and yet effective:
  - Allows computed, compound, custom values etc. to be indexed without adding complexity...
- Performance: No reflection at work and no (runtime) code-gen necessary
- AOT-friendly including full trimming support

### Updating key-values
**The current implementation requires any keys of any type to never change the value while the instance is within the set**. Hence, in order to update any key you will need to remove the instance, update the keys and add the instance again.

## FAQs

### How do I use multiple index types for the same property?

Use "named" indices by using static methods:

```csharp
record Data(int PrimaryKey, int SecondaryKey);

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

> ℹ We recommend using the lambda syntax for "simple" properties and static methods for more complicated ones. It's easy to read, resembles "normal" LINQ-Queries and all the magic strings are compiler generated.

## Roadmap
Potential features (not ordered):
- [ ] Thread-safe version
- [ ] Easier updating of keys
- [ ] Events for changed values
- [ ] More index types (Trie)
- [ ] Tree-based range index for better insertion performance
- [ ] Analyzers to help with best practices
- [x] Range insertion and corresponding `.ToIndexedSet().WithIndex(x => ...).[...].Build()`
- [x] Refactoring to allow a primarykey-less set: this was an artifical restriction that is not necessary
- [ ] Aggregates (i.e. sum or average: interface based on state & add/removal state update functions)
- [ ] Benchmarks

If you have any suggestion or found a bug / unexpected behavior, open an issue! I will also review PRs and integrate them if they fit the project.
