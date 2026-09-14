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

nonisolated enum IdentityError: Error {
  case missingAccountID
  case missingEmail
  case emptyOrganizationID
}

nonisolated struct IdentityFailure: AggregateError {
  let first: IdentityError
  let remaining: [IdentityError]
}

nonisolated struct AccountIdentity: Equatable, Sendable {
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

nonisolated enum AuthenticationState: Equatable, Codable, Sendable {
  case connected
  case signInRequired
}

nonisolated enum AccountOperation: Equatable, Sendable {
  case refreshing
  case selecting
  case starting
  case signingIn
  case signingOut
  case removing

  var description: String {
    switch self {
    case .refreshing: "Refreshing usage"
    case .selecting: "Switching account"
    case .starting: "Starting session"
    case .signingIn: "Signing in"
    case .signingOut: "Signing out"
    case .removing: "Removing account"
    }
  }
}

nonisolated enum AuthenticationPhase: Equatable, Sendable {
  case pending
  case completing
  case refused(String)
  case cancelling
}

nonisolated struct AuthenticationPresentation: Identifiable, Equatable, Sendable {
  let id: UUID
  let provider: Provider
  let phase: AuthenticationPhase
  let startedAt: Date

  func entering(_ phase: AuthenticationPhase) -> AuthenticationPresentation {
    AuthenticationPresentation(id: id, provider: provider, phase: phase, startedAt: startedAt)
  }
}

nonisolated protocol ProviderFailure: LocalizedError, Sendable {
  var requiresSignIn: Bool { get }
  var isCancellation: Bool { get }
}

nonisolated struct ProviderError: LocalizedError, Sendable {
  let failure: any ProviderFailure

  var requiresSignIn: Bool { failure.requiresSignIn }
  var isCancellation: Bool { failure.isCancellation }
  var errorDescription: String? { failure.errorDescription }
}
