# [ETO]

Deep use of Eto.Forms as the host of a drawn scene: handler seams each platform fills, native reach confined to one platform file, pixel interop without copies, lifecycle, threading, input, commands, and windows.

## [01]-[HANDLER_SEAMS]

Every widget is a platform-neutral object over one handler the platform created, and a capability a drawn scene needs from the platform takes the same shape:
- Widgets declare `new interface IHandler : Control.IHandler`, a platform assembly registers one class per interface through `Platform.Add<T>`
- `Widget()` resolves its handler through `Platform.Instance.Create(typeof(IHandler))`, a `Widget(IHandler handler)` constructor takes a prebuilt one
- Handler classes derive from `WidgetHandler<TControl, TWidget, TCallback>`, reach the native view through `Control`, and the widget through `Widget`
- `Create<T>()` throws `InvalidOperationException` for an unregistered type, `CreateShared<T>()` keeps one instance per platform, `Supports<T>()` tests registration
- `Find<T>()` returns the factory or `null`, a seam reads it once at construction, a read per call hides a missing platform until that call
- `ThemedControlHandler<TControl, TWidget, TCallback>` builds a control from other Eto controls on a platform that lacks a native one
- `Platform.Instance.IsMac`, `IsWpf`, `IsGtk`, `IsWinForms`, and `SupportedFeatures` answer inside the platform assembly or its one platform file in shared code
- `PlatformFeatures.DrawableWithTransparentContent` allows a `Drawable` over native content, `MultiThreadedUI` allows widgets off the main thread, `Mnemonics` draws accelerators
- `HandlerCreated` and `WidgetCreated` fire per creation, `Widget.Properties` holds per-widget data in a `PropertyStore` without fields
- `Platform.Cache<TKey, TValue>(key)` returns one dictionary per key the platform owns, a cache per platform instance in place of a static

Native reach lives in the platform file:
- `ControlObject` is the native view (`NSView`, a WPF `FrameworkElement`), `NativeHandle` its handle, the Eto.Mac `ToNative()` and `ToEto(this NSView)` extensions convert both ways
- `ToNative(attach: true)` fires the load events for a control hosted in a native view, `ToNative()` on a scrolled control returns the container view, not the inner one
- `Style.Add<THandler>(name, StyleHandler<THandler>)` runs for every widget whose `Style` equals `name` and whose handler is `THandler`, `Style.Add<TWidget>` runs on the widget type
- Style handlers read `handler.Control` and set what Eto exposes no property for, an empty `name` matches every widget of the type
- `Widget.Style` is a space-separated list, `TriggerStyleChanged()` reruns the handlers, `Style.Provider` replaces the registry
- Native behavior enters as a view subclass declared in the platform file and installed by the handler, the toolkit's view class stays as registered
- Objective-C class pairs, blocks, and `GCHandle` pins allocated for a view release with that view, an allocation per instance with no release leaks per view

```csharp
public interface IFrameClock : Widget.IHandler {
    IDisposable Start(Control view, Action<TimeSpan> tick);
}

internal static class FrameClocks {
    public static IFrameClock Resolve() => Platform.Instance.CreateShared<IFrameClock>();
}
```

Each platform assembly registers one `IFrameClock` class, and a platform that registers none fails at the first `Resolve()` in place of a refused frame.

## [02]-[LIFECYCLE]

Controls attach to a window in a fixed order and detach in reverse, and each stage makes a fact available:

| [INDEX] | [EVENT]                   | [AVAILABLE]                                       |
| :-----: | :------------------------ | :------------------------------------------------ |
|  [01]   | `PreLoad`                 | Widget tree, no native control in its parent      |
|  [02]   | `Load`                    | Native control, size unknown                      |
|  [03]   | `LoadComplete`            | Whole tree loaded, `ParentWindow` set, size known |
|  [04]   | `Shown`                   | First display on screen                           |
|  [05]   | `UnLoad`                  | Removal from the tree, before dispose             |
|  [06]   | `Window.Closing`          | `CancelEventArgs.Cancel` keeps the window         |
|  [07]   | `Window.Closed`           | Window gone                                       |
|  [08]   | `LogicalPixelSizeChanged` | Monitor scale change on the window                |

