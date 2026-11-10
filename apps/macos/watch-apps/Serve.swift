import ApplicationServer
import Applications
import ArgumentParser
import CurrentProcess
import EffectRPC
import Encoding
import Foundation
import Logging
import System

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Serve: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Serves ApplicationServer requests as JSON lines over standard input and output, under its parent's privacy grants")

    func run() async {
        let logger = Logger(label: String(reflecting: Self.self))
        let definitions = ScriptingDefinitions()
        let calls = DispatchQueue(label: "\(Program.current.name).calls", attributes: .concurrent)
        let output = DispatchSerialQueue(label: "\(Program.current.name).output")
        await Run.execute(Self.self, output: nil, accessibility: false) { run throws(ApplicationFailure) in
            do throws(EffectRPCFailure) {
                try await serve(
                    reading: FileHandle.standardInput.bytes.jsonLines,
                    writing: { line in _ = try await withTaskExecutorPreference(output) { try FileDescriptor.standardOutput.writeAll(line) } },
                    dropped: { failure in logger.debug("Dropped request", error: failure) },
                ) { (request: Request) in
                    let response = try await withTaskExecutorPreference(calls) {
                        try await request.perform(admitting: { NSPredicate(value: true) }, refusing: { _ in [] }, definitions: definitions)
                    }
                    return if case .launch = request, case .stream(let steps) = response { .stream(run.recording(steps)) } else { response }
                }
            } catch {
                throw ApplicationFailure(subject: "connection", reason: .host(error as NSError))
            }
        }
    }
}
