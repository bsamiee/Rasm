import AsyncAlgorithms
import Darwin
import Foundation
import JSON
import Processes
import Results
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
    case setModel = "set_model"
    case success
}

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct ClaudeControlRequest: Encodable, Sendable {
    enum Request: Encodable, Sendable {
        case initialize
        case setModel(String)

        enum CodingKeys: CodingKey {
            case subtype, model
        }

        func encode(to encoder: any Encoder) throws {
            var container: KeyedEncodingContainer<CodingKeys> = encoder.container(keyedBy: CodingKeys.self)
            switch self {
                case .initialize: try container.encode(ClaudeSubtype.initialize, forKey: .subtype)
                case .setModel(let model):
                    try container.encode(ClaudeSubtype.setModel, forKey: .subtype)
                    try container.encode(model, forKey: .model)
            }
        }
    }

    let type: ClaudeMessageType = .controlRequest
    let requestID: String
    let request: Request
}

private nonisolated struct ClaudeUserMessage: Encodable, Sendable {
    struct Message: Encodable, Sendable {
        let role: String = "user"
        let content: String
    }

    let type: ClaudeMessageType = .user
    let sessionID: UUID
    let parentToolUseID: JSONValue = .null
    let message: Message
}

private nonisolated struct ClaudeRateLimitInfo: Decodable, Sendable {
    struct Windows: Decodable, Sendable {
        struct Window: Decodable, Sendable {
            let resetsAt: Double?
        }

        let fiveHour: Window?
    }

    let unifiedWindows: Windows?
    let rateLimitType: String?
    let resetsAt: Double?

    var sessionResetSeconds: Double? {
        unifiedWindows?.fiveHour?.resetsAt ?? (rateLimitType == "five_hour" ? resetsAt : nil)
    }
}

private nonisolated struct ClaudeModelOption: Decodable, Sendable {
    let value: String?
    let resolvedModel: String?
}

private nonisolated struct ClaudeControlResponse: Decodable, Sendable {
    struct Response: Decodable, Sendable {
        let models: [ClaudeModelOption]?
    }

    let requestId: String
    @Lenient var subtype: ClaudeSubtype?
    let response: Response?
}

private nonisolated struct ClaudeMessage: Decodable, Sendable {
    @Lenient var type: ClaudeMessageType?
    @Lenient var subtype: ClaudeSubtype?
    let isError: Bool?
    let rateLimitInfo: ClaudeRateLimitInfo?
    let response: ClaudeControlResponse?
}

