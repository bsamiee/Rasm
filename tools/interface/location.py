"""Site and moment every application places its sun and geolocation at: Houston at local noon of the March equinox.

Latitude and longitude are WGS84 degrees, elevation is meters above mean sea level, and north is degrees counter-clockwise from +Y.
"""

from datetime import datetime, timedelta, UTC
from typing import Final
from zoneinfo import ZoneInfo

# --- [CONSTANTS] ------------------------------------------------------------------------

LATITUDE: Final = 29.7632836
LONGITUDE: Final = -95.3632715
ELEVATION: Final = 11.0
NORTH: Final = 0.0
MOMENT: Final = datetime(2026, 3, 20, 12, tzinfo=ZoneInfo("America/Chicago"))
DAYLIGHT: Final = MOMENT.dst() or timedelta()
OFFSET: Final = MOMENT.replace(tzinfo=UTC) - MOMENT - DAYLIGHT
CRS: Final = f"EPSG:{(32600 if LATITUDE >= 0 else 32700) + int((LONGITUDE + 180) // 6) + 1}"

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CRS", "DAYLIGHT", "ELEVATION", "LATITUDE", "LONGITUDE", "MOMENT", "NORTH", "OFFSET"]
