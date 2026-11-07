import Encoding
import Files
import Foundation
import Processes
import Security
import Subprocess
import System

// --- [SERVICES] ------------------------------------------------------------------------

nonisolated extension ClaudeCredentialStore {
    // --- [KEYCHAIN_ITEM]

    func readItem() async throws(ClaudeFailure) -> JSONDocument<ClaudeCredentialItem>? {
        guard let printed: String = try await security(["find-generic-password", "-a", username, "-s", service, "-g"]) else { return nil }
        let password: Data? =
            switch printed.firstMatch(of: #/^password: (?:0x(?<hex>[0-9A-F]*)|"(?<text>.*)")?/#.anchorsMatchLineEndings())?.output {
                case .some((_, .some(let hex), _)): Data(hexEncoded: hex)
                case .some((_, .none, .some(let text))): Data(text.utf8)
                case .some((_, .none, .none)): Data()
                case .none: nil
            }
        guard let password else { throw .invalidCredentials(nil) }
        return try Self.decode(JSONDocument<ClaudeCredentialItem>.self, from: password)
    }

    func writeOAuth(
        _ oauth: JSONDocument<ClaudeOAuthToken.Attributes>,
        over item: JSONDocument<ClaudeCredentialItem>?,
    ) async throws(ClaudeFailure) {
        func quoted(_ value: String) -> String {
            "\"" + value.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\""
        }
        let data: Data
        do {
            data = try JSONEncoder().encode(JSONDocument(fields: item?.fields ?? [:], known: ClaudeCredentialItem(claudeAiOauth: oauth)))
        } catch {
            throw .invalidCredentials(error)
        }
        let line: String = "add-generic-password -U -a \(quoted(username)) -s \(quoted(service)) -X \(data.hexEncoded())\n"
        guard line.utf8.count < 4096 else { throw .credentialTooLong }
        _ = try await security(["-i"], input: .string(line))
        do { _ = try ifPresent { try FileManager.default.removeItem(at: directory.appending(path: ".credentials.json")) } } catch { throw .filesystem(error) }
    }

    func deleteItem() async throws(ClaudeFailure) {
        _ = try await security(["delete-generic-password", "-a", username, "-s", service])
    }

    // --- [SECURITY]

    private func security(_ arguments: Arguments, input: some InputProtocol = .none, within timeout: Duration = .seconds(15)) async throws(ClaudeFailure) -> String? {
        func code(_ status: OSStatus) -> Int32 { status & 0x00FF_FFFF }
        do throws(ProcessFailure) {
            return try await Configuration(
                executable: .path("/usr/bin/security"),
                arguments: arguments,
                environment: .custom([:]),
                workingDirectory: FilePath(URL.homeDirectory.path(percentEncoded: false)),
                platformOptions: .session(),
            )
            .run(output: .discarded, within: timeout, input: input, error: .string(limit: 1 << 20))
            .standardError
        } catch {
            return switch error.reason {
                case .exit(.exited(code(errSecItemNotFound))): nil
                case .exit(.exited(code(errSecInteractionNotAllowed))): throw .interactionNotAllowed
                default: throw ClaudeFailure(process: error)
            }
        }
    }
}