- `Loaded` is `true` from `Load` to `UnLoad`, `IsAttached` from attachment to a native parent, `ParentWindow` is `null` outside that span
- Override `OnLoadComplete` to acquire host subscriptions and `OnUnLoad` to release them, a reload re-acquires, so one scoped subscription type serves every control
- Constructors build the tree, an effect run or a realization in a constructor runs before the host exists
- `Parent`, `LogicalParent`, and `VisualParent` differ where a `ThemedControlHandler` inserts controls, `VisualControls` lists the visual children
- `Detach()` removes a control without disposing it, `AttachNative()` and `DetachNative()` embed a control in a non-Eto host, `Application.Attach` joins a host message loop
- `FindParent<T>()` walks the logical tree, `Tag` holds caller data, `ID` names a control for `FindParent` and styles
- `Invalidate()`, `Invalidate(rect)`, and `Invalidate(invalidateChildren)` schedule paints on a visible control, `Update(region)` paints now or behaves as `Invalidate` per platform
- `UpdateLayout()` runs a pending layout, `GetPreferredSize(available)` measures, `SuspendLayout()` and `ResumeLayout()` nest around a batch and `ResumeLayout` throws when unbalanced
- `Closing` runs for a user close and a programmatic `Close()`, `Closed` disposes what the window owned

## [03]-[THREADING]

Eto runs one UI thread and checks it on every widget call:
- `Application.Instance.IsUIThread` tests the thread that ran `Run` or `Attach`
- `UIThreadCheckMode` defaults to `Error` with a debugger attached and `Warning` otherwise, `EnsureUIThread()` throws or traces per mode, `None` disables the check
- `Invoke(action)` and `Invoke<T>(func)` run inline on the UI thread and block any other caller until it runs, a UI thread waiting on that caller deadlocks
- `AsyncInvoke(action)` posts and returns, `InvokeAsync(action)` and `InvokeAsync<T>(func)` post and return a `Task`
- Work off the UI thread computes a value and posts the widget write, a worker holding a widget reference writes nothing through it
- `RunIteration()` runs one message loop pass inside a blocking host call
- `UITimer` fires `Elapsed` on the UI thread every `Interval` seconds (`double`) between `Start()` and `Stop()`, `Started` reads the state
- `UITimer` serves cadence and debounce, a frame clock is the platform display link behind a seam
- Frame clocks pause while nothing animates and resume on the first damage
- `PlatformFeatures.MultiThreadedUI` is absent on macOS, `Platform.ThreadStart()` returns the disposable a worker holds while it creates widgets where the flag is present
- `UnhandledException` and `Terminating` are process-level events, `Terminating` carries `CancelEventArgs`

## [04]-[GEOMETRY_AND_SCALE]

Every control measure is in logical points, and device scale is a fact of the window's screen:
- `Screen.Scale` is the logical scale, `DPI` is `Scale * 72`, `RealScale` the device scale, `LogicalPixelSize` is `RealScale / Scale`, device pixels per point
- `Window.LogicalPixelSize` reads the current monitor through the window, `LogicalPixelSizeChanged` fires on a monitor move
- Backing bitmaps, icon rasters, and text blob caches key on that scale
- `SizeChanged` alone misses a monitor move, a maximum over `Screen.Screens` mismatches every other monitor
- `Graphics.PixelsPerPoint` and `PointsPerPixel` give the scale of one graphics context, `Graphics.DPI` is `72 / PointsPerPixel`
- `PointToScreen`, `PointFromScreen`, `RectangleToScreen`, and `RectangleFromScreen` move between logical control space and logical screen space
- `Screen.FromPoint` and `FromRectangle` find the screen under a point or most of a rectangle, `WorkingArea` excludes the dock and taskbar
- `Control.Size` set before load is a request, `GetPreferredSize(available)` is the measured answer

