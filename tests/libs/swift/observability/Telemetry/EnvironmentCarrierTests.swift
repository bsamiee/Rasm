import CustomDump
import InMemoryTracing
import Instrumentation
import Telemetry
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let variables: [(key: String, variable: String)] = [
    (key: "traceparent", variable: "TRACEPARENT"), (key: "x-b3-traceid", variable: "X_B3_TRACEID"), (key: "2fa", variable: "_2FA"), (key: "", variable: "_"),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct EnvironmentCarrierTests {
    @Test
    func carriesASpanContextToAChildProcessEnvironmentAndBack() {
        let tracer = InMemoryTracer()
        var sent = InstrumentationContext.topLevel
        sent.inMemorySpanContext = InMemorySpanContext(traceID: "4bf92f3577b34da6", spanID: "00f067aa0ba902b7", parentSpanID: nil)
        var environment: [String: String] = [:]
        tracer.inject(sent, into: &environment, using: EnvironmentCarrier())
        var received = InstrumentationContext.topLevel
        tracer.extract(environment, into: &received, using: EnvironmentCarrier())
        expectNoDifference(environment, ["IN_MEMORY_TRACE_ID": "4bf92f3577b34da6", "IN_MEMORY_SPAN_ID": "00f067aa0ba902b7"])
        expectNoDifference(received.inMemorySpanContext, sent.inMemorySpanContext)
    }

    @Test(arguments: variables)
    func namesEachKeyAsAVariableAShellAccepts(key: String, variable: String) {
        var environment: [String: String] = [:]
        EnvironmentCarrier().inject("value", forKey: key, into: &environment)
        expectNoDifference(Array(environment.keys), [variable])
    }
}
