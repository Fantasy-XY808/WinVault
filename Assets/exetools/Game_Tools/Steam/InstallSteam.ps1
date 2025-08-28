$url = "https://media.st.dl.eccdnx.com/client/installer/SteamSetup.exe"

$output = "$env:TEMP\SteamSetup.exe"

if (Test-Path $output) {
    Write-Host "Steam installer found. Starting installation..."
} else {
    Write-Host "Steam installer not found. Starting download..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $output
        Write-Host "Download completed."
    } catch {
        Write-Host "Download failed: $($_.Exception.Message)"
        Read-Host "Press any key to exit..."
        exit
    }
}

Write-Host "Starting Steam installer..."
Start-Process $output