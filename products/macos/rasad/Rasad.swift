import ApplicationServer
import Foundation

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Rasad {
    static func main() async {
        await ApplicationServer.main(cached: { _ in nil })
    }
}
