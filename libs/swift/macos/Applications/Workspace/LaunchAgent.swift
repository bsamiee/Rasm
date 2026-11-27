import Encoding
import Files
package import Foundation
import Processes
import System
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

package struct LaunchAgent: Sendable {
    package enum KeepAlive: Decodable, Sendable {
        case demand
        case always
        case conditions

        package init(from decoder: any Decoder) throws {
            let value = try decoder.singleValueContainer()
            self = try value.decodeIfMatching(Bool.self).map { kept in kept ? .always : .demand } ?? (value.decode([String: JSONValue].self).isEmpty ? .demand : .conditions)
        }
    }

    private struct Declaration: Decodable {
        private enum CodingKeys: String, CodingKey {
            case label = "Label"
            case bundleProgram = "BundleProgram"
            case program = "Program"
            case arguments = "ProgramArguments"
            case keepAlive = "KeepAlive"
            case exitTimeOut = "ExitTimeOut"
            case throttleInterval = "ThrottleInterval"
        }

        let label: String
        let bundleProgram: String?
        let program: String?
        let arguments: [String]?
        let keepAlive: KeepAlive?
        let exitTimeOut: Int?
        let throttleInterval: Int?
    }

    package let label: String
    let program: URL
    package let plist: URL
    package let contents: Data
    package let keepAlive: KeepAlive
    package let exitTimeOut: Duration?
    package let throttleInterval: Duration
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LaunchAgent {
    init(_ plist: URL, in bundle: URL) throws(ApplicationFailure) {
        let path = plist.path(percentEncoded: false)
        let contents: Data
        let declared: Declaration
        do {
            contents = try Data(contentsOf: plist)
            declared = try PropertyListDecoder().decode(Declaration.self, from: contents)
        } catch {
            throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
        }
        let stated = declared.bundleProgram.map { program in bundle.appending(path: program) } ?? (declared.program ?? declared.arguments?.first).map { program in URL(filePath: program) }
        guard let program = stated else { throw ApplicationFailure(subject: path, reason: .noExecutable) }
        self.init(
            label: declared.label,
            program: program,
            plist: plist,
            contents: contents,
            keepAlive: declared.keepAlive ?? .demand,
            exitTimeOut: declared.exitTimeOut.flatMap { seconds in seconds > 0 ? .seconds(seconds) : nil },
            throttleInterval: .seconds(declared.throttleInterval ?? 10),
        )
    }

    func instances() throws(ApplicationFailure) -> [ProcessInstance] {
        do throws(Errno) {
            let executable = try ifPresent { () throws(Errno) in FilePath(try program.canonical().path(percentEncoded: false)) }
            return try executable.map { executable throws(Errno) in try ProcessInstance.all { path in path == executable } } ?? []
        } catch {
            throw ApplicationFailure(subject: program.path(percentEncoded: false), reason: .system(error))
        }
    }

    package func runsInstalledProgram() throws(ApplicationFailure) -> Bool {
        try instances().map { instance throws(ApplicationFailure) in
            do throws(ProcessFailure) { return try instance.runsInstalledCode() } catch  where error.reason == .system(.noSuchProcess) { return false } catch {
                throw ApplicationFailure(subject: label, reason: .foundation(error as NSError))
            }
        }
        .contains(true)
    }

    package func terminate() async throws(ApplicationFailure) {
        let running = try instances()
        for instance in running {
            do throws(Errno) { try instance.signal(SIGTERM) } catch  where error == .noSuchProcess { continue } catch {
                throw ApplicationFailure(subject: label, reason: .system(error))
            }
        }
        await withDiscardingTaskGroup { group in
            for instance in running { group.addTask { for await _ in instance.exits() {} } }
        }
    }
}

extension Installation {
    package func agents() throws(ApplicationFailure) -> [LaunchAgent] {
        let folder = url.appending(components: "Contents", "Library", "LaunchAgents", directoryHint: .isDirectory)
        let files: [URL]
        do { files = try ifPresent { try FileManager.default.contentsOfDirectory(at: folder, includingPropertiesForKeys: nil) } ?? [] } catch {
            throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .foundation(error as NSError))
        }
        return try files.filter { file in UTType(filenameExtension: file.pathExtension)?.conforms(to: .propertyList) == true }
            .sorted(using: KeyPathComparator(\.lastPathComponent))
            .map { file throws(ApplicationFailure) in try LaunchAgent(file, in: url) }
    }
}
