# [ETO]

Eto.Forms as the host of a drawn scene: handler seams, lifecycle, threading, geometry and scale, pixel interop, input, commands and menus, layout and controls, windows and theme.

## [01]-[HANDLER_SEAMS]

Widgets are platform-neutral objects over one handler each platform creates, a capability a drawn scene needs takes the same shape:
- Widgets declare `new interface IHandler : Control.IHandler`, a platform assembly registers one class per interface through `Platform.Add<T>`
- `Widget()` resolves its handler through `Platform.Instance.Create(typeof(IHandler))`, a `Widget(IHandler handler)` constructor takes a prebuilt one
- Handler classes derive from `WidgetHandler<TControl, TWidget, TCallback>`, reach the native view through `Control`, and the widget through `Widget`
- `Create<T>()` throws for an unregistered type, `CreateShared<T>()` keeps one instance per platform, `Supports<T>()` tests registration
- `Find<T>()` returns the `Func<T>` factory or `null`, a seam reads it once at construction, a read per call hides a missing platform until that call
- Missing registrations are configuration failures at launch, `CreateShared<T>()` throws there and no event refuses later
- `ThemedControlHandler<TControl, TWidget, TCallback>` builds a control from other Eto controls on a platform that lacks a native one
- `Platform.Instance.IsMac`, `IsWpf`, `IsGtk`, `IsWinForms`, and `SupportedFeatures` identify the running platform
- `PlatformFeatures` flags: `DrawableWithTransparentContent`, `MultiThreadedUI`, `Mnemonics`
- `CustomCellSupportsControlView` and `TabIndexWithCustomContainers` complete `PlatformFeatures`
- `HandlerCreated` and `WidgetCreated` fire per creation
- `Widget.Properties` holds per-widget data in a `PropertyStore` without fields
- `Platform.Cache<TKey, TValue>(key)` returns one dictionary per key the platform owns, a cache per platform instance in place of a static
- `IPlatformInitializer.Initialize(platform)` runs once per platform assembly under `[assembly: ExportInitializer(type, PlatformID = ...)]`

Native reach stays in the platform file:
- `ControlObject` is the native view (`NSView`, a WPF `FrameworkElement`), `NativeHandle` its handle
- Eto.Mac `ToNative(attach)` and `ToEto(this NSView)` convert both ways, `GetContainerView()` answers the outer view of a scrolled control
- `ToNative(attach: true)` fires the load events for a control hosted in a native view
- `Style.Add<THandler>(name, StyleHandler<THandler>)` runs for every widget with `Style` equal to `name` and a `THandler` handler
- `Style.Add<TWidget>(name, StyleWidgetHandler<TWidget>)` runs on the widget type
- Style handlers read `handler.Control` and set what Eto exposes no property for, an empty `name` matches every widget of the type
- `Widget.Style` is a space-separated list, `Control.TriggerStyleChanged()` reruns the handlers, `Style.Provider` replaces the registry
- Objective-C class pairs, blocks, and `GCHandle` pins allocated for a view release with it, an allocation with no release leaks per view

```csharp
public interface IFrameClock : Widget.IHandler {
    public IDisposable Start(Control view, Action<Duration> tick);
}

internal sealed class Scene(Control view, Action<Duration> tick) {
    private readonly IFrameClock clock = Platform.Instance.CreateShared<IFrameClock>();

    public IDisposable Animate() => clock.Start(view, tick);
}
```

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
- Effects or realization in a constructor run before the host exists
- `Parent`, `LogicalParent`, and `VisualParent` differ where a `ThemedControlHandler` inserts controls, `VisualControls` lists the visual children
- `Detach()` removes a control without disposing it
- `AttachNative()` and `DetachNative()` embed a control in a non-Eto host, `Application.Attach` joins a host message loop
- `FindParent<T>()` walks the logical tree, `Tag` holds caller data, `ID` names a control for `FindParent` and styles
- `Invalidate()`, `Invalidate(rect)`, and `Invalidate(invalidateChildren)` schedule paints on a visible control
- `Drawable.Update(region)` paints synchronously or acts as `Invalidate` per platform
- `UpdateLayout()` runs a pending layout
- `SuspendLayout()` and `ResumeLayout()` nest around a batch, `ResumeLayout` throws when unbalanced
- `Closing` runs for a user close and a programmatic `Close()`, `Closed` disposes what the window owned

