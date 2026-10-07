import Darwin
import Foundation
import Subprocess
import System

// --- [TYPES] ---------------------------------------------------------------------------

private nonisolated enum ClaudeMessageType: String, Codable, Sendable {
    case user
    case controlRequest = "control_request"
    case controlResponse = "control_response"
    case rateLimitEvent = "rate_limit_event"
    case result
}

private nonisolated enum ClaudeSubtype: String, Codable, Sendable {
    case initialize
    case listModels = "list_models"
    case setModel = "set_model"
    case success
}

private nonisolated enum ClaudeRole: String, Encodable, Sendable {
    case user
}

private nonisolated enum ClaudeRateLimitType: String, Decodable, Sendable {
    case fiveHour = "five_hour"
}

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct ClaudeControlRequest: Encodable, Sendable {
    struct Request: Encodable, Sendable {
        let subtype: ClaudeSubtype
        let model: String?
    }

    let type: ClaudeMessageType = .controlRequest
    let requestID: String
    let request: Request
}

private nonisolated struct ClaudeUserMessage: Encodable, Sendable {
    struct Message: Encodable, Sendable {
        let role: ClaudeRole = .user
        let content: String
    }

    let type: ClaudeMessageType = .user
    let sessionID: UUID
    let message: Message

    enum CodingKeys: CodingKey {
        case type, sessionID, parentToolUseID, message
    }

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<CodingKeys> = encoder.container(keyedBy: CodingKeys.self)
        try container.encode(type, forKey: .type)
        try container.encode(sessionID, forKey: .sessionID)
        try container.encodeNil(forKey: .parentToolUseID)
        try container.encode(message, forKey: .message)
    }
}

private nonisolated struct ClaudeRateLimitInfo: Decodable, Sendable {
    struct Windows: Decodable, Sendable {
        struct Window: Decodable, Sendable {
            let resetsAt: Double?
        }

        let fiveHour: Window?
    }

    let unifiedWindows: Windows?
    let rateLimitType: Lenient<ClaudeRateLimitType>
    let resetsAt: Double?

    var sessionResetSeconds: Double? {
        unifiedWindows?.fiveHour?.resetsAt ?? (rateLimitType.value == .fiveHour ? resetsAt : nil)
    }
}

private nonisolated struct ClaudeModelOption: Decodable, Sendable {
    let value: String?
    let resolvedModel: String?
    let disabled: Bool?
}

private nonisolated struct ClaudeControlResponse: Decodable, Sendable {
    struct Response: Decodable, Sendable {
        let models: [ClaudeModelOption]?
    }

    let requestID: String?
    let subtype: Lenient<ClaudeSubtype>
    let response: Response?

    enum CodingKeys: String, CodingKey {
        case requestID = "requestId"
        case subtype, response
    }
}

private nonisolated struct ClaudeMessage: Decodable, Sendable {
    let type: Lenient<ClaudeMessageType>
    let subtype: Lenient<ClaudeSubtype>
    let isError: Bool?
    let rateLimitInfo: ClaudeRateLimitInfo?
    let response: ClaudeControlResponse?
}

private nonisolated enum ClaudeSessionPhase: Sendable {
    case initializing(String)
    case readingModels(String)
    case settingModel(String)
    case greeting

    var requestID: String? {
        switch self {
            case .initializing(let id), .readingModels(let id), .settingModel(let id): id
            case .greeting: nil
        }
    }
}

