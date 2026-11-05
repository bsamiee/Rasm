import ApplicationServices
import CustomDump
import Encoding
import Foundation
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let token: Element.ID = Element.ID(token: Data([1, 2, 3]))
private let control: Control = Control(
    id: token,
    role: kAXPopUpButtonRole,
    subrole: kAXCloseButtonSubrole,
    label: "Size",
    identifier: "size",
    attribute: kAXDefaultButtonAttribute,
    value: .string("Large"),
    enabled: false,
    actions: [kAXPressAction],
    items: [Control(id: token, role: kAXMenuItemRole, subrole: nil, label: nil, identifier: nil, attribute: nil, value: nil, enabled: true, actions: [], items: nil)],
)
private let kinds: [(element: Element, kind: Element.Kind, asking: Bool)] = [
    (element(role: kAXSheetRole, identifier: "save-panel", description: "save"), .filePanel, true),
    (element(role: kAXWindowRole, subrole: kAXStandardWindowSubrole, modal: false, identifier: "open-panel"), .filePanel, false),
    (element(role: kAXWindowRole, subrole: kAXDialogSubrole, modal: true, identifier: "save-panel"), .filePanel, true),
    (element(role: kAXSheetRole, description: "alert"), .sheet, true),
    (element(role: kAXWindowRole, subrole: kAXDialogSubrole, modal: true, description: "alert"), .alert, true),
    (element(role: kAXWindowRole, subrole: kAXDialogSubrole, modal: true), .modal, true),
    (element(role: "AXLayoutArea", subrole: kAXDialogSubrole, modal: true), .modal, true),
    (element(role: kAXWindowRole, subrole: kAXSystemDialogSubrole, modal: false, description: "alert"), .system, true),
    (element(role: kAXWindowRole, modal: false, helper: true), .system, true),
    (element(role: kAXWindowRole, subrole: kAXSystemDialogSubrole, modal: false), .window, false),
    (element(role: kAXMenuRole), .menu, false),
    (element(role: kAXMenuBarRole), .menu, false),
    (element(role: kAXWindowRole, subrole: kAXDialogSubrole, modal: false), .window, false),
    (element(role: kAXWindowRole, subrole: kAXStandardWindowSubrole, modal: false), .window, false),
    (element(role: kAXPopoverRole), .window, false),
    (element(role: kAXDrawerRole), .window, false),
]
private let labelled: [(identifier: String?, attribute: String?, label: String?)] = [
    (nil, kAXDefaultButtonAttribute, "Save"), ("DontSaveButton", nil, "Don't Save"), (nil, kAXCancelButtonAttribute, "Cancel"), (nil, kAXCloseButtonAttribute, nil),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func element(
    role: String,
    subrole: String? = nil,
    modal: Bool? = nil,
    identifier: String? = nil,
    description: String? = nil,
    controls: [Control]? = nil,
    helper: Bool = false,
) -> Element {
    Element(
        id: token,
        role: role,
        subrole: subrole,
        modal: modal,
        identifier: identifier,
        description: description,
        title: nil,
        document: nil,
        edited: nil,
        minimized: nil,
        actions: [],
        detail: controls.map { controls in Element.Detail(main: nil, focused: nil, text: [], controls: controls, unreadable: []) },
        helper: helper,
    )
}

private func button(identifier: String?, attribute: String?, label: String?) -> Control {
    Control(id: token, role: kAXButtonRole, subrole: nil, label: label, identifier: identifier, attribute: attribute, value: nil, enabled: true, actions: [kAXPressAction], items: nil)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Elements {
    @Test(arguments: kinds)
    func kindFollowsItsTableInOrder(element: Element, kind: Element.Kind, asking: Bool) {
        expectNoDifference(element.kind, kind)
        expectNoDifference(element.asking, asking)
    }

    @Test
    func menuBarEncodesItsFactsWithKindAndAsking() throws {
        expectNoDifference(try wire(element(role: kAXMenuBarRole)), try json(#"{"id":"AQID","role":"AXMenuBar","actions":[],"kind":"menu","asking":false}"#))
    }

    @Test
    func walkedElementEncodesItsDetailAndAnswers() throws {
        let walked: Element = Element(
            id: token,
            role: kAXWindowRole,
            subrole: kAXDialogSubrole,
            modal: true,
            identifier: "panel",
            description: "alert",
            title: "Save",
            document: URL(filePath: "/Document.txt"),
            edited: true,
            minimized: false,
            actions: [kAXPressAction],
            detail: Element.Detail(
                main: true,
                focused: false,
                text: ["Name"],
                controls: [],
                unreadable: [FailedRead(reads: [kAXValueAttribute], role: kAXTextFieldRole, label: "Name", error: .cannotComplete)],
            ),
            helper: false,
        )
        let line: String = #"""
            {"id":"AQID","role":"AXWindow","subrole":"AXDialog","modal":true,"identifier":"panel","description":"alert","title":"Save","document":"file:///Document.txt",
            "edited":true,"minimized":false,"actions":["AXPress"],"kind":"alert","asking":true,"detail":{"main":true,"focused":false,"text":["Name"],"controls":[],
            "unreadable":[{"reads":["AXValue"],"role":"AXTextField","label":"Name","error":-25204}]}}
            """#
        expectNoDifference(try wire(walked), try json(line))
    }

    @Test
    func controlEncodesItsWireShape() throws {
        let line: String = #"""
            {"id":"AQID","role":"AXPopUpButton","subrole":"AXCloseButton","label":"Size","identifier":"size","attribute":"AXDefaultButton","value":"Large","enabled":false,
            "actions":["AXPress"],"items":[{"id":"AQID","role":"AXMenuItem","enabled":true,"actions":[]}]}
            """#
        expectNoDifference(try wire(control), try json(line))
    }

    @Test
    func answersReadAppKitAttributesAndIdentifiers() throws {
        let controls: [Control] = labelled.map { row in button(identifier: row.identifier, attribute: row.attribute, label: row.label) }
        let answers: Element.Answers = try #require(element(role: kAXSheetRole, controls: controls).answers)
        expectNoDifference(answers.accept?.label, "Save")
        expectNoDifference(answers.discard?.label, "Don't Save")
        expectNoDifference(answers.cancel?.label, "Cancel")
        expectNoDifference(answers.buttons.compactMap(\.label), ["Save", "Don't Save", "Cancel"])
    }

    @Test
    func filePanelButtonsAnswerByIdentifier() throws {
        let answers: Element.Answers = try #require(
            element(
                role: kAXSheetRole,
                identifier: "save-panel",
                controls: [button(identifier: "OKButton", attribute: nil, label: "Save"), button(identifier: "CancelButton", attribute: nil, label: "Cancel")],
            )
            .answers
        )
        expectNoDifference(answers.accept?.identifier, "OKButton")
        expectNoDifference(answers.cancel?.identifier, "CancelButton")
    }

    @Test
    func modalWithNothingToPressHoldsNoAnswers() {
        expectNoDifference(element(role: kAXWindowRole, modal: true, controls: []).answers, nil)
        expectNoDifference(element(role: kAXWindowRole, modal: true).answers, nil)
    }
}