```csharp
internal sealed class Lifetime : IDisposable {
    private readonly Control control;
    private readonly Func<Window, IDisposable> attach;
    private readonly EventHandler<EventArgs> attached;
    private readonly EventHandler<EventArgs> released;
    private Option<IDisposable> held;

    private Lifetime(Control control, Func<Window, IDisposable> attach) {
        (this.control, this.attach) = (control, attach);
        attached = (_, _) => Attach();
        released = (_, _) => Release();
    }

    public static Lifetime Loaded(Control control, Func<Window, IDisposable> attach) {
        Lifetime lifetime = new(control, attach);
        control.LoadComplete += lifetime.attached;
        control.UnLoad += lifetime.released;
        if (control.Loaded) lifetime.Attach();
        return lifetime;
    }

    public void Dispose() {
        control.LoadComplete -= attached;
        control.UnLoad -= released;
        Release();
    }

    private void Attach() {
        Release();
        held = Some(attach(control.ParentWindow));
    }

    private void Release() {
        _ = held.Iter(static lease => lease.Dispose());
        held = None;
    }
}
```

## [03]-[THREADING]

Eto runs one UI thread and checks it on every widget call:
- `Application.Instance.IsUIThread` tests the thread that ran `Run` or `Attach`
- `UIThreadCheckMode` defaults to `Error` with a debugger attached and `Warning` otherwise, `None` disables the check
- `EnsureUIThread()` throws or traces per mode
- `Invoke(action)` and `Invoke<T>(func)` block the caller until the UI thread runs them, a UI thread waiting on that caller deadlocks
- `AsyncInvoke(action)` posts and returns, `InvokeAsync(action)` and `InvokeAsync<T>(func)` post and return a `Task`
- Work off the UI thread computes a value and posts the widget write, a worker holding a widget reference writes nothing through it
- `RunIteration()` runs one message loop pass inside a blocking host call
- `UITimer` fires `Elapsed` on the UI thread every `Interval` seconds (`double`) between `Start()` and `Stop()`, `Started` reads the state
- `UITimer` serves cadence and debounce, a frame clock is the platform display link behind a seam
- `PlatformFeatures.MultiThreadedUI` is absent on macOS, a worker under the flag holds `Platform.ThreadStart()` while creating widgets
- `UnhandledException` and `Terminating` are process-level events, `Terminating` holds `CancelEventArgs`

```csharp
internal sealed class Debounce : IDisposable {
    private readonly UITimer timer;

    public Debounce(Duration quiet, Action fire) =>
        timer = new UITimer((_, _) => {
            Stop();
            fire();
        }) { Interval = quiet.TotalSeconds };

    public void Touch() {
        Stop();
        timer.Start();
    }

    public void Dispose() => timer.Dispose();

    private void Stop() {
        if (timer.Started) timer.Stop();
    }
}
```

## [04]-[GEOMETRY_AND_SCALE]

Every control measure is in logical points, and device scale is a fact of the window's screen:
- `Screen.Scale` is the logical scale, `DPI` is `Scale * 72`, `RealScale` the device scale
- `LogicalPixelSize` is `RealScale / Scale`, device pixels per point
- Backing bitmaps, icon rasters, and text blob caches key on that scale
- `Graphics.PixelsPerPoint` and `PointsPerPixel` give the scale of one graphics context, `Graphics.DPI` is `72 / PointsPerPixel`
- `PointToScreen`, `PointFromScreen`, `RectangleToScreen`, and `RectangleFromScreen` move between logical control space and logical screen space
- `Screen.FromPoint` and `FromRectangle` find the screen under a point or most of a rectangle, `WorkingArea` excludes the dock and taskbar
- `Control.Size` set before load is a request, `GetPreferredSize(available)` is the measured answer
- `Size.Ceiling(SizeF)` rounds a scaled logical size outward to whole device pixels, `Size * float` scales into a `SizeF`

