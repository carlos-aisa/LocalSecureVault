using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Vault.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        
#if ANDROID
        Services.AndroidVaultFilePicker.OnActivityResult(requestCode, resultCode, data);
#endif
    }
}
