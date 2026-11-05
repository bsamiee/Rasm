import { Array, Effect, Match, type Record, Result, Schema, Struct } from 'effect';
import { Outcome, Records } from '#/cases/results';
import { Expression, Name, Panel, Reads } from '#/cases/schema';
import { HarnessError } from '#/harness-error';

// --- [TYPES] ---------------------------------------------------------------------------

type Engines = Record.ReadonlyRecord<string, { readonly extensions?: readonly string[] }>;
interface Contracted<G extends Engines, C extends Schema.Top, O extends Schema.Struct.Fields, F extends Schema.Struct.Fields, H extends Schema.Top, R extends Schema.Struct.Fields, B extends Schema.Struct.Fields> {
    readonly engines: G;
    readonly Case: C;
    readonly outcome: O;
    readonly survey: { readonly question: string; readonly host: H; readonly fields: F };
    readonly reply: R;
    readonly batch: B;
}

// --- [MODELS] --------------------------------------------------------------------------

const Source = Schema.Struct({ script: Schema.String.annotate({ description: 'Original script path the case answers as the running script' }), code: Schema.String.annotate({ description: 'Prepared source the engine evaluates' }) });
const Args = Schema.Array(Schema.Json);
const Folder = Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes' });
const Failed = Schema.Struct({ failed: Schema.String.annotate({ description: 'Batch failure before or around the cases' }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const forms = <P extends Schema.Top, S extends Schema.Struct.Fields>(precondition: P, shared: S) => {
    const common = { name: Name, read: Reads, precondition, folder: Folder, ...shared };
    return {
        Run: { ...common, source: Source, args: Args },
        Call: { ...common, source: Schema.optionalKey(Source), call: Expression, args: Args },
        Probe: { ...common, evaluate: Expression },
    };
};
const hostCase = Object.assign(<P extends Schema.Top, S extends Schema.Struct.Fields>(precondition: P, shared: S) => Schema.TaggedUnion(forms(precondition, shared)), {
    linked: <P extends Schema.Top, S extends Schema.Struct.Fields>(precondition: P, shared: S) =>
        Schema.TaggedUnion({
            ...forms(precondition, shared),
            Panel: {
                ...forms(precondition, shared).Probe,
                panel: Panel.fields.panel,
                plugin: Schema.String.annotate({ description: 'Plugin project folder the batch names, built to dial the harness and loaded into the instance' }),
                until: Schema.optionalKey(Expression),
                capture: Panel.fields.capture,
            },
        }),
});
const engineOf = <G extends Engines>(engines: G) => {
    const names = Struct.keys(engines);
    const claims = (file: string) => (name: keyof G & string) => engines[name]?.extensions?.some((extension) => file.endsWith(`.${extension}`)) ?? true;
    return (scripts: readonly string[]): Result.Result<keyof G & string, HarnessError> => {
        const unavailable = HarnessError.unavailable(scripts.join(', '));
        const held = scripts.map((file) => Result.fromOption(Array.findFirst(names, claims(file)), () => file));
        return Match.value({ unknown: Array.getFailures(held), found: Array.dedupe(Array.getSuccesses(held)) }).pipe(
            Match.when({ unknown: (listed) => listed.length > 0 }, ({ unknown }) => Result.fail(unavailable(`${unknown.join(', ')} holds no extension of ${names.map((engine) => `${engine} ${(engines[engine]?.extensions ?? []).join(' ')}`).join(', ')}`))),
            Match.when({ found: (listed) => listed.length > 1 }, ({ found }) => Result.fail(unavailable(`cases mix engines ${found.join(', ')}`))),
            Match.orElse(({ found }) => Result.fromOption(Array.head([...found, ...names]), () => unavailable('the host declares no engine'))),
        );
    };
};
const contract = <const G extends Engines, C extends Schema.Top, O extends Schema.Struct.Fields, F extends Schema.Struct.Fields, H extends Schema.Top, R extends Schema.Struct.Fields, B extends Schema.Struct.Fields>(spec: Contracted<G, C, O, F, H, R, B>) => {
    const Survey = Schema.Struct({ ...spec.survey.fields, host: spec.survey.host }).annotate({ description: spec.survey.question });
    const outcome = Outcome.mapFields(Struct.assign(spec.outcome));
    const Header = Schema.Struct(Struct.assign({ application: Schema.String, version: Schema.String, survey: Schema.optionalKey(Survey) }, spec.reply));
    const Reply = Schema.fromJsonString(Schema.Union([Header, Failed])).annotate({ description: 'Engine answer to one batch, case outcomes sitting in the records file' });
    return {
        engines: spec.engines,
        Engine: Schema.Literals(Struct.keys(spec.engines)),
        engine: engineOf(spec.engines),
        Case: spec.Case,
        Outcome: outcome,
        Recorded: Records.line(outcome),
        Survey,
        question: spec.survey.question,
        Reply: Object.assign(Reply, {
            replied: (subject: string) => (answer: unknown) => Effect.filterOrElse(Schema.decodeUnknownEffect(Reply)(answer), Schema.is(Header), (held) => Effect.flatMap(Schema.decodeUnknownEffect(Failed)(held), ({ failed }) => HarnessError.unavailable(subject)(failed))),
        }),
        Encoded: Schema.fromJsonString(Schema.Struct({ records: Records, survey: Schema.Boolean.annotate({ description: 'Reply carries the installation survey' }), cases: Schema.Array(spec.Case), ...spec.batch })),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { contract, Failed, hostCase };
