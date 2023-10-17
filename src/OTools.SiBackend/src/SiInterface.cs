using SPORTident;
using SPORTident.Communication;
using SPORTident.Communication.UsbDevice;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;



namespace OTools.SiBackend
{
    public enum Action
    {
        NotSet,
        NewDeviceBattery,
        NewSiacBattery,
        ReadCards,
        ViewPunch,
        FirmwareUpdate,
        StartBootFirmware,
        ConfigCards,
        ChangeBaudrate,
        Clock
    }

    public class SiInterface
    {
        private DeviceInfo _currentDevice;
        private readonly Communication _comm = new Communication();
        private TargetDevice _currentTargetDevice = TargetDevice.Direct;
        private Action _followUpAction;

        private Timer _timer = new Timer(100);

        public SiInterface()
        {
            _comm = new Communication();

            _comm.DeviceStateChanged += SiComm_DeviceStateChanged;
            _comm.StationConfigRead += SiComm_StationConfigRead;
            _comm.StationConfigFailed += SiComm_StationConfigFailed;
            _comm.StationConfigWritten += SiComm_StationConfigWritten;
            _comm.ProgressCompleted += SiComm_ProgressCompleted;
            _comm.CommunicationFailed += SiComm_CommunicationFailed;
            _comm.CommunicationRetried += SiComm_CommunicationRetried;
            _comm.BackupMemoryReadCompleted += SiComm_BackupReadCompleted;
            _comm.BackupReadProgressChanged += SiComm_BackupReadProgressChanged;
            _comm.SiCardReadProgressChanged += SiComm_SiCardReadProgressChanged;
            _comm.SiCardReadCompleted += SiComm_SiCardReadCompleted;
            _comm.TriggerPunchReceived += SiComm_TriggerPunchReceived;
            _comm.SiCardIn += Comm_SiCardIn;
            _comm.SiCardOut += Comm_SiCardOut;

            _timer.Elapsed += FollowUpAction_Do;
        }

        public bool OpenSiComm(DeviceInfo deviceInfo, TargetDevice targetDevice, bool keepCurrentBaudrate = false)
        {
            if (!CloseSiComm())
                return false;

            try
            {
                _comm.DeviceConnection = deviceInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set device to {deviceInfo}");
                Console.WriteLine(ex.ToString());

                return false;
            }

            _comm.BaudRate = keepCurrentBaudrate ? _comm.CurrentBaudRate : Communication.BAUDRATE_AUTO_DETECT;
            _comm.TargetDevice = targetDevice;
            _comm.CardsReadMode = CardsReadMode.None;

            try
            {
                _comm.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set device to {deviceInfo}");
                Console.WriteLine(ex.ToString());

                return false;
            }

            return true;
        }
        public bool CloseSiComm()
        {
            if (!_comm.IsOpen) return true;

            try
            {
                _comm.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to close device connecion");
                Console.WriteLine(ex.ToString());

                return false;
            }

            return true;
        }

        public bool SetCurrentTargetDevice(TargetDevice targetDevice, bool allowRemote = true)
        {
            if (!allowRemote && targetDevice == TargetDevice.Remote)
                targetDevice = TargetDevice.Direct;

            _currentTargetDevice = targetDevice;

            // int.TryParse(_currentDevice.DeviceSerial, out var serialNo);

            return true;
        }

        public bool SetCurrentDevice(DeviceInfo device)
        {
            _currentDevice = device;

            return true;
        }

        public bool ReadMemory(TargetDevice targetDevice)
        {
            if (_currentDevice == null)
            {
                Console.WriteLine("Please select a device connection!");
                return false;
            }

            if (!OpenSiComm(_currentDevice, targetDevice))
            {
                Console.WriteLine("Failed to open device connection!");
                return false;
            }

            _comm.GetBackupMemory();
            return true;
        }

        public bool ReadDevice(TargetDevice targetDevice, Action followUpAction = Action.NotSet)
        {
            _followUpAction = followUpAction;

            if (_currentDevice == null)
            {
                Console.WriteLine("Please select a device connection!");
                return false;
            }

            SetCurrentTargetDevice(targetDevice);

            if (!OpenSiComm(_currentDevice, targetDevice))
            {
                Console.WriteLine("Failed to open device connection!");
                return false;
            }

            _comm.GetSystemData();
            return true;
        }

