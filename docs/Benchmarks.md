# Benchmarks
Benchmarks measured on 27.07.2022:

## Environment
``` ini
BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1928/22H2/2022Update/SunValley2)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.304
  [Host]   : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.18 (6.0.1823.26907), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
```

All benchmarks are currenlty using *1k elements*. Benchmarks showing the scaling is on the roadmap - expect IndexedSet to scale much
much better than the naive LINQ Queries. As .NET 7.0 brings in [a bunch of performance improvements](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/),
the benchmarks are run against both .NET 6 and 7.

## Unique-Index
|                       Method |  Runtime |         Mean |       Error |      StdDev | Ratio |   Gen0 | Code Size | Allocated | Alloc Ratio |
|----------------------------- |--------- |-------------:|------------:|------------:|------:|-------:|----------:|----------:|------------:|
|                  Unqiue_Linq | .NET 6.0 | 162,090.0 ns |   409.27 ns |   319.53 ns | 1.000 | 0.7324 |     753 B |   12800 B |        1.00 |
|            Unique_Dictionary | .NET 6.0 |     468.1 ns |     0.72 ns |     0.60 ns | 0.003 |      - |     681 B |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 6.0 |   1,702.4 ns |     5.46 ns |     5.10 ns | 0.011 |      - |     512 B |         - |        0.00 |
|     Unique_IndexedSet_Single | .NET 6.0 |   1,573.2 ns |     6.85 ns |     6.40 ns | 0.010 |      - |   1,224 B |         - |        0.00 |
|                              |          |              |             |             |       |        |           |           |             |
|                  Unqiue_Linq | .NET 7.0 | 160,159.7 ns | 2,988.17 ns | 2,648.94 ns | 1.000 | 0.7324 |     696 B |   12800 B |        1.00 |
|            Unique_Dictionary | .NET 7.0 |     404.1 ns |     0.41 ns |     0.32 ns | 0.003 |      - |     590 B |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 7.0 |   1,394.0 ns |     2.89 ns |     2.70 ns | 0.009 |      - |   1,084 B |         - |        0.00 |
|     Unique_IndexedSet_Single | .NET 7.0 |   1,414.8 ns |     3.26 ns |     2.73 ns | 0.009 |      - |   1,145 B |         - |        0.00 |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
|                Method |  Runtime |     Mean |    Error |   StdDev | Ratio | Code Size |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |--------- |---------:|---------:|---------:|------:|----------:|-------:|----------:|------------:|
|       MultiValue_Linq | .NET 6.0 | 62.54 μs | 0.383 μs | 0.340 μs |  1.00 |   2,575 B |      - |    1680 B |        1.00 |
|     Multivalue_Lookup | .NET 6.0 | 12.24 μs | 0.078 μs | 0.069 μs |  0.20 |   1,768 B | 0.0153 |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 6.0 | 12.79 μs | 0.036 μs | 0.030 μs |  0.20 |   2,729 B | 0.0153 |     360 B |        0.21 |
|                       |          |          |          |          |       |           |        |           |             |
|       MultiValue_Linq | .NET 7.0 | 57.03 μs | 0.373 μs | 0.291 μs |  1.00 |   2,569 B | 0.0610 |    1680 B |        1.00 |
|     Multivalue_Lookup | .NET 7.0 | 23.25 μs | 0.158 μs | 0.148 μs |  0.41 |   1,746 B |      - |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 7.0 | 13.12 μs | 0.059 μs | 0.055 μs |  0.23 |   2,638 B | 0.0153 |     360 B |        0.21 |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
|              Method |  Runtime |         Mean |        Error |       StdDev | Ratio |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|-------------------- |--------- |-------------:|-------------:|-------------:|------:|-------:|-------:|----------:|------------:|
|       LessThan_Linq | .NET 6.0 | 17,085.96 ns |    42.395 ns |    35.402 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 6.0 |  3,904.46 ns |     7.180 ns |     6.365 ns |  0.23 |      - |      - |      72 B |        0.45 |
|                     |          |              |              |              |       |        |        |           |             |
|       LessThan_Linq | .NET 7.0 | 18,436.02 ns |    77.562 ns |    72.552 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 7.0 |  3,468.00 ns |     4.656 ns |     3.888 ns |  0.19 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |              |              |              |       |        |        |           |             |
|            Min_Linq | .NET 6.0 | 75,491.15 ns | 1,115.048 ns | 1,043.016 ns | 1.000 |      - |      - |      40 B |        1.00 |
|      Min_IndexedSet | .NET 6.0 |     10.13 ns |     0.043 ns |     0.041 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |              |              |              |       |        |        |           |             |
|            Min_Linq | .NET 7.0 | 69,477.64 ns | 1,000.359 ns |   935.736 ns | 1.000 |      - |      - |      40 B |        1.00 |
|      Min_IndexedSet | .NET 7.0 |     10.40 ns |     0.055 ns |     0.046 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |              |              |              |       |        |        |           |             |
|         Paging_Linq | .NET 6.0 | 74,073.38 ns |   417.396 ns |   390.433 ns | 1.000 | 9.5215 | 2.3193 |  160352 B |       1.000 |
|   Paging_IndexedSet | .NET 6.0 |    178.31 ns |     0.899 ns |     0.751 ns | 0.002 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |              |              |              |       |        |        |           |             |
|         Paging_Linq | .NET 7.0 | 77,957.27 ns | 1,269.528 ns | 1,187.517 ns | 1.000 | 9.5215 | 2.3193 |  160352 B |       1.000 |
|   Paging_IndexedSet | .NET 7.0 |    177.73 ns |     1.209 ns |     1.072 ns | 0.002 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |              |              |              |       |        |        |           |             |
|          Range_Linq | .NET 6.0 | 21,216.35 ns |   126.899 ns |   112.493 ns |  1.00 |      - |      - |     168 B |        1.00 |
|    Range_IndexedSet | .NET 6.0 |  2,865.57 ns |     4.970 ns |     4.406 ns |  0.14 | 0.0038 |      - |      72 B |        0.43 |
|                     |          |              |              |              |       |        |        |           |             |
|          Range_Linq | .NET 7.0 | 26,593.48 ns |   104.328 ns |    87.118 ns |  1.00 |      - |      - |     160 B |        1.00 |
|    Range_IndexedSet | .NET 7.0 |  2,676.51 ns |     4.266 ns |     3.562 ns |  0.10 | 0.0038 |      - |      72 B |        0.45 |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
|                     Method |  Runtime |        Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |--------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
|       FuzzyStartsWith_Linq | .NET 6.0 | 67,594.1 ns | 439.91 ns | 411.49 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 6.0 | 21,357.0 ns |  67.98 ns |  60.26 ns |  0.32 |      - |     240 B |       0.002 |
|                            |          |             |           |           |       |        |           |             |
|       FuzzyStartsWith_Linq | .NET 7.0 | 60,252.2 ns | 131.99 ns | 110.22 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 7.0 | 17,309.5 ns |  97.84 ns |  86.73 ns |  0.29 |      - |     240 B |       0.002 |
|                            |          |             |           |           |       |        |           |             |
|            StartsWith_Linq | .NET 6.0 |  3,910.7 ns |  11.42 ns |  10.12 ns |  1.00 | 0.0076 |     128 B |        1.00 |
|      StartsWith_IndexedSet | .NET 6.0 |    635.7 ns |   2.04 ns |   1.91 ns |  0.16 | 0.0086 |     144 B |        1.12 |
|                            |          |             |           |           |       |        |           |             |
|            StartsWith_Linq | .NET 7.0 |  2,622.0 ns |  16.95 ns |  15.02 ns |  1.00 | 0.0076 |     128 B |        1.00 |
|      StartsWith_IndexedSet | .NET 7.0 |    604.3 ns |   2.00 ns |   1.87 ns |  0.23 | 0.0086 |     144 B |        1.12 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

