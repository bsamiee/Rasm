module;

#include <fmt/format.h>

module rasm.tolerance;

// --- [OPERATIONS] ----------------------------------------------------------------------

namespace rasm {

auto described(double left, double right) -> std::string { return fmt::format("{} within tolerance of {}: {}", left, right, within_ulps(left, right, 4U)); }

}
