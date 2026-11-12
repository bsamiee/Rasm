#include <rasm/tolerance.hpp>

import std;
import rasm.tolerance;

// --- [OPERATIONS] ----------------------------------------------------------------------

namespace rasm {

auto within(double left, double right, unsigned ulps) noexcept -> bool { return within_ulps(left, right, ulps); }

auto labelled(unsigned ulps) -> std::string { return std::to_string(ulps) + " ulps"; }

}
