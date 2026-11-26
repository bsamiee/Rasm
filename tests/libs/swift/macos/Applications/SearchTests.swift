import AppKit
import Applications
import CustomDump
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct SearchTests {
    @Test(arguments: [[], ["com.apple."], ["com.apple.", "com.microsoft."]], [false, true])
    @MainActor
    func listsExactlyTheMatchingApplicationsForPredicatesSpotlightRefuses(prefixes: [String], negated: Bool) async throws {
        let any = NSCompoundPredicate(orPredicateWithSubpredicates: prefixes.map { prefix in NSPredicate(format: "%K BEGINSWITH %@", NSMetadataItemCFBundleIdentifierKey, prefix) })
        let predicate = negated ? NSCompoundPredicate(notPredicateWithSubpredicate: any) : any
        let running = {
            Set(
                NSWorkspace.shared.runningApplications.filter { application in application.activationPolicy == .regular && predicate.evaluate(bundleIdentifier: application.bundleIdentifier) }
                    .compactMap { application in application.bundleURL?.resolvingSymlinksInPath().pathComponents }
            )
        }
        let before = running()
        var listings = Installation.search(predicate, in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)).makeAsyncIterator()
        let first: Installation.Listing? = await listings.next(isolation: #isolation)
        let listing = try #require(first)
        expectNoDifference(listing.installations.map(\.identifier).filter { identifier in !predicate.evaluate(bundleIdentifier: identifier) }, [])
        expectNoDifference(before.intersection(running()).subtracting(listing.installations.map(\.url.pathComponents)), [])
    }
}
