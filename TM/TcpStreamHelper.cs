// $Id: $

/*************************************************************************
 *                                                                       *
 * Copyright (C) 2021,   Valeriy Onuchin                                 *
 * All rights reserved.                                                  *
 *                                                                       *
 *************************************************************************/

using System;
using System.IO;

namespace TM
{
   /// <summary>
   /// Class TcpStreamHelper.
   /// </summary>
   internal static class TcpStreamHelper
   {
      #region Public methods

      // This is not suitable for large files because the SEND() buffer may get filled up and throw an exception
      // if you attempt to write to it. You should change this to use the strongly typed networkstream and ensure
      // you have enough room to send data
      /// <summary>
      /// Copies the stream to stream.
      /// </summary>
      /// <param name="source">The source.</param>
      /// <param name="destination">The destination.</param>
      /// <param name="completed">The completed.</param>
      public static void CopyStreamToStream(Stream source, Stream destination, Action<Stream, Stream, Exception> completed)
      {
         var buffer = new byte[0x1000];
         int read;

         try {
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0) {
               destination.Write(buffer, 0, read);
            }

            if (completed != null) {
               completed(source, destination, null);
            }
         } catch (Exception exc) {
            if (completed != null) {
               completed(source, destination, exc);
            }
         }
      }

      #endregion
   }
}
