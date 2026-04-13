@echo off
REM Build ex_plorer for Windows 2000 (.NET Framework 2.0, C# 2.0)
REM Requires .NET Framework 2.0 SDK (csc.exe)

set CSC=C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe

if not exist "%CSC%" (
    echo ERROR: .NET Framework 2.0 compiler not found at %CSC%
    exit /b 1
)

set OUT=ex_plorer.exe
set ICON=..\app.ico
set ASSETS=..\assets

echo Building ex_plorer for Windows 2000...

"%CSC%" /nologo /target:winexe /out:%OUT% /win32icon:%ICON% ^
    /r:System.dll ^
    /r:System.Drawing.dll ^
    /r:System.Windows.Forms.dll ^
    /resource:"%ASSETS%\comctl32.dll",ex_plorer.Assets.comctl32.dll ^
    /resource:"%ASSETS%\icons\classic_app.ico",ex_plorer.Assets.Icons.classic_app.ico ^
    /resource:"%ASSETS%\icons\classic_desktop.ico",ex_plorer.Assets.Icons.classic_desktop.ico ^
    /resource:"%ASSETS%\icons\classic_mycomputer.ico",ex_plorer.Assets.Icons.classic_mycomputer.ico ^
    /resource:"%ASSETS%\icons\classic_drive_fixed.ico",ex_plorer.Assets.Icons.classic_drive_fixed.ico ^
    /resource:"%ASSETS%\icons\classic_drive_removable.ico",ex_plorer.Assets.Icons.classic_drive_removable.ico ^
    /resource:"%ASSETS%\icons\classic_drive_cd.ico",ex_plorer.Assets.Icons.classic_drive_cd.ico ^
    /resource:"%ASSETS%\icons\classic_drive_network.ico",ex_plorer.Assets.Icons.classic_drive_network.ico ^
    /resource:"%ASSETS%\icons\classic_folder_closed.ico",ex_plorer.Assets.Icons.classic_folder_closed.ico ^
    /resource:"%ASSETS%\icons\classic_folder_open.ico",ex_plorer.Assets.Icons.classic_folder_open.ico ^
    /resource:"%ASSETS%\icons\classic_file.ico",ex_plorer.Assets.Icons.classic_file.ico ^
    /resource:"%ASSETS%\icons\w98_address_book_copy.ico",ex_plorer.Assets.Icons.w98_address_book_copy.ico ^
    /resource:"%ASSETS%\icons\w98_write_file.ico",ex_plorer.Assets.Icons.w98_write_file.ico ^
    /resource:"%ASSETS%\icons\w98_display_properties.ico",ex_plorer.Assets.Icons.w98_display_properties.ico ^
    /resource:"%ASSETS%\icons\w98_shell_window1.ico",ex_plorer.Assets.Icons.w98_shell_window1.ico ^
    /resource:"%ASSETS%\icons\w98_shell_window2.ico",ex_plorer.Assets.Icons.w98_shell_window2.ico ^
    /resource:"%ASSETS%\icons\w98_shell_window3.ico",ex_plorer.Assets.Icons.w98_shell_window3.ico ^
    /resource:"%ASSETS%\icons\w98_shell_window4.ico",ex_plorer.Assets.Icons.w98_shell_window4.ico ^
    /unsafe ^
    AssemblyInfo.cs ^
    Program.cs ^
    Utils.cs ^
    IconPair.cs ^
    DirManager.cs ^
    NativeIcon.cs ^
    ClassicIcons.cs ^
    ToolbarImages.cs ^
    ClipboardHelper.cs ^
    ShellFileOperations.cs ^
    FileSystemItemComparer.cs ^
    GotoForm.cs ^
    ExplorerForm.Fields.cs ^
    ExplorerForm.cs ^
    ExplorerForm.UI.cs ^
    ExplorerForm.Common.cs ^
    ExplorerForm.Navigation.cs ^
    ExplorerForm.Tree.cs ^
    ExplorerForm.Operations.cs ^
    ExplorerForm.WatcherAndDragDrop.cs

if %ERRORLEVEL% neq 0 (
    echo.
    echo BUILD FAILED
    exit /b 1
)

echo.
echo BUILD SUCCEEDED: %OUT%
echo Target: .NET Framework 2.0 (Windows 2000 SP4+)
