cmake_minimum_required(VERSION 4.4)

# --- [COMPOSITION] ---------------------------------------------------------------------

project(grammar LANGUAGES C)

set(GRAMMAR_SOURCE "" CACHE FILEPATH "Grammar extension")
set(PARSER_SOURCE "${CMAKE_CURRENT_BINARY_DIR}/src/parser.c" CACHE FILEPATH "Parser source")
set(SCANNER_SOURCE "" CACHE FILEPATH "External scanner source")
set(SCANNER_COMPILE_OPTIONS "" CACHE STRING "External scanner compiler options")
set(AST_GREP_CONFIG "" CACHE FILEPATH "Project language registration")

if(GRAMMAR_SOURCE)
    file(REAL_PATH "${GRAMMAR_SOURCE}" GRAMMAR_SOURCE)
    set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${GRAMMAR_SOURCE}")
    execute_process(COMMAND tree-sitter generate --output "${CMAKE_CURRENT_BINARY_DIR}/src" "${GRAMMAR_SOURCE}" COMMAND_ERROR_IS_FATAL ANY)
endif()

file(REAL_PATH "${PARSER_SOURCE}" PARSER_SOURCE)
if(SCANNER_SOURCE)
    file(REAL_PATH "${SCANNER_SOURCE}" SCANNER_SOURCE)
    set_property(SOURCE "${SCANNER_SOURCE}" PROPERTY COMPILE_OPTIONS "${SCANNER_COMPILE_OPTIONS}")
endif()

get_filename_component(parser_dir "${PARSER_SOURCE}" DIRECTORY)
get_filename_component(language "${CMAKE_CURRENT_BINARY_DIR}" NAME)

add_library(${PROJECT_NAME} MODULE "${PARSER_SOURCE}" ${SCANNER_SOURCE})
target_include_directories(${PROJECT_NAME} PRIVATE "${parser_dir}")
target_compile_features(${PROJECT_NAME} PRIVATE c_std_11)
set(output_dir "$<PATH:GET_PARENT_PATH,${CMAKE_CURRENT_BINARY_DIR}>")
set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "" SUFFIX ".so" OUTPUT_NAME "${language}" LIBRARY_OUTPUT_DIRECTORY "${output_dir}" RUNTIME_OUTPUT_DIRECTORY "${output_dir}")

if(AST_GREP_CONFIG)
    file(REAL_PATH "${AST_GREP_CONFIG}" AST_GREP_CONFIG)
    set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${AST_GREP_CONFIG}")
    get_filename_component(config_dir "${AST_GREP_CONFIG}" DIRECTORY)
    get_filename_component(library_dir "${CMAKE_CURRENT_BINARY_DIR}" DIRECTORY)
    get_target_property(library_suffix ${PROJECT_NAME} SUFFIX)
    file(RELATIVE_PATH project_library "${config_dir}" "${CMAKE_CURRENT_BINARY_DIR}${library_suffix}")
    cmake_path(RELATIVE_PATH config_dir BASE_DIRECTORY "${library_dir}" OUTPUT_VARIABLE project_root)
    execute_process(
        COMMAND yq -o=json
            [[{"ruleDirs": [], "customLanguages": (.customLanguages | with_entries(select(.value.libraryPath == strenv(AST_GREP_PROJECT_LIBRARY))))}
              | (.customLanguages[].libraryPath) = strenv(AST_GREP_LIBRARY)
              | (.customLanguages[][] | select(key == "outlineRules")) |= strenv(AST_GREP_PROJECT_ROOT) + "/" + .]]
            "${AST_GREP_CONFIG}"
        ENVIRONMENT "AST_GREP_PROJECT_LIBRARY=${project_library}" "AST_GREP_LIBRARY=${language}${library_suffix}" "AST_GREP_PROJECT_ROOT=${project_root}"
        OUTPUT_FILE "${CMAKE_CURRENT_BINARY_DIR}/sgconfig.yml"
        COMMAND_ERROR_IS_FATAL ANY
    )
    file(RENAME "${CMAKE_CURRENT_BINARY_DIR}/sgconfig.yml" "${CMAKE_CURRENT_BINARY_DIR}.yml")
endif()
