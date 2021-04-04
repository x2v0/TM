using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
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
///    Delegate ServerStateChangedHandler
/// </summary>
/// <param name="state">The state.</param>
public delegate void ServerStateChangedHandler(StateData state);

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
      /// Continue 	         2 	- Debug is ON
      /// SilentlyContinue 	0 	- Debug is OFF
      /// </code>
      /// </summary>
      /// <value>The debug preference.</value>
      public static int DebugPreference
      {
         get;
         set;
      }

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
   ///    Class TM.Client.
   ///    <br />Implements the <see cref="System.IDisposable" />
   /// </summary>
   /// <seealso cref="System.IDisposable" />
   public class Client : IDisposable
   {
      #region Static fields

      /// <summary>
      ///    The default value of connection try out count
      /// </summary>
      public static int ConnectionTryCount = 5;

      #endregion

      #region Constructors and destructors

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
      ///    Gets the packet header.
      /// </summary>
      /// <value>The header.</value>
      public PacketHeader Header
      {
         get;
         private set;
      }

      public string IP
      {
         get
         {
            return IpAddress;
         }

         set
         {
            IpAddress = value;
         }
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

      /// <summary>
      ///    Gets a value indicating whether this <see cref="TM.Client" /> is connected.
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
      ///    Gets the StateData structure.
      /// </summary>
      /// <value>The server.</value>
      public StateData StateData
      {
         get;
         protected set;
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
      ///    Connects the specified ip.
      /// </summary>
      /// <param name="ip">The ip.</param>
      /// <param name="port">The port.</param>
      /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
      public virtual bool Connect(string ip = null, int port = 0)
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
            if (Globals.Debug) { // ActionPreference.Continue = Debugging is ON
               Console.WriteLine(Resources.Failed_to_connect_to + " " + IpAddress + " , " + Resources.port_number + " = " + Port);
            }

            return false;
         }

         fListenThread = new Thread(ListenForData);
         fListenThread.Start();

         if (Globals.Debug) { // ActionPreference.Continue = Debugging is ON
            Console.WriteLine(Resources.Connected_to + " " + IpAddress + " , " + Resources.port_number + " = " + Port);
         }

         if (ServerConnected != null) {
            ServerConnected.Invoke();
         }

         return true;
      }

      /// <summary>
      ///    Disconnects 
      /// </summary>
      /// <returns><c>true</c> if disconnect is OK, <c>false</c> otherwise.</returns>
      public virtual bool Disconnect()
      {
         if (Globals.Debug) { // ActionPreference.Continue = Debugging is ON
            Console.WriteLine(Resources.Disconnected_from + " " + IpAddress + ":" + Port);
         }

         Reset();
         if (ServerDisconnected != null) {
            ServerDisconnected.Invoke();
         }

         return true;
      }

      /// <summary>
      ///    Process received data from server 
      /// </summary>
      public virtual void ProcessData(BufferChunk readData, int numberOfBytesRead)
      {
         if (DataBlockReceived != null) {
            DataBlockReceived.Invoke(ReadData, numberOfBytesRead);
         }
      }

      /// <summary>
      ///    Process received state data from server 
      /// </summary>
      public virtual void ProcessState(StateData stateData)
      {
         if (ServerStateChanged != null) {
            ServerStateChanged.Invoke(StateData);
         }
      }

      /// <summary>
      ///    Resets this instance.
      /// </summary>
      public virtual void Reset()
      {
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
      }

      /// <summary>
      ///    Sends the plan as byte array to server.
      /// </summary>
      /// <param name="data">The data.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendDataException"></exception>
      public virtual bool Send(byte[] data, EServerType server_type = EServerType.MCS)
      {
         bool ret;

         if (Sender == null) {
            return false;
         }

         try {
#if LOCAL_DEBUG
            Console.WriteLine("Sending data to server ... ");
#endif
            var packet = new Packet(server_type, EPacketType.Data, (byte) EDataCommand.SHOTSBLOCK, 0, data);
            ret = Send(packet);
         } catch (Exception ex) {
            var msg = "SendData : " + ex.Message;

            if (Globals.Debug) { // ActionPreference.Continue
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
      public virtual bool Send(BufferChunk data, EServerType server_type = EServerType.MCS)
      {
         return Send((uint) data.Length, (byte[]) data, server_type);
      }

      /// <summary>
      ///    Sends the byte array of data to server.
      /// </summary>
      /// <param name="len">The length.</param>
      /// <param name="data">The data.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendDataException"></exception>
      public virtual bool Send(uint len, byte[] data, EServerType server_type = EServerType.MCS)
      {
         bool ret;

         if (Sender == null) {
            return false;
         }

         try {
#if LOCAL_DEBUG
            Console.WriteLine("Sending data to server: length = " + len);
#endif
            var packet = new Packet(server_type, EPacketType.Data, (byte) EDataCommand.SHOTSBLOCK, len, data);
            ret = Send(packet);
         } catch (Exception ex) {
            var msg = "SendData : " + ex.Message;

            if (Globals.Debug) { // ActionPreference.Continue = Debugging is ON
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
      ///    Sends the Packet to server.
      /// </summary>
      /// <param name="p">The Packet.</param>
      /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
      public virtual bool Send(Packet p)
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

      /// <summary>
      ///    Sends the information to server.
      /// </summary>
      /// <param name="info">The information.</param>
      /// <param name="server_type">Type of the server.</param>
      /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
      /// <exception cref="TM.SendInfoException"></exception>
      public virtual bool SendInfo(string info, EServerType server_type = EServerType.MCS)
      {
         bool ret;

         if (Sender == null) {
            return false;
         }

         try {
            if (Globals.Debug) { // ActionPreference.Continue
               Console.WriteLine(Resources.Sending_info_to_server + ": " + info);
            }

            var packet = new Packet(server_type, EPacketType.Info, info);
            ret = Send(packet);
         } catch (Exception ex) {
            var msg = "SendInfo : " + ex.Message;

            if (Globals.Debug) { // ActionPreference.Continue = Debugging is ON
               Console.WriteLine(msg);
            }

            throw new SendInfoException(msg);
         }

         return ret;
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
            if (Globals.Debug) { // ActionPreference.Continue
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

            if (numberOfBytesRead < PacketHeader.Length) {
               Thread.Sleep(200);
               continue;
            }

            Header = ReadData.NextPacketHeader();

            switch ((EPacketType) Header.type) {
               case EPacketType.Info:
                  if (InfoReceived != null) {
                     InfoReceived.Invoke();
                  }

                  break;
               case EPacketType.Error:
                  if (ErrorReceived != null) {
                     ErrorReceived.Invoke();
                  }

                  break;
               case EPacketType.Data:
               {
                  var cmd = (EDataCommand) Header.value;

                  if (cmd == EDataCommand.STATE) {
                     StateData = ReadData.StateData();
                     ProcessState(StateData);
                  }

                  ProcessData(ReadData, numberOfBytesRead);
                  break;
               }
            }

            Thread.Sleep(200);
         }

         Disconnect();
      }

      #endregion
   }

   #region Exception classes

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

   #endregion
}