```csharp
internal static class Scales {
    public static Size Device(Control control) => Size.Ceiling(control.Size * control.ParentWindow.LogicalPixelSize);

    public static Screen Under(Control control) => Screen.FromRectangle(control.RectangleToScreen(new RectangleF(PointF.Empty, control.Size)));

    public static Size Fitted(Control control, SizeF available) => Size.Ceiling(control.GetPreferredSize(available));
}
```

## [05]-[PIXEL_INTEROP]

`Drawable` paints through `Graphics`, and `Bitmap.Lock()` exposes pixels another renderer fills with no copy:
- `Drawable.Paint` supplies `PaintEventArgs.Graphics` and `ClipRectangle` in logical points, a renderer paints the clip and skips the rest
- `Drawable(largeCanvas: true)` hints a canvas larger than the screen, a platform can tile its painting, desktop platforms paint it whole
- `Drawable.CanFocus` accepts keyboard focus
- `SupportsCreateGraphics` gates `CreateGraphics()` outside `Paint`, `Graphics.IsRetained` reports drawn content kept between paints
- `Bitmap(size, PixelFormat.Format32bppRgba)` holds alpha, `Format32bppRgb` and `Format24bppRgb` hold none
- `Lock()` returns `BitmapData` with `Data` (pointer), `ScanWidth` (bytes per row), `BytesPerPixel`, `PremultipliedAlpha`, and `Flipped` (row order)
- `TranslateArgbToData(argb)` maps one ARGB value into the platform byte order and premultiplies under `PremultipliedAlpha`, an opaque probe survives
- `new SKPixmap(info, data.Data, data.ScanWidth)` under `SKSurface.Create(pixmap, props)` draws into the locked bits, `info` follows the probe
- `BitmapData` disposes before the bitmap draws or reads, platforms publish pixels on unlock and a draw during the lock shows stale pixels
- `Graphics.DrawImage(image, RectangleF)` of a device-sized bitmap into logical bounds is the one resample, `ImageInterpolation.None` keeps it a copy
- `Graphics.AntiAlias` and `ClipBounds` describe the context, `SaveTransformState()` scopes a transform
- `PixelOffsetMode.None` aligns fills to pixel boundaries, `Half` offsets strokes alone by half a pixel, `Aligned` offsets strokes and fills alike
- `new Icon(scale, bitmap)` wraps one device raster as a frame at that scale, `Bitmap.WithSize(width, height)` yields an `Icon` fitted to one size
- `Drawable` over native content needs `PlatformFeatures.DrawableWithTransparentContent`, otherwise `BackgroundColor` fills behind the scene
- `TextComposition` and `TextInsertionBoundsRequested` on `Drawable` are the IME hooks of a drawn editor
- `TextCompositionEventArgs.IsActive` tells a running composition from its end

```csharp
internal static class Pixels {
    public static SKImageInfo Info(BitmapData data, Size pixels) =>
        new(
            pixels.Width,
            pixels.Height,
            (data.TranslateArgbToData(Colors.Red.ToArgb()) & 0xFF) is 0xFF ? SKColorType.Rgba8888 : SKColorType.Bgra8888,
            data.PremultipliedAlpha ? SKAlphaType.Premul : SKAlphaType.Unpremul,
            SKColorSpace.CreateSrgb());
}

internal static class Icons {
    public static Icon Rendered(SKPicture art, Size logical, float scale) {
        Size pixels = Size.Ceiling(logical * scale);
        Bitmap bitmap = new(pixels, PixelFormat.Format32bppRgba);
        using (BitmapData data = bitmap.Lock()) {
            using SKPixmap pixmap = new(Pixels.Info(data, pixels), data.Data, data.ScanWidth);
            using SKSurface surface = SKSurface.Create(pixmap);
            surface.Canvas.Clear(SKColors.Transparent);
            surface.Canvas.Scale(scale);
            surface.Canvas.DrawPicture(art);
        }
        return new Icon(scale, bitmap);
    }
}
```

