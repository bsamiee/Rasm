// --- [MODELS] --------------------------------------------------------------------------

package struct Backoff: Sendable {
    let count: Int
    let base: Duration
    let growth: Double
    let cap: Duration?
    let jitter: Double

    package init(count: Int, base: Duration, growth: Double = 2, cap: Duration? = nil, jitter: Double = 0) {
        (self.count, self.base, self.growth, self.cap, self.jitter) = (count, base, growth, cap, jitter)
    }

    package func delays(using generator: inout some RandomNumberGenerator) -> [Duration] {
        sequence(first: base) { (delay: Duration) -> Duration in delay * growth }.prefix(count).map { (delay: Duration) -> Duration in
            let stretched = delay * (1 + jitter * Double.random(in: 0..<1, using: &generator))
            return cap.map { (cap: Duration) -> Duration in min(stretched, cap) } ?? stretched
        }
    }
}
