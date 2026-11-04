#!/usr/bin/env node

import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/automation/node';
import { Config, Effect, Layer, Logger, String } from 'effect';
import { Command } from 'effect/cli';
import { SecureRandom } from 'effect/crypto';
import { cli } from '#/cli';
import { printed } from '#/instance';

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.map(printed('git', ['rev-parse', 'HEAD']), String.trim).pipe(
    Effect.flatMap((version) =>
        Command.run(
            cli(Config.String('HARNESS_LOGS'), Automation.layer(Config.String('ATELIER_PATH')), (written, file) => Logger.layer([Logger.toFile(Logger.make(written), file)])),
            { version },
        ),
    ),
    Effect.provide(Layer.mergeAll(layer, SecureRandom.layerWebCrypto)),
    runMain,
);
