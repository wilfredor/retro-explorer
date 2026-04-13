# Retro Explorer

A Windows 95-style file explorer rebuilt from scratch as a WinForms application. Designed to replicate the look, feel, and behavior of the original Windows 95 Explorer while running on modern Windows.

![.NET Framework 4.7.2](https://img.shields.io/badge/.NET_Framework-4.7.2-blue)
![.NET Framework 2.0](https://img.shields.io/badge/.NET_Framework-2.0_(Win2K)-orange)
![Platform](https://img.shields.io/badge/platform-Windows_2000+-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

**Navigation**
- Single-window browsing with Back, Forward, and Up history
- Editable address bar
- Folder tree panel with Desktop / My Computer root
- Open current folder in a new window

**File operations**
- Cut, Copy, Paste, Rename, Delete
- Delete to Recycle Bin by default, Shift+Delete for permanent
- Drag and drop between folders and from external apps
- Context menu with common actions

**Views**
- Large Icons, Small Icons, List, Details
- Column sorting in Details view (ascending/descending)
- Arrange Icons submenu (by name, type, size, date)

**Classic UI**
- Toolbar icons extracted from the original Windows 95 `comctl32.dll`
- Classic folder and drive icons
- Menus: File, Edit, View, Go, Tools, Help
- Go menu with recent path history
- Status bar with current path and free disk space
- About dialog

**System integration**
- Opens files through Windows shell associations
- FileSystemWatcher for automatic directory refresh
- Keyboard shortcuts: Backspace, Alt+Left, Alt+Right, Alt+Enter, F5

## Origin

This project was reconstructed from the `ex_plorer.exe` binary found in [ProjectChicago](https://www.reddit.com/r/windows95/comments/1mnyoej/projectchicago_a_windows_10_installation/), a community project that heavily modifies Windows 10 to look and feel like Windows 95. The original decompiled code was cleaned up, restructured, debugged, and extended.

## Build

### Modern build (.NET Framework 4.7.2)

Requires the .NET Framework 4.7.2 targeting pack (included with Visual Studio or available separately).

#### Visual Studio

1. Open `ex_plorer.original.csproj`
2. Build in Release

#### Command line

```powershell
dotnet build ex_plorer.original.csproj -c Release
```

The output is a single standalone executable:

```
bin\Release\net472\ex_plorer.exe
```

No external DLLs required. All dependencies (including toolbar icon resources) are embedded in the exe via [Costura.Fody](https://github.com/Fody/Costura).

### Windows 2000 build (.NET Framework 2.0)

The `win2k/` directory contains a full port of the source code downgraded to C# 2.0 / .NET Framework 2.0, compatible with **Windows 2000 SP4** and later.

#### Requirements

- .NET Framework 2.0 SDK (the `csc.exe` compiler at `C:\Windows\Microsoft.NET\Framework\v2.0.50727\`)

#### Build

```cmd
cd win2k
build.bat
```

Output:

```
win2k\ex_plorer.exe
```

#### Running on Windows 2000

The target machine needs the [.NET Framework 2.0 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=6523) installed (~23 MB). No other dependencies are required.

#### What changed in the port

The source was mechanically downgraded from C# 12 / .NET 4.7.2 to C# 2.0 / .NET 2.0:

- `async/await` replaced with synchronous calls
- LINQ replaced with manual loops
- String interpolation replaced with concatenation
- Auto-properties replaced with backing fields
- Object/collection initializers replaced with assignments
- Pattern matching replaced with explicit casts
- `string.IsNullOrWhiteSpace` replaced with a custom helper
- `EnumerateDirectories`/`EnumerateFileSystemInfos` replaced with `GetDirectories`/`GetFileSystemInfos`
- `Stream.CopyTo` replaced with manual buffer copy
- Extension methods converted to static methods
- All `using` declarations converted to `try/finally` with `Dispose`

## Project structure

```
win2k/                            Windows 2000 port (C# 2.0 / .NET 2.0)
  build.bat                       Build script using .NET 2.0 csc.exe
  *.cs                            Downgraded source files
ex_plorer/
  ExplorerForm.cs               Main form, constructor, icon setup
  ExplorerForm.Fields.cs        UI control declarations
  ExplorerForm.UI.cs             Layout, toolbar, menus
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
  ShellFileOperations.cs        Shell integration (delete, properties)
  FileSystemItemComparer.cs     Column sort logic
  GotoForm.cs                   Go To dialog
  Program.cs                    Entry point
assets/
  comctl32.dll                  Windows 95 toolbar bitmaps (embedded)
  icons/                        Classic .ico files (embedded)
```

## What this is not

- Not a pixel-perfect replica of the original Explorer
- Not a shell replacement
- Context menu and drag & drop are practical reinterpretations
- Some operations use the modern Windows shell underneath

## Changelog

- Reconstructed from decompiled binary into clean, maintainable source
- Fixed original Paste bug for folders
- Fixed GDI handle leak in icon extraction
- Eliminated multiple IO-related crash points
- Added single-window navigation with full history
- Added folder tree panel with Desktop/My Computer root
- Added Cut, drag & drop, Recycle Bin delete
- Added Details view column sorting and Arrange Icons
- Added toolbar icons from original Win95 comctl32.dll
- Packaged as single self-contained exe
