public import AppKit
import AsyncAlgorithms
import CasePathsCore
public import Results
import System
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
        func readable(_ listed: (installations: [Self], failures: [ApplicationFailure])) throws(ApplicationFailure) -> [Self] {
            if let failure = listed.failures.first, listed.installations.isEmpty { throw failure }
            return listed.installations
        }
        func listing(_ key: String, _ comparison: NSComparisonPredicate.Operator, _ value: String) async throws(ApplicationFailure) -> [Self] {
            let matching = NSComparisonPredicate(
                leftExpression: NSExpression(forKeyPath: key),
                rightExpression: NSExpression(forConstantValue: value),
                modifier: .direct,
                type: comparison,
                options: [.caseInsensitive, .diacriticInsensitive],
            )
            guard let listed = await listings(matching, in: directories).first(where: { @Sendable _ in true }) else {
                throw ApplicationFailure(subject: "Spotlight query for \(name)", reason: .cancelled)
            }
            return try readable(listed)
        }
        let path = FilePath(name)
        let located =
            URL(string: name).flatMap { url in url.isFileURL ? url : nil }
            ?? (path.removingLastComponent().isEmpty && path.lastComponent?.kind == .regular ? nil : URL(filePath: name, directoryHint: .isDirectory))
        if let located { return [try Self(at: located)] }
        if let pid = pid_t(name) {
            guard let bundle = NSRunningApplication(processIdentifier: pid)?.bundleURL else { throw ApplicationFailure(subject: name, reason: .noProcess) }
            return [try Self(at: bundle)]
        }
        let registered = try readable(await listed(NSWorkspace.shared.urlsForApplications(withBundleIdentifier: name).map { url in url.path(percentEncoded: false) }))
        guard registered.isEmpty else { return registered }
        let filed = try await listing(NSMetadataItemFSNameKey, .equalTo, URL(filePath: name).appendingPathExtension(for: .applicationBundle).lastPathComponent)
        let named = try await filed.isEmpty ? listing(NSMetadataItemDisplayNameKey, .equalTo, name) : filed
        guard named.isEmpty else { return named }
        throw ApplicationFailure(subject: name, reason: .unnamed(try await listing(NSMetadataItemDisplayNameKey, .contains, name).map(\.url)))
    }

    private static func metadataForm(of predicate: NSPredicate) -> NSPredicate? {
        guard predicate != NSPredicate(value: true) else { return nil }
        guard let compound = predicate as? NSCompoundPredicate, compound.compoundPredicateType != .not else { return predicate }
        let clauses = compound.subpredicates.compactMap { $0 as? NSPredicate }.compactMap(metadataForm(of:))
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
                query.sortDescriptors = [NSSortDescriptor(key: NSMetadataItemDisplayNameKey, ascending: true)]
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
                    let paths = (0..<query.resultCount).map { index in (query.result(at: index) as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String }
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
        let reads = paths.enumerated().map { index, path in
            path.map { path in Result { () throws(ApplicationFailure) in try Self(at: URL(filePath: path, directoryHint: .isDirectory)) } }
                ?? .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath))
        }
        return (reads.compactMap { read in read[case: \.success] }, reads.compactMap { read in read[case: \.failure] })
    }
}

extension NSWorkspace {
    @MainActor
    public func instances(named name: String) async throws(ApplicationFailure) -> [NSRunningApplication] {
        if let pid = pid_t(name) { return [NSRunningApplication(processIdentifier: pid)].compactMap(\.self) }
        let found = try await Installation.installed(named: name)
        return instances { instance in found.contains { installation in installation.runs(instance) } }
    }
}
