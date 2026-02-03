# Benchmarks

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
<!-- begin-snippet: Akade.IndexedSet.Benchmarks UniqueIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                       | Job       | Runtime   | Mean        | Error     | StdDev    | Median      | Ratio | RatioSD | Code Size | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |------------:|----------:|----------:|------------:|------:|--------:|----------:|-------:|----------:|------------:|
| Unqiue_Linq                  | .NET 10.0 | .NET 10.0 | 13,148.3 ns | 261.58 ns | 601.02 ns | 12,869.1 ns |  1.00 |    0.06 |     721 B | 0.5188 |    8800 B |        1.00 |
| Unique_Dictionary            | .NET 10.0 | .NET 10.0 |    288.7 ns |   5.37 ns |  10.96 ns |    283.7 ns |  0.02 |    0.00 |     524 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 10.0 | .NET 10.0 |    814.4 ns |  16.15 ns |  30.33 ns |    801.2 ns |  0.06 |    0.00 |     769 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 10.0 | .NET 10.0 |    725.4 ns |  14.40 ns |  29.42 ns |    710.7 ns |  0.06 |    0.00 |     861 B |      - |         - |        0.00 |
|                              |           |           |             |           |           |             |       |         |           |        |           |             |
| Unqiue_Linq                  | .NET 9.0  | .NET 9.0  | 10,493.8 ns | 208.35 ns | 370.34 ns | 10,403.4 ns |  1.00 |    0.05 |   1,005 B | 0.5188 |    8800 B |        1.00 |
| Unique_Dictionary            | .NET 9.0  | .NET 9.0  |    414.5 ns |   8.32 ns |  15.62 ns |    405.5 ns |  0.04 |    0.00 |     485 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 9.0  | .NET 9.0  |    833.0 ns |  24.01 ns |  70.80 ns |    778.8 ns |  0.08 |    0.01 |     754 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 9.0  | .NET 9.0  |    822.3 ns |   1.49 ns |   1.32 ns |    822.6 ns |  0.08 |    0.00 |     843 B |      - |         - |        0.00 |

<!-- end-snippet -->
> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
<!-- begin-snippet: Akade.IndexedSet.Benchmarks MultiValueIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                | Job       | Runtime   | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0   | Code Size | Allocated | Alloc Ratio |
|---------------------- |---------- |---------- |----------:|----------:|----------:|----------:|------:|--------:|-------:|----------:|----------:|------------:|
| MultiValue_Linq       | .NET 10.0 | .NET 10.0 | 38.952 μs | 0.7758 μs | 1.7980 μs | 38.045 μs |  1.00 |    0.06 | 0.0610 |   3,774 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 10.0 | .NET 10.0 |  8.504 μs | 0.1633 μs | 0.2236 μs |  8.442 μs |  0.22 |    0.01 | 0.0153 |   2,531 B |     288 B |        0.17 |
| Multivalue_IndexedSet | .NET 10.0 | .NET 10.0 |  9.829 μs | 0.1937 μs | 0.3493 μs |  9.643 μs |  0.25 |    0.01 | 0.0153 |   3,161 B |     360 B |        0.21 |
|                       |           |           |           |           |           |           |       |         |        |           |           |             |
| MultiValue_Linq       | .NET 9.0  | .NET 9.0  | 36.465 μs | 0.7226 μs | 1.5241 μs | 35.834 μs |  1.00 |    0.06 | 0.0610 |   3,148 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 9.0  | .NET 9.0  |  8.977 μs | 0.1777 μs | 0.3787 μs |  8.808 μs |  0.25 |    0.01 | 0.0153 |   2,414 B |     288 B |        0.17 |
| Multivalue_IndexedSet | .NET 9.0  | .NET 9.0  |  9.394 μs | 0.1872 μs | 0.4263 μs |  9.206 μs |  0.26 |    0.02 | 0.0153 |   2,772 B |     360 B |        0.21 |

