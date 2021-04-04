
//#include "DataServers.h"		// абстрактный сервер - источник данных
//#include "lnklb.h"				// библиотека сетевого обмена (П. Лунёв)
//#include "RS-232_RW.h"			// структуры данных и операции с COM-портом
//#include "RemReq.h"				// библиотека сетевого обмена (П. Лунёв)
//#include "TCPSrv.h"				// сервер обрабадывающий подключение к нему по TCP
//#include "TM_protocol.h"		// протокол обмена сообщениями проекта "томограф" (рентген, кресло, термоконтроль)
//#include "Queue.h"				// очереди блоков данных


using System;
using System.Collections.Generic;
using System.ComponentModel;
using TM;
using TMSrv;


public delegate void ConnectHandler(Server pds, int res);
public delegate void PostParseHandler(int cmd, int cid);
public delegate void IncomingPacketHandler(Server pds, Packet p, byte[] pdata);
public delegate int  MakeCommandHandler(int cmd, byte[] send_data, int n, DevDescr p_dev, short reqrespdelay);
public delegate void ServerHandler(Server pds);

namespace TMSrv
{
// для всех контролов заполнить:
//   GetCntrlTemplaiteByType
//   GetDefaultControlName
//   Termo_ChangeTermometersStatus || XRay_ChangeDevicesStatus || PFSDS_ChangeDevicesStatus || ChangeTmCDevicesStatus || ChangeVacDevicesStatus || ACC_ChangeDevicesStatus || ECSrv_ChangeDevicesStatus
//   SetCurrentServer
//   ShowDevicesStatus
//   GetSelectedControlTypeAvailable


//#define VCCSTATUS_ON_MOVE	1
//#define VCCSTATUS_STABLE	0
/*
static int TMPR_DEVICES_SENSE_COUNT = 8; //количество датчиков на слэйве в интерфейсе температурного сервера

static int MAX_TIMER_TICKS_FOR_CORRECTDATA = 10; // *10секунд запуск таймера


static int  DATASERVER_CONNECTED = 0; // значение, возвращаемое IsConnected при наличии коннекта
static int  DATASERVER_DISCONNECTED = 1; // значение, возвращаемое IsConnected при отсутствии коннекта

static int  DS_CMD_NOTHING = 0; // пустышка, ничего не отправлять
static int DATA_LENGTH_0 = 0; // длина данных - 0
static int DATA_STRUCT_NULL = NULL; // пустышка вместо данных
*/

   /// <summary>
   ///    визуализация датчика. Добавляется серверу посредством AddVCControl
   /// </summary>
   public class VCControl
   {
      #region  Fields

// для контролов вида CONTROL_TYPE_TEMPSET, CONTROL_TYPE_TEMPSINGLE
      /// <summary>
      ///    CID
      /// </summary>
      [Description("CID")]
      int CID;

      /// <summary>
      ///     id LED контрола, отображающего статус
      /// </summary>
      [Description("id LED контрола, отображающего статус")]
      int ControlID;

      /// <summary>
      ///    для контролов-индикаторов
      /// </summary>
      [Description("для контролов-индикаторов")]
      float control_val; // 

      /// <summary>
      /// "количество подключенных к слэйву датчиков
      /// </summary>
      [Description("количество подключенных к слэйву датчиков")]
      int count; // 

      /// <summary>
      ///    статус устройства DEVICE_STATUS_OK DEVICE_STATUS_MOVING
      /// </summary>
      [Description("статус устройства DEVICE_STATUS_OK DEVICE_STATUS_MOVING")]
      int device_status; // 

      /// <summary>
      ///    CID
      /// </summary>
      [Description("CID")]
      int left;

      /// <summary>
      ///    основное устройство - для температурных датчиков только
      /// </summary>
      [Description("основное устройство - для температурных датчиков только")]
      int masterCID;

      /// <summary>
      ///    текст метки
      /// </summary>
      [Description("текст метки")]
      byte[] name = new byte[32];

