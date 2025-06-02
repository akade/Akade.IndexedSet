# Benchmarks
Benchmarks measured on 27.07.2022:

## Environment
``` ini
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  .NET 9.0 : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
```

All benchmarks are currenlty using *1k elements*. Benchmarks showing the scaling is on the roadmap - expect IndexedSet to scale much
much better than the naive LINQ Queries. As .NET 9.0 brings in [a bunch of performance improvements](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-9/),
the benchmarks are run against both .NET 8 and 9.

## Unique-Index
| Method                       | Job      | Runtime  | Mean        | Error       | StdDev      | Ratio | RatioSD | Code Size | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |--------- |--------- |------------:|------------:|------------:|------:|--------:|----------:|-------:|----------:|------------:|
| Unqiue_Linq                  | .NET 8.0 | .NET 8.0 | 95,150.6 ns | 1,856.80 ns | 3,441.70 ns | 1.001 |    0.05 |   1,040 B | 0.7324 |   12800 B |        1.00 |
| Unique_Dictionary            | .NET 8.0 | .NET 8.0 |    360.3 ns |     0.49 ns |     0.43 ns | 0.004 |    0.00 |     514 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 8.0 | .NET 8.0 |    789.7 ns |     1.76 ns |     1.64 ns | 0.008 |    0.00 |   1,622 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 8.0 | .NET 8.0 |    798.9 ns |     1.95 ns |     1.83 ns | 0.008 |    0.00 |   1,387 B |      - |         - |        0.00 |
|                              |          |          |             |             |             |       |         |           |        |           |             |
| Unqiue_Linq                  | .NET 9.0 | .NET 9.0 |  9,734.1 ns |    27.71 ns |    25.92 ns |  1.00 |    0.00 |     260 B | 0.5188 |    8800 B |        1.00 |
| Unique_Dictionary            | .NET 9.0 | .NET 9.0 |    358.5 ns |     0.62 ns |     0.58 ns |  0.04 |    0.00 |     485 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 9.0 | .NET 9.0 |    850.3 ns |     1.93 ns |     1.80 ns |  0.09 |    0.00 |     754 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 9.0 | .NET 9.0 |    784.0 ns |     1.93 ns |     1.71 ns |  0.08 |    0.00 |     843 B |      - |         - |        0.00 |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
| Method                | Job      | Runtime  | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Code Size | Allocated | Alloc Ratio |
|---------------------- |--------- |--------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|----------:|------------:|
| MultiValue_Linq       | .NET 8.0 | .NET 8.0 | 39.322 μs | 0.7690 μs | 1.3058 μs |  1.00 |    0.05 | 0.0610 |   3,844 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 8.0 | .NET 8.0 |  8.726 μs | 0.0972 μs | 0.0909 μs |  0.22 |    0.01 | 0.0153 |   2,864 B |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 8.0 | .NET 8.0 |  8.742 μs | 0.1702 μs | 0.1960 μs |  0.22 |    0.01 | 0.0153 |   3,624 B |     360 B |        0.21 |
|                       |          |          |           |           |           |       |         |        |           |           |             |
| MultiValue_Linq       | .NET 9.0 | .NET 9.0 | 37.828 μs | 0.7422 μs | 0.8835 μs |  1.00 |    0.03 | 0.0610 |   3,135 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 9.0 | .NET 9.0 |  7.385 μs | 0.0256 μs | 0.0200 μs |  0.20 |    0.00 | 0.0153 |   2,414 B |     288 B |        0.17 |
| Multivalue_IndexedSet | .NET 9.0 | .NET 9.0 |  8.571 μs | 0.1204 μs | 0.1126 μs |  0.23 |    0.01 | 0.0153 |   2,772 B |     360 B |        0.21 |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
| Method              | Job      | Runtime  | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------- |--------- |--------- |--------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| LessThan_Linq       | .NET 8.0 | .NET 8.0 | 12,202.818 ns |  38.0986 ns |  31.8141 ns |  1.00 |    0.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 8.0 | .NET 8.0 |  3,510.905 ns |  33.8899 ns |  31.7006 ns |  0.29 |    0.00 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| LessThan_Linq       | .NET 9.0 | .NET 9.0 |  4,391.714 ns |  31.0503 ns |  25.9284 ns |  1.00 |    0.01 | 0.0076 |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 9.0 | .NET 9.0 |  3,480.802 ns |  11.6068 ns |  10.8570 ns |  0.79 |    0.01 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Min_Linq            | .NET 8.0 | .NET 8.0 | 38,406.650 ns | 764.9037 ns | 967.3583 ns | 1.001 |    0.04 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 8.0 | .NET 8.0 |      5.287 ns |   0.0341 ns |   0.0319 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Min_Linq            | .NET 9.0 | .NET 9.0 | 31,559.012 ns | 609.4903 ns | 748.5089 ns | 1.001 |    0.03 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 9.0 | .NET 9.0 |      5.087 ns |   0.0272 ns |   0.0242 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Paging_Linq         | .NET 8.0 | .NET 8.0 | 49,019.614 ns | 538.2468 ns | 503.4764 ns | 1.000 |    0.01 | 9.5215 | 2.3804 |  160352 B |       1.000 |
| Paging_IndexedSet   | .NET 8.0 | .NET 8.0 |    153.676 ns |   1.6417 ns |   1.5357 ns | 0.003 |    0.00 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Paging_Linq         | .NET 9.0 | .NET 9.0 | 40,252.494 ns | 709.1066 ns | 663.2987 ns | 1.000 |    0.02 | 9.5215 | 1.1597 |  160464 B |       1.000 |
| Paging_IndexedSet   | .NET 9.0 | .NET 9.0 |    123.875 ns |   2.2537 ns |   2.1081 ns | 0.003 |    0.00 | 0.0138 |      - |     232 B |       0.001 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Range_Linq          | .NET 8.0 | .NET 8.0 | 14,455.695 ns | 284.9172 ns | 468.1271 ns |  1.00 |    0.05 |      - |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 8.0 | .NET 8.0 |  2,704.781 ns |  53.9501 ns |  50.4649 ns |  0.19 |    0.01 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Range_Linq          | .NET 9.0 | .NET 9.0 |  4,376.953 ns |  42.5539 ns |  39.8050 ns |  1.00 |    0.01 | 0.0076 |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 9.0 | .NET 9.0 |  2,946.508 ns |  19.8687 ns |  18.5852 ns |  0.67 |    0.01 | 0.0038 |      - |      72 B |        0.45 |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
| Method                     | Job      | Runtime  | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |--------- |--------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| FuzzyStartsWith_Linq       | .NET 8.0 | .NET 8.0 | 58,445.7 ns | 157.84 ns | 147.64 ns |  1.00 |    0.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 8.0 | .NET 8.0 | 21,850.5 ns |  49.15 ns |  43.57 ns |  0.37 |    0.00 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |         |        |           |             |
| FuzzyStartsWith_Linq       | .NET 9.0 | .NET 9.0 | 58,022.0 ns | 680.04 ns | 636.11 ns |  1.00 |    0.02 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 9.0 | .NET 9.0 | 21,369.8 ns |  90.70 ns |  84.85 ns |  0.37 |    0.00 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |         |        |           |             |
| StartsWith_Linq            | .NET 8.0 | .NET 8.0 |  2,025.9 ns |   4.27 ns |   3.78 ns |  1.00 |    0.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 8.0 | .NET 8.0 |    602.7 ns |   1.41 ns |   1.25 ns |  0.30 |    0.00 | 0.0086 |     144 B |        1.12 |
|                            |          |          |             |           |           |       |         |        |           |             |
| StartsWith_Linq            | .NET 9.0 | .NET 9.0 |  1,276.4 ns |   3.59 ns |   3.36 ns |  1.00 |    0.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 9.0 | .NET 9.0 |    396.8 ns |   1.12 ns |   1.05 ns |  0.31 |    0.00 | 0.0086 |     144 B |        1.12 |
> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

