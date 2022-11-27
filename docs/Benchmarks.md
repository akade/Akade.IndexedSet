# Benchmarks
Benchmarks measured on 27.07.2022:

## Environment
``` ini
BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.755)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.100
  [Host]   : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.11 (6.0.1122.52304), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
```

All benchmarks are currenlty using *1k elements*. Benchmarks showing the scaling is on the roadmap - expect IndexedSet to scale much
much better than the naive LINQ Queries. As .NET 7.0 brings in [a bunch of performance improvements](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/),
the benchmarks are run against both .NET 6 and 7.

## Unique-Index
|                       Method |  Runtime |         Mean |       Error |      StdDev | Ratio |   Gen0 | Code Size | Allocated | Alloc Ratio |
|----------------------------- |--------- |-------------:|------------:|------------:|------:|-------:|----------:|----------:|------------:|
|                  Unqiue_Linq | .NET 6.0 | 156,714.6 ns |   341.01 ns |   284.76 ns | 1.000 | 0.7324 |     753 B |   12800 B |        1.00 |
|            Unique_Dictionary | .NET 6.0 |     473.6 ns |     1.47 ns |     1.38 ns | 0.003 |      - |     681 B |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 6.0 |   1,692.7 ns |     6.08 ns |     5.69 ns | 0.011 |      - |     512 B |         - |        0.00 |
|     Unique_IndexedSet_Single | .NET 6.0 |   1,538.3 ns |    10.43 ns |     9.24 ns | 0.010 |      - |   1,224 B |         - |        0.00 |
|                              |          |              |             |             |       |        |           |           |             |
|                  Unqiue_Linq | .NET 7.0 | 158,745.0 ns | 2,950.18 ns | 2,615.26 ns | 1.000 | 0.7324 |     696 B |   12800 B |        1.00 |
|            Unique_Dictionary | .NET 7.0 |     397.4 ns |     0.64 ns |     0.60 ns | 0.003 |      - |     590 B |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey | .NET 7.0 |   1,379.7 ns |     3.11 ns |     2.59 ns | 0.009 |      - |   1,084 B |         - |        0.00 |
|     Unique_IndexedSet_Single | .NET 7.0 |   1,393.3 ns |     2.46 ns |     2.18 ns | 0.009 |      - |   1,145 B |         - |        0.00 |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
|                Method |  Runtime |     Mean |    Error |   StdDev | Ratio | Code Size |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |--------- |---------:|---------:|---------:|------:|----------:|-------:|----------:|------------:|
|       MultiValue_Linq | .NET 6.0 | 64.40 μs | 0.476 μs | 0.445 μs |  1.00 |   2,575 B |      - |    1680 B |        1.00 |
|     Multivalue_Lookup | .NET 6.0 | 12.53 μs | 0.151 μs | 0.141 μs |  0.19 |   1,768 B | 0.0153 |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 6.0 | 12.78 μs | 0.068 μs | 0.063 μs |  0.20 |   2,729 B | 0.0153 |     360 B |        0.21 |
|                       |          |          |          |          |       |           |        |           |             |
|       MultiValue_Linq | .NET 7.0 | 56.89 μs | 1.024 μs | 0.799 μs |  1.00 |   2,569 B | 0.0610 |    1680 B |        1.00 |
|     Multivalue_Lookup | .NET 7.0 | 11.71 μs | 0.023 μs | 0.022 μs |  0.21 |   1,746 B | 0.0153 |     360 B |        0.21 |
| Multivalue_IndexedSet | .NET 7.0 | 12.86 μs | 0.106 μs | 0.088 μs |  0.23 |   2,638 B | 0.0153 |     360 B |        0.21 |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
|              Method |  Runtime |         Mean |        Error |       StdDev |       Median | Ratio |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|-------------------- |--------- |-------------:|-------------:|-------------:|-------------:|------:|-------:|-------:|----------:|------------:|
|       LessThan_Linq | .NET 6.0 | 16,878.19 ns |    65.965 ns |    58.477 ns | 16,887.68 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 6.0 |  3,637.28 ns |     3.884 ns |     3.243 ns |  3,638.40 ns |  0.22 | 0.0038 |      - |      72 B |        0.45 |
|                     |          |              |              |              |              |       |        |        |           |             |
|       LessThan_Linq | .NET 7.0 | 18,117.09 ns |    67.318 ns |    62.970 ns | 18,120.34 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet | .NET 7.0 |  3,858.55 ns |     3.655 ns |     3.240 ns |  3,858.11 ns |  0.21 |      - |      - |      72 B |        0.45 |
|                     |          |              |              |              |              |       |        |        |           |             |
|            Min_Linq | .NET 6.0 | 79,119.22 ns | 1,245.002 ns | 1,164.576 ns | 79,051.43 ns | 1.000 |      - |      - |      40 B |        1.00 |
|      Min_IndexedSet | .NET 6.0 |     10.28 ns |     0.025 ns |     0.024 ns |     10.28 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |              |              |              |              |       |        |        |           |             |
|            Min_Linq | .NET 7.0 | 67,645.12 ns |   755.328 ns |   589.710 ns | 67,805.86 ns | 1.000 |      - |      - |      40 B |        1.00 |
|      Min_IndexedSet | .NET 7.0 |     10.05 ns |     0.014 ns |     0.012 ns |     10.05 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |          |              |              |              |              |       |        |        |           |             |
|         Paging_Linq | .NET 6.0 | 74,292.36 ns | 1,203.178 ns | 1,125.454 ns | 74,021.94 ns | 1.000 | 9.5215 | 2.3193 |  160352 B |       1.000 |
|   Paging_IndexedSet | .NET 6.0 |    183.20 ns |     0.814 ns |     0.721 ns |    183.05 ns | 0.002 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |              |              |              |              |       |        |        |           |             |
|         Paging_Linq | .NET 7.0 | 74,679.34 ns | 1,492.779 ns | 1,941.034 ns | 75,474.67 ns | 1.000 | 9.5215 | 2.3193 |  160352 B |       1.000 |
|   Paging_IndexedSet | .NET 7.0 |    174.12 ns |     0.902 ns |     0.753 ns |    174.16 ns | 0.002 | 0.0277 |      - |     464 B |       0.003 |
|                     |          |              |              |              |              |       |        |        |           |             |
|          Range_Linq | .NET 6.0 | 20,597.43 ns |   398.112 ns |   473.924 ns | 20,733.38 ns |  1.00 |      - |      - |     168 B |        1.00 |
|    Range_IndexedSet | .NET 6.0 |  2,829.46 ns |     2.671 ns |     2.498 ns |  2,829.46 ns |  0.14 | 0.0038 |      - |      72 B |        0.43 |
|                     |          |              |              |              |              |       |        |        |           |             |
|          Range_Linq | .NET 7.0 | 26,647.35 ns |   751.672 ns | 2,216.321 ns | 24,949.59 ns |  1.00 |      - |      - |     160 B |        1.00 |
|    Range_IndexedSet | .NET 7.0 |  3,006.22 ns |     7.875 ns |     7.366 ns |  3,009.67 ns |  0.11 | 0.0038 |      - |      72 B |        0.45 |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
|                     Method |  Runtime |         Mean |      Error |     StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |--------- |-------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
|       FuzzyStartsWith_Linq | .NET 6.0 | 48,835.23 ns | 750.969 ns | 702.457 ns |  1.00 | 4.7607 |   80040 B |        1.00 |
| FuzzyStartsWith_IndexedSet | .NET 6.0 | 13,862.70 ns | 210.747 ns | 197.133 ns |  0.28 | 1.3275 |   22304 B |        0.28 |
|                            |          |              |            |            |       |        |           |             |
|       FuzzyStartsWith_Linq | .NET 7.0 | 43,357.99 ns | 516.779 ns | 483.395 ns |  1.00 | 4.7607 |   80040 B |        1.00 |
| FuzzyStartsWith_IndexedSet | .NET 7.0 | 12,734.14 ns | 161.187 ns | 150.775 ns |  0.29 | 1.2207 |   20608 B |        0.26 |
|                            |          |              |            |            |       |        |           |             |
|            StartsWith_Linq | .NET 6.0 |  8,723.26 ns |  60.368 ns |  50.410 ns | 1.000 |      - |      40 B |        1.00 |
|      StartsWith_IndexedSet | .NET 6.0 |     76.42 ns |   0.237 ns |   0.210 ns | 0.009 |      - |         - |        0.00 |
|                            |          |              |            |            |       |        |           |             |
|            StartsWith_Linq | .NET 7.0 |  7,248.18 ns |  54.945 ns |  42.898 ns |  1.00 |      - |      40 B |        1.00 |
|      StartsWith_IndexedSet | .NET 7.0 |     78.57 ns |   0.181 ns |   0.170 ns |  0.01 |      - |         - |        0.00 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

