import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const base = 'pdfjs';
const directories = { cMapUrl: 'cmaps', iccUrl: 'iccs', standardFontDataUrl: 'standard_fonts', wasmUrl: 'wasm' } as const satisfies { readonly [K in keyof DocumentInitParameters]?: string };

// --- [EXPORTS] -------------------------------------------------------------------------

export { base, directories };
