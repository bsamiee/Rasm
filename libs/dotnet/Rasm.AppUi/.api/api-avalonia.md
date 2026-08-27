# [RASM_APPUI_API_AVALONIA]

`Avalonia` owns the retained UI object model every `SurfaceMount` mounts onto: typed property and element trees, binding, selector styling, resources, layout, paint, effects, transitions, input, routed events, and the render dispatcher. It holds the data-transfer boundary — clipboard and drag-drop — the shell input page composes, and marshals every cross-thread UI mutation through one render-thread hop. Every `SurfaceMount` case binds the whole substrate through the retained-ui layer.

## [01]-[PUBLIC_TYPES]

[BASE_OBJECTS]: retained property and element model

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :--------------------------------- | :------------ | :-------------------- |
|  [01]   | `AvaloniaObject`                   | class         | property owner        |
|  [02]   | `AvaloniaProperty`                 | class         | property identity     |
|  [03]   | `StyledProperty<TValue>`           | class         | inherited property    |
|  [04]   | `DirectProperty<TOwner,TValue>`    | class         | direct property       |
|  [05]   | `AttachedProperty<TValue>`         | class         | attached property     |
|  [06]   | `AvaloniaPropertyMetadata`         | class         | property metadata     |
|  [07]   | `AvaloniaPropertyRegistry`         | class         | property registry     |
|  [08]   | `AvaloniaPropertyChangedEventArgs` | class         | change event          |
|  [09]   | `BindingValue<T>`                  | struct        | binding-value carrier |

[ELEMENT_TREE]: styled, logical, visual, and layout participation

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]             |
| :-----: | :-------------- | :------------ | :----------------------- |
|  [01]   | `StyledElement` | class         | style participant        |
|  [02]   | `Visual`        | class         | visual tree node         |
|  [03]   | `Interactive`   | class         | routed-event node        |
|  [04]   | `InputElement`  | class         | focus + key-binding node |
|  [05]   | `Layoutable`    | class         | measure/arrange node     |
|  [06]   | `Orientation`   | enum          | layout axis vocabulary   |
|  [07]   | `ILogical`      | interface     | logical tree node        |
|  [08]   | `IResourceHost` | interface     | resource owner           |

[CONTROL_SURFACES]: product surface and shell controls

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------- |
|  [01]   | `Application`                                  | class         | application root               |
|  [02]   | `AppBuilder`                                   | class         | application builder            |
|  [03]   | `TopLevel`                                     | class         | host root                      |
|  [04]   | `Window`                                       | class         | window shell                   |
|  [05]   | `UserControl`                                  | class         | screen surface                 |
|  [06]   | `ContentControl`                               | class         | content host                   |
|  [07]   | `ItemsControl`                                 | class         | item host                      |
|  [08]   | `SelectingItemsControl`                        | class         | selection-carrying item host   |
|  [09]   | `Button`                                       | class         | command surface                |
|  [10]   | `TextBox`                                      | class         | text entry surface             |
|  [11]   | `NumericUpDown`                                | class         | bounded numeric entry          |
|  [12]   | `CalendarDatePicker`                           | class         | date entry surface             |
|  [13]   | `ComboBox` / `ComboBoxItem`                    | class         | bounded-choice surface         |
|  [14]   | `ListBox` / `ListBoxItem`                      | class         | selecting list surface         |
|  [15]   | `RadioButton` / `ToggleSwitch`                 | class         | exclusive and binary toggles   |
|  [16]   | `Slider`                                       | class         | ranged scalar surface          |
|  [17]   | `TreeView`                                     | class         | hierarchy surface              |
|  [18]   | `Menu` / `TabControl` / `TabItem` / `Expander` | class         | container and disclosure hosts |

[LAYOUT_PANEL_TYPES]: `Avalonia.Controls` — the arrangement surfaces a screen composes and the tracks they size

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `Panel`                                | class         | child-collection layout base                    |
|  [02]   | `Canvas`                               | class         | absolute placement by attached edge offsets     |
|  [03]   | `Grid`                                 | class         | track grid with row and column spacing          |
|  [04]   | `StackPanel` / `ReversibleStackPanel`  | class         | single-axis stack, reversed order variant       |
|  [05]   | `DockPanel`                            | class         | edge docking with per-axis spacing              |
|  [06]   | `WrapPanel`                            | class         | wrapping flow with item and line spacing        |
|  [07]   | `RelativePanel`                        | class         | sibling-relative and panel-relative alignment   |
|  [08]   | `UniformGrid`                          | class         | equal-cell grid, `Avalonia.Controls.Primitives` |
|  [09]   | `VirtualizingPanel`                    | class         | realization-window panel base                   |
|  [10]   | `VirtualizingStackPanel`               | class         | virtualized single-axis stack                   |
|  [11]   | `VirtualizingCarouselPanel`            | class         | virtualized single-item carousel panel          |
|  [12]   | `Decorator`                            | class         | single-`Child` wrapper base                     |
|  [13]   | `Viewbox`                              | class         | scale-to-fit child under `Stretch`              |
|  [14]   | `LayoutTransformControl`               | class         | transform applied before measure                |
|  [15]   | `SplitView`                            | class         | pane-plus-content shell split                   |
|  [16]   | `Carousel`                             | class         | paged selector with swipe and page transition   |
|  [17]   | `ColumnDefinitions` / `RowDefinitions` | class         | grid track collections                          |
|  [18]   | `GridSplitter` / `GridResizeDirection` | class, enum   | split track resize surface                      |
|  [19]   | `Dock` / `WrapPanelItemsAlignment`     | enum          | dock edge and wrap alignment vocabularies       |

[STATE_AND_STYLE]: binding, resources, styles, and templates

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]       |
| :-----: | :------------------------- | :------------ | :----------------- |
|  [01]   | `BindingBase`              | class         | binding root       |
|  [02]   | `Binding`                  | class         | reflection binding |
|  [03]   | `CompiledBindingExtension` | class         | compiled binding   |
|  [04]   | `MultiBinding`             | class         | composite binding  |
|  [05]   | `TemplateBinding`          | class         | template binding   |
|  [06]   | `BindingNotification`      | class         | binding result     |
|  [07]   | `ResourceDictionary`       | class         | resource scope     |
|  [08]   | `Styles`                   | class         | style collection   |
|  [09]   | `Style`                    | class         | selector style     |
|  [10]   | `Setter`                   | class         | styled assignment  |
|  [11]   | `ControlTheme`             | class         | per-type style set |
|  [12]   | `DataTemplate`             | class         | data presentation  |
|  [13]   | `IBrush`                   | interface     | paint contract     |
|  [14]   | `SolidColorBrush`          | class         | mutable color fill |

[SELECTOR_TYPES]: `Avalonia.Styling` + `Avalonia.Controls` — the match algebra a `Style` or `ControlTheme` binds and the class state it reads

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `Selector`                | class         | abstract match node                            |
|  [02]   | `Selectors`               | class         | combinator extension owner over `Selector?`    |
|  [03]   | `StyleBase`               | class         | setter, child, animation, and resource carrier |
|  [04]   | `SetterBase`              | class         | styled-assignment base                         |
|  [05]   | `Classes`                 | class         | style-class list also facing `IPseudoClasses`  |
|  [06]   | `IPseudoClasses`          | interface     | pseudo-class write face                        |
|  [07]   | `PseudoClassesExtensions` | class         | boolean pseudo-class toggle                    |

[MARKUP_EXTENSION_TYPES]: `Avalonia.Markup.Xaml.MarkupExtensions` + `Avalonia.Markup.Xaml.Templates` — XAML-authored value and template producers

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `StaticResourceExtension`             | class         | parse-time one-shot resource resolve            |
|  [02]   | `DynamicResourceExtension`            | class         | `BindingBase` re-resolving on dictionary change |
|  [03]   | `OnPlatformExtension` / `<TReturn>`   | class         | per-OS value selection                          |
|  [04]   | `OnFormFactorExtension` / `<TReturn>` | class         | per-form-factor value selection                 |
|  [05]   | `ResolveByNameExtension`              | class         | namescope lookup by element name                |
|  [06]   | `ControlTemplate`                     | class         | XAML `IControlTemplate` with `TargetType`       |
|  [07]   | `FuncControlTemplate`                 | class         | delegate-built control template                 |
|  [08]   | `ItemsPanelTemplate`                  | class         | XAML `ITemplate<Panel?>` for `ItemsControl`     |

[PAINT_TYPES]: `Avalonia.Media` — the fill, stroke, and shadow vocabulary a `Border` or `Shape` binds

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :--------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `GradientBrush`        | class         | gradient stop and spread base                          |
|  [02]   | `LinearGradientBrush`  | class         | two-point linear ramp                                  |
|  [03]   | `RadialGradientBrush`  | class         | elliptical ramp with independent radii                 |
|  [04]   | `ConicGradientBrush`   | class         | angular sweep ramp                                     |
|  [05]   | `GradientStops`        | class         | ordered stop collection                                |
|  [06]   | `GradientSpreadMethod` | enum          | `Pad` / `Reflect` / `Repeat` outside the ramp          |
|  [07]   | `BoxShadow`            | struct        | one offset, blur, spread, color, inset shadow          |
|  [08]   | `BoxShadows`           | struct        | ordered multi-shadow value with inset probe            |
|  [09]   | `BackgroundSizing`     | enum          | `InnerBorderEdge` / `OuterBorderEdge` / `CenterBorder` |

[VECTOR_IMAGE_TYPES]: `Avalonia.Media` + `Avalonia.Media.Imaging` — the retained drawing and bitmap surfaces an `Image.Source` or `IImage` consumer binds

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `IImage`             | interface      | `Size` plus `Draw(context, source, dest)` — the one   |
|  [02]   |                      |                | contract a raster and a vector product both satisfy   |
|  [03]   | `Geometry`           | abstract class | path model with `Bounds`, `Transform`, hit tests      |
|  [04]   | `StreamGeometry`     | class          | path-data geometry with a streaming build context     |
|  [05]   | `Drawing`            | abstract class | retained draw node with `GetBounds()`                 |
|  [06]   | `GeometryDrawing`    | sealed class   | geometry with its own brush and pen                   |
|  [07]   | `ImageDrawing`       | sealed class   | an `IImage` placed in a `Rect`                        |
|  [08]   | `DrawingGroup`       | sealed class   | children with transform, clip, opacity, effect        |
|  [09]   | `DrawingImage`       | class          | `IImage` over a `Drawing` under an optional `Viewbox` |
|  [10]   | `MatrixTransform`    | sealed class   | a `Matrix` as a bindable `Transform`                  |
|  [11]   | `Bitmap`             | class          | decoded raster, `IImage` and `IDisposable`            |
|  [12]   | `RenderTargetBitmap` | class          | `Bitmap` a drawing context renders into               |

- `DrawingImage.Size` reads `Viewbox ?? Drawing?.GetBounds() ?? default`, so setting `Viewbox` pins the product extent regardless of where the drawing's own geometry lands; leaving it unset makes the extent the drawing's bounds and a non-square glyph then reports a non-square size.
- `DrawingGroup.Children` is a `DirectProperty` returning a live `DrawingCollection`, so a group composes through a collection initializer and its `Transform` applies to every child at once.
- `RenderTargetBitmap(PixelSize)` plus `CreateDrawingContext()` is the one lane turning any `IImage` into a `Bitmap` — the image draws itself into the target and the target IS a `Bitmap`, so a per-source rasterizer is never needed.

