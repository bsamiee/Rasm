// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import colors from '@radix-ui/colors';
import { Array, Cause, Console, Effect, FileSystem, flow, Path } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HEADER = `"""Radix Colors dark steps as display bytes, written by \`radix.ts\` from \`@radix-ui/colors\`.

Step 1 is the app background, 2 a subtle background, 3 a UI element background, 4 a hovered UI element background, 5 an active or selected UI element background,
6 a subtle border or separator, 7 a UI element border or focus ring, 8 a hovered UI element border, 9 a solid background, 10 a hovered solid background,
11 low-contrast text, and 12 high-contrast text.
"""

from typing import Final

# --- [TYPES] ----------------------------------------------------------------------------

type Rgb = tuple[int, int, int]

# --- [CONSTANTS] ------------------------------------------------------------------------


class Radix:
    """Every dark scale by its published scale and step name."""

`;
const _FOOTER = `


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Radix", "Rgb"]
`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const bytes = (hex: string): string => `(${[...hex.matchAll(/[0-9a-f]{2}/gu)].map(([pair]) => Number.parseInt(pair, 16)).join(', ')})`;
const member = ([step, hex]: readonly [string, string]): string => `    ${step}: Final[Rgb] = ${bytes(hex)}`;

// --- [ENTRY] ---------------------------------------------------------------------------

Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const members = Object.entries(colors)
        .filter(([name]) => name.endsWith('Dark'))
        .flatMap(([, scale]) => Object.entries(scale).map(member));
    yield* fs.writeFileString(path.join(import.meta.dirname, 'radix.py'), `${_HEADER}${members.join('\n')}${_FOOTER}`);
    yield* Console.log(`radix.py: ${members.length} steps`);
}).pipe(
    Effect.tapError((error) => Effect.forEach(Array.ensure(error), (failure) => Console.error(`${failure._tag}: ${failure.message}`))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
