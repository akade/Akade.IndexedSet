# Benchmarks
Benchmarks measured on 27.07.2022:

## Environment
``` ini
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3447/23H2/2023Update/SunValley3)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.300-preview.24203.14
  [Host]   : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.29 (6.0.2924.17105), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
```

All benchmarks are currenlty using *1k elements*. Benchmarks showing the scaling is on the roadmap - expect IndexedSet to scale much
much better than the naive LINQ Queries. As .NET 8.0 brings in [a bunch of performance improvements](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/),
the benchmarks are run against both .NET 6 and 8.
.NET 7 is out of support and hence, no longer an explicit target (but the library can be consumed from .NET 7 without any issue).

## Unique-Index
| Method                       | Job      | Runtime  | Mean        | Error       | StdDev    | Ratio | Code Size | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |--------- |--------- |------------:|------------:|----------:|------:|----------:|-------:|----------:|------------:|
| Unqiue_Linq                  | .NET 8.0 | .NET 8.0 | 92,618.7 ns | 1,077.03 ns | 954.76 ns | 1.000 |   1,040 B | 0.7324 |   12800 B |        1.00 |
| Unique_Dictionary            | .NET 8.0 | .NET 8.0 |    373.2 ns |     0.78 ns |   0.65 ns | 0.004 |     505 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 8.0 | .NET 8.0 |    788.6 ns |     0.76 ns |   0.67 ns | 0.009 |   1,631 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 8.0 | .NET 8.0 |    803.5 ns |     1.31 ns |   1.02 ns | 0.009 |   1,387 B |      - |         - |        0.00 |
|                              |          |          |             |             |           |       |           |        |           |             |
| Unqiue_Linq                  | .NET 9.0 | .NET 9.0 | 10,730.4 ns |    25.36 ns |  22.48 ns |  1.00 |     260 B | 0.5188 |    8800 B |        1.00 |
| Unique_Dictionary            | .NET 9.0 | .NET 9.0 |    356.9 ns |     0.30 ns |   0.27 ns |  0.03 |     485 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 9.0 | .NET 9.0 |    810.5 ns |     1.28 ns |   1.14 ns |  0.08 |     754 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 9.0 | .NET 9.0 |    746.7 ns |     1.23 ns |   1.02 ns |  0.07 |     843 B |      - |         - |        0.00 |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
| Method                | Job      | Runtime  | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Code Size | Allocated | Alloc Ratio |
|---------------------- |--------- |--------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|----------:|------------:|
| MultiValue_Linq       | .NET 8.0 | .NET 8.0 | 34.851 μs | 0.6304 μs | 1.1206 μs |  1.00 |    0.04 | 0.0610 |   3,832 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 8.0 | .NET 8.0 |  8.323 μs | 0.0343 μs | 0.0286 μs |  0.24 |    0.01 | 0.0153 |   2,879 B |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 8.0 | .NET 8.0 |  8.814 μs | 0.1749 μs | 0.2148 μs |  0.25 |    0.01 | 0.0153 |   3,623 B |     360 B |        0.21 |
|                       |          |          |           |           |           |       |         |        |           |           |             |
| MultiValue_Linq       | .NET 9.0 | .NET 9.0 | 37.035 μs | 0.6287 μs | 0.5881 μs |  1.00 |    0.02 | 0.0610 |   3,135 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 9.0 | .NET 9.0 |  7.816 μs | 0.0377 μs | 0.0352 μs |  0.21 |    0.00 | 0.0153 |   2,412 B |     288 B |        0.17 |
| Multivalue_IndexedSet | .NET 9.0 | .NET 9.0 |  8.639 μs | 0.0633 μs | 0.0592 μs |  0.23 |    0.00 | 0.0153 |   2,772 B |     360 B |        0.21 |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
| Method              | Job      | Runtime  | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------- |--------- |--------- |--------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| LessThan_Linq       | .NET 8.0 | .NET 8.0 | 12,172.573 ns |  65.0425 ns |  60.8408 ns |  1.00 |    0.01 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 8.0 | .NET 8.0 |  3,439.486 ns |   5.9086 ns |   5.5269 ns |  0.28 |    0.00 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| LessThan_Linq       | .NET 9.0 | .NET 9.0 |  4,379.574 ns |  26.8995 ns |  23.8457 ns |  1.00 |    0.01 | 0.0076 |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 9.0 | .NET 9.0 |  3,449.437 ns |   8.2712 ns |   6.9068 ns |  0.79 |    0.00 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Min_Linq            | .NET 8.0 | .NET 8.0 | 35,974.568 ns | 715.3502 ns | 765.4167 ns | 1.000 |    0.03 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 8.0 | .NET 8.0 |      5.179 ns |   0.0153 ns |   0.0143 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Min_Linq            | .NET 9.0 | .NET 9.0 | 30,744.936 ns | 607.9471 ns | 746.6137 ns | 1.001 |    0.03 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 9.0 | .NET 9.0 |      4.948 ns |   0.0066 ns |   0.0062 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Paging_Linq         | .NET 8.0 | .NET 8.0 | 42,730.709 ns | 165.4306 ns | 154.7439 ns | 1.000 |    0.00 | 9.5215 | 2.3804 |  160352 B |       1.000 |
| Paging_IndexedSet   | .NET 8.0 | .NET 8.0 |    146.687 ns |   2.1497 ns |   2.0108 ns | 0.003 |    0.00 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Paging_Linq         | .NET 9.0 | .NET 9.0 | 41,435.160 ns | 455.6568 ns | 426.2217 ns | 1.000 |    0.01 | 9.5215 | 1.1597 |  160464 B |       1.000 |
| Paging_IndexedSet   | .NET 9.0 | .NET 9.0 |    121.028 ns |   0.6311 ns |   0.5903 ns | 0.003 |    0.00 | 0.0138 |      - |     232 B |       0.001 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Range_Linq          | .NET 8.0 | .NET 8.0 | 13,410.602 ns |  65.6024 ns |  61.3645 ns |  1.00 |    0.01 |      - |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 8.0 | .NET 8.0 |  2,919.190 ns |   2.2875 ns |   1.7859 ns |  0.22 |    0.00 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |             |             |       |         |        |        |           |             |
| Range_Linq          | .NET 9.0 | .NET 9.0 |  4,440.144 ns |  20.0350 ns |  18.7407 ns |  1.00 |    0.01 | 0.0076 |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 9.0 | .NET 9.0 |  2,713.934 ns |   9.9596 ns |   8.8289 ns |  0.61 |    0.00 | 0.0038 |      - |      72 B |        0.45 |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
| Method                     | Job      | Runtime  | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |--------- |--------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| FuzzyStartsWith_Linq       | .NET 8.0 | .NET 8.0 | 58,900.4 ns | 187.75 ns | 175.62 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 8.0 | .NET 8.0 | 15,862.9 ns |  51.72 ns |  43.19 ns |  0.27 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |        |           |             |
| FuzzyStartsWith_Linq       | .NET 9.0 | .NET 9.0 | 61,080.0 ns | 230.47 ns | 215.58 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 9.0 | .NET 9.0 | 13,758.7 ns |  21.79 ns |  17.01 ns |  0.23 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |        |           |             |
| StartsWith_Linq            | .NET 8.0 | .NET 8.0 |  2,137.0 ns |   1.58 ns |   1.40 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 8.0 | .NET 8.0 |    592.4 ns |   1.11 ns |   1.04 ns |  0.28 | 0.0086 |     144 B |        1.12 |
|                            |          |          |             |           |           |       |        |           |             |
| StartsWith_Linq            | .NET 9.0 | .NET 9.0 |  1,347.1 ns |   0.99 ns |   0.83 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 9.0 | .NET 9.0 |    381.8 ns |   0.54 ns |   0.48 ns |  0.28 | 0.0086 |     144 B |        1.12 |
> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

