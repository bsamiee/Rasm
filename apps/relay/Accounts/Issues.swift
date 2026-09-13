protocol IssueAggregate: Error {
  associatedtype Issue: Sendable

  init(first: Issue, remaining: [Issue])

  var first: Issue { get }
  var remaining: [Issue] { get }
}

extension IssueAggregate {
  var issues: [Issue] { [first] + remaining }

  init(_ issue: Issue) {
    self.init(first: issue, remaining: [])
  }

  init?(collecting issues: [Issue]) {
    guard let first: Issue = issues.first else { return nil }
    self.init(first: first, remaining: Array(issues.dropFirst()))
  }

  func appending(_ other: Self) -> Self {
    Self(first: first, remaining: remaining + other.issues)
  }
}

func combine<First, Second, Failure: IssueAggregate>(
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

func combine<First, Second, Third, Failure: IssueAggregate>(
  _ first: Result<First, Failure>,
  _ second: Result<Second, Failure>,
  _ third: Result<Third, Failure>
) -> Result<(First, Second, Third), Failure> {
  combine(combine(first, second), third).map { pair in (pair.0.0, pair.0.1, pair.1) }
}

func traverse<Element, Value, Failure: IssueAggregate>(
  _ elements: some Sequence<Element>,
  _ transform: (Element) -> Result<Value, Failure>
) -> Result<[Value], Failure> {
  var values: [Value] = []
  var issues: [Failure.Issue] = []
  for element in elements {
    switch transform(element) {
    case .success(let value): values.append(value)
    case .failure(let failure): issues.append(contentsOf: failure.issues)
    }
  }
  return Failure(collecting: issues).map { failure in .failure(failure) } ?? .success(values)
}
