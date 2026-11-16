import CasePaths
import Files
import Foundation
import JSONRPC
import Logging
import Processes
import Results
import Security
import Subprocess
import Telemetry

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

    func metadata(_ id: UUID, _ fields: Logger.Metadata = [:]) -> Logger.Metadata {
        fields.merging(["provider": "\(name, privacy: .public)", "account": "\(id, privacy: .public)"]) { stated, _ in stated }
    }
}

nonisolated enum AuthenticationState: Codable, Sendable {
    case connected
    case signInRequired
}

enum AccountOperation: Sendable {
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

    var level: Logger.Level {
        switch self {
            case .refreshing: .debug
            case .selecting, .signingIn, .signingOut, .removing: .notice
        }
    }
}

nonisolated protocol ProviderClient: Actor {
    func currentSelection(selected: Account?) async throws(AccountFailure) -> AccountIdentity?
    func holdsCredential(for account: Account, isSelected: Bool) async throws(AccountFailure) -> Bool
    func select(_ account: Account, outgoing: Account?) async throws(AccountFailure) -> AccountIdentity
    func signIn(_ target: SignInTarget, codes: AsyncStream<String>) async throws(AccountFailure) -> AccountIdentity
    func signOut(_ account: Account, isSelected: Bool) async throws(AccountFailure)
    func deletePrivateStore(id: UUID) async throws(AccountFailure)
    func usage(for account: Account, isSelected: Bool) async throws(AccountFailure) -> AccountUsage
}

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct AccountIdentity: Codable, Equatable, Sendable {
    let accountID: String
    let organizationID: String?
    let email: String
    let plan: String?

    init(accountID: String?, organizationID: String?, email: String?, plan: String?) throws(AccountFailure) {
        func required(_ value: String?, _ field: CodingKeys) -> Result<String, AggregateError<AccountFailure>> {
            value.map { value in value.trimmingCharacters(in: .whitespacesAndNewlines) }.flatMap { value in value.isEmpty ? nil : value }.map(Result.success)
                ?? .failure(AggregateError(first: AccountFailure(subject: field.stringValue, reason: .missing)))
        }
        (self.accountID, self.email, self.organizationID) = try combine(
            required(accountID, .accountID),
            required(email, .email),
            organizationID.map { value in required(value, .organizationID).map(Optional.some) } ?? .success(nil),
        )
        .mapError { causes in AccountFailure(subject: "Account identity", reason: .invalid(causes)) }
        .get()
        self.plan = plan
    }

    init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        try self.init(
            accountID: fields.decode(String.self, forKey: .accountID),
            organizationID: fields.decodeIfPresent(String.self, forKey: .organizationID),
            email: fields.decode(String.self, forKey: .email),
            plan: fields.decodeIfPresent(String.self, forKey: .plan),
        )
    }

    func isSameAccount(as other: Self) -> Bool {
        accountID == other.accountID && organizationID == other.organizationID
    }
}

nonisolated struct Account: Codable, Identifiable, Sendable {
    private enum CodingKeys: CodingKey {
        case id, provider
    }

    let id: UUID
    let provider: Provider
    var identity: AccountIdentity

    init(id: UUID, provider: Provider, identity: AccountIdentity) {
        (self.id, self.provider, self.identity) = (id, provider, identity)
    }

    init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        try self.init(id: fields.decode(UUID.self, forKey: .id), provider: fields.decode(Provider.self, forKey: .provider), identity: AccountIdentity(from: decoder))
    }

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(id, forKey: .id)
        try fields.encode(provider, forKey: .provider)
        try identity.encode(to: encoder)
    }

    func metadata(_ fields: Logger.Metadata = [:]) -> Logger.Metadata {
        provider.metadata(id, fields.merging(["email": "\(identity.email, privacy: .private)"]) { stated, _ in stated })
    }
}

nonisolated enum SignInTarget: Sendable {
    case newAccount(UUID)
    case account(Account, isSelected: Bool)
}

