import Foundation
import Subprocess

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
    let fiveHourResetsAt: Double?
    let rateLimitType: ClaudeRateLimitType?
    let resetsAt: Double?

    enum CodingKeys: CodingKey {
        case unifiedWindows, fiveHour, rateLimitType, resetsAt
    }

    var sessionResetSeconds: Double? {
        fiveHourResetsAt ?? (rateLimitType == .fiveHour ? resetsAt : nil)
    }

    init(from decoder: any Decoder) {
        let container: KeyedDecodingContainer<CodingKeys>? = try? decoder.container(keyedBy: CodingKeys.self)
        let windows: KeyedDecodingContainer<CodingKeys>? = try? container?.nestedContainer(
            keyedBy: CodingKeys.self,
            forKey: .unifiedWindows,
        )
        let fiveHour: KeyedDecodingContainer<CodingKeys>? = try? windows?.nestedContainer(
            keyedBy: CodingKeys.self,
            forKey: .fiveHour,
        )
        fiveHourResetsAt = try? fiveHour?.decodeIfPresent(Double.self, forKey: .resetsAt)
        rateLimitType = try? container?.decodeIfPresent(ClaudeRateLimitType.self, forKey: .rateLimitType)
        resetsAt = try? container?.decodeIfPresent(Double.self, forKey: .resetsAt)
    }
}

private nonisolated struct ClaudeModelOption: Decodable, Sendable {
    let value: String?
    let resolvedModel: String?
    let disabled: Bool?

    enum CodingKeys: CodingKey {
        case value, resolvedModel, disabled
    }

    init(from decoder: any Decoder) {
        let container: KeyedDecodingContainer<CodingKeys>? = try? decoder.container(keyedBy: CodingKeys.self)
        value = try? container?.decodeIfPresent(String.self, forKey: .value)
        resolvedModel = try? container?.decodeIfPresent(String.self, forKey: .resolvedModel)
        disabled = try? container?.decodeIfPresent(Bool.self, forKey: .disabled)
    }
}

private nonisolated struct ClaudeControlResponse: Decodable, Sendable {
    let requestID: String?
    let subtype: ClaudeSubtype?
    let models: [ClaudeModelOption]?

    enum CodingKeys: CodingKey {
        case requestId, subtype, response, models
    }

    init(from decoder: any Decoder) {
        let container: KeyedDecodingContainer<CodingKeys>? = try? decoder.container(keyedBy: CodingKeys.self)
        let payload: KeyedDecodingContainer<CodingKeys>? = try? container?.nestedContainer(
            keyedBy: CodingKeys.self,
            forKey: .response,
        )
        requestID = try? container?.decodeIfPresent(String.self, forKey: .requestId)
        subtype = try? container?.decodeIfPresent(ClaudeSubtype.self, forKey: .subtype)
        models = try? payload?.decodeIfPresent([ClaudeModelOption].self, forKey: .models)
    }
}

private nonisolated struct ClaudeMessage: Decodable, Sendable {
    let type: ClaudeMessageType?
    let subtype: ClaudeSubtype?
    let isError: Bool?
    let rateLimitInfo: ClaudeRateLimitInfo?
    let response: ClaudeControlResponse?

    enum CodingKeys: CodingKey {
        case type, subtype, isError, rateLimitInfo, response
    }

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<CodingKeys>? = try? decoder.container(keyedBy: CodingKeys.self)
        type = try? container?.decodeIfPresent(ClaudeMessageType.self, forKey: .type)
        subtype = try? container?.decodeIfPresent(ClaudeSubtype.self, forKey: .subtype)
        isError = try? container?.decodeIfPresent(Bool.self, forKey: .isError)
        rateLimitInfo = try container?.decodeIfPresent(ClaudeRateLimitInfo.self, forKey: .rateLimitInfo)
        response = try container.flatMap { keyed in
            try keyed.contains(.response) ? keyed.decode(ClaudeControlResponse.self, forKey: .response) : nil
        }
    }
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
        invocation: ProcessInvocation,
        sessionID: UUID,
    ) async -> Result<Date?, ClaudeFailure> {
        let outcome: Result<(Result<Date?, ClaudeFailure>, TerminationStatus), ProcessFailure> =
            await ProcessRun.stream(invocation, deadline: .seconds(120)) { execution in
                await readMessages(execution: execution, sessionID: sessionID)
            }
        return outcome.mapError(ClaudeFailure.init(process:)).flatMap { result, status in
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
        if message.type == .rateLimitEvent, let seconds: Double = message.rateLimitInfo?.sessionResetSeconds {
            return .success(
                .awaiting(phase, sessionReset: Date(timeIntervalSince1970: seconds))
            )
        }
        if case .greeting = phase, message.type == .result {
            return message.subtype == .success && message.isError != true
                ? .success(.finished(sessionReset: sessionReset)) : .failure(.greetingFailed)
        }
        guard message.type == .controlResponse,
            let response: ClaudeControlResponse = message.response,
            response.requestID == phase.requestID
        else { return .success(.awaiting(phase, sessionReset: sessionReset)) }
        guard response.subtype == .success else { return .failure(.protocolFailure) }
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
                    if let model: ClaudeModelOption = response.models?.first(where: { item in
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