| Method                   | Job      | Runtime  | Mean           | Error        | StdDev       | Ratio | Gen0     | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |---------------:|-------------:|-------------:|------:|---------:|----------:|------------:|
| Contains_Linq            | .NET 8.0 | .NET 8.0 |     5,683.6 ns |      5.98 ns |      5.60 ns |  1.00 |   0.0229 |     408 B |        1.00 |
| Contains_IndexedSet      | .NET 8.0 | .NET 8.0 |       424.4 ns |      0.52 ns |      0.46 ns |  0.07 |   0.0238 |     400 B |        0.98 |
|                          |          |          |                |              |              |       |          |           |             |
| Contains_Linq            | .NET 9.0 | .NET 9.0 |     5,003.8 ns |      5.28 ns |      4.68 ns |  1.00 |   0.0076 |     176 B |        1.00 |
| Contains_IndexedSet      | .NET 9.0 | .NET 9.0 |       337.7 ns |      0.80 ns |      0.75 ns |  0.07 |   0.0238 |     400 B |        2.27 |
|                          |          |          |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 8.0 | .NET 8.0 | 6,252,849.7 ns | 11,934.09 ns | 11,163.15 ns |  1.00 | 281.2500 | 4831742 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 8.0 | .NET 8.0 |   140,481.3 ns |    325.54 ns |    304.51 ns |  0.02 |        - |     608 B |       0.000 |
|                          |          |          |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 9.0 | .NET 9.0 | 5,974,308.9 ns | 17,245.07 ns | 16,131.04 ns |  1.00 | 281.2500 | 4831736 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 9.0 | .NET 9.0 |   124,523.5 ns |    789.63 ns |    738.62 ns |  0.02 |        - |     608 B |       0.000 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

