public import Encoding
import Files
public import Foundation
public import Processes
import System
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct LaunchAgent: Sendable {
    private struct Declaration: Decodable {
        private enum CodingKeys: String, CodingKey {
            case label = "Label"
            case bundleProgram = "BundleProgram"
            case program = "Program"
            case arguments = "ProgramArguments"
            case keepAlive = "KeepAlive"
        }

        let label: String
        let bundleProgram: String?
        let program: String?
        let arguments: [String]?
        let keepAlive: JSONValue?
    }

    public let label: String
    public let program: URL
    public let plist: URL
    public let keepAlive: JSONValue?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LaunchAgent {
    init(_ plist: URL, in bundle: URL) throws(ApplicationFailure) {
        let path = plist.path(percentEncoded: false)
        let declared: Declaration
        do { declared = try PropertyListDecoder().decode(Declaration.self, from: Data(contentsOf: plist)) } catch { throw ApplicationFailure(subject: path, reason: .foundation(error as NSError)) }
        let stated = declared.bundleProgram.map { program in bundle.appending(path: program) } ?? (declared.program ?? declared.arguments?.first).map { program in URL(filePath: program) }
        guard let program = stated else { throw ApplicationFailure(subject: path, reason: .noExecutable) }
        self.init(label: declared.label, program: program, plist: plist, keepAlive: declared.keepAlive)
    }

    public func instances() throws(ApplicationFailure) -> [ProcessInstance] {
        do throws(Errno) {
            let executable = try ifPresent { () throws(Errno) in FilePath(try program.canonical().path(percentEncoded: false)) }
            return try executable.map { executable throws(Errno) in try ProcessInstance.all { path in path == executable } } ?? []
        } catch {
            throw ApplicationFailure(subject: program.path(percentEncoded: false), reason: .system(error))
        }
    }

    public func runsInstalledProgram() throws(ApplicationFailure) -> Bool {
        try instances().map { instance throws(ApplicationFailure) in
            do throws(Errno) { return try instance.runsInstalledCode() } catch  where error == .noSuchProcess { return false } catch {
                throw ApplicationFailure(subject: label, reason: .system(error))
            }
        }
        .contains(true)
    }
}

extension Installation {
    public func agents() throws(ApplicationFailure) -> [LaunchAgent] {
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
