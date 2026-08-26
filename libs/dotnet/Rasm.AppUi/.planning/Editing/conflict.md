# [APPUI_CONFLICT_EDITING]

Three-way conflict resolution as one projection: `ThreeWay` aligns base/local/remote by exact LCS over the divergent middle and hunks by REGION, `HunkVerdict` decides stable/auto-merged/conflicted once, `ConflictPane` renders and `PreviewMerge` completes only when every conflicted hunk carries a choice, and the in-editor chrome (`HunkBands`/`HunkMargin` under a `HunkPosture` row) paints and raises over one live segment collection. The resolution verbs are `ConflictIntent` rows the frozen command deck generates from.

## [01]-[INDEX]

- [02]-[CONFLICT_MODEL]: The differ policy, the side and grain axes, the intent roster, the region hunk with its one verdict.
- [03]-[THREE_WAY]: The LCS alignment, the region hunking fold, and the admitted line ceiling.
- [04]-[HUNK_CHROME]: In-editor bands and the verb gutter over one segment collection; the posture rows; the published mount.

## [02]-[CONFLICT_MODEL]

- Owner: `DiffPolicy` the differ's admission row; `ConflictSide` the resolution-side axis; `ConflictGrain` the verb-grain axis carrying its admitted payload domain; `ConflictIntent` the non-generic resolution-verb vocabulary the frozen deck reads; `HunkVerdict` the one region verdict; `ThreeWayHunk` the region hunk; `ConflictPreview`, `GeometryDiff`, `ConflictPane` the projection.
- Cases: kind keys local-win, remote-win, merged, rejected arrive as projection values from the Persistence conflict union; `ConflictSide` = local | remote | both | base; `ConflictGrain` = target | hunk, each row carrying the payload domain its verbs admit; `HunkVerdict` = Stable | AutoMerged(side) | Conflicted — decided ONCE in `ThreeWay.Region`, so the merged projection, the gutter admission, and the preview gate read one verdict and no consumer re-derives it from run equality.
- Entry: `ConflictPane.Project(string kind, string target, string local, string remote, string baseText, string stamp, Option<GeometryDiff> geometry, DiffPolicy policy)` — `Fin<ConflictPane>` gated on the differ's admitted line ceiling; `PreviewMerge(HashMap<int, ConflictSide> choices)` returns the merged text and the ordered resolution evidence only after every conflicted hunk has a choice; `ConflictIntent.ForHunk(ConflictSide)` the gutter's side-to-key read.
- Law: a hunk side is a LINE SEQUENCE, never a joined string — the empty run a deletion leaves and the one-blank-line run an edit leaves are the same string and different merges; `Both` concatenates line-wise with no separator to invent, and the preview flattens taken runs so an accepted deletion contributes no line.
- Result: chosen verbs return their command outcome through the deck; modal presentation reuses the Form dialog intent with one conflict content-template row.
- Packages: Avalonia.AvaloniaEdit, CommunityToolkit.HighPerformance, LanguageExt.Core
- Growth: one resolution intent is one `ConflictIntent` row — key, grain, side, chord — whose deck row generates at `Shell/commands#INTENT_TABLE` `DeckRows.Conflict` with no edit there; one gutter reading is one `HunkPosture` row; one payload domain is one `ConflictGrain` column value.
- Boundary: the differ stays PAGE-OWNED against the admitted package set — `DiffPlex` (proof cluster) is two-way only, `LoroCs` merges CRDT history between two `Frontiers` of one document, and `JsonPatch` applies RFC 6902 patches; none answers three unrelated texts, so admission would replace the alignment alone and leave the region law, the verdict, and the ceiling exactly where they are while adding a package, a pin, a catalog, and a boundary. `GeometryDiff` projects the geometry-diff viewport — added/removed/modified ids beside the two `Viewpoint` cameras — SPIKE-gated on the viewport GPU surface over the 2D-fallback projection; the side-by-side text body renders `Local`, `Remote`, and `Base` through three read-only `Editing/codepane.md` viewers. The verbs REACH the pane through the one frozen registry: the eight keys live on `ConflictIntent`, `Project` seeds `ResolutionIntents` from the roster, and a chord, a gutter press, and a replayed journal entry raise ONE intent over the same `Invoke` route. The gutter's `take` arrow is the surface-owned lifting arrow: it lowers `(index, side)` through `ConflictIntent.ForHunk` onto the addressed payload and runs the frozen row; the Navigating posture's `Base` press stays the read-only seat's navigation arrow and reaches no resolution channel.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class ConflictSide {
    public static readonly ConflictSide Local = new("local");
    public static readonly ConflictSide Remote = new("remote");
    public static readonly ConflictSide Both = new("both");
    public static readonly ConflictSide Base = new("base");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConflictGrain {
    public static readonly ConflictGrain Target = new("target", ["none", "single"]);
    public static readonly ConflictGrain Hunk = new("hunk", ["single", "fields"]);

    public string[] Accepts { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConflictIntent {
    public static readonly ConflictIntent AcceptLocal = new("conflict.accept-local", ConflictGrain.Target,
        Some(ConflictSide.Local), Some(new KeyGesture(Key.L, KeyModifiers.Control | KeyModifiers.Alt)));
    public static readonly ConflictIntent AcceptRemote = new("conflict.accept-remote", ConflictGrain.Target,
        Some(ConflictSide.Remote), Some(new KeyGesture(Key.R, KeyModifiers.Control | KeyModifiers.Alt)));
    public static readonly ConflictIntent Merge = new("conflict.merge", ConflictGrain.Target,
        None, Some(new KeyGesture(Key.M, KeyModifiers.Control | KeyModifiers.Alt)));
    public static readonly ConflictIntent Discard = new("conflict.discard", ConflictGrain.Target, None, None);
    public static readonly ConflictIntent HunkLocal = new("conflict.hunk-local", ConflictGrain.Hunk, Some(ConflictSide.Local), None);
    public static readonly ConflictIntent HunkRemote = new("conflict.hunk-remote", ConflictGrain.Hunk, Some(ConflictSide.Remote), None);
    public static readonly ConflictIntent HunkBoth = new("conflict.hunk-both", ConflictGrain.Hunk, Some(ConflictSide.Both), None);
    public static readonly ConflictIntent Preview = new("conflict.preview-resolve", ConflictGrain.Target,
        None, Some(new KeyGesture(Key.P, KeyModifiers.Control | KeyModifiers.Alt)));

    public ConflictGrain Grain { get; }

    public Option<ConflictSide> Side { get; }

    public Option<KeyGesture> Chord { get; }

    public string[] Accepts => Grain.Accepts;

    public static Option<ConflictIntent> ForHunk(ConflictSide side) =>
        toSeq(Items).Find(row => row.Grain == ConflictGrain.Hunk && row.Side.Exists(held => held == side));

    public static Seq<string> Keys => toSeq(Items).Map(static row => row.Key);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record DiffPolicy(Dimension LineCeiling) {
    public static readonly DiffPolicy Default = new(Dimension.Create(value: 2_000));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HunkVerdict {
    private HunkVerdict() { }
    public sealed record Stable : HunkVerdict;
    public sealed record AutoMerged(ConflictSide Take) : HunkVerdict;
    public sealed record Conflicted : HunkVerdict;
}

public readonly record struct ThreeWayHunk(Seq<string> Base, Seq<string> Local, Seq<string> Remote, HunkVerdict Verdict) {
    public Seq<string> Side(ConflictSide side) => side.Switch(
        local: _ => Local,
        remote: _ => Remote,
        both: _ => Local + Remote,
        @base: _ => Base);

    public Seq<string> Merged => Verdict.Switch(
        stable: _ => Base,
        autoMerged: taken => Side(taken.Take),
        conflicted: _ => Base);
}

public sealed record ConflictPreview(string Text, Seq<(int Hunk, ConflictSide Side)> Resolutions);

public readonly record struct GeometryDiff(
    Seq<string> AddedIds,
    Seq<string> RemovedIds,
    Seq<string> ModifiedIds,
    Option<Viewpoint> LocalView,
    Option<Viewpoint> RemoteView);

public sealed record ConflictPane(
    string Kind,
    string Target,
    string Local,
    string Remote,
    string Base,
    string Stamp,
    Seq<ThreeWayHunk> Hunks,
    Option<GeometryDiff> Geometry,
    Seq<string> ResolutionIntents) {
    public static Fin<ConflictPane> Project(
        string kind, string target, string local, string remote, string baseText, string stamp,
        Option<GeometryDiff> geometry, DiffPolicy policy) =>
        ThreeWay.Diff(target, baseText, local, remote, policy)
            .Map(hunks => new ConflictPane(
                kind, target, local, remote, baseText, stamp, hunks, geometry, ConflictIntent.Keys));

    public Fin<ConflictPreview> PreviewMerge(HashMap<int, ConflictSide> choices) {
        Seq<int> unresolved = Hunks.Map(static (hunk, index) => (hunk, index))
            .Filter(row => row.hunk.Verdict is HunkVerdict.Conflicted && choices.Find(row.index).IsNone)
            .Map(static row => row.index);
        return unresolved.IsEmpty
            ? Fin.Succ(new ConflictPreview(
                string.Join('\n', Hunks.Map((hunk, index) => hunk.Verdict is HunkVerdict.Conflicted ? hunk.Side(choices[index]) : hunk.Merged).Flatten()),
                Hunks.Map(static (hunk, index) => (hunk, index))
                    .Filter(static row => row.hunk.Verdict is HunkVerdict.Conflicted)
                    .Map(row => (row.index, choices[row.index]))))
            : Fin.Fail<ConflictPreview>(new EditFault.ResolutionAbsent(unresolved));
    }
}
```

## [03]-[THREE_WAY]

- Owner: `ThreeWay` — the base-local-remote differ: per-side LCS-anchored alignment, then REGION hunking over the anchor structure.
- Law: a hunk is a REGION — consecutive divergent anchors accumulate and close at the next stable anchor, so a multi-line edit is one hunk and one choice; conflict is decided over the WHOLE accumulated run, so two sides that made the same multi-line edit agree as one region. The positional zip and the per-anchor emission are the two deleted forms — the second asked the resolver for twenty choices where the region law asks one.
- Law: the alignment is an exact LCS table over the divergent middle after the shared prefix and suffix strip, admitted against `DiffPolicy.LineCeiling` before either table allocates — the quadratic cost tracks the DIVERGENCE, not the document.
- Packages: CommunityToolkit.HighPerformance, LanguageExt.Core, BCL inbox
- Growth: zero new surface — the differ is closed; a new consumer reading is a `HunkPosture` row at `[04]`.
- Boundary: the LCS fill and backtrack ride a pooled buffer through `Span2D<int>` under the `EXPRESSION_SPINE` exemption stated on the fence — the table is the one measured statement boundary on the page; every projection around it (strip, anchor walk, region fold) is a fold.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ThreeWay {
    public static Fin<Seq<ThreeWayHunk>> Diff(string target, string baseText, string local, string remote, DiffPolicy policy) {
        Seq<string> baseLines = Lines(baseText);
        Seq<string> localLines = Lines(local);
        Seq<string> remoteLines = Lines(remote);
        int widest = int.Max(baseLines.Count, int.Max(localLines.Count, remoteLines.Count));
        return widest <= policy.LineCeiling.Value
            ? Fin.Succ(Hunks(baseLines, Align(baseLines, localLines), Align(baseLines, remoteLines)))
            : Fin.Fail<Seq<ThreeWayHunk>>(new EditFault.Invariant(
                target, $"{widest} lines exceeds the {policy.LineCeiling.Value}-line alignment ceiling"));
    }

    static Seq<string> Lines(string text) => toSeq(text.Split('\n'));

    static Seq<(Option<string> Base, Option<string> Side)> Align(Seq<string> baseLines, Seq<string> side) {
        int head = baseLines.Zip(side).TakeWhile(static pair => pair.First == pair.Second).Count();
        int floor = int.Min(baseLines.Count, side.Count) - head;
        int tail = toSeq(Enumerable.Range(1, int.Max(0, floor)))
            .TakeWhile(back => baseLines[baseLines.Count - back] == side[side.Count - back])
            .Count();
        return Matched(baseLines.Take(head))
            + Table(baseLines.Skip(head).Take(baseLines.Count - head - tail).Strict(),
                    side.Skip(head).Take(side.Count - head - tail).Strict())
            + Matched(baseLines.Skip(baseLines.Count - tail));
    }

    static Seq<(Option<string> Base, Option<string> Side)> Matched(Seq<string> lines) =>
        lines.Map(static line => (Some(line), Some(line)));

    static Seq<(Option<string> Base, Option<string> Side)> Table(Seq<string> baseLines, Seq<string> side) {
        int rows = baseLines.Count + 1;
        int cols = side.Count + 1;
        int[] rented = ArrayPool<int>.Shared.Rent(rows * cols);
        try {
            rented.AsSpan(0, rows * cols).Clear();
            Span2D<int> lcs = new(rented, 0, rows, cols, 0);
            for (int i = baseLines.Count - 1; i >= 0; i--) {
                for (int j = side.Count - 1; j >= 0; j--) {
                    lcs[i, j] = baseLines[i] == side[j] ? lcs[i + 1, j + 1] + 1 : int.Max(lcs[i + 1, j], lcs[i, j + 1]);
                }
            }
            List<(Option<string>, Option<string>)> aligned = new(capacity: rows + cols);
            (int bi, int si) = (0, 0);
            while (bi < baseLines.Count && si < side.Count) {
                if (baseLines[bi] == side[si]) { aligned.Add((Some(baseLines[bi]), Some(side[si]))); bi++; si++; }
                else if (lcs[bi + 1, si] >= lcs[bi, si + 1]) { aligned.Add((Some(baseLines[bi]), Option<string>.None)); bi++; }
                else { aligned.Add((Option<string>.None, Some(side[si]))); si++; }
            }
            while (bi < baseLines.Count) { aligned.Add((Some(baseLines[bi]), Option<string>.None)); bi++; }
            while (si < side.Count) { aligned.Add((Option<string>.None, Some(side[si]))); si++; }
            return toSeq(aligned);
        }
        finally { ArrayPool<int>.Shared.Return(rented); }
    }

    static Seq<ThreeWayHunk> Hunks(
        Seq<string> baseLines,
        Seq<(Option<string> Base, Option<string> Side)> local,
        Seq<(Option<string> Base, Option<string> Side)> remote) {
        Map<int, Seq<string>> localByAnchor = ByAnchor(local);
        Map<int, Seq<string>> remoteByAnchor = ByAnchor(remote);
        var walked = toSeq(Enumerable.Range(0, baseLines.Count + 1)).Fold(
            (Pending: (Base: Seq<string>(), Local: Seq<string>(), Remote: Seq<string>()), Open: false, Closed: Seq<ThreeWayHunk>()),
            (held, anchor) => {
                Seq<string> baseRun = anchor < baseLines.Count ? Seq(baseLines[anchor]) : Seq<string>();
                Seq<string> localRun = localByAnchor.Find(anchor).IfNone(baseRun);
                Seq<string> remoteRun = remoteByAnchor.Find(anchor).IfNone(baseRun);
                return localRun == baseRun && remoteRun == baseRun
                    ? ((Seq<string>(), Seq<string>(), Seq<string>()), false,
                       held.Open ? held.Closed.Add(Region(held.Pending)) : held.Closed)
                    : ((held.Pending.Base + baseRun, held.Pending.Local + localRun, held.Pending.Remote + remoteRun), true, held.Closed);
            });
        return walked.Open ? walked.Closed.Add(Region(walked.Pending)) : walked.Closed;
    }

    static ThreeWayHunk Region((Seq<string> Base, Seq<string> Local, Seq<string> Remote) run) =>
        new(run.Base, run.Local, run.Remote,
            run.Local == run.Base && run.Remote == run.Base ? new HunkVerdict.Stable()
            : run.Local == run.Base ? new HunkVerdict.AutoMerged(ConflictSide.Remote)
            : run.Remote == run.Base || run.Local == run.Remote ? new HunkVerdict.AutoMerged(ConflictSide.Local)
            : new HunkVerdict.Conflicted());

    static Map<int, Seq<string>> ByAnchor(Seq<(Option<string> Base, Option<string> Side)> aligned) {
        var folded = aligned.Fold(
            (Runs: Map<int, Seq<string>>(), Anchor: 0, Pending: Seq<string>()),
            static (held, pair) => pair.Base.IsSome
                ? (held.Runs.AddOrUpdate(held.Anchor, held.Pending + pair.Side.ToSeq()), held.Anchor + 1, Seq<string>())
                : (held.Runs, held.Anchor, held.Pending + pair.Side.ToSeq()));
        return folded.Pending.IsEmpty ? folded.Runs : folded.Runs.AddOrUpdate(folded.Anchor, folded.Pending);
    }
}
```

## [04]-[HUNK_CHROME]

- Owner: `HunkSegment` the live band segment carrying its verdict; `HunkPosture` the gutter's reading rows; `HunkMount` the published mount; `HunkBands` the background renderer; `HunkMargin` the verb gutter.
- Cases: `HunkPosture` = resolving | navigating — a merge offers three sides over the hunks that carry a genuine choice; a read-only reading offers one navigation affordance over EVERY hunk, because a compare runs the resolver degenerate (baseline on both base and local legs) and reports no conflict at all, so a conflicted-only filter would blank its gutter whole. The read-only seat's own half of this contract is settled at folder `RULINGS` `[02]-[SHAPE]`.
- Entry: `HunkBands.Attach(TextEditor editor, Seq<ThreeWayHunk> hunks, Func<int, (int First, int Last)> span, HunkPosture posture, Action<int, ConflictSide> take)` — `HunkMount` mounting the band renderer and the gutter margin over one live segment collection and publishing that collection as the overview change-lane arrow (`Editing/codepane.md` `LaneSource`) a strip consumer binds.
- Law: a hunk band is a live SEGMENT — the `TextSegmentCollection` constructs against the document and `UpdateOffsets` moves every held span through each edit, so a band still frames its region after an earlier resolution; a hand-tracked offset table drifts on the first accepted one.
- Law: the gutter is an `AbstractMargin` in `TextArea.LeftMargins`, which scrolls and wraps with the text; both surfaces resolve geometry through `BackgroundGeometryBuilder.GetRectsForSegment`, which already folds the scroll offset, so a band and its gutter row share one Y by construction.
- Packages: Avalonia.AvaloniaEdit, LanguageExt.Core
- Growth: one gutter reading is one `HunkPosture` row carrying its verb roster, width, and row admission.
- Boundary: bands paint on `KnownLayer.Background` as an `IBackgroundRenderer` added to `TextView.BackgroundRenderers` — `InsertLayer` refuses at runtime for anything but `Above` against `Background`, and an `Above` layer paints over the text; the ink seats write from a constructor body because a field initializer cannot reference the instance being built; the margin's inks resolve only once it is IN the tree, because a resource observable off a detached element resolves against nothing. The mount is a `HunkMount` VALUE rather than a bare lifetime, so the segment collection it measured crosses to the pane's overview strip and no consumer re-derives line spans onto a second offset set — `Document/media#DIFF_SEAT` hands the returned `Lane` straight to `CodePane.Open`.

```csharp
// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed class HunkSegment : TextSegment {
    public required int Index { get; init; }

    public required HunkVerdict Verdict { get; init; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HunkPosture {
    public static readonly HunkPosture Resolving = new("resolving",
        Seq((ConflictSide.Local, PaintRole.Info),
            (ConflictSide.Both, PaintRole.Accent),
            (ConflictSide.Remote, PaintRole.Warning)),
        static segment => segment.Verdict is HunkVerdict.Conflicted);
    public static readonly HunkPosture Navigating = new("navigating",
        Seq((ConflictSide.Base, PaintRole.Selection)),
        static _ => true);

    public const double SlotWidth = 12d;

    public Seq<(ConflictSide Side, PaintRole Role)> Verbs { get; }

    public double Width => Verbs.Count * SlotWidth;

    [UseDelegateFromConstructor]
    public partial bool Admits(HunkSegment segment);
}

public sealed record HunkMount(IDisposable Lifetime, LaneSource Lane) : IDisposable {
    public void Dispose() => Lifetime.Dispose();
}

public sealed class HunkBands : IBackgroundRenderer, IDisposable {
    readonly TextSegmentCollection<HunkSegment> segments;

    readonly IDisposable inks;

    IBrush? conflicted;

    IBrush? merged;

    public HunkBands(TextView view, TextSegmentCollection<HunkSegment> segments) =>
        (this.segments, inks) = (segments, new CompositeDisposable(
            Track(view, PaintRole.Error.At(3), brush => conflicted = brush),
            Track(view, PaintRole.Info.At(3), brush => merged = brush)));

    public KnownLayer Layer => KnownLayer.Background;

    static IDisposable Track(TextView target, TokenKey key, Action<IBrush?> seat) =>
        target.GetResourceObservable(key.Value).Subscribe(value => {
            seat(value as IBrush);
            target.InvalidateLayer(KnownLayer.Background);
        });

    public void Draw(TextView textView, DrawingContext drawingContext) =>
        toSeq(segments).Iter(segment => {
            BackgroundGeometryBuilder builder = new() { AlignToWholePixels = true, ExtendToFullWidthAtLineEnd = true };
            builder.AddSegment(textView, segment);
            Optional(builder.CreateGeometry()).Iter(geometry =>
                drawingContext.DrawGeometry(segment.Verdict is HunkVerdict.Conflicted ? conflicted : merged, null, geometry));
        });

    public void Dispose() => inks.Dispose();

    public static HunkMount Attach(
        TextEditor editor, Seq<ThreeWayHunk> hunks, Func<int, (int First, int Last)> span,
        HunkPosture posture, Action<int, ConflictSide> take) {
        TextSegmentCollection<HunkSegment> segments = new(editor.Document);
        hunks.Map(static (hunk, index) => (hunk, index)).Iter(row => Spanned(editor.Document, span(row.index))
            .Iter(bounds => segments.Add(new HunkSegment {
                Index = row.index, Verdict = row.hunk.Verdict, StartOffset = bounds.Start, Length = bounds.Length,
            })));
        HunkBands bands = new(editor.TextArea.TextView, segments);
        HunkMargin margin = new(segments, posture, take);
        editor.TextArea.TextView.BackgroundRenderers.Add(bands);
        editor.TextArea.LeftMargins.Insert(0, margin);
        return new HunkMount(
            Disposable.Create(() => {
                ignore(editor.TextArea.TextView.BackgroundRenderers.Remove(bands));
                ignore(editor.TextArea.LeftMargins.Remove(margin));
                segments.Disconnect(editor.Document);
                bands.Dispose();
                margin.Dispose();
            }),
            lane => lane == OverviewLane.Change
                ? toSeq(segments).Map(static segment => (TextSegment)segment)
                : Seq<TextSegment>());
    }

    static Option<(int Start, int Length)> Spanned(TextDocument document, (int First, int Last) span) =>
        span is { First: >= 1 } && span.Last >= span.First && span.Last <= document.LineCount
            ? Some((document.GetLineByNumber(span.First).Offset,
                    document.GetLineByNumber(span.Last).EndOffset - document.GetLineByNumber(span.First).Offset))
            : Option<(int Start, int Length)>.None;
}

public sealed class HunkMargin(TextSegmentCollection<HunkSegment> segments, HunkPosture posture, Action<int, ConflictSide> take)
    : AbstractMargin, IDisposable {
    const double Inset = 2d;

    readonly Atom<HashMap<string, IBrush>> inks = Atom(HashMap<string, IBrush>());

    readonly CompositeDisposable subscriptions = [];

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        posture.Verbs.Iter(verb => subscriptions.Add(this.GetResourceObservable(verb.Role.At(0).Value).Subscribe(value => {
            ignore(inks.Swap(map => value is IBrush brush ? map.AddOrUpdate(verb.Side.Key, brush) : map.Remove(verb.Side.Key)));
            InvalidateVisual();
        })));
    }

    protected override Size MeasureOverride(Size availableSize) => new(posture.Width, 0d);

    public override void Render(DrawingContext context) =>
        Rows().Iter(row => Slots().Iter(slot => inks.Value.Find(slot.Side.Key).Iter(brush =>
            context.DrawRectangle(brush, null,
                new Rect(slot.Left + Inset, row.Rect.Top + Inset, slot.Span - (Inset * 2d), row.Rect.Height - (Inset * 2d))))));

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        Point at = e.GetPosition(this);
        Rows()
            .Find(row => at.Y >= row.Rect.Top && at.Y < row.Rect.Bottom)
            .Iter(row => Slots()
                .Find(slot => at.X >= slot.Left && at.X < slot.Left + slot.Span)
                .Iter(slot => take(row.Index, slot.Side)));
    }

    Seq<(ConflictSide Side, double Left, double Span)> Slots() =>
        posture.Verbs.Map(static (verb, index) => (verb.Side, index * HunkPosture.SlotWidth, HunkPosture.SlotWidth));

    Seq<(int Index, Rect Rect)> Rows() =>
        Optional(TextView).Map(view => toSeq(segments)
            .Filter(posture.Admits)
            .Choose(segment => toSeq(BackgroundGeometryBuilder.GetRectsForSegment(view, segment))
                .Head
                .Map(rect => (segment.Index, rect))))
            .IfNone(Seq<(int, Rect)>());

    public void Dispose() => subscriptions.Dispose();
}
```
