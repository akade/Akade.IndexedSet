# Benchmarks
Benchmarks measured on 27.07.2022:

## Environment
``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.302
  [Host]     : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
  DefaultJob : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
```

All benchmarks are currenlty using *1k elements*. Benchmarks showing the scaling is on the roadmap - expect IndexedSet to scale much
much better than the naive LINQ Queries.

## Unique-Index
|                       Method |         Mean |     Error |    StdDev | Ratio | Code Size |  Gen 0 | Allocated |
|----------------------------- |-------------:|----------:|----------:|------:|----------:|-------:|----------:|
|                  Unqiue_Linq | 158,313.5 ns | 845.71 ns | 706.21 ns | 1.000 |     753 B | 0.7324 |  12,800 B |
|            Unique_Dictionary |     478.1 ns |   0.84 ns |   0.70 ns | 0.003 |     681 B |      - |         - |
| Unique_IndexedSet_PrimaryKey |   1,762.0 ns |  34.62 ns |  34.01 ns | 0.011 |     513 B |      - |         - |
|     Unique_IndexedSet_Single |   1,545.8 ns |   7.05 ns |   6.60 ns | 0.010 |   1,225 B |      - |         - |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the directory. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
|                Method |     Mean |    Error |   StdDev | Ratio | Code Size |  Gen 0 | Allocated |
|---------------------- |---------:|---------:|---------:|------:|----------:|-------:|----------:|
|       MultiValue_Linq | 64.66 μs | 0.709 μs | 0.663 μs |  1.00 |   2,575 B |      - |   1,680 B |
|     Multivalue_Lookup | 12.31 μs | 0.139 μs | 0.116 μs |  0.19 |   1,768 B | 0.0153 |     360 B |
| Multivalue_IndexedSet | 13.45 μs | 0.080 μs | 0.062 μs |  0.21 |   2,728 B | 0.0153 |     360 B |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
|              Method |         Mean |        Error |       StdDev | Ratio |  Gen 0 |  Gen 1 | Allocated |
|-------------------- |-------------:|-------------:|-------------:|------:|-------:|-------:|----------:|
|          Range_Linq | 22,643.85 ns |   211.794 ns |   187.750 ns |  1.00 |      - |      - |     168 B |
|    Range_IndexedSet |  2,865.35 ns |     3.205 ns |     2.677 ns |  0.13 | 0.0038 |      - |      72 B |
|                     |              |              |              |       |        |        |           |
|         Paging_Linq | 78,183.43 ns |   845.911 ns |   749.878 ns | 1.000 | 9.5215 | 2.3193 | 160,352 B |
|   Paging_IndexedSet |    189.49 ns |     1.085 ns |     1.015 ns | 0.002 | 0.0277 |      - |     464 B |
|                     |              |              |              |       |        |        |           |
|            Min_Linq | 74,497.53 ns | 1,258.503 ns | 1,177.204 ns | 1.000 |      - |      - |      40 B |
|      Min_IndexedSet |     10.10 ns |     0.028 ns |     0.025 ns | 0.000 |      - |      - |         - |
|                     |              |              |              |       |        |        |           |
|       LessThan_Linq | 16,915.54 ns |   127.021 ns |   106.069 ns |  1.00 |      - |      - |     160 B |
| LessThan_IndexedSet |  3,908.94 ns |     5.453 ns |     4.554 ns |  0.23 |      - |      - |      72 B |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
|                     Method |         Mean |      Error |     StdDev | Ratio |  Gen 0 | Allocated |
|--------------------------- |-------------:|-----------:|-----------:|------:|-------:|----------:|
|            StartsWith_Linq |  8,576.15 ns | 138.038 ns | 122.367 ns | 1.000 |      - |      40 B |
|      StartsWith_IndexedSet |     82.25 ns |   0.308 ns |   0.273 ns | 0.010 |      - |         - |
|                            |              |            |            |       |        |           |
|       FuzzyStartsWith_Linq | 44,471.02 ns | 205.713 ns | 192.424 ns |  1.00 | 4.7607 |  80,040 B |
| FuzzyStartsWith_IndexedSet | 14,699.35 ns |  73.623 ns |  68.867 ns |  0.33 | 1.3275 |  22,304 B |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

|                   Method |           Mean |        Error |       StdDev | Ratio |    Gen 0 |   Allocated |
|------------------------- |---------------:|-------------:|-------------:|------:|---------:|------------:|
|            Contains_Linq |    40,012.5 ns |    410.37 ns |    363.79 ns | 1.000 |        - |        40 B |
|      Contains_IndexedSet |       145.5 ns |      0.60 ns |      0.56 ns | 0.004 |   0.0114 |       192 B |
|                          |                |              |              |       |          |             |
|       FuzzyContains_Linq | 4,755,780.7 ns | 13,276.35 ns | 11,769.14 ns |  1.00 | 281.2500 | 4,831,712 B |
| FuzzyContains_IndexedSet |   137,248.9 ns |  1,233.79 ns |    963.26 ns |  0.03 |  15.3809 |   260,624 B |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

|                   Method |         Mean |     Error |    StdDev | Ratio |  Gen 0 | Allocated |
|------------------------- |-------------:|----------:|----------:|------:|-------:|----------:|
|             UniqueLookup |     15.60 ns |  0.044 ns |  0.041 ns |  1.00 |      - |         - |
|   ConcurrentUniqueLookup |     34.66 ns |  0.052 ns |  0.043 ns |  2.22 |      - |         - |
|                          |              |           |           |       |        |           |
|           LessThanLookup |     25.64 ns |  0.131 ns |  0.116 ns |  1.00 | 0.0038 |      64 B |
| ConcurrentLessThanLookup |     53.80 ns |  0.120 ns |  0.106 ns |  2.10 | 0.0057 |      96 B |
|                          |              |           |           |       |        |           |
|           FullTextLookup | 10,756.76 ns | 46.046 ns | 43.071 ns |  1.00 | 1.1444 |  19,288 B |
| ConcurrentFullTextLookup | 11,128.86 ns | 97.457 ns | 86.393 ns |  1.03 | 1.1444 |  19,360 B |

> ℹ️ For more complex scenarios, the synchronization cost is negligble. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
