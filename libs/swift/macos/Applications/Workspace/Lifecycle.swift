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
    public init(of installation: Installation, reports limit: Int) throws(ApplicationFailure) {
        let agents = try installation.agents()
        let (bundle, running): (FilePath, [ProcessInstance])
        do throws(Errno) {
            bundle = FilePath(try installation.url.canonical().path(percentEncoded: false))
            running = try ProcessInstance.all { path in path.starts(with: bundle) }
        } catch {
            throw ApplicationFailure(subject: installation.name, reason: .system(error))
        }
        let installed = Bundle(url: installation.url)
        self.init(
            agents: agents,
            processes: running.map { instance in
                Row(instance: instance, executable: Result { () throws(Errno) in try instance.executable() }, replaced: Result { () throws(Errno) in try !instance.runsInstalledCode() })
            },
            reports: DiagnosticReport.reports(limit: limit) { report in
                report.executable.map { path in FilePath(path).starts(with: bundle) } == true || installed.map(report.isWritten(by:)) == true
                    || agents.contains { agent in agent.label == report.service }
            }
            .reports,
        )
    }
}
