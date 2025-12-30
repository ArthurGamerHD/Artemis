using System;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class RecentlyUpdatedItemViewModel : ActivatableViewModelBase
{
    public IGetRecentUpdates_Entries Entry { get; }
    public IGetRecentUpdates_Entries_LatestRelease Release { get; }
    public InstalledEntry InstalledEntry { get; }

    public RecentlyUpdatedItemViewModel(IGetRecentUpdates_Entries entry, IWorkshopService workshopService)
    {
        Entry = entry;
        Release = entry.LatestRelease ?? throw new InvalidOperationException("Entry does not have a latest release");
        InstalledEntry = workshopService.GetInstalledEntry(entry.Id) ?? throw new InvalidOperationException("Entry is not installed");
    }

    public bool NotYetInstalled => InstalledEntry.ReleaseId < Release.Id;
}