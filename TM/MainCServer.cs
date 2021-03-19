﻿// $Id: $

/*************************************************************************
 *                                                                       *
 * Copyright (C) 2021,   Valeriy Onuchin                                 *
 * All rights reserved.                                                  *
 *                                                                       *
 *************************************************************************/

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace TM
{
   /// <summary>
   ///    подтипы данных CMD в пакете пересылки клиент-&gt;сервер и сервер-&gt;клиент
   /// </summary>
   public enum EDataCommand
   {
      /// <summary>
      ///    блок выстрелов (часть плана) клиент->сервер
      /// </summary>
      [Description("блок выстрелов (часть плана) клиент->сервер")]
      SHOTSBLOCK = 1,

      /// <summary>
      ///    текущее состояние сервера  клиент-сервер <see cref="TM.MCS_State_topass" />
      /// </summary>
      [Description("текущее состояние сервера клиент<-сервер MCS_State_topass")]
      STATE = 2,

      /// <summary>
      ///    результат выполнения серии выстрелов клиент-сервер MCS_shot_results_topass
      /// </summary>
      [Description("результат выполнения серии выстрелов клиент<-сервер MCS_shot_results_topass")]
      SHOTSRESULTS = 3 // 
   }

   /// <summary>
   ///    состояния сервера MainControl (транслятор, симулятор) по протоколу TM
   /// </summary>
   public enum ECommandState
   {
      /// <summary>
      ///    не определенный
      /// </summary>
      [Description("не определенный")]
      UNKNOWN = 111, // 

      /// <summary>
      ///    "не готов
      /// </summary>
      [Description("не готов")]
      NOTREADY = 0,

      /// <summary>
      ///    готовность
      /// </summary>
      [Description("готовность")]
      READY = 1,

      /// <summary>
      ///    процесс исполнения плана
      /// </summary>
      [Description("процесс исполнения плана")]
      INPROCESS = 2,

      /// <summary>
      ///    процесс исполнения плана приостановлен
      /// </summary>
      [Description("процесс исполнения плана приостановлен")]
      PAUSED = 4,

      /// <summary>
      ///    процесс исполнения плана завершен
      /// </summary>
      [Description("процесс исполнения плана завершен")]
      FINISHED = 5
   }

   /// <summary>
   ///    состояния сервера MainControl как возвращаемое значение MainControl
   /// </summary>
   public enum EServerState
   {
      /// <summary>
      ///    не определён
      /// </summary>
      [Description("не определён")]
      UNKNOWN = -2,

      /// <summary>
      ///    сеанс лечения не готов
      /// </summary>
      [Description("сеанс лечения не готов")]
      NOTREADY = -1,

      /// <summary>
      ///    сеанс лечения готов
      /// </summary>
      [Description("сеанс лечения готов")]
      READY = 0,

      /// <summary>
      ///    получен фай
      /// </summary>
      [Description("получен файл")]
      FILEACCEPTED = 1,

      /// <summary>
      ///    сеанс лечения в процессе
      /// </summary>
      [Description("сеанс лечения в процессе")]
      INPROCESS = 2,

      /// <summary>
      ///    сеанс лечения закончен
      /// </summary>
      [Description("сеанс лечения закончен")]
      FINISHED = 3,

      /// <summary>
      ///    сеанс лечения прерван
      /// </summary>
      [Description("сеанс лечения прерван")]
      BREAK = 4,

      /// <summary>
      ///    сеанс лечения временно приостановлен по внешнему запросу
      /// </summary>
      [Description("сеанс лечения временно приостановлен по внешнему запросу")]
      PAUSED_EXT = 5,

      /// <summary>
      ///    сеанс лечения временно приостановлен по команде оператора ускорителя
      /// </summary>
      [Description("сеанс лечения временно приостановлен по команде оператора ускорителя")]
      PAUSED_OPT = 6,

      /// <summary>
      ///    сеанс лечения временно приостановлен. Требуется верификация перед сменой направления.
      /// </summary>
      [Description("сеанс лечения временно приостановлен. Требуется верификация перед сменой направления.")]
      WAIT_D = 7,

      /// <summary>
      ///    сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты
      /// </summary>
      [Description("сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты")]
      WAIT_H = 8,

      /// <summary>
      ///    сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты и ракурса
      /// </summary>
      [Description("сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты и ракурса")]
      WAIT_HD = 15,

      /// <summary>
      ///    сеанс лечения временно приостановлен. "Кресло" не встало в требуемую позицию
      /// </summary>
      [Description("сеанс лечения временно приостановлен. \"Кресло\" не встало в требуемую позицию")]
      BREAK_PFS_ERR = 9
   }

   /// <summary>
   ///    результат выполнения выстрела
   /// </summary>
   public enum ESpotResult
   {
      /// <summary>
      ///    выстрел еще не производился
      /// </summary>
      [Description("выстрел еще не производился")]
      NONE = 0,

      /// <summary>
      ///    выстрел произведен успешно
      /// </summary>
      [Description("выстрел произведен успешно")]
      DONE = 1,

      /// <summary>
      ///    выстрел произведен частично
      /// </summary>
      [Description("выстрел произведен частично")]
      PARTIALLY = 2,

      /// <summary>
      ///    выстрел признан некорректным, отсеян
      /// </summary>
      [Description("выстрел признан некорректным, отсеян")]
      INCORRECT = 3,

      /// <summary>
      ///    результат не получен системой контроля выпуска
      /// </summary>
      [Description("результат не получен системой контроля выпуска")]
      TIMEOUT = 4,

      /// <summary>
      ///    отправлен на отстрел, результата еще нет
      /// </summary>
      [Description("отправлен на отстрел, результата еще нет")]
      PASSED = 5
   }

   /// <summary>
   ///    ошибка, приведшая к не выполнению команды <br />
   ///    при пополнении править: GetLastErrorDescription, список расшифровок
   /// </summary>
   public enum ECommandError
   {
      /// <summary>
      ///    процесс прерван по внешнему запросу
      /// </summary>
      [Description("процесс прерван по внешнему запросу")]
      EXT_INERRUPT = 2,

      /// <summary>
      ///    план успешно принят ускорителем
      /// </summary>
      [Description("план успешно принят ускорителем")]
      PLANACCEPTED = 1,

      /// <summary>
      ///    выполнение команды успешно
      /// </summary>
      [Description("выполнение команды успешно")]
      NOERROR = 0,

      /// <summary>
      ///    ошибка подключения к ретранслятору
      /// </summary>
      [Description("ошибка подключения к ретранслятору")]
      NOTCONNECTED = -1,

      /// <summary>
      ///    попытка выполнить команду при неготовности сервера
      /// </summary>
      [Description("попытка выполнить команду при неготовности сервера")]
      STATENOTREADY = -2,

      /// <summary>
      ///    ошибка доступа к файловой систем
      /// </summary>
      [Description("ошибка доступа к файловой систем")]
      FILEIOERROR = -3,

      /// <summary>
      ///    некорректные данные в плане
      /// </summary>
      [Description("некорректные данные в плане")]
      INCORRECTDATA = -4,

      /// <summary>
      ///    ошибка подключения ретранслятора к MainControl
      /// </summary>
      [Description("ошибка подключения ретранслятора к MainControl")]
      ACCNOTCONNECTED = -5,

      /// <summary>
      ///    не готовность ускорителя к приему плана
      /// </summary>
      [Description("не готовность ускорителя к приему плана")]
      ACCNOTREADY = -6,

      /// <summary>
      ///    процесс прерван оператором MainControl
      /// </summary>
      [Description("процесс прерван оператором MainControl")]
      OP_INERRUPT = -7,

      /// <summary>
      ///    процесс приостановлен оператором
      /// </summary>
      [Description("процесс приостановлен оператором")]
      OP_PAUSE = -8,

      /// <summary>
      ///    ожидание смены направления
      /// </summary>
      [Description("ожидание смены направления")]
      WAIT_D = -9,

      /// <summary>
      ///    ожидание смены высоты
      /// </summary>
      [Description("ожидание смены высоты")]
      WAIT_H = -10,

      /// <summary>
      ///    ожидание смены высоты и направления
      /// </summary>
      [Description("ожидание смены высоты и направления")]
      WAIT_HD = -11,

      /// <summary>
      ///    ошибка положения системы фиксации
      /// </summary>
      [Description("ошибка положения системы фиксации")]
      PFSPOSITION = -12,

      /// <summary>
      ///    повторный запуск плана запрещен
      /// </summary>
      [Description("повторный запуск плана запрещен")]
      RESTART_PROHIBITED = -13,

      /// <summary>
      ///    план еще не загружен в интерфейс
      /// </summary>
      [Description("план еще не загружен в интерфейс")]
      NO_PLAN_UPLOADED = -14,

      /// <summary>
      ///    план не содержит корректных выстрелов
      /// </summary>
      [Description("план не содержит корректных выстрелов")]
      PLAN_EMPTY = -15,

      /// <summary>
      ///    неизвестная команда|данные
      /// </summary>
      [Description("неизвестная команда|данные")]
      UNKNOWNCMD = -16,

      /// <summary>
      ///    ошибка контрольной суммы в присланном пакете
      /// </summary>
      [Description("ошибка контрольной суммы в присланном пакете")]
      CHECKSUMMERR = -17,

      /// <summary>
      ///    получено данных меньше, чем указано в заголовке пакета
      /// </summary>
      [Description("получено данных меньше, чем указано в заголовке пакета")]
      CROPPEDDATA = -18,

      /// <summary>
      ///    ошибка подключения сервера к серверу контроля пучка
      /// </summary>
      [Description("ошибка подключения сервера к серверу контроля пучка")]
      ECSNOTCONNECTED = -19
   }

   /// <summary>
   ///    состояние сервера MainCServer - структура ServerData в DataServer
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct MCS_State
   {
      /// <summary>
      ///    The length of MCS_State structure
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(MCS_State));

      #region  Fields

      // состояние транслятора или симулятора:
      /// <summary>
      ///    The state
      /// </summary>
      [Description("текущее состояние сервера из списка состояний")]
      public uint state; // текущее состояние сервера из списка состояний - MCS_STATE_READY

      /// <summary>
      ///    The lasterror
      /// </summary>
      [Description("крайняя возникшая ошибка, приведшая к отказу")]
      public uint lasterror; // крайняя возникшая ошибка, приведшая к отказу  - MCS_ERR_NOERROR 

      /// <summary>
      ///    количество уже произведенных выстрелов в плане
      /// </summary>
      [Description("количество уже произведенных выстрелов в плане")]
      public uint spots_passed; // 

      /// <summary>
      ///    The pcount passed
      /// </summary>
      [Description("количество выпущенных протонов по плану")]
      public float pcount_passed; // количество выпущенных протонов по плану

      /// <summary>
      ///    The spots on remote
      /// </summary>
      [Description("количество выстрелов в плане на удаленном сервер")]
      public uint spots_on_remote; // 

      //ARArray spots;					// набор выстрелов для исполнения ускорителем. Элементы типа plan_spot
      /// <summary>
      ///    The arr
      /// </summary>
      [Description("массив элементов")]
      private readonly IntPtr arr; // массив элементов

      /// <summary>
      ///    The elsize
      /// </summary>
      [Description("размер элемента (байт)")]
      public int elsize; // размер элемента (байт)

      /// <summary>
      ///    The size
      /// </summary>
      [Description("размер массива (кол-во элементов)")]
      public int size; // 

      /// <summary>
      ///    The count
      /// </summary>
      [Description("текущий размер массива (кол-во элементов)")]
      public int count; // текущий размер массива (кол-во элементов)

      /// <summary>
      ///    The incsize
      /// </summary>
      [Description("приращение массива")]
      public int incsize; // приращение массива

      /// <summary>
      ///    The plan finished
      /// </summary>
      [Description("флаг - план исполнен, для предотвращения повторного запуска плана")]
      public uint plan_finished; // флаг - план исполнен, для предотвращения повторного запуска плана

      /// <summary>
      ///    The waitshots count
      /// </summary>
      public uint waitshots_count;

      /// <summary>
      ///    The state mc
      /// </summary>
      [Description("текущее состояние сервера MainControl")]
      public int stateMC; // текущее состояние сервера MainControl - MC_STATE_READY

      /// <summary>
      ///    The progress
      /// </summary>
      [Description("прогресс исполнения текущего плана")]
      public double progress; // прогресс исполнения текущего плана 

      /// <summary>
      ///    The time
      /// </summary>
      [Description("общее время выполнения облучения")]
      public float time; // общее время выполнения облучения

      /// <summary>
      ///    The start time
      /// </summary>
      [Description("время старта облучения возвращенное Timer")]
      public float startTime; // время старта облучения возвращенное Timer()

      /// <summary>
      ///    The spots pcount
      /// </summary>
      [Description("суммарное количество протонов для пересчета исполнения")]
      public double spots_pcount; // суммарное количество протонов для пересчета исполнения

      /// <summary>
      ///    The state mc changed
      /// </summary>
      [Description("флаг - состояние изменилось, требуется рассылка клиентам")]
      public int stateMC_changed; // флаг - состояние изменилось, требуется рассылка клиентам

      /// <summary>
      ///    The progress new
      /// </summary>
      [Description("прогресс исполнения текущего плана")]
      public double progress_new; // прогресс исполнения текущего плана (присланный сервером)

      // входящие данные от самой программы в транслятор, используются MainCInt

      #endregion
   }

   /// <summary>
   ///    состояние сервера MainCServer - структура для пересылки
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct MCS_State_topass
   {
      #region  Fields

      /// <summary>
      ///    The length of MCS_State_topass structure
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(MCS_State_topass));

      /// <summary>
      ///    текущее состояние сервера из списка состояний - MCS_STATE_READY
      /// </summary>
      [Description("текущее состояние сервера из списка состояний - MCS_STATE_READY")]
      public int state;

      /// <summary>
      ///    ошибка выполнения крайней команды из списка ошибок - MCS_ERR_NOERROR
      /// </summary>
      [Description("ошибка выполнения крайней команды из списка ошибок - MCS_ERR_NOERROR")]
      public uint lasterror;

      /// <summary>
      ///    количество уже произведенных выстрелов в плане
      /// </summary>
      [Description("количество уже произведенных выстрелов в плане")]
      public uint spots_passed;

      /// <summary>
      ///    количество загруженных выстрелов в плане
      /// </summary>
      [Description("количество загруженных выстрелов в плане")]
      public uint spots_count;

      #endregion

      /// <summary>
      ///    Returns a <see cref="System.String" /> that represents this instance.
      /// </summary>
      public override string ToString()
      {
         var sb = new StringBuilder();
         sb.AppendLine("Статус сервера : state = " + ((EServerState) state).Description() + ", lasterror = " + lasterror + ", spots_passed = " + spots_passed + ", spots_count = " + spots_count);
         return sb.ToString();
      }
   }

   /// <summary>
   ///    состояние сервера MainCServer - структура для пересылки <br />
   ///    используется в обоих протоколах
   /// </summary>
   public struct MCS_spot_results_topass
   {
      #region Static fields

      /// <summary>
      ///    The length of MCS_spot_results_topass structure
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(MCS_spot_results_topass));

      #endregion

      #region  Fields

      /// <summary>
      ///    результат выполнения MCS_SHOT_RESULT_DONE
      /// </summary>
      [Description("результат выполнения MCS_SHOT_RESULT_DONE")]
      private int done;

      /// <summary>
      ///    уникальный идентификатор записи (напр. счетчик)
      /// </summary>
      [Description("уникальный идентификатор записи (напр. счетчик)")]
      private int id;

      /// <summary>
      ///    количество протонов
      /// </summary>
      [Description("количество протонов")]
      private float result_pcount;

      /// <summary>
      ///    угол по горизонтали
      /// </summary>
      [Description("угол по горизонтали")]
      private float result_xangle;

      /// <summary>
      ///    угол по вертикали
      /// </summary>
      [Description("угол по вертикали")]
      private float result_zangle;

      #endregion
   }

   /// <summary>
   ///    <code>
   ///    формат структуры обмена с программой MainContol как сервером (версия Протвино, Обнинск)
   /// 
   ///    client to server:
   ///    0–запрос о состоянии ускорителя,
   ///    1–начать лечение,
   ///    2–прервать лечение,
   ///    3–временно приостановить лечение,
   ///    4–возобновить лечение
   /// 
   ///    server to client:
   ///   -1 – ускоритель не готов;
   ///    0 – ускоритель готов;
   ///    1 – файл-задание на терапию успешно принят сервером
   ///    2 – сеанс лечения начался в pt.t_procent передается процент выполнения задания в pt.file_name
   ///    передается имя файла истории облучения
   ///    3 – сеанс лечения закончился
   ///    4 – сеанс лечения прерван
   ///    5 – сеанс лечения временно приостановлен по внешнему запросу
   ///    6 – сеанс лечения временно приостановлен по команде оператора ускорителя
   ///    7 – сеанс лечения временно приостановлен. Требуется верификация перед сменой направления.
   ///    8 – сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты.
   ///    15 – сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты и ракурса.
   ///    9 – сеанс лечения временно приостановлен. "Кресло" не встало в требуемую позицию.
   ///  </code>
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct MC_PT
   {
      /// <summary>
      ///    The length of MC_PT structure
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(MC_PT));

      #region  Fields

      /// <summary>
      ///    статус/признак передаваемой информации
      /// </summary>
      [Description("статус/признак передаваемой информации")]
      public int s_info; //

      /// <summary>
      ///    полное имя файла-задания на терапию
      /// </summary>
      [Description("полное имя файла-задания на терапию")]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
      public byte[] filePlan_name; //

      /// <summary>
      ///    передается процент, с которого надо начать выполнение задания
      /// </summary>
      [Description("передается процент, с которого надо начать выполнение задания")]
      public double t_procent;

      /// <summary>
      ///    время выполнения плана от начала до останова
      /// </summary>
      [Description("время выполнения плана от начала до останова")]
      public float ir_tm;

      /// <summary>
      ///    полное имя файла проекций
      /// </summary>
      [Description("полное имя файла проекций")]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
      public byte[] fileProection_name;

      /// <summary>
      ///    полное имя файла контуров
      /// </summary>
      [Description("полное имя файла контуров")]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
      public byte[] fileContours_name;

      /// <summary>
      ///    режим облучения (1 – сидя, 3 - лежа)
      /// </summary>
      [Description("режим облучения (1 – сидя, 3 - лежа)")]
      public int PFSMode;

      /// <summary>
      ///    делать остановку при смене угла (0 - не делать, 1 - делать)
      /// </summary>
      [Description("делать остановку при смене угла (0 - не делать, 1 - делать)")]
      public int Fl_ChAngle;

      /// <summary>
      ///    делать остановку при смене высоты (0 - не делать, 1 - делать)
      /// </summary>
      [Description("делать остановку при смене высоты (0 - не делать, 1 - делать)")]
      public int Fl_ChHeight;

      #endregion
   }

   /// <summary>
   ///    формат структуры обмена с программой MainContol как сервером (версия Израиль), заголовок
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct MC_PT_ILH
   {
      /// <summary>
      ///    The length of MC_PT_ILH structure
      /// </summary>
      public static uint Length = (uint) Marshal.SizeOf(typeof(MC_PT_ILH));

      #region  Fields

      /// <summary>
      ///    signature: "TMCL"
      /// </summary>
      [Description("signature: \"TMCL\"")]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public byte[] sign;

      /// <summary>
      ///    type: 1 - shot results
      /// </summary>
      [Description("type: 1 - shot results")]
      public int type;

      /// <summary>
      ///    статус/признак передаваемой информации
      ///    <code>
      ///    client to server:
      ///       0 – запрос о состоянии ускорителя, 
      ///  		1 – начать лечение, 
      ///  		2 – прервать лечение, 
      ///  		3 – временно приостановить лечение, 
      ///  		4 – возобновить лечение
      ///  
      ///    server to clent:
      ///      -1 – ускоритель не готов; 
      ///  		0 – ускоритель готов; 
      ///  		1 – файл-задание на терапию успешно принят сервером 
      ///  		2 – сеанс лечения начался в pt.t_procent передается процент выполнения задания в pt.file_name передается имя файла истории облучения 
      ///  		3 – сеанс лечения закончился 
      ///  		4 – сеанс лечения прерван 
      ///  		5 – сеанс лечения временно приостановлен по внешнему запросу 
      ///  		6 – сеанс лечения временно приостановлен по команде оператора ускорителя
      ///  		7 – сеанс лечения временно приостановлен. Требуется верификация перед сменой направления. 
      ///  		8 – сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты. 
      ///  		15 – сеанс лечения временно приостановлен. Требуется верификация перед сменой высоты и ракурса. 
      ///  		9 – сеанс лечения временно приостановлен. "Кресло" не встало в требуемую позицию. 
      /// </code>
      /// </summary>
      [Description("статус/признак передаваемой информации")]
      public int s_info;

      /// <summary>
      ///    procent
      /// </summary>
      public double t_procent;

      /// <summary>
      ///    The ir tm
      /// </summary>
      public int ir_tm;

      /// <summary>
      ///    количество выстрелов в отчете
      /// </summary>
      [Description("количество выстрелов в отчете")]
      public int spots_count;

      #endregion
   }

   public class MainCServer : TMDataServer
   {
   }
}