[EFFECT_TYPES]: `Avalonia.Media` — the per-visual pixel filter and its immutable render twin

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `IEffect`                            | interface     | effect contract a `Visual` binds           |
|  [02]   | `IImmutableEffect`                   | interface     | render-thread-safe effect contract         |
|  [03]   | `Effect`                             | class         | animatable effect base owning `Parse`      |
|  [04]   | `BlurEffect`                         | class         | gaussian blur by `Radius`                  |
|  [05]   | `DropShadowEffectBase`               | class         | blur radius, color, and opacity base       |
|  [06]   | `DropShadowEffect`                   | class         | cartesian-offset drop shadow               |
|  [07]   | `DropShadowDirectionEffect`          | class         | polar-offset drop shadow                   |
|  [08]   | `ImmutableBlurEffect`                | class         | frozen blur                                |
|  [09]   | `ImmutableDropShadowEffect`          | class         | frozen cartesian drop shadow               |
|  [10]   | `ImmutableDropShadowDirectionEffect` | class         | frozen polar drop shadow                   |
|  [11]   | `EffectExtensions`                   | class         | `IEffect` to `IImmutableEffect` projection |

[RENDER_TUNING_TYPES]: `Avalonia.Media` + `Avalonia.Media.Transformation` — per-subtree rasterization policy, caching, and interpolable transforms

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `RenderOptions`               | struct        | record-struct raster policy with merge                      |
|  [02]   | `TextOptions`                 | struct        | record-struct text raster policy with merge                 |
|  [03]   | `EdgeMode`                    | enum          | `Unspecified` / `Antialias` / `Aliased`                     |
|  [04]   | `TextRenderingMode`           | enum          | `Unspecified` / `SubpixelAntialias` / `Antialias` / `Alias` |
|  [05]   | `BitmapInterpolationMode`     | enum          | image resample quality, `Avalonia.Media.Imaging`            |
|  [06]   | `BitmapBlendingMode`          | enum          | image composite operator, `Avalonia.Media.Imaging`          |
|  [07]   | `CacheMode`                   | class         | abstract cache policy owning `Parse`                        |
|  [08]   | `BitmapCache`                 | class         | rasterized subtree cache with scale knobs                   |
|  [09]   | `TransformOperations`         | class         | ordered transform op list, operation-wise interpolable      |
|  [10]   | `TransformOperations.Builder` | struct        | append-only op accumulator                                  |

