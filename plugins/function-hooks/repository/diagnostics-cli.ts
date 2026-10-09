// biome-ignore-all lint/correctness/noNodejsModules: Node owns stdin, process output, and cancellation signals for this CLI

import process from 'node:process';
import { text } from 'node:stream/consumers';
import { bind, decoded, fault, ok, rendered } from '../composition.ts';
import { diagnostics } from './diagnostics.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const controller = new AbortController();
const cancel = (): void => controller.abort();
process.on('SIGINT', cancel);
process.on('SIGTERM', cancel);
const request = decoded<{ readonly root: string; readonly paths: readonly string[] }>('diagnostic request', await text(process.stdin).then(ok, (cause: unknown) => fault<string>({ kind: 'unread', subject: 'stdin', cause })));
const result = await bind(request, ({ root, paths }) => diagnostics(root, paths, controller.signal));
if (result.kind === 'ok') {
    process.stdout.write(JSON.stringify(result.value));
} else {
    process.stderr.write(`${rendered(result.faults)}\n`);
    process.exitCode = 1;
}
