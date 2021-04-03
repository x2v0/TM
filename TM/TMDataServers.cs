
// $Id: $

/*************************************************************************
 *                                                                       *
 * Copyright (C) 2021,   Valeriy Onuchin                                 *
 * All rights reserved.                                                  *
 *                                                                       *
 *************************************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using TM;

namespace TMSrv
{

   /// <summary>
   ///    универсальный сервер - источник данных для подключения к разным источникам данных:
   ///    TCP серверам по протоколу TM, устройствам через линклиб или com порт
   /// </summary>
   public partial class TMDataServer
   {

   #region Static Methods
   /*
/// <summary>
/// для совместимости с linklib
/// Get Server ID by request code
/// </summary>
static int sIDr(short rqc) 
{ 
   return (rqc>>RS_SHFT) & RS_MASK;
}
*/

// Добавление нового сервера в набор
      public virtual int AddServer(string name, string ip, int port, EServerType type)
      {
         var ds = new TMDataServer();
         DataServers.Add(ds);

         ds.OnProcessing = 1;
         ds.type = type;
         ds.port = port;
         ds.name = name;
         ds.ip = ip;
         ds.ControlID = -1;
         //ds.panelHandle = -1;
         ds.lnk.sid = -1;
         ds.handle = -1;
         ds.DevStatus = EDeviceStatus.OFF;
         
         // выделяем память под специфические данные  
         // для температурного сервера
         //if( type == SERVER_TYPE_TEMPERATURE )
         //{
            // вынесенно в Termo_AddServer
         //}
         // для рентген - сервера
         //else if( type == SERVER_TYPE_XRAY )
         //{
            // вынесенно в XRay_AddServer
         //}
         // для сервера кресла
         //else if( type == SERVER_TYPE_PFS )
         //{
            //вынесено в PFS_AddServer
         //}
         // для сервера прокси-кресла
         //else if( type == SERVER_TYPE_PFS_PROXY )
         //{
            //вынесено в PFSProxy_AddServer
         //}
         // для сервера - программы-томографа
         //else 
         if (type == EServerType.TMC)
         {
            //ds.ServerData = tmAlloc( sizeof(PFS_data) ); 
            //memset( ds.ServerData, 0, sizeof(PFS_data) );

            ds.ConnectType = EServerConnectType.TMPROTOCOL;
            ds.ConnectAvailable = EServerConnectType.TMPROTOCOL;
         }
         // для сервера вакуума
         else if( type == EServerType.DACADC )
         {
            ds.ServerData = new DACADC_data();   
            //memset( ds.ServerData, 0, sizeof(DACADC_data) );
            ds.ConnectType = EServerConnectType.LNKLIB;
            ds.ConnectAvailable = EServerConnectType.LNKLIB;
            
            var dad= (DACADC_data) ds.ServerData;

            if( dad != null )  {
               dad.MAX_DAC = DACADC_MAX_DAC;
               dad.MAX_ADC = DACADC_MAX_ADC;
            }
         }
         // для ускоряющего сервера 
         //else if( type == SERVER_TYPE_ACC )
         //{
            //вынесено в ACC_AddServer
         //}
         // для конфигуратора ретранслятора
         else if (type == EServerType.NETRT) {
            ds.ConnectType = EServerConnectType.TMPROTOCOL;
            ds.ConnectAvailable = EServerConnectType.TMPROTOCOL;
         }
         //
         //else if( type == SERVER_TYPE_MCS )
         //{
            //вынесено в MCSrv_AddServer
         //}
         //
         //else if( type == SERVER_TYPE_ECS )
         //{
            //вынесено в ECSrv_AddServer
         //}
         //else if( type == SERVER_TYPE_DM )
         //{
            //вынесено в DMSrv_AddServer
         //}
 
         ds.OnProcessing = 0;
         return DataServers.Count;
      }

