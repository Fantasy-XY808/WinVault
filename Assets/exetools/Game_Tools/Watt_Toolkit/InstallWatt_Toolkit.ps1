$url = "https://gitee.com/rmbgame/SteamTools/releases/download/3.0.0-rc.16/Steam%20%20_v3.0.0-rc.16_win_x64.exe"

$output = "$env:TEMP\Watt_ToolkitSetup.exe"

if (Test-Path $output) {
    Write-Host "Watt_Toolkit installer found. Starting installation..."
} else {
    Write-Host "Watt_Toolkit installer not found. Starting download..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $output
        Write-Host "Download completed."
    } catch {
        Write-Host "Download failed: $($_.Exception.Message)"
        Read-Host "Press any key to exit..."
        exit
    }
}

Write-Host "Starting Watt_Toolkit installer..."
Start-Process $output