[ANIMATION_TYPES]: `Avalonia.Animation` + `Avalonia.Animation.Easings` + the transitioning content host — implicit transitions, keyframe animations, page swaps, and the easing vocabulary

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Animatable`                                     | class         | `Transitions`-carrying property owner under `StyledElement` |
|  [02]   | `ITransition`                                    | interface     | transition contract                                         |
|  [03]   | `TransitionBase`                                 | class         | duration, delay, easing, and target-property base           |
|  [04]   | `Transition<T>`                                  | class         | typed interpolating transition base                         |
|  [05]   | `<ValueType>Transition`                          | class         | one implicit transition per animatable value type           |
|  [06]   | `Transitions`                                    | class         | validated `ITransition` list an `Animatable` binds          |
|  [07]   | `IPageTransition`                                | interface     | page-swap transition contract                               |
|  [08]   | `CrossFade` / `PageSlide` / `Rotate3DTransition` | class         | page-swap transitions                                       |
|  [09]   | `CompositePageTransition`                        | class         | page transitions run together                               |
|  [10]   | `Animation`                                      | class         | keyframe animation with playback policy                     |
|  [11]   | `KeyFrame` / `KeyFrames`                         | class         | one keyframe's setters and their ordered collection         |
|  [12]   | `Cue`                                            | struct        | normalized keyframe position                                |
|  [13]   | `KeySpline`                                      | class         | cubic-bezier progress reshape                               |
|  [14]   | `IterationCount`                                 | struct        | finite or infinite repeat count                             |
|  [15]   | `FillMode` / `PlaybackDirection`                 | enum          | value retention and direction                               |
|  [16]   | `PlaybackBehavior`                               | enum          | `Auto` / `Always` / `OnlyIfVisible` run gating              |
|  [17]   | `Easing`                                         | class         | easing base owning `Parse`                                  |
|  [18]   | `<Curve>Ease{In,Out,InOut}`                      | class         | one easing per curve family and direction                   |
|  [19]   | `LinearEasing` / `SplineEasing` / `SpringEasing` | class         | identity, bezier, and physical easings                      |
|  [20]   | `IProgressPageTransition`                        | interface     | page swap drivable from a normalized progress value         |
|  [21]   | `PageSlide.SlideAxis`                            | enum          | `Horizontal` / `Vertical` slide orientation                 |
|  [22]   | `TransitioningContentControl`                    | class         | `Avalonia.Controls` content host running a page transition  |

[WINDOW_CHROME_TYPES]: `Avalonia.Controls` + `Avalonia.Controls.Chrome` + `Avalonia.Media` — window translucency and the decoration surface a theme redraws

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `WindowTransparencyLevel`         | struct        | record-struct translucency request key              |
|  [02]   | `ExperimentalAcrylicBorder`       | class         | acrylic-painted `Decorator`                         |
|  [03]   | `ExperimentalAcrylicMaterial`     | class         | tint, opacity, and fallback knobs                   |
|  [04]   | `IExperimentalAcrylicMaterial`    | interface     | resolved material the backend paints                |
|  [05]   | `AcrylicBackgroundSource`         | enum          | `None` / `Digger`                                   |
|  [06]   | `WindowDecorations`               | enum          | `None` / `BorderOnly` / `Full`                      |
|  [07]   | `WindowDrawnDecorations`          | class         | Avalonia-drawn frame, title bar, and shadow element |
|  [08]   | `IWindowDrawnDecorationsTemplate` | interface     | drawn-decoration template contract                  |
|  [09]   | `WindowDrawnDecorationsTemplate`  | class         | XAML drawn-decoration template                      |
|  [10]   | `WindowDecorationProperties`      | class         | attached decoration-role owner                      |
|  [11]   | `WindowDecorationsElementRole`    | enum          | hit-role vocabulary, `Avalonia.Input`               |

[THEME_VARIANT_TYPES]: the variant key that scopes resource resolution

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :--------------------- | :------------ | :--------------------------- |
|  [01]   | `ThemeVariant`         | record        | variant key                  |
|  [02]   | `PlatformThemeVariant` | enum          | OS probe value               |
|  [03]   | `ThemeVariantScope`    | class         | `Decorator` subtree override |

[PLATFORM_PREFERENCE_TYPES]: `Avalonia.Platform` — the OS appearance and contrast surface, and the whole of what the framework probes

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `IPlatformSettings`           | interface     | `[NotClientImplementable]` OS settings contract |
|  [02]   | `PlatformColorValues`         | record        | appearance, contrast, and three accent colors   |
|  [03]   | `ColorContrastPreference`     | enum          | `NoPreference` / `High`                         |
|  [04]   | `PlatformHotkeyConfiguration` | class         | per-OS gesture roster the settings carry        |

- `PlatformColorValues` carries `ThemeVariant` (`PlatformThemeVariant`), `ContrastPreference` (`ColorContrastPreference`), and `AccentColor1`/`AccentColor2`/`AccentColor3`, where the second and third fall back to the first when a platform reports one accent.
- Reduced motion, reduced transparency, and text scale have NO surface here: the framework probes appearance and contrast alone, so a product needing the remaining accessibility preferences reads them through its own host port and never through this interface.

[INPUT_AND_FOCUS_TYPES]: key gestures, bindings, focus, and modifiers

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :----------------------------------------- | :------------ | :-------------------- |
|  [01]   | `KeyGesture`                               | class         | value-equal chord     |
|  [02]   | `KeyBinding`                               | class         | gesture-binding row   |
|  [03]   | `KeyModifiers`                             | enum          | logical modifiers     |
|  [04]   | `RawInputModifiers`                        | enum          | raw modifier flags    |
|  [05]   | `FocusManager`                             | class         | focus ownership       |
|  [06]   | `NavigationMethod`                         | enum          | focus-move cause      |
|  [07]   | `KeyEventArgs` / `PointerPressedEventArgs` | class         | input event payloads  |
|  [08]   | `Dispatcher`                               | class         | render-thread marshal |
|  [09]   | `Cursor`                                   | class         | disposable pointer    |
|  [10]   | `StandardCursorType`                       | enum          | platform pointer set  |

- `StandardCursorType` members: `Arrow` `Ibeam` `Wait` `Cross` `UpArrow` `SizeWestEast` `SizeNorthSouth` `SizeAll` `No` `Hand` `AppStarting` `Help` `TopSide` `BottomSide` `LeftSide` `RightSide` `TopLeftCorner` `TopRightCorner` `BottomLeftCorner` `BottomRightCorner` `DragMove` `DragCopy` `DragLink` `None` — the roster carries no open-hand or closed-hand pointer and no diagonal size member, so a grab affordance is a bitmap cursor and a diagonal resize takes the matching corner member.

[POINTER_TYPES]: `Avalonia.Input` — the pointer identity, its per-event device state, and the digitizer properties a pen surface reads

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------------- | :------------ | :------------------------------------ |
|  [01]   | `IPointer`               | interface     | pointer identity and capture custody  |
|  [02]   | `PointerType`            | enum          | `Mouse` / `Touch` / `Pen`             |
|  [03]   | `PointerPoint`           | record struct | one positioned reading                |
|  [04]   | `PointerPointProperties` | record struct | button, pen, and contact device state |
|  [05]   | `PointerUpdateKind`      | enum          | the state change that raised an event |
|  [06]   | `PointerEventArgs`       | class         | pointer event payload base            |
|  [07]   | `PointerDeltaEventArgs`  | class         | touchpad rotate/magnify/swipe delta   |

- `IPointer` members: `int Id`, `IInputElement? Captured`, `PointerType Type`, `bool IsPrimary`, `void Capture(IInputElement?)` — `[NotClientImplementable]`, so a synthesized pointer is unspellable outside the framework.
- `PointerPoint` members: `IPointer Pointer`, `PointerPointProperties Properties`, `Point Position`.
- `PointerPointProperties` members: `Pressure` `XTilt` `YTilt` `Twist` (`float`), `IsBarrelButtonPressed` `IsEraser` `IsInverted` (`bool`), the five `Is*ButtonPressed` mouse flags, `Rect ContactRect`, `PointerUpdateKind PointerUpdateKind`, and the `None` default. `Pressure` spans 0 to 1 and DEFAULTS TO 0.5, `Twist` spans 0 to 359 degrees clockwise, and `XTilt`/`YTilt` are degrees left/up negative and right/down positive — so a mouse or touch pointer reports a constant mid pressure and zero tilt rather than absence, and a pen read must gate on `IPointer.Type` first.
- `IsEraser` and `IsInverted` are DISTINCT flags off `RawInputModifiers.PenEraser` and `RawInputModifiers.PenInverted`: an eraser-tipped stylus and a barrel-inverted one set different bits for the same user intent.
- `PointerUpdateKind` members: the five `<Button>Pressed` and five `<Button>Released` pairs with `Other`; the parameterless `PointerPointProperties` constructor seats `LeftButtonPressed`, so the default value carries a kind rather than a neutral one.

[AUTOMATION_TYPES]: `Avalonia.Automation` — the accessibility surface every shell announcement and audit reads

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `AutomationProperties`                      | static        | attached automation-property owner                             |
|  [02]   | `AutomationLiveSetting`                     | enum          | `Off` / `Polite` / `Assertive`                                 |
|  [03]   | `AutomationControlType`                     | enum          | control-type override vocabulary                               |
|  [04]   | `AutomationLandmarkType`                    | enum          | landmark override vocabulary                                   |
|  [05]   | `AccessibilityView` / `IsOffscreenBehavior` | enum          | tree-visibility and offscreen policy                           |
|  [06]   | `AutomationPeer` / `ControlAutomationPeer`  | class         | peer bases a synthesized region derives                        |
|  [07]   | `KeyboardNavigation`                        | static        | attached tab-navigation owner                                  |
|  [08]   | `KeyboardNavigationMode`                    | enum          | `Continue` / `Cycle` / `Contained` / `Once` / `None` / `Local` |

[EMBED_TYPES]: `Avalonia.Controls.Embedding` + `Avalonia.Platform` — the foreign-view boundary an in-host mount crosses

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `EmbeddableControlRoot`        | class         | `TopLevel` root hosted by a foreign view   |
|  [02]   | `IPlatformHandle`              | interface     | `nint Handle` + `string? HandleDescriptor` |
|  [03]   | `PlatformHandle`               | class         | concrete handle carrier                    |
|  [04]   | `IMacOSTopLevelPlatformHandle` | interface     | macOS `NSView`/`NSWindow` handle access    |

[SHELL_CHROME_TYPES]: `Avalonia.Controls` — OS-owned menu and tray chrome

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :------------------------------ | :------------ | :-------------------------------- |
|  [01]   | `NativeMenu` / `NativeMenuItem` | class         | OS menu model and item            |
|  [02]   | `NativeMenuItemSeparator`       | class         | menu separator item               |
|  [03]   | `NativeMenuBar`                 | class         | in-window managed menu control    |
|  [04]   | `TrayIcon` / `TrayIcons`        | class         | tray indicator and its collection |
|  [05]   | `MenuItemToggleType`            | enum          | menu-item toggle vocabulary       |

[METADATA_ATTRIBUTES]: XAML and template metadata

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]      |
| :-----: | :------------------------------ | :------------ | :---------------- |
|  [01]   | `PseudoClassesAttribute`        | attribute     | style metadata    |
|  [02]   | `TemplatePartAttribute`         | attribute     | template metadata |
|  [03]   | `ContentAttribute`              | attribute     | XAML content      |
|  [04]   | `TemplateContentAttribute`      | attribute     | template content  |
|  [05]   | `ControlTemplateScopeAttribute` | attribute     | template scope    |

[NOTIFICATION_TYPES]: transient notification surfaces

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :---------------------------- | :------------ | :------------------- |
|  [01]   | `WindowNotificationManager`   | class         | toast manager        |
|  [02]   | `INotificationManager`        | interface     | manager contract     |
|  [03]   | `IManagedNotificationManager` | interface     | content manager      |
|  [04]   | `NotificationType`            | enum          | severity vocabulary  |
|  [05]   | `NotificationPosition`        | enum          | placement vocabulary |

[STORAGE_TYPES]: per-surface file and folder picker surfaces

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------------ | :------------ | :-------------------- |
|  [01]   | `IStorageProvider`        | interface     | picker contract       |
|  [02]   | `IStorageFile`            | interface     | selected file token   |
|  [03]   | `IStorageFolder`          | interface     | selected folder token |
|  [04]   | `FilePickerFileType`      | class         | one named filter      |
|  [05]   | `FilePickerOpenOptions`   | class         | open-picker options   |
|  [06]   | `FilePickerSaveOptions`   | class         | save-picker options   |
|  [07]   | `FolderPickerOpenOptions` | class         | folder-picker options |

[DATA_TRANSFER_TYPES]: clipboard and drag data-transfer surfaces

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :-------------------- | :------------ | :--------------------------- |
|  [01]   | `IClipboard`          | interface     | clipboard contract           |
|  [02]   | `ClipboardExtensions` | class         | typed clip ops               |
|  [03]   | `IDataTransfer`       | interface     | sync transfer contract       |
|  [04]   | `IAsyncDataTransfer`  | interface     | async transfer contract      |
|  [05]   | `DataTransfer`        | class         | transfer payload             |
|  [06]   | `DataTransferItem`    | class         | per-format item              |
|  [07]   | `IDataTransferItem`   | interface     | item contract                |
|  [08]   | `DataFormat`          | class         | format identity              |
|  [09]   | `DataFormat<T>`       | class         | typed format                 |
|  [10]   | `DataFormatKind`      | enum          | format-kind vocabulary       |
|  [11]   | `DragDrop`            | class         | drop-target and drag surface |
|  [12]   | `DragDropEffects`     | enum          | drag-effect flags            |
|  [13]   | `DragEventArgs`       | class         | drop payload                 |

## [02]-[ENTRYPOINTS]

[PROPERTY_OPERATIONS]: retained property registration and observation

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `AvaloniaProperty.Register<TOwner,TValue>`                        | static   | styled property             |
|  [02]   | `AvaloniaProperty.RegisterDirect<TOwner,TValue>`                  | static   | direct property             |
|  [03]   | `AvaloniaProperty.RegisterAttached<THost,TValue>`                 | static   | attached property           |
|  [04]   | `AvaloniaObject.Bind(property, IObservable)`                      | instance | observable to state binding |
|  [05]   | `AvaloniaObjectExtensions.GetObservable(property)`                | static   | typed state stream          |
|  [06]   | `AvaloniaObjectExtensions.GetBindingObservable(property)`         | static   | `BindingValue<T>` stream    |
|  [07]   | `AvaloniaObjectExtensions.GetPropertyChangedObservable(property)` | static   | change-args stream          |

[ASSET_LOOKUP_OPERATIONS]: resource and name lookup

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `FindResource(IResourceHost, object)`                                | static   | throwing-free lookup, null when absent |
|  [02]   | `FindResource(IResourceHost, ThemeVariant?, object)`                 | static   | same read under an explicit variant    |
|  [03]   | `TryFindResource(IResourceHost, object, out object?)`                | static   | guarded lookup, ambient variant        |
|  [04]   | `TryFindResource(IResourceHost, object, ThemeVariant?, out object?)` | static   | guarded lookup under a variant         |
|  [05]   | `INameScope.Register`                                                | instance | name ownership                         |
|  [06]   | `INameScope.Find`                                                    | instance | named lookup                           |
|  [07]   | `Styles.Add`                                                         | instance | style admission                        |
|  [08]   | `ResourceDictionary.Add`                                             | instance | resource admission                     |
|  [09]   | `ResourceDictionary.TryGetValue`                                     | instance | keyed value read                       |
|  [10]   | `ResourceDictionary.AddDeferred`                                     | instance | lazy admission                         |
|  [11]   | `ResourceDictionary.AddNotSharedDeferred`                            | instance | per-read admission                     |

- `ThemeVariant?` null reads the host's own `ActualThemeVariant`, so a conformance sweep proving a key resolves under Light AND Dark passes each variant explicitly rather than flipping the application's requested variant between reads.
- Every `ResourceNodeExtensions` static extends `IResourceHost`, so `Application.Current` and any `StyledElement` serve equally as the lookup root, and each no-variant arm delegates to its variant arm with null.

- `TryGetValue` builds a deferred entry on first read and WRITES the built value back over the factory, so a shared deferred resource materializes once and every later reader takes the same instance; an `AddNotSharedDeferred` entry rebuilds per read and is never written back.
- That write-back and the re-entrancy key it parks make the read path unsafe off the UI thread — a concurrent first read of the same dictionary corrupts its backing store, so a code-side read of a compiled resource dictionary marshals like any other retained-state read.

[DYNAMIC_RESOURCE_OPERATIONS]: `ResourceNodeExtensions` and `NameScopeExtensions` statics — the code-side counterpart of `{DynamicResource}` and of a template-part read

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `GetResourceObservable(IResourceHost, object, Func?)`                    | static   | re-resolving value stream off a host     |
|  [02]   | `GetResourceObservable(IResourceProvider, object, Func?)`                | static   | same stream off a provider               |
|  [03]   | `GetResourceObservable(IResourceProvider, object, ThemeVariant?, Func?)` | static   | stream under a default variant           |
|  [04]   | `TryGetResource(IResourceHost, object, out object?)`                     | static   | single non-tracking read                 |
|  [05]   | `IResourceNode.TryGetResource(object, ThemeVariant?, out object?)`       | instance | node-level read both statics delegate to |
|  [06]   | `Find<T>(INameScope, string) -> T?`                                      | static   | nullable typed template-part lookup      |
|  [07]   | `Get<T>(INameScope, string) -> T`                                        | static   | throwing typed template-part lookup      |

- `GetResourceObservable` is the ONE code-side dynamic read; a `SetValue` of a resolved resource seats a LocalValue that no dictionary edit re-resolves, so a code-driven consumer binds this observable and never writes the value.
- `Find<T>` returns null for a missing or mistyped part while `Get<T>` throws, so a control resolving an optional part takes `Find` and a required-part refusal is the caller's own typed fault rather than an escaped exception.

[SELECTOR_OPERATIONS]: `Avalonia.Styling.Selectors` — the combinator chain a `Style` or `ControlTheme` selector builds

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `Selectors.OfType(Type)` / `OfType<T>()`                    | static  | exact runtime type match                |
|  [02]   | `Selectors.Is(Type)` / `Is<T>()`                            | static  | type-or-derived match                   |
|  [03]   | `Selectors.Class(string)`                                   | static  | style-class or pseudo-class match       |
|  [04]   | `Selectors.Name(string)`                                    | static  | namescope name match                    |
|  [05]   | `Selectors.PropertyEquals(AvaloniaProperty, object?)`       | static  | property-value match                    |
|  [06]   | `Selectors.Child()` / `Descendant()`                        | static  | direct-child and any-descendant descent |
|  [07]   | `Selectors.Template()`                                      | static  | descend into the templated visual tree  |
|  [08]   | `Selectors.Nesting()`                                       | static  | re-enter the enclosing selector         |
|  [09]   | `Selectors.Not(Selector)` / `Not(Func<Selector?,Selector>)` | static  | invert a selector                       |
|  [10]   | `Selectors.Or(params Selector[])`                           | static  | union of selectors                      |
|  [11]   | `Selectors.NthChild(int, int)` / `NthLastChild(int, int)`   | static  | step-and-offset sibling position match  |
|  [12]   | `new Style(Func<Selector?, Selector>)` and `Style.Selector` | ctor    | bind the chain to a style               |

- Every combinator except `Or` extends a nullable previous `Selector`, so a chain starts from a null receiver and `Or` composes finished chains; `PropertyEquals<T>` takes `AvaloniaProperty<T>` and the untyped overload takes `AvaloniaProperty`.
- `Class` matches a pseudo-class when the name carries the leading colon — `.Class(":pointerover")` — because pseudo-classes live in the same `Classes` list as style classes.

[STYLE_SCOPE_OPERATIONS]: style, theme, resource, and pseudo-class scopes a control tree binds

| [INDEX] | [SURFACE]                                                              | [SHAPE]   | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------------- | :-------- | :------------------------------------- |
|  [01]   | `StyleBase.Setters` (`IList<SetterBase>`)                              | property  | styled assignments                     |
|  [02]   | `StyleBase.Animations` (`IList<IAnimation>`)                           | property  | selector-triggered animations          |
|  [03]   | `StyleBase.Children` (`IList<IStyle>`)                                 | property  | nested styles under this scope         |
|  [04]   | `StyleBase.Resources` (`IResourceDictionary`)                          | property  | style-scoped resources                 |
|  [05]   | `ControlTheme.TargetType` / `BasedOn`                                  | property  | theme target and inheritance           |
|  [06]   | `ResourceDictionary.ThemeDictionaries` (`IDictionary<ThemeVariant,…>`) | property  | per-variant resource partition         |
|  [07]   | `ResourceDictionary.MergedDictionaries` (`IList<IResourceProvider>`)   | property  | merged provider chain                  |
|  [08]   | `ResourceDictionary.AddDeferred(object, IDeferredContent)`             | instance  | shared lazily-built resource           |
|  [09]   | `ResourceDictionary.AddNotSharedDeferred(object, IDeferredContent)`    | instance  | per-request lazily-built resource      |
|  [10]   | `ThemeVariantScope.RequestedThemeVariant` / `ActualThemeVariant`       | property  | subtree variant request and resolution |
|  [11]   | `ItemsControl.ItemContainerTheme` (`ControlTheme?`)                    | property  | per-container theme                    |
|  [12]   | `ItemsControl.ItemsPanel` (`ITemplate<Panel?>`)                        | property  | realized panel factory                 |
|  [13]   | `StyledElement.PseudoClasses` (`IPseudoClasses`)                       | protected | self-owned pseudo-class write face     |
|  [14]   | `PseudoClassesExtensions.Set(IPseudoClasses, string, bool)`            | static    | boolean pseudo-class toggle            |

- `ControlTheme` nests through the inherited `StyleBase.Children`, so a per-state theme rule is a nested `Style` under the theme rather than a sibling registration.
- `Classes.Add`/`Insert`/`Remove` throw on a colon-prefixed name and `Classes.Clear()` keeps pseudo-classes, so a control mutates its own pseudo-classes only through the protected `IPseudoClasses` face; stock rosters ride `[PseudoClasses]` on `InputElement`, `Button`, `ToggleButton`, `ListBoxItem`, and `TextBox`.

[MARKUP_EXTENSION_OPERATIONS]: XAML-authored value producers and template classes

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `StaticResourceExtension(object).ProvideValue(sp)`  | ctor     | resolve once at parse                           |
|  [02]   | `DynamicResourceExtension(object).ProvideValue(sp)` | ctor     | yield a `BindingBase` that re-resolves          |
|  [03]   | `OnPlatformExtension<TReturn>(TReturn)`             | ctor     | default value plus per-OS `On<TReturn>` options |
|  [04]   | `OnFormFactorExtension<TReturn>(TReturn)`           | ctor     | default value plus per-form-factor options      |
|  [05]   | `ResolveByNameExtension(string).ProvideValue(sp)`   | ctor     | resolve a named element from the namescope      |
|  [06]   | `ControlTemplate.TargetType` / `Content`            | property | template target type and its content            |
|  [07]   | `ItemsPanelTemplate.Content` / `Build()`            | property | panel factory content and materialization       |

- `DynamicResourceExtension` derives `BindingBase`, so a dynamic reference is a live binding a dictionary edit pushes through; `StaticResourceExtension` returns the resolved object and never re-reads.

[INPUT_AND_ROUTE_OPERATIONS]: focus, key binding, routed events, and dispatch

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `InputElement.Focus(NavigationMethod, KeyModifiers)`                     | instance | focus movement             |
|  [02]   | `FocusManager.GetFocusedElement() / TryMoveFocus(NavigationDirection)`   | instance | focus ownership and move   |
|  [03]   | `InputElement.KeyBindings` (`List<KeyBinding>`)                          | property | gesture-binding collection |
|  [04]   | `KeyGesture(Key, KeyModifiers) / Parse / Matches`                        | ctor     | value-equal chord          |
|  [05]   | `Interactive.AddHandler / RemoveHandler(RoutedEvent, handler, strategy)` | instance | routed-event handling      |
|  [06]   | `InteractiveExtensions.GetObservable(RoutedEvent)`                       | static   | routed-event stream        |
|  [07]   | `Dispatcher.UIThread.Invoke / InvokeAsync / Post`                        | static   | render-thread marshal      |
|  [08]   | `Dispatcher.CheckAccess() / VerifyAccess()`                              | instance | thread-affinity guard      |
|  [09]   | `Dispatcher.ToTaskScheduler() / ToTaskScheduler(DispatcherPriority)`     | instance | TaskScheduler for TPL      |

- `Dispatcher.ToTaskScheduler`: yields a `TaskScheduler` that runs continuations on this dispatcher; the no-arg form captures the current `AvaloniaSynchronizationContext` priority, else `DispatcherPriority.Default`.

[POINTER_OPERATIONS]: the per-event pointer reads a gesture or digitizer surface takes

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `PointerEventArgs.GetPosition(Visual?)`           | instance | position in a visual's frame    |
|  [02]   | `PointerEventArgs.GetCurrentPoint(Visual?)`       | instance | the event's own reading         |
|  [03]   | `PointerEventArgs.GetIntermediatePoints(Visual?)` | instance | the whole coalesced burst       |
|  [04]   | `PointerEventArgs.Properties` / `.Pointer`        | property | device state and pointer        |
|  [05]   | `PointerEventArgs.Timestamp` / `.KeyModifiers`    | property | event instant and modifiers     |
|  [06]   | `PointerEventArgs.PreventGestureRecognition()`    | instance | suppress downstream recognizers |
|  [07]   | `Error.New(IInputElement?.Message, IInputElement?)` / `.Captured`  | instance | capture custody                 |

- `GetIntermediatePoints` returns the platform's coalesced samples with the current point APPENDED LAST, and answers a one-element list carrying only the current point when the platform coalesced nothing — so it is total and `GetCurrentPoint` is the read that discards a burst, never the safer one.
- Each intermediate point rebuilds its `PointerPointProperties` from the event's own properties with that sample's raw twist, pressure, tilt, and contact rect, so per-sample digitizer state survives the projection while the button flags stay the event's.
- `GetPosition` round-trips through screen coordinates across a `PresentationSource` boundary and answers the default `Point` when either source fails to resolve, so a position read against a foreign root degrades to origin rather than throwing.

[XAML_AND_RENDER_OPERATIONS]: XAML load and visual invalidation

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `AppBuilder.Configure<TApp>() / Configure`         | static   | application root        |
|  [02]   | `AvaloniaXamlLoader.Load / Parse`                  | static   | XAML materialize        |
|  [03]   | `Visual.InvalidateVisual`                          | instance | render refresh          |
|  [04]   | `Visual.OnAttachedToVisualTree / OnDetachedFrom…`  | override | tree-attachment hooks   |
|  [05]   | `Visual.AttachedToVisualTree / DetachedFrom…`      | event    | the same pair as events |
|  [06]   | `Layoutable.InvalidateMeasure`                     | instance | layout refresh          |
|  [07]   | `Layoutable.InvalidateArrange`                     | instance | arrange refresh         |
|  [08]   | `TopLevel.RequestAnimationFrame(Action<TimeSpan>)` | instance | one frame-tick callback |

- `TopLevel.RequestAnimationFrame` delivers a single tick carrying the frame timestamp; re-requesting from inside the callback is the frame loop, and on an embedded root the host's own run loop is what advances it — `StartRendering()` beside that self-rescheduling callback needs no clock of the caller's own.

[HOST_BUILD_OPERATIONS]: application-builder option admission and native host handle

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]         |
| :-----: | :---------------------------------------------------- | :------- | :------------------- |
|  [01]   | `AppBuilder.With<T>(T) / With<T>(Func<T>)`            | instance | option registration  |
|  [02]   | `AppBuilder.SetupWithoutStarting()`                   | instance | run-loop-free setup  |
|  [03]   | `TopLevel.TryGetPlatformHandle() -> IPlatformHandle?` | instance | native window handle |

- `AppBuilder.SetupWithoutStarting`: builds and configures without entering the run loop.
- `TopLevel.TryGetPlatformHandle`: returns `IPlatformHandle?` whose `Handle` is `nint`.

[LAYOUT_PASS_OPERATIONS]: the measure/arrange pass a custom `Panel` overrides

| [INDEX] | [SURFACE]                                                    | [SHAPE]   | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------- | :-------- | :---------------------------- |
|  [01]   | `Layoutable.Measure(Size)` / `Arrange(Rect)`                 | instance  | drive a child's pass          |
|  [02]   | `Layoutable.MeasureOverride(Size)` / `ArrangeOverride(Size)` | protected | own the pass body             |
|  [03]   | `Layoutable.MeasureCore(Size)` / `ArrangeCore(Rect)`         | protected | pre-override pass scaffolding |
|  [04]   | `Layoutable.DesiredSize` (`Size`, private set)               | property  | last measured extent          |
|  [05]   | `Layoutable.IsMeasureValid` / `IsArrangeValid`               | property  | pass validity flags           |
|  [06]   | `Layoutable.AffectsMeasure<T>` / `AffectsArrange<T>`         | static    | property-to-invalidation bind |
|  [07]   | `Layoutable.UpdateLayout()`                                  | instance  | synchronous pass drive        |
|  [08]   | `Layoutable.LayoutUpdated` / `EffectiveViewportChanged`      | event     | post-pass and viewport edges  |

- `Measure` short-circuits when `IsMeasureValid` holds against the same `availableSize`, and it notifies the VISUAL parent only when the newly measured `DesiredSize` differs from the previous one; that notification is `internal` and invalidates the parent's measure only while the parent is not itself mid-measure, so a child measured inside the parent's own `MeasureOverride` never re-enters the pass, while an out-of-band child re-measure that moves its desired size does. `InvalidateMeasure` walks no ancestor — it flags the element and queues it on the layout manager — so parent re-entry rides the desired-size edge alone and never a subscription.

[PANEL_LAYOUT_OPERATIONS]: the per-panel spacing knobs and attached placement slots a screen writes

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------------- |
|  [01]   | `Panel.Children` (`Controls`)                                       | property | child collection                             |
|  [02]   | `StackPanel.Spacing` / `Orientation`                                | property | inter-child gap and axis                     |
|  [03]   | `Grid.RowSpacing` / `ColumnSpacing`                                 | property | inter-track gaps                             |
|  [04]   | `Grid.RowDefinitions` / `ColumnDefinitions`                         | property | track definitions                            |
|  [05]   | `DockPanel.HorizontalSpacing` / `VerticalSpacing` / `LastChildFill` | property | per-axis gaps and residual fill              |
|  [06]   | `WrapPanel.ItemSpacing` / `LineSpacing` / `ItemsAlignment`          | property | in-line gap, cross-line gap, and alignment   |
|  [07]   | `UniformGrid.RowSpacing` / `ColumnSpacing` / `Rows` / `Columns`     | property | cell gaps and explicit cell counts           |
|  [08]   | `Canvas.SetLeft / SetTop / SetRight / SetBottom(AvaloniaObject)`    | static   | absolute edge offsets                        |
|  [09]   | `Grid.SetRow / SetColumn / SetRowSpan / SetColumnSpan(Control)`     | static   | track placement and span                     |
|  [10]   | `Grid.SetIsSharedSizeScope(Control, bool)`                          | static   | shared-size grouping scope                   |
|  [11]   | `DockPanel.SetDock(Control, Dock)`                                  | static   | dock edge                                    |
|  [12]   | `RelativePanel.SetLeftOf / SetAbove(AvaloniaObject, object)`        | static   | sibling-relative placement                   |
|  [13]   | `RelativePanel.SetAlignLeftWithPanel(AvaloniaObject, bool)`         | static   | panel-relative placement                     |
|  [14]   | `LayoutTransformControl.LayoutTransform` / `UseRenderTransform`     | property | pre-measure transform and render-path opt-in |
|  [15]   | `Viewbox.Child` / `Stretch` / `StretchDirection`                    | property | scaled child and fit policy                  |

- Every attached placement slot pairs a `Set…`/`Get…` accessor with its `…Property` field; `Canvas` and `RelativePanel` accessors take `AvaloniaObject` while `Grid` and `DockPanel` accessors take `Control`.
- `RelativePanel` carries a sibling-target family (`AlignLeftWith`, `AlignTopWith`, `AlignRightWith`, `AlignBottomWith`, `AlignHorizontalCenterWith`, `AlignVerticalCenterWith`, `LeftOf`, `RightOf`, `Above`, `Below`) typed `object` and a panel-relative `…WithPanel` twin of each alignment slot typed `bool`.

[LAYOUT_ROUNDING_OPERATIONS]: `Avalonia.Layout.LayoutHelper` — device-pixel snapping a custom pass reuses

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `Layoutable.UseLayoutRounding` (`bool`)                 | property | inherited snapping opt-in          |
|  [02]   | `LayoutHelper.GetLayoutScale(Layoutable)`               | static   | effective device-pixel scale       |
|  [03]   | `LayoutHelper.RoundLayoutValue(double, double)`         | static   | nearest-pixel scalar snap          |
|  [04]   | `LayoutHelper.RoundLayoutValueUp(double, double)`       | static   | ceiling scalar snap                |
|  [05]   | `LayoutHelper.RoundLayoutSizeUp(Size, double)`          | static   | ceiling extent snap                |
|  [06]   | `LayoutHelper.RoundLayoutThickness(Thickness, double)`  | static   | per-edge thickness snap            |
|  [07]   | `LayoutHelper.RoundLayoutPoint(Point, double)`          | static   | nearest-pixel point snap           |
|  [08]   | `LayoutHelper.LayoutEpsilon` (`double`)                 | property | comparison tolerance the pass uses |
|  [09]   | `LayoutHelper.MeasureChild / ArrangeChild(…)`           | static   | padded and bordered child pass     |
|  [10]   | `LayoutHelper.ApplyLayoutConstraints(Layoutable, Size)` | static   | min/max clamp of a constraint      |

- `UseLayoutRoundingProperty` registers `inherits: true` with a `true` default, so snapping reaches every descendant until a subtree writes `false`.

[PAINT_OPERATIONS]: the fill, border, shadow, and gradient slots a surface writes

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Border.Background` / `BorderBrush` / `BorderThickness`           | property | fill and stroke                                |
|  [02]   | `Border.CornerRadius` (`CornerRadius`)                            | property | per-corner rounding                            |
|  [03]   | `Border.BackgroundSizing` (`BackgroundSizing`)                    | property | which border edge clips the fill               |
|  [04]   | `Border.BoxShadow` (`BoxShadows`)                                 | property | ordered shadow stack                           |
|  [05]   | `BoxShadow.OffsetX / OffsetY / Blur / Spread / Color / IsInset`   | property | one shadow's geometry and paint                |
|  [06]   | `BoxShadow.Parse(string)`                                         | static   | `[inset] X Y [Blur [Spread]] Color`, or `none` |
|  [07]   | `BoxShadows.Parse(string)`                                        | static   | comma-separated shadow list, or `none`         |
|  [08]   | `BoxShadows.Count` / `this[int]` / `HasInsetShadows`              | property | stack arity, indexer, and inset probe          |
|  [09]   | `BoxShadows.TransformBounds(in Rect)`                             | instance | shadow-inflated bounds                         |
|  [10]   | `BoxShadows(BoxShadow)` / `BoxShadows(BoxShadow, BoxShadow[])`    | ctor     | single shadow, or leading shadow plus the rest |
|  [11]   | `GradientBrush.GradientStops` / `SpreadMethod`                    | property | ramp stops and outside-ramp policy             |
|  [12]   | `LinearGradientBrush.StartPoint` / `EndPoint` (`RelativePoint`)   | property | ramp axis                                      |
|  [13]   | `RadialGradientBrush.Center` / `GradientOrigin` (`RelativePoint`) | property | ellipse centre and focal point                 |
|  [14]   | `RadialGradientBrush.RadiusX` / `RadiusY` (`RelativeScalar`)      | property | independent ellipse radii                      |
|  [15]   | `ConicGradientBrush.Center` / `Angle`                             | property | sweep centre and start angle                   |
|  [16]   | `IMutableBrush.ToImmutable() -> IImmutableBrush`                  | instance | render-thread-safe brush snapshot              |
|  [17]   | `Brush.Parse(string) -> IBrush`                                   | static   | brush value from markup                        |

