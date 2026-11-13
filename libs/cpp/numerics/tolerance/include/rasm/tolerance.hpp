#pragma once

// --- [OPERATIONS] ----------------------------------------------------------------------

namespace rasm {

[[nodiscard]] auto within(double left, double right, unsigned ulps) noexcept -> bool;

}
