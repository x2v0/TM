#### Set path to currentdirectory and load TMClient.dll ####
$currentScriptDirectory = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
[System.IO.Directory]::SetCurrentDirectory($currentScriptDirectory)
$dllpath  = Join-Path $currentScriptDirectory "TMClient.dll"
$asm = [System.Reflection.Assembly]::LoadFrom($dllpath)

$dump = Join-Path $currentScriptDirectory "DumpPlan.cs"
$source = Get-Content -Path $dump
Add-Type -ReferencedAssemblies $asm -TypeDefinition "$source" -Language CSharp

#### Load plan data ####
$plan = new-object Dump.Plan
$spots = $plan.Load().Values

#### Print out plan data to table ####
write-output $spots | Format-Table

#### Convert angles from radians to degrees ####
$rad2deg = 57.2957795130823230
$spotsD  = $spots | 
         Select-Object id, 
         @{Label="X-angle (degree)";  Expression={$PSItem.xangle*$rad2deg}}, 
         @{Label="Z-angle (degree)";  Expression={$PSItem.zangle*$rad2deg}},
         @{Label="Energy (MeV)";      Expression={$PSItem.energy}},
         @{Label="Number of protons"; Expression={$PSItem.pcount}}

#### Print out plan data to GridView ####
write-output $spotsD | Out-GridView -Title "Plan"


