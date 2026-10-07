import Foundation
import Security
import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum KeychainFailure: Error, Sendable {
    case interactionNotAllowed
    case itemTooLarge
    case failed(TerminationStatus)
    case process(ProcessFailure)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum Keychain {
    private static let notFoundStatus: Int32 = exitStatus(of: errSecItemNotFound)
    private static let interactionNotAllowedStatus: Int32 = exitStatus(of: errSecInteractionNotAllowed)

    static func readGenericPassword(
        service: String,
        account: String,
    ) async -> Result<Data?, KeychainFailure> {
        await security(["find-generic-password", "-a", account, "-s", service, "-w"]).map { printed in
            printed.map(decode)
        }
    }

    static func writeGenericPassword(
        service: String,
        account: String,
        data: Data,
    ) async -> Result<Void, KeychainFailure> {
        let interactiveLineLimit: Int = 4095
        let command: String =
            "add-generic-password -U -a \(quoted(account)) -s \(quoted(service)) -X \(data.hexEncoded)"
        return command.utf8.count > interactiveLineLimit
            ? .failure(.itemTooLarge)
            : await security(["-i"], input: .string(command + "\n")).map { _ in () }
    }

    static func deleteGenericPassword(
        service: String,
        account: String,
    ) async -> Result<Void, KeychainFailure> {
        await security(["delete-generic-password", "-a", account, "-s", service]).map { _ in () }
    }

    private static func security<Input: InputProtocol>(
        _ arguments: [String],
        input: Input = .none,
    ) async -> Result<String?, KeychainFailure> {
        await ProcessRun.collect(
            ProcessRun.configuration(
                executable: .path("/usr/bin/security"),
                arguments: arguments,
                environment: [:],
                workingDirectory: URL.homeDirectory,
            ),
            deadline: .seconds(15),
            input: input,
        )
        .mapError(KeychainFailure.process)
        .flatMap { output in
            switch output.terminationStatus {
                case .exited(0): .success(output.standardOutput)
                case .exited(notFoundStatus): .success(nil)
                case .exited(interactionNotAllowedStatus): .failure(.interactionNotAllowed)
                case .exited, .signaled: .failure(.failed(output.terminationStatus))
            }
        }
    }

    private static func exitStatus(of status: OSStatus) -> Int32 {
        Int32(UInt32(bitPattern: status) & 0x00FF_FFFF)
    }

    private static func decode(_ printed: String) -> Data {
        let text: String = printed.replacing(/\n\z/, with: "")
        return text.wholeMatch(of: /(?:[0-9a-fA-F]{2})+/) == nil
            ? Data(text.utf8) : Data(text.matches(of: /[0-9a-fA-F]{2}/).compactMap { pair in UInt8(pair.output, radix: 16) })
    }

    private static func quoted(_ value: String) -> String {
        "\"" + value.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\""
    }
}

nonisolated extension Sequence<UInt8> {
    var hexEncoded: String {
        map { byte in String(byte >> 4, radix: 16) + String(byte & 0x0F, radix: 16) }.joined()
    }
}
