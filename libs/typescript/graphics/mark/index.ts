import { Cause, Effect, FileSystem, Stream } from 'effect';
import { Hex } from 'effect/encoding';
import { ChildProcess } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fill {
    readonly color: string;
    readonly opacity?: number;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fillColor = ({ color, opacity = 1 }: Fill & { readonly color: `#${string}` }) => {
    const opaque = 0xff;
    return `${color}${Hex.encode(Uint8Array.of(Math.round(opacity * opaque)))}`;
};
const paint = Effect.fn('paint')(function* (master: string, { color, opacity = 1 }: Fill, width?: number) {
    const fs = yield* FileSystem.FileSystem;
    const stylesheet = yield* fs.makeTempFileScoped({ suffix: '.css' });
    yield* fs.writeFileString(stylesheet, `svg { color: ${color}; opacity: ${opacity}; }`);
    const [tool, sizing] = width === undefined ? (['usvg', []] as const) : (['resvg', ['--width', `${width}`]] as const);
    const handle = yield* ChildProcess.make(tool, [...sizing, '--stylesheet', stylesheet, master, '-c'], { stdin: 'ignore', stderr: 'inherit' });
    const [source, code] = yield* Effect.all([Stream.mkUint8Array(handle.stdout), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? source : yield* new Cause.UnknownError(code, `${tool} over ${master} exited with code ${code}`);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Fill, fillColor, paint };
