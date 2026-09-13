import Darwin
import Foundation

struct ClaudeStoredAccount: Codable, Sendable {
  let id: UUID
  let accountID: String
  let organizationID: String?
  let email: String
  let plan: String?
  let sessionPolicy: SessionPolicy

  init(_ account: RelayAccount) {
    id = account.id
    accountID = account.identity.accountID
    organizationID = account.identity.organizationID
    email = account.identity.email
    plan = account.identity.plan
    sessionPolicy = account.sessionPolicy
  }

  func account() -> Result<RelayAccount, IdentityFailure> {
    AccountIdentity.make(
      accountID: accountID, organizationID: organizationID, email: email, plan: plan
    ).map { identity in
      RelayAccount(id: id, provider: .claude, identity: identity, sessionPolicy: sessionPolicy)
    }
  }

  func identifies(_ identity: AccountIdentity) -> Bool {
    accountID == identity.accountID && organizationID == identity.organizationID
  }
}

struct ClaudeChildIdentity: Codable, Equatable, Sendable {
  let processID: Int32
  let startedSeconds: UInt64
  let startedMicroseconds: UInt64

  static func read(processID: Int32) -> Result<ClaudeChildIdentity?, ClaudeFailure> {
    var information: proc_bsdinfo = proc_bsdinfo()
    let size: Int32 = Int32(MemoryLayout<proc_bsdinfo>.size)
    let read: Int32 = proc_pidinfo(processID, PROC_PIDTBSDINFO, 0, &information, size)
    guard read == size else {
      if read == 0, errno == ESRCH { return .success(nil) }
      return .failure(.systemCall(operation: "proc_pidinfo", code: errno))
    }
    return .success(
      ClaudeChildIdentity(
        processID: processID,
        startedSeconds: information.pbi_start_tvsec,
        startedMicroseconds: information.pbi_start_tvusec
      ))
  }

  func isRunning() -> Result<Bool, ClaudeFailure> {
    Self.read(processID: processID).map { current in current == self }
  }
}

struct ClaudePendingSelection: Codable, Sendable {
  enum Phase: String, Codable, Sendable {
    case prepared
    case importing
  }

  let incoming: ClaudeStoredAccount
  let outgoing: ClaudeStoredAccount?
  var phase: Phase
  var child: ClaudeChildIdentity?
}

struct ClaudeSelectionJournal: Codable, Sendable {
  var selectedAccountID: UUID?
  var unavailableAccountIDs: Set<UUID>
  var preservedAccounts: [ClaudeStoredAccount]
  var pending: ClaudePendingSelection?

  static func empty() -> ClaudeSelectionJournal {
    ClaudeSelectionJournal(
      selectedAccountID: nil, unavailableAccountIDs: [], preservedAccounts: [], pending: nil
    )
  }

  static func read(at url: URL) -> Result<ClaudeSelectionJournal, ClaudeFailure> {
    do {
      let data: Data = try Data(contentsOf: url)
      return .success(try JSONDecoder().decode(ClaudeSelectionJournal.self, from: data))
    } catch let error as CocoaError where error.code == .fileReadNoSuchFile {
      return .success(.empty())
    } catch {
      return .failure(.filesystem(error))
    }
  }

  func write(to url: URL) -> Result<Void, ClaudeFailure> {
    do {
      try FileManager.default.createDirectory(
        at: url.deletingLastPathComponent(), withIntermediateDirectories: true)
      let data: Data = try JSONEncoder().encode(self)
      try data.write(to: url, options: .atomic)
      return .success(())
    } catch {
      return .failure(.filesystem(error))
    }
  }
}
