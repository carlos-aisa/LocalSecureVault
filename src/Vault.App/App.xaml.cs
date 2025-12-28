namespace Vault.App;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
        try {
             System.IO.File.AppendAllText(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "VaultAppLog.txt"), "MAUI App constructor called.\n");
        } catch {}
		InitializeComponent();

		MainPage = new MainPage();
        try {
             System.IO.File.AppendAllText(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "VaultAppLog.txt"), "MAUI App constructor finished.\n");
        } catch {}
	}
}