// останавливает таймер периодических опросов
int StopUpdateTimer()
{
   // если таймер уже был взведен - ликвидируем
   if(UpdateTimerID > 0) {
      //DiscardCtrl(panelHandle, UpdateTimerID);
      UpdateTimerID = -1;
      return 0;
   }
   return -1; // не был запущен
}

      /*
      // запускает таймер периодических опросов
      // до запуска должна быть инициализирована переменная ds.panelHandle
      // заводит таймер, который с периодичностью period секунд будет дергать функцию fupdate
      int TMDataServers_StartUpdateTimer( TMDataServer *ds, double period,  CtrlCallbackPtr fupdate )
      {
      ds.F_update = fupdate;

      // если таймер уже был взведен - ликвидируем
      TMDataServers_StopUpdateTimer( ds );

      // заводим новый таймер
      if((ds.UpdateTimerID = NewCtrl( ds.panelHandle ,CTRL_TIMER,"", 0,0)) < 0)
         return -1;

      if( 0 > InstallCtrlCallback ( ds.panelHandle, ds.UpdateTimerID, ds.F_update, ds) ) 
      {
         DiscardCtrl(ds.panelHandle, ds.UpdateTimerID);
         ds.UpdateTimerID = -1;
         return -2;
      }

      SetCtrlAttribute(ds.panelHandle, ds.UpdateTimerID,ATTR_INTERVAL, period);
      SetCtrlAttribute(ds.panelHandle, ds.UpdateTimerID,ATTR_ENABLED,1);
      ds.UpdatePeriod = period; 

      return 0;
      }

      // текущий статус сетвера
      //int TMDataServers_GetServerStatus( TMDataServer *ds )
      //{
      //return ds.DevStatus;
      //}



      // добавление визуального контрола для датчика
      //int type - тип контрола (напр CONTROL_TYPE_TEMPSINGLE ) - для каждого типа описаны его представление и источник данных
      //int masterCID - номер мастера (для Температурных датчиков)
      //int CID - (для Температурных датчиков)
      //int num - номер датчика в наборе (для полного набора - -1)
      //int count - количество элементов в полном наборе
      VC_Control *TMDataServers_AddVC_Control( TMDataServer *ds, int type, int masterCID, int CID, int num, int count )
      {
      // добавляем новый
      ds.Controls = tmReAlloc( ds.Controls, sizeof(VC_Control) *(ds.ControlsCount+1), sizeof(VC_Control)*ds.ControlsCount );
      if(ds.Controls == NULL) return NULL;

      VC_Control *vcc = &ds.Controls[ ds.ControlsCount ];
      vcc.type = type;
      vcc.masterCID = masterCID;
      vcc.CID = CID;
      vcc.num = num;
      vcc.count = count;
      vcc.device_status = DEVICE_STATUS_OFF;
      vcc.name[0] = '\0';
      vcc.control_val = 0.0;
      vcc.ControlID = -1;
      vcc.panelHandle = ds.panelHandle;
      vcc.view_mode = 0;

      if( vcc.type == CONTROL_TYPE_TEMPSET || vcc.type == CONTROL_TYPE_TEMPSINGLE )
         vcc.term_correct = tmAllocEmpty( sizeof(int)*TMPR_DEVICES_SENSE_COUNT ); //коррекция температур
      else
         vcc.term_correct = NULL;

      ds.ControlsCount++;

      return &ds.Controls[ ds.ControlsCount-1 ];
      }

      // удаление датчика по его номеру
      int TMDataServers_DelVC_Control( TMDataServer *ds, int num )
      {
      if( num < 0 || num >= ds.ControlsCount ) return -1;

      if( ds.Controls[num].term_correct != NULL ) tmFree( ds.Controls[num].term_correct );

      if( num < ds.ControlsCount-1 )
      {
         memcpy( &ds.Controls[num],  &ds.Controls[num+1], sizeof(VC_Control)*(ds.ControlsCount-num-1));
      }
      ds.ControlsCount--;
      return 0;
      }

      // удалить сервер из коллекции по номеру
      int TMDataServers_Del( int num )
      {
      TMDataServer *ds;         // сервер

      if( num <0 || num>=TMDataServerCount ) return -1;
      ds = TMDataServers[num];  

      // подчищаем список
      for(int i=num; i<TMDataServerCount-1; i++)
      {
         TMDataServers[i] = TMDataServers[i+1];
      }
      ds.NeedForReconnect = 0;
      TMDataServerCount--;

      // отключаемся от источника данных
      TMDataServers_DisconnectFromServer(ds);  

      // если таймер уже был взведен - ликвидируем
      if( ds.UpdateTimerID > 0) 
      {
         DiscardCtrl(ds.panelHandle, ds.UpdateTimerID);
         ds.UpdateTimerID = -1;
      }

      // для сервера температур, например, чистим сложную структуру данных
      if( ds.F_FreeServerData != NULL )
         (*ds.F_FreeServerData)(ds);

      if(ds.ServerData !=NULL) tmFree( ds.ServerData );
      if(ds.Controls != NULL) tmFree( ds.Controls );

      TmQueue_DropAll ( &ds.lnk.asks, 1 );
      tmFree( ds );
      return 0;
      }

      // удалить сервер из коллекции по указателю на элемент коллекции
      int DeleteDataServer(TMDataServer ds)
      {
      foreach(var datasever in DataServers)
      {
         if( TMDataServers[i] == ds ) 
            return TMDataServers_Del(i);
      }
      return -1;
      }

      // удаление всех серверов
      int DeleteAll()
      {
      if (DataServers == null ) return -1;

      foreach(var ds in DataServers)
      { 

      return 0;
      }

      // количество серверов
      int GetServerCount()
      {
         return DataServers.Count;
      }

      // один сервер из коллекции по номеру
      TMDataServer GetServer(int num)
      {
      if( num <0 || num>=TMDataServerCount ) return NULL;

      return TMDataServers[num];   
      }

      //поиск сервера по контролу его или датчика
      TMDataServer *TMDataServers_GetServerByCntlId(int id)
      {
      TMDataServer *ds;
      for(int i=0; i<TMDataServerCount; i++)
      {
         ds = TMDataServers[i];

         // посмотрим среди контролов коннекта
         if( ds.ControlID == id )
            return ds;

         // посмотрим среди прочих контролов
         for( int j=0; j<ds.ControlsCount; j++ )
         {
            if( ds.Controls[ j ].ControlID == id )
            {
               return ds;
            }
         }
      }
      return NULL;
      }

      VC_Control GetCntlById(int id)
      {
      for(int i=0; i<TMDataServerCount; i++)
      {
         TMDataServer *ds = TMDataServers[i];
         for( int j=0; j<ds.ControlsCount; j++ )
         {
            if( ds.Controls[ j ].ControlID == id )
            {
               return &ds.Controls[ j ];
            }
         }
      }
      return 0;
      }

      // полное имя контрола для отображения в режиме конфигурации
      // вызывается из CreateVCControl
      char *GetPicControlName(VC_Control *vcc, char *buf)
      {
      switch( vcc.type )
      {
         case CONTROL_TYPE_FARADAY_POS: // 
            sprintf( buf, "F");
         break;
         default:
            return GetDefaultControlName( vcc, buf );
      }
      return buf;
      }


      // полное имя контрола для отображения в списке датчиков
      //см. также GetCntrlTemplaiteByType( int vcc_type )
      // вызывается из GetPicControlName
      // вызывается из SetCurrentServer
      char *GetDefaultControlName(VC_Control *vcc, char *buf)
      {
      switch( vcc.type )
      {
         case CONTROL_TYPE_TEMPSET:
            sprintf( buf, "T%d M%02d ID:%02d", vcc.count, vcc.masterCID, vcc.CID);
         break;
         case CONTROL_TYPE_TEMPSINGLE:
            sprintf( buf, "T%d/%d M%02d ID:%02d", vcc.num+1, vcc.count, vcc.masterCID, vcc.CID);
         break;
         case CONTROL_TYPE_TEMPSTATUS:
            sprintf( buf, "Status M:%2d", vcc.masterCID);//, vcc.CID);
         break;
         case CONTROL_TYPE_DOOR:
            sprintf( buf, "Door open|close");//, vcc.CID);
         break;
         case CONTROL_TYPE_IRRADIATION:
            sprintf( buf, "Radiation");//, vcc.CID);
         break;
         case CONTROL_TYPE_ATTENTION:
            sprintf( buf, "Block Status");//, vcc.CID);
         break;
         case CONTROL_TYPE_WATERFLOW:
            sprintf( buf, "Water Flow");//, vcc.CID);
         break;
         case CONTROL_TYPE_XRAY:
            sprintf( buf, "XRay");//, vcc.CID);
         break;
         case CONTROL_TYPE_XRAYMODE:
            sprintf( buf, "XRay Mode");//, vcc.CID);
         break;
         case CONTROL_TYPE_ANGLE_IENC:
            sprintf( buf, "Angle IEnc");//, vcc.CID);
         break;
         case CONTROL_TYPE_ANGLE:
            sprintf( buf, "Angle");//, vcc.CID);
         break;
         case CONTROL_TYPE_ALTITUDE:
            sprintf( buf, "Altitude");//, vcc.CID);
         break;
         case CONTROL_TYPE_MOVING:
            sprintf( buf, "Moving");//, vcc.CID);
         break;
         case CONTROL_TYPE_DETECTOR:
            sprintf( buf, "Detector");//, vcc.CID);
         break;
         case CONTROL_TYPE_PFS_DETECTORLR:
            sprintf( buf, "L<.R");//, vcc.CID);
         break;
         case CONTROL_TYPE_ANGLE_PIC:
            sprintf( buf, "Angle");//, vcc.CID);
         break;
         case CONTROL_TYPE_TEXTLABEL:
            sprintf( buf, "Text Label");//, vcc.CID);
         break;
         case CONTROL_TYPE_XR_IHV:
            sprintf( buf, "I_tube");
         break;
         case CONTROL_TYPE_XR_UHV:
            sprintf( buf, "U_tube");
         break;
         case CONTROL_TYPE_XR_IVAC:
            sprintf( buf, "I_Vac");
         break;
         case CONTROL_TYPE_XR_HEAT:
            sprintf( buf, "Heat");
         break;
         case CONTROL_TYPE_XR_HV:
            sprintf( buf, "HV");
         break;
         case CONTROL_TYPE_PFS_HAND:
            sprintf( buf, "Hand Mode");
         break;
         case CONTROL_TYPE_PFS_HORMOVE:
            sprintf( buf, "Hor Move");
         break;
         case CONTROL_TYPE_DAC:
            sprintf( buf, "DAC %d", vcc.num);
         break;
         case CONTROL_TYPE_ADC:
            sprintf( buf, "ADC %d", vcc.num);
         break;
         case CONTROL_TYPE_UPDATE_NUM:
            sprintf( buf, "UPD");
         break;
         case CONTROL_TYPE_STATUS: // круглая лампочка статуса
            sprintf( buf, "Status");
         break;
         //case CONTROL_TYPE_FARADEY_CUP:
         // sprintf( buf, "Faradey Cup Position");
         //break;
         case CONTROL_TYPE_FARADAY_STATUS: // прямоугольная лампочка статуса
            sprintf( buf, "Faraday Status");
         break;
         case CONTROL_TYPE_FARADAY_POS: // 
            sprintf( buf, "Faraday Pos");
         break;
         case CONTROL_TYPE_FARADAY_LOCAL:// прямоугольная лампочка статуса
            sprintf( buf, "Faraday Local");
         break;
         case CONTROL_TYPE_PFS_MODE:   // текстовое сообщение
            sprintf( buf, "Mode");
         break;
         case CONTROL_TYPE_HOST:       // host - иконка с компьютером
            sprintf( buf, "Host");
         break;
         case CONTROL_TYPE_XR_SHOTMODE:   // текущий режим сервера XRay - номер набора настроек
            sprintf( buf, "ShotMode");
         break;
         case CONTROL_TYPE_MODE_TXT:      // текущий режим сервера ECS - ECS_MODE_READY
            sprintf( buf, "Mode");
         break;
         default:
            buf[0] = '\0';
         break;
      }

      if( strlen( vcc.name ) > 0 )
      {
         strcat( buf, " (" );
         strcat( buf, vcc.name );
         strcat( buf, " )" );
      }
      //if( vcc.device_status == DEVICE_STATUS_OFF )
      // strcat( 
      return buf;

      }

      // разбор входящего по сети TM_Protocol пакета и реакция на вошедшие данные
      // вызывается из ClientTCPCB
      void ClientTCP_ParseIncommingPacketTM(TMDataServer *ds, int handle)
      {
      Packet *p;
      unsigned char *pdata=NULL;
      int long_packet_flag = 0;
       unsigned char buf[TM_START_BUFFER_SIZE] = {0};
      ssize_t dataSize = sizeof (Packet);

      if ((dataSize = ClientTCPRead (handle, buf, dataSize, 1000)) < 0)
      {                                                                              
         if( configParams.trace_flag) mprntf_t("Error incomming packet reading");
         return;
      }                                                                              

      if( ds == NULL) return;
      if( configParams.trace_flag )                                                            
            TraceBytes("Net Incomming 0x:", buf, (int)dataSize);

      // для протокола TM - проверяем 
      // формируем пришедшие данные в структуру
      int tmpksz = (int)sizeof(Packet); 
      if( dataSize < tmpksz )
         return;

      p = (Packet *) buf;
      // анализируем заголовок пакета
      int srvType = Packet_CheckMarker( p );
      if( srvType == SERVER_TYPE_UNKNOWN ) // вообще не тот протокол 
      {
         if( configParams.trace_flag)
            mprntf_t("Packet received from %s is wrong TM packet", ds.name);
         ds.DevStatus = DEVICE_STATUS_CRACKED;
         return;
      }

      if( srvType != ds.type && p.type != Packet_TYPE_INFORMATION ) // сервер другого типа - обрабатываем только текстовые сообщения
         return;

      if( p.datalength > 0 ) // дочитываем хвост пакета
      {
         if( p.datalength < TM_START_BUFFER_SIZE-sizeof(Packet))
            pdata = buf+sizeof(Packet);
         else
         {
            pdata = tmAlloc(p.datalength);
            long_packet_flag = 1;
            if( pdata == NULL ) return;
         }

         int szread;
         while( dataSize < p.datalength)
         {
            szread = ClientTCPRead(handle, pdata, p.datalength, 1000);
            if (szread < 0)
            {                                                                              
               if( configParams.trace_flag )
                  mprntf_t( "Reading packetdata from %s error", ds.name );
               return;
            } 
            dataSize += szread;
         }

         // сохраняем при необходимости запрос в лог
         if( ds.F_LogResponce )  //.MCSrv_LogResponce
            (*ds.F_LogResponce) ( pdata, 0, p.datalength, ds, 0, 0 );
      }

      // запускаем дополнительный обработчик, если он есть
      if( ds.F_ParseIncommingPacketPriv != NULL )
            (*ds.F_ParseIncommingPacketPriv) ( ds, p, pdata );      //Termo_ParseImcommingPacket, XRay_ParseImcommingPacket, PFSProxy_ParseImcommingPacket, TmCSrv_ParseImcommingPacket  TmNS_ParseImcommingPacket NetRTcfg_ParseImcommingPacket

      if( long_packet_flag ) tmFree(pdata);
      }

      // разбор входящего по сети пакета и реакция на вошедшие данные, протокол не TM_Protocol
      // вызывается из ClientTCPCB
      void ClientTCP_ParseIncommingPacketNTM(TMDataServer *ds, int handle)
      {
      Packet *p=NULL;
      unsigned char *pdata=NULL;
       unsigned char buf[10240] = {0};

      ssize_t dataSize = sizeof (buf);
      if( ds.ntm.waitpacketsize ) dataSize = ds.ntm.waitpacketsize;

      if ((dataSize = ClientTCPRead (handle, buf, dataSize, 1000)) < 0)
      {                                                                              
         if( configParams.trace_flag) mprntf_t("Error incomming packet reading");
         return;
      }                                                                              

      if( ds == NULL) return;
      if( configParams.trace_flag )                                                            
            TraceBytes("Net Incomming 0x:", buf, (int)dataSize);

      if( ds.ConnectType == SERVER_ConnectType_TCPCUSTOM )  // подключение TCP CUSTOM, пакет разбирается вольно
      {                                              // заголовок генерим формально
         p = (Packet *) tmAllocEmpty(sizeof(Packet));
         p.datalength = (unsigned int) dataSize;
         pdata = buf;
      }
      else 
         return;  //неизвестно как его разбирать вообще

      // сохраняем при необходимости запрос в лог
      if( ds.F_LogResponce )  //.MCSrv_LogResponce
         (*ds.F_LogResponce) ( pdata, 0, p.datalength, ds, 0, 0 );

      // запускаем дополнительный обработчик, если он есть
      if( ds.F_ParseIncommingPacketPriv != NULL )
            (*ds.F_ParseIncommingPacketPriv) ( ds, p, pdata );      //Termo_ParseImcommingPacket, XRay_ParseImcommingPacket, PFSProxy_ParseImcommingPacket, TmCSrv_ParseImcommingPacket  TmNS_ParseImcommingPacket NetRTcfg_ParseImcommingPacket

      tmFree( p );
      }

      // обновление состояния всех контролов на основании данных в ServerData
      void TMDataServers_UpdateDevicesStatus(void)
      {
      for(int i=0; i<TMDataServerCount; i++)
      {
         TMDataServer *ds = TMDataServers[i];
         if( ds.F_ChangeDevicesStatus != 0 )  // вызываем дополнительный клиентский обработчик, если он задан
            (*ds.F_ChangeDevicesStatus ) ( ds );

      }
      }     

      // пересчитываем состояние датчиков вакуум сервера
      // вызывается из ParseImcommingVacSrv
      // вызывается из TMDataServers_UpdateDevicesStatus
      // статус отображается в TMDataServer_ShowDevicesStatus
      void ChangeVacDevicesStatus( TMDataServer *ds )
      {

      }

      // отправка пакета по TCP
      // тоько для подключения типа SERVER_ConnectType_TCPCUSTOM
      int TMDataServers_SendCustomToServer(TMDataServer *ds, void *data, size_t size)
      {
      if( ds == NULL ) return -1;
      if( ds.ConnectType != SERVER_ConnectType_TCPCUSTOM ) return -2;

      if( ds.F_LogRequest ) // процедура сохранения запроса в лог, например MCSrv_LogRequest
         (*ds.F_LogRequest) ( data, (int)size, ds, 0, 0);

      if( ClientTCPWrite( ds.handle, data, size, 1000) < 0)                         
          return -1;
                                                                
      return 0;
      }

      // увеличение циклического счетчика отправленных команд
      int  TMDataServers_GetNextAskNum( TMDataServer *ds )
      {
      int num = ds.lnk.asknum++;
      if( ds.lnk.asknum > 999 ) ds.lnk.asknum = 0;

      return num;
      }

      // отправка команды на сервер по TM протоколу
      int  TMDataServers_SendTMCommand( TMDataServer *ds, int cmd )
      {
      int res;
      if( ds == NULL ) return -1;
      if( ds.ConnectType != SERVER_ConnectType_TMPROTOCOL ) return -2;

      //switch( cmd )
      //{
      // default:
            //mprntf("send %d", cmd);
      res = SendCommandToServer( ds.type, ds.handle, cmd, DATA_LENGTH_0, DATA_STRUCT_NULL,  TMDataServers_GetNextAskNum(ds), ds.TMSettings );  
      if( res != 0 )
         ds.DevStatus = DEVICE_STATUS_OFF;
      // break;
      //}
      return res;
      }

      // отправка команды на сервер по TM протоколу, c дополнительными данными
      int  TMDataServers_SendTMCommandD( TMDataServer *ds, int cmd, size_t sz, unsigned char *data  )
      {
      if( ds == NULL ) return -1;
      if( ds.ConnectType != SERVER_ConnectType_TMPROTOCOL ) return -2;

      SendCommandToServer( ds.type, ds.handle, cmd, sz, data,  TMDataServers_GetNextAskNum(ds), ds.TMSettings ); 

      return 0;
      }

      // отправка данных на сервер по TM протоколу
      int  TMDataServers_SendTMData( TMDataServer *ds, int cmd, size_t sz, unsigned char *data )
      {
      //unsigned char *data;
      //size_t sz;

      if( ds == NULL ) return -1;
      if( ds.ConnectType != SERVER_ConnectType_TMPROTOCOL ) return -2;


      if( ds.F_SendTMDataPriv != NULL )
         return (*ds.F_SendTMDataPriv)(ds, cmd, sz, data);    //XRay_SendTMData для TmC
      else
         return SendDataToServer( ds.type, ds.handle, cmd, sz, data, TMDataServers_GetNextAskNum(ds), ds.TMSettings);
      }

      // отправка данных - текстовой строки - на сервер по TM протоколу
      int  TMDataServers_SendTMInfo( TMDataServer *ds, int cmd, size_t sz, unsigned char *data )
      {
      if( ds == NULL ) return -1;
      if( ds.ConnectType != SERVER_ConnectType_TMPROTOCOL ) return -2;

      return SendInfoToServer( ds.type, ds.handle, cmd, sz, data, TMDataServers_GetNextAskNum(ds), ds.TMSettings);
      }

      // отображаем цветом состояние датчиков.
      // статус меняется в ChangeTermometersStatus или ChangeXRayDevicesStatus
      void TMDataServers_ShowServersDevicesStatus( void )
      {
      TMDataServer *ds;

      //перебираем все сервера
      for(int i=0; i<TMDataServerCount; i++)
      {
         if( configParams.StopAll ) return;
         ds = TMDataServers[i];
         //  перебираем все датчики
         TMDataServers_ShowDevicesStatus( ds );
      } // по всем серверам
      }


      // отображение состояния датчика - цветом диода или отображением картинки
      // изменяется состояние в Termo_ChangeDevicesStatus, XRay_ChangeDevicesStatus, PFS_ChangeDevicesStatus
      void TMDataServers_ShowDevicesStatus( TMDataServer *ds )
      {
      int color, old_color, val = -1, old_val = -1;
      VC_Control *vcc;  
      //  перебираем все датчики
      for(int j=0; j<ds.ControlsCount; j++)
      {
         if( configParams.StopAll ) return;
         vcc = &ds.Controls[j];
         if( vcc.ControlID < 0 ) continue;
         if( vcc.panelHandle< 0 ) continue;
         int type = vcc.type;

         // отображаем цветом состояние датчиков.
         // считаем данные устаревшими, если прошло более 10 минут
         // для контролов-картинок - показываем статус переключением картинки
         switch( type )
         {
            case CONTROL_TYPE_DOOR:  // датчик открытой двери по рентгену
            case CONTROL_TYPE_PFS_HAND:// статус - ручное управление креслом
               //DEVICE_STATUS_OFF 
               if( vcc.device_status == DEVICE_STATUS_OPEN )
                     val = 1;
               else if( vcc.device_status == DEVICE_STATUS_OK )
                     val = 2;
               else val = 0;
            break;
            case CONTROL_TYPE_HOST:    // хост для пинга
               if( vcc.device_status == DEVICE_STATUS_OK )
                     val = 1;
               else if( vcc.device_status == DEVICE_STATUS_ATTENTION )
                     val = 2;
               else val = 0;
            break;
            case CONTROL_TYPE_ATTENTION: 
            case CONTROL_TYPE_TEMPSTATUS:    // статус блокировки, статус потока рентгена
            case CONTROL_TYPE_XRAY:       // излучение сервера рентгеновской установки 
               if( vcc.device_status == DEVICE_STATUS_ATTENTION )
                     val = 1;
               else if( vcc.device_status == DEVICE_STATUS_HOT )
                     val = 2;
               else val = 0;
               if( type == CONTROL_TYPE_TEMPSTATUS )  // статус температурного сервера
               {
                  // мигание меткой, если были блокировки
                  GetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_LABEL_BGCOLOR, &old_color);                 
                  color = old_color;
                  if( vcc.control_val > 0 ) // ранее были блокировки
                  {                                                                              
                     if( configParams.LightSwitcher )
                        color = device_status_color1[ DEVICE_STATUS_HOT ];
                     else
                        color = device_status_color2[ DEVICE_STATUS_HOT ];
                  }
                  else color = 0xECE9D8;  // серенький

                  if( old_color != color )
                     SetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_LABEL_BGCOLOR, color);                
               }
            break;
            // лампочка, отображающая статус цветом
            case CONTROL_TYPE_STATUS:        // статус устройства, лампочка
            case CONTROL_TYPE_TEMPSET:       // мигание цветом в зависимости от статуса датчика
            case CONTROL_TYPE_TEMPSINGLE:   // датчики температуры, датчики состояния, 
            case CONTROL_TYPE_WATERFLOW: 
            case CONTROL_TYPE_MOVING: 
            case CONTROL_TYPE_XR_HV: 
            case CONTROL_TYPE_PFS_STATUS:
            case CONTROL_TYPE_FARADAY_STATUS:
            case CONTROL_TYPE_FARADAY_LOCAL:
               GetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_ON_COLOR, &old_color);                
               color = old_color;
               if( vcc.device_status != DEVICE_STATUS_OFF) 
               {                                                                              
                  if( configParams.LightSwitcher )
                     color = device_status_color1[ vcc.device_status ];
                  else
                     color = device_status_color2[ vcc.device_status ];

                  val = 1;
               }
               else
               {
                  val = 0;    
               }

               if( old_color != color )
                  SetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_ON_COLOR, color);                  
            break;
            // круглая CMD кнопка, цветом отображает статус
            case CONTROL_TYPE_FARADAY_POS: 
               GetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_CMD_BUTTON_COLOR, &old_color);                 
               color = old_color;
               if( vcc.device_status != DEVICE_STATUS_OFF) 
               {                                                                              
                  if( configParams.LightSwitcher )
                     color = device_status_color1[ vcc.device_status ];
                  else
                     color = device_status_color2[ vcc.device_status ];

                  val = 1;
               }
               else
               {
                  val = 0;    
               }

               if( old_color != color )
                  SetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_CMD_BUTTON_COLOR, color);                
            break;
             // контрол - цифровой индикатор, float
            case CONTROL_TYPE_PFS_HORMOVE:
            case CONTROL_TYPE_ANGLE:
            case CONTROL_TYPE_ALTITUDE:  
            case CONTROL_TYPE_XR_IHV:
            case CONTROL_TYPE_XR_UHV:
            case CONTROL_TYPE_XR_IVAC:
               SetCtrlVal ( vcc.panelHandle, vcc.ControlID, vcc.control_val );  // float
               continue;
            // контролы - картинки, индикаторы разного вида, тип Int
            case CONTROL_TYPE_ANGLE_IENC: // поворот - внутренний энкодер
            case CONTROL_TYPE_DETECTOR:   // детектор - положение
            case CONTROL_TYPE_XR_HEAT:    // рентген - накал            
               val = (int) vcc.control_val;
            break;
            case CONTROL_TYPE_ANGLE_PIC:  // картинка поворота кресла
               int bitmapID;
                unsigned char   *bits, *mask;
               int *colorTable, bytesPerRow, pixelDepth, width, height;
               int rotated = 0; // флаг - повернуто плавно

               // вытаскиваем картинку с положением 0
               GetCtrlBitmap(vcc.panelHandle, vcc.ControlID, 1, &bitmapID ); 
                  AllocBitmapData (bitmapID, &colorTable, &bits, &mask);
                  GetBitmapData (bitmapID, &bytesPerRow, &pixelDepth, &width, &height, colorTable, bits, mask);
               // пробуем повернуть строго на нужный угол
               if( pixelDepth == sizeof(int)*8 )
               {
                  int *bou = tmAllocEmpty( sizeof(int)*width*height );
                  if( bou )
                  {
                     RotateBMPImage( (int*)bits, bou, width, height, width/2, height/2, grad2rad(vcc.control_val) );            
                     SetBitmapData( bitmapID, bytesPerRow, pixelDepth, colorTable, (unsigned char *)bou, mask );
                     SetCtrlBitmap(vcc.panelHandle, vcc.ControlID, 2, bitmapID ); // сюда будем повёрнутую картинку класть
                     val = 1; // значение для картинки с индексом 2
                     //memcpy( bits, bou, sizeof(float)*width*height );
                     tmFree( bou ); bou = NULL;
                     rotated = TRUE;
                  }
               }
                  DiscardBitmap (bitmapID);
                  free (colorTable);
                  free (bits);
                  free (mask);
               if( rotated ) break; // уже повернули за счет поворота картинки. Если нет, то покажется ближайшая картинка из набора 8 положений

               val = (int)((vcc.control_val+22.5) / 45 );
               if( val < 0 || val > 7 ) val = 0;
               break;
            case CONTROL_TYPE_PFS_DETECTORLR: // индикатор горизонтального положения детектора  "L<.R"
               //отображаем текст, соответствующий текущему статусу
               if( vcc.device_status == DEVICE_STATUS_CRACKED || vcc.device_status == DEVICE_STATUS_OFF )
                  SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "L<.R" );
               else if( vcc.device_status == DEVICE_STATUS_MOVING )
                  if( vcc.control_val == 2.0 )    SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "L<-R" );
                  else                    SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "L.R" );
               else
                  if( vcc.control_val == 1.0 )    SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "L<-" );
                  else                    SetCtrlVal ( vcc.panelHandle, vcc.ControlID, ".R" );

               // отображаем цвет
               if( vcc.device_status != DEVICE_STATUS_OK) 
               {                                                                              
                  if( configParams.LightSwitcher )
                     color = device_status_color1[ vcc.device_status ];
                  else
                     color = device_status_color2[ vcc.device_status ];
               }
               else color = VAL_BLACK;    

               GetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_TEXT_COLOR, &old_color);                 
               if( old_color != color )
                  SetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_TEXT_COLOR, color);                
               continue;
            case CONTROL_TYPE_UPDATE_NUM:    // номер апдейта сервера
            case CONTROL_TYPE_XR_SHOTMODE:  // текущий режим сервера XRay - номер набора настроек
               SetCtrlVal ( vcc.panelHandle, vcc.ControlID, (unsigned short int) vcc.control_val );  // unsigned short int
               continue;
            case CONTROL_TYPE_PFS_MODE: // кресло  - текущий режим терапии - текстовая подпись
               switch( (int)vcc.control_val )
               {
                  case 1:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Chair" );  
                  break;
                  case 2:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Stand" );  
                  break;
                  case 3:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Bench" );  
                  break;
                  default:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Mode??" );  
                  break;
               }
               continue;
            //break;       
            case CONTROL_TYPE_XRAYMODE:      // текущий режим сервера XRay
               switch( (int)vcc.control_val )
               {
                  case 21:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "On" );  
                  break;
                  case 22:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Warning" );  
                  break;
                  case 23:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Ready" );  
                  break;
                  case 24:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Start Light" );  
                  break;
                  case 25:
                  case 26:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Light" );  
                  break;
                  case 27:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Standby" );  
                  break;
                  case 255:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Off" );  
                  break;
                  default:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Unknown" );  
                  break;
               }
               continue;
            case CONTROL_TYPE_MODE_TXT:      // текущий режим сервера ECS - ECS_MODE_READY
               if( ds.type == SERVER_TYPE_ECS )
               switch( (int)vcc.control_val )
               {
                  case 0:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Not ready" );  
                  break;
                  case 1:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Ready" );  
                  break;
                  case 2:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Process" );  
                  break;
                  case 3:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Finished" );  
                  break;
                  default:
                     SetCtrlVal ( vcc.panelHandle, vcc.ControlID, "Unknown" );  
                  break;
               }
               continue;
            //break;       
            default:  // текстовая метка или еще что-то необрабатываемое
               continue;
         } // switch type

         GetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_CTRL_VAL, &old_val);                  
         if( old_val != val )
            SetCtrlVal ( vcc.panelHandle, vcc.ControlID, val );
      }// по всем датчикам
      }

      // фиксация отсоединения от сервера
      // вызывается из DisconSrv для  linklib
      // вызывается из ClientTCPCB для TM_Protocol
      void DisconnectFix( TMDataServer *ds )
      {
      if( ds == NULL ) { mprntf_t("Fix Unknown Server disconnection"); return; }

       mprntf_t("Connection with %s has closed", ds.name);                      
      if( ds.panelHandle > 0 && ds.ControlID > 0)     // гасим лампочку, если она подключена
          SetCtrlVal ( ds.panelHandle, ds.ControlID, 0);                                   
       ds.handle = -1;
       ds.lnk.sid = -1;      // только для подключения linklib: sid
      ds.DevStatus = DEVICE_STATUS_OFF;
      ds.COM.PortOpen = 0;

      if( ds.F_AfterDisconnect != NULL ) // дополнительный обработчик после коннекта
         (*ds.F_AfterDisconnect) (ds);
      }

      // дисконнект при соединении через LNKLIB
      void DisconSrv(int sid)
      {
      for(int i=0; i<TMDataServerCount; i++)
      {
         TMDataServer *ds = TMDataServers[i];
         if( ds.lnk.sid == sid )
            DisconnectFix( ds );
      }
      }

      void ErrReceive(int sid,int errcod){
      mprntf_t("Server %d Received Error (%d)",sid,errcod);
      }

      // обработка соединения с сервером на клиенте
      // колбэк по котовности данных или отцеплении клиента
      // фиксируется при создании соединения в TMDataServers_ConnectToServer  при типе соединения SERVER_ConnectType_TMPROTOCOL
      int CVICALLBACK ClientTCPCB (unsigned handle, int event, int error,                            
                                void *callbackData)                                               
      {                                                                                              
      TMDataServer *ds;

       switch (event)                                                                             
           {                                                                                      
           case TCP_DATAREADY: // сервер прислал данные

            ds = TMDataServers_GetServerByHandle(handle);
            if( ds !=NULL && ds.ConnectType == SERVER_ConnectType_TMPROTOCOL)
               ClientTCP_ParseIncommingPacketTM(ds, handle);
            else
               ClientTCP_ParseIncommingPacketNTM(ds, handle);

            break;                                                                             
           case TCP_DISCONNECT:  // сервер отцепился
            ds = TMDataServers_GetServerByHandle( handle );
            DisconnectFix( ds );
               break;                                                                             
       }
       return 0;                                                                                  
      }                                                                                              

      // подключение к серверу по TCP, линклибу или открытие порта
      // возвращает TMDataServer_CONNECTED = 0 в случае успеха
      int ConnectToServer(TMDataServer ds)
      {
      int oldErrState = SetBreakOnLibraryErrors(0);
      int res = -1;
      if( ds == NULL ) { res =  -1; goto ex; }  // нет источника данных

      if( ds.F_Connect ) 
         res = (*ds.F_Connect) (ds);  // специализированный коннект к устройству напр. Autonics_autpmc_Connect
      // подключаемся согласно указанному типу подключения
      else if( ds.ConnectType == SERVER_ConnectType_LNKLIB )
      {
         mprntf_t("LinkLib connect to %s (%s:%d)", ds.name, ds.ip, ds.port);
         //sRq = TMDataServers_GetServerNum( ds ); // для уникальности номера
         switch( ds.type )
         {
            case SERVER_TYPE_ACC:
               ds.lnk.RQ = R2_RSDEV;
            break;
            case SERVER_TYPE_TEMPERATURE:
               ds.lnk.RQ = R2_RSDEV; //RQ_ZERO; //R2_ZERO;
            break;
            case SERVER_TYPE_PFS:
               ds.lnk.RQ = R2_RSDEV;//R2_ZERO;
            break;
            case SERVER_TYPE_XRAY:
               ds.lnk.RQ = R2_RSDEV;
            break;
            case SERVER_TYPE_TERMLITE:
               ds.lnk.RQ = R2_RSDEV; //RQ_ZERO; //R2_ZERO;
            break;
            case SERVER_TYPE_NETSCAN:  // для генератора дыхания пусть будет
               ds.lnk.RQ = R2_RSDEV; //RQ_ZERO; //R2_ZERO;
            break;
            case SERVER_TYPE_CAMSRV:   // камеры CamSrv 
               ds.lnk.RQ = R4_ZERO;
            break;
            default:
               ds.lnk.RQ = R2_RSDEV; //RQ_ZERO; //R2_ZERO;
               //mprntf_t("Connect: Unknown Server Type for linklib");
               //res =  -2; goto ex;
            break;
         }

         res = ConToSrv( ds.panelHandle, ds.lnk.RQ, ds.ip, ds.port, DisconSrv, ErrReceive);
         if( res == 0 ) {
            int sid = sIDr( ds.lnk.RQ );
               ds.handle = getHConver( sid );
               ds.lnk.sid = sid;
         }
      }
      else if( ds.ConnectType == SERVER_ConnectType_TMPROTOCOL )
      {
         mprntf_t("TmConnect to: %s (%s:%d)", ds.name, ds.ip, ds.port);
         res = ConnectToTCPServer ( (unsigned int *)&ds.handle, ds.port, ds.ip, ClientTCPCB, NULL, 1000);
      }
      else if(  ds.ConnectType == SERVER_ConnectType_DIRECT || ds.ConnectType == SERVER_ConnectType_RS232 )  // прямое подключение к ком-порту
      {
         mprntf_t("Сonnect to: %s (COM %d)", ds.name, ds.COM.comport);
         res = OpenComPort( &ds.COM );
      }
      else if(  ds.ConnectType == SERVER_ConnectType_IMITATION )  // имитация подключения к устройству - всегда успешное. Для программ-имитаторов
      {
         mprntf_t("Connect imitation to: %s", ds.name);
         ds.COM.PortOpen = 1; // используем как флаг успешного подключения
         res = TMDataServer_CONNECTED; // 0 - всегда успех
      }
      else if( ds.ConnectType == SERVER_ConnectType_TCPCUSTOM )
      {
         mprntf_t("TCP Connect to: %s (%s:%d)", ds.name, ds.ip, ds.port);
         res = ConnectToTCPServer ( (unsigned int *)&ds.handle, ds.port, ds.ip, ClientTCPCB, NULL, 2000);
      }
      else
      {
         mprntf_t("Unknown connection protocol type");
         ds.NeedForReconnect = 0;
         { res =  -3; goto ex; }
      }

      // анализируем результат подключения
      if (res != 0)                                                
      {
         mprntf_t("Connection to Server %s faled! (%d)", ds.name, res);
         if( ds.panelHandle >= 0 && ds.ControlID >= 0)
            SetCtrlVal (ds.panelHandle, ds.ControlID, 0);
         ds.handle = -1;
         ds.lnk.sid = -1;
      }
      else
      {
         mprntf_t("Connection to Server %s OK!", ds.name);
         if( ds.panelHandle >= 0 && ds.ControlID >= 0)
            SetCtrlVal (ds.panelHandle, ds.ControlID, 1);                                   
      } 

      ex:
      SetBreakOnLibraryErrors (oldErrState);
      if( ds && ds.F_AfterTryConnect != NULL ) // дополнительный обработчик после коннекта
         (*ds.F_AfterTryConnect) (ds, res);
      return res;
      }

      // распечатка данных TMDataServer серверов в структуру-дерево
      int TMDataServers_Info2Tree( int panel, int control, int n )
      {
      for(int i=0; i<TMDataServerCount; i++)
      {
         n = TMDataServer_Info2Tree( TMDataServers[i], panel, control, n );
      }
      return n;   
      }

      // распечатка данных TMDataServer сервера в структуру-дерево
      int TMDataServer_Info2Tree( TMDataServer *ds, int panel, int control, int n )
      {
      char buf[128];
      int h = n;
      sprintf( buf, "Data Server \"%s\"", ds.name );
      InsertTreeItem (panel, control, VAL_SIBLING, 0, VAL_LAST, buf, NULL, NULL, n++);
      if( ds != NULL )
      {
         // тип сервера
         sprintf( buf, "type: %d (%s)", ds.type,  Packet_GetSignatyre(ds.type) );
         InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);

         sprintf( buf, "connection: %s", (TMDataServer_CONNECTED == TMDataServers_IsConnected(ds))?"Connected":"Not connected" );
         InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);

         sprintf( buf, "restore connection: %d (%d)", ds.NeedForReconnect, ds.NeedAutoReconnect );
         InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);
         // тип подключения
         if( ds.ConnectType == SERVER_ConnectType_LNKLIB )
         {
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, "con type: 1 (LnkLib)", NULL, NULL, n++);
            sprintf( buf, "addr: %s:%d", ds.ip, ds.port );
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);
         }
         else if( ds.ConnectType == SERVER_ConnectType_TMPROTOCOL )
         {
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, "con type: 2 (TM Protocol)", NULL, NULL, n++);
            sprintf( buf, "addr: %s:%d", ds.ip, ds.port );
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);
         }
         else if( ds.ConnectType == SERVER_ConnectType_DIRECT )
         {
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, "con type: 4 (Direct)", NULL, NULL, n++);
            sprintf( buf, "COM: %d", ds.COM.comport );
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);
         }
         else if( ds.ConnectType == SERVER_ConnectType_IMITATION )
         {
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, "con type: 8 (Imitation)", NULL, NULL, n++);
         }
         else if( ds.ConnectType == SERVER_ConnectType_TCPCUSTOM )
         {
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, "con type: 16 (TCP Custom)", NULL, NULL, n++);
            sprintf( buf, "addr: %s:%d", ds.ip, ds.port );
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);
         }
         else if( ds.ConnectType == SERVER_ConnectType_RS232 )
         {
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, "con type: 4 (RS232)", NULL, NULL, n++);
            sprintf( buf, "COM: %d (%s)", ds.COM.comport );
            InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);
         }

         sprintf( buf, "Ask: %d Queue: %d Store: %d/%d", ds.lnk.asknum, ds.lnk.asks.count, 
                TmQueue_StoreAvailableCount(&ds.lnk.asks), ds.lnk.asks.store_limit );
         InsertTreeItem (panel, control, VAL_CHILD, h, VAL_LAST, buf, NULL, NULL, n++);

      }
      return n;
      }


      //восстановление соединения с серверами
      //вызывается по таймеру
      void ReconnectServers()
      {
      int res;
      char buf[64];

      for(int i=0; i<TMDataServerCount; i++)
      {
         TMDataServer *ds = TMDataServers[i];

         if( configParams.StopAll ) return;
         if( ds == NULL ) continue;

         // восстанавливаем подключение у не подключенных
         if( TMDataServer_CONNECTED != TMDataServers_IsConnected( ds ) && ds.NeedForReconnect )  // уже подключен или восстанавливать не требуется
         {

            // восстанавливаем подключение
            ds.OnProcessing = 1;

            res = TMDataServers_ConnectToServer( ds );
            // если подключились - представимся подключенному серверу
            if( res == TMDataServer_CONNECTED && ds.ConnectType == SERVER_ConnectType_TMPROTOCOL )
            {
               sprintf(buf, "I'm %s client on %s", ds.name, configParams.appName ); 
               TMDataServers_SendTMInfo( ds, Packet_INFO_MESSAGE, sizeof(buf), (unsigned char*)buf );
            }
            ProcessSystemEventsIfTime( 1.0 );
            //if( !res ) continue;
         }     

         if( configParams.StopAll ) return;
         if( ds == NULL ) return;

         //Cчетчик тиков. Сбрасывается при пришедших от сервера данных.       
         //Если счетчик насчитает много, данные достоверными считать нельзядля 
         if( ds.TimerTicks < MAX_TIMER_TICKS_FOR_CORRECTDATA) ds.TimerTicks++;  

         if( ds.type != SERVER_TYPE_PFS_PROXY ) // прокси не присылает данные, если ничего не происходит
         {
            if( ds.TimerTicks >= MAX_TIMER_TICKS_FOR_CORRECTDATA )       // сервер слишком долго молчит - наверное сломался
            {
               ds.DevStatus = DEVICE_STATUS_CRACKED;
            }
         }     
         ds.OnProcessing = 0;  

      }
      }

      // отключение от сервера
      void TMDataServers_DisconnectFromServer(TMDataServer ds)
      {
      if( ds == NULL ) return;
      if( ds.F_Disconnect ) 
      { 
         (*ds.F_Disconnect) (ds); 
      }
      // подключаемся согласно указанному типу подключения
      else if( ds.ConnectType == SERVER_ConnectType_TMPROTOCOL || ds.ConnectType == SERVER_ConnectType_TCPCUSTOM )
      {
         if( ds.handle >= 0 )
            DisconnectFromTCPServer ( ds.handle );
      }
      else if(  ds.ConnectType == SERVER_ConnectType_LNKLIB  )
      {
         if( ds.lnk.sid >= 0 )
            DisFrSid( ds.lnk.sid );
            //DisFrSrv( ds.sid );
      }
      else if(  ds.ConnectType == SERVER_ConnectType_DIRECT || ds.ConnectType == SERVER_ConnectType_RS232  )
      {
         if( ds.COM.PortOpen )
         {
            int br=SetBreakOnLibraryErrors (0);
            CloseCom( ds.COM.comport );
            ds.COM.PortOpen = 0;
            SetBreakOnLibraryErrors (br);
         }
      }
      else if(  ds.ConnectType == SERVER_ConnectType_IMITATION  )
      {
         ds.COM.PortOpen = 0;
      }

      DisconnectFix( ds );
      TmQueue_DropAll( &ds.lnk.asks, TRUE );
      }

      // подключен ли сервер к источнику данных?
      // результат TMDataServer_CONNECTED (0) - подключен
      int TMDataServers_IsConnected( TMDataServer *ds )
      {
      if( ds == NULL ) return -1;

      if( ds.ConnectType == SERVER_ConnectType_TMPROTOCOL || ds.ConnectType == SERVER_ConnectType_TCPCUSTOM )
      {
         if( ds.handle >= 0 )
            return TMDataServer_CONNECTED; // подключен
      }
      else if(  ds.ConnectType == SERVER_ConnectType_LNKLIB  )
      {
         if( ds.lnk.sid >= 0 )
            return TMDataServer_CONNECTED; // подключен
      }
      else if(  ds.ConnectType == SERVER_ConnectType_DIRECT || ds.ConnectType == SERVER_ConnectType_RS232  )
      {
         if( ds.COM.PortOpen )
            return TMDataServer_CONNECTED; // подключен
      }
      else if(  ds.ConnectType == SERVER_ConnectType_IMITATION || ds.ConnectType == SERVER_ConnectType_NONE )
      {
         if( ds.COM.PortOpen )
            return TMDataServer_CONNECTED; // подключен
      }

      return -1; // нет
      }

      // изменение способа полдлючения к серверу данных с проверкой возможности подключения
      // предварительно следует отключение
      // 0 - успех
      int  TMDataServers_SetConnectType( TMDataServer *ds, int ConnectType )
      {
      if( ds == NULL ) return -1;

      if( TMDataServers_IsConnected( ds ) )
         TMDataServers_DisconnectFromServer( ds );

      // сравниваем с маской
      if( 0 == (ds.ConnectAvailable & ConnectType) )
      {
         mprntf_t("Connection type restricted for this type of TMDataServers");
         return -1;
      }

      ds.ConnectType = ConnectType;
      return 0;
      }

      // номер сервера в списке серверов. Для упорядочивания
      // вызывается из AddServer
      int TMDataServers_GetServerNum( TMDataServer *ds )
      {
      for(int i=0; i<TMDataServerCount; i++)
      {
         if( ds == TMDataServers[i] )
            return i;
      }
      return -1;
      }

      // возвращает сервер как элемент массива
      TMDataServer *TMDataServers_GetServerByNum( int n )
      {
      if( n < 0 || n>= TMDataServerCount ) return NULL;

      return TMDataServers[n];
      }

      // расшифровка статуса возвращенного линклиб
      void TMDataServers_ShowLnkLibSts( TMDataServer *ds, unsigned char rq_sts )     
      {
         // Error code returned, no other valid data
         switch( rq_sts ){
            case PE_COMOPE:
               mprntf_t("%s: LnkLib Translator say: Open COM Port Error  %d", ds.name, rq_sts);
            break;
            case PE_COMISG:
               mprntf_t("%s: LnkLib Translator say: Invalid Segment  %d", ds.name, rq_sts);
            break;
            case PE_COMCMD:
               mprntf_t("%s: LnkLib Translator say: Command not supported  %d", ds.name, rq_sts);
            break;
            case PE_COMRER:
               mprntf_t("%s: LnkLib Translator say: Port Read Error  %d", ds.name, rq_sts);
            break;
            case PE_COMTMO:
               mprntf_t("%s: LnkLib Translator say: TimeOut - there was no record terminator in time  (sts: %d)", ds.name, rq_sts);
            break;
            case PE_COMOVR:
               mprntf_t("%s: LnkLib Translator say: Too many bytes in answer  %d", ds.name, rq_sts);
            break;
            case PE_COMCHR:
               mprntf_t("%s: LnkLib Translator say: Bad Chars in Answer  %d", ds.name, rq_sts);
            break;
            case PE_COMODD:
               mprntf_t("%s: LnkLib Translator say: Odd byte counter in Answer  %d", ds.name, rq_sts);
            break;
            case PE_COMLDR:
               mprntf_t("%s: LnkLib Translator say: no Lider in Answer  %d", ds.name, rq_sts);
            break;
            case PE_COMTER:
               mprntf_t("%s: LnkLib Translator say: no Terminator in Answer %d", ds.name, rq_sts);
            break;
            case PE_COMCSM:
               mprntf_t("%s: LnkLib Translator say: Bad CheckSumm %d", ds.name, rq_sts);
            break;
            case PE_COMECD:
               mprntf_t("%s: LnkLib Translator say: Device return Error Code  %d", ds.name, rq_sts);
            break;
            default:
               mprntf_t("%s: LnkLib Translator say: Port error %d", ds.name, rq_sts);
            break;
         }

         //sprintf(buf, "Status %d", ds.name, rq.sts ); 
      }

      // общие ошибки устройств на интерфейсе RS-485
      // используется в PFS_GetErrorMessage
      int RS485_GetErrorMessage(int err_code, int lang, char *buf, int buf_sz)
      {
      buf[0] = '\0';

      if( err_code >=0 && err_code <=11) // 0..11
      {
         // пользуемся тем, что все расшифровки в массиве идут последовательно   
         strncpy( buf, DS_LangText[DS_LANG_ERR_NOT+err_code][lang], buf_sz );
         return 0;
      }

      return -1; // неизвестная ошибка устройства
      }

      // ошибки модуля USART микроконтроллера
      // используется в PFS_GetErrorMessage
      int USART_GetErrorMessage(int err_code, int lang, char *buf, int buf_sz)
      {
      buf[0] = '\0';

      if( err_code >=128 && err_code <= 167) 
      {
         // пользуемся тем, что все расшифровки в массиве идут последовательно   
         strncpy( buf, DS_LangText[DS_LANG_ERR_TO_RO+err_code-128][lang], buf_sz );
         return 0;
      }
      return -1; // неизвестная ошибка устройства
      }

      int TMDataServers_LinkLibCBND( PKT *rq )
      {
      int res = 0;
      return res;
      }

      // колбэк от linklib - ответ от кресла
      // инициализируется в ( SendCommandToPFS ). TMDataServers_SendCommandToHW
      static int TMDataServers_LinkLibCB( PKT *rq, void *data )
      {
      // PFS_data *pfsd = NULL;  // данные сервера кресла
      TMDataServer *ds = NULL;     // сервер кресла
      DEV_DESCR *dds;
      int res = 0;
      //unsigned char sts;
      //char buf[128];

      if( data == NULL ) return -1; //TMC_ERROR_PFSCMD_UNKNOWN_ERR;
      if( configParams.StopAll ) return 0;

      DS_LnkLb_ask *ask = (DS_LnkLb_ask *) data;
      ds  = (TMDataServer *) ask.p_ds;
      dds = (DEV_DESCR *)  ask.p_dds;
      TmQueueElement *el = TmQueue_GetElemByDataPtr( &ds.lnk.asks, ask );

      if( !el ) // не должно такого быть, ну а вдруг повторный ответ
         return -1;

      // pfsd = (PFS_data*) ds.ServerData;
      ds.TimerTicks = 0;

      // разбираем статус ответа транслятора
      if( rq.sts) {
         // Error code returned, no other valid data
         TMDataServers_ShowLnkLibSts( ds, rq.sts );
         ds.DevStatus  = DEVICE_STATUS_CRACKED;
         dds.status    = DEVICE_STATUS_CRACKED;
         //LogStr("KSF_Log", buf);
         //return 0;
         res = -3; //PFS_result( -3, ds ); //PFS_ERROR_LLB;

         // отображаем цветом лампочки сервера, что запрос к железу прошел неудачно
         //DelayWithEventProcessing( 1.2 );
         if( ds.panelHandle >= 0 && ds.ControlID >= 0)
            SetCtrlAttribute (ds.panelHandle, ds.ControlID, ATTR_ON_COLOR, VAL_RED);                                   
      }
      else
      {
         // разбираем ответ системы фиксации побайтно
      //    unsigned char b0 =  rq.d[0];
      //    unsigned char b1 =  rq.d[1];
      //    unsigned char b2 =  rq.d[2];
      //    unsigned char b3 =  rq.d[3];
      //    unsigned char mark = rq.d[4]; 
         unsigned char cnt = rq.d[5];
      //    unsigned char dn =  rq.d[6];
      //    unsigned char b7 =  rq.d[7];

         // сбрасываем ответ в лог, если заказано сохранение ответов
         if( ds.F_LogResponce )
            (*ds.F_LogResponce) ( (unsigned char *)rq.d, 7, cnt, ask.p_dds, ask.cmd, ask.asknum );      // PFS_LogResponce etc

         // разбираем ответ и фиксируем данные
         if( ds.F_ParseResponce )
            res = (*ds.F_ParseResponce) ( (unsigned char *)rq.d, 7, cnt, ask.p_dds );    // PFS_ParseResponce Termo_ParseResponce XRay_ParseResponce etc

         // отображаем цветом лампочки сервера, что запрос к железу обработан успешно
         //DelayWithEventProcessing( 1.2 );
         if( ds.panelHandle >= 0 && ds.ControlID >= 0)
            SetCtrlAttribute (ds.panelHandle, ds.ControlID, ATTR_ON_COLOR, VAL_GREEN);                                   
      }

      TmQueue_DropByPtr( &ds.lnk.asks, el, 1 );  // удаляем запрос из очереди

      if( ds.F_FixResponceResult )
         (*ds.F_FixResponceResult) ( res, dds );    // PFSDS_FixResponceResult


      return res; 
      }

      // обработка ответа он контроллера системы фиксации при прямом обращении в порт
      // вызывается из [ SendCommandToPFS, Termo_SendCommandToDev ] . TMDataServers_SendCommandToHW
      // команда формируется в [ PFS_MakeCommand, Termo_MakeCommand, ASh_MakeCommand ]
      int ack_parser( DEV_DESCR *pdds, unsigned char *rx_buff, int cmd, int asknum )
      {
      unsigned char dn=0, cnt=0;
      int n;
      int res = 0;
      TMDataServer *ds = (TMDataServer *)pdds.p_ds;

      n=0;
      if( pdds.dev_type != DEVICE_TYPE_ANCSXY ) // кроме контроллера перемещалки
      {     
         if(rx_buff[n++] != '+')   // какой-то мусор, а не ответ контроллера
            return -5;

         cnt = rx_buff[n++];//1
         dn =  rx_buff[n++];//2
      }

      // сбрасываем ответ в лог, если заказано сохранение ответов
      if( ds.F_LogResponce )
         (*ds.F_LogResponce) ( rx_buff, n, cnt, ds, cmd, asknum );      // PFS_LogResponce etc

      // выводим, если надо строку в панель сообщений
      if( configParams.trace_flag )                                                            
            TraceBytes("Resp 0x:", (unsigned char *)rx_buff, cnt);

      // разбираем ответ и фиксируем данные
      if( ds.F_ParseResponce )
         res = (*ds.F_ParseResponce) ( rx_buff, n, cnt, pdds );    // PFS_ParseResponce Termo_ParseResponce TermoL_ParseResponce etc

      // отрисовка состояний, повторные запросы если надо
      if( ds.F_FixResponceResult )
         (*ds.F_FixResponceResult) ( res, ds );     // PFSDS_FixResponceResult ( res, ds )  

      //ex:
      pdds.InWait = 0;
      return res; 
      }

      // отработка команды к одному из контроллеров (к железу) с ожиданием ответа
      // по результатам заполняются внутренние структуры данных
      // 0 - команда успешно отработана, результат получен
      // вызывается из DIM_SendCommand, MBPS_SendCommand, XRAYC_SendCommand
      int TMDataServers_SendCommandToDev( DEV_DESCR *dds, unsigned char command, double wait)
      {
      int res = 0;
      TMDataServer *ds = (TMDataServer *) dds.p_ds;

      if( !dds.configured ) return 0;

      res = TMDataServers_SendCommandToHW( dds, command );
      if( res ) return res;

      // ждем отработки команды, но не более ... c
      double t0 = Timer();
      if( wait > 0 )
         while( ds.InWait )
         {
            if( (Timer() - t0) > wait)
            {
               ds.InWait = 0;  
               mprntf_t("%s DN=%d: Responce TimeOuted", dds.name, dds.DN);
               return -7;  //  Time Out
            }
            ProcessSystemEvents();
         }  
      return res;

      }

      // подписка на уведомления для библиотеки линклиб
      int TMDataServers_SetLL_NTF( DEV_DESCR *pdds, int cmd, int (* cbfnq)(PKT *) )
      {
      TMDataServer *ds = (TMDataServer *)pdds.p_ds;

      SDEV sdev;
      sdev.seg = (short) (pdds.DN>>8);
      sdev.wcn = 0;                    // Number of data bytes to send
      sdev.rcn = 120;                  // Answer must be less equal .. bytes
      sdev.udt[sdev.wcn++] = (unsigned char) (pdds.DN & 0xFF);   // Address
         //. PFS_MakeCommand Termo_MakeCommand XRay_MakeCommand FDCY_MakeCommand AUX_MakeCommand ASh_MakeCommand
      sdev.wcn =  (unsigned char) (*ds.F_MakeCommand) (cmd, (unsigned char *)sdev.udt, (unsigned char)sdev.wcn, pdds, &sdev.tmo);   

      //int h=
      PktWrC( ds.lnk.RQ|FN_ANS|FN_NFY, NULL, 0, cbfnq );
      return 0;
      }

      // отправка команды к железу дата-сервера по активному протоколу
      // используетася в SendCommandToPFS, Termo_SendCommandToDev, ASh_SendCommandToDev
      int TMDataServers_SendCommandToHW( DEV_DESCR *pdds, int cmd )
      {
      int res = 0;
      TMDataServer *ds = (TMDataServer *)pdds.p_ds;
      //PFS_data *pfsd; // данные сервера кресла
      if( ds == NULL ) 
         return -1;     // нет источник данных
      if( ds.F_MakeCommand == NULL ) return -2;

      //pfsd = (PFS_data*) ds.ServerData;
      //if( pfsd == NULL ) return -2;        // нет блока данных
      if( cmd == DS_CMD_NOTHING ) return 0;  // команда - пустышка, просто завершаем выполнение

      if( TMDataServer_CONNECTED != TMDataServers_IsConnected(ds) ) 
         return -1;
      if( ds.ConnectType == SERVER_ConnectType_IMITATION )  
         return 0; // имитация подключения к устройству
      //mprntf("try send %d", cmd );

      // отображаем цветом лампочки сервера, что запрос к железу отправлен
      if( ds.panelHandle >= 0 && ds.ControlID >= 0)
         SetCtrlAttribute (ds.panelHandle, ds.ControlID, ATTR_ON_COLOR, VAL_YELLOW);                                   

      if( ds.ConnectType == SERVER_ConnectType_LNKLIB )   // подключение по протоколу LNKLIB
      {
         if( ds.handle < 0 ) return -3; // нет коннекта к серверу
         //SDEV dp;
         DS_LnkLb_ask *ask_str = tmAllocEmpty( sizeof(DS_LnkLb_ask) );   // элемент для очереди - данные запроса
         ask_str.p_ds = ds;
         ask_str.asknum = TMDataServers_GetNextAskNum( ds );;
         ask_str.p_dds = pdds;

         TmQueue_AddFirst ( &ds.lnk.asks, cmd, 0.0, ask_str );

         ask_str.sdev.seg = (short) (pdds.DN>>8);
         ask_str.sdev.wcn = 0;                    // Number of data bytes to send
         ask_str.sdev.rcn = 120;                  // Answer must be less equal .. bytes
         ask_str.sdev.udt[ask_str.sdev.wcn++] = (unsigned char) (pdds.DN & 0xFF);   // Address
         ask_str.cmd = cmd;
         //. PFS_MakeCommand Termo_MakeCommand XRay_MakeCommand FDCY_MakeCommand AUX_MakeCommand ASh_MakeCommand
         ask_str.sdev.wcn =  (unsigned char) (*ds.F_MakeCommand) (cmd, (unsigned char *)ask_str.sdev.udt, (unsigned char)ask_str.sdev.wcn, pdds, &ask_str.sdev.tmo);   

         //LogTraceStr("KSF_Log", "Reqv: ", dp.udt+1, dp.wcn-1);
         if( configParams.trace_flag )                                                            
               TraceBytes("Out 0x:", (unsigned char *)ask_str.sdev.udt, (int)ask_str.sdev.wcn);
         //mprntf("ask %d", cmd);
         ds.InWait = 1;

         // сохраняем запрошенную команду в лог
         if( ds.F_LogRequest )
            (*ds.F_LogRequest) ( (unsigned char*) ask_str.sdev.udt+1, ask_str.sdev.wcn-1, pdds, cmd, ask_str.asknum );     // PFS_LogRequest etc

         PktWrCBD( ds.lnk.RQ|FN_ANS|FN_WEV, &ask_str.sdev, (int)sizeof(ask_str.sdev), TMDataServers_LinkLibCB, ask_str );
      // PktWrC( ds.lnk.RQ|FN_ANS|FN_NFY, &ask_str.sdev, (int)sizeof(ask_str.sdev), TMDataServers_LinkLibNTFCB );
      //    PktWrCBD( ds.lnk.RQ|FN_ANS|FN_WEV, &ask_str.sdev, (int)sizeof(ask_str.sdev), TMDataServers_LinkLibCB, ask_str ); // +
      //    PktWrC( ds.lnk.RQ|FN_ANS|FN_CYC, &ask_str.sdev, (int)sizeof(ask_str.sdev), TMDataServers_LinkLibCBND );
      //    return PktWrC(RQRSDEV|FN_ANS|fnc, &dp,sizeof(dp), sdevCB);

      } 
      else if( ds.ConnectType == SERVER_ConnectType_DIRECT )  // прямое подключение через ком-порт c оберткой
      {
         // local connect
         //unsigned long int val_i; 
         //unsigned char valb[4];
         //int inqlen = 0;
         unsigned char send_data[1024];
         unsigned char read_data[1024];//, buf[32];
         int n=0,read_data_size = 1024;
         short delay;


         send_data[n++] = 0x3A;        //CP ':'
         n++; // cnt
         send_data[n++] = pdds.DN & 0xFF;  //DN

         // формируем строку-команду для контроллера
         n =  (*ds.F_MakeCommand) ( cmd, send_data, n, pdds, &delay );

         if( n<=0 ) return -n; // команда не сформирована
         send_data[1] = (unsigned char)n-1; 
         send_data[n] = 0;//calc_check_summ( send_data, n );

         // выводим, если надо строку в панель сообщений
         if( configParams.trace_flag )                                                            
               TraceBytes("Out 0x:", (unsigned char *)send_data, n);

         // сохраняем запрошенную команду в лог
         if( ds.F_LogRequest )
            (*ds.F_LogRequest) ( send_data+1, n, pdds, cmd, 0 );     // PFS_LogRequest etc

         res = ReadWriteCOMPort( &ds.COM, send_data, n+1, read_data, read_data_size-1);

         // отображаем цветом лампочки сервера, что запрос к железу отработан
         if( ds.panelHandle >= 0 && ds.ControlID >= 0)
         {
            if( res == 0 )
               SetCtrlAttribute (ds.panelHandle, ds.ControlID, ATTR_ON_COLOR, VAL_GREEN);                                   
            else
               SetCtrlAttribute (ds.panelHandle, ds.ControlID, ATTR_ON_COLOR, VAL_RED);                                   
         }
         if( res != 0 ) return res;

         // разбираем ответ от железа
         ack_parser( pdds, read_data, cmd, 0 );
      }
      else if( ds.ConnectType == SERVER_ConnectType_RS232 )  // прямое подключение через ком-порт
      {
         unsigned char send_data[1024];
         unsigned char read_data[1024];//, buf[32];
         int n=0,read_data_size = 1024;
         short delay;

         // формируем строку-команду для контроллера
         n =  (*ds.F_MakeCommand) ( cmd, send_data, n, pdds.p_dev, &delay );

         if( n<=0 ) return -1; // команда не сформирована

         // выводим, если надо строку в панель сообщений
         if( configParams.trace_flag )                                                            
               TraceBytes("Out 0x:", (unsigned char *)send_data, n);

         // сохраняем запрошенную команду в лог
         if( ds.F_LogRequest )
            (*ds.F_LogRequest) ( send_data, n, pdds, cmd, 0 );       // PFS_LogRequest etc

         res = ReadWriteCOMPortMODBUS( &ds.COM, send_data, n, read_data, read_data_size-1);

         // отображаем цветом лампочки сервера, что запрос к железу отработан
         if( ds.panelHandle >= 0 && ds.ControlID >= 0)
         {
            if( res == 0 )
               SetCtrlAttribute (ds.panelHandle, ds.ControlID, ATTR_ON_COLOR, VAL_GREEN);                                   
            else
               SetCtrlAttribute (ds.panelHandle, ds.ControlID, ATTR_ON_COLOR, VAL_RED);                                   
         }
         if( res != 0 ) return res;

         // сбрасываем в лог и разбираем ответ
         //LogKSFReqResp( send_data, read_data );
         ack_parser( pdds, read_data, cmd, 0 );
      }
      return 0;
      }

      // отображение статуса устройства индикатором (согласно TM протоколу)
      // черный-неотконф., красный-поломка, зеленый-ОК
      int ShowDeviceStateLed( int configured, int status, int panelHandle, int control )
      {
      if( control <= 0 ) return -1;

      if( !configured )
      {
         SetCtrlAttribute (panelHandle, control, ATTR_OFF_COLOR, VAL_BLACK);                 
         SetCtrlAttribute (panelHandle, control, ATTR_CTRL_VAL, 0);                 
      }
      else
      {
         if( status == DEVICE_STATUS_CRACKED ) // не откликается - красный
         {
            SetCtrlAttribute (panelHandle, control, ATTR_OFF_COLOR, VAL_RED);                
            SetCtrlAttribute (panelHandle, control, ATTR_CTRL_VAL, 0);                 
         }
         else if( status == DEVICE_STATUS_OFF ) // не откликается - черный
         {
            SetCtrlAttribute (panelHandle, control, ATTR_OFF_COLOR, VAL_BLACK);                 
            SetCtrlAttribute (panelHandle, control, ATTR_CTRL_VAL, 0);                 
         }
         else  // всё в порядке - зелёный
            SetCtrlAttribute (panelHandle, control, ATTR_CTRL_VAL, 1);                 
      }
      return 0;
      }

      // отрисовка байта статуса устройства на панели в 8 лампочках
      // вызывается из ShowEDeviceState, ShowLDeviceState
      // вызывается из KrC_LightOnTimer, ECS_GUITimerCB
      void ShowStatusByteState( int panelHandle, int *StsControls, int off_all, unsigned char STS, unsigned char blueMask )
      {
      int color, old_color;
      unsigned short mask;

      mask= 1<<8;
      if( StsControls == NULL ) return;

      for(int i=0; i<=7; i++) 
      {
         mask >>=1;
         if( StsControls[i] == 0 ) continue;

         //if( descr.configured == 0 || descr.status == DEVICE_STATUS_CRACKED )  // black
         if( off_all )
         {
            SetCtrlVal (panelHandle, StsControls[i], 0);
            //GetCtrlAttribute (panelHandle, leds_stsL[i], ATTR_ON_COLOR, &old_color);
         }
         else  // иначе: сигнал присутствует - красим в красный, нет - зеленым
         {
            GetCtrlAttribute (panelHandle, StsControls[i], ATTR_ON_COLOR, &old_color);
            if( STS & mask )  
            {
               if( mask & blueMask )
                  color = COLOR_ACTION_PRESENT;
               else
                  color = COLOR_BLOCK_PRESENT;
            }
            else
                  color = COLOR_BLOCK_NONE;

            if( old_color != color )
               SetCtrlAttribute (panelHandle, StsControls[i], ATTR_ON_COLOR, color);
            SetCtrlVal (panelHandle, StsControls[i], 1);
         }     
      }
      }

      // упаковка строки в пакет пересылки
      int SendCurrentConfig_PackStr( unsigned char *data, int n, char *str )
      {
      size_t len;
      len = strlen( str );
      data[n++] = len>255?255:(unsigned char)len;
      strncpy( (char*)data+n, str, len ); 
      n+= len; 

      return n;
      }

      // упаковка типа int в пакет пересылки
      int SendCurrentConfig_PackInt( unsigned char *data, int n, int val )
      {
      data[n++] = (unsigned char) (val     & 0xFF);
      data[n++] = (unsigned char) (val>>8  & 0xFF);
      data[n++] = (unsigned char) (val>>16 & 0xFF);
      data[n++] = (unsigned char) (val>>24 & 0xFF);

      return n;
      }

      // упаковка типа int как набора байт конфигурации в пакет пересылки
      int SendCurrentConfig_PackDevCfgAsTag( unsigned char *data, int n, int val )
      {
      data[n++] = Packet_DATA_CFGTAG_DEVCONFIG;  // 
      n = SendCurrentConfig_PackInt( data, n, val );

      return n;
      }

      // упаковка типа данных об устройстве как тэг в пакет пересылки
      int SendCurrentConfig_PackDevAsTag( unsigned char *data, int n, DEV_DESCR *dev )
      {
      int sz_pos;
      data[n++] = Packet_DATA_CFGTAG_DEVICE;  // 
      sz_pos = n++; // длина блока следом
      data[n++] = (unsigned char) dev.dev_type;
      data[n++] = (unsigned char) dev.configured;
      data[n++] = (unsigned char) dev.DN;
      n = SendCurrentConfig_PackStr( data, n, dev.name );
      data[sz_pos] = (unsigned char) (n-sz_pos-1); // фиксируем длину блока

      return n;
      }

      // упаковка информации о соединении как тэг в пакет пересылки
      int SendCurrentConfig_PackConnectAsTag( unsigned char *data, int n, TMDataServer *ds )
      {
      int sz_pos;
      data[n++] = Packet_DATA_CFGTAG_CONNECT; // 
      sz_pos = n++; // длина блока следом
      data[n++] = (unsigned char) ds.ConnectType;
      n = SendCurrentConfig_PackStr( data, n, ds.ip );
      n = SendCurrentConfig_PackInt( data, n, ds.port );
      data[n++] = (unsigned char) ds.COM.comport;
      n = SendCurrentConfig_PackStr( data, n, ds.name );
      data[sz_pos] = (unsigned char) (n-sz_pos-1); // фиксируем длину блока

      return n;
      }

      // упаковка информации о версии ПО как тэг в пакет пересылки
      int SendCurrentConfig_PackSoftwareVerAsTag( unsigned char *data, int n )
      {
      data[n++] = Packet_DATA_CFGTAG_VER;  // 
      n = SendCurrentConfig_PackStr( data, n, PACKET_VERTION_STR );

      return n;
      }

      // упаковка текстовой информации. Распаковка в ParseCfgPacket_GetStr 
      int SendCurrentConfig_PackTxtInfoAsTag( unsigned char *data, int n, char *info )
      {
      data[n++] = Packet_DATA_CFGTAG_TXTINFO; // 
      n = SendCurrentConfig_PackStr( data, n, info );

      return n;
      }

      // упаковка информации о линке ретранслятора (тэг Packet_DATA_CFGTAG_RTLINK полностью). Распаковка в ParseCfgPacket_GetRTLinkData 
      int SendCurrentConfig_PackRTLinkAsTag( unsigned char *data, int n, TCPSrvServer *tcp )
      {
      int sz_pos;
      data[n++] = Packet_DATA_CFGTAG_RTLINK;  // 
      sz_pos = n++; // длина блока следом
      n = SendCurrentConfig_PackStr( data, n, tcp.name );
      n = SendCurrentConfig_PackInt( data, n, tcp.port );
      n = SendCurrentConfig_PackStr( data, n, tcp.NetRT.remote_ip );
      n = SendCurrentConfig_PackInt( data, n, tcp.NetRT.remote_port );
      data[n++] = (unsigned char) tcp.NetRT.configured;
      data[n++] = (unsigned char) tcp.NetRT.multyconnection;
      data[sz_pos] = (unsigned char) (n-sz_pos-1); // фиксируем длину блока

      return n;
      }


      // разбор тега из пакета конфигурации - вытаскиваем строку. 
      //Упаковка в SendCurrentConfig_PackTxtInfoAsTag
      int ParseCfgPacket_GetStr( unsigned char *data, int n, char *buf, int buf_size )
      {
      unsigned char cnt = *(data+n++);
      memset( buf, 0, buf_size );

      strncpy( buf, (char*)data+n, min( buf_size-1, cnt ) );
      n+=cnt;
      return n;
      }

      // разбор тега из пакета конфигурации - вытаскиваем строку
      int ParseCfgPacket_GetInt( unsigned char *data, int n, int *val )
      {
      int b1, b2, b3, b4;
      b1 = *(data+n++);
      b2 = *(data+n++);
      b3 = *(data+n++);
      b4 = *(data+n++);
      *val = b1 | (b2<<8)| (b3<<16) | (b4<<24);
      return n;
      }

      // разбор тега из пакета конфигурации - вытаскиваем данные о подключении (тэг Packet_DATA_CFGTAG_CONNECT полностью). Упаковка в SendCurrentConfig_PackLinkAsTag
      int ParseCfgPacket_GetLinkData( unsigned char *data, int n, TMDataServer *ds )
      {
      //n++; 
      unsigned char cnt = *(data+n++);
      int res = cnt+n;

      ds.type = *(data+n++);
      if( n>=res ) return res;
      n = ParseCfgPacket_GetStr( data, n, ds.ip, 42 );
      if( n>=res ) return res;
      n = ParseCfgPacket_GetInt( data, n, &ds.port );
      if( n>=res ) return res;
      ds.COM.comport = *(data+n++);
      n = ParseCfgPacket_GetStr( data, n, ds.name, 16 );

      return n;
      }

      // разбор тега из пакета конфигурации - вытаскиваем описание устройства (тэг Packet_DATA_CFGTAG_DEVICE полностью)
      int ParseCfgPacket_GetDevDescr( unsigned char *data, int n, DEV_DESCR *dds )
      {
      int res;
      unsigned char cnt = *(data+n++);
      res = cnt+n;

      dds.dev_type = *(data+n++);
      if( n>=res ) return res;
      dds.configured = *(data+n++);
      if( n>=res ) return res;
      dds.DN = *(data+n++);
      if( n>=res ) return res;
      n = ParseCfgPacket_GetStr( data, n, dds.name, 256 );

      return res;
      }

      // разбор тега из пакета конфигурации - вытаскиваем данные о линке ретранслятора (тэг Packet_DATA_CFGTAG_RTLINK полностью). Упаковка в SendCurrentConfig_PackRTLinkAsTag 
      int ParseCfgPacket_GetRTLinkData( unsigned char *data, int n, TCPSrvServer *tcp )
      {
      unsigned char cnt = *(data+n++);
      int res = cnt+n;

      n = ParseCfgPacket_GetStr( data, n, tcp.name, 32 );  // имя соединеия
      if( n>=res ) return res;
      n = ParseCfgPacket_GetInt( data, n, &tcp.port );     // входящий порт
      if( n>=res ) return res;
      n = ParseCfgPacket_GetStr( data, n, tcp.NetRT.remote_ip, 42 ); // удаленный сервер
      if( n>=res ) return res;
      n = ParseCfgPacket_GetInt( data, n, &tcp.NetRT.remote_port ); // удаленный порт
      tcp.NetRT.configured = *(data+n++);
      tcp.NetRT.multyconnection = *(data+n++);

      return res;
      }


      // разбор одного тега из пакета конфигурации в зависимости от типа источника
      // вызывается из TmNS_ParseImcommingPacket
      int ParseTagAsString( int srv_type, unsigned char *pdata, int n, char *out, int out_sz )
      {
      char buf[256];
      TMDataServer tmp_ds = {0};
      DEV_DESCR dds = {0};
      TCPSrvServer tcp = {0};
      int ival;

      unsigned char tag = *(pdata+n++);  // тип тега
      switch( tag )
      {
         case Packet_DATA_CFGTAG_VER:        // версия пакета, строка PACKET_VERTION_STR
            n = ParseCfgPacket_GetStr( pdata, n, buf, 256 );
            sprintf( out, "Software Ver.: %s", buf );
         break;
         case Packet_DATA_CFGTAG_CONNECT:    // коннект
            n = ParseCfgPacket_GetLinkData( pdata, n, &tmp_ds );
            switch( tmp_ds.type )
            {
               case SERVER_ConnectType_LNKLIB:
                  sprintf( out, "LnkLib connect %s (%s: %02d)", tmp_ds.name, tmp_ds.ip, tmp_ds.port );
               break;
               case SERVER_ConnectType_TMPROTOCOL:
                  sprintf( out, "TM connect %s (%s: %02d)", tmp_ds.name, tmp_ds.ip, tmp_ds.port );
               break;
               case SERVER_ConnectType_RS232:
               case SERVER_ConnectType_DIRECT:
                  sprintf( out, "Connect %s throw COM%d", tmp_ds.name, tmp_ds.COM.comport );
               break;
               case SERVER_ConnectType_TCPCUSTOM:
                  sprintf( out, "Custom TCP connect %s (%s: %02d)", tmp_ds.name, tmp_ds.ip, tmp_ds.port );
               break;
               default:
                  sprintf( out, "Unknown connect %s type %d", tmp_ds.name, tmp_ds.type );
               break;
            }
         break;
         case Packet_DATA_CFGTAG_DEVICE:    // подкключенное железо: dev_type-configured-DN 
            n = ParseCfgPacket_GetDevDescr( pdata, n, &dds );
            sprintf( out, "Device %s (%s) DN: %02d %s", GetDeviceTypeName(dds.dev_type, buf), dds.name, dds.DN, dds.configured?"":"(not configured)" );
         break;
         case Packet_DATA_CFGTAG_DEVCONFIG:    // подкключенное железо: dev_type-configured-DN 
            n = ParseCfgPacket_GetInt( pdata, n, &ival );
            sprintf( out, "Device cfg (0x%02x)", ival );
         break;
         case Packet_DATA_CFGTAG_RTLINK:    // линк ретранслятора ( для NetRT . TmNetScan )
            n = ParseCfgPacket_GetRTLinkData( pdata, n, &tcp );
            sprintf( out, "Link %s %d . %s: %d %s", tcp.name, tcp.port, tcp.NetRT.remote_ip, tcp.NetRT.remote_port, tcp.NetRT.configured?"":"(not configured)" );
         break;
         case Packet_DATA_CFGTAG_TXTINFO:   // текстовая информация в свободном виде
            n = ParseCfgPacket_GetStr( pdata, n, out, out_sz );
         break;
      }
      return n;
      }

      // возвращает короткое наименование типа серийного устройства для текстового вывода. 
      // Использует промежуточный буфер buf
      // вызывается из ParseTagAsString
      char *GetDeviceTypeName(int type, char *buf)
      {
      switch( type )
      {
         case DEVICE_TYPE_MBPS:
            sprintf( buf, "MBPS" );
         break;
         case DEVICE_TYPE_XRAYC:
            sprintf( buf, "XRAYC" );
         break;
         case DEVICE_TYPE_DIMS:
            sprintf( buf, "DIMS" );
         break;
         case DEVICE_TYPE_TERMO_S:
            sprintf( buf, "TermSlave" );
         break;
         case DEVICE_TYPE_TERMO_M:
            sprintf( buf, "TermMaster" );
         break;
         case DEVICE_TYPE_PFS:
            sprintf( buf, "PFS" );
         break;
         case DEVICE_TYPE_FDCY:
            sprintf( buf, "Faraday" );
         break;
         case DEVICE_TYPE_ASH_ST:
            sprintf( buf, "ASh-Common" );
         break;
         case DEVICE_TYPE_ANCSXY:
            sprintf( buf, "Autronics" );
         break;
         case DEVICE_TYPE_MVCAM:
            sprintf( buf, "MV Cam" );
         break;
         case DEVICE_TYPE_PTWUNI:
            sprintf( buf, "PTW Unidose" );
         break;
         default:
            sprintf( buf, "Unknown Device (type %d)", type );
         break;
      }
      return buf;
      }

      // определяет тип сервера по текстовой конструкции из конфига.
      // например "Tm_C", "4" - клиент томографа - в type будет записано SERVER_TYPE_TMC (4)
      int  TMDataServers_ParseServerType( char *buf )
      {
      int type;

      // пытаемся интерпретированть как маркер   
      type = Packet_CheckMarkerStr(buf);

      if( type == 0 )      // не удалось - просто как номер типа сервера
         type = atoi( buf );

      return type;      
      }

      // определяет тип подключения по текстовой конструкции из конфига.
      // например "TM", "2" - протокол TM_Protocol - в type будет записано SERVER_ConnectType_TMPROTOCOL (2)
      int  TMDataServers_ParseConnectType( char *buf )
      {
      int type=0;

      // пытаемся интерпретированть как маркер   
      if( !strncmp( SERVER_CONNECT_SIGNATURE_LNKLIB, buf, strlen(SERVER_CONNECT_SIGNATURE_LNKLIB)))
            return SERVER_ConnectType_LNKLIB;
      else if( !strncmp( SERVER_CONNECT_SIGNATURE_TMPROTOCOL, buf, strlen(SERVER_CONNECT_SIGNATURE_TMPROTOCOL)))
            return SERVER_ConnectType_TMPROTOCOL;
      else if( !strncmp( SERVER_CONNECT_SIGNATURE_DIRECT, buf, strlen(SERVER_CONNECT_SIGNATURE_DIRECT)))
            return SERVER_ConnectType_DIRECT;
      else if( !strncmp( SERVER_CONNECT_SIGNATURE_IMITATION, buf, strlen(SERVER_CONNECT_SIGNATURE_IMITATION)))
            return SERVER_ConnectType_IMITATION;
      else if( !strncmp( SERVER_CONNECT_SIGNATURE_TCPCUSTOM, buf, strlen(SERVER_CONNECT_SIGNATURE_TCPCUSTOM)))
            return SERVER_ConnectType_TCPCUSTOM;
      else if( !strncmp( SERVER_CONNECT_SIGNATURE_RS232, buf, strlen(SERVER_CONNECT_SIGNATURE_RS232)))
            return SERVER_ConnectType_RS232;

      if( type == 0 )      // не удалось - просто как номер 
         type = atoi( buf );

      return type;      
      }

      // тип подключения в строку, как метку (для конфига)
      int  TMDataServers_PrintConnectType( TMDataServer *ds, char *buf )
      {
      if( ds.ConnectType == SERVER_ConnectType_LNKLIB )
         strcpy( buf, SERVER_CONNECT_SIGNATURE_LNKLIB );
      else if( ds.ConnectType == SERVER_ConnectType_TMPROTOCOL )
         strcpy( buf, SERVER_CONNECT_SIGNATURE_TMPROTOCOL );
      else if( ds.ConnectType == SERVER_ConnectType_TCPCUSTOM )
         strcpy( buf, SERVER_CONNECT_SIGNATURE_TCPCUSTOM );
      else if( ds.ConnectType == SERVER_ConnectType_DIRECT )
         strcpy( buf, SERVER_CONNECT_SIGNATURE_DIRECT );
      else if( ds.ConnectType == SERVER_ConnectType_IMITATION )
         strcpy( buf, SERVER_CONNECT_SIGNATURE_IMITATION );
      else if( ds.ConnectType == SERVER_ConnectType_RS232 )
         strcpy( buf, SERVER_CONNECT_SIGNATURE_RS232 );
      else // новый какой-то тип или не определен
         sprintf( buf, "%d", ds.ConnectType );

      return 0;
      }

      // проверка состояния удаленного хоста. Вызывается по таймеру
      // таймер генерится в TMDataServers_StartUpdateTimer( ds, 10.0, TMDataServers_PingServiceTimerCB );
      // используется VisualControl: VC_AddServer
      int CVICALLBACK TMDataServers_PingServiceTimerCB (int panel, int control, int event,
         void *callbackData, int eventData1, int eventData2)
      {
      TMDataServer *ds = (TMDataServer *) callbackData;
      //mprntf("Ping CB", ds.ip);
      if( TMDataServers_IsConnected( ds ) == TMDataServer_CONNECTED )
      {
         if(0 != TMDataServers_Ping(ds) )
            DisconnectFix( ds );
      }
      return 0;
      }

      // пинг удаленного хоста
      // вызывается из TMDataServers_PingServiceTimerCB
      // 0 - пинг успешен
      int TMDataServers_Ping(void *pds)
      {
      int available;
      VC_Control *vcc;  
      char buf[1024];
      char buf2[1024];

      TMDataServer *ds = (TMDataServer *) pds;

      ds.update++;
      int res = InetPing (ds.ip, &available, 1.0);

      //mprntf("Ping %s", ds.ip);
      //if( res == 0 ) 

      // анализируем результаты пинга  
      if( available ) 
      {
         ds.COM.PortOpen = 1;
         res = 0; // пинг успешен
         ds.TimerTicks = 0;
      }
      else
         res = -1;

      //  перебираем все датчики данного сервера
      for(int i=0; i<ds.ControlsCount; i++)
      {
         vcc = &ds.Controls[i];
         buf[0] = '\0';          
         switch( vcc.type )
         {
            case CONTROL_TYPE_UPDATE_NUM: // нопер апдейта дата-сервера 
               vcc.control_val = ds.update;
               continue;

            case CONTROL_TYPE_TEXTLABEL:
               continue;

            case CONTROL_TYPE_HOST:    // хост
               if( res ) // неудачное подключение
               {
                  vcc.device_status = DEVICE_STATUS_CRACKED;
               }
               else
               {
                  vcc.device_status = DEVICE_STATUS_OK;
               }
               sprintf( buf, "%s: %s", vcc.name, 
                      vcc.device_status==DEVICE_STATUS_OK?"OK":"No responce" );
            break;
         }

         if( configParams.config_flag ) 
            GetDefaultControlName(vcc, buf);
         GetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_LABEL_TEXT , buf2);
         if( strcmp(buf, buf2) )
            SetCtrlAttribute (vcc.panelHandle, vcc.ControlID, ATTR_LABEL_TEXT , buf);
      }

      return res; // не срослось
      }
      */
   #endregion
   }
}