<!-- end-snippet -->

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
<!-- begin-snippet: Akade.IndexedSet.Benchmarks RangeIndexBenchmarks(importer:benchmarkdotnet) -->
| Method              | Job       | Runtime   | Mean          | Error       | StdDev        | Median        | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------- |---------- |---------- |--------------:|------------:|--------------:|--------------:|------:|--------:|-------:|-------:|----------:|------------:|
| LessThan_Linq       | .NET 10.0 | .NET 10.0 | 15,706.581 ns |  74.2673 ns |    65.8360 ns | 15,701.244 ns |  1.00 |    0.01 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 10.0 | .NET 10.0 |  3,179.832 ns |   4.1719 ns |     3.9024 ns |  3,179.079 ns |  0.20 |    0.00 | 0.0038 |      - |      72 B |        0.45 |
|                     |           |           |               |             |               |               |       |         |        |        |           |             |
| LessThan_Linq       | .NET 9.0  | .NET 9.0  |  4,316.148 ns |  22.1099 ns |    20.6816 ns |  4,321.387 ns |  1.00 |    0.01 | 0.0076 |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 9.0  | .NET 9.0  |  3,634.847 ns |  40.2365 ns |    37.6372 ns |  3,622.781 ns |  0.84 |    0.01 | 0.0038 |      - |      72 B |        0.45 |
|                     |           |           |               |             |               |               |       |         |        |        |           |             |
| Min_Linq            | .NET 10.0 | .NET 10.0 | 14,678.955 ns |  23.4169 ns |    20.7585 ns | 14,678.968 ns | 1.000 |    0.00 |      - |      - |         - |          NA |
| Min_IndexedSet      | .NET 10.0 | .NET 10.0 |      4.847 ns |   0.0110 ns |     0.0098 ns |      4.847 ns | 0.000 |    0.00 |      - |      - |         - |          NA |
|                     |           |           |               |             |               |               |       |         |        |        |           |             |
| Min_Linq            | .NET 9.0  | .NET 9.0  | 37,915.652 ns | 888.5133 ns | 2,577.7383 ns | 37,360.938 ns | 1.004 |    0.10 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 9.0  | .NET 9.0  |      5.593 ns |   0.1341 ns |     0.1966 ns |      5.558 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
|                     |           |           |               |             |               |               |       |         |        |        |           |             |
| Paging_Linq         | .NET 10.0 | .NET 10.0 | 42,206.163 ns | 841.4204 ns | 2,201.8508 ns | 41,359.338 ns | 1.003 |    0.07 | 9.5215 | 1.1597 |  160464 B |       1.000 |
| Paging_IndexedSet   | .NET 10.0 | .NET 10.0 |    106.013 ns |   2.1498 ns |     4.4397 ns |    104.024 ns | 0.003 |    0.00 | 0.0138 |      - |     232 B |       0.001 |
|                     |           |           |               |             |               |               |       |         |        |        |           |             |
| Paging_Linq         | .NET 9.0  | .NET 9.0  | 44,843.363 ns | 890.3682 ns | 2,422.3126 ns | 43,845.044 ns | 1.003 |    0.07 | 9.5215 | 1.1597 |  160464 B |       1.000 |
| Paging_IndexedSet   | .NET 9.0  | .NET 9.0  |    149.767 ns |   2.9575 ns |     7.3101 ns |    146.858 ns | 0.003 |    0.00 | 0.0138 |      - |     232 B |       0.001 |
|                     |           |           |               |             |               |               |       |         |        |        |           |             |
| Range_Linq          | .NET 10.0 | .NET 10.0 | 19,502.712 ns | 386.9089 ns |   857.3635 ns | 19,081.104 ns |  1.00 |    0.06 |      - |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 10.0 | .NET 10.0 |  2,773.752 ns |  55.4947 ns |   150.9773 ns |  2,714.090 ns |  0.14 |    0.01 | 0.0038 |      - |      72 B |        0.45 |
|                     |           |           |               |             |               |               |       |         |        |        |           |             |
| Range_Linq          | .NET 9.0  | .NET 9.0  |  5,084.356 ns | 101.6391 ns |   233.5328 ns |  4,994.598 ns |  1.00 |    0.06 | 0.0076 |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 9.0  | .NET 9.0  |  3,224.274 ns |  64.1235 ns |   147.3343 ns |  3,152.154 ns |  0.64 |    0.04 | 0.0038 |      - |      72 B |        0.45 |

