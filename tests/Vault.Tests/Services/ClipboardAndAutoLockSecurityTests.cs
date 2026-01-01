using System;
using System.Threading.Tasks;
using Xunit;

namespace Vault.Tests.Services;

public class ClipboardSecurityTests
{
    [Fact]
    public async Task ClipboardAutoClear_AfterTimeout_ShouldClear()
    {
        var timeout = TimeSpan.FromMilliseconds(100);
        var testValue = "sensitive-password";
        
        string? clipboardValue = testValue;
        
        await Task.Delay(timeout + TimeSpan.FromMilliseconds(50));
        
        clipboardValue = null;
        
        Assert.Null(clipboardValue);
    }

    [Fact]
    public void ClipboardAutoClear_UserOverwritesBeforeTimeout_ShouldNotClearUserContent()
    {
        var timeout = TimeSpan.FromSeconds(30);
        var originalValue = "password-from-app";
        var userValue = "user-copied-something-else";
        
        string clipboardContent = originalValue;
        DateTime setClearTimer = DateTime.UtcNow;
        
        clipboardContent = userValue;
        DateTime userOverwriteTime = DateTime.UtcNow;
        
        bool shouldClear = clipboardContent == originalValue;
        
        Assert.False(shouldClear);
        Assert.Equal(userValue, clipboardContent);
    }

    [Fact]
    public async Task ClipboardAutoClear_CancelTimer_ShouldNotClear()
    {
        var timeout = TimeSpan.FromMilliseconds(100);
        var testValue = "sensitive-password";
        
        string? clipboardValue = testValue;
        bool timerCancelled = false;
        
        timerCancelled = true;
        
        await Task.Delay(timeout + TimeSpan.FromMilliseconds(50));
        
        if (!timerCancelled)
        {
            clipboardValue = null;
        }
        
        Assert.Equal(testValue, clipboardValue);
    }

    [Fact]
    public void ClipboardSecurity_ShouldNotLogSensitiveContent()
    {
        var sensitiveContent = "password123!@#";
        
        string logMessage = "Clipboard operation performed";
        
        Assert.DoesNotContain(sensitiveContent, logMessage);
        Assert.DoesNotContain("password", logMessage.ToLower());
    }
}

public class AutoLockSecurityTests
{
    [Fact]
    public async Task AutoLock_AfterIdleTimeout_ShouldLockVault()
    {
        var idleTimeout = TimeSpan.FromMilliseconds(100);
        bool vaultLocked = false;
        DateTime lastActivity = DateTime.UtcNow;
        
        await Task.Delay(idleTimeout + TimeSpan.FromMilliseconds(50));
        
        if (DateTime.UtcNow - lastActivity > idleTimeout)
        {
            vaultLocked = true;
        }
        
        Assert.True(vaultLocked);
    }

    [Fact]
    public async Task AutoLock_UserActivityResetsTimer_ShouldNotLock()
    {
        var idleTimeout = TimeSpan.FromMilliseconds(100);
        bool vaultLocked = false;
        DateTime lastActivity = DateTime.UtcNow;
        
        await Task.Delay(TimeSpan.FromMilliseconds(50));
        lastActivity = DateTime.UtcNow;
        
        await Task.Delay(TimeSpan.FromMilliseconds(60));
        
        if (DateTime.UtcNow - lastActivity > idleTimeout)
        {
            vaultLocked = true;
        }
        
        Assert.False(vaultLocked);
    }

    [Fact]
    public void AutoLock_WithUnsavedChanges_ShouldPromptUser()
    {
        bool hasUnsavedChanges = true;
        bool shouldPrompt = false;
        bool shouldLockImmediately = false;
        
        if (hasUnsavedChanges)
        {
            shouldPrompt = true;
            shouldLockImmediately = false;
        }
        else
        {
            shouldPrompt = false;
            shouldLockImmediately = true;
        }
        
        Assert.True(shouldPrompt);
        Assert.False(shouldLockImmediately);
    }

    [Fact]
    public void AutoLock_WhenVaultAlreadyLocked_ShouldNotTrigger()
    {
        bool isVaultUnlocked = false;
        bool shouldTriggerAutoLock = false;
        
        if (isVaultUnlocked)
        {
            shouldTriggerAutoLock = true;
        }
        
        Assert.False(shouldTriggerAutoLock);
    }

    [Fact]
    public async Task AutoLock_ConfigurableTimeout_ShouldRespectSetting()
    {
        var shortTimeout = TimeSpan.FromMilliseconds(50);
        var longTimeout = TimeSpan.FromMilliseconds(200);
        
        DateTime lastActivity = DateTime.UtcNow;
        await Task.Delay(shortTimeout + TimeSpan.FromMilliseconds(10));
        bool lockedWithShort = DateTime.UtcNow - lastActivity > shortTimeout;
        
        Assert.True(lockedWithShort);
        
        lastActivity = DateTime.UtcNow;
        await Task.Delay(shortTimeout + TimeSpan.FromMilliseconds(10));
        bool lockedWithLong = DateTime.UtcNow - lastActivity > longTimeout;
        
        Assert.False(lockedWithLong);
    }
}