private nonisolated enum ClaudeSessionStep: Sendable {
    case awaiting(ClaudeSessionPhase, sessionReset: Date?)
    case finished(sessionReset: Date?)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum ClaudeSession {
    static func run(
        executable: Executable,
        token: ClaudeOAuthToken,
        environment: [String: String],
        workingDirectory: URL,
    ) async -> Result<Date?, ClaudeFailure> {
        await Result { try FileDescriptor.pipe() }.mapError(ClaudeFailure.filesystem)
            .flatMap { pipe in
                Result { try pipe.writeEnd.closeAfter { try pipe.writeEnd.writeAll(token.accessToken.utf8) } }
                    .map { _ in pipe.readEnd }
                    .mapError { error in
                        try? pipe.readEnd.close()
                        return .filesystem(error)
                    }
            }
            .bind { descriptor in
                defer { try? descriptor.close() }
                let sessionID: UUID = UUID()
                let tokenVariables: [String: String?] = [
                    "CLAUDE_CODE_SUBSCRIPTION_TYPE": token.document.known.subscriptionType,
                    "CLAUDE_CODE_RATE_LIMIT_TIER": token.document.known.rateLimitTier,
                    "CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR": String(descriptor.rawValue),
                ]
                var invocation: Configuration = ProcessRun.configuration(
                    executable: executable,
                    arguments: [
                        "-p", "--input-format", "stream-json", "--output-format", "stream-json", "--verbose",
                        "--tools", "", "--setting-sources=", "--strict-mcp-config", "--disable-slash-commands",
                        "--no-session-persistence", "--max-turns", "1", "--session-id", sessionID.uuidString,
                        "--system-prompt", "Reply with one word.",
                        "--settings", "{\"disableAllHooks\":true,\"autoMemoryEnabled\":false}",
                    ],
                    environment: environment.merging(tokenVariables.compactMapValues(\.self)) { _, new in new },
                    workingDirectory: workingDirectory,
                )
                let source: Int32 = descriptor.rawValue
                unsafe invocation.platformOptions.preSpawnProcessConfigurator = { _, actions in
                    guard unsafe posix_spawn_file_actions_addinherit_np(&actions, source) == 0 else {
                        throw ProcessFailure.descriptorNotInherited(source)
                    }
                }
                return await ProcessRun.stream(invocation, deadline: .seconds(120)) { execution in
                    await readMessages(execution: execution, sessionID: sessionID)
                }
                .mapError(ClaudeFailure.init(process:))
            }
            .flatMap { result, status in
                result.flatMap { reset in
                    status.isSuccess ? .success(reset) : .failure(.process(.exit(status)))
                }
            }
    }

    private static func readMessages(
        execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>,
        sessionID: UUID,
    ) async -> Result<Result<Date?, ClaudeFailure>, ProcessFailure> {
        var step: Result<ClaudeSessionStep, ClaudeFailure> = await sendControlRequest(
            ClaudeControlRequest.Request(subtype: .initialize, model: nil),
            as: ClaudeSessionPhase.initializing,
            sessionReset: nil,
            to: execution,
        )
        let lines: SubprocessOutputSequence.StringSequence<UTF8> = execution.standardOutput.strings()
        let decoder: JSONDecoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        let read: Result<Void, any Error> = await Result {
            for try await line: String in lines {
                guard case .success(.awaiting(let phase, let reset)) = step else { break }
                step = await Result { try decoder.decode(ClaudeMessage.self, from: Data(line.utf8)) }
                    .mapError { _ in ClaudeFailure.protocolFailure }
                    .bind { message in
                        await advance(
                            phase,
                            sessionReset: reset,
                            message: message,
                            execution: execution,
                            sessionID: sessionID,
                        )
                    }
                if case .success(.finished) = step { break }
            }
        }
        let outcome: Result<Date?, ClaudeFailure> = step.flatMap { step in
            switch step {
                case .finished(let reset): .success(reset)
                case .awaiting: .failure(Task.isCancelled ? .cancelled : .protocolFailure)
            }
        }
        return read.mapError(ProcessRun.failure).map { _ in outcome }
    }

    private static func advance(
        _ phase: ClaudeSessionPhase,
        sessionReset: Date?,
        message: ClaudeMessage,
        execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>,
        sessionID: UUID,
    ) async -> Result<ClaudeSessionStep, ClaudeFailure> {
        guard !Task.isCancelled else { return .failure(.cancelled) }
        if message.type.value == .rateLimitEvent, let seconds: Double = message.rateLimitInfo?.sessionResetSeconds {
            return .success(
                .awaiting(phase, sessionReset: Date(timeIntervalSince1970: seconds))
            )
        }
        if case .greeting = phase, message.type.value == .result {
            return message.subtype.value == .success && message.isError != true
                ? .success(.finished(sessionReset: sessionReset)) : .failure(.greetingFailed)
        }
        guard message.type.value == .controlResponse,
            let response: ClaudeControlResponse = message.response,
            response.requestID == phase.requestID
        else { return .success(.awaiting(phase, sessionReset: sessionReset)) }
        guard response.subtype.value == .success else { return .failure(.protocolFailure) }
        switch phase {
            case .initializing:
                return await sendControlRequest(
                    ClaudeControlRequest.Request(subtype: .listModels, model: nil),
                    as: ClaudeSessionPhase.readingModels,
                    sessionReset: sessionReset,
                    to: execution,
                )
            case .readingModels:
                return
                    if let model: ClaudeModelOption = response.response?.models?.first(where: { item in
                        item.disabled != true
                            && (item.value == "haiku" || item.resolvedModel?.hasPrefix("claude-haiku-") == true)
                    }),
                    let resolved: String = model.resolvedModel
                {
                    await sendControlRequest(
                        ClaudeControlRequest.Request(subtype: .setModel, model: resolved),
                        as: ClaudeSessionPhase.settingModel,
                        sessionReset: sessionReset,
                        to: execution,
                    )
                } else {
                    .failure(.modelUnavailable)
                }
            case .settingModel:
                let greeting: ClaudeUserMessage = ClaudeUserMessage(
                    sessionID: sessionID,
                    message: ClaudeUserMessage.Message(content: "hi"),
                )
                return await send(greeting, to: execution, closingInput: true).map { _ in
                    .awaiting(.greeting, sessionReset: sessionReset)
                }
            case .greeting:
                return .failure(.protocolFailure)
        }
    }

    private static func sendControlRequest(
        _ request: ClaudeControlRequest.Request,
        as phase: (String) -> ClaudeSessionPhase,
        sessionReset: Date?,
        to execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>,
    ) async -> Result<ClaudeSessionStep, ClaudeFailure> {
        let id: String = UUID().uuidString
        return await send(ClaudeControlRequest(requestID: id, request: request), to: execution, closingInput: false)
            .map { _ in .awaiting(phase(id), sessionReset: sessionReset) }
    }

    private static func send(
        _ message: some Encodable & Sendable,
        to execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>,
        closingInput: Bool,
    ) async -> Result<Void, ClaudeFailure> {
        await Result {
            let encoder: JSONEncoder = JSONEncoder()
            encoder.keyEncodingStrategy = .convertToSnakeCase
            return try encoder.encode(message) + [UInt8(ascii: "\n")]
        }
        .mapError { _ in ClaudeFailure.protocolFailure }
        .bind { line in
            await Result {
                _ = try await execution.standardInputWriter.write(line.bytes)
                if closingInput { try await execution.standardInputWriter.finish() }
            }
            .mapError { error in ClaudeFailure(process: ProcessRun.failure(error)) }
        }
    }
}