## [05]-[PIXEL_INTEROP]

`Drawable` paints through `Graphics`, and `Bitmap.Lock()` exposes pixels another renderer fills with no copy:
- `Drawable.Paint` supplies `PaintEventArgs.Graphics` and `ClipRectangle` in logical points, a renderer paints the clip and skips the rest
- `Drawable(largeCanvas: true)` tiles the backing for content past the texture limit, `CanFocus` admits keyboard focus
- `SupportsCreateGraphics` gates `CreateGraphics()` outside `Paint`, `Graphics.IsRetained` reports whether the platform keeps drawn content between paints
- `Bitmap(width, height, PixelFormat.Format32bppRgba)` holds alpha, `Format32bppRgb` and `Format24bppRgb` hold none
- `Lock()` returns `BitmapData` with `Data` (pointer), `ScanWidth` (bytes per row), `BytesPerPixel`, `PremultipliedAlpha`, and `Flipped` (row order)
- `TranslateArgbToData(argb)` maps one ARGB value into the platform's byte order, premultiplying when `PremultipliedAlpha` holds
- Seams derive `SKColorType` once from `TranslateArgbToData(0x00FF0000)`, red in byte 0 is `Rgba8888` and in byte 2 is `Bgra8888`
- `SKSurface.Create(new SKImageInfo(w, h, order, alpha), data.Data, data.ScanWidth)` draws into the locked bits, `alpha` follows `PremultipliedAlpha`
- `BitmapData` disposes before the bitmap is drawn or read, a platform publishes pixels on unlock and a draw during the lock shows stale or undefined pixels
- `Graphics.DrawImage(image, RectangleF)` of a device-sized bitmap into logical bounds is the one resample, `ImageInterpolation.None` keeps it a copy
- `Graphics.AntiAlias`, `ClipBounds`, and `IsVisible(rect)` describe the context, `SaveTransformState()` scopes a transform
- `PixelOffsetMode.None` aligns fills to pixel boundaries, `Half` offsets strokes alone by half a pixel, `Aligned` offsets strokes and fills alike
- `Bitmap.WithSize(width, height)` yields an `Icon` with one frame at that size, an `Icon` holds one frame per scale for host menus and toolbars
- `Drawable` over native content needs `PlatformFeatures.DrawableWithTransparentContent`, otherwise `BackgroundColor` fills behind the scene
- `TextComposition` and `TextInsertionBoundsRequested` on `Drawable` are the IME hooks of a seated or drawn editor, `CancelTextComposition()` and `CommitTextComposition()` end one

## [06]-[INPUT]