|                   Method |  Runtime |           Mean |        Error |       StdDev | Ratio |     Gen0 | Allocated | Alloc Ratio |
|------------------------- |--------- |---------------:|-------------:|-------------:|------:|---------:|----------:|------------:|
|            Contains_Linq | .NET 6.0 |    40,473.6 ns |    366.10 ns |    342.45 ns | 1.000 |        - |      40 B |        1.00 |
|      Contains_IndexedSet | .NET 6.0 |       131.8 ns |      0.54 ns |      0.51 ns | 0.003 |   0.0114 |     192 B |        4.80 |
|                          |          |                |              |              |       |          |           |             |
|            Contains_Linq | .NET 7.0 |    12,074.5 ns |     85.10 ns |     75.44 ns |  1.00 |        - |      40 B |        1.00 |
|      Contains_IndexedSet | .NET 7.0 |       136.5 ns |      0.54 ns |      0.51 ns |  0.01 |   0.0114 |     192 B |        4.80 |
|                          |          |                |              |              |       |          |           |             |
|       FuzzyContains_Linq | .NET 6.0 | 4,654,014.7 ns | 15,103.29 ns | 14,127.63 ns |  1.00 | 281.2500 | 4831711 B |        1.00 |
| FuzzyContains_IndexedSet | .NET 6.0 |   139,105.9 ns |    469.74 ns |    439.40 ns |  0.03 |  15.3809 |  260624 B |        0.05 |
|                          |          |                |              |              |       |          |           |             |
|       FuzzyContains_Linq | .NET 7.0 | 4,927,249.0 ns |  8,211.41 ns |  7,279.20 ns |  1.00 | 281.2500 | 4831710 B |        1.00 |
| FuzzyContains_IndexedSet | .NET 7.0 |   122,926.1 ns |    905.08 ns |    802.33 ns |  0.02 |  14.6484 |  247200 B |        0.05 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

