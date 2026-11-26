public import AppKit
import AsyncAlgorithms
import CasePathsCore
import OrderedCollections
import Results
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

extension Installation {
    public struct Listing: Encodable, Equatable, Sendable {
        public let installations: [Installation]
        public let unreadable: [ApplicationFailure]
    }
}

extension NSMetadataQuery {
    struct DidUpdateMessage: NotificationCenter.AsyncMessage {
        typealias Subject = NSMetadataQuery

        static var name: Notification.Name { .NSMetadataQueryDidUpdate }

        static func makeMessage(_: Notification) -> Self? { Self() }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSPredicate {
    public func evaluate(bundleIdentifier: String?) -> Bool {
        bundleIdentifier.map { identifier in evaluate(with: [NSMetadataItemCFBundleIdentifierKey: identifier]) } == true
    }

    fileprivate func metadataQueryPredicate(within candidates: NSPredicate) -> NSPredicate {
        let compound = self as? NSCompoundPredicate
        let subpredicates = (compound?.subpredicates ?? []).compactMap { subpredicate in (subpredicate as? NSPredicate)?.metadataQueryPredicate(within: candidates) }
        return switch (compound?.compoundPredicateType, subpredicates.first, subpredicates.count) {
            case (.not, let subpredicate?, _): NSCompoundPredicate(notPredicateWithSubpredicate: subpredicate)
            case (.and, _, 0): candidates
            case (.or, _, 0): NSCompoundPredicate(notPredicateWithSubpredicate: candidates)
            case (_?, let subpredicate?, 1): subpredicate
            case (let type?, _, _): NSCompoundPredicate(type: type, subpredicates: subpredicates)
            case (nil, _, _) where self == NSPredicate(value: true): candidates
            case (nil, _, _) where self == NSPredicate(value: false): NSCompoundPredicate(notPredicateWithSubpredicate: candidates)
            case (nil, _, _): self
        }
    }
}

extension Installation {
    @MainActor
    public static func search(_ predicate: NSPredicate, in directories: [URL] = []) -> some AsyncSequence<Listing, Never> & Sendable {
        let (listings, continuation) = AsyncStream<Listing>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let following = Task { @MainActor in
            defer { continuation.finish() }
            let applications = NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier)
            let query = NSMetadataQuery()
            query.predicate = NSCompoundPredicate(andPredicateWithSubpredicates: [applications, predicate.metadataQueryPredicate(within: applications)])
            query.searchScopes = directories
            query.sortDescriptors = [NSSortDescriptor(key: NSMetadataItemDisplayNameKey, ascending: true)]
            query.operationQueue = .main
            var gathered = NotificationCenter.default.messages(of: query, for: .didFinishGathering, bufferSize: 1).makeAsyncIterator()
            query.start()
            defer { query.stop() }
            guard await gathered.next(isolation: #isolation) != nil else { return }
            let (changes, changed) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
            let observation = NSWorkspace.shared.observe(\.runningApplications, options: [.old, .new]) { _, change in
                if [change.oldValue, change.newValue].compactMap(\.self).joined().contains(where: { application in application.activationPolicy == .regular }) { changed.yield() }
            }
            defer { observation.invalidate() }
            var updates = merge(NotificationCenter.default.messages(of: query, for: NSMetadataQuery.DidUpdateMessage.self, bufferSize: 1).map { _ in }, changes).makeAsyncIterator()
            var held: [URL: (modified: Date?, read: Result<Self, ApplicationFailure>)] = [:]
            repeat {
                query.disableUpdates()
                let indexed: [URL?] = query.results.map { result in
                    ((result as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String).map { path in URL(filePath: path, directoryHint: .isDirectory) }
                }
                query.enableUpdates()
                let running = OrderedSet(
                    NSWorkspace.shared.runningApplications.filter { application in
                        application.activationPolicy == .regular && predicate.evaluate(bundleIdentifier: application.bundleIdentifier)
                    }
                    .compactMap(\.bundleURL)
                )
                .subtracting(indexed.compactMap(\.self))
                let (listing, reads) = await read(indexed + running.map(Optional.some), reusing: held)
                held = reads
                continuation.yield(listing)
            } while await updates.next(isolation: #isolation) != nil
        }
        continuation.onTermination = { _ in following.cancel() }
        return listings.removeDuplicates()
    }

    public static func handlers(of document: URL, matching predicate: NSPredicate) async throws(ApplicationFailure) -> Listing {
        let subject = document.isFileURL ? document.path(percentEncoded: false) : document.absoluteString
        if document.isFileURL {
            do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
        }
        let (listing, _) = await read(
            OrderedSet([NSWorkspace.shared.urlForApplication(toOpen: document)].compactMap(\.self) + NSWorkspace.shared.urlsForApplications(toOpen: document)).map(Optional.some)
        )
        let matched = listing.installations.filter { installation in predicate.evaluate(bundleIdentifier: installation.identifier) }
        guard !matched.isEmpty else {
            throw ApplicationFailure(
                subject: subject,
                reason: .unmatched(Unmatched(subject: "application opening \(subject)", query: predicate.predicateFormat, candidates: listing.installations.map(\.name))),
            )
        }
        return Listing(installations: matched, unreadable: listing.unreadable)
    }

    @concurrent
    private static func read(
        _ bundles: [URL?],
        reusing held: [URL: (modified: Date?, read: Result<Self, ApplicationFailure>)] = [:],
    ) async -> (listing: Listing, held: [URL: (modified: Date?, read: Result<Self, ApplicationFailure>)]) {
        let reads: [(url: URL?, modified: Date?, read: Result<Self, ApplicationFailure>)] = bundles.enumerated().map { index, url in
            let modified = url.flatMap { url in try? url.appending(components: "Contents", "Info.plist").resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate }
            return switch (url, url.flatMap { url in held[url] }) {
                case (.some(let url), .some(let kept)) where modified != nil && kept.modified == modified: (url, modified, kept.read)
                case (.some(let url), _): (url, modified, Result { () throws(ApplicationFailure) in try Self(at: url) })
                case (.none, _): (nil, nil, .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath)))
            }
        }
        return (
            Listing(installations: OrderedSet(reads.compactMap { read in read.read[case: \.success] }).elements, unreadable: reads.compactMap { read in read.read[case: \.failure] }),
            Dictionary(reads.compactMap { read in read.url.map { url in (url, (modified: read.modified, read: read.read)) } }) { first, _ in first },
        )
    }
}
