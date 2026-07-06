# [API_ETO_RUNTIME]

Catalog scope: the `Eto.Forms` runtime surface inside the GH2 process — application/thread marshalling, timers, input state, clipboard/drag-drop, notifications, and screen metrics.

[NAMESPACES]:
- `Eto.Forms` application — `Application.Instance` (`Invoke`, `NotificationActivated`), `UITimer`.
- `Eto.Forms` input state — `Keys`, `Keyboard` (`Modifiers`, `ModifiersChanged`), `Mouse` (`IsSupported`, `Position`, `Buttons`), `MouseButtons`, `Cursor`/`Cursors` (full roster), `MouseEventArgs`, `KeyEventArgs`, `TextInputEventArgs`.
- `Eto.Forms` data transfer — `Clipboard` (text/html/image/uris/typed families), `DataObject`/`IDataObject`, `DragEffects`, `DragEventArgs`.
- `Eto.Forms` notifications — `Notification`, `NotificationEventArgs`, `TrayIndicator`.
- `Eto.Forms` metrics — `Screen` (`Bounds`, `LogicalPixelSize`), window logical-pixel-size change events, `SystemColors`.
