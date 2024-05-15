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
| Method                       | Job      | Runtime  | Mean         | Error     | StdDev    | Ratio | Code Size | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |--------- |--------- |-------------:|----------:|----------:|------:|----------:|-------:|----------:|------------:|
| Unqiue_Linq                  | .NET 6.0 | .NET 6.0 | 165,871.8 ns | 527.50 ns | 411.84 ns | 1.000 |     753 B | 0.7324 |   12800 B |        1.00 |
| Unique_Dictionary            | .NET 6.0 | .NET 6.0 |     474.1 ns |   0.33 ns |   0.31 ns | 0.003 |     681 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 6.0 | .NET 6.0 |   1,679.1 ns |   0.98 ns |   0.87 ns | 0.010 |     512 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 6.0 | .NET 6.0 |   1,576.2 ns |   2.36 ns |   1.97 ns | 0.010 |   1,224 B |      - |         - |        0.00 |
|                              |          |          |              |           |           |       |           |        |           |             |
| Unqiue_Linq                  | .NET 8.0 | .NET 8.0 | 101,937.5 ns | 582.10 ns | 516.02 ns | 1.000 |   1,040 B | 0.7324 |   12800 B |        1.00 |
| Unique_Dictionary            | .NET 8.0 | .NET 8.0 |     360.0 ns |   0.23 ns |   0.21 ns | 0.004 |     505 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 8.0 | .NET 8.0 |     779.1 ns |   1.04 ns |   0.93 ns | 0.008 |   1,659 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 8.0 | .NET 8.0 |     778.3 ns |   1.20 ns |   1.00 ns | 0.008 |   1,363 B |      - |         - |        0.00 |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
| Method                | Job      | Runtime  | Mean      | Error     | StdDev    | Ratio | Gen0   | Code Size | Allocated | Alloc Ratio |
|---------------------- |--------- |--------- |----------:|----------:|----------:|------:|-------:|----------:|----------:|------------:|
| MultiValue_Linq       | .NET 6.0 | .NET 6.0 | 62.683 μs | 0.3767 μs | 0.3524 μs |  1.00 |      - |   2,575 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 6.0 | .NET 6.0 | 12.298 μs | 0.1433 μs | 0.1340 μs |  0.20 | 0.0153 |   1,768 B |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 6.0 | .NET 6.0 | 13.071 μs | 0.0222 μs | 0.0197 μs |  0.21 | 0.0153 |   2,729 B |     360 B |        0.21 |
|                       |          |          |           |           |           |       |        |           |           |             |
| MultiValue_Linq       | .NET 8.0 | .NET 8.0 | 37.620 μs | 0.6172 μs | 0.8448 μs |  1.00 | 0.0610 |   3,848 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 8.0 | .NET 8.0 |  8.359 μs | 0.0546 μs | 0.0510 μs |  0.22 | 0.0153 |   2,869 B |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 8.0 | .NET 8.0 |  8.267 μs | 0.1633 μs | 0.1527 μs |  0.22 | 0.0153 |   3,624 B |     360 B |        0.21 |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
| Method              | Job      | Runtime  | Mean          | Error         | StdDev      | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------- |--------- |--------- |--------------:|--------------:|------------:|------:|-------:|-------:|----------:|------------:|
| LessThan_Linq       | .NET 6.0 | .NET 6.0 | 15,447.752 ns |    94.0371 ns |  73.4180 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 6.0 | .NET 6.0 |  4,572.376 ns |     7.1871 ns |   6.3712 ns |  0.30 |      - |      - |      72 B |        0.45 |
|                     |          |          |               |               |             |       |        |        |           |             |
| LessThan_Linq       | .NET 8.0 | .NET 8.0 | 12,302.704 ns |    39.4503 ns |  36.9018 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 8.0 | .NET 8.0 |  3,713.056 ns |     2.8611 ns |   2.3892 ns |  0.30 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |               |             |       |        |        |           |             |
| Min_Linq            | .NET 6.0 | .NET 6.0 | 76,513.335 ns | 1,015.6848 ns | 900.3781 ns | 1.000 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 6.0 | .NET 6.0 |     10.768 ns |     0.0468 ns |   0.0438 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |          |               |               |             |       |        |        |           |             |
| Min_Linq            | .NET 8.0 | .NET 8.0 | 36,296.705 ns |   721.4676 ns | 858.8558 ns | 1.000 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 8.0 | .NET 8.0 |      5.080 ns |     0.0302 ns |   0.0282 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |          |               |               |             |       |        |        |           |             |
| Paging_Linq         | .NET 6.0 | .NET 6.0 | 74,342.609 ns |   413.9514 ns | 387.2104 ns | 1.000 | 9.5215 | 2.3193 |  160352 B |       1.000 |
| Paging_IndexedSet   | .NET 6.0 | .NET 6.0 |    189.378 ns |     0.5182 ns |   0.4594 ns | 0.003 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |          |               |               |             |       |        |        |           |             |
| Paging_Linq         | .NET 8.0 | .NET 8.0 | 46,618.368 ns |   199.4397 ns | 176.7981 ns | 1.000 | 9.5215 | 2.3804 |  160352 B |       1.000 |
| Paging_IndexedSet   | .NET 8.0 | .NET 8.0 |    149.267 ns |     2.0515 ns |   1.9190 ns | 0.003 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |          |               |               |             |       |        |        |           |             |
| Range_Linq          | .NET 6.0 | .NET 6.0 | 22,935.722 ns |   392.1182 ns | 366.7876 ns |  1.00 |      - |      - |     168 B |        1.00 |
| Range_IndexedSet    | .NET 6.0 | .NET 6.0 |  3,608.993 ns |    18.2732 ns |  14.2665 ns |  0.16 | 0.0038 |      - |      72 B |        0.43 |
|                     |          |          |               |               |             |       |        |        |           |             |
| Range_Linq          | .NET 8.0 | .NET 8.0 | 14,482.250 ns |   283.2456 ns | 406.2223 ns |  1.00 |      - |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 8.0 | .NET 8.0 |  2,921.471 ns |     3.4446 ns |   2.8764 ns |  0.20 | 0.0038 |      - |      72 B |        0.45 |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
| Method                     | Job      | Runtime  | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |--------- |--------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| FuzzyStartsWith_Linq       | .NET 6.0 | .NET 6.0 | 66,355.7 ns | 275.48 ns | 230.04 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 6.0 | .NET 6.0 | 21,151.0 ns |  26.36 ns |  24.66 ns |  0.32 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |        |           |             |
| FuzzyStartsWith_Linq       | .NET 8.0 | .NET 8.0 | 59,134.2 ns | 216.75 ns | 192.15 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 8.0 | .NET 8.0 | 15,645.4 ns |  54.92 ns |  42.88 ns |  0.26 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |        |           |             |
| StartsWith_Linq            | .NET 6.0 | .NET 6.0 |  3,854.5 ns |  16.95 ns |  15.86 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 6.0 | .NET 6.0 |    699.8 ns |   0.40 ns |   0.33 ns |  0.18 | 0.0086 |     144 B |        1.12 |
|                            |          |          |             |           |           |       |        |           |             |
| StartsWith_Linq            | .NET 8.0 | .NET 8.0 |  2,175.6 ns |   6.48 ns |   6.06 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 8.0 | .NET 8.0 |    607.8 ns |   0.52 ns |   0.49 ns |  0.28 | 0.0086 |     144 B |        1.12 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

