public import Subprocess

// --- [OPERATIONS] ----------------------------------------------------------------------

extension PlatformOptions {
    public static var session: Self {
        var options: Self = Self()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
        return options
    }
}