|                   Method |  Runtime |           Mean |         Error |        StdDev | Ratio |     Gen0 | Allocated | Alloc Ratio |
|------------------------- |--------- |---------------:|--------------:|--------------:|------:|---------:|----------:|------------:|
|            Contains_Linq | .NET 6.0 |    16,333.1 ns |      26.30 ns |      24.60 ns |  1.00 |        - |     408 B |        1.00 |
|      Contains_IndexedSet | .NET 6.0 |       479.9 ns |       0.93 ns |       0.82 ns |  0.03 |   0.0238 |     400 B |        0.98 |
|                          |          |                |               |               |       |          |           |             |
|            Contains_Linq | .NET 7.0 |     6,599.3 ns |       8.86 ns |       6.92 ns |  1.00 |   0.0229 |     408 B |        1.00 |
|      Contains_IndexedSet | .NET 7.0 |       467.7 ns |       1.11 ns |       0.99 ns |  0.07 |   0.0238 |     400 B |        0.98 |
|                          |          |                |               |               |       |          |           |             |
|       FuzzyContains_Linq | .NET 6.0 | 7,204,157.6 ns |  80,806.42 ns |  71,632.79 ns |  1.00 | 281.2500 | 4831744 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 6.0 |   182,005.5 ns |     876.51 ns |     819.89 ns |  0.03 |        - |     608 B |       0.000 |
|                          |          |                |               |               |       |          |           |             |
|       FuzzyContains_Linq | .NET 7.0 | 7,365,406.9 ns | 143,088.92 ns | 153,103.54 ns |  1.00 | 281.2500 | 4831743 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 7.0 |   147,192.9 ns |   1,507.18 ns |   1,409.81 ns |  0.02 |        - |     608 B |       0.000 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

