using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Services;
using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Pages.Admin
{
    public class SettingsModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<SettingsModel> _logger;

        public SettingsModel(
            IAuthService authService,
            ILogger<SettingsModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public SystemSettings Settings { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? returnUrl)
        {
            try
            {
                // Verifica che l'utente sia admin
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Admin/Settings" });
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    _logger.LogWarning("Non-admin user {Email} tried to access admin settings page", currentUser?.Email);
                    return RedirectToPage("/Index");
                }

                LoadSettings();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin settings page");
                LoadDefaultSettings();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Verifica che l'utente sia admin
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login");
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    return RedirectToPage("/Index");
                }

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Controlla i dati inseriti";
                    return Page();
                }

                // In una vera implementazione, salveresti le impostazioni nel database
                _logger.LogInformation("Settings updated by admin {Email}", currentUser.Email);
                
                TempData["SuccessMessage"] = "Impostazioni salvate con successo";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                TempData["ErrorMessage"] = "Errore nel salvataggio delle impostazioni";
                return Page();
            }
        }

        private void LoadSettings()
        {
            // In una vera implementazione, caricheresti dal database
            LoadDefaultSettings();
        }

        private void LoadDefaultSettings()
        {
            Settings = new SystemSettings
            {
                // General Settings
                SystemName = "SharingMezzi",
                SystemVersion = "2.1.0",
                MaintenanceMode = false,
                AllowNewRegistrations = true,
                RequireEmailVerification = true,
                MaxLoginAttempts = 5,
                SessionTimeoutMinutes = 120,

                // Vehicle Settings
                DefaultBatteryThreshold = 20,
                MaxTripDurationHours = 24,
                AutoReturnTimeoutMinutes = 180,
                MaintenanceIntervalDays = 30,

                // Payment Settings
                DefaultTariffPerMinute = 0.15m,
                MinimumCreditAmount = 5.00m,
                MaximumCreditAmount = 500.00m,
                LowCreditThreshold = 10.00m,
                AutoRechargeEnabled = false,
                AutoRechargeAmount = 25.00m,

                // Notification Settings
                EmailNotificationsEnabled = true,
                SmsNotificationsEnabled = false,
                PushNotificationsEnabled = true,
                MaintenanceAlertsEnabled = true,
                LowBatteryAlertsEnabled = true,

                // Security Settings
                TwoFactorAuthRequired = false,
                PasswordMinLength = 8,
                PasswordRequireSpecialChars = true,
                ApiRateLimitPerHour = 1000,
                LogRetentionDays = 90,

                // Integration Settings
                MqttBrokerUrl = "localhost:1883",
                MqttEnabled = true,
                SignalREnabled = true,
                ApiLoggingEnabled = true,
                TelemetryEnabled = true
            };
        }
    }

    public class SystemSettings
    {
        // General Settings
        public string SystemName { get; set; } = string.Empty;
        public string SystemVersion { get; set; } = string.Empty;
        public bool MaintenanceMode { get; set; }
        public bool AllowNewRegistrations { get; set; }
        public bool RequireEmailVerification { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int SessionTimeoutMinutes { get; set; }

        // Vehicle Settings
        public int DefaultBatteryThreshold { get; set; }
        public int MaxTripDurationHours { get; set; }
        public int AutoReturnTimeoutMinutes { get; set; }
        public int MaintenanceIntervalDays { get; set; }

        // Payment Settings
        public decimal DefaultTariffPerMinute { get; set; }
        public decimal MinimumCreditAmount { get; set; }
        public decimal MaximumCreditAmount { get; set; }
        public decimal LowCreditThreshold { get; set; }
        public bool AutoRechargeEnabled { get; set; }
        public decimal AutoRechargeAmount { get; set; }

        // Notification Settings
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool MaintenanceAlertsEnabled { get; set; }
        public bool LowBatteryAlertsEnabled { get; set; }

        // Security Settings
        public bool TwoFactorAuthRequired { get; set; }
        public int PasswordMinLength { get; set; }
        public bool PasswordRequireSpecialChars { get; set; }
        public int ApiRateLimitPerHour { get; set; }
        public int LogRetentionDays { get; set; }

        // Integration Settings
        public string MqttBrokerUrl { get; set; } = string.Empty;
        public bool MqttEnabled { get; set; }
        public bool SignalREnabled { get; set; }
        public bool ApiLoggingEnabled { get; set; }
        public bool TelemetryEnabled { get; set; }
    }
}
