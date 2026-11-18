import Files
import Foundation
public import Processes
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct Lifecycle: Encodable, Sendable {
    public struct Row: Encodable, Sendable {
        private enum Key: CodingKey {
            case instance, pid, executable, executableFailure, replaced, replacedFailure
        }

        public let instance: ProcessInstance
        public let executable: Result<FilePath, Errno>
        public let replaced: Result<Bool, Errno>

        public func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: Key.self)
            try fields.encode(instance, forKey: .instance)
            try fields.encode(instance.pid, forKey: .pid)
            switch executable {
                case .success(let path): try fields.encode(path.string, forKey: .executable)
                case .failure(let error): try fields.encode(error.description, forKey: .executableFailure)
            }
            switch replaced {
                case .success(let replaced): try fields.encode(replaced, forKey: .replaced)
                case .failure(let error): try fields.encode(error.description, forKey: .replacedFailure)
            }
        }
    }

    public let agents: [LaunchAgent]
    public let processes: [Row]
    public let reports: [DiagnosticReport]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Lifecycle {
    public init(of name: String, reports limit: Int) async throws(ApplicationFailure) {
        let exited = ProcessInstance(rawValue: name)
        let named: ProcessInstance?
        do throws(Errno) { named = try ProcessInstance(named: name) } catch  where error == .noSuchProcess && exited != nil { named = exited } catch {
            throw ApplicationFailure(error, process: name)
        }
        let file: URL?
        do throws(Errno) { file = named == nil && !FilePath(name).removingLastComponent().isEmpty ? try ifPresent { () throws(Errno) in try URL(filePath: name).canonical() } : nil } catch {
            throw ApplicationFailure(subject: name, reason: .system(error))
        }
        let regular: Bool
        do { regular = try file?.resourceValues(forKeys: [.isRegularFileKey]).isRegularFile == true } catch { throw ApplicationFailure(subject: name, reason: .host(error as NSError)) }
        let executable = regular ? file.map { file in FilePath(file.path(percentEncoded: false)) } : nil
        let installations = named == nil && executable == nil ? try await Installation.installed(named: name) : []
        let agents = Array(try installations.map { installation throws(ApplicationFailure) in try installation.agents() }.joined())
        let bundles: [FilePath]
        do throws(Errno) { bundles = try installations.map { installation throws(Errno) in FilePath(try installation.url.canonical().path(percentEncoded: false)) } } catch {
            throw ApplicationFailure(subject: name, reason: .system(error))
        }
        let scoped: (FilePath) -> Bool = { path in path == executable || bundles.contains(where: path.starts(with:)) }
        let installed = installations.compactMap { installation in Bundle(url: installation.url) }
        let owned: (DiagnosticReport) -> Bool = { report in
            report.executable.map { path in scoped(FilePath(path)) } == true || installed.contains { bundle in report.written(by: bundle) }
                || agents.contains { agent in agent.label == report.service }
        }
        let running: [ProcessInstance]
        do throws(Errno) {
            running =
                switch named {
                    case .some(let named) where named.hasExited: []
                    case .some(let named): [named]
                    case .none: try ProcessInstance.executing(scoped)
                }
        } catch {
            throw ApplicationFailure(subject: name, reason: .system(error))
        }
        self.init(
            agents: agents,
            processes: running.map { instance in
                Row(instance: instance, executable: Result { () throws(Errno) in try instance.executable() }, replaced: Result { () throws(Errno) in try !instance.runsInstalledCode() })
            },
            reports: DiagnosticReport.reports(limit: limit) { report in named.map { named in report.instance == named } ?? owned(report) }.reports,
        )
    }
}
