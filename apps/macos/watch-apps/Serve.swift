import ApplicationServer
import Applications
import ArgumentParser
import CurrentProcess
import EffectRPC
import Encoding
import Foundation
import System
import os

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Serve: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Serves ApplicationServer requests as JSON lines over standard input and output, under its parent's privacy grants")

    func run() async {
        let definitions = ScriptingDefinitions()
        let calls = DispatchQueue(label: "\(program).calls", attributes: .concurrent)
        let output = DispatchSerialQueue(label: "\(program).output")
        await Run.execute(output: nil, accessibility: false) { run throws(ApplicationFailure) in
            do throws(EffectRPCFailure) {
                try await serve(
                    reading: FileHandle.standardInput.bytes.jsonLines,
                    writing: { line in _ = try await withTaskExecutorPreference(output) { try FileDescriptor.standardOutput.writeAll(line) } },
                    dropped: { failure in Logger(for: Self.self).debug("Dropped \(failure, privacy: .public)") },
                ) { (request: Request) in
                    let response: Response = try await withTaskExecutorPreference(calls) {
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
