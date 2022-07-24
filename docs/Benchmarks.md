# Benchmarks
Benchmarks measured on 24.07.2022:

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
|                       Method |         Mean |       Error |      StdDev | Ratio | Code Size |  Gen 0 | Allocated |
|----------------------------- |-------------:|------------:|------------:|------:|----------:|-------:|----------:|
|                  Unqiue_Linq | 169,263.4 ns | 2,222.49 ns | 1,855.88 ns | 1.000 |     753 B | 0.7324 |  12,800 B |
|            Unique_Dictionary |     480.8 ns |     4.56 ns |     4.26 ns | 0.003 |     681 B |      - |         - |
| Unique_IndexedSet_PrimaryKey |   1,720.6 ns |     9.70 ns |     8.60 ns | 0.010 |     513 B |      - |         - |
|     Unique_IndexedSet_Single |   1,541.7 ns |     5.55 ns |     4.34 ns | 0.009 |   1,225 B |      - |         - |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only iff the executing class has direct access
> to the directory. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
|                Method |     Mean |    Error |   StdDev | Ratio | Code Size |  Gen 0 | Allocated |
|---------------------- |---------:|---------:|---------:|------:|----------:|-------:|----------:|
|       MultiValue_Linq | 59.90 μs | 0.467 μs | 0.437 μs |  1.00 |   2,575 B | 0.0610 |   1,680 B |
|     Multivalue_Lookup | 12.07 μs | 0.099 μs | 0.083 μs |  0.20 |   1,768 B | 0.0153 |     360 B |
| Multivalue_IndexedSet | 13.64 μs | 0.272 μs | 0.279 μs |  0.23 |   2,728 B | 0.0153 |     360 B |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup())

## Range-Index
|              Method |         Mean |        Error |       StdDev | Ratio |  Gen 0 |  Gen 1 | Allocated |
|-------------------- |-------------:|-------------:|-------------:|------:|-------:|-------:|----------:|
|          Range_Linq | 19,729.19 ns |   121.733 ns |   101.653 ns |  1.00 |      - |      - |     168 B |
|    Range_IndexedSet |  2,875.20 ns |     9.278 ns |     8.225 ns |  0.15 | 0.0038 |      - |      72 B |
|                     |              |              |              |       |        |        |           |
|         Paging_Linq | 75,402.10 ns | 1,471.280 ns | 1,376.237 ns | 1.000 | 9.5215 | 2.3193 | 160,352 B |
|   Paging_IndexedSet |    192.40 ns |     3.040 ns |     2.844 ns | 0.003 | 0.0277 |      - |     464 B |
|                     |              |              |              |       |        |        |           |
|            Min_Linq | 78,278.78 ns |   802.724 ns |   750.869 ns | 1.000 |      - |      - |      40 B |
|      Min_IndexedSet |     10.09 ns |     0.026 ns |     0.024 ns | 0.000 |      - |      - |         - |
|                     |              |              |              |       |        |        |           |
|       LessThan_Linq | 16,912.22 ns |    90.034 ns |    79.813 ns |  1.00 |      - |      - |     160 B |
| LessThan_IndexedSet |  3,939.25 ns |    21.145 ns |    17.657 ns |  0.23 |      - |      - |      72 B |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
|                     Method |         Mean |      Error |     StdDev | Ratio |  Gen 0 | Allocated |
|--------------------------- |-------------:|-----------:|-----------:|------:|-------:|----------:|
|            StartsWith_Linq |  8,518.25 ns | 101.522 ns |  89.996 ns | 1.000 |      - |      40 B |
|      StartsWith_IndexedSet |     81.77 ns |   0.508 ns |   0.476 ns | 0.010 |      - |         - |
|                            |              |            |            |       |        |           |
|       FuzzyStartsWith_Linq | 43,364.19 ns | 732.398 ns | 685.086 ns |  1.00 | 4.7607 |  80,040 B |
| FuzzyStartsWith_IndexedSet | 14,439.15 ns | 101.423 ns |  94.871 ns |  0.33 | 1.3275 |  22,304 B |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

|                   Method |           Mean |        Error |       StdDev | Ratio |    Gen 0 |   Allocated |
|------------------------- |---------------:|-------------:|-------------:|------:|---------:|------------:|
|            Contains_Linq |    38,791.0 ns |    361.18 ns |    337.85 ns | 1.000 |        - |        40 B |
|      Contains_IndexedSet |       150.8 ns |      0.54 ns |      0.51 ns | 0.004 |   0.0114 |       192 B |
|                          |                |              |              |       |          |             |
|       FuzzyContains_Linq | 5,127,638.6 ns | 46,980.92 ns | 43,945.99 ns |  1.00 | 281.2500 | 4,831,711 B |
| FuzzyContains_IndexedSet |   134,235.8 ns |  1,888.35 ns |  1,766.36 ns |  0.03 |  15.3809 |   260,624 B |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.