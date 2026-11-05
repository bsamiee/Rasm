public import AppKit
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
    @TaskLocal public static var instance: NSRunningApplication?

    let product: String
    let named: String?
    let environment: [String: String]
    let launches: Bool
    let activationPolicy: NSApplication.ActivationPolicy

    public var isRecursive: Bool { true }

    public func provideScope(for test: Test, testCase: Test.Case?, performing function: @concurrent @Sendable () async throws -> Void) async throws {
        let tag: String = UUID().uuidString
        let name: String = named ?? "\(product) \(tag.prefix(8))"
        let identifier: String = "\(product).\(tag)"
        let process: String = "\(product)-\(tag.prefix(8))"
        let folder: URL = try FileManager.default.url(for: .cachesDirectory, in: .userDomainMask, appropriateFor: nil, create: true).appending(component: identifier, directoryHint: .isDirectory)
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: false)
        let bundle: URL = folder.appending(component: "\(name).app", directoryHint: .isDirectory)
        func succeed(_ executable: Executable, _ arguments: [String]) async throws {
            let result: (standardOutput: String, status: TerminationStatus, standardError: String) = try await output(of: executable, arguments: arguments)
            guard result.status.isSuccess else {
                throw TestFailure(subject: "\(executable) \(arguments.joined(separator: " "))", reason: .exited(result.status, standardError: result.standardError))
            }
        }
        func assemble() async throws -> NSRunningApplication? {
            let executable: URL = bundle.appending(components: "Contents", "MacOS", process)
            try FileManager.default.createDirectory(at: executable.deletingLastPathComponent(), withIntermediateDirectories: true)
            try FileManager.default.copyItem(at: builtProduct(named: product), to: executable)
            let info: [String: Any] = [
                kCFBundleIdentifierKey as String: identifier, kCFBundleNameKey as String: name, kCFBundleExecutableKey as String: process, "CFBundlePackageType": "APPL",
                "LSUIElement": activationPolicy == .accessory, "LSBackgroundOnly": activationPolicy == .prohibited,
            ]
            try PropertyListSerialization.data(fromPropertyList: info, format: .xml, options: 0).write(to: bundle.appending(components: "Contents", "Info.plist"))
            try await succeed(.name("codesign"), ["--sign", "-", "--force", bundle.path(percentEncoded: false)])
            let registered: OSStatus = LSRegisterURL(bundle as CFURL, true)
            guard registered == noErr else { throw TestFailure(subject: "register \(bundle.path(percentEncoded: false))", reason: .refused(registered)) }
            guard launches else { return nil }
            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
            configuration.activates = false
            configuration.addsToRecentItems = false
            configuration.environment = environment
            return try await NSWorkspace.shared.openApplication(at: bundle, configuration: configuration)
        }
        func stop() async {
            for instance: NSRunningApplication in NSRunningApplication.runningApplications(withBundleIdentifier: identifier) {
                instance.forceTerminate()
                let (terminated, terminate): (AsyncStream<Never>, AsyncStream<Never>.Continuation) = AsyncStream.makeStream()
                let observation: NSKeyValueObservation = instance.observe(\.isTerminated, options: [.initial]) { observed, _ in
                    if observed.isTerminated { terminate.finish() }
                }
                for await _: Never in terminated {}
                observation.invalidate()
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
            let launched: NSRunningApplication? = try await assemble()
            try await Self.$current.withValue(bundle) { try await Self.$instance.withValue(launched) { try await function() } }
        }
        await withTaskCancellationShield {
            await stop()
            let decoder: JSONDecoder = JSONDecoder()
            let crashes: [URL] = entries(of: URL.libraryDirectory.appending(components: "Logs", "DiagnosticReports")).filter { file in
                guard file.pathExtension == "ips", let report: Data = FileManager.default.contents(atPath: file.path(percentEncoded: false)) else { return false }
                let header: ReportHeader? = try? decoder.decode(ReportHeader.self, from: report.prefix { byte in byte != UInt8(ascii: "\n") })
                return header?.bundleID == identifier
            }
            remove(crashes)
            UserDefaults.standard.removePersistentDomain(forName: identifier)
            let length: Int = confstr(_CS_DARWIN_USER_CACHE_DIR, nil, 0)
            var cache: [CChar] = Array(repeating: 0, count: length)
            let written: Int = unsafe confstr(_CS_DARWIN_USER_CACHE_DIR, &cache, length)
            if written > 0, let caches: String = String(validating: cache.prefix { byte in byte != 0 }, as: UTF8.self) {
                remove(entries(of: URL(filePath: caches)).filter { entry in entry.lastPathComponent == identifier })
            }
            do {
                let launchServices: String = "com.apple.LaunchServices"
                guard let services: Bundle = Bundle(identifier: launchServices) else { throw TestFailure(subject: launchServices, reason: .missing) }
                try await succeed(.path(FilePath(services.bundleURL.appending(components: "Support", "lsregister").path(percentEncoded: false))), ["-u", bundle.path(percentEncoded: false)])
            } catch {
                Issue.record(error)
            }
            remove([folder])
        }
        try tested.get()
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Trait where Self == TestApplication {
    public static func application(
        _ entry: any Any.Type,
        named name: String? = nil,
        environment: [String: String] = [:],
        launches: Bool = true,
        activationPolicy: NSApplication.ActivationPolicy = .regular,
    ) -> Self {
        let product: String = String(String(reflecting: entry).prefix { character in character != "." })
        return Self(
            product: product,
            named: name,
            environment: environment,
            launches: launches,
            activationPolicy: activationPolicy,
        )
    }
}
