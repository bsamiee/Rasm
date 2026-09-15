import CoreGraphics
import Foundation
let list = CGWindowListCopyWindowInfo([.optionAll], kCGNullWindowID) as! [[String: Any]]
for w in list {
  let owner = w["kCGWindowOwnerName"] as? String ?? ""
  if owner.contains("InDesign") {
    let num = w["kCGWindowNumber"] as? Int ?? 0
    let name = w["kCGWindowName"] as? String ?? ""
    let b = w["kCGWindowBounds"] as? [String: Any] ?? [:]
    let layer = w["kCGWindowLayer"] as? Int ?? 0
    let on = w["kCGWindowIsOnscreen"] as? Bool ?? false
    print("\(num)\t\(layer)\t\(on)\t\(name)\t\(b)")
  }
}
