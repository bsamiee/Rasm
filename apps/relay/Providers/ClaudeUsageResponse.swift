import Foundation

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudeUsageResponse: Decodable, Sendable {
    struct Window: Decodable, Sendable {
        let utilization: Double?
        let resetsAt: String?
        let status: String?

        func quotaWindow(kind: QuotaKind) -> Result<QuotaWindow?, AggregateError<QuotaFailure>> {
            utilization.map { utilization in
                combine(
                    UsageAmount.make(percent: utilization).mapError { failure in AggregateError(first: failure, remaining: []) },
                    ClaudeUsageResponse.resetDate(resetsAt),
                ).map { amount, reset in
                    QuotaWindow(kind: kind, used: amount, resetsAt: reset, rejected: status == "rejected")
                }
            } ?? .success(nil)
        }
    }

    struct Limit: Decodable, Sendable {
        struct Model: Decodable, Sendable {
            let displayName: String?
            let id: String?
        }

        struct Scope: Decodable, Sendable {
            let model: Model?
        }

        let kind: String
        let percent: Double?
        let utilization: Double?
        let resetsAt: String?
        let status: String?
        let scope: Scope?

        var modelName: String? {
            scope?.model.flatMap { model in model.displayName ?? model.id }
        }

        func quotaWindow() -> Result<QuotaWindow?, AggregateError<QuotaFailure>> {
            if kind == "weekly_scoped", let name: String = modelName {
                Window(utilization: percent ?? utilization, resetsAt: resetsAt, status: status)
                    .quotaWindow(kind: .model(name))
            } else {
                .success(nil)
            }
        }
    }

    let fiveHour: Window?
    let sevenDay: Window?
    let limits: [Limit]?

    func usage(observedAt: Date, signInExpiresAt: Date?) -> Result<AccountUsage, ClaudeFailure> {
        let session: Result<QuotaWindow?, AggregateError<QuotaFailure>> =
            fiveHour?.quotaWindow(kind: .session) ?? .success(nil)
        let weekly: Result<QuotaWindow?, AggregateError<QuotaFailure>> =
            sevenDay?.quotaWindow(kind: .weekly) ?? .success(nil)
        let models: Result<[QuotaWindow?], AggregateError<QuotaFailure>> = traverse(limits ?? []) { limit in
            limit.quotaWindow()
        }
        return combine(session, weekly, models).mapError(ClaudeFailure.invalidQuota).flatMap { session, weekly, models in
            let windows: [QuotaWindow] = [session, weekly].compactMap(\.self) + models.compactMap(\.self)
            return windows.isEmpty
                ? .failure(.invalidResponse)
                : .success(
                    AccountUsage(
                        windows: windows,
                        includedUsageAllowed: nil,
                        observedAt: observedAt,
                        signInExpiresAt: signInExpiresAt,
                    )
                )
        }
    }

    static func resetDate(_ value: String?) -> Result<Date?, AggregateError<QuotaFailure>> {
        value.map { value in
            Result { try Date(value, strategy: .iso8601) }
                .map(Optional.some)
                .mapError { _ in AggregateError(first: .invalidResetDate, remaining: []) }
        } ?? .success(nil)
    }
}
