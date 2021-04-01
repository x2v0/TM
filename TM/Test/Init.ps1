#### Set path to the current directory and load TM.dll ####

# Set dir by "Right-Mouse Click" -> "Context menu" -> "Open with" -> "Windows PowerShell"
$currentScriptDirectory = $MyInvocation.MyCommand.Path
if ([string]::IsNullOrEmpty($currentScriptDirectory)) {
	$currentScriptDirectory = Get-Location
} else {
	$currentScriptDirectory = Split-Path $currentScriptDirectory -Path  -Parent
}

$dllpath = Join-Path $currentScriptDirectory "../TM.dll"

[System.IO.Directory]::SetCurrentDirectory($currentScriptDirectory)
Set-Location $currentScriptDirectory

Write-Host  "Loading TM.dll ..."
$asm = [System.Reflection.Assembly]::LoadFrom($dllpath)
$typ = Add-Type -Path $dllpath -PassThru
Import-Module $dllpath

