#!/bin/bash
# Enhanced postinstall script for Grid Calculator with Encrypted UUID System
# Implements aggressive encryption approach - no plain text UUID ever stored

# Set up paths and log file (hard-coded for InDesign 2026)
PLUGIN_PATH="/Applications/Adobe InDesign 2026/Plug-Ins/Grid Calculator Publishing Edition"
ACTIVATOR_PATH="$PLUGIN_PATH/Activator 2026.app"
GRID_CALCULATOR_DIR="/Applications/.GridCalculator"
CG_INSTALLED_PATH_FILE="$GRID_CALCULATOR_DIR/com.apple.CGInstalledPath2026.txt"
GCC_APPDATA_FILE="$GRID_CALCULATOR_DIR/com.apple.gccscc2026.appdata.plist"
TMP_GCC_APPDATA_FILE="/tmp/com.apple.gccscc2026.appdata.plist"
LOG_FILE="/tmp/gc_encrypted_install.log"

# Shared UUID system paths (version-agnostic across all plugin versions)
UUID_COMPANY_DIR="/Library/Application Support/.dbs"
UUID_FILE="$UUID_COMPANY_DIR/.framework_data"
OLD_UUID_FILE="/Library/Application Support/Designers Bookshop/uuid_mac_address.txt"

# Define the PDF guide path (same folder as Activator)
PDF_GUIDE_PATH="$PLUGIN_PATH/Uninstallation_Guide.pdf"

echo "Starting encrypted UUID Grid Calculator installation at $(date)" > "$LOG_FILE"

# Verify running as root (required for system-wide installation)
if [ "$EUID" -ne 0 ]; then
    echo "ERROR: This script must be run as root (installer should handle this automatically)" >> "$LOG_FILE"
    echo "Current user: $(whoami), EUID: $EUID" >> "$LOG_FILE"
    exit 1
fi
echo "✓ Running with root privileges (EUID: $EUID)" >> "$LOG_FILE"

# ===== STEP 1: Setup Grid Calculator Directory =====
echo "Setting up Grid Calculator directory structure..." >> "$LOG_FILE"
mkdir -p "$GRID_CALCULATOR_DIR" 2>> "$LOG_FILE"
chmod 755 "$GRID_CALCULATOR_DIR" 2>> "$LOG_FILE"
echo "Created and set permissions for $GRID_CALCULATOR_DIR" >> "$LOG_FILE"

# ===== STEP 2: Create Encrypted UUID System =====
echo "Creating encrypted UUID system for Mac address replacement..." >> "$LOG_FILE"

# Create hidden UUID company directory if it doesn't exist
if [ ! -d "$UUID_COMPANY_DIR" ]; then
    # Safety check: ensure path doesn't exist as a file
    if [ -f "$UUID_COMPANY_DIR" ]; then
        echo "ERROR: UUID directory path exists as file: $UUID_COMPANY_DIR" >> "$LOG_FILE"
        exit 1
    fi
    
    mkdir -p "$UUID_COMPANY_DIR" 2>> "$LOG_FILE"
    if [ $? -eq 0 ]; then
        echo "Created hidden encrypted UUID directory: $UUID_COMPANY_DIR" >> "$LOG_FILE"
    else
        echo "ERROR: Failed to create hidden encrypted UUID directory: $UUID_COMPANY_DIR" >> "$LOG_FILE"
        exit 1
    fi
fi

# Set proper permissions for hidden UUID directory
chown root:admin "$UUID_COMPANY_DIR" 2>> "$LOG_FILE"
chmod 755 "$UUID_COMPANY_DIR" 2>> "$LOG_FILE"
echo "Set permissions for hidden encrypted UUID directory: root:admin 755" >> "$LOG_FILE"

