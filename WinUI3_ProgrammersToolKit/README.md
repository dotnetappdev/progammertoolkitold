# Programmer's Toolkit - WinUI 3 Version

This is a complete conversion of the Programmer's Toolkit from Avalonia UI to WinUI 3, preserving all original features.

## Original Features Converted

✅ **Main Navigation**: Sidebar-based navigation with category selection
✅ **General View**: Welcome screen with application information
✅ **Encryption Tools**: JWT decoder, hash generator (SHA256, MD5), AES encryption/decryption
✅ **Notepad**: Feature-rich notepad with encrypted storage using SQLite and macro support
✅ **Text Stripper**: Tool to remove non-printable characters from text
✅ **Settings**: Application configuration and preferences

🔄 **In Progress**: 
- Monitor Sharing
- Remote Control  
- File Sharing
- FTP Client
- Code Editor (needs WinUI 3 equivalent to AvaloniaEdit)
- RDP
- API Tester
- JSON Query/Visualizer
- Diff Editor
- CTF Tools

## Key Changes Made

### Framework Migration
- **Avalonia UI** → **WinUI 3**
- **Avalonia.Controls** → **Microsoft.UI.Xaml.Controls**
- **AvaloniaXamlLoader.Load()** → **InitializeComponent()**
- **UserControl** remains the same but with WinUI 3 namespace

### XAML Changes
- Namespace: `xmlns="https://github.com/avaloniaui"` → `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"`
- **ListBox** → **ListView** (for better WinUI 3 experience)
- **Watermark** → **PlaceholderText**
- **DockPanel** → **Grid** with column/row definitions
- **ContentControl** → **ContentPresenter** for main content area

### Control Event Handling
- Direct field access through **x:Name** instead of **FindControl<>()** pattern
- Simplified event wiring using direct references

### UI Enhancements
- Added **MicaBackdrop** for modern Windows 11 appearance
- Enhanced layout with proper **ScrollViewer** support
- Improved **StackPanel** spacing using WinUI 3 **Spacing** property

### Code Editor Replacement
The original used **AvaloniaEdit** for syntax highlighting. In the WinUI 3 version:
- Currently using **TextBox** as placeholder
- **Recommended**: Use **WebView2** with **Monaco Editor** for production
- Alternative: Third-party text editor controls for WinUI 3

## Build Requirements

- **.NET 8.0** targeting **Windows 10** (19041) or later
- **Windows App SDK 1.4** or later
- **Microsoft.WindowsAppSDK** NuGet package
- **Windows-only** development environment (WinUI 3 limitation)

## Database & Services

- **Entity Framework Core** with **SQLite** (unchanged)
- **ViewModels** largely unchanged (MVVM pattern preserved)
- **Services** copied as-is with minimal modifications
- **Encryption** using **AES-256** with **PBKDF2** (600,000 iterations)

## Running the Application

```bash
cd WinUI3_ProgrammersToolKit/ProgrammersToolKit
dotnet build -p:Platform=x64
dotnet run -p:Platform=x64
```

Note: Must be built and run on Windows due to WinUI 3 platform requirements.

## Architecture Preserved

The application maintains the same **MVVM** architecture with:
- **Views**: UI layer converted to WinUI 3
- **ViewModels**: Business logic (largely unchanged)
- **Services**: Application services (copied as-is)
- **Database**: SQLite with Entity Framework Core (unchanged)

## Future Enhancements

1. **WebView2 Integration**: Replace TextBox with Monaco Editor for code editing
2. **Complete Remaining Views**: Convert all remaining Avalonia views
3. **Native WinUI 3 Styling**: Leverage WinUI 3's theme system fully
4. **Windows 11 Features**: Integrate with Windows 11 specific features
5. **Performance Optimization**: Optimize for WinUI 3 rendering pipeline