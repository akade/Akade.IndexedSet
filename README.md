# Akade.IndexedSet

[![CI Build](https://github.com/akade/Akade.IndexedSet/actions/workflows/ci-build.yml/badge.svg?branch=master)](https://github.com/akade/Akade.IndexedSet/actions/workflows/ci-build.yml)
![NuGet version (Akade.IndexedSet)](https://img.shields.io/nuget/v/Akade.IndexedSet.svg)

Provides an In-Memory data structure, the IndexedSet, that allows to easily add indices to allow efficient querying. Based on often seeing inefficient usage of 
`.FirstOrDefault`, `.Where`, `.Single` etc... and implementing a data-structure to improve those queries for every project I'm on.

## Overview

Performance / Operation-Support of the different indices:

- n: total number of elements
- m: number of elements in the return set

| Query  | Unique-Index | NonUnique-Index | Range-Index     |
| ------ | ------------ | --------------- | --------------- |
| Single | ✔ O(1)      | ⚠ O(1)         | ⚠ O(1)         |
| Where  | ❌           | ✔ O(m)         | ✔ O(log n + m) |
| Range  | ❌           | ❌             | ✔ O(log n + m) |
| < / <= | ❌           | ❌             | ✔ O(log n + m) |
| > / >= | ❌           | ❌             | ✔ O(log n + m) |

## FAQs

### How do I use multiple index types for the same property?

Use "named" indices by using static methods:

```csharp
record Data(int Id, int SecondaryKey);

static sealed class DataIndices
{
        public int UniqueIndex(Data x) => x.SecondaryKey;
}

var set = IndexedSetBuilder<Data>.Create(x => x.PrimaryKey)
           .WithUniqueIndex(DataIndices.UniqueIndex)
           .WithRangeIndex(x => x.SecondaryKey)
           .Build();

// querying unique index:
var data = set.Single(DataIndices.UniqueIndex, 4); // Uses the unique index
var data2 = set.Range(x => x.SecondaryKey, 4); // Uses the range index
var inRange = set.Range(x => x.SecondaryKey, 1, 10) // Uses the range index
```

> ℹ We recommend using the lambda syntax for "simple" properties and static methods for more complicated ones. It's easy to read, resembles "normal" LINQ-Queries and all the magic strings are compiler generated.

## Features
This project aims to provide a data structure (*it's not a DB!*) that allows to easily setup fast access on different properties:
### Unique index (single entity, singkle key)
Dictionary-based, O(1), access on primary and secondary keys:

```csharp
var set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
        .WithUniqueIndex(x => x.SecondaryKey)
        .Build();

set.Add(new(primaryKey: 1, secondaryKey: 5));

// fast access via primary key
var data = set[1];

// fast access via secondary key
var data = set.Single(x => x.SecondaryKey, 5);
```

### Non-unqiue index (multiple entities, single key)
Dictionary-based, O(1), access on keys (single value) with multiple values (multiple keys):

```csharp
var set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
        .WithIndex(x => x.SecondaryKey)
        .Build();

set.Add(new(primaryKey: 1, secondaryKey: 5));
set.Add(new(primaryKey: 2, secondaryKey: 5));

// fast access via secondary key
IEnumerable<Data> data = set.Where(x => x.SecondaryKey, 5);
```

### Non-unique index (multiple entities, multiple keys)
Dictionary-based, O(1), access on denormalized keys i.e. multiple keys for multiple entities:
```csharp

var set = IndexedSetBuilder<GraphNode>.Create(a => a.Id)
        .WithIndex(x => x.ConnectsTo) // Where ConnectsTo returns an IEnumerable<int>
        .Build();

//   1   2
//   |\ /
//   | 3
//    \|
//     4

set.Add(new(Id: 1, connectsTo: new[]{ 3, 4 }));
set.Add(new(Id: 2, connectsTo: new[]{ 3 }));
set.Add(new(Id: 3, connectsTo: new[]{ 1, 2 , 3 }));
set.Add(new(Id: 4, connectsTo: new[]{ 1, 3 }));

// fast access via secondary key
// TODO: Should maybe be named contains, even if it is technically the same (or just an alias?)
IEnumerable<GraphNode> nodesThatConnectTo1 = set.Where(x => x.ConnectsTo, 1); // returns nodes 3 & 4
IEnumerable<GraphNode> nodesThatConnectTo3 = set.Where(x => x.ConnectsTo, 1); // returns nodes 1 & 2 & 3

// Optimizes a Where(x => x.Contains(...)) query:
IEnumerable<GraphNode> nodesThatConnectTo1 = set.FullScan().Where(x => x.ConnectsTo.Contains(1)); // returns nodes 3 & 4, but enumerates through the entire set
```

### Range index
Binary-heap based O(log(n)) access for range based, smaller than (or equals) or bigger than (or equals) queries.

```csharp
var set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
        .WithRangeIndex(x => x.SecondaryKey)
        .Build();

set.Add(new(primaryKey: 1, secondaryKey: 3));
set.Add(new(primaryKey: 2, secondaryKey: 4));

// fast access via range query
IEnumerable<Data> data = set.Range(x => x.SecondaryKey, 1, 5);
```
For more samples, checkout the unit tests.

### Computed or compound key

The data structure also allows to use computed or compund keys:

```csharp
var set = IndexedSetBuilder<Data>.Create(a => a.PrimaryKey)
        .WithIndex(x => (x.start, x.end))
        .WithIndex(x => x.end - x.start)
        .WithIndex(ComputedKey.SomeStaticMethod)
        .Build();

set.Add(new(primaryKey: 1, start: 2, end: 10));

// fast access via indices
IEnumerable<Data> data = set.Where(x => (x.start, x.end), (2, 10));
IEnumerable<Data> data = set.Where(x => x.end - x.start, 8);
IEnumerable<Data> data = set.Where(x => ComputedKey.SomeStaticMethod, 42);
```

### Reflection- & expression-free - convention-based index naming

We are using the [CallerArgumentExpression](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute)-Feature 
of .Net 6/C# 10 to provide convention-based naming of the indices:
- `set.Where(x => (x.Prop1, x.Prop2), (1, 2))` tries to use an index named `"x => (x.Prop1, x.Prop2)"`
- `set.Where(ComputedKeys.NumberOfDays, 5)` tries to use an index named `"ComputedKeys.NumberOfDays"`
- **Hence, be careful what you pass in. The convention is to always use a lambda with x as variable name or use static methods.**

Reasons
- Simple and yet effective:
  - Allows computed, compund, custom values to be indexed...
- Performance: No reflection at work and no (runtime) code-gen necessary
- AOT-friendly

### Updating key-values
**The current implementation requires any keys of any type to never change the value while the instance is within the set**. Hence, in order to update any key you will need to remove the instance, update the keys and add the instance again.

### Roadmap
Potential features:
- Thread-safe version
- Easier updating of keys
- Events for changed values
- More index types
- Benchmarks

If you have any suggestion or found a bug / unexpected behavior, open an issue!