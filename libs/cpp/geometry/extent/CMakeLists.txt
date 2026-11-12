cmake_minimum_required(VERSION 4.5)

project(rasm-extent VERSION 0.2.0 COMPAT_VERSION 0.1.0 SPDX_LICENSE MIT DESCRIPTION "Closed interval on the real line" HOMEPAGE_URL "https://github.com/bsamiee/Rasm" LANGUAGES CXX)

# --- [DEPENDENCIES] ---------------------------------------------------------------------

include(GNUInstallDirs)
if(PROJECT_IS_TOP_LEVEL)
    find_package(rasm-tolerance CONFIG REQUIRED)
endif()

# --- [TARGETS] --------------------------------------------------------------------------

add_library(rasm-extent)
add_library(rasm-extent::extent ALIAS rasm-extent)
set_target_properties(rasm-extent PROPERTIES EXPORT_NAME extent)
target_compile_features(rasm-extent PUBLIC cxx_std_26)
target_sources(rasm-extent PUBLIC FILE_SET CXX_MODULES FILES extent.cppm)
target_link_libraries(rasm-extent PUBLIC rasm-tolerance::tolerance)

# --- [INSTALL] --------------------------------------------------------------------------

install(TARGETS rasm-extent EXPORT rasm-extent FILE_SET CXX_MODULES DESTINATION "${CMAKE_INSTALL_DATADIR}/rasm-extent/modules")
install(PACKAGE_INFO rasm-extent EXPORT rasm-extent CXX_MODULES_DIRECTORY modules)
install(SBOM rasm-extent EXPORTS rasm-extent)
