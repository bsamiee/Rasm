// --- [IMPORTS] -------------------------------------------------------------------------

import { deploy, main } from '@rasm/creative-cloud-server/uxp';
import { HOSTS } from '@rasm/creative-cloud-server/values';
import { Option } from 'effect';
import { Command } from 'effect/unstable/cli';
import { bridge } from './uxp.config.ts';

// --- [ENTRY] ---------------------------------------------------------------------------

main(Command.make('automation').pipe(Command.withSubcommands([Command.make('deploy', {}, () => deploy(HOSTS.photoshop, bridge, import.meta.dirname, Option.none()))])), bridge.manifest.version);
