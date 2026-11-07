public import AppKit
import Applications
public import Foundation
import Processes
import Results
import Subprocess
import System
public import Testing

// --- [MODELS] --------------------------------------------------------------------------

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
        let started: Date = .now
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
        let tested: Result<Void, any Error> = await Result {
            let launched: NSRunningApplication? = try await assemble()
            try await Self.$current.withValue(bundle) { try await Self.$instance.withValue(launched) { try await function() } }
        }
        await withTaskCancellationShield {
            let stopped: Bool = await Self.stop(identifier, at: bundle)
            for report: CrashReport in CrashReport.reports(for: identifier, since: started).reports {
                do {
                    Attachment.record(try await Attachment(contentsOf: report.url))
                    try FileManager.default.removeItem(at: report.url)
                } catch { Issue.record(error) }
            }
            guard stopped else { return }
            UserDefaults.standard.removePersistentDomain(forName: identifier)
            Self.removeCache(identifier)
            do {
                let launchServices: String = "com.apple.LaunchServices"
                guard let services: Bundle = Bundle(identifier: launchServices) else { throw TestFailure(subject: launchServices, reason: .missing) }
                try await succeed(.path(FilePath(services.bundleURL.appending(components: "Support", "lsregister").path(percentEncoded: false))), ["-u", bundle.path(percentEncoded: false)])
            } catch {
                Issue.record(error)
            }
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        try tested.get()
    }

    private static func stop(_ identifier: String, at bundle: URL) async -> Bool {
        let instances: [NSRunningApplication] = NSRunningApplication.runningApplications(withBundleIdentifier: identifier)
        return await withTaskGroup(of: Bool.self) { group in
            for instance: NSRunningApplication in instances {
                group.addTask {
                    do { try await stop(instance, at: bundle) } catch {
                        Issue.record(error)
                        return false
                    }
                    return true
                }
            }
            return await group.allSatisfy(\.self)
        }
    }

    private static func stop(_ instance: NSRunningApplication, at bundle: URL) async throws(TestFailure) {
        let process: pid_t = instance.processIdentifier
        let subject: String = "\(bundle.path(percentEncoded: false)) (PID \(process))"
        let limit: Duration = .seconds(10)
        let exited: AsyncStream<Result<ProcessExit, Errno>> = termination(of: process)
        if !instance.forceTerminate(), !instance.isTerminated {
            guard Result(catching: { () throws(Errno) in try ProcessStatus(of: process) }) == .failure(.noSuchProcess) else {
                throw TestFailure(subject: subject, reason: .terminationRefused)
            }
        }
        let expired: TestFailure = TestFailure(subject: subject, reason: .terminationTimedOut(limit))
        try await withTimeout(limit, expired: expired) { () async throws(TestFailure) in
            switch await exited.first(where: { _ in true }) {
                case .some(.success), .some(.failure(.noSuchProcess)): return
                case .some(.failure(let error)): throw TestFailure(subject: subject, reason: .host(error))
                case .none: throw expired
            }
        }
    }

    private static func removeCache(_ identifier: String) {
        let length: Int = confstr(_CS_DARWIN_USER_CACHE_DIR, nil, 0)
        var cache: [CChar] = Array(repeating: 0, count: length)
        let written: Int = unsafe confstr(_CS_DARWIN_USER_CACHE_DIR, &cache, length)
        guard written > 0, let caches: String = String(validating: cache.prefix { byte in byte != 0 }, as: UTF8.self) else { return }
        do { try FileManager.default.removeItem(at: URL(filePath: caches).appending(component: identifier)) } catch CocoaError.fileNoSuchFile {} catch {
            Issue.record(error)
        }
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
