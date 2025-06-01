using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Akade.IndexedSet.Tests.CommonIndexTests;
internal abstract partial class BaseIndexTest<TIndexKey, TElement, TIndex, TComparer>(Func<TElement, TIndexKey> keyAccessor, TComparer comparer)
    where TIndex : TypedIndex<TElement, TIndexKey>
    where TIndexKey : notnull
    where TComparer : notnull
{
    private readonly Func<TElement, TIndexKey> _keyAccessor = keyAccessor;
    protected readonly TComparer _comparer = comparer;

    protected abstract TElement[] GetUniqueData();

    protected abstract TElement[] GetNonUniqueData();

    protected virtual TIndexKey GetNotExistingKey()
    {
        return default!;
    }

    protected abstract bool SupportsNonUniqueKeys { get; }

    protected virtual bool SupportsRangeBasedQueries => false;
    protected virtual bool SupportsStartsWithQueries => false;
    protected virtual bool SupportsContainsQueries => false;

    protected abstract TIndex CreateIndex();

    private TIndex CreateIndexWithData(TElement[] elements)
    {
        TIndex result = CreateIndex();
        AddElements(elements, result);
        return result;
    }

    protected void AddElements(TElement[] elements, TIndex index)
    {
        index.AddRange(elements.Select(element => KeyValuePair.Create(_keyAccessor(element), element)));
    }

    [BaseTestMethod]
    public void Adding_unique_data_should_throw_if_nonunique_keys_are_not_supported()
    {
        if (!SupportsNonUniqueKeys)
        {
            _ = Assert.ThrowsExactly<ArgumentException>(() => CreateIndexWithData(GetNonUniqueData()));
        }
    }
}

internal interface IBaseIndexTest<TComparer>
    where TComparer : notnull
{
    static abstract IEnumerable<TComparer> GetComparers();
}

[AttributeUsage(AttributeTargets.Method)]
internal class BaseTestMethodAttribute : Attribute { }

internal static class BaseIndexTest
{
    public static IEnumerable<object[]> GetTestMethods<T, TComparer>()
        where T : IBaseIndexTest<TComparer>
        where TComparer : notnull
    {
        return typeof(T).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.Name != "Test")
                        .Where(m => m.GetCustomAttribute<BaseTestMethodAttribute>() is not null)
                        .SelectMany(method => T.GetComparers().Select(comparer => new object[] { method.Name, comparer }))
                        .ToArray();
    }

    public static void RunTest<T, TComparer>(string method, object comparer)
        where T : IBaseIndexTest<TComparer>
        where TComparer : notnull
    {
        var testClass = (T)(Activator.CreateInstance(typeof(T), comparer) ?? throw new InvalidOperationException());
        MethodInfo methodInfo = typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new ArgumentOutOfRangeException(nameof(method));

        _ = methodInfo.Invoke(testClass, null);
    }
}