- `BoxShadow.Parse` accepts three to six whitespace-separated tokens, reading a third value as `Blur` and a fourth as `Spread`; `BoxShadows.Parse` splits on commas outside parentheses so a function-form colour survives the split.
- The stack carries a LEADING shadow beside an optional rest array, so a composed stack binds row [10]'s two-argument form and `default(BoxShadows)` is the empty stack a variant carving every shadow slot resolves; a stack folded from a carrier therefore binds the leading element rather than assuming one.
- `RadialGradientBrush` sizes on `RadiusX`/`RadiusY` typed `RelativeScalar`, both defaulting to `RelativeScalar.Middle`.

[VECTOR_IMAGE_OPERATIONS]: retained-drawing construction, geometry parsing, and the raster round-trip an image product takes

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `Geometry.Parse(string) -> Geometry`                        | static   | path-data to geometry            |
|  [02]   | `StreamGeometry.Parse(string) -> StreamGeometry`            | static   | typed path-data parse            |
|  [03]   | `Geometry.Bounds` / `GetRenderBounds(IPen)`                 | property | fill and stroke extents          |
|  [04]   | `GeometryDrawing.{Geometry,Brush,Pen}`                      | property | geometry, fill, stroke           |
|  [05]   | `ImageDrawing.{ImageSource,Rect}`                           | property | placed image and its box         |
|  [06]   | `DrawingGroup.{Children,Transform,ClipGeometry,Opacity}`    | property | composed retained drawing        |
|  [07]   | `DrawingImage.{Drawing,Viewbox}` / `.Size` / `.Invalidated` | property | image product, extent, re-render |
|  [08]   | `Matrix.{CreateScale,CreateTranslation,CreateRotation}`     | static   | transform factors                |
|  [09]   | `MatrixTransform(Matrix)`                                   | ctor     | matrix as bindable transform     |
|  [10]   | `Bitmap(Stream)` / `.PixelSize` / `.Dpi` / `.Dispose()`     | instance | decoded raster and its extents   |
|  [11]   | `RenderTargetBitmap(PixelSize)` / `CreateDrawingContext()`  | instance | render target and its context    |
|  [12]   | `IImage.Draw(DrawingContext, Rect, Rect)`                   | instance | source-to-destination blit       |

