# Benchmarks

## Environment
``` ini
BenchmarkDotNet v0.16.0-preview.2, Windows 11 (10.0.26300.9711)
AMD Ryzen 9 5900X 3.69GHz, 1 CPU, 24 logical and 12 physical cores
Memory: 31.91 GB Total, 7.72 GB Available
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-GVKUBM : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-IHFIKV : .NET 11.0.0 (11.0.0-rc.2.26504.105, 11.0.26.50505), X64 RyuJIT x86-64-v3
```

All benchmarks are currenlty using *1k elements*. Benchmarks showing the scaling is on the roadmap - expect IndexedSet to scale much
much better than the naive LINQ Queries.

## Unique-Index
<!-- begin-snippet: Akade.IndexedSet.Benchmarks UniqueIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                       | Runtime   | Mean        | Error    | StdDev   | Ratio | Gen0   | Code Size | Allocated | Alloc Ratio |
|----------------------------- |---------- |------------:|---------:|---------:|------:|-------:|----------:|----------:|------------:|
| Unqiue_Linq                  | .NET 10.0 | 10,013.7 ns | 40.70 ns | 38.07 ns |  1.00 | 0.5188 |     721 B |    8800 B |        1.00 |
| Unique_Dictionary            | .NET 10.0 |    253.9 ns |  1.25 ns |  1.11 ns |  0.03 |      - |     529 B |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 10.0 |    672.2 ns |  4.58 ns |  3.58 ns |  0.07 |      - |     717 B |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 10.0 |    654.0 ns |  2.99 ns |  2.50 ns |  0.07 |      - |     809 B |         - |        0.00 |
|                              |           |             |          |          |       |        |           |           |             |
| Unqiue_Linq                  | .NET 11.0 | 11,346.9 ns | 40.44 ns | 35.85 ns |  1.00 | 0.4730 |     745 B |    8000 B |        1.00 |
| Unique_Dictionary            | .NET 11.0 |    251.2 ns |  0.67 ns |  0.63 ns |  0.02 |      - |     511 B |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 11.0 |    661.1 ns |  2.13 ns |  1.78 ns |  0.06 |      - |   1,475 B |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 11.0 |    618.4 ns |  6.77 ns |  6.34 ns |  0.05 |      - |   1,150 B |         - |        0.00 |

<!-- end-snippet -->
> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
<!-- begin-snippet: Akade.IndexedSet.Benchmarks MultiValueIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                | Runtime   | Mean      | Error     | StdDev    | Ratio | Code Size | Gen0   | Allocated | Alloc Ratio |
|---------------------- |---------- |----------:|----------:|----------:|------:|----------:|-------:|----------:|------------:|
| MultiValue_Linq       | .NET 10.0 | 36.640 μs | 0.7197 μs | 0.6732 μs |  1.00 |   3,706 B | 0.0610 |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 10.0 |  7.956 μs | 0.0419 μs | 0.0392 μs |  0.22 |   2,455 B | 0.0153 |     288 B |        0.17 |
| Multivalue_IndexedSet | .NET 10.0 |  9.321 μs | 0.1108 μs | 0.1036 μs |  0.25 |   3,008 B | 0.0153 |     360 B |        0.21 |
|                       |           |           |           |           |       |           |        |           |             |
| MultiValue_Linq       | .NET 11.0 | 51.536 μs | 0.2483 μs | 0.2201 μs |  1.00 |   3,735 B | 0.0610 |    1600 B |        1.00 |
| Multivalue_Lookup     | .NET 11.0 | 19.618 μs | 0.1160 μs | 0.1028 μs |  0.38 |   2,413 B |      - |     288 B |        0.18 |
| Multivalue_IndexedSet | .NET 11.0 | 20.996 μs | 0.1067 μs | 0.0998 μs |  0.41 |   3,430 B |      - |     360 B |        0.23 |

