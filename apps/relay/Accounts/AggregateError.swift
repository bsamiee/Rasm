nonisolated protocol AggregateError: Error {
  associatedtype Element: Sendable

  init(first: Element, remaining: [Element])

  var first: Element { get }
  var remaining: [Element] { get }
}

nonisolated extension AggregateError {
  var errors: [Element] { [first] + remaining }

  init(_ error: Element) {
    self.init(first: error, remaining: [])
  }

  init?(collecting errors: [Element]) {
    guard let first: Element = errors.first else { return nil }
    self.init(first: first, remaining: Array(errors.dropFirst()))
  }

  func appending(_ other: Self) -> Self {
    Self(first: first, remaining: remaining + other.errors)
  }
}

nonisolated extension Result {
  func bind<Next>(
    _ next: (Success) async -> Result<Next, Failure>
  ) async -> Result<Next, Failure> {
    switch self {
    case .success(let value): await next(value)
    case .failure(let error): .failure(error)
    }
  }
}

nonisolated func combine<First, Second, Failure: AggregateError>(
  _ first: Result<First, Failure>,
  _ second: Result<Second, Failure>
) -> Result<(First, Second), Failure> {
  switch (first, second) {
  case (.success(let first), .success(let second)): .success((first, second))
  case (.failure(let first), .failure(let second)): .failure(first.appending(second))
  case (.failure(let first), .success): .failure(first)
  case (.success, .failure(let second)): .failure(second)
  }
}

nonisolated func combine<First, Second, Third, Failure: AggregateError>(
  _ first: Result<First, Failure>,
  _ second: Result<Second, Failure>,
  _ third: Result<Third, Failure>
) -> Result<(First, Second, Third), Failure> {
  combine(combine(first, second), third).map { pair in (pair.0.0, pair.0.1, pair.1) }
}

nonisolated func traverse<Element, Value, Failure: AggregateError>(
  _ elements: some Sequence<Element>,
  _ transform: (Element) -> Result<Value, Failure>
) -> Result<[Value], Failure> {
  var values: [Value] = []
  var errors: [Failure.Element] = []
  for element in elements {
    switch transform(element) {
    case .success(let value): values.append(value)
    case .failure(let failure): errors.append(contentsOf: failure.errors)
    }
  }
  return Failure(collecting: errors).map { failure in .failure(failure) } ?? .success(values)
}
