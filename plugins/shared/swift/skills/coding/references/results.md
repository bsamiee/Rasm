# [RESULTS]

## [01]-[ACCUMULATION]

Independent steps all run, and the caller receives every rejection at once. Module `Results` declares the accumulating forms once:

| [INDEX] | [MEMBER]                  | [STATES]                                                            |
| :-----: | :------------------------ | :------------------------------------------------------------------ |
|  [01]   | `AggregateError<Element>` | `first` beside `remaining`, at least one failure by construction    |
|  [02]   | `AggregateError(_:)`      | `nil` for an empty collection, the one path to no aggregate         |
|  [03]   | `errors`, `map(_:)`       | Every failure in order, every failure retyped                       |
|  [04]   | `Result.aggregated(_:)`   | One step's failure as a one-element aggregate through a transform   |
|  [05]   | `combine(_:)`             | Pack of aggregate results to a tuple of successes, or every failure |
|  [06]   | `traverse(_:_:)`          | One pass over a sequence to an array of successes, or every failure |

`Program.require()` requests each missing privacy grant, then reports every grant still missing beside a failed read of the app's designated requirement:

```swift
let missing = Grant.allCases.filter { grant in !grant.isHeld }
_ = try combine(identified, traverse(missing) { grant in Result(catching: grant.check).aggregated(\.self) }).get()
```

`identified` is a `Result<Data, AggregateError<ApplicationFailure>>`, and `check()` throws `ApplicationFailure`. A loop throwing at the first missing grant left every later grant unrequested:
- `Result(catching:)` takes a typed throwing method by reference, synchronous or async, and keeps its error type
- `aggregated` takes `\.self` to keep the module error, a case constructor to wrap it (`Reason.die`), or a closure filing a package refusal
- Dependent steps of one element run in one closure returning an aggregate, and `traverse` still reports every element
- Listings of independent items return successes beside failures (`Installation.Listing`), as one unreadable bundle failed a whole aggregate
- Aggregates of errors are `LocalizedError` with one line per failure and `CustomNSError` holding every failure under `NSMultipleUnderlyingErrorsKey`
- CasePaths makes `Result` case-pathable, `[case: \.success]` and `[case: \.failure]` read one outcome with no `switch`
- `[case:]` over a `Result` with a tuple `Success` crashes IR generation with no `error:` line, and such a success takes a record type

## [02]-[VALIDATION]

Initializers build a record from raw input and reject every invalid field at once. `map(Result.success)` lifts a failable step beside its rejection, and an optional field maps its present value and succeeds with `nil` when absent:

```swift
init(accountID: String?, organizationID: String?, email: String?, plan: String?) throws(AccountFailure) {
    func require(_ value: String?, _ field: CodingKeys) -> Result<String, AggregateError<AccountFailure>> {
        value.map { value in value.trimmingCharacters(in: .whitespacesAndNewlines) }.flatMap { value in value.isEmpty ? nil : value }.map(Result.success)
            ?? .failure(AggregateError(first: AccountFailure(subject: field.stringValue, reason: .missing)))
    }
    (self.accountID, self.email, self.organizationID) = try combine(
        require(accountID, .accountID),
        require(email, .email),
        organizationID.map { value in require(value, .organizationID).map(Optional.some) } ?? .success(nil),
    )
    .mapError { causes in AccountFailure(subject: "Account identity", reason: .invalid(causes)) }
    .get()
    self.plan = plan
}
```

`AccountIdentity.init(from:)` decodes through this initializer, so a decoded identity meets the same rules. `AccountFailure.Reason` declares `indirect case invalid(AggregateError<AccountFailure>)`, as an error holding its own aggregate inline has no finite size:
- Field rules a type holds (`LongestSide(rawValue:)` refusing a non-positive side) lift its failable init in place of a local rule
- Callers acting on each failure receive `AggregateError<E>`, callers treating the rejection as one failure an `invalid` reason holding it
- Cross-field rules run on the values `combine` returns
- Rules read as one message are rows `[(kept: Bool, rule: String)]`, and `Input.init` throws once naming every failed row

