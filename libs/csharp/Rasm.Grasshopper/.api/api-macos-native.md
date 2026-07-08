# [RASM_GRASSHOPPER_API_MACOS_NATIVE]

Catalog scope: the macOS-native seam behind canvas motion cosmetics, gesture/pressure input, and vsync pacing inside the GH2 process. Every member is platform-gated; Windows paths pace on message-loop timers and skip the cosmetic layer.

[NAMESPACES]:
- `AppKit` views and chrome — `NSView` (gesture recognizer add/remove, point conversion, `Window`, `Bounds`, `PressureConfiguration`, `GetDisplayLink`), `NSWindow`, `NSScreen` (`MaximumFramesPerSecond`, `MaximumRefreshInterval`), `NSVisualEffectView` (+material/blending enums), `NSColorSpace`, `NSColor.FromDisplayP3`, `NSFont`, `NSHapticFeedbackManager` (+pattern/timing enums), `NSWorkspace` (accessibility flags, `SharedWorkspace.NotificationCenter`, display-options notification), `NSApplication` screen-change notifications.
- `AppKit` events — `NSEvent` (type/phase/momentum/scrolling-delta/magnification/rotation/pressure/stage/tilt/key-code/modifier members, local monitor add/remove), `NSEventMask`, `NSEventType`, `NSEventPhase`, `NSGestureRecognizer` (+state), `NSPressureBehavior`/`NSPressureConfiguration`.
- `CoreAnimation` — `CALayer`/`CAShapeLayer`/`CAGradientLayer`/`CATextLayer`/`CAReplicatorLayer`/`CAEmitterLayer` (+`CAEmitterCell`), `CATransform3D`, `CATransaction`, `CABasicAnimation`/`CASpringAnimation`/`CAKeyFrameAnimation`/`CAAnimationGroup`, `CAAnimationDelegate`, `CAMediaTimingFunction`, `CADisplayLink`, `CAFrameRateRange`.
- `CoreGraphics` — `CGPoint`/`CGSize`/`CGRect`, `CGPath`, `CGColor`, `CGAffineTransform`.
- `CoreImage` — `CIFilter` (`CIGaussianBlur`, `CIColorControls`).
- `Foundation` / `ObjCRuntime` — `NSObject`, `NSString`/`NSNumber`/`NSValue`/`NSAttributedString`, `Selector`, `Runtime.GetNSObject<T>`, `[Export]`.
