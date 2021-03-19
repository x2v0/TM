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
   ///    теги пакета конфигурации устройства
   /// </summary>
   public enum EConfigTag
   {
      /// <summary>
      ///    версия пакета, строка PACKET_VERTION_STR
      /// </summary>
      [Description("версия пакета")]
      VER = 1,

      /// <summary>
      ///   коннект, ParceCfgPacket_GetLinkData
      /// </summary>
      [Description("коннект")]
      CONNECT = 2,

      /// <summary>
      ///    подключенное железо: 
      /// </summary>
      [Description("подключенное железо")]
      DEVICE = 3,

      /// <summary>
      ///    линк ретранслятора (для NetRT -> TmNetScan)
      /// </summary>
      [Description("линк ретранслятора")]
      RTLINK = 4,

      /// <summary>
      ///    текстовая информация в свободном виде
      /// </summary>
      [Description("текстовая информация в свободном виде")]
      TXTINFO = 5,

      /// <summary>
      ///    байт конфигурации устройства, зависит от устройства
      /// </summary>
      [Description("байт конфигурации устройства")]
      DEVCONFIG = 6
   }

   /// <summary>
   ///    статус аппаратной ошибки, сгенеренной устройством
   /// </summary>
   public enum EDevErrorStatus
   {
      /// <summary>
      ///    несерьезная ошибка
      /// </summary>
      [Description("несерьезная ошибка")]
      INFORMATION = 0,

      /// <summary>
      ///    предупреждение
      /// </summary>
      [Description("предупреждение")]
      WARNING = 1,

      /// <summary>
      ///    aппаратная проблема
      /// </summary>
      [Description("aппаратная проблема")]
      PROBLEM = 2
   }

   /// <summary>
   ///    состояние устройства
   /// </summary>
   public enum EDeviceStatus
   {
      /// <summary>
      ///    выключено или неотконфигурировано
      /// </summary>
      [Description("выключено или неотконфигурировано")]
      OFF = 0, //

      /// <summary>
      ///    работает
      /// </summary>
      [Description("работает")]
      OK = 1, //

      /// <summary>
      ///    предупреждение
      /// </summary>
      [Description("предупреждение")]
      ATTENTION = 2, //

      /// <summary>
      ///    активно
      /// </summary>
      [Description("активно")]
      HOT = 3,

      /// <summary>
      ///    для двери
      /// </summary>
      [Description("для двери")]
      OPEN = HOT,

      /// <summary>
      ///    неисправно
      /// </summary>
      [Description("неисправно")]
      CRACKED = 4,

      /// <summary>
      ///    в движении - перемещение кресла, детектора и т.д.
      /// </summary>
      [Description("в движении")]
      MOVING = 5, //

      /// <summary>
      ///    в работе, но не блокирующей устройство, например для цилиндра фарадея
      /// </summary>
      [Description("в работе")]
      IN_WORK = 6 //
   }

   /// <summary>
   ///    битовое поле TCPSrvServer::TMSettings или
   ///    TCPSrvServer::TMSettings, настройки протокола взаимодействия
   /// </summary>
   public enum EPacketSettings
   {
      /// <summary>
      ///    никаких доп. настроек
      /// </summary>
      [Description("никаких доп. настроек")]
      NULL = 0,

      /// <summary>
      ///    добавлять контрольную сумму в пакетак к клиенту
      /// </summary>
      [Description("добавлять контрольную сумму в пакетак к клиенту")]
      AddCheckSumm = 1,

      /// <summary>
      ///    ожидать и проверять контрольную сумму в пакетах от клиента
      /// </summary>
      [Description("ожидать и проверять контрольную сумму в пакетах от клиента")]
      ExpectCheckSumm = 2,

      /// <summary>
      ///    проверять входящий размер пакета (значение поля TMpacketSizeLimit)
      /// </summary>
      [Description("проверять входящий размер пакета")]
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
   ///    один "выстрел" для пересылки (направление+энергия+интенсивность)
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
   ///    Объединённая структура <see cref="TM.PlanSpot" /> + <see cref="TM.PlanSpotResult" /> 
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
         sb.AppendLine(", done = " + done + ", result_xangle = " + 
                       result_xangle + ", result_zangle = " + result_zangle + 
                       ", result_pcount = " + result_pcount);

         return sb.ToString();
      }
   } //44bytes

   /// <summary>
   ///    один "выстрел" для пересылки (направление+энергия+интенсивность)
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct PlanSpotTopass
   {
      /// <summary>
      ///    The length
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(PlanSpotTopass));

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
   ///   Структура с результатами выполнения плана облучения
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
   } //20bytes

   /// <summary>
   ///    Enum EServerType
   /// </summary>
   public enum EServerType
   {
      /// <summary>
      ///    неизвестный тип. Только для пинг-запросов
      /// </summary>
      [Description("неизвестный тип")]
      UNKNOWN = 0,

      /// <summary>
      ///    температурный сервер активный TM, пассивный Linklib
      /// </summary>
      [Description("температурный сервер")]
      TEMPERATURE = 1,

      /// <summary>
      ///    рентгеновский сервер активный TM, пассивный Linklib
      /// </summary>
      [Description("рентгеновский сервер")]
      XRAY = 2,

      /// <summary>
      ///    сервер системы фиксации пациента - TM протокол не использует пассивный
      /// </summary>
      [Description("сервер системы фиксации пациента")]
      PFS = 3,

      /// <summary>
      ///    сервер - программы управления томографом TMC, активный TM
      /// </summary>
      [Description("сервер - программы управления томографом TMC")]
      TMC = 4,

      /// <summary>
      ///    сервер ЦАП/АЦП- TM протокол не использует, пассивный
      /// </summary>
      [Description("сервер ЦАП/АЦП")]
      DACADC = 5, //

      /// <summary>
      ///    ускоряющий сервер - TM протокол не использует, пассивный
      /// </summary>
      [Description("ускоряющий сервер")]
      ACC = 6, //

      /// <summary>
      ///    кресло-контроль - прокси-сервер системы фиксации пациента, активный
      /// </summary>
      [Description("кресло-контроль - прокси-сервер системы фиксации пациента")]
      PFS_PROXY = 7, //

      /// <summary>
      ///    упрощенный температурный сервер (один слэйв, пассивный DIRECT. пассивный Linklib
      /// </summary>
      [Description("упрощенный температурный сервер")]
      TERMLITE = 8,

      /// <summary>
      ///    ретранслятор (конфигурация), пассивный TM
      /// </summary>
      [Description("ретранслятор (конфигурация)")]
      NETRT = 9, //

      /// <summary>
      ///    глобал-тест, пассивный TM
      /// </summary>
      [Description("глобал-тест")]
      GT = 10, //

      /// <summary>
      ///    для сканера - только от клиента к неизвестному серверу, TM
      /// </summary>
      [Description("для сканера")]
      NETSCAN = 11, //

      /// <summary>
      ///    устройство на AT-Mega, пассивный DIRECT, пассивный Linklib
      /// </summary>
      [Description("устройство на AT-Mega")]
      ASH = 12,

      /// <summary>
      ///    MainControl-упр.ускорителем как сервер, активный TM
      /// </summary>
      [Description("MainControl-упр.ускорителем как сервер")]
      MCS = 14,

      /// <summary>
      ///    перемещатель Autonics пассивный RS232, активный TM
      /// </summary>
      [Description("перемещатель Autonics")]
      AUTONICS = 15, //

      /// <summary>
      ///    камеры (П.Лунев) активный Linklib
      /// </summary>
      [Description("камеры (П.Лунев)")]
      CAMSRV = 16,

      /// <summary>
      ///    контроль выпуска - камера+фарадей пассивный CUSTOM, активный TM
      /// </summary>
      [Description("контроль выпуска - камера+фарадей")]
      ECS = 17
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

   /// <summary>
   ///    Class TMPacketSignature.
   /// </summary>
   public static class TMPacketSignature
   {
      #region Enums

      /// <summary>
      ///    ошибки, выбираемые RS485_GetErrorMessage
      /// </summary>
      public enum EComPortErr
      {
         /// <summary>
         ///    Нет ошибок
         /// </summary>
         [Description("Нет ошибок")]
         NOT = 0,

         /// <summary>
         ///    Ошибка контрольной суммы
         /// </summary>
         [Description("Ошибка контрольной суммы")]
         CS,

         /// <summary>
         ///    Ошибка операции с EEPROM
         /// </summary>
         [Description("Ошибка операции с EEPROM")]
         EEPROM,

         /// <summary>
         ///    Ошибка значения счётчика байт записи
         /// </summary>
         [Description("Ошибка значения счётчика байт записи")]
         CNT,

         /// <summary>
         ///    Код команды не поддерживается
         /// </summary>
         [Description("Код команды не поддерживается")]
         CMD,

         /// <summary>
         ///    Ошибка номера канала
         /// </summary>
         [Description("Ошибка номера канала")]
         CID,

         /// <summary>
         ///    Ошибка номера параметра
         /// </summary>
         [Description("Ошибка номера параметра")]
         PID,

         /// <summary>
         ///    Ошибка количества параметров
         /// </summary>
         [Description("Ошибка количества параметров")]
         PRM,

         /// <summary>
         ///    Ошибка значения параметра
         /// </summary>
         [Description("Ошибка значения параметра")]
         VAL,

         /// <summary>
         ///    Ошибка времени ожидания
         /// </summary>
         [Description("Ошибка времени ожидания")]
         TO,

         /// <summary>
         ///    Устройство занято
         /// </summary>
         [Description("Устройство занято")]
         BUSY,

         /// <summary>
         ///    Не совпали адреса запрашиваемого и ответившего устройства <br/>
         ///    или код запроса не равен коду ответа
         /// </summary>
         [Description("Не совпали адреса запрашиваемого и ответившего устройства или код запроса не равен коду ответа")]
         ANQ,

         /// <summary>
         ///    Истекло время ожидания в канале приёма USART0
         /// </summary>
         [Description("Истекло время ожидания в канале приёма USART0")]
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
         ///    Истекло время ожидания в канале приёма USART1
         /// </summary>
         [Description("Истекло время ожидания в канале приёма USART1")]
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
         ///    Истекло время ожидания в канале приёма USART2
         /// </summary>
         [Description("Истекло время ожидания в канале приёма USART2")]
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
         ///    Истекло время ожидания в канале приёма USART3
         /// </summary>
         [Description("Истекло время ожидания в канале приёма USART3")]
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
         ///    Переполнение буфера приёма USART0
         /// </summary>
         [Description("Переполнение буфера приёма USART0")]
         OVF_R0,

         /// <summary>
         ///    Переполнение буфера приёма USART1
         /// </summary>
         [Description("Переполнение буфера приёма USART1")]
         OVF_R1,

         /// <summary>
         ///    Переполнение буфера приёма USART2"
         /// </summary>
         [Description("Переполнение буфера приёма USART2")]
         OVF_R2,

         /// <summary>
         ///    Переполнение буфера приёма USART3
         /// </summary>
         [Description("Переполнение буфера приёма USART3")]
         OVF_R3,

         /// <summary>
         ///    Переполнение буфера приёма USART0
         /// </summary>
         [Description("Переполнение буфера приёма USART0")]
         OVF_T0,

         /// <summary>
         ///    Переполнение буфера приёма USART1
         /// </summary>
         [Description("Переполнение буфера приёма USART1")]
         OVF_T1,

         /// <summary>
         ///    Переполнение буфера приёма USART2
         /// </summary>
         [Description("Переполнение буфера приёма USART2")]
         OVF_T2,

         /// <summary>
         ///    Переполнение буфера приёма USART3
         /// </summary>
         [Description("Переполнение буфера приёма USART3")]
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
         ///    "Неопределённый тип
         /// </summary>
         [Description("Неопределённый тип")]
         UNKNOWN = 0,

         /// <summary>
         ///    набор датчиков (температурных)
         /// </summary>
         [Description("набор датчиков (температурных)")]
         TEMPSET = 1,

         /// <summary>
         ///    один датчик из набора
         /// </summary>
         [Description("один датчик из набора")]
         TEMPSINGLE = 2,

         /// <summary>
         ///    картинка блокировок/состояний
         /// </summary>
         [Description("картинка блокировок/состояний")]
         TEMPSTATUS = 3,

         /// <summary>
         ///    картинка \"дверь\"
         /// </summary>
         [Description("картинка \"дверь\"")]
         DOOR = 4,

         /// <summary>
         ///    картинка с восклицательным знаком  как и блокировка состояний
         /// </summary>
         [Description("картинка с восклицательным знаком как и блокировка состояний")]
         ATTENTION = 5,

         /// <summary>
         ///    картинка с символом радиации - не используется пока
         /// </summary>
         [Description("картинка с символом радиации - не используется пока")]
         IRRADIATION = 6,

         /// <summary>
         ///    проток воды
         /// </summary>
         [Description("проток воды")]
         WATERFLOW = 7,

         /// <summary>
         ///    включено излучение
         /// </summary>
         [Description("включено излучение")]
         XRAY = 8,

         /// <summary>
         ///    текущий режим сервера XRay
         /// </summary>
         [Description("текущий режим сервера XRay")]
         XRAYMODE = 9,

         /// <summary>
         ///   текстовая подпись
         /// </summary>
         [Description("текстовая подпись")]
         TEXTLABEL = 10,

         /// <summary>
         ///    угол поворота кресла, градусы
         /// </summary>
         [Description("угол поворота кресла, градусы")]
         ANGLE = 11,

         /// <summary>
         ///    угол поворота кресла, изображение
         /// </summary>
         [Description("угол поворота кресла, изображение")]
         ANGLE_PIC = 12,

         /// <summary>
         ///    высота полъема
         /// </summary>
         [Description("высота полъема")]
         ALTITUDE = 13,

         /// <summary>
         ///    кресло в движении (круглая желтая лампочка)
         /// </summary>
         [Description("кресло в движении")]
         MOVING = 14,

         /// <summary>
         ///    положение детектора
         /// </summary>
         [Description("положение детектора")]
         DETECTOR = 15,

         /// <summary>
         ///    рентген - ток в трубке
         /// </summary>
         [Description("рентген - ток в трубке")]
         XR_IHV = 16,

         /// <summary>
         ///    рентген - напряжение в трубке
         /// </summary>
         [Description("рентген - напряжение в трубке")]
         XR_UHV = 17,

         /// <summary>
         ///   рентген - ток насоса
         /// </summary>
         [Description("рентген - ток насоса")]
         XR_IVAC = 18,

         /// <summary>
         ///    рентген - накал
         /// </summary>
         [Description("рентген - накал")]
         XR_HEAT = 19,

         /// <summary>
         ///    рентген - высокое
         /// </summary>
         [Description("рентген - высокое")]
         XR_HV = 20,

         /// <summary>
         ///    кресло - ручное управление
         /// </summary>
         [Description("кресло - ручное управление")]
         PFS_HAND = 21,

         /// <summary>
         ///    канал ЦАП
         /// </summary>
         [Description("канал ЦАП")]
         DAC = 22,

         /// <summary>
         ///    канал АЦП
         /// </summary>
         [Description("канал АЦП")]
         ADC = 23,

         /// <summary>
         ///    номер апдейта дата-сервера (доступно для всех типов)
         /// </summary>
         [Description("номер апдейта дата-сервера (доступно для всех типов)")]
         UPDATE_NUM = 24,

         /// <summary>
         ///    детектор - индикатор горизонтального положения L<->R
         /// </summary>
         [Description("детектор - индикатор горизонтального положения L<->R")]
         PFS_DETECTORLR = 25,

         /// <summary>
         ///    угол поворота - значение энкодера
         /// </summary>
         [Description("угол поворота - значение энкодера")]
         ANGLE_IENC = 26,

         /// <summary>
         ///    картинка круглая лампочка - статус сервера
         /// </summary>
         [Description("картинка круглая лампочка - статус сервера")]
         STATUS = 27,

         /// <summary>
         ///    кресло - горизонтальное перемещение, мм
         /// </summary>
         [Description("кресло - горизонтальное перемещение, мм")]
         PFS_HORMOVE = 28,

         /// <summary>
         ///    блок для вывода сообщений. Не входит в сервера данных,
         /// относится к общему интерфейсу App VisualControl
         /// </summary>
         [Description("блок для вывода сообщений")]
         MESSAGE_LIST = 29,

         /// <summary>
         ///    картинка прямоугольная лампочка - статус устройства
         /// </summary>
         [Description("картинка прямоугольная лампочка - статус устройства")]
         PFS_STATUS = 30,

         /// <summary>
         ///    картинка прямоугольная лампочка - статус устройства
         /// </summary>
         [Description("картинка прямоугольная лампочка - статус устройства")]
         FARADAY_STATUS = 31,

         /// <summary>
         ///    положение цилиндра фарадея - круглая кнопка, цветом отображает активное положение
         /// </summary>
         [Description("положение цилиндра фарадея")]
         FARADAY_POS = 32,

         /// <summary>
         ///    картинка прямоугольная лампочка - локальный режим управления цилиндром фарадея
         /// </summary>
         [Description("локальный режим управления цилиндром фарадея")]
         FARADAY_LOCAL = 33,

         /// <summary>
         ///    кресло - текущий режим терапии - текстовая подпись
         /// </summary>
         [Description("кресло  - текущий режим терапии")]
         PFS_MODE = 34,

         /// <summary>
         ///    картинка - компьютер, цветом отображает успушное или нет подключение
         /// </summary>
         [Description("картинка - компьютер")]
         HOST = 35,

         /// <summary>
         ///    текущий режим сервера XRay - номер набора настроек
         /// </summary>
         [Description("текущий режим сервера XRay")]
         XR_SHOTMODE = 36,

         /// <summary>
         ///    текущий режим сервера ECS - ECS_MODE_READY
         /// </summary>
         [Description("текущий режим сервера ECS - ECS_MODE_READY")]
         MODE_TXT = 37
      }

      //сигнатуры типа подключения для использования в конфигурационных xml вместо цифр
      //при пополнении править ParseConnectType, PrintConnectType


      /// <summary>
      ///    типы устройств-микроконтроллеров	для структуры DevDescr
      /// </summary>
      public enum EDeviceType
      {
         /// <summary>
         ///    Неопределённый тип
         /// </summary>
         [Description("Неопределённый тип")]
         UNKNOWN = 0,

         /// <summary>
         ///   блок линз, например
         /// </summary>
         [Description("блок линз, например")]
         MBPS = 1,

         /// <summary>
         ///    блок электроники для старой модели рентгеновского сервера (до 2016 года в терапии)
         /// </summary>
         [Description("блок электроники для старой модели рентгеновского сервера (до 2016 года в терапии)")]
         XRAYC = 2,

         /// <summary>
         ///    вакуум, высокое, сетка. накал
         /// </summary>
         [Description("вакуум, высокое, сетка. накал")]
         DIMS = 3,

         /// <summary>
         ///    термо, слэйв-контроллер 8 датчиков
         /// </summary>
         [Description("термо, слэйв-контроллер 8 датчиков")]
         TERMO_S = 4,

         /// <summary>
         ///    термо, мастер-контроллер 8 слэйвов
         /// </summary>
         [Description("термо, мастер-контроллер 8 слэйвов")]
         TERMO_M = 5,

         /// <summary>
         ///   контроллер кресла
         /// </summary>
         [Description("контроллер кресла")]
         PFS = 6,

         /// <summary>
         ///    контроллер цилиндра фарадея
         /// </summary>
         [Description("контроллер цилиндра фарадея")]
         FDCY = 7, //

         /// <summary>
         ///    универсальный контроллер на атмега для SERVER_TYPE_ASH
         /// </summary>
         [Description("универсальный контроллер на атмега для SERVER_TYPE_ASH")]
         ASH_ST = 8,

         /// <summary>
         ///    контроллер перемещателя AUTONICS
         /// </summary>
         [Description("контроллер перемещателя AUTONICS")]
         ANCSXY = 9,

         /// <summary>
         ///    видеокамера MV
         /// </summary>
         [Description("видеокамера MV")]
         MVCAM = 10
      }

      /// <summary>
      ///    Enum EServerConnectSignature
      /// </summary>
      public enum EServerConnectSignature
      {
         /// <summary>
         ///    линклиб
         /// </summary>
         [Description("LL")]
         LNKLIB,

         /// <summary>
         ///    протокол TM_Protocol
         /// </summary>
         [Description("TM")]
         TMPROTOCOL,

         /// <summary>
         ///    прямое подключение к COM порту c оберткой для 485
         /// </summary>
         [Description("COM")]
         DIRECT,

         /// <summary>
         ///    имитация подключения
         /// </summary>
         [Description("IM")]
         IMITATION,

         /// <summary>
         ///    простое TCP
         /// </summary>
         [Description("TCP")]
         TCPCUSTOM,

         /// <summary>
         ///    прямое подключение к COM порту
         /// </summary>
         [Description("RS232")]
         RS232
      }

      /// <summary>
      /// способы подключения сервера к источнику данных <br/>
      /// используется в ParseTagAsString, ParseConnectType, _DataServer_Info2Tree, <br/>
      /// ConnectToServer, DisconnectFromServer, IsConnected <br/>
      /// </summary>
      public enum EServerConnectType
      {
         /// <summary>
         ///    заглушка
         /// </summary>
         [Description("заглушка")]
         NONE = 0, //

         /// <summary>
         ///    библиотека linklib.h by P.Lunev (ретранслятор на кресло, температурные мастера, рентген-сервер(железо)...)
         /// </summary>
         [Description("библиотека linklib.h by P.Lunev (ретранслятор на кресло, температурные мастера, рентген-сервер(железо)...)")]
         LNKLIB = 1,

         /// <summary>
         ///    протокол TM_Protocol ( расширение TCP - температурный сервер-клиент, рентген-сервер-томограф, прокси кресла )
         /// </summary>
         [Description("протокол TM_Protocol ( расширение TCP - температурный сервер-клиент, рентген-сервер-томограф, прокси кресла )")]
         TMPROTOCOL = 2,

         /// <summary>
         ///    прямое подключение к COM порту <br/>
         /// (реализовано для кресла, для температурного сервера, для ретген-сервера)<br/>
         /// c оберткой для RS485 \"+\"/\":\"")]
         /// </summary>
         [Description("прямое подключение к COM порту (реализовано для кресла, для температурного сервера, для ретген-сервера) c оберткой для RS485 \"+\"/\":\"")]
         DIRECT = 4,

         /// <summary>
         ///    имитация подключения к устройству - всегда успешное. Для программ-имитаторов
         /// </summary>
         [Description("имитация подключения к устройству - всегда успешное. Для программ-имитаторов")]
         IMITATION = 8,

         /// <summary>
         ///    простое TCP - подключение к устройству. Разбор полностью реализуется по месту использования
         /// </summary>
         [Description("простое TCP - подключение к устройству. Разбор полностью реализуется по месту использования")]
         TCPCUSTOM = 16,

         /// <summary>
         ///    прямое подключение к ком-порту (COM порт реализовано Autonics)
         /// </summary>
         [Description("прямое подключение к ком-порту (COM порт реализовано Autonics)")]
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
      ///    у-во на AT-Mega. Не является маркером, просто для идентификации в DataServers_Info2Tree и конфигах
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
      ///    кресло. Не является маркером, просто для идентификации в DataServers_Info2Tree.
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
      ///    упрощенный температурный сервер (один слэйв).<br/>
      ///    Не является маркером, просто для идентификации в DataServers_Info2Tree и конфигах
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
      ///   перемещатель Autonics
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
      ///   сервер видеокамер. Не является маркером, просто для идентификации в конфигах
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
      ///   контроль выпуска (результаты выполнения)	TM
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
      ///   MainControl Interface Software - транслятор к программе упр.ускорителем как сервер TM
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
      ///   MainControl Software - программа упр.ускорителем как сервер (результаты выполнения, Израиль, не TM)
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
      ///   клиент томографа как сервер
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
      ///    кресло-контроль - прокси-сервер
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
      ///   глобал-тест
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
      ///   для сканера - только от клиента к неизвестному серверу	TM
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
      ///    сервер термоконтроля.
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
      ///   ретранслятор (конфигурация)
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
      ///   сервер рентгена
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
               return TMPR; //сервер термоконтроля
            case EServerType.XRAY:
               return XRay; //сервер рентгена
            case EServerType.PFS:
               return _PFS; //кресло. Не является маркером, просто для идентификации в DataServers_Info2Tree
            case EServerType.TMC:
               return TM_C; //клиент томографа как сервер
            case EServerType.DACADC:
               return ADDA;
            case EServerType.ACC:
               return _ACC;
            case EServerType.PFS_PROXY:
               return TMCP; //кресло-контроль - прокси-сервер
            case EServerType.TERMLITE:
               return _TML; //упрощенный температурный сервер (один слэйв). Не является маркером, просто для идентификации в DataServers_Info2Tree
            case EServerType.NETRT:
               return TMRT; //ретранслятор (конфигурация)
            case EServerType.GT:
               return TMGT; //глобал-тест
            case EServerType.NETSCAN:
               return TMCP; //для сканера - только от клиента к неизвестному серверу			TM
            case EServerType.ASH:
               return _ASH; //у-во на AT-Mega. Не является маркером, просто для идентификации в DataServers_Info2Tree
            case EServerType.MCS:
               return MCSv; //MainControl Software - программа упр.ускорителем как сервер
            case EServerType.AUTONICS:
               return ANCS; //перемещатель Autonics
            case EServerType.CAMSRV:
               return CAMS; //сервер видеокамер. Не является маркером, просто для идентификации в конфигах
            case EServerType.ECS:
               return ECSv; //контроль выпуска (результаты выполнения)	TM
            default:
               return TMNS;
         }

         return UNKNOWN;
      }

      #endregion
   }
}
