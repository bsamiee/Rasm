using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RuntimeBindingTests : CSharpCodeFixVerifier<RuntimeBindingAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "Members named by string are reported with the string binding as their fact; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldReportError")]
    public Task BoundByName() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Linq.Expressions;
        using System.Reflection;

        internal static class BoundByName {
            internal static Expression Called(Expression target) => {|#0:Expression.Call(target, "ToString", null)|};

            internal static Expression Read(Expression target) => {|#1:Expression.Property(target, "Length")|};

            internal static Expression Stored(Expression target) => {|#2:Expression.Field(target, "held")|};

            internal static Expression Either(Expression target) => {|#3:Expression.PropertyOrField(target, "Length")|};

            internal static Delegate Converted(object target) => {|#4:Delegate.CreateDelegate(typeof(Action), target, "Run")|};

            internal static object? Invoked(Type type, object target) => {|#5:type.InvokeMember("Run", BindingFlags.InvokeMethod, null, target, null)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("""Expression.Call(target, "ToString", null)""", "binds a member by string", "name it through an expression lambda or a `MethodInfo`, `PropertyInfo`, or `FieldInfo` overload"),
        Diagnostic().WithLocation(1).WithArguments("""Expression.Property(target, "Length")""", "binds a member by string", "name it through an expression lambda or a `MethodInfo`, `PropertyInfo`, or `FieldInfo` overload"),
        Diagnostic().WithLocation(2).WithArguments("""Expression.Field(target, "held")""", "binds a member by string", "name it through an expression lambda or a `MethodInfo`, `PropertyInfo`, or `FieldInfo` overload"),
        Diagnostic().WithLocation(3).WithArguments("""Expression.PropertyOrField(target, "Length")""", "binds a member by string", "name it through an expression lambda or a `MethodInfo`, `PropertyInfo`, or `FieldInfo` overload"),
        Diagnostic().WithLocation(4).WithArguments("""Delegate.CreateDelegate(typeof(Action), target, "Run")""", "binds a method by string", "convert a method group or call `CreateDelegate(Type, MethodInfo)`"),
        Diagnostic().WithLocation(5).WithArguments("""type.InvokeMember("Run", BindingFlags.InvokeMethod, null, target, null)""", "binds a member by reflection",
            "call it directly, or read static abstract members through a type parameter constrained to their interface"));

    [Fact(DisplayName = "Reflected constructors, interface maps, uninitialized instances, and compiled expression trees are reported; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldReportError")]
    public Task ResolvedAtRunTime() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using System.Linq.Expressions;
        using System.Reflection;
        using System.Runtime.CompilerServices;

        internal static class ResolvedAtRunTime {
            internal static object? Hidden(Type type) => {|#0:Activator.CreateInstance(type, nonPublic: true)|};

            internal static object? Supplied(Type type, object[] arguments) => {|#1:Activator.CreateInstance(type, arguments)|};

            internal static object? Loaded(AppDomain domain) => {|#2:domain.CreateInstanceAndUnwrap("Sample", "Sample.Widget")|};

            internal static IEnumerable<ConstructorInfo> Listed(Type type) => {|#3:type.GetTypeInfo().DeclaredConstructors|};

            internal static InterfaceMapping Mapped(Type type) => {|#4:type.GetInterfaceMap(typeof(IDisposable))|};

            internal static object Blank(Type type) => {|#5:RuntimeHelpers.GetUninitializedObject(type)|};

            internal static Func<Type, object> Grouped => {|#6:RuntimeHelpers.GetUninitializedObject|};

            internal static Func<int> Typed(Expression<Func<int>> tree) => {|#7:tree.Compile()|};

            internal static Delegate Untyped(LambdaExpression tree) => {|#8:tree.Compile()|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Activator.CreateInstance(type, nonPublic: true)", "resolves a type or constructor at run time", "call a declared factory or public constructor"),
        Diagnostic().WithLocation(1).WithArguments("Activator.CreateInstance(type, arguments)", "resolves a type or constructor at run time", "call a declared factory or public constructor"),
        Diagnostic().WithLocation(2).WithArguments("""domain.CreateInstanceAndUnwrap("Sample", "Sample.Widget")""", "resolves a type from name strings", "construct through `new`"),
        Diagnostic().WithLocation(3).WithArguments("type.GetTypeInfo().DeclaredConstructors", "lists non-public constructors", "call a declared factory or public constructor"),
        Diagnostic().WithLocation(4).WithArguments("type.GetInterfaceMap(typeof(IDisposable))", "binds a member by reflection",
            "call it directly, or read static abstract members through a type parameter constrained to their interface"),
        Diagnostic().WithLocation(5).WithArguments("RuntimeHelpers.GetUninitializedObject(type)", "creates an instance without running a constructor", "call a declared factory or public constructor"),
        Diagnostic().WithLocation(6).WithArguments("RuntimeHelpers.GetUninitializedObject", "creates an instance without running a constructor", "call a declared factory or public constructor"),
        Diagnostic().WithLocation(7).WithArguments("tree.Compile()", "compiles an expression tree at run time", "take the delegate and its call text through `[CallerArgumentExpression]`"),
        Diagnostic().WithLocation(8).WithArguments("tree.Compile()", "compiles an expression tree at run time", "take the delegate and its call text through `[CallerArgumentExpression]`"));

    [Fact(DisplayName = "Reflection-info overloads, method groups, constrained type parameters, the public parameterless constructor of a run-time type, and nameof are clean; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldNotReportError")]
    public Task BoundByCompiler() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Linq.Expressions;
        using System.Reflection;

        internal static class BoundByCompiler {
            internal static Expression Called(Expression target, MethodInfo method) => Expression.Call(target, method);

            internal static Expression Read(Expression target, PropertyInfo property) => Expression.Property(target, property);

            internal static Expression Stored(Expression target, FieldInfo field) => Expression.Field(target, field);

            internal static Expression<Func<string, int>> Measured => static text => text.Length;

            internal static Delegate Converted(MethodInfo method) => Delegate.CreateDelegate(typeof(Action), method);

            internal static Action Grouped(IDisposable owned) => owned.Dispose;

            internal static T Made<T>() where T : new() => new T();

            internal static T Defaulted<T>() => Activator.CreateInstance<T>();

            internal static object? Public(Type type) => Activator.CreateInstance(type);

            internal static T Parsed<T>(string text) where T : IParsable<T> => T.Parse(text, null);

            internal static string Named => nameof(Type.InvokeMember);
        }
        """);
}
