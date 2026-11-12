cmake_minimum_required(VERSION 4.5)

project(
    rasm-tolerance
    VERSION 0.1.0
    COMPAT_VERSION 0.1.0
    SPDX_LICENSE MIT
    DESCRIPTION "Floating-point comparison by units in the last place"
    HOMEPAGE_URL "https://github.com/bsamiee/Rasm"
    LANGUAGES CXX
)

# --- [DEPENDENCIES] ---------------------------------------------------------------------

include(GNUInstallDirs)
find_package(fmt CONFIG REQUIRED)

# --- [TARGETS] --------------------------------------------------------------------------

add_library(rasm-tolerance)
add_library(rasm-tolerance::tolerance ALIAS rasm-tolerance)
set_target_properties(rasm-tolerance PROPERTIES EXPORT_NAME tolerance)
target_compile_features(rasm-tolerance PUBLIC cxx_std_26)
target_sources(rasm-tolerance PRIVATE tolerance.cpp within.cpp PUBLIC FILE_SET CXX_MODULES FILES tolerance.cppm PUBLIC FILE_SET HEADERS BASE_DIRS include FILES include/rasm/tolerance.hpp)
target_link_libraries(rasm-tolerance PRIVATE fmt::fmt)

# --- [INSTALL] --------------------------------------------------------------------------

install(TARGETS rasm-tolerance EXPORT rasm-tolerance FILE_SET CXX_MODULES DESTINATION "${CMAKE_INSTALL_DATADIR}/rasm-tolerance/modules" FILE_SET HEADERS)
install(PACKAGE_INFO rasm-tolerance EXPORT rasm-tolerance CXX_MODULES_DIRECTORY modules)
install(SBOM rasm-tolerance EXPORTS rasm-tolerance)
