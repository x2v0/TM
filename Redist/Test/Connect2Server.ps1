$id = get-random

$code = @"
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using TM;

namespace Connect$id
{
	public class Program
	{
      private static TMClient Client;
      
		public static void Main()
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
            // connect to server
            ok = Client.Connect("localhost", 9996);
         } catch(Exception ex){
            Console.WriteLine("Failed to connect! " + ex.Message);
            return;
         }
         
         if (!ok) {
            Console.WriteLine("Failed to connect to " + 
                              Client.IpAddress + ":" + Client.Port);
            return;
         }
         
         ok = Client.SendInfo("Hello Server!");
         ok = Client.SendCommand(EPlanCommand.GETSTATE);
         
         Console.WriteLine("\nWaiting for server events. Click ON/OFF \"Ready\" button on MainCSimulator");
         Console.WriteLine("Press any key to exit session...");
         Console.WriteLine("__________________________________________________________________________");
         Console.ReadKey();
		}
      
      /// <summary>
      /// Called when server state was changed:
      ///   NOTREADY, READY, INPROCESS, PAUSED, FINISHED
      /// </summary>
      private static void OnStateChanged(ECommandState state)
      {
         if (state == ECommandState.INPROCESS) { // plan processing
            Console.WriteLine("Spot processed/total = " + Client.SpotsPassed + "/" + Client.SpotsTotal);
         } else {
            Console.WriteLine(state);
         }
      }
   
      /// <summary>
      /// Called when server was diconnected
      /// </summary>
      private static void OnDisconnected()
      {
         Console.WriteLine("Server " + Client.IpAddress + ":" + Client.Port + " diconnected");
      }
	}
}
"@

$currentScriptDirectory = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
[System.IO.Directory]::SetCurrentDirectory($currentScriptDirectory)
$dllpath = Join-Path $currentScriptDirectory 'TMClient.dll'
$asm = [System.Reflection.Assembly]::LoadFrom($dllpath)
 
Add-Type -ReferencedAssemblies $asm -TypeDefinition $code -Language CSharp

# execute the code
Invoke-Expression "[Connect$id.Program]::Main()"
