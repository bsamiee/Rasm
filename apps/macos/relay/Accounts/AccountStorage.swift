import Files
import Foundation
import Results

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct SavedAccount: Sendable {
    let account: Account
    let authentication: AuthenticationState
    let usage: AccountUsage?
    let retryAfter: Date?
}

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum AccountStorageError: Error {
    case invalidAccount(id: UUID, error: IdentityError)
    case invalidUsage(id: UUID, kind: QuotaKind, failure: QuotaFailure)
}

nonisolated enum AccountStorageFailure: LocalizedError {
    case read(any Error)
    case write(any Error)
    case invalidData(AggregateError<AccountStorageError>)

    var errorDescription: String? {
        switch self {
            case .read(let error): "Could not read accounts.json. \(error.localizedDescription)"
            case .write(let error): "Could not save accounts.json. \(error.localizedDescription)"
            case .invalidData: "Invalid data in accounts.json"
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor AccountStorage {
    // --- [STATE]
    private let fileURL: URL

    init(fileURL: URL) {
        self.fileURL = fileURL
    }

    // --- [PERSISTENCE]
    func load(at now: Date) -> Result<[SavedAccount], AccountStorageFailure> {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        return ifPresent { try Data(contentsOf: fileURL) }
            .flatMap { data in Result { try data.map { data in try decoder.decode(Document.self, from: data) } } }
            .mapError(AccountStorageFailure.read)
            .flatMap { document in
                document?.savedAccounts(at: now) ?? .success([])
            }
    }

    func save(_ value: [SavedAccount]) -> Result<Void, AccountStorageFailure> {
        let encoder: JSONEncoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601
        encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
        return Result {
            try FileManager.default.createDirectory(
                at: fileURL.deletingLastPathComponent(),
                withIntermediateDirectories: true,
            )
            try encoder.encode(Document(value)).write(to: fileURL, options: .atomic)
        }
        .mapError(AccountStorageFailure.write)
    }

    // --- [DOCUMENT]
    private struct Document: Codable {
        let records: [Record]

        init(_ value: [SavedAccount]) {
            records = value.map(Record.init)
        }

        func savedAccounts(at now: Date) -> Result<[SavedAccount], AccountStorageFailure> {
            traverse(records) { record in record.savedAccount(at: now) }.mapError(AccountStorageFailure.invalidData)
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
        let usage: Snapshot?
        let retryAfter: Date?

        init(_ value: SavedAccount) {
            id = value.account.id
            provider = value.account.provider
            accountID = value.account.identity.accountID
            organizationID = value.account.identity.organizationID
            email = value.account.identity.email
            plan = value.account.identity.plan
            sessionPolicy = value.account.sessionPolicy
            authentication = value.authentication
            usage = value.usage.map(Snapshot.init)
            retryAfter = value.retryAfter
        }

        func savedAccount(at now: Date) -> Result<SavedAccount, AggregateError<AccountStorageError>> {
            let identity: Result<AccountIdentity, AggregateError<AccountStorageError>> = AccountIdentity.make(
                accountID: accountID,
                organizationID: organizationID,
                email: email,
                plan: plan,
            ).mapError { failure in failure.map { error in .invalidAccount(id: id, error: error) } }
            let snapshot: Result<AccountUsage?, AggregateError<AccountStorageError>> =
                usage?.accountUsage(id: id).map(Optional.some) ?? .success(nil)
            return combine(identity, snapshot).map { identity, snapshot in
                SavedAccount(
                    account: Account(
                        id: id,
                        provider: provider,
                        identity: identity,
                        sessionPolicy: sessionPolicy,
                    ),
                    authentication: authentication,
                    usage: snapshot,
                    retryAfter: retryAfter.flatMap { date in date > now ? date : nil },
                )
            }
        }
    }

    private struct Window: Codable {
        let kind: QuotaKind
        let percent: Double
        let resetsAt: Date?
        let rejected: Bool

        init(_ value: QuotaWindow) {
            kind = value.kind
            percent = value.used.percent
            resetsAt = value.resetsAt
            rejected = value.rejected
        }

        func quotaWindow(id: UUID) -> Result<QuotaWindow, AggregateError<AccountStorageError>> {
            UsageAmount.make(percent: percent)
                .aggregated { failure in .invalidUsage(id: id, kind: kind, failure: failure) }
                .map { amount in QuotaWindow(kind: kind, used: amount, resetsAt: resetsAt, rejected: rejected) }
        }
    }

    private struct Snapshot: Codable {
        let windows: [Window]
        let includedUsageAllowed: Bool?
        let observedAt: Date
        let signInExpiresAt: Date?

        init(_ value: AccountUsage) {
            windows = value.windows.map(Window.init)
            includedUsageAllowed = value.includedUsageAllowed
            observedAt = value.observedAt
            signInExpiresAt = value.signInExpiresAt
        }

        func accountUsage(id: UUID) -> Result<AccountUsage, AggregateError<AccountStorageError>> {
            traverse(windows) { window in window.quotaWindow(id: id) }.map { windows in
                AccountUsage(
                    windows: windows,
                    includedUsageAllowed: includedUsageAllowed,
                    observedAt: observedAt,
                    signInExpiresAt: signInExpiresAt,
                )
            }
        }
    }
}
