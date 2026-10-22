public import AppKit
import AsyncAlgorithms
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
    public let scriptable: Bool
    public let scriptingDefinitionFile: String?

    public static func at(_ url: URL) -> Result<Self, DiscoveryFailure> {
        let bundle: URL = url.resolvingSymlinksInPath()
        return Result { () throws(DiscoveryFailure) in
            let (info, values): (BundleInfo?, URLResourceValues) = try Result {
                try (
                    ifPresent { try Data(contentsOf: bundle.appending(components: "Contents", "Info.plist")) }.get().map { plist in try PropertyListDecoder().decode(BundleInfo.self, from: plist) },
                    bundle.resourceValues(forKeys: [.applicationIsScriptableKey]),
                )
            }
            .mapError { error in DiscoveryFailure.unreadable(bundle, error) }.get()
            guard let identifier: String = info?.identifier else { throw .noBundleIdentifier(bundle) }
            return Self(
                url: bundle,
                identifier: identifier,
                name: FileManager.default.displayName(atPath: bundle.path(percentEncoded: false)),
                version: info?.version,
                scriptable: values.applicationIsScriptable == true,
                scriptingDefinitionFile: info?.scriptingDefinitionFile,
            )
        }
    }

    @MainActor
    public static func installed(named name: String, in directories: [URL] = []) async -> Result<[Self], AggregateError<DiscoveryFailure>> {
        let listing: @MainActor (NSComparisonPredicate.Operator) async -> Result<[Self], AggregateError<DiscoveryFailure>> = { comparison in
            await NSWorkspace.shared.applications(
                matching: NSCompoundPredicate(
                    orPredicateWithSubpredicates: [kMDItemCFBundleIdentifier as String, NSMetadataItemDisplayNameKey].map { key in
                        NSComparisonPredicate(
                            leftExpression: NSExpression(forKeyPath: key),
                            rightExpression: NSExpression(forConstantValue: name),
                            modifier: .direct,
                            type: comparison,
                            options: [.caseInsensitive, .diacriticInsensitive],
                        )
                    }
                ),
                in: directories,
            )
            .first { @Sendable _ in true } ?? .failure(AggregateError(first: .cancelled))
        }
        return switch await listing(.equalTo) {
            case .success([]):
                await listing(.contains).flatMap { rows in
                    .failure(AggregateError(first: .unmatched(Unmatched(subject: "installed application", query: name, candidates: rows.map(\.identifier)))))
                }
            case let found: found
        }
    }

    public static func single(of identifier: String, among installations: [Self]) -> Result<Self, Unmatched> {
        Unmatched.single(installations.filter { installation in installation.identifier == identifier }, subject: "installation of \(identifier)", query: nil, candidates: []) { installation in
            installation.url.path(percentEncoded: false)
        }
    }

    @concurrent
    static func listed(_ paths: [String?]) async -> Result<[Self], AggregateError<DiscoveryFailure>> {
        traverse(paths) { path in (path.map { path in at(URL(filePath: path, directoryHint: .isDirectory)) } ?? .failure(.noPath)).aggregated { error in error } }
    }
}

extension NSMetadataQuery {
    struct DidUpdateMessage: NotificationCenter.AsyncMessage {
        typealias Subject = NSMetadataQuery

        static var name: Notification.Name { .NSMetadataQueryDidUpdate }

        static func makeMessage(_: Notification) -> Self? { Self() }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum DiscoveryFailure: CustomNSError, LocalizedError {
    case noPath
    case noBundleIdentifier(URL)
    case unreadable(URL, any Error)
    case unmatched(Unmatched)
    case cancelled

    public var errorDescription: String? {
        switch self {
            case .noPath: "Spotlight result has no kMDItemPath"
            case .noBundleIdentifier(let url): "\(url.path(percentEncoded: false)) has no CFBundleIdentifier"
            case .unreadable(let url, let error): "\(url.path(percentEncoded: false)): \(error.localizedDescription)"
            case .unmatched(let miss): miss.errorDescription
            case .cancelled: "Canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .unreadable(_, let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .noPath, .noBundleIdentifier, .cancelled: [:]
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    @MainActor
    public func applications(matching predicate: NSPredicate, in directories: [URL] = []) -> some AsyncSequence<Result<[Installation], AggregateError<DiscoveryFailure>>, Never> & Sendable {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task { @MainActor in
                let query: NSMetadataQuery = NSMetadataQuery()
                query.predicate = NSCompoundPredicate(andPredicateWithSubpredicates: [NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier), predicate])
                query.searchScopes = directories
                query.sortDescriptors = [NSMetadataItemDisplayNameKey, NSMetadataItemVersionKey].map { key in NSSortDescriptor(key: key, ascending: true) }
                query.operationQueue = .main
                let changes: some AsyncSequence<Void, Never> = merge(
                    NotificationCenter.default.messages(of: query, for: .didFinishGathering).map { _ in () },
                    NotificationCenter.default.messages(of: query, for: NSMetadataQuery.DidUpdateMessage.self).map { _ in () },
                )
                query.start()
                defer { query.stop() }
                for await _: Void in changes {
                    query.disableUpdates()
                    let paths: [String?] = (0..<query.resultCount).map { index in (query.result(at: index) as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String }
                    query.enableUpdates()
                    continuation.yield(await Installation.listed(paths))
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
        .removeDuplicates { previous, next in if case (.success(let previous), .success(let next)) = (previous, next) { previous == next } else { false } }
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
