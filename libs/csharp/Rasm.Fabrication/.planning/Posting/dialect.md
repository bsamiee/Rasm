# [RASM_FABRICATION_DIALECT]

`Dialect` owns one admitted `CutProgram`-to-byte projection: grammar-family lowering, motion-directive lowering, command admission, modal rendering, physical-record census, block framing, sequence framing, and content-key minting resolve through one correspondence. `PostDialect`, `GNode`, and `GWord` remain the frozen input vocabulary; `PostImage` is the sole egress value, and `Nc1Canonical` projects `SteelPart` without a mirror model.

`EmitPolicy` parameterizes text encoding, line termination, final termination, record framing, and block-limit enforcement. `RecordFrame` carries plain and sequence-numbered egress as cases, `SequenceCounter` admits the numbering law once, and `ChecksumRule` carries the digest, separator, and width as row data. `BlockLimit.Observe` exposes an over-cap measurement to optimization while `BlockLimit.Enforce` gates final egress.

Controller syntax is posting-owned: `MacroGrammar` is the control-language discriminant every vendor-specific record generator dispatches on, so a dialect identity test never appears in emission.

## [01]-[INDEX]

- [01]-[EMISSION]: `Dialect.Emit` lowers, renders, frames, seals, and keys one program.
- [02]-[PROJECTION]: `GWord.Render` derives emitted records, modal state, nested bodies, faults, and record count together.
- [03]-[FRAMING]: `BlockFrame` structure, `SequenceCounter` numbering, and `ChecksumRule` digests survive to the byte stream.
- [04]-[COORDINATES]: `GNode.CoordinateFrame` lowers the assigned `WcsSlot` into an offset write and its selection word.
- [05]-[NC1]: `Nc1Canonical` projects the admitted steel owner into canonical DSTV records.

## [02]-[EMISSION]

- Owner: `Dialect` owns the byte projection, while `PostImage` carries the exact records, bytes, kind, key, and physical-record count.
- Cases: `PostFamily` selects word-address, conversational, additive, or forming grammar; `MacroGrammar` selects the control language rendering dialect cycles and conversational motion; `RecordFrame` selects plain or numbered egress; `BlockLimit` selects measurement or final enforcement.
- Entry: `Dialect.Emit` is the one public operation and consumes a complete `CutProgram` with `EmitPolicy`.
- Auto: `GCommand.Admits` discharges the command row's own declared `Requires` and `Modalities` against the dialect, so emission gates only what the parameters decide — rotary addresses, compensation kind, revolution dwell, and arc representation.
- Receipt: `PostImage.Records` is the same population counted by `PhysicalRecords`, encoded into `Bytes`, and passed to `ContentKey.Of`; an empty population fails rather than keying a null artifact.
- Packages: `LanguageExt.Core` supplies `Fin<T>`, `Traverse`, `Bind`, `Fold`, `Map`, `Choose`, `Range`, and generated `Switch`; `Thinktecture.Runtime.Extensions` generates `RecordFrame`, `BlockLimit`, `ChecksumRule`, `SequenceCounter`, and `EmitPolicy`; `GCommand.Admit`, `GCommand.Admits`, and `GWord.Render` compose `Posting/program`; `Encoding.GetBytes` and `ContentKey.Of` seal egress; `DSTV.Net` constrains `SteelPart` vocabulary parity.
- Growth: one grammar family adds one `PostFamily` arm; one control language adds one `MacroGrammar` row and its two rendering arms; one checksum convention adds one `ChecksumRule` row; one steel feature adds one exhaustive `SteelFeature` arm.
- Boundary: `Dialect` never reparses, reconditions motion, invents absent command parameters, or maintains a second block-count projection.

## [03]-[PROJECTION]

`GWord.Render` is the render correspondence composed from `Posting/program`. `Dialect.Emit` frames and counts only its returned `RenderReceipt.Lines`, so macro assignments, dialect-cycle parameters, subprogram definitions, additive records, and NC1 records cannot escape `BlockCap`. Subprogram definitions hoist into one label-keyed stream; identical definitions share one row, and conflicting bodies fail before rendering.

## [04]-[FRAMING]

`BlockFrame` evidence survives lowering: `Delimiter` emits the tape mark, `Program` emits the program-number record, `Comments` emit verbatim, and an `Optional` block renders through `GWord.Render` and prefixes every produced record with the block-delete character. Parsed `Sequence` and `Checksum` values never survive, because `RecordFrame` owns numbering and digest on re-emission.

`SequenceCounter` admits first value, step, and wrap modulus once; conversational controls number records bare and word-address controls prefix `N`. `ChecksumRule` carries the digest fold, separator, and rendered width, so no framing case holds a caller-supplied function.

## [05]-[COORDINATES]

`GNode.CoordinateFrame` carries the setup's `WcsSlot` and mounting `Plane`, so emission writes the offset before selecting it; selection-only posting silently assumes the control already holds the frame. `WcsRoster` bounds every ordinal, `Local` lowers to the local-shift word against its parent, and `Rotary` carries its axis into the offset write.

## [06]-[NC1]

`Nc1Canonical` consumes `SteelHeader` and `SteelFeature` directly. Canonical records are both the file payload and the `EgressKind.Nc1` content-key input; the read-only `DSTV.Net` model constrains header and feature parity without becoming an emission dependency.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ChecksumRule {
    public static readonly ChecksumRule Xor = new("xor", "*", 0, static record => Fold(record, 0u, static (state, value) => state ^ value));
    public static readonly ChecksumRule Sum = new("sum", "*", 0, static record => Fold(record, 0u, static (state, value) => (state + value) & 0xFFu));
    public static readonly ChecksumRule Crc16Ccitt = new("crc16-ccitt", "*", 4, static record => Fold(record, 0xFFFFu, Ccitt));

    public string Separator { get; }
    public int Width { get; }

    [UseDelegateFromConstructor]
    public partial uint Digest(ReadOnlyMemory<byte> record);

    public string Render(string record, Encoding codec) =>
        $"{record}{Separator}{Digest(codec.GetBytes(record)).ToString(Width > 0 ? $"X{Width.ToString(CultureInfo.InvariantCulture)}" : "D", CultureInfo.InvariantCulture)}";

    private static uint Fold(ReadOnlyMemory<byte> record, uint seed, Func<uint, byte, uint> step) =>
        toSeq(record.ToArray()).Fold(seed, step);

    private static uint Ccitt(uint state, byte value) => Range(0, 8).Fold(
        (state ^ ((uint)value << 8)) & 0xFFFFu,
        static (current, _) => ((current & 0x8000u) != 0u ? (current << 1) ^ 0x1021u : current << 1) & 0xFFFFu);
}

[ComplexValueObject]
public sealed partial class SequenceCounter {
    public int First { get; }
    public int Step { get; }
    public int Modulus { get; }

