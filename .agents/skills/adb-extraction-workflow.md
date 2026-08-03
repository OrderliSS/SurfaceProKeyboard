# ADB Android Package Extraction Workflow Skill

## Purpose
Standardize local Android package (APK) path extraction and pulling via Android Debug Bridge (ADB).

## Execution Guidelines
1. **Clean Command Execution**:
   - Execute ADB commands cleanly, preferring WSL 2 (Windows Subsystem for Linux 2) execution where applicable.
2. **Defensive Connection Checks**:
   - Always run defensive checks to confirm USB/device connection status before executing pull commands:
     ```bash
     adb devices
     ```
   - Validate that the device status is active (`device`) and not `offline` or unauthorized before proceeding.
3. **APK Extraction Steps**:
   - Extract APK path for the target package:
     ```bash
     adb shell pm path <package_name>
     ```
   - Pull the APK file to local filesystem:
     ```bash
     adb pull <remote_apk_path> <local_destination>
     ```
