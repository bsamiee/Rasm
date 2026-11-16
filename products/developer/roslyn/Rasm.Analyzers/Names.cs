using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NamespaceOwnerAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Borrowed = new(
        "RASM3005",
        "Namespace owned by a referenced assembly, declare under project namespace",
        "`{1}` declares `{0}`, declare types under the project namespace",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Declare types under project namespaces, outside every namespace a referenced assembly declares.",
        Link("RASM3005"));

    private static readonly DiagnosticDescriptor Clash = new(
        "RASM3006",
        "Type name clashes with an imported type, rename the declaration",
        "`{0}` clashes with `{1}`, rename the project declaration",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Name namespace-level types apart from public imported types and public types of project references, of equal arity.",
        Link("RASM3006"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Borrowed, Clash];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            IAssemblySymbol own = start.Compilation.Assembly;
            bool Exposed(INamedTypeSymbol other) => other.DeclaredAccessibility is Accessibility.Public && !Same(other.ContainingAssembly, own);
            Lazy<ILookup<(string Name, int Arity), INamedTypeSymbol>> available = new(() => {
                List<INamedTypeSymbol> referenced = [];
                Stack<INamespaceSymbol> remaining = new(Projects(start.Compilation, start.Options).Select(static source => source.GlobalNamespace));
                while (remaining.Count is not 0) {
                    INamespaceSymbol space = remaining.Pop();
                    referenced.AddRange(space.GetTypeMembers().Where(Exposed));
                    foreach (INamespaceSymbol nested in space.GetNamespaceMembers()) remaining.Push(nested);
                }
                return referenced.ToLookup(static type => (type.Name, type.Arity));
            });
            start.RegisterSyntaxNodeAction(
                static node => {
                    BaseNamespaceDeclarationSyntax declaration = (BaseNamespaceDeclarationSyntax)node.Node;
                    if (node.SemanticModel.GetDeclaredSymbol(declaration, node.CancellationToken) is INamespaceSymbol declared
                        && node.Compilation.GetCompilationNamespace(declared)?.ConstituentNamespaces.FirstOrDefault(space => space.ContainingAssembly is { } owner && !Same(owner, node.Compilation.Assembly)) is { ContainingAssembly: { } assembly })
                        node.ReportDiagnostic(Diagnostic.Create(Borrowed, declaration.Name.GetLocation(), declared.ToDisplayString(), assembly.Name));
                },
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.FileScopedNamespaceDeclaration);
            start.RegisterSymbolStartAction(
                symbol => {
                    if (symbol.Symbol is not INamedTypeSymbol { ContainingType: null } type || type.Locations.FirstOrDefault(static location => location.IsInSource) is not { } first) return;
                    ConcurrentBag<INamedTypeSymbol> clashes = [];
                    symbol.RegisterSyntaxNodeAction(
                        node => {
                            if (!Same(node.SemanticModel.GetDeclaredSymbol(node.Node, node.CancellationToken), type)) return;
                            foreach (INamedTypeSymbol found in node.SemanticModel.GetImportScopes(node.Node.SpanStart, node.CancellationToken)
                                .SelectMany(static scope => scope.Imports).Select(static import => import.NamespaceOrType).OfType<INamespaceSymbol>()
                                .Distinct<INamespaceSymbol>(SymbolEqualityComparer.Default).SelectMany(space => space.GetTypeMembers(type.Name, type.Arity)).Where(Exposed))
                                clashes.Add(found);
                        },
                        SyntaxKind.ClassDeclaration,
                        SyntaxKind.StructDeclaration,
                        SyntaxKind.RecordDeclaration,
                        SyntaxKind.RecordStructDeclaration,
                        SyntaxKind.InterfaceDeclaration,
                        SyntaxKind.EnumDeclaration,
                        SyntaxKind.DelegateDeclaration);
                    symbol.RegisterSymbolEndAction(end => {
                        if (clashes.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
                            .OrderBy(static found => found.ContainingAssembly.Identity.ToString(), StringComparer.Ordinal).ThenBy(static found => found.ToDisplayString(), StringComparer.Ordinal)
                            .Concat(available.Value[(type.Name, type.Arity)]).FirstOrDefault() is { } clash)
                            end.ReportDiagnostic(Diagnostic.Create(Clash, first, type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), clash.ToDisplayString()));
                    });
                },
                SymbolKind.NamedType);
        });
    }

    internal static ImmutableArray<IAssemblySymbol> Projects(Compilation compilation, AnalyzerOptions options) {
        ImmutableHashSet<string> projects = [
            with(StringComparer.Ordinal),
            .. options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.ProjectReferenceAssemblies", out string? listed)
                ? listed.Split([','], StringSplitOptions.RemoveEmptyEntries)
                : [],
        ];
        return [.. compilation.SourceModule.ReferencedAssemblySymbols.Where(assembly => projects.Contains(assembly.Identity.Name))];
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class QualifiedNameAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Shadowed = new(
        "RASM3009",
        "Qualified name around a project declaration, rename the declaration",
        "`{0}` is qualified around project declaration `{1}`, rename `{1}` and import `{2}`",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Rename each project type, member, parameter, or local shadowing a package or statically imported name, then import its owner.",
        Link("RASM3009"));

    private static readonly DiagnosticDescriptor Qualified = new(
        "RASM3010",
        "Qualified name with no clash, import and write the simple name",
        "`{0}` clashes with nothing in scope, import `{1}` and write `{2}`",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Import package namespaces and write simple names when no namespace type clashes with project declarations or file simple names.",
        Link("RASM3010"));

    private static readonly DiagnosticDescriptor Renamed = new(
        "RASM3014",
        "Using alias renames a type or namespace, write its name at each use",
        "`{0}` renames a {1}, write the {1} at each use",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Write types and delegates at each use: where two imported namespaces hold one simple name, import the namespace the file names simply and write the other type through its namespace; a namespace alias keeps the namespace's own name, and a tuple alias stays only where more than one use reads its shape.",
        Link("RASM3014"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Shadowed, Qualified, Renamed];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(
            static node => {
                if (node.Node is UsingDirectiveSyntax { Alias: not null } directive
                    && node.SemanticModel.GetDeclaredSymbol(directive, node.CancellationToken) is { Target: var target } alias
                    && target switch {
                        INamespaceSymbol space => !string.Equals(alias.Name, space.Name, StringComparison.Ordinal),
                        INamedTypeSymbol { IsTupleType: true } when directive.GlobalKeyword.IsKind(SyntaxKind.None) => !directive.Parent!.DescendantNodes().OfType<IdentifierNameSyntax>()
                            .Where(used => string.Equals(used.Identifier.ValueText, alias.Name, StringComparison.Ordinal)
                                && SymbolEqualityComparer.Default.Equals(node.SemanticModel.GetAliasInfo(used, node.CancellationToken), alias)).Skip(1).Any(),
                        _ => true,
                    })
                    node.ReportDiagnostic(Diagnostic.Create(Renamed, directive.GetLocation(), alias.Name, target switch {
                        INamespaceSymbol => "namespace",
                        INamedTypeSymbol { IsTupleType: true } => "tuple",
                        INamedTypeSymbol { TypeKind: TypeKind.Delegate } => "delegate",
                        _ => "type",
                    }));
            },
            SyntaxKind.UsingDirective);
        context.RegisterCompilationStartAction(static start => {
            ConcurrentDictionary<SyntaxTree, ILookup<string, SimpleNameSyntax>> unqualified = new();
            ConcurrentDictionary<SyntaxTree, ConcurrentDictionary<INamespaceSymbol, bool>> ambiguous = new();
            static bool Looked(SimpleNameSyntax name) =>
                name.Parent switch {
                    QualifiedNameSyntax qualified => qualified.Right != name,
                    MemberAccessExpressionSyntax access => access.Name != name,
                    AliasQualifiedNameSyntax aliased => aliased.Name != name,
                    MemberBindingExpressionSyntax => false,
                    _ => true,
                };
            start.RegisterSyntaxNodeAction(
                node => {
                    (ExpressionSyntax Left, SimpleNameSyntax Right)? parts = node.Node switch {
                        QualifiedNameSyntax qualified => (qualified.Left, qualified.Right),
                        MemberAccessExpressionSyntax access => (access.Expression, access.Name),
                        _ => null,
                    };
                    if (parts is not (var left, var right)
                        || (node.Node.Parent is QualifiedNameSyntax { Left: var outer } && outer == node.Node)
                        || (node.Node.Parent is MemberAccessExpressionSyntax { Expression: var receiver } && receiver == node.Node)
                        || node.Node.Ancestors().Any(static held => held is UsingDirectiveSyntax)
                        || (node.Node.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is { Name: var declared } && declared.Span.Contains(node.Node.Span)))
                        return;
                    CancellationToken token = node.CancellationToken;
                    SemanticModel model = node.SemanticModel;
                    ISymbol? owner = model.GetSymbolInfo(left, token).Symbol;
                    if (owner is not (INamespaceSymbol or INamedTypeSymbol) || Sourced(owner)) return;
                    ImmutableArray<ISymbol> scopes = [.. model.GetImportScopes(node.Node.SpanStart, token).SelectMany(static scope => scope.Imports).Select(static import => import.NamespaceOrType)];
                    if ((owner is INamedTypeSymbol) != Among(owner, scopes) || model.GetSymbolInfo(right, token).Symbol is not { } target || target is INamespaceSymbol) return;
                    bool typed = SyntaxFacts.IsInNamespaceOrTypeContext((ExpressionSyntax)node.Node);
                    ImmutableArray<ISymbol> visible = [.. model.LookupSymbols(node.Node.SpanStart, name: right.Identifier.ValueText).Where(found => !Same(found, target)
                        && (!typed || found is INamespaceOrTypeSymbol or IAliasSymbol)
                        && !(found is IFieldSymbol or IPropertySymbol or ILocalSymbol or IParameterSymbol && Same(found switch {
                            IFieldSymbol field => field.Type,
                            IPropertySymbol property => property.Type,
                            ILocalSymbol local => local.Type,
                            IParameterSymbol parameter => parameter.Type,
                            _ => null,
                        }, target)))];
                    ILookup<string, SimpleNameSyntax> used = unqualified.GetOrAdd(
                        node.Node.SyntaxTree,
                        tree => tree.GetRoot(token).DescendantNodes().OfType<SimpleNameSyntax>().Where(Looked).ToLookup(static held => held.Identifier.ValueText, StringComparer.Ordinal));
                    bool Ambiguous(INamedTypeSymbol member) =>
                        used[member.Name].Any(held => held.Arity == member.Arity && model.GetAliasInfo(held, token) is null
                            && model.GetSymbolInfo(held, token).Symbol is INamedTypeSymbol { ContainingType: null } bound && !Same(bound, member));
                    if (visible.FirstOrDefault(static found => found is not IAliasSymbol && Sourced(found)) is { } shadowing)
                        node.ReportDiagnostic(Diagnostic.Create(Shadowed, node.Node.GetLocation(), node.Node, shadowing.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), owner.ToDisplayString()));
                    else if (visible.IsEmpty && owner is INamespaceSymbol space) {
                        ImmutableHashSet<string> sourceNames = [with(StringComparer.Ordinal), .. model.LookupSymbols(node.Node.SpanStart).Where(Sourced).Select(static symbol => symbol.Name)];
                        ConcurrentDictionary<INamespaceSymbol, bool> imports = ambiguous.GetOrAdd(node.Node.SyntaxTree, static _ => new(SymbolEqualityComparer.Default));
                        if (!space.GetTypeMembers().Any(member => sourceNames.Contains(member.Name))
                            && !imports.GetOrAdd(space, importedSpace => importedSpace.GetTypeMembers().Any(Ambiguous)))
                            node.ReportDiagnostic(Diagnostic.Create(Qualified, node.Node.GetLocation(), node.Node, owner.ToDisplayString(), right));
                    }
                },
                SyntaxKind.QualifiedName,
                SyntaxKind.SimpleMemberAccessExpression);
        });
    }
}
