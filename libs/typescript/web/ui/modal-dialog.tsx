import { useAtomSet } from '@effect/atom-react';
import { Number } from 'effect';
import { Atom } from 'effect/reactivity';
import { type ReactNode, useEffect } from 'react';
import { Button, type ButtonProps, Dialog, type DialogProps, DialogTrigger, Heading, Modal, ModalOverlay, type ModalOverlayProps, Text } from 'react-aria-components';

// --- [SERVICES] ------------------------------------------------------------------------

const mounted = Atom.make(0);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Counted = ({ children }: { readonly children: ReactNode }) => {
    const adjust = useAtomSet(mounted);
    useEffect(() => {
        adjust(Number.increment);
        return () => adjust(Number.decrement);
    }, [adjust]);
    return children;
};
const ModalDialog = Object.assign(
    ({ modal, dialog, children, ...overlay }: Omit<ModalOverlayProps, 'children'> & Pick<DialogProps, 'children'> & { readonly modal?: Omit<ModalOverlayProps, 'children'>; readonly dialog?: Omit<DialogProps, 'children'> }) => (
        <ModalOverlay {...overlay}>
            <Counted>
                <Modal {...modal}>
                    <Dialog {...dialog}>{children}</Dialog>
                </Modal>
            </Counted>
        </ModalOverlay>
    ),
    {
        open: Atom.readable((get) => get(mounted)),
        Confirm: ({ label, description, cancel, onConfirm, isDisabled = false, trigger }: { readonly label: string; readonly description: string; readonly cancel: string; readonly onConfirm: () => void; readonly isDisabled?: boolean; readonly trigger?: Omit<ButtonProps, 'children' | 'isDisabled'> }) => (
            <DialogTrigger>
                <Button {...trigger} isDisabled={isDisabled}>
                    {label}
                </Button>
                <ModalDialog dialog={{ role: 'alertdialog' }}>
                    <Heading slot="title">{label}</Heading>
                    <Text elementType="p" slot="description">
                        {description}
                    </Text>
                    <div>
                        <Button autoFocus slot="close">
                            {cancel}
                        </Button>
                        <Button onPress={onConfirm} slot="close">
                            {label}
                        </Button>
                    </div>
                </ModalDialog>
            </DialogTrigger>
        ),
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ModalDialog };