- `Matrix` composes row-vector first: `a * b` applies `a` then `b`, so a place-then-mirror fold reads left to right.
- `Bitmap` and `RenderTargetBitmap` are `IDisposable` over platform surfaces while `DrawingImage` is not, so a cache holding mixed products releases through the disposable interface rather than a per-type branch.

[CURSOR_OPERATIONS]: pointer construction and the inherited slot every interaction surface writes

| [INDEX] | [SURFACE]                    | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------- | :------- | :----------------------------- |
|  [01]   | `Cursor(StandardCursorType)` | ctor     | platform pointer               |
|  [02]   | `Cursor(Bitmap, PixelPoint)` | ctor     | drawn pointer with hotspot     |
|  [03]   | `Cursor.Default`             | static   | the arrow pointer              |
|  [04]   | `Cursor.Parse(string)`       | static   | case-insensitive member parse  |
|  [05]   | `Cursor.Dispose()`           | instance | platform handle release        |
|  [06]   | `InputElement.Cursor`        | property | `StyledProperty<Cursor?>` slot |

- Both constructors resolve `ICursorFactory` from the application locator, so cursor construction throws before the platform initializes and belongs behind a typed trap.
- `InputElement.CursorProperty` registers with inheritance on, so one write at an interaction root reaches every descendant and a per-control cursor write is only for the descendants that differ.
- `Cursor.Parse` throws `ArgumentException` for an unrecognized member, so a string-addressed pointer is a runtime failure where a typed row is a compile-time one.

[VISUAL_COMPOSITION_OPERATIONS]: the per-`Visual` compositing slots and the drawing-context scopes that carry them

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Visual.Opacity` (`double`) / `OpacityMask` (`IBrush?`)                | property | uniform and masked transparency        |
|  [02]   | `Visual.Clip` (`Geometry?`) / `ClipToBounds` (`bool`)                  | property | explicit and bounds clipping           |
|  [03]   | `Visual.ZIndex` (`int`)                                                | property | sibling paint order                    |
|  [04]   | `Visual.Effect` (`IEffect?`)                                           | property | per-visual pixel filter                |
|  [05]   | `Visual.CacheMode` (`CacheMode?`)                                      | property | rasterized-subtree caching             |
|  [06]   | `Visual.RenderTransform` (`ITransform?`) / `RenderTransformOrigin`     | property | post-layout transform and its pivot    |
|  [07]   | `Visual.FlowDirection` and `Visual.GetFlowDirection(Visual)`           | property | inherited reading direction            |
|  [08]   | `DrawingContext.PushOpacity(double)` / `PushOpacityMask(IBrush, Rect)` | instance | scoped transparency                    |
|  [09]   | `DrawingContext.PushClip(Rect)` / `PushGeometryClip(Geometry)`         | instance | scoped clipping                        |
|  [10]   | `DrawingContext.PushTransform(Matrix)`                                 | instance | scoped transform                       |
|  [11]   | `DrawingContext.PushEffect(IEffect, Rect)`                             | instance | scoped pixel filter over a bounds rect |
|  [12]   | `DrawingContext.PushRenderOptions(RenderOptions)`                      | instance | scoped raster policy                   |
|  [13]   | `DrawingContext.PushTextOptions(TextOptions)`                          | instance | scoped text raster policy              |

- Every `Push…` returns a `PushedState` the caller disposes to pop the scope.

[EFFECT_OPERATIONS]: the pixel-filter chain a `Visual` or drawing scope binds

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Effect.Parse(string) -> IEffect`                                | static   | `blur(r)` or `drop-shadow(x y [blur] [color])` |
|  [02]   | `BlurEffect.Radius` (`double`)                                   | property | gaussian radius                                |
|  [03]   | `DropShadowEffect.OffsetX` / `OffsetY` (`double`)                | property | cartesian shadow offset                        |
|  [04]   | `DropShadowEffectBase.BlurRadius` / `Color` / `Opacity`          | property | shadow softness and paint                      |
|  [05]   | `DropShadowDirectionEffect.Direction` / `ShadowDepth`            | property | polar shadow offset                            |
|  [06]   | `EffectExtensions.ToImmutable(this IEffect) -> IImmutableEffect` | static   | render-thread-safe effect snapshot             |
|  [07]   | `Effect.Invalidated`                                             | event    | repaint edge a mutable effect raises           |