      /// <summary>
      ///    номер данного датчика ( для CONTROL_TYPE_TEMPSINGLE )
      /// </summary>
      [Description("номер данного датчика ( для CONTROL_TYPE_TEMPSINGLE )")]
      int num;

//	int update;
      /// <summary>
      ///    панель контрола, на которой расположен датчик
      /// </summary>
      [Description("панель контрола, на которой расположен датчик")]
      int panelHandle;

      /// <summary>
      ///    коррекция температур
      /// </summary>
      [Description("коррекция температур")]
      int term_correct;

      /// <summary>
      ///    положение контрола
      /// </summary>
      [Description("положение контрола")]
      int top;

      /// <summary>
      ///    тип контрола начиная с CONTROL_TYPE_UNKNOWN
      /// </summary>
      [Description("тип контрола начиная с CONTROL_TYPE_UNKNOWN")]
      int type;

      /// <summary>
      ///    режим отображения датчика
      /// </summary>
      [Description("режим отображения датчика")]
      int view_mode;

      #endregion
   }

   /// <summary>
   ///    серверные данные <br/>
   ///    только для сервера вакуума
   /// </summary>
   public class DACADC_data
   {
      #region Static fields

      /// <summary>
      ///    максимальное количество каналов в сервере
      /// </summary>
      [Description("максимальное количество каналов в сервере")]
      public static int DACADC_MAX_ADC = 8;

      /// <summary>
      ///    "максимальное количество каналов в сервере
      /// </summary>
      [Description("максимальное количество каналов в сервере")]
      public static int DACADC_MAX_DAC = 8;

      #endregion

      #region  Fields

      /// <summary>
      ///    The adc
      /// </summary>
      public double[] adc = new double[DACADC_MAX_ADC];

      /// <summary>
      ///    The addr
      /// </summary>
      public byte addr;

      /// <summary>
      /// The dac
      /// </summary>
      public ushort[] dac = new ushort[DACADC_MAX_DAC];

      /// <summary>
      /// максимальное количество входных каналов на этом устройстве
      /// </summary>
      [Description("максимальное количество входных каналов на этом устройстве")]
      public byte MAX_ADC;

      /// <summary>
      /// максимальное количество выходных каналов на этом устройстве
      /// </summary>
      [Description("максимальное количество выходных каналов на этом устройстве")]
      public byte MAX_DAC;

      #endregion
   }

// параметры подключения к устройству через протокол линклиб

   /*
   class LinkLib 
   {
   int sid;	// sid
   int seg;	// сегмент
   };


   // структура для запроса через линклиб
   typedef struct {
   SDEV sdev;
   void *p_ds;  			// указатель на дата-сервер  DataServer
   void *p_dds;  			// указатель на структуру-дескриптор устройства, к которому был запрос  DevDescr *
   int asknum; 			// номер запроса, циклический счетчик
   int cmd;				// текущая выполняемая команда (внутренний код программы)
   //byte log_resp; // флаг - вывести ответ в лог
		
   }	DS_LnkLb_ask;
   */


   /// <summary>
   ///    Class DevDescr.
   /// </summary>
   public class DevDescr
   {
      #region  Fields

      /// <summary>
      ///    устройство отконфигурировано, можно делать запросы
      /// </summary>
      [Description("устройство отконфигурировано, можно делать запросы")]
      public int configured;

      // void* p_ds; // указатель на датасервер
      // void* p_dev; // указатель на устройство - владельца (как внутреннее устройство датасервера)

      /// <summary>
      ///    тип устройства  (DEVICE_TYPE_DIMS etc)
      /// </summary>
      [Description("тип устройства  (DEVICE_TYPE_DIMS etc)")]
      public int dev_type; //

      /// <summary>
      ///     индивидуальный адрес устройства
      /// </summary>
      [Description("индивидуальный адрес устройства")]
      public uint DN;

      /// <summary>
      ///    The error in wait
      /// </summary>
      [Description("The error in wait")]
      public byte err_InWait;

      /// <summary>
      ///    сервер в режиме ожидания ответа от железа
      /// </summary>
      [Description("сервер в режиме ожидания ответа от железа")]
      int InWait;

