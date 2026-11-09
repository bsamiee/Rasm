public import AppKit
import AsyncAlgorithms
import CasePathsCore
import CoreServices
public import Results
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

extension NSMetadataQuery {
    struct DidUpdateMessage: NotificationCenter.AsyncMessage {
        typealias Subject = NSMetadataQuery

        static var name: Notification.Name { .NSMetadataQueryDidUpdate }

        static func makeMessage(_: Notification) -> Self? { Self() }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    @MainActor
    public static func search(_ predicate: NSPredicate, in directories: [URL] = []) -> some AsyncSequence<Result<[Self], AggregateError<ApplicationFailure>>, Never> & Sendable {
        listings(predicate, in: directories)
            .map { listed in AggregateError(listed.failures).map(Result.failure) ?? .success(listed.installations) }
            .removeDuplicates { previous, next in if case (.success(let previous), .success(let next)) = (previous, next) { previous == next } else { false } }
    }

    @MainActor
    public static func installed(named name: String, in directories: [URL] = []) async throws(ApplicationFailure) -> [Self] {
        func listing(_ comparison: NSComparisonPredicate.Operator) async throws(ApplicationFailure) -> [Self] {
            let matching = NSCompoundPredicate(
                orPredicateWithSubpredicates: [kMDItemCFBundleIdentifier as String, NSMetadataItemDisplayNameKey].map { key in
                    NSComparisonPredicate(
                        leftExpression: NSExpression(forKeyPath: key),
                        rightExpression: NSExpression(forConstantValue: name),
                        modifier: .direct,
                        type: comparison,
                        options: [.caseInsensitive, .diacriticInsensitive],
                    )
                }
            )
            guard let listed: [Self] = await listings(matching, in: directories).first(where: { @Sendable _ in true })?.installations else {
                throw ApplicationFailure(subject: "Spotlight query for \(name)", reason: .cancelled)
            }
            return listed
        }
        if name.hasPrefix("/") { return [try Self(at: URL(filePath: name, directoryHint: .isDirectory))] }
        if let pid: pid_t = pid_t(name) {
            guard let bundle: URL = NSRunningApplication(processIdentifier: pid)?.bundleURL else { throw ApplicationFailure(subject: name, reason: .noProcess) }
            return [try Self(at: bundle)]
        }
        let registered: [Self] = NSWorkspace.shared.urlsForApplications(withBundleIdentifier: name).compactMap { url in
            Result { () throws(ApplicationFailure) in try Self(at: url) }[case: \.success]
        }
        guard registered.isEmpty else { return registered }
        let exact: [Self] = try await listing(.equalTo)
        let found: [Self] = try await exact.isEmpty ? listing(.contains) : exact
        guard found.isEmpty else { return found }
        throw ApplicationFailure(subject: name, reason: .unmatched(Unmatched(subject: "installed application", query: name, candidates: [])))
    }

    private static func metadataForm(of predicate: NSPredicate) -> NSPredicate? {
        guard predicate != NSPredicate(value: true) else { return nil }
        guard let compound = predicate as? NSCompoundPredicate, compound.compoundPredicateType != .not else { return predicate }
        let clauses: [NSPredicate] = compound.subpredicates.compactMap { $0 as? NSPredicate }.compactMap(metadataForm(of:))
        return if compound.compoundPredicateType == .or, clauses.count < compound.subpredicates.count {
            nil
        } else if clauses.count > 1 {
            NSCompoundPredicate(type: compound.compoundPredicateType, subpredicates: clauses)
        } else {
            clauses.first
        }
    }

    @MainActor
    private static func listings(_ predicate: NSPredicate, in directories: [URL]) -> some AsyncSequence<(installations: [Self], failures: [ApplicationFailure]), Never> & Sendable {
        AsyncStream { continuation in
            let following = Task { @MainActor in
                let query = NSMetadataQuery()
                let application = NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier)
                query.predicate = metadataForm(of: NSCompoundPredicate(andPredicateWithSubpredicates: [application, predicate])) ?? application
                query.searchScopes = directories
                query.sortDescriptors = [NSMetadataItemDisplayNameKey, NSMetadataItemVersionKey].map { key in NSSortDescriptor(key: key, ascending: true) }
                query.operationQueue = .main
                let (changes, change) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
                let observations: [NotificationCenter.ObservationToken] = [
                    NotificationCenter.default.addObserver(of: query, for: .didFinishGathering) { _ in change.yield() },
                    NotificationCenter.default.addObserver(of: query, for: NSMetadataQuery.DidUpdateMessage.self) { _ in change.yield() },
                ]
                query.start()
                defer {
                    query.stop()
                    observations.forEach(NotificationCenter.default.removeObserver)
                }
                for await _ in changes {
                    query.disableUpdates()
                    let paths: [String?] = (0..<query.resultCount).map { index in (query.result(at: index) as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String }
                    query.enableUpdates()
                    continuation.yield(await Self.listed(paths))
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }

    @concurrent
    static func listed(_ paths: [String?]) async -> (installations: [Self], failures: [ApplicationFailure]) {
        let reads: [Result<Self, ApplicationFailure>] = paths.enumerated().map { index, path in
            path.map { path in Result { () throws(ApplicationFailure) in try Self(at: URL(filePath: path, directoryHint: .isDirectory)) } }
                ?? .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath))
        }
        return (reads.compactMap { read in read[case: \.success] }, reads.compactMap { read in read[case: \.failure] })
    }
}
