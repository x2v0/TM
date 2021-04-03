// $Id: $

/*************************************************************************
 *                                                                       *
 * Copyright (C) 2021,   Valeriy Onuchin                                 *
 * All rights reserved.                                                  *
 *                                                                       *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using TMSrv;

namespace TM
{
   /// <summary>
   /// Class Extensions
   /// </summary>
   public static class Extensions
   {
      #region Public methods

      /// <summary>
      /// Adds the specified header.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <param name="header">The header.</param>
      /// <returns>BufferChunk.</returns>
      public static BufferChunk Add(this BufferChunk buf, TMPacketHeader header)
      {
         buf.Reset();

         buf += header.sign;
         buf += header.type;
         buf += header.value;
         buf += header.reserved;
         buf += header.datalength;
         buf += header.packet_number;

         return buf;
      }

      /// <summary>
      /// Adds the specified plan.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <param name="plan">The plan.</param>
      /// <returns>BufferChunk.</returns>
      public static BufferChunk Add(this BufferChunk buf, PlanSpot plan)
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
      /// Bytes the array to structure.
      /// </summary>
      /// <param name="bytearray">The bytearray.</param>
      /// <param name="structureObj">The structure object.</param>
      /// <param name="position">The position.</param>
      /// <returns>System.Object.</returns>
      public static object ByteArrayToStructure(this byte[] bytearray, object structureObj, int position)
      {
         var length = Marshal.SizeOf(structureObj);
         var ptr = Marshal.AllocHGlobal(length);
         Marshal.Copy(bytearray, 0, ptr, length);
         structureObj = Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(bytearray, position), structureObj.GetType());
         Marshal.FreeHGlobal(ptr);
         return structureObj;
      }

      /// <summary>
      /// Copies the specified hist.
      /// </summary>
      /// <param name="hist">The hist.</param>
      /// <returns>System.Double[][].</returns>
      public static double[] Copy(this double[] hist)
      {
         if (hist == null) {
            return null;
         }

         var array = new double[hist.Length];
         Buffer.BlockCopy(hist, 0, array, 0, hist.Length << 3);

         return array;
      }

      /// <summary>
      /// This is not suitable for large files because the SEND() buffer may get filled up and throw an exception
      /// if you attempt to write to it. You should change this to use the strongly typed networkstream and ensure
      /// you have enough room to send data
      /// </summary>
      /// <param name="source">The source.</param>
      /// <param name="destination">The destination.</param>
      /// <param name="completed">The completed.</param>
      public static void Copy(this Stream source, Stream destination, Action<Stream, Stream, Exception> completed = null)
      {
         var buffer = new byte[1024 * 16];
         int read;

         try {
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0) {
               destination.Write(buffer, 0, read);
            }

            if (completed != null) {
               completed(source, destination, null);
            }
         } catch (Exception ex) {
            Console.WriteLine(MethodBase.GetCurrentMethod() + " : " + ex.Message);

            if (completed != null) {
               completed(source, destination, ex);
            }
         }

         source.Close();
      }

      /// <summary>
      /// Descriptions the specified value.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns>System.String.</returns>
      public static string Description(this Enum value)
      {
         var str = value.ToString();
         var fi = value.GetType().GetField(str);

         if (fi == null) {
            return str;
         }

         var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

         return attributes.Length > 0 ? attributes[0].Description : str;
      }

      /// <summary>
      /// Mcs the pt ilh.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>MC_PT_ILH.</returns>
      public static MC_PT_ILH MC_PT_ILH(this BufferChunk buf)
      {
         var state = new MC_PT_ILH();

         try {
            state.sign = Encoding.ASCII.GetBytes("TMCL");
            state.type = buf.NextInt32();
            state.s_info = buf.NextInt32();
            state.t_procent = buf.NextDouble();
            state.ir_tm = buf.NextInt32();
            state.spots_count = buf.NextInt32();
            return state;
         } catch {
            state.sign = Encoding.ASCII.GetBytes("0000");
            return state;
         }

         return state;
      }

      /// <summary>
      /// Nexts the server state.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;MCS_State_topass&gt;.</returns>
      public static MCS_State_topass MCS_State(this BufferChunk buf)
      {
         var state = new MCS_State_topass();

         try {
            state.state = buf.NextInt32();
            state.lasterror = buf.NextUInt32();
            state.spots_passed = buf.NextUInt32();
            state.spots_count = buf.NextUInt32();
            return state;
         } catch {
            state.state = (int) EServerState.UNKNOWN;
            return state;
         }

         return state;
      }

      /// <summary>
      /// Nexts the full spot.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;PlanSpotFull&gt;.</returns>
      public static PlanSpotFull? NextFullSpot(this BufferChunk buf)
      {
         var plan = new PlanSpotFull();

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

            return plan;
         } catch {
            //plan.id = -1; //
            return null;
         }

         return plan;
      }

      /// <summary>
      /// Nexts the TMPacketHeader
      /// </summary>
      /// <param name="buf">The buffer chunk.</param>
      /// <returns>TMPacketHeader.</returns>
      public static TMPacketHeader NextPacketHeader(this BufferChunk buf)
      {
         var header = new TMPacketHeader();
         try {
            header.sign = new byte[4];

            for (var i = 0; i < 4; i++) {
               header.sign[i] = buf.NextByte();
            }

            header.type = buf.NextByte();
            header.value = buf.NextByte();
            header.reserved = new byte[2];
            header.reserved[0] = buf.NextByte();
            header.reserved[1] = buf.NextByte();
            header.datalength = buf.NextUInt32();
            header.packet_number = buf.NextInt32();
         } catch {
            header.packet_number = -1; //
         }

         return header;
      }


      /// <summary>
      /// Nexts the plan spot result.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;PlanSpotResult&gt;.</returns>
      public static PlanSpotResult? NextPlanSpotResult(this BufferChunk buf)
      {
         var plan = new PlanSpotResult();

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

      /// <summary>
      /// Nexts the result spot.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;PlanSpotResult&gt;.</returns>
      public static PlanSpotResult? NextResultSpot(this BufferChunk buf)
      {
         var plan = new PlanSpotResult();

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
      /// Nexts the PlanSpot.
      /// </summary>
      /// <param name="buf">The buffer chunk.</param>
      /// <returns>PlanSpot.</returns>
      public static PlanSpot? NextSpot(this BufferChunk buf)
      {
         var plan = new PlanSpot();

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
      /// Nexts the PlanSpotFull.
      /// </summary>
      /// <param name="buf">The buffer chunk.</param>
      /// <returns>PlanSpotFull.</returns>
      public static PlanSpotFull? NextSpotFull(this BufferChunk buf)
      {
         var plan = new PlanSpotFull();

         try {
            plan.id = buf.NextInt32();
            plan.xangle = buf.NextFloat();
            plan.zangle = buf.NextFloat();
            plan.energy = buf.NextFloat();
            plan.pcount = buf.NextFloat();

            plan.done = buf.NextInt32();
            plan.result_xangle = buf.NextFloat();
            plan.result_zangle = buf.NextFloat();
            plan.result_pcount = buf.NextInt32();
            plan.changed = buf.NextInt32();
            plan.need_to_sent = buf.NextInt32();
         } catch {
            //plan.id = -1; //
            return null;
         }

         return plan;
      }

      /// <summary>
      /// Nexts the spot result.
      /// </summary>
      /// <param name="buf">The buf.</param>
      /// <returns>System.Nullable&lt;PlanSpotResult&gt;.</returns>
      public static PlanSpotResult? NextSpotResult(this BufferChunk buf)
      {
         var plan = new PlanSpotResult();

         try {
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
      /// Splits the string.
      /// </summary>
      /// <param name="s">The s.</param>
      /// <param name="length">The length.</param>
      /// <returns>IEnumerable&lt;System.String&gt;.</returns>
      public static IEnumerable<string> SplitString(this string s, int length)
      {
         var buf = new char[length];
         using (var rdr = new StringReader(s)) {
            int l;
            l = rdr.ReadBlock(buf, 0, length);

            while (l > 0) {
               yield return new string(buf, 0, l) + Environment.NewLine;
               l = rdr.ReadBlock(buf, 0, length);
            }
         }
      }

      /// <summary>
      /// To the bool.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <param name="def">if set to <c>true</c> [def].</param>
      /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
      public static bool ToBool(this string str, bool def)
      {
         bool val;
         return bool.TryParse(str, out val) ? val : def;
      }

      /// <summary>
      /// Converts the 16bit to to 8bit array.
      /// </summary>
      /// <param name="arr">The arr.</param>
      /// <returns>System.Byte[][].</returns>
      public static byte[] ToByte(this short[] arr)
      {
         lock (arr) {
            var ret = new byte[arr.Length];
            for (var i = 0; i < arr.Length; i++) {
               ret[i] = (byte) ((ushort) arr[i] >> 8);
            }

            //Parallel.For(0, arr.Length, index => ret[index] = (byte) ((ushort) arr[index] >> 8));
            return ret;
         }
      }

      /// <summary>
      /// Converts to degrees.
      /// </summary>
      /// <param name="radians">The radians.</param>
      /// <returns>System.Single.</returns>
      public static float ToDegrees(this float radians)
      {
         var degrees = (float) ((180 / Math.PI) * radians);
         return degrees;
      }

      /// <summary>
      /// To the double.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <param name="def">The def.</param>
      /// <returns>System.Double.</returns>
      public static double ToDouble(this string str, double def)
      {
         double val;
         return double.TryParse(str, out val) ? val : def;
      }

      /// <summary>
      /// To the float.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <param name="def">The def.</param>
      /// <returns>System.Single.</returns>
      public static float ToFloat(this string str, float def)
      {
         float val;
         return float.TryParse(str, out val) ? val : def;
      }

      /// <summary>
      /// To the int.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <param name="def">The def.</param>
      /// <returns>System.Int32.</returns>
      public static int ToInt(this string str, int def)
      {
         int val;
         return int.TryParse(str, out val) ? val : def;
      }

      /// <summary>
      /// Writes the multi line.
      /// </summary>
      /// <param name="lstring">The lstring.</param>
      /// <param name="len">The length.</param>
      public static void WriteMultiLine(this string lstring, int len = 110)
      {
         var strs = lstring.SplitString(len);

         foreach (var str in strs) {
            Console.WriteLine("\t" + str);
         }
      }

      #endregion
   }
}
