// biome-ignore-all lint/style/noMagicNumbers: the seed table and the lightness ladder are the palette's numeric source
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import Color from 'colorjs.io';
import { Array, Cause, Console, Effect, FileSystem, flow, Path } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Family {
    readonly family: string;
    readonly hue: number;
    readonly lightness: number;
    readonly chroma: number;
    readonly drift: readonly [dark: number, light: number];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const GRAYS = [17, 26, 33, 50, 57, 62, 71, 82, 96, 123, 180, 238] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const lightnessOf = (gray: number): number => new Color(`rgb(${gray} ${gray} ${gray})`).get('oklab.l');
const hues = (seed: Family): readonly Color[] => {
    const [top, gamutShare, solid] = [0.935, 0.97, seed.lightness];
    const fills = GRAYS.slice(0, 8).map(lightnessOf);
    const lightnesses = solid >= 0.8 ? [...fills, solid, solid - 0.03, Math.min(0.93, solid + 0.05), top] : [...fills, solid, solid + 0.035, Math.max(0.8, solid + 0.06), top];
    return Array.zip(lightnesses, [0.1, 0.15, 0.34, 0.48, 0.56, 0.6, 0.64, 0.74, 1, 0.95, 0.8, 0.32]).map(([lightness, share]) => {
        const [end, drift] = lightness < solid ? [lightnessOf(GRAYS[0]), seed.drift[0]] : [top, seed.drift[1]];
        const hue = Color.util.mapRange([solid, end], [seed.hue, seed.hue + drift], lightness);
        const ceiling = new Color('oklch', [lightness, (share * seed.chroma) / gamutShare, hue]).toGamut({ space: 'srgb', method: 'oklch.c', jnd: 0 });
        return new Color('oklch', [lightness, gamutShare * ceiling.get('oklch.c'), hue]);
    });
};
const hex = (color: Color): string => color.to('srgb').toString({ format: 'hex', collapse: false }).slice(1);
const source = (scales: readonly (readonly [family: string, steps: readonly Color[]])[]): string => `"""Palette steps as display bytes, written by \`palette.ts\` from its seed table."""

from enum import Enum

# --- [TYPES] ----------------------------------------------------------------------------


class Palette(Enum):
    """Twelve steps of every family as display bytes, darkest first."""

${scales.map(([family, steps]) => `    ${family.toUpperCase()} = bytes.fromhex("${steps.map(hex).join(' ')}")`).join('\n')}

    def __getitem__(self, step: int) -> tuple[int, int, int]:
        """Display bytes of the family's step, 1 the darkest."""
        red, green, blue = self.value[3 * step - 3 : 3 * step]
        return (red, green, blue)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Palette"]
`;

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const seeds = [
        { family: 'rose', hue: 3, lightness: 0.7, chroma: 0.15, drift: [4, -4] },
        { family: 'crimson', hue: 13, lightness: 0.53, chroma: 0.18, drift: [0, 0] },
        { family: 'red', hue: 29, lightness: 0.62, chroma: 0.17, drift: [-6, 4] },
        { family: 'orange', hue: 53, lightness: 0.74, chroma: 0.14, drift: [-8, 6] },
        { family: 'yellow', hue: 88, lightness: 0.85, chroma: 0.145, drift: [-16, 3] },
        { family: 'lime', hue: 121, lightness: 0.79, chroma: 0.15, drift: [-10, -4] },
        { family: 'green', hue: 144, lightness: 0.67, chroma: 0.145, drift: [6, -4] },
        { family: 'emerald', hue: 152, lightness: 0.57, chroma: 0.14, drift: [0, 0] },
        { family: 'teal', hue: 178, lightness: 0.71, chroma: 0.11, drift: [4, -4] },
        { family: 'cyan', hue: 213, lightness: 0.76, chroma: 0.115, drift: [4, -6] },
        { family: 'cerulean', hue: 227, lightness: 0.54, chroma: 0.105, drift: [0, 0] },
        { family: 'blue', hue: 259, lightness: 0.62, chroma: 0.16, drift: [6, -8] },
        { family: 'ultramarine', hue: 280, lightness: 0.56, chroma: 0.175, drift: [2, -6] },
        { family: 'violet', hue: 307, lightness: 0.63, chroma: 0.14, drift: [-2, 0] },
        { family: 'magenta', hue: 339, lightness: 0.66, chroma: 0.18, drift: [2, -4] },
        { family: 'slate', hue: 241, lightness: 0.62, chroma: 0.038, drift: [0, 0] },
    ] as const satisfies readonly Family[];
    const scales = [...seeds.map((seed) => [seed.family, hues(seed)] as const), ['neutral', GRAYS.map((gray) => new Color(`rgb(${gray} ${gray} ${gray})`))] as const];
    yield* fs.writeFileString(path.join(import.meta.dirname, 'palette.py'), source(scales));
}).pipe(Effect.tapCause(flow(Cause.pretty, Console.error)), Effect.provide(NodeServices.layer), NodeRuntime.runMain({ disableErrorReporting: true }));
