$url = "https://dl.gamepp.com/global/GamePP_International.exe"

$output = "$env:TEMP\GamePPSetup.exe"

if (Test-Path $output) {
    Write-Host "GamePP installer found. Starting installation..."
} else {
    Write-Host "GamePP installer not found. Starting download..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $output
        Write-Host "Download completed."
    } catch {
        Write-Host "Download failed: $($_.Exception.Message)"
        Read-Host "Press any key to exit..."
        exit
    }
}

Write-Host "Starting GamePP installer..."
Start-Process $output