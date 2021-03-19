Write-Host  "Loading TMClient.dll ..."
Import-Module ./TMClient.dll

$commands = Get-command -module TMClient 
$commands | Select-Object Name | Out-Null

Read-Host "Press any key to print available commands"
Write-Host "-------------------------------------------------------------------------------"

$commands | foreach ($_) {
   $help = Get-Help "$_" 
   $help.Synopsis
}
