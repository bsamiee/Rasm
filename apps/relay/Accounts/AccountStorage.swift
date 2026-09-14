import Foundation

nonisolated struct SavedAccount: Sendable {
  let account: Account
  let authentication: AuthenticationState
  let usage: AccountUsage?
}

nonisolated struct SavedAccounts: Sendable {
  let accounts: [SavedAccount]
}

nonisolated enum AccountStorageError: Error {
  case invalidAccount(id: UUID, error: IdentityError)
  case invalidUsage(id: UUID, kind: String, failure: QuotaFailure)
  case invalidWindowKind(id: UUID, kind: String)
  case invalidObservedAt(id: UUID)
  case duplicateAccount(id: UUID)
}

nonisolated struct AccountStorageErrors: AggregateError {
  let first: AccountStorageError
  let remaining: [AccountStorageError]
}

nonisolated enum AccountStorageFailure: LocalizedError {
  case read(any Error)
  case write(any Error)
  case unavailable
  case invalidData(AccountStorageErrors)

  var errorDescription: String? {
    switch self {
    case .read: "Account settings could not be read"
    case .write, .unavailable: "Account settings could not be saved"
    case .invalidData: "Account settings contain invalid data"
    }
  }
}

actor AccountStorage {
  private let fileURL: URL

  init(fileURL: URL) {
    self.fileURL = fileURL
  }

  func load() -> Result<SavedAccounts, AccountStorageFailure> {
    let decoder: JSONDecoder = JSONDecoder()
    decoder.dateDecodingStrategy = .iso8601
    return Result { try decoder.decode(Document.self, from: Data(contentsOf: fileURL)) }
      .flatMapError { error -> Result<Document, any Error> in
        (error as? CocoaError)?.code == .fileReadNoSuchFile
          ? .success(Document(SavedAccounts(accounts: [])))
          : .failure(error)
      }
      .mapError(AccountStorageFailure.read)
      .flatMap { document in document.savedAccounts() }
  }

  func save(_ value: SavedAccounts) -> Result<Void, AccountStorageFailure> {
    let encoder: JSONEncoder = JSONEncoder()
    encoder.dateEncodingStrategy = .iso8601
    encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
    return Result {
      try FileManager.default.createDirectory(
        at: fileURL.deletingLastPathComponent(), withIntermediateDirectories: true)
      try encoder.encode(Document(value)).write(to: fileURL, options: .atomic)
    }
    .mapError(AccountStorageFailure.write)
  }

  private struct Document: Codable {
    let records: [Record]

    init(_ value: SavedAccounts) {
      records = value.accounts.map(Record.init)
    }

    func savedAccounts() -> Result<SavedAccounts, AccountStorageFailure> {
      let checked: [Result<SavedAccount, AccountStorageErrors>] = records.map { record in
        record.savedAccount()
      }
      let identities: [AccountIdentity?] = checked.map { result in
        if case .success(let saved) = result { saved.account.identity } else { nil }
      }
      let accounts: Result<[SavedAccount], AccountStorageErrors> = traverse(checked) { $0 }
      let duplicates: [AccountStorageError] = records.indices
        .filter { index in isDuplicate(index, identities: identities) }
        .map { index in .duplicateAccount(id: records[index].id) }
      let uniqueness: Result<Void, AccountStorageErrors> =
        AccountStorageErrors(collecting: duplicates).map { errors in .failure(errors) }
        ?? .success(())
      return combine(accounts, uniqueness)
        .map { accounts, _ in SavedAccounts(accounts: accounts) }
        .mapError(AccountStorageFailure.invalidData)
    }

    private func isDuplicate(_ index: Int, identities: [AccountIdentity?]) -> Bool {
      let record: Record = records[index]
      let repeatedID: Bool = records[..<index].contains { earlier in earlier.id == record.id }
      let repeatedIdentity: Bool =
        identities[index].map { identity in
          zip(records[..<index], identities[..<index]).contains { earlier, candidate in
            earlier.provider == record.provider && candidate?.isSameAccount(as: identity) == true
          }
        } ?? false
      return repeatedID || repeatedIdentity
    }
  }

  private struct Record: Codable {
    let id: UUID
    let provider: Provider
    let accountID: String
    let organizationID: String?
    let email: String
    let plan: String?
    let sessionPolicy: SessionPolicy
    let authentication: AuthenticationState
    let usage: Snapshot?

    init(_ value: SavedAccount) {
      id = value.account.id
      provider = value.account.provider
      accountID = value.account.identity.accountID
      organizationID = value.account.identity.organizationID
      email = value.account.identity.email
      plan = value.account.identity.plan
      sessionPolicy = value.account.sessionPolicy
      authentication = value.authentication
      usage = value.usage.map(Snapshot.init)
    }

    func savedAccount() -> Result<SavedAccount, AccountStorageErrors> {
      let identity: Result<AccountIdentity, AccountStorageErrors> = AccountIdentity.make(
        accountID: accountID, organizationID: organizationID, email: email, plan: plan
      ).mapError { failure in
        AccountStorageErrors(
          first: .invalidAccount(id: id, error: failure.first),
          remaining: failure.remaining.map { error in .invalidAccount(id: id, error: error) }
        )
      }
      let snapshot: Result<AccountUsage?, AccountStorageErrors> =
        usage.map { value in value.accountUsage(id: id).map(Optional.some) } ?? .success(nil)
      return combine(identity, snapshot).map { identity, snapshot in
        SavedAccount(
          account: Account(
            id: id, provider: provider, identity: identity, sessionPolicy: sessionPolicy),
          authentication: authentication,
          usage: snapshot
        )
      }
    }
  }

  private struct Window: Codable {
    let kind: String
    let percent: Double
    let resetsAt: Date?
    let rejected: Bool

    init(_ value: QuotaWindow) {
      kind =
        switch value.kind {
        case .session: "session"
        case .weekly: "weekly"
        case .model(let name): "model:\(name)"
        }
      percent = value.used.percent
      resetsAt = value.resetsAt
      rejected = value.rejected
    }

    func quotaWindow(id: UUID) -> Result<QuotaWindow, AccountStorageErrors> {
      let quotaKind: Result<QuotaKind, AccountStorageErrors> =
        switch kind {
        case "session": .success(.session)
        case "weekly": .success(.weekly)
        case _ where kind.hasPrefix("model:") && kind.count > 6:
          .success(.model(String(kind.dropFirst(6))))
        default: .failure(AccountStorageErrors(.invalidWindowKind(id: id, kind: kind)))
        }
      let amount: Result<UsageAmount, AccountStorageErrors> = UsageAmount.make(percent: percent)
        .mapError { failure in
          AccountStorageErrors(.invalidUsage(id: id, kind: kind, failure: failure))
        }
      let reset: Result<Date?, AccountStorageErrors> =
        switch resetsAt {
        case .none: .success(nil)
        case .some(let date) where date.timeIntervalSince1970.isFinite: .success(date)
        case .some:
          .failure(
            AccountStorageErrors(.invalidUsage(id: id, kind: kind, failure: .invalidResetDate)))
        }
      return combine(quotaKind, amount, reset).map { quotaKind, amount, date in
        QuotaWindow(kind: quotaKind, used: amount, resetsAt: date, rejected: rejected)
      }
    }
  }

  private struct Snapshot: Codable {
    let windows: [Window]
    let includedUsageAllowed: Bool?
    let observedAt: Date
    let signInExpiresAt: Date?

    init(_ value: AccountUsage) {
      windows = value.windows.map(Window.init)
      includedUsageAllowed = value.includedUsageAllowed
      observedAt = value.observedAt
      signInExpiresAt = value.signInExpiresAt
    }

    func accountUsage(id: UUID) -> Result<AccountUsage, AccountStorageErrors> {
      let checked: Result<[QuotaWindow], AccountStorageErrors> = traverse(windows) { window in
        window.quotaWindow(id: id)
      }
      let observed: Result<Date, AccountStorageErrors> =
        observedAt.timeIntervalSince1970.isFinite
        ? .success(observedAt)
        : .failure(AccountStorageErrors(.invalidObservedAt(id: id)))
      return combine(checked, observed).map { windows, date in
        AccountUsage(
          windows: windows, includedUsageAllowed: includedUsageAllowed, observedAt: date,
          signInExpiresAt: signInExpiresAt)
      }
    }
  }
}
