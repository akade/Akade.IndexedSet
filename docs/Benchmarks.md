# Benchmarks
Benchmarks measured on 27.07.2022:

## Environment
``` ini
BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.674)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.402
  [Host]     : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX2
```

All benchmarks are currenlty using *1k elements*. Benchmarks showing the scaling is on the roadmap - expect IndexedSet to scale much
much better than the naive LINQ Queries.

## Unique-Index
|                       Method |         Mean |     Error |    StdDev | Ratio | Code Size |   Gen0 | Allocated | Alloc Ratio |
|----------------------------- |-------------:|----------:|----------:|------:|----------:|-------:|----------:|------------:|
|                  Unqiue_Linq | 165,265.0 ns | 311.89 ns | 243.51 ns | 1.000 |     753 B | 0.7324 |   12800 B |        1.00 |
|            Unique_Dictionary |     473.9 ns |   0.44 ns |   0.39 ns | 0.003 |     681 B |      - |         - |        0.00 |
| Unique_IndexedSet_PrimaryKey |   1,687.4 ns |   2.21 ns |   2.06 ns | 0.010 |     512 B |      - |         - |        0.00 |
|     Unique_IndexedSet_Single |   1,561.7 ns |   4.48 ns |   4.19 ns | 0.009 |   1,224 B |      - |         - |        0.00 |

> ℹ️ Note that manually maintaining a dictionary is *currently* faster but only if the executing class has direct access
> to the dictionary. Ideas to bring it on par are being explored for scenarios with very tight loops around dictionary lookup.

## MultiValue-Index
|                Method |     Mean |    Error |   StdDev | Ratio | Code Size |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |---------:|---------:|---------:|------:|----------:|-------:|----------:|------------:|
|       MultiValue_Linq | 58.50 μs | 0.237 μs | 0.222 μs |  1.00 |   2,575 B | 0.0610 |    1680 B |        1.00 |
|     Multivalue_Lookup | 12.04 μs | 0.032 μs | 0.028 μs |  0.21 |   1,768 B | 0.0153 |     360 B |        0.21 |
| Multivalue_IndexedSet | 13.10 μs | 0.139 μs | 0.116 μs |  0.22 |   2,729 B | 0.0153 |     360 B |        0.21 |

> ℹ️ Solution is on par with manually using LINQ's lookup (i.e. `.ToLookup()`)

## Range-Index
|              Method |          Mean |         Error |        StdDev | Ratio |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|-------------------- |--------------:|--------------:|--------------:|------:|-------:|-------:|----------:|------------:|
|          Range_Linq | 23,038.455 ns |   459.8874 ns |   597.9835 ns |  1.00 |      - |      - |     168 B |        1.00 |
|    Range_IndexedSet |  2,663.595 ns |     1.5612 ns |     1.3840 ns |  0.11 | 0.0038 |      - |      72 B |        0.43 |
|                     |               |               |               |       |        |        |           |             |
|         Paging_Linq | 72,168.508 ns |   229.3990 ns |   214.5800 ns | 1.000 | 9.5215 | 2.3193 |  160352 B |       1.000 |
|   Paging_IndexedSet |    182.269 ns |     0.5060 ns |     0.4733 ns | 0.003 | 0.0277 |      - |     464 B |       0.003 |
|                     |               |               |               |       |        |        |           |             |
|            Min_Linq | 81,774.092 ns | 1,613.7419 ns | 1,430.5402 ns | 1.000 |      - |      - |      40 B |        1.00 |
|      Min_IndexedSet |      9.948 ns |     0.0121 ns |     0.0114 ns | 0.000 |      - |      - |         - |        0.00 |
|                     |               |               |               |       |        |        |           |             |
|       LessThan_Linq | 16,925.834 ns |    60.9109 ns |    56.9761 ns |  1.00 |      - |      - |     160 B |        1.00 |
| LessThan_IndexedSet |  3,672.340 ns |     2.1130 ns |     1.8731 ns |  0.22 | 0.0038 |      - |      72 B |        0.45 |

> ℹ️ There is no built-in range data structure, hence no comparison. Paging covers sorting scenarios, while min-queries cover MinBy and MaxBy-Queries as well.

## Prefix-Index
|                     Method |         Mean |      Error |     StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |-------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
|            StartsWith_Linq |  8,660.27 ns | 151.443 ns | 141.660 ns | 1.000 |      - |      40 B |        1.00 |
|      StartsWith_IndexedSet |     77.29 ns |   0.092 ns |   0.086 ns | 0.009 |      - |         - |        0.00 |
|                            |              |            |            |       |        |           |             |
|       FuzzyStartsWith_Linq | 45,732.01 ns | 141.950 ns | 132.781 ns |  1.00 | 4.7607 |   80040 B |        1.00 |
| FuzzyStartsWith_IndexedSet | 13,112.31 ns |  81.449 ns |  76.188 ns |  0.29 | 1.3275 |   22304 B |        0.28 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)

## FullText-Index

|                   Method |           Mean |       Error |      StdDev | Ratio |     Gen0 | Allocated | Alloc Ratio |
|------------------------- |---------------:|------------:|------------:|------:|---------:|----------:|------------:|
|            Contains_Linq |    40,103.8 ns |   272.77 ns |   255.15 ns | 1.000 |        - |      40 B |        1.00 |
|      Contains_IndexedSet |       139.9 ns |     0.29 ns |     0.27 ns | 0.003 |   0.0114 |     192 B |        4.80 |
|                          |                |             |             |       |          |           |             |
|       FuzzyContains_Linq | 5,052,830.3 ns | 8,964.55 ns | 8,385.45 ns |  1.00 | 281.2500 | 4831711 B |        1.00 |
| FuzzyContains_IndexedSet |   133,142.1 ns |   433.97 ns |   405.94 ns |  0.03 |  15.3809 |  260624 B |        0.05 |

> ℹ️ Fuzzy-Matching of string pairs within the Linq-Benchmark is done with [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) and enumerating possible infixes.

## ConcurrentSet

|                   Method |         Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |-------------:|----------:|----------:|------:|-------:|----------:|------------:|
|             UniqueLookup |     15.03 ns |  0.024 ns |  0.022 ns |  1.00 |      - |         - |          NA |
|   ConcurrentUniqueLookup |     33.84 ns |  0.073 ns |  0.068 ns |  2.25 |      - |         - |          NA |
|                          |              |           |           |       |        |           |             |
|           LessThanLookup |     25.80 ns |  0.069 ns |  0.064 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| ConcurrentLessThanLookup |     52.88 ns |  0.086 ns |  0.080 ns |  2.05 | 0.0057 |      96 B |        1.50 |
|                          |              |           |           |       |        |           |             |
|             UniqueLookup |     15.03 ns |  0.024 ns |  0.022 ns |  1.00 |      - |         - |          NA |
|   ConcurrentUniqueLookup |     33.84 ns |  0.073 ns |  0.068 ns |  2.25 |      - |         - |          NA |

> ℹ️ For more complex scenarios, the synchronization cost is negligble. For simple queries, it is not.
The difference in allocated memory is due to materialization of results in the concurrent case.
