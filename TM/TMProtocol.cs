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
      ///    текущее состояние сервера  клиент-сервер <see cref="TM.State2Pass" />
      /// </summary>
      [Description("текущее состояние сервера клиент<-сервер")]
      STATE = 2,

      /// <summary>
      ///    результат выполнения серии выстрелов клиент-сервер
      /// </summary>
      [Description("результат выполнения серии выстрелов клиент<-сервер")]
      SHOTSRESULTS = 3 // 
   }

   /// <summary>
   ///    состояния сервера в процессе исполнения плана
   /// </summary>
   public enum EPlanState
   {
      /// <summary>
      ///    не определенный
      /// </summary>
      [Description("не определенный")]
      UNKNOWN = 111, // 

      /// <summary>
      ///    не готов
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
   ///   динамическое состояние сервера
   /// </summary>
   public enum EProcessState
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
      ///    получен файл
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
      ///    проверять входящий размер пакета (значение поля PacketSizeLimit)
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
   public enum EConnectType
   {
      /// <summary>
      ///    заглушка
      /// </summary>
      [Description("заглушка")]
      NONE = 0, //

      /// <summary>
      ///    библиотека linklib.h by P.Lunev (ретранслятор на кресло,
      ///      температурные мастера, рентген-сервер(железо)...)
      /// </summary>
      [Description("библиотека linklib.h by P.Lunev (ретранслятор на кресло, температурные мастера, рентген-сервер(железо)...)")]
      LNKLIB = 1,

      /// <summary>
      ///    протокол TM_Protocol ( расширение TCP - температурный сервер-клиент,
      ///    рентген-сервер-томограф, прокси кресла )
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

   /// <summary>
   ///    Class PacketSignature.
   /// </summary>
   public static class PacketSignature
   {
  
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
