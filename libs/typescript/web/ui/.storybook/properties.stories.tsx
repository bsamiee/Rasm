import { CustomProperties } from '../properties.tsx';
import preview from './preview.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({ component: CustomProperties, args: { properties: { '--measure': '32ch' }, children: (scope) => <p {...scope}>Text set at the scoped measure</p> } });

const Scoped = meta.story();

// --- [EXPORTS] -------------------------------------------------------------------------

export { Scoped };