        private void Comm_SiCardOut(object sender, SiCardEventArgs e)
        {
            Console.WriteLine($"Card #{e.Card.Siid} out");
        }
        private void Comm_SiCardIn(object sender, SiCardEventArgs e)
        {
            Console.WriteLine($"Card #{e.Card.Siid} in");
        }
        private void SiComm_BackupReadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine($"Backup memory read progress: {e.ProgressPercentage:##0.0} %");
        }
        private void SiComm_TriggerPunchReceived(object sender, SportidentDataEventArgs e)
        {
            Console.WriteLine("Trigger punch received");

            if (!e.HasPunchData) return;

            Console.WriteLine("Punch data:");
            foreach (var punch in e.PunchData)
                Console.WriteLine($"Punch: {punch.CodeNumber}, {punch.PunchDateTime}");
        }
        private void SiComm_SiCardReadCompleted(object sender, SportidentDataEventArgs e)
        {
            Console.WriteLine("Card read completed");

            SiCardRead.Invoke(sender, e); 
        }
        private void SiComm_SiCardReadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine($"Card read progress: {e.ProgressPercentage:##0.0} %");
        }
        private void SiComm_BackupReadCompleted(object sender, SportidentDataEventArgs e)
        {
            Console.WriteLine("Backup memory read completed");

            if (e.HasCards)
            {
                Console.WriteLine("Cards:");
                foreach (var card in e.Cards)
                    Console.WriteLine($"Card: {card.Siid}, {card.CardType}");
            }

            if (e.HasPunchData)
            {
                Console.WriteLine("Punch data:");
                foreach (var punch in e.PunchData)
                    Console.WriteLine($"Punch: {punch.CodeNumber}, {punch.PunchDateTime}");
            }
        }
        private void SiComm_CommunicationRetried(object sender, InstructionEventArgs e)
        {
            Console.WriteLine($"Communication retried the last instruction 0x{e.Instruction.CommandByte:X2}");
        }
        private void SiComm_CommunicationFailed(object sender, InstructionEventArgs e)
        {
            bool allowRetry = (e.Source != null) && ((ActionItem)e.Source).AllowResume;

            Console.WriteLine($"Communication failed on instruction 0x{e.Instruction.CommandByte:X2}, allow retry: {allowRetry}");
        }
        private void SiComm_ProgressCompleted(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine("Current operation completed");

            if (_followUpAction == Action.NotSet)
                return;

            _timer.Stop();
            _timer.Start();
        }
        private void SiComm_StationConfigWritten(object sender, StationConfigurationEventArgs e)
        {
            Console.WriteLine("Station configuration written successfully");
        }
        private void SiComm_StationConfigFailed(object sender, StationConfigurationEventArgs e)
        {
            var msg = "no result specified";
            switch (e.Result)
            {
                case StationConfigurationResult.OperatingModeNotSupported:
                    msg = "The selected operating mode is not supported on the current device.";
                    break;
                case StationConfigurationResult.DeviceDoesNotHaveBackup:
                    msg = "The device does not have a backup memory storage.";
                    break;
                case StationConfigurationResult.ReadoutMasterBackupNotSupported:
                    msg = "Reading the backup of SI-Master with firmware < FW595 is not supported.";
                    break;
                default:
                    msg = e.Result.ToString();
                    break;
            }

            Console.WriteLine($"The current operation failed: {msg}");
        }
        private void SiComm_StationConfigRead(object sender, StationConfigurationEventArgs e)
        {
            Console.WriteLine("Station configuration read successfully");
        }
        private void SiComm_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            var state = e.CurrentState;

            if (!_comm.IsOpen)
                state = DeviceState.D4Disconnected;

            //string stateStr = state switch
            //{
            //    DeviceState.D0Online => $"Current device state: Connected @ {_comm.CurrentBaudRate} Baud",
            //    DeviceState.D5Busy => $"Current device state: Busy @ {_comm.CurrentBaudRate} Baud",
            //    _ => $"Current device state: Connected @ {_comm?.CurrentBaudRate} Baud",
            //};

            //Console.WriteLine(stateStr);
        }

        private void FollowUpAction_Do(object sender, EventArgs e)
        {
            _timer.Stop();

            switch (_followUpAction)
            {
                case Action.ReadCards:
                    if (_currentDevice == null)
                    {
                        Console.WriteLine("Please select a device connection!");
                        return;
                    }

                    if (_comm.Device.OperatingMode != OperatingMode.Readout)
                    {
                        Console.WriteLine("The current device is not in readout mode!");
                        break;
                    }

                    if (_comm.Device.AutoSendMode || _comm.Device.LegacyProtocolMode)
                    {
                        Console.WriteLine("Your device is not configured optimally to read cards. Please: Disable autosend flag - and - disable legacy protocol");
                        break;
                    }

                    ConnectAndKeepAliveDevice();

                    _comm.CardsReadMode = CardsReadMode.ReadCards;
                    _comm.SiacMeasureBatteryOnRead = true;

                    break;
                case Action.ViewPunch:
                    ConnectAndKeepAliveDevice();
                    break;
            }
        }

        public void ToggleTargetDevice()
        {
            _currentTargetDevice = _currentTargetDevice == TargetDevice.Direct
                ? TargetDevice.Remote : TargetDevice.Direct;

            CloseSiComm();

            SetCurrentTargetDevice(_currentTargetDevice);
        }
        public void ReadSettings()
        {
            CloseSiComm();

            ReadDevice(_currentTargetDevice);
        }
        public void ReadBackup()
        {
            CloseSiComm();

            ReadMemory(_currentTargetDevice);
        }
        public void ReadCards()
        {
            CloseSiComm();

            _currentTargetDevice = TargetDevice.Direct;
            ReadDevice(_currentTargetDevice, Action.ReadCards);
        }

        public void ChangeDeviceBaudrate()
        {
            int oldBaudrate = _comm.CurrentBaudRate;
            int baudrate = oldBaudrate == 38400 ? 4800 : 38400;

            if (!OpenSiComm(_currentDevice, TargetDevice.Direct)) return;

            _comm.SetBaudrate(baudrate);
        }
        public void BeepIfReady()
        {
            if (!OpenSiComm(_currentDevice, TargetDevice.Direct)) return;

            _comm.BeepIfCardIsReady();
        }
        public void ClearBackupMemory()
        {
            if (!OpenSiComm(_currentDevice, TargetDevice.Direct)) return;

            _comm.ClearBackupMemory();
        }

        public void TriggerPunch()
        {
            CloseSiComm();

            ReadDevice(_currentTargetDevice);
        }
        public void ConnectAndKeepAliveDevice()
        {
            if (_currentDevice is null)
                return;

            OpenSiComm(_currentDevice, TargetDevice.Direct, true);
        }

        public event EventHandler<SportidentDataEventArgs> SiCardRead;

        public static IEnumerable<DeviceInfo> GetAllDevices()
            => DeviceInfo.GetAvailableDeviceList(true, (int)DeviceType.Serial | (int)DeviceType.UsbHid);
    }
}
