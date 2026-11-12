export module rasm.extent;

import std;
import rasm.tolerance;

// --- [MODELS] --------------------------------------------------------------------------

export namespace rasm {

struct Extent {
    double lower;
    double upper;

    [[nodiscard]] constexpr auto length() const noexcept -> double { return upper - lower; }

    [[nodiscard]] auto reaches(double value) const noexcept -> bool { return (lower <= value && value <= upper) || within_ulps(value, lower, 4U) || within_ulps(value, upper, 4U); }
};

}
