using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProgrammersToolKit.Views
{
    public partial class CtfToolsMenuView : UserControl
    {
        public Button? InspectHeadersBtn { get; private set; }
        public Button? JwtDecodeBtn { get; private set; }
        public Button? HashCrackBtn { get; private set; }

        public CtfToolsMenuView()
        {
            AvaloniaXamlLoader.Load(this);
            InspectHeadersBtn = this.FindControl<Button>("MenuInspectHeadersBtn");
            JwtDecodeBtn = this.FindControl<Button>("MenuJwtDecodeBtn");
            HashCrackBtn = this.FindControl<Button>("MenuHashCrackBtn");
        }
    }
}