## [03]-[WORKFLOWS]

Dependent steps bind with `try`, and the first failure ends them. Behind a step that throws an aggregate, a later step's single failure lifts into a one-element aggregate:

```swift
public func send(_ query: String?, arguments: [String: AppleEventValue], target: sending NSAppleEventDescriptor) async throws(AggregateError<AppleEventFailure>) -> AppleEventValue {
    let command = try Result { () throws(Unmatched) in try command(query, taking: Set(arguments.keys)) }
        .aggregated { miss in AppleEventFailure(subject: query ?? miss.subject, reason: .unmatched(miss)) }.get()
    let event = try event(command, arguments: arguments, target: target)
    do { return try await NSAppleEventDescriptor.send(event) { reply in AppleEventValue(reply, in: self) } } catch { throw AggregateError(first: error) }
}
```

`command(_:taking:)` throws `Unmatched`, `event(_:arguments:target:)` encodes every argument through `traverse`, and `NSAppleEventDescriptor.send` throws `AppleEventFailure`:
- Lifts in an expression (a `combine` argument, a closure) go through `aggregated`, lifts in a statement through its `catch`
- `do` bodies infer their error type from their `try` sites, a `throw` statement inside widens it to `any Error`, and `do throws(E)` states it
- `Unmatched.single(_:subject:query:candidates:named:)` returns one match, else an `Unmatched` naming the candidates or every match found
- Module reasons hold an `Unmatched` whole (`.unmatched(Unmatched)`), its candidates reaching the caller
- Cleanup failing after a failed step throws both as `AggregateError(first: error, remaining: [cleanup]) as NSError` in an `NSError` reason
- Retries answer one reason another process ends (`failure.reason.is(\.held)` on a lock), each wait the next of `Backoff.delays(using:)`

## [04]-[TASK_RESULTS]

`Task` declares its throwing initializers for `Failure == any Error` alone, throwing groups fix their failure to `any Error`, and `async let` has no typed form. Children return a typed `Result` through the async `Result(catching:)`, and the parent decides once:

```swift
func register(_ agents: [LaunchAgent]) async throws(AggregateError<ApplicationFailure>) {
    let registered: [Result<Void, ApplicationFailure>] = await withTaskGroup { group in
        for agent in agents { group.addTask { await Result(catching: agent.register) } }
        return await Array(group)
    }
    _ = try traverse(registered) { outcome in outcome.aggregated(\.self) }.get()
}
```

`register()` is a `@concurrent` method throwing `ApplicationFailure`. AsyncAlgorithms builds the `Array` from the group in completion order, every registration runs, and `traverse` reports each failed one:
- Fixed counts bind each `async let` to a `Result` and read every binding in one `combine`
- `race(_:against:)` and `withTimeout(_:expired:clock:_:)` from `Results` keep `Failure` typed through their task group

## [05]-[ENTRY_POINTS]

Entry points map a typed failure to host terms once, and layers below return it unchanged. Process entries log a failure once at `critical` with `error:`, a cancelled run at `info`, and exit with `EXIT_FAILURE` after their `ServiceGroup` ends. Servers encode a failure once as their wire error, and an aggregate erased by an untyped package member (`withSpan` rethrows `any Error`) reads back through the `NSError` bridge:

```swift
extension AggregateError where Element == Reason {
    init(failing error: any Error) {
        self = Self(((error as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [any Error] ?? []).map(Reason.fail)) ?? Self(first: .fail(error))
    }
}
```

`Reason` is the JSON-RPC exit reason, `fail` an expected failure and `die` a defect, as Effect's `Cause` tags them for the TypeScript client. An aggregate of any module's error arrives as one `fail` per element, and one `Cause` encodes every element with the first one's message.
