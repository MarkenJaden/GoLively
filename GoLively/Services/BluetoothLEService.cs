using System.Collections.ObjectModel;
using System.Diagnostics;
using AntDesign;
using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using Application = Android.App.Application;

namespace GoLively.Services;

public class BluetoothLEService
{
    public ObservableCollection<IBlePeripheral> DeviceCandidateList { get; } = new();

    public IBluetoothLowEnergyAdapter ble = BluetoothLowEnergyAdapter.ObtainDefaultAdapter(Application.Context);

    public NotificationService _notice { get; set; }

    public async Task<IBleGattServerConnection> ConnectToDevice(IBlePeripheral peripheral)
    {
        var connection = await ble.ConnectToDevice(peripheral, progress => Debug.WriteLine(progress));

        if (connection.IsSuccessful())
        {
            return connection.GattServer;
        }

        await Notice("Connection failed", "Please try again.", NotificationType.Error);

        return null;
    }

    public async void ScanForDevicesAsync()
    {
        var permissionStatus = await CheckBluetoothPermissions();
        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await RequestBluetoothPermissions();
            if (permissionStatus != PermissionStatus.Granted)
            {
                await Notice($"Bluetooth LE permissions", $"Bluetooth LE permissions are not granted.", NotificationType.Error);
                return;
            }
        }

        if (ble.AdapterCanBeEnabled && ble.CurrentState.IsDisabledOrDisabling())
        {
            await ble.EnableAdapter();
        }

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await ble.ScanForBroadcasts(DeviceDiscovered, cts.Token);
    }

    private async void DeviceDiscovered(IBlePeripheral peripheral)
    {
        if(string.IsNullOrWhiteSpace(peripheral.Advertisement.DeviceName) || !peripheral.Advertisement.DeviceName.Contains("GoPro", StringComparison.CurrentCultureIgnoreCase) || DeviceCandidateList.Any(x => x.Advertisement.DeviceName.Equals(peripheral.Advertisement.DeviceName))) return;

        DeviceCandidateList.Add(peripheral);
    }

#if ANDROID
    #region BluetoothPermissions
    public async Task<PermissionStatus> CheckBluetoothPermissions()
    {
        var status = PermissionStatus.Unknown;
        try
        {
            status = await Permissions.CheckStatusAsync<BluetoothPermissions>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to check Bluetooth LE permissions: {ex.Message}.");
            await Shell.Current.DisplayAlert($"Unable to check Bluetooth LE permissions", $"{ex.Message}.", "OK");
        }
        return status;
    }

    public async Task<PermissionStatus> RequestBluetoothPermissions()
    {
        var status = PermissionStatus.Unknown;
        try
        {
            status = await Permissions.RequestAsync<BluetoothPermissions>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to request Bluetooth LE permissions: {ex.Message}.");
            await Shell.Current.DisplayAlert($"Unable to request Bluetooth LE permissions", $"{ex.Message}.", "OK");
        }
        return status;
    }
    #endregion BluetoothPermissions
#elif IOS
#elif WINDOWS
#endif

    public async Task Notice(string title, string message = "", NotificationType type = NotificationType.Info)
    {
        Debug.WriteLine($"{title} - {message}");
        if (_notice == null) return;
        await _notice.Open(new()
        {
            Message = title,
            Description = message,
            NotificationType = type,
        });
    }
}

