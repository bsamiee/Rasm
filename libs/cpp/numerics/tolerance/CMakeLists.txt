cmake_minimum_required(VERSION 4.5)

project(
    rasm-tolerance
    VERSION 0.0.0
    COMPAT_VERSION 0.0.0
    SPDX_LICENSE MIT
    DESCRIPTION "Floating-point comparison by units in the last place"
    HOMEPAGE_URL "https://github.com/bsamiee/Rasm"
    LANGUAGES CXX
)

# --- [DEPENDENCIES] ---------------------------------------------------------------------

include(GNUInstallDirs)

# --- [TARGETS] --------------------------------------------------------------------------

add_library(rasm-tolerance STATIC)
add_library(rasm-tolerance::tolerance ALIAS rasm-tolerance)
set_target_properties(rasm-tolerance PROPERTIES EXPORT_NAME tolerance)
target_compile_features(rasm-tolerance PUBLIC cxx_std_26)
target_sources(rasm-tolerance PRIVATE within.cpp PUBLIC FILE_SET HEADERS BASE_DIRS include FILES include/rasm/tolerance.hpp)
if(CMAKE_CXX_SCAN_FOR_MODULES)
    target_sources(rasm-tolerance PUBLIC FILE_SET CXX_MODULES FILES tolerance.cppm face.cppm)
    set(modules FILE_SET CXX_MODULES DESTINATION "${CMAKE_INSTALL_DATADIR}/rasm-tolerance/modules")
endif()

# --- [INSTALL] --------------------------------------------------------------------------

install(TARGETS rasm-tolerance EXPORT rasm-tolerance FILE_SET HEADERS ${modules})
install(PACKAGE_INFO rasm-tolerance EXPORT rasm-tolerance CXX_MODULES_DIRECTORY modules)
install(SBOM rasm-tolerance EXPORTS rasm-tolerance)
