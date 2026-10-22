public import CasePaths
import Encoding
public import Foundation
public import Processes
import Results
import Security
import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

@CasePathable
public enum KeychainFailure: CustomNSError, LocalizedError {
    case interactionNotAllowed
    case lineTooLong
    case unprinted
    case process(ProcessFailure)

    public var errorDescription: String? {
        switch self {
            case .interactionNotAllowed: SecCopyErrorMessageString(errSecInteractionNotAllowed, nil) as String?
            case .lineTooLong: "Command exceeds security -i line buffer of 4096 bytes"
            case .unprinted: "security printed no password line"
            case .process(let error): error.localizedDescription
        }
    }

    public var errorUserInfo: [String: Any] {
        self[case: \.process].map { error in [NSUnderlyingErrorKey: error] } ?? [:]
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum Keychain: Sendable {
    public static func genericPassword(service: String, account: String, within timeout: Duration) async -> Result<Data?, KeychainFailure> {
        await security(["find-generic-password", "-a", account, "-s", service, "-g"], within: timeout).flatMap { output in
            switch (output, output?.standardError.firstMatch(of: #/^password: (?:0x(?<hex>[0-9A-F]*)|"(?<text>.*)")?/#.anchorsMatchLineEndings())) {
                case (.none, _): .success(nil)
                case (.some, .none): .failure(.unprinted)
                case (.some, .some(let line)):
                    .success(
                        line.hex.map { hex in Data(hex.matches(of: /[0-9A-F]{2}/).compactMap { pair in UInt8(pair.output, radix: 16) }) } ?? line.text.map { text in Data(text.utf8) } ?? Data()
                    )
            }
        }
    }

    public static func setGenericPassword(_ data: Data, service: String, account: String, within timeout: Duration) async -> Result<Void, KeychainFailure> {
        func quoted(_ value: String) -> String {
            "\"" + value.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\""
        }
        let line: String = "add-generic-password -U -a \(quoted(account)) -s \(quoted(service)) -X \(data.hexEncoded)\n"
        return line.utf8.count >= 4096 ? .failure(.lineTooLong) : await security(["-i"], within: timeout, input: .string(line)).map { _ in () }
    }

    public static func deleteGenericPassword(service: String, account: String, within timeout: Duration) async -> Result<Void, KeychainFailure> {
        await security(["delete-generic-password", "-a", account, "-s", service], within: timeout).map { _ in () }
    }

    private static func security(
        _ arguments: [String],
        within timeout: Duration,
        input: some InputProtocol = .none,
    ) async -> Result<ExecutionResult<Void, DiscardedOutput, StringOutput<UTF8>>?, KeychainFailure> {
        func exitStatus(of status: OSStatus) -> Int32 { status & 0x00FF_FFFF }
        return await withTimeout(timeout) {
            await Result {
                try await Subprocess.run(
                    .path("/usr/bin/security"),
                    arguments: Arguments(arguments),
                    environment: .custom([:]),
                    workingDirectory: FilePath(URL.homeDirectory.path(percentEncoded: false)),
                    platformOptions: .session,
                    input: input,
                    output: .discarded,
                    error: .string(limit: 1 << 20),
                )
            }
            .mapError(ProcessFailure.init)
        }
        .mapError(KeychainFailure.process)
        .flatMap { output in
            switch output.terminationStatus {
                case .exited(0): .success(output)
                case .exited(exitStatus(of: errSecItemNotFound)): .success(nil)
                case .exited(exitStatus(of: errSecInteractionNotAllowed)): .failure(.interactionNotAllowed)
                case .exited, .signaled: .failure(.process(.exit(output.terminationStatus)))
            }
        }
    }
}
