#!/usr/bin/env node

import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/applications/node';
import { Array, Config, Effect, FileSystem, Layer, Option, Path, String } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { SecureRandom } from 'effect/crypto';
import { ChildProcess } from 'effect/process';
import { RESULTS } from '#/cases/results';
import { CASES } from '#/cases/schema';
import { judged, proved, recompared } from '#/cases/verdict';
import { HarnessError } from '#/harness-error';
import { type Cased, type Harness, installations, type Surveyed } from '#/session/harness';
import { printed } from '#/session/instance';
import { Run, ran } from '#/session/run';
import { traced } from '#/trace/query';

// --- [OPERATIONS] ----------------------------------------------------------------------

const casesUnder = Effect.fn('casesUnder')(function* (target: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { type } = yield* fs.stat(target);
    const found = type === 'Directory' ? (yield* fs.glob(`**/*${CASES}`, { root: target })).map((file) => path.join(target, file)) : [target];
    return found.map((file) => path.resolve(file));
});
const targets = (paths: readonly string[]) => Effect.map(Effect.forEach(paths, casesUnder), (files) => Array.dedupe(files.flat()).toSorted());

// --- [COMPOSITION] ---------------------------------------------------------------------

const cases = Argument.Path('target', { mustExist: true }).pipe(Argument.atLeast(1), Argument.withDescription(`Product folder whose **/*${CASES} files run, or one case file, results written beside each as <name>.<version>${RESULTS}`));
const cli = <A, E, R>(commit: string, logs: string, automation: Layer.Layer<A, E, R>) => {
    const provided = (name: string) => Layer.mergeAll(automation, Run.layer(name, logs, commit));
    const compare = <S extends Surveyed, C extends Cased, X, P>(name: string, hosted: Effect.Effect<Harness<S, C, X, P>, HarnessError>) =>
        Command.make(
            'compare',
            { targets: cases, module: Flag.Path('module', { mustExist: true }).pipe(Flag.optional, Flag.withDescription('Module compared in place of the batch module, a distilled module among them')) },
            Effect.fn('compare')(function* ({ targets: paths, module }) {
                const [host, path, files] = yield* Effect.all([hosted, Path.Path, targets(paths)]);
                const override = Option.map(module, (held) => path.resolve(held));
                const verdicts = yield* Effect.forEach(files, (file) => Effect.annotateLogs(recompared(name, host, file, override), { proof: file }));
                yield* judged(verdicts.flat(), name);
            }, ran),
        ).pipe(Command.provide(provided(name)));
    const log = (name: string) => Command.make('log', {}, () => traced(logs, name)).pipe(Command.withDescription(`Prints the current ${name} run's records as JSON Lines with reports linked since it ended, the newest ${name} run of this session or else the newest ${name} run`));
    const subcommand = <N extends string, S extends Surveyed, C extends Cased, X, P>(name: N, load: () => Promise<{ readonly [K in N]: Effect.Effect<Harness<S, C, X, P>> }>) => {
        const hosted = Effect.flatMap(Effect.tryPromise({ try: load, catch: HarnessError.refused(name) }), (loaded) => loaded[name]);
        return Command.make(
            name,
            {
                targets: cases,
                release: Flag.String('release').pipe(Flag.atLeast(0), Flag.withDescription('Version prefix an installation reports, repeated, the newest installation when absent')),
                only: Flag.String('only').pipe(Flag.atLeast(0), Flag.withDescription('Case name to run, repeated')),
            },
            Effect.fn('cases')(function* ({ targets: paths, release, only }) {
                const host = yield* hosted;
                const [files, chosen] = yield* Effect.all([targets(paths), Effect.flatMap(host.installed, (rows) => installations(rows, release, name))]);
                const verdicts = yield* Effect.forEach(Array.cartesian(files, chosen), ([file, installed]) => Effect.annotateLogs(proved(name, host, installed, file, only), { version: installed.version, proof: file }));
                yield* judged(verdicts, name);
            }, ran),
        ).pipe(Command.provide(provided(name)), Command.withSubcommands([compare(name, hosted), log(name)]));
    };
    return Command.make('harness').pipe(
        Command.withSubcommands([
            subcommand('acrobat', () => import('#/hosts/adobe/acrobat/host')),
            subcommand('blender', () => import('#/hosts/blender/host')),
            subcommand('illustrator', () => import('#/hosts/adobe/illustrator/host')),
            subcommand('indesign', () => import('#/hosts/adobe/indesign/host')),
            subcommand('photoshop', () => import('#/hosts/adobe/photoshop/host')),
            subcommand('rhino', () => import('#/hosts/rhino/host')),
        ]),
    );
};

Effect.all([Effect.map(printed('git', ['rev-parse', 'HEAD']), String.trim), Config.String('HARNESS_LOGS')]).pipe(
    Effect.flatMap(([version, logs]) => Command.run(cli(version, logs, Automation.layer(ChildProcess.make('watch-apps', ['serve'], { stderr: 'inherit' }))), { version })),
    Effect.provide(Layer.mergeAll(layer, SecureRandom.layerWebCrypto)),
    runMain,
);
