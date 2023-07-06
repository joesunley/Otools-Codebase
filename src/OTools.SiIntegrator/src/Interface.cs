using ownsmtp.logging;
using SPORTident.Communication;
using SPORTident.Communication.UsbDevice;

namespace OTools.SiIntegrator;

public class SiInterface
{
	private readonly Communication _comm = new();

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
		
		)
	}

	public static IEnumerable<DeviceInfo> GetAllDevices() 
		=> DeviceInfo.GetAvailableDeviceList(true, (int)DeviceType.Serial | (int)DeviceType.UsbHid);
}