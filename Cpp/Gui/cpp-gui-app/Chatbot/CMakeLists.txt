cmake_minimum_required(VERSION 3.16)

project(Chatbot VERSION 0.1 LANGUAGES CXX)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Enable automatic MOC, UIC, RCC for Qt6
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

# Find Qt6 and required modules
find_package(Qt6 6.5 REQUIRED COMPONENTS Quick QuickControls2 Concurrent)

# Find external dependencies (vcpkg will provide these if toolchain is set)
set(CURL_INCLUDE_DIR "${CMAKE_SOURCE_DIR}/packages/curl_x64-windows/include")
set(CURL_LIBRARY "${CMAKE_SOURCE_DIR}/packages/curl_x64-windows/lib/libcurl.lib")
include_directories(${CURL_INCLUDE_DIR})
link_libraries(${CURL_LIBRARY})

set(NLOHMANN_JSON_INCLUDE_DIR "${CMAKE_SOURCE_DIR}/packages/nlohmann-json_x64-windows/include")
include_directories(${NLOHMANN_JSON_INCLUDE_DIR})

qt_standard_project_setup(REQUIRES 6.5)

# Add executable sources
qt_add_executable(appChatbot
    main.cpp
    AiConnector.cpp
    AiConnector.h
)

# Add QML module
qt_add_qml_module(appChatbot
    URI Chatbot
    VERSION 1.0
    QML_FILES
        Main.qml
    SOURCES
        AiConnector.cpp
        AiConnector.h
)

# Platform-specific settings
if(WIN32)
    set_target_properties(appChatbot PROPERTIES
        WIN32_EXECUTABLE TRUE
    )
elseif(APPLE)
    set_target_properties(appChatbot PROPERTIES
        MACOSX_BUNDLE TRUE
        # MACOSX_BUNDLE_GUI_IDENTIFIER com.example.appChatbot
        MACOSX_BUNDLE_BUNDLE_VERSION ${PROJECT_VERSION}
        MACOSX_BUNDLE_SHORT_VERSION_STRING ${PROJECT_VERSION_MAJOR}.${PROJECT_VERSION_MINOR}
    )
endif()

# Include directories if needed (Qt and vcpkg should handle this automatically)
# target_include_directories(appChatbot PRIVATE ...)

# Link libraries
target_link_libraries(appChatbot
    PRIVATE Qt6::Quick
    PRIVATE ${CURL_LIBRARY}
    PRIVATE Qt6::QuickControls2
    PRIVATE Qt6::Concurrent
    #PRIVATE nlohmann_json::nlohmann_json
)    # "${Qt6_DIR}/../../../bin/Qt6Quick.dll"

if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    set(DLLS_TO_COPY
        "${CMAKE_SOURCE_DIR}/libcurl.dll"
        "${CMAKE_SOURCE_DIR}/zlibd1.dll"
        # "${Qt6_DIR}/../../../bin/Qt6QuickControls2.dll"
        # "${Qt6_DIR}/../../../bin/Qt6Concurrent.dll"
        # Set DLLs to copy based on build type
        # Add other debug DLLs as needed
    )
else()
    set(DLLS_TO_COPY
        "${CMAKE_SOURCE_DIR}/libcurl.dll"
        "${CMAKE_SOURCE_DIR}/zlib1.dll"
        # Add other release DLLs as needed
    )
endif()

add_custom_command(TARGET appChatbot POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        ${DLLS_TO_COPY}
        $<TARGET_FILE_DIR:appChatbot>
)
# Install rules
include(GNUInstallDirs)
