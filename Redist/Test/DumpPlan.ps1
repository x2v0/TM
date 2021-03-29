# $Id: $

#/*************************************************************************
# *                                                                       *
# * Copyright (C) 2021,   Valeriy Onuchin                                 *
# * All rights reserved.                                                  *
# *                                                                       *
#*************************************************************************/

$source =  @"
using System;
using System.Collections.Generic;
using TM;

namespace D
{
   public class Plan
   {
      public Dictionary<int, PlanSpot> Load()
      {
         var plan = TMClient.LoadPlanData("test_plan.txt");
         Console.WriteLine("\nPress any key to continue ...");
         Console.ReadKey();
         return plan;
      }
   }
}
"@

# Set dir by "Right-Mouse Click" -> "Context menu" -> "Open with" -> "Windows PowerShell"
$cd = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
Import-Module "$cd./Init.ps1"

Add-Type -ReferencedAssemblies $asm -TypeDefinition $source -Language CSharp

#### Load plan data ####
$plan = new-object D.Plan
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


