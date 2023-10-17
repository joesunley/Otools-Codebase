using ownsmtp.logging;
using SPORTident;
using SPORTident.Communication;
using SPORTident.Communication.UsbDevice;
using System.ComponentModel;
using System.Security.AccessControl;

namespace OTools.SiIntegrator;

public class SiInterface
{
	private DeviceInfo? _currentDevice;
	private readonly Communication _comm = new();
	private TargetDevice _currentTargetDevice = TargetDevice.Direct;

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
			ODebugger.Error($"Failed to set device to {deviceInfo}");
			ODebugger.Error(ex.ToString());

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
			ODebugger.Error($"Failed to set device to {deviceInfo}");
			ODebugger.Error(ex.ToString());

			return false;
		}

		return true;
	}
	public bool CloseSiComm()
	{
		if (!_comm.IsOpen) return false;

		try
		{
			_comm.Close();
		}
		catch (Exception ex)
		{
			ODebugger.Error("Failed to close device connecion");
			ODebugger.Error(ex.ToString());

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
			ODebugger.Error("Please select a device connection!");
			return false;
		}

		if (!OpenSiComm(_currentDevice, targetDevice))
		{
            ODebugger.Error("Failed to open device connection!");
            return false;
        }

		_comm.GetBackupMemory();
		return true;
	}

	public bool ReadDevice(TargetDevice targetDevice)
	{
		  if (_currentDevice == null)
		{
            ODebugger.Error("Please select a device connection!");
            return false;
        }

		SetCurrentTargetDevice(targetDevice);

        if (!OpenSiComm(_currentDevice, targetDevice))
		{
            ODebugger.Error("Failed to open device connection!");
            return false;
        }

        _comm.GetSystemData();
        return true;
	}

	private void Comm_SiCardOut(object sender, SiCardEventArgs e)
	{
		ODebugger.Info($"Card #{e.Card.Siid} out");
	}
	private void Comm_SiCardIn(object sender, SiCardEventArgs e)
	{
		ODebugger.Info($"Card #{e.Card.Siid} in");
	}
	private void SiComm_BackupReadProgressChanged(object sender, ProgressChangedEventArgs e)
	{
        ODebugger.Info($"Backup memory read progress: {e.ProgressPercentage:##0.0} %");
    }
	private void SiComm_TriggerPunchReceived(object sender, SportidentDataEventArgs e)
	{
		ODebugger.Info("Trigger punch received");

		if (!e.HasPunchData) return;

		ODebugger.Info("Punch data:");
		foreach (var punch in e.PunchData)
            ODebugger.Debug($"Punch: {punch.CodeNumber}, {punch.PunchDateTime}");
	}
	private void SiComm_SiCardReadCompleted(object sender, SportidentDataEventArgs e)
	{
		ODebugger.Info("Card read completed");
	}
	private void SiComm_SiCardReadProgressChanged(object sender, ProgressChangedEventArgs e)
	{
        ODebugger.Info($"Card read progress: {e.ProgressPercentage:##0.0} %");
    }
	private void SiComm_BackupReadCompleted(object sender, SportidentDataEventArgs e)
	{
        ODebugger.Info("Backup memory read completed");

		if (e.HasCards)
		{
			ODebugger.Info("Cards:");
			foreach (var card in e.Cards)
                ODebugger.Debug($"Card: {card.Siid}, {card.CardType}");
		}

		if (e.HasPunchData)
		{
			ODebugger.Info("Punch data:");
			foreach (var punch in e.PunchData)
                ODebugger.Debug($"Punch: {punch.CodeNumber}, {punch.PunchDateTime}");
		}
    }
	private void SiComm_CommunicationRetried(object sender, InstructionEventArgs e)
	{
        ODebugger.Info($"Communication retried the last instruction 0x{e.Instruction.CommandByte:X2}");
    }
	private void SiComm_CommunicationFailed(object sender, InstructionEventArgs e)
	{
		bool allowRetry = (e.Source != null) && ((ActionItem)e.Source).AllowResume;
		
		ODebugger.Info($"Communication failed on instruction 0x{e.Instruction.CommandByte:X2}, allow retry: {allowRetry}");
	}
	private void SiComm_ProgressCompleted(object sender, ProgressChangedEventArgs e)
	{
		ODebugger.Info("Current operation completed");
	}
	private void SiComm_StationConfigWritten(object sender, StationConfigurationEventArgs e)
	{
		ODebugger.Info("Station configuration written successfully");
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

        ODebugger.Info($"The current operation failed: {msg}");
    }
	private void SiComm_StationConfigRead(object sender, StationConfigurationEventArgs e)
	{
		  ODebugger.Info("Station configuration read successfully");
	}
	private void SiComm_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
	{
		var state = e.CurrentState;

		if (!_comm.IsOpen)
			state = DeviceState.D4Disconnected;

        string stateStr = state switch
        {
            DeviceState.D0Online => $"Current device state: Connected @ {_comm.CurrentBaudRate} Baud",
            DeviceState.D5Busy => $"Current device state: Busy @ {_comm.CurrentBaudRate} Baud",
            _ => $"Current device state: Connected @ {_comm?.CurrentBaudRate} Baud",
        };

		ODebugger.Info(stateStr);
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
		ReadDevice(_currentTargetDevice);
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

    public static IEnumerable<DeviceInfo> GetAllDevices() 
		=> DeviceInfo.GetAvailableDeviceList(true, (int)DeviceType.Serial | (int)DeviceType.UsbHid);
}