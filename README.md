# Discord RPC Manager 

A powerful and easy-to-use Discord Rich Presence (RPC) manager for Windows. Allows you to fully customize your Discord status, supporting multiple profiles and automation features.

![Banner](https://github.com/dat514/Discord-RPC-Manager/blob/main/Img/Screenshot.png)

## 📥 Download

You can download the latest version (`DiscordRPCManager1.0.2.zip`) here:

[![Download Latest Release](https://img.shields.io/github/v/release/dat514/Discord-RPC-Manager?label=Download\&style=for-the-badge\&color=blue)](https://github.com/dat514/Discord-RPC-Manager/releases/latest/download/DiscordRPCManager1.0.2.zip)

> **Note:** The link above will automatically download `DiscordRPCManager1.0.2.zip` from the latest release.
> Extract the `.zip` file to access the `.exe` file inside.
> To view all versions, visit the [Releases Page](https://github.com/dat514/Discord-RPC-Manager/releases).


## ✨ Key Features

*   **Multi-Profile Management:** Create and save multiple RPC configurations for different purposes (Coding, Gaming, Music, etc.).
*   **Detailed Customization:**
    *   **Application ID (Client ID):** Use your own custom App ID.
    *   **Details & State:** Customize the first and second status lines.
    *   **Assets:** Support for Large Image and Small Image (with tooltips).
    *   **Timestamp:** Display Elapsed time or Custom Time Offset.
*   **Watchdog Mode (App Monitoring):** 
    *   Only display RPC when a specific `.exe` file is running (e.g., only show when Visual Studio is open).
    *   Automatically stops RPC when that application closes.
*   **Automation:**
    *   📵 **Run at Startup:** Automatically launches the app when Windows starts.
    *   🚀 **Auto-Start RPC:** Automatically activates the last used RPC profile on launch.
*   **Modern UI:** Sleek Dark Mode design, smooth hover effects, user-friendly interface.

## 🚀 How to Use

1.  **Download** the application from the link above and open `DiscordRPCManager.exe`.
2.  **Get Application ID:**
    *   Go to the [Discord Developer Portal](https://discord.com/developers/applications).
    *   Create a new Application -> Copy the **Application ID**.
    *   Go to **Rich Presence** -> **Art Assets** to upload images (if you want to use them).
    *   *Note: Image names uploaded here are the "Keys" you'll use in the app.*
3.  **Create a Profile:**
    *   Click **Add Profile**.
    *   Enter a Profile Name and your **Application ID**.
    *   Enter Details, State.
    *   Enter the Image Keys (Large/Small Image Key) corresponding to the assets you uploaded.
4.  **Activate:**
    *   Click **Start RPC** to display it on Discord immediately.
    *   Or use **Target EXE** (Browse button) to use the Watchdog mode (auto-show when target app runs).

## 💻 System Requirements

*   OS: Windows 10/11.
*   .NET Desktop Runtime (usually pre-installed on updated Windows systems).
*   Discord App (must be running).

## 🛠️ Build from Source

If you want to build the application yourself:

```bash
git clone https://github.com/dat514/Discord-RPC-Manager.git
cd DiscordRPCManager
```