      /// <summary>
      ///    статус устройства (согласно TM протоколу) ( DEVICE_STATUS_OK	etc )
      /// </summary>
      [Description("статус устройства (согласно TM протоколу) ( DEVICE_STATUS_OK	etc )")]
      public int status;

      /// <summary>
      ///    контрол статуса; черный-неотконф., красный-поломка, зеленый-ОК
      /// </summary>
      [Description("контрол статуса; черный-неотконф., красный-поломка, зеленый-ОК")]
      public int StatusControl;

      /// <summary>
      ///    название устройства для вывода сообщений в консоль
      /// </summary>
      [Description("название устройства для вывода сообщений в консоль")]
      public string name; // 

      #endregion
   }
   public class TmLnk
   {
      // sid, seg
      public int sid
      {
         get;
         set;
      }

      // RQ_ZERO и т.д
      public ushort RQ 
      {
         get;
         set;
      } 

      //TmQueue asks;			// очередь запросов
      // текущий запрос (циклический счетчик до 999), для TM используется как счетчик запросов
      public int asknum 
      {
         get;
         set;
      } 
   }

   /// <summary>
   /// Class ComPortData.
   /// </summary>
   public class ComPortData
   {
      /// <summary>
      /// настройки сконфигурированы под реальный порт, можно работать
      /// </summary>
      [Description("The configuration flag")]
      public int config_flag
      {
         get;
         set;
      }

      /// <summary>
      /// номер COM порта  
      /// </summary>
      [Description("номер COM порта")]
      public int comport
      {
         get;
         set;
      }

      /// <summary>
      /// COM port baud rate
      /// </summary>
      [Description("baud rate")]
      public int baudrate
      {
         get;
         set;
      }

      /// <summary>
      /// The COM port parity
      /// </summary>
      [Description("parity")]
      public int parity
      {
         get;
         set;
      }

      /// <summary>
      /// The COM port data bits
      /// </summary>
      [Description("databits")]
      public int databits
      {
         get;
         set;
      }

      /// <summary>
      /// The COM port stop bits
      /// </summary>
      [Description("stopbits")]
      public int stopbits
      {
         get;
         set;
      }

      /// <summary>
      /// Sets input queue length in OpenComConfig 
      /// </summary>
      public int inputq
      {
         get;
         set;
      }

      /// <summary>
      /// Sets output queue length in OpenComConfig
      /// </summary>
      public int outputq
      {
         get;
         set;
      }

      /// <summary>
      /// The port open
      /// </summary>
      public int PortOpen
      {
         get;
         set;
      }

      /// <summary>
      /// The RS232 error
      /// </summary>
      public int RS232Error
      {
         get;
         set;
      }

      /// <summary>
      /// The timeout
      /// </summary>
      public double timeout
      {
         get;
         set;
      }

      /// <summary>
      /// The device name
      /// </summary>
      public string devicename
      {
         get;
         set;
      }

      public int PortBusyFlag
      {
         get;
         set;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ComData"/> class.
      /// </summary>
      public ComPortData()
      {
      }
   }

   /// <summary>
   ///    универсальный сервер - источник данных для подключения к разным источникам данных:
   ///    TCP серверам по протоколу TM, устройствам через линклиб или com порт
   /// </summary>
   public partial class Server : IDisposable
   {
      public static List<Server> Servers = new List<Server>();

      /// <summary>
      ///  максимальное количество каналов в сервере
      /// </summary>
      public static byte DACADC_MAX_DAC = 8;

      /// <summary>
      ///    максимальное количество каналов в сервере
      /// </summary>
      public static byte DACADC_MAX_ADC = 8;

      #region  Properties  
      
      /// <summary>
      ///   название устройства для вывода сообщений в консоль
      ///   для коммуникации. -1 если не используется сейчас, 0+ если канал действует.
      /// </summary>
      [Description("название устройства для вывода сообщений в консоль")]
      public int handle
      {
         get;
         set;
      }

