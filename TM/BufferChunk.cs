﻿// $Id: $


/*************************************************************************
 *                                                                       *
 * Copyright (C) 2021,   Valeriy Onuchin                                 *
 * All rights reserved.                                                  *
 *                                                                       *
 *************************************************************************/

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace TM
{
   /// <summary>
   ///    Overview:
   /// <code>
   ///   ----------------------------------------------------------------------
   ///   BufferChunk is a helper class created to make network
   ///   calls in DotNet easier by allowing byte[] to be passed
   ///   around along with an index (or offset) and length (or chunksize).
   ///   Members:
   ///   ----------------------------------------------------------------------
   ///   - index  - offset inside the buffer where valid data starts
   ///   - length - amount of valid data
   ///   - buffer - byte[] containing the data
   ///   Except for constructors (which set index and length member variables),
   ///   when index and length are passed as parameters, they are used as
   ///   offsets into the valid data, not offsets into the buffer.
   ///    Object State:
   ///   ----------------------------------------------------------------------
   ///   BufferChunk does not accept or return null or zero-length objects.
   ///   However, it is valid
   ///    for a BufferChunk to be in a state where it has no data to manipulate
   ///   i.e. length == 0 this.index + this.length cannot be &gt; buffer.Length
   ///   index + length cannot be &gt; this.length when manipulating inside the
   ///   valid data
   ///    index must be &gt;= 0
   ///    length must be &gt;= 0
   ///    Integral types:
   ///    ---------------------------------------------------------------------
   ///    BufferChunk allows the reading and writing of integral types
   ///    (Int16, Int32, Int64 and the unsigned counterparts) into the byte[].
   /// </code>
   ///    <br/>Implements the <see cref="System.IDisposable" />
   ///    <br/>Implements the <see cref="System.ICloneable" />
   /// </summary>
   /// <seealso cref="System.IDisposable" />
   /// <seealso cref="System.ICloneable" />
   /// <summary>
   /// Class BufferChunk.
   /// Implements the <see cref="System.IDisposable" />
   /// Implements the <see cref="System.ICloneable" />
   /// </summary>
   /// <seealso cref="System.IDisposable" />
   /// <seealso cref="System.ICloneable" />
   [ComVisible(false)]
   public class BufferChunk : IDisposable, ICloneable
   {
      #region Static fields

      /// <summary>
      ///    For doing conversions with strings
      /// </summary>
      /// <summary>
      /// The UTF8
      /// </summary>
      private static readonly UTF8Encoding utf8 = new UTF8Encoding();

      /// <summary>
      ///    The little endian
      /// </summary>
      /// <summary>
      /// The little endian
      /// </summary>
      private static bool littleEndian;

      #endregion

      #region Constructors and destructors

      /// <summary>
      ///    Initializes static members of the <see cref="BufferChunk" /> class.
      /// </summary>
      static BufferChunk()
      {
         littleEndian = BitConverter.IsLittleEndian;
      }

      /// <summary>
      ///    Constructor, create a new BufferChunk and allocate a new byte[] to hold the data.
      /// </summary>
      /// <param name="size">int size of the new byte[] to create, must be &gt;= 1</param>
      /// <example>
      ///    BufferChunk bufferChunk = new BufferChunk(2000);
      /// </example>
      public BufferChunk(int size = 1024)
      {
         ValidateNonNegative(size);
         ValidateNotZeroLength(size);

         buffer = new byte[size];
         length = 0;
      }

      /// <summary>
      ///    Constructor, create a BufferChunk using an existing byte[] without performing a memcopy
      /// </summary>
      /// <param name="buffer">
      ///    byte[] to be used as the data store for the BufferChunk,
      ///    cannot be null or zero length
      /// </param>
      /// <example>
      ///    byte[] buffer = new byte[2000];
      ///    BufferChunk bufferChunk = new BufferChunk(buffer);
      /// </example>
      public BufferChunk(byte[] buffer)
      {
         ValidateObject(buffer);
         ValidateNotZeroLength(buffer.Length);

         this.buffer = buffer;
         length = buffer.Length;
      }

      /// <summary>
      ///    Constructor, create a BufferChunk from its constituent parts
      /// </summary>
      /// <param name="buffer">byte[] to be used as the data store for the BufferChunk</param>
      /// <param name="index">offset at which the valid data starts</param>
      /// <param name="length">amount of 'valid data'</param>
      /// <example>
      ///    byte[] buffer = new byte[2000];
      ///    BufferChunk bufferChunk = new BufferChunk(buffer, 10, 200);
      /// </example>
      public BufferChunk(byte[] buffer, int index, int length)
      {
         ValidateObject(buffer);
         ValidateNotZeroLength(buffer.Length);
         ValidateNonNegative(index);
         ValidateNonNegative(length);
         ValidatePointerData(index, length, buffer.Length);

         this.buffer = buffer;
         this.index = index;
         this.length = length;
      }

      #endregion

      #region  Fields

      /// <summary>
      ///    Data storage
      /// </summary>
      private byte[] buffer;

      /// <summary>
      ///    Flag indicating whether the object is disposed or not
      /// </summary>
      private bool disposed;

      /// <summary>
      ///    Offset where the valid data starts
      /// </summary>
      private int index;

      /// <summary>
      ///    Length of the valid data
      /// </summary>
      private int length;

      #endregion

      #region Public properties

      /// <summary>
      ///    Buffer gives you direct access to the byte[] which is storing the raw data of the
      ///    BufferChunk.  Buffer is simply a byte[] that is passed ByRef so you have easy and
      ///    efficient access to the basic data.
      ///    Note: This property may be removed going forward
      /// </summary>
      /// <value>The buffer.</value>
      /// <example>
      ///    public int SendTo(BufferChunk bufferChunk, EndPoint endPoint)
      ///    {
      ///    return SendTo(bufferChunk.Buffer, bufferChunk.Index, bufferChunk.Length,
      ///    SocketFlags.None, endPoint);
      ///    }
      /// </example>
      public byte[] Buffer
      {
         get
         {
            return buffer;
         }
      }

      /// <summary>
      ///    Index points to the start of the valid data area
      ///    Note: This property may be removed going forward
      /// </summary>
      /// <value>The index.</value>
      /// <example>
      ///    public int SendTo(BufferChunk bufferChunk, EndPoint endPoint)
      ///    {
      ///    return SendTo(bufferChunk.Buffer, bufferChunk.Index, bufferChunk.Length,
      ///    SocketFlags.None, endPoint);
      ///    }
      /// </example>
      public int Index
      {
         get
         {
            return index;
         }
      }

      /// <summary>
      ///    Length is amount of valid data in the buffer
      ///    Length should not be directly manipulated to select smaller sections of the BufferChunk
      ///    because this would abandon valid data.  Instead, you should use the method
      ///    <see cref="BufferChunk.Peek" />  to create a shallow copy new BufferChunk pointing to
      ///    just the section you want.
      ///    Note: This property may be removed going forward
      /// </summary>
      /// <value>The length.</value>
      /// <example>
      ///    public int SendTo(BufferChunk bufferChunk, EndPoint endPoint)
      ///    {
      ///    return SendTo(bufferChunk.Buffer, bufferChunk.Index, bufferChunk.Length,
      ///    SocketFlags.None, endPoint);
      ///    }
      /// </example>
      public int Length
      {
         get
         {
            return length;
         }

         set
         {
            ValidateNonNegative(value);
            ValidatePointerData(index, value, buffer.Length);

            length = value;
         }
      }

      #endregion

      #region  Other properties

      /// <summary>
      ///    Gets the available buffer.
      ///    How much space the buffer has left (for operator+)
      /// </summary>
      /// <value>The available buffer.</value>
      private int AvailableBuffer
      {
         get
         {
            return buffer.Length - index - length;
         }
      }

      #endregion

      #region Public indexers

      /// <summary>
      ///    Indexer used to allow us to treat a BufferChunk like a byte[].  Useful when making in place modifications or reads
      ///    from a BufferChunk.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.Byte.</returns>
      public byte this[int index]
      {
         get
         {
            ValidateNonNegative(index);
            ValidateNotZeroLength(length);
            ValidateSufficientData(1, length - index);

            return buffer[this.index + index];
         }

         set
         {
            ValidateNonNegative(index);

            if (1 > (length - index)) {
               ExpandBuffer();
            }

            ValidateSufficientSpace(1, length - index);

            buffer[this.index + index] = value;
         }
      }

      #endregion

      #region Interface methods

      /// <summary>
      ///    Creates a shallow copy (new Index and Length, duplicate reference to the same Buffer) of a BufferChunk.
      /// </summary>
      /// <returns>BufferChunk instance with ref Buffer, ByVal Index, and ByVal Length</returns>
      public object Clone()
      {
         return new BufferChunk(buffer, index, length);
      }

      /// <summary>
      ///    Disposes the internal state of the object
      /// </summary>
      public void Dispose()
      {
         if (!disposed) {
            buffer = null;
            disposed = true;
         }
      }

      #endregion

      #region Public methods

      /// <summary>
      ///    Compares the specified obj1.
      /// </summary>
      /// <param name="obj1">The obj1.</param>
      /// <param name="obj2">The obj2.</param>
      /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
      public static bool Compare(byte[] obj1, byte[] obj2)
      {
         var ret = false;

         if ((obj1 == null) ||
             (obj2 == null)) {
            if (obj1 == obj2) {
               ret = true;
            }
         } else if (obj1.Length == obj2.Length) {
            var i = 0;

            for (; i < obj1.Length; i++) {
               if (obj1[i] != obj2[i]) {
                  break;
               }
            }

            if (i == obj1.Length) {
               ret = true;
            }
         }

         return ret;
      }

      /// <summary>
      ///    Copies the specified source.
      /// </summary>
      /// <param name="source">The source.</param>
      /// <returns>System.Byte[][].</returns>
      public static byte[] Copy(byte[] source)
      {
         byte[] ret = null;

         if (source != null) {
            ret = new byte[source.Length];
            Array.Copy(source, 0, ret, 0, source.Length);
         }

         return ret;
      }

      /// <summary>
      ///    Override + and += operator to allow appending of buffers,
      ///    provided there is room in the left-most BufferChunk
      /// </summary>
      /// <param name="destination">BufferChunk destination that will be appended to</param>
      /// <param name="source">BufferChunk source</param>
      /// <returns>Reference to BufferChunk destination</returns>
      public static BufferChunk operator +(BufferChunk destination, BufferChunk source)
      {
         ValidateObject(source);
         ValidateNotZeroLength(source.length);

         if (source.length > destination.AvailableBuffer) {
            destination.ExpandBuffer(source.length); //!!!
         }

         ValidateSufficientSpace(source.length, destination.AvailableBuffer);

         Array.Copy(source.buffer, source.index, destination.buffer, destination.index + destination.length, source.length);
         destination.length += source.length;

         return destination;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="source">The source.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, byte[] source)
      {
         /*
         ValidateObject(source);
         ValidateNotZeroLength(source.Length);
         var str = Encoding.ASCII.GetString(source);
         if (source.Length > destination.AvailableBuffer) {
            destination.ExpandBuffer(source.Length); //!!!
         }

         ValidateSufficientSpace(source.Length, destination.AvailableBuffer);

         Array.Copy(source, 0, destination.buffer, 
                    destination.index + destination.length, source.Length);
         destination.length += source.Length;

         for (var i = 0; i < source.Length; i++) {
            destination += source[i];
         }
*/
         ValidateObject(source);
         ValidateNotZeroLength(source.Length);

         if (source.Length > destination.AvailableBuffer) {
            destination.ExpandBuffer(source.Length); //!!!
         }

         Array.Copy(source, 0, destination.buffer, destination.index + destination.length, source.Length);
         destination.length += source.Length;

         return destination;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="b">The b.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, byte b)
      {
         if (1 > destination.AvailableBuffer) {
            destination.ExpandBuffer();
         }

         ValidateSufficientSpace(1, destination.AvailableBuffer);

         destination[destination.length++] = b;

         return destination;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, short data)
      {
         if (2 > destination.AvailableBuffer) {
            destination.ExpandBuffer();
         }

         ValidateSufficientSpace(2, destination.AvailableBuffer);

         // Advance the length 2, and set the data
         destination.length += 2;
         destination._SetInt16(destination.length - 2, data);

         return destination;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, float data)
      {
         var bytes = GetBytes(data);
         return destination + bytes;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, double data)
      {
         var bytes = GetBytes(data);
         return destination + bytes;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, int data)
      {
         if (4 > destination.AvailableBuffer) {
            destination.ExpandBuffer();
         }

         ValidateSufficientSpace(4, destination.AvailableBuffer);

         // Advance the length 4, and set the data
         destination.length += 4;
         destination._SetInt32(destination.length - 4, data);

         return destination;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, long data)
      {
         return destination += (ulong)data;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, ushort data)
      {
         return destination + (short)data;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, uint data)
      {
         return destination + (int)data;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="data">The data.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, ulong data)
      {
         if (8 > destination.AvailableBuffer) {
            destination.ExpandBuffer(); //!!
         }

         ValidateSufficientSpace(8, destination.AvailableBuffer);

         // Advance the length 8, and set the data
         destination.length += 8;

         destination._SetUInt64(destination.length - 8, data);

         return destination;
      }

      /// <summary>
      ///    <br/>Implements the +.
      /// </summary>
      /// <param name="destination">The destination.</param>
      /// <param name="s">The s.</param>
      /// <returns>The result of the operator.</returns>
      public static BufferChunk operator +(BufferChunk destination, string s)
      {
         ValidateObject(s);
         ValidateNotZeroLength(s.Length);

         byte[] bytes;

         lock (utf8) {
            bytes = utf8.GetBytes(s);
         }

         ValidateSufficientSpace(bytes.Length, destination.AvailableBuffer);

         return destination += (BufferChunk)bytes;
      }

      /// <summary>
      ///    Explicitly cast the valid data into a new byte[]. This function creates a copy of the
      ///    BufferChunk data and omits the bytes before the Index and after the Length from the
      ///    byte[] copy.  This is a simple way to interoperate BufferChunks with functions that
      ///    only know how to deal with byte[].
      /// </summary>
      /// <param name="source">BufferChunk</param>
      /// <returns>byte[] containing the valid data from the BufferChunk</returns>
      /// <example>
      ///    using P;
      ///    using System.Net.Sockets;
      ///    Socket socket = new Socket(...);  // This standard socket only knows byte[]
      ///    BufferChunk bufferChunk = new bufferChunk(500); // Create a new BufferChunk containing a 500 byte buffer
      ///    socket.Send((byte[])bufferChunk, SocketFlags.None);    //Note the explicit cast from BufferChunk to byte[]
      /// </example>
      public static explicit operator byte[](BufferChunk source)
      {
         ValidateObject(source);
         ValidateNotZeroLength(source.length);

         var returnBuffer = new byte[source.length];
         Array.Copy(source.buffer, source.index, returnBuffer, 0, source.length);
         return returnBuffer;
      }

      /// <summary>
      ///    Explicitly cast the valid data to a string.  Helpful for applications that want to send
      ///    strings or XML over the network without worrying about the String to UTF8 logic.
      /// </summary>
      /// <param name="source">BufferChunk containing the data</param>
      /// <returns>string form of data</returns>
      /// <example>
      ///    BufferChunk bc = new BufferChunk(new byte[] {74, 97, 115, 111, 110});
      ///    if((string)bc == "Jason")...
      /// </example>
      public static explicit operator string(BufferChunk source)
      {
         ValidateObject(source);
         ValidateNotZeroLength(source.length);

         lock (utf8) {
            return utf8.GetString((byte[])source);
         }
      }

      /// <summary>
      ///    Explicitly cast a string to a BufferChunk.  Helpful for applications that want to send strings or XML over the
      ///    network without worrying about the String to UTF8 logic.
      /// </summary>
      /// <param name="source">The source.</param>
      /// <returns>The result of the conversion.</returns>
      public static explicit operator BufferChunk(string source)
      {
         ValidateObject(source);
         ValidateNotZeroLength(source.Length);

         lock (utf8) {
            return new BufferChunk(utf8.GetBytes(source));
         }
      }

      /// <summary>
      ///    Explicitly cast a byte[] into a BufferChunk.  Useful when you want to start acting upon a byte[] in an incremental
      ///    fashion by taking advantage of The functionality a BufferChunk provides over a byte[].  For instance, this is useful
      ///    for aking a large (say 500k) dataset and dividing it up into smaller (say 1.5k) chunks.
      ///    This is functionally equivalent to <c>new BufferChunk(buffer)</c>
      /// </summary>
      /// <param name="buffer">byte[] buffer containing valid data</param>
      /// <returns>BufferChunk</returns>
      public static explicit operator BufferChunk(byte[] buffer)
      {
         // Let the constructor do the checking for us
         return new BufferChunk(buffer);
      }

      /// <summary>
      ///    Sets the networking.
      /// </summary>
      /// <param name="v">if set to <c>true</c> [v].</param>
      public static void SetNetworking(bool v = true)
      {
         littleEndian = !v && BitConverter.IsLittleEndian; // we use network
      }

      /// <summary>
      ///    Used to zero out the data area of the BufferChunk.
      /// </summary>
      public void Clear()
      {
         for (var i = index; i < (index + length); i++) {
            buffer[i] = 0;
         }
      }

      /// <summary>
      ///    Copies from.
      /// </summary>
      /// <param name="src">The SRC.</param>
      /// <param name="length">The length.</param>
      public void CopyFrom(IntPtr src, int length)
      {
         ValidateIntPtr(src);
         ValidateNonNegative(length);
         ValidateNotZeroLength(length);
         ValidateSufficientSpace(length, AvailableBuffer);

         Marshal.Copy(src, buffer, index + this.length, length);
         this.length += length;
      }

      /// <summary>
      ///    Copy the valid data section of 'this' to the destination BufferChunk
      ///    overwriting dest's previous contents
      ///    This method does not allow dest's valid data section to grow or shrink
      ///    (i.e. treat valid data as a fixed buffer)
      /// </summary>
      /// <param name="destination">BufferChunk</param>
      /// <param name="index">offset in the destination BufferChunk's valid data</param>
      public void CopyTo(BufferChunk destination, int index)
      {
         ValidateObject(destination);
         ValidateNonNegative(index);
         ValidateNotZeroLength(length);

         if (length > (destination.length - index)) {
            ExpandBuffer(destination.length - index);
         }

         ValidateSufficientSpace(length, destination.length - index);

         Array.Copy(buffer, this.index, destination.buffer, destination.index + index, length);
      }

      /// <summary>
      ///    Copies to.
      /// </summary>
      /// <param name="dest">The dest.</param>
      /// <param name="length">The length.</param>
      public void CopyTo(IntPtr dest, int length)
      {
         ValidateIntPtr(dest);
         ValidateNonNegative(length);
         ValidateNotZeroLength(length);

         if (length > this.length) {
            ExpandBuffer(length);
         }

         ValidateSufficientData(length, this.length);

         Marshal.Copy(buffer, index, dest, length);

         this.length -= length;
         index += length;
      }

      /// <summary>
      ///    Retrieves 1 byte from inside the BufferChunk
      ///    This method is included for consistency.  It simply forwards to the indexer.
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.Byte.</returns>
      public byte GetByte(int index)
      {
         // Let the indexer do the checking
         return this[index];
      }

      /// <summary>
      ///    Retrieves 8 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.Double.</returns>
      public double GetDouble(int index)
      {
         ValidateNonNegative(index);
         ValidateNotZeroLength(length - index);
         ValidateSufficientData(8, length - index);

         return _GetDouble(index);
      }

      /// <summary>
      ///    Retrieves 4 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.Single.</returns>
      public float GetFloat(int index)
      {
         ValidateNonNegative(index);
         ValidateNotZeroLength(length - index);
         ValidateSufficientData(4, length - index);

         return _GetFloat(index);
      }

      /// <summary>
      ///    Retrieves 2 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.Int16.</returns>
      public short GetInt16(int index)
      {
         ValidateNonNegative(index);
         ValidateNotZeroLength(length - index);
         ValidateSufficientData(2, length - index);

         return _GetInt16(index);
      }

      /// <summary>
      ///    Retrieves 4 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.Int32.</returns>
      public int GetInt32(int index)
      {
         ValidateNonNegative(index);
         ValidateNotZeroLength(length - index);
         ValidateSufficientData(4, length - index);

         return _GetInt32(index);
      }

      /// <summary>
      ///    Retrieves 8 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.Int64.</returns>
      public long GetInt64(int index)
      {
         return (long)GetUInt64(index);
      }

      /// <summary>
      ///    Gets the padded U int16.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.UInt16.</returns>
      public ushort GetPaddedUInt16(int index)
      {
         ValidateNonNegative(index);
         ValidateSufficientData(1, length - index); // We need at least 1 byte

         ushort ret = 0;

         var dataSize = 2;
         if ((index + dataSize) <= length) // All the data requested
         {
            ret = (ushort)_GetInt16(index);
         } else // The rest of the data in the buffer
         {
            var offset = 0;
            while ((index + offset) < length) {
               int shift;

               if (littleEndian) {
                  shift = dataSize - offset - 1;
               } else {
                  shift = offset;
               }

               ret += (ushort)(buffer[this.index + index + offset] << (shift * 8));
               offset++;
            }
         }

         return ret;
      }

      /// <summary>
      ///    Gets the padded UInt32.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.UInt32.</returns>
      public uint GetPaddedUInt32(int index)
      {
         ValidateNonNegative(index);
         ValidateSufficientData(1, length - index);

         uint ret = 0;

         var dataSize = 4;
         if ((index + dataSize) < length) // All the data requested
         {
            ret = (uint)_GetInt32(index);
         } else // The rest of the data in the buffer
         {
            var offset = 0;
            while ((index + offset) < length) {
               int shift;

               if (littleEndian) {
                  shift = dataSize - offset - 1;
               } else {
                  shift = offset;
               }

               ret += (uint)buffer[this.index + index + offset] << (shift * 8);
               offset++;
            }
         }

         return ret;
      }

      /// <summary>
      ///    Gets the padded UInt64.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.UInt64.</returns>
      public ulong GetPaddedUInt64(int index)
      {
         var dataSize = 8;
         var availableData = length - index;

         ValidateNonNegative(index);
         ValidateSufficientData(1, availableData);

         ulong ret = 0;

         if (dataSize < availableData) // All the data requested
         {
            ret = _GetUInt64(index);
         } else // The rest of the data in the buffer
         {
            var indexOffset = this.index + index;

            if (littleEndian) {
               for (int offset = 0, shift = dataSize - 1; offset < availableData; offset++, shift--) {
                  ret += (ulong)buffer[indexOffset + offset] << (shift * 8);
               }
            } else {
               for (int offset = 0, shift = 0; offset < availableData; offset++, shift++) {
                  ret += (ulong)buffer[indexOffset + offset] << (shift * 8);
               }
            }
         }

         return ret;
      }

      /// <summary>
      ///    Retrieves 2 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.UInt16.</returns>
      public ushort GetUInt16(int index)
      {
         return (ushort)GetInt16(index);
      }

      /// <summary>
      ///    Retrieves 4 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.UInt32.</returns>
      public uint GetUInt32(int index)
      {
         return (uint)GetInt32(index);
      }

      /// <summary>
      ///    Retrieves 8 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <returns>System.UInt64.</returns>
      public ulong GetUInt64(int index)
      {
         ValidateNonNegative(index);
         ValidateNotZeroLength(length - index);
         ValidateSufficientData(8, length - index);

         return _GetUInt64(index);
      }

      /// <summary>
      ///    Retrieves length bytes from inside the BufferChunk and converts from UTF8 string
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="length">The length.</param>
      /// <returns>System.String.</returns>
      public string GetUTF8String(int index, int length)
      {
         ValidateNonNegative(index);
         ValidateNonNegative(length);
         ValidateNotZeroLength(length);
         ValidateNotZeroLength(this.length - index);
         ValidateSufficientData(length, this.length - index);

         lock (utf8) {
            return utf8.GetString(buffer, this.index + index, length);
         }
      }

      /// <summary>
      ///    Returns a BufferChunk consisting of the next 'length' bytes of the BufferChunk instance.
      ///    Automatically increments Index and decrements Length.
      ///    This function is useful for iterative functions that parse through a large BufferChunk returning smaller
      ///    BufferChunks
      /// </summary>
      /// <param name="length">int</param>
      /// <returns>BufferChunk</returns>
      /// <example>
      ///    ...
      ///    frameBuffer = new BufferChunk(500000);
      ///    ...
      ///    int packetsInFrame = (ushort)((frameBuffer.Length + RtpHeaderExtensionSize) / (MaximumPacketPayload));
      ///    if (((frameBuffer.Length + RtpHeaderExtensionSize) % (MaximumPacketPayload)) &gt; 0)
      ///    packetsInFrame++;
      ///    for (int i = 0; i &lt; packetsInFrame; i++)
      ///    {
      ///    int sizeToCopy = (frameBuffer.Length &lt; MaximumPacketPayload) ? frameBuffer.Length : MaximumPacketPayload;
      ///    socket.Send((byte[])frameBuffer.NextBufferChunk(sizeToCopy));
      ///    }
      /// </example>
      public BufferChunk NextBufferChunk(int length)
      {
         // Peek will validate for us
         var retBC = Peek(0, length);

         this.length -= length;
         index += length;

         return retBC;
      }

      /// <summary>
      ///    Returns the requested amount of data, or whatever remains if length &gt; this.length
      /// </summary>
      /// <param name="length">The length.</param>
      /// <returns>BufferChunk.</returns>
      /// <summary>
      /// Nexts the buffer chunk maximum.
      public BufferChunk NextBufferChunkMax(int length)
      {
         if (length > Length) {
            length = Length;
         }

         return NextBufferChunk(length);
      }

      /// <summary>
      ///    Nexts the byte.
      /// </summary>
      /// <returns>System.Byte.</returns>
      public byte NextByte()
      {
         // Let GetByte do the checking
         var ret = GetByte(0);

         length--;
         index++;

         return ret;
      }

      /// <summary>
      ///    Nexts the Double.
      /// </summary>
      /// <returns>System.Double.</returns>
      public double NextDouble()
      {
         // Let GetInt do the checking
         var ret = GetDouble(0);

         length -= 8;
         index += 8;

         return ret;
      }

      /// <summary>
      ///    Nexts the float.
      /// </summary>
      /// <returns>System.Single.</returns>
      public float NextFloat()
      {
         // Let GetInt do the checking
         var ret = GetFloat(0);

         length -= 4;
         index += 4;

         return ret;
      }

      /// <summary>
      ///    Nexts the Int16.
      /// </summary>
      /// <returns>System.Int16.</returns>
      public short NextInt16()
      {
         // Let GetShort do the checking
         var ret = GetInt16(0);

         length -= 2;
         index += 2;

         return ret;
      }

      /// <summary>
      ///    Nexts the Int32.
      /// </summary>
      /// <returns>System.Int32.</returns>
      public int NextInt32()
      {
         // Let GetInt do the checking
         var ret = GetInt32(0);

         length -= 4;
         index += 4;

         return ret;
      }

      /// <summary>
      ///    Nexts the Int64.
      /// </summary>
      /// <returns>System.Int64.</returns>
      public long NextInt64()
      {
         // Let GetInt do the checking
         var ret = GetInt64(0);

         length -= 8;
         index += 8;

         return ret;
      }

      /// <summary>
      ///    Nexts the UInt16.
      /// </summary>
      /// <returns>System.UInt16.</returns>
      public ushort NextUInt16()
      {
         return (ushort)NextInt16();
      }

      /// <summary>
      ///    Nexts the UInt32.
      /// </summary>
      /// <returns>System.UInt32.</returns>
      public uint NextUInt32()
      {
         return (uint)NextInt32();
      }

      /// <summary>
      ///    Nexts the UInt64.
      /// </summary>
      /// <returns>System.UInt64.</returns>
      public ulong NextUInt64()
      {
         return (ulong)NextInt64();
      }

      /// <summary>
      ///    Nexts the UTF8 string.
      /// </summary>
      /// <param name="length">The length.</param>
      /// <returns>System.String.</returns>
      public string NextUtf8String(int length)
      {
         // Let GetUTFString do the checking
         var ret = GetUTF8String(0, length);

         this.length -= length;
         index += length;

         return ret;
      }

      /// <summary>
      ///    Create a return BufferChunk containing a subset of the data from the valid data.
      /// </summary>
      /// <param name="index">int index into the valid data area</param>
      /// <param name="length">int length of the data to copy</param>
      /// <returns>BufferChunk length Length that was extracted from the source BufferChunk</returns>
      public BufferChunk Peek(int index, int length)
      {
         return new BufferChunk(buffer, this.index + index, length);
      }

      /// <summary>
      ///    Reset the BufferChunk's Index and Length pointers to zero so it is ready for reuse as an empty BufferChunk.
      ///    Note that the actual byte[] buffer is not reset, so the memory is not deallocated/reallocated, allowing for
      ///    more efficient reuse of memory without abusing the GC
      /// </summary>
      public void Reset()
      {
         index = 0;
         length = 0;
      }

      /// <summary>
      ///    Reset the BufferChunk's Index and Length pointers to supplied values
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="length">The length.</param>
      public void Reset(int index, int length)
      {
         ValidateNonNegative(index);
         ValidateNonNegative(length);

         if ((index + length) > buffer.Length) {
            ExpandBuffer(length);
            length += buffer.Length;
         }

         ValidatePointerData(index, length, buffer.Length);

         this.index = index;
         this.length = length;
      }

      /// <summary>
      ///    Modifies 1 byte inside the BufferChunk
      ///    This method is included for consistency.  It simply forwards to the indexer.
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetByte(int index, byte data)
      {
         // Let the indexer do the checking
         this[index] = data;
      }

      /// <summary>
      ///    Modifies 2 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetInt16(int index, short data)
      {
         ValidateNonNegative(index);

         if (2 > (length - index)) {
            ExpandBuffer();
         }

         ValidateSufficientSpace(2, length - index);

         _SetInt16(index, data);
      }

      /// <summary>
      ///    Modifies 4 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetInt32(int index, int data)
      {
         ValidateNonNegative(index);

         if (4 > (length - index)) {
            ExpandBuffer();
         }

         ValidateSufficientSpace(4, length - index);

         _SetInt32(index, data);
      }

      /// <summary>
      ///    Modifies 8 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetInt64(int index, long data)
      {
         SetUInt64(index, (ulong)data);
      }

      /// <summary>
      ///    Sets the padded UInt16.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="data">The data.</param>
      /// <summary>
      public void SetPaddedUInt16(int index, ushort data)
      {
         ValidateNonNegative(index);

         if (1 > (length - index)) {
            ExpandBuffer();
         }

         ValidateSufficientData(1, length - index);

         var dataSize = 2;
         if ((index + dataSize) < length) // All the data requested
         {
            _SetInt16(index, (short)data);
         } else // The rest of the data in the buffer
         {
            var offset = 0;
            while ((index + offset) < length) {
               int shift;

               if (littleEndian) {
                  shift = dataSize - offset - 1;
               } else {
                  shift = offset;
               }

               buffer[this.index + index + offset] = (byte)(data >> (shift * 8));
               offset++;
            }
         }
      }

      /// <summary>
      ///    Sets the padded UInt32.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="data">The data.</param>
      public void SetPaddedUInt32(int index, uint data)
      {
         ValidateNonNegative(index);

         if (1 > (length - index)) {
            ExpandBuffer();
         }

         ValidateSufficientData(1, length - index);

         var dataSize = 4;
         if ((index + dataSize) < length) // All the data requested
         {
            _SetInt32(index, (int)data);
         } else // The rest of the data in the buffer
         {
            var offset = 0;
            while ((index + offset) < length) {
               int shift;

               if (littleEndian) {
                  shift = dataSize - offset - 1;
               } else {
                  shift = offset;
               }

               buffer[this.index + index + offset] = (byte)(data >> (shift * 8));
               offset++;
            }
         }
      }

      /// <summary>
      ///    Sets the padded UInt64.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="data">The data.</param>
      public void SetPaddedUInt64(int index, ulong data)
      {
         var dataSize = 8;
         var availableData = length - index;

         ValidateNonNegative(index);

         if (1 > availableData) {
            ExpandBuffer();
         }

         ValidateSufficientData(1, availableData);

         if (dataSize < availableData) // All the data requested
         {
            _SetUInt64(index, data);
         } else // The rest of the data in the buffer
         {
            var indexOffset = this.index + index;

            if (littleEndian) {
               for (int offset = 0, shift = dataSize - 1; offset < availableData; offset++, shift--) {
                  buffer[indexOffset + offset] = (byte)(data >> (shift * 8));
               }
            } else {
               for (int offset = 0, shift = 0; offset < availableData; offset++, shift++) {
                  buffer[indexOffset + offset] = (byte)(data >> (shift * 8));
               }
            }
         }
      }

      /// <summary>
      ///    Modifies 2 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetUInt16(int index, ushort data)
      {
         SetInt16(index, (short)data);
      }

      /// <summary>
      ///    Modifies 4 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetUInt32(int index, uint data)
      {
         SetInt32(index, (int)data);
      }

      /// <summary>
      ///    Modifies 8 bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetUInt64(int index, ulong data)
      {
         ValidateNonNegative(index);

         if (8 > (length - index)) {
            ExpandBuffer();
         }

         ValidateSufficientSpace(8, length - index);

         _SetUInt64(index, data);
      }

      /// <summary>
      ///    Modifies UTF8.GetBytes(data) bytes inside the BufferChunk
      /// </summary>
      /// <param name="index">Offset into the valid data</param>
      /// <param name="data">Value to write at index</param>
      public void SetUTF8String(int index, string data)
      {
         utf8.GetBytes(data, 0, data.Length, buffer, this.index + index);
      }

      /// <summary>
      ///    Skips the specified length.
      /// </summary>
      /// <param name="len">The length.</param>
      public void Skip(int len)
      {
         for (var i = 0; i < len; i++) {
            NextByte();
         }
      }

      #endregion

      #region Private methods

      /// <summary>
      ///    Gets the bytes.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns>System.Byte[].</returns>
      private static unsafe byte[] GetBytes(float value)
      {
         var bytes = new byte[4];

         fixed (byte* b = bytes) {
            *(int*)b = *(int*)&value;
         }

         return bytes;
      }

      /// <summary>
      ///    Gets the bytes.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns>System.Byte[].</returns>
      private static unsafe byte[] GetBytes(double value)
      {
         var bytes = new byte[8];

         fixed (byte* b = bytes) {
            *(int*)b = *(int*)&value;
         }

         return bytes;
      }

      /// <summary>
      ///    Validates the IntPtr
      ///    Make sure it is not a null pointer
      /// </summary>
      /// <param name="ptr">The IntPtr.</param>
      /// <exception cref="ArgumentException">Null pointers are invalid</exception>
      /// <exception cref="System.ArgumentException">Null pointers are invalid</exception>
      private static void ValidateIntPtr(IntPtr ptr)
      {
         if (ptr == IntPtr.Zero) {
            throw new ArgumentException("Null pointers are invalid");
         }
      }

      /// <summary>
      ///    Validates the non negative.
      ///    Index and Length must be &gt;= 0
      /// </summary>
      /// <param name="val">The val.</param>
      /// <exception cref="ArgumentOutOfRangeException">val - All Integer Values Must Be Positive</exception>
      private static void ValidateNonNegative(int val)
      {
         if (val < 0) {
            throw new ArgumentOutOfRangeException("val", val, "All Integer Values Must Be Positive");
         }
      }

      /// <summary>
      ///    Validates the length of the not zero.
      ///    BufferChunk does not accept or create objects of zero length
      /// </summary>
      /// <param name="length">The length.</param>
      /// <exception cref="TM.NoDataException">BufferChunk Does Not Accept Zero Length</exception>
      private static void ValidateNotZeroLength(int length)
      {
         if (length == 0) {
            throw new NoDataException("BufferChunk Does Not Accept Zero Length");
         }
      }

      /// <summary>
      ///    Validates the object.
      ///    BufferChunk does not accept or create null objects
      /// </summary>
      /// <param name="o">The o.</param>
      /// <exception cref="ArgumentNullException">BufferChunk doesn't accept null objects</exception>
      private static void ValidateObject(object o)
      {
         if (o == null) {
            throw new ArgumentNullException("BufferChunk doesn't accept null objects");
         }
      }

      /// <summary>
      ///    Validates the pointer data.
      ///    When setting pointers (Index and Length), make sure they fall within the valid
      ///    data area - buffer.Length or this.length (valid data)
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="length">The length.</param>
      /// <param name="dataLength">Length of the data.</param>
      /// <exception cref="ArgumentOutOfRangeException"></exception>
      private static void ValidatePointerData(int index, int length, int dataLength)
      {
         if ((index + length) > dataLength) {
            throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture,
                                                                "Index And Length Invalid Data", index, length, dataLength));
         }
      }

      /// <summary>
      ///    Validates the sufficient data.
      ///    Make sure we have as much data as they are requesting
      /// </summary>
      /// <param name="requested">The requested.</param>
      /// <param name="actual">The actual.</param>
      /// <exception cref="TM.InsufficientDataException"></exception>
      private static void ValidateSufficientData(int requested, int actual)
      {
         if (requested > actual) {
            throw new InsufficientDataException(string.Format(CultureInfo.CurrentCulture, "BufferChunk Does Not Have Enough Data", requested, actual));
         }
      }

      /// <summary>
      ///    Validates the sufficient space.
      ///    Make sure we have as much space as they are requesting
      /// </summary>
      /// <param name="requested">The requested.</param>
      /// <param name="actual">The actual.</param>
      /// <exception cref="TM.InsufficientSpaceException"></exception>
      private static void ValidateSufficientSpace(int requested, int actual)
      {
         if (requested > actual) {
            throw new InsufficientSpaceException(string.Format(CultureInfo.CurrentCulture, "BufferChunk Does Not Have Enough Buffer", requested, actual));
         }
      }

      /// <summary>
      ///    Gets the double.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.Double.</returns>
      private unsafe double _GetDouble(int index)
      {
         double ret;

         fixed (byte* pb = &buffer[this.index + index]) {
            ret = *(double*)pb;
         }

         return ret;
      }

      /// <summary>
      ///    Gets the float.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.Single.</returns>
      private unsafe float _GetFloat(int index)
      {
         float ret;

         fixed (byte* pb = &buffer[this.index + index]) {
            ret = *(float*)pb;
         }

         return ret;
      }

      /// <summary>
      ///    Gets Int16.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.Int16.</returns>
      private short _GetInt16(int index)
      {
         short ret;

         // BigEndian network -> LittleEndian architecture
         if (littleEndian) {
            ret = (short)(buffer[this.index + index + 0] << (1 * 8));
            ret += (short)(buffer[this.index + index + 1] << (0 * 8));
         } else // BigEndian network -> BigEndian architecture
         {
            ret = (short)(buffer[this.index + index + 0] << (0 * 8));
            ret += (short)(buffer[this.index + index + 1] << (1 * 8));
         }

         return ret;
      }

      /// <summary>
      ///    Gets Int32.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.Int32.</returns>
      private int _GetInt32(int index)
      {
         int ret;

         // BigEndian network -> LittleEndian architecture
         if (littleEndian) {
            ret = buffer[this.index + index + 0] << (3 * 8);
            ret += buffer[this.index + index + 1] << (2 * 8);
            ret += buffer[this.index + index + 2] << (1 * 8);
            ret += buffer[this.index + index + 3] << (0 * 8);
         } else // BigEndian network -> BigEndian architecture
         {
            ret = buffer[this.index + index + 0] << (0 * 8);
            ret += buffer[this.index + index + 1] << (1 * 8);
            ret += buffer[this.index + index + 2] << (2 * 8);
            ret += buffer[this.index + index + 3] << (3 * 8);
         }

         return ret;
      }

      /// <summary>
      ///    Gets the Uint64.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <returns>System.UInt64.</returns>
      private unsafe ulong _GetUInt64(int index)
      {
         ulong ret;

         fixed (byte* pb = &buffer[this.index + index]) {
            ret = *(ulong*)pb;
         }

         return ret;
      }

      /// <summary>
      ///    Sets Int16.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="data">The data.</param>
      private void _SetInt16(int index, short data)
      {
         // LittleEndian architecture -> BigEndian network
         if (littleEndian) {
            buffer[this.index + index + 0] = (byte)(data >> (1 * 8));
            buffer[this.index + index + 1] = (byte)(data >> (0 * 8));
         } else // BigEndian architecture -> BigEndian network
         {
            buffer[this.index + index + 0] = (byte)(data >> (0 * 8));
            buffer[this.index + index + 1] = (byte)(data >> (1 * 8));
         }
      }

      /// <summary>
      ///    Sets Int32.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="data">The data.</param>
      private void _SetInt32(int index, int data)
      {
         // LittleEndian architecture -> BigEndian network
         if (littleEndian) {
            buffer[this.index + index + 0] = (byte)(data >> (3 * 8));
            buffer[this.index + index + 1] = (byte)(data >> (2 * 8));
            buffer[this.index + index + 2] = (byte)(data >> (1 * 8));
            buffer[this.index + index + 3] = (byte)(data >> (0 * 8));
         } else // BigEndian architecture -> BigEndian network
         {
            buffer[this.index + index + 0] = (byte)(data >> (0 * 8));
            buffer[this.index + index + 1] = (byte)(data >> (1 * 8));
            buffer[this.index + index + 2] = (byte)(data >> (2 * 8));
            buffer[this.index + index + 3] = (byte)(data >> (3 * 8));
         }
      }

      /// <summary>
      ///    Sets UInt64.
      /// </summary>
      /// <param name="index">The index.</param>
      /// <param name="data">The data.</param>
      private unsafe void _SetUInt64(int index, ulong data)
      {
         fixed (byte* pb = &buffer[this.index + index]) {
            *(ulong*)pb = data;
         }
      }

      /// <summary>
      ///    Expands the buffer.
      /// </summary>
      /// <param name="len">The length.</param>
      private void ExpandBuffer(int len = 1024)
      {
         var largerBuffer = new byte[length + len];

         if ((buffer != null) &&
             (length > 0)) {
            Array.Copy(buffer, 0, largerBuffer, 0, length);
         }

         //length = 0;//this.length + len;
         buffer = largerBuffer;
      }

      #endregion
   }

   /// <summary>
   ///    Class InsufficientDataException
   ///    Raised when requesting more data than current buffer holds
   ///    <br/>Implements the <see cref="System.ApplicationException" />
   /// </summary>
   /// <seealso cref="System.ApplicationException" />
   public class InsufficientDataException : ApplicationException
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="InsufficientDataException" /> class.
      /// </summary>
      public InsufficientDataException()
      {
      }

      /// <summary>
      ///    Initializes a new instance of the <see cref="InsufficientDataException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      public InsufficientDataException(string msg) : base(msg)
      {
      }

      /// <summary>
      ///    Initializes a new instance of the <see cref="InsufficientDataException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      /// <param name="inner">The inner.</param>
      public InsufficientDataException(string msg, Exception inner) : base(msg, inner)
      {
      }

      #endregion
   }

   /// <summary>
   ///    Class InsufficientSpaceException
   ///    Raised when trying to add more data than current buffer can hold
   ///    <br/>Implements the <see cref="System.ApplicationException" />
   /// </summary>
   /// <seealso cref="System.ApplicationException" />
   public class InsufficientSpaceException : ApplicationException
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="InsufficientSpaceException" /> class.
      /// </summary>
      public InsufficientSpaceException()
      {
      }

      /// <summary>
      ///    Initializes a new instance of the <see cref="InsufficientSpaceException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      public InsufficientSpaceException(string msg) : base(msg)
      {
      }

      /// <summary>
      ///    Initializes a new instance of the <see cref="InsufficientSpaceException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      /// <param name="inner">The inner.</param>
      public InsufficientSpaceException(string msg, Exception inner) : base(msg, inner)
      {
      }

      #endregion
   }

   /// <summary>
   ///    Class NoDataException
   ///    Raised when requesting more data than current buffer holds
   ///    <br/>Implements the <see cref="System.ApplicationException" />
   /// </summary>
   public class NoDataException : ApplicationException
   {
      #region Constructors and destructors

      /// <summary>
      /// Initializes a new instance of the <see cref="NoDataException" /> class.
      /// </summary>
      public NoDataException()
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="NoDataException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      public NoDataException(string msg) : base(msg)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="NoDataException" /> class.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      /// <param name="inner">The inner.</param>
      public NoDataException(string msg, Exception inner) : base(msg, inner)
      {
      }

      #endregion
   }

   public static class BufferChunkExt 
   {

   }
}
