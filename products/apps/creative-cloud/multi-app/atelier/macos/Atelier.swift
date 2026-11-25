import ApplicationServer
import Applications
import Foundation

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    // --- [DICTIONARIES]

    @Sendable
    private static func cachedDictionary(of installation: Installation) -> URL? {
        let version = installation.version.map { version in version.split(separator: ".").prefix(2).joined(separator: ".") }
        let caches = URL.cachesDirectory.appending(component: "Adobe \(installation.bundleName ?? installation.name)", directoryHint: .isDirectory)
        return FileManager.default.enumerator(at: caches, includingPropertiesForKeys: nil)?.lazy.compactMap { item in item as? URL }
            .first { file in file.pathExtension == "sdef" && file.deletingLastPathComponent().lastPathComponent == version }
    }

    // --- [ENTRY]

    static func main() async { await ApplicationServer.main(cached: cachedDictionary) }
}