- `Effect.Parse` yields an immutable instance directly, and a parsed `drop-shadow` fixes `Opacity` at `1.0`; omitting the colour paints black.
- `DropShadowDirectionEffect.ToImmutable()` passes its computed `OffsetX`/`OffsetY` into the `ImmutableDropShadowDirectionEffect(direction, shadowDepth, …)` slots, so the snapshot carries transposed geometry — bind `DropShadowEffect` for any effect that reaches the render thread through `ToImmutable`.

[RENDER_TUNING_OPERATIONS]: per-subtree rasterization policy, caching, and operation-wise transforms

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `RenderOptions.SetEdgeMode / GetEdgeMode(Visual)`                   | static   | geometry antialias policy                   |
|  [02]   | `RenderOptions.SetBitmapInterpolationMode / Get…(Visual)`           | static   | image resample quality                      |
|  [03]   | `RenderOptions.SetBitmapBlendingMode / Get…(Visual)`                | static   | image composite operator                    |
|  [04]   | `RenderOptions.SetRequiresFullOpacityHandling / Get…(Visual)`       | static   | forced opacity-layer handling               |
|  [05]   | `RenderOptions.MergeWith(RenderOptions)`                            | instance | inherit-then-override merge                 |
|  [06]   | `TextOptions.SetTextRenderingMode / SetTextHintingMode(Visual)`     | static   | glyph raster and hinting policy             |
|  [07]   | `TextOptions.SetBaselinePixelAlignment(Visual)`                     | static   | baseline snapping policy                    |
|  [08]   | `TextOptions.SetTextOptions / GetTextOptions(Visual)`               | static   | whole-record read and write                 |
|  [09]   | `BitmapCache.RenderAtScale / SnapsToDevicePixels / EnableClearType` | property | cache resolution and text-raster knobs      |
|  [10]   | `CacheMode.Parse(string)`                                           | static   | cache-mode value from markup                |
|  [11]   | `TransformOperations.Parse(string)`                                 | static   | CSS-shaped transform list                   |
|  [12]   | `TransformOperations.CreateBuilder(int) -> Builder`                 | static   | capacity-bounded op accumulator             |
|  [13]   | `Builder.AppendTranslate / AppendRotate / AppendScale / AppendSkew` | instance | one op per call                             |
|  [14]   | `Builder.AppendMatrix(Matrix)` / `AppendIdentity()` / `Build()`     | instance | raw matrix op, identity op, and finish      |
|  [15]   | `TransformOperations.Interpolate(from, to, double)`                 | static   | operation-wise blend of two lists           |
|  [16]   | `TransformOperations.Operations` / `Value` / `IsIdentity`           | property | op list, collapsed `Matrix`, identity probe |

- `TransformOperations.Interpolate` blends op-by-op rather than blending collapsed matrices, so a transform a style or transition animates binds `TransformOperations`; every other `ITransform` interpolates through its matrix.
- `RenderOptions` and `TextOptions` are record structs whose attached accessors write one field of the inherited record; `MergeWith` fills every field still holding its `Unspecified` member from the argument.

[ANIMATION_OPERATIONS]: implicit transitions, keyframe animations, and easing selection

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `Animatable.Transitions` (`Transitions?`)                       | property | implicit-transition list of an element       |
|  [02]   | `TransitionBase.Property` (`AvaloniaProperty?`)                 | property | the slot a transition animates               |
|  [03]   | `TransitionBase.Duration` / `Delay` / `Easing`                  | property | transition timing                            |
|  [04]   | `Animation.Duration` / `Delay` / `DelayBetweenIterations`       | property | animation timing                             |
|  [05]   | `Animation.IterationCount` (`IterationCount`)                   | property | finite or infinite repeat                    |
|  [06]   | `Animation.PlaybackDirection` / `PlaybackBehavior` / `FillMode` | property | direction, run gating, and value retention   |
|  [07]   | `Animation.SpeedRatio` / `Easing`                               | property | rate scale and curve                         |
|  [08]   | `Animation.Children` (`KeyFrames`)                              | property | ordered keyframes                            |
|  [09]   | `Animation.RunAsync(Animatable, CancellationToken)`             | instance | drive one animation to completion            |
|  [10]   | `Animation.RegisterCustomAnimator<T, TAnimator>()`              | static   | admit an interpolator for a value type       |
|  [11]   | `KeyFrame.Cue` / `KeyTime` / `KeySpline` / `Setters`            | property | keyframe position, reshape, and assignments  |
|  [12]   | `KeySpline.Parse(string, CultureInfo?)` / `GetSplineProgress`   | static   | bezier control points and progress reshape   |
|  [13]   | `Easing.Parse(string) -> Easing`                                | static   | easing by type name, or a spline from points |
|  [14]   | `SplineEasing.X1 / Y1 / X2 / Y2`                                | property | cubic-bezier control points                  |
|  [15]   | `SpringEasing.Mass / Stiffness / Damping / InitialVelocity`     | property | physical spring parameters                   |
|  [16]   | `AvaloniaProperty.IsDirect`                                     | property | the admission a transition target must fail  |
|  [17]   | `CrossFade.Duration` / `FadeInEasing` / `FadeOutEasing`         | property | dissolve length and its asymmetric curves    |
|  [18]   | `PageSlide(TimeSpan, SlideAxis)` / `Orientation`                | ctor     | slide length and axis                        |
|  [19]   | `PageSlide.SlideInEasing` / `SlideOutEasing`                    | property | the asymmetric entrance and exit curves      |
|  [20]   | `CompositePageTransition.PageTransitions`                       | property | `List<IPageTransition>` run together         |
|  [21]   | `IPageTransition.Start(Visual?, Visual?, bool, Cancellation)`   | instance | run one swap to completion                   |
|  [22]   | `IProgressPageTransition.Update(...)` / `Reset(Visual)`         | instance | drive a swap from progress, then clear it    |
|  [23]   | `TransitioningContentControl.PageTransition`                    | property | the swap this host runs                      |
|  [24]   | `TransitioningContentControl.IsTransitionReversed`              | property | direction of a bidirectional swap            |
|  [25]   | `TransitioningContentControl.TransitionCompleted`               | event    | direct-routed completion with its two pages  |

- `Transitions` validates on admission from the UI thread and throws for a `DirectProperty` target, so only a styled or attached slot transitions.
- `TransitioningContentControl.PageTransitionProperty` defaults to an internal immutable cross-fade carrying its own inline duration, so an unassigned host still animates on a timing no token owns; assignment at mount is the only way that default stops running.
- Every shipped page transition implements `IProgressPageTransition` beside `IPageTransition` — `CrossFade`, `PageSlide`, and `CompositePageTransition` declare it and `Rotate3DTransition` inherits it from `PageSlide` — so a gesture-driven swap drives `Update` per pointer sample and `Reset` releases the visuals.
- `Easing.Parse` reads a comma-bearing string as `KeySpline` control points and otherwise resolves the simple type name inside `Avalonia.Animation.Easings`, so `<Curve>Ease{In,Out,InOut}`, `LinearEasing`, `SplineEasing`, and `SpringEasing` are exactly the parseable names.
- Each implicit transition names the value type it animates, and the shipped set closes at `BoolTransition`, `BoxShadowsTransition`, `BrushTransition`, `ColorTransition`, `CornerRadiusTransition`, `DoubleTransition`, `EffectTransition`, `FloatTransition`, `IntegerTransition`, `PointTransition`, `RelativePointTransition`, `SizeTransition`, `ThicknessTransition`, `TransformOperationsTransition`, and `VectorTransition`; `BrushTransition` and `EffectTransition` swap discretely at half progress when the two ends carry incompatible shapes, so a continuously varying effect parameter is a redraw rather than a transition.

[AUTOMATION_OPERATIONS]: attached automation identity, live regions, peers, and keyboard navigation

| [INDEX] | [SURFACE]                                                                | [SHAPE]   | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------------- | :-------- | :---------------------------------------- |
|  [01]   | `AutomationProperties.SetAutomationId / GetAutomationId(StyledElement)`  | static    | stable automation identity                |
|  [02]   | `AutomationProperties.SetName / GetName(StyledElement)`                  | static    | announced name                            |
|  [03]   | `AutomationProperties.SetHelpText / GetHelpText(StyledElement)`          | static    | announced description                     |
|  [04]   | `AutomationProperties.SetLiveSetting / GetLiveSetting(StyledElement)`    | static    | live-region posture                       |
|  [05]   | `AutomationProperties.SetAccessKey / GetAccessKey(StyledElement)`        | static    | announced accelerator text                |
|  [06]   | `AutomationProperties.SetLabeledBy / GetLabeledBy(StyledElement)`        | static    | external label association                |
|  [07]   | `Control.OnCreateAutomationPeer() -> AutomationPeer`                     | protected | per-control peer mint                     |
|  [08]   | `ControlAutomationPeer(Control owner)` / `.Owner`                        | ctor      | the peer base an authored control derives |
|  [09]   | `AutomationPeer.GetAutomationControlTypeCore() -> AutomationControlType` | protected | declared control type                     |
|  [10]   | `AutomationPeer.GetClassNameCore() -> string`                            | protected | declared class name                       |
|  [11]   | `AutomationPeer.GetAutomationIdCore() -> string?`                        | protected | declared automation id                    |
|  [12]   | `KeyboardNavigation.SetTabIndex / GetTabIndex(IInputElement)`            | static    | tab rank                                  |
|  [13]   | `KeyboardNavigation.SetTabNavigation / GetTabNavigation(InputElement)`   | static    | region navigation mode                    |
|  [14]   | `KeyboardNavigation.SetIsTabStop / GetIsTabStop(InputElement)`           | static    | tab-stop admission                        |
|  [15]   | `KeyboardNavigation.SetTabOnceActiveElement / GetTabOnceActiveElement`   | static    | `Once` region re-entry seat               |
|  [16]   | `InputElement.IsHitTestVisible` (`bool`)                                 | property  | pointer transparency                      |

- `TabIndexProperty` defaults to `int.MaxValue` and `TabNavigationProperty` to `KeyboardNavigationMode.Continue`, so an unranked stop sorts last and an unset region continues outward; `LiveSettingProperty` defaults to `Off`, which is why a silent row states `Off` rather than omitting the write.

[EMBED_OPERATIONS]: foreign-view root lifecycle and native handle access

| [INDEX] | [SURFACE]                                                                  | [SHAPE]   | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------- | :-------- | :-------------------------------- |
|  [01]   | `new EmbeddableControlRoot()` / `(ITopLevelImpl)`                          | ctor      | embedded root construction        |
|  [02]   | `EmbeddableControlRoot.Prepare()`                                          | instance  | initialize and apply template     |
|  [03]   | `EmbeddableControlRoot.StartRendering() / StopRendering()`                 | instance  | render loop start and stop        |
|  [04]   | `EmbeddableControlRoot.EnforceClientSize` (`bool`)                         | protected | track the host view's client size |
|  [05]   | `EmbeddableControlRoot.Dispose()`                                          | instance  | root teardown                     |
|  [06]   | `IMacOSTopLevelPlatformHandle.NSView / NSWindow` (`nint`)                  | property  | unretained native handles         |
|  [07]   | `IMacOSTopLevelPlatformHandle.GetNSViewRetained() / GetNSWindowRetained()` | instance  | retained native handles           |

