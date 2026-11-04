#!/usr/bin/env node

import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/automation/node';
import { Array, Config, Console, DateTime, Effect, Formatter, Layer, Logger, MutableRef, Option, Record, References } from 'effect';
import { Command } from 'effect/cli';
import { SecureRandom } from 'effect/crypto';
import effect from 'effect/package.json' with { type: 'json' };
import { cli } from '#/cli';
import { begun, counted, declared, type Phase, Run, type Tally } from '#/log';
import { since } from '#/query';

// --- [COMPOSITION] ---------------------------------------------------------------------

const atelier = Automation.layer(Config.String('ATELIER_PATH'));
const root = Config.String('HARNESS_LOGS');
const logged = Effect.fnUntraced(function* (host: string) {
    const { run, folder, file } = yield* begun(host, root);
    const current = MutableRef.make<Phase>('run');
    const tally = MutableRef.make<Tally>({ cases: { ran: 0, failed: 0, notRun: 0 }, signatures: [], instances: [] });
    const line = Logger.make(({ message, logLevel, fiber, date }) => {
        const stamp = Option.match(fiber.getRef(Run.Instance), { onNone: () => ({}), onSome: ({ pid, started }) => ({ pid, started: DateTime.formatIso(started) }) });
        const text = Array.ensure(message).join(' ');
        const entry = { at: date.toISOString(), level: logLevel, message: text, phase: MutableRef.get(current), ...declared(text, { ...stamp, ...fiber.getRef(References.CurrentLogAnnotations) }) };
        const json = Formatter.formatJson(entry);
        MutableRef.update(tally, counted(entry));
        fiber.getRef(Console.Console).log(json);
        return json;
    });
    const summary = Effect.gen(function* () {
        const held = MutableRef.get(tally);
        const signatures = yield* Array.match(held.signatures, { onEmpty: () => Effect.succeedNone, onNonEmpty: (signed) => Effect.asSome(since(root, host, signed)) });
        yield* Console.log(Formatter.formatJson({ run, log: file, cases: held.cases, ...Record.getSomes({ signatures }), instances: held.instances }));
    });
    yield* Effect.addFinalizer(() => Effect.orDie(summary));
    return Layer.mergeAll(Layer.succeed(Run, { folder, phase: current }), Logger.layer([Logger.toFile(line, file)]));
}, Layer.unwrap);

cli(root, (host) => Layer.mergeAll(atelier, logged(host))).pipe(Command.run({ version: effect.version }), Effect.provide(Layer.mergeAll(layer, SecureRandom.layerWebCrypto)), runMain);
