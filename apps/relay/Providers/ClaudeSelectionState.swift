import Darwin
import Foundation

nonisolated struct ClaudeStoredAccount: Codable, Sendable {
  let id: UUID
  let accountID: String
  let organizationID: String?
  let email: String
  let plan: String?
  let sessionPolicy: SessionPolicy

  init(_ account: Account) {
    id = account.id
    accountID = account.identity.accountID
    organizationID = account.identity.organizationID
    email = account.identity.email
    plan = account.identity.plan
    sessionPolicy = account.sessionPolicy
  }

  func account() -> Result<Account, IdentityFailure> {
    AccountIdentity.make(
      accountID: accountID, organizationID: organizationID, email: email, plan: plan
    ).map { identity in
      Account(id: id, provider: .claude, identity: identity, sessionPolicy: sessionPolicy)
    }
  }

  func isSameAccount(as identity: AccountIdentity) -> Bool {
    accountID == identity.accountID && organizationID == identity.organizationID
  }
}

nonisolated struct ClaudeChildProcess: Codable, Equatable, Sendable {
  let processID: Int32
  let startedSeconds: UInt64
  let startedMicroseconds: UInt64

  static func read(processID: Int32) -> Result<ClaudeChildProcess?, ClaudeFailure> {
    var information: proc_bsdinfo = proc_bsdinfo()
    let size: Int32 = Int32(MemoryLayout<proc_bsdinfo>.size)
    let read: Int32 = proc_pidinfo(processID, PROC_PIDTBSDINFO, 0, &information, size)
    guard read == size else {
      if read == 0, errno == ESRCH { return .success(nil) }
      return .failure(.systemCall(.procPIDInfo, errno))
    }
    return .success(
      ClaudeChildProcess(
        processID: processID,
        startedSeconds: information.pbi_start_tvsec,
        startedMicroseconds: information.pbi_start_tvusec
      ))
  }

  func isRunning() -> Result<Bool, ClaudeFailure> {
    Self.read(processID: processID).map { current in current == self }
  }
}

nonisolated struct ClaudePendingSelection: Codable, Sendable {
  enum Phase: String, Codable, Sendable {
    case prepared
    case importing
  }

  let incoming: ClaudeStoredAccount
  let outgoing: ClaudeStoredAccount?
  var phase: Phase
  var child: ClaudeChildProcess?
}

nonisolated struct ClaudeSelectionState: Codable, Sendable {
  var selectedAccountID: UUID?
  var unavailableAccountIDs: Set<UUID> = []
  var preservedAccounts: [ClaudeStoredAccount] = []
  var pending: ClaudePendingSelection?

  func preservedAccount(for identity: AccountIdentity) -> ClaudeStoredAccount? {
    preservedAccounts.first { record in record.isSameAccount(as: identity) }
  }

  mutating func preserve(_ record: ClaudeStoredAccount) {
    preservedAccounts.removeAll { existing in existing.id == record.id }
    preservedAccounts.append(record)
  }

  static func read(at url: URL) -> Result<ClaudeSelectionState, ClaudeFailure> {
    Result { try Data(contentsOf: url) }.map(Optional.some)
      .flatMapError { error -> Result<Data?, ClaudeFailure> in
        (error as? CocoaError)?.code == .fileReadNoSuchFile
          ? .success(nil) : .failure(.filesystem(error))
      }
      .flatMap { data in
        data.map { data in
          Result { try JSONDecoder().decode(Self.self, from: data) }
            .mapError(ClaudeFailure.filesystem)
        } ?? .success(ClaudeSelectionState())
      }
  }

  func write(to url: URL) -> Result<Void, ClaudeFailure> {
    Result {
      try FileManager.default.createDirectory(
        at: url.deletingLastPathComponent(), withIntermediateDirectories: true)
      try JSONEncoder().encode(self).write(to: url, options: .atomic)
    }
    .mapError(ClaudeFailure.filesystem)
  }
}