Input arrives as logical points, flags, and characters, with `Handled` stopping the platform default:
- `MouseEventArgs` holds `Buttons` (`MouseButtons.Primary`, `Alternate`, `Middle`), `Location` as `PointF`, `Modifiers`, `Pressure`, `IsDirectionInverted`, and `Delta` as `SizeF`
- `Delta` is wheel lines, fractional and momentum-driven on a trackpad, a scroll by whole lines rounds and keeps the remainder
- `KeyEventArgs` holds `KeyData`, `Key` (`KeyData & Keys.KeyMask`), `Modifiers` (`KeyData & Keys.ModifierMask`), `KeyChar` with `IsChar`, and `IsKeyDown(key, modifier)`
- `Keys.Application` is the platform command key, `Application.Instance.CommonModifier` is it on macOS and `Control` elsewhere, `AlternateModifier` the other
- `TextInput` carries composed text after keyboard layout and IME with `Cancel`, an editor reads `TextInput` and reserves `KeyDown` for navigation
- `Handled = true` on `KeyDown` stops the menu shortcut lookup, so a drawn view marks only keys it consumed
- `CaptureMouse()` and `ReleaseMouseCapture()` bracket a drag, `IsMouseCaptured` reads the state, a drag without capture loses moves outside the view
- `MouseDown`, `MouseUp`, `MouseMove`, `MouseWheel`, `MouseDoubleClick`, `MouseEnter`, and `MouseLeave` are the pointer events, `Gestures` holds platform recognizers
- `Cursor`, `ToolTip`, `TabIndex`, `TabStop`, `Focus()`, and `HasFocus` cover pointer shape, hover text, and focus order
- `Control.DoDragDrop(data, effects, image, offset)` starts a drag with a preview, `AllowDrop` enables `DragEnter`, `DragOver`, `DragDrop`, and `DragLeave`
- `DragEventArgs.Effects` defaults to `DragEffects.None`, a target sets `Copy`, `Move`, or `Link` in `DragOver` to accept, the source reads the result in `DragEnd`
- `DoDragDrop` returns before the drop on macOS and after it on WPF, so completion logic sits in `DragEnd` on every platform
- `Mouse.Position`, `Mouse.Buttons`, and `Mouse.SetCursor(cursor)` read and set pointer state outside an event
- `DragEventArgs.Data`, `AllowedEffects`, `Location`, `Modifiers`, `Buttons`, and `Source` describe the drag, `SetDropDescription(format, inner)` sets the platform hint
- `Clipboard.Instance` and `DataObject` share `Text`, `Html`, `Image`, `Uris`, `SetString(value, type)`, `GetString(type)`, and `Types`
- Custom type names are strings both sides agree on, `Types` lists what a foreign source offers, `TreeGridView.GetDragInfo(args)` answers the row and insertion position

## [07]-[COMMANDS_AND_MENUS]

One `Command` carries text, image, shortcut, and state into every menu and toolbar that shows it:
- `Command` holds `MenuText`, `ToolBarText`, `ToolTip`, `Image`, `Shortcut`, `Enabled`, `CommandParameter`, and `Executed`, `Execute()` triggers it
- Implicit conversions to `MenuItem` and `ToolItem` bind the same instance, `Enabled` written once updates each item and `EnabledChanged` fires
- `Shortcut` is a `Keys` chord the menu dispatches, a `KeyDown` handler matching chords by hand duplicates it
- `CheckCommand.Checked` with `CheckedChanged` and `RadioCommand.Controller` cover toggles and groups, `DelegatedCommand` adapts an `ICommand`
- `MapPlatformCommand(systemCommand, command)` binds a platform edit command (`"cut"`, `"copy"`, `"paste"`, `"selectAll"`, `"delete"`, `"undo"`, `"redo"`), `SupportedPlatformCommands` lists them
- `MenuBar.IncludeSystemItems` (`MenuBarSystemItems.None`, `Common`, `Quit`, `All`) places platform items where each platform expects them, `QuitItem` and `AboutItem` name the commands they wrap
- `MenuBar.ApplicationItems` and `HelpItems` are the platform menus, `Items` the top-level menus
- `ContextMenu.Show(control, location)` opens at a point, a `null` location opens at the pointer, `Opening` refreshes state before display
- `Control.ContextMenu` is the menu the platform opens on the secondary button

## [08]-[LAYOUT_AND_CONTROLS]

