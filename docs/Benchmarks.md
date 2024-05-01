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
much better than the naive LINQ Queries. As .NET 8.0 brings in [a bunch of performance improvements](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/),
the benchmarks are run against both .NET 6 and 8.
.NET 7 is out of support and hence, no longer an explicit target (but the library can be consumed from .NET 7 without any issue).

## Unique-Index
| Method                       | Job      | Runtime  | Mean         | Error       | StdDev      | Ratio | Code Size | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |--------- |--------- |-------------:|------------:|------------:|------:|----------:|-------:|----------:|------------:|
| Unqiue_Linq                  | .NET 6.0 | .NET 6.0 | 166,577.5 ns |   401.58 ns |   375.63 ns | 1.000 |     753 B | 0.7324 |   12800 B |        1.00 |
| Unique_Dictionary            | .NET 6.0 | .NET 6.0 |     466.2 ns |     0.70 ns |     0.58 ns | 0.003 |     681 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 6.0 | .NET 6.0 |   1,722.4 ns |     2.05 ns |     1.91 ns | 0.010 |     512 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 6.0 | .NET 6.0 |   1,576.3 ns |     2.44 ns |     2.28 ns | 0.009 |   1,224 B |      - |         - |        0.00 |
|                              |          |          |              |             |             |       |           |        |           |             |
| Unqiue_Linq                  | .NET 8.0 | .NET 8.0 |  91,877.8 ns | 1,426.28 ns | 1,191.01 ns | 1.000 |   1,040 B | 0.7324 |   12800 B |        1.00 |
| Unique_Dictionary            | .NET 8.0 | .NET 8.0 |     358.5 ns |     0.75 ns |     0.63 ns | 0.004 |     514 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 8.0 | .NET 8.0 |     823.1 ns |     0.71 ns |     0.63 ns | 0.009 |   1,650 B |      - |         - |        0.00 |
| Unique_IndexedSet_Single     | .NET 8.0 | .NET 8.0 |     777.7 ns |     1.51 ns |     1.41 ns | 0.008 |   1,363 B |      - |         - |        0.00 |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
| Method                | Job      | Runtime  | Mean      | Error     | StdDev    | Median    | Ratio | Gen0   | Code Size | Allocated | Alloc Ratio |
|---------------------- |--------- |--------- |----------:|----------:|----------:|----------:|------:|-------:|----------:|----------:|------------:|
| MultiValue_Linq       | .NET 6.0 | .NET 6.0 | 62.644 μs | 0.7596 μs | 0.7105 μs | 62.814 μs |  1.00 |      - |   2,575 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 6.0 | .NET 6.0 | 12.502 μs | 0.0712 μs | 0.0595 μs | 12.510 μs |  0.20 | 0.0153 |   1,768 B |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 6.0 | .NET 6.0 | 13.423 μs | 0.1427 μs | 0.1335 μs | 13.456 μs |  0.21 | 0.0153 |   2,729 B |     360 B |        0.21 |
|                       |          |          |           |           |           |           |       |        |           |           |             |
| MultiValue_Linq       | .NET 8.0 | .NET 8.0 | 38.470 μs | 0.7642 μs | 1.4723 μs | 37.729 μs |  1.00 | 0.0610 |   3,847 B |    1680 B |        1.00 |
| Multivalue_Lookup     | .NET 8.0 | .NET 8.0 |  8.295 μs | 0.1240 μs | 0.0968 μs |  8.321 μs |  0.21 | 0.0153 |   2,864 B |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 8.0 | .NET 8.0 |  8.349 μs | 0.1221 μs | 0.1142 μs |  8.289 μs |  0.21 | 0.0153 |   3,639 B |     360 B |        0.21 |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
| Method              | Job      | Runtime  | Mean          | Error         | StdDev        | Median        | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------- |--------- |--------- |--------------:|--------------:|--------------:|--------------:|------:|-------:|-------:|----------:|------------:|
| LessThan_Linq       | .NET 6.0 | .NET 6.0 | 17,043.924 ns |    50.4800 ns |    44.7492 ns | 17,055.573 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 6.0 | .NET 6.0 |  3,665.244 ns |     9.8124 ns |     7.6609 ns |  3,665.690 ns |  0.21 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |               |               |               |       |        |        |           |             |
| LessThan_Linq       | .NET 8.0 | .NET 8.0 | 11,979.139 ns |    64.8170 ns |    57.4586 ns | 11,976.392 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 8.0 | .NET 8.0 |  2,932.307 ns |     7.9799 ns |     6.6635 ns |  2,929.376 ns |  0.24 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |          |               |               |               |               |       |        |        |           |             |
| Min_Linq            | .NET 6.0 | .NET 6.0 | 77,685.195 ns | 1,543.5157 ns | 1,515.9388 ns | 77,807.800 ns | 1.000 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 6.0 | .NET 6.0 |     10.182 ns |     0.1023 ns |     0.0957 ns |     10.179 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |          |               |               |               |               |       |        |        |           |             |
| Min_Linq            | .NET 8.0 | .NET 8.0 | 38,149.004 ns |   761.1209 ns |   845.9839 ns | 38,267.505 ns | 1.000 |      - |      - |      40 B |        1.00 |
| Min_IndexedSet      | .NET 8.0 | .NET 8.0 |      5.042 ns |     0.0304 ns |     0.0285 ns |      5.040 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |          |               |               |               |               |       |        |        |           |             |
| Paging_Linq         | .NET 6.0 | .NET 6.0 | 74,859.271 ns | 1,454.0230 ns | 1,360.0940 ns | 75,383.374 ns | 1.000 | 9.5215 | 2.3193 |  160352 B |       1.000 |
| Paging_IndexedSet   | .NET 6.0 | .NET 6.0 |    184.119 ns |     3.5691 ns |     3.6652 ns |    181.758 ns | 0.002 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |          |               |               |               |               |       |        |        |           |             |
| Paging_Linq         | .NET 8.0 | .NET 8.0 | 45,858.666 ns |   907.2320 ns | 1,211.1286 ns | 44,941.455 ns | 1.000 | 9.5215 | 2.3804 |  160352 B |       1.000 |
| Paging_IndexedSet   | .NET 8.0 | .NET 8.0 |    141.107 ns |     2.0993 ns |     1.7530 ns |    140.655 ns | 0.003 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |          |               |               |               |               |       |        |        |           |             |
| Range_Linq          | .NET 6.0 | .NET 6.0 | 21,044.604 ns |   446.4377 ns | 1,316.3317 ns | 21,423.367 ns |  1.00 |      - |      - |     168 B |        1.00 |
| Range_IndexedSet    | .NET 6.0 | .NET 6.0 |  3,174.231 ns |    18.6308 ns |    14.5457 ns |  3,170.776 ns |  0.15 | 0.0038 |      - |      72 B |        0.43 |
|                     |          |          |               |               |               |               |       |        |        |           |             |
| Range_Linq          | .NET 8.0 | .NET 8.0 | 13,434.074 ns |    48.3800 ns |    45.2547 ns | 13,429.517 ns |  1.00 |      - |      - |     160 B |        1.00 |
| Range_IndexedSet    | .NET 8.0 | .NET 8.0 |  2,278.306 ns |     2.3033 ns |     2.0418 ns |  2,277.767 ns |  0.17 | 0.0038 |      - |      72 B |        0.45 |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
| Method                     | Job      | Runtime  | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |--------- |--------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| FuzzyStartsWith_Linq       | .NET 6.0 | .NET 6.0 | 64,420.4 ns | 985.66 ns | 921.99 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 6.0 | .NET 6.0 | 21,313.8 ns | 166.94 ns | 139.40 ns |  0.33 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |        |           |             |
| FuzzyStartsWith_Linq       | .NET 8.0 | .NET 8.0 | 64,301.0 ns | 994.86 ns | 881.92 ns |  1.00 | 5.7373 |   96088 B |       1.000 |
| FuzzyStartsWith_IndexedSet | .NET 8.0 | .NET 8.0 | 15,486.3 ns |  74.07 ns |  65.66 ns |  0.24 |      - |     240 B |       0.002 |
|                            |          |          |             |           |           |       |        |           |             |
| StartsWith_Linq            | .NET 6.0 | .NET 6.0 |  3,861.6 ns |  15.43 ns |  12.88 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 6.0 | .NET 6.0 |    602.1 ns |   1.38 ns |   1.22 ns |  0.16 | 0.0086 |     144 B |        1.12 |
|                            |          |          |             |           |           |       |        |           |             |
| StartsWith_Linq            | .NET 8.0 | .NET 8.0 |  2,188.6 ns |   5.78 ns |   5.12 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| StartsWith_IndexedSet      | .NET 8.0 | .NET 8.0 |    613.7 ns |   2.47 ns |   2.31 ns |  0.28 | 0.0086 |     144 B |        1.12 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

