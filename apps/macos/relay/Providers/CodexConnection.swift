import Foundation
import RasmMacOS
import Subprocess
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

actor CodexConnection {
    // --- [STATE]
    private struct PendingReply: Sendable {
        let method: CodexProtocol.Method
        let continuation: AsyncStream<Result<Data, CodexFailure>>.Continuation
    }

    private struct NotificationWaiter: Sendable {
        let id: UUID
        let accept: @Sendable (Data) -> (@Sendable () -> Void)?
        let fail: @Sendable (CodexFailure) -> Void
    }

    private struct Registry: Sendable {
        var waiters: [CodexProtocol.Method: [NotificationWaiter]] = [:]
        var terminalFailure: CodexFailure?
    }

    nonisolated let updates: AsyncStream<CodexProtocol.RateLimitsUpdated>
    private let updatesContinuation: AsyncStream<CodexProtocol.RateLimitsUpdated>.Continuation
    private let execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>
    private let registry: Mutex<Registry> = Mutex(Registry())
    private var nextRequestID: Int = 0
    private var replies: [String: PendingReply] = [:]
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
            serverVersion = result.userAgent?.firstMatch(of: #/^[^/]+/(\S+)/#).map { match in String(match.1) }
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
            let (answers, answer): (AsyncStream<Result<Data, CodexFailure>>, AsyncStream<Result<Data, CodexFailure>>.Continuation) = AsyncStream.makeStream()
            replies[id] = PendingReply(method: method, continuation: answer)
            defer { replies[id] = nil }
            return await write(CodexProtocol.Request(id: id, method: method, params: params)).bind { _ in
                await answers.first(where: { _ in true }) ?? .failure(.cancelled)
            }
        }
    }

    // --- [NOTIFICATIONS]
    func notification<Value: Decodable & Sendable>(
        _: Value.Type,
        _ method: CodexProtocol.Method,
        matching predicate: @escaping @Sendable (Value) -> Bool,
    ) async -> Result<Value, CodexFailure> {
        let matched: @Sendable (Data) -> Value? = { line in
            Self.decode(CodexProtocol.ParamsField<Value>.self, line).map(\.params).flatMap { value in predicate(value) ? value : nil }
        }
        return await ready.bind { _ in
            if let retained: (index: Int, value: Value) = notifications[method]?.enumerated().lazy
                .compactMap({ index, line in matched(line).map { value in (index, value) } }).first
            {
                notifications[method]?.remove(at: retained.index)
                return .success(retained.value)
            }
            return await notification(method, matching: matched, waiter: UUID())
        }
    }

    private func notification<Value: Sendable>(
        _ method: CodexProtocol.Method,
        matching matched: @escaping @Sendable (Data) -> Value?,
        waiter id: UUID,
    ) async -> Result<Value, CodexFailure> {
        await withTaskCancellationHandler {
            await withCheckedContinuation { (continuation: CheckedContinuation<Result<Value, CodexFailure>, Never>) in
                let waiter: NotificationWaiter = NotificationWaiter(
                    id: id,
                    accept: { line -> (@Sendable () -> Void)? in
                        matched(line).map { value -> @Sendable () -> Void in { continuation.resume(returning: .success(value)) } }
                    },
                    fail: { failure in continuation.resume(returning: .failure(failure)) },
                )
                let refusal: CodexFailure? = registry.withLock { state in
                    if let failure: CodexFailure = state.terminalFailure { return failure }
                    guard !Task.isCancelled else { return .cancelled }
                    state.waiters[method, default: []].append(waiter)
                    return nil
                }
                if let refusal { continuation.resume(returning: .failure(refusal)) }
            }
        } onCancel: {
            Self.cancelWaiter(id, method: method, in: self.registry)
        }
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
        waiter?.fail(.cancelled)
    }

    // --- [TRANSPORT]
    func read() async {
        do {
            for try await text: String in execution.standardOutput.strings() {
                let line: Data = Data(text.utf8)
                switch Result(catching: { try JSONDecoder().decode(CodexProtocol.ServerMessage.self, from: line) }) {
                    case .success(.request(let id)):
                        let refusal: Result<Void, CodexFailure> = await write(
                            CodexProtocol.ErrorReply(
                                id: id,
                                error: CodexProtocol.ErrorObject(code: -32601, message: "Relay does not provide agent tools"),
                            )
                        )
                        if case .failure(let error) = refusal { finish(error) }
                    case .success(.response(let id, let outcome)):
                        switch (replies.removeValue(forKey: id), outcome) {
                            case (.some(let pending), .some(.success)): pending.continuation.yield(.success(line))
                            case (.some(let pending), .some(.failure(let error))):
                                pending.continuation.yield(
                                    .failure(.requestRejected(method: pending.method, code: error.code, message: error.message, serverVersion: serverVersion))
                                )
                            case (.some(let pending), .none): pending.continuation.yield(.failure(.invalidResponse(field: "request result")))
                            case (.none, _): break
                        }
                    case .success(.notification(.accountRateLimitsUpdated)):
                        if let updated: CodexProtocol.ParamsField<CodexProtocol.RateLimitsUpdated> = Self.decode(
                            CodexProtocol.ParamsField<CodexProtocol.RateLimitsUpdated>.self,
                            line,
                        ) {
                            updatesContinuation.yield(updated.params)
                        }
                    case .success(.notification(let method)):
                        let delivery: (@Sendable () -> Void)? = registry.withLock { state in
                            state.waiters[method]?.enumerated().lazy
                                .compactMap { index, waiter in waiter.accept(line).map { resume in (index, resume) } }.first
                                .map { index, resume in
                                    state.waiters[method]?.remove(at: index)
                                    return resume
                                }
                        }
                        let retainedNotifications: Set<CodexProtocol.Method> = [.accountLoginCompleted, .turnCompleted]
                        if let delivery {
                            delivery()
                        } else if retainedNotifications.contains(method) {
                            notifications[method, default: []].append(line)
                        }
                    case .success(.ignored): continue
                    case .failure: finish(.invalidResponse(field: "message"))
                }
            }
            finish(.connectionClosed)
        } catch {
            finish(CodexFailure(process: ProcessRun.failure(error)))
        }
    }

    func finish(_ failure: CodexFailure) {
        let waiters: [NotificationWaiter]? = registry.withLock { state in
            guard state.terminalFailure == nil else { return nil }
            state.terminalFailure = failure
            let waiters: [NotificationWaiter] = Array(state.waiters.values.joined())
            state.waiters.removeAll()
            return waiters
        }
        guard let waiters else { return }
        updatesContinuation.finish()
        notifications.removeAll()
        for pending: PendingReply in replies.values {
            pending.continuation.yield(.failure(failure))
        }
        replies.removeAll()
        for waiter: NotificationWaiter in waiters {
            waiter.fail(failure)
        }
    }

    private static func decode<Value: Decodable>(_ type: Value.Type, _ line: Data) -> Value? {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.dateDecodingStrategy = .secondsSince1970
        return try? decoder.decode(type, from: line)
    }

    private func write(_ value: some Encodable) async -> Result<Void, CodexFailure> {
        await Result { try JSONEncoder().encode(value) + [UInt8(ascii: "\n")] }
            .mapError { _ in CodexFailure.invalidResponse(field: "request") }
            .bind { line in
                await Result { () async throws(SubprocessError) in _ = try await execution.standardInputWriter.write(line.bytes) }
                    .mapError { error in CodexFailure(process: ProcessRun.failure(error)) }
            }
    }
}
