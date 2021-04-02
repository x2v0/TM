# Author: Valeriy Onuchin 31.03.2021

# Set the current directory by "Right-Mouse Click" -> "Context menu" -> "Open with" -> "Windows PowerShell"
#  ... or by Double-Clicking
$currentScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
[System.IO.Directory]::SetCurrentDirectory($currentScriptDirectory)
Set-Location $currentScriptDirectory

dotnet build --configuration Release ./TM.sln


