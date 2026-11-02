import ApplicationServices
import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let token: Generator<Data, AnySequence<Any>> = Gen<UInt8>.value().array(of: 1...32).map { bytes in Data(bytes) }.eraseToAny()
private let text: Generator<String, AnySequence<Any>> = Gen<Character>.character(in: "\u{20}"..."\u{2FF}").string(of: 0...12).eraseToAny()
private let word: Generator<String, AnySequence<Any>> = Gen<Character>.letterOrNumber.string(of: 1...12).eraseToAny()
private let finite: Generator<Double, AnySequence<Any>> = Gen<Double>.double(in: -10_000...10_000).eraseToAny()
private let scalar: Generator<JSONValue, AnySequence<Any>> = Gen<JSONValue>.oneOf(
    Gen<Bool>.bool.map(JSONValue.bool),
    finite.map(JSONValue.number),
    text.map(JSONValue.string),
)
.eraseToAny()
private let leaf: Generator<Control, AnySequence<Any>> = zip(token, word, word.optional, text.optional, word.optional, word.optional, scalar.optional, Gen<Bool>.bool, word.array(of: 0...3))
    .map { id, role, subrole, label, identifier, attribute, value, enabled, actions in
        Control(id: id, role: role, subrole: subrole, label: label, identifier: identifier, attribute: attribute, value: value, enabled: enabled, actions: actions, items: nil)
    }
    .eraseToAny()
private let control: Generator<Control, AnySequence<Any>> = zip(leaf, leaf.array(of: 0...3).optional).map { control, items in
    Control(
        id: control.id,
        role: control.role,
        subrole: control.subrole,
        label: control.label,
        identifier: control.identifier,
        attribute: control.attribute,
        value: control.value,
        enabled: control.enabled,
        actions: control.actions,
        items: items,
    )
}
.eraseToAny()
private let read: Generator<FailedRead, AnySequence<Any>> = zip(
    word.array(of: 1...3),
    word.optional,
    text.optional,
    Gen<Int32>.int32(in: -25_214...(-25_200)).compactMap { code in AXError(rawValue: code) },
).map { reads, role, label, error in
    FailedRead(reads: reads, role: role, label: label, error: error)
}
.eraseToAny()
private let frame: Generator<CGRect, AnySequence<Any>> = zip(finite, finite, Gen<Double>.double(in: 0...10_000), Gen<Double>.double(in: 0...10_000)).map { x, y, width, height in
    CGRect(x: x, y: y, width: width, height: height)
}
.eraseToAny()
private let detail: Generator<Element.Detail, AnySequence<Any>> = zip(
    frame.optional,
    Gen<Bool>.bool.optional,
    Gen<Bool>.bool.optional,
    text.array(of: 0...4),
    control.array(of: 0...4),
    read.array(of: 0...2),
)
.map { frame, main, focused, text, controls, unreadable in
    Element.Detail(frame: frame, main: main, focused: focused, text: text, controls: controls, unreadable: unreadable)
}
.eraseToAny()
private let outline: Generator<Element, AnySequence<Any>> = zip(
    token,
    Gen<UInt32>.uint32().optional,
    word,
    word.optional,
    Gen<Bool>.bool.optional,
    word.optional,
    text.optional,
    text.optional,
    word.array(of: 0...3),
)
.map { id, window, role, subrole, modal, identifier, description, title, actions in
    Element(
        id: id,
        window: window,
        role: role,
        subrole: subrole,
        modal: modal,
        identifier: identifier,
        description: description,
        title: title,
        document: nil,
        edited: nil,
        minimized: nil,
        actions: actions,
        requiresAnswer: false,
        detail: nil,
    )
}
.eraseToAny()
private let element: Generator<Element, AnySequence<Any>> = zip(
    outline,
    word.map { name in URL.temporaryDirectory.appending(component: "\(name) document.txt") }.optional,
    Gen<Bool>.bool.optional,
    Gen<Bool>.bool.optional,
    Gen<Bool>.bool,
    detail.optional,
)
.map { outline, document, edited, minimized, requiresAnswer, detail in
    Element(
        id: outline.id,
        window: outline.window,
        role: outline.role,
        subrole: outline.subrole,
        modal: outline.modal,
        identifier: outline.identifier,
        description: outline.description,
        title: outline.title,
        document: document,
        edited: edited,
        minimized: minimized,
        actions: outline.actions,
        requiresAnswer: requiresAnswer,
        detail: detail,
    )
}
.eraseToAny()

// --- [OPERATIONS] ----------------------------------------------------------------------

private func object(_ fields: [String: JSONValue?]) -> JSONValue { .object(fields.compactMapValues(\.self)) }

private func expected(_ control: Control) -> JSONValue {
    object([
        "id": .string(control.id.base64EncodedString()), "role": .string(control.role), "subrole": control.subrole.map(JSONValue.string), "label": control.label.map(JSONValue.string),
        "identifier": control.identifier.map(JSONValue.string), "attribute": control.attribute.map(JSONValue.string), "value": control.value, "enabled": .bool(control.enabled),
        "actions": .array(control.actions.map(JSONValue.string)), "items": control.items.map { items in .array(items.map(expected)) },
    ])
}

private func expected(_ read: FailedRead) -> JSONValue {
    let code: Double = Double(read.error.rawValue)
    return object([
        "reads": .array(read.reads.map(JSONValue.string)), "role": read.role.map(JSONValue.string), "label": read.label.map(JSONValue.string), "error": .number(code),
    ])
}

private func expected(_ detail: Element.Detail) -> JSONValue {
    let pair: (Double, Double) -> JSONValue = { first, second in .array([.number(first), .number(second)]) }
    return object([
        "frame": detail.frame.map { frame in .array([pair(frame.origin.x, frame.origin.y), pair(frame.size.width, frame.size.height)]) }, "main": detail.main.map(JSONValue.bool),
        "focused": detail.focused.map(JSONValue.bool), "text": .array(detail.text.map(JSONValue.string)),
        "controls": .array(detail.controls.map(expected)), "unreadable": .array(detail.unreadable.map(expected)),
    ])
}

private func expected(_ element: Element) -> JSONValue {
    object([
        "id": .string(element.id.base64EncodedString()), "window": element.window.map { window in .number(Double(window)) }, "role": .string(element.role),
        "subrole": element.subrole.map(JSONValue.string), "modal": element.modal.map(JSONValue.bool), "identifier": element.identifier.map(JSONValue.string),
        "description": element.description.map(JSONValue.string), "title": element.title.map(JSONValue.string), "document": element.document.map { url in .string(url.absoluteString) },
        "edited": element.edited.map(JSONValue.bool), "minimized": element.minimized.map(JSONValue.bool), "actions": .array(element.actions.map(JSONValue.string)),
        "requiresAnswer": .bool(element.requiresAnswer),
        "detail": element.detail.map(expected),
    ])
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Elements {
    @Test
    func elementEncodesTheWireElementShape() async {
        await propertyCheck(input: element) { element in
            expectNoDifference(try wire(element), expected(element))
        }
    }
}
