import type { Project, premierepro, Sequence } from '@adobe/premierepro';
import { Containment } from '@rasm/object-model/host';
import { HostError } from '@rasm/object-model/model';
import type { Render } from '@rasm/object-model/rpcs';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Array, Context, Effect, Layer, Option, Path, Predicate, Schema, Stream } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Module = Schema.declare((module: unknown): module is premierepro => Array.every(['Project', 'Exporter', 'EventManager', 'Constants'] satisfies readonly (keyof premierepro)[], (key) => Predicate.hasProperty(module, key)));
const Guided = Schema.Struct({ guid: Schema.Unknown });

// --- [SERVICES] ------------------------------------------------------------------------

class Premiere extends Context.Service<Premiere, premierepro>()('@rasm/premierepro/uxp/Premiere') {
    static readonly layer = Layer.effect(Premiere)(Uxp.require('premierepro', Module));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const snapshot = Effect.gen(function* () {
    const { Project: projects } = yield* Premiere;
    const project = Option.fromNullishOr(yield* Effect.tryPromise({ try: () => projects.getActiveProject(), catch: HostError.refused('active project') }));
    const sequences = yield* Option.match(project, {
        onNone: () => Effect.succeed(Array.empty<Sequence>()),
        onSome: (open) => Effect.tryPromise({ try: () => open.getSequences(), catch: HostError.refused(`sequences of project ${open.name}`) }),
    });
    const containment: Containment = {
        root: { project: Option.getOrUndefined(project) },
        kinds: {
            project: Containment.kind({ property: (root: { readonly project: Project | undefined }) => root.project }),
            sequence: Containment.kind({ items: () => sequences, key: Guided, owner: () => Option.getOrUndefined(project) }),
        },
    };
    return { containment, sequences };
});
const execute = (code: string) => Effect.flatMap(snapshot, ({ containment }) => Uxp.execute(containment, code));
const render = Effect.fnUntraced(function* ({ target }: Render, written: Pick<File, 'name' | 'nativePath'>) {
    const [{ Exporter }, { containment, sequences }, path] = yield* Effect.all([Premiere, snapshot, Path.Path]);
    const found = yield* Uxp.locate(containment, target);
    const sequence = yield* Effect.fromOption(
        Array.findFirst(sequences, (held) => held === found),
        () => Unmatched.of({ subject: 'sequence', query: Option.some(target.path) }),
    );
    const [time, { width, height }] = yield* Effect.all([
        Effect.tryPromise({ try: () => sequence.getPlayerPosition(), catch: HostError.refused(`player position of sequence ${sequence.name}`) }),
        Effect.tryPromise({ try: () => sequence.getFrameSize(), catch: HostError.refused(`frame size of sequence ${sequence.name}`) }),
    ]);
    const refused = HostError.refused(`exportSequenceFrame of sequence ${sequence.name} to ${written.nativePath}`);
    yield* Effect.tryPromise({ try: () => Exporter.exportSequenceFrame(sequence, time, written.name, `${path.dirname(written.nativePath)}${path.sep}`, width, height), catch: refused }).pipe(Effect.flatMap((exported) => (exported ? Effect.void : refused())));
});
const events = Stream.unwrap(
    Premiere.useSync(({ EventManager, Constants: { ProjectEvent, SequenceEvent } }) =>
        Stream.mergeAll(
            Array.map([ProjectEvent.OPENED, ProjectEvent.ACTIVATED, ProjectEvent.CLOSED, SequenceEvent.ACTIVATED, SequenceEvent.CLOSED], (name) =>
                Uxp.listen(`global listener of ${String(name)}`, { add: (listener) => EventManager.addGlobalEventListener(name, listener), remove: (listener) => EventManager.removeGlobalEventListener(name, listener) }).pipe(Stream.as({ name: String(name), target: Option.none(), detail: Option.none() })),
            ),
            { concurrency: 'unbounded' },
        ),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { events, execute, Premiere, render };
