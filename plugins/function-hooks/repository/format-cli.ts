// biome-ignore-all lint/correctness/noNodejsModules: Node owns CLI arguments, process I/O, and repository path resolution

import { resolve } from 'node:path';
import process from 'node:process';
import { text } from 'node:stream/consumers';
import { parseArgs } from 'node:util';
import { readJsonFile } from 'nx/src/devkit-exports.js';
import { bind, decoded, fault, ok, type Result, rendered } from '../composition.ts';
import { formatEdited, type WriterProject } from './format.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const controller = new AbortController();
process.once('SIGINT', () => controller.abort());
process.once('SIGTERM', () => controller.abort());
const { values } = parseArgs({ options: { all: { type: 'boolean' } } });
const request: Result<{ readonly root: string; readonly paths: readonly string[] | 'all' }> = values.all ? ok({ root: process.cwd(), paths: 'all' }) : decoded('format request', await text(process.stdin).then(ok, (cause: unknown) => fault<string>({ kind: 'unread', subject: 'stdin', cause })));
const result = await bind(request, async ({ root, paths }) => {
    const project = readJsonFile<{ readonly name: string; readonly nx: { readonly targets: { readonly format: { readonly metadata: { readonly writers: WriterProject['writers'] } } } } }>(resolve(root, 'package.json'));
    return ok(await formatEdited(root, paths, controller.signal, { name: project.name, writers: project.nx.targets.format.metadata.writers }));
});
if (result.kind === 'ok') {
    process.stdout.write(`${values.all ? result.value.join('\n') : JSON.stringify(result.value)}\n`);
    process.exitCode = values.all && result.value.length > 0 ? 1 : 0;
} else {
    process.stderr.write(`${rendered(result.faults)}\n`);
    process.exitCode = 1;
}