| Method                   | Job      | Runtime  | Mean           | Error        | StdDev       | Ratio | Gen0     | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |---------------:|-------------:|-------------:|------:|---------:|----------:|------------:|
| Contains_Linq            | .NET 8.0 | .NET 8.0 |     5,915.0 ns |     16.05 ns |     15.02 ns |  1.00 |   0.0229 |     408 B |        1.00 |
| Contains_IndexedSet      | .NET 8.0 | .NET 8.0 |       429.2 ns |      1.63 ns |      1.36 ns |  0.07 |   0.0238 |     400 B |        0.98 |
|                          |          |          |                |              |              |       |          |           |             |
| Contains_Linq            | .NET 9.0 | .NET 9.0 |     5,029.3 ns |      7.84 ns |      6.95 ns |  1.00 |   0.0076 |     176 B |        1.00 |
| Contains_IndexedSet      | .NET 9.0 | .NET 9.0 |       351.9 ns |      1.46 ns |      1.37 ns |  0.07 |   0.0238 |     400 B |        2.27 |
|                          |          |          |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 8.0 | .NET 8.0 | 6,356,542.1 ns | 65,872.79 ns | 61,617.45 ns |  1.00 | 281.2500 | 4831744 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 8.0 | .NET 8.0 |   198,529.9 ns |    642.52 ns |    601.01 ns |  0.03 |        - |     608 B |       0.000 |
|                          |          |          |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 9.0 | .NET 9.0 | 6,084,268.4 ns | 58,886.56 ns | 55,082.53 ns |  1.00 | 281.2500 | 4831737 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 9.0 | .NET 9.0 |   196,501.6 ns |  1,008.63 ns |    943.48 ns |  0.03 |        - |     608 B |       0.000 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