<!-- end-snippet -->

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
<!-- begin-snippet: Akade.IndexedSet.Benchmarks RangeIndexBenchmarks(importer:benchmarkdotnet) -->
| Method              | Runtime   | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------- |---------- |--------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| LessThan_Linq       | .NET 10.0 | 16,422.814 ns | 314.3499 ns | 278.6630 ns |  1.00 |    0.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 10.0 |  3,243.279 ns |  34.3283 ns |  32.1108 ns |  0.20 |    0.00 | 0.0038 |      - |      72 B |        0.45 |
|                     |           |               |             |             |       |         |        |        |           |             |
| LessThan_Linq       | .NET 11.0 |  4,369.294 ns |  86.4966 ns | 146.8778 ns |  1.00 |    0.00 | 0.0076 |      - |     152 B |        1.00 |
| LessThan_IndexedSet | .NET 11.0 |  3,470.992 ns |   9.1059 ns |   8.0722 ns |  0.80 |    0.03 | 0.0038 |      - |      72 B |        0.47 |
|                     |           |               |             |             |       |         |        |        |           |             |
| Min_Linq            | .NET 10.0 | 15,497.860 ns | 305.9479 ns | 665.1057 ns | 1.000 |    0.00 |      - |      - |         - |          NA |
| Min_IndexedSet      | .NET 10.0 |      5.591 ns |   0.0299 ns |   0.0293 ns | 0.000 |    0.00 |      - |      - |         - |          NA |
|                     |           |               |             |             |       |         |        |        |           |             |
| Min_Linq            | .NET 11.0 |  5,454.670 ns |  38.0917 ns |  29.7395 ns | 1.000 |    0.00 |      - |      - |         - |          NA |
| Min_IndexedSet      | .NET 11.0 |      4.949 ns |   0.0414 ns |   0.0345 ns | 0.001 |    0.00 |      - |      - |         - |          NA |
|                     |           |               |             |             |       |         |        |        |           |             |
| Paging_Linq         | .NET 10.0 | 34,887.788 ns | 558.0706 ns | 522.0196 ns | 1.000 |    0.00 | 9.5215 | 1.1597 |  160464 B |       1.000 |
| Paging_IndexedSet   | .NET 10.0 |    101.239 ns |   1.1623 ns |   1.0303 ns | 0.003 |    0.00 | 0.0138 |      - |     232 B |       0.001 |
|                     |           |               |             |             |       |         |        |        |           |             |
| Paging_Linq         | .NET 11.0 | 37,199.729 ns | 611.2982 ns | 571.8088 ns | 1.000 |    0.00 | 9.5215 | 1.1597 |  160464 B |       1.000 |
| Paging_IndexedSet   | .NET 11.0 |     97.084 ns |   1.0172 ns |   0.9514 ns | 0.003 |    0.00 | 0.0138 |      - |     232 B |       0.001 |
|                     |           |               |             |             |       |         |        |        |           |             |
| Range_Linq          | .NET 10.0 | 18,421.383 ns |  63.8470 ns |  59.7225 ns |  1.00 |    0.00 |      - |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 10.0 |  2,704.473 ns |   5.8248 ns |   5.4485 ns |  0.15 |    0.00 | 0.0038 |      - |      72 B |        0.45 |
|                     |           |               |             |             |       |         |        |        |           |             |
| Range_Linq          | .NET 11.0 |  4,405.702 ns |  27.1274 ns |  25.3750 ns |  1.00 |    0.00 | 0.0076 |      - |     152 B |        1.00 |
| Range_IndexedSet    | .NET 11.0 |  2,921.238 ns |   6.8852 ns |   6.4404 ns |  0.66 |    0.00 | 0.0038 |      - |      72 B |        0.47 |

<!-- end-snippet -->

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
<!-- begin-snippet: Akade.IndexedSet.Benchmarks PrefixIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                     | Runtime   | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------- |------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| FuzzyStartsWith_Linq       | .NET 10.0 | 59,794.8 ns | 318.22 ns | 297.66 ns |  1.00 |    0.00 | 2.3804 |      - |   40088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 10.0 | 19,849.1 ns | 124.72 ns | 116.66 ns |  0.33 |    0.00 |      - |      - |     240 B |       0.006 |
| FuzzyStartsWith_Autypo     | .NET 10.0 | 63,074.0 ns | 373.30 ns | 349.19 ns |  1.05 |    0.01 | 0.3662 |      - |    6656 B |       0.166 |
|                            |           |             |           |           |       |         |        |        |           |             |
| FuzzyStartsWith_Linq       | .NET 11.0 | 55,906.9 ns | 307.36 ns | 287.50 ns |  1.00 |    0.00 | 2.3804 |      - |   40088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 11.0 | 17,567.5 ns |  69.96 ns |  65.44 ns |  0.31 |    0.00 |      - |      - |     240 B |       0.006 |
| FuzzyStartsWith_Autypo     | .NET 11.0 | 60,173.8 ns | 275.17 ns | 243.93 ns |  1.08 |    0.01 | 0.3662 |      - |    6656 B |       0.166 |
|                            |           |             |           |           |       |         |        |        |           |             |
| StartsWith_Linq            | .NET 10.0 |  1,809.5 ns |  12.87 ns |  12.04 ns |  1.00 |    0.00 | 0.0076 |      - |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 10.0 |    363.1 ns |   1.22 ns |   1.02 ns |  0.20 |    0.00 | 0.0086 |      - |     144 B |        1.12 |
| StartsWith_Autypo          | .NET 10.0 |  2,101.0 ns |  33.78 ns |  28.21 ns |  1.16 |    0.02 | 0.3967 | 0.0038 |    6664 B |       52.06 |
|                            |           |             |           |           |       |         |        |        |           |             |
| StartsWith_Linq            | .NET 11.0 |  2,066.8 ns |  10.28 ns |   9.62 ns |  1.00 |    0.00 | 0.0076 |      - |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 11.0 |    355.3 ns |   0.94 ns |   0.83 ns |  0.17 |    0.00 | 0.0086 |      - |     144 B |        1.12 |
| StartsWith_Autypo          | .NET 11.0 |  2,158.9 ns |  31.11 ns |  25.98 ns |  1.04 |    0.01 | 0.3967 | 0.0038 |    6664 B |       52.06 |

