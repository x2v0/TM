
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

$cd = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
Import-Module "$cd./Init.ps1"
 
# load embedded code
Add-Type -ReferencedAssemblies $asm, "System.Console", "System.Collections", "netstandard" `
         -TypeDefinition $code -Language CSharp

# execute the code
Invoke-Expression "[Execute.Plan]::Main()"

