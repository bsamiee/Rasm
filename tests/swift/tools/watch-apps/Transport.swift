import Encoding
import Foundation
import Processes
import Subprocess
import TestSupport
import Testing

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Transport {
    @Test(.timeLimit(.minutes(1)))
    func returnsACompleteReplyLargerThanThePipe() async throws {
        let id: String = String(repeating: "x", count: 262_144)
        let request: Data = try JSONEncoder().encodeLine(["id": id, "method": "unrecognized"])
        let run: ExecutionResult<[String: JSONValue]?, SequenceOutput, DiscardedOutput> = try await Subprocess.run(
            .product(named: "watch-apps"),
            arguments: ["serve"],
            platformOptions: .session(),
            input: .inputWriter,
            output: .sequence,
            error: .discarded,
        ) { execution in
            _ = try await execution.standardInputWriter.write(request)
            for try await line: String in execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"]), bufferingPolicy: .unbounded) where !line.isEmpty {
                let reply: [String: JSONValue] = try JSONDecoder().decode([String: JSONValue].self, from: Data(line.utf8))
                try await execution.standardInputWriter.finish()
                return reply
            }
            return nil
        }
        let reply: [String: JSONValue] = try #require(run.closureResult)
        let correlated: Bool = reply["id"] == .string(id)
        let succeeded: Bool = run.terminationStatus == .exited(0)
        #expect(correlated)
        #expect(reply["error"] != nil)
        #expect(succeeded)
    }
}