      /// <summary>
      /// IP (= "127.0.0.1")
      /// </summary>
      [Description("IP")]
      public string ip
      {
         get;
         set;
      }

      /// <summary>
      ///    Порт (= 9995)
      /// </summary>
      [Description("Port")]
      public int port
      {
         get;
         set;
      }

      /// <summary>
      /// тип подключения: linklib, TMProtocol или
      /// локальное  SERVER_ConnectType_TMPROTOCOL
      /// </summary>
      [Description("тип подключения")]
      public EConnectType ConnectType
      {
         get;
         set;
      }

      /// <summary>
      /// возможные (реализованные) способы подключения,
      /// напр. SERVER_CONNESERVER_ConnectType_DIRECT
      /// </summary>
      [Description("возможные способы подключения")]
      public EConnectType ConnectAvailable
      {
         get;
         set;
      }

      /// <summary>
      ///    тип сервера 0-неизвестный, SERVER_TYPE_TEMPERATURE и т.д
      /// </summary>
      [Description("тип сервера")]
      public EServerType type
      {
         get;
         set;
      }

      /// <summary>
      ///    имя сервера
      /// </summary>
      [Description("имя сервера")]
      public string name
      {
         get;
         set;
      }

      /// <summary>
      ///    флаг - сервер находится в процессе, удаление требует ожидания
      /// </summary>
      [Description("сервер находится в процессе")]
      public int OnProcessing
      {
         get;
         set;
      }

      public ComPortData COM 
      {
         get;
         set;
      }

      public TmLnk lnk 
      {
         get;
         set;
      }

      /// <summary>
      ///    id контрола, отображающего коннект к серверу
      /// </summary>
      [Description("id контрола")]
      public int ControlID
      {
         get;
         set;
      }

      /// <summary>
      ///   правая граница контрола, для выстраивания цепочки
      /// </summary>
      [Description("правая граница контрола")]
      public int ControlRight
      {
         get;
         set;
      }

      /// <summary>
      ///    интерфейсные контролы
      /// </summary>
      [Description("интерфейсные контролы")]
      public List<VCControl> Controls = new List<VCControl>(); //

      /// <summary>
      ///   количество интерфейсных контролов
      /// </summary>
      [Description("количество интерфейсных контролов")]
      public int ControlsCount
      {
         get;
         set;
      }

      /// <summary>
      ///   статус устройства - напр. DEVICE_STATUS_OK
      /// </summary>
      [Description("статус устройства")]
      public EDeviceStatus DevStatus
      {
         get;
         set;
      }

      /// <summary>
      /// расшифровка статуса датчика
      /// для датчиков состояний
      /// вызывается из ChangeTermometersStatus
      /// </summary>
      string StatusString 
      {
         get 
         {
            switch (DevStatus) {
               case EDeviceStatus.OK:
                  return "OK";
               case EDeviceStatus.ATTENTION:
                  return "Attention!";
               case EDeviceStatus.HOT:
                  return "Warning!";
               case EDeviceStatus.OFF:
                  return "OFF!";
               case EDeviceStatus.CRACKED:
                  return "Cracked";
            }

            return "Unknown";
         }
      }

      /// <summary>
      ///     сервер в режиме ожидания ответа от железа
      /// </summary>
      [Description("сервер в режиме ожидания ответа от железа")]
      public byte InWait
      {
         get;
         set;
      }

      /// <summary>
      ///    The log path
      /// </summary>
      [Description("The log path")]
      public string LogPath
      {
         get;
         set;
      }

      /// <summary>
      ///    автоматическое подключение
      /// </summary>
      [Description("автоматическое подключение")]
      public int NeedAutoReconnect
      {
         get;
         set;
      }

      /// <summary>
      ///    необходимость восстановления соединения в текущем сеансе
      /// </summary>
      [Description("необходимость восстановления соединения в текущем сеансе")]
      public int NeedForReconnect
      {
         get;
         set;
      }

