# Retro Explorer

Windows 95-style file explorer. WinForms / C#.

Reconstructed from decompiled `ex_plorer.exe` binary in [ProjectChicago](https://www.reddit.com/r/windows95/comments/1mnyoej/projectchicago_a_windows_10_installation/).

![.NET Framework 4.7.2](https://img.shields.io/badge/.NET_Framework-4.7.2-blue)
![.NET Framework 2.0](https://img.shields.io/badge/.NET_Framework-2.0_(Win2K)-orange)
![Platform](https://img.shields.io/badge/platform-Windows_2000+-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

- Single-window navigation: Back, Forward, Up, address bar, folder tree (Desktop/My Computer root)
- File operations: Cut, Copy, Paste, Rename, Delete (Recycle Bin default, Shift+Del permanent), drag & drop
- Views: Large Icons, Small Icons, List, Details (with column sorting)
- Toolbar icons from original Win95 `comctl32.dll`
- FileSystemWatcher for auto-refresh
- Keyboard: Backspace, Alt+Left/Right, Alt+Enter, F5
- Status bar with path and free disk space

## Build

### .NET Framework 4.7.2

```powershell
dotnet build ex_plorer.original.csproj -c Release
```

Output: `bin\Release\net472\ex_plorer.exe` — single standalone exe (dependencies embedded via [Costura.Fody](https://github.com/Fody/Costura)).

### Windows 2000 (.NET Framework 2.0)

Port in `win2k/` — C# 2.0 / .NET 2.0. Compatible with Windows 2000 SP4+.

```cmd
cd win2k
build.bat
```

Requires .NET Framework 2.0 SDK (`csc.exe` at `C:\Windows\Microsoft.NET\Framework\v2.0.50727\`). Target machine needs [.NET 2.0 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=6523).

C# 12 → C# 2.0 changes: async/await removed, LINQ replaced with loops, string interpolation → concatenation, auto-properties → backing fields, pattern matching → explicit casts, extension methods → static methods.

## Project structure

```
win2k/                          Windows 2000 port (C# 2.0 / .NET 2.0)
  build.bat
  *.cs
ex_plorer/
  ExplorerForm.cs               Main form
  ExplorerForm.Fields.cs        UI control declarations
  ExplorerForm.UI.cs            Layout, toolbar, menus
  ExplorerForm.Navigation.cs    Back/Forward/Up, address bar
  ExplorerForm.Operations.cs    Cut/Copy/Paste/Delete/Rename
  ExplorerForm.Tree.cs          TreeView folder panel
  ExplorerForm.WatcherAndDragDrop.cs  FileSystemWatcher, drag & drop
  ExplorerForm.Common.cs        Shared utilities
  ToolbarImages.cs              Win95 comctl32.dll bitmap extraction
  ClassicIcons.cs               Embedded icon cache
  NativeIcon.cs                 Shell icon P/Invoke
  DirManager.cs                 Directory enumeration & icons
  ClipboardHelper.cs            Clipboard file operations
  ShellFileOperations.cs        Shell integration
  FileSystemItemComparer.cs     Column sort logic
  GotoForm.cs                   Go To dialog
  Program.cs                    Entry point
assets/
  comctl32.dll                  Win95 toolbar bitmaps (embedded)
  icons/                        Classic .ico files (embedded)
```

## Changelog

- Reconstructed from decompiled binary into clean source
- Fixed Paste bug for folders
- Fixed GDI handle leak in icon extraction
- Eliminated IO-related crash points
- Added single-window navigation with full history
- Added folder tree panel
- Added Cut, drag & drop, Recycle Bin delete
- Added Details view column sorting
- Added Win95 toolbar icons from comctl32.dll
- Packaged as single self-contained exe