## [06]-[INPUT]

Input arrives as logical points, flags, and characters, with `Handled` stopping the platform default:
- `MouseEventArgs` holds `Buttons` (`MouseButtons.Primary`, `Alternate`, `Middle`), `Location` as `PointF`, and `Modifiers`
- `MouseEventArgs.Pressure`, `IsDirectionInverted`, and `Delta` as `SizeF` describe pen and wheel input
- `Delta` is wheel lines, fractional and momentum-driven on a trackpad, a scroll by whole lines rounds and keeps the remainder
- `KeyEventArgs` holds `KeyData`, `Key` (`KeyData & Keys.KeyMask`), and `Modifiers` (`KeyData & Keys.ModifierMask`)
- `KeyChar` with `IsChar` and `IsKeyDown(key, modifier)` complete `KeyEventArgs`
- `Application.Instance.CommonModifier` is `Keys.Application` (the command key) on macOS and `Control` elsewhere, `AlternateModifier` the other
- `TextInput` holds composed text after keyboard layout and IME with `Cancel`, an editor reads `TextInput` and reserves `KeyDown` for navigation
- `Handled = true` on `KeyDown` stops the menu shortcut lookup, a drawn view marks only keys it consumed
- `CaptureMouse()` and `ReleaseMouseCapture()` bracket a drag, `IsMouseCaptured` reads the state, a drag without capture loses moves outside the view
- `MouseDown`, `MouseUp`, `MouseMove`, `MouseWheel`, `MouseDoubleClick`, `MouseEnter`, and `MouseLeave` are the pointer events
- `Gestures` holds `MagnificationGesture`, `RotationGesture`, and `PanGesture` recognizers
- `Cursor`, `ToolTip`, `TabIndex`, `TabStop`, `Focus()`, and `HasFocus` cover pointer shape, hover text, and focus order
- `DoDragDrop(data, effects, image, offset)` starts a drag with a preview, `AllowDrop` enables `DragEnter`, `DragOver`, `DragDrop`, and `DragLeave`
- `DragEventArgs.Effects` defaults to `DragEffects.None`, a target accepts by setting `Copy`, `Move`, or `Link` in `DragOver`
- `DoDragDrop` returns before the drop on macOS (`NSDraggingSession`) and after it on WPF (blocking), the source reads `Effects` in `DragEnd`
- `Mouse.Position`, `Mouse.Buttons`, and `Mouse.SetCursor(cursor)` read and set pointer state outside an event
- `DragEventArgs.Data`, `AllowedEffects`, `Location`, `Modifiers`, `Buttons`, and `Source` describe the drag
- `DragEventArgs.SetDropDescription(format, inner)` sets the platform hint
- `Clipboard.Instance` and `DataObject` share `Text`, `Html`, `Image`, `Uris`, `SetString(value, type)`, `GetObject<T>(type)`, and `Types`
- Custom type names are strings both sides agree on, `Types` lists what a foreign source offers
- `TreeGridView.GetDragInfo(args)` answers the row and insertion position

```csharp
internal sealed class Drag(Control view) {
    private Option<PointF> pressed;

    public void Down(MouseEventArgs e) {
        pressed = e.Buttons.HasFlag(MouseButtons.Primary) && view.CaptureMouse() ? Some(e.Location) : None;
        e.Handled = pressed.IsSome;
    }

    public Option<SizeF> Moved(MouseEventArgs e) => pressed.Map(start => new SizeF(e.Location.X - start.X, e.Location.Y - start.Y));

    public void Up(MouseEventArgs e) {
        view.ReleaseMouseCapture();
        pressed = None;
        e.Handled = true;
    }

    public void Export(string text, Bitmap preview) {
        using DataObject data = new() { Text = text };
        view.DoDragDrop(data, DragEffects.Copy, preview, new PointF(preview.Width / 2f, preview.Height / 2f));
    }
}
```

