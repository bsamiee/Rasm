import { Context } from 'effect';

// --- [SERVICES] ------------------------------------------------------------------------

class Dom extends Context.Service<Dom, typeof import('adobe:indesign')>()('@rasm/indesign/dom') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dom };
