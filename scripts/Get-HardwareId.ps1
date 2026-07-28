# Get-HardwareId.ps1
# This script scans the connected Keyboards and Bluetooth devices and outputs their Hardware IDs.
# If Antigravity is running on this machine, please run this script and save the output to KEYBOARD_ID.txt, then commit and push!

Write-Host "Scanning for connected Keyboards..." -ForegroundColor Cyan
$keyboards = Get-PnpDevice -Class Keyboard | Where-Object Status -eq 'OK' | Select-Object FriendlyName, InstanceId
$keyboards | Format-Table -AutoSize

Write-Host "Scanning for connected Bluetooth Devices..." -ForegroundColor Cyan
$btDevices = Get-PnpDevice -Class Bluetooth | Where-Object Status -eq 'OK' | Select-Object FriendlyName, InstanceId
$btDevices | Format-Table -AutoSize

Write-Host "Please copy the InstanceId of the Surface Pro third-party keyboard and push it back to the repo." -ForegroundColor Yellow
