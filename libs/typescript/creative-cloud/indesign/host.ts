import { Context } from 'effect';

// --- [SERVICES] ------------------------------------------------------------------------

class Host extends Context.Service<Host, { readonly indesign: typeof import('adobe:indesign'); readonly fs: typeof import('adobe:fs'); readonly uxp: typeof import('adobe:uxp').default }>()('@rasm/indesign/host') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
