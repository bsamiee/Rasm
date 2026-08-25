# [RASM_FABRICATION_DIALECT]

`Dialect` owns one admitted `CutProgram`-to-byte projection: grammar-family lowering, motion-directive lowering, command admission, modal rendering, physical-record census, block framing, sequence framing, and content-key minting resolve through one correspondence. `PostDialect`, `GNode`, and `GWord` remain the frozen input vocabulary; `PostImage` is the sole egress value, and `Nc1Map` projects `SteelHeader` through the inverse of the steel reader's own transcription rather than a second positional roster.

Controller syntax is posting-owned and lives in ROWS: `FamilyGrammar` carries one row per `PostFamily`, `MacroSyntax` and `SubprogramSyntax` carry the control-language prefixes and call templates their `family` rows only name, and every vendor spelling a body once carried — extended and dynamic work offsets, subprogram call and return, layer marks, hotend and bed temperature, the extrude move — resolves through `PostDialect.CodeOverride`, keyed on a `CommandKeys` static where posting owns the concept and on the command row's own key where `GCommand` already names it. A dialect identity test never appears in emission, and a hardcoded vendor word in a method body is the deleted form. `Number` renders through `PostDialect.Decimals` at ONE declaration, so a controller's declared precision is what reaches its records.

## [01]-[INDEX]

- [02]-[EMISSION_POLICY]: `ChecksumRule`, `SequenceCounter`, `RecordFrame`, `BlockLimit`, `EmitPolicy`, and `PostImage`.
- [03]-[GRAMMAR_ROWS]: `FamilyGrammar`, `MacroSyntax`, `SubprogramSyntax`, and the vendor spellings every row resolves through `CommandKeys`.
- [04]-[EMISSION]: `Dialect.Emit` lowers, renders, frames, seals, and keys one program through one generated dispatch.
- [05]-[DIRECTIVES]: `MotionDirective` lowering to executable words where the dialect's features admit them and to a declared annotation where they do not.
- [06]-[COORDINATES]: `GNode.CoordinateFrame` lowering the assigned `WcsSlot` into an offset write and its selection word.
- [07]-[NC1]: `Nc1Map` projecting `SteelHeader` and `SteelFeature` into canonical DSTV records as the steel reader's inverse.
- [08]-[DELIVERY]: `ProgramDelivery` binding an emitted image to its acknowledged controller hand-off with a verified digest.

## [02]-[EMISSION_POLICY]

- Owner: `EmitPolicy` parameterizes text encoding, line termination, final termination, record framing, and block-limit enforcement; `RecordFrame` carries plain and sequence-numbered egress as cases; `SequenceCounter` admits the numbering law once; `ChecksumRule` carries the digest, separator, and rendered width as row data.
- Law: `BlockLimit.Observe` exposes an over-cap measurement to optimization while `BlockLimit.Enforce` gates final egress; a measurement policy reaching final egress would post past the controller's own storage cap.
- Auto: `PostImage.Records` is the same population counted by `PhysicalRecords`, encoded into `Bytes`, and passed to `ContentKey.Of`; an empty population fails rather than keying a null artifact.
- Exemption: `ChecksumRule.Fold` is a measured byte kernel over the record span — the prior form copied the record to an array and re-wrapped it as a sequence for every digest, so a numbered program paid two allocations per line.
- Result: `ProgramDelivery` reads `PhysicalRecords` as the transferred count, so the acknowledgement and the image agree on one census.
- Packages: `Thinktecture.Runtime.Extensions` generates `ChecksumRule`, `SequenceCounter`, `RecordFrame`, `BlockLimit`, and `EmitPolicy`; `Encoding.GetBytes` and `ContentKey.Of` seal egress.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] ---------------------------------------------------------------------------
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

    private static uint Fold(ReadOnlyMemory<byte> record, uint seed, Func<uint, byte, uint> step) {
        uint state = seed;
        ReadOnlySpan<byte> span = record.Span;
        for (int index = 0; index < span.Length; index++)
            state = step(state, span[index]);
        return state;
    }

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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref int first, ref int step, ref int modulus) {
        if (first < 0 || step <= 0 || modulus < 0 || (modulus > 0 && first >= modulus))
            validationError = new ValidationError("dialect:sequence-counter");
    }

    public static Fin<SequenceCounter> Admit(int first, int step, int modulus) =>
        Validate(first, step, modulus, out SequenceCounter counter).Admitted(counter);
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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Encoding codec,
        ref string newLine,
        ref bool finalTerminator,
        ref RecordFrame frame,
        ref BlockLimit limit) {
        if (newLine.Length == 0)
            validationError = new ValidationError("dialect:emit-policy:newline");
    }

    public static Fin<EmitPolicy> Admit(
        Encoding codec, string newLine, bool finalTerminator, RecordFrame frame, BlockLimit limit) =>
        Validate(codec, newLine, finalTerminator, frame, limit, out EmitPolicy policy).Admitted(policy);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PostImage(
    EgressKind Kind,
    Seq<string> Records,
    ReadOnlyMemory<byte> Bytes,
    ContentKey Key,
    int PhysicalRecords);
```

## [03]-[GRAMMAR_ROWS]

- Owner: `FamilyGrammar` owns one row per `PostFamily` — the word renderer, the cycle renderer, and whether the family admits structured bodies; `MacroSyntax` and `SubprogramSyntax` own the control-language spellings their `family.md` grammar rows only NAME.
- Law: a vendor word never appears in a method body. Extended and dynamic work offsets, subprogram call and return, layer marks, temperature words, and the extrude move all resolve through `PostDialect.CodeOverride` — keyed on a `CommandKeys` static, or on the command row's own key where `GCommand` names it — so a controller that spells one differently is one map entry on its own `PostDialect` row and a body that hardcoded it is the deleted form.
- Law: `PostFamily.Forming` carries a real row. A press-brake control reads a numbered bend record, so a forming dialect renders its words as bend records rather than falling to the unsupported arm — a family the vocabulary declares and emission cannot answer is a controller the package claims to post for and refuses.
- Cases: `MacroSyntax` carries the assignment prefix each control language spells its parameters with; `SubprogramSyntax` carries the call, definition-open, and definition-close templates; both key on the `family.md` grammar row so a new control language is one row on each table.
- Auto: `Number` renders through `PostDialect.Decimals` at ONE declaration, so a controller's declared precision reaches every value it emits and the four-decimal literal the prior body carried cannot contradict a three-decimal dialect.
- Growth: one grammar family adds one `FamilyGrammar` row; one control language adds one `MacroSyntax` and one `SubprogramSyntax` row; one vendor spelling adds one `CommandKeys` static and one `CodeOverrides` entry on the owning dialect row, or one `Codes` bundle member where a family of controllers spells it identically.
- Boundary: these rows decide SPELLING alone — admissibility stays with `GCommand.Admits` and `PostDialect.Features`.

```csharp
// --- [GRAMMAR_ROWS] --------------------------------------------------------------------
public sealed record FamilyGrammar(
    Func<PostDialect, GNode.Word, Fin<GWord>> Word,
    Func<PostDialect, GNode.CannedCycle, Fin<GWord>> Cycle,
    bool Structured);

