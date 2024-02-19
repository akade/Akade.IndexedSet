namespace Akade.IndexedSet;

#if !NET8_0_OR_GREATER
/// <summary>
/// Polyfill for the .NET 8 <see cref="ExperimentalAttribute"/> class.
/// Indicates that an API is experimental and it may change in the future.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly |
                AttributeTargets.Module |
                AttributeTargets.Class |
                AttributeTargets.Struct |
                AttributeTargets.Enum |
                AttributeTargets.Constructor |
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Field |
                AttributeTargets.Event |
                AttributeTargets.Interface |
                AttributeTargets.Delegate, Inherited = false)]
public class ExperimentalAttribute : Attribute
{
    private readonly string _diagnosticsId;

    /// <summary>
    ///  Initializes a new instance of the <see cref="ExperimentalAttribute"/> class, specifying the ID that the compiler will use
    ///  when reporting a use of the API the attribute applies to.
    /// </summary>
    internal ExperimentalAttribute(string diagnosticsId)
    {
        _diagnosticsId = diagnosticsId;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return UrlFormat is null ? _diagnosticsId : string.Format(UrlFormat, _diagnosticsId);
    }

    /// <summary>
    ///  Gets or sets the URL for corresponding documentation.
    ///  The API accepts a format string instead of an actual URL, creating a generic URL that includes the diagnostic ID.
    /// </summary>
    public string? UrlFormat { get; set; }
}

#endif

internal static class Experiments
{
    public const string TextSearchImprovements = "AkadeIndexedSetEXP0001";
    public const string UrlTemplate = "https://github.com/akade/Akade.IndexedSet/blob/main/docs/Experiments.md#{0}";
}