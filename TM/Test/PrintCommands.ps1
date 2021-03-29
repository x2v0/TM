# $Id: $

#/*************************************************************************
# *                                                                       *
# * Copyright (C) 2021,   Valeriy Onuchin                                 *
# * All rights reserved.                                                  *
# *                                                                       *
#*************************************************************************/

Write-Host  "Loading TMClient.dll ..."
$cd = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
Import-Module "$cd./Init.ps1"

$commands = Get-command -module TMClient
$commands | Select-Object Name | Out-Null

Read-Host "Press any key to print available commands"
Write-Host "-------------------------------------------------------------------------------"

$commands | foreach ($_) {
   $help = Get-Help "$_" 
   $help.Synopsis
}


Read-Host "`n`n`n`nPress any key to print available classes and objects"
Write-Host "-------------------------------------------------------------------------------"
$typ | select -property FullName, BaseType
$typ | select -property FullName, BaseType | ogv

Read-Host "`n`n`n`nPress any key to display available Enums"
Write-Host "-------------------------------------------------------------------------------"

$enums = $typ | select -property FullName, BaseType | ? {$_.BaseType -like "*Enum"}
$enums | % { $_.FullName; [Enum]::GetValues($_.FullName)} | ogv

Read-Host "`n`n`n`nPress any key to display available Structs"
Write-Host "-------------------------------------------------------------------------------"

$structs = $typ | select -property FullName, BaseType | ? {$_.BaseType -like "*ValueType"}
