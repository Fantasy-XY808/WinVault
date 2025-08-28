$url = "https://static3.cdn.ubi.com/orbit/launcher_installer/UbisoftConnectInstaller.exe"

$output = "$env:TEMP\UplaySetup.exe"

if (Test-Path $output) {
    Write-Host "Uplay installer found. Starting installation..."
} else {
    Write-Host "Uplay installer not found. Starting download..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $output
        Write-Host "Download completed."
    } catch {
        Write-Host "Download failed: $($_.Exception.Message)"
        Read-Host "Press any key to exit..."
        exit
    }
}

Write-Host "Starting Uplay installer..."
Start-Process $output