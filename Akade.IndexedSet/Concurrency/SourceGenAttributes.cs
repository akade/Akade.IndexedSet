using System.Diagnostics;

namespace Akade.IndexedSet.Concurrency;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[Conditional("RemovedFromBuiltCode")]
internal class ReadAccessAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[Conditional("RemovedFromBuiltCode")]
internal class WriteAccessAttribute : Attribute { }