<!-- end-snippet -->

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
<!-- begin-snippet: Akade.IndexedSet.Benchmarks PrefixIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                     | Job       | Runtime   | Mean        | Error       | StdDev      | Median      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------- |---------- |------------:|------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| FuzzyStartsWith_Linq       | .NET 10.0 | .NET 10.0 | 62,237.0 ns | 1,239.18 ns | 2,896.55 ns | 61,720.4 ns |  1.00 |    0.06 | 2.3804 |      - |   40088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 10.0 | .NET 10.0 | 24,341.0 ns |   484.00 ns | 1,214.25 ns | 23,822.7 ns |  0.39 |    0.03 |      - |      - |     240 B |       0.006 |
| FuzzyStartsWith_Autypo     | .NET 10.0 | .NET 10.0 | 71,487.6 ns | 1,480.23 ns | 4,364.49 ns | 69,695.2 ns |  1.15 |    0.09 | 0.3662 |      - |    6656 B |       0.166 |
|                            |           |           |             |             |             |             |       |         |        |        |           |             |
| FuzzyStartsWith_Linq       | .NET 9.0  | .NET 9.0  | 66,353.2 ns | 1,285.98 ns | 1,263.01 ns | 66,357.9 ns |  1.00 |    0.03 | 5.7373 |      - |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 9.0  | .NET 9.0  | 24,245.6 ns |   481.63 ns | 1,225.90 ns | 24,021.4 ns |  0.37 |    0.02 |      - |      - |     240 B |       0.002 |
| FuzzyStartsWith_Autypo     | .NET 9.0  | .NET 9.0  | 75,983.9 ns | 1,517.40 ns | 3,693.57 ns | 74,524.4 ns |  1.15 |    0.06 | 0.3662 |      - |    6688 B |       0.070 |
|                            |           |           |             |             |             |             |       |         |        |        |           |             |
| StartsWith_Linq            | .NET 10.0 | .NET 10.0 |  2,412.0 ns |    76.82 ns |   225.31 ns |  2,396.5 ns |  1.01 |    0.13 | 0.0076 |      - |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 10.0 | .NET 10.0 |    361.8 ns |     4.59 ns |     8.28 ns |    358.3 ns |  0.15 |    0.01 | 0.0086 |      - |     144 B |        1.12 |
| StartsWith_Autypo          | .NET 10.0 | .NET 10.0 |  2,135.0 ns |    23.38 ns |    21.87 ns |  2,137.2 ns |  0.89 |    0.08 | 0.3967 |      - |    6664 B |       52.06 |
|                            |           |           |             |             |             |             |       |         |        |        |           |             |
| StartsWith_Linq            | .NET 9.0  | .NET 9.0  |  1,284.3 ns |     5.16 ns |     4.82 ns |  1,285.4 ns |  1.00 |    0.01 | 0.0076 |      - |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 9.0  | .NET 9.0  |    401.3 ns |     0.97 ns |     0.91 ns |    400.9 ns |  0.31 |    0.00 | 0.0086 |      - |     144 B |        1.12 |
| StartsWith_Autypo          | .NET 9.0  | .NET 9.0  |  2,166.4 ns |    24.01 ns |    22.46 ns |  2,162.1 ns |  1.69 |    0.02 | 0.3967 | 0.0038 |    6696 B |       52.31 |

<!-- end-snippet -->

## FullText-Index

<!-- begin-snippet: Akade.IndexedSet.Benchmarks FullTextIndexBenchmarks(importer:benchmarkdotnet) -->
| Method                   | Job       | Runtime   | Mean           | Error         | StdDev        | Median         | Ratio | RatioSD | Gen0     | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |---------------:|--------------:|--------------:|---------------:|------:|--------:|---------:|----------:|------------:|
| Contains_Linq            | .NET 10.0 | .NET 10.0 |     5,838.9 ns |      13.81 ns |      11.53 ns |     5,839.7 ns |  1.00 |    0.00 |   0.0076 |     176 B |        1.00 |
| Contains_IndexedSet      | .NET 10.0 | .NET 10.0 |       320.4 ns |       0.99 ns |       0.83 ns |       320.4 ns |  0.05 |    0.00 |   0.0238 |     400 B |        2.27 |
|                          |           |           |                |               |               |                |       |         |          |           |             |
| Contains_Linq            | .NET 9.0  | .NET 9.0  |     5,554.7 ns |     110.00 ns |     306.63 ns |     5,413.8 ns |  1.00 |    0.08 |   0.0076 |     176 B |        1.00 |
| Contains_IndexedSet      | .NET 9.0  | .NET 9.0  |       371.0 ns |       7.44 ns |      12.63 ns |       367.1 ns |  0.07 |    0.00 |   0.0238 |     400 B |        2.27 |
|                          |           |           |                |               |               |                |       |         |          |           |             |
| FuzzyContains_Linq       | .NET 10.0 | .NET 10.0 | 7,426,216.0 ns | 149,406.59 ns | 438,183.70 ns | 7,290,326.6 ns |  1.00 |    0.08 | 281.2500 | 4831736 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 10.0 | .NET 10.0 |   215,252.5 ns |   4,270.85 ns |   8,914.85 ns |   212,033.3 ns |  0.03 |    0.00 |        - |     608 B |       0.000 |
|                          |           |           |                |               |               |                |       |         |          |           |             |
| FuzzyContains_Linq       | .NET 9.0  | .NET 9.0  | 7,401,515.6 ns | 145,233.19 ns | 254,364.24 ns | 7,416,882.8 ns |  1.00 |    0.05 | 281.2500 | 4831736 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 9.0  | .NET 9.0  |   221,219.2 ns |   4,371.49 ns |   9,027.88 ns |   216,925.3 ns |  0.03 |    0.00 |        - |     608 B |       0.000 |

