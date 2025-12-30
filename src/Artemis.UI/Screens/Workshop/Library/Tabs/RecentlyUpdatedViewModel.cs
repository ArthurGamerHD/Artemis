using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class RecentlyUpdatedViewModel : RoutableScreen
{
    private readonly IWorkshopService _workshopService;
    private readonly IWorkshopClient _client;
    private readonly IRouter _router;
    private readonly SourceCache<IGetRecentUpdates_Entries, long> _entries;
    private readonly ObservableAsPropertyHelper<bool> _empty;

    [Notify] private bool _isLoading = true;
    [Notify] private bool _workshopReachable;
    [Notify] private string? _searchEntryInput;

    public RecentlyUpdatedViewModel(IWorkshopService workshopService,
        IWorkshopClient client,
        IRouter router,
        Func<IGetRecentUpdates_Entries, RecentlyUpdatedItemViewModel> getRecentlyUpdatedItemViewModel)
    {
        IObservable<Func<IGetRecentUpdates_Entries, bool>> searchFilter = this.WhenAnyValue(vm => vm.SearchEntryInput)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(CreatePredicate);

        _workshopService = workshopService;
        _client = client;
        _router = router;
        _entries = new SourceCache<IGetRecentUpdates_Entries, long>(e => e.Id);

        _entries.Connect()
            .Filter(searchFilter)
            .Transform(getRecentlyUpdatedItemViewModel)
            .SortAndBind(
                out ReadOnlyObservableCollection<RecentlyUpdatedItemViewModel> entries,
                SortExpressionComparer<RecentlyUpdatedItemViewModel>.Descending(p => p.Release.CreatedAt)
            )
            .Subscribe();

        _empty = _entries.Connect().Count().Select(c => c == 0).ToProperty(this, vm => vm.Empty);

        Entries = entries;

        this.WhenActivatedAsync(async d =>
        {
            WorkshopReachable = await workshopService.ValidateWorkshopStatus(true, d.AsCancellationToken());
            if (WorkshopReachable)
                await GetEntries(d.AsCancellationToken());
        });
    }

    public bool Empty => _empty.Value;
    public ReadOnlyObservableCollection<RecentlyUpdatedItemViewModel> Entries { get; }

    public async Task OpenWorkshop()
    {
        await _router.Navigate("workshop");
    }
    
    private async Task GetEntries(CancellationToken ct)
    {
        IsLoading = true;

        try
        {
            List<InstalledEntry> installedEntries = _workshopService.GetInstalledEntries();
            IOperationResult<IGetRecentUpdatesResult> result = await _client.GetRecentUpdates.ExecuteAsync(
                installedEntries.Select(e => e.Id).ToList(),
                DateTimeOffset.Now.AddDays(-30).ToUniversalTime(),
                ct);

            if (result.Data?.Entries == null)
                _entries.Clear();
            else
                _entries.Edit(e =>
                {
                    e.Clear();
                    e.AddOrUpdate(result.Data.Entries);
                });
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Func<IGetRecentUpdates_Entries, bool> CreatePredicate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _ => true;

        return data => data.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }
}