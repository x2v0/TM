��� ���������� �������� ������� ������������� ���������� **TMClient.dll**  
��� ��������� �������� ���������, �� ������� ������ ���� �������� ��������� **MainCSimulator**.


## ������������� TMClient.dll � �������� ������������ ���������� 

������ �� ������ �������� ���������� C# ���, ������� ���������� **TMClient.dll**, ��� ������������ ����������. 

- **DumpPlan.ps1**       - ��������� � ������������� ���� ��������� � ���� ������� � GridView ��������.  
C# ��� ���������� � ����� **DumpPlan.cs**, ������� ����������� � �������� ����������.

      //
      using System;
      using System.IO;
      using System.Text;
      using System.Collections.Generic;
      using TM;
      using TM.Properties;
      
      namespace Dump
      {
      
         public class Plan
         {
            public Dictionary<int, PlanSpot> Load()
            {  
               var plan = TMClient.LoadPlanData("test_plan.txt");

               Console.WriteLine("\n"+"��� ���������� ����� ��������� ������� ����� �������"+" ...");
               Console.WriteLine("__________________________________________________________________________");
               Console.ReadKey();
         
               return plan;
            }
         }
      }

- **Connect2Server.ps1** - ������ ����������� � ��������� �������, �� ������� �������������� ���� ���������.  
                           ��������� ����������� ��������� �� ������� �������.   
                           �� ������, ������������ ��������� Ready/Not-Ready.



      //
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
                     // connect to server - "localhost:9996" 
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



- **ProcessPlan.ps1**    - ������������������� ������ ��������� ����� ��������� �� �������� �������.  
                           ���� �������� ���������� C# ���, ������� ����� ���������� ������� �������������
                           � ��������� **ProcessPlan.exe**
 


      //
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


- **ExecutePlan.ps1**    - ������ ������������� ������ **TMClient.ExecutePlan("test_plan.txt", "localhost", 9996)**,  
������� ��������� ��������� ���� ��������� � ������� ������ ������������� ������.



      //       
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
               
                     Console.WriteLine("Dump plan results");
                     client.DumpPlanResults();
                     Console.ReadKey();
                     Environment.Exit(0);
                  }
               }
            }



��� ������� ����� ��������������� ����� ���������:

1 ������:

- ������� ����� �� �����-���� �� ������������� ������, �������� ���
- ������ ������ ������� �����
- ������� "������� � �������" -> "Windows PowerShell"

2 ������:

- ��������� PowerShell
- ������� � ����������, ��� ����������� ��� ����� `PS>cd xxx\Test`
- ��������� �����-���� �� ���� ������ `PS>./ExecutePlan.ps1`

 
## ������������� TMClient.dll � �������� PowerShell ������ 

**PrintCommands.ps1** ������� ������ ��������� ������ � ����� **PowerShell**: 

- `Clear-Plan [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Connect-Server [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Disconnect-Server [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Get-Plan [-Path] <string> [-debug]`
- `Get-Results [-debug]`
- `Get-ServerState [[-WaitTime] [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Invoke-Plan [-Path] <string> [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Send-Command [-Command] <EPlanCommand> [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Send-Data [Data] <byte[]> [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Send-Info [-Info] <string> [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Send-Plan [[-Plan] <Object>] [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Set-DefaultServer [-IpAddress] <string> [-Port] <int> [-debug]`
- `Start-Plan [-Resume] [[-IpAddress] <string>] [[-Port] <int>] [ [-debug]`
- `Stop-Plan [[-IpAddress] <string>] [[-Port] <int>] [-debug]`
- `Suspend-Plan [[-IpAddress] <string>] [[-Port] <int>] [-debug]`


###  ������ ����������� ������


      # 
         # �������� ������
         PS>Import-Module ./TMClient.dll

         # ������� ������:����, �� ������� ����� ����������� ���� ���������
         PS>Set-DefaultServer localhost 9996

         # ������ ����� �� ����� "test_plan.txt" � �������� ��� �� ������
         PS>$client = Get-Plan test_plan.txt | Send-Plan

         # ����������� ������� (Event) "PlanFinished", ������� ����� ��������� ������-����,  
         # ����������� � ������� {...}. ��� ������ ���������� ���������� ����� � ���� �������  
         # ����������� ������-���� - { Get-Results | Write-Output | Format-Table }  
         PS>Register-ObjectEvent $client PlanFinished -Action { Get-Results | Write-Output | Format-Table }

         # ������� �� ������ ������� ������� ����� ���������
         PS>Start-Plan | Out-Null

      
