import DeveloperToolsSupport
import Foundation

nonisolated enum Provider: String, CaseIterable, Codable, Sendable {
  case claude
  case openAI

  var name: String {
    switch self {
    case .claude: "Claude"
    case .openAI: "OpenAI"
    }
  }

  @MainActor var symbol: ImageResource {
    switch self {
    case .claude: .claude
    case .openAI: .openAI
    }
  }
}

nonisolated enum SessionPolicy: String, Codable, Sendable {
  case manual
  case automatic
}

nonisolated enum SessionStartState: String, Codable, Sendable {
  case idle
  case awaitingConfirmation
}

nonisolated enum IdentityError: Error {
  case missingAccountID
  case missingEmail
  case emptyOrganizationID
}

nonisolated struct IdentityFailure: AggregateError {
  let first: IdentityError
  let remaining: [IdentityError]
}

nonisolated struct AccountIdentity: Sendable {
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

  func isSameAccount(as other: AccountIdentity) -> Bool {
    accountID == other.accountID && organizationID == other.organizationID
  }
}

nonisolated struct Account: Identifiable, Sendable {
  let id: UUID
  let provider: Provider
  var identity: AccountIdentity
  var sessionPolicy: SessionPolicy
}

nonisolated struct AccountSelection: Sendable {
  let identity: AccountIdentity
  let preservedAccount: Account?
}

nonisolated enum AuthenticationState: Equatable, Codable, Sendable {
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

struct AccountViewModel: Identifiable, Sendable {
  let account: Account
  let authentication: AuthenticationState
  let usageState: UsageState
  let operation: AccountOperation
  let issue: String?
  let isSelected: Bool

  var id: UUID { account.id }
  var isBusy: Bool { operation != .idle && operation != .awaitingWindow }
  var canSelect: Bool { authentication == .connected && !isBusy }
  var canStartSession: Bool { authentication == .connected && operation == .idle }
  var isUsageCurrent: Bool { if case .current = usageState { true } else { false } }
}

enum AuthenticationPhase: Equatable, Sendable {
  case pending
  case refused(String)
  case cancelling
}

struct AuthenticationPresentation: Identifiable {
  let id: UUID
  let provider: Provider
  let phase: AuthenticationPhase
}
