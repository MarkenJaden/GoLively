using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using nexus.protocols.ble;

namespace GoLively;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);
        BluetoothLowEnergyAdapter.Init(this);
    }
    protected sealed override void OnActivityResult(Int32 requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        BluetoothLowEnergyAdapter.OnActivityResult(requestCode, resultCode, data);
    }
}
