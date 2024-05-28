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
namespace Scentient 
{

    public class ScentientDevice : MonoBehaviour
    {
        public event Action<string> OnStatusChangedEvent;
        public event Action<States> OnStateChangedEvent;
        public event Action<bool> OnButtonChangedEvent;

        public UnityEvent OnConnectedEvent;
    
        public readonly ScentTable scentTable = new ScentTable();

        public bool verbose;
        public bool autoConnectToLastDevice;

        const string DeviceName = "Scentient Escents";
        const string ServiceUUID = "EDDD4E1F-16FA-4C7C-AD7F-171EDBD7EFF7";
        const string ScentMessageUUID = "45335526-67BA-4D9D-8CFB-C3D97E8D8208";

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

        Int16[] channelScentIds; 

        const string LastAddressKey = "last_address";

        const string ComPortFilename = "port.txt";

        SerialPort m_serialPort;
    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        const bool isWindows = true;

    #else 
        const bool isWindows = false;
    #endif


        public enum SignalStrength {
            None = 0,
            Weak = 1,
            Medium = 2,
            Strong = 3,
        }

        public SignalStrength SignalStrengthLevel
        {
            get
            {
                if(_rssi<-80){
                    return SignalStrength.Weak;
                }else if(_rssi<-60){
                    return SignalStrength.Medium;
                }else if(_rssi<-40){
                    return SignalStrength.Strong;
                }else{
                    return SignalStrength.None;
                }
            }
        }

        [System.Serializable]
        public class ScentMessage 
        {
            public byte channel;
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
        private bool _rssiOnly = false;
        private int _rssi = 0;
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

        public States State { get  { return _state; } }

        void Reset()
        {
            _connected = false;
            _timeout = 0f;
            _state = States.None;
            _deviceAddress = null;
            _foundButtonUUID = false;
            _foundLedUUID = false;
            _rssi = 0;
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
            BleManager.Instance.Initialize(); 
            
            if( autoConnectToLastDevice && PlayerPrefs.HasKey(LastAddressKey) ){
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
            Connect();   
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
            if(isWindows && m_serialPort.IsOpen){
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
                        UpdateChannelScentIds();                   
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

        private void UpdateChannelScentIds()
        {
            
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
                        BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound, 10000));
    /*                     LEGACY_BLE_LIBRARY.ScanForPeripheralsWithServices( new string[]{"180A"}, (address, name) =>
                        {
                            // if your device does not advertise the rssi and manufacturer specific data
                            // then you must use this callback because the next callback only gets called
                            // if you have manufacturer specific data
                            if (!_rssiOnly)
                            {
                                if (name.Contains(DeviceName))
                                {
                                    StatusMessage = "Device Discovered";

                                    // found a device with the name we want
                                    // this example does not deal with finding more than one
                                    _deviceAddress = address;
                                    SetState(States.Connect, 0.5f);
                                }
                            }

                        }, (address, name, rssi, bytes) =>
                        {
                            // use this one if the device responses with manufacturer specific data and the rssi

                            if (name.Contains(DeviceName))
                            {
                                StatusMessage = "Device Discovered";

                                if (_rssiOnly)
                                {
                                    _rssi = rssi;
                                }
                                else
                                {
                                    // found a device with the name we want
                                    // this example does not deal with finding more than one
                                    _deviceAddress = address;
                                    SetState(States.Connect, 0.5f);
                                }
                            }

                        }, _rssiOnly); // this last setting allows RFduino to send RSSI without having manufacturer data */

                        
                        break;
                    case States.Connect:
                        StatusMessage = "Connecting...";

                        // set these flags
                        _foundButtonUUID = false;
                        _foundLedUUID = false;

                        BleManager.Instance.QueueCommand( new ConnectToDevice(_deviceAddress,OnConnected,OnDisconnected,OnServiceDiscovered,OnCharacteristicDiscovered) );

                        // LEGACY_BLE_LIBRARY.ConnectToPeripheral(_deviceAddress, null, null, (address, serviceUUID, characteristicUUID) =>
                        // {
                        //     StatusMessage = "Device Connected";

                        //     LEGACY_BLE_LIBRARY.StopScan();

                        //     if (IsEqual(serviceUUID, ServiceUUID))
                        //     {
                        //         //StatusMessage = "Found Service UUID";

                        //         _foundButtonUUID = _foundButtonUUID || IsEqual(characteristicUUID, ButtonUUID);
                        //         _foundLedUUID = _foundLedUUID || IsEqual(characteristicUUID, ScentMessageUUID);

                        //         // if we have found both characteristics that we are waiting for
                        //         // set the state. make sure there is enough timeout that if the
                        //         // device is still enumerating other characteristics it finishes
                        //         // before we try to subscribe
                        //         if (_foundButtonUUID && _foundLedUUID)
                        //         {
                        //             _connected = true;
                        //             SetState(States.RequestMTU, 2f);
                        //         }
                        //     }
                        // }, (string _deviceAddress)=>{
                        //     StatusMessage = "Device Disconnected";
                        //     _connected = false;
                        //     SetState(States.Scan, 0.5f);
                        // });
                        break;

                    case States.RequestMTU:
                        StatusMessage = "Requesting MTU";

                        // LEGACY_BLE_LIBRARY.RequestMtu(_deviceAddress, 185, (address, newMTU) =>
                        // {
                        //     StatusMessage = "MTU set to " + newMTU.ToString();

                        //     SetState(States.Subscribe, 0.1f);
                        // });
                        break;

                    case States.Subscribe:
                        StatusMessage = "Subscribing...";
                        BleManager.Instance.QueueCommand(new SubscribeToCharacteristic(_deviceAddress,ServiceUUID,ButtonUUID,(byte[] data)=>{
                            //button state changed
                            ProcessButton(data);
                        },customGatt:true));

                        for(int i=0;i<ChannelScentIdCharacterisiticUUIDs.Length;i++){
                            var index=i;
                            BleManager.Instance.QueueCommand(new SubscribeToCharacteristic(_deviceAddress,ServiceUUID,ChannelScentIdCharacterisiticUUIDs[i],(byte[] data)=>{
                            //button state changed
                            
                            UpdateChannelScentIds( index, BitConverter.ToInt16(data,0) );
                        },customGatt:true));
                        }

                        StatusMessage = "Device Ready";
                        SetState(States.Ready, 0.1f);

                        // LEGACY_BLE_LIBRARY.SubscribeCharacteristicWithDeviceAddress(_deviceAddress, ServiceUUID, ButtonUUID, (notifyAddress, notifyCharacteristic) =>
                        // {
                        //     StatusMessage = "Device Ready";

                        //     // read the initial state of the button
                        //     LEGACY_BLE_LIBRARY.ReadCharacteristic(_deviceAddress, ServiceUUID, ButtonUUID, (characteristic, bytes) =>
                        //     {
                        //         ProcessButton(bytes);
                        //     });

                        //     SetState(States.ReadRSSI, 1f);

                        // }, (address, characteristicUUID, bytes) =>
                        // {
                        //     if (_state != States.Ready)
                        //     {
                        //         // some devices do not properly send the notification state change which calls
                        //         // the lambda just above this one so in those cases we don't have a great way to
                        //         // set the state other than waiting until we actually got some data back.
                        //         // The esp32 sends the notification above, but if yuor device doesn't you would have
                        //         // to send data like pressing the button on the esp32 as the sketch for this demo
                        //         // would then send data to trigger this.
                        //         StatusMessage = "Device Ready";

                        //         SetState(States.ReadRSSI, 1f);
                        //     }

                        //     // we received some data from the device
                        //     ProcessButton(bytes);
                        // });
                        break;
                    case States.Ready:
                        OnConnectedEvent.Invoke();
                        break;
                    case States.Unsubscribe:
                        // LEGACY_BLE_LIBRARY.UnSubscribeCharacteristic(_deviceAddress, ServiceUUID, ButtonUUID, null);
                        SetState(States.Disconnect, 4f);
                        break;

                    case States.Disconnect:
                        StatusMessage = "Device disconnect.";

                        if (_connected)
                        {
                            // LEGACY_BLE_LIBRARY.DisconnectPeripheral(_deviceAddress, (address) =>
                            // {
                            //     StatusMessage = "Device disconnected";
                            //     LEGACY_BLE_LIBRARY.DeInitialize(() =>
                            //     {
                            //         _connected = false;
                            //         _state = States.None;
                            //     });
                            // });
                        }
                        else
                        {
                            // LEGACY_BLE_LIBRARY.DeInitialize(() =>
                            // {
                            //     _state = States.None;
                            // });
                        }
                    break;                            
                }
            }
        }

