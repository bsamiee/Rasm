public import AppKit
import Files
public import Results
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

private struct BundleInfo: Decodable {
    private enum CodingKeys: String, CodingKey {
        case identifier = "CFBundleIdentifier"
        case version = "CFBundleShortVersionString"
        case scriptingDefinitionFile = "OSAScriptingDefinition"
    }

    let identifier: String?
    let version: String?
    let scriptingDefinitionFile: String?
}

public struct Installation: Encodable, Hashable, Sendable {
    public let url: URL
    public let identifier: String
    public let name: String
    public let version: String?
    public let instances: [pid_t]
    public let scriptable: Bool
    public let scriptingDefinitionFile: String?

    public static func at(_ url: URL, among runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications) -> Result<Self, DiscoveryFailure> {
        let bundle: URL = url.resolvingSymlinksInPath()
        return Result { () throws(DiscoveryFailure) in
            let (info, values): (BundleInfo?, URLResourceValues) = try Result {
                try (
                    ifPresent { try Data(contentsOf: bundle.appending(components: "Contents", "Info.plist")) }.get().map { plist in try PropertyListDecoder().decode(BundleInfo.self, from: plist) },
                    bundle.resourceValues(forKeys: [.applicationIsScriptableKey]),
                )
            }
            .mapError { error in DiscoveryFailure.unreadable(bundle, error) }.get()
            guard let identifier: String = info?.identifier else { throw .unbundled(bundle) }
            return Self(
                url: bundle,
                identifier: identifier,
                name: FileManager.default.displayName(atPath: bundle.path(percentEncoded: false)),
                version: info?.version,
                instances: NSRunningApplication.instances(at: bundle, among: runningApplications).map(\.processIdentifier),
                scriptable: values.applicationIsScriptable == true,
                scriptingDefinitionFile: info?.scriptingDefinitionFile,
            )
        }
    }

    @concurrent
    static func listed(_ paths: [String?], among runningApplications: [NSRunningApplication]) async -> Result<[Self], AggregateError<DiscoveryFailure>> {
        traverse(paths) { path in (path.map { path in at(URL(filePath: path, directoryHint: .isDirectory), among: runningApplications) } ?? .failure(.unpathed)).aggregated { error in error } }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum DiscoveryFailure: CustomNSError, LocalizedError {
    case unpathed
    case unbundled(URL)
    case unreadable(URL, any Error)

    public var errorDescription: String? {
        switch self {
            case .unpathed: "Spotlight listed an application bundle without a path"
            case .unbundled(let url): "\(url.path(percentEncoded: false)) holds no application bundle with an identifier"
            case .unreadable(let url, let error): "Launch Services could not read \(url.path(percentEncoded: false)): \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .unreadable(_, let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    @MainActor
    public func applications(matching predicate: NSPredicate, in directories: [URL] = []) -> AsyncStream<Result<[Installation], AggregateError<DiscoveryFailure>>> {
        let query: NSMetadataQuery = NSMetadataQuery()
        query.predicate = NSCompoundPredicate(andPredicateWithSubpredicates: [NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier), predicate])
        query.searchScopes = directories
        query.sortDescriptors = [NSMetadataItemDisplayNameKey, NSMetadataItemVersionKey].map { key in NSSortDescriptor(key: key, ascending: true) }
        query.operationQueue = .main
        let (events, sink): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let (listings, continuation): (AsyncStream<Result<[Installation], AggregateError<DiscoveryFailure>>>, AsyncStream<Result<[Installation], AggregateError<DiscoveryFailure>>>.Continuation) =
            AsyncStream.makeStream()
        let observation: NSKeyValueObservation = observe(\.runningApplications) { _, _ in sink.yield() }
        let notes: any NSObjectProtocol = NotificationCenter.default.addObserver(forName: nil, object: query, queue: nil) { _ in sink.yield() }
        let following: Task<Void, Never> = Task { @MainActor in
            defer {
                query.stop()
                observation.invalidate()
                NotificationCenter.default.removeObserver(notes)
                continuation.finish()
            }
            query.start()
            var last: [Installation]?
            for await _: Void in events where !query.isGathering {
                query.disableUpdates()
                let paths: [String?] = (0..<query.resultCount).map { index in query.value(ofAttribute: NSMetadataItemPathKey, forResultAt: index) as? String }
                query.enableUpdates()
                let listing: Result<[Installation], AggregateError<DiscoveryFailure>> = await Installation.listed(paths, among: runningApplications)
                switch listing {
                    case .success(last): continue
                    case .success(let rows): last = rows
                    case .failure: break
                }
                continuation.yield(listing)
            }
        }
        continuation.onTermination = { _ in following.cancel() }
        return listings
    }
}

extension NSRunningApplication {
    var createsWindows: Bool { activationPolicy != .prohibited }

    var launchedWithWindows: Bool { isFinishedLaunching && createsWindows }

    public static func instances(withBundleIdentifier identifier: String) -> [NSRunningApplication] {
        runningApplications(withBundleIdentifier: identifier).filter(\.launchedWithWindows)
    }

    public static func instances(at url: URL, among runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications) -> [NSRunningApplication] {
        let path: String = url.path(percentEncoded: false)
        return runningApplications.filter { instance in instance.launchedWithWindows && instance.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == path }
    }
}
