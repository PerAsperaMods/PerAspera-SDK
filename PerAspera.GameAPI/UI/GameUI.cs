using Il2CppInterop.Runtime;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Managed mirror of <c>UIManager.PanelKey</c> (interop dump: UIManager.cs:25).
    /// Values MUST stay in the same order as the native enum.
    /// </summary>
    public enum GamePanel
    {
        None,
        PlanetStats,
        ResourceStats,
        BuildingScreenPanel,
        BuildingTable,
        FactionGraphs,
        SiteInfoScreenPanel,
        MultipleBuildingScreenPanel,
        DistrictInfoPanel,
        DistrictListPanel
    }

    /// <summary>
    /// Managed mirror of <c>UIManager.ModalKey</c> (interop dump: UIManager.cs:12).
    /// Values MUST stay in the same order as the native enum.
    /// </summary>
    public enum GameModal
    {
        Techtree,
        Menu,
        Knowledge,
        PopUp,
        TerraformingPlan,
        Demo,
        SpaceProject,
        None,
        MultiplayerPlayerList
    }

    /// <summary>Managed mirror of <c>Notification.Urgency</c>.</summary>
    public enum NotificationUrgency
    {
        Default,
        Urgent,
        Warning,
        Dialogue,
        Info
    }

    /// <summary>
    /// Typed façade over the game's native UI system.
    ///
    /// Entry point chain (all typed interop, no reflection):
    /// <c>BaseGame.canvasRefs : GameCanvasReferences</c> → UiManager, PopupManager,
    /// NotificationPresenter, and every native panel (buildingScreenPanel,
    /// knowledgeBasePanel, resourcesPanel…).
    ///
    /// Requires the game UI to be loaded — call after <c>GameFullyLoadedEvent</c>
    /// (all members return null/false safely before that).
    /// </summary>
    /// <example>
    /// // Native popup with a callback button:
    /// GameUI.ShowPopup("Trade Complete", "Helios Corp", "You earned 500 credits.", "OK",
    ///     () => LogAspera.Info("popup closed"));
    ///
    /// // Open / close native panels:
    /// GameUI.OpenPanel(GamePanel.PlanetStats);
    /// GameUI.CloseCurrentPanel();
    /// </example>
    public static class GameUI
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.UI.GameUI");

        // ==================== TYPED ACCESS ====================

        /// <summary>
        /// The game's UI reference hub (null until the game scene is loaded).
        /// Exposes every native panel typed: buildingScreenPanel, knowledgeBasePanel,
        /// techTreePanel, resourcesPanel, dialoguePresenter, tooltipPresenter…
        /// </summary>
        /// <example>var kb = GameUI.CanvasRefs?.knowledgeBasePanel;</example>
        public static GameCanvasReferences? CanvasRefs
            => BaseGameWrapper.GetCurrent()?.CanvasRefs;

        /// <summary>Native UI manager — panel/modal lifecycle (null before game load).</summary>
        /// <example>GameUI.UiManager?.ClosePanel(UIManager.PanelKey.PlanetStats);</example>
        public static UIManager? UiManager => CanvasRefs?.UiManager;

        /// <summary>Native popup manager (null before game load).</summary>
        /// <example>GameUI.Popups?.CloseAllNonModalPopups();</example>
        public static PopupManager? Popups => UiManager?.popupManager;

        /// <summary>Native notification presenter (null before game load).</summary>
        /// <example>GameUI.Notifications?.AddNotification(myNotification);</example>
        public static NotificationPresenter? Notifications => CanvasRefs?.notificationPresenter;

        /// <summary>True when the native UI is initialized and usable.</summary>
        /// <example>if (GameUI.IsReady) GameUI.OpenPanel(GamePanel.PlanetStats);</example>
        public static bool IsReady => CanvasRefs != null && UiManager != null;

        // ==================== POPUPS ====================

        /// <summary>
        /// Show a native game popup (same visual as tech-unlocked popups) with one button.
        /// Wraps <c>PopupManager.ShowGenericPopup</c> — the managed callback is converted
        /// to an IL2CPP delegate via <c>DelegateSupport</c>.
        /// </summary>
        /// <param name="title">Popup title (plain text, not a localization key).</param>
        /// <param name="subtitle">Subtitle line under the title.</param>
        /// <param name="description">Body text.</param>
        /// <param name="buttonLabel">Label of the single action button.</param>
        /// <param name="onButton">Optional managed callback invoked when the button is clicked.</param>
        /// <param name="canCloseWithEsc">Allow closing with Escape (default true).</param>
        /// <returns>True if the popup was shown, false if the UI is not ready.</returns>
        /// <example>
        /// GameUI.ShowPopup("Embargo!", "Helios Corp", "Reputation dropped below -50.", "Understood");
        /// </example>
        public static bool ShowPopup(string title, string subtitle, string description,
            string buttonLabel, Action? onButton = null, bool canCloseWithEsc = true)
        {
            var popups = Popups;
            if (popups == null)
            {
                _log.Warning("ShowPopup: PopupManager not available (UI not loaded yet?)");
                return false;
            }

            try
            {
                Il2CppSystem.Action? il2cppAction = null;
                if (onButton != null)
                    il2cppAction = DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(onButton);

                popups.ShowGenericPopup(title, subtitle, description, buttonLabel,
                    il2cppAction!, null, null, null, canCloseWithEsc);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"ShowPopup failed: {ex.Message}");
                return false;
            }
        }

        // ==================== PANELS / MODALS ====================

        /// <summary>
        /// Open a native side panel (PlanetStats, ResourceStats, BuildingTable…).
        /// </summary>
        /// <param name="panel">Panel to open.</param>
        /// <param name="closeIfAlreadyOpen">Toggle behavior when already open.</param>
        /// <returns>True if dispatched to the native UIManager.</returns>
        /// <example>GameUI.OpenPanel(GamePanel.FactionGraphs);</example>
        public static bool OpenPanel(GamePanel panel, bool closeIfAlreadyOpen = false)
        {
            var ui = UiManager;
            if (ui == null) return false;
            try
            {
                ui.OpenPanel((UIManager.PanelKey)(int)panel, closeIfAlreadyOpen);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"OpenPanel({panel}) failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Close a native side panel.</summary>
        /// <example>GameUI.ClosePanel(GamePanel.PlanetStats);</example>
        public static bool ClosePanel(GamePanel panel)
        {
            var ui = UiManager;
            if (ui == null) return false;
            try
            {
                ui.ClosePanel((UIManager.PanelKey)(int)panel);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"ClosePanel({panel}) failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Close whatever panel is currently open (no-op if none).</summary>
        /// <example>GameUI.CloseCurrentPanel();</example>
        public static bool CloseCurrentPanel()
        {
            var current = CurrentPanel;
            return current != GamePanel.None && ClosePanel(current);
        }

        /// <summary>The currently open native panel (GamePanel.None if none).</summary>
        /// <example>if (GameUI.CurrentPanel == GamePanel.PlanetStats) { ... }</example>
        public static GamePanel CurrentPanel
        {
            get
            {
                var ui = UiManager;
                if (ui == null) return GamePanel.None;
                try { return (GamePanel)(int)ui.currentPanelKey; }
                catch { return GamePanel.None; }
            }
        }

        /// <summary>
        /// Open a native modal window (tech tree, knowledge base, space projects…).
        /// </summary>
        /// <example>GameUI.OpenModal(GameModal.SpaceProject);</example>
        public static bool OpenModal(GameModal modal, bool force = false)
        {
            var ui = UiManager;
            if (ui == null) return false;
            try
            {
                ui.OpenModal((UIManager.ModalKey)(int)modal, force);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"OpenModal({modal}) failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Close a native modal window.</summary>
        /// <example>GameUI.CloseModal(GameModal.Techtree);</example>
        public static bool CloseModal(GameModal modal)
        {
            var ui = UiManager;
            if (ui == null) return false;
            try
            {
                ui.CloseModal((UIManager.ModalKey)(int)modal);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"CloseModal({modal}) failed: {ex.Message}");
                return false;
            }
        }

        // ==================== NOTIFICATIONS (EXPERIMENTAL) ====================

        /// <summary>
        /// EXPERIMENTAL — show a native notification in the right-side notification feed.
        /// Builds a <c>Notification</c> from scratch (public ctor + writable fields confirmed
        /// in the interop dump). Text resolution goes through <c>displayName</c>;
        /// the <c>textId</c> localization path is untested for non-localized strings —
        /// validate in game before shipping a mod that relies on it.
        /// </summary>
        /// <param name="displayName">Text shown in the notification item.</param>
        /// <param name="urgency">Urgency column (Info / Warning / Urgent).</param>
        /// <param name="timeoutDays">Lifetime in game days before auto-expiry (0 = no expiry).</param>
        /// <returns>True if handed to the native NotificationPresenter.</returns>
        /// <example>
        /// GameUI.ShowNotification("Cargo arrived in orbit", NotificationUrgency.Info, timeoutDays: 2f);
        /// </example>
        public static bool ShowNotification(string displayName,
            NotificationUrgency urgency = NotificationUrgency.Info, float timeoutDays = 0f)
        {
            var presenter = Notifications;
            if (presenter == null)
            {
                _log.Warning("ShowNotification: NotificationPresenter not available");
                return false;
            }

            try
            {
                var notification = new Notification
                {
                    displayName = displayName,
                    urgency = (Notification.Urgency)(int)urgency,
                };
                // 'expires' is a computed read-only property — the native side derives it
                // from timeout, so setting timeout > 0 is enough for auto-expiry.
                if (timeoutDays > 0f)
                    notification.timeout = timeoutDays;

                presenter.AddNotification(notification);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"ShowNotification failed: {ex.Message}");
                return false;
            }
        }
    }
}
#pragma warning restore CS1591