    public long At(int index) => Modulus > 0
        ? (First + ((long)Step * index)) % Modulus
        : First + ((long)Step * index);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int first, ref int step, ref int modulus) =>
        validationError = first < 0 || step <= 0 || modulus < 0 || (modulus > 0 && first >= modulus)
            ? new ValidationError("dialect:sequence-counter") : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RecordFrame {
    private RecordFrame() { }

    public sealed record Plain : RecordFrame;
    public sealed record Numbered(SequenceCounter Counter, Option<ChecksumRule> Checksum) : RecordFrame;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockLimit {
    private BlockLimit() { }

    public sealed record Observe : BlockLimit;
    public sealed record Enforce : BlockLimit;
}

[ComplexValueObject]
public sealed partial class EmitPolicy {
    public Encoding Codec { get; }
    public string NewLine { get; }
    public bool FinalTerminator { get; }
    public RecordFrame Frame { get; }
    public BlockLimit Limit { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Encoding codec,
        ref string newLine,
        ref bool finalTerminator,
        ref RecordFrame frame,
        ref BlockLimit limit) =>
        validationError = codec is null || string.IsNullOrEmpty(newLine) || frame is null || limit is null
            ? new ValidationError("dialect:emit-policy") : null;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record PostImage(
    EgressKind Kind,
    Seq<string> Records,
    ReadOnlyMemory<byte> Bytes,
    ContentKey Key,
    int PhysicalRecords);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Dialect {
    public static Fin<PostImage> Emit(CutProgram program, EmitPolicy policy) {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(policy);

        return from kind in OutputKind(program)
               from lowered in Lower(program.Dialect, program.Nodes)
               from executable in GWord.Render(lowered.Executable)
               from unique in Distinct(lowered.Definitions)
               from definitions in GWord.Render(unique)
               from records in Frame(program.Dialect, kind, policy, executable.Lines.Concat(definitions.Lines))
               from _ in Cap(program.Dialect, policy.Limit, records)
               from image in Image(program.Dialect, kind, policy, records)
               select image;
    }

    private static Fin<EgressKind> OutputKind(CutProgram program) {
        Seq<GNode> leaves = program.Nodes.Bind(Leaves);
        bool anyNc1 = leaves.Exists(static node => node is GNode.Nc1);
        bool onlyNc1 = !leaves.IsEmpty && leaves.ForAll(static node => node is GNode.Nc1);
        return !anyNc1 || onlyNc1
            ? Fin.Succ(onlyNc1 ? EgressKind.Nc1 : EgressKind.CutProgram)
            : Fin.Fail<EgressKind>(Error.New($"dialect:mixed-grammar:{program.Dialect.Key}"));
    }

    private static Seq<GNode> Leaves(GNode node) => node.Switch(
        block: static block => block.Body.ToSeq().Bind(Leaves),
        word: static word => Seq<GNode>(word),
        cannedCycle: static cycle => Seq<GNode>(cycle),
        coordinateFrame: static frame => Seq<GNode>(frame),
        macro: static macro => macro.Body.ToSeq().Bind(Leaves),
        subprogram: static subprogram => subprogram.Body.ToSeq().Bind(Leaves),
        additiveLayer: static layer => Seq<GNode>(layer),
        nc1: static nc1 => Seq<GNode>(nc1),
        directive: static directive => Seq<GNode>(directive));

    // One complete definition owns each subprogram head; repeated calls share it, while conflicting bodies rail.
    private static Fin<Seq<GWord>> Distinct(Seq<GWord> definitions) => definitions
        .FoldM<Fin, (Seq<GWord> Words, Map<string, GWord.Subprogram> Definitions)>(
            (Seq<GWord>(), Map<string, GWord.Subprogram>()),
            static (state, word) => word is not GWord.Subprogram definition
                ? Fin.Succ((state.Words.Add(word), state.Definitions))
                : definition.Open.Head.ToFin(Error.New("dialect:subprogram-label")).Bind(label =>
                    state.Definitions.Find(label).Match(
                        Some: held => held == definition
                            ? Fin.Succ(state)
                            : Fin.Fail<(Seq<GWord>, Map<string, GWord.Subprogram>)>(
                                Error.New($"dialect:subprogram-conflict:{label}")),
                        None: () => Fin.Succ((state.Words.Add(word),
                            state.Definitions.AddOrUpdate(label, definition)))))).As()
        .Map(static state => state.Words);

    private static Fin<PostImage> Image(PostDialect dialect, EgressKind kind, EmitPolicy policy, Seq<string> records) {
        if (records.IsEmpty)
            return Fin.Fail<PostImage>(Error.New($"dialect:empty-image:{dialect.Key}"));
        string text = string.Join(policy.NewLine, records.ToArray()) + (policy.FinalTerminator ? policy.NewLine : string.Empty);
        ReadOnlyMemory<byte> bytes = policy.Codec.GetBytes(text);
        return Fin.Succ(new PostImage(kind, records, bytes, ContentKey.Of(kind, bytes.Span), records.Count));
    }

    private static Fin<Seq<string>> Cap(PostDialect dialect, BlockLimit limit, Seq<string> records) => limit.Switch(
        state: (Dialect: dialect, Records: records),
        observe: static state => Fin.Succ(state.Records),
        enforce: static state => state.Dialect.BlockCap.Match(
            Some: cap => state.Records.Count > cap
                ? Fin.Fail<Seq<string>>(new FabricationFault.BlockCapExceeded(state.Dialect, state.Records.Count, cap).ToError())
                : Fin.Succ(state.Records),
            None: () => Fin.Succ(state.Records)));

    private static Fin<Seq<string>> Frame(PostDialect dialect, EgressKind kind, EmitPolicy policy, Seq<string> records) =>
        AdmitFrame(dialect, kind, policy.Frame).Map(frame => frame.Switch(
            state: (Dialect: dialect, Policy: policy, Records: records),
            plain: static state => state.Records,
            numbered: static (state, numbered) => state.Records.Map(
                (line, index) => Numbered(state.Dialect, numbered, state.Policy.Codec, line, index))));

    private static Fin<RecordFrame> AdmitFrame(PostDialect dialect, EgressKind kind, RecordFrame frame) => frame.Switch(
        state: (Dialect: dialect, Kind: kind, Frame: frame),
        plain: static state => Fin.Succ(state.Frame),
        numbered: static (state, numbered) => state.Kind != EgressKind.Nc1
            && state.Dialect.Features.Contains(DialectFeature.LineNumbers)
            && numbered.Checksum.ForAll(_ => state.Dialect.Features.Contains(DialectFeature.Checksum))
                ? Fin.Succ(state.Frame)
                : Fin.Fail<RecordFrame>(Error.New($"dialect:frame:numbered:{state.Dialect.Key}")));

    private static string Numbered(PostDialect dialect, RecordFrame.Numbered frame, Encoding codec, string line, int index) {
        string numbered = $"{Sequence(dialect)}{frame.Counter.At(index).ToString(CultureInfo.InvariantCulture)} {line}";
        return frame.Checksum.Map(rule => rule.Render(numbered, codec)).IfNone(numbered);
    }

    private static string Sequence(PostDialect dialect) =>
        dialect.Family == PostFamily.Conversational ? string.Empty : "N";

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Lower(PostDialect dialect, GNode node) => node switch {
        GNode.Block block => Lower(dialect, block.Body.ToSeq()).Bind(body => Framed(block.Frame, body)),
        GNode.Nc1 nc1 => Fin.Succ(Executable(Nc1Canonical.Word(nc1.Receipt))),
        GNode.CoordinateFrame frame => WcsFrame(dialect, frame).Map(static words => (words, Seq<GWord>())),
        GNode.Word word when dialect.Family == PostFamily.WordAddress || dialect.Family == PostFamily.AdditiveGcode =>
            Address(dialect, word).Map(Executable),
        GNode.Word word when dialect.Family == PostFamily.Conversational => Verb(dialect, word).Map(Executable),
        GNode.CannedCycle cycle when dialect.Family == PostFamily.WordAddress || dialect.Family == PostFamily.Conversational =>
            Cycle(dialect, cycle).Map(Executable),
        GNode.CannedCycle cycle when dialect.Family == PostFamily.AdditiveGcode => ExpandedCycle(dialect, cycle).Map(Executable),
        GNode.Macro macro when dialect.Family != PostFamily.AdditiveGcode && dialect.Family != PostFamily.Forming => Macro(dialect, macro),
        GNode.Subprogram subprogram when dialect.Family != PostFamily.AdditiveGcode && dialect.Family != PostFamily.Forming =>
            Subprogram(dialect, subprogram),
        GNode.AdditiveLayer layer when dialect.Family == PostFamily.AdditiveGcode => Fin.Succ(Executable(AdditiveRecord(layer, dialect.Decimals))),
        GNode.Directive directive => Directive(dialect, directive.Value),
        _ => Unsupported(dialect, node).Map(Executable),
    };

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Directive(
        PostDialect dialect,
        MotionDirective directive) => directive.Switch(
        state: dialect,
        spindle: static (post, row) => Lower(post, new GNode.Word(
            row.Control == SpindleControl.ConstantSurface ? GCommand.Css : GCommand.Spindle,
            row.Control == SpindleControl.ConstantSurface
                ? Arr(GParam.Number('S', row.SurfaceMetersPerMinute, ProgramUnits.Metric), GParam.Number('D', row.ResolvedRpm, ProgramUnits.Metric))
                : Arr(GParam.Number('S', row.ResolvedRpm, ProgramUnits.Metric)),
            None)),
        dwell: static (post, row) => Lower(post, new GNode.Word(
            GCommand.Dwell,
            Arr(GParam.Number('U', row.Revolutions, ProgramUnits.Metric)),
            None)),
        orientedStop: static (post, row) => Lower(post, new GNode.Word(
            GCommand.SpindleOrient,
            Arr(
                GParam.Number('R', Math.Atan2(row.Retract.Y, row.Retract.X) * 180.0 / Math.PI, ProgramUnits.Metric),
                GParam.Number('P', row.Retract.Length, ProgramUnits.Metric)),
            None)),
        synchronize: static (post, row) => post.CodeOverride("motion-synchronize")
            .ToFin(Error.New("dialect:motion-synchronize"))
            .Map(code => Executable(new GWord.Address(code, ModalGroup.NonModal,
                Arr(
                    GParam.Number('P', row.FromMove, ProgramUnits.Metric),
                    GParam.Number('Q', row.ToMove, ProgramUnits.Metric),
                    GParam.Number('S', row.Rpm, ProgramUnits.Metric),
                    GParam.Number('F', row.Lead, ProgramUnits.Metric),
                    GParam.Number('H', row.Hand == RotationSense.Clockwise ? 1.0 : -1.0, ProgramUnits.Metric)),
                None,
                WordRetention.Explicit))),
        channelBarrier: static (post, row) => post.CodeOverride("channel-barrier")
            .ToFin(Error.New("dialect:channel-barrier"))
            .Map(code => Executable(new GWord.Text(Seq(
                $"{code} {row.Channel} WAIT[{string.Join(',', row.WaitFor.ToArray())}] SIGNAL[{row.Signal.IfNone(string.Empty)}]")))),
        specialized: static (post, row) => Specialized(post, row.Payload));

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Specialized(
        PostDialect dialect,
        SpecializedToolpathEnvelope payload) => payload.IsValid
            ? Fin.Succ(Executable(new GWord.Text(payload.Rows.Map(row =>
                SpecializedRecord("RASM", row, dialect.Decimals)).Map(record => Annotation(dialect, record)))))
            : Fin.Fail<(Seq<GWord>, Seq<GWord>)>(Error.New($"dialect:specialized:{payload.Kind.Key}:invalid"));

    private static string Annotation(PostDialect dialect, string record) =>
        dialect.Family == PostFamily.WordAddress ? $"({record})" : $";{record}";

    private static string SpecializedRecord(string code, SpecializedToolpathRow row, int decimals) => row.Switch(
        state: (Code: code, Decimals: decimals),
        wire: static (state, value) => $"{state.Code} WIRE P{value.Pass} S{Value(value.Station, state.Decimals)} "
            + $"L{Point(value.Lower, state.Decimals)} U{Point(value.Upper, state.Decimals)} A{value.Action} "
            + $"G{Value(value.LagMm, state.Decimals)} R{value.RotaryDeg.Map(angle => Value(angle, state.Decimals)).IfNone(string.Empty)}",
        bevel: static (state, value) => $"{state.Code} BEVEL P{value.Pass} N{value.Move} X{Point(value.Point, state.Decimals)} "
            + $"A{Vector(value.ToolAxis, state.Decimals)} V{Point(value.Pivot, state.Decimals)} "
            + $"B{Value(value.AngleDeg, state.Decimals)} C{Value(value.CrossTiltDeg, state.Decimals)} F{Value(value.FeedMmPerMin, state.Decimals)}",
        link: static (state, value) => $"{state.Code} LINK {value.From}>{value.To} K{value.Transition} "
            + $"D{Value(value.DistanceMm, state.Decimals)} T{Value(value.DurationSeconds, state.Decimals)} "
            + $"L{Value(value.LiftMm, state.Decimals)} R{Value(value.RotationPenalty, state.Decimals)}",
        inspection: static (state, value) => $"{state.Code} INSPECT P{value.Pass} B{value.FromBlock}:{value.ToBlockExclusive} "
            + $"A{Value(value.AngleDeviationDeg, state.Decimals)} O{Value(value.OffsetDeviationMm, state.Decimals)} C{(value.Conforming ? 1 : 0)}",
        turningThread: static (state, value) => $"{state.Code} TURN THREAD {value.Form} {value.Side} "
            + $"L{Value(value.LoadFlankDeg, state.Decimals)} C{Value(value.ClearanceFlankDeg, state.Decimals)}",
        turningAxial: static (state, value) => $"{state.Code} TURN AXIAL {value.Kind} N{value.FromMove}:{value.ToMove} "
            + $"D{Value(value.Diameter, state.Decimals)} Z{Value(value.Depth, state.Decimals)} A{Value(value.TipAngleDeg, state.Decimals)}",
        turningTap: static (state, value) => $"{state.Code} TURN TAP {value.Form} {value.Hand} N{value.FromMove}:{value.ToMove} "
            + $"D{Value(value.Diameter, state.Decimals)} Z{Value(value.Depth, state.Decimals)} P{Value(value.Pitch, state.Decimals)}",
        turningKnurl: static (state, value) => $"{state.Code} TURN KNURL {value.Pattern} N{value.FromMove}:{value.ToMove} "
            + $"P{Value(value.Pressure, state.Decimals)}",
        turningHandoff: static (state, value) => $"{state.Code} TURN {value.Kind} {value.From}>{value.To} "
            + $"G{Value(value.GripPlane, state.Decimals)} L{Value(value.GripLength, state.Decimals)} P{Value(value.PullDistance, state.Decimals)}");

    private static string Point(Point3d point, int decimals) =>
        $"{Value(point.X, decimals)},{Value(point.Y, decimals)},{Value(point.Z, decimals)}";

    private static string Vector(Vector3d vector, int decimals) =>
        $"{Value(vector.X, decimals)},{Value(vector.Y, decimals)},{Value(vector.Z, decimals)}";

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Lower(PostDialect dialect, Seq<GNode> body) =>
        body.Traverse(node => Lower(dialect, node)).As().Map(static rows => rows.Fold(
            (Executable: Seq<GWord>(), Definitions: Seq<GWord>()),
            static (state, row) => (state.Executable.Concat(row.Executable), state.Definitions.Concat(row.Definitions))));

    // Block-delete renders through the one correspondence, so a prefixed record is still exactly one counted record.
    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Framed(
        BlockFrame frame, (Seq<GWord> Executable, Seq<GWord> Definitions) body) =>
        (frame.Optional
            ? GWord.Render(body.Executable).Map(static rendered =>
                Seq<GWord>(GWord.Text(rendered.Lines.Map(static line => $"/{line}"))))
            : Fin.Succ(body.Executable))
        .Map(executable => (Seq<GWord>(GWord.Text(Structure(frame))).Concat(executable), body.Definitions));

    private static Seq<string> Structure(BlockFrame frame) => (frame.Delimiter ? Seq("%") : Seq<string>())
        .Concat(frame.Program.Map(static value => $"O{value.ToString(CultureInfo.InvariantCulture)}").ToSeq())
        .Concat(frame.Comments);

    private static (Seq<GWord> Executable, Seq<GWord> Definitions) Executable(GWord word) =>
        (Seq(word), Seq<GWord>());

    private static Fin<Seq<GWord>> WcsFrame(PostDialect dialect, GNode.CoordinateFrame node) => node.Assignment.Slot.Switch(
        state: (Dialect: dialect, Frame: node.Frame),
        @base: static (state, slot) => Base(state.Dialect, slot.Ordinal)
            .Map(select => Seq(Offset(state.Dialect, state.Frame, 2, slot.Ordinal, Arr<GParam>()), select)),
        extended: static (state, slot) => Extended(state.Dialect, "G54.1", slot.Ordinal)
            .Map(select => Seq(Offset(state.Dialect, state.Frame, 20, slot.Ordinal, Arr<GParam>()), select)),
        dynamic: static (state, slot) => state.Dialect.Features.Contains(DialectFeature.Tcp)
            ? Extended(state.Dialect, "G54.2", slot.Ordinal)
                .Map(select => Seq(Offset(state.Dialect, state.Frame, 2, slot.Ordinal, Arr<GParam>()), select))
            : Fin.Fail<Seq<GWord>>(Error.New($"dialect:wcs:dynamic:{state.Dialect.Key}")),
        rotary: static (state, slot) => state.Dialect.Features.Contains(DialectFeature.Rotary)
            ? Extended(state.Dialect, "G54.1", slot.Ordinal).Map(select => Seq(
                Offset(state.Dialect, state.Frame, 20, slot.Ordinal, Arr(GParam.Number(
                    slot.Axis <= 0 ? 'A' : slot.Axis == 1 ? 'B' : 'C',
                    Math.Atan2(state.Frame.XAxis.Y, state.Frame.XAxis.X) * 180.0 / Math.PI,
                    ProgramUnits.Metric))),
                select))
            : Fin.Fail<Seq<GWord>>(Error.New($"dialect:wcs:rotary:{state.Dialect.Key}")),
        local: static (state, slot) => slot.Parent > 0
            ? Fin.Succ(Seq<GWord>(GWord.Address(
                state.Dialect.CodeOverride(GCommand.LocalShift.Key).IfNone(GCommand.LocalShift.Code),
                ModalGroup.Transform, Origin(state.Frame, state.Dialect.Decimals), None, state.Dialect.Retention)))
            : Fin.Fail<Seq<GWord>>(Error.New($"dialect:wcs:local:{state.Dialect.Key}")));

    private static Fin<GWord> Base(PostDialect dialect, int ordinal) => ordinal > 0 && ordinal <= Math.Min(dialect.Wcs.Slots, 6)
        ? Fin.Succ<GWord>(GWord.Address($"G{53 + ordinal}", ModalGroup.Wcs, Arr<GParam>(), None, dialect.Retention))
        : Fin.Fail<GWord>(Error.New($"dialect:wcs:{dialect.Key}"));

    private static Fin<GWord> Extended(PostDialect dialect, string code, int ordinal) => ordinal > 0 && ordinal <= dialect.Wcs.Extended
        ? Fin.Succ<GWord>(GWord.Address(code, ModalGroup.Wcs, Arr(GParam.Number('P', ordinal, ProgramUnits.Metric)), None, dialect.Retention))
        : Fin.Fail<GWord>(Error.New($"dialect:wcs:{dialect.Key}"));

    private static GWord Offset(PostDialect dialect, Plane frame, int level, int ordinal, Arr<GParam> extra) => GWord.Address(
        dialect.CodeOverride(GCommand.SetWcs.Key).IfNone(GCommand.SetWcs.Code),
        ModalGroup.NonModal,
        Arr(GParam.Number('L', level, ProgramUnits.Metric), GParam.Number('P', ordinal, ProgramUnits.Metric))
            .Concat(Origin(frame, dialect.Decimals)).Concat(extra).ToArr(),
        None,
        dialect.Retention);

    private static Arr<GParam> Origin(Plane frame, int decimals) => Arr(
        GParam.Number('X', Math.Round(frame.Origin.X, decimals), ProgramUnits.Metric),
        GParam.Number('Y', Math.Round(frame.Origin.Y, decimals), ProgramUnits.Metric),
        GParam.Number('Z', Math.Round(frame.Origin.Z, decimals), ProgramUnits.Metric));

    private static Fin<GWord> Address(PostDialect dialect, GNode.Word word) =>
        Admit(dialect, word).Bind(admitted => Address(dialect, word, admitted));

    private static Fin<GWord> Address(PostDialect dialect, GNode.Word word, Arr<GParam> admitted) =>
        word.Command == GCommand.Wcs || word.Command == GCommand.WcsExtended
        ? WcsWord(dialect, admitted, word.Command == GCommand.WcsExtended)
        : Fin.Succ<GWord>(GWord.Address(
            dialect.CodeOverride(word.Command.Key).IfNone(word.Command.Code),
            word.Command.Group,
            admitted.Map(parameter => parameter.Round(dialect.Decimals)).ToArr(),
            word.Mode,
            dialect.Retention));

    // A conversational control without its own record grammar fails typed; emitting word-address records under a
    // conversational family would post a program the control cannot read.
    private static Fin<GWord> Verb(PostDialect dialect, GNode.Word word) =>
        Admit(dialect, word).Bind(admitted => dialect.Macro.Switch(
            state: (Dialect: dialect, Word: word, Words: admitted),
            qParam: static state => Klartext(state.Dialect, state.Word, state.Words),
            macroB: static state => Unsupported(state.Dialect, state.Word),
            rParam: static state => Unsupported(state.Dialect, state.Word),
            userTask: static state => Unsupported(state.Dialect, state.Word),
            none: static state => Unsupported(state.Dialect, state.Word)));

    private static Fin<GWord> Klartext(PostDialect dialect, GNode.Word word, Arr<GParam> admitted) => word.Command switch {
        var command when command == GCommand.Rapid || command == GCommand.Feed =>
            from motion in Fin.Succ(admitted.Filter(parameter => parameter.Address is 'X' or 'Y' or 'Z' or 'A' or 'B' or 'C'
                || (command == GCommand.Feed && parameter.Address == 'F')).ToArr())
            from _ in motion.ForAll(static parameter => parameter.Value.Scalar.IsSome)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.DialectUnsupported(dialect, word.Subject).ToError())
            select (GWord)GWord.Conversational(Seq(($"L {Coordinates(motion, dialect.Decimals)}"
                + (command == GCommand.Rapid ? " FMAX" : string.Empty)).Trim())),
        var command when command == GCommand.ArcCw || command == GCommand.ArcCcw =>
            from i in Center(admitted, 'I')
            from j in Center(admitted, 'J')
            select (GWord)GWord.Conversational(Seq(
                $"CC IX{Signed(i, dialect.Decimals)} IY{Signed(j, dialect.Decimals)}",
                $"C {Coordinates(admitted, dialect.Decimals)} DR{(command == GCommand.ArcCw ? "-" : "+")}")),
        _ => Address(dialect, word, admitted),
    };

    // An omitted center offset is zero by RS274 definition, so absence is the value; a present symbolic offset fails.
    private static Fin<double> Center(Arr<GParam> words, char address) =>
        words.Find(parameter => parameter.Address == address).Match(
            Some: parameter => Native(parameter.Value).ToFin(Error.New($"dialect:arc:{address}")),
            None: static () => Fin.Succ(0.0));

    private static string Coordinates(Arr<GParam> words, int decimals) => string.Join(
        " ",
        words.Filter(static parameter => parameter.Address is 'X' or 'Y' or 'Z' or 'A' or 'B' or 'C' or 'F')
            .Choose(parameter => Native(parameter.Value).Map(value => parameter.Address == 'F'
                ? $"F{Number(Math.Round(value, decimals))}"
                : $"{parameter.Address}{Signed(value, decimals)}"))
            .ToArray());

    private static string Signed(double value, int decimals) =>
        $"{(value >= 0.0 ? "+" : string.Empty)}{Number(Math.Round(value, decimals))}";

    private static Fin<GWord> WcsWord(PostDialect dialect, Arr<GParam> words, bool extended) {
        Option<int> ordinal = words.Find(static parameter => parameter.Address == 'P')
            .Bind(static parameter => parameter.Value.Scalar)
            .Filter(static value => value is > 0.0 and <= int.MaxValue && value == Math.Truncate(value))
            .Map(static value => checked((int)value));
        return ordinal.Match(
            Some: value => extended ? Extended(dialect, "G54.1", value) : Base(dialect, value),
            None: () => Fin.Fail<GWord>(Error.New($"dialect:wcs:{dialect.Key}")));
    }

    private static Fin<GWord> Cycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        Admit(dialect, new GNode.Word(cycle.Command, cycle.SingleBlockWords, cycle.Mode)).Bind(admitted => dialect.Cycles.Switch(
            state: (Dialect: dialect, Cycle: cycle, Words: admitted),
            singleBlock: static state => Address(state.Dialect, new GNode.Word(state.Cycle.Command, state.Words, state.Cycle.Mode), state.Words),
            expanded: static state => ExpandedCycle(state.Dialect, state.Cycle),
            dialectCycle: static state => Fin.Succ<GWord>(GWord.CycleCall(CycleRecords(state.Dialect, state.Cycle, state.Words)))));

    private static Fin<GWord> ExpandedCycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        GNode.Moves(cycle.ExpandedMoves, Point3d.Origin).Traverse(node => node is GNode.Word word
                ? Address(dialect, word)
                : Unsupported(dialect, node)).As()
            .Map<GWord>(static words => GWord.Expanded(words));

    // Control language, never controller identity: one MacroGrammar row is one vendor record grammar.
    private static Seq<string> CycleRecords(PostDialect dialect, GNode.CannedCycle cycle, Arr<GParam> words) {
        string code = dialect.CodeOverride(cycle.Command.Key).IfNone(cycle.Command.Code);
        Seq<string> values = words.Map(parameter => Value(parameter.Value, dialect.Decimals)).ToSeq();
        Seq<string> addressed = words.Map(parameter => $"{parameter.Address}{Value(parameter.Value, dialect.Decimals)}").ToSeq();
        return dialect.Macro.Switch(
            state: (Cycle: cycle, Code: code, Values: values, Addressed: addressed),
            rParam: static state => Seq((state.Cycle.Repeats > 1 ? "MCALL " : string.Empty)
                + $"{state.Code}({string.Join(", ", state.Values.ToArray())})"),
            qParam: static state => Seq($"CYCL DEF {state.Code}").Concat(state.Addressed)
                .Add(state.Cycle.Repeats > 1 ? $"CYCL CALL REP{state.Cycle.Repeats.ToString(CultureInfo.InvariantCulture)}" : "CYCL CALL"),
            macroB: static state => Iso(state.Code, state.Addressed, state.Cycle.Repeats),
            userTask: static state => Iso(state.Code, state.Addressed, state.Cycle.Repeats),
            none: static state => Iso(state.Code, state.Addressed, state.Cycle.Repeats));
    }

    private static Seq<string> Iso(string code, Seq<string> addressed, int repeats) => Seq(
        ($"{code} {string.Join(" ", addressed.ToArray())}"
            + (repeats > 1 ? $" L{repeats.ToString(CultureInfo.InvariantCulture)}" : string.Empty)).Trim());

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Macro(PostDialect dialect, GNode.Macro macro) =>
        Lower(dialect, macro.Body.ToSeq()).Bind(body => dialect.Macro.Switch(
            state: (Dialect: dialect, Macro: macro, Body: body),
            macroB: static state => Macro(state.Macro, state.Body, '#', state.Dialect.Decimals),
            rParam: static state => Macro(state.Macro, state.Body, 'R', state.Dialect.Decimals),
            qParam: static state => Macro(state.Macro, state.Body, 'Q', state.Dialect.Decimals),
            userTask: static state => Macro(state.Macro, state.Body, 'V', state.Dialect.Decimals),
            none: static state => Unsupported(state.Dialect, state.Macro).Map(Executable)));

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Macro(
        GNode.Macro macro,
        (Seq<GWord> Executable, Seq<GWord> Definitions) body,
        char prefix,
        int decimals) => Fin.Succ((
            Seq<GWord>(GWord.Macro(Assignments(Slots(macro.Slots, prefix), decimals), body.Executable, Seq<string>())),
            body.Definitions));

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Subprogram(PostDialect dialect, GNode.Subprogram subprogram) =>
        Lower(dialect, subprogram.Body.ToSeq()).Bind(body => dialect.Subprogram.Switch(
            state: (Dialect: dialect, Subprogram: subprogram, Body: body),
            m98: static state => Fin.Succ((
                Seq<GWord>(GWord.Text(Seq(($"M98 P{state.Subprogram.Label.ToString(CultureInfo.InvariantCulture)}"
                    + (state.Subprogram.Repeats > 1 ? $" L{state.Subprogram.Repeats.ToString(CultureInfo.InvariantCulture)}" : string.Empty))))),
                Seq<GWord>(GWord.Subprogram(
                    Seq($"O{state.Subprogram.Label.ToString(CultureInfo.InvariantCulture)}"),
                    state.Body.Executable,
                    Seq("M99"))).Concat(state.Body.Definitions))),
            label: static state => Fin.Succ((
                Seq<GWord>(GWord.Text(Seq($"CALL LBL {state.Subprogram.Label.ToString(CultureInfo.InvariantCulture)} REP {state.Subprogram.Repeats.ToString(CultureInfo.InvariantCulture)}"))),
                Seq<GWord>(GWord.Subprogram(
                    Seq($"LBL {state.Subprogram.Label.ToString(CultureInfo.InvariantCulture)}"),
                    state.Body.Executable,
                    Seq("LBL 0"))).Concat(state.Body.Definitions))),
            none: static state => Unsupported(state.Dialect, state.Subprogram).Map(Executable)));

    private static Arr<MacroSlot> Slots(Arr<MacroSlot> slots, char prefix) =>
        slots.Map(slot => slot with { Key = $"{prefix}{slot.Index.ToString(CultureInfo.InvariantCulture)}" }).ToArr();

    private static Seq<string> Assignments(Arr<MacroSlot> slots, int decimals) =>
        slots.Map(slot => $"{slot.Key}={Value(slot.Value, decimals)}").ToSeq();

    private static GWord AdditiveRecord(GNode.AdditiveLayer layer, int decimals) => GWord.Additive(Seq(
        $";LAYER:{layer.Layer.ToString(CultureInfo.InvariantCulture)}",
        $"M104 S{Number(Math.Round(layer.Temperatures.Hotend, decimals))}",
        $"M140 S{Number(Math.Round(layer.Temperatures.Bed, decimals))}",
        $"G1 E{Number(Math.Round(layer.Extrusion.Amount, decimals))} F{Number(Math.Round(layer.Extrusion.Feed, decimals))}"));

    private static Fin<GWord> Unsupported(PostDialect dialect, GNode node) =>
        Fin.Fail<GWord>(new FabricationFault.DialectUnsupported(dialect, node.Subject).ToError());

    private static Fin<Arr<GParam>> Admit(PostDialect dialect, GNode.Word word) =>
        word.Command.Admit(0, word.Words).Bind(parameters =>
            parameters.ForAll(static parameter => parameter.Value.Scalar.ForAll(double.IsFinite))
                && word.Command.Admits(dialect)
                && Capability(dialect, word.Command, parameters)
                ? Fin.Succ(parameters)
                : Fin.Fail<Arr<GParam>>(new FabricationFault.DialectUnsupported(dialect, word.Subject).ToError()));

    // GCommand.Admits already discharges the row's declared feature and modality demand; only parameter-decided
    // capability survives here.
    private static bool Capability(PostDialect dialect, GCommand command, Arr<GParam> parameters) {
        bool rotary = !parameters.Exists(static parameter => parameter.Address is 'A' or 'B' or 'C')
            || dialect.Features.Contains(DialectFeature.Rotary);
        bool radius = command != GCommand.CompLeft && command != GCommand.CompRight && command != GCommand.CompOff
            || dialect.Compensation.Contains(CutterCompKind.Radius);
        bool length = command != GCommand.LengthOffset && command != GCommand.LengthCancel
            || dialect.Compensation.Contains(CutterCompKind.Length);
        bool dwell = command != GCommand.Dwell
            || !parameters.Exists(static parameter => parameter.Address is 'X' or 'U')
            || dialect.Features.Contains(DialectFeature.RevolutionDwell);
        bool arc = command != GCommand.ArcCw && command != GCommand.ArcCcw || dialect.Arc.Exists(mode => {
            bool byRadius = parameters.Exists(static parameter => parameter.Address == 'R'
                && parameter.Value.Scalar.Exists(static value => Math.Abs(value) > 0.0));
            bool byCenter = parameters.Exists(static parameter => parameter.Address is 'I' or 'J' or 'K'
                && parameter.Value.Scalar.Exists(static value => Math.Abs(value) > 0.0));
            return mode == ArcMode.Both && byRadius != byCenter || mode == ArcMode.RWord && byRadius && !byCenter
                || mode == ArcMode.Ijk && byCenter && !byRadius;
        });
        return rotary && radius && length && dwell && arc;
    }

    private static string Value(GValue value, int decimals) => value.Switch(
        state: decimals,
        number: static (places, item) => Number(Math.Round(item.SourceUnits.Native(item.Canonical), places)),
        integer: static (_, item) => item.Value.ToString(CultureInfo.InvariantCulture),
        variable: static (_, item) => item.Lexeme,
        expression: static (_, item) => item.Lexeme,
        text: static (_, item) => item.Value);

    private static Option<double> Native(GValue value) => value.Switch(
        number: static item => Some(item.SourceUnits.Native(item.Canonical)),
        integer: static item => Some((double)item.Value),
        variable: static _ => None,
        expression: static _ => None,
        text: static _ => None);

    private static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------------------------------------------------------------
public static class Nc1Canonical {
    public static GWord Word(SteelImportReceipt receipt) => GWord.Nc1(Render(receipt.Part), receipt.Key);

    public static Seq<string> Render(SteelPart part) => Header(part.Header)
        .Concat(part.Features.Bind(Feature))
        .Add("EN");

    private static Seq<string> Header(SteelHeader header) => Seq(
        "ST",
        $"  {header.OrderIdentification}", $"  {header.DrawingIdentification}", $"  {header.PhaseIdentification}", $"  {header.PieceIdentification}",
        $"  {header.SteelQuality}", $"  {header.QuantityOfPieces}", $"  {header.Profile}", $"  {header.ProfileCode}",
        $"  {Mm(header.Length)}", $"  {Mm(header.SawLength)}", $"  {Mm(header.ProfileHeight)}", $"  {Mm(header.FlangeWidth)}",
        $"  {Mm(header.FlangeThickness)}", $"  {Mm(header.WebThickness)}", $"  {Mm(header.Radius)}", $"  {Deg(header.WebStartCut)}",
        $"  {Deg(header.WebEndCut)}", $"  {Deg(header.FlangeStartCut)}", $"  {Deg(header.FlangeEndCut)}", $"  {Number(header.WeightByMeter)}",
        $"  {Number(header.PaintingSurfaceByMeter)}", $"  {header.Text1InfoOnPiece}", $"  {header.Text2InfoOnPiece}",
        $"  {header.Text3InfoOnPiece}", $"  {header.Text4InfoOnPiece}");

    private static Seq<string> Feature(SteelFeature feature) => feature.Switch(
        hole: static feature => Seq("BO", $"  {feature.Face.ToValue()}  {Coord(feature.Center)}  {Mm(feature.Diameter)}  {Mm(feature.Depth)}"),
        slot: static feature => Seq("BO", $"  {feature.Face.ToValue()}  {Coord(feature.Center)}  {Mm(feature.Diameter)}  {Mm(feature.Depth)}  l{Mm(feature.Span)}  w{Mm(feature.Width)}  a{Deg(feature.Rotation)}"),
        cut: static feature => Seq("SC", $"  {feature.Face.ToValue()}  {Coord(feature.At)}"),
        numeration: static feature => Seq("SI", $"  {feature.Face.ToValue()}  {Coord(feature.At)}"),
        boundary: static feature => Contour(feature.Contour),
        marking: static feature => Contour(feature.Contour));

    private static Seq<string> Contour(SteelContour contour) =>
        Seq(contour.Block.Key).Concat(toSeq(contour.Vertices).Map(Vertex));

    private static string Vertex(SteelVertex vertex) =>
        $"  {Coord(vertex.At)}{(vertex.IsNotch ? "  n" : string.Empty)}{(vertex.Radius.As(LengthUnit.Millimeter) > 0.0 ? $"  r{Mm(vertex.Radius)}" : string.Empty)}"
        + vertex.Bevel.Map(static bevel => $"  v{Deg(bevel.FirstAngle)},{Mm(bevel.FirstBlunting)},{Deg(bevel.SecondAngle)},{Mm(bevel.SecondBlunting)}")
            .IfNone(string.Empty);

    private static string Coord(Point3d point) => $"u{Number(point.X)}  v{Number(point.Y)}  w{Number(point.Z)}";

    private static string Mm(UnitsNet.Length value) => Number(value.As(LengthUnit.Millimeter));

    private static string Deg(UnitsNet.Angle value) => Number(value.As(AngleUnit.Degree));

    private static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
```
