
$cd = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
Import-Module "$cd./Init.ps1"

$commands = Get-command -module TM
$commands | Select-Object Name | Out-Null

Read-Host "Press any key to print available commands"
'______________________________________________________________________________________________';

$commands | foreach ($_) {
   $help = Get-Help "$_" 
   $help.Synopsis
}


Read-Host "`n`n`n`nPress any key to print available classes"
'______________________________________________________________________________________________';
$typ | select -property FullName, BaseType | ft

Read-Host "`n`n`n`nPress any key to display available Enums"

$enums = $typ | select -property FullName, BaseType | ? {$_.BaseType -like "*Enum"}
$enums | 
   foreach { 
'______________________________________________________________________________________________';
      $_.FullName; 
      [Enum]::GetValues($_.FullName)
   }

Read-Host "`n`n`n`nPress any key to display available Structures"

$structs = $typ | select -property FullName, BaseType | ? {$_.BaseType -like "*ValueType"}
$structs | 
   foreach { 
      $t = $asm.GetType($_.FullName);
'______________________________________________________________________________________________';
      $_.FullName; 
      $props = $t.GetProperties(); 
      foreach ($p in $props) {
         New-Object PSObject -Property @{ 
            Name = $p.Name; 
            Type = $p.PropertyType.Name;
            Description = $p.CustomAttributes.ConstructorArguments.Value
         }
      }
      $fields = $t.GetFields(); 
      foreach ($f in $fields) {
         New-Object PSObject -Property @{ 
            Name = $f.Name; 
            Type = $f.FieldType.Name;
            Description = $f.CustomAttributes.ConstructorArguments.Value
         }
      }
   }
 