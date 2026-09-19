// --- [IMPORTS] -------------------------------------------------------------------------

import { FileSystem, Layer, Schema, Struct } from 'effect';
import { Tool, Toolkit } from 'effect/unstable/ai';
import { register } from './contract.ts';
import { compile, PaletteError, PaletteFile, read } from './palette.ts';

// --- [TOOLS] ---------------------------------------------------------------------------

const tools = Toolkit.make(
    Tool.make('palette_compile', {
        description:
            'Compiles a typed palette to Adobe Swatch Exchange and reads it back. Preserves groups, process/global/spot identity, channel models, and exact Unicode names. Gray is a normalized white fraction from 0 (black) to 1 (white). ASE cannot represent swatch tints; every affected color is reported before writing.',
        parameters: PaletteFile,
        success: PaletteFile,
        failure: PaletteError,
        failureMode: 'return',
        dependencies: [FileSystem.FileSystem],
    }).annotate(Tool.Readonly, false),
    Tool.make('palette_read', {
        description:
            'Reads Adobe Swatch Exchange into the canonical palette. Retains groups, process/global/spot identity, model components, and exact Unicode names. Rejects malformed framing or groups. Gray is a normalized white fraction from 0 (black) to 1 (white).',
        parameters: Schema.Struct(Struct.pick(PaletteFile.fields, ['path', 'format'])),
        success: PaletteFile,
        failure: PaletteError,
        failureMode: 'return',
        dependencies: [FileSystem.FileSystem],
    }).annotate(Tool.Readonly, true),
);

const layer: Layer.Layer<never, never, FileSystem.FileSystem> = register(tools).pipe(
    Layer.provide(tools.toLayer({ [tools.tools.palette_compile.name]: ({ palette, path }) => compile(palette, path), [tools.tools.palette_read.name]: ({ path }) => read(path) })),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
