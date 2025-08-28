$url = "https://launcher-public-service-prod.ak.epicgames.com/launcher/api/installer/download/EpicGamesLauncherInstaller.msi"

$output = "$env:TEMP\EpicSetup.msi"

if (Test-Path $output) {
    Write-Host "Epic installer found. Starting installation..."
} else {
    Write-Host "Epic installer not found. Starting download..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $output
        Write-Host "Download completed."
    } catch {
        Write-Host "Download failed: $($_.Exception.Message)"
        Read-Host "Press any key to exit..."
        exit
    }
}

Write-Host "Starting Epic installer..."
Start-Process $output