public sealed record MacroSyntax(char Prefix);

public sealed record SubprogramSyntax(
    Func<PostDialect, int, int, Seq<string>> Call,
    Func<PostDialect, int, Seq<string>> Open,
    Func<PostDialect, Seq<string>> Close);

public static partial class Dialect {
    private static readonly FrozenDictionary<PostFamily, FamilyGrammar> Families =
        new Dictionary<PostFamily, FamilyGrammar> {
            [PostFamily.WordAddress] = new(Address, Cycle, Structured: true),
            [PostFamily.Conversational] = new(Verb, Cycle, Structured: true),
            [PostFamily.AdditiveGcode] = new(Address, ExpandedCycle, Structured: false),
            [PostFamily.Forming] = new(BendRecord, BendCycle, Structured: false),
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<MacroGrammar, MacroSyntax> Macros =
        new Dictionary<MacroGrammar, MacroSyntax> {
            [MacroGrammar.MacroB] = new('#'),
            [MacroGrammar.RParam] = new('R'),
            [MacroGrammar.QParam] = new('Q'),
            [MacroGrammar.UserTask] = new('V'),
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<SubprogramGrammar, SubprogramSyntax> Subprograms =
        new Dictionary<SubprogramGrammar, SubprogramSyntax> {
            [SubprogramGrammar.M98] = new(
                static (post, label, repeats) => Seq((Word(post, CommandKeys.SubprogramCall)
                    + Ordinal(post, CommandKeys.SubprogramLabel, label)
                    + (repeats > 1 ? Ordinal(post, CommandKeys.SubprogramRepeat, repeats) : string.Empty)).Trim()),
                static (post, label) => Seq(Word(post, CommandKeys.SubprogramDefine) + Integer(label)),
                static post => Seq(Word(post, CommandKeys.SubprogramReturn))),
            [SubprogramGrammar.Label] = new(
                static (post, label, repeats) => Seq((Word(post, CommandKeys.SubprogramCall) + " " + Integer(label)
                    + " " + Word(post, CommandKeys.SubprogramRepeat) + " " + Integer(repeats)).Trim()),
                static (post, label) => Seq(Word(post, CommandKeys.SubprogramDefine) + " " + Integer(label)),
                static post => Seq(Word(post, CommandKeys.SubprogramReturn))),
        }.ToFrozenDictionary();

    private static Fin<string> Spelling(PostDialect dialect, string commandKey, FaultSubject.ProgramNode subject) =>
        dialect.CodeOverride(commandKey).ToFin(new FabricationFault.DialectUnsupported(dialect, subject));

    private static string Word(PostDialect dialect, string commandKey) =>
        dialect.CodeOverride(commandKey).IfNone(commandKey);

    private static string Ordinal(PostDialect dialect, string commandKey, int value) =>
        $" {Word(dialect, commandKey)}{Integer(value)}";

    private static string Integer(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Number(PostDialect dialect, double value) =>
        Math.Round(value, dialect.Decimals).ToString($"F{Integer(dialect.Decimals)}", CultureInfo.InvariantCulture)
            .TrimEnd('0').TrimEnd('.');

    private static string Value(PostDialect dialect, GValue value) => value.Switch(
        state: dialect,
        number: static (post, item) => Number(post, item.SourceUnits.Native(item.Canonical)),
        integer: static (_, item) => Integer(item.Value),
        variable: static (_, item) => item.Lexeme,
        expression: static (_, item) => item.Lexeme,
        text: static (_, item) => item.Value);

    private static Option<double> Native(GValue value) => value.Switch(
        number: static item => Some(item.SourceUnits.Native(item.Canonical)),
        integer: static item => Some((double)item.Value),
        variable: static _ => None,
        expression: static _ => None,
        text: static _ => None);
}
```

## [04]-[EMISSION]

- Owner: `Dialect` owns the byte projection, while `PostImage` carries the exact records, bytes, kind, key, and physical-record count.
- Law: lowering dispatches through the GENERATED `Switch` over `GNode` and reads the family row inside each arm, so a new node case cannot compile without an arm and a new family cannot fall silently to an unsupported discard.
- Entry: `Dialect.Emit` is the one public operation and consumes a complete `CutProgram` with `EmitPolicy`.
- Auto: `GCommand.Admits` discharges the command row's own declared `Requires` and `Modalities` against the dialect, so emission gates only what the parameters decide — rotary addresses, compensation kind, revolution dwell, and arc representation. `GWord.Render` frames and counts only its returned `ProgramRender.Lines`, so macro assignments, dialect-cycle parameters, subprogram definitions, additive records, and NC1 records cannot escape `BlockCap`.
- Result: subprogram definitions hoist into one label-keyed stream; identical definitions share one row, and conflicting bodies fail before rendering.
- Boundary: `Dialect` never reparses, reconditions motion, invents absent command parameters, or maintains a second block-count projection. Parsed `Sequence` and `Checksum` values never survive, because `RecordFrame` owns numbering and digest on re-emission.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Dialect {
    public static Fin<PostImage> Emit(CutProgram program, EmitPolicy policy) =>
        from kind in OutputKind(program)
        from lowered in Lower(program.Dialect, program.Nodes)
        from executable in GWord.Render(lowered.Executable)
        from unique in Distinct(program.Dialect, lowered.Definitions)
        from definitions in GWord.Render(unique)
        from records in Frame(program.Dialect, kind, policy, executable.Lines.Concat(definitions.Lines))
        from _ in Cap(program.Dialect, policy.Limit, records)
        from image in Image(program.Dialect, kind, policy, records)
        select image;

    private static Fin<EgressKind> OutputKind(CutProgram program) {
        Seq<GNode> leaves = program.Nodes.Bind(Leaves);
        bool anyNc1 = leaves.Exists(static node => node is GNode.Nc1);
        bool onlyNc1 = !leaves.IsEmpty && leaves.ForAll(static node => node is GNode.Nc1);
        return !anyNc1 || onlyNc1
            ? Fin.Succ(onlyNc1 ? EgressKind.Nc1 : EgressKind.CutProgram)
            : Fin.Fail<EgressKind>(new FabricationFault.DialectUnsupported(
                program.Dialect, new FaultSubject.ProgramNode("mixed-grammar")));
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

    private static Fin<Seq<GWord>> Distinct(PostDialect dialect, Seq<GWord> definitions) => definitions
        .FoldM<Fin, (Seq<GWord> Words, Map<string, GWord.Subprogram> Definitions)>(
            (Seq<GWord>(), Map<string, GWord.Subprogram>()),
            (state, word) => word is not GWord.Subprogram definition
                ? Fin.Succ((state.Words.Add(word), state.Definitions))
                : definition.Open.Head
                    .ToFin(new FabricationFault.DialectUnsupported(dialect, new FaultSubject.ProgramNode("subprogram")))
                    .Bind(label => state.Definitions.Find(label).Match(
                        Some: held => held == definition
                            ? Fin.Succ(state)
                            : Fin.Fail<(Seq<GWord>, Map<string, GWord.Subprogram>)>(
                                new FabricationFault.DialectUnsupported(dialect, new FaultSubject.ProgramNode($"subprogram:{label}"))),
                        None: () => Fin.Succ((state.Words.Add(word),
                            state.Definitions.AddOrUpdate(label, definition)))))).As()
        .Map(static state => state.Words);

    private static Fin<PostImage> Image(PostDialect dialect, EgressKind kind, EmitPolicy policy, Seq<string> records) {
        if (records.IsEmpty)
            return Fin.Fail<PostImage>(new FabricationFault.DialectUnsupported(
                dialect, new FaultSubject.ProgramNode("empty-image")));
        string text = string.Join(policy.NewLine, records.ToArray()) + (policy.FinalTerminator ? policy.NewLine : string.Empty);
        ReadOnlyMemory<byte> bytes = policy.Codec.GetBytes(text);
        return Fin.Succ(new PostImage(kind, records, bytes, ContentKey.Of(kind, bytes.Span), records.Count));
    }

    private static Fin<Seq<string>> Cap(PostDialect dialect, BlockLimit limit, Seq<string> records) => limit.Switch(
        state: (Dialect: dialect, Records: records),
        observe: static state => Fin.Succ(state.Records),
        enforce: static state => state.Dialect.BlockCap.Match(
            Some: cap => state.Records.Count > cap
                ? Fin.Fail<Seq<string>>(new FabricationFault.BlockCapExceeded(state.Dialect, state.Records.Count, cap))
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
                : Fin.Fail<RecordFrame>(new FabricationFault.DialectUnsupported(
                    state.Dialect, new FaultSubject.ProgramNode("record-frame"))));

    private static string Numbered(PostDialect dialect, RecordFrame.Numbered frame, Encoding codec, string line, int index) {
        string numbered = $"{Sequence(dialect)}{frame.Counter.At(index).ToString(CultureInfo.InvariantCulture)} {line}";
        return frame.Checksum.Map(rule => rule.Render(numbered, codec)).IfNone(numbered);
    }

    private static string Sequence(PostDialect dialect) =>
        dialect.Family == PostFamily.Conversational ? string.Empty : "N";

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Lower(PostDialect dialect, GNode node) =>
        node.Switch(
            state: dialect,
            block: static (post, value) => Lower(post, value.Body.ToSeq()).Bind(body => Framed(value.Frame, body)),
            word: static (post, value) => Grammar(post).Word(post, value).Map(Executable),
            cannedCycle: static (post, value) => Grammar(post).Cycle(post, value).Map(Executable),
            coordinateFrame: static (post, value) => WcsFrame(post, value).Map(static words => (words, Seq<GWord>())),
            macro: static (post, value) => Grammar(post).Structured
                ? Macro(post, value)
                : Unsupported(post, value).Map(Executable),
            subprogram: static (post, value) => Grammar(post).Structured
                ? Subprogram(post, value)
                : Unsupported(post, value).Map(Executable),
            additiveLayer: static (post, value) => post.Family == PostFamily.AdditiveGcode
                ? AdditiveRecord(post, value).Map(Executable)
                : Unsupported(post, value).Map(Executable),
            nc1: static (_, value) => Fin.Succ(Executable(Nc1Canonical.Word(value.Import))),
            directive: static (post, value) => Directive(post, value.Value));

    private static FamilyGrammar Grammar(PostDialect dialect) => Families[dialect.Family];

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Lower(PostDialect dialect, Seq<GNode> body) =>
        body.Traverse(node => Lower(dialect, node)).As().Map(static rows => rows.Fold(
            (Executable: Seq<GWord>(), Definitions: Seq<GWord>()),
            static (state, row) => (state.Executable.Concat(row.Executable), state.Definitions.Concat(row.Definitions))));

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Framed(
        BlockFrame frame, (Seq<GWord> Executable, Seq<GWord> Definitions) body) =>
        (frame.Optional
            ? GWord.Render(body.Executable).Map(static rendered =>
                Seq<GWord>(new GWord.Text(rendered.Lines.Map(static line => $"/{line}"))))
            : Fin.Succ(body.Executable))
        .Map(executable => (Seq<GWord>(new GWord.Text(Structure(frame))).Concat(executable), body.Definitions));

    private static Seq<string> Structure(BlockFrame frame) => (frame.Delimiter ? Seq("%") : Seq<string>())
        .Concat(frame.Program.Map(static value => $"O{Integer(value)}").ToSeq())
        .Concat(frame.Comments);

    private static (Seq<GWord> Executable, Seq<GWord> Definitions) Executable(GWord word) =>
        (Seq(word), Seq<GWord>());

    private static Fin<GWord> Address(PostDialect dialect, GNode.Word word) =>
        Admit(dialect, word).Bind(admitted => Address(dialect, word, admitted));

    private static Fin<GWord> Address(PostDialect dialect, GNode.Word word, Arr<GParam> admitted) =>
        word.Command == GCommand.Wcs || word.Command == GCommand.WcsExtended
        ? WcsWord(dialect, admitted, word.Command)
        : Fin.Succ<GWord>(new GWord.Address(
            Word(dialect, word.Command.Key),
            word.Command.Group,
            admitted.Map(parameter => parameter.Round(dialect.Decimals)).ToArr(),
            word.Mode,
            dialect.Retention));

    private static Fin<GWord> Verb(PostDialect dialect, GNode.Word word) =>
        Admit(dialect, word).Bind(admitted => dialect.Macro == MacroGrammar.QParam
            ? Klartext(dialect, word, admitted)
            : Unsupported(dialect, word));

    private static Fin<GWord> Klartext(PostDialect dialect, GNode.Word word, Arr<GParam> admitted) => word.Command switch {
        var command when command == GCommand.Rapid || command == GCommand.Feed =>
            from motion in Fin.Succ(admitted.Filter(parameter => parameter.Address is 'X' or 'Y' or 'Z' or 'A' or 'B' or 'C'
                || (command == GCommand.Feed && parameter.Address == 'F')).ToArr())
            from _ in motion.ForAll(static parameter => parameter.Value.Scalar.IsSome)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.DialectUnsupported(dialect, word.Subject))
            select (GWord)new GWord.Conversational(Seq(($"L {Coordinates(dialect, motion)}"
                + (command == GCommand.Rapid ? " FMAX" : string.Empty)).Trim())),
        var command when command == GCommand.ArcCw || command == GCommand.ArcCcw =>
            from i in Center(dialect, word, admitted, 'I')
            from j in Center(dialect, word, admitted, 'J')
            select (GWord)new GWord.Conversational(Seq(
                $"CC IX{Signed(dialect, i)} IY{Signed(dialect, j)}",
                $"C {Coordinates(dialect, admitted)} DR{(command == GCommand.ArcCw ? "-" : "+")}")),
        _ => Address(dialect, word, admitted),
    };

    private static Fin<GWord> BendRecord(PostDialect dialect, GNode.Word word) =>
        Admit(dialect, word).Map(admitted => (GWord)new GWord.Conversational(Seq(
            ($"{Word(dialect, word.Command.Key)} "
                + string.Join(" ", admitted.Map(parameter =>
                    $"{parameter.Address}{Value(dialect, parameter.Value)}").ToArray())).Trim())));

    private static Fin<GWord> BendCycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        BendRecord(dialect, new GNode.Word(cycle.Command, cycle.SingleBlockWords, cycle.Mode));

    private static Fin<double> Center(PostDialect dialect, GNode.Word word, Arr<GParam> words, char address) =>
        words.Find(parameter => parameter.Address == address).Match(
            Some: parameter => Native(parameter.Value)
                .ToFin(new FabricationFault.DialectUnsupported(dialect, word.Subject)),
            None: static () => Fin.Succ(0.0));

    private static string Coordinates(PostDialect dialect, Arr<GParam> words) => string.Join(
        " ",
        words.Filter(static parameter => parameter.Address is 'X' or 'Y' or 'Z' or 'A' or 'B' or 'C' or 'F')
            .Choose(parameter => Native(parameter.Value).Map(value => parameter.Address == 'F'
                ? $"F{Number(dialect, value)}"
                : $"{parameter.Address}{Signed(dialect, value)}"))
            .ToArray());

    private static string Signed(PostDialect dialect, double value) =>
        $"{(value >= 0.0 ? "+" : string.Empty)}{Number(dialect, value)}";

    private static Fin<GWord> Cycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        Admit(dialect, new GNode.Word(cycle.Command, cycle.SingleBlockWords, cycle.Mode)).Bind(admitted => dialect.Cycles.Switch(
            state: (Dialect: dialect, Cycle: cycle, Words: admitted),
            singleBlock: static state => Address(state.Dialect, new GNode.Word(state.Cycle.Command, state.Words, state.Cycle.Mode), state.Words),
            expanded: static state => ExpandedCycle(state.Dialect, state.Cycle),
            dialectCycle: static state => Fin.Succ<GWord>(new GWord.CycleCall(CycleRecords(state.Dialect, state.Cycle, state.Words)))));

    private static Fin<GWord> ExpandedCycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        GNode.Moves(cycle.ExpandedMoves, Point3d.Origin).Traverse(node => node is GNode.Word word
                ? Address(dialect, word)
                : Unsupported(dialect, node)).As()
            .Map<GWord>(static words => new GWord.Expanded(words));

    private static Seq<string> CycleRecords(PostDialect dialect, GNode.CannedCycle cycle, Arr<GParam> words) {
        string code = Word(dialect, cycle.Command.Key);
        Seq<string> values = words.Map(parameter => Value(dialect, parameter.Value)).ToSeq();
        Seq<string> addressed = words.Map(parameter => $"{parameter.Address}{Value(dialect, parameter.Value)}").ToSeq();
        return dialect.Macro.Switch(
            state: (Dialect: dialect, Cycle: cycle, Code: code, Values: values, Addressed: addressed),
            rParam: static state => Seq((state.Cycle.Repeats > 1 ? "MCALL " : string.Empty)
                + $"{state.Code}({string.Join(", ", state.Values.ToArray())})"),
            qParam: static state => Seq($"CYCL DEF {state.Code}").Concat(state.Addressed)
                .Add(state.Cycle.Repeats > 1 ? $"CYCL CALL REP{Integer(state.Cycle.Repeats)}" : "CYCL CALL"),
            macroB: static state => Iso(state.Code, state.Addressed, state.Cycle.Repeats),
            userTask: static state => Iso(state.Code, state.Addressed, state.Cycle.Repeats),
            none: static state => Iso(state.Code, state.Addressed, state.Cycle.Repeats));
    }

    private static Seq<string> Iso(string code, Seq<string> addressed, int repeats) => Seq(
        ($"{code} {string.Join(" ", addressed.ToArray())}"
            + (repeats > 1 ? $" L{Integer(repeats)}" : string.Empty)).Trim());

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Macro(PostDialect dialect, GNode.Macro macro) =>
        Macros.TryGetValue(dialect.Macro, out MacroSyntax? syntax)
            ? Lower(dialect, macro.Body.ToSeq()).Map(body => (
                Seq<GWord>(new GWord.Macro(
                    macro.Slots.Map(slot => $"{syntax.Prefix}{Integer(slot.Index)}={Value(dialect, slot.Value)}").ToSeq(),
                    body.Executable,
                    Seq<string>())),
                body.Definitions))
            : Unsupported(dialect, macro).Map(Executable);

    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Subprogram(
        PostDialect dialect, GNode.Subprogram subprogram) =>
        Subprograms.TryGetValue(dialect.Subprogram, out SubprogramSyntax? syntax)
            ? Lower(dialect, subprogram.Body.ToSeq()).Map(body => (
                Seq<GWord>(new GWord.Text(syntax.Call(dialect, subprogram.Label, subprogram.Repeats))),
                Seq<GWord>(new GWord.Subprogram(
                    syntax.Open(dialect, subprogram.Label), body.Executable, syntax.Close(dialect)))
                    .Concat(body.Definitions)))
            : Unsupported(dialect, subprogram).Map(Executable);

    private static Fin<GWord> AdditiveRecord(PostDialect dialect, GNode.AdditiveLayer layer) =>
        from mark in Spelling(dialect, CommandKeys.LayerMark, new FaultSubject.ProgramNode("additive-layer"))
        from extrude in Spelling(dialect, CommandKeys.ExtrudeMove, new FaultSubject.ProgramNode("additive-layer"))
        select (GWord)new GWord.Additive(Seq(
            $"{mark}{Integer(layer.Layer)}",
            $"{Word(dialect, GCommand.HotendTemp.Key)} S{Number(dialect, layer.Temperatures.Hotend)}",
            $"{Word(dialect, GCommand.BedTemp.Key)} S{Number(dialect, layer.Temperatures.Bed)}",
            $"{extrude}{Number(dialect, layer.Extrusion.Amount)} F{Number(dialect, layer.Extrusion.Feed)}"));

    private static Fin<GWord> Unsupported(PostDialect dialect, GNode node) =>
        Fin.Fail<GWord>(new FabricationFault.DialectUnsupported(dialect, node.Subject));

    private static Fin<Arr<GParam>> Admit(PostDialect dialect, GNode.Word word) =>
        word.Command.Admit(0, word.Words).Bind(parameters =>
            parameters.ForAll(static parameter => parameter.Value.Scalar.ForAll(double.IsFinite))
                && word.Command.Admits(dialect)
                && Capability(dialect, word.Command, parameters)
                ? Fin.Succ(parameters)
                : Fin.Fail<Arr<GParam>>(new FabricationFault.DialectUnsupported(dialect, word.Subject)));

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
}
```

## [05]-[DIRECTIVES]

- Owner: `Dialect.Directive` owns the lowering of every `MotionDirective` case the S0 atoms floor declares.
- Law: a directive lowers to an EXECUTABLE word where the dialect's declared features admit it, and to a declared ANNOTATION where they do not — never silently. A controller with no revolution-dwell feature receives the dwell as a comment carrying its own basis and amount, so the operator reads the intent the control cannot execute rather than running a program the intent vanished from.
- Law: `OrientedStop` emits the DECLARED orient angle. Re-deriving an angle from the retract vector discards the angle the atom carries and publishes a different stop position wherever the two disagree.
- Cases: spindle law with its control mode and hand; a dwell carrying `DwellBasis`, which decides the time or revolution address; a synchronized channel pair; an oriented stop with its orient angle and retract; a channel barrier; and an admitted `SpecializedToolpathEnvelope`.
- Auto: `DialectFeature.TimeDwell` and `DialectFeature.RevolutionDwell` decide the dwell address from the basis, so no arm tests a controller identity; `CommandKeys.MotionSynchronize` and `CommandKeys.ChannelBarrier` resolve their vendor words through the dialect's own override map.
- Result: every `SpecializedToolpathEnvelope` arrives ADMITTED, so each row renders its evidence directly and no local revalidation runs.
- Boundary: an annotation rides the family's own comment channel — parenthesised for word-address, semicolon for every other family — so a controller that ignores comments loses nothing.

```csharp
// --- [DIRECTIVES] ----------------------------------------------------------------------
public static partial class Dialect {
    private static Fin<(Seq<GWord> Executable, Seq<GWord> Definitions)> Directive(
        PostDialect dialect, MotionDirective directive) => directive.Switch(
        state: dialect,
        spindle: static (post, row) => Lower(post, new GNode.Word(
            row.Control == SpindleControl.ConstantSurface ? GCommand.Css : GCommand.Spindle,
            row.Control == SpindleControl.ConstantSurface
                ? Arr(GParam.Number('S', row.SurfaceMetersPerMinute, ProgramUnits.Metric),
                    GParam.Number('D', row.ResolvedRpm, ProgramUnits.Metric))
                : Arr(GParam.Number('S', row.ResolvedRpm, ProgramUnits.Metric)),
            None)),
        dwell: static (post, row) => DwellAddress(post, row.Basis).Match(
            Some: address => Lower(post, new GNode.Word(
                GCommand.Dwell, Arr(GParam.Number(address, row.Amount, ProgramUnits.Metric)), None)),
            None: () => Fin.Succ(Executable(Annotated(post, Seq(
                $"DWELL {row.Basis.Key.ToUpperInvariant()} {Number(post, row.Amount)}"))))),
        synchronize: static (post, row) => post.CodeOverride(CommandKeys.MotionSynchronize).Match(
            Some: code => Fin.Succ(Executable(new GWord.Address(code, ModalGroup.NonModal,
                Arr(
                    GParam.Number('P', row.FromMove, ProgramUnits.Metric),
                    GParam.Number('Q', row.ToMove, ProgramUnits.Metric),
                    GParam.Number('S', row.Rpm, ProgramUnits.Metric),
                    GParam.Number('F', row.Lead, ProgramUnits.Metric),
                    GParam.Number('H', row.Hand == RotationSense.Clockwise ? 1.0 : -1.0, ProgramUnits.Metric)),
                None,
                WordRetention.Explicit))),
            None: () => Fin.Succ(Executable(Annotated(post, Seq(
                $"SYNC {Integer(row.FromMove)}:{Integer(row.ToMove)} S{Number(post, row.Rpm)} "
                + $"F{Number(post, row.Lead)} {row.Hand.Key}"))))),
        orientedStop: static (post, row) => Lower(post, new GNode.Word(
            GCommand.SpindleOrient,
            Arr(
                GParam.Number('R', row.OrientDeg, ProgramUnits.Metric),
                GParam.Number('P', row.Retract.Length, ProgramUnits.Metric)),
            None)),
        channelBarrier: static (post, row) => post.CodeOverride(CommandKeys.ChannelBarrier).Match(
            Some: code => Fin.Succ(Executable(new GWord.Text(Seq(
                $"{code} {row.Channel} WAIT[{string.Join(',', row.WaitFor.ToArray())}] "
                + $"SIGNAL[{row.Signal.IfNone(string.Empty)}]")))),
            None: () => Fin.Succ(Executable(Annotated(post, Seq(
                $"BARRIER {Integer(row.Step)} {row.Channel} "
                + $"WAIT[{string.Join(',', row.WaitFor.ToArray())}] SIGNAL[{row.Signal.IfNone(string.Empty)}]"))))),
        specialized: static (post, row) => Fin.Succ(Executable(Annotated(post,
            row.Payload.Rows.Map(item => SpecializedRecord(post, item))))));

    private static Option<char> DwellAddress(PostDialect dialect, DwellBasis basis) => basis.Switch(
        state: dialect,
        seconds: static (post, _) => post.Features.Contains(DialectFeature.TimeDwell) ? Some('P') : None,
        revolutions: static (post, _) => post.Features.Contains(DialectFeature.RevolutionDwell) ? Some('U') : None);

    private static GWord Annotated(PostDialect dialect, Seq<string> records) => new GWord.Text(records.Map(record =>
        dialect.Family == PostFamily.WordAddress ? $"({RasmRecord} {record})" : $";{RasmRecord} {record}"));

    private const string RasmRecord = "RASM";

    private static string SpecializedRecord(PostDialect dialect, SpecializedToolpathRow row) => row.Switch(
        state: dialect,
        wire: static (post, value) => $"WIRE P{Integer(value.Pass)} S{Number(post, value.Station)} "
            + $"L{Point(post, value.Lower)} U{Point(post, value.Upper)} A{value.Action.Key} "
            + $"G{Number(post, value.LagMm)} R{value.RotaryDeg.Map(angle => Number(post, angle)).IfNone(string.Empty)}",
        bevel: static (post, value) => $"BEVEL P{Integer(value.Pass)} N{Integer(value.Move)} X{Point(post, value.Point)} "
            + $"A{Vector(post, value.ToolAxis)} V{Point(post, value.Pivot)} "
            + $"B{Number(post, value.AngleDeg)} C{Number(post, value.CrossTiltDeg)} F{Number(post, value.FeedMmPerMin)}",
        link: static (post, value) => $"LINK {value.From}>{value.To} K{value.Transition.Key} "
            + $"D{Number(post, value.DistanceMm)} T{Number(post, value.DurationSeconds)} "
            + $"L{Number(post, value.LiftMm)} R{Number(post, value.RotationPenalty)}",
        inspection: static (post, value) => $"INSPECT P{Integer(value.Pass)} "
            + $"B{Integer(value.FromBlock)}:{Integer(value.ToBlockExclusive)} "
            + $"A{Number(post, value.AngleDeviationDeg)} O{Number(post, value.OffsetDeviationMm)} "
            + $"C{(value.Conforming ? "1" : "0")}",
        turningThread: static (post, value) => $"TURN THREAD {value.Form.Key} {value.Side.Key} "
            + $"L{Number(post, value.LoadFlankDeg)} C{Number(post, value.ClearanceFlankDeg)}",
        turningAxial: static (post, value) => $"TURN AXIAL {value.Kind.Key} "
            + $"N{Integer(value.FromMove)}:{Integer(value.ToMove)} "
            + $"D{Number(post, value.Diameter)} Z{Number(post, value.Depth)} A{Number(post, value.TipAngleDeg)}",
        turningTap: static (post, value) => $"TURN TAP {value.Form.Key} {value.Hand.Key} "
            + $"N{Integer(value.FromMove)}:{Integer(value.ToMove)} "
            + $"D{Number(post, value.Diameter)} Z{Number(post, value.Depth)} P{Number(post, value.Pitch)}",
        turningKnurl: static (post, value) => $"TURN KNURL {value.Pattern.Key} "
            + $"N{Integer(value.FromMove)}:{Integer(value.ToMove)} P{Number(post, value.Pressure)}",
        turningHandoff: static (post, value) => $"TURN {value.Kind.Key} {value.From}>{value.To} "
            + $"G{Number(post, value.GripPlane)} L{Number(post, value.GripLength)} P{Number(post, value.PullDistance)}");

    private static string Point(PostDialect dialect, Point3d point) =>
        $"{Number(dialect, point.X)},{Number(dialect, point.Y)},{Number(dialect, point.Z)}";

    private static string Vector(PostDialect dialect, Vector3d vector) =>
        $"{Number(dialect, vector.X)},{Number(dialect, vector.Y)},{Number(dialect, vector.Z)}";
}
```

## [06]-[COORDINATES]

- Owner: `Dialect.WcsFrame` owns the lowering of one `WcsSlot` into an offset write and its selection word.
- Law: emission writes the OFFSET before selecting it; selection-only posting silently assumes the control already holds the frame. `WcsRoster` bounds every ordinal, `Local` lowers to the local-shift word against its parent, and `Rotary` carries its axis into the offset write.
- Auto: the extended and dynamic selection codes resolve through `CommandKeys.WcsExtended` and `CommandKeys.WcsDynamic` on the dialect's own override map, so a controller spelling either differently is one map entry.
- Boundary: a dialect declaring no dynamic-frame or rotary feature refuses the slot rather than degrading it to a base offset that means a different frame.

```csharp
// --- [COORDINATES] ---------------------------------------------------------------------
public static partial class Dialect {
    private static Fin<Seq<GWord>> WcsFrame(PostDialect dialect, GNode.CoordinateFrame node) => node.Assignment.Slot.Switch(
        state: (Dialect: dialect, Frame: node.Frame),
        @base: static (state, slot) => Base(state.Dialect, slot.Ordinal)
            .Map(select => Seq(Offset(state.Dialect, state.Frame, BaseLevel, slot.Ordinal, Arr<GParam>()), select)),
        extended: static (state, slot) => Spelling(state.Dialect, CommandKeys.WcsExtended, WcsSubject)
            .Bind(code => Extended(state.Dialect, code, slot.Ordinal))
            .Map(select => Seq(Offset(state.Dialect, state.Frame, ExtendedLevel, slot.Ordinal, Arr<GParam>()), select)),
        dynamic: static (state, slot) => state.Dialect.Features.Contains(DialectFeature.Tcp)
            ? Spelling(state.Dialect, CommandKeys.WcsDynamic, WcsSubject)
                .Bind(code => Extended(state.Dialect, code, slot.Ordinal))
                .Map(select => Seq(Offset(state.Dialect, state.Frame, BaseLevel, slot.Ordinal, Arr<GParam>()), select))
            : Fin.Fail<Seq<GWord>>(new FabricationFault.DialectUnsupported(state.Dialect, WcsSubject)),
        rotary: static (state, slot) => state.Dialect.Features.Contains(DialectFeature.Rotary)
            ? Spelling(state.Dialect, CommandKeys.WcsExtended, WcsSubject)
                .Bind(code => Extended(state.Dialect, code, slot.Ordinal))
                .Map(select => Seq(
                    Offset(state.Dialect, state.Frame, ExtendedLevel, slot.Ordinal, Arr(GParam.Number(
                        RotaryAddress(slot.Axis),
                        Math.Atan2(state.Frame.XAxis.Y, state.Frame.XAxis.X) * 180.0 / Math.PI,
                        ProgramUnits.Metric))),
                    select))
            : Fin.Fail<Seq<GWord>>(new FabricationFault.DialectUnsupported(state.Dialect, WcsSubject)),
        local: static (state, slot) => slot.Parent > 0
            ? Fin.Succ(Seq<GWord>(new GWord.Address(
                Word(state.Dialect, GCommand.LocalShift.Key),
                ModalGroup.Transform, Origin(state.Dialect, state.Frame), None, state.Dialect.Retention)))
            : Fin.Fail<Seq<GWord>>(new FabricationFault.DialectUnsupported(state.Dialect, WcsSubject)));

    private const int BaseLevel = 2;
    private const int ExtendedLevel = 20;

    private static readonly FaultSubject.ProgramNode WcsSubject = new("coordinate-frame");

    private static char RotaryAddress(double axis) => axis <= 0.0 ? 'A' : axis == 1.0 ? 'B' : 'C';

    private static Fin<GWord> Base(PostDialect dialect, int ordinal) =>
        ordinal > 0 && ordinal <= Math.Min(dialect.Wcs.Slots, 6)
            ? Fin.Succ<GWord>(new GWord.Address($"G{Integer(53 + ordinal)}", ModalGroup.Wcs, Arr<GParam>(), None, dialect.Retention))
            : Fin.Fail<GWord>(new FabricationFault.DialectUnsupported(dialect, WcsSubject));

    private static Fin<GWord> Extended(PostDialect dialect, string code, int ordinal) =>
        ordinal > 0 && ordinal <= dialect.Wcs.Extended
            ? Fin.Succ<GWord>(new GWord.Address(code, ModalGroup.Wcs,
                Arr(GParam.Number('P', ordinal, ProgramUnits.Metric)), None, dialect.Retention))
            : Fin.Fail<GWord>(new FabricationFault.DialectUnsupported(dialect, WcsSubject));

    private static GWord Offset(PostDialect dialect, Plane frame, int level, int ordinal, Arr<GParam> extra) => new GWord.Address(
        Word(dialect, GCommand.SetWcs.Key),
        ModalGroup.NonModal,
        toSeq(Arr(GParam.Number('L', level, ProgramUnits.Metric), GParam.Number('P', ordinal, ProgramUnits.Metric))
            .Concat(Origin(dialect, frame)).Concat(extra)).ToArr(),
        None,
        dialect.Retention);

    private static Arr<GParam> Origin(PostDialect dialect, Plane frame) => Arr(
        GParam.Number('X', Math.Round(frame.Origin.X, dialect.Decimals), ProgramUnits.Metric),
        GParam.Number('Y', Math.Round(frame.Origin.Y, dialect.Decimals), ProgramUnits.Metric),
        GParam.Number('Z', Math.Round(frame.Origin.Z, dialect.Decimals), ProgramUnits.Metric));

    private static Fin<GWord> WcsWord(PostDialect dialect, Arr<GParam> words, GCommand command) =>
        words.Find(static parameter => parameter.Address == 'P')
            .Bind(static parameter => parameter.Value.Scalar)
            .Filter(static value => value is > 0.0 and <= int.MaxValue && value == Math.Truncate(value))
            .Map(static value => checked((int)value))
            .Match(
                Some: value => command == GCommand.WcsExtended
                    ? Spelling(dialect, CommandKeys.WcsExtended, WcsSubject).Bind(code => Extended(dialect, code, value))
                    : Base(dialect, value),
                None: () => Fin.Fail<GWord>(new FabricationFault.DialectUnsupported(dialect, WcsSubject)));
}
```

## [07]-[NC1]

- Owner: `Nc1Header` is the NC1 `ST` descriptor as a POSITIONAL record of rendered fields; `Nc1Map` projects `SteelHeader` onto it; `Nc1Canonical` renders the whole part.
- Law: `Nc1Map` is the declared INVERSE of the steel reader's `DstvMap` and shares its unit codec family through `[UseStaticMapper]`, so the read and the write cannot drift on a unit. `RequiredMappingStrategy.Target` makes every descriptor column a build obligation — the prior form built the block by hand, so a column added to `SteelHeader` compiled clean and posted a short header.
- Law: `EnabledConversions = MappingConversionType.None` forces every `Length` and `Angle` through a declared `[UserMapping]`. The silent `ToString` fallback renders `1234 mm` where the descriptor demands `1234`, so a conversion Mapperly would have supplied automatically must be loud.
- Cases: hole, slot, cut, numeration, boundary contour, and marking contour each render their own DSTV block; a contour renders its block key then one record per vertex with its notch, radius, and bevel evidence.
- Auto: canonical records are both the file payload and the `EgressKind.Nc1` content-key input, so the read-only `DSTV.Net` model constrains header and feature parity without becoming an emission dependency.
- Packages: `Riok.Mapperly` owns the descriptor projection; `Rasm.Fabrication.Ingress` owns `SteelHeader`, `SteelFeature`, `SteelContour`, and the `DstvMap` codec family this mapper composes.
- Boundary: NC1 records carry their own fixed numeric spelling — a DSTV descriptor is read by field position and a controller-declared decimal count has no meaning in it — so this cluster renders through its own declared format rather than `PostDialect.Decimals`.

```csharp
// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed record Nc1Header(
    string OrderIdentification,
    string DrawingIdentification,
    string PhaseIdentification,
    string PieceIdentification,
    string SteelQuality,
    string QuantityOfPieces,
    string Profile,
    string ProfileCode,
    string Length,
    string SawLength,
    string ProfileHeight,
    string FlangeWidth,
    string FlangeThickness,
    string WebThickness,
    string Radius,
    string WebStartCut,
    string WebEndCut,
    string FlangeStartCut,
    string FlangeEndCut,
    string WeightByMeter,
    string PaintingSurfaceByMeter,
    string Text1InfoOnPiece,
    string Text2InfoOnPiece,
    string Text3InfoOnPiece,
    string Text4InfoOnPiece) {
    public Seq<string> Records => Seq(
        OrderIdentification, DrawingIdentification, PhaseIdentification, PieceIdentification, SteelQuality,
        QuantityOfPieces, Profile, ProfileCode, Length, SawLength, ProfileHeight, FlangeWidth, FlangeThickness,
        WebThickness, Radius, WebStartCut, WebEndCut, FlangeStartCut, FlangeEndCut, WeightByMeter,
        PaintingSurfaceByMeter, Text1InfoOnPiece, Text2InfoOnPiece, Text3InfoOnPiece, Text4InfoOnPiece);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, EnabledConversions = MappingConversionType.None)]
[UseStaticMapper(typeof(DstvMap))]
internal static partial class Nc1Map {
    [MapProperty(nameof(SteelHeader.QuantityOfPieces), nameof(Nc1Header.QuantityOfPieces), Use = nameof(Count))]
    [MapProperty(nameof(SteelHeader.ProfileCode), nameof(Nc1Header.ProfileCode), Use = nameof(Code))]
    [MapProperty(nameof(SteelHeader.Length), nameof(Nc1Header.Length), Use = nameof(Mm))]
    [MapProperty(nameof(SteelHeader.SawLength), nameof(Nc1Header.SawLength), Use = nameof(Mm))]
    [MapProperty(nameof(SteelHeader.ProfileHeight), nameof(Nc1Header.ProfileHeight), Use = nameof(Mm))]
    [MapProperty(nameof(SteelHeader.FlangeWidth), nameof(Nc1Header.FlangeWidth), Use = nameof(Mm))]
    [MapProperty(nameof(SteelHeader.FlangeThickness), nameof(Nc1Header.FlangeThickness), Use = nameof(Mm))]
    [MapProperty(nameof(SteelHeader.WebThickness), nameof(Nc1Header.WebThickness), Use = nameof(Mm))]
    [MapProperty(nameof(SteelHeader.Radius), nameof(Nc1Header.Radius), Use = nameof(Mm))]
    [MapProperty(nameof(SteelHeader.WebStartCut), nameof(Nc1Header.WebStartCut), Use = nameof(Deg))]
    [MapProperty(nameof(SteelHeader.WebEndCut), nameof(Nc1Header.WebEndCut), Use = nameof(Deg))]
    [MapProperty(nameof(SteelHeader.FlangeStartCut), nameof(Nc1Header.FlangeStartCut), Use = nameof(Deg))]
    [MapProperty(nameof(SteelHeader.FlangeEndCut), nameof(Nc1Header.FlangeEndCut), Use = nameof(Deg))]
    [MapProperty(nameof(SteelHeader.WeightByMeter), nameof(Nc1Header.WeightByMeter), Use = nameof(Scalar))]
    [MapProperty(nameof(SteelHeader.PaintingSurfaceByMeter), nameof(Nc1Header.PaintingSurfaceByMeter), Use = nameof(Scalar))]
    public static partial Nc1Header Header(SteelHeader source);

    [UserMapping]
    internal static string Mm(UnitsNet.Length value) => Scalar(value.As(LengthUnit.Millimeter));

    [UserMapping]
    internal static string Deg(UnitsNet.Angle value) => Scalar(value.As(AngleUnit.Degree));

    [UserMapping]
    internal static string Code(SteelProfileCode value) => value.Key;

    [UserMapping]
    internal static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);

    [UserMapping]
    internal static string Scalar(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

public static class Nc1Canonical {
    private const string Indent = "  ";

    public static GWord Word(ImportedSteel result) => new GWord.Nc1(Render(result.Part), result.Key);

    public static Seq<string> Render(SteelPart part) =>
        Seq(SteelBlockKind.St.Key)
            .Concat(Nc1Map.Header(part.Header).Records.Map(static field => $"{Indent}{field}"))
            .Concat(part.Features.Bind(Feature))
            .Add("EN");

    private static Seq<string> Feature(SteelFeature feature) => feature.Switch(
        hole: static row => Seq(SteelBlockKind.Bo.Key,
            $"{Indent}{row.Face.Key}{Indent}{Coord(row.Center)}{Indent}{Nc1Map.Mm(row.Diameter)}{Indent}{Nc1Map.Mm(row.Depth)}"),
        slot: static row => Seq(SteelBlockKind.Bo.Key,
            $"{Indent}{row.Face.Key}{Indent}{Coord(row.Center)}{Indent}{Nc1Map.Mm(row.Diameter)}{Indent}{Nc1Map.Mm(row.Depth)}"
            + $"{Indent}l{Nc1Map.Mm(row.Span)}{Indent}w{Nc1Map.Mm(row.Width)}{Indent}a{Nc1Map.Deg(row.Rotation)}"),
        cut: static row => Seq(SteelBlockKind.Sc.Key, $"{Indent}{row.Face.Key}{Indent}{Coord(row.At)}"),
        numeration: static row => Seq(SteelBlockKind.Si.Key, $"{Indent}{row.Face.Key}{Indent}{Coord(row.At)}"),
        boundary: static row => Contour(row.Contour),
        marking: static row => Contour(row.Contour));

    private static Seq<string> Contour(SteelContour contour) =>
        Seq(contour.Block.Key).Concat(toSeq(contour.Vertices).Map(Vertex));

    private static string Vertex(SteelVertex vertex) =>
        $"{Indent}{Coord(vertex.At)}{(vertex.IsNotch ? $"{Indent}n" : string.Empty)}"
        + (vertex.Radius.As(LengthUnit.Millimeter) > 0.0 ? $"{Indent}r{Nc1Map.Mm(vertex.Radius)}" : string.Empty)
        + vertex.Bevel.Map(static bevel =>
            $"{Indent}v{Nc1Map.Deg(bevel.FirstAngle)},{Nc1Map.Mm(bevel.FirstBlunting)},"
            + $"{Nc1Map.Deg(bevel.SecondAngle)},{Nc1Map.Mm(bevel.SecondBlunting)}").IfNone(string.Empty);

    private static string Coord(Point3d point) =>
        $"u{Nc1Map.Scalar(point.X)}{Indent}v{Nc1Map.Scalar(point.Y)}{Indent}w{Nc1Map.Scalar(point.Z)}";
}
```

## [08]-[DELIVERY]

- Owner: `ProgramDelivery` binds a posted `PostImage` to one `CellDelivery`: image key, transferred key, controller, upload state, drive log, operator, record count, and instant.
- Law: `Of` derives controller identity from the RESULT and rejects any non-upload, absent payload, or absent controller; `Verified` requires uploaded state, controller identity, and kind-plus-digest equality, so an acknowledgement that transferred a different artifact reads as unverified rather than as a hand-off.
- Auto: `Of` writes the settled custody verdict through `FabricationInstruments.DeliveryPrograms`; operator classification redacts shop identity.
- Result: `Documentation/traveler` reads `Verified` as hold evidence.
- Boundary: robot delivery rides the `CellDrive` upload channel, and a dialect gains no second delivery surface per transport.

```csharp
// --- [DELIVERY] ------------------------------------------------------------------------
public sealed record ProgramDelivery(
    ContentKey Image,
    Option<ContentKey> Transferred,
    string Controller,
    CellDriveKind Acknowledged,
    Seq<string> Log,
    [property: PersonalData] Option<string> Operator,
    int Records,
    Instant At) {
    public bool Verified => Acknowledged == CellDriveKind.Uploaded
        && Witness.Keyed(Controller)
        && Transferred.Exists(key => key.Kind == Image.Kind && key.Digest == Image.Digest);

    public static Fin<ProgramDelivery> Of(
        PostImage image,
        CellDelivery drive,
        Instant at,
        Option<InstrumentSet> set = default,
        Option<string> operatorId = default) =>
        from _state in drive.Kind == CellDriveKind.Uploaded
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Refusal("state"))
        from transferred in drive.Uploaded.ToFin(Refusal("uploaded"))
        from _digest in transferred.Kind == image.Key.Kind && transferred.Digest == image.Key.Digest
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Refusal("digest"))
        from controller in drive.Controller.Filter(Witness.Keyed).ToFin(Refusal("controller"))
        let delivery = new ProgramDelivery(
            image.Key, Some(transferred), controller, drive.Kind, drive.Log, operatorId, image.PhysicalRecords, at)
        from _delivery in set.Write(FabricationInstruments.DeliveryPrograms, 1d,
            (FabricationInstruments.KindSlot, delivery.Image.Kind.Key),
            (FabricationInstruments.VerdictSlot, delivery.Verified
                ? FabricationInstruments.Verified
                : FabricationInstruments.Unverified),
            (FabricationInstruments.ControllerSlot, delivery.Controller))
        select delivery;

    private static FabricationFault Refusal(string slot) =>
        FabricationFault.Inadmissible(FabConcern.Posting, $"dialect:delivery:{slot}");
}
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
