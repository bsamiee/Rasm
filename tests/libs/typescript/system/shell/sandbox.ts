import { readFile } from 'node:fs/promises';
import { registerHooks } from 'node:module';
import { argv, stdout } from 'node:process';

// --- [COMPOSITION] ---------------------------------------------------------------------

const reached: string[] = [];
const hooks = registerHooks({
    resolve: (specifier, context, nextResolve) => {
        const resolved = nextResolve(specifier, { ...context, conditions: ['browser', ...context.conditions] });
        reached.push(resolved.url);
        return resolved;
    },
});
const { Shell } = await import('@rasm/shell');
hooks.deregister();
const [{ layer }, { Effect, FileSystem }, { getProcessor }] = await Promise.all([import('@effect/platform-node/NodeServices'), import('effect'), import('sh-syntax/sync')]);
const shell = Effect.runSync(Shell.make({ processor: getProcessor(new WebAssembly.Module(await readFile(new URL(import.meta.resolve('sh-syntax/main.wasm'))))) }).pipe(Effect.provideService(FileSystem.FileSystem, FileSystem.makeNoop({})), Effect.provide(layer)));
const [binary = '', source = ''] = argv.slice(2);
const dialect = await Effect.runPromise(shell.dialect({ shell: binary, environment: {}, timeout: '2 minutes' }).pipe(Effect.provide(layer)));
const globals = ['setTimeout', 'clearTimeout', 'setInterval', 'clearInterval', 'setImmediate', 'clearImmediate', 'queueMicrotask', 'fetch'];
globals.forEach((name) => {
    Reflect.deleteProperty(globalThis, name);
});
const script = Effect.runSync(shell.read(source, { dialect }).pipe(Effect.provideService(FileSystem.FileSystem, FileSystem.makeNoop({}))));
stdout.write(JSON.stringify({ reached, left: globals.filter((name) => name in globalThis), syntax: script.syntax }));
