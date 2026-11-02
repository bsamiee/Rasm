public import AppKit
import Combine
public import Foundation
import Subprocess
import System
public import Testing

// --- [MODELS] --------------------------------------------------------------------------

private struct ReportHeader: Decodable {
    let bundleID: String?
}

public struct TestApplication: SuiteTrait, TestTrait, TestScoping {
    @TaskLocal public static var current: URL?

    let product: String
    let named: String?
    let arguments: [String]
    let launches: Bool
    let activationPolicy: NSApplication.ActivationPolicy
    let multipleInstancesProhibited: Bool
    let answering: @Sendable (_ name: String) async -> Void

    public var isRecursive: Bool { true }

    public func provideScope(for test: Test, testCase: Test.Case?, performing function: @concurrent @Sendable () async throws -> Void) async throws {
        let tag: String = UUID().uuidString
        let name: String = named ?? "\(product) \(tag.prefix(8))"
        let identifier: String = "\(product).\(tag)"
        let process: String = "\(product)-\(tag.prefix(8))"
        let folder: URL = try FileManager.default.url(for: .userDirectory, in: .localDomainMask, appropriateFor: nil, create: false)
            .appending(components: "Shared", identifier, directoryHint: .isDirectory)
        let bundle: URL = folder.appending(component: "\(name).app", directoryHint: .isDirectory)
        func succeed(_ executable: Executable, _ arguments: [String]) async throws {
            let result: ExecutionResult<Void, DiscardedOutput, StringOutput<UTF8>> = try await Subprocess.run(
                executable,
                arguments: Arguments(arguments),
                platformOptions: session(),
                output: .discarded,
                error: .string(limit: 1 << 16),
            )
            guard result.terminationStatus.isSuccess else {
                throw TestFailure(subject: "\(executable) \(arguments.joined(separator: " "))", reason: .exited(result.terminationStatus, standardError: result.standardError))
            }
        }
        func assemble() async throws {
            let executable: URL = bundle.appending(components: "Contents", "MacOS", process)
            try FileManager.default.createDirectory(at: executable.deletingLastPathComponent(), withIntermediateDirectories: true)
            try FileManager.default.copyItem(at: builtProduct(named: product), to: executable)
            let info: [String: Any] = [
                kCFBundleIdentifierKey as String: identifier, kCFBundleNameKey as String: name, kCFBundleExecutableKey as String: process, "CFBundlePackageType": "APPL",
                "LSMultipleInstancesProhibited": multipleInstancesProhibited, "LSUIElement": activationPolicy == .accessory, "LSBackgroundOnly": activationPolicy == .prohibited,
            ]
            try PropertyListSerialization.data(fromPropertyList: info, format: .xml, options: 0).write(to: bundle.appending(components: "Contents", "Info.plist"))
            try await succeed(.name("codesign"), ["--sign", "-", "--force", bundle.path(percentEncoded: false)])
            let registered: OSStatus = LSRegisterURL(bundle as CFURL, true)
            guard registered == noErr else { throw TestFailure(subject: "register \(bundle.path(percentEncoded: false))", reason: .refused(registered)) }
            guard launches else { return }
            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
            configuration.activates = false
            configuration.addsToRecentItems = false
            configuration.hides = activationPolicy != .regular
            configuration.arguments = arguments
            let _: NSRunningApplication = try await NSWorkspace.shared.openApplication(at: bundle, configuration: configuration)
        }
        func stop() async {
            let running: [NSRunningApplication] = NSRunningApplication.runningApplications(withBundleIdentifier: identifier)
            for instance: NSRunningApplication in running { instance.forceTerminate() }
            for instance: NSRunningApplication in running {
                _ = await instance.publisher(for: \.isTerminated).buffer(size: 1, prefetch: .byRequest, whenFull: .dropOldest).values.first(where: \.self)
            }
        }
        func entries(of directory: URL) -> [URL] {
            do { return try FileManager.default.contentsOfDirectory(at: directory, includingPropertiesForKeys: nil) } catch {
                Issue.record(error)
                return []
            }
        }
        func remove(_ owned: [URL]) {
            for url: URL in owned {
                do { try FileManager.default.removeItem(at: url) } catch { Issue.record(error) }
            }
        }
        let tested: Result<Void, any Error> = await Result {
            try await assemble()
            try await Self.$current.withValue(bundle) { try await function() }
        }
        await withTaskCancellationShield {
            await stop()
            let decoder: JSONDecoder = JSONDecoder()
            let crashes: [URL] = entries(of: URL.libraryDirectory.appending(components: "Logs", "DiagnosticReports")).filter { file in
                guard file.pathExtension == "ips", let report: Data = FileManager.default.contents(atPath: file.path(percentEncoded: false)) else { return false }
                let header: ReportHeader? = try? decoder.decode(ReportHeader.self, from: report.prefix { byte in byte != UInt8(ascii: "\n") })
                return header?.bundleID == identifier
            }
            if !crashes.isEmpty { await answering(name) }
            remove(crashes)
            UserDefaults.standard.removePersistentDomain(forName: identifier)
            let length: Int = confstr(_CS_DARWIN_USER_CACHE_DIR, nil, 0)
            var cache: [CChar] = Array(repeating: 0, count: length)
            let written: Int = unsafe confstr(_CS_DARWIN_USER_CACHE_DIR, &cache, length)
            if written > 0, let caches: String = String(bytes: cache.prefix { byte in byte != 0 }.map(UInt8.init(bitPattern:)), encoding: .utf8) {
                remove(entries(of: URL(filePath: caches)).filter { entry in entry.lastPathComponent == identifier })
            }
            do {
                let launchServices: String = "com.apple.LaunchServices"
                guard let services: Bundle = Bundle(identifier: launchServices) else { throw TestFailure(subject: launchServices, reason: .missing) }
                try await succeed(.path(FilePath(services.bundleURL.appending(components: "Support", "lsregister").path(percentEncoded: false))), ["-u", bundle.path(percentEncoded: false)])
            } catch {
                Issue.record(error)
            }
            remove(entries(of: folder.deletingLastPathComponent()).filter { entry in entry.lastPathComponent == identifier })
        }
        try tested.get()
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Trait where Self == TestApplication {
    public static func application(
        _ entry: any Any.Type,
        named name: String? = nil,
        arguments: [String] = [],
        launches: Bool = true,
        activationPolicy: NSApplication.ActivationPolicy = .accessory,
        multipleInstancesProhibited: Bool = false,
        answering alerts: @escaping @Sendable (_ name: String) async -> Void = { _ in },
    ) -> Self {
        let product: String = String(String(reflecting: entry).prefix { character in character != "." })
        return Self(
            product: product,
            named: name,
            arguments: arguments,
            launches: launches,
            activationPolicy: activationPolicy,
            multipleInstancesProhibited: multipleInstancesProhibited,
            answering: alerts,
        )
    }
}
