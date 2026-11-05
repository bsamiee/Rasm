#!/usr/bin/env node

import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/applications/node';
import { Config, Effect, Layer, String } from 'effect';
import { Command } from 'effect/cli';
import { SecureRandom } from 'effect/crypto';
import { ChildProcess } from 'effect/process';
import { cli } from '#/cli';
import { printed } from '#/instance';

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.all([Effect.map(printed('git', ['rev-parse', 'HEAD']), String.trim), Config.String('HARNESS_LOGS')]).pipe(
    Effect.flatMap(([version, logs]) => Command.run(cli(version, logs, Automation.layer(ChildProcess.make('watch-apps', ['serve'], { stderr: 'inherit' }))), { version })),
    Effect.provide(Layer.mergeAll(layer, SecureRandom.layerWebCrypto)),
    runMain,
);
