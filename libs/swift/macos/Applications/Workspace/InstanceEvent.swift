public import AppKit
import AsyncAlgorithms
public import CasePaths
import CasePathsCore
public import OrderedCollections
import Processes

// --- [MODELS] --------------------------------------------------------------------------

public struct InstanceEvent: Encodable, Sendable {
    public struct Window: Encodable, Sendable {
        public let number: CGWindowID
        public let title: String?
        public let document: URL?
        public let subrole: String?
        public let kind: InstanceState.Window.Description

        init(_ row: InstanceState.Window) {
            let element = row.description[case: \.element]
            (number, title, document, subrole, kind) = (row.number, element?.title ?? row.title, element?.document, element?.subrole, row.description)
        }
    }

    public struct Dialog: Encodable, Sendable {
        public let window: CGWindowID
        public let element: Element.ID?
        public let kind: InstanceState.Window.Description
        public let title: String?
        public let text: [String]
        public let answers: [Element.ID]

        init(_ row: InstanceState.Window) {
            let element = row.description[case: \.element]
            let marked: [Control] = [element?.answers?.accept, element?.answers?.discard, element?.answers?.cancel].compactMap(\.self)
            let answers: [Control] = marked + (element?.answers?.buttons ?? [])
            (window, self.element, kind, title, text) = (row.number, element?.id, row.description, element?.title ?? row.title, element?.detail?.text ?? [])
            self.answers = Array(OrderedSet(answers.map(\.id)))
        }
    }

    @CasePathable
    public enum Change: Sendable {
        case started
        case opened(Window)
        case closed(Window)
        case asking(Dialog)
        case answered(Dialog)
        case status(InstanceState.Running.Status)
        case exited(InstanceState.Ended)
    }

    private enum Key: CodingKey {
        case instance, pid, identifier, name, at, started, opened, closed, asking, answered, status, process, exited
    }

    public let instance: Instance
    public let at: Date
    public let change: Change

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(instance.process, forKey: .instance)
        try fields.encode(instance.process.pid, forKey: .pid)
        try fields.encode(at.formatted(Date.ISO8601FormatStyle(includingFractionalSeconds: true)), forKey: .at)
        switch change {
            case .started:
                try fields.encode(true, forKey: .started)
                try fields.encodeIfPresent(instance.application.bundleIdentifier, forKey: .identifier)
                try fields.encodeIfPresent(instance.application.localizedName, forKey: .name)
            case .opened(let window): try fields.encode(window, forKey: .opened)
            case .closed(let window): try fields.encode(window, forKey: .closed)
            case .asking(let dialog): try fields.encode(dialog, forKey: .asking)
            case .answered(let dialog): try fields.encode(dialog, forKey: .answered)
            case .status(let status):
                try fields.encode(status, forKey: .status)
                try fields.encodeIfPresent(status[case: \.unresponsive], forKey: .process)
            case .exited(let ended): try fields.encode(ended, forKey: .exited)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension InstanceEvent {
    public static func changes(from last: OrderedDictionary<Instance, InstanceState>, to next: OrderedDictionary<Instance, InstanceState>, at: Date) -> [Self] {
        next.flatMap { instance, state in changes(from: last[instance], to: state).map { change in Self(instance: instance, at: at, change: change) } }
    }

    private static func changes(from was: InstanceState?, to now: InstanceState) -> [Change] {
        let (before, after) = (was?[case: \.running], now[case: \.running])
        let (shown, showing): ([InstanceState.Window], [InstanceState.Window]) = ((before?.windows ?? []).filter(\.presented), (after?.windows ?? []).filter(\.presented))
        let (asked, asking): ([InstanceState.Window], [InstanceState.Window]) = (before?.asking ?? [], after?.asking ?? [])
        let (held, holding) = (Set(shown.map(\.number)), Set(showing.map(\.number)))
        let (posed, posing) = (Set(asked.map(\.number)), Set(asking.map(\.number)))
        let started: [Change] = was == nil && !now.is(\.exited) ? [.started] : []
        let status: [Change] = after.map { after in after.status == before?.status ? [] : [Change.status(after.status)] } ?? []
        let ending: InstanceState.Ended? = now[case: \.exited]
        let exited: [Change] = ending.map { ended in was?[case: \.exited] == ended ? [] : [Change.exited(ended)] } ?? []
        let closed: [Change] = shown.filter { row in !holding.contains(row.number) }.map { row in Change.closed(Window(row)) }
        let opened: [Change] = showing.filter { row in !held.contains(row.number) }.map { row in Change.opened(Window(row)) }
        let answered: [Change] = asked.filter { row in !posing.contains(row.number) }.map { row in Change.answered(Dialog(row)) }
        let raised: [Change] = asking.filter { row in !posed.contains(row.number) }.map { row in Change.asking(Dialog(row)) }
        return [started, closed, opened, answered, raised, status, exited].flatMap(\.self)
    }
}

extension NSWorkspace {
    public func instanceEvents(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<InstanceEvent, Never> & Sendable {
        chain([OrderedDictionary<Instance, InstanceState>()].async, instanceStates(of: matching, clock: clock))
            .adjacentPairs()
            .flatMap { last, next in InstanceEvent.changes(from: last, to: next, at: .now).async }
    }
}
