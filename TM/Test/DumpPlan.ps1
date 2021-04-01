# $Id: $

#/*************************************************************************
# *                                                                       *
# * Copyright (C) 2021,   Valeriy Onuchin                                 *
# * All rights reserved.                                                  *
# *                                                                       *
#*************************************************************************/

# Set dir by "Right-Mouse Click" -> "Context menu" -> "Open with" -> "Windows PowerShell"
$cd = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
Import-Module "$cd./Init.ps1"
$code = Get-Content "$cd./DumpPlan.cs"

Add-Type -ReferencedAssemblies $asm, "System.Collections", "System.Runtime", "netstandard"  -TypeDefinition "$code"

#### Load plan data ####
$plan = new-object My.Plan
$spots = $plan.Load("test_plan.txt")

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


