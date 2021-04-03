// $Id: $


/*************************************************************************
 *                                                                       *
 * Copyright (C) 2021,   Valeriy Onuchin                                 *
 * All rights reserved.                                                  *
 *                                                                       *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TM;
using TM.Properties;
using TMSrv;


#region  Delegates

/// <summary>
///    Delegate ClientDataHandler
/// </summary>
/// <param name="data">The data.</param>
/// <param name="bytesRead">The bytes read.</param>
public delegate void ClientDataHandler(BufferChunk data = null, int bytesRead = 0);

/// <summary>
///    Delegate ClientHandler
/// </summary>
public delegate void ClientHandler();

/// <summary>
///    Delegate PlanResultsHandler
/// </summary>
/// <param name="results">The results.</param>
public delegate void PlanResultsHandler(List<PlanSpotResult> results);

/// <summary>
///    Delegate ServerStateChangedHandler
/// </summary>
/// <param name="state">The state.</param>
public delegate void ServerStateChangedHandler(ECommandState state);

#endregion
namespace TM
{
   /// <summary>
   ///    Class Globals == Default values and global objects
   /// </summary>
   public static class Globals
   {
      #region Static fields

      /// <summary>
      ///    GLOBAL Client
      /// </summary>
      public static TMClient Client;

      /// <summary>
      ///    The default IP
      /// </summary>
      public static string IP = "localhost";

      /// <summary>
      ///    The default port
      /// </summary>
      public static int Port = 9996;

      private static string fLanguage = "ru";

      #endregion

      #region Public properties

      public static string Language
      {
         get
         {
            return fLanguage;
         }

         set
         {
            fLanguage = value;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(fLanguage);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(fLanguage);

            var customCulture = (CultureInfo) Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            Thread.CurrentThread.CurrentCulture = customCulture;
         }
      }

      #endregion
   }

   /// <summary>
   ///    Class TMClient.
   ///    <br />Implements the <see cref="System.IDisposable" />
   /// </summary>
   /// <seealso cref="System.IDisposable" />
   public class TMClient : IDisposable
   {
      #region Static fields

      /// <summary>
      ///    The default value of connection try out count
      /// </summary>
      public static int ConnectionTryCount = 5;

      #endregion

      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="TMClient" /> class.
      /// </summary>
      public TMClient()
      {
         Plan = new List<PlanSpot>();
         PlanResults = new List<PlanSpotResult>();
         DebugPreference = 2; // ActionPreference.Continue == DEBUG is ON

         Globals.Client = this;
         Globals.Language = "ru";
      }

      #endregion

      #region  Fields

      /// <summary>
      ///    The read data
      /// </summary>
      public BufferChunk ReadData;

      /// <summary>
      ///    The TCP sender
      /// </summary>
      public TcpClient Sender;

      /// <summary>
      ///    The connection try out count
      /// </summary>
      private int fConnectionTryCount;

      /// <summary>
      ///    The listening thread. Process data from received from server.
      /// </summary>
      private Thread fListenThread;

      /// <summary>
      ///    The network stream
      /// </summary>
      private NetworkStream fNetworkStream;

      #endregion

      #region Public events

      /// <summary>
      ///    Occurs when [data block received].
      /// </summary>
      public event ClientDataHandler DataBlockReceived;

      /// <summary>
      ///    Occurs when [on error received].
      /// </summary>
      public event ClientHandler ErrorReceived;

      /// <summary>
      ///    Occurs when [on info received].
      /// </summary>
      public event ClientHandler InfoReceived;

      /// <summary>
      ///    Occurs when [plan processing is finished].
      /// </summary>
      public event ClientHandler PlanFinished;

      /// <summary>
      ///    Occurs when [plan loaded].
      /// </summary>
      public event ClientHandler PlanLoaded;

      /// <summary>
      ///    Occurs when part of [plan results processed and received].
      /// </summary>
      public event PlanResultsHandler PlanResultsProcessed;

      /// <summary>
      ///    Occurs when [server connected].
      /// </summary>
      public event ClientHandler ServerConnected;

      /// <summary>
      ///    Occurs when [server disconnected].
      /// </summary>
      public event ClientHandler ServerDisconnected;

      /// <summary>
      ///    Occurs when [server state changed].
      /// </summary>
      public event ServerStateChangedHandler ServerStateChanged;

      #endregion

      #region Public properties

      /// <summary>
      ///    Set Debugging ON/OFF
      /// </summary>
      /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
      public static bool Debug
      {
         get
         {
            return DebugPreference == 2;
         }

         set
         {
            DebugPreference = value ? 2 : 0;
         }
      }

      /// <summary>
      ///    Enum.ActionPreference:
      ///    <code>
      /// Continue 	      2 	- Debug is ON
      /// SilentlyContinue 	0 	- Debug is OFF
      /// </code>
      /// </summary>
      /// <value>The debug preference.</value>
      public static int DebugPreference
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets the packet header.
      /// </summary>
      /// <value>The header.</value>
      public TMPacketHeader Header
      {
         get;
         private set;
      }

      /// <summary>
      ///    Gets the server IP address.
      /// </summary>
      /// <value>The IP address.</value>
      public string IpAddress
      {
         get;
         set;
      }
      public string IP
      {
         get
         {
            return IpAddress;
         }
      }

      /// <summary>
      ///    Gets a value indicating whether this <see cref="TMClient" /> is connected.
      /// </summary>
      /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
      public bool IsConnected
      {
         get
         {
            try {
               if ((Sender != null) &&
                   (Sender.Client != null) &&
                   Sender.Client.Connected) {
                  /* pear to Sender documentation on Poll:
                   * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                   * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                   * -or- true if data is available for reading; 
                   * -or- true if the connection has been closed, reset, or terminated; 
                   * otherwise, returns false
                   */

                  // Detect if client disconnected
                  if (Sender.Client.Poll(0, SelectMode.SelectRead)) {
                     var buff = new byte[1];
                     if (Sender.Client.Receive(buff, SocketFlags.Peek) == 0) {
                        // Client disconnected
                        return false;
                     }

                     return true;
                  }

                  return true;
               }

               return false;
            } catch {
               return false;
            }
         }
      }

      /// <summary>
      ///    Gets the local end point.
      /// </summary>
      /// <value>The local end point.</value>
      public IPEndPoint LocalEndPoint
      {
         get
         {
            return ((Sender != null) && Sender.Connected ? Sender.Client.LocalEndPoint : null) as IPEndPoint;
         }
      }

      /// <summary>
      ///    Gets the local IP address.
      /// </summary>
      /// <value>The local IP address.</value>
      public string LocalIpAddress
      {
         get
         {
            return LocalEndPoint != null ? LocalEndPoint.Address.ToString() : string.Empty;
         }
      }

      /// <summary>
      ///    Gets the local port.
      /// </summary>
      /// <value>The local port.</value>
      public int LocalPort
      {
         get
         {
            return LocalEndPoint != null ? LocalEndPoint.Port : 0;
         }
      }

      /// <summary>
      ///    Gets the MCS_State_Server structure.
      /// </summary>
      /// <value>The server.</value>
      public MCS_State_topass MCS_State_Server
      {
         get;
         private set;
      }

      /// <summary>
      ///    Loaded plan data
      /// </summary>
      /// <value>The plan.</value>
      public List<PlanSpot> Plan
      {
         get;
         private set;
      }

      /// <summary>
      ///    Processed plan results
      /// </summary>
      /// <value>The plan results.</value>
      public List<PlanSpotResult> PlanResults
      {
         get;
         private set;
      }

      /// <summary>
      ///    Gets the remote server port.
      /// </summary>
      /// <value>The remote port.</value>
      public int Port
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets a value indicating whether [processing is on].
      /// </summary>
      /// <value><c>true</c> if [processing is on]; otherwise, <c>false</c>.</value>
      public bool ProcessingIsOn
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets the remote end point.
      /// </summary>
      /// <value>The remote end point.</value>
      public IPEndPoint RemoteEndPoint
      {
         get
         {
            return ((Sender != null) && Sender.Connected ? Sender.Client.RemoteEndPoint : null) as IPEndPoint;
         }
      }

      /// <summary>
      ///    Gets the state of the server.
      /// </summary>
      /// <value>The state of the server.</value>
      public ECommandState ServerState
      {
         get;
         private set;
      }

      /// <summary>
      ///    Gets the number of spots processed.
      /// </summary>
      /// <value>The spots passed.</value>
      public uint SpotsPassed
      {
         get;
         private set;
      }

      /// <summary>
      ///    Gets the number of spots total in plan.
      /// </summary>
      /// <value>The spots total.</value>
      public uint SpotsTotal
      {
         get;
         private set;
      }

      #endregion

      #region Interface methods

      /// <summary>
      ///    Performs application-defined tasks associated with freeing,
      ///    releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose()
      {
         Reset();
      }

      #endregion

      #region Public methods

      /// <summary>
      ///    Dumps the plan data.
      /// </summary>
      /// <param name="plan">The plan data.</param>
      public static void DumpPlan(List<PlanSpot> plan)
      {
         try {
            foreach (var spot in plan) {
               Console.WriteLine(spot);
            }
         } catch {
            // ignored
         }
      }

      /// <summary>
      ///    Dumps the plan results.
      /// </summary>
      /// <param name="results">The results.</param>
      public static void DumpPlanResults(List<PlanSpotResult> results)
      {
         try {
            foreach (var spot in results) {
               Console.WriteLine(spot);
            }
         } catch {
            // ignored
         }
      }

      /// <summary>
      ///    Reads the file and loads the plan data.
      /// </summary>
      /// <param name="file">The file with plan data.</param>
      /// <returns>BufferChunk. The raw array of bytes</returns>
      /// <exception cref="System.IO.FileNotFoundException"></exception>
      /// <exception cref="ReadPlanException">
      /// </exception>
      /// <exception cref="FileNotFoundException"></exception>
      public static List<PlanSpot> LoadPlanData(string file)
      {
         if (string.IsNullOrEmpty(file) ||
             !File.Exists(file)) {
            if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
               Console.WriteLine(Resources.Loading_PlanData + " : " + Resources.file_not_found + " - " + file);
            }

            throw new FileNotFoundException();
         }

         var cnt = 0;
         uint length = 0;

         try {
            if (Globals.Client == null) {
               Globals.Client = new TMClient();
            }

            var r = new Regex(@"\s+");

            if (DebugPreference == 2) { // ActionPreference.Continue
               Console.WriteLine(Resources.Loading_PlanData + ", " + Resources.file + " - " + file);
            }

            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read)) {
               using (var sr = new StreamReader(fs, Encoding.UTF8)) {
                  string line;

                  while ((line = sr.ReadLine()) != null) {
                     if (string.IsNullOrEmpty(line) ||
                         line.StartsWith("//")) {
                        continue;
                     }

                     if ((line == "\n") ||
                         (line == "\r\n")) {
                        continue;
                     }

                     var parts = r.Split(line);
                     var spot = new PlanSpot();

                     try {
                        spot.id = cnt;
                        spot.xangle = float.Parse(parts[0], CultureInfo.InvariantCulture);
                        spot.zangle = float.Parse(parts[1], CultureInfo.InvariantCulture);
                        spot.energy = float.Parse(parts[2], CultureInfo.InvariantCulture);
                        spot.pcount = float.Parse(parts[3], CultureInfo.InvariantCulture);

                        cnt++;
                        length += PlanSpot.Length;
                        Globals.Client.Plan.Add(spot);
                     } catch (Exception ex) {
                        if (DebugPreference == 2) { // ActionPreference.Continue
                           Console.WriteLine(Resources.Failed_to_load + " PlanData (" + Resources.wrong_format_data + "), " + Resources.file + " - " + file + "\nentries = " + cnt + " " +
                                             Resources.Error + ": " + ex.Message);
                        }

                        throw new ReadPlanException(file);
                     }
                  }
               }

               if (DebugPreference == 2) { // ActionPreference.Continue
                  Console.WriteLine("PlanData" + " " + Resources.loaded + ": entries = " + cnt + ", size = " + (length / 1000.0) + " Kb");
               }
            }

            return Globals.Client.Plan;
         } catch (Exception ex) {
            if (DebugPreference == 2) { // ActionPreference.Continue  = Debugging is ON
               Console.WriteLine(Resources.Failed_to_load + " " + "PlanData" + ", " + Resources.file + " - " + file + "\nentries = " + cnt + " " + Resources.Error + ": " + ex.Message);
            }

            throw new ReadPlanException(file);
            return null;
         }

         return null;
      }

      /// <summary>
      ///    Sends the plan to server.
      /// </summary>
      /// <param name="client">The client.</param>
      /// <param name="spots">The spots.</param>
      /// <param name="nblocks">The nblocks.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendPlanException">
      /// </exception>
      public static bool SendPlan(TMClient client, List<PlanSpot> spots, uint nblocks = 10)
      {
         var ok = false;
         client.SendCommand(EPlanCommand.CLEARPLAN);

         BufferChunk.SetNetworking();
         var plan = new BufferChunk();

         foreach (var spot in spots) {
            plan.Add(spot);
         }

         var len = PlanSpot.Length * nblocks;
         if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
            Console.WriteLine(Resources.Sending_plan_to_server + ": length = " + (plan.Length / 1000.0) + " Kb");
         }

         try {
            while (plan.Length > len) {
               var bf = plan.NextBufferChunk((int) len);
               client.SendData(bf);
            }

            try {
               if (plan.Length >= PlanSpot.Length) {
                  var bf = plan.NextBufferChunk(plan.Length);
                  client.SendData(bf);
               }
            } catch {
               throw new SendPlanException();
               return false;
            }
         } catch {
            if (plan.Length >= PlanSpot.Length) { // send the last portion of data
               try {
                  var bf = plan.NextBufferChunk(plan.Length);
                  client.SendData(bf);
               } catch {
                  throw new SendPlanException();
                  return false;
               }
            }
         }

         if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
            Console.WriteLine(Resources.Plan_sent_to_server + ".");
         }

         client.SendCommand(EPlanCommand.GETSTATE);
         return true;
      }

      /// <summary>
      ///    Sends the plan data to server.
      /// </summary>
      /// <param name="client">The client.</param>
      /// <param name="spots">The plan as list of spots.</param>
      /// <param name="nblocks">The nblocks.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendPlanException"></exception>
      public static bool SendPlan(TMClient client, Dictionary<int, PlanSpot> spots, uint nblocks = 10)
      {
         var list = new List<PlanSpot>();
         foreach (var spot in spots.Values) {
            list.Add(spot);
         }

         return SendPlan(client, list, nblocks);
      }

      /// <summary>
      ///    SendCommand(EPlanCommand.GETSTATE); to the server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool AskServerState()
      {
         return SendCommand(EPlanCommand.GETSTATE);
      }

      /// <summary>
      ///    Clears the plan.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool ClearPlan()
      {
         return SendCommand(EPlanCommand.CLEARPLAN);
      }

      /// <summary>
      ///    Connects the specified ip.
      /// </summary>
      /// <param name="ip">The ip.</param>
      /// <param name="port">The port.</param>
      /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
      public bool Connect(string ip = null, int port = 0)
      {
         Port = port;
         IpAddress = ip;

         if (ip == null) {
            IpAddress = Globals.IP;
         }

         if (port == 0) {
            Port = Globals.Port;
         }

         var ok = CreateSender(IpAddress, Port);

         if (!ok) {
            if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
               Console.WriteLine(Resources.Failed_to_connect_to + " " + IpAddress + " , " + Resources.port_number + " = " + Port);
            }

            return false;
         }

         fListenThread = new Thread(ListenForData);
         fListenThread.Start();

         if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
            Console.WriteLine(Resources.Connected_to + " " + IpAddress + " , " + Resources.port_number + " = " + Port);
         }

         if (ServerConnected != null) {
            ServerConnected();
         }

         return true;
      }

      /// <summary>
      ///    Disconnects this TMClient.
      /// </summary>
      /// <returns><c>true</c> if disconnect is OK, <c>false</c> otherwise.</returns>
      public bool Disconnect()
      {
         if (ServerDisconnected != null) {
            ServerDisconnected();
         }

         if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
            Console.WriteLine(Resources.Disconnected_from + " " + IpAddress + ":" + Port);
         }

         Reset();

         return true;
      }

      /// <summary>
      ///    Dumps the plan results.
      /// </summary>
      public void DumpPlanResults()
      {
         DumpPlanResults(PlanResults);
      }

      /// <summary>
      ///    <code>
      /// LoadPlan(file) - loads the specified file with plan data.
      /// SendPlan()     - sends plan to the server specified by ip nad port
      /// StartPlan()    - starts plan processing on the server
      /// while (ProcessingIsOn) - waits for results of processing
      /// when results of plan processing received from server, fills PlanResults list
      /// if (ServerState == ECommandState.FINISHED) - execute PlanFinished() event
      /// </code>
      /// </summary>
      /// <param name="file">The file with plan data.</param>
      /// <param name="ip">The server IP.</param>
      /// <param name="port">The port.</param>
      /// <returns>Dictionary&lt;System.Int32, PlanSpotFull&gt;.</returns>
      public List<PlanSpotFull> ExecutePlan(string file, string ip = null, int port = 0)
      {
         Reset();
         ServerStateChanged += OnServerStateChanged;
         ProcessingIsOn = false;

         var ok = Connect(ip, port);

         if (!ok) {
            return null;
         }

         ClearPlan();

         ok = LoadPlan(file) != null;

         if (!ok) {
            return null;
         }

         ok = SendPlan();

         if (!ok) {
            if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
               Console.WriteLine(Resources.Failed_to_send + " " + Resources.plan);
            }

            return null;
         }

         ok = StartPlan();

         ProcessingIsOn = true;

         while (ProcessingIsOn) {
            ok = AskServerState();

            if (!ok ||
                (ServerState == ECommandState.NOTREADY)) {
               if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
                  Console.WriteLine(Resources.Server_not_ready);
               }

               //return null;
            }

            if ((ServerState == ECommandState.FINISHED) &&
                (PlanResults.Count > 1)) {
               ProcessingIsOn = false;

               if (PlanFinished != null) {
                  PlanFinished();
               }
            }

            Thread.Sleep(300);
         }

         var ret = new List<PlanSpotFull>();

         foreach (var spot in PlanResults) {
            var plan = Plan[spot.id];
            var full = new PlanSpotFull();
            full.id = spot.id;
            full.xangle = plan.xangle;
            full.zangle = plan.zangle;
            full.pcount = plan.pcount;
            full.energy = plan.energy;
            full.result_xangle = spot.result_xangle;
            full.result_zangle = spot.result_zangle;
            full.result_pcount = spot.result_pcount;
            full.done = spot.done;
            //full.changed = spot.Value.changed;
            ret.Add(full);
         }

         return ret;
      }

      /// <summary>
      ///    Loads the specified file with plan data.
      /// </summary>
      /// <param name="file">The file.</param>
      /// <returns>Dictionary&lt;System.Int32, PlanSpot&gt;.</returns>
      public List<PlanSpot> LoadPlan(string file)
      {
         if (Plan == null) {
            Plan = new List<PlanSpot>();
         }

         Plan = LoadPlanData(file);

         if (PlanLoaded != null) {
            PlanLoaded();
         }

         return Plan;
      }

      /// <summary>
      ///    Pauses the plan processing on server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool PausePlan()
      {
         return SendCommand(EPlanCommand.PAUSEPLAN);
      }

      /// <summary>
      ///    Resets this instance.
      /// </summary>
      public void Reset()
      {
         ServerStateChanged -= OnServerStateChanged;
         ProcessingIsOn = false;

         fListenThread = null;

         if ((Sender != null) &&
             Sender.Connected) {
            fNetworkStream.Close();
            fNetworkStream = null;
         }

         if (Sender != null) {
            Sender.Close();
         }

         Sender = null;
         PlanResults.Clear();
      }

      /// <summary>
      ///    Sends the EPlanCommand to server.
      ///    <code>
      /// public enum EPlanCommand
      /// {
      ///    [Description("запрос на статус сервера")]
      ///    GETSTATE = 1,
      ///    
      ///    [Description("запрос на очистку плана ")]
      ///    CLEARPLAN = 2,
      ///  
      ///    [Description("запрос на старт плана ")]
      ///    STARTPLAN = 3,
      ///  
      ///    [Description("запрос на паузу")]
      ///    PAUSEPLAN = 4,
      ///  
      ///    [Description("запрос на останов")]
      ///    STOPPLAN = 5
      /// }
      /// </code>
      /// </summary>
      /// <param name="cmd">The EPlanCommand.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendCommandException"></exception>
      public bool SendCommand(EPlanCommand cmd, EServerType server_type = EServerType.MCS)
      {
         bool ret;

         if (Sender == null) {
            return false;
         }

         if (cmd == EPlanCommand.CLEARPLAN) {
            PlanResults.Clear();
         }

         try {
            var packet = new TMPacket(server_type, EPacketType.Command, (byte) cmd);
            if (cmd != EPlanCommand.GETSTATE) {
               if (DebugPreference == 2) { // ActionPreference.Continue
                  Console.WriteLine(Resources.Sending_command_to_server + ": " + cmd.Description());
               }
            }

            ret = SendPacket(packet);
         } catch (Exception ex) {
            var msg = "SendCommand : " + cmd + " - " + ex.Message;

            if (DebugPreference == 2) { // ActionPreference.Continue
               Console.WriteLine(msg);
            }

            throw new SendCommandException(msg);
            return false;
         }

         return ret;
      }

      /// <summary>
      ///    Sends the plan as byte array to server.
      /// </summary>
      /// <param name="data">The data.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendDataException"></exception>
      public bool SendData(byte[] data, EServerType server_type = EServerType.MCS)
      {
         bool ret;

         if (Sender == null) {
            return false;
         }

         try {
#if LOCAL_DEBUG
            Console.WriteLine("Sending data to server ... ");
#endif
            var packet = new TMPacket(server_type, EPacketType.Data, (byte) EDataCommand.SHOTSBLOCK, 0, data);
            ret = SendPacket(packet);
         } catch (Exception ex) {
            var msg = "SendData : " + ex.Message;

            if (DebugPreference == 2) { // ActionPreference.Continue
               Console.WriteLine(msg);
            }

            throw new SendDataException(msg);
         }

#if LOCAL_DEBUG
         Console.WriteLine("Data sent to server");
#endif
         return ret;
      }

      /// <summary>
      ///    Sends the plan data as BufferChunk to server.
      /// </summary>
      /// <param name="data">The data.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool SendData(BufferChunk data, EServerType server_type = EServerType.MCS)
      {
         return SendData((uint) data.Length, (byte[]) data, server_type);
      }

      /// <summary>
      ///    Sends the byte array of data to server.
      /// </summary>
      /// <param name="len">The length.</param>
      /// <param name="data">The data.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendDataException"></exception>
      public bool SendData(uint len, byte[] data, EServerType server_type = EServerType.MCS)
      {
         bool ret;

         if (Sender == null) {
            return false;
         }

         try {
#if LOCAL_DEBUG
            Console.WriteLine("Sending data to server: length = " + len);
#endif
            var packet = new TMPacket(server_type, EPacketType.Data, (byte) EDataCommand.SHOTSBLOCK, len, data);
            ret = SendPacket(packet);
         } catch (Exception ex) {
            var msg = "SendData : " + ex.Message;

            if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
               Console.WriteLine(msg);
            }

            throw new SendDataException(msg);
         }