      /// <summary>
      ///     строковый буфер для временного хранения сообщений
      /// </summary>
      [Description("строковый буфер")]
      public string buf
      {
         get;
         set;
      }

      /// <summary>
      ///   таймаут ожидания ответа от железа (задействован в термоконтроле), секунды
      /// </summary>
      [Description("таймаут ожидания ответа от железа")]
      public float timeout
      {
         get;
         set;
      }

      /// <summary>
      ///    Cчетчик тиков. Сбрасывается при пришедших от сервера данных
      /// </summary>
      [Description("Cчетчик тиков")]
      public int TimerTicks
      {
         get;
         set;
      }

/*
// только для подключения linklib:
struct {
int sid;				// sid, seg
unsigned short RQ;		// RQ_ZERO и т.д
TmQueue asks;			// очередь запросов
int asknum; 			// текущий запрос (циклический счетчик до 999), для TM используется как счетчик запросов
} lnk;
*/

// только для подключения TM
      /// <summary>
      ///   битовое поле, настройки протокола взаимодействия - TMSettings_ADDCHECKSUMM
      /// </summary>
      [Description("настройки протокола взаимодействия")]
      public byte TMSettings
      {
         get;
         set;
      }

      /// <summary>
      ///    флаг - сохранять обмен пакетами с устройством в лог LogPath
      /// </summary>
      [Description("сохранять обмен пакетами с устройством в лог")]
      public byte traceToLog
      {
         get;
         set;
      }

      /// <summary>
      ///    счетчик апдейтов
      /// </summary>
      [Description("счетчик апдейтов")]
      public byte update
      {
         get;
         set;
      }

      /// <summary>
      ///    интервал таймера в секундах
      /// </summary>
      [Description("интервал таймера в секундах")]
      public float UpdatePeriod
      {
         get;
         set;
      }

// только для пассивных серверов:
      /// <summary>
      ///    таймер периодических опросов. заводится и стартует из StartUpdateTimer
      /// </summary>
      [Description("таймер периодических опросов")]
      public int UpdateTimerID
      {
         get;
         set;
      }

      /// <summary>
      /// флаг - изменилась аппаратная конфигурация - надо разослать инф. клиентам
      /// </summary>
      [Description("изменилась аппаратная конфигурация")]
      public byte configChanged
      {
         get;
         set;
      }

      #endregion

      #region Nested structs

      /// <summary>
      /// для пакетов, реализующих какой-то уникальный протокол
      /// </summary>
      public struct ntm
      {
         #region Properties

         /// <summary>
         /// текущий ожидаемый размер пакета.
         /// Если 0, то вычитвается максимально возможное количество байт
         /// </summary>
         [Description("текущий ожидаемый размер пакета")]
         public int waitpacketsize
         {
            get;
            set;
         }


         #endregion
      }

