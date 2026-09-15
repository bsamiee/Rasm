// ax-dump — read a macOS application's accessibility tree and emit JSON.
//
// Subcommands:
//   menus  <pid>   the menu bar tree, with shortcuts and check marks
//   tree   <pid>   every window's element tree
//   dialog <pid>   the frontmost sheet or dialog window only
//   unlock <pid>   set AXManualAccessibility, else AXEnhancedUserInterface,
//                  re-walk, report the node-count delta, restore the prior value
//
// Reads use AXUIElementCopyMultipleAttributeValues so one IPC carries every
// attribute of a node. A failed read becomes an `errors` row on the node; it is
// never retried and never guessed.

import Cocoa
import ApplicationServices

// MARK: - attributes read for every node

let batched: [String] = [
    kAXRoleAttribute, kAXSubroleAttribute, kAXTitleAttribute, kAXDescriptionAttribute,
    kAXValueAttribute, kAXHelpAttribute, kAXEnabledAttribute, kAXFocusedAttribute,
    kAXSelectedAttribute, kAXRoleDescriptionAttribute, kAXIdentifierAttribute,
    "AXFrame", "AXMenuItemCmdChar", "AXMenuItemCmdModifiers",
    "AXMenuItemCmdVirtualKey", "AXMenuItemMarkChar",
]

// MARK: - JSON value

enum JSON: Encodable {
    case s(String), n(Double), b(Bool), null
    case arr([JSON]), obj([(String, JSON)])

    func encode(to encoder: Encoder) throws {
        switch self {
        case .s(let v): var c = encoder.singleValueContainer(); try c.encode(v)
        case .n(let v): var c = encoder.singleValueContainer(); try c.encode(v)
        case .b(let v): var c = encoder.singleValueContainer(); try c.encode(v)
        case .null: var c = encoder.singleValueContainer(); try c.encodeNil()
        case .arr(let v): var c = encoder.unkeyedContainer(); for i in v { try c.encode(i) }
        case .obj(let pairs):
            var c = encoder.container(keyedBy: Key.self)
            for (k, v) in pairs { try c.encode(v, forKey: Key(k)) }
        }
    }
    struct Key: CodingKey {
        var stringValue: String, intValue: Int? = nil
        init(_ s: String) { stringValue = s }
        init?(stringValue s: String) { stringValue = s }
        init?(intValue: Int) { return nil }
    }
}

func emit(_ j: JSON) {
    let enc = JSONEncoder()
    enc.outputFormatting = [.prettyPrinted, .withoutEscapingSlashes]
    if let d = try? enc.encode(j), let s = String(data: d, encoding: .utf8) { print(s) }
}

// MARK: - AX value conversion

/// Converts one attribute value into JSON, decoding AXValue structs properly
/// rather than letting them stringify to an opaque description.
func toJSON(_ v: CFTypeRef?) -> JSON {
    guard let v = v else { return .null }
    if CFGetTypeID(v) == CFNullGetTypeID() { return .null }
    if let s = v as? String { return .s(s) }
    if let n = v as? NSNumber {
        if CFGetTypeID(v) == CFBooleanGetTypeID() { return .b(n.boolValue) }
        return .n(n.doubleValue)
    }
    if CFGetTypeID(v) == AXValueGetTypeID() {
        let av = v as! AXValue
        switch AXValueGetType(av) {
        case .axError, .illegal:
            // CopyMultipleAttributeValues returns an axError placeholder for every
            // attribute the element does not support. That is absence, not data.
            return .null
        case .cgPoint:
            var p = CGPoint.zero; AXValueGetValue(av, .cgPoint, &p)
            return .obj([("x", .n(p.x)), ("y", .n(p.y))])
        case .cgSize:
            var s = CGSize.zero; AXValueGetValue(av, .cgSize, &s)
            return .obj([("w", .n(s.width)), ("h", .n(s.height))])
        case .cgRect:
            var r = CGRect.zero; AXValueGetValue(av, .cgRect, &r)
            return .obj([("x", .n(r.minX)), ("y", .n(r.minY)),
                         ("w", .n(r.width)), ("h", .n(r.height))])
        case .cfRange:
            var r = CFRange(); AXValueGetValue(av, .cfRange, &r)
            return .obj([("location", .n(Double(r.location))), ("length", .n(Double(r.length)))])
        default:
            return .s("<AXValue \(AXValueGetType(av).rawValue)>")
        }
    }
    if let a = v as? [AnyObject] { return .n(Double(a.count)) }
    return .s(String(describing: v))
}

// MARK: - element reads