|                   Method |  Runtime |         Mean |      Error |     StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
|           FullTextLookup | .NET 6.0 | 25,362.62 ns |  86.745 ns |  81.141 ns |  1.00 |    0.00 |      - |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 6.0 | 25,073.88 ns | 103.270 ns |  91.546 ns |  0.99 |    0.00 |      - |     464 B |        1.09 |
|                          |          |              |            |            |       |         |        |           |             |
|           FullTextLookup | .NET 7.0 | 19,415.45 ns | 134.922 ns | 119.604 ns |  1.00 |    0.00 |      - |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 7.0 | 19,553.58 ns | 135.104 ns | 126.376 ns |  1.01 |    0.01 |      - |     464 B |        1.09 |
|                          |          |              |            |            |       |         |        |           |             |
|           LessThanLookup | .NET 6.0 |     25.35 ns |   0.119 ns |   0.112 ns |  1.00 |    0.00 | 0.0038 |      64 B |        1.00 |
| ConcurrentLessThanLookup | .NET 6.0 |     52.05 ns |   0.118 ns |   0.110 ns |  2.05 |    0.01 | 0.0038 |      64 B |        1.00 |
|                          |          |              |            |            |       |         |        |           |             |
|           LessThanLookup | .NET 7.0 |     22.00 ns |   0.035 ns |   0.033 ns |  1.00 |    0.00 |      - |         - |          NA |
| ConcurrentLessThanLookup | .NET 7.0 |     49.20 ns |   0.319 ns |   0.298 ns |  2.24 |    0.02 |      - |         - |          NA |
|                          |          |              |            |            |       |         |        |           |             |
|             UniqueLookup | .NET 6.0 |     15.75 ns |   0.061 ns |   0.057 ns |  1.00 |    0.00 |      - |         - |          NA |
|   ConcurrentUniqueLookup | .NET 6.0 |     34.87 ns |   0.716 ns |   0.906 ns |  2.24 |    0.06 |      - |         - |          NA |
|                          |          |              |            |            |       |         |        |           |             |
|             UniqueLookup | .NET 7.0 |     15.43 ns |   0.014 ns |   0.012 ns |  1.00 |    0.00 |      - |         - |          NA |
|   ConcurrentUniqueLookup | .NET 7.0 |     34.09 ns |   0.068 ns |   0.060 ns |  2.21 |    0.00 |      - |         - |          NA |

> ℹ️ For more complex scenarios, the synchronization cost is negligble. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