      #endregion

/*
   static int CVICALLBACK
    PingServiceTimerCB(int panel, int control, int event,
   static void* callbackData,
    int eventData1,
    int eventData2);
*/
      /// <summary>
      /// ошибки, выбираемые RS485_GetErrorMessage
      /// </summary>
      public static Dictionary<int, string[]> DS_LangText = new Dictionary<int, string[]>
      {
         {0, new[] {"Нет ошибок", "No error"}}, 
         {1, new[] {"Ошибка контрольной суммы", "Check summ error"}},
         {2, new[] {"Ошибка операции с EEPROM, считанные данные не соответствуют записанным", "EEPROM operation error, read data and wrotten dat not equals"}},
         {3, new[] {"Ошибка значения счётчика байт записи", "Bytes counter error"}}, 
         {4, new[] {"Код команды не поддерживается", "Command not supported"}},
         {5, new[] {"Ошибка номера канала", "Chanel number error"}}, 
         {6, new[] {"Ошибка номера параметра", "Parameter number error"}},
         {7, new[] {"Ошибка количества параметров", "Parameters count error"}}, 
         {8, new[] {"Ошибка значения параметра", "Parameter value error"}},
         {9, new[] {"Ошибка времени ожидания", "Time out error"}},
         {10, new[] {"Устройство занято", "Device buzy"}},
         {11, new[] {"Не совпали адреса запрашиваемого и ответившего устройства или код запроса не равен коду ответа", "ANQ error"}},
         {12, new[] {"Истекло время ожидания в канале приёма USART0", "USART0 Time out"}},
         {13, new[] {"UART ошибка четности USART0", "UART Parity Error USART0"}},
         {14, new[] {"Data Overrun USART0", "Data Overrun USART0"}}, 
         {15, new[] {"ERR_DOR0_UPE0", "ERR_DOR0_UPE0"}},
         {16, new[] {"Ошибка кадра USART0", "Frame Error USART0"}},
         {17, new[] {"ERR_FE0_UPE0", "ERR_FE0_UPE0"}}, 
         {18, new[] {"ERR_FE0_DOR0", "ERR_FE0_DOR0"}},
         {19, new[] {"ERR_FE0_DOR0_UPE0", "ERR_FE0_DOR0_UPE0"}},
         {20, new[] {"Истекло время ожидания в канале приёма USART1", "Timeout read chanel USART1"}},
         {21, new[] {"UART Ошибка четности USART1", "UART Parity Error USART1"}},
         {22, new[] {"Data Overrun USART1", "Data Overrun USART1"}}, 
         {23, new[] {"ERR_DOR1_UPE1", "ERR_DOR1_UPE1"}}, 
         {24, new[] {"Frame Error USART1", "Frame Error USART1"}},
         {25, new[] {"ERR_FE1_UPE1", "ERR_FE1_UPE1"}}, 
         {26, new[] {"ERR_FE1_DOR1", "ERR_FE1_DOR1"}}, 
         {27, new[] {"ERR_FE1_DOR1_UPE1", "ERR_FE1_DOR1_UPE1"}},
         {28, new[] {"Истекло время ожидания в канале приёма USART2", "Time Out USART2"}},
         {29, new[] {"Data Overrun USART2", "Data Overrun USART2"}},
         {30, new[] {"ERR_DOR2_UPE2", "ERR_DOR2_UPE2"}},
         {31, new[] {"Ошибка кадра USART2", "Ошибка кадра USART2"}}, 
         {32, new[] {"ERR_FE2_UPE2", "ERR_FE2_UPE2"}}, 
         {33, new[] {"ERR_FE2_DO2", "ERR_FE2_DO2"}},
         {34, new[] {"ERR_FE2_DOR2_UPE2", "ERR_FE2_DOR2_UPE2"}}, 
         {35, new[] {"Истекло время ожидания в канале приёма USART3", "Time Out USART3"}},
         {36, new[] {"UART Ошибка четности USART3", "UART Parity Error USART3"}}, 
         {37, new[] {"Data Overrun USART3", "Data Overrun USART3"}},
         {38, new[] {"ERR_DOR3_UPE3", "ERR_DOR3_UPE3"}},
         {39, new[] {"Ошибка кадра USART3", "Frame Error USART3"}},
         {40, new[] {"ERR_FE3_UPE3", "ERR_FE3_UPE3"}}, 
         {41, new[] {"ERR_FE3_DOR3", "ERR_FE3_DOR3"}},
         {42, new[] {"ERR_FE3_DOR3_UPE3", "ERR_FE3_DOR3_UPE3"}}, 
         {43, new[] {"Переполнение буфера приёма USART0", "Over Load In buffer USART0"}},
         {44, new[] {"Переполнение буфера приёма USART1", "Over Load In buffer USART1"}},
         {45, new[] {"Переполнение буфера приёма USART2", "Over Load In buffer USART2"}},
         {46, new[] {"Переполнение буфера приёма USART3", "Over Load In buffer USART3"}}, 
         {47, new[] {"Переполнение буфера передачи USART0", "Over Load Out buffer USART0"}},
         {48, new[] {"Переполнение буфера передачи USART1", "Over Load Out buffer USART1"}}, 
         {49, new[] {"Переполнение буфера передачи USART2", "Over Load Out buffer USART2"}},
         {50, new[] {"Переполнение буфера передачи USART3", "Over Load Out buffer USART3"}}
      };

