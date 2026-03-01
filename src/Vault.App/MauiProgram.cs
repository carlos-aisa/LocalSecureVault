using Microsoft.AspNetCore.Components.WebView.Maui;
using Vault.Application.Abstractions;
using Vault.Application.Services;
using Vault.Application.Import.Markdown;
using Vault.Application.UseCases;
using Vault.Crypto;
using Vault.Storage;
using Vault.Storage.Serialization;
using Vault.App.Platform;
using Vault.App.Services;
using Microsoft.Maui.LifecycleEvents;

namespace Vault.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

#if WINDOWS
		builder.Services.AddSingleton<IFileDialogService, WindowsFileDialogService>();
#endif

		// Platform-specific file picker
#if ANDROID
		builder.Services.AddSingleton<IVaultFilePicker, AndroidVaultFilePicker>();
		builder.Services.AddSingleton<IVaultExportPicker, AndroidVaultExportPicker>();
#else
		builder.Services.AddSingleton<IVaultFilePicker, MauiVaultFilePicker>();
		builder.Services.AddSingleton<IVaultExportPicker, NullVaultExportPicker>();
#endif

		// Platform-specific biometric authentication
#if ANDROID
		builder.Services.AddSingleton<IBiometricAuthService, AndroidBiometricAuthService>();
#elif WINDOWS
		builder.Services.AddSingleton<IBiometricAuthService, WindowsBiometricAuthService>();
#else
		builder.Services.AddSingleton<IBiometricAuthService, WindowsBiometricAuthService>();
#endif

		builder.Services.AddSingleton<ICryptoProvider, CryptoProvider>();
		builder.Services.AddSingleton<IVaultStore, FileVaultStore>();
		builder.Services.AddSingleton<IVaultPayloadSerializer, JsonVaultPayloadSerializer>();
		builder.Services.AddSingleton<IVaultCryptoService, VaultCryptoService>();
		builder.Services.AddSingleton<IRecentVaultPathStore, PreferencesRecentVaultPathStore>();
		builder.Services.AddSingleton<Services.ClipboardService>();
		builder.Services.AddSingleton<AppState>();
		builder.Services.AddSingleton<MarkdownVaultImporter>();
		builder.Services.AddSingleton<VaultSaveService>();
		builder.Services.AddSingleton<VaultImportService>();
		builder.Services.AddSingleton<EntryUseCases>();
		builder.Services.AddSingleton<VaultAppService>();
		builder.Services.AddSingleton<SearchService>();
		builder.Services.AddScoped<InactivityMonitor>();


		return builder.Build();
	}
}
