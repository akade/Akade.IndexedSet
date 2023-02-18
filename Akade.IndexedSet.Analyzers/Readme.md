# Supporting Analyzer

IndexedSet ships with analyzers that help to avoid common mistakes with index naming.

# Index Naming Conventions

> :information_source: IndexedSet uses `[CallerArgumentExpression]` to obtain the name of an index.
For example, `.WithIndex(x => x.ToLowerInvariant())` and `.WithIndex(y => y.ToLowerInvariant())` are functionally equivalent, but will result
in two different index names. This "literal naming" allows IndexedSet to be expression- and reflection-free, as well as allowing pretty much anything as a key
within an index. However, this is different to other popular libraries such as EF, which is expression based and can be easily forgotten and will result
in an `IndexNotFoundException`.

## AkadeIndexedSet0001

:warning: Use x as parameter name in any lambdas that determines an index name.

### Example

#### Violates

```csharp
IndexedSet<int> set = IndexedSetBuilder<int>.Create(y => y)
                                            .WithIndex(y => y / 2)
                                            .Build();

_ = set.Where(z => z / 2, 3);
```

#### Follows the convention

```csharp
IndexedSet<int> set = IndexedSetBuilder<int>.Create(x => x)
                                            .WithIndex(x => x / 2)
                                            .Build();

_ = set.Where(x => x / 2, 3);
```

## AkadeIndexedSet0002

:warning: Do not use parentheses in any lambda that determines an index name

### Example

#### Violates

```csharp
IndexedSet<int> set = IndexedSetBuilder<int>.Create((x) => x)
                                            .WithIndex((x) => x / 2)
                                            .Build();

_ = set.Where((x) => x / 2, 3);
```

#### Follows the convention

```csharp
IndexedSet<int> set = IndexedSetBuilder<int>.Create(x => x)
                                            .WithIndex(x => x / 2)
                                            .Build();

_ = set.Where(x => x / 2, 3);
```

## AkadeIndexedSet0003

:warning: Do not use block bodied in any lambda that determines an index name

### Example

#### Violates

```csharp
IndexedSet<int> set = IndexedSetBuilder<int>.Create(x =>
                                            {
                                                return x;
                                            }).WithIndex(x =>
                                            {
                                                return x / 2;
                                            }.WithIndex(x =>
                                            {
                                                double cosine = Math.Cos(x);
                                                double sine = Math.Sin(x);
                                                return cosine * sine;
                                            }).Build();

_ = set.Where(x =>
{
    return x / 2, 3);
};
```

#### Follows the convention

```csharp
IndexedSet<int> set = IndexedSetBuilder<int>.Create(x => x)
                                            .WithIndex(x => x / 2)
                                            .WithIndex(IntIndices.CosTimesSin)
                                            .Build();

_ = set.Where(x => x / 2, 3);

// <...>

public static sealed class IntIndices
{
    public static double CosTimesSin(int input)
    {
        double cosine = Math.Cos(x);
        double sine = Math.Sin(x);
        return cosine * sine;
    }
}

```