���������� ���� �� ������������, ������� ����� ���������������� **Suspend-Plan** �   
������������ **Start-Plan -Resume** ������� ���������� ����� ���������.

��������� ���������� ����� ��������� ����� ������ { Get-Results | Write-Output | Format-Table }:   

      #
         id    xangle    zangle energy   pcount done result_xangle result_zangle result_pcount changed
         --    ------    ------ ------   ------ ---- ------------- ------------- ------------- -------
          1 -0,007468 -0,004124   97,5 2,47E+07    1     -0,007468     -0,004124      2,47E+07       0
          4 -0,010635 -0,004124     98 1,08E+07    1     -0,010635     -0,004124      1,08E+07       0
          5 -0,011426 -0,004124   98,5  6180000    1     -0,011426     -0,004124       6180000       0
          6 -0,011822 -0,004124   98,5  8080000    1     -0,011822     -0,004124       8080000       0
         10  -0,01103 -0,003729     98  9760000    1      -0,01103     -0,003729       9760000       0
         11 -0,014988 -0,003729     98 1,55E+07    1     -0,014988     -0,003729      1,55E+07       0
         12 -0,016176 -0,003729     98 3,34E+07    1     -0,016176     -0,003729      3,34E+07       0
         13 -0,007468 -0,002541     98  7020000    1     -0,007468     -0,002541       7020000       0
         15 -0,007468  -0,00175     98  7820000    1     -0,007468      -0,00175       7820000       0
         17 -0,013009  -0,00175   98,5  1,3E+07    1     -0,013009      -0,00175       1,3E+07       0
         18 -0,016176  -0,00175   97,5  5490000    1     -0,016176      -0,00175       5490000       0
         19 -0,012218 -0,001354   98,5  5050000    1     -0,012218     -0,001354       5050000       0
         22 -0,007468 -0,000562     98  5720000    1     -0,007468     -0,000562       5720000       0
         25 -0,007468 -0,000166     98 1,45E+07    1     -0,007468     -0,000166      1,45E+07       0
         29 -0,016176 -0,000166   97,5  2,2E+07    1     -0,016176     -0,000166       2,2E+07       0
         30 -0,007072  0,000229   74,5 1,44E+07    1     -0,007072      0,000229      1,44E+07       0
         32 -0,013009  0,000229     98  5160000    1     -0,013009      0,000229       5160000       0
         33 -0,016176  0,000229   97,5  5500000    1     -0,016176      0,000229       5500000       0
         34 -0,007468  0,002208     98 3,96E+07    1     -0,007468      0,002208      3,96E+07       0
         36  -0,01103  0,002208     98 2,02E+07    1      -0,01103      0,002208      2,02E+07       0
         37 -0,011426  0,002208   97,5  7100000    1     -0,011426      0,002208       7100000       0
         41 -0,013009  0,002208     98  8130000    1     -0,013009      0,002208       8130000       0
         43  -0,01578  0,002208     97  9150000    1      -0,01578      0,002208       9150000       0
         44 -0,016176  0,002208     97 2,69E+07    1     -0,016176      0,002208      2,69E+07       0
         46 -0,007468 -0,004124     96 1,59E+07    1     -0,007468     -0,004124      1,59E+07       0
         50 -0,013405 -0,004124     96  5410000    1     -0,013405     -0,004124       5410000       0


� ������ �������, ����������  **$client** - ��� ������ **TMClient**, ������� �������� ��� ���������� � ������.

������ ����� **$client** :

