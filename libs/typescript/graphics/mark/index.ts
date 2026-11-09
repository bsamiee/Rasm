import { Hex } from 'effect/encoding';

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Fill, fillColor };
