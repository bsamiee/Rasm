using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AmbientClockAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Clock = new(
        "RASM3106",
        "Ambient clock read, take a `TimeProvider`",
        "`{0}` reads the ambient clock, {1}",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Time enters as a `TimeProvider` argument, so code reading `DateTime.Now`, `Stopwatch`, `Environment.TickCount`, NodaTime's `SystemClock`, or `Thread.Sleep` depends on a clock no caller controls.",
        Link("RASM3106"));

    private static readonly DiagnosticDescriptor System = new(
        "RASM3107",
        "`TimeProvider.System` outside a composition root, take a `TimeProvider`",
        "`{0}` names the system clock, take a `TimeProvider` argument and let the composition root pass `TimeProvider.System`",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Only a composition root names `TimeProvider.System`, every other member takes the `TimeProvider` its caller holds, executables are composition roots and path rows name others.",
        Link("RASM3107"));

    private static readonly ImmutableArray<(string Id, string Remedy)> Readings = [
        ("P:System.DateTime.Now", "read `GetLocalNow()` from a `TimeProvider` argument"),
        ("P:System.DateTime.UtcNow", "read `GetUtcNow()` from a `TimeProvider` argument"),
        ("P:System.DateTime.Today", "read `GetLocalNow().Date` from a `TimeProvider` argument"),
        ("P:System.DateTimeOffset.Now", "read `GetLocalNow()` from a `TimeProvider` argument"),
        ("P:System.DateTimeOffset.UtcNow", "read `GetUtcNow()` from a `TimeProvider` argument"),
        ("P:System.Environment.TickCount", "read `GetTimestamp()` from a `TimeProvider` argument"),
        ("P:System.Environment.TickCount64", "read `GetTimestamp()` from a `TimeProvider` argument"),
        ("T:System.Diagnostics.Stopwatch", "read `GetTimestamp()` and `GetElapsedTime(start)` from a `TimeProvider` argument"),
        ("P:System.TimeProvider.TimestampFrequency", "call `GetElapsedTime(start)` on the `TimeProvider` in place of the timestamp ratio"),
        ("P:NodaTime.SystemClock.Instance", "read time from a `TimeProvider` argument"),
        ("M:System.Threading.Thread.Sleep(System.Int32)", "await `Task.Delay(delay, timeProvider)`"),
        ("M:System.Threading.Thread.Sleep(System.TimeSpan)", "await `Task.Delay(delay, timeProvider)`"),
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Clock, System];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            ImmutableArray<(ISymbol Symbol, string Remedy)> readings = [.. Readings.SelectMany(row => Declared(start.Compilation, row.Id).Select(symbol => (symbol, row.Remedy)))];
            ISymbol? system = start.Compilation.Options.OutputKind is OutputKind.ConsoleApplication or OutputKind.WindowsApplication or OutputKind.WindowsRuntimeApplication
                ? null : Declared(start.Compilation, "P:System.TimeProvider.System").FirstOrDefault();
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.FieldReference, OperationKind.MethodReference, OperationKind.ObjectCreation], (operation, _) =>
                (operation switch { IObjectCreationOperation creation => creation.Constructor, _ => Target(operation) }) is not { } member || Inside<INameOfOperation>(operation) ? []
                : Same(member, system) ? [Diagnostic.Create(System, operation.Syntax.GetLocation(), operation.Syntax)]
                : readings.FirstOrDefault(row => Same(row.Symbol, member) || Same(row.Symbol, member.ContainingType)) is { Symbol: not null } row
                    ? [Diagnostic.Create(Clock, operation.Syntax.GetLocation(), operation.Syntax, row.Remedy)]
                : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RuntimeBindingAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3108",
        "Member bound at run time, name the member in code",
        "`{0}` {1}, {2}",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Members named by string, constructors reached by reflection, interface maps, uninitialized instances, and compiled expression trees bypass the binding the compiler checks, a direct call, method group, or constrained type parameter states the member.",
        Link("RASM3108"));

    private static readonly Func<ISymbol, bool> Named = static member => member is IMethodSymbol method && method.Parameters.Any(static parameter => parameter.Type.SpecialType is SpecialType.System_String);

    private static readonly Func<ISymbol, bool> Constructed = static member => member is IMethodSymbol { IsGenericMethod: false, Parameters: var parameters }
        && !(parameters.Length is 1 && parameters[0].Type.SpecialType is not SpecialType.System_String);

    private static readonly Func<ISymbol, bool> Any = static _ => true;

    private static readonly ImmutableArray<(string Owner, ImmutableArray<string> Members, Func<ISymbol, bool> Bound, string Fact, string Remedy)> Bindings = [
        ("System.Linq.Expressions.Expression", ["Call", "Field", "Property", "PropertyOrField"], Named, "binds a member by string", "name it through an expression lambda or a `MethodInfo`, `PropertyInfo`, or `FieldInfo` overload"),
        ("System.Delegate", ["CreateDelegate"], Named, "binds a method by string", "convert a method group or call `CreateDelegate(Type, MethodInfo)`"),
        ("System.Activator", ["CreateInstance", "CreateInstanceFrom"], Constructed, "resolves a type or constructor at run time", "call a declared factory or public constructor"),
        ("System.AppDomain", ["CreateInstance", "CreateInstanceAndUnwrap", "CreateInstanceFrom", "CreateInstanceFromAndUnwrap"], Any, "resolves a type from name strings", "construct through `new`"),
        ("System.Type", ["InvokeMember", "GetInterfaceMap"], Any, "binds a member by reflection", "call it directly, or read static abstract members through a type parameter constrained to their interface"),
        ("System.Reflection.TypeInfo", ["DeclaredConstructors"], Any, "lists non-public constructors", "call a declared factory or public constructor"),
        ("System.Runtime.CompilerServices.RuntimeHelpers", ["GetUninitializedObject"], Any, "creates an instance without running a constructor", "call a declared factory or public constructor"),
        ("System.Runtime.Serialization.FormatterServices", ["GetUninitializedObject", "GetSafeUninitializedObject"], Any, "creates an instance without running a constructor", "call a declared factory or public constructor"),
        ("System.Linq.Expressions.Expression`1", ["Compile"], Any, "compiles an expression tree at run time", "take the delegate and its call text through `[CallerArgumentExpression]`"),
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            ImmutableArray<(ISymbol Member, string Fact, string Remedy)> bindings = [
                .. Bindings.SelectMany(row => row.Members.SelectMany(name => types.GetOrCreateTypeByMetadataName(row.Owner)?.GetMembers(name) ?? [])
                    .Where(row.Bound).Select(member => (member, row.Fact, row.Remedy))),
            ];
            if (bindings.IsEmpty) return;
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.MethodReference], (operation, _) =>
                Target(operation) is { } member && !Inside<INameOfOperation>(operation) && bindings.FirstOrDefault(row => Same(row.Member, member)) is { Member: not null } row
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax, row.Fact, row.Remedy)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlaceholderValueAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3109",
        "No-op placeholder value, declare the optional member with its default",
        "`{0}` stands in for an absent callback, declare optional callbacks as `init` members holding their default",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Packaged no-op delegates and disposables restate absence as a value, an optional `init` member holding its default states it once.",
        Link("RASM3109"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            INamedTypeSymbol? empty = types.GetOrCreateTypeByMetadataName("Thinktecture.Empty");
            ImmutableArray<ISymbol> disposables = Declared(start.Compilation, "P:System.Reactive.Disposables.Disposable.Empty");
            if (empty is null && disposables.IsEmpty) return;
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.FieldReference, OperationKind.MethodReference], (operation, _) =>
                Target(operation) is { } member && !Inside<INameOfOperation>(operation) && (Same(member.ContainingType, empty) || Among(member, disposables))
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TrySiblingAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3110",
        "Throwing member with a `Try` sibling, call the `Try` form",
        "`{0}` throws where `{1}` returns the failure as data, call `{1}`",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "A member of a `RASM3110.types` type whose declaring type offers a `Try` sibling taking the same arguments and an `out` of its result throws on the input the sibling refuses, the sibling turns that input into a value the caller handles.",
        Link("RASM3110"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            ImmutableArray<INamedTypeSymbol> types = [.. Option(start.Options, "RASM3110.types").SelectMany(id => Declared(start.Compilation, id)).OfType<INamedTypeSymbol>()];
            if (types.IsEmpty) return;
            static IMethodSymbol? Sibling(IMethodSymbol method, string name) =>
                method.ContainingType.GetMembers(name).OfType<IMethodSymbol>().Where(tried => tried.Arity == method.Arity && tried.IsStatic == method.IsStatic)
                    .Select(tried => tried.IsGenericMethod ? tried.Construct([.. method.TypeArguments]) : tried)
                    .FirstOrDefault(tried => tried is { ReturnType.SpecialType: SpecialType.System_Boolean } && tried.Parameters.Length == method.Parameters.Length + 1
                        && tried.Parameters[method.Parameters.Length] is { RefKind: RefKind.Out } result && SymbolEqualityComparer.Default.Equals(result.Type, method.ReturnType)
                        && method.Parameters.Zip(tried.Parameters, static (held, other) => other.RefKind is RefKind.None && SymbolEqualityComparer.Default.Equals(held.Type, other.Type)).All(static same => same));
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.PropertyReference], (operation, _) =>
                !Among(Target(operation)?.ContainingType, types) ? []
                : (operation switch {
                    IInvocationOperation { TargetMethod: { ReturnsVoid: false } method } when !method.Name.StartsWith("Try", StringComparison.Ordinal) => Sibling(method.ReducedFrom ?? method, $"Try{method.Name}"),
                    IPropertyReferenceOperation { Property: { IsIndexer: true, GetMethod: { } getter } } read when !Written(read) => Sibling(getter, "TryGetValue"),
                    _ => null,
                }) is { } sibling
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax, sibling.Name)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlatformMemberAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Names = new(
        "RASM3111",
        "Platform-dependent name set, declare the portable reserved set",
        "`{0}` answers the running platform's set, declare one `SearchValues<char>` over the portable reserved set",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`Path.GetInvalidFileNameChars` and `GetInvalidPathChars` return NUL and `/` alone on macOS and Linux, so names they pass fail on Windows.",
        Link("RASM3111"));

    private static readonly DiagnosticDescriptor Lanes = new(
        "RASM3112",
        "`Vector2` or `Vector3` lane comparison, compare through the lane masks",
        "`{0}` compares undefined lanes past the vector's components, compare through `AllWhereAllBitsSet` or `AnyWhereAllBitsSet` over the lane comparison, or `==`",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`Vector2` and `Vector3` `*All` and `*Any` comparisons run over the widened register and read lanes past the declared components, the lane-mask forms or `==` compare the components alone.",
        Link("RASM3112"));

    private static readonly DiagnosticDescriptor Defect = new(
        "RASM3113",
        "Package member with a known defect, call the working form",
        "`{0}` {1}, {2}",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Package members whose result is wrong or leaks on a path the signature hides, each with the form that works.",
        Link("RASM3113"));

    private static readonly ImmutableArray<(string Id, string Fact, string Remedy)> Defects = [
        ("M:System.Drawing.Bitmap.GetHicon", "leaks a handle no `Icon` releases", "call host overloads taking an assembly and resource name, or `new Icon(Stream)` on `.ico` resources"),
        ("M:SkiaSharp.SKCanvas.Flush", "does nothing on a raster canvas", "present GPU surfaces through `SKSurface.Flush(submit: true)`"),
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Names, Lanes, Defect];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            ImmutableArray<IMethodSymbol> names = Members(types.GetOrCreateTypeByMetadataName("System.IO.Path"), "GetInvalidFileNameChars", "GetInvalidPathChars");
            ImmutableArray<IMethodSymbol> lanes = [.. new[] { types.GetOrCreateTypeByMetadataName("System.Numerics.Vector2"), types.GetOrCreateTypeByMetadataName("System.Numerics.Vector3") }.OfType<INamedTypeSymbol>()
                .SelectMany(static vector => vector.GetMembers().OfType<IMethodSymbol>().Where(method => method is { IsStatic: true, Parameters.Length: 2 }
                    && (method.Name.EndsWith("All", StringComparison.Ordinal) || method.Name.EndsWith("Any", StringComparison.Ordinal))
                    && method.Parameters.All(parameter => Same(parameter.Type, vector))))];
            ImmutableArray<(ISymbol Member, string Fact, string Remedy)> defects = [.. Defects.SelectMany(row => Declared(start.Compilation, row.Id).Select(member => (member, row.Fact, row.Remedy)))];
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.MethodReference], (operation, _) =>
                Target(operation) is not IMethodSymbol method || Inside<INameOfOperation>(operation) ? []
                : Among(method, names) ? [Diagnostic.Create(Names, operation.Syntax.GetLocation(), operation.Syntax)]
                : Among(method, lanes) ? [Diagnostic.Create(Lanes, operation.Syntax.GetLocation(), operation.Syntax)]
                : defects.FirstOrDefault(row => Same(row.Member, method)) is { Member: not null } row ? [Diagnostic.Create(Defect, operation.Syntax.GetLocation(), operation.Syntax, row.Fact, row.Remedy)]
                : []);
        });
    }
}