- `EmbeddableControlRoot` derives `TopLevel` and implements `IFocusScope` and `IDisposable`; `StartRendering`/`StopRendering` are `new` members shadowing the `TopLevel` pair, and `EnforceClientSize` is a protected setter reachable only from a derived capsule.
- `TopLevel.GetTopLevel(Visual)` is the ONLY public root query — `Avalonia.VisualTree.VisualExtensions` declares no `GetVisualRoot` and `Visual.VisualRoot` is `protected internal` — and it keeps answering the root after `EmbeddableControlRoot.Dispose()`, so it proves attachment and never liveness.
- `Dispose()` on an embedded root raises no `Closed`, `DetachedFromVisualTree`, or `DetachedFromLogicalTree` edge, and a second `Dispose()` or a post-dispose `StartRendering()` is inert, so teardown ordering is the caller's disposable and never a lifecycle subscription.
- `IMacOSTopLevelPlatformHandle` carries Avalonia's `[Unstable]` marker, and the two `…Retained` accessors hand back a retained pointer whose release the caller owns; the unretained `NSView`/`NSWindow` properties do not.

[SHELL_CHROME_OPERATIONS]: OS menu export and tray indicator composition

| [INDEX] | [SURFACE]                                                           | [SHAPE]         | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------ | :-------------- | :--------------------------- |
|  [01]   | `NativeMenu.MenuProperty` (`AttachedProperty<NativeMenu?>`)         | attached        | menu attach point            |
|  [02]   | `NativeMenu.SetMenu / GetMenu(AvaloniaObject)`                      | static          | menu assignment and lookup   |
|  [03]   | `NativeMenu.GetIsNativeMenuExported(TopLevel) -> bool`              | static          | OS-export probe              |
|  [04]   | `NativeMenu.Items` / `Add(NativeMenuItemBase)`                      | instance        | menu composition             |
|  [05]   | `NativeMenu.NeedsUpdate / Opening / Closed`                         | event           | menu lifecycle edges         |
|  [06]   | `NativeMenuItem.Header / Icon / ToolTip / Gesture`                  | property        | item presentation            |
|  [07]   | `NativeMenuItem.Command / CommandParameter / IsEnabled / IsVisible` | property        | item command and gating      |
|  [08]   | `NativeMenuItem.IsChecked / ToggleType` (`MenuItemToggleType`)      | property        | item toggle state            |
|  [09]   | `TrayIcon.IconsProperty` (`AttachedProperty<TrayIcons?>`)           | attached        | tray collection attach point |
|  [10]   | `TrayIcon.SetIcons / GetIcons(Application)`                         | static          | tray collection assignment   |
|  [11]   | `TrayIcon.Icon / ToolTipText / Menu / IsVisible`                    | property        | indicator presentation       |
|  [12]   | `TrayIcon.Command / CommandParameter` and `Clicked`                 | property, event | indicator activation         |

- `NativeMenu.IsNativeMenuExportedProperty` is the attached flag the platform sets; `GetIsNativeMenuExported` takes a `TopLevel` because the export is per-window, and `TrayIcon.SetIcons` takes the `Application` because the tray is per-process.

[WINDOW_CHROME_OPERATIONS]: window translucency, acrylic material, and the decoration surface a theme redraws

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `TopLevel.TransparencyLevelHint` (`IReadOnlyList<WindowTransparencyLevel>`) | property | ranked translucency request                  |
|  [02]   | `TopLevel.ActualTransparencyLevel` (`WindowTransparencyLevel`)              | property | the level the backend granted                |
|  [03]   | `TopLevel.TransparencyBackgroundFallback` (`IBrush`)                        | property | paint behind an ungranted level              |
|  [04]   | `WindowTransparencyLevel.None / Transparent / Blur / AcrylicBlur / Mica`    | static   | the requestable level vocabulary             |
|  [05]   | `ExperimentalAcrylicBorder.Material` / `CornerRadius`                       | property | acrylic-painted decorator                    |
|  [06]   | `ExperimentalAcrylicMaterial.TintColor` / `TintOpacity` (`0.8`)             | property | tint hue and its weight                      |
|  [07]   | `ExperimentalAcrylicMaterial.MaterialOpacity` (`0.5`) / `FallbackColor`     | property | material weight and opaque fallback          |
|  [08]   | `ExperimentalAcrylicMaterial.BackgroundSource` (`AcrylicBackgroundSource`)  | property | `None` tint over content, `Digger` erases it |
|  [09]   | `ExperimentalAcrylicMaterial.PlatformTransparencyCompensationLevel`         | property | per-backend opacity correction               |
|  [10]   | `Window.WindowDecorations` (`WindowDecorations`)                            | property | frame extent the platform draws              |
|  [11]   | `Window.WindowDecorationsTheme` (`ControlTheme?`)                           | property | theme applied to the drawn decorations       |
|  [12]   | `Window.ExtendClientAreaToDecorationsHint` (`bool`)                         | property | draw content under the decorations           |
|  [13]   | `Window.ExtendClientAreaTitleBarHeightHint` (`double`)                      | property | requested title-bar band height              |
|  [14]   | `Window.IsExtendedIntoWindowDecorations` / `WindowDecorationMargin`         | property | granted extension and its inset              |
|  [15]   | `WindowDrawnDecorations.Template` (`IWindowDrawnDecorationsTemplate?`)      | property | the drawn frame, title bar, and shadow tree  |
|  [16]   | `WindowDrawnDecorations.DefaultTitleBarHeight / DefaultFrameThickness`      | property | decoration metrics before platform override  |
|  [17]   | `WindowDecorationProperties.SetElementRole(Visual, …ElementRole)`           | static   | hit-role of one decoration element           |

- `TransparencyLevelHint` is a ranked list and the backend takes the first level it implements, so `ActualTransparencyLevel` is the only honest read of what landed.
- macOS maps `None`, `Transparent`, and `AcrylicBlur` alone; a list carrying only `Blur` or `Mica` resolves to `None`.
- Acrylic paints a tint-over-material shader composed with a fixed noise bitmap, never a live backdrop blur; `Digger` also sets a source blend mode erasing everything already drawn beneath the border, so it needs a translucent window behind it and digs through to nothing when the root is embedded in a host view.
- `ExtendClientArea…` hints and `WindowDecorations` live on `Window` alone — an `EmbeddableControlRoot` or other bare `TopLevel` carries neither.

[THEME_VARIANT_OPERATIONS]: variant request, resolution, and OS-probe read

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `Application.RequestedThemeVariant`                            | property | variant request     |
|  [02]   | `StyledElement.RequestedThemeVariant`                          | property | variant request     |
|  [03]   | `ThemeVariantScope.RequestedThemeVariant`                      | property | variant request     |
|  [04]   | `StyledElement.ActualThemeVariant / ActualThemeVariantChanged` | property | resolution and flip |
|  [05]   | `new ThemeVariant(inheritVariant)`                        | ctor     | inherited key       |
|  [06]   | `(ThemeVariant)platformThemeVariant`                           | operator | OS-probe cast       |
|  [07]   | `FluentTheme.Palettes[ThemeVariant]`                           | property | palette key         |

