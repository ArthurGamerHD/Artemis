using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class RecentlyUpdatedItemViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopService _workshopService;
    private readonly IRouter _router;
    [Notify] private bool _notYetInstalled;

    public RecentlyUpdatedItemViewModel(IGetRecentUpdates_Entries entry, IWorkshopService workshopService, IRouter router)
    {
        _workshopService = workshopService;
        _router = router;
        Releases = entry.Releases;
        Entry = entry;
        InstalledEntry = workshopService.GetInstalledEntry(entry.Id) ?? throw new InvalidOperationException("Entry is not installed");
        LatestRelease = Releases.First(r => r.Id == entry.LatestReleaseId);
        NotYetInstalled = InstalledEntry.ReleaseId != Releases.Max(r => r.Id);
    }


    public InstalledEntry InstalledEntry { get; }
    public IGetRecentUpdates_Entries Entry { get; }
    public IReadOnlyList<IGetRecentUpdates_Entries_Releases> Releases { get; set; }
    public IGetRecentUpdates_Entries_Releases LatestRelease { get; }

    public async Task NavigateToEntry()
    {
        await _workshopService.NavigateToEntry(Entry.Id, Entry.EntryType);
    }

    public async Task NavigateToRelease(IGetRecentUpdates_Entries_Releases? release)
    {
        if (release == null)
            return;

        await _router.Navigate($"workshop/entries/{Entry.EntryType.ToString()}s/details/{Entry.Id}/releases/{release.Id}");
    }
}