<!-- end-snippet -->

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

<!-- begin-snippet: Akade.IndexedSet.Benchmarks ConcurrentSetBenchmarks(importer:benchmarkdotnet) -->
| Method                   | Job       | Runtime   | Mean          | Error       | StdDev        | Median        | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |--------------:|------------:|--------------:|--------------:|------:|--------:|----------:|------------:|
| FullTextLookup           | .NET 10.0 | .NET 10.0 | 26,927.069 ns | 531.1862 ns | 1,209.7799 ns | 26,783.429 ns |  1.00 |    0.06 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 10.0 | .NET 10.0 | 27,261.862 ns | 796.1398 ns | 2,309.7462 ns | 26,265.707 ns |  1.01 |    0.10 |     464 B |        1.09 |
|                          |           |           |               |             |               |               |       |         |           |             |
| FullTextLookup           | .NET 9.0  | .NET 9.0  | 26,550.452 ns | 525.4506 ns | 1,186.0296 ns | 25,940.977 ns |  1.00 |    0.06 |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 9.0  | .NET 9.0  | 26,556.150 ns | 516.2834 ns | 1,206.7959 ns | 26,321.509 ns |  1.00 |    0.06 |     464 B |        1.09 |
|                          |           |           |               |             |               |               |       |         |           |             |
| LessThanLookup           | .NET 10.0 | .NET 10.0 |      9.895 ns |   0.2172 ns |     0.4185 ns |      9.726 ns |  1.00 |    0.06 |         - |          NA |
| ConcurrentLessThanLookup | .NET 10.0 | .NET 10.0 |     23.018 ns |   0.4820 ns |     0.8934 ns |     22.627 ns |  2.33 |    0.13 |         - |          NA |
|                          |           |           |               |             |               |               |       |         |           |             |
| LessThanLookup           | .NET 9.0  | .NET 9.0  |     12.156 ns |   0.2669 ns |     0.5206 ns |     12.066 ns |  1.00 |    0.06 |         - |          NA |
| ConcurrentLessThanLookup | .NET 9.0  | .NET 9.0  |     60.554 ns |   1.2057 ns |     1.1841 ns |     60.056 ns |  4.99 |    0.23 |         - |          NA |
|                          |           |           |               |             |               |               |       |         |           |             |
| UniqueLookup             | .NET 10.0 | .NET 10.0 |      6.976 ns |   0.1641 ns |     0.4709 ns |      6.932 ns |  1.00 |    0.09 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 10.0 | .NET 10.0 |     15.597 ns |   0.0534 ns |     0.0499 ns |     15.581 ns |  2.25 |    0.14 |         - |          NA |
|                          |           |           |               |             |               |               |       |         |           |             |
| UniqueLookup             | .NET 9.0  | .NET 9.0  |      9.318 ns |   0.0222 ns |     0.0197 ns |      9.315 ns |  1.00 |    0.00 |         - |          NA |
| ConcurrentUniqueLookup   | .NET 9.0  | .NET 9.0  |     24.100 ns |   0.2788 ns |     0.2328 ns |     24.143 ns |  2.59 |    0.02 |         - |          NA |

<!-- end-snippet -->

> ℹ️ For more complex scenarios, the synchronization cost is negligible. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