func children(_ el: AXUIElement) -> ([AXUIElement], String?) {
    var v: CFTypeRef?
    let e = AXUIElementCopyAttributeValue(el, kAXChildrenAttribute as CFString, &v)
    if e == .success { return ((v as? [AXUIElement]) ?? [], nil) }
    if e == .noValue || e == .attributeUnsupported { return ([], nil) }
    return ([], "AXChildren=\(e.rawValue)")
}

func actionNames(_ el: AXUIElement) -> [String] {
    var v: CFArray?
    guard AXUIElementCopyActionNames(el, &v) == .success else { return [] }
    return (v as? [String]) ?? []
}

var nodeCount = 0
var deepest = 0

/// Walks one element and its descendants into a JSON object tree.
/// `path` is the index chain from the root, so any node can be re-addressed later.
func walk(_ el: AXUIElement, path: String, depth: Int, maxDepth: Int, withActions: Bool) -> JSON {
    nodeCount += 1
    deepest = max(deepest, depth)

    var pairs: [(String, JSON)] = [("path", .s(path)), ("depth", .n(Double(depth)))]
    var errors: [JSON] = []

    var values: CFArray?
    let e = AXUIElementCopyMultipleAttributeValues(
        el, batched as CFArray, AXCopyMultipleAttributeOptions(rawValue: 0), &values)
    if e == .success, let got = values as? [CFTypeRef], got.count == batched.count {
        for (i, name) in batched.enumerated() {
            let j = toJSON(got[i])
            if case .null = j { continue }
            pairs.append((name, j))
        }
    } else {
        errors.append(.s("AXUIElementCopyMultipleAttributeValues=\(e.rawValue)"))
    }

    if withActions {
        let a = actionNames(el)
        if !a.isEmpty { pairs.append(("actions", .arr(a.map { .s($0) }))) }
    }

    let (kids, kidErr) = children(el)
    if let k = kidErr { errors.append(.s(k)) }
    if !errors.isEmpty { pairs.append(("errors", .arr(errors))) }

    if depth < maxDepth && !kids.isEmpty {
        var out: [JSON] = []
        out.reserveCapacity(kids.count)
        for (i, c) in kids.enumerated() {
            out.append(walk(c, path: "\(path)/\(i)", depth: depth + 1,
                            maxDepth: maxDepth, withActions: withActions))
        }
        pairs.append(("children", .arr(out)))
    } else if !kids.isEmpty {
        pairs.append(("truncatedChildren", .n(Double(kids.count))))
    }
    return .obj(pairs)
}

/// Counts nodes without building a tree — used for the unlock before/after delta.
func countNodes(_ el: AXUIElement, _ depth: Int) -> Int {
    if depth > 40 { return 1 }
    let (kids, _) = children(el)
    return kids.reduce(1) { $0 + countNodes($1, depth + 1) }
}

// MARK: - entry

let args = CommandLine.arguments
guard args.count >= 3, let rawPid = Int32(args[2]) else {
    FileHandle.standardError.write("usage: ax-dump <menus|tree|dialog|unlock> <pid>\n".data(using: .utf8)!)
    exit(2)
}
let command = args[1]
let app = AXUIElementCreateApplication(pid_t(rawPid))
// An Adobe-drawn dialog can answer AX so slowly it is unusable, and a busy
// Adobe app stops answering altogether, so the timeout is bounded. AX_TIMEOUT
// raises it for an application known to be under load.
let timeout = Float(ProcessInfo.processInfo.environment["AX_TIMEOUT"] ?? "") ?? 5.0
AXUIElementSetMessagingTimeout(app, timeout)

func header(_ extra: [(String, JSON)]) -> [(String, JSON)] {
    var h: [(String, JSON)] = [("pid", .n(Double(rawPid))), ("command", .s(command))]
    h.append(contentsOf: extra)
    return h
}

func windows() -> ([AXUIElement], String?) {
    var v: CFTypeRef?
    let e = AXUIElementCopyAttributeValue(app, kAXWindowsAttribute as CFString, &v)
    guard e == .success else { return ([], "AXWindows=\(e.rawValue)") }
    return ((v as? [AXUIElement]) ?? [], nil)
}

