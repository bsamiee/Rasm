import CasePaths
import DeveloperToolsSupport
import Foundation
import Processes
import Results
import os

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

    var tool: String {
        switch self {
            case .claude: "Claude Code"
            case .openAI: "Codex"
        }
    }

    @MainActor var symbol: ImageResource {
        switch self {
            case .claude: .claude
            case .openAI: .openAI
        }
    }
}

nonisolated enum AuthenticationState: Equatable, Codable, Sendable {
    case connected
    case signInRequired
}

enum AccountOperation: Equatable, Sendable {
    case refreshing
    case selecting
    case signingIn
    case signingOut
    case removing

    var name: StaticString {
        switch self {
            case .refreshing: "Refreshing usage"
            case .selecting: "Switching account"
            case .signingIn: "Signing in"
            case .signingOut: "Signing out"
            case .removing: "Removing account"
        }
    }

    var level: OSLogType {
        switch self {
            case .refreshing: .debug
            case .selecting, .signingIn, .signingOut, .removing: .default
        }
    }

    var refusalDescription: String { "\(name), retry after it finishes" }
}

nonisolated protocol ProviderFailure: LocalizedError {
    static func process(_ failure: ProcessFailure) -> Self
    static func cancelled(_ error: CancellationError) -> Self

    var requiresSignIn: Bool { get }
    var isAccountMismatch: Bool { get }
    var isCancellation: Bool { get }
    var retryAfter: Date? { get }
}

nonisolated extension ProviderFailure {
    init(process error: any Error) {
        self =
            switch ProcessFailure(error) {
                case .cancelled(let error): .cancelled(error)
                case let failure: .process(failure)
            }
    }
}

nonisolated protocol ProviderClient: Actor {
    func currentSelection(selected: Account?) async -> Result<AccountIdentity?, ProviderError>
    func holdsCredential(for account: Account, isSelected: Bool) async -> Result<Bool, ProviderError>
    func select(_ account: Account, outgoing: Account?) async -> Result<AccountIdentity, ProviderError>
    func signIn(_ target: SignInTarget, codes: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError>
    func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError>
    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError>
    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError>
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

    static func make(accountID: String?, organizationID: String?, email: String?, plan: String?) -> Result<Self, AggregateError<IdentityError>> {
        func required(_ value: String?, _ field: any PartialKeyPath<Self> & Sendable) -> Result<String, AggregateError<IdentityError>> {
            value.map { value in value.trimmingCharacters(in: .whitespacesAndNewlines) }.flatMap { value in value.isEmpty ? nil : value }.map(Result.success)
                ?? .failure(AggregateError(first: IdentityError(field: field)))
        }
        return combine(
            required(accountID, \.accountID),
            required(email, \.email),
            organizationID.map { value in required(value, \.organizationID).map(Optional.some) } ?? .success(nil),
        ).map { id, email, organization in Self(accountID: id, organizationID: organization, email: email, plan: plan) }
    }

    func isSameAccount(as other: Self) -> Bool {
        accountID == other.accountID && organizationID == other.organizationID
    }
}

nonisolated struct Account: Identifiable, Sendable {
    let id: UUID
    let provider: Provider
    var identity: AccountIdentity
}

nonisolated enum SignInTarget: Sendable {
    case newAccount(UUID)
    case account(Account, isSelected: Bool)
}

enum AuthenticationRefusal: Sendable {
    case differentAccount(email: String)
    case alreadyConnected(email: String)
    case failure(ProviderError)
}

@CasePathable
enum AuthenticationPhase: Sendable {
    case pending
    case completing
    case refused(AuthenticationRefusal)
    case cancelling
}

struct AuthenticationPresentation: Identifiable, Sendable {
    let id: UUID
    let provider: Provider
    var phase: AuthenticationPhase
    let startedAt: Date
    let codes: AsyncStream<String>.Continuation
}

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated struct IdentityError: Error, CustomStringConvertible {
    let field: any PartialKeyPath<AccountIdentity> & Sendable

    var description: String { "Missing \(field)" }
}

nonisolated struct ProviderError: Error {
    let failure: any ProviderFailure
}