| Method                   | Job      | Runtime  | Mean           | Error        | StdDev       | Ratio | Gen0     | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |---------------:|-------------:|-------------:|------:|---------:|----------:|------------:|
| Contains_Linq            | .NET 6.0 | .NET 6.0 |    16,443.7 ns |    101.95 ns |     90.37 ns |  1.00 |        - |     408 B |        1.00 |
| Contains_IndexedSet      | .NET 6.0 | .NET 6.0 |       484.1 ns |      5.68 ns |      5.31 ns |  0.03 |   0.0238 |     400 B |        0.98 |
|                          |          |          |                |              |              |       |          |           |             |
| Contains_Linq            | .NET 8.0 | .NET 8.0 |     5,714.5 ns |     21.24 ns |     18.83 ns |  1.00 |   0.0229 |     408 B |        1.00 |
| Contains_IndexedSet      | .NET 8.0 | .NET 8.0 |       493.7 ns |      4.41 ns |      4.12 ns |  0.09 |   0.0238 |     400 B |        0.98 |
|                          |          |          |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 6.0 | .NET 6.0 | 7,116,201.8 ns | 53,574.33 ns | 50,113.46 ns |  1.00 | 281.2500 | 4831744 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 6.0 | .NET 6.0 |   172,160.2 ns |    424.16 ns |    354.20 ns |  0.02 |        - |     608 B |       0.000 |
|                          |          |          |                |              |              |       |          |           |             |
| FuzzyContains_Linq       | .NET 8.0 | .NET 8.0 | 6,082,955.1 ns | 10,276.95 ns |  9,613.07 ns |  1.00 | 281.2500 | 4831744 B |       1.000 |
| FuzzyContains_IndexedSet | .NET 8.0 | .NET 8.0 |   137,149.8 ns |    997.75 ns |    933.30 ns |  0.02 |        - |     608 B |       0.000 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

