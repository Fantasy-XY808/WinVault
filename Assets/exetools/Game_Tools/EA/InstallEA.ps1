$url = "https://origin-a.akamaihd.net/EA-Desktop-Client-Download/installer-releases/EAappInstaller.exe"

$output = "$env:TEMP\EASetup.exe"

if (Test-Path $output) {
    Write-Host "EA installer found. Starting installation..."
} else {
    Write-Host "EA installer not found. Starting download..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $output
        Write-Host "Download completed."
    } catch {
        Write-Host "Download failed: $($_.Exception.Message)"
        Read-Host "Press any key to exit..."
        exit
    }
}

Write-Host "Starting EA installer..."
Start-Process $output