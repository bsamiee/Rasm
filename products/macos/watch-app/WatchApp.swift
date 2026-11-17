import Applications
import ArgumentParser
import CurrentProcess
import Foundation
import System

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApp: AsyncParsableCommand {
    static let configuration = CommandConfiguration(
        abstract: "Observes and drives installed applications in the background. Exits 1 after printing any fail line",
        discussion: """
            \(Program.current.name) holds its own Accessibility grant, needed by every command but find, processes, and log, and its own Screen Recording grant, needed by capture \
            and watch --record, whatever process starts it. A missing grant opens the system prompt and prints a fail line.

            Settings read \(Program.current.name.uppercased().replacing("-", with: "_"))_<KEY> from the environment, then key <key> of defaults domain \(Program.current.identifier) \
            (defaults write \(Program.current.identifier) <key> <value>): session names the journal runs share, log.dir the folder holding it and the failure log \
            (\(Program.current.directory(.log).path(percentEncoded: false))), runtime.dir the folder captures write to (\(Program.current.directory(.runtime).path(percentEncoded: false))), \
            log.level and log.filter the failure log's levels, log.size, log.age, and log.files its rotation, and OTEL_* variables OpenTelemetry export.
            """,
        version: Program.current.version ?? "",
        subcommands: [Find.self, ProcessTable.self, Watch.self, Launch.self, Read.self, Capture.self, Perform.self, SetValue.self, Key.self, Quit.self, Log.self],
    )

    static func main() async {
        do throws(Errno) { try disclaimResponsibility() } catch {
            let run = Run()
            run.fail(Program.current.name, ApplicationFailure(subject: CommandLine.arguments.joined(separator: " "), reason: .system(error)))
            await run.finish()
        }
        signal(SIGPIPE, SIG_IGN)
        do {
            var command = try parseAsRoot()
            if var command = command as? any AsyncParsableCommand { try await command.run() } else { try command.run() }
        } catch  where exitCode(for: error).isSuccess {
            exit(withError: error)
        } catch {
            let run = Run()
            run.fail(Program.current.name, ApplicationFailure(subject: CommandLine.arguments.dropFirst().joined(separator: " "), reason: .malformed(message(for: error))))
            await run.finish()
        }
    }
}
