import AppKit
import AsyncAlgorithms
import CasePathsCore
import Files
package import Foundation
import OrderedCollections
import Results
import Synchronization
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

extension Installation {
    package struct Listing: Encodable, Equatable, Sendable {
        let installations: [Installation]
        let unreadable: [ApplicationFailure]
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
    package func evaluate(bundleIdentifier: String?) -> Bool {
        bundleIdentifier.map { identifier in evaluate(with: [NSMetadataItemCFBundleIdentifierKey: identifier]) } == true
    }

    fileprivate func metadataQueryPredicate(within candidates: NSPredicate, negated: Bool = false) -> NSPredicate {
        let compound = self as? NSCompoundPredicate
        let inner: Bool = negated != (compound?.compoundPredicateType == .not)
        let subpredicates = (compound?.subpredicates ?? []).compactMap { subpredicate in (subpredicate as? NSPredicate)?.metadataQueryPredicate(within: candidates, negated: inner) }
        let queried: Bool =
            (self as? NSComparisonPredicate).map { (comparison: NSComparisonPredicate) -> Bool in
                let right: NSExpression = comparison.rightExpression
                let constant: Any? = right.expressionType == .constantValue ? right.constantValue : nil
                let operands: Bool = [.keyPath, .anyKey].contains(comparison.leftExpression.expressionType) && (constant is String || constant is NSNumber || constant is Date)
                return operands && ![.matches, .customSelector, .between, .in].contains(comparison.predicateOperatorType)
            } ?? false
        return switch (compound?.compoundPredicateType, subpredicates.first, subpredicates.count) {
            case (.not, let subpredicate?, _): NSCompoundPredicate(notPredicateWithSubpredicate: subpredicate)
            case (.and, _, 0): candidates
            case (.or, _, 0): NSCompoundPredicate(notPredicateWithSubpredicate: candidates)
            case (_?, let subpredicate?, 1): subpredicate
            case (let type?, _, _): NSCompoundPredicate(type: type, subpredicates: subpredicates)
            case (nil, _, _) where self == NSPredicate(value: true): candidates
            case (nil, _, _) where self == NSPredicate(value: false): NSCompoundPredicate(notPredicateWithSubpredicate: candidates)
            case (nil, _, _) where queried: self
            case (nil, _, _): negated ? NSCompoundPredicate(notPredicateWithSubpredicate: candidates) : candidates
        }
    }
}

extension Installation {
    @MainActor
    package static func search(_ predicate: NSPredicate, in directories: [URL] = []) -> some AsyncSequence<Listing, Never> & Sendable {
        let (listings, continuation) = AsyncStream<Listing>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let (changes, changed) = AsyncStream<ApplicationFailure?>.makeStream()
        let edits: [Task<Void, Never>] = directories.map { directory in
            Task {
                let notices = FileEvent.events(of: directory).filter { (batch: Result<[FileEvent], FileFailure>) -> Bool in
                    batch[case: \.success]?.contains { event in event.mustScanSubDirs || event.url.pathComponents.suffix(2).elementsEqual(["Contents", "Info.plist"]) } != false
                }
                for await batch in notices { changed.yield(batch[case: \.failure].map { failure in ApplicationFailure(subject: failure.subject, reason: .foundation(failure as NSError)) }) }
            }
        }
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
            let observation = NSWorkspace.shared.observe(\.runningApplications, options: [.old, .new]) { _, change in
                if [change.oldValue, change.newValue].compactMap(\.self).joined().contains(where: { application in application.activationPolicy == .regular }) { changed.yield(nil) }
            }
            defer { observation.invalidate() }
            let updates = merge(NotificationCenter.default.messages(of: query, for: NSMetadataQuery.DidUpdateMessage.self, bufferSize: 1).map { _ in ApplicationFailure?.none }, changes)
            var (held, unwatched): ([URL: (modified: Date?, read: Result<Self, ApplicationFailure>)], [ApplicationFailure]) = ([:], [])
            for await update in chain([ApplicationFailure?.none].async, updates) {
                unwatched += [update].compactMap(\.self)
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
                let matched: [Self] = listing.installations.filter { installation in predicate.evaluate(bundleIdentifier: installation.identifier) }
                continuation.yield(Listing(installations: matched, unreadable: unwatched + listing.unreadable))
            }
        }
        continuation.onTermination = { _ in
            following.cancel()
            for edit in edits { edit.cancel() }
        }
        return listings.removeDuplicates()
    }

    package static func handlers(of document: URL, matching predicate: NSPredicate) async throws(ApplicationFailure) -> Listing {
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
        let finished: Mutex<[Int: (url: URL?, modified: Date?, read: Result<Self, ApplicationFailure>)]> = Mutex([:])
        DispatchQueue.concurrentPerform(iterations: bundles.count) { index in
            let url: URL? = bundles[index]
            let modified = url.flatMap { url in try? url.appending(components: "Contents", "Info.plist").resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate }
            let read: (url: URL?, modified: Date?, read: Result<Self, ApplicationFailure>) =
                switch (url, url.flatMap { url in held[url] }) {
                    case (.some(let url), .some(let kept)) where modified != nil && kept.modified == modified: (url, modified, kept.read)
                    case (.some(let url), _): (url, modified, Result { () throws(ApplicationFailure) in try Self(at: url) })
                    case (.none, _): (nil, nil, .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath)))
                }
            finished.withLock { finished in finished[index] = read }
        }
        let reads: [(url: URL?, modified: Date?, read: Result<Self, ApplicationFailure>)] = finished.withLock { finished in bundles.indices.compactMap { index in finished[index] } }
        return (
            Listing(installations: OrderedSet(reads.compactMap { read in read.read[case: \.success] }).elements, unreadable: reads.compactMap { read in read.read[case: \.failure] }),
            Dictionary(reads.compactMap { read in read.url.map { url in (url, (modified: read.modified, read: read.read)) } }) { first, _ in first },
        )
    }
}
