cmake_minimum_required(VERSION 4.5)

project(rasm LANGUAGES CXX)

# --- [POLICY] ---------------------------------------------------------------------------

set(CMAKE_COMPILE_WARNING_AS_ERROR ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_LINK_LIBRARIES_ONLY_TARGETS ON)

# --- [FLAGS] ----------------------------------------------------------------------------

# --- [CHECKS]

set(RASM_FLAGS
    -Wall
    -Wextra
    -Wpedantic
    -Wshadow
    -Wconversion
    -Wdouble-promotion
    -Wold-style-cast
    -Wcast-align
    -Wcast-qual
    -Wnon-virtual-dtor
    -Wnull-dereference
    -Wformat=2
    -Wimplicit-fallthrough
    -Wthread-safety
    -D_LIBCPP_HARDENING_MODE=_LIBCPP_HARDENING_MODE_EXTENSIVE
)

# --- [INSTRUMENTATION]

set(RASM_SANITIZERS "" CACHE STRING "Sanitizers every target compiles and links with")
option(RASM_COVERAGE "Whether every target records source-based coverage to the profile LLVM_PROFILE_FILE names" OFF)
if(RASM_SANITIZERS)
    list(JOIN RASM_SANITIZERS "," sanitizers)
    list(APPEND RASM_FLAGS -fsanitize=${sanitizers} -fno-sanitize-recover=all -fno-omit-frame-pointer)
endif()
if(RASM_COVERAGE)
    list(APPEND RASM_FLAGS -fprofile-instr-generate -fcoverage-mapping)
endif()

# --- [LANGUAGES]

list(JOIN RASM_FLAGS " " RASM_FLAGS)
string(APPEND CMAKE_CXX_FLAGS " ${RASM_FLAGS}")

# --- [PACKAGING] ------------------------------------------------------------------------

set(CMAKE_PROJECT_INCLUDE "${CMAKE_CURRENT_SOURCE_DIR}/.config/cmake/package.cmake")
set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS ".config/cmake/triplets/${VCPKG_TARGET_TRIPLET}.cmake" ".config/cmake/triplets/${VCPKG_HOST_TRIPLET}.cmake")

# --- [MEMBERS] --------------------------------------------------------------------------

option(BUILD_TESTING "Whether the test project of each package builds" ON)
file(GLOB manifests CONFIGURE_DEPENDS RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}" "libs/cpp/*/*/CMakeLists.txt")
if(BUILD_TESTING)
    enable_testing()
    set(CMAKE_CATCH_DISCOVER_TESTS_DISCOVERY_MODE PRE_TEST)
    include(.config/cmake/tests.cmake)
    file(GLOB suites CONFIGURE_DEPENDS RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}" "tests/libs/cpp/*/*/CMakeLists.txt")
endif()
foreach(manifest IN LISTS manifests suites)
    cmake_path(GET manifest PARENT_PATH member)
    add_subdirectory("${member}")
endforeach()
