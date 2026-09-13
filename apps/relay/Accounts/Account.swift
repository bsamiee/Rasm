import Foundation

enum RelayProvider: String, CaseIterable, Codable, Sendable {
  case claude
  case openAI

  var name: String {
    switch self {
    case .claude: "Claude"
    case .openAI: "OpenAI"
    }
  }
}

enum SessionPolicy: String, CaseIterable, Codable, Sendable {
  case manual
  case automatic
}

enum SessionStartState: String, Codable, Sendable {
  case idle
  case awaitingConfirmation
}

enum IdentityIssue: Equatable, Sendable {
  case missingAccountID
  case missingEmail
  case emptyOrganizationID
}

struct IdentityFailure: IssueAggregate, Equatable {
  let first: IdentityIssue
  let remaining: [IdentityIssue]
}

struct AccountIdentity: Equatable, Sendable {
  let accountID: String
  let organizationID: String?
  let email: String
  let plan: String?

  private init(accountID: String, organizationID: String?, email: String, plan: String?) {
    self.accountID = accountID
    self.organizationID = organizationID
    self.email = email
    self.plan = plan
  }

  static func make(
    accountID: String,
    organizationID: String?,
    email: String,
    plan: String?
  ) -> Result<AccountIdentity, IdentityFailure> {
    let canonicalID: String = accountID.trimmingCharacters(in: .whitespacesAndNewlines)
    let canonicalEmail: String = email.trimmingCharacters(in: .whitespacesAndNewlines)
    let canonicalOrganization: String? = organizationID.map { value in
      value.trimmingCharacters(in: .whitespacesAndNewlines)
    }
    let checkedID: Result<String, IdentityFailure> =
      canonicalID.isEmpty ? .failure(IdentityFailure(.missingAccountID)) : .success(canonicalID)
    let checkedEmail: Result<String, IdentityFailure> =
      canonicalEmail.isEmpty ? .failure(IdentityFailure(.missingEmail)) : .success(canonicalEmail)
    let checkedOrganization: Result<String?, IdentityFailure> =
      switch canonicalOrganization {
      case .none: .success(nil)
      case .some(let value) where value.isEmpty: .failure(IdentityFailure(.emptyOrganizationID))
      case .some(let value): .success(value)
      }
    return combine(checkedID, checkedEmail, checkedOrganization).map { id, email, organization in
      AccountIdentity(accountID: id, organizationID: organization, email: email, plan: plan)
    }
  }

  func identifies(_ other: AccountIdentity) -> Bool {
    accountID == other.accountID && organizationID == other.organizationID
  }
}

struct RelayAccount: Identifiable, Equatable, Sendable {
  let id: UUID
  let provider: RelayProvider
  var identity: AccountIdentity
  var sessionPolicy: SessionPolicy
}

struct AccountSelection: Sendable {
  let identity: AccountIdentity
  let preservedAccount: RelayAccount?
}

enum AuthenticationState: Equatable, Codable, Sendable {
  case connected
  case signInRequired
}

enum AccountOperation: Equatable, Sendable {
  case idle
  case refreshing
  case selecting
  case starting
  case awaitingWindow
  case signingIn
  case signingOut
  case removing
}

struct AccountPresentation: Identifiable, Sendable {
  let account: RelayAccount
  let authentication: AuthenticationState
  let usageState: UsageState
  let operation: AccountOperation
  let issue: String?
  let isSelected: Bool

  var id: UUID { account.id }
  var usage: UsageSnapshot? { usageState.snapshot }
  var isUsageCurrent: Bool {
    switch usageState {
    case .current: true
    case .unavailable, .stale: false
    }
  }
}

struct AuthenticationPresentation: Identifiable {
  let id: UUID
  let provider: RelayProvider
  let issue: String?
}
