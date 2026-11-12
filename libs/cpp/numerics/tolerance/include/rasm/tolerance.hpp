#ifndef RASM_TOLERANCE_HPP
#define RASM_TOLERANCE_HPP

#include <string>

// --- [CONSTANTS] -----------------------------------------------------------------------

namespace rasm {

inline constexpr unsigned default_ulps = 4;

}

// --- [OPERATIONS] ----------------------------------------------------------------------

namespace rasm {

[[nodiscard]] auto within(double left, double right, unsigned ulps) noexcept -> bool;

[[nodiscard]] auto labelled(unsigned ulps) -> std::string;

}

#endif
