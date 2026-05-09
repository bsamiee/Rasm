using Core.Domain;
using LanguageExt.Common;
using Thinktecture;
namespace Core.Runtime;

// --- [MODELS] ----------------------------------------------------------------------------------

[ValueObject<int>(SkipFactoryMethods = true)]
public readonly partial struct IndexHint {
    public int Value =>
        _value;
    public static Fin<IndexHint> Create(int value) =>
        Fin.Succ(new IndexHint(value: value));
}

public sealed record AnalysisRuntime(GeometryContext Context, Option<IndexHint> Index = default);