| Method                   | Job      | Runtime  | Mean           | Error        | StdDev       | Median         | Ratio | Gen0     | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |---------------:|-------------:|-------------:|---------------:|------:|---------:|----------:|------------:|
| Contains_Linq            | .NET 6.0 | .NET 6.0 |    16,860.5 ns |    104.58 ns |     97.82 ns |    16,824.8 ns |  1.00 |        - |     408 B |        1.00 |
| Contains_IndexedSet      | .NET 6.0 | .NET 6.0 |       494.3 ns |      9.82 ns |     17.95 ns |       481.1 ns |  0.03 |   0.0238 |     400 B |        0.98 |
|                          |          |          |                |              |              |                |       |          |           |             |
| Contains_Linq            | .NET 8.0 | .NET 8.0 |     6,414.3 ns |     13.07 ns |     11.58 ns |     6,412.4 ns |  1.00 |   0.0229 |     408 B |        1.00 |
| Contains_IndexedSet      | .NET 8.0 | .NET 8.0 |       493.9 ns |      0.66 ns |      0.56 ns |       493.8 ns |  0.08 |   0.0238 |     400 B |        0.98 |
|                          |          |          |                |              |              |                |       |          |           |             |
| FuzzyContains_Linq       | .NET 6.0 | .NET 6.0 | 7,093,172.0 ns | 18,687.32 ns | 16,565.82 ns | 7,092,050.8 ns |  1.00 | 281.2500 | 4831745 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 6.0 | .NET 6.0 |   189,163.9 ns |    696.50 ns |    651.50 ns |   189,138.3 ns |  0.03 |        - |     608 B |       0.000 |
|                          |          |          |                |              |              |                |       |          |           |             |
| FuzzyContains_Linq       | .NET 8.0 | .NET 8.0 | 6,418,291.2 ns | 74,491.47 ns | 62,203.75 ns | 6,391,822.7 ns |  1.00 | 281.2500 | 4831742 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 8.0 | .NET 8.0 |   138,812.8 ns |    394.75 ns |    349.94 ns |   138,727.1 ns |  0.02 |        - |     608 B |       0.000 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

