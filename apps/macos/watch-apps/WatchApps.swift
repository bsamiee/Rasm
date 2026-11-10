import Applications
import ArgumentParser
import CurrentProcess
import Foundation

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Observes and drives installed applications in the background. Exits 1 after printing any fail line",
        version: Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String ?? "",
        subcommands: [Find.self, Watch.self, Launch.self, Read.self, Capture.self, Perform.self, SetValue.self, Key.self, Quit.self, Log.self, Serve.self],
    )

    static func main() async {
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