## [07]-[COMMANDS_AND_MENUS]

One `Command` holds text, image, shortcut, and state for every menu and toolbar that shows it:
- `Command` holds `MenuText`, `ToolBarText`, `ToolTip`, `Image`, `Shortcut`, `Enabled`, `CommandParameter`, and `Executed`, `Execute()` triggers it
- Implicit conversions to `MenuItem` and `ToolItem` call `CreateMenuItem()` and `CreateToolItem()` on one instance
- `Enabled` written once updates each item and `EnabledChanged` fires
- `Shortcut` is a `Keys` chord the menu dispatches, a `KeyDown` handler matching chords by hand duplicates it
- `CheckCommand.Checked` with `CheckedChanged` and `RadioCommand.Controller` cover toggles and groups, `Command.DelegatedCommand` adapts an `ICommand`
- `MapPlatformCommand(systemCommand, command)` binds a platform edit command, `SupportedPlatformCommands` lists them
- Platform edit commands are `"cut"`, `"copy"`, `"paste"`, `"selectAll"`, `"delete"`, `"undo"`, and `"redo"`
- `MenuBar.IncludeSystemItems` (`MenuBarSystemItems.None`, `Common`, `Quit`, `All`) places platform items where each platform expects them
- `QuitItem` and `AboutItem` name the commands they wrap
- `MenuBar.ApplicationItems` and `HelpItems` are the platform menus, `Items` the top-level menus
- `ContextMenu.Show(control, location)` opens at a point, a `null` location opens at the pointer, `Opening` refreshes state before display
- `Control.ContextMenu` is the menu a platform opens on its secondary button

```csharp
internal sealed record Chrome(MenuBar Menu, ToolBar Bar);

internal static class Actions {
    public static Command Keyed(string menuText, Keys shortcut, Action run) =>
        new((_, _) => run()) { MenuText = menuText, Shortcut = Application.Instance.CommonModifier | shortcut };

    public static Chrome Built(Control editor, string fileMenu, Command save, Command undo) {
        editor.MapPlatformCommand("undo", undo);
        return new Chrome(
            new MenuBar { IncludeSystemItems = MenuBarSystemItems.All, Items = { new SubMenuItem { Text = fileMenu, Items = { save } } } },
            new ToolBar { Items = { save } });
    }
}
```

## [08]-[LAYOUT_AND_CONTROLS]

