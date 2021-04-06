using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TM;
using TM.Properties;
using TMSrv;

namespace TMPlan
{
   /// <summary>
   ///    Delegate PlanResultsHandler
   /// </summary>
   /// <param name="results">The results.</param>
   public delegate void PlanResultsHandler(List<SpotResult> results);

   /// <summary>
   /// 
   /// </summary>
   public class PlanClient : Client
   {
      #region Static fields

      /// <summary>
      /// 
      /// </summary>
      public static PlanClient This;

      #endregion

      #region Constructors and destructors

      /// <summary>
      /// 
      /// </summary>
      public PlanClient()
      {
         This = this;
         Plan = new List<Spot>();
         PlanResults = new List<SpotResult>();
      }

      #endregion

      #region Public events

      /// <summary>
      ///    Occurs when [plan cleared].
      /// </summary>
      public event ClientHandler PlanCleared;

      /// <summary>
      ///    Occurs when [plan processing is finished].
      /// </summary>
      public event ClientHandler PlanFinished;

      /// <summary>
      ///    Occurs when [plan loaded].
      /// </summary>
      public event ClientHandler PlanLoaded;

      /// <summary>
      ///    Occurs when [plan paused].
      /// </summary>
      public event ClientHandler PlanPaused;

      /// <summary>
      ///    Occurs when part of [plan results processed and received].
      /// </summary>
      public event PlanResultsHandler PlanResultsProcessed;

      /// <summary>
      ///    Occurs when [plan started].
      /// </summary>
      public event ClientHandler PlanStarted;

      /// <summary>
      ///    Occurs when [plan stopped].
      /// </summary>
      public event ClientHandler PlanStopped;

      #endregion

      #region Public properties

      /// <summary>
      ///    Gets the processing state of the server.
      /// </summary>
      /// <value>The state of processing on the server.</value>
      public EPlanState PlanState
      {
         get;
         protected set;
      }

      /// <summary>
      ///    Loaded plan data
      /// </summary>
      /// <value>The plan.</value>
      public List<Spot> Plan
      {
         get;
         set;
      }

