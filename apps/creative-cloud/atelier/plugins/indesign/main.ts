import indesign from 'adobe:indesign';
import { Contract, host } from '@rasm/indesign/host';
import { plugin } from '@rasm/uxp/entrypoints';
import { Link } from '@rasm/uxp/link';
import { typesetting } from '#/typesetting';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { server, application, local } = host(indesign);

plugin((link) => Link.dial(Contract, server, { ...link, application }), { commands: {}, panels: { typesetting: typesetting(local) } });