|                   Method |      Job |  Runtime |         Mean |      Error |     StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |--------- |--------- |-------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
|           FullTextLookup | .NET 6.0 | .NET 6.0 | 10,495.89 ns |  53.539 ns |  50.081 ns |  1.00 | 1.1444 |   19288 B |        1.00 |
| ConcurrentFullTextLookup | .NET 6.0 | .NET 6.0 | 10,573.92 ns |  61.314 ns |  57.353 ns |  1.01 | 1.1444 |   19360 B |        1.00 |
|                          |          |          |              |            |            |       |        |           |             |
|           FullTextLookup | .NET 7.0 | .NET 7.0 | 10,387.22 ns | 112.930 ns | 105.634 ns |  1.00 | 1.0986 |   18520 B |        1.00 |
| ConcurrentFullTextLookup | .NET 7.0 | .NET 7.0 | 10,791.14 ns |  54.652 ns |  48.448 ns |  1.04 | 1.0986 |   18592 B |        1.00 |
|                          |          |          |              |            |            |       |        |           |             |
|           LessThanLookup | .NET 6.0 | .NET 6.0 |     24.40 ns |   0.065 ns |   0.057 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| ConcurrentLessThanLookup | .NET 6.0 | .NET 6.0 |     53.61 ns |   0.197 ns |   0.184 ns |  2.20 | 0.0057 |      96 B |        1.50 |
|                          |          |          |              |            |            |       |        |           |             |
|           LessThanLookup | .NET 7.0 | .NET 7.0 |     21.83 ns |   0.075 ns |   0.070 ns |  1.00 |      - |         - |          NA |
| ConcurrentLessThanLookup | .NET 7.0 | .NET 7.0 |     48.59 ns |   0.154 ns |   0.128 ns |  2.23 | 0.0019 |      32 B |          NA |
|                          |          |          |              |            |            |       |        |           |             |
|             UniqueLookup | .NET 6.0 | .NET 6.0 |     15.32 ns |   0.058 ns |   0.045 ns |  1.00 |      - |         - |          NA |
|   ConcurrentUniqueLookup | .NET 6.0 | .NET 6.0 |     33.45 ns |   0.173 ns |   0.145 ns |  2.18 |      - |         - |          NA |
|                          |          |          |              |            |            |       |        |           |             |
|             UniqueLookup | .NET 7.0 | .NET 7.0 |     15.32 ns |   0.010 ns |   0.008 ns |  1.00 |      - |         - |          NA |
|   ConcurrentUniqueLookup | .NET 7.0 | .NET 7.0 |     36.11 ns |   0.044 ns |   0.039 ns |  2.36 |      - |         - |          NA |

> ℹ️ For more complex scenarios, the synchronization cost is negligble. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