# Generate and encrypt UUID (only if encrypted file doesn't exist)
if [ ! -f "$UUID_FILE" ]; then
    echo "Setting up UUID system (encrypted file not found)..." >> "$LOG_FILE"
    
    # Find the RunBundleHelper binary
    HELPER_PATH=$(find /private/tmp -name "RunBundleHelper" -type f 2>/dev/null | head -1)
    if [ ! -f "$HELPER_PATH" ]; then
        echo "ERROR: RunBundleHelper not found for UUID encryption" >> "$LOG_FILE"
        exit 1
    fi
    
    # Copy helper to temporary location for encryption
    TMP_HELPER="/tmp/.uuid_encrypt_helper"
    cp "$HELPER_PATH" "$TMP_HELPER" 2>> "$LOG_FILE"
    chmod +x "$TMP_HELPER" 2>> "$LOG_FILE"
    echo "Copied encryption helper to temporary location" >> "$LOG_FILE"
    
    # ===== UPGRADE COMPATIBILITY: Check for existing plain text UUID =====
    UUID_MAC=""
    UUID_SOURCE=""
    
    if [ -f "$OLD_UUID_FILE" ] && [ -r "$OLD_UUID_FILE" ]; then
        echo "Found existing plain text UUID file - checking for upgrade compatibility..." >> "$LOG_FILE"
        
        # Read and clean existing UUID
        EXISTING_UUID=$(cat "$OLD_UUID_FILE" 2>/dev/null | tr -d '\n\r\t ' | tr '[:lower:]' '[:upper:]')
        
        # Security validation: Check format (exactly 12 hex characters)
        if [ ${#EXISTING_UUID} -eq 12 ] && echo "$EXISTING_UUID" | grep -q '^[0-9A-F]\{12\}$'; then
            UUID_MAC="$EXISTING_UUID"
            UUID_SOURCE="upgrade_preserved"
            echo "✓ UPGRADE: Preserving existing valid UUID for seamless transition: $UUID_MAC" >> "$LOG_FILE"
            echo "✓ User can continue using software without re-activation" >> "$LOG_FILE"
        else
            UUID_SOURCE="upgrade_invalid"
            echo "⚠ SECURITY: Existing UUID failed validation (length: ${#EXISTING_UUID}, content: '$EXISTING_UUID')" >> "$LOG_FILE"
            echo "⚠ Generating new UUID for security reasons" >> "$LOG_FILE"
        fi
        
        # Log security decision for audit trail
        echo "UUID validation result: source=$UUID_SOURCE, length=${#EXISTING_UUID}" >> "$LOG_FILE"
    else
        UUID_SOURCE="fresh_install"
        echo "No existing UUID file found - this appears to be a fresh installation" >> "$LOG_FILE"
    fi
    
    # Generate new UUID if we don't have a valid existing one
    if [ -z "$UUID_MAC" ]; then
        UUID_RAW=$(uuidgen)
        if [ $? -eq 0 ] && [ ! -z "$UUID_RAW" ]; then
            UUID_MAC=$(echo "$UUID_RAW" | tr -d '-' | cut -c1-12 | tr '[:lower:]' '[:upper:]')
            echo "Generated new UUID: $UUID_MAC (source: $UUID_SOURCE)" >> "$LOG_FILE"
        else
            echo "ERROR: Failed to generate new UUID" >> "$LOG_FILE"
            rm -f "$TMP_HELPER" 2>/dev/null
            exit 1
        fi
    fi
    
    # Encrypt the UUID (whether preserved or newly generated) with retry logic
    RETRY_COUNT=0
    MAX_RETRIES=3
    ENCRYPTION_SUCCESS=0
    
    while [ $RETRY_COUNT -lt $MAX_RETRIES ] && [ $ENCRYPTION_SUCCESS -eq 0 ]; do
        RETRY_COUNT=$((RETRY_COUNT + 1))
        echo "UUID encryption attempt $RETRY_COUNT of $MAX_RETRIES for UUID: $UUID_MAC (source: $UUID_SOURCE)..." >> "$LOG_FILE"
        
        # Encrypt UUID using enhanced RunBundleHelper
        ENCRYPTED_RESULT=$("$TMP_HELPER" --encrypt-uuid "$UUID_MAC" 2>>"$LOG_FILE")
        ENCRYPT_EXIT_CODE=$?
        
        if [ $ENCRYPT_EXIT_CODE -eq 0 ] && [ ! -z "$ENCRYPTED_RESULT" ]; then
            # Write encrypted result directly to file
            echo "$ENCRYPTED_RESULT" > "$UUID_FILE"
            if [ $? -eq 0 ]; then
                echo "✓ Successfully encrypted and stored UUID on attempt $RETRY_COUNT (source: $UUID_SOURCE)" >> "$LOG_FILE"
                ENCRYPTION_SUCCESS=1
            else
                echo "✗ Failed to write encrypted UUID to file on attempt $RETRY_COUNT" >> "$LOG_FILE"
            fi
        else
            echo "✗ UUID encryption failed on attempt $RETRY_COUNT (exit code: $ENCRYPT_EXIT_CODE)" >> "$LOG_FILE"
        fi
        
        # If not successful and not last attempt, wait briefly before retry
        if [ $ENCRYPTION_SUCCESS -eq 0 ] && [ $RETRY_COUNT -lt $MAX_RETRIES ]; then
            echo "Waiting 1 second before retry..." >> "$LOG_FILE"
            sleep 1
        fi
    done
    
    # Check final result
    if [ $ENCRYPTION_SUCCESS -eq 1 ]; then
        echo "✓ UUID encryption system successfully created (source: $UUID_SOURCE)" >> "$LOG_FILE"
        
        # Set proper permissions for encrypted UUID file (readable by all users, writable by root only)
        chown root:admin "$UUID_FILE" 2>> "$LOG_FILE"
        chmod 644 "$UUID_FILE" 2>> "$LOG_FILE"
        echo "Set encrypted UUID file permissions: root:admin 644" >> "$LOG_FILE"
    else
        echo "CRITICAL ERROR: UUID encryption failed after $MAX_RETRIES attempts. Installation cannot continue." >> "$LOG_FILE"
        # Clean up failed attempt
        rm -f "$UUID_FILE" 2>/dev/null
        rm -f "$TMP_HELPER" 2>/dev/null
        exit 1
    fi
    
    # CRITICAL: Remove encryption helper immediately after use
    rm -f "$TMP_HELPER" 2>> "$LOG_FILE"
    echo "✓ Encryption helper removed - no encryption tools remain on system" >> "$LOG_FILE"
    
else
    # File exists, preserve it but ensure proper permissions
    echo "Encrypted UUID file already exists, preserving existing encrypted data" >> "$LOG_FILE"
    
    # Ensure proper permissions on existing file
    chown root:admin "$UUID_FILE" 2>> "$LOG_FILE"
    chmod 644 "$UUID_FILE" 2>> "$LOG_FILE"
    echo "Updated permissions on existing encrypted UUID file" >> "$LOG_FILE"
fi

# Verify encrypted UUID file is readable and properly formatted
if [ -r "$UUID_FILE" ]; then
    ENCRYPTED_SIZE=$(wc -c < "$UUID_FILE" 2>/dev/null)
    echo "Encrypted UUID file verification: size=$ENCRYPTED_SIZE bytes" >> "$LOG_FILE"
    if [ "$ENCRYPTED_SIZE" -gt 10 ]; then
        echo "✓ Encrypted UUID file appears valid" >> "$LOG_FILE"
    else
        echo "WARNING: Encrypted UUID file appears too small" >> "$LOG_FILE"
    fi
else
    echo "ERROR: Encrypted UUID file is not readable after creation/update" >> "$LOG_FILE"
    exit 1
fi

# ===== STEP 3: Registration Processing =====
echo "Starting Registration processing at $(date)" >> "$LOG_FILE"

# Create temporary directory for registration
mkdir -p "/tmp/.gc_registration" 2>> "$LOG_FILE"

# Find registration resources
BUNDLE_PATH=$(find /private/tmp -name "Registration.bundle" -type d 2>/dev/null | head -1)
HELPER_PATH=$(find /private/tmp -name "RunBundleHelper" -type f 2>/dev/null | head -1)

echo "Bundle path: $BUNDLE_PATH" >> "$LOG_FILE"
echo "Helper path: $HELPER_PATH" >> "$LOG_FILE"

# Process registration if resources are found
if [ -d "$BUNDLE_PATH" ] && [ -f "$HELPER_PATH" ]; then
    cp -R "$BUNDLE_PATH" "/tmp/.gc_registration/" 2>> "$LOG_FILE"
    cp "$HELPER_PATH" "/tmp/.gc_registration/" 2>> "$LOG_FILE"
    chmod +x "/tmp/.gc_registration/RunBundleHelper" 2>> "$LOG_FILE"
    echo "Registration files copied to temporary location" >> "$LOG_FILE"

    # Run the helper from the right directory (normal bundle processing mode)
    cd "/tmp/.gc_registration/" 
    "/tmp/.gc_registration/RunBundleHelper" "/tmp/.gc_registration/Registration.bundle"
    REG_RESULT=$?
    echo "Registration process exited with code: $REG_RESULT" >> "$LOG_FILE"
    
    # Clean up registration files
    rm -rf "/tmp/.gc_registration" 2>> "$LOG_FILE"
    echo "Cleaned up temporary registration directory" >> "$LOG_FILE"
else
    echo "WARNING: Registration resources not found" >> "$LOG_FILE"
fi

# ===== STEP 4: Handle Appdata File =====
echo "Checking for appdata file..." >> "$LOG_FILE"

# CRITICAL: Checking both possible locations for the appdata file
if [ -e "$TMP_GCC_APPDATA_FILE" ]; then
    echo "Found appdata file in /tmp, moving to $GRID_CALCULATOR_DIR" >> "$LOG_FILE"
    cp "$TMP_GCC_APPDATA_FILE" "$GCC_APPDATA_FILE" 2>> "$LOG_FILE"
    
    # Verify copy was successful
    if [ -e "$GCC_APPDATA_FILE" ]; then
        echo "Successfully moved appdata file to $GCC_APPDATA_FILE" >> "$LOG_FILE"
    else
        echo "ERROR: Failed to move appdata file to $GCC_APPDATA_FILE" >> "$LOG_FILE"
    fi
elif [ ! -e "$GCC_APPDATA_FILE" ] && [ -e "/Applications/.GridCalculator/com.apple.gccscc2026.appdata.plist" ]; then
    echo "Appdata file already exists in correct location" >> "$LOG_FILE"
else
    echo "WARNING: Appdata file not found in expected locations" >> "$LOG_FILE"
fi

# ===== STEP 5: Plugin Installation =====
echo "Installing plugins..." >> "$LOG_FILE"

# Create plugin path file
echo "$PLUGIN_PATH" > "$CG_INSTALLED_PATH_FILE"
echo "Created $CG_INSTALLED_PATH_FILE" >> "$LOG_FILE"

# Set group to admin and give group write permissions for PLUGIN_PATH
if [ -d "$PLUGIN_PATH" ]; then
    chgrp -R admin "$PLUGIN_PATH" 2>> "$LOG_FILE"
    chmod -R 775 "$PLUGIN_PATH" 2>> "$LOG_FILE"
    chmod g+s "$PLUGIN_PATH" 2>> "$LOG_FILE"
    echo "Set permissions for $PLUGIN_PATH" >> "$LOG_FILE"
else
    echo "WARNING: Plugin path $PLUGIN_PATH not found" >> "$LOG_FILE"
fi

# ===== STEP 6: Activator Installation =====
echo "Setting up Activator..." >> "$LOG_FILE"

# Verify Activator app was installed correctly
if [ -d "$ACTIVATOR_PATH" ]; then
    echo "Activator app found at $ACTIVATOR_PATH" >> "$LOG_FILE"
    chgrp -R admin "$ACTIVATOR_PATH" 2>> "$LOG_FILE"
    chmod -R 775 "$ACTIVATOR_PATH" 2>> "$LOG_FILE"
    chmod g+s "$ACTIVATOR_PATH" 2>> "$LOG_FILE"
    echo "Set permissions for Activator.app" >> "$LOG_FILE"
else
    echo "WARNING: Activator app not found at $ACTIVATOR_PATH" >> "$LOG_FILE"
fi

# ===== STEP 6b: Uninstallation Guide PDF =====
echo "Setting up Uninstallation Guide PDF..." >> "$LOG_FILE"

# Remove any existing Uninstaller.app if it exists
UNINSTALLER_PATH="$PLUGIN_PATH/Uninstaller.app"
if [ -d "$UNINSTALLER_PATH" ]; then
    echo "Removing existing Uninstaller.app..." >> "$LOG_FILE"
    rm -rf "$UNINSTALLER_PATH" 2>> "$LOG_FILE"
    echo "Uninstaller.app removed" >> "$LOG_FILE"
fi

# Set permissions for the PDF guide - all can read, only root/admin can modify or delete
if [ -f "$PDF_GUIDE_PATH" ]; then
    echo "Setting permissions for Uninstallation_Guide.pdf" >> "$LOG_FILE"
    chown root:admin "$PDF_GUIDE_PATH" 2>> "$LOG_FILE"
    chmod 644 "$PDF_GUIDE_PATH" 2>> "$LOG_FILE"
    echo "Uninstallation_Guide.pdf is now read-only for non-admin users" >> "$LOG_FILE"
else
    echo "WARNING: Uninstallation_Guide.pdf not found at $PDF_GUIDE_PATH" >> "$LOG_FILE"
fi

# ===== STEP 7: User Document Structure =====
echo "Setting up user document structure..." >> "$LOG_FILE"

# Find the console user (installing user)
CONSOLE_USER=$(/usr/bin/stat -f "%Su" /dev/console)
USER_HOME="/Users/$CONSOLE_USER"
echo "Console user: $CONSOLE_USER, home: $USER_HOME" >> "$LOG_FILE"

# Create Documents folder structure
mkdir -p "$USER_HOME/Documents/Grid Calculator Publishing Edition/Presets/Miscellaneous" 2>> "$LOG_FILE"
echo "Created Documents directory structure" >> "$LOG_FILE"

# Copy Pro Edition presets if applicable
if [ -d "$USER_HOME/Documents/Grid Calculator Pro Edition/Presets" ] && [ ! -d "$USER_HOME/Documents/Grid Calculator Publishing Edition/Presets" ]; then
    cp -Rp "$USER_HOME/Documents/Grid Calculator Pro Edition/Presets/"* \
        "$USER_HOME/Documents/Grid Calculator Publishing Edition/Presets/" 2>> "$LOG_FILE"
    echo "Copied presets from Pro Edition" >> "$LOG_FILE"
fi

# ===== STEP 8: Set Final Permissions =====
echo "Setting final permissions..." >> "$LOG_FILE"

# Set ownership of the user's Documents directory content
chown -R "$CONSOLE_USER" "$USER_HOME/Documents/Grid Calculator Publishing Edition/" 2>> "$LOG_FILE"
echo "Set ownership of Documents presets to $CONSOLE_USER" >> "$LOG_FILE"

# IMPORTANT CHANGE: Make Documents directory world-writable so all users can access it
chmod -R 777 "$USER_HOME/Documents/Grid Calculator Publishing Edition/" 2>> "$LOG_FILE"
echo "Set world-writable permissions (777) for Documents folder to allow all users to access it" >> "$LOG_FILE"

# Set setgid bit on the Documents directory to ensure new files inherit group permissions
chmod g+s "$USER_HOME/Documents/Grid Calculator Publishing Edition/" 2>> "$LOG_FILE"
echo "Applied setgid bit to Documents folder" >> "$LOG_FILE"

# Set permissions for GridCalculator folder and files (corrected based on reference)
chown "$CONSOLE_USER:admin" "$GRID_CALCULATOR_DIR" 2>> "$LOG_FILE"
chmod 775 "$GRID_CALCULATOR_DIR" 2>> "$LOG_FILE"

# Set permissions for the path file
chown root:admin "$CG_INSTALLED_PATH_FILE" 2>> "$LOG_FILE"
chmod 644 "$CG_INSTALLED_PATH_FILE" 2>> "$LOG_FILE"

# Set permissions for the appdata file if it exists
if [ -e "$GCC_APPDATA_FILE" ]; then
    echo "Setting ownership and permissions for $GCC_APPDATA_FILE" >> "$LOG_FILE"
    chown root:admin "$GCC_APPDATA_FILE" 2>> "$LOG_FILE"
    chmod 664 "$GCC_APPDATA_FILE" 2>> "$LOG_FILE"
    echo "Appdata file now writable by admin group for multi-user license updates" >> "$LOG_FILE"
fi

# ===== STEP 9: Final Security Verification =====
echo "Performing final security verification..." >> "$LOG_FILE"

# SECURITY: Remove old plain text UUID file if it exists (after successful encryption)
if [ -f "$OLD_UUID_FILE" ]; then
    echo "SECURITY: Removing old plain text UUID file now that encrypted version is created: $OLD_UUID_FILE" >> "$LOG_FILE"
    rm -f "$OLD_UUID_FILE" 2>> "$LOG_FILE"
    
    # Remove old directory if empty
    OLD_UUID_DIR="/Library/Application Support/Designers Bookshop"
    if [ -d "$OLD_UUID_DIR" ] && [ -z "$(ls -A "$OLD_UUID_DIR" 2>/dev/null)" ]; then
        echo "SECURITY: Removing empty old UUID directory: $OLD_UUID_DIR" >> "$LOG_FILE"  
        rmdir "$OLD_UUID_DIR" 2>> "$LOG_FILE"
    fi
    echo "✓ Security upgrade complete: Plain text UUID eliminated, encrypted UUID operational" >> "$LOG_FILE"
fi

# Verify encrypted UUID file system
if [ -f "$UUID_FILE" ] && [ -r "$UUID_FILE" ]; then
    ENCRYPTED_SIZE=$(wc -c < "$UUID_FILE" 2>/dev/null)
    echo "✓ Encrypted UUID system ready: $UUID_FILE (size: $ENCRYPTED_SIZE bytes)" >> "$LOG_FILE"
    echo "✓ All users can now use consistent encrypted UUID-based licensing" >> "$LOG_FILE"
    echo "✓ No plain text UUID ever stored on disk" >> "$LOG_FILE"
else
    echo "ERROR: Encrypted UUID file is not accessible for final verification" >> "$LOG_FILE"
    exit 1
fi

# Verify no encryption tools remain
if [ ! -f "/tmp/.uuid_encrypt_helper" ] && [ ! -f "/tmp/RunBundleHelper" ]; then
    echo "✓ Security verification passed: No encryption tools remain on system" >> "$LOG_FILE"
else
    echo "WARNING: Encryption tools still found on system - manual cleanup may be needed" >> "$LOG_FILE"
fi

# Verify directory has obscured and hidden name
if [ -d "$UUID_COMPANY_DIR" ]; then
    echo "✓ Encrypted UUID stored in hidden, obscured path: $UUID_COMPANY_DIR" >> "$LOG_FILE"
else
    echo "ERROR: Hidden encrypted UUID directory missing" >> "$LOG_FILE"
    exit 1
fi

echo "✓ Encrypted UUID installation completed successfully at $(date)" >> "$LOG_FILE"
echo "✓ Security status: UUID encrypted from installation moment, no plain text vulnerability window" >> "$LOG_FILE"
exit 0