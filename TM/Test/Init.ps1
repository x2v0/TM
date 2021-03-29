#### Set path to current directory and load TMClient.dll ####

# Set dir by "Right-Mouse Click" -> "Context menu" -> "Open with" -> "Windows PowerShell"
$currentScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
$dllpath  = Join-Path $currentScriptDirectory "TMClient.dll"

[System.IO.Directory]::SetCurrentDirectory($currentScriptDirectory)
Set-Location $currentScriptDirectory

$asm = [System.Reflection.Assembly]::LoadFrom($dllpath)
$typ = Add-Type -Path $dllpath -PassThru
Import-Module $dllpath

