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

class Program
{
   // The client
   private static TMClient Client;

   public static void Main(string[] args)
   {
      if (Client == null) {
         Client = new TMClient();
       
         // Subscribe to server disconnected event
         Client.ServerDisconnected += OnDisconnected;
         
         // Subscribe to "server's state" event
         Client.ServerStateChanged += OnStateChanged;
      }

      Console.WriteLine("Connecting to server ... ");
      var ok = false;
      
      try {
         // connect to server - "localhost:9996"
         ok = Client.Connect("localhost", 9996);
      } catch(Exception ex){
         Console.WriteLine("Failed to connect! " + ex.Message);
         Console.ReadKey();
         Environment.Exit(1);
      }
      
      if (!ok) {
         Console.WriteLine("Failed to connect to " + 
                           Client.IpAddress + ":" + Client.Port);
         Console.ReadKey();
         Environment.Exit(1);
      }
      
      try {
         var file = args != null && args.Length > 0 ? args[0] : "test_plan.txt";
         var plan = Client.LoadPlan(file);
         ok = ok && Client.SendPlan(plan);
         
         // wait for a second
         Thread.Sleep(1000);
         
         // start plan processing on the server
         
         ok = ok && Client.StartPlan();
         
         if (!ok) {
            Console.WriteLine("Failed to start plan processing");
            Console.ReadKey();
            Environment.Exit(1);
         }
      } catch(Exception ex){
         Console.WriteLine("Error: " + ex.Message);
         Console.ReadKey();
         Environment.Exit(1);
      }
      
      Console.WriteLine("\nWaiting untill plan processing is over");
      Console.WriteLine("__________________________________________________________________________");
      Console.ReadKey();
      Environment.Exit(0);
   }
   
   /// <summary>
   /// Called when server state was changed:
   ///   NOTREADY, READY, INPROCESS, PAUSED, FINISHED
   /// </summary>
   private static void OnStateChanged(ECommandState state)
   {
      if (state == ECommandState.INPROCESS) { // plan processing is ON
         Console.WriteLine("Spot processed/total = " + Client.SpotsPassed + "/" + Client.SpotsTotal);
      } else {
         Console.WriteLine(state);
      } 
      
      if (state == ECommandState.FINISHED) {
         Console.WriteLine("\nDump results of plan processing");
         Console.WriteLine("_______________________________________________________________________");
         
         if (Client.PlanResults.Count == 0) {
            Console.WriteLine("Server Main Control programm is in FINISHED state!");
            Console.WriteLine("Restart the program!");
            Client.StopPlan();
            Client.ClearPlan();
            Console.WriteLine("Press any key to exit session ...");
            Console.SetOut(new NulTextWriter());
            //var fileName = Assembly.GetExecutingAssembly().Location;
            Console.ReadKey();
            //System.Diagnostics.Process.Start(fileName);
            Environment.Exit(0);
         }
         
         Client.DumpPlanResults();

         Console.WriteLine("\nFINISHED");
         Console.WriteLine("Press any key to exit session ...");
         Console.ReadKey();
         Environment.Exit(0);
      }
   }

   /// <summary>
   /// Called when server was diconnected
   /// </summary>
   private static void OnDisconnected()
   {
      Console.WriteLine("Server " + Client.IpAddress + ":" + Client.Port + " diconnected");
   }
   
   public sealed class NulTextWriter: TextWriter
   {
      public override Encoding Encoding
      {
        get
        {
            return Encoding.UTF8;
        }
      }
   } 
}

"@

$currentScriptDirectory = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
[System.IO.Directory]::SetCurrentDirectory($currentScriptDirectory)
$dllpath = Join-Path $currentScriptDirectory 'TMClient.dll'
$asm = [System.Reflection.Assembly]::LoadFrom($dllpath)
 
Add-Type -ReferencedAssemblies $asm -TypeDefinition $code -Language CSharp -OutputAssembly "$currentScriptDirectory\ProcessPlan.exe" -OutputType ConsoleApplication

# execute the code
$plan = Join-Path $currentScriptDirectory test_plan.txt
$program = Join-Path $currentScriptDirectory ProcessPlan.exe
Start-Process $program $plan

