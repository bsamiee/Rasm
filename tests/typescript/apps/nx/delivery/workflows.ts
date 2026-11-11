import { layer } from '@effect/platform-node/NodeServices';
import { workspaceRoot } from '@nx/devkit';
import { Context, Effect, FileSystem, Layer, Path, Predicate, Record, Schema } from 'effect';
import { Yaml } from 'effect/encoding';

// --- [MODELS] --------------------------------------------------------------------------

const Values = Schema.Record(Schema.String, Schema.Unknown);
const Granted = Schema.Record(Schema.String, Schema.String);
const Grants = Schema.Union([Schema.String, Granted]);
const Step = Schema.Struct({ uses: Schema.optionalKey(Schema.String), run: Schema.optionalKey(Schema.String), if: Schema.optionalKey(Schema.Unknown), with: Schema.optionalKey(Values), env: Schema.optionalKey(Values) });
const Job = Schema.Struct({ 'runs-on': Schema.optionalKey(Schema.Unknown), if: Schema.optionalKey(Schema.Unknown), permissions: Schema.optionalKey(Grants), steps: Schema.optionalKey(Schema.Array(Step)) });
const Events = Schema.Union([Schema.String, Schema.Array(Schema.String), Schema.Record(Schema.String, Schema.Unknown)]);
const Workflow = Schema.Struct({ on: Events, permissions: Schema.optionalKey(Grants), jobs: Schema.Record(Schema.String, Job) }).annotate({
    description: 'Workflow file fields of GitHub workflow syntax: on, permissions, jobs.<id>.runs-on, if, permissions, steps[*].uses, run, if, with, env',
});

interface Located {
    readonly at: string;
    readonly file: string;
    readonly triggers: readonly string[];
    readonly runner: unknown;
    readonly grants: Record.ReadonlyRecord<string, string>;
    readonly gates: readonly string[];
    readonly earlier: readonly (typeof Step.Type)[];
    readonly step: typeof Step.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKOUT = /^actions\/checkout@/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const triggered = (on: typeof Events.Type): readonly string[] => [on].flat().flatMap((event) => (Predicate.isString(event) ? [event] : Record.keys(event)));

const matched = (pattern: RegExp, text: string | undefined) => text !== undefined && pattern.test(text);

const fetched = ({ earlier }: Located) => earlier.some(({ uses, with: given = {} }) => matched(CHECKOUT, uses) && given['fetch-depth'] === 0);

const located = (file: string, { on, permissions, jobs }: typeof Workflow.Type): readonly Located[] =>
    Record.collect(jobs, (job, stated) =>
        (stated.steps ?? []).map((step, index, steps) => ({
            at: [file, job, step.uses ?? step.run].filter(Predicate.isNotUndefined).join(' '),
            file,
            triggers: triggered(on),
            runner: stated['runs-on'],
            grants: [stated.permissions, permissions].find(Schema.is(Granted)) ?? {},
            gates: [stated.if, step.if].filter(Predicate.isNotUndefined).map(String),
            earlier: steps.slice(0, index),
            step,
        })),
    ).flat();

// --- [SERVICES] ------------------------------------------------------------------------

class Workflows extends Context.Service<Workflows>()('@rasm/test-nx/delivery/workflows', {
    make: Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = path.join(workspaceRoot, '.github/workflows');
        const names = (yield* fs.readDirectory(folder)).filter((name) => ['.yml', '.yaml'].includes(path.extname(name)));
        const files = yield* Effect.forEach(names, (name) =>
            fs.readFileString(path.join(folder, name)).pipe(
                Effect.map(Yaml.parse),
                Effect.flatMap(Schema.decodeUnknownEffect(Workflow)),
                Effect.map((workflow) => [name, workflow] as const),
            ),
        );
        return { files: Record.fromEntries(files), steps: files.flatMap(([name, workflow]) => located(name, workflow)) };
    }),
}) {
    static readonly live = Layer.provideMerge(Layer.effect(this, this.make), layer);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { fetched, type Located, matched, Workflows };