[PLATFORM_PREFERENCE_OPERATIONS]: the OS appearance and contrast read and its change edge

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Application.PlatformSettings -> IPlatformSettings?`                 | property | the one reachable settings handle             |
|  [02]   | `IPlatformSettings.GetColorValues() -> PlatformColorValues`          | instance | current appearance, contrast, and accents     |
|  [03]   | `IPlatformSettings.ColorValuesChanged`                               | event    | `EventHandler<PlatformColorValues>` flip edge |
|  [04]   | `IPlatformSettings.HoldWaitDuration` / `HotkeyConfiguration`         | property | hold gesture window and per-OS gesture roster |
|  [05]   | `IPlatformSettings.GetTapSize / GetDoubleTapSize / GetDoubleTapTime` | instance | per-pointer-type gesture geometry and window  |

- `TopLevel.PlatformSettings` is PRIVATE, so `Application.PlatformSettings` is the one reachable read, and it is nullable until the application initializes — a probe capsule therefore takes the resolved instance rather than resolving one per read.

[COMPILED_TEMPLATE_OPERATIONS]: per-control compiled template, theme binding, and template-part resolution

| [INDEX] | [SURFACE]                                                       | [SHAPE]   | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------- | :-------- | :---------------------------------- |
|  [01]   | `TemplatedControl.Template -> IControlTemplate?`                | property  | compiled visual-tree template       |
|  [02]   | `TemplatedControl.TemplateProperty`                             | static    | styled slot the template binds      |
|  [03]   | `StyledElement.Theme -> ControlTheme?`                          | property  | per-element control-theme bind      |
|  [04]   | `StyledElement.ThemeProperty`                                   | static    | styled slot the theme binds         |
|  [05]   | `TemplatedControl.OnApplyTemplate(TemplateAppliedEventArgs)`    | protected | per-apply template-part resolution  |
|  [06]   | `TemplateAppliedEventArgs.NameScope -> INameScope`              | property  | the applied template's name scope   |
|  [07]   | `TemplatePartAttribute(string name, Type type)` / `.IsRequired` | attribute | declared part name, type, necessity |
|  [08]   | `PseudoClassesAttribute(params string[])` / `.PseudoClasses`    | attribute | declared pseudo-class roster        |

[CONTROL_PROPERTY_OPERATIONS]: the styled slots a materialize fold writes by property rather than by member

| [INDEX] | [SURFACE]                                                           | [SHAPE]          | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------ | :--------------- | :------------------------------ |
|  [01]   | `AvaloniaObject.SetValue / GetValue / ClearValue(AvaloniaProperty)` | instance         | untyped slot write, read, reset |
|  [02]   | `ItemsControl.ItemsSourceProperty` (`IEnumerable?`)                 | static           | item source slot                |
|  [03]   | `SelectingItemsControl.SelectedValueProperty` (`object?`)           | static           | selected VALUE slot             |
|  [04]   | `SelectingItemsControl.SelectedValueBinding` (`BindingBase?`)       | property         | value projection off the item   |
|  [05]   | `SelectingItemsControl.SelectionModeProperty` (`SelectionMode`)     | static           | selection arity slot            |
|  [06]   | `TextBox.TextProperty` / `Watermark` / `AcceptsReturn`              | static, property | text entry slots                |
|  [07]   | `TextBlock.TextProperty` (`string`)                                 | static           | read-only text slot             |
|  [08]   | `NumericUpDown.ValueProperty` / `Minimum` / `Maximum` / `Increment` | static, property | `decimal` numeric slots         |
|  [09]   | `RangeBase.ValueProperty` / `Minimum` / `Maximum`                   | static, property | ranged scalar slots             |
|  [10]   | `ToggleButton.IsCheckedProperty` (`bool?`)                          | static           | tri-state toggle slot           |
|  [11]   | `ContentControl.ContentProperty` (`object?`)                        | static           | content slot                    |
|  [12]   | `TemplatedControl.ForegroundProperty` / `FontSizeProperty`          | static           | paint and metric slots          |
|  [13]   | `Layoutable.MinHeightProperty` / `MinWidthProperty`                 | static           | minimum-extent slots            |
|  [14]   | `GridSplitter.ResizeDirection` (`GridResizeDirection`)              | property         | split axis selection            |

- `NumericUpDown` slots are `decimal` while `RangeBase` slots are `double`, so a `double`-typed domain value casts at the `NumericUpDown` bind edge and nowhere else; `SelectedValueBinding` is what makes `SelectedValue` the option's own value rather than the container item, so a value-round-tripping bounded choice binds the pair, never `SelectedItem`.
- `ClearValue(AvaloniaProperty)` resets a slot to its default across every priority; the typed `ClearValue<T>` overloads exist for `AvaloniaProperty<T>`, `StyledProperty<T>`, and `DirectPropertyBase<T>`.
- `ListBox : SelectingItemsControl` re-declares `SelectedItems`, `Selection`, and `SelectionMode` as `new` public slots, so its arity is reachable where the base hides it; `SelectionMode` is `Single = 0`, `Multiple = 1`, `Toggle = 2`, `AlwaysSelected = 4`, so single selection is the DEFAULT and `AlwaysSelected` is the separate opt-in a group with no initial pick refuses.
- `TextBlock : Control` — NOT a `ContentControl` — so its text slot is `TextProperty` and `ContentControl.ContentProperty` is unregistered on it; an untyped `SetValue`/`ClearValue` of an unregistered slot throws, so a per-type value-slot table names `TextBlock` rather than falling to the content default. `Skeleton` (`Irihi.Ursa`) DOES derive `ContentControl`, so the content default is honest there.

[NOTIFICATION_OPERATIONS]: toast presentation surfaces

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------- | :------- | :-------------------- |
|  [01]   | `WindowNotificationManager.Show`             | instance | toast present         |
|  [02]   | `WindowNotificationManager.Close / CloseAll` | instance | toast close and clear |
|  [03]   | `WindowNotificationManager.Position`         | property | placement knob        |
|  [04]   | `WindowNotificationManager.MaxItems`         | property | queue cap             |

[STORAGE_OPERATIONS]: per-surface capsule resolution and picker dispatch

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `TopLevel.GetTopLevel(Visual?) -> TopLevel?`                      | static   | per-surface capsule resolve  |
|  [02]   | `TopLevel.StorageProvider -> IStorageProvider`                    | property | picker capsule               |
|  [03]   | `IStorageProvider.CanOpen / CanSave / CanPickFolder`              | property | per-kind platform capability |
|  [04]   | `IStorageProvider.OpenFilePickerAsync(FilePickerOpenOptions)`     | instance | open picker                  |
|  [05]   | `IStorageProvider.SaveFilePickerAsync(FilePickerSaveOptions)`     | instance | save picker                  |
|  [06]   | `IStorageProvider.OpenFolderPickerAsync(FolderPickerOpenOptions)` | instance | folder picker                |
|  [07]   | `FilePickerFileType(string?)` with `Patterns` / `MimeTypes`       | ctor     | one filter row               |
|  [08]   | `FilePickerOpenOptions.AllowMultiple / FileTypeFilter`            | property | open cardinality and filter  |
|  [09]   | `FilePickerSaveOptions.FileTypeChoices / DefaultExtension`        | property | save filter and extension    |

- `TopLevel.GetTopLevel` returns null for a visual attached to no root; `TopLevel.StorageProvider` NEVER returns null — an unserved platform yields an `internal` no-op provider whose three capability properties all answer false, so availability reads the capability the operation needs and a provider type test is unspellable outside the assembly.

[DATA_TRANSFER_OPERATIONS]: clipboard and drag data-transfer composition

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `IClipboard.SetDataAsync(IAsyncDataTransfer?)`                                       | instance | clipboard write                  |
|  [02]   | `IClipboard.TryGetDataAsync() / TryGetInProcessDataAsync()`                          | instance | clipboard read                   |
|  [03]   | `IClipboard.ClearAsync() / FlushAsync()`                                             | instance | clear and flush                  |
|  [04]   | `ClipboardExtensions.GetDataFormatsAsync()`                                          | static   | present-format probe             |
|  [05]   | `ClipboardExtensions.TryGetValueAsync<T>(DataFormat<T>) / TryGetValuesAsync<T>`      | static   | typed clip read                  |
|  [06]   | `ClipboardExtensions.SetValueAsync<T>(DataFormat<T>, T?) / SetValuesAsync<T>`        | static   | typed clip write                 |
|  [07]   | `ClipboardExtensions.TryGetTextAsync() / SetTextAsync(string?)`                      | static   | text clip read/write             |
|  [08]   | `ClipboardExtensions.TryGetFilesAsync() / TryGetBitmapAsync()`                       | static   | file and bitmap clip read        |
|  [09]   | `DataTransfer.Add(DataTransferItem)`                                                 | instance | item compose                     |
|  [10]   | `DataTransfer.Formats / Items`                                                       | property | format and item inventory        |
|  [11]   | `DataTransferItem.Create<T>(DataFormat<T>, T?) / Create<T>(DataFormat<T>, Func<T?>)` | factory  | per-format item make             |
|  [12]   | `DataTransferItem.CreateText(string?)`                                               | factory  | text item make                   |
|  [13]   | `DataTransferItem.SetText(string?) / Set<T>(DataFormat<T>, T?)`                      | instance | text and typed set               |
|  [14]   | `DataTransferItem.TryGetRaw(DataFormat)`                                             | instance | untyped per-format read          |
|  [15]   | `DataFormat.CreateBytesApplicationFormat / CreateStringApplicationFormat`            | static   | byte and string app format       |
|  [16]   | `DataFormat.CreateInProcessFormat<T> / Text / Bitmap / File`                         | static   | in-process and universal formats |
|  [17]   | `DragDrop.SetAllowDrop(Interactive, bool) / GetAllowDrop`                            | static   | enable drop target               |
|  [18]   | `DragDrop.DoDragDropAsync(PointerPressedEventArgs, IDataTransfer, DragDropEffects)`  | static   | drag start                       |
|  [19]   | `DragDrop.DragEnterEvent / DragOverEvent / DragLeaveEvent / DropEvent`               | static   | drop routed events               |

- `IClipboard.SetDataAsync`: Avalonia takes ownership of the passed `IAsyncDataTransfer` and disposes it once the transfer leaves the clipboard.
- `DragDrop.DoDragDropAsync`: returns the accepted `DragDropEffects`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every product UI concept enters as a typed retained surface — an `AvaloniaObject` property, a `StyledElement` tree node, a `Style` or `ResourceDictionary` entry — and its state flows through the property system, observed as a stream, never polled through a manual change-handler chain.
- Every cross-thread UI mutation crosses the `Dispatcher.UIThread` marshal; `CheckAccess`/`VerifyAccess` guard the affinity.
- Every appearance rule reaches its target through a `Selector` on a `Style` or `ControlTheme`, keyed on type, class, pseudo-class, name, or property value; a control writes only its own pseudo-classes and the selector decides which rule fires.
- Every animated appearance change rides a `Transitions` entry on the styled slot or an `Animation` over keyframes; a transformed slot binds `TransformOperations` so the interpolation stays operation-wise.

[STACKING]:
- `api-reactive.md`: `AvaloniaObjectExtensions.GetObservable(property)` and `GetPropertyChangedObservable` emit `IObservable<T>` for `System.Reactive` operators and ReactiveUI `WhenAnyValue`; a control-state reaction is `GetObservable(prop).Throttle(...).DistinctUntilChanged().Subscribe(...)` under a `CompositeDisposable`, and `AvaloniaObject.Bind(property, observable)` pushes a stream back into a property.
- `api-reactive.md`: `Dispatcher.UIThread` (imperative marshal) and `SynchronizationContextScheduler` (stream marshal) share one render-thread boundary; a live-data bind composes `ObserveOn(SynchronizationContextScheduler)` once at the bind edge, an imperative cross-thread write uses `Dispatcher.UIThread.Post`/`InvokeAsync`, and a TPL continuation pins to the render thread through `Dispatcher.UIThread.ToTaskScheduler()` handed to `TaskFactory.StartNew`/`Task.ContinueWith`.
- `Shell/input` `HOTKEY_DERIVATION`: hotkeys derive from the command table onto Avalonia primitives — a value-equal `KeyGesture(Key, KeyModifiers)` with `Parse`/`Matches`, `KeyBinding` rows carrying `Gesture`/`Command` through `InputElement.KeyBindings`; `RawInputModifiers` carries mouse buttons for the headless input harness.
- `Shell/input` `DRAG_CLIPBOARD`: a drop target binds through `DragDrop.SetAllowDrop(control, true)` with routed `DragOverEvent`/`DropEvent` handlers reading `DragEventArgs.DataTransfer` and writing `DragEventArgs.DragEffects`; drags start through `DragDrop.DoDragDropAsync(pointerArgs, dataTransfer, allowedEffects)`.
- `Shell/input` `DRAG_CLIPBOARD`: structured copy crosses one `IClipboard.SetDataAsync(IAsyncDataTransfer)` carrying a `DataTransfer` of one `DataTransferItem` per format, keyed by `DataFormat.CreateBytesApplicationFormat`/`CreateStringApplicationFormat` and built by `DataTransferItem.Create<T>`/`CreateText`; reads ride `TryGetDataAsync` gated by `ClipboardExtensions.GetDataFormatsAsync`, then `TryGetTextAsync`/`TryGetValueAsync<T>`/`TryGetRaw`.
- `api-avalonia-fluent.md`: `FluentTheme` enters `Application.Styles` as one `IStyle`, so a product override is a later `Styles` entry whose `ControlTheme.BasedOn` chains onto the theme's per-control theme and whose `ResourceDictionary.ThemeDictionaries` replaces a `System*Color` key `ColorPaletteResources` seeds.
- within-lib: settled-vocabulary value types (`Thinktecture` `[SmartEnum]`/`[ValueObject]`, `NodaTime` instants, `UnitsNet` quantities) bind into properties through compiled `{Binding}` or `Bind(property, observable)`; AppUi owners never re-model them as Avalonia types.
- `Theme/tokens.md` + `api-avalonia-fluent.md`: `ThemeVariant` is the sealed-record key requested by `Application.RequestedThemeVariant`, resolved by `StyledElement.ActualThemeVariant`/`ActualThemeVariantChanged`, overridden per subtree by `ThemeVariantScope`, and indexed by `FluentTheme.Palettes`; `Theme/tokens.md` owns the `[SmartEnum<string>]` `ThemeVariantRow` whose `Variant` member carries one `ThemeVariant` per row, the high-contrast row `new ThemeVariant("high-contrast", ThemeVariant.Dark)` falling through the dark inheritance chain.
- `Theme/tokens.md`: `IPlatformSettings.GetColorValues()` crosses the OS probe to `PlatformColorValues.ThemeVariant` and casts through the explicit `ThemeVariant` operator; `Mount`/`ApplyTo` write `application.RequestedThemeVariant = row.Variant` and key `FluentTheme.Palettes[ThemeVariant.Light]`/`[ThemeVariant.Dark]` from the same resolution.

[LOCAL_ADMISSION]:
- Product UI concepts enter through typed retained surfaces; `TopLevel.GetTopLevel(control)` resolves the per-surface `Clipboard`/`FocusManager`/`StorageProvider`, and generated and handwritten markup share one namescope through `AvaloniaXamlLoader.Load`/`Parse`.
