import Foundation

struct SavedAccount: Sendable {
  let account: RelayAccount
  let authentication: AuthenticationState
  let sessionStart: SessionStartState
  let usage: UsageSnapshot?
}

struct SavedAccounts: Sendable {
  let accounts: [SavedAccount]
  let selected: [RelayProvider: UUID]
}

enum AccountQuota: Sendable {
  case session
  case weekly
  case fable
}

enum AccountStorageIssue: Sendable {
  case invalidAccount(id: UUID, issue: IdentityIssue)
  case invalidUsage(id: UUID, quota: AccountQuota, failure: QuotaFailure)
  case invalidObservedAt(id: UUID)
  case duplicateAccount(id: UUID)
  case invalidSelection(provider: String, id: UUID)
}

struct AccountStorageIssues: IssueAggregate {
  let first: AccountStorageIssue
  let remaining: [AccountStorageIssue]
}

enum AccountStorageFailure: Error {
  case read(any Error)
  case write(any Error)
  case unavailable
  case invalidData(AccountStorageIssues)

  var userMessage: String {
    switch self {
    case .read: "Account settings could not be read"
    case .write, .unavailable: "Account settings could not be saved"
    case .invalidData: "Account settings contain invalid data"
    }
  }
}

actor AccountRepository {
  private let fileURL: URL

  init(fileURL: URL) {
    self.fileURL = fileURL
  }

  func load() -> Result<SavedAccounts, AccountStorageFailure> {
    do {
      let data: Data = try Data(contentsOf: fileURL)
      let decoder: JSONDecoder = JSONDecoder()
      decoder.dateDecodingStrategy = .iso8601
      return try decoder.decode(Document.self, from: data).savedAccounts()
    } catch let error as CocoaError where error.code == .fileReadNoSuchFile {
      return .success(SavedAccounts(accounts: [], selected: [:]))
    } catch {
      return .failure(.read(error))
    }
  }

  // Synchronous body: the actor orders saves in call order
  func save(_ value: SavedAccounts) -> Result<Void, AccountStorageFailure> {
    do {
      try FileManager.default.createDirectory(
        at: fileURL.deletingLastPathComponent(), withIntermediateDirectories: true)
      let encoder: JSONEncoder = JSONEncoder()
      encoder.dateEncodingStrategy = .iso8601
      encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
      try encoder.encode(Document(value)).write(to: fileURL, options: .atomic)
      return .success(())
    } catch {
      return .failure(.write(error))
    }
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
      let checked: [Result<SavedAccount, AccountStorageIssues>] = records.map { record in
        record.savedAccount()
      }
      let identities: [AccountIdentity?] = checked.map { result in
        switch result {
        case .success(let saved): saved.account.identity
        case .failure: nil
        }
      }
      let accounts: Result<[SavedAccount], AccountStorageIssues> = traverse(checked) { $0 }
      let duplicates: [AccountStorageIssue] = records.indices
        .filter { index in isDuplicate(index, identities: identities) }
        .map { index in .duplicateAccount(id: records[index].id) }
      let uniqueness: Result<Void, AccountStorageIssues> =
        AccountStorageIssues(collecting: duplicates).map { issues in .failure(issues) }
        ?? .success(())
      let selections: Result<[(RelayProvider, UUID)], AccountStorageIssues> = traverse(
        selected.sorted { $0.key < $1.key }
      ) { entry -> Result<(RelayProvider, UUID), AccountStorageIssues> in
        guard let provider: RelayProvider = RelayProvider(rawValue: entry.key),
          records.contains(where: { $0.id == entry.value && $0.provider == provider })
        else {
          return .failure(
            AccountStorageIssues(.invalidSelection(provider: entry.key, id: entry.value)))
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
            earlier.provider == record.provider && candidate?.identifies(identity) == true
          }
        } ?? false
      return repeatedID || repeatedIdentity
    }
  }

  private struct Record: Codable {
    let id: UUID
    let provider: RelayProvider
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

    func savedAccount() -> Result<SavedAccount, AccountStorageIssues> {
      let identity: Result<AccountIdentity, AccountStorageIssues> = AccountIdentity.make(
        accountID: accountID, organizationID: organizationID, email: email, plan: plan
      ).mapError { failure in
        AccountStorageIssues(
          first: .invalidAccount(id: id, issue: failure.first),
          remaining: failure.remaining.map { issue in .invalidAccount(id: id, issue: issue) }
        )
      }
      let snapshot: Result<UsageSnapshot?, AccountStorageIssues> =
        usage.map { value in value.usageSnapshot(id: id).map(Optional.some) } ?? .success(nil)
      return combine(identity, snapshot).map { identity, snapshot in
        SavedAccount(
          account: RelayAccount(
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

    func quotaWindow(id: UUID, quota: AccountQuota) -> Result<QuotaWindow, AccountStorageIssues> {
      let amount: Result<UsageAmount, AccountStorageIssues> = UsageAmount.make(percent: percent)
        .mapError { failure in
          AccountStorageIssues(.invalidUsage(id: id, quota: quota, failure: failure))
        }
      let reset: Result<Date?, AccountStorageIssues> =
        switch resetsAt {
        case .none: .success(nil)
        case .some(let date) where date.timeIntervalSince1970.isFinite: .success(date)
        case .some:
          .failure(
            AccountStorageIssues(.invalidUsage(id: id, quota: quota, failure: .invalidResetDate)))
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

    func usageSnapshot(id: UUID) -> Result<UsageSnapshot, AccountStorageIssues> {
      let windows: Result<(QuotaWindow?, QuotaWindow?, QuotaWindow?), AccountStorageIssues> =
        combine(
          quotaWindow(session, quota: .session, id: id),
          quotaWindow(weekly, quota: .weekly, id: id),
          quotaWindow(fable, quota: .fable, id: id)
        )
      let observed: Result<Date, AccountStorageIssues> =
        observedAt.timeIntervalSince1970.isFinite
        ? .success(observedAt)
        : .failure(AccountStorageIssues(.invalidObservedAt(id: id)))
      return combine(windows, observed).map { windows, date in
        UsageSnapshot(session: windows.0, weekly: windows.1, fable: windows.2, observedAt: date)
      }
    }

    private func quotaWindow(
      _ window: Window?, quota: AccountQuota, id: UUID
    ) -> Result<QuotaWindow?, AccountStorageIssues> {
      window.map { value in value.quotaWindow(id: id, quota: quota).map(Optional.some) }
        ?? .success(nil)
    }
  }
}