| Method                   | Job      | Runtime  | Mean          | Error      | StdDev     | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |--------------:|-----------:|-----------:|------:|--------:|----------:|------------:|
| FullTextLookup           | .NET 8.0 | .NET 8.0 | 17,781.201 ns | 30.5715 ns | 28.5966 ns |  1.00 |    0.00 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 8.0 | .NET 8.0 | 17,562.165 ns | 39.1982 ns | 34.7482 ns |  0.99 |    0.00 |     464 B |        1.09 |
|                          |          |          |               |            |            |       |         |           |             |
| FullTextLookup           | .NET 9.0 | .NET 9.0 | 16,162.325 ns | 13.2668 ns | 10.3578 ns |  1.00 |    0.00 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 9.0 | .NET 9.0 | 16,220.753 ns | 25.7419 ns | 22.8195 ns |  1.00 |    0.00 |     464 B |        1.09 |
|                          |          |          |               |            |            |       |         |           |             |
| LessThanLookup           | .NET 8.0 | .NET 8.0 |     10.967 ns |  0.0387 ns |  0.0343 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentLessThanLookup | .NET 8.0 | .NET 8.0 |     56.053 ns |  0.8625 ns |  0.7646 ns |  5.11 |    0.07 |         - |          NA |
|                          |          |          |               |            |            |       |         |           |             |
| LessThanLookup           | .NET 9.0 | .NET 9.0 |     12.063 ns |  0.0193 ns |  0.0180 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentLessThanLookup | .NET 9.0 | .NET 9.0 |     41.691 ns |  0.8385 ns |  0.9656 ns |  3.46 |    0.08 |         - |          NA |
|                          |          |          |               |            |            |       |         |           |             |
| UniqueLookup             | .NET 8.0 | .NET 8.0 |      9.380 ns |  0.0273 ns |  0.0242 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 8.0 | .NET 8.0 |     33.221 ns |  0.6806 ns |  1.7447 ns |  3.54 |    0.19 |         - |          NA |
|                          |          |          |               |            |            |       |         |           |             |
| UniqueLookup             | .NET 9.0 | .NET 9.0 |      8.900 ns |  0.0231 ns |  0.0205 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 9.0 | .NET 9.0 |     24.075 ns |  0.4980 ns |  1.0396 ns |  2.71 |    0.12 |         - |          NA |

> ℹ️ For more complex scenarios, the synchronization cost is negligible. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