private nonisolated enum ClaudeSessionPhase: Sendable {
    case requesting(ClaudeControlRequest)
    case greeting
    case finished
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum ClaudeSession {
    static func run(_ invocation: Configuration, token: ClaudeOAuthToken) async throws(ClaudeFailure) -> Date? {
        let pipe: (readEnd: FileDescriptor, writeEnd: FileDescriptor) = try Result { () throws(Errno) in try FileDescriptor.pipe(options: .closeOnExec) }
            .mapError(ClaudeFailure.filesystem)
            .get()
        let sessionID: UUID = UUID()
        var greeting: Configuration = invocation
        greeting.arguments = [
            "-p", "--input-format", "stream-json", "--output-format", "stream-json", "--verbose",
            "--tools", "", "--setting-sources=", "--strict-mcp-config", "--disable-slash-commands",
            "--no-session-persistence", "--max-turns", "1", "--session-id", sessionID.uuidString,
            "--system-prompt", "Reply with one word.",
            "--settings", "{\"disableAllHooks\":true,\"autoMemoryEnabled\":false}",
        ]
        greeting.environment = invocation.environment.updating([
            "CLAUDE_CODE_SUBSCRIPTION_TYPE": token.document.known.subscriptionType,
            "CLAUDE_CODE_RATE_LIMIT_TIER": token.document.known.rateLimitTier,
            "CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR": String(pipe.readEnd.rawValue),
        ])
        let source: Int32 = pipe.readEnd.rawValue
        unsafe greeting.platformOptions.preSpawnProcessConfigurator = { _, actions in
            let code: Int32 = unsafe posix_spawn_file_actions_addinherit_np(&actions, source)
            guard code == 0 else { throw Errno(rawValue: code) }
        }
        @Sendable func converse(_ execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>) async -> Result<Date?, ClaudeFailure> {
            func send(_ message: some Encodable) async throws(ClaudeFailure) {
                do {
                    try await execution.standardInputWriter.writeLine(message, encoder: ClaudeEndpoint.encoder)
                } catch {
                    throw ClaudeFailure(process: error)
                }
            }
            func request(_ request: ClaudeControlRequest.Request) async throws(ClaudeFailure) -> ClaudeSessionPhase {
                let sent: ClaudeControlRequest = ClaudeControlRequest(requestID: UUID().uuidString, request: request)
                try await send(sent)
                return .requesting(sent)
            }
            func advance(_ phase: ClaudeSessionPhase, _ message: ClaudeMessage) async throws(ClaudeFailure) -> ClaudeSessionPhase {
                guard !Task.isCancelled else { throw .deadline(.cancelled) }
                switch (phase, message.type) {
                    case (.greeting, .result):
                        guard message.subtype == .success, message.isError != true else { throw .greetingFailed }
                        return .finished
                    case (.requesting(let sent), .controlResponse):
                        guard let response: ClaudeControlResponse = message.response, response.requestId == sent.requestID else { return phase }
                        guard response.subtype == .success else { throw .protocolFailure }
                        switch sent.request {
                            case .initialize:
                                guard
                                    let model: String = response.response?.models?
                                        .first(where: { item in item.value == "haiku" || item.resolvedModel?.hasPrefix("claude-haiku-") == true })?
                                        .resolvedModel
                                else { throw .modelUnavailable }
                                return try await request(.setModel(model))
                            case .setModel:
                                try await send(ClaudeUserMessage(sessionID: sessionID, message: ClaudeUserMessage.Message(content: "hi")))
                                do { try await execution.standardInputWriter.finish() } catch { throw ClaudeFailure(process: error) }
                                return .greeting
                        }
                    default: return phase
                }
            }
            return await Result { () async throws(ClaudeFailure) in
                var phase: ClaudeSessionPhase = try await request(.initialize)
                var reset: Date?
                for try await line: Data in execution.standardOutput.jsonLines.mapError(ClaudeFailure.init(process:)) {
                    let message: ClaudeMessage = try Result { try ClaudeEndpoint.decoder.decode(ClaudeMessage.self, from: line) }.mapError { _ in ClaudeFailure.protocolFailure }.get()
                    if message.type == .rateLimitEvent, let seconds: Double = message.rateLimitInfo?.sessionResetSeconds { reset = Date(timeIntervalSince1970: seconds) }
                    phase = try await advance(phase, message)
                    if case .finished = phase { return reset }
                }
                throw Task.isCancelled ? .deadline(.cancelled) : .protocolFailure
            }
        }
        let outcome: Result<Date?, ClaudeFailure> = await Result { () async throws(ClaudeFailure) in
            _ = try Result { try pipe.writeEnd.closeAfter { try pipe.writeEnd.writeAll(token.accessToken.utf8) } }.mapError(ClaudeFailure.filesystem).get()
            let result: ExecutionResult<Result<Date?, ClaudeFailure>, SequenceOutput, DiscardedOutput> = try await withTimeout(.seconds(120)) { [greeting] in
                await Result { try await Subprocess.run(greeting, input: .inputWriter, output: .sequence, error: .discarded, body: converse) }.mapError(ClaudeFailure.init(child:))
            }
            .get()
            guard result.terminationStatus.isSuccess else { throw .process(.exit(result.terminationStatus)) }
            return try result.closureResult.get()
        }
        let closed: Result<Void, ClaudeFailure> = Result { try pipe.readEnd.close() }.mapError(ClaudeFailure.filesystem)
        return try outcome.mapError { error in error.releasing(closed) }.flatMap { reset in closed.map { _ in reset } }.get()
    }
}
