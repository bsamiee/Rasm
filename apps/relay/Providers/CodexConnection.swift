import Foundation
import Subprocess
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

actor CodexConnection {
    // --- [STATE]
    private enum PendingReply: Sendable {
        case unclaimed(method: CodexProtocol.Method)
        case arrived(Result<Data, CodexFailure>)
        case awaited(method: CodexProtocol.Method, CheckedContinuation<Result<Data, CodexFailure>, Never>)
    }

    private struct NotificationWaiter: Sendable {
        let id: UUID
        let matches: @Sendable (Data) -> Bool
        let continuation: CheckedContinuation<Result<Data, CodexFailure>, Never>
    }

    private struct Registry: Sendable {
        var replies: [String: PendingReply] = [:]
        var waiters: [CodexProtocol.Method: [NotificationWaiter]] = [:]
        var terminalFailure: CodexFailure?
    }

    nonisolated let updates: AsyncStream<CodexProtocol.RateLimitsUpdated>
    private let updatesContinuation: AsyncStream<CodexProtocol.RateLimitsUpdated>.Continuation
    private let execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>
    private let registry: Mutex<Registry> = Mutex(Registry())
    private var nextRequestID: Int = 0
    private var serverVersion: String?
    private var notifications: [CodexProtocol.Method: [Data]] = [:]

    init(execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>) {
        self.execution = execution
        (updates, updatesContinuation) = AsyncStream<CodexProtocol.RateLimitsUpdated>.makeStream()
    }

    var isFinished: Bool { terminalFailure != nil }

    private var terminalFailure: CodexFailure? {
        registry.withLock(\.terminalFailure)
    }

    private var ready: Result<Void, CodexFailure> {
        terminalFailure.map(Result.failure)
            ?? (Task.isCancelled ? .failure(.cancelled) : .success(()))
    }

    // --- [REQUESTS]
    func initialize() async -> Result<Void, CodexFailure> {
        await request(
            CodexProtocol.InitializeResult.self,
            .initialize,
            params: CodexProtocol.InitializeParams(
                clientInfo: CodexProtocol.InitializeParams.ClientInfo(name: "relay", title: "Relay", version: "1.0"),
                capabilities: CodexProtocol.InitializeParams.Capabilities(experimentalApi: true),
            ),
        ).bind { result in
            serverVersion = result.userAgent.flatMap { agent in
                agent.firstMatch(of: /^[^\/]+\/(\S+)/).map { match in String(match.1) }
            }
            return await write(CodexProtocol.Notification(method: .initialized))
        }
    }

    func request<Value: Decodable & Sendable, Params: Encodable & Sendable>(
        _: Value.Type,
        _ method: CodexProtocol.Method,
        params: Params?,
    ) async -> Result<Value, CodexFailure> {
        await exchange(method, params: params).flatMap { line in
            Self.decode(CodexProtocol.ResultField<Value>.self, line).map { field in .success(field.result) }
                ?? .failure(.invalidResponse(field: "\(method.rawValue) response"))
        }
    }

    func request<Params: Encodable & Sendable>(
        _ method: CodexProtocol.Method,
        params: Params? = Never?.none,
    ) async -> Result<Void, CodexFailure> {
        await exchange(method, params: params).map { _ in () }
    }

    private func exchange<Params: Encodable & Sendable>(
        _ method: CodexProtocol.Method,
        params: Params?,
    ) async -> Result<Data, CodexFailure> {
        await ready.bind { _ in
            nextRequestID += 1
            let id: String = String(nextRequestID)
            registry.withLock { state in state.replies[id] = .unclaimed(method: method) }
            if case .failure(let error) = await write(CodexProtocol.Request(id: id, method: method, params: params)) {
                registry.withLock { state in _ = state.replies.removeValue(forKey: id) }
                return .failure(error)
            }
            return await reply(to: id)
        }
    }

    private func reply(to id: String) async -> Result<Data, CodexFailure> {
        await withTaskCancellationHandler {
            await withCheckedContinuation { continuation in
                let immediate: Result<Data, CodexFailure>? = registry.withLock { state in
                    Self.claim(id, for: continuation, in: &state)
                }
                if let immediate { continuation.resume(returning: immediate) }
            }
        } onCancel: {
            Self.cancelReply(id, in: self.registry)
        }
    }

    private static func claim(
        _ id: String,
        for continuation: CheckedContinuation<Result<Data, CodexFailure>, Never>,
        in state: inout Registry,
    ) -> Result<Data, CodexFailure>? {
        if let failure: CodexFailure = state.terminalFailure {
            state.replies.removeValue(forKey: id)
            return .failure(failure)
        }
        switch state.replies[id] {
            case .arrived(let reply):
                state.replies.removeValue(forKey: id)
                return reply
            case .unclaimed(let method) where !Task.isCancelled:
                state.replies[id] = .awaited(method: method, continuation)
                return nil
            case .unclaimed, .awaited, .none:
                state.replies.removeValue(forKey: id)
                return .failure(.cancelled)
        }
    }

    private nonisolated static func cancelReply(
        _ id: String,
        in registry: borrowing Mutex<Registry>,
    ) {
        let awaited: CheckedContinuation<Result<Data, CodexFailure>, Never>? = registry.withLock {
            state in
            if case .awaited(_, let continuation) = state.replies.removeValue(forKey: id) { continuation } else { nil }
        }
        awaited?.resume(returning: .failure(.cancelled))
    }

    private static func reply(
        error: CodexProtocol.ErrorObject?,
        result: Data?,
        method: CodexProtocol.Method,
        serverVersion: String?,
    ) -> Result<Data, CodexFailure> {
        switch (error, result) {
            case (.some(let error), _):
                .failure(.requestRejected(method: method, code: error.code, message: error.message, serverVersion: serverVersion))
            case (.none, .some(let result)): .success(result)
            case (.none, .none): .failure(.invalidResponse(field: "request result"))
        }
    }

    // --- [NOTIFICATIONS]
    func notification<Value: Decodable & Sendable>(
        _: Value.Type,
        _ method: CodexProtocol.Method,
        matching predicate: @escaping @Sendable (Value) -> Bool,
    ) async -> Result<Value, CodexFailure> {
        let params: @Sendable (Data) -> Value? = { line in
            Self.decode(CodexProtocol.ParamsField<Value>.self, line)?.params
        }
        let matches: @Sendable (Data) -> Bool = { line in params(line).map(predicate) ?? false }
        return await ready.bind { _ in
            if let index: Int = notifications[method]?.firstIndex(where: matches),
                let line: Data = notifications[method]?.remove(at: index)
            {
                .success(line)
            } else {
                await notification(method, matching: matches, waiter: UUID())
            }
        }
        .flatMap { line in
            params(line).map(Result.success) ?? .failure(.invalidResponse(field: "\(method.rawValue) notification"))
        }
    }

    private func notification(
        _ method: CodexProtocol.Method,
        matching predicate: @escaping @Sendable (Data) -> Bool,
        waiter id: UUID,
    ) async -> Result<Data, CodexFailure> {
        await withTaskCancellationHandler {
            await withCheckedContinuation { continuation in
                let immediate: Result<Data, CodexFailure>? = registry.withLock { state in
                    Self.register(
                        NotificationWaiter(id: id, matches: predicate, continuation: continuation),
                        for: method,
                        in: &state,
                    )
                }
                if let immediate { continuation.resume(returning: immediate) }
            }
        } onCancel: {
            Self.cancelWaiter(id, method: method, in: self.registry)
        }
    }

    private static func register(
        _ waiter: NotificationWaiter,
        for method: CodexProtocol.Method,
        in state: inout Registry,
    ) -> Result<Data, CodexFailure>? {
        if let failure: CodexFailure = state.terminalFailure { return .failure(failure) }
        guard !Task.isCancelled else { return .failure(.cancelled) }
        state.waiters[method, default: []].append(waiter)
        return nil
    }

    private nonisolated static func cancelWaiter(
        _ id: UUID,
        method: CodexProtocol.Method,
        in registry: borrowing Mutex<Registry>,
    ) {
        let waiter: NotificationWaiter? = registry.withLock { state in
            state.waiters[method]?.firstIndex(where: { waiter in waiter.id == id })
                .flatMap { index in state.waiters[method]?.remove(at: index) }
        }
        waiter?.continuation.resume(returning: .failure(.cancelled))
    }

    // --- [TRANSPORT]
    func read() async {
        do {
            for try await text: String in execution.standardOutput.strings() {
                let line: Data = Data(text.utf8)
                switch Result(catching: { try JSONDecoder().decode(CodexProtocol.ServerMessage.self, from: line) }) {
                    case .success(let message): await receive(message, line: line)
                    case .failure: finish(.invalidResponse(field: "message"))
                }
            }
            finish(.connectionClosed)
        } catch {
            finish(CodexFailure(process: ProcessRun.failure(error)))
        }
    }

    func finish(_ failure: CodexFailure) {
        let pending: (replies: [PendingReply], waiters: [NotificationWaiter])? = registry.withLock {
            state in
            guard state.terminalFailure == nil else { return nil }
            state.terminalFailure = failure
            let replies: [PendingReply] = Array(state.replies.values)
            let waiters: [NotificationWaiter] = Array(state.waiters.values.joined())
            state.replies.removeAll()
            state.waiters.removeAll()
            return (replies, waiters)
        }
        guard let pending else { return }
        updatesContinuation.finish()
        notifications.removeAll()
        for case .awaited(_, let continuation) in pending.replies {
            continuation.resume(returning: .failure(failure))
        }
        for waiter: NotificationWaiter in pending.waiters {
            waiter.continuation.resume(returning: .failure(failure))
        }
    }

    private static func decode<Value: Decodable>(_ type: Value.Type, _ line: Data) -> Value? {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.dateDecodingStrategy = .secondsSince1970
        return try? decoder.decode(type, from: line)
    }

    private func write(_ value: some Encodable & Sendable) async -> Result<Void, CodexFailure> {
        await Result { try JSONEncoder().encode(value) + [UInt8(ascii: "\n")] }
            .mapError { _ in CodexFailure.invalidResponse(field: "request") }
            .bind { line in
                await Result { _ = try await execution.standardInputWriter.write(line.bytes) }
                    .mapError { error in CodexFailure(process: ProcessRun.failure(error)) }
            }
    }

    private func receive(_ message: CodexProtocol.ServerMessage, line: Data) async {
        switch message {
            case .request(let id):
                let refusal: Result<Void, CodexFailure> = await write(
                    CodexProtocol.ErrorReply(
                        id: id,
                        error: CodexProtocol.ErrorObject(code: -32601, message: "Relay does not provide agent tools"),
                    )
                )
                if case .failure(let error) = refusal { finish(error) }
            case .response(let id, let error, let holdsResult):
                let serverVersion: String? = serverVersion
                let result: Data? = holdsResult ? line : nil
                let awaited:
                    (
                        reply: Result<Data, CodexFailure>,
                        continuation: CheckedContinuation<Result<Data, CodexFailure>, Never>
                    )? = registry.withLock { state in
                        switch state.replies[id] {
                            case .unclaimed(let method):
                                state.replies[id] = .arrived(
                                    Self.reply(error: error, result: result, method: method, serverVersion: serverVersion)
                                )
                                return nil
                            case .awaited(let method, let continuation):
                                state.replies.removeValue(forKey: id)
                                return (
                                    Self.reply(error: error, result: result, method: method, serverVersion: serverVersion),
                                    continuation,
                                )
                            case .arrived, .none: return nil
                        }
                    }
                if let awaited { awaited.continuation.resume(returning: awaited.reply) }
            case .notification(.accountRateLimitsUpdated):
                if let updated: CodexProtocol.ParamsField<CodexProtocol.RateLimitsUpdated> = Self.decode(
                    CodexProtocol.ParamsField<CodexProtocol.RateLimitsUpdated>.self,
                    line,
                ) {
                    updatesContinuation.yield(updated.params)
                }
            case .notification(let method):
                let waiter: NotificationWaiter? = registry.withLock { state in
                    state.waiters[method]?.firstIndex(where: { waiter in waiter.matches(line) })
                        .flatMap { index in state.waiters[method]?.remove(at: index) }
                }
                let retainedNotifications: Set<CodexProtocol.Method> = [.accountLoginCompleted, .turnCompleted]
                if let waiter {
                    waiter.continuation.resume(returning: .success(line))
                } else if retainedNotifications.contains(method) {
                    notifications[method, default: []].append(line)
                }
            case .ignored: return
        }
    }
}