enum AuthenticationRefusal: Sendable {
    case differentAccount(email: String)
    case alreadyConnected(email: String)
    case failure(AccountFailure)
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

nonisolated struct AccountFailure: LocalizedError, CustomNSError, LockFailure {
    @CasePathable
    enum Reason: Sendable {
        case missing
        case outOfRange
        indirect case invalid(AggregateError<AccountFailure>)
        case unreadable(any Error)
        case filesystem(any Error)
        case notInstalled
        case process(ProcessFailure)
        case rpc(JSONRPCFailure)
        case lock(FileFailure)
        case timedOut(Duration)
        case cancelled
        case interactionNotAllowed
        case signInRequired
        case signInPageUnopened
        case signInRefused(reason: String?)
        case accountOnHold(appeal: URL)
        case accountChanged
        case selectionChanged
        case usageUnavailable
        case keyringStorage
        case forcedWorkspace
        case desktopRunning
        indirect case cleanup(operation: AccountFailure, release: AccountFailure)
    }

    let subject: String
    let reason: Reason

    init(subject: String, reason: Reason) {
        (self.subject, self.reason) = (subject, reason)
    }

    init(_ provider: Provider, _ reason: Reason) {
        self.init(subject: provider.name, reason: reason)
    }

    init(_ provider: Provider, child failure: ProcessFailure) {
        let reason: Reason =
            switch failure.reason {
                case .cancelled: .cancelled
                case .subprocess(let error) where error.code == .executableNotFound: .notInstalled
                case .exit(_, let standardError) where standardError.contains("Your account is on hold"):
                    standardError.firstMatch(of: URL.ParseStrategy()).map { link in .accountOnHold(appeal: link.output) } ?? .process(failure)
                default: .process(failure)
            }
        self.init(subject: provider.tool, reason: reason)
    }

    static func lock(_ failure: FileFailure) -> Self { Self(subject: failure.subject, reason: .lock(failure)) }

    static func cancelled(_: CancellationError) -> Self { Self(subject: "Lock", reason: .cancelled) }

    func releasing(_ release: Result<Void, Self>) -> Self {
        switch release {
            case .success: self
            case .failure(let error): Self(subject: subject, reason: .cleanup(operation: self, release: error))
        }
    }

    var cause: Self { reason[case: \.cleanup]?.operation.cause ?? self }

    var requiresSignIn: Bool { cause.reason.is(\.signInRequired) || cause.reason.is(\.accountChanged) }

    var isAccountMismatch: Bool { cause.reason.is(\.accountChanged) }

    var isCancellation: Bool { cause.reason.is(\.cancelled) }

    var errorDescription: String? {
        switch reason {
            case .missing: "\(subject) missing"
            case .outOfRange: "\(subject) out of range"
            case .invalid(let causes): "\(subject) invalid: \(causes.errors.map(\.localizedDescription).joined(separator: ", "))"
            case .unreadable: "Unreadable \(subject)"
            case .filesystem(let error): "\(subject) access failed: \(error.localizedDescription)"
            case .notInstalled: "\(subject) not installed"
            case .process(let failure): failure.localizedDescription
            case .rpc(let failure): "\(subject) failed: \(failure.localizedDescription)"
            case .lock(let failure) where failure.reason.is(\.held): "Credential update in progress, retry after it finishes"
            case .lock(let failure): failure.localizedDescription
            case .timedOut(let timeout): "\(subject) timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "\(subject) canceled"
            case .interactionNotAllowed: SecCopyErrorMessageString(errSecInteractionNotAllowed, nil) as String?
            case .signInRequired: "\(subject) sign-in required"
            case .signInPageUnopened: "Could not open \(subject) sign-in page"
            case .signInRefused(let detail): ["\(subject) sign-in failed", detail].compactMap(\.self).joined(separator: ". ")
            case .accountOnHold(let appeal): "\(subject) account on hold, view details or appeal at \(appeal.absoluteString)"
            case .accountChanged: "Signed in to different \(subject) account"
            case .selectionChanged: "Active \(subject) account changed during switch"
            case .usageUnavailable: "\(subject) could not fetch usage"
            case .keyringStorage: "Set cli_auth_credentials_store = \"file\" to switch accounts"
            case .forcedWorkspace: "Unset forced_chatgpt_workspace_id to switch accounts"
            case .desktopRunning: "Quit \(subject) to switch accounts"
            case .cleanup(let operation, _): operation.localizedDescription
        }
    }

    var errorUserInfo: [String: Any] {
        switch reason {
            case .unreadable(let cause), .filesystem(let cause), .process(let cause as any Error), .rpc(let cause as any Error), .lock(let cause as any Error):
                [NSUnderlyingErrorKey: cause]
            case .invalid(let causes): [NSMultipleUnderlyingErrorsKey: causes.errors]
            case .cleanup(let operation, let release): [NSUnderlyingErrorKey: operation, NSMultipleUnderlyingErrorsKey: [release]]
            case .missing, .outOfRange, .notInstalled, .timedOut, .cancelled, .interactionNotAllowed, .signInRequired, .signInPageUnopened, .signInRefused, .accountOnHold,
                .accountChanged, .selectionChanged, .usageUnavailable, .keyringStorage, .forcedWorkspace, .desktopRunning:
                [:]
        }
    }
}
