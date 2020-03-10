using SeventhHeavenUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.Classes
{
    public enum StringKey
    {
        Play,
        PlayWithMods,
        StartedClickHereToViewAppLog,
        HintLabel,
        CheckingForUpdates,
        ModRequiresModsIsMissingWarning,
        UnsupportedModVersionsWarning,
        ThisModRequiresYouActivateFollowingMods,
        ModRequiresYouDeactivateTheFollowingMods,
        CannotActivateModBecauseItIsIncompatibleWithOtherMod,
        CannotActivateModBecauseDependentIsIncompatible,
        SelectAll,
        Unknown,
        UpdateAvailable,
        UpdateDownloading,
        NoUpdates,
        UpdatesIgnored,
        AutoUpdate,
        FollowingErrorsFoundInConfiguration,
        ErrorsFoundInGeneralSettingsViewAppLog,
        AppUpdateIsAvailableMessage,
        NewVersionAvailable,
        ThisModContainsDataThatCouldHarm,
        CannotOpenHelp,
        SubscriptionIsAlreadyAdded,
        AddedToSubscriptions,
        IrosLinkMayBeFormatedIncorrectly,
        FailedToSetBackgroundImageFromTheme
    }

    public static class ResourceHelper
    {
        public static string GetString(StringKey key)
        {
            try
            {
                return App.Current.Resources[key.ToString()].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