      #region Server Events

      /// <summary>
      /// дополнительный обработчик после коннекта, вызывается в ConnectToServer
      /// </summary>
      public event ConnectHandler AfterTryConnect;

      /// <summary>
      /// дополнительный обработчик после дисконнекта, вызывается в DisconnectFix 
      /// </summary>
      public event ConnectHandler AfterDisconnect;

      /// <summary>
      ///   дополнительный обработчик после дисконнекта, вызывается в DisconnectFix 
      /// </summary>
      public event ConnectHandler Disconnect;

      /// <summary>
      ///   специализированный коннект.
      ///   Если задан он, то используется он в ConnectToServer
      /// </summary>
      public event ConnectHandler Connect;

      /// <summary>
      ///  переопределенная функция
      /// </summary>
      public event ServerHandler ChangeDevicesStatus;

      /// <summary>
      ///   доп. функция освобождения серверных данных.
      /// </summary>
      public event ServerHandler FreeServerData;

      /// <summary>
      /// дополнительный обработчик входящего пакета для ParseIncommingPacketTM
      /// </summary>
      public event IncomingPacketHandler ParseIncomingPacket;

      /// <summary>
      /// указатель на функцию - дополнительный обработчик входящих команд.
      /// Вызывается после обработки данных, например,
      /// для доп.реакции по отображению данных использовать необязательно
      /// для PFS - параметры cmd и cid из пришедшей команды
      /// для остальных - тип пакета и вид данных вызывается из парсера
      ///  TMCPFS_PostParseData, Kr_PostParseData
      /// </summary>
      public event PostParseHandler PostParseData;

      /// <summary>
      ///  внешняя функция для формирования команды к устройству.
      ///  Напр. PFS_MakeCommand Termo_MakeCommand XRAY_MakeCommand TermoL_MakeCommand ACC_MakeCommand
      /// </summary>
      public event MakeCommandHandler MakeCommand;

      public Client Client = new Client();

      /// <summary>
      ///  уникальная начинка сервера в соответствии с типом
      /// </summary>
      public object ServerData;

      /*
           
      // для type = SERVER_TYPE_TEMPERATURE  ->  данные типа TermServerData*
      // для type = SERVER_TYPE_XRAY  ->  данные типа XrayData *xrd = (XrayData *) ds->ServerData;
 
         int (*F_SendTMDataPriv) ( void*pds, int cmd, uint sz, byte[] data ); // переопределен. Если есть, то вызывается он
         int (*F_FixHardwareError) ( void*pds, int pid, int err_code, byte err_status ); // внешняя функция фиксации аппаратной ошибки. Поддерживается в PFS_ParseResponce   - Kr_FixHardwareError

         int (*F_ParseResponce) ( byte[] rx_buff, int n, int cnt, void*p_dev
            ); // разбор ответа от контроллера. Может придти напрямую через ack_parser   
       * Напр. PFS_ParseResponce Termo_ParseResponce XRay_ParseResponce ACC_ParseResponce

      // должна в конце попытаться вызвать F_PostParseData
         int (*F_LogResponce ) ( byte[] rx_buff, int n, int cnt, void*p_dev, int cmd, int asknum ); // сброс ответа от контроллера в лог  Напр. PFS_LogResponce, MCSrv_LogResponce, 
         int (*F_LogRequest) ( byte[] rq_buff, int n, void*p_dev, int cmd, int asknum); // сброс запроса к контроллеру в лог  Напр. PFS_LogRequest MCSrv_LogRequest
         int (*F_FixResponceResult ) ( int res, void*p_dev ); // фиксация результата запроса к железу, не обязательная. PFSDS_FixResponceResult
         CtrlCallbackPtr F_update; // функция периодического обновления данных по таймеру
      */

      #endregion
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }
      protected virtual void Dispose(bool disposing)
      {
         if (disposing) {
            Client.Dispose();
         }
      }
   }
}
