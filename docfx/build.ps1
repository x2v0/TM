# Builds the TM API documentation using docfx

dotnet build --configuration Release ../TM.sln

rm ../docs -Recurse -Force

$env:VSINSTALLDIR=""
docfx --metadata

#cd ../docs
docfx --serve