        private void OnCharacteristicDiscovered(string deviceAddress, string serviceAddress, string characteristicAddress)
        {
            //throw new NotImplementedException();
        }


        private void OnServiceDiscovered(string deviceAddress, string serviceAddress)
        {
            if(serviceAddress == ServiceUUID){
                SetState(States.Subscribe, 0.1f);
            }
        }


        private void OnDisconnected(string deviceAddress)
        {
            //throw new NotImplementedException();
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
                        
            scentTable.Load();
            scentTable.loadSucessfulEvent += OnScentTableLoaded;

            if(isWindows){
                StartProcessWin();
            }
            else {
                StartProcess();
            }
            
        }

        private void OnScentTableLoaded()
        {
            
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
        /// Work in Progress
        /// Trigger a scent to be emitted on the sending device. 
        /// </summary>
        /// <param name="scentName">The unique scent name</param>
        /// <param name="intensity">value between 0-255, 0 being off</param>
        /// <param name="duration">in milliseconds</param>
        /// <returns>If the scent can not be found, or the scent is not availible on the device, returns false, otherwise true</returns>
        public bool SendScentMessage(string scentName, byte intensity, UInt16 duration)
        {
            if(!scentTable.ScentTableLoadedSuccessfully){
                Debug.LogWarning("Unable to look up channel by scent name, ScentTable has not been loaded");
                return false;
            }

            //get scent id from name

            //get channel from scent id on device

            //if no channel has matching scent id, log warning and return false

            //send scent message
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
                // LEGACY_BLE_LIBRARY.WriteCharacteristic(_deviceAddress, ServiceUUID, ScentMessageUUID, messageBytes, messageBytes.Length, true, (characteristicUUID) =>
                // {
                //     LEGACY_BLE_LIBRARY.Log("Write Succeeded");
                // });
            }
        }
    }

}