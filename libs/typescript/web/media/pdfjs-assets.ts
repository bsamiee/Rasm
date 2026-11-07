import type { getDocument } from 'pdfjs-dist';

// --- [CONSTANTS] -----------------------------------------------------------------------

const pdfjsAssets = { path: 'pdfjs', directories: { cMapUrl: 'cmaps', iccUrl: 'iccs', standardFontDataUrl: 'standard_fonts', wasmUrl: 'wasm' } as const satisfies { readonly [K in keyof NonNullable<Parameters<typeof getDocument>[0]>]?: string } } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfjsAssets };
