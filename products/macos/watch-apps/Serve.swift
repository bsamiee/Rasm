import AppKit
import ApplicationServer
import Applications
import ArgumentParser
import Encoding
import JSONRPC
import System

// --- [MODELS] --------------------------------------------------------------------------

private enum Journaled: Decodable, Sendable {
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Serve: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Serves ApplicationServer requests as JSON lines over standard input and output, under its parent's privacy grants, journaling launched instances"
    )

    func run() async {
        await Run.execute(Self.self, output: nil, accessibility: false) { run throws(ApplicationFailure) in
            do throws(JSONRPCFailure) {
                try await serve(
                    reading: FileHandle.standardInput.bytes.jsonLines,
                    writing: { line in _ = try FileDescriptor.standardOutput.writeAll(line) },
                    admitting: { NSPredicate(value: true) },
                    refusing: { _ in [] },
                ) { (request: Journaled) in
                    switch request {
                        case .launch(let url, let documents, let arguments, let environment, let new):
                            .stream(
                                try NSWorkspace.shared.launching(Installation(at: url), opening: documents, arguments: arguments, environment: environment, newInstance: new)
                                    .map { step -> any Encodable & Sendable in
                                        if case .launched(let launched) = step { run.show(step, as: launched.instance.key) }
                                        return step
                                    }
                            )
                    }
                }
            } catch {
                throw ApplicationFailure(subject: "connection", reason: .host(error as NSError))
            }
        }
    }
}
