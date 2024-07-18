/* This is a simple example to show the steps and one possible way of
 * automatically scanning for and connecting to a device to receive
 * notification data from the device.
 *
 * It works with the esp32 sketch included at the bottom of this source file.
 */

using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Text;
using Android.BLE;
using Android.BLE.Commands;

using System.IO.Ports;
using System.IO;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scentient 
{

    public class ScentientDevice : MonoBehaviour
    {
        public event Action<string> OnStatusChangedEvent;
        public event Action<States> OnStateChangedEvent;
        public event Action<bool> OnButtonChangedEvent;

        public event Action<int, string> OnChannelScentsUpdatedEvent;

        public UnityEvent OnConnectedEvent;
    
        public readonly ScentTable scentTable = new ScentTable();

        public bool verbose;
        public bool reconnectToLastDevice = true;

        public bool autoConnectOnStart;

        public bool autoRequestPermissionsOnConnect = true;

        const string DeviceName = "Scentient Escents";
        const string ServiceUUID = "eddd4e1f-16fa-4c7c-ad7f-171edbd7eff7";
        const string ScentMessageUUID = "45335526-67ba-4d9d-8cfb-c3d97e8d8208";

        // long version of short code "04C3";
        const string ButtonUUID = "000004C3-0000-1000-8000-00805f9b34fb"; 

        /// <summary>
        /// Each channel has a characteristic to store which scent is currently loaded into the device
        /// </summary>
        readonly string[] ChannelScentIdCharacterisiticUUIDs = new string[]{
            "527bac61-60dc-4a73-aa99-d37ac6242931",
            "88a45d22-9a11-48de-8789-ae339f714132",
            "3f83c8bc-e98b-4959-aaa9-526297a1a5be",
            "0c4bd73e-7111-4a03-a900-aabad7c3e34c"
        };

        const int _numChannels = 4;
        Int16[] _channelScentIds = new Int16[_numChannels]; 
        string[] _channelScentNames = new string[_numChannels];

        const string LastAddressKey = "last_address";

        const string ComPortFilename = "port.txt";

        SerialPort m_serialPort;
    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        const bool isWindows = true;

    #else 
        const bool isWindows = false;
    #endif


        // public enum SignalStrength {
        //     None = 0,
        //     Weak = 1,
        //     Medium = 2,
        //     Strong = 3,
        // }

        // public SignalStrength SignalStrengthLevel
        // {
        //     get
        //     {
        //         if(_rssi<-80){
        //             return SignalStrength.Weak;
        //         }else if(_rssi<-60){
        //             return SignalStrength.Medium;
        //         }else if(_rssi<-40){
        //             return SignalStrength.Strong;
        //         }else{
        //             return SignalStrength.None;
        //         }
        //     }
        // }

        [System.Serializable]
        
        public class ScentMessage 
        {
            /// <summary>
            /// channel value from 1-4 for Escents device
            /// </summary>
            public byte channel;

            /// <summary>
            /// 0, no intensity
            /// 255, some intensity
            /// </summary>
            public byte intensity;
            public ushort duration;

            public ScentMessageStruct ToStruct()
            {
                return new ScentMessageStruct(){
                    channel = channel, intensity = intensity, duration = duration
                };
            }

            public byte[] ToBytes()
            {
                var bytes=new byte[SizeInBytes()];                
                bytes[0] = channel;
                bytes[1] = intensity;            
                BitConverter.TryWriteBytes(new Span<byte>(bytes,2,2),duration);          
                return bytes;
            }

            public static int SizeInBytes()
            {
                return 4;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ScentMessageStruct 
        {
            public byte channel;
            public byte intensity;
            public ushort duration;

            public byte[] ToBytes()
            {
                var bytes=new byte[SizeInBytes()];
                bytes[0] = channel;
                bytes[1] = intensity;            
                BitConverter.TryWriteBytes(new Span<byte>(bytes,2,2),duration);          
                return bytes;
            }

            public static int SizeInBytes()
            {
                return 4;
            }
        }

        public enum States
        {
            None,
            Scan,
            ScanRSSI,
            ReadRSSI,
            Connect,
            RequestMTU,
            Subscribe,
            Unsubscribe,
            Disconnect,
            Ready,
        }

        private bool _connected = false;
        private float _timeout = 0f;
        private States _state = States.None;
        private string _deviceAddress;
        private bool _foundButtonUUID = false;
        private bool _foundLedUUID = false;
        private string _status;
        public string StatusMessage
        {
            private set
            {
                if(_status!=value){
                    _status = value;
                    OnStatusChangedEvent?.Invoke(_status);
                }
            }
            get {
                return _status;
            }
        }

        public string[] ChannelScentNames {
            get {
                return _channelScentNames;
            }
        }

        public States State { get  { return _state; } }


        void Awake()
        {
            scentTable.loadSucessfulEvent += OnScentTableLoaded;
        }


        void Reset()
        {
            _connected = false;
            _timeout = 0f;
            _state = States.None;
            _deviceAddress = null;
            _foundButtonUUID = false;
            _foundLedUUID = false;
        }

        void SetState(States newState, float timeout)
        {
            if(_state!=newState){
                Debug.Log($"Changing state to {newState}");
                _state = newState;                      
                OnStateChangedEvent?.Invoke(_state);
            }
            _timeout = timeout;
        }

        void StartProcessWin()
        {
            if(_connected){
                return;
            }
            Reset();
            var names = SerialPort.GetPortNames();
            var comPortFilePath = Path.Join( Application.dataPath, ComPortFilename );
            int portIndex = 0;
            if(names.Length==0){
                SetState(States.None,1f);
                StatusMessage = "No Device Found";
                return;
            }
            if(File.Exists(comPortFilePath)){
                try{
                    
                    var portName = File.ReadAllText(comPortFilePath).Trim();
                    portIndex = Array.IndexOf( names, portName );
                    if(portIndex==-1){
                        Debug.LogWarning($"Port not found: {portName}");
                        portIndex=0;
                    }
                }
                catch(Exception){

                }
            }
            else {
                Debug.LogWarning($"Com port file {comPortFilePath} not found");
            }
            if(m_serialPort!=null){
                if(m_serialPort.IsOpen){
                    m_serialPort.Close();
                }
                m_serialPort.PortName = names[portIndex];
            }
            else {
                m_serialPort = new SerialPort(names[portIndex],57600);
                m_serialPort.RtsEnable = true;
                m_serialPort.DtrEnable = true;
                m_serialPort.Handshake = Handshake.None;
                m_serialPort.ReadTimeout = 10;
                //m_serialPort.WriteTimeout = 1000;
            }

            try{
                m_serialPort.Open();
                SetState(States.Connect,0.5f);
            }
            catch(IOException e) {
                StatusMessage = e.Message;            
                SetState(States.Disconnect,0.5f);
            }
        }

        void StartProcess()
        {        
            if(_connected){
                return;
            }
            Reset();
            
            if(autoRequestPermissionsOnConnect){
                var appPerm = GameObject.FindAnyObjectByType<AppPermissions>();
                
                if(appPerm==null){
                    appPerm = gameObject.AddComponent<AppPermissions>();
                }
                else {
                    appPerm.allPermissionsGrantedEvent.RemoveListener(Connect);
                }
                if(!appPerm.AllPermissionsGranted){
                    // If we need to request permissions, abort connecting until permissions are granted                    
                    appPerm.allPermissionsGrantedEvent.AddListener( Connect );
                    appPerm.RequestPermissions();
                    return;
                }

            }

            BleManager.Instance.Initialize(); 
            
            if( reconnectToLastDevice && PlayerPrefs.HasKey(LastAddressKey) ){
                _deviceAddress = PlayerPrefs.GetString(LastAddressKey);
                SetState(States.Connect,0.5f);
            }
            else {
                SetState(States.Scan, 0.1f);
            }
        }

        // Use this for initialization
        void Start()
        {
            if(verbose){
                var adapter = BleManager.Instance.GetComponentInChildren<BleAdapter>();
                adapter.OnMessageReceived += OnBLEMessageRecieved;
                adapter.OnErrorReceived += OnBLEMessageErrorReceived;                
            }
            if(autoConnectOnStart){
                Connect();
            }
        }

        private void OnBLEMessageRecieved(BleObject obj)
        {
            Debug.Log($"BLE Message command={obj.Command}");
        }

        private void OnBLEMessageErrorReceived(string errorMessage)
        {
            Debug.Log($"BLE Error {errorMessage}");
        }

        void OnDestroy()
        {
            if(isWindows && m_serialPort!=null && m_serialPort.IsOpen){
                m_serialPort.Close();
            }

        }

        private void ProcessButton(byte[] bytes)
        {
            OnButtonChangedEvent?.Invoke( bytes[0] != 0x00 );
        }

        // Update is called once per frame
        void Update()
        {
            if(isWindows){
                ProcessStateWindows();
            }
            else {
                ProcessState();
            }
        }

        void ProcessStateWindows()
        {
            if (_timeout > 0f)
            {
                _timeout -= Time.deltaTime;
                if (_timeout >= 0f)
                {
                    return;
                }
                switch(_state){
                    case States.None:
                        break;
                    case States.Connect:
                        StatusMessage = "Connecting...";
                        if(m_serialPort.IsOpen){
                            _connected = true;
                            SetState( States.Ready, 1f );                        
                            ReadSerialRoutine();
                        }
                        else {
                            SetState( States.Connect, 0.2f);                   
                        }
                    break;
                    case States.Ready:
                        StatusMessage = "Connected";
                        PlayerPrefs.SetString(LastAddressKey,_deviceAddress); 
                                       
                    break;
                    case States.Unsubscribe:
                        SetState( States.Disconnect,Time.deltaTime );
                    break;
                    case States.Disconnect:                    
                        if(m_serialPort.IsOpen){
                            m_serialPort.Close();                        
                        }
                        _connected = false;
                        SetState(States.None,0.1f);
                    break;
                }
            }
        }

        private void UpdateChannelScentIds(int channel, Int16 scentId)
        {
            _channelScentIds[channel] = scentId;            
            var scentName = _channelScentNames[channel] = scentTable.GetScentNameById(scentId);
            Debug.Log($"UpdateChannelScentIds channel={channel} scentId={scentId} scentName={_channelScentNames[channel]}");
            OnChannelScentsUpdatedEvent.Invoke(channel,scentName);
        }

        async void ReadSerialRoutine()
        {
            var buffer = new byte[4096];
            int readBytes = 0;
            int currentIndex = 0;
            try {
                while (true)
                {   
                    try {
                        readBytes = await m_serialPort.BaseStream.ReadAsync(buffer, currentIndex, buffer.Length-currentIndex);
                    }    
                    catch(TimeoutException){
                        readBytes = 0;
                    }
                    currentIndex += readBytes;
                    if(!m_serialPort.IsOpen){
                        SetState(States.Disconnect,0.5f);
                        break;
                    }
                    if(readBytes==0 && currentIndex!=0){                        
                        string recieved = System.Text.Encoding.Default.GetString(buffer,0,currentIndex);
                        Debug.Log($"Message received from device: {recieved}");
                        currentIndex = 0;
                    }
                }
            }
            catch(IOException){

                m_serialPort.Close();    

                StatusMessage = "Disconnected";
                SetState(States.None,0.1f);
                _connected = false;
            }
                
            
        }

        void ProcessState()
        {
            if (_timeout > 0f)
            {
                _timeout -= Time.deltaTime;
                if (_timeout >= 0f)
                {
                    return;
                }

                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        break;

                    case States.Scan:
                        StatusMessage = "Scanning...";
                        BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound,OnScanFinished, 10000));
                        break;
                    case States.Connect:
                        StatusMessage = "Connecting...";

                        // set these flags
                        _foundButtonUUID = false;
                        _foundLedUUID = false;

                        BleManager.Instance.QueueCommand( new ConnectToDevice(_deviceAddress,OnConnected,OnDisconnected,OnServiceDiscovered,OnCharacteristicDiscovered) );

                        break;

                    case States.RequestMTU:
                        StatusMessage = "Requesting MTU";
                        break;
                    case States.Subscribe:
                        for(int i=0;i<ChannelScentIdCharacterisiticUUIDs.Length;i++){
                            var index=i;
                            // Subscribe to channel scent id characteristic. 
                            // This will be needed when hot swapping of scents is available in the future.
                            // BleManager.Instance.QueueCommand(new SubscribeToCharacteristic(_deviceAddress,ServiceUUID,ChannelScentIdCharacterisiticUUIDs[i],(byte[] data)=>{
                            //     UpdateChannelScentIds( index, BitConverter.ToInt16(data,0) );
                            // },customGatt:true));

                            //Getting scent ids
                            BleManager.Instance.QueueCommand( new ReadFromCharacteristic(_deviceAddress,ServiceUUID,ChannelScentIdCharacterisiticUUIDs[i],(byte[] data)=>{
                                Debug.Log($"Valued Received {data}");
                                UpdateChannelScentIds( index, BitConverter.ToInt16(data,0) );
                            },customGatt:true));
                        }

                        StatusMessage = "Device Ready";
                        SetState(States.Ready, 0.1f);

                        break;
                    case States.Ready:
                        OnConnectedEvent.Invoke();
                        break;
                    case States.Unsubscribe:
                        SetState(States.Disconnect, 4f);
                        break;

                    case States.Disconnect:
                        StatusMessage = "Device disconnect.";

                        if (_connected)
                        {
                            // Currently disconnect is unimplemented in BLE library
                            // Once implmented, call disconnect and remove event listeners
                        }
                    break;                            
                }
            }
        }

        private void OnCharacteristicDiscovered(string deviceAddress, string serviceAddress, string characteristicAddress)
        {
        }


        private void OnServiceDiscovered(string deviceAddress, string serviceAddress)
        {
            if(serviceAddress == ServiceUUID){
                SetState(States.Subscribe, 0.1f);
            }
        }


        private void OnDisconnected(string deviceAddress)
        {
            Debug.Log($"OnDisconnected");
        }


        private void OnConnected(string deviceAddress)
        {
            _connected = true;
            Debug.Log($"OnConnected");
            SetState(States.Subscribe, 0.1f);
        }


        private void OnDeviceFound(string deviceAddress, string name)
        {
            if( !string.IsNullOrEmpty(name) && name.Contains(DeviceName) ){
                _deviceAddress = deviceAddress;
                SetState(States.Connect,0.1f);
            }
        }

        private void OnScanFinished()
        {
            Debug.Log($"OnScanFinished: state={State}");
            if(State==States.Scan){
                SetState(States.None,0.1f);
            }
        }


        private static string ToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in bytes)
            {
                sb.Append(item.ToString("X2"));
            }
            return sb.ToString();
        }

        string FullUUID(string uuid)
        {
            string fullUUID = uuid;
            if (fullUUID.Length == 4)
                fullUUID = "0000" + uuid + "-0000-1000-8000-00805f9b34fb";

            return fullUUID;
        }

        bool IsEqual(string uuid1, string uuid2)
        {
            if (uuid1.Length == 4)
                uuid1 = FullUUID(uuid1);
            if (uuid2.Length == 4)
                uuid2 = FullUUID(uuid2);

            return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
        }

        public void Connect()
        {
            if(!scentTable.Loaded){
                scentTable.Load();
            }

            if(isWindows){
                StartProcessWin();
            }
            else {
                StartProcess();
            }
            
        }

        private void OnScentTableLoaded()
        {
            if(verbose){
                Debug.Log("Scent table loaded");
                StringBuilder sb = new StringBuilder();
                for(int i=0;i<scentTable.RowCount();i++){
                    if(scentTable.TryGetInt(0,i,out int id)){
                        
                        sb.Append($"{id}={scentTable.GetString(1,i)}");
                    }
                }
                Debug.Log(sb.ToString());
            }

        }

        public async Task<string[]> GetChannelScentNamesAsync()
        {
            while(!_connected || !scentTable.Loaded){
                await Task.Yield();            
            }
            return GetChannelScentNames();
        }

        public string[] GetChannelScentNames()
        {
            if(!_connected){
                Debug.LogWarning("Unable to get scene channel names: Not connected to device");
                return null;
            }
            if(!scentTable.Loaded){
                Debug.LogWarning("Unable to get scene channel names: scent table not loaded yet");
                return null;
            }
            Dictionary<int,string> idToScentName = new Dictionary<int,string>();
            int len = scentTable.RowCount();
            for(int i=0;i<len;i++){
                if( scentTable.TryGetInt(0,i, out int id) ){
                    string name=scentTable.GetString(1,i);
                    idToScentName[id]=name;
                }
            }

            var names = _channelScentIds.Select<short,string>((id)=>{
                if(!idToScentName.ContainsKey(id)){
                    return "scent not found";
                }
                return idToScentName[id];
            });
            return names.ToArray();
        } 

        public void Disconnect()
        {
            if(_connected){
                SetState(States.Unsubscribe, 4f);
                _connected = false;
            }
        }

    /// <summary>
    /// Emits a scent from one of the 4 scent emitters, with the provided intensity and duration 
    /// </summary>
    /// <param name="channel">channel value 1-4</param>
    /// <param name="intensity">value between 0-255, 0 being off</param>
    /// <param name="duration">in milliseconds</param>
        public void SendScentMessage(byte channel, byte intensity, UInt16 duration)
        {
            ScentMessage scentMessage = new ScentMessage(){
                channel = channel,
                intensity = intensity,
                duration = duration
            };
            SendScentMessage(scentMessage);

        }


        /// <summary>
        /// Emits a scent for a fixed period of time.
        /// Any scent being emitted can be stopped by calling this method with a duration of 0
        /// </summary>
        /// <param name="scentName">The name of the scent, can include spaces, should be one of the names in the following table https://api.scentient.tech/scent-table_en.csv</param>
        /// <param name="duration">duration in seconds</param>
        /// <returns></returns>
        public bool EmitScent(string scentName, float duration){
            return EmitScent(scentName,1f,duration);
        }

                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scentName">The name of the scent, can include spaces, should be one of the names in the following table https://api.scentient.tech/scent-table_en.csv</param>
        /// <param name="intensity">Value from 0 to 1</param>
        /// <param name="duration">duration in seconds</param>
        /// <returns></returns>
        public bool EmitScent(string scentName, float intensity, float duration)
        {
            if(!scentTable.ScentTableLoadedSuccessfully){
                Debug.LogWarning("Unable to look up channel by scent name, ScentTable has not been loaded");
                return false;
            }

            if(duration<0){
                Debug.LogWarning("Duration must be a positive ");
                return  false;
            }

            //sanitising the input
            scentName = scentName.Substring(0,Mathf.Min(64,scentName.Length)).Trim().ToLower();

            if(!scentTable.GetScentIdByName(scentName, out int id)){
                Debug.LogWarning($"Scent \"{scentName}\" not found in scent table.");
                return false;
            }
            else if(verbose){
                Debug.Log($"Scent found in scent table {scentName}=={id}");
            }

            //get channel from scent id on device
            var channel = System.Array.IndexOf( _channelScentNames, scentName )+1;
            //if no channel has matching scent id, log warning and return false
            if(channel==0){
                Debug.LogWarning($"Scent {scentName} not found on device");
                return false;
            }
            else {
                Debug.Log($"Scent channel found with scent id {scentName}=={channel}");
            }

            var durationMillis = Mathf.FloorToInt(duration*1000);
            if( ushort.MaxValue < durationMillis ){
                durationMillis = ushort.MaxValue;
            }

            byte intensityByte = (byte)Mathf.RoundToInt( 0xff * Mathf.Clamp01(intensity) );

            //send scent message
            ScentMessage scentMessage = new ScentMessage(){
                channel = (byte)channel,
                intensity = intensityByte,
                duration = (ushort) durationMillis
            };
            SendScentMessage(scentMessage);
            return true;
        }

        public void SendScentMessage(ScentMessage scentMessage){
            var messageBytes = scentMessage.ToBytes();
            
            if(verbose){
                Debug.Log($"ScentMessage {ToHexString(messageBytes)}");                    
            }

            if(isWindows){
                UInt16 messageId = Convert.ToUInt16(ScentMessageUUID,16); 
                if(!m_serialPort.IsOpen){
                    return;
                }
                byte[] messageBuf = new byte[ScentMessageStruct.SizeInBytes()+2];
                BitConverter.TryWriteBytes(new Span<byte>(messageBuf),messageId);
                Array.Copy(messageBytes,0,messageBuf,2,messageBytes.Length);
                m_serialPort.BaseStream.Write(messageBuf,0,messageBuf.Length);
                m_serialPort.BaseStream.Flush();                            
            }
            else {
                BleManager.Instance.QueueCommand(new WriteToCharacteristic(_deviceAddress,ServiceUUID,ScentMessageUUID,messageBytes,customGatt:true));

            }
        }

        public Dictionary<short, string> GetScentDict()
        {
            Dictionary<short,string> result = new Dictionary<short, string>(4);
            for(int i=0;i<scentTable.RowCount();i++){
                string val=string.Empty;
                var success = scentTable.TryGetInt(0,i, out int key) && scentTable.TryGetString(1,i, out val);
                if(!success){
                    continue;
                }
                result.Add((short)key,val);
            }
            return result;
        }

        public void SetChannelScent(int channelToSet, short scentId)
        {
            _channelScentIds[channelToSet]=scentId;
            var scentName = _channelScentNames[channelToSet] = scentTable.GetScentNameById(scentId);
            if(_connected){
                var messageBytes = BitConverter.GetBytes(scentId);
                BleManager.Instance.QueueCommand(new WriteToCharacteristic(_deviceAddress,ServiceUUID,ChannelScentIdCharacterisiticUUIDs[channelToSet],messageBytes,customGatt:true));
                OnChannelScentsUpdatedEvent.Invoke(channelToSet,scentName);
            }
            else {
                Debug.LogWarning($"Cannot set channel scent, not connected to device");
            }
        }
    }

}