<!-- end-snippet -->

## FullText-Index

<!-- begin-snippet: Akade.IndexedSet.Benchmarks FullTextIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                   | Runtime   | Mean           | Error        | StdDev       | Ratio | Gen0     | Allocated | Alloc Ratio |
|------------------------- |---------- |---------------:|-------------:|-------------:|------:|---------:|----------:|------------:|
| Contains_Linq            | .NET 10.0 |     5,772.6 ns |     50.22 ns |     41.93 ns |  1.00 |   0.0076 |     176 B |        1.00 |
| Contains_IndexedSet      | .NET 10.0 |       340.3 ns |      6.18 ns |      5.78 ns |  0.06 |   0.0238 |     400 B |        2.27 |
|                          |           |                |              |              |       |          |           |             |
| Contains_Linq            | .NET 11.0 |     5,035.0 ns |     14.11 ns |     11.79 ns |  1.00 |   0.0076 |     176 B |        1.00 |
| Contains_IndexedSet      | .NET 11.0 |       328.3 ns |      3.29 ns |      3.08 ns |  0.07 |   0.0238 |     400 B |        2.27 |
|                          |           |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 10.0 | 6,744,603.1 ns | 49,077.14 ns | 45,906.79 ns |  1.00 | 281.2500 | 4831736 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 10.0 |   169,752.4 ns |  1,877.87 ns |  1,568.10 ns |  0.03 |        - |     608 B |       0.000 |
|                          |           |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 11.0 | 6,371,142.9 ns | 41,181.53 ns | 34,388.44 ns |  1.00 | 281.2500 | 4831736 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 11.0 |   158,040.4 ns |    503.09 ns |    445.97 ns |  0.02 |        - |     608 B |       0.000 |

<!-- end-snippet -->

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

<!-- begin-snippet: Akade.IndexedSet.Benchmarks ConcurrentSetBenchmarks(importer:benchmarkdotnet) -->
| Method                   | Runtime   | Mean          | Error       | StdDev      | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------- |---------- |--------------:|------------:|------------:|------:|--------:|----------:|------------:|
| FullTextLookup           | .NET 10.0 | 19,532.806 ns |  58.1227 ns |  54.3680 ns |  1.00 |    0.00 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 10.0 | 19,686.142 ns |  96.5034 ns |  75.3435 ns |  1.01 |    0.00 |     464 B |        1.09 |
|                          |           |               |             |             |       |         |           |             |
| FullTextLookup           | .NET 11.0 | 19,937.915 ns |  70.0117 ns |  65.4890 ns |  1.00 |    0.00 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 11.0 | 20,082.995 ns | 113.7688 ns | 106.4194 ns |  1.01 |    0.01 |     464 B |        1.09 |
|                          |           |               |             |             |       |         |           |             |
| LessThanLookup           | .NET 10.0 |      9.557 ns |   0.0287 ns |   0.0268 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentLessThanLookup | .NET 10.0 |     20.776 ns |   0.0870 ns |   0.0814 ns |  2.17 |    0.01 |         - |          NA |
|                          |           |               |             |             |       |         |           |             |
| LessThanLookup           | .NET 11.0 |      8.828 ns |   0.0426 ns |   0.0398 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentLessThanLookup | .NET 11.0 |     27.859 ns |   0.5511 ns |   1.0485 ns |  3.16 |    0.12 |         - |          NA |
|                          |           |               |             |             |       |         |           |             |
| UniqueLookup             | .NET 10.0 |      7.972 ns |   0.0355 ns |   0.0315 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 10.0 |     16.679 ns |   0.0755 ns |   0.0669 ns |  2.09 |    0.01 |         - |          NA |
|                          |           |               |             |             |       |         |           |             |
| UniqueLookup             | .NET 11.0 |      7.259 ns |   0.0130 ns |   0.0108 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 11.0 |     18.728 ns |   0.3679 ns |   0.4911 ns |  2.58 |    0.07 |         - |          NA |

<!-- end-snippet -->

> ℹ️ For more complex scenarios, the synchronization cost is negligible. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
