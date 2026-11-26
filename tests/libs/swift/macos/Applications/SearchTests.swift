import AppKit
import Applications
import CustomDump
import Testing
import UniformTypeIdentifiers

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct SearchTests {
    enum Form: CaseIterable, Sendable {
        case any
        case negated
        case requested
    }

    @Test(arguments: [[], ["com.apple."], ["com.apple.", "com.microsoft."]], Form.allCases)
    @MainActor
    func listsExactlyTheMatchingApplicationsForPredicatesSpotlightRefuses(prefixes: [String], form: Form) async throws {
        let any = NSCompoundPredicate(orPredicateWithSubpredicates: prefixes.map { prefix in NSPredicate(format: "%K BEGINSWITH %@", NSMetadataItemCFBundleIdentifierKey, prefix) })
        let predicate: NSPredicate =
            switch form {
                case .any: any
                case .negated: NSCompoundPredicate(notPredicateWithSubpredicate: any)
                case .requested: NSCompoundPredicate(andPredicateWithSubpredicates: [NSPredicate(value: true), any])
            }
        let directories = FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)
        let listed: [URL] = directories.flatMap { (directory: URL) -> [URL] in
            let entries: FileManager.DirectoryEnumerator? = FileManager.default.enumerator(
                at: directory,
                includingPropertiesForKeys: nil,
                options: [.skipsSubdirectoryDescendants, .skipsPackageDescendants],
            )
            return entries?.compactMap { (entry: Any) -> URL? in entry as? URL } ?? []
        }
        let bundles: [URL] = listed.filter { (url: URL) -> Bool in
            let bundle: Bool = UTType(filenameExtension: url.pathExtension)?.conforms(to: .applicationBundle) == true
            return bundle && predicate.evaluate(bundleIdentifier: Bundle(url: url)?.bundleIdentifier)
        }
        let placed: Set<[String]> = Set(bundles.map { (url: URL) -> [String] in url.resolvingSymlinksInPath().pathComponents })
        let running = {
            Set(
                NSWorkspace.shared.runningApplications.filter { application in application.activationPolicy == .regular && predicate.evaluate(bundleIdentifier: application.bundleIdentifier) }
                    .compactMap { application in application.bundleURL?.resolvingSymlinksInPath().pathComponents }
            )
        }
        let before = running()
        var listings = Installation.search(predicate, in: directories).makeAsyncIterator()
        let first: Installation.Listing? = await listings.next(isolation: #isolation)
        let listing = try #require(first)
        expectNoDifference(listing.installations.map(\.identifier).filter { identifier in !predicate.evaluate(bundleIdentifier: identifier) }, [])
        expectNoDifference(placed.union(before.intersection(running())).subtracting(listing.installations.map(\.url.pathComponents)), [])
    }
}