Containers lay out logical sizes, a scale flag takes remaining space, and native controls expose the state a view model binds:
- `TableLayout` holds `Rows`, `Spacing`, and `Padding`, `TableCell(control, scaleWidth)` and `TableRow.ScaleHeight` mark growing cells, `SetColumnScale` and `SetRowScale` do the same by index
- `TableLayout.Horizontal`, `HorizontalScaled`, and `AutoSized` build one-row and one-cell tables
- `DynamicLayout` builds with `BeginVertical`, `BeginHorizontal`, `AddRow`, `AddSeparateRow`, `AddSpace`, `AddCentered`, `BeginGroup`, and `BeginScrollable`, `IsCreated` reports the built tree
- `StackLayout` holds `Orientation`, `Spacing`, `AlignLabels`, `HorizontalContentAlignment`, and `VerticalContentAlignment`
- `Panel` holds `Content`, `Padding`, and `MinimumSize`, `PixelLayout` places children at absolute logical positions and seats native controls inside a drawn scene
- `string` converts to a `Label` and `Image` to an `ImageView` through implicit operators on `Control`
- `Scrollable` holds `ScrollPosition`, `ScrollSize`, `VisibleRect`, `Border`, `ExpandContentWidth`, `ExpandContentHeight`, and `Zoom`, raises `Scroll`, and `UpdateScrollSizes()` remeasures
- Drawn surfaces inside a `Scrollable` lay out and paint `VisibleRect` alone, a `Height` written per paint relayouts the parent per frame
- `Drawable(largeCanvas: true)` under WPF clips painting to the enclosing `Scrollable.VisibleRect`, the flag is a hint elsewhere
- `Splitter` holds `Panel1`, `Panel2`, `Orientation`, `Position`, `RelativePosition`, `FixedPanel`, and the minimum sizes, and raises `PositionChanging`, `PositionChanged`, and `PositionChangeCompleted`
- `TabControl` holds `Pages`, `SelectedIndex`, `SelectedPage`, and `TabPosition`, `Expander` holds `Header` and `Expanded`
- `Label` holds `Wrap`, `TextAlignment`, `VerticalAlignment`, `Trimming`, and `UseMnemonic`, `Font` and `TextColor` show a modified state
- `TextBox` holds `CaretIndex`, `Selection`, `PlaceholderText`, `MaxLength`, and `ReadOnly`, `TextChanging` runs before the change with `Range`, `OldText`, `NewText`, `Source`, and `Cancel`
- `TextChangeSource` is `Keyboard`, `Paste`, `Cut`, `Composition`, or `Programmatic`, a format mask edits in `TextChanging` and a model write happens in `TextChanged`
- `NumericStepper` holds `Value`, `MinValue`, `MaxValue`, `Increment`, `DecimalPlaces`, `MaximumDecimalPlaces`, `FormatString`, `CultureInfo`, and `Wrap`
- `ProgressBar` holds `Value`, `MinValue`, `MaxValue`, and `Indeterminate`
- `TreeGridView.DataStore` is an `ITreeGridStore<ITreeGridItem>` with `Expanded`, `Expandable`, and `Parent` per item, `ReloadItem(item, reloadChildren)` refreshes one subtree
- `Grid` holds `Columns`, `AllowMultipleSelection`, `SelectedRows`, and `SelectedItems`, and raises `SelectionChanged`, `CellFormatting`, `CellClick`, and `CellEdited`
- `Grid.CellFormatting` sets `BackgroundColor`, `ForegroundColor`, and `Font` per cell at draw time, `BeginEdit(row, column)`, `CommitEdit()`, and `CancelEdit()` drive editing

## [09]-[WINDOWS_AND_THEME]

Windows own a content panel, a menu, a toolbar, and a modality, and appearance changes arrive as events:
- `Dialog.ShowModal(owner)` blocks until `Close()`, `ShowModalAsync(owner)` awaits, `DefaultButton` and `AbortButton` bind Enter and Escape
- `Dialog.PositiveButtons`, `NegativeButtons`, and `DisplayMode` place buttons and select a sheet or a window per platform
- `Form.Show()` and `ShowAsync()` float, `Owner` keeps a form above its parent, `Topmost`, `ShowInTaskbar`, `WindowStyle`, `WindowState`, `RestoreBounds`, and `Opacity` complete the frame
- `WindowStyle.None` with `Opacity` below 1 and a `Drawable` content makes a floating overlay, `Topmost` keeps it over its owner
- `Application.Instance.Theme` reads the appearance, `Themes.System`, `Light`, and `Dark` are the values, `ThemeChanged` on the application and on each control signals the change
- Theme projections answer a result the consumer reads once per change
- Icons and tinted rasters rebuild per (scale, theme), the previous instance disposes
- `SystemColors` supplies host accent and highlight where the host exposes no theme setting of its own
