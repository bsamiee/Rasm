public import AppKit
public import Applications
import AsyncAlgorithms
import CasePaths
import CasePathsCore
import OrderedCollections
public import ScreenCaptureKit

// --- [MODELS] --------------------------------------------------------------------------

public struct Launched: Encodable, Sendable {
    public let pid: pid_t
    public let created: Bool
}

public enum Launching: Encodable, Sendable {
    private enum Key: CodingKey {
        case launched, state, prompts, failed
    }

    case launched(Launched)
    case state(RunningInstance)
    case prompts([RunningInstance])
    case failed(ApplicationFailure)

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .launched(let launched): try fields.encode(launched, forKey: .launched)
            case .state(let state): try fields.encode(state, forKey: .state)
            case .prompts(let prompts): try fields.encode(prompts, forKey: .prompts)
            case .failed(let failure): try fields.encode(failure, forKey: .failed)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func transparent(_ image: CGImage) -> Bool {
    guard
        let context: CGContext = unsafe CGContext(
            data: nil,
            width: image.width,
            height: image.height,
            bitsPerComponent: 8,
            bytesPerRow: 0,
            space: nil as CGColorSpace?,
            bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.alphaOnly.rawValue),
        )
    else { return false }
    context.draw(image, in: CGRect(x: 0, y: 0, width: image.width, height: image.height))
    return context.makeImage()?.dataProvider?.data.map { alpha in (alpha as Data).allSatisfy { byte in byte == 0 } } ?? false
}

extension NSWorkspace {
    public func launching(
        _ installation: Installation,
        opening documents: [URL] = [],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<Launching, Never> & Sendable {
        AsyncStream { output in
            let prompting: Task<Void, Never> = Task {
                for await held: OrderedDictionary<NSRunningApplication, InstanceState> in NSWorkspace.shared.systemPrompts(naming: [installation.name], clock: clock) {
                    output.yield(.prompts(held.compactMap { instance, state in RunningInstance(instance, state: state) }))
                }
            }
            let following: Task<Void, Never> = Task {
                defer {
                    prompting.cancel()
                    output.finish()
                }
                do throws(ApplicationFailure) {
                    let (instance, created): (NSRunningApplication, Bool) = try await NSWorkspace.shared.launch(
                        installation,
                        opening: documents,
                        arguments: arguments,
                        environment: environment,
                        newInstance: newInstance,
                    )
                    prompting.cancel()
                    output.yield(.launched(Launched(pid: instance.processIdentifier, created: created)))
                    for await state: InstanceState in instance.states(clock: clock) {
                        if let running: RunningInstance = RunningInstance(instance, state: state) { output.yield(.state(running)) }
                        switch state {
                            case .running(let running):
                                if running.status.settled || !running.asking.isEmpty { return }
                            case .failed: return
                            case .exited: continue
                        }
                    }
                } catch {
                    output.yield(.failed(error))
                }
            }
            output.onTermination = { _ in
                prompting.cancel()
                following.cancel()
            }
        }
    }
}

extension NSRunningApplication {
    public func reading(window: CGWindowID? = nil) async -> InstanceState {
        guard let window else { return await state { _ in true } }
        let id: Element.ID? = await state()[case: \.running]?.windows.first { row in row.number == window }?.description[case: \.element]?.id
        return await state { element in element.id == id }
    }

    public func performing(_ action: ElementAction, clock: some Clock<Duration> & Sendable = ContinuousClock()) async throws(ApplicationFailure) -> InstanceState {
        if case .perform(_, kAXRaiseAction) = action {
            throw ApplicationFailure(subject: "\(action), raising a window takes focus from the frontmost application,", reason: .refused(nil))
        }
        return try await action.perform(in: processIdentifier, clock: clock)
    }

    public func quitting(force: Bool = false, clock: some Clock<Duration> & Sendable = ContinuousClock()) -> any AsyncSequence<Result<InstanceState, ApplicationFailure>, Never> & Sendable {
        guard force else { return quit(clock: clock) }
        let states: some AsyncSequence<InstanceState, Never> & Sendable = states(clock: clock)
        guard forceTerminate() else {
            return [Result<InstanceState, ApplicationFailure>.failure(ApplicationFailure(subject: String(processIdentifier), reason: isTerminated ? .noProcess : .refused(nil)))].async
        }
        return states.map(Result<InstanceState, ApplicationFailure>.success)
    }

    public func capturing(window: CGWindowID? = nil, longestSide: Int? = nil) async throws(ApplicationFailure) -> [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] {
        if let window { return try await screenshots(of: [window], longestSide: longestSide) }
        switch await state() {
            case .running(let running):
                return try await screenshots(of: running.windows.filter(\.presented).map(\.number), longestSide: longestSide).filter { _, shot in
                    switch shot {
                        case .success(let output): output.sdrImage.map(transparent) != true
                        case .failure: true
                    }
                }
            case .failed(let failure): throw failure
            case .exited: throw ApplicationFailure(subject: String(processIdentifier), reason: .noProcess)
        }
    }
}
