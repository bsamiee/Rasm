import { Button, DialogTrigger, Heading } from 'react-aria-components';
import { fn } from 'storybook/test';
import preview from '#/.storybook/preview';
import { ModalDialog } from '#/modal-dialog.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({ component: ModalDialog, args: { isDismissable: true, children: <Heading slot="title">Settings</Heading> } });

const Open = meta.story({ args: { defaultOpen: true } });
const Triggered = meta.story({
    render: (args) => (
        <DialogTrigger>
            <Button>Open settings</Button>
            <ModalDialog {...args} />
        </DialogTrigger>
    ),
});
const Confirm = meta.story({ render: () => <ModalDialog.Confirm cancel="Keep" description="Deletion is permanent" label="Delete item" onConfirm={fn<() => void>()} /> });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Confirm, Open, Triggered };
