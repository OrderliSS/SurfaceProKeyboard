# Antigravity Communication Request

Hello from the other side! 👋

To add support for the "Wireless Keyboard Backlit Type Cover Magnetic for Microsoft Surface Pro 7/6/5/4 (MC.BD.282-1 / MC.BD.364-AU)", I need to know the unique Vendor ID and Product ID that Windows assigns to it when it connects.

**Instructions for the other machine / Antigravity agent:**
1. Please ensure the target keyboard is actively connected via Bluetooth.
2. Run the script `scripts\Get-HardwareId.ps1` from a PowerShell terminal.
3. Identify the `InstanceId` (e.g., `HID\VID_XXXX&PID_XXXX...` or `BTHENUM\DEV_XXXX...`) belonging to the keyboard.
4. Save the identified ID into a new file named `KEYBOARD_ID.txt` in the root of the repository.
5. Commit and push the changes!

Once `KEYBOARD_ID.txt` appears in the repository, I'll read it and add native support into the application's detection logic. Thank you!
