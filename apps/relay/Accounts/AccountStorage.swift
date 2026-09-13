import Foundation

nonisolated struct SavedAccount: Sendable {
  let account: Account
  let authentication: AuthenticationState
  let sessionStart: SessionStartState
  let usage: UsageSnapshot?
}

nonisolated struct SavedAccounts: Sendable {
  let accounts: [SavedAccount]
  let selected: [Provider: UUID]
}

nonisolated enum AccountQuota: Sendable {
  case session
  case weekly
  case fable

  var name: String {
    switch self {
    case .session: "Session"
    case .weekly: "Weekly"
    case .fable: "Fable"
    }
  }
}

nonisolated enum AccountStorageError: Error {
  case invalidAccount(id: UUID, error: IdentityError)
  case invalidUsage(id: UUID, quota: AccountQuota, failure: QuotaFailure)
  case invalidObservedAt(id: UUID)
  case duplicateAccount(id: UUID)
  case invalidSelection(provider: String, id: UUID)
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
          ? .success(Document(SavedAccounts(accounts: [], selected: [:])))
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
    let selected: [String: UUID]

    init(_ value: SavedAccounts) {
      records = value.accounts.map(Record.init)
      selected = Dictionary(
        uniqueKeysWithValues: value.selected.map { ($0.key.rawValue, $0.value) })
    }

    func savedAccounts() -> Result<SavedAccounts, AccountStorageFailure> {
      let checked: [Result<SavedAccount, AccountStorageErrors>] = records.map { record in
        record.savedAccount()
      }
      let identities: [AccountIdentity?] = checked.map { result in
        (try? result.get())?.account.identity
      }
      let accounts: Result<[SavedAccount], AccountStorageErrors> = traverse(checked) { $0 }
      let duplicates: [AccountStorageError] = records.indices
        .filter { index in isDuplicate(index, identities: identities) }
        .map { index in .duplicateAccount(id: records[index].id) }
      let uniqueness: Result<Void, AccountStorageErrors> =
        AccountStorageErrors(collecting: duplicates).map { errors in .failure(errors) }
        ?? .success(())
      let selections: Result<[(Provider, UUID)], AccountStorageErrors> = traverse(
        selected.sorted { $0.key < $1.key }
      ) { entry -> Result<(Provider, UUID), AccountStorageErrors> in
        guard let provider: Provider = Provider(rawValue: entry.key),
          records.contains(where: { $0.id == entry.value && $0.provider == provider })
        else {
          return .failure(
            AccountStorageErrors(.invalidSelection(provider: entry.key, id: entry.value)))
        }
        return .success((provider, entry.value))
      }
      return combine(accounts, uniqueness, selections)
        .map { accounts, _, selections in
          SavedAccounts(accounts: accounts, selected: Dictionary(uniqueKeysWithValues: selections))
        }
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
    let sessionStart: SessionStartState
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
      sessionStart = value.sessionStart
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
      let snapshot: Result<UsageSnapshot?, AccountStorageErrors> =
        usage.map { value in value.usageSnapshot(id: id).map(Optional.some) } ?? .success(nil)
      return combine(identity, snapshot).map { identity, snapshot in
        SavedAccount(
          account: Account(
            id: id, provider: provider, identity: identity, sessionPolicy: sessionPolicy),
          authentication: authentication,
          sessionStart: sessionStart,
          usage: snapshot
        )
      }
    }
  }

  private struct Window: Codable {
    let percent: Double
    let resetsAt: Date?

    init(_ value: QuotaWindow) {
      percent = value.amount.percent
      resetsAt = value.resetsAt
    }

    func quotaWindow(id: UUID, quota: AccountQuota) -> Result<QuotaWindow, AccountStorageErrors> {
      let amount: Result<UsageAmount, AccountStorageErrors> = UsageAmount.make(percent: percent)
        .mapError { failure in
          AccountStorageErrors(.invalidUsage(id: id, quota: quota, failure: failure))
        }
      let reset: Result<Date?, AccountStorageErrors> =
        switch resetsAt {
        case .none: .success(nil)
        case .some(let date) where date.timeIntervalSince1970.isFinite: .success(date)
        case .some:
          .failure(
            AccountStorageErrors(.invalidUsage(id: id, quota: quota, failure: .invalidResetDate)))
        }
      return combine(amount, reset).map { amount, date in
        QuotaWindow(amount: amount, resetsAt: date)
      }
    }
  }

  private struct Snapshot: Codable {
    let session: Window?
    let weekly: Window?
    let fable: Window?
    let observedAt: Date

    init(_ value: UsageSnapshot) {
      session = value.session.map(Window.init)
      weekly = value.weekly.map(Window.init)
      fable = value.fable.map(Window.init)
      observedAt = value.observedAt
    }

    func usageSnapshot(id: UUID) -> Result<UsageSnapshot, AccountStorageErrors> {
      let windows: Result<(QuotaWindow?, QuotaWindow?, QuotaWindow?), AccountStorageErrors> =
        combine(
          quotaWindow(session, quota: .session, id: id),
          quotaWindow(weekly, quota: .weekly, id: id),
          quotaWindow(fable, quota: .fable, id: id)
        )
      let observed: Result<Date, AccountStorageErrors> =
        observedAt.timeIntervalSince1970.isFinite
        ? .success(observedAt)
        : .failure(AccountStorageErrors(.invalidObservedAt(id: id)))
      return combine(windows, observed).map { windows, date in
        UsageSnapshot(session: windows.0, weekly: windows.1, fable: windows.2, observedAt: date)
      }
    }

    private func quotaWindow(
      _ window: Window?, quota: AccountQuota, id: UUID
    ) -> Result<QuotaWindow?, AccountStorageErrors> {
      window.map { value in value.quotaWindow(id: id, quota: quota).map(Optional.some) }
        ?? .success(nil)
    }
  }
}