switch command {
case "menus":
    var v: CFTypeRef?
    let e = AXUIElementCopyAttributeValue(app, kAXMenuBarAttribute as CFString, &v)
    guard e == .success, let mb = v else {
        emit(.obj(header([("ok", .b(false)), ("error", .s("AXMenuBar=\(e.rawValue)"))])))
        exit(0)
    }
    let tree = walk(mb as! AXUIElement, path: "menubar", depth: 0, maxDepth: 20, withActions: false)
    emit(.obj(header([("ok", .b(true)), ("nodes", .n(Double(nodeCount))),
                      ("maxDepth", .n(Double(deepest))), ("menuBar", tree)])))

case "tree", "dialog":
    let (wins, err) = windows()
    if let err = err {
        emit(.obj(header([("ok", .b(false)), ("error", .s(err))])))
        exit(0)
    }
    var out: [JSON] = []
    for (i, w) in wins.enumerated() {
        if command == "dialog" {
            // Only modal-ish surfaces: a sheet, or a window carrying no close button.
            var sv: CFTypeRef?
            let isSheet = (AXUIElementCopyAttributeValue(w, kAXSubroleAttribute as CFString, &sv) == .success)
                && ((sv as? String) == "AXSheet" || (sv as? String) == "AXDialog")
            var mv: CFTypeRef?
            let isModal = (AXUIElementCopyAttributeValue(w, kAXModalAttribute as CFString, &mv) == .success)
                && ((mv as? NSNumber)?.boolValue == true)
            if !isSheet && !isModal { continue }
        }
        out.append(walk(w, path: "window[\(i)]", depth: 0, maxDepth: 40, withActions: true))
    }
    emit(.obj(header([("ok", .b(true)), ("windowCount", .n(Double(wins.count))),
                      ("nodes", .n(Double(nodeCount))), ("maxDepth", .n(Double(deepest))),
                      ("windows", .arr(out))])))

case "unlock":
    // No published test of these flags against an Adobe app exists. Try
    // AXManualAccessibility first, fall back to AXEnhancedUserInterface, pump the
    // run loop so a lazily-built tree has time to appear, then restore.
    func nodes() -> Int {
        let (wins, _) = windows()
        return wins.reduce(0) { $0 + countNodes($1, 0) }
    }
    let before = nodes()
    var report: [(String, JSON)] = [("nodesBefore", .n(Double(before)))]
    var applied = "none"

    for flag in ["AXManualAccessibility", "AXEnhancedUserInterface"] {
        var prior: CFTypeRef?
        let hadPrior = AXUIElementCopyAttributeValue(app, flag as CFString, &prior) == .success
        let setErr = AXUIElementSetAttributeValue(app, flag as CFString, kCFBooleanTrue)
        report.append(("\(flag).priorValue", hadPrior ? toJSON(prior) : .null))
        report.append(("\(flag).setError", .n(Double(setErr.rawValue))))
        if setErr == .success {
            applied = flag
            RunLoop.current.run(until: Date().addingTimeInterval(0.8))
            let after = nodes()
            report.append(("\(flag).nodesAfter", .n(Double(after))))
            // Restore whatever the app had before this process touched it.
            let restore: CFTypeRef = hadPrior ? prior! : (kCFBooleanFalse as CFTypeRef)
            let rErr = AXUIElementSetAttributeValue(app, flag as CFString, restore)
            report.append(("\(flag).restoreError", .n(Double(rErr.rawValue))))
            if after != before { break }
        }
    }
    let final = nodes()
    report.append(("appliedFlag", .s(applied)))
    report.append(("nodesAfterRestore", .n(Double(final))))
    report.append(("unlocked", .b(final > before)))
    emit(.obj(header([("ok", .b(true))] + report)))

case "windowlist":
    // Window-server geometry, independent of AX. Works even when the app is too
    // busy to answer accessibility requests, so screenshots stay possible.
    let info = CGWindowListCopyWindowInfo(
        [.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]] ?? []
    var out: [JSON] = []
    for w in info where (w[kCGWindowOwnerPID as String] as? Int) == Int(rawPid) {
        var pairs: [(String, JSON)] = []
        pairs.append(("windowId", .n(Double(w[kCGWindowNumber as String] as? Int ?? 0))))
        pairs.append(("name", .s(w[kCGWindowName as String] as? String ?? "")))
        pairs.append(("layer", .n(Double(w[kCGWindowLayer as String] as? Int ?? 0))))
        if let b = w[kCGWindowBounds as String] as? [String: CGFloat] {
            pairs.append(("bounds", .obj([("x", .n(b["X"] ?? 0)), ("y", .n(b["Y"] ?? 0)),
                                          ("w", .n(b["Width"] ?? 0)), ("h", .n(b["Height"] ?? 0))])))
        }
        out.append(.obj(pairs))
    }
    emit(.obj(header([("ok", .b(true)), ("windows", .arr(out))])))

default:
    FileHandle.standardError.write("unknown command \(command)\n".data(using: .utf8)!)
    exit(2)
}
