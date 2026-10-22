import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const assets = { path: 'pdfjs', directories: { cMapUrl: 'cmaps', iccUrl: 'iccs', standardFontDataUrl: 'standard_fonts', wasmUrl: 'wasm' } as const satisfies { readonly [K in keyof DocumentInitParameters]?: string } } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets };
