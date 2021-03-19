// $Id: $

/*************************************************************************
 *                                                                       *
 * Copyright (C) 2021,   Valeriy Onuchin                                 *
 * All rights reserved.                                                  *
 *                                                                       *
 *************************************************************************/

//TM_protocol.h : protocol for ThermoControl Server and Clients dialogs
//                                                  
// autor: Aleksey Shestopalov 
// email: ashestopalov@yandex.ru
// (c) Protom                                       
// 


using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace TM
{
   /// <summary>
   ///    ���� ������ ������������ ����������
   /// </summary>
   public enum EConfigTag
   {
      /// <summary>
      ///    ������ ������, ������ PACKET_VERTION_STR
      /// </summary>
      [Description("������ ������")]
      VER = 1,

      /// <summary>
      ///   �������, ParceCfgPacket_GetLinkData
      /// </summary>
      [Description("�������")]
      CONNECT = 2,

      /// <summary>
      ///    ������������ ������: 
      /// </summary>
      [Description("������������ ������")]
      DEVICE = 3,

      /// <summary>
      ///    ���� ������������� (��� NetRT -> TmNetScan)
      /// </summary>
      [Description("���� �������������")]
      RTLINK = 4,

      /// <summary>
      ///    ��������� ���������� � ��������� ����
      /// </summary>
      [Description("��������� ���������� � ��������� ����")]
      TXTINFO = 5,

      /// <summary>
      ///    ���� ������������ ����������, ������� �� ����������
      /// </summary>
      [Description("���� ������������ ����������")]
      DEVCONFIG = 6
   }

   /// <summary>
   ///    ������ ���������� ������, ����������� �����������
   /// </summary>
   public enum EDevErrorStatus
   {
      /// <summary>
      ///    ����������� ������
      /// </summary>
      [Description("����������� ������")]
      INFORMATION = 0,

      /// <summary>
      ///    ��������������
      /// </summary>
      [Description("��������������")]
      WARNING = 1,

      /// <summary>
      ///    a��������� ��������
      /// </summary>
      [Description("a��������� ��������")]
      PROBLEM = 2
   }

   /// <summary>
   ///    ��������� ����������
   /// </summary>
   public enum EDeviceStatus
   {
      /// <summary>
      ///    ��������� ��� �������������������
      /// </summary>
      [Description("��������� ��� �������������������")]
      OFF = 0, //

      /// <summary>
      ///    ��������
      /// </summary>
      [Description("��������")]
      OK = 1, //

      /// <summary>
      ///    ��������������
      /// </summary>
      [Description("��������������")]
      ATTENTION = 2, //

      /// <summary>
      ///    �������
      /// </summary>
      [Description("�������")]
      HOT = 3,

      /// <summary>
      ///    ��� �����
      /// </summary>
      [Description("��� �����")]
      OPEN = HOT,

      /// <summary>
      ///    ����������
      /// </summary>
      [Description("����������")]
      CRACKED = 4,

      /// <summary>
      ///    � �������� - ����������� ������, ��������� � �.�.
      /// </summary>
      [Description("� ��������")]
      MOVING = 5, //

      /// <summary>
      ///    � ������, �� �� ����������� ����������, �������� ��� �������� �������
      /// </summary>
      [Description("� ������")]
      IN_WORK = 6 //
   }

   /// <summary>
   ///    ������� ���� TCPSrvServer::TMSettings ���
   ///    TCPSrvServer::TMSettings, ��������� ��������� ��������������
   /// </summary>
   public enum EPacketSettings
   {
      /// <summary>
      ///    ������� ���. ��������
      /// </summary>
      [Description("������� ���. ��������")]
      NULL = 0,

      /// <summary>
      ///    ��������� ����������� ����� � ������� � �������
      /// </summary>
      [Description("��������� ����������� ����� � ������� � �������")]
      AddCheckSumm = 1,

      /// <summary>
      ///    ������� � ��������� ����������� ����� � ������� �� �������
      /// </summary>
      [Description("������� � ��������� ����������� ����� � ������� �� �������")]
      ExpectCheckSumm = 2,

      /// <summary>
      ///    ��������� �������� ������ ������ (�������� ���� TMpacketSizeLimit)
      /// </summary>
      [Description("��������� �������� ������ ������")]
      CheckPacketSize = 4
   }

   //#define TM_PROTOCOL_VERTION		"4.0.2"
   /// <summary>
   ///    Enum EPacketType
   /// </summary>
   public enum EPacketType
   {
      /// <summary>
      ///    The command
      /// </summary>
      Command = 1,

      /// <summary>
      ///    The data
      /// </summary>
      Data = 2,

      /// <summary>
      ///    The information
      /// </summary>
      Info = 3,

      /// <summary>
      ///    The error
      /// </summary>
      Error = 4
   }

   /// <summary>
   ///    ���� "�������" ��� ��������� (�����������+�������+�������������)
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct PlanSpot
   {
      /// <summary>
      ///    The length of structure
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(PlanSpot));

      #region  Properties

      /// <summary>
      ///    ���������� ������������� ������ (����. �������)
      /// </summary>
      [Description("���������� ������������� ������ (����. �������)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� �����������
      /// </summary>
      [Description("���� �� �����������")]
      public float xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� ���������
      /// </summary>
      [Description("���� �� ���������")]
      public float zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    �������, MeV
      /// </summary>
      [Description("�������, MeV")]
      public float energy
      {
         get;
         set;
      }

      /// <summary>
      ///    ���������� ��������
      /// </summary>
      [Description("���������� ��������")]
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
   ///    ����������� ��������� <see cref="TM.PlanSpot" /> + <see cref="TM.PlanSpotResult" /> 
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct PlanSpotFull
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(PlanSpotFull));

      #region  Properties

      /// <summary>
      ///    ���������� ������������� ������ (����. �������)
      /// </summary>
      [Description("���������� ������������� ������ (����. �������)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� �����������
      /// </summary>
      [Description("���� �� �����������")]
      public float xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� ���������
      /// </summary>
      [Description("���� �� ���������")]
      public float zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    �������, MeV
      /// </summary>
      [Description("�������, MeV")]
      public float energy
      {
         get;
         set;
      }

      /// <summary>
      ///    ���������� ��������
      /// </summary>
      [Description("���������� ��������")]
      public float pcount
      {
         get;
         set;
      }

      /// <summary>
      ///    ������� ������
      /// </summary>
      [Description("������� ������")]
      public int done
      {
         get;
         set;
      }

      /// <summary>
      ///    ��������� ���� �� �����������
      /// </summary>
      [Description("��������� ���� �� �����������")]
      public float result_xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    ��������� ���� �� ���������
      /// </summary>
      [Description("��������� ���� �� ���������")]
      public float result_zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    ��������� ���������� ��������
      /// </summary>
      [Description("��������� ���������� ��������")]
      public float result_pcount
      {
         get;
         set;
      }

      /// <summary>
      ///    ��������� ����������
      /// </summary>
      [Description("��������� ����������")]
      public int changed
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �������� ��������� �������
      /// </summary>
      [Description("���� �������� ��������� �������")]
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
         sb.AppendLine(", done = " + done + ", result_xangle = " + 
                       result_xangle + ", result_zangle = " + result_zangle + 
                       ", result_pcount = " + result_pcount);

         return sb.ToString();
      }
   } //44bytes

   /// <summary>
   ///    ���� "�������" ��� ��������� (�����������+�������+�������������)
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct PlanSpotTopass
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(PlanSpotTopass));

      /// <summary>
      ///    ���������� ������������� ������ (����. �������)
      /// </summary>
      [Description("���������� ������������� ������ (����. �������)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� �����������
      /// </summary>
      [Description("���� �� �����������")]
      public float xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� ���������
      /// </summary>
      [Description("���� �� ���������")]
      public float zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    �������, MeV
      /// </summary>
      [Description("�������, MeV")]
      public float energy;

      /// <summary>
      ///    ���������� ��������
      /// </summary>
      [Description("���������� ��������")]
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
   ///   ��������� � ������������ ���������� ����� ���������
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct PlanSpotResult
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(PlanSpotResult));

      #region  Properties

      /// <summary>
      ///    ���������� ������������� ������ (����. �������)
      /// </summary>
      [Description("���������� ������������� ������ (����. �������)")]
      public int id
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� �����������
      /// </summary>
      [Description("���� �� �����������")]
      public float result_xangle
      {
         get;
         set;
      }

      /// <summary>
      ///    ���� �� ���������
      /// </summary>
      [Description("���� �� ���������")]
      public float result_zangle
      {
         get;
         set;
      }

      /// <summary>
      ///    ���������� ��������
      /// </summary>
      [Description("���������� ��������")]
      public float result_pcount
      {
         get;
         set;
      }

      /// <summary>
      ///    ��������� ���������� MCS_SHOT_RESULT_DONE
      /// </summary>
      [Description("��������� ���������� MCS_SHOT_RESULT_DONE")]
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
   } //20bytes

   /// <summary>
   ///    Enum EServerType
   /// </summary>
   public enum EServerType
   {
      /// <summary>
      ///    ����������� ���. ������ ��� ����-��������
      /// </summary>
      [Description("����������� ���")]
      UNKNOWN = 0,

      /// <summary>
      ///    ������������� ������ �������� TM, ��������� Linklib
      /// </summary>
      [Description("������������� ������")]
      TEMPERATURE = 1,

      /// <summary>
      ///    ������������� ������ �������� TM, ��������� Linklib
      /// </summary>
      [Description("������������� ������")]
      XRAY = 2,

      /// <summary>
      ///    ������ ������� �������� �������� - TM �������� �� ���������� ���������
      /// </summary>
      [Description("������ ������� �������� ��������")]
      PFS = 3,

      /// <summary>
      ///    ������ - ��������� ���������� ���������� TMC, �������� TM
      /// </summary>
      [Description("������ - ��������� ���������� ���������� TMC")]
      TMC = 4,

      /// <summary>
      ///    ������ ���/���- TM �������� �� ����������, ���������
      /// </summary>
      [Description("������ ���/���")]
      DACADC = 5, //

      /// <summary>
      ///    ���������� ������ - TM �������� �� ����������, ���������
      /// </summary>
      [Description("���������� ������")]
      ACC = 6, //

      /// <summary>
      ///    ������-�������� - ������-������ ������� �������� ��������, ��������
      /// </summary>
      [Description("������-�������� - ������-������ ������� �������� ��������")]
      PFS_PROXY = 7, //

      /// <summary>
      ///    ���������� ������������� ������ (���� �����, ��������� DIRECT. ��������� Linklib
      /// </summary>
      [Description("���������� ������������� ������")]
      TERMLITE = 8,

      /// <summary>
      ///    ������������ (������������), ��������� TM
      /// </summary>
      [Description("������������ (������������)")]
      NETRT = 9, //

      /// <summary>
      ///    ������-����, ��������� TM
      /// </summary>
      [Description("������-����")]
      GT = 10, //

      /// <summary>
      ///    ��� ������� - ������ �� ������� � ������������ �������, TM
      /// </summary>
      [Description("��� �������")]
      NETSCAN = 11, //

      /// <summary>
      ///    ���������� �� AT-Mega, ��������� DIRECT, ��������� Linklib
      /// </summary>
      [Description("���������� �� AT-Mega")]
      ASH = 12,

      /// <summary>
      ///    MainControl-���.����������� ��� ������, �������� TM
      /// </summary>
      [Description("MainControl-���.����������� ��� ������")]
      MCS = 14,

      /// <summary>
      ///    ������������ Autonics ��������� RS232, �������� TM
      /// </summary>
      [Description("������������ Autonics")]
      AUTONICS = 15, //

      /// <summary>
      ///    ������ (�.�����) �������� Linklib
      /// </summary>
      [Description("������ (�.�����)")]
      CAMSRV = 16,

      /// <summary>
      ///    �������� ������� - ������+������� ��������� CUSTOM, �������� TM
      /// </summary>
      [Description("�������� ������� - ������+�������")]
      ECS = 17
   }

   /// <summary>
   ///    ������� �� ���������� CMD � ������ ��������� ������-&gt;������
   /// </summary>
   public enum EPlanCommand
   {
      /// <summary>
      ///    ������ �� ������ �������
      /// </summary>
      [Description("������ �� ������ �������")]
      GETSTATE = 1,

      /// <summary>
      ///    ������ �� ������� �����
      /// </summary>
      [Description("������ �� ������� �����")]
      CLEARPLAN = 2,

      /// <summary>
      ///    ������ �� ����� �����
      /// </summary>
      [Description("������ �� ����� �����")]
      STARTPLAN = 3,

      /// <summary>
      ///    ������ �� �����
      /// </summary>
      [Description("������ �� �����")]
      PAUSEPLAN = 4, //

      /// <summary>
      ///    ������ �� �������
      /// </summary>
      [Description("������ �� �������")]
      STOPPLAN = 5
   }

   /// <summary>
   ///    Class TMPacketSignature.
   /// </summary>
   public static class TMPacketSignature
   {
      #region Enums

      /// <summary>
      ///    ������, ���������� RS485_GetErrorMessage
      /// </summary>
      public enum EComPortErr
      {
         /// <summary>
         ///    ��� ������
         /// </summary>
         [Description("��� ������")]
         NOT = 0,

         /// <summary>
         ///    ������ ����������� �����
         /// </summary>
         [Description("������ ����������� �����")]
         CS,

         /// <summary>
         ///    ������ �������� � EEPROM
         /// </summary>
         [Description("������ �������� � EEPROM")]
         EEPROM,

         /// <summary>
         ///    ������ �������� �������� ���� ������
         /// </summary>
         [Description("������ �������� �������� ���� ������")]
         CNT,

         /// <summary>
         ///    ��� ������� �� ��������������
         /// </summary>
         [Description("��� ������� �� ��������������")]
         CMD,

         /// <summary>
         ///    ������ ������ ������
         /// </summary>
         [Description("������ ������ ������")]
         CID,

         /// <summary>
         ///    ������ ������ ���������
         /// </summary>
         [Description("������ ������ ���������")]
         PID,

         /// <summary>
         ///    ������ ���������� ����������
         /// </summary>
         [Description("������ ���������� ����������")]
         PRM,

         /// <summary>
         ///    ������ �������� ���������
         /// </summary>
         [Description("������ �������� ���������")]
         VAL,

         /// <summary>
         ///    ������ ������� ��������
         /// </summary>
         [Description("������ ������� ��������")]
         TO,

         /// <summary>
         ///    ���������� ������
         /// </summary>
         [Description("���������� ������")]
         BUSY,

         /// <summary>
         ///    �� ������� ������ �������������� � ����������� ���������� <br/>
         ///    ��� ��� ������� �� ����� ���� ������
         /// </summary>
         [Description("�� ������� ������ �������������� � ����������� ���������� ��� ��� ������� �� ����� ���� ������")]
         ANQ,

         /// <summary>
         ///    ������� ����� �������� � ������ ����� USART0
         /// </summary>
         [Description("������� ����� �������� � ������ ����� USART0")]
         TO_RO,

         /// <summary>
         ///    UART Parity Error USART0
         /// </summary>
         [Description("UART Parity Error USART0")]
         UPE0,

         /// <summary>
         ///    Data Overrun USART0
         /// </summary>
         [Description("Data Overrun USART0")]
         DOR0,

         /// <summary>
         ///    DOR0_UPE0
         /// </summary>
         [Description("DOR0_UPE0")]
         DOR0_UPE0,

         /// <summary>
         ///    Frame Error USART0
         /// </summary>
         [Description("Frame Error USART0")]
         FE0,

         /// <summary>
         ///    FE0_UPE0
         /// </summary>
         [Description("FE0_UPE0")]
         FE0_UPE0,

         /// <summary>
         ///    FE0_DOR0
         /// </summary>
         [Description("FE0_DOR0")]
         FE0_DOR0,

         /// <summary>
         ///    FE0_DOR0_UPE0
         /// </summary>
         [Description("FE0_DOR0_UPE0")]
         FE0_DOR0_UPE0,

         /// <summary>
         ///    ������� ����� �������� � ������ ����� USART1
         /// </summary>
         [Description("������� ����� �������� � ������ ����� USART1")]
         TO_R1,

         /// <summary>
         ///    UART Parity Error USART1
         /// </summary>
         [Description("UART Parity Error USART1")]
         UPE1,

         /// <summary>
         ///    Data Overrun USART1
         /// </summary>
         [Description("Data Overrun USART1")]
         DOR1,

         /// <summary>
         ///    DOR1_UPE1
         /// </summary>
         [Description("DOR1_UPE1")]
         DOR1_UPE1,

         /// <summary>
         ///    Frame Error USART1
         /// </summary>
         [Description("Frame Error USART1")]
         FE1,

         /// <summary>
         ///    FE1_UPE1
         /// </summary>
         [Description("FE1_UPE1")]
         FE1_UPE1,

         /// <summary>
         ///    FE1_DOR1
         /// </summary>
         [Description("FE1_DOR1_UPE1")]
         FE1_DOR1,

         /// <summary>
         ///    FE1_DOR1_UPE1
         /// </summary>
         [Description("FE1_DOR1_UPE1")]
         FE1_DOR1_UPE1,

         /// <summary>
         ///    ������� ����� �������� � ������ ����� USART2
         /// </summary>
         [Description("������� ����� �������� � ������ ����� USART2")]
         TO_R2,

         /// <summary>
         ///    UART Parity Error USART2
         /// </summary>
         [Description("UART Parity Error USART2")]
         UPE2,

         /// <summary>
         ///    Data Overrun USART2
         /// </summary>
         [Description("Data Overrun USART2")]
         DOR2,

         /// <summary>
         ///    DOR2_UPE2
         /// </summary>
         [Description("")]
         DOR2_UPE2,

         /// <summary>
         ///    Frame Error USART2
         /// </summary>
         [Description("Frame Error USART2")]
         FE2,

         /// <summary>
         ///    ������� ����� �������� � ������ ����� USART3
         /// </summary>
         [Description("������� ����� �������� � ������ ����� USART3")]
         TO_R3,

         /// <summary>
         ///    UART Parity Error USART3
         /// </summary>
         [Description("UART Parity Error USART3")]
         UPE3,

         /// <summary>
         ///    DOR3
         /// </summary>
         [Description("DOR3")]
         DOR3,

         /// <summary>
         ///    UPE3
         /// </summary>
         [Description("UPE3")]
         DOR3_UPE3,

         /// <summary>
         ///    FE3
         /// </summary>
         [Description("FE3")]
         FE3,

         /// <summary>
         ///    UPE3
         /// </summary>
         [Description("UPE3")]
         FE3_UPE3,

         /// <summary>
         ///    DOR3
         /// </summary>
         [Description("DOR3")]
         FE3_DOR3,

         /// <summary>
         ///    FE3_DOR3_UPE3
         /// </summary>
         FE3_DOR3_UPE3,

         /// <summary>
         ///    ������������ ������ ����� USART0
         /// </summary>
         [Description("������������ ������ ����� USART0")]
         OVF_R0,

         /// <summary>
         ///    ������������ ������ ����� USART1
         /// </summary>
         [Description("������������ ������ ����� USART1")]
         OVF_R1,

         /// <summary>
         ///    ������������ ������ ����� USART2"
         /// </summary>
         [Description("������������ ������ ����� USART2")]
         OVF_R2,

         /// <summary>
         ///    ������������ ������ ����� USART3
         /// </summary>
         [Description("������������ ������ ����� USART3")]
         OVF_R3,

         /// <summary>
         ///    ������������ ������ ����� USART0
         /// </summary>
         [Description("������������ ������ ����� USART0")]
         OVF_T0,

         /// <summary>
         ///    ������������ ������ ����� USART1
         /// </summary>
         [Description("������������ ������ ����� USART1")]
         OVF_T1,

         /// <summary>
         ///    ������������ ������ ����� USART2
         /// </summary>
         [Description("������������ ������ ����� USART2")]
         OVF_T2,

         /// <summary>
         ///    ������������ ������ ����� USART3
         /// </summary>
         [Description("������������ ������ ����� USART3")]
         OVF_T3,

         /// <summary>
         ///    The error 1
         /// </summary>
         ERR_1,

         /// <summary>
         ///    The error 2
         /// </summary>
         ERR_2,

         /// <summary>
         ///    The count
         /// </summary>
         COUNT
      }

      /// <summary>
      ///    Enum EControlType
      /// </summary>
      public enum EControlType
      {
         /// <summary>
         ///    "������������� ���
         /// </summary>
         [Description("������������� ���")]
         UNKNOWN = 0,

         /// <summary>
         ///    ����� �������� (�������������)
         /// </summary>
         [Description("����� �������� (�������������)")]
         TEMPSET = 1,

         /// <summary>
         ///    ���� ������ �� ������
         /// </summary>
         [Description("���� ������ �� ������")]
         TEMPSINGLE = 2,

         /// <summary>
         ///    �������� ����������/���������
         /// </summary>
         [Description("�������� ����������/���������")]
         TEMPSTATUS = 3,

         /// <summary>
         ///    �������� \"�����\"
         /// </summary>
         [Description("�������� \"�����\"")]
         DOOR = 4,

         /// <summary>
         ///    �������� � ��������������� ������  ��� � ���������� ���������
         /// </summary>
         [Description("�������� � ��������������� ������ ��� � ���������� ���������")]
         ATTENTION = 5,

         /// <summary>
         ///    �������� � �������� �������� - �� ������������ ����
         /// </summary>
         [Description("�������� � �������� �������� - �� ������������ ����")]
         IRRADIATION = 6,

         /// <summary>
         ///    ������ ����
         /// </summary>
         [Description("������ ����")]
         WATERFLOW = 7,

         /// <summary>
         ///    �������� ���������
         /// </summary>
         [Description("�������� ���������")]
         XRAY = 8,

         /// <summary>
         ///    ������� ����� ������� XRay
         /// </summary>
         [Description("������� ����� ������� XRay")]
         XRAYMODE = 9,

         /// <summary>
         ///   ��������� �������
         /// </summary>
         [Description("��������� �������")]
         TEXTLABEL = 10,

         /// <summary>
         ///    ���� �������� ������, �������
         /// </summary>
         [Description("���� �������� ������, �������")]
         ANGLE = 11,

         /// <summary>
         ///    ���� �������� ������, �����������
         /// </summary>
         [Description("���� �������� ������, �����������")]
         ANGLE_PIC = 12,

         /// <summary>
         ///    ������ �������
         /// </summary>
         [Description("������ �������")]
         ALTITUDE = 13,

         /// <summary>
         ///    ������ � �������� (������� ������ ��������)
         /// </summary>
         [Description("������ � ��������")]
         MOVING = 14,

         /// <summary>
         ///    ��������� ���������
         /// </summary>
         [Description("��������� ���������")]
         DETECTOR = 15,

         /// <summary>
         ///    ������� - ��� � ������
         /// </summary>
         [Description("������� - ��� � ������")]
         XR_IHV = 16,

         /// <summary>
         ///    ������� - ���������� � ������
         /// </summary>
         [Description("������� - ���������� � ������")]
         XR_UHV = 17,

         /// <summary>
         ///   ������� - ��� ������
         /// </summary>
         [Description("������� - ��� ������")]
         XR_IVAC = 18,

         /// <summary>
         ///    ������� - �����
         /// </summary>
         [Description("������� - �����")]
         XR_HEAT = 19,

         /// <summary>
         ///    ������� - �������
         /// </summary>
         [Description("������� - �������")]
         XR_HV = 20,

         /// <summary>
         ///    ������ - ������ ����������
         /// </summary>
         [Description("������ - ������ ����������")]
         PFS_HAND = 21,

         /// <summary>
         ///    ����� ���
         /// </summary>
         [Description("����� ���")]
         DAC = 22,

         /// <summary>
         ///    ����� ���
         /// </summary>
         [Description("����� ���")]
         ADC = 23,

         /// <summary>
         ///    ����� ������� ����-������� (�������� ��� ���� �����)
         /// </summary>
         [Description("����� ������� ����-������� (�������� ��� ���� �����)")]
         UPDATE_NUM = 24,

         /// <summary>
         ///    �������� - ��������� ��������������� ��������� L<->R
         /// </summary>
         [Description("�������� - ��������� ��������������� ��������� L<->R")]
         PFS_DETECTORLR = 25,

         /// <summary>
         ///    ���� �������� - �������� ��������
         /// </summary>
         [Description("���� �������� - �������� ��������")]
         ANGLE_IENC = 26,

         /// <summary>
         ///    �������� ������� �������� - ������ �������
         /// </summary>
         [Description("�������� ������� �������� - ������ �������")]
         STATUS = 27,

         /// <summary>
         ///    ������ - �������������� �����������, ��
         /// </summary>
         [Description("������ - �������������� �����������, ��")]
         PFS_HORMOVE = 28,

         /// <summary>
         ///    ���� ��� ������ ���������. �� ������ � ������� ������,
         /// ��������� � ������ ���������� App VisualControl
         /// </summary>
         [Description("���� ��� ������ ���������")]
         MESSAGE_LIST = 29,

         /// <summary>
         ///    �������� ������������� �������� - ������ ����������
         /// </summary>
         [Description("�������� ������������� �������� - ������ ����������")]
         PFS_STATUS = 30,

         /// <summary>
         ///    �������� ������������� �������� - ������ ����������
         /// </summary>
         [Description("�������� ������������� �������� - ������ ����������")]
         FARADAY_STATUS = 31,

         /// <summary>
         ///    ��������� �������� ������� - ������� ������, ������ ���������� �������� ���������
         /// </summary>
         [Description("��������� �������� �������")]
         FARADAY_POS = 32,

         /// <summary>
         ///    �������� ������������� �������� - ��������� ����� ���������� ��������� �������
         /// </summary>
         [Description("��������� ����� ���������� ��������� �������")]
         FARADAY_LOCAL = 33,

         /// <summary>
         ///    ������ - ������� ����� ������� - ��������� �������
         /// </summary>
         [Description("������  - ������� ����� �������")]
         PFS_MODE = 34,

         /// <summary>
         ///    �������� - ���������, ������ ���������� �������� ��� ��� �����������
         /// </summary>
         [Description("�������� - ���������")]
         HOST = 35,

         /// <summary>
         ///    ������� ����� ������� XRay - ����� ������ ��������
         /// </summary>
         [Description("������� ����� ������� XRay")]
         XR_SHOTMODE = 36,

         /// <summary>
         ///    ������� ����� ������� ECS - ECS_MODE_READY
         /// </summary>
         [Description("������� ����� ������� ECS - ECS_MODE_READY")]
         MODE_TXT = 37
      }

      //��������� ���� ����������� ��� ������������� � ���������������� xml ������ ����
      //��� ���������� ������� ParseConnectType, PrintConnectType


      /// <summary>
      ///    ���� ���������-�����������������	��� ��������� DevDescr
      /// </summary>
      public enum EDeviceType
      {
         /// <summary>
         ///    ������������� ���
         /// </summary>
         [Description("������������� ���")]
         UNKNOWN = 0,

         /// <summary>
         ///   ���� ����, ��������
         /// </summary>
         [Description("���� ����, ��������")]
         MBPS = 1,

         /// <summary>
         ///    ���� ����������� ��� ������ ������ �������������� ������� (�� 2016 ���� � �������)
         /// </summary>
         [Description("���� ����������� ��� ������ ������ �������������� ������� (�� 2016 ���� � �������)")]
         XRAYC = 2,

         /// <summary>
         ///    ������, �������, �����. �����
         /// </summary>
         [Description("������, �������, �����. �����")]
         DIMS = 3,

         /// <summary>
         ///    �����, �����-���������� 8 ��������
         /// </summary>
         [Description("�����, �����-���������� 8 ��������")]
         TERMO_S = 4,

         /// <summary>
         ///    �����, ������-���������� 8 �������
         /// </summary>
         [Description("�����, ������-���������� 8 �������")]
         TERMO_M = 5,

         /// <summary>
         ///   ���������� ������
         /// </summary>
         [Description("���������� ������")]
         PFS = 6,

         /// <summary>
         ///    ���������� �������� �������
         /// </summary>
         [Description("���������� �������� �������")]
         FDCY = 7, //

         /// <summary>
         ///    ������������� ���������� �� ������ ��� SERVER_TYPE_ASH
         /// </summary>
         [Description("������������� ���������� �� ������ ��� SERVER_TYPE_ASH")]
         ASH_ST = 8,

         /// <summary>
         ///    ���������� ������������ AUTONICS
         /// </summary>
         [Description("���������� ������������ AUTONICS")]
         ANCSXY = 9,

         /// <summary>
         ///    ����������� MV
         /// </summary>
         [Description("����������� MV")]
         MVCAM = 10
      }

      /// <summary>
      ///    Enum EServerConnectSignature
      /// </summary>
      public enum EServerConnectSignature
      {
         /// <summary>
         ///    �������
         /// </summary>
         [Description("LL")]
         LNKLIB,

         /// <summary>
         ///    �������� TM_Protocol
         /// </summary>
         [Description("TM")]
         TMPROTOCOL,

         /// <summary>
         ///    ������ ����������� � COM ����� c �������� ��� 485
         /// </summary>
         [Description("COM")]
         DIRECT,

         /// <summary>
         ///    �������� �����������
         /// </summary>
         [Description("IM")]
         IMITATION,

         /// <summary>
         ///    ������� TCP
         /// </summary>
         [Description("TCP")]
         TCPCUSTOM,

         /// <summary>
         ///    ������ ����������� � COM �����
         /// </summary>
         [Description("RS232")]
         RS232
      }

      /// <summary>
      /// ������� ����������� ������� � ��������� ������ <br/>
      /// ������������ � ParseTagAsString, ParseConnectType, _DataServer_Info2Tree, <br/>
      /// ConnectToServer, DisconnectFromServer, IsConnected <br/>
      /// </summary>
      public enum EServerConnectType
      {
         /// <summary>
         ///    ��������
         /// </summary>
         [Description("��������")]
         NONE = 0, //

         /// <summary>
         ///    ���������� linklib.h by P.Lunev (������������ �� ������, ������������� �������, �������-������(������)...)
         /// </summary>
         [Description("���������� linklib.h by P.Lunev (������������ �� ������, ������������� �������, �������-������(������)...)")]
         LNKLIB = 1,

         /// <summary>
         ///    �������� TM_Protocol ( ���������� TCP - ������������� ������-������, �������-������-��������, ������ ������ )
         /// </summary>
         [Description("�������� TM_Protocol ( ���������� TCP - ������������� ������-������, �������-������-��������, ������ ������ )")]
         TMPROTOCOL = 2,

         /// <summary>
         ///    ������ ����������� � COM ����� <br/>
         /// (����������� ��� ������, ��� �������������� �������, ��� ������-�������)<br/>
         /// c �������� ��� RS485 \"+\"/\":\"")]
         /// </summary>
         [Description("������ ����������� � COM ����� (����������� ��� ������, ��� �������������� �������, ��� ������-�������) c �������� ��� RS485 \"+\"/\":\"")]
         DIRECT = 4,

         /// <summary>
         ///    �������� ����������� � ���������� - ������ ��������. ��� ��������-����������
         /// </summary>
         [Description("�������� ����������� � ���������� - ������ ��������. ��� ��������-����������")]
         IMITATION = 8,

         /// <summary>
         ///    ������� TCP - ����������� � ����������. ������ ��������� ����������� �� ����� �������������
         /// </summary>
         [Description("������� TCP - ����������� � ����������. ������ ��������� ����������� �� ����� �������������")]
         TCPCUSTOM = 16,

         /// <summary>
         ///    ������ ����������� � ���-����� (COM ���� ����������� Autonics)
         /// </summary>
         [Description("������ ����������� � ���-����� (COM ���� ����������� Autonics)")]
         RS232 = 32
      }

      #endregion

      #region Public properties

      /// <summary>
      ///    Gets the acc.
      /// </summary>
      public static byte[] _ACC
      {
         get
         {
            var str = "_ACC";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///    �-�� �� AT-Mega. �� �������� ��������, ������ ��� ������������� � DataServers_Info2Tree � ��������
      /// </summary>
      public static byte[] _ASH
      {
         get
         {
            var str = "_ASH";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///    ������. �� �������� ��������, ������ ��� ������������� � DataServers_Info2Tree.
      /// </summary>
      public static byte[] _PFS
      {
         get
         {
            var str = "_PFS";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///    ���������� ������������� ������ (���� �����).<br/>
      ///    �� �������� ��������, ������ ��� ������������� � DataServers_Info2Tree � ��������
      /// </summary>
      public static byte[] _TML
      {
         get
         {
            var str = "_TML";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   Gets the adda.
      /// </summary>
      public static byte[] ADDA
      {
         get
         {
            var str = "ADDA";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   ������������ Autonics
      /// </summary>
      public static byte[] ANCS
      {
         get
         {
            var str = "ANCS";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   ������ ����������. �� �������� ��������, ������ ��� ������������� � ��������
      /// </summary>
      /// <value>The CAMS.</value>
      public static byte[] CAMS
      {
         get
         {
            var str = "CAMS";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   �������� ������� (���������� ����������)	TM
      /// </summary>
      public static byte[] ECSv
      {
         get
         {
            var str = "ECSv";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   MainControl Interface Software - ���������� � ��������� ���.����������� ��� ������ TM
      /// </summary>
      public static byte[] MCSv
      {
         get
         {
            byte[] str = {(byte) 'M', (byte) 'C', (byte) 'S', (byte) 'v'};
            //return str;
            return Encoding.ASCII.GetBytes("MCSv");
         }
      }

      /// <summary>
      ///   MainControl Software - ��������� ���.����������� ��� ������ (���������� ����������, �������, �� TM)
      /// </summary>
      public static byte[] MCTL
      {
         get
         {
            var str = "MCTL";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   ������ ��������� ��� ������
      /// </summary>
      public static byte[] TM_C
      {
         get
         {
            var str = "Tm_C";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///    ������-�������� - ������-������
      /// </summary>
      public static byte[] TMCP
      {
         get
         {
            var str = "TmCP";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   ������-����
      /// </summary>
      public static byte[] TMGT
      {
         get
         {
            var str = "TmGT";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   ��� ������� - ������ �� ������� � ������������ �������	TM
      /// </summary>
      public static byte[] TMNS
      {
         get
         {
            var str = "TmNS";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///    ������ �������������.
      /// </summary>
      public static byte[] TMPR
      {
         get
         {
            var str = "Ther";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   ������������ (������������)
      /// </summary>
      public static byte[] TMRT
      {
         get
         {
            var str = "TmRT";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   Gets the unknown.
      /// </summary>
      public static byte[] UNKNOWN
      {
         get
         {
            var str = "Unkn";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      /// <summary>
      ///   ������ ��������
      /// </summary>
      public static byte[] XRay
      {
         get
         {
            var str = "XRay";
            return Encoding.ASCII.GetBytes(str);
         }
      }

      #endregion

      #region Public methods

      /// <summary>
      ///   Servers the type.
      /// </summary>
      /// <param name="mark">The mark.</param>
      /// <returns>EServerType.</returns>
      public static EServerType ServerType(this byte[] mark)
      {
         var str = Encoding.ASCII.GetString(mark);
         return str.ServerType();
      }

      /// <summary>
      ///   Servers the type.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <returns>EServerType.</returns>
      public static EServerType ServerType(this string str)
      {
         if (Encoding.ASCII.GetString(TMPR) == str) {
            return EServerType.TEMPERATURE;
         }

         if (Encoding.ASCII.GetString(XRay) == str) {
            return EServerType.XRAY;
         }

         if (Encoding.ASCII.GetString(_PFS) == str) {
            return EServerType.PFS;
         }

         if (Encoding.ASCII.GetString(TM_C) == str) {
            return EServerType.TMC;
         }

         if (Encoding.ASCII.GetString(ADDA) == str) {
            return EServerType.DACADC;
         }

         if (Encoding.ASCII.GetString(_ACC) == str) {
            return EServerType.ACC;
         }

         if (Encoding.ASCII.GetString(TMCP) == str) {
            return EServerType.PFS_PROXY;
         }

         if (Encoding.ASCII.GetString(_TML) == str) {
            return EServerType.TERMLITE;
         }

         if (Encoding.ASCII.GetString(TMRT) == str) {
            return EServerType.NETRT;
         }

         if (Encoding.ASCII.GetString(TMGT) == str) {
            return EServerType.GT;
         }

         if (Encoding.ASCII.GetString(TMNS) == str) {
            return EServerType.NETSCAN;
         }

         if (Encoding.ASCII.GetString(_ASH) == str) {
            return EServerType.ASH;
         }

         if (Encoding.ASCII.GetString(MCSv) == str) {
            return EServerType.MCS;
         }

         if (Encoding.ASCII.GetString(ANCS) == str) {
            return EServerType.AUTONICS;
         }

         if (Encoding.ASCII.GetString(CAMS) == str) {
            return EServerType.CAMSRV;
         }

         if (Encoding.ASCII.GetString(ECSv) == str) {
            return EServerType.ECS;
         }

         return EServerType.UNKNOWN;
      }

      /// <summary>
      ///   Signatures the specified type.
      /// </summary>
      /// <param name="type">The type.</param>
      /// <returns>System.Byte[].</returns>
      public static byte[] Signature(this EServerType type)
      {
         switch (type) {
            case EServerType.TEMPERATURE:
               return TMPR; //������ �������������
            case EServerType.XRAY:
               return XRay; //������ ��������
            case EServerType.PFS:
               return _PFS; //������. �� �������� ��������, ������ ��� ������������� � DataServers_Info2Tree
            case EServerType.TMC:
               return TM_C; //������ ��������� ��� ������
            case EServerType.DACADC:
               return ADDA;
            case EServerType.ACC:
               return _ACC;
            case EServerType.PFS_PROXY:
               return TMCP; //������-�������� - ������-������
            case EServerType.TERMLITE:
               return _TML; //���������� ������������� ������ (���� �����). �� �������� ��������, ������ ��� ������������� � DataServers_Info2Tree
            case EServerType.NETRT:
               return TMRT; //������������ (������������)
            case EServerType.GT:
               return TMGT; //������-����
            case EServerType.NETSCAN:
               return TMCP; //��� ������� - ������ �� ������� � ������������ �������			TM
            case EServerType.ASH:
               return _ASH; //�-�� �� AT-Mega. �� �������� ��������, ������ ��� ������������� � DataServers_Info2Tree
            case EServerType.MCS:
               return MCSv; //MainControl Software - ��������� ���.����������� ��� ������
            case EServerType.AUTONICS:
               return ANCS; //������������ Autonics
            case EServerType.CAMSRV:
               return CAMS; //������ ����������. �� �������� ��������, ������ ��� ������������� � ��������
            case EServerType.ECS:
               return ECSv; //�������� ������� (���������� ����������)	TM
            default:
               return TMNS;
         }

         return UNKNOWN;
      }

      #endregion
   }
}
