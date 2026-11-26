# [FUNCTIONS]

## [01]-[CLOSURE_PARAMETERS]

Closure parameters take the annotations the place their callee runs them demands. Root `Package.swift` enables `NonisolatedNonsendingByDefault`, so a nonisolated async function runs on its caller's actor:

| [INDEX] | [CALLEE_RUNS_IT]                           | [ANNOTATION]                     | [SITE]                                         |
| :-----: | :----------------------------------------- | :------------------------------- | :--------------------------------------------- |
|  [01]   | In its own isolation before it returns     | None beyond `async`, `throws(E)` | `Watch.presenting(_:clock:_:)`, `Watch.spaced` |
|  [02]   | Inside an actor the caller stands outside  | `@Sendable`, `Value: Sendable`   | `Watch.perform(on:_:)`                         |
|  [03]   | In a child task, or after the call returns | `@escaping @Sendable`            | `race(_:against:)`, `Lockfile.withLocks`       |

Actors owning host handles run a caller's body beside them, and only its `Sendable` result leaves:

```swift
public func perform<Value: Sendable>(on ref: Int?, _ body: @Sendable (AXUIElement?) throws(ApplicationFailure) -> Value) async throws(ApplicationFailure) -> Value {
    let element: AXUIElement? = if let ref { try await resolve(ref) } else { nil }
    return try body(element)
}
```

`perform` is a `Watch` member, and `resolve(_:)` reads the element a ref names from the actor's handle table:
- `presenting` is `nonisolated`, so its body runs on the caller's actor, holds non-`Sendable` `SCWindow` and `SCContentFilter` values, and returns an unconstrained `Value`
- Scoped members are generic over the body's `Value` and keep its typed error, as `Clock.measure` takes a `Void` closure and rethrows untyped
- Scopes that learn a fact return it beside the value (`(value: Value, presentation: Presentation?)`)
- Non-`Sendable` values a callee hands to a task are `sending` parameters, and a `@Sendable` reading closure turns the host reply into a `Sendable` value inside that task (`NSAppleEventDescriptor.send(_:timeout:reading:)`)

## [02]-[FUNCTION_REFERENCES]

Combinators take a named function in place of a closure literal restating one call. Swift has no curried declarations, and each form below is a function value:

| [INDEX] | [FORM]                   | [EXAMPLE]                                                                      |
| :-----: | :----------------------- | :----------------------------------------------------------------------------- |
|  [01]   | Case constructor         | `bundle.executableURL.map(LogEntry.Source.executable)`                         |
|  [02]   | Generic case constructor | `failures.map(Result<Self, ProcessFailure>.failure)`                           |
|  [03]   | Initializer by labels    | `flatMap(FourCharCode.init(sdef:))`, `outcomes.map(Exit.init)`                 |
|  [04]   | Bound method             | `compactMap(descriptor.atIndex)`, `installed.map(report.isWritten(by:))`       |
|  [05]   | Key path                 | `windows.map(\.windowID)`                                                      |
|  [06]   | Case key path            | `list?.compactMap(\.element)` over a `@CasePathable @dynamicMemberLookup` enum |

- Case constructors state the meaning a closure body restated: `.aggregated(Reason.die)` replaced `.mapError { error in AggregateError(first: Reason.die(error)) }`, and `map(LogEntry.Source.executable)` removed a tuple subject and the arm that built or omitted the case
- Generic case constructors name each generic argument the surrounding expression leaves open
- `@dynamicMemberLookup` on a `@CasePathable` enum turns each case into a key path to its optional associated value, the form `compactMap` and `flatMap` take
- Closure literals stay where the body passes a second argument or joins calls (`map { file throws(ApplicationFailure) in try LaunchAgent(file, in: url) }`)

## [03]-[PREDICATE_VALUES]

Rules a consumer stores, prints, compares, or hands to another engine are values of the type that engine reads. Rules one call reads and nobody reads back are closures:

| [INDEX] | [RULE]                       | [VALUE]                               | [READER]                                                        |
| :-----: | :--------------------------- | :------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | Sort order                   | `KeyPathComparator`                   | `sorted(using:)`, `Hashable` and `Sendable` as `SortComparator` |
|  [02]   | Interval over host constants | `Range<Int>` of `CGWindowLevelForKey` | `contains(_:)` at every reader of one constant                  |
|  [03]   | Query a host engine runs     | `NSPredicate`, `NSCompoundPredicate`  | `NSMetadataQuery.predicate`, `log --predicate`, refusals        |
|  [04]   | Test of one call             | `(T) -> Bool`                         | `DiagnosticReport.reports(where:)`                              |

`Installation.search` reads the caller's predicate as a tree, removes always-true clauses before `NSMetadataQuery` runs it, and evaluates the whole predicate over running applications:

```swift
private static func metadataForm(of predicate: NSPredicate) -> NSPredicate? {
    guard predicate != NSPredicate(value: true) else { return nil }
    guard let compound = predicate as? NSCompoundPredicate, compound.compoundPredicateType != .not else { return predicate }
    let clauses = compound.subpredicates.compactMap { clause in (clause as? NSPredicate).flatMap(metadataForm(of:)) }
    return switch (compound.compoundPredicateType, clauses.count) {
        case (.or, ..<compound.subpredicates.count), (_, 0): nil
        case (_, 1): clauses.first
        default: NSCompoundPredicate(type: compound.compoundPredicateType, subpredicates: clauses)
    }
}
```

- Refusals print the rule a caller passed through `predicateFormat`, and `NSCompoundPredicate` joins a library's clause with the caller's
- `NSPredicate` declares no `Sendable` conformance, so a library shared across tasks takes a `@Sendable () -> NSPredicate` factory
- Checks run per element keep each verdict by key in a `Mutex<[String: Bool]>` in place of parsing a predicate format per element
- One fact holds one interval: `Model.swift` and `Watch.swift` each declared a modal window band and disagreed on utility and dock windows

## [04]-[DEPENDENCIES]

Libraries own their host calls and take as parameters what an application decides: transport, policy, and the error type its module throws. One operation is a function value, and operations one type supplies together are a protocol:

| [INDEX] | [CALLER_DECIDES] | [FORM]                                                       | [SITE]                                              |
| :-----: | :--------------- | :----------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | Transport        | `@escaping @Sendable` function values                        | `serve(reading:writing:matching:cachedDefinition:)` |
|  [02]   | Policy           | Rule value, or a `@Sendable` factory of a non-`Sendable` one | `matching: @Sendable () -> NSPredicate`             |
|  [03]   | Error type       | Generic `Failure` over a protocol of constructors            | `Lockfile.withLocks`, `LockFailure`                 |
|  [04]   | Package step     | Conformance to the package's own protocol                    | `UnrecognizedValues: AccessReporter`                |

`ConfigReader(providers:accessReporter:)` takes the reporter collecting unrecognized settings at the seam `AccessLogger` and `FileAccessLogger` use. Use `generics.md` for an error type a generic library builds through requirements of the caller's error type.

Function parameters that callers fill alike are the callee's own logic:

| [INDEX] | [FORM]                            | [REPLACEMENT]                                           | [SITE]                                     |
| :-----: | :-------------------------------- | :------------------------------------------------------ | :----------------------------------------- |
|  [01]   | Closure every caller writes alike | Callee calls the member, merging targets a split forced | `Watches(recognize:)`                      |
|  [02]   | Closure one caller wrote          | Plain value the callee's expression reads               | `close(interrupting:)` to `close(failed:)` |
|  [03]   | Parameter no caller sets          | Deleted with every reader                               | `serve(…extra:)` and `Call<Extra>`         |
