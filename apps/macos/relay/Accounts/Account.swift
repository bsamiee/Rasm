import DeveloperToolsSupport
import Foundation

// --- [TYPES] ---------------------------------------------------------------------------

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

    var refusalDescription: String { "\(description), try again when it finishes" }
}

nonisolated enum RefreshTrigger: Equatable, Sendable {
    case background
    case revalidation
    case immediate
}

nonisolated protocol ProviderFailure: DeadlineFailure, LocalizedError {
    static func process(_ failure: ProcessFailure) -> Self

    var requiresSignIn: Bool { get }
    var isAccountMismatch: Bool { get }
    var isCancellation: Bool { get }
    var retryAfter: Date? { get }
}

nonisolated extension ProviderFailure {
    init(process failure: ProcessFailure) {
        self =
            switch failure {
                case .cancelled: .cancelled
                case .timedOut: .timedOut
                default: .process(failure)
            }
    }
}

nonisolated protocol ProviderClient: Actor {
    func currentSelection(known: [Account]) async -> Result<AccountIdentity?, ProviderError>
    func holdsCredential(for account: Account) async -> Result<Bool, ProviderError>
    func select(_ account: Account, outgoing: Account?) async -> Result<AccountIdentity, ProviderError>
    func connect(id: UUID, codes: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError>
    func reconnect(account: Account, isSelected: Bool, codes: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError>
    func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError>
    func remove(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError>
    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError>
    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError>
    func startSession(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError>
}

// --- [MODELS] --------------------------------------------------------------------------

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
        plan: String?,
    ) -> Result<Self, AggregateError<IdentityError>> {
        let canonicalID: String = accountID.trimmingCharacters(in: .whitespacesAndNewlines)
        let canonicalEmail: String = email.trimmingCharacters(in: .whitespacesAndNewlines)
        let checkedID: Result<String, AggregateError<IdentityError>> =
            canonicalID.isEmpty ? .failure(AggregateError(first: .missingAccountID, remaining: [])) : .success(canonicalID)
        let checkedEmail: Result<String, AggregateError<IdentityError>> =
            canonicalEmail.isEmpty ? .failure(AggregateError(first: .missingEmail, remaining: [])) : .success(canonicalEmail)
        let checkedOrganization: Result<String?, AggregateError<IdentityError>> =
            switch organizationID?.trimmingCharacters(in: .whitespacesAndNewlines) {
                case .none: .success(nil)
                case .some(let value) where value.isEmpty: .failure(AggregateError(first: .emptyOrganizationID, remaining: []))
                case .some(let value): .success(value)
            }
        return combine(checkedID, checkedEmail, checkedOrganization).map { id, email, organization in
            Self(accountID: id, organizationID: organization, email: email, plan: plan)
        }
    }

    func isSameAccount(as other: Self) -> Bool {
        accountID == other.accountID && organizationID == other.organizationID
    }
}

nonisolated struct Account: Identifiable, Sendable {
    let id: UUID
    let provider: Provider
    var identity: AccountIdentity
    var sessionPolicy: SessionPolicy
}

nonisolated enum AuthenticationRefusal: Sendable {
    case differentAccount(email: String)
    case alreadyConnected(email: String)
    case failure(ProviderError)

    var description: String {
        switch self {
            case .differentAccount(let email): "Signed in to a different account than \(email)"
            case .alreadyConnected(let email): "\(email) is already connected"
            case .failure(let error): error.failure.localizedDescription
        }
    }
}

nonisolated enum AuthenticationPhase: Sendable {
    case pending
    case completing
    case refused(AuthenticationRefusal)
    case cancelling

    var description: String {
        switch self {
            case .pending: "Waiting for browser sign-in"
            case .completing: "Completing sign-in"
            case .refused(let refusal): refusal.description
            case .cancelling: "Canceling sign-in"
        }
    }
}

nonisolated struct AuthenticationPresentation: Identifiable, Sendable {
    let id: UUID
    let provider: Provider
    var phase: AuthenticationPhase
    let startedAt: Date
}

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum IdentityError: Error {
    case missingAccountID
    case missingEmail
    case emptyOrganizationID
}

nonisolated struct ProviderError: Error {
    let failure: any ProviderFailure
}