#if LOCAL_DEBUG
         Console.WriteLine("Data sent to server");
#endif

         return ret;
      }

      /// <summary>
      ///    Sends the information to server.
      /// </summary>
      /// <param name="info">The information.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendInfoException"></exception>
      public bool SendInfo(string info, EServerType server_type = EServerType.MCS)
      {
         bool ret;

         if (Sender == null) {
            return false;
         }

         try {
            if (DebugPreference == 2) { // ActionPreference.Continue
               Console.WriteLine(Resources.Sending_info_to_server + ": " + info);
            }

            var packet = new TMPacket(server_type, EPacketType.Info, info);
            ret = SendPacket(packet);
         } catch (Exception ex) {
            var msg = "SendInfo : " + ex.Message;

            if (DebugPreference == 2) { // ActionPreference.Continue = Debugging is ON
               Console.WriteLine(msg);
            }

            throw new SendInfoException(msg);
         }

         return ret;
      }

      /// <summary>
      ///    Sends the loaded plan to server.
      /// </summary>
      /// <param name="plan">The plan.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool SendPlan(List<PlanSpot> plan = null)
      {
         if (plan == null) {
            plan = Plan;
         }

         return SendPlan(this, plan);
      }

      /// <summary>
      ///    Sends the plan as collection of spots to server.
      /// </summary>
      /// <param name="list">The list.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool SendPlan(ICollection<PlanSpot> list)
      {
         var plan = new Dictionary<int, PlanSpot>();

         foreach (var spot in list) {
            plan.Add(spot.id, spot);
         }

         return SendPlan(this, plan);
      }

      /// <summary>
      ///    Sends the plan as array of PSObjects to remote server
      /// </summary>
      /// <param name="arr">The array of PSObjects.</param>
      /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
      public bool SendPlan(object[] arr)
      {
         var plan = new Dictionary<int, PlanSpot>();

         foreach (var obj in arr) {
            var ps = (PSObject) obj;
            var spot = (PlanSpot) ps.BaseObject;
            plan.Add(spot.id, spot);
         }

         return SendPlan(this, plan);
      }

      /// <summary>
      ///    Starts the plan processing on remote server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool StartPlan()
      {
         return SendCommand(EPlanCommand.STARTPLAN);
      }

      /// <summary>
      ///    Stops the plan processing on remote server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool StopPlan()
      {
         return SendCommand(EPlanCommand.STOPPLAN);
      }

      #endregion

      #region Private methods

      /// <summary>
      ///    Connects to remote server via TcpClient
      /// </summary>
      /// <param name="ip">remote server IP</param>
      /// <param name="port">remote server port</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      private bool CreateSender(string ip, int port)
      {
         if (fConnectionTryCount < 0) {
            fConnectionTryCount = ConnectionTryCount;
            Sender = null;
            return false;
         }

         try {
            Sender = new TcpClient();

            Sender.SendTimeout = 1000;
            Sender.ReceiveTimeout = 1000;
            Sender.Connect(ip, port);

            if (Sender.Connected) {
               fConnectionTryCount = ConnectionTryCount;
               return true;
            }
         } catch (Exception ex) {
            if (DebugPreference == 2) { // ActionPreference.Continue
               Console.WriteLine(ex.Message);
            }

            fConnectionTryCount--;
            CreateSender(ip, port);
         }

         return (Sender != null) && Sender.Connected;
      }

      /// <summary>
      ///    Listens for data from server. The Main processing thread.
      /// </summary>
      private void ListenForData()
      {
         while (IsConnected) {
            var readingData = new byte[0x1000];
            ReadData = new BufferChunk(readingData);
            var numberOfBytesRead = 0;
            fNetworkStream = Sender.GetStream();

            try {
               while (fNetworkStream.DataAvailable) {
                  var len = fNetworkStream.Read(readingData, 0, readingData.Length);
                  numberOfBytesRead += len;
                  ReadData += readingData;
               }
            } catch {
               //ignore errors
            }

            if (numberOfBytesRead == 0) {
               Thread.Sleep(100);
               continue;
            }

            if (numberOfBytesRead >= TMPacketHeader.Length) {
               Header = ReadData.NextPacketHeader();
            }

            if (Header.type == (byte) EPacketType.Data) {
               var cmd = (EDataCommand) Header.value;

               if (cmd == EDataCommand.STATE) {
                  MCS_State_Server = ReadData.MCS_State();
                  var prevState = ServerState;

                  ServerState = (ECommandState) MCS_State_Server.state;

                  if (ServerState == ECommandState.FINISHED) {
                     if (PlanFinished != null) {
                        PlanFinished();
                     }
                  }

                  var spotsPassed = MCS_State_Server.spots_passed;
                  SpotsTotal = MCS_State_Server.spots_count;

                  var changed = (ServerState == ECommandState.INPROCESS) && (spotsPassed != SpotsPassed);

                  if (changed) {
                     SpotsPassed = spotsPassed;
                  }

                  if (ServerStateChanged != null) {
                     if (changed || (ServerState != ECommandState.INPROCESS)) {
                        ServerStateChanged(ServerState);
                     }
                  }

                  if (ServerState == ECommandState.NOTREADY) {
                     Thread.Sleep(100);
                     continue;
                  }

                  if (ServerState == ECommandState.INPROCESS) {
                     Header = ReadData.NextPacketHeader();
                     cmd = (EDataCommand) Header.value;
                     ReadData.Skip(1);
                     var len = numberOfBytesRead - 50;

                     if (DataBlockReceived != null) {
                        DataBlockReceived(ReadData, len);
                     }

                     ProcessPlanResults(ReadData, len);
                  }

                  continue;
               }

               if (cmd == EDataCommand.SHOTSRESULTS) {
                  if (DataBlockReceived != null) {
                     DataBlockReceived(ReadData, numberOfBytesRead);
                  }

                  continue;
               }
            }

            if ((InfoReceived != null) &&
                (Header.type == (byte) EPacketType.Info)) {
               //Send off the data for other classes to handle
               InfoReceived();
               continue;
            }

            if ((ErrorReceived != null) &&
                (Header.type == (byte) EPacketType.Error)) {
               //Send off the data for other classes to handle
               ErrorReceived();
               continue;
            }

            Thread.Sleep(200);
         }

         Disconnect();
      }

      /// <summary>
      ///    Event called when [server state changed].
      /// </summary>
      /// <param name="state">The server state.</param>
      private void OnServerStateChanged(ECommandState state)
      {
         if (state == ECommandState.INPROCESS) { // plan processing is ON
            if (DebugPreference == 2) { // ActionPreference.Continue == DEBUG is ON
               Console.WriteLine("Spot processed/total = " + SpotsPassed + "/" + SpotsTotal);
            }
         }

         if (state != ECommandState.FINISHED) {
            return;
         }

         if (PlanFinished != null) {
            PlanFinished();
         }
      }

      /// <summary>
      ///    Converts BufferChunk to PlanSpotResults
      /// </summary>
      /// <param name="data">The data.</param>
      /// <param name="bytesRead">The bytes read.</param>
      /// <returns>List&lt;PlanSpotResult&gt;.</returns>
      private void ProcessPlanResults(BufferChunk data, int bytesRead)
      {
         var len = bytesRead;
         var dt = (int) PlanSpotResult.Length;

         try {
            while (len >= 0) {
               var spot = (PlanSpotResult) data.NextPlanSpotResult();

               if (spot.done == 1) {
                  PlanResults.Add(spot);
               }

               len -= dt;
            }
         } catch {
            // ignored
         }

         if (PlanResultsProcessed != null) {
            PlanResultsProcessed(PlanResults);
         }
      }

      /// <summary>
      ///    Sends the TMPacket to server.
      /// </summary>
      /// <param name="p">The TMPacket.</param>
      /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
      private bool SendPacket(TMPacket p)
      {
         if (Sender == null) {
            return false;
         }

         try {
            fNetworkStream = Sender.GetStream();
            fNetworkStream.Write(p.Data.Buffer, 0, p.Data.Length);
            fNetworkStream.Flush();
         } catch (Exception ex) {
            return false;
         }

         return true;
      }

      #endregion

   }

   #region Exception classes

   /// <summary>
   ///    Exception during reading plan from a file
   ///    <br />Implements the <see cref="System.Exception" />
   /// </summary>
   /// <seealso cref="System.Exception" />
   public class ReadPlanException : Exception
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="ReadPlanException" /> class.
      /// </summary>
      /// <param name="file">The file.</param>
      public ReadPlanException(string file) : base(Resources.Failed_to_read + " " + Resources.plan_data + ": " + file)
      {
      }

      #endregion
   }

   /// <summary>
   ///    Exception during sending command to server
   ///    <br />Implements the <see cref="System.Exception" />
   /// </summary>
   /// <seealso cref="System.Exception" />
   public class SendCommandException : Exception
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="SendCommandException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      public SendCommandException(string msg) : base(msg)
      {
      }

      #endregion
   }

   /// <summary>
   ///    Exception during sending DATA to server
   ///    <br />Implements the <see cref="System.Exception" />
   /// </summary>
   /// <seealso cref="System.Exception" />
   public class SendDataException : Exception
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="SendDataException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      public SendDataException(string msg) : base(msg)
      {
      }

      #endregion
   }

   /// <summary>
   ///    Exception during sending info to server
   ///    <br />Implements the <see cref="System.Exception" />
   /// </summary>
   /// <seealso cref="System.Exception" />
   public class SendInfoException : Exception
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="SendInfoException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      public SendInfoException(string msg) : base(msg)
      {
      }

      #endregion
   }

   /// <summary>
   ///    Exception during sending plan to server
   ///    <br />Implements the <see cref="System.Exception" />
   /// </summary>
   /// <seealso cref="System.Exception" />
   public class SendPlanException : Exception
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="SendPlanException" /> class.
      /// </summary>
      public SendPlanException() : base(Resources.Failed_to_send + " " + Resources.plan_data)
      {
      }

      #endregion
   }

   #endregion
}
