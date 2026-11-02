// --- [MODELS] --------------------------------------------------------------------------

public struct Backoff: Sendable {
    let count: Int
    let base: Duration
    let growth: Double
    let cap: Duration?
    let jitter: Double

    public init(count: Int, base: Duration, growth: Double = 2, cap: Duration? = nil, jitter: Double = 0) {
        self.count = max(count, 1)
        self.base = base
        self.growth = max(growth, 1)
        self.cap = cap
        self.jitter = min(max(jitter, 0), 1)
    }

    func delays(using generator: inout some RandomNumberGenerator) -> [Duration] {
        sequence(first: base) { (delay: Duration) -> Duration in delay * growth }.prefix(count).map { (delay: Duration) -> Duration in
            let stretched: Duration = delay * (1 + jitter * Double.random(in: 0..<1, using: &generator))
            return min(stretched, cap ?? stretched)
        }
    }
}
