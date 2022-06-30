# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/SpiralNeo/*" -Force -Recurse
dotnet publish "./SpiralNeo.csproj" -c Release -o "$env:RELOADEDIIMODS/SpiralNeo" /p:OutputPath="./bin/Release" /p:RobustILLink="true"

# Restore Working Directory
Pop-Location