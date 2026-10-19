public import Foundation
import Security
public import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

public enum KeychainFailure: Error {
    case interactionNotAllowed
    case itemTooLarge
    case unprinted
    case failed(TerminationStatus)
    case process(ProcessFailure)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum SecurityCommand {
    public static func readGenericPassword(service: String, account: String) async -> Result<Data?, KeychainFailure> {
        await security(["find-generic-password", "-a", account, "-s", service, "-g"]).flatMap { output in
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

    public static func writeGenericPassword(service: String, account: String, data: Data) async -> Result<Void, KeychainFailure> {
        func quoted(_ value: String) -> String {
            "\"" + value.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\""
        }
        let interactiveLineLimit: Int = 4095
        let command: String = "add-generic-password -U -a \(quoted(account)) -s \(quoted(service)) -X \(data.hexEncoded)"
        return command.utf8.count > interactiveLineLimit ? .failure(.itemTooLarge) : await security(["-i"], input: .string(command + "\n")).map { _ in () }
    }

    public static func deleteGenericPassword(service: String, account: String) async -> Result<Void, KeychainFailure> {
        await security(["delete-generic-password", "-a", account, "-s", service]).map { _ in () }
    }

    private static func security(
        _ arguments: [String],
        input: some InputProtocol = .none,
    ) async -> Result<ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>>?, KeychainFailure> {
        let configuration: Configuration = ProcessRun.configuration(executable: .path("/usr/bin/security"), arguments: arguments, environment: [:], workingDirectory: URL.homeDirectory)
        return await ProcessRun.run(deadline: .seconds(15)) {
            try await Subprocess.run(configuration, input: input, output: .string(limit: 1 << 20), error: .string(limit: 1 << 20))
        }
        .mapError(KeychainFailure.process)
        .flatMap { output in
            switch output.terminationStatus {
                case .exited(0): .success(output)
                case .exited(exitStatus(of: errSecItemNotFound)): .success(nil)
                case .exited(exitStatus(of: errSecInteractionNotAllowed)): .failure(.interactionNotAllowed)
                case .exited, .signaled: .failure(.failed(output.terminationStatus))
            }
        }
        func exitStatus(of status: OSStatus) -> Int32 { Int32(UInt8(truncatingIfNeeded: status)) }
    }
}

extension Sequence<UInt8> {
    public var hexEncoded: String {
        map { byte in String(byte >> 4, radix: 16) + String(byte & 0x0F, radix: 16) }.joined()
    }
}
