# Local Hardware Resilience & System Notes

## Surface Pro 4 & Third-Party Keyboard Diagnostic Guide

### 1. Keyboard Keystroke Dropout Fix
- **Windows Filter Keys Check**:
  - If third-party keyboard keystrokes drop out or exhibit delay/skipping, ensure Windows **Filter Keys** is disabled.
  - Navigation: `Settings > Accessibility > Keyboard > Filter Keys` (Toggle OFF).

### 2. Surface Type Cover Attachment Connection Fix
- **Driver Reset Procedure**:
  - If Surface Type Cover keyboard connection fails or is unrecognized:
    1. Open **Device Manager** (`devmgmt.msc`).
    2. Expand **Keyboards** or **System devices**.
    3. Locate **Surface Type Cover Filter Device**.
    4. Right-click and select **Uninstall device**.
    5. Click **Action > Scan for hardware changes** to force driver re-initialization and reinstall the filter device driver.
