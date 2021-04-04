
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TM
{
   /// <summary>
   ///    Struct PacketHeader
   /// </summary>
   [StructLayout(LayoutKind.Sequential, Pack = 1)]
   public struct PacketHeader
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(PacketHeader));

      /// <summary>
      ///    signature: "XRay" | "Ther" | .
      /// </summary>
      [Description("signature")]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public byte[] sign;

      /// <summary>
      ///    type: command, data, info
      /// </summary>
      [Description("type")]
      public byte type;

      /// <summary>
      ///    command number
      /// </summary>
      [Description("command number")]
      public byte value;

      /// <summary>
      ///    The reserved
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
      public byte[] reserved;

      /// <summary>
      ///    length of data after this header (bytes)
      /// </summary>
      [Description("lenght of data after this header")]
      public uint datalength;

      /// <summary>
      ///    The packet number
      /// </summary>
      [Description("optional,can be any")]
      public int packet_number;

      /// <summary>
      ///    Initializes a new instance of the <see cref="PacketHeader" /> struct.
      /// </summary>
      public PacketHeader(EServerType server_type = EServerType.MCS)
      {
         sign = server_type.Signature();
         value = 0;
         type = 0;
         reserved = new byte[2]; //{0, 0};
         datalength = 0;
         packet_number = 0;
      }

      /// <summary>
      ///    Returns a <see cref="System.String" /> that represents this instance.
      /// </summary>
      public override string ToString()
      {
         var sb = new StringBuilder();
         var t = (EPacketType) type;
         var v = (EProcessState) value;
         sb.AppendLine("sign = " + Encoding.ASCII.GetString(sign) + ", type = " + 
                       t.Description() + ", value = " + v + ", datalength = " + 
                       datalength + ", packet_number = " + packet_number);

         return sb.ToString();
      }
   }

   /// <summary>
   /// Implements the exchange packet<see cref="System.IDisposable" />
   /// </summary>
   /// <seealso cref="System.IDisposable" />
   public class Packet : IDisposable
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes static members of the <see cref="Packet" /> class.
      /// </summary>
      static Packet()
      {
         BufferChunk.SetNetworking();
      }

      /// <summary>
      ///    Initializes a new instance of the <see cref="Packet" /> class.
      /// </summary>
      public Packet(EServerType server_type, EPacketType type, byte cmd)
      {
         BufferChunk.SetNetworking();
         Header = new PacketHeader(server_type);

         Header.type = (byte) type;
         Header.value = cmd;
         Header.packet_number = PacketNumber++;
         Header.datalength = 1;
         var len = PacketHeader.Length;
         len += Header.datalength;
         Data = new BufferChunk((int) len);
         Data.Add(Header);

         AddChecksumm();
      }

      /// <summary>
      ///    Initializes a new instance of the <see cref="Packet" /> class.
      /// </summary>
      public Packet(EServerType server_type, EPacketType type, 
                      byte cmd, byte value = 0, byte[] data = null)
      {
         BufferChunk.SetNetworking();
         Header = new PacketHeader(server_type);

         Header.type = (byte) type;
         Header.value = value;
         Header.packet_number = PacketNumber++;
         var len = PacketHeader.Length + 1; // +1 checksum
         Header.datalength = 1;

         if ((data != null) &&
             (data.Length > 0)) {
            Header.datalength += (uint) data.Length;
            len += Header.datalength;
            Data = new BufferChunk((int) len);
            Data.Add(Header);
            Data += data;
         } else {
            Data = new BufferChunk((int) len);
            Data.Add(Header);
         }

         AddChecksumm();

#if LOCAL_DEBUG
         Console.WriteLine(Encoding.ASCII.GetString(Header.sign));

         if (data != null) {
            var str = Encoding.ASCII.GetString(data);
            Console.WriteLine(str);
         }
#endif
      }

      /// <summary>
      ///    Initializes a new instance of the <see cref="Packet" /> class.
      /// </summary>
      public Packet(EServerType server_type, EPacketType type, byte value, uint length, byte[] data)
      {
         BufferChunk.SetNetworking();
         Header = new PacketHeader(server_type);

         Header.type = (byte) type;
         Header.value = value;
         Header.packet_number = PacketNumber++;
         var len = PacketHeader.Length + 1; // +1 checksum
         Header.datalength = 1;

         if ((data != null) &&
             (length > 0)) {
            Header.datalength = length + 1;
            len += Header.datalength;
            Data = new BufferChunk((int) len);

            Data.Add(Header);
            Data += data;
            //var str = Encoding.ASCII.GetString(Data.Buffer);
         } else {
            Data = new BufferChunk((int) len);
            Data.Add(Header);
         }

         AddChecksumm();
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Packet"/> class.
      /// </summary>
      public Packet(EServerType server_type, EPacketType type, string data)
      {
         BufferChunk.SetNetworking();
         Header = new PacketHeader(server_type);

         Header.type = (byte) type;
         Header.value = 0;
         Header.packet_number = PacketNumber++;
         data += "\0"; // quick&dirty

         if (!string.IsNullOrEmpty(data)) {
            Header.datalength = (uint) data.Length + 1; // +1 checksum
            var len = PacketHeader.Length;
            len += Header.datalength;
            Data = new BufferChunk((int) len);
            Data.Add(Header);
            Data += data;
            var str = Encoding.ASCII.GetString(Data.Buffer);
         } else {
            Header.datalength = 1;
            Data = new BufferChunk((int) (PacketHeader.Length + 1));
            Data.Add(Header);
         }

         AddChecksumm();

#if LOCAL_DEBUG
         Console.WriteLine(Encoding.ASCII.GetString(Header.sign));

         if (!string.IsNullOrEmpty(data)) {
            var str = data;
            Console.WriteLine(str);
         }
#endif
      }

      #endregion

      #region  Fields

      /// <summary>
      ///    The packet header
      /// </summary>
      public PacketHeader Header;

      /// <summary>
      /// The memory stream
      /// </summary>
      private MemoryStream fMemoryStream;

      #endregion

      #region Public properties

      /// <summary>
      /// The absolute counter of packets
      /// </summary>
      public static int PacketNumber
      {
         get;
         private set;
      }

      /// <summary>
      /// The array byte of TotalSize
      /// </summary>
      public BufferChunk Data
      {
         get;
         private set;
      }

      /// <summary>
      /// Gets the type of the server.
      /// </summary>
      public EServerType ServerType
      {
         get
         {
            return Header.sign != null ? Header.sign.ServerType() : EServerType.UNKNOWN;
         }
      }

      /// <summary>
      /// The Total Size = sizeof(header) + sizeof(Data) + checksum
      /// </summary>
      public int TotalSize
      {
         get
         {
            return Data.Length;
         }
      }

      #endregion

      #region Interface methods

      /// <summary>
      /// Performs application-defined tasks associated with freeing,</br>
      /// releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose() 
      {
         if (fMemoryStream != null) {
            fMemoryStream.Close();
            fMemoryStream.Dispose();
         }

         fMemoryStream = null;
      }

      #endregion

      #region Public methods

      /// <summary>
      /// Checks the checksum.
      /// </summary>
      /// <param name="pdata">The pdata.</param>
      /// <returns><c>true</c> if OK, <c>false</c> otherwise.</returns>
      public bool IsChecksumOK(byte[] pdata)
      {
         byte cs = 0;
         var size = pdata.Length;

         if (size != TotalSize) {
            return false;
         }

         for (var i = 0; i < TotalSize; i++) {
            cs = (byte) (cs ^ Data[i]);
         }

         if (pdata[size - 1] != cs) {
            return false;
         }

         return true;
      }

      /// <summary>
      /// Returns a <see cref="System.String" /> that represents this instance.
      /// </summary>
      public override string ToString() 
      {
         var sb = new StringBuilder();
         sb.AppendLine("Header : " + Header);

         if (Header.datalength > 0) {
            sb.AppendLine(" Data : " + Encoding.ASCII.GetString(Data.Buffer));
         }

         return sb.ToString();
      }

      #endregion

      #region Private methods

      /// <summary>
      /// Adds the checksumm to the Data.
      /// </summary>
      private void AddChecksumm() 
      {
         byte cs = 0;

         for (var i = 0; i < TotalSize; i++) {
            cs = (byte) (cs ^ Data[i]);
         }

         Data = Data + cs;
      }

      #endregion
   }
}
