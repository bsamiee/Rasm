using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RuntimeBindingTests : CSharpCodeFixVerifier<RuntimeBindingAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsMembersBoundByStringAndIgnoresOverloadsTakingAReflectedMember() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Linq.Expressions;
        using System.Reflection;

        internal static class Bound {
            internal static Expression Named(Expression target) => {|RASM3108:Expression.Property(target, "Length")|};
            internal static Expression Reflected(Expression target, PropertyInfo property) => Expression.Property(target, property);
            internal static object? Invoked(Type type, object target) => {|RASM3108:type.InvokeMember("Run", BindingFlags.InvokeMethod, null, target, null)|};
            internal static string Mentioned => nameof(Type.InvokeMember);
        }
        """);

    [Fact]
    public Task ReportsConstructionThatBypassesConstructorsAndIgnoresThePublicConstructorOfARunTimeType() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using System.Linq.Expressions;
        using System.Reflection;
        using System.Runtime.CompilerServices;

        internal static class Constructed {
            internal static object? Hidden(Type type) => {|RASM3108:Activator.CreateInstance(type, nonPublic: true)|};
            internal static object? Public(Type type) => Activator.CreateInstance(type);
            internal static T Defaulted<T>() => Activator.CreateInstance<T>();
            internal static IEnumerable<ConstructorInfo> Listed(Type type) => {|RASM3108:type.GetTypeInfo().DeclaredConstructors|};
            internal static Func<Type, object> Blank => {|RASM3108:RuntimeHelpers.GetUninitializedObject|};
            internal static Func<int> Compiled(Expression<Func<int>> tree) => {|RASM3108:tree.Compile()|};
        }
        """);
}