Containers lay out logical sizes, a scale flag takes remaining space, and native controls expose the state a view model binds:
- `TableLayout` holds `Rows`, `Spacing`, and `Padding`
- `TableCell(control, scaleWidth)` and `TableRow.ScaleHeight` mark growing cells, `SetColumnScale` and `SetRowScale` do so by index
- `TableLayout.Horizontal`, `HorizontalScaled`, and `AutoSized` build one-row and one-cell tables
- `DynamicLayout` builds with `BeginVertical`, `BeginHorizontal`, `AddRow`, `AddSeparateRow`, `AddSpace`, and `AddCentered`
- `BeginGroup` and `BeginScrollable` open containers, `IsCreated` reports the built tree
- `StackLayout` holds `Orientation`, `Spacing`, `AlignLabels`, `HorizontalContentAlignment`, and `VerticalContentAlignment`
- `Panel` holds `Content`, `Padding`, and `MinimumSize`
- `PixelLayout` places children at absolute logical positions through `Add(control, x, y)` and `Move(control, x, y)`
- `string` converts to a `Label` and `Image` to an `ImageView` through implicit operators on `Control`
- `Scrollable` holds `ScrollPosition`, `ScrollSize`, `VisibleRect`, `Border`, `ExpandContentWidth`, `ExpandContentHeight`, and `Zoom`
- `Scrollable` raises `Scroll`, `UpdateScrollSizes()` remeasures
- `Splitter` holds `Panel1`, `Panel2`, `Orientation`, `Position`, `RelativePosition`, `FixedPanel`, and minimum sizes
- `Splitter` raises `PositionChanging`, `PositionChanged`, and `PositionChangeCompleted`
- `TabControl` holds `Pages`, `SelectedIndex`, `SelectedPage`, and `TabPosition`
- `Expander` holds `Header` and `Expanded`
- `Label` holds `Wrap`, `TextAlignment`, `VerticalAlignment`, `Trimming`, and `UseMnemonic`, `Font` and `TextColor` show a modified state
- `TextBox` holds `CaretIndex`, `Selection`, `PlaceholderText`, `MaxLength`, and `ReadOnly`
- `TextBox.TextChanging` runs before the change with `Range`, `OldText`, `NewText`, `Source`, and `Cancel`
- `TextChangeSource` is `Keyboard`, `Paste`, `Cut`, `Composition`, `Programmatic`, or `Unknown`
- Format masks edit in `TextChanging`, model writes happen in `TextChanged`
- `NumericStepper` holds `Value`, `MinValue`, `MaxValue`, `Increment`, `DecimalPlaces`, `MaximumDecimalPlaces`, `FormatString`, `CultureInfo`, `Wrap`
- `ProgressBar` holds `Value`, `MinValue`, `MaxValue`, and `Indeterminate`
- `TreeGridView.DataStore` is an `ITreeGridStore<ITreeGridItem>` with `Expanded`, `Expandable`, and `Parent` per item
- `ReloadItem(item, reloadChildren)` refreshes one subtree
- `Grid` holds `Columns`, `AllowMultipleSelection`, `SelectedRows`, and `SelectedItems`
- `Grid` raises `SelectionChanged`, `CellFormatting`, `CellClick`, and `CellEdited`
- `Grid.CellFormatting` sets `BackgroundColor`, `ForegroundColor`, and `Font` per cell at draw time
- `Grid.BeginEdit(row, column)`, `CommitEdit()`, and `CancelEdit()` drive editing

```csharp
internal sealed class DigitBox : TextBox {
    protected override void OnTextChanging(TextChangingEventArgs e) {
        e.Cancel = e.Source is not TextChangeSource.Programmatic && !e.NewText.All(char.IsAsciiDigit);
        base.OnTextChanging(e);
    }
}
```

## [09]-[WINDOWS_AND_THEME]

Windows own a content panel, a menu, a toolbar, and a modality, and appearance changes arrive as events:
- `Dialog.ShowModal(owner)` blocks until `Close()`, `ShowModalAsync(owner)` awaits, `DefaultButton` and `AbortButton` bind Enter and Escape
- `Dialog.PositiveButtons`, `NegativeButtons`, and `DisplayMode` place buttons and select a sheet or a window per platform
- `Form.Show()` and `ShowAsync()` float, `Owner` keeps a form above its parent, `FloatingForm` floats above every window of its owner
- `Topmost`, `ShowInTaskbar`, `WindowStyle`, `WindowState`, `RestoreBounds`, and `Opacity` complete the frame
- `WindowStyle.None` with `Opacity` below 1 and a `Drawable` content makes a floating overlay, `Topmost` keeps it over its owner
- `Application.Instance.Theme` is a `Theme` with `Name` and `ThemeStyle`, compared through `Equals`
- `Themes.System`, `Light`, `Dark`, and `AllThemes` are the values
- `ThemeChanged` on the application and on each control signals the change
- `SystemColors.Highlight`, `HighlightText`, `SelectionText`, `ControlText`, `DisabledText`, `ControlBackground`, `WindowBackground` are host colors

```csharp
internal sealed class Overlay : FloatingForm {
    public Overlay(Window owner, Drawable scene, double opacity) {
        Owner = owner;
        WindowStyle = WindowStyle.None;
        Topmost = true;
        ShowInTaskbar = false;
        Opacity = opacity;
        Content = scene;
    }

    protected override void OnThemeChanged(EventArgs e) {
        base.OnThemeChanged(e);
        BackgroundColor = Application.Instance.Theme.Equals(Themes.Dark) ? SystemColors.WindowBackground : SystemColors.ControlBackground;
    }
}
```