| Method                   | Job      | Runtime  | Mean          | Error      | StdDev     | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |--------------:|-----------:|-----------:|------:|--------:|----------:|------------:|
| FullTextLookup           | .NET 8.0 | .NET 8.0 | 24,578.302 ns | 42.3126 ns | 37.5090 ns |  1.00 |    0.00 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 8.0 | .NET 8.0 | 25,049.091 ns | 81.2626 ns | 72.0372 ns |  1.02 |    0.00 |     464 B |        1.09 |
|                          |          |          |               |            |            |       |         |           |             |
| FullTextLookup           | .NET 9.0 | .NET 9.0 | 23,501.549 ns | 37.4894 ns | 33.2334 ns |  1.00 |    0.00 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 9.0 | .NET 9.0 | 23,613.616 ns | 33.7388 ns | 31.5593 ns |  1.00 |    0.00 |     464 B |        1.09 |
|                          |          |          |               |            |            |       |         |           |             |
| LessThanLookup           | .NET 8.0 | .NET 8.0 |     10.952 ns |  0.0518 ns |  0.0484 ns |  1.00 |    0.01 |         - |          NA |
| ConcurrentLessThanLookup | .NET 8.0 | .NET 8.0 |     55.013 ns |  0.8904 ns |  0.7893 ns |  5.02 |    0.07 |         - |          NA |
|                          |          |          |               |            |            |       |         |           |             |
| LessThanLookup           | .NET 9.0 | .NET 9.0 |     10.724 ns |  0.0096 ns |  0.0090 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentLessThanLookup | .NET 9.0 | .NET 9.0 |     44.744 ns |  0.8647 ns |  0.8492 ns |  4.17 |    0.08 |         - |          NA |
|                          |          |          |               |            |            |       |         |           |             |
| UniqueLookup             | .NET 8.0 | .NET 8.0 |      9.759 ns |  0.0245 ns |  0.0229 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 8.0 | .NET 8.0 |     38.876 ns |  0.7559 ns |  0.8401 ns |  3.98 |    0.08 |         - |          NA |
|                          |          |          |               |            |            |       |         |           |             |
| UniqueLookup             | .NET 9.0 | .NET 9.0 |      9.196 ns |  0.0690 ns |  0.0645 ns |  1.00 |    0.01 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 9.0 | .NET 9.0 |     25.773 ns |  0.5374 ns |  0.7533 ns |  2.80 |    0.08 |         - |          NA |

> ℹ️ For more complex scenarios, the synchronization cost is negligible. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
