Write-Host -fore green "=========================================="
dotnet build .
Write-Host -fore green "=========================================="
dotnet pack .
Write-Host -fore green "=========================================="
dotnet tool uninstall -g Krosoft.ObjectStorage.CLI
Write-Host -fore green "=========================================="
dotnet tool install --global --add-source .\publish\ Krosoft.ObjectStorage.CLI
Write-Host -fore green "=========================================="
dotnet tool list --global
Write-Host -fore green "=========================================="
krosoft help
Write-Host -fore green "=========================================="