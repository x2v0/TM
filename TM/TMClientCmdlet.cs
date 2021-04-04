using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using TM;
using TM.Properties;
using TMPlan;
using Timer = System.Timers.Timer;

namespace TMCmdLet
{
   /// <summary>
   ///    Base class of PlanCommands
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   /// </summary>
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   public class PlanCmdlet : PSCmdlet
   {
      #region Public properties

      /// <summary>
      ///    Gets or sets the destination hostname or IP address.
      /// </summary>
      /// <value>The ip address.</value>
      [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("ComputerName", "IP", "Host")]
      public string IpAddress
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets or sets whether to perform a TCP port
      /// </summary>
      /// <value>The port.</value>
      [Parameter(Position = 2, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("TcpPort")]
      public int Port
      {
         get;
         set;
      }

      #endregion

      #region  Other properties

      /// <summary>
      ///    OK == success
      /// </summary>
      /// <value><c>true</c> if ok; otherwise, <c>false</c>.</value>
      protected bool OK
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    ProcessRecord implementation
      /// </summary>
      protected override void ProcessRecord()
      {
         var client = PlanClient.This ?? new PlanClient();
         Globals.DebugPreference = (int) GetVariableValue("DebugPreference");

         if (string.IsNullOrEmpty(IpAddress)) {
            IpAddress = client.IpAddress;
         }

         if (Port <= 0) {
            Port = client.Port;
         }

         if (client.IsConnected) {
            return;
         }

         OK = client.Connect(IpAddress, Port);

         if (!OK) {
            WriteDebug(Resources.Server_is_not_connected);
         }
      }

      #endregion
   }

   /// <summary>
   ///    Read plan data from file specified by -Path
   ///    Returns plan data as list of <see cref="TMPlan.Spot" /> objects.
   ///    For example:
   ///    $plan = Get-Plan -Path test_plan.txt
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   /// </summary>
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommon.Get, "Plan")]
   [OutputType(typeof(Spot))]
   public class GetPlanCmdlet : PSCmdlet
   {
      #region Public properties

      /// <summary>
      ///    Gets or sets the plan file.
      /// </summary>
      /// <value>The path to plan file.</value>
      [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("file", "f")]
      [ValidateNotNullOrEmpty]
      public string Path
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Processes the record.
      /// </summary>
      protected override void ProcessRecord()
      {
         Globals.DebugPreference = (int) GetVariableValue("DebugPreference");

         var path = SessionState.Path.CurrentLocation.Path;
         path = System.IO.Path.Combine(path, Path);

         var spots = PlanClient.LoadPlan(path);

         foreach (var spot in spots) {
            WriteObject(spot);
         }
      }

      #endregion
   }

   /// <summary>
   ///    1. reads plan data from file
   ///    <para />
   ///    2. sends plan to server
   ///    <para />
   ///    3. sends command to start plan processing
   ///    <para />
   ///    4. waits for processing finished
   ///    <para />
   ///    Use Control-C - to stop plan processing
   ///    <para />
   ///    To resume processing - use Start-Plan -resume
   /// </summary>
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsLifecycle.Invoke, "Plan")]
   [OutputType(typeof(SpotFull))]
   public class InvokePlanCmdlet : PSCmdlet
   {
      #region Public properties

      /// <summary>
      ///    Gets or sets the destination hostname or IP address.
      /// </summary>
      /// <value>The IP address.</value>
      [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("ComputerName", "IP", "Host")]
      public string IpAddress
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets or sets the plan file.
      /// </summary>
      /// <value>The path to plan file.</value>
      [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("file", "f")]
      [ValidateNotNullOrEmpty]
      public string Path
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets or sets a TCP port
      /// </summary>
      /// <value>The port.</value>
      [Parameter(Position = 2, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("TcpPort")]
      public int Port
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Processes the record.
      /// </summary>
      protected override void ProcessRecord()
      {
         var client = PlanClient.This ?? new PlanClient();

         Globals.DebugPreference = (int) GetVariableValue("DebugPreference");
         var path = SessionState.Path.CurrentLocation.Path;

         Path = System.IO.Path.Combine(path, Path);

         Console.WriteLine(Resources.Press + " Ctrl-C " + Resources.to_interrupt);

         client.Reset();
         client.ProcessingIsOn = false;

         if (string.IsNullOrEmpty(IpAddress)) {
            IpAddress = client.IpAddress;
         }

         if (Port <= 0) {
            Port = client.Port;
         }

         var ok = client.Connect(IpAddress, Port);

         if (!ok) {
            return;
         }

         ok = client.Load(Path) != null;

         if (!ok) {
            return;
         }

         ok = client.Send();

         if (!ok) {
            WriteDebug(Resources.Failed_to_send_plan);
            return;
         }

         ok = client.Start();

         client.ServerStateChanged += OnStateChanged;
         client.ProcessingIsOn = true;

         while (client.ProcessingIsOn) {
            if (!ok || (client.PlanState == EPlanState.NOTREADY)) {
               WriteDebug(Resources.Server_not_ready);
            }

            if ((client.PlanState == EPlanState.FINISHED) && (client.PlanResults.Count > 1)) {
               client.ProcessingIsOn = false;
            }

            Thread.Sleep(1000);
         }

         client.ServerStateChanged -= OnStateChanged;

         foreach (var spot in client.PlanResults) {
            var plan = client.Plan[spot.id];
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
            WriteObject(full);
         }
      }

      /// <summary>
      ///    On Control-C, the plan execution will immediately cancel.
      ///    <para />
      ///    This allows a cancellation to occur during plan execution
      ///    <para />
      ///    without having to wait for the end of processing.
      ///    <para />
      /// </summary>
      protected override void StopProcessing()
      {
         var client = PlanClient.This ?? new PlanClient();
         var sav = Globals.DebugPreference;
         Globals.DebugPreference = (int) ActionPreference.Continue;

         if (client != null) {
            client.Stop();
            client.ProcessingIsOn = false;
         }

         Globals.DebugPreference = sav;
      }

      #endregion

      #region Private methods

      /// <summary>
      ///    Called when [server state changed].
      /// </summary>
      /// <param name="state">The server state.</param>
      private void OnStateChanged(int state)
      {
         var client = PlanClient.This ?? new PlanClient();

         if (((EPlanState) state == EPlanState.INPROCESS) && !Globals.Debug) { // plan processing is ON
            var passed = (int) ((client.SpotsPassed * 100.0) / client.SpotsTotal);
            Console.Write("\r" + Resources.Plan_processed + " = " + passed + "%  ");
         }
      }

      #endregion
   }

   /// <summary>
   ///    Sends plan data to remote server.
   ///    Returns <see cref="Client" /> object (the same object as in Connect-Server)
   ///    <example>
   ///       <code>
   ///  
   ///    $client = Get-Plan test_plan.txt | Send-Plan
   ///       or
   ///    $plan = Get-Plan test_plan.txt
   ///    $client = Send-Plan $plan
   ///  
   /// </code>
   ///    </example>
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   /// </summary>
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommunications.Send, "Plan")]
   [OutputType(typeof(Client))]
   public class SendPlanCmdlet : PSCmdlet
   {
      #region  Fields

      /// <summary>
      ///    The plan - list of <see cref="TMPlan.Spot" />s
      /// </summary>
      private readonly List<Spot> Plan = new List<Spot>();

      #endregion

      #region Public properties

      /// <summary>
      ///    Gets or sets the input object.
      /// </summary>
      /// <value>The input object.</value>
      [Parameter(Position = 0, ValueFromPipeline = true)]
      [Alias("Plan")]
      public object Input
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets or sets the destination hostname or IP address.
      /// </summary>
      /// <value>The IP address.</value>
      [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("ComputerName", "IP", "Host")]
      public string IpAddress
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets or sets the TCP port
      /// </summary>
      /// <value>The port.</value>
      [Parameter(Position = 2, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("TcpPort")]
      public int Port
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Ends the processing.
      /// </summary>
      protected override void EndProcessing()
      {
         var client = PlanClient.This ?? new PlanClient();
         if (string.IsNullOrEmpty(IpAddress)) {
            IpAddress = client.IpAddress;
         }

         if (Port <= 0) {
            Port = client.Port;
         }

         bool OK;

         if (!client.IsConnected) {
            OK = client.Connect(IpAddress, Port);

            if (!OK) {
               WriteDebug(Resources.Server_is_not_connected);
            }
         }

         OK = (Plan != null) && (Plan.Count > 1) ? client.Send(Plan) : client.Send();

         if (OK) {
            WriteObject(client);
         }
      }

      /// <summary>
      ///    ProcessRecord implementation
      /// </summary>
      protected override void ProcessRecord()
      {
         var client = PlanClient.This ?? new PlanClient();

         Globals.DebugPreference = (int) GetVariableValue("DebugPreference");

         if (Input == null) {
            return;
         }

         if (Input is PSObject) {
            var ps = (PSObject) Input;

            if (Input.ToString() == IpAddress) {
               IpAddress = string.Empty;
            }

            Plan.Add((Spot) ps.BaseObject);
            return;
         }

         if (string.IsNullOrEmpty(IpAddress)) {
            IpAddress = client.IpAddress;
         }

         if (Port <= 0) {
            Port = client.Port;
         }

         if (!client.IsConnected) {
            var OK = client.Connect(IpAddress, Port);

            if (!OK) {
               WriteDebug(Resources.Server_is_not_connected);
            }
         }

         var objects = Input as object[];

         if (objects != null) {
            var OK = client.Send(objects);

            if (OK) {
               WriteObject(client);
            }
         }
      }

      /// <summary>
      ///    On receiving the StopProcessing() request, the cmdlet will immediately cancel.
      ///    <para />
      ///    This allows a cancellation to occur during a connection request without having
      ///    <para />
      ///    to wait for the timeout.
      /// </summary>
      protected override void StopProcessing()
      {
      }

      #endregion
   }

   /// <summary>
   ///    Starts plan processing on remote server
   ///    <para />
   ///    Returns <see cref="Client" /> object
   ///    <example>
   ///       <code>
   ///  
   ///    # Load module
   ///    Import-Module ./Globals.Debug
   ///  
   ///    # Set default Hostname:Port
   ///    Set-DefaultServer localhost 9996
   ///  
   ///    # Read plan from file and send it to server
   ///    $client = Get-Plan test_plan.txt | Send-Plan
   ///  
   ///    # Subscribe to PlanFinished event to run script { Write-Host "Congratulations!" }
   ///    Register-ObjectEvent $client PlanFinished -Action { Write-Host "Congratulations!" }
   ///  
   ///    # Starts the plan processing on remote server
   ///    Start-Plan
   ///  
   /// </code>
   ///    </example>
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   [Cmdlet(VerbsLifecycle.Start, "Plan")]
   [OutputType(typeof(Client))]
   public class StartPlanCmdlet : PlanCmdlet
   {
      #region  Fields

      /// <summary>
      ///    The spots passed
      /// </summary>
      private uint SpotsPassed;

      /// <summary>
      ///    The progress
      /// </summary>
      private ProgressRecord theProgress;

      /// <summary>
      ///    The timer
      /// </summary>
      private Timer theTimer;

      #endregion

      #region Public properties

      /// <summary>
      ///    Resume plan processing <see cref="Client" />
      /// </summary>
      /// <value>The resume.</value>
      [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      public SwitchParameter Resume
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Start plan processing on remote server
      /// </summary>
      protected override void ProcessRecord()
      {
         var client = PlanClient.This ?? new PlanClient();
         base.ProcessRecord();

         OK = client.Start();

         if (Resume || !OK) {
            return;
         }

         theProgress = new ProgressRecord(1, Resources.Executing_plan, Resources.Progress_);

         theTimer = new Timer(500);
         theTimer.Elapsed += DoWork;

         theTimer.AutoReset = true;
         theTimer.Enabled = true;

         client.ProcessingIsOn = true;
         theTimer.Start();
         WriteObject(client);
      }

      /// <summary>
      ///    On Control-C, the plan execution will immediately cancel.
      ///    <para />
      ///    This allows a cancellation to occur during plan execution without having
      ///    <para />
      ///    to wait for the end of processing.
      /// </summary>
      protected override void StopProcessing()
      {
         var client = PlanClient.This ?? new PlanClient();
         var sav = Globals.DebugPreference;
         Globals.DebugPreference = (int) ActionPreference.Continue;

         if (client != null) {
            client.Stop();
         }

         theTimer.Stop();
         Globals.DebugPreference = sav;
      }

      #endregion

      #region Private methods

      /// <summary>
      ///    Do the work.
      /// </summary>
      /// <param name="myObject">My object - not used.</param>
      /// <param name="myEventArgs">The <see cref="EventArgs" /> - not used.</param>
      private void DoWork(object myObject, EventArgs myEventArgs)
      {
         var client = PlanClient.This ?? new PlanClient();
         if (!client.ProcessingIsOn) {
            theTimer.Stop();
            WriteDebug(Resources.Plan_processing_finished_);
            return;
         }

         if (SpotsPassed != client.SpotsPassed) {
            var passed = (int) ((client.SpotsPassed * 100.0) / client.SpotsTotal);
            SpotsPassed = client.SpotsPassed;
            theProgress.PercentComplete = passed;
            WriteProgress(theProgress);
         }

         if (!OK || (client.PlanState == EPlanState.NOTREADY)) {
            if (Globals.DebugPreference == (int) ActionPreference.Continue) {
               WriteDebug(Resources.Server_not_ready);
            }
         }

         if ((client.PlanState == EPlanState.FINISHED) && (client.PlanResults.Count > 1)) {
            client.ProcessingIsOn = false;
            theTimer.Stop();

            WriteDebug(Resources.Plan_processing_finished_);
         }
      }

      #endregion
   }

   /// <summary>
   ///    Stops plan processing on remote server
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsLifecycle.Stop, "Plan")]
   [OutputType(typeof(bool))]
   public class StopPlanCmdlet : PlanCmdlet
   {
      #region Protected methods

      /// <summary>
      ///    Process record implementation
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var client = PlanClient.This ?? new PlanClient();
         OK = client.Stop();
         WriteObject(OK);
      }

      #endregion
   }

   /// <summary>
   ///    Pauses plan processing on remote server.
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsLifecycle.Suspend, "Plan")]
   [OutputType(typeof(bool))]
   public class SuspendPlanCmdlet : PlanCmdlet
   {
      #region Protected methods

      /// <summary>
      ///    Process record implementation.
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var client = PlanClient.This ?? new PlanClient();
         OK = client.Pause();
         WriteObject(OK);
      }

      #endregion
   }

   /// <summary>
   ///    Returns result of plan processing as a list of <see cref="TMPlan.SpotFull" /> objects
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   /// </summary>
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   [Cmdlet(VerbsCommon.Get, "Results")]
   [OutputType(typeof(SpotFull))]
   public class GetPlanResultsCmdlet : PSCmdlet
   {
      #region Protected methods

      /// <summary>
      ///    ProcessRecord
      /// </summary>
      protected override void ProcessRecord()
      {
         var client = PlanClient.This ?? new PlanClient();

         Globals.DebugPreference = (int) GetVariableValue("DebugPreference");

         foreach (var spot in client.PlanResults) {
            var plan = client.Plan[spot.id];
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
            WriteObject(full);
         }
      }

      #endregion
   }

   /// <summary>
   ///    Clears plan on the server.
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   [Cmdlet(VerbsCommon.Clear, "Plan")]
   [OutputType(typeof(bool))]
   public class ClearPlanCmdlet : PlanCmdlet
   {
      #region Protected methods

      /// <summary>
      ///    ProcessRecord
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var client = PlanClient.This ?? new PlanClient();
         OK = client.Clear();
         WriteObject(OK);
      }

      #endregion
   }

   /// <summary>
   ///    Connects to remote server. Returns <see cref="Client" />  object.
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   /// </summary>
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommunications.Connect, "Server")]
   [OutputType(typeof(Client))]
   public class ConnectServerCmdlet : PSCmdlet
   {
      #region Public properties

      /// <summary>
      ///    Gets or sets the destination hostname or IP address.
      /// </summary>
      /// <value>The IP address.</value>
      [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [ValidateNotNullOrEmpty]
      [Alias("ComputerName", "IP", "Host")]
      public string IpAddress
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets or sets whether to perform a TCP port.
      /// </summary>
      /// <value>The port.</value>
      [ValidateRange(0, 65535)]
      [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("TcpPort")]
      public int Port
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Process record implementation.
      /// </summary>
      protected override void ProcessRecord()
      {
         var client = PlanClient.This ?? new PlanClient();

         Globals.DebugPreference = (int) GetVariableValue("DebugPreference");

         if (string.IsNullOrEmpty(IpAddress)) {
            IpAddress = client.IpAddress;
         }

         if (Port == 0) {
            Port = client.Port;
         }

         var ret = client.Connect(IpAddress, Port);

         if (ret) {
            WriteObject(client);
         }
      }

      /// <summary>
      ///    On Control-C = interrupt operation and disconnect
      /// </summary>
      protected override void StopProcessing()
      {
         var sav = Globals.DebugPreference;
         Globals.DebugPreference = (int) ActionPreference.Continue;

         var client = PlanClient.This ?? new PlanClient();
         if (client != null) {
            client.Disconnect();
         }

         Globals.DebugPreference = sav;
      }

      #endregion
   }

   /// <summary>
   ///    Sets default IP/Port of remote server.<br />
   ///    That allows to skip calling (-ip "hostname" -port XXX) in PlanCmdlets
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   /// </summary>
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommon.Set, "DefaultServer")]
   [OutputType(typeof(bool))]
   public class SetDefaultsCmdlet : PSCmdlet
   {
      #region Public properties

      /// <summary>
      ///    Gets or sets the destination hostname or IP address.
      /// </summary>
      /// <value>The ip address.</value>
      [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [ValidateNotNullOrEmpty]
      [Alias("ComputerName", "IP", "Host")]
      public string IpAddress
      {
         get;
         set;
      }

      /// <summary>
      ///    Gets or sets whether to perform a TCP port
      /// </summary>
      /// <value>The port.</value>
      [ValidateRange(0, 65535)]
      [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("TcpPort")]
      public int Port
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Process record implementation
      /// </summary>
      protected override void ProcessRecord()
      {
         var client = PlanClient.This ?? new PlanClient();

         Globals.DebugPreference = (int) GetVariableValue("DebugPreference");

         client.IpAddress = IpAddress;
         client.Port = Port;
      }

      #endregion
   }

   /// <summary>
   ///    Disconnects from remote server.
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   [Cmdlet(VerbsCommunications.Disconnect, "Server")]
   [OutputType(typeof(bool))]
   public class DisconnectServerCmdlet : PlanCmdlet
   {
      #region Protected methods

      /// <summary>
      ///    ProcessRecord
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var client = PlanClient.This ?? new PlanClient();
         OK = client.Disconnect();
         WriteObject(OK);
      }

      #endregion
   }

   /// <summary>
   ///    Send EPlanCommand to server.
   ///    <para />
   ///    <code>
   ///    public enum EPlanCommand
   ///    {
   ///       [Description("запрос на статус сервера")]
   ///       GETSTATE = 1,
   ///  
   ///       [Description("запрос на очистку плана ")]
   ///       CLEARPLAN = 2,
   ///  
   ///       [Description("запрос на старт плана ")]
   ///       STARTPLAN = 3,
   ///  
   ///       [Description("запрос на паузу")]
   ///       PAUSEPLAN = 4,
   ///  
   ///       [Description("запрос на останов")]
   ///       STOPPLAN = 5
   /// }
   /// </code>
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommunications.Send, "Command")]
   [OutputType(typeof(bool))]
   public class SendCommandCmdlet : PlanCmdlet
   {
      #region Public properties

      /// <summary>
      ///    Gets or sets the command send to server.
      ///    <para />
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
      /// <value>The command.</value>
      [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [ValidateRange(1, 5)]
      public EPlanCommand Command
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Process record implementation
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var client = PlanClient.This ?? new PlanClient();
         OK = client.SendCommand(Command);
         WriteObject(OK);
      }

      #endregion
   }

   /// <summary>
   ///    Send "info" message to server
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommunications.Send, "Info")]
   [OutputType(typeof(bool))]
   public class SendInfoCmdlet : PlanCmdlet
   {
      #region Public properties

      /// <summary>
      ///    Gets or sets the info message send to server.
      /// </summary>
      /// <value>The information.</value>
      [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [ValidateNotNullOrEmpty]
      public string Info
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Process Record
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var client = PlanClient.This ?? new PlanClient();
         OK = client.SendInfo(Info);
         WriteObject(OK);
      }

      #endregion
   }

   /// <summary>
   ///    Class used to send bulk of raw data (byte[]) to server
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="System.Management.Automation.PSCmdlet" />
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   /// <seealso cref="System.Management.Automation.PSCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommunications.Send, "Data")]
   [OutputType(typeof(bool))]
   public class SendDataCmdlet : PlanCmdlet
   {
      #region Public properties

      /// <summary>
      ///    RAW BULK of DATA sent to server.
      /// </summary>
      /// <value>The data.</value>
      [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [ValidateNotNullOrEmpty]
      public byte[] Data
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    Process record implementation
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var client = PlanClient.This ?? new PlanClient();
         OK = client.Send(Data);
         WriteObject(OK);
      }

      #endregion
   }

   /// <summary>
   ///    Returns Server State.
   ///    <br />Implements the <see cref="System.Management.Automation.Cmdlet" />
   ///    <br />Implements the <see cref="TMCmdLet.PlanCmdlet" />
   /// </summary>
   /// <seealso cref="TMCmdLet.PlanCmdlet" />
   /// <seealso cref="System.Management.Automation.Cmdlet" />
   [Cmdlet(VerbsCommon.Get, "ServerState")]
   [OutputType(typeof(EPlanState))]
   public class GetServerStateCmdlet : PlanCmdlet
   {
      #region Public properties

      /// <summary>
      ///    To get server state - request is sent
      ///    This is the time to wait for server response
      /// </summary>
      /// <value>wait time.</value>
      [ValidateRange(0, 65535)]
      [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
      [Alias("wait")]
      public int WaitTime
      {
         get;
         set;
      }

      #endregion

      #region Protected methods

      /// <summary>
      ///    ProcessRecord implementation for GetServerStateCommand.
      /// </summary>
      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         if (WaitTime < 100) {
            WaitTime = 100;
         }

         var client = PlanClient.This ?? new PlanClient();
         OK = client.AskServerState();
         Thread.Sleep(WaitTime);
         WriteObject(client.PlanState);
      }

      #endregion
   }
}