      /// <summary>
      ///    Results of plan processing
      /// </summary>
      /// <value>The plan results.</value>
      public List<SpotResult> PlanResults
      {
         get;
         set;
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

      #region Public methods

      /// <summary>
      ///    Dumps the plan data.
      /// </summary>
      /// <param name="plan">The plan data.</param>
      public static void Dump(List<Spot> plan)
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
      public static void Dump(List<SpotResult> results)
      {
         try {
            foreach (var spot in results) {
               Console.WriteLine(spot);
            }
         } catch {
            // ignored
         }
      }

      public static List<Spot> LoadPlan(string file)
      {
         var client = This ?? new PlanClient();
         return client.Load(file);
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
      ///    Clears the plan data.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public virtual bool Clear()
      {
         var ret = SendCommand(EPlanCommand.CLEARPLAN);

         if (ret && (PlanCleared != null)) {
            PlanCleared.Invoke();
         }

         return ret;
      }

      /// <summary>
      /// Clear plan loaded locally
      /// </summary>
      public void ClearPlan() 
      {
         Plan.Clear();
         PlanResults.Clear();
      }

      /// <summary>
      ///    Dumps the plan results.
      /// </summary>
      public virtual void Dump()
      {
         Dump(PlanResults);
      }

      /// <summary>
      ///  True - server is ready
      /// </summary>
      public bool IsReady 
      {
         get
         {
            return IsConnected && PlanState == EPlanState.READY;
         }
      }

      /// <summary>
      ///  True - plan processing is finished
      /// </summary>
      public bool IsFinished 
      {
         get 
         {
            return PlanState == EPlanState.FINISHED;
         }
      }

      /// <summary>
      ///  True - plan processing is ON
      /// </summary>
      public bool IsProcessing 
      {
         get 
         {
            return PlanState == EPlanState.INPROCESS;
         }
      }

      /// <summary>
      ///    <code>
      /// LoadPlan(file) - loads the specified file with plan data.
      /// SendPlan()     - sends plan to the server specified by ip nad port
      /// StartPlan()    - starts plan processing on the server
      /// while (ProcessingIsOn) - waits for results of processing
      /// when results of plan processing received from server, fills PlanResults list
      /// if (ServerState == EPlanState.FINISHED) - execute PlanFinished() event
      /// </code>
      /// </summary>
      /// <param name="file">The file with plan data.</param>
      /// <param name="ip">The server IP.</param>
      /// <param name="port">The port.</param>
      /// <returns>Dictionary&lt;System.Int32, SpotFull&gt;.</returns>
      public virtual List<SpotFull> Execute(string file, string ip = null, int port = 0)
      {
         ClearPlan();

         var ok = Connect(ip, port);

         if (!ok) {
            return null;
         }

         ok = Load(file) != null;

         if (!ok) {
            return null;
         }

         ok = Send();

         if (!ok) {
            if (Globals.Debug) {
               Console.WriteLine(Resources.Failed_to_send + " " + Resources.plan);
            }

            return null;
         }

         ok = Start();

         while (ProcessingIsOn) {
            ok = AskServerState();

            if (!ok || (PlanState == EPlanState.NOTREADY)) {
               if (Globals.Debug) {
                  Console.WriteLine(Resources.Server_not_ready);
               }

               //return null;
            }

            if ((PlanState == EPlanState.FINISHED) &&
                (PlanResults.Count > 1)) {
               if (PlanFinished != null) {
                  PlanFinished.Invoke();
               }
            }

            Thread.Sleep(300);
         }

         var ret = new List<SpotFull>();

         foreach (var spot in PlanResults) {
            var plan = Plan[spot.id];
            var full = new SpotFull();
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
      ///    Reads the file and loads the plan data.
      /// </summary>
      /// <param name="file">The file with plan data.</param>
      /// <returns>BufferChunk. The raw array of bytes</returns>
      /// <exception cref="System.IO.FileNotFoundException"></exception>
      /// <exception cref="ReadPlanException">
      /// </exception>
      /// <exception cref="FileNotFoundException"></exception>
      public virtual List<Spot> Load(string file)
      {
         if (string.IsNullOrEmpty(file) ||
             !File.Exists(file)) {
            if (Globals.Debug) {
               Console.WriteLine(Resources.Loading_PlanData + " : " + Resources.file_not_found + " - " + file);
            }

            throw new FileNotFoundException();
         }

         var cnt = 0;
         var length = 0L;

         try {
            var r = new Regex(@"\s+");

            if (Globals.Debug) { 
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
                     var spot = new Spot();

                     try {
                        spot.id = cnt;
                        spot.xangle = float.Parse(parts[0], CultureInfo.InvariantCulture);
                        spot.zangle = float.Parse(parts[1], CultureInfo.InvariantCulture);
                        spot.energy = float.Parse(parts[2], CultureInfo.InvariantCulture);
                        spot.pcount = float.Parse(parts[3], CultureInfo.InvariantCulture);

                        cnt++;
                        length += Spot.Length;
                        Plan.Add(spot);
                     } catch (Exception ex) {
                        if (Globals.Debug) { // ActionPreference.Continue
                           Console.WriteLine(Resources.Failed_to_load + " PlanData (" +
                                             Resources.wrong_format_data + "), " + Resources.file + " - " + 
                                             file + "\nentries = " + cnt + " " +
                                             Resources.Error + ": " + ex.Message);
                        }

                        throw new ReadPlanException(file);
                     }
                  }
               }

               if (Globals.Debug) {
                  Console.WriteLine("PlanData" + " " + Resources.loaded + ": entries = " + 
                                    cnt + ", size = " + (length / 1000.0) + " Kb");
               }
            }

            if (PlanLoaded != null) {
               PlanLoaded.Invoke();
            }

            return Plan;
         } catch (Exception ex) {
            if (Globals.Debug) { 
               Console.WriteLine(Resources.Failed_to_load + " " + "PlanData" + ", " + 
                                 Resources.file + " - " + file + "\nentries = " + cnt + " " + 
                                 Resources.Error + ": " + ex.Message);
            }

            throw new ReadPlanException(file);
         }
      }

      public bool IsPlanLoaded
      {
         get
         {
            return Plan != null && Plan.Count > 0;
         }
      }

      /// <summary>
      ///    Pauses the plan processing on server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public virtual bool Pause()
      {
         var ret = SendCommand(EPlanCommand.PAUSEPLAN);

         if (ret && (PlanPaused != null)) {
            PlanPaused.Invoke();
         }

         return ret;
      }

      /// <summary>
      ///    Converts BufferChunk to SpotResults
      /// </summary>
      /// <param name="data">The data.</param>
      /// <param name="bytesRead">The bytes read.</param>
      public override void ProcessData(BufferChunk data, int bytesRead)
      {
         if ((data == null) ||
             (PlanState != EPlanState.INPROCESS)) {
            return;
         }

         var len = bytesRead;
         var dt = (int) SpotResult.Length;

         try {
            while (len >= 0) {
               var spot = (SpotResult) data.NextSpotResult();

               if (spot.done == 1) {
                  PlanResults.Add(spot);
               }

               len -= dt;
            }
         } catch {
            // ignored
         }

         if (PlanResultsProcessed != null) {
            PlanResultsProcessed.Invoke(PlanResults);
         }

         base.ProcessData(data, bytesRead);
      }

      public override void ProcessState(StateData data)
      {
         PlanState = (EPlanState) data.state;
         var changed = SpotsPassed != data.spots_passed;
         SpotsPassed = data.spots_passed;
         SpotsTotal = data.spots_count;

         switch (PlanState) {
            case EPlanState.INPROCESS:
               Header = ReadData.NextPacketHeader();
               var cmd = (EDataCommand) Header.value;
               ReadData.Skip(1);
               if (changed) {
                  //PlanInProcess.Invoke(SpotsPassed, SpotsTotal);
               }

               break;
            case EPlanState.FINISHED:
               if (PlanFinished != null) {
                  PlanFinished.Invoke();
               }

               break;
            case EPlanState.PAUSED:
               if (PlanPaused != null) {
                  PlanPaused.Invoke();
               }

               break;
            case EPlanState.NOTREADY:
               break;
            case EPlanState.READY:
               break;
            case EPlanState.UNKNOWN:
               break;
         }

         base.ProcessState(data);
      }

      public override void Reset()
      {
         ClearPlan();
         base.Reset();
      }

      /// <summary>
      ///    Sends the plan to server.
      /// </summary>
      /// <param name="spots">The spots.</param>
      /// <param name="nblocks">The nblocks.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      /// <exception cref="SendPlanException">
      /// </exception>
      public virtual bool Send(List<Spot> spots, uint nblocks = 10)
      {
         if (!IsConnected) {
            if (Globals.Debug) {
               Console.WriteLine(Resources.Server_is_not_connected);
            }

            return false;
         }

         BufferChunk.SetNetworking();
         var plan = new BufferChunk();

         foreach (var spot in spots) {
            plan.Add(spot);
         }

         var len = Spot.Length * nblocks;
         if (Globals.Debug) {
            Console.WriteLine(Resources.Sending_plan_to_server + ": length = " + 
                              (plan.Length / 1000.0) + " Kb");
         }

         try {
            while (plan.Length > len) {
               var bf = plan.NextBufferChunk((int) len);
               Send(bf);
            }

            try {
               if (plan.Length >= Spot.Length) {
                  var bf = plan.NextBufferChunk(plan.Length);
                  Send(bf);
               }
            } catch {
               throw new SendPlanException();
            }
         } catch {
            if (plan.Length >= Spot.Length) { // send the last portion of data
               try {
                  var bf = plan.NextBufferChunk(plan.Length);
                  Send(bf);
               } catch {
                  throw new SendPlanException();
               }
            }
         }

         if (Globals.Debug) {
            Console.WriteLine(Resources.Plan_sent_to_server);
         }

         SendCommand(EPlanCommand.GETSTATE);
         return true;
      }

      /// <summary>
      ///    Sends the loaded plan to server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public virtual bool Send()
      {
         return Send(Plan);
      }

      /// <summary>
      ///    Sends the plan as array of PSObjects to remote server
      /// </summary>
      /// <param name="arr">The array of PSObjects.</param>
      /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
      public virtual bool Send(object[] arr)
      {
         var plan = new List<Spot>();

         foreach (var obj in arr) {
            var ps = (PSObject) obj;
            var spot = (Spot) ps.BaseObject;
            plan.Add(spot);
         }

         return Send(plan);
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
      public virtual bool SendCommand(EPlanCommand cmd, EServerType server_type = EServerType.MCS)
      {
         if (!IsConnected) {
            if (Globals.Debug) {
               Console.WriteLine(Resources.Server_is_not_connected);
            }
            return false;
         }

         bool ret;

         if (cmd == EPlanCommand.CLEARPLAN) {
            ClearPlan();
         }

         try {
            var packet = new Packet(server_type, EPacketType.Command, (byte) cmd);
            if (Globals.Debug) { 
               Console.WriteLine(Resources.Sending_command_to_server + ": " + cmd.Description());
            }

            ret = Send(packet);
         } catch (Exception ex) {
            var msg = "SendCommand : " + cmd + " - " + ex.Message;

            if (Globals.Debug) {
               Console.WriteLine(msg);
            }

            throw new SendCommandException(msg);
         }

         if (Globals.Debug && !ret) {
            Console.WriteLine(Resources.Network_error);
         }
         return ret;
      }

      /// <summary>
      ///    Starts the plan processing on remote server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public virtual bool Start()
      {
         var ret = SendCommand(EPlanCommand.STARTPLAN);

         if (ret) {
            if (PlanStarted != null) {
               PlanStarted.Invoke();
            }
         }

         return ret;
      }

      /// <summary>
      ///    Stops the plan processing on remote server.
      /// </summary>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public virtual bool Stop()
      {
         var ret = SendCommand(EPlanCommand.STOPPLAN);

         if (ret && (PlanStopped != null)) {
            PlanStopped.Invoke();
         }

         return ret;
      }

      #endregion

      #region Private methods

    

      #endregion
   }

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

   public static class PlanExt
   {
      #region Public methods

      /// <summary>
      ///    Adds the specified plan.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <param name="plan">The plan.</param>
      /// <returns>BufferChunk.</returns>
      public static BufferChunk Add(this BufferChunk buf, Spot plan)
      {
         //buf.Reset
         buf += plan.id;
         buf += plan.xangle;
         buf += plan.zangle;
         buf += plan.energy;
         buf += plan.pcount;

         return buf;
      }

      /// <summary>
      ///    Nexts the result spot.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;SpotResult&gt;.</returns>
      public static SpotResult? NextResultSpot(this BufferChunk buf)
      {
         var plan = new SpotResult();

         try {
            plan.done = buf.NextInt32();
            plan.id = buf.NextInt32();
            plan.result_xangle = buf.NextFloat();
            plan.result_zangle = buf.NextFloat();
            plan.result_pcount = buf.NextFloat();
         } catch {
            //plan.id = -1; //
            return null;
         }

         return plan;
      }

      /// <summary>
      ///    Nexts the Spot.
      /// </summary>
      /// <param name="buf">The buffer chunk.</param>
      /// <returns>Spot.</returns>
      public static Spot? NextSpot(this BufferChunk buf)
      {
         var plan = new Spot();

         try {
            plan.id = buf.NextInt32();
            plan.xangle = buf.NextFloat();
            plan.zangle = buf.NextFloat();
            plan.energy = buf.NextFloat();
            plan.pcount = buf.NextFloat();
         } catch {
            //plan.id = -1; //
            return null;
         }

         return plan;
      }

      /// <summary>
      ///    Nexts the full spot.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;SpotFull&gt;.</returns>
      public static SpotFull? NextSpotFull(this BufferChunk buf)
      {
         var plan = new SpotFull();

         try {
            plan.changed = buf.NextInt32();
            plan.done = buf.NextInt32();
            plan.energy = buf.NextFloat();
            plan.id = buf.NextInt32();
            plan.need_to_sent = buf.NextInt32();
            plan.pcount = buf.NextFloat();
            plan.result_pcount = buf.NextFloat();
            plan.result_xangle = buf.NextFloat();
            plan.result_zangle = buf.NextFloat();
            plan.xangle = buf.NextFloat();
            plan.zangle = buf.NextFloat();
         } catch {
            //plan.id = -1; //
            return null;
         }

         return plan;
      }

      /// <summary>
      ///    Nexts the plan spot result.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;SpotResult&gt;.</returns>
      public static SpotResult? NextSpotResult(this BufferChunk buf)
      {
         var plan = new SpotResult();

         try {
            plan.id = buf.NextInt32();
            plan.result_xangle = buf.NextFloat();
            plan.result_zangle = buf.NextFloat();
            plan.result_pcount = buf.NextFloat();
            plan.done = buf.NextInt32();
         } catch {
            //plan.id = -1; //
            return null;
         }

         return plan;
      }

      #endregion
   }

   /// <summary>
   ///    Объединённая структура <see cref="Spot" /> + <see cref="SpotResult" />
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct SpotFull
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(SpotFull));

      #region  Properties

      /// <summary>
      ///    уникальный идентификатор записи (напр. счетчик)
      /// </summary>
      [Description("уникальный идентификатор записи (напр. счетчик)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по горизонтали
      /// </summary>
      [Description("угол по горизонтали")]
      public float xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по вертикали
      /// </summary>
      [Description("угол по вертикали")]
      public float zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    энергия, MeV
      /// </summary>
      [Description("энергия, MeV")]
      public float energy
      {
         get;
         set;
      }

      /// <summary>
      ///    количество протонов
      /// </summary>
      [Description("количество протонов")]
      public float pcount
      {
         get;
         set;
      }

      /// <summary>
      ///    выстрел сделан
      /// </summary>
      [Description("выстрел сделан")]
      public int done
      {
         get;
         set;
      }

      /// <summary>
      ///    результат угол по горизонтали
      /// </summary>
      [Description("результат угол по горизонтали")]
      public float result_xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    результат угол по вертикали
      /// </summary>
      [Description("результат угол по вертикали")]
      public float result_zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    результат количество протонов
      /// </summary>
      [Description("результат количество протонов")]
      public float result_pcount
      {
         get;
         set;
      }

      /// <summary>
      ///    состояние изменилось
      /// </summary>
      [Description("состояние изменилось")]
      public int changed
      {
         get;
         set;
      }

      /// <summary>
      ///    надо отослать изменения клиенту
      /// </summary>
      [Description("надо отослать изменения клиенту")]
      public int need_to_sent
      {
         get;
         set;
      }

      #endregion

      /// <summary>
      ///    Returns a <see cref="System.String" /> that represents this instance.
      /// </summary>
      public override string ToString()
      {
         var sb = new StringBuilder();
         sb.AppendLine("id = " + id + ", xangle = " + xangle + ", zangle = " +
                       zangle + ", energy = " + energy + ", pcount = " + pcount);
         sb.AppendLine(", done = " + done + ", result_xangle = " + result_xangle +
                       ", result_zangle = " + result_zangle + ", result_pcount = " + result_pcount);

         return sb.ToString();
      }
   } //44bytes

   /// <summary>
   ///    один "выстрел" для пересылки (направление+энергия+интенсивность)
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct SpotTopass
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(SpotTopass));

      /// <summary>
      ///    уникальный идентификатор записи (напр. счетчик)
      /// </summary>
      [Description("уникальный идентификатор записи (напр. счетчик)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по горизонтали
      /// </summary>
      [Description("угол по горизонтали")]
      public float xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по вертикали
      /// </summary>
      [Description("угол по вертикали")]
      public float zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    энергия, MeV
      /// </summary>
      [Description("энергия, MeV")]
      public float energy;

      /// <summary>
      ///    количество протонов
      /// </summary>
      [Description("количество протонов")]
      public float pcount
      {
         get;
         set;
      }

      /// <summary>
      ///    Returns a <see cref="System.String" /> that represents this instance.
      /// </summary>
      public override string ToString()
      {
         var sb = new StringBuilder();
         sb.AppendLine("id = " + id + ", xangle = " + xangle + ", zangle = " + 
                       zangle + ", energy = " + energy + ", pcount = " + pcount);

         return sb.ToString();
      }
   }

   /// <summary>
   ///    Структура с результатами выполнения плана облучения
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct SpotResult
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(SpotResult));

      #region  Properties

      /// <summary>
      ///    уникальный идентификатор записи (напр. счетчик)
      /// </summary>
      [Description("уникальный идентификатор записи (напр. счетчик)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по горизонтали
      /// </summary>
      [Description("угол по горизонтали")]
      public float result_xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по вертикали
      /// </summary>
      [Description("угол по вертикали")]
      public float result_zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    количество протонов
      /// </summary>
      [Description("количество протонов")]
      public float result_pcount
      {
         get;
         set;
      }

      /// <summary>
      ///    результат выполнения MCS_SHOT_RESULT_DONE
      /// </summary>
      [Description("результат выполнения MCS_SHOT_RESULT_DONE")]
      public int done
      {
         get;
         set;
      }

      #endregion

      /// <summary>
      ///    Returns a <see cref="System.String" /> that represents this instance.
      /// </summary>
      public override string ToString()
      {
         var sb = new StringBuilder();
         sb.AppendLine("id = " + id + ", result_xangle = " + result_xangle + 
                       ", result_zangle = " + result_zangle + ", result_pcount = " + 
                       result_pcount + ", done = " + done);

         return sb.ToString();
      }
   }

   /// <summary>
   ///    один "выстрел" для пересылки (направление+энергия+интенсивность)
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct Spot
   {
      /// <summary>
      ///    The length of structure
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(Spot));

      #region  Properties

      /// <summary>
      ///    уникальный идентификатор записи (напр. счетчик)
      /// </summary>
      [Description("уникальный идентификатор записи (напр. счетчик)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по горизонтали
      /// </summary>
      [Description("угол по горизонтали")]
      public float xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    угол по вертикали
      /// </summary>
      [Description("угол по вертикали")]
      public float zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    энергия, MeV
      /// </summary>
      [Description("энергия, MeV")]
      public float energy
      {
         get;
         set;
      }

      /// <summary>
      ///    количество протонов
      /// </summary>
      [Description("количество протонов")]
      public float pcount
      {
         get;
         set;
      }

      #endregion

      /// <summary>
      ///    Returns a <see cref="System.String" /> that represents this instance.
      /// </summary>
      public override string ToString()
      {
         var sb = new StringBuilder();
         sb.AppendLine("id = " + id + ", xangle = " + xangle + ", zangle = " + 
                       zangle + ", energy = " + energy + ", pcount = " + pcount);
         return sb.ToString();
      }
   }

   /// <summary>
   ///    команды на выполнение CMD в пакете пересылки клиент-&gt;сервер
   /// </summary>
   public enum EPlanCommand
   {
      /// <summary>
      ///    запрос на статус сервера
      /// </summary>
      [Description("запрос на статус сервера")]
      GETSTATE = 1,

      /// <summary>
      ///    запрос на очистку плана
      /// </summary>
      [Description("запрос на очистку плана")]
      CLEARPLAN = 2,

      /// <summary>
      ///    запрос на старт плана
      /// </summary>
      [Description("запрос на старт плана")]
      STARTPLAN = 3,

      /// <summary>
      ///    запрос на паузу
      /// </summary>
      [Description("запрос на паузу")]
      PAUSEPLAN = 4, //

      /// <summary>
      ///    запрос на останов
      /// </summary>
      [Description("запрос на останов")]
      STOPPLAN = 5
   }
}
