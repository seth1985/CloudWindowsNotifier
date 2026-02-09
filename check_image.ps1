Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Image]::FromFile('C:\Users\crven\OneDrive\Documents\AG_Projects\Cloud_Based_Notifier\frontend\public\Hero\hero1.png')
Write-Host "Dimensions: $($img.Width)x$($img.Height)"
$img.Dispose()
