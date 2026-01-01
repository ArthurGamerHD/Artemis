using System.Threading.Tasks;
using Artemis.WebClient.Workshop;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class RecentlyUpdatedItemView : ReactiveUserControl<RecentlyUpdatedItemViewModel>
{
    public RecentlyUpdatedItemView()
    {
        InitializeComponent();
    }

    private void Entry_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ViewModel?.NavigateToEntry();
    }

    private void Release_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        object? dataContext = (sender as TextBlock)?.DataContext;
        ViewModel?.NavigateToRelease(dataContext as IGetRecentUpdates_Entries_Releases);
    }
}