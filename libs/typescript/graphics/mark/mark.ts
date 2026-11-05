import { Cause, Effect, FileSystem, Stream } from 'effect';
import { Hex } from 'effect/encoding';
import { ChildProcess } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Ink {
    readonly color: string;
    readonly opacity?: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPAQUE = 0xff;

// --- [OPERATIONS] ----------------------------------------------------------------------

const inkColor = ({ color, opacity = 1 }: Ink & { readonly color: `#${string}` }) => `${color}${Hex.encode(Uint8Array.of(Math.round(opacity * OPAQUE)))}`;
const paint = Effect.fn('paint')(function* (master: string, { color, opacity = 1 }: Ink, width?: number) {
    const fs = yield* FileSystem.FileSystem;
    const stylesheet = yield* fs.makeTempFileScoped({ suffix: '.css' });
    yield* fs.writeFileString(stylesheet, `svg { color: ${color}; opacity: ${opacity}; }`);
    const [tool, sizing] = width === undefined ? (['usvg', []] as const) : (['resvg', ['--width', `${width}`]] as const);
    const handle = yield* ChildProcess.make(tool, [...sizing, '--stylesheet', stylesheet, master, '-c'], { stdin: 'ignore', stderr: 'inherit' });
    const [source, code] = yield* Effect.all([Stream.mkUint8Array(handle.stdout), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? source : yield* new Cause.UnknownError(code, `${tool} over ${master} exited with code ${code}`);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Ink, inkColor, paint };
