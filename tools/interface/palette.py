"""Palette steps as display bytes, written by `palette.ts` from its seed table."""

from enum import Enum

# --- [TYPES] ----------------------------------------------------------------------------


class Palette(Enum):
    """Twelve steps of every family as display bytes, darkest first."""

    ROSE = bytes.fromhex("170e10 231618 35161d 4f202c 5c2333 642637 702d40 82334a e97295 f180a1 fd9cb9 fee0e9")
    CRIMSON = bytes.fromhex("180e0f 251516 391219 551b25 631d2a 6b1f2e 782635 8c293d bc2c4d c53e59 fd9fa9 fee1e3")
    RED = bytes.fromhex("180e0e 251514 381413 541e1c 61201e 6a2320 772a26 8a2f2a d95446 e26353 fda291 fee2dc")
    ORANGE = bytes.fromhex("160f0c 221712 33190c 4d2611 592a10 612e11 6d3616 7e3d15 ef904f f79d5f f6aa76 fee4d0")
    YELLOW = bytes.fromhex("15100a 20190f 2f1d02 452d02 4e3303 543803 5f4004 6c4b06 f5c84e eabe4d fcdb80 f5e9c7")
    LIME = bytes.fromhex("11120a 1a1b0f 222303 323502 393d03 3e4203 464c05 515806 adc950 bad362 c5d97d e7eeca")
    GREEN = bytes.fromhex("0d130e 131d15 0e2714 143c1d 14441f 164a22 1d5428 20612c 57ac58 67b665 92d18b daf2d4")
    EMERALD = bytes.fromhex("0d130e 131d15 0d2715 123c20 104424 114a27 17542e 166234 1c8e4c 349858 84d39a d4f3db")
    TEAL = bytes.fromhex("0c1311 111d1b 082722 083b34 04433b 044940 065349 086054 42b8a1 57c2ac 7cd1bc d3f2e8")
    CYAN = bytes.fromhex("0b1214 111c1f 05262d 033944 04414d 044653 06505e 085c6c 42c4dc 59cfe5 77d5e6 cef1f6")
    CERULEAN = bytes.fromhex("0c1215 121c20 0c242e 103746 0e3f51 0e4458 144e63 135a73 0e7a9c 2484a6 81c9e8 d3effb")
    BLUE = bytes.fromhex("0e1118 141a25 14203b 1d3059 1f3767 213c70 27457e 2b4f93 4684e5 5290ec 91c1fd dbecfe")
    ULTRAMARINE = bytes.fromhex("101019 181926 1d1d3c 2c2b5b 32306a 363473 3e3d81 474597 6462d7 6d6fde acb9fd e3e9fe")
    VIOLET = bytes.fromhex("121016 1c1822 261b33 3a284d 432d59 493161 53396d 61417f 9f70cb a97cd3 d1abf6 f0e3fe")
    MAGENTA = bytes.fromhex("170e13 23151e 34142a 4e1d3f 5a2049 61224f 6d295a 7f2e69 d65fb6 df6dc0 f898df fedef6")
    SLATE = bytes.fromhex("101113 181a1c 1c2227 2a333b 2f3b43 344049 3c4953 455460 728a9b 7e94a5 adc1d0 e3ebf1")
    NEUTRAL = bytes.fromhex("111111 1a1a1a 212121 323232 393939 3e3e3e 474747 525252 606060 7b7b7b b4b4b4 eeeeee")

    def __getitem__(self, step: int) -> tuple[int, int, int]:
        """Display bytes of the family's step, 1 the darkest."""
        red, green, blue = self.value[3 * step - 3 : 3 * step]
        return (red, green, blue)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Palette"]
