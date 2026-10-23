import preview from '#/.storybook/preview';
import { CustomProperties } from '#/custom-properties';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({ component: CustomProperties, args: { properties: { '--measure': '32ch' }, children: (scope) => <p {...scope}>Text at scoped measure</p> } });

const Scoped = meta.story();

// --- [EXPORTS] -------------------------------------------------------------------------

export { Scoped };