| Method                   | Job      | Runtime  | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |--------------:|------------:|------------:|------:|--------:|-------:|----------:|------------:|
| FullTextLookup           | .NET 6.0 | .NET 6.0 | 24,811.507 ns | 103.8346 ns |  81.0672 ns |  1.00 |    0.00 |      - |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 6.0 | .NET 6.0 | 24,738.256 ns |  98.5952 ns |  82.3314 ns |  1.00 |    0.01 |      - |     464 B |        1.09 |
|                          |          |          |               |             |             |       |         |        |           |             |
| FullTextLookup           | .NET 8.0 | .NET 8.0 | 17,805.678 ns |  58.7119 ns |  45.8384 ns |  1.00 |    0.00 |      - |     424 B |        1.00 |
| ConcurrentFullTextLookup | .NET 8.0 | .NET 8.0 | 17,847.506 ns |  55.7845 ns |  49.4515 ns |  1.00 |    0.00 |      - |     464 B |        1.09 |
|                          |          |          |               |             |             |       |         |        |           |             |
| LessThanLookup           | .NET 6.0 | .NET 6.0 |     25.600 ns |   0.5139 ns |   0.4807 ns |  1.00 |    0.00 | 0.0038 |      64 B |        1.00 |
| ConcurrentLessThanLookup | .NET 6.0 | .NET 6.0 |     58.042 ns |   1.1181 ns |   1.0459 ns |  2.27 |    0.05 | 0.0038 |      64 B |        1.00 |
|                          |          |          |               |             |             |       |         |        |           |             |
| LessThanLookup           | .NET 8.0 | .NET 8.0 |      9.387 ns |   0.0318 ns |   0.0282 ns |  1.00 |    0.00 |      - |         - |          NA |
| ConcurrentLessThanLookup | .NET 8.0 | .NET 8.0 |     49.537 ns |   1.0059 ns |   1.7617 ns |  5.38 |    0.12 |      - |         - |          NA |
|                          |          |          |               |             |             |       |         |        |           |             |
| UniqueLookup             | .NET 6.0 | .NET 6.0 |     15.309 ns |   0.0549 ns |   0.0459 ns |  1.00 |    0.00 |      - |         - |          NA |
| ConcurrentUniqueLookup   | .NET 6.0 | .NET 6.0 |     33.912 ns |   0.1571 ns |   0.1393 ns |  2.22 |    0.01 |      - |         - |          NA |
|                          |          |          |               |             |             |       |         |        |           |             |
| UniqueLookup             | .NET 8.0 | .NET 8.0 |      9.572 ns |   0.0428 ns |   0.0379 ns |  1.00 |    0.00 |      - |         - |          NA |
| ConcurrentUniqueLookup   | .NET 8.0 | .NET 8.0 |     34.708 ns |   0.7367 ns |   2.1721 ns |  3.33 |    0.20 |      - |         - |          NA |

> ℹ️ For more complex scenarios, the synchronization cost is negligible. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
