"""Applies the interface independently to each installed product among Illustrator, Photoshop, InDesign, and Acrobat."""

import anyio

from interface.adobe import acrobat, illustrator, indesign, photoshop
from interface.adobe.session import converged
from interface.host import Applied, environment, Error, Failed, Home, Host, outcome

# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Applied | Failed, ...]:
    """Outcome of each installed product, or the failure of the environment variables the products read."""
    if isinstance(variables := environment(Home, host.environ), Error):
        return (outcome(host.app, (variables,)),)
    return tuple(filter(None, await anyio.gather(*(converged(host, variables.home, product) for product in (illustrator.PRODUCT, photoshop.PRODUCT, indesign.PRODUCT, acrobat.PRODUCT)))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