| ���� | ������ | 
| ----------- | -----------  |
| Header           | sign = MCSv, type = Data, value = STARTPLAN, datalength = 17, packet_number = 17 |
| IpAddress        | localhost |
| IsConnected      | True |
| LocalEndPoint    | 127.0.0.1:51958 |
| LocalIpAddress   | 127.0.0.1 |
| LocalPort        | 51958 |
| MCS_State_Server | ������ ������� : state = ����� ������� �������� ������������� �� �������� �������, | 
|                  |                  lasterror = 0, spots_passed = 52, spots_count = 52 |
| Plan             | ( 0, id = 0, xangle = -0,007072, zangle = -0,004124, energy = 74,5, pcount = 3410000), ... |
| PlanResults      | (1, id = 1, result_xangle = -0,007468, result_zangle = -0,004124, result_pcount = 2,47E+07, done = 1 ), ... |
| Port             | 9996 |
| ProcessingIsOn   | False |
| RemoteEndPoint   | 127.0.0.1:9996 |
| ServerState      | FINISHED |
| SpotsPassed      | 52 |
| SpotsTotal       | 52 |
| ReadData         | TM.BufferChunk |
| Sender           | System.Net.Sockets.TcpClient |


### ������ ���������� ������

���������� ���������� ����� ��������� ����������� � ������� ����� ������������ �������:

**$result = Invoke-Plan test_plan.txt localhost 9996**

� ���������� ���� ���������� ��������� ��������:
- ������� ������:����, �� ������� ����� ����������� ���� ���������
- ������ ����� �� ����� "test_plan.txt" � �������� ��� �� ������
- ������� �� ������ ������� ������� ����� ���������

��� ���� ���������� ���� ����� ������������.  
��� ����, ����� ��� �������������� � ������������� ���� ��������� ���������� ������ **Control-C**  
��� ������������� ���������� ����� ��������� ���������� ������� ������� **Start-Plan -Resume**  
��� ������ ���������� ����� ��������� ����� ������� ������� **Write-Output $result | Format-Table**
 

### ������ � ���������� ����� �������� � ���� GridView ��������

      #
         $plan = Get-Plan test_plan.txt  
         $rad2deg = 57.2957795130823230
         $spots = $plan | 
               Select-Object id, 
               @{Label="X-angle (degree)";  Expression={$PSItem.xangle*$rad2deg}}, 
               @{Label="Z-angle (degree)";  Expression={$PSItem.zangle*$rad2deg}},
               @{Label="Energy (MeV)";      Expression={$PSItem.energy}},
               @{Label="Number of protons"; Expression={$PSItem.pcount}}
               
         Write-Output $spots | Out-GridView -Title "Plan"

         # ��������� ����� �� ������
         Send-Plan $plan


### ������, ��������, ������� ������� ���� TMClient

**#������ ������� (Events)**  
PS> ([TM.TMClient]).GetEvents() | Select Name   

      #
         Name
         ----
         DataBlockReceived
         ErrorReceived
         InfoReceived
         PlanFinished
         PlanLoaded
         PlanResultsProcessed
         ServerConnected
         ServerDisconnected
         ServerStateChanged


**#������ ������� (Properties)**  
PS> ([TM.TMClient]).GetProperties() | Select Name 

      #
         Name
         ----
         Debug
         DebugPreference
         Header
         IpAddress
         IsConnected
         LocalEndPoint
         LocalIpAddress
         LocalPort
         MCS_State_Server
         Plan
         PlanResults
         Port
         ProcessingIsOn
         RemoteEndPoint
         ServerState
         SpotsPassed
         SpotsTotal


**#������ �������**  
PS> ([TM.TMClient]).GetMethds() | Select Name | Select-String -pattern '_' -NotMatch

      #
         @{Name=Dispose}
         @{Name=DumpPlan}
         @{Name=DumpPlanResults}
         @{Name=LoadPlanData}
         @{Name=SendPlan}
         @{Name=SendPlan}
         @{Name=AskServerState}
         @{Name=ClearPlan}
         @{Name=Connect}
         @{Name=Disconnect}
         @{Name=DumpPlanResults}
         @{Name=ExecutePlan}
         @{Name=LoadPlan}
         @{Name=PausePlan}
         @{Name=Reset}
         @{Name=SendCommand}
         @{Name=SendData}
         @{Name=SendData}
         @{Name=SendData}
         @{Name=SendInfo}
         @{Name=SendPlan}
         @{Name=SendPlan}
         @{Name=SendPlan}
         @{Name=StartPlan}
         @{Name=StopPlan}
         @{Name=Equals}
         @{Name=GetHashCode}
         @{Name=GetType}
         @{Name=ToString}

