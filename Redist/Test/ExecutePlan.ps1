# $Id: $

#/*************************************************************************
# *                                                                       *
# * Copyright (C) 2021,   Valeriy Onuchin                                 *
# * All rights reserved.                                                  *
# *                                                                       *
#*************************************************************************/


$code = @"
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using TM;

namespace Execute
{
   public static class Plan
   {
      public static void Main()
      {
         var client = new TMClient();
         Console.WriteLine("Execute plan on server ... ");
         var results = client.ExecutePlan("test_plan.txt", "localhost", 9996);
      
         if (results == null) {
            Console.WriteLine("Failed to execute plan");
            Environment.Exit(1);
         }
      
         Console.WriteLine("Dump plan reults");
         client.DumpPlanResults();
         Console.ReadKey();
         Environment.Exit(0);
      }
   }
}
"@

# prepare path to load ./TMClient.dll
$currentScriptDirectory = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
[System.IO.Directory]::SetCurrentDirectory($currentScriptDirectory)
$dllpath = Join-Path $currentScriptDirectory 'TMClient.dll'
$asm = [System.Reflection.Assembly]::LoadFrom($dllpath)
 
# load embedded code
Add-Type -ReferencedAssemblies $asm -TypeDefinition $code -Language CSharp

# execute the code
Invoke-Expression "[Execute.Plan]::Main()"