| Method                   | Job      | Runtime  | Mean          | Error      | StdDev     | Median        | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |--------------:|-----------:|-----------:|--------------:|------:|--------:|-------:|----------:|------------:|
| FullTextLookup           | .NET 6.0 | .NET 6.0 | 25,215.813 ns | 96.5161 ns | 85.5590 ns | 25,203.891 ns |  1.00 |    0.00 |      - |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 6.0 | .NET 6.0 | 25,208.556 ns | 45.0981 ns | 39.9783 ns | 25,202.513 ns |  1.00 |    0.00 |      - |     464 B |        1.09 |
|                          |          |          |               |            |            |               |       |         |        |           |             |
| FullTextLookup           | .NET 8.0 | .NET 8.0 | 17,750.163 ns | 21.3387 ns | 17.8188 ns | 17,749.068 ns |  1.00 |    0.00 |      - |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 8.0 | .NET 8.0 | 17,844.096 ns | 49.3671 ns | 46.1780 ns | 17,832.971 ns |  1.01 |    0.00 |      - |     464 B |        1.09 |
|                          |          |          |               |            |            |               |       |         |        |           |             |
| LessThanLookup           | .NET 6.0 | .NET 6.0 |     25.078 ns |  0.1039 ns |  0.0971 ns |     25.041 ns |  1.00 |    0.00 | 0.0038 |      64 B |        1.00 |
| ConcurrentLessThanLookup | .NET 6.0 | .NET 6.0 |     52.316 ns |  0.1319 ns |  0.1169 ns |     52.307 ns |  2.09 |    0.01 | 0.0038 |      64 B |        1.00 |
|                          |          |          |               |            |            |               |       |         |        |           |             |
| LessThanLookup           | .NET 8.0 | .NET 8.0 |      9.451 ns |  0.0112 ns |  0.0099 ns |      9.450 ns |  1.00 |    0.00 |      - |         - |          NA |
| ConcurrentLessThanLookup | .NET 8.0 | .NET 8.0 |     45.381 ns |  0.9246 ns |  2.1613 ns |     44.943 ns |  4.88 |    0.30 |      - |         - |          NA |
|                          |          |          |               |            |            |               |       |         |        |           |             |
| UniqueLookup             | .NET 6.0 | .NET 6.0 |     15.515 ns |  0.0645 ns |  0.0604 ns |     15.523 ns |  1.00 |    0.00 |      - |         - |          NA |
| ConcurrentUniqueLookup   | .NET 6.0 | .NET 6.0 |     34.343 ns |  0.1085 ns |  0.0962 ns |     34.339 ns |  2.21 |    0.01 |      - |         - |          NA |
|                          |          |          |               |            |            |               |       |         |        |           |             |
| UniqueLookup             | .NET 8.0 | .NET 8.0 |      9.690 ns |  0.0387 ns |  0.0362 ns |      9.677 ns |  1.00 |    0.00 |      - |         - |          NA |
| ConcurrentUniqueLookup   | .NET 8.0 | .NET 8.0 |     33.247 ns |  0.8520 ns |  2.5122 ns |     33.908 ns |  3.03 |    0.24 |      - |         - |          NA |

> ℹ️ For more complex scenarios, the synchronization cost is negligible. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
