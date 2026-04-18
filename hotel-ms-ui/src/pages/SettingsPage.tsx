import { useState } from 'react';
import { User, Bell, Shield, Globe, Palette, Key, Save, Sun, Moon, Monitor, Mail } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Select } from '../components/ui/Select';
import { Badge } from '../components/ui/Badge';
import { useAuthStore, useThemeStore, useSettingsStore, useToast } from '../lib/store';
import { useTheme } from '../hooks/useTheme';
import { cn, getInitials } from '../lib/utils';
import api from '../lib/axios';

const CURRENCIES = [
  { value: 'USD', label: 'USD — US Dollar ($)' },
  { value: 'EUR', label: 'EUR — Euro (€)' },
  { value: 'GBP', label: 'GBP — British Pound (£)' },
  { value: 'NGN', label: 'NGN — Nigerian Naira (₦)' },
  { value: 'GHS', label: 'GHS — Ghanaian Cedi (₵)' },
  { value: 'KES', label: 'KES — Kenyan Shilling (KSh)' },
  { value: 'ZAR', label: 'ZAR — South African Rand (R)' },
  { value: 'CAD', label: 'CAD — Canadian Dollar (CA$)' },
  { value: 'AUD', label: 'AUD — Australian Dollar (A$)' },
  { value: 'JPY', label: 'JPY — Japanese Yen (¥)' },
  { value: 'CNY', label: 'CNY — Chinese Yuan (¥)' },
  { value: 'INR', label: 'INR — Indian Rupee (₹)' },
  { value: 'AED', label: 'AED — UAE Dirham (د.إ)' },
  { value: 'SAR', label: 'SAR — Saudi Riyal (﷼)' },
  { value: 'CHF', label: 'CHF — Swiss Franc (CHF)' },
];

const TIMEZONES = [
  { value: 'UTC', label: 'UTC' },
  { value: 'America/New_York', label: 'Eastern Time (ET)' },
  { value: 'America/Chicago', label: 'Central Time (CT)' },
  { value: 'America/Denver', label: 'Mountain Time (MT)' },
  { value: 'America/Los_Angeles', label: 'Pacific Time (PT)' },
  { value: 'Europe/London', label: 'London (GMT/BST)' },
  { value: 'Europe/Paris', label: 'Paris (CET)' },
  { value: 'Africa/Lagos', label: 'Lagos (WAT)' },
  { value: 'Africa/Nairobi', label: 'Nairobi (EAT)' },
  { value: 'Africa/Johannesburg', label: 'Johannesburg (SAST)' },
  { value: 'Asia/Dubai', label: 'Dubai (GST)' },
  { value: 'Asia/Kolkata', label: 'India (IST)' },
  { value: 'Asia/Singapore', label: 'Singapore (SGT)' },
  { value: 'Asia/Tokyo', label: 'Tokyo (JST)' },
  { value: 'Australia/Sydney', label: 'Sydney (AEST)' },
];

const TABS = [
  { id: 'notifications', label: 'Notifications', icon: <Bell className="h-4 w-4" /> },
  { id: 'appearance', label: 'Appearance', icon: <Palette className="h-4 w-4" /> },
  { id: 'tenant', label: 'Hotel Settings', icon: <Globe className="h-4 w-4" /> },
  { id: 'security', label: 'Security', icon: <Shield className="h-4 w-4" /> },
];

function ToggleSwitch({ checked, onChange }: { checked: boolean; onChange: (v: boolean) => void }) {
  return (
    <button
      type="button"
      onClick={() => onChange(!checked)}
      className={cn(
        'relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2',
        checked ? 'bg-indigo-600' : 'bg-slate-200 dark:bg-slate-700'
      )}
    >
      <span
        className={cn(
          'inline-block h-4 w-4 transform rounded-full bg-white shadow transition-transform',
          checked ? 'translate-x-6' : 'translate-x-1'
        )}
      />
    </button>
  );
}

export function SettingsPage() {
  const [activeTab, setActiveTab] = useState('notifications');
  const { user } = useAuthStore();
  const { theme, setTheme } = useTheme();
  const { currency, setCurrency } = useThemeStore();
  const { notifications, tenantConfig, setNotifications, setTenantConfig } = useSettingsStore();
  const toast = useToast();
  const [isSaving, setIsSaving] = useState(false);
  const [isSendingReset, setIsSendingReset] = useState(false);

  const nameParts = (user?.fullName ?? '').trim().split(/\s+/);
  const initials = user ? getInitials(nameParts[0] || 'U', nameParts.slice(1).join(' ') || 'S') : 'US';

  // Local draft state for tenant config (committed on Save)
  const [draft, setDraft] = useState({ ...tenantConfig });

  const handleSaveNotifications = () => {
    // already live-saved via store toggles; just confirm
    toast.success('Notifications saved', 'Your notification preferences have been updated');
  };

  const handleSaveTenant = () => {
    setIsSaving(true);
    setTenantConfig(draft);
    setIsSaving(false);
    toast.success('Hotel settings saved', 'Settings have been saved to your browser');
  };

  const handleSaveCurrency = () => {
    toast.success('Currency saved', `Currency set to ${currency}`);
  };

  const handleSendResetEmail = async () => {
    if (!user?.email) return;
    setIsSendingReset(true);
    try {
      await api.post('/api/v1/user/forgot-password', { email: user.email });
      toast.success('Reset email sent', `A password reset link has been sent to ${user.email}`);
    } catch {
      toast.error('Failed', 'Could not send reset email. Please try again.');
    } finally {
      setIsSendingReset(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <h2 className="page-title">Settings</h2>
        <p className="page-subtitle">Manage your account and system preferences</p>
      </div>

      {/* User info banner */}
      <Card className="mb-6" padding="sm">
        <div className="flex items-center gap-4 px-2 py-1">
          <div className="w-12 h-12 rounded-xl bg-indigo-600 flex items-center justify-center flex-shrink-0">
            <span className="text-lg font-bold text-white">{initials}</span>
          </div>
          <div>
            <p className="font-semibold text-slate-900 dark:text-slate-100">{user?.fullName || user?.email || 'User'}</p>
            <p className="text-sm text-slate-500 dark:text-slate-400">{user?.email}</p>
          </div>
          <div className="ml-auto">
            <Badge variant="primary">{user?.roles?.[0] ?? 'Admin'}</Badge>
          </div>
        </div>
      </Card>

      <div className="flex flex-col lg:flex-row gap-6">
        {/* Sidebar tabs */}
        <div className="lg:w-52 flex-shrink-0">
          <Card padding="sm">
            <nav className="space-y-1">
              {TABS.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={cn(
                    'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all',
                    activeTab === tab.id
                      ? 'bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400'
                      : 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 hover:text-slate-900 dark:hover:text-slate-100'
                  )}
                >
                  {tab.icon}
                  {tab.label}
                </button>
              ))}
            </nav>
          </Card>
        </div>

        {/* Content */}
        <div className="flex-1">

          {/* Notifications Tab */}
          {activeTab === 'notifications' && (
            <Card>
              <CardHeader>
                <CardTitle>Notification Preferences</CardTitle>
              </CardHeader>
              <div className="space-y-1">
                <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">
                  Choose which in-app notifications you'd like to receive.
                </p>

                {([
                  { key: 'newReservations',   label: 'New Reservations',    desc: 'When a new reservation is created' },
                  { key: 'checkInReminders',  label: 'Check-in Reminders',  desc: 'Reminders for upcoming guest check-ins' },
                  { key: 'checkOutReminders', label: 'Check-out Reminders', desc: 'Reminders for scheduled check-outs' },
                  { key: 'maintenanceAlerts', label: 'Maintenance Alerts',  desc: 'Alerts for room maintenance issues' },
                  { key: 'systemUpdates',     label: 'System Updates',      desc: 'News about system features and updates' },
                ] as const).map((item) => (
                  <div key={item.key} className="flex items-center justify-between py-4 border-b border-slate-100 dark:border-slate-700 last:border-0">
                    <div>
                      <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{item.label}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{item.desc}</p>
                    </div>
                    <ToggleSwitch
                      checked={notifications[item.key]}
                      onChange={(val) => setNotifications({ [item.key]: val })}
                    />
                  </div>
                ))}

                <div className="flex justify-end pt-4">
                  <Button leftIcon={<Save className="h-4 w-4" />} onClick={handleSaveNotifications}>
                    Save Preferences
                  </Button>
                </div>
              </div>
            </Card>
          )}

          {/* Appearance Tab */}
          {activeTab === 'appearance' && (
            <Card>
              <CardHeader>
                <CardTitle>Appearance</CardTitle>
              </CardHeader>
              <div className="space-y-6">
                <div>
                  <p className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Theme</p>
                  <div className="grid grid-cols-3 gap-3">
                    {[
                      { value: 'light', label: 'Light', icon: <Sun className="h-5 w-5" /> },
                      { value: 'dark',  label: 'Dark',  icon: <Moon className="h-5 w-5" /> },
                    ].map((t) => (
                      <button
                        key={t.value}
                        onClick={() => setTheme(t.value as 'light' | 'dark')}
                        className={cn(
                          'flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all',
                          theme === t.value
                            ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-600 dark:text-indigo-400'
                            : 'border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-400 hover:border-slate-300 dark:hover:border-slate-600'
                        )}
                      >
                        {t.icon}
                        <span className="text-sm font-medium">{t.label}</span>
                      </button>
                    ))}
                    <button
                      className="flex flex-col items-center gap-2 p-4 rounded-xl border-2 border-slate-200 dark:border-slate-700 text-slate-400 cursor-default opacity-50"
                      title="System theme coming soon"
                      disabled
                    >
                      <Monitor className="h-5 w-5" />
                      <span className="text-sm font-medium">System</span>
                    </button>
                  </div>
                </div>

                <div className="border-t border-slate-100 dark:border-slate-700 pt-6">
                  <p className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Currency</p>
                  <div className="flex gap-3 items-end">
                    <div className="flex-1 max-w-xs">
                      <Select
                        value={currency}
                        onChange={(e) => setCurrency(e.target.value)}
                        options={CURRENCIES}
                      />
                    </div>
                    <Button leftIcon={<Save className="h-4 w-4" />} onClick={handleSaveCurrency}>
                      Save
                    </Button>
                  </div>
                  <p className="text-xs text-slate-400 dark:text-slate-500 mt-2">
                    Affects all currency displays across the application.
                  </p>
                </div>
              </div>
            </Card>
          )}

          {/* Hotel Settings Tab */}
          {activeTab === 'tenant' && (
            <Card>
              <CardHeader>
                <CardTitle>Hotel Settings</CardTitle>
              </CardHeader>
              <div className="space-y-5">
                <div className="p-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-xl">
                  <p className="text-xs text-amber-700 dark:text-amber-400">
                    These settings are saved locally in your browser. For centralized multi-device config, manage via the Properties page.
                  </p>
                </div>

                <div>
                  <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3">Hotel Info</p>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <Input
                      label="Hotel Name"
                      value={draft.hotelName}
                      onChange={(e) => setDraft((p) => ({ ...p, hotelName: e.target.value }))}
                      placeholder="Grand Hotel"
                    />
                    <Input
                      label="Hotel Email"
                      type="email"
                      value={draft.hotelEmail}
                      onChange={(e) => setDraft((p) => ({ ...p, hotelEmail: e.target.value }))}
                      placeholder="info@hotel.com"
                    />
                    <Input
                      label="Phone Number"
                      type="tel"
                      value={draft.phoneNumber}
                      onChange={(e) => setDraft((p) => ({ ...p, phoneNumber: e.target.value }))}
                      placeholder="+1 555-0100"
                    />
                    <Input
                      label="Website"
                      value={draft.website}
                      onChange={(e) => setDraft((p) => ({ ...p, website: e.target.value }))}
                      placeholder="www.yourhotel.com"
                    />
                    <div className="sm:col-span-2">
                      <Input
                        label="Address"
                        value={draft.address}
                        onChange={(e) => setDraft((p) => ({ ...p, address: e.target.value }))}
                        placeholder="Full street address"
                      />
                    </div>
                    <Input
                      label="City"
                      value={draft.city}
                      onChange={(e) => setDraft((p) => ({ ...p, city: e.target.value }))}
                      placeholder="City"
                    />
                    <Input
                      label="Country"
                      value={draft.country}
                      onChange={(e) => setDraft((p) => ({ ...p, country: e.target.value }))}
                      placeholder="Country"
                    />
                    <Select
                      label="Timezone"
                      value={draft.timezone}
                      onChange={(e) => setDraft((p) => ({ ...p, timezone: e.target.value }))}
                      options={TIMEZONES}
                    />
                  </div>
                </div>

                <div className="border-t border-slate-100 dark:border-slate-700 pt-5">
                  <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3">Hotel Policies</p>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <Input
                      label="Check-in Time"
                      type="time"
                      value={draft.checkInTime}
                      onChange={(e) => setDraft((p) => ({ ...p, checkInTime: e.target.value }))}
                    />
                    <Input
                      label="Check-out Time"
                      type="time"
                      value={draft.checkOutTime}
                      onChange={(e) => setDraft((p) => ({ ...p, checkOutTime: e.target.value }))}
                    />
                    <Input
                      label="Late Check-out Fee"
                      type="number"
                      value={draft.lateCheckOutFee}
                      onChange={(e) => setDraft((p) => ({ ...p, lateCheckOutFee: e.target.value }))}
                      placeholder="e.g. 50"
                    />
                    <Input
                      label="Cancellation Window (hours)"
                      type="number"
                      value={draft.cancellationWindowHours}
                      onChange={(e) => setDraft((p) => ({ ...p, cancellationWindowHours: e.target.value }))}
                    />
                  </div>
                </div>

                <div className="flex justify-end">
                  <Button leftIcon={<Save className="h-4 w-4" />} isLoading={isSaving} onClick={handleSaveTenant}>
                    Save Settings
                  </Button>
                </div>
              </div>
            </Card>
          )}

          {/* Security Tab */}
          {activeTab === 'security' && (
            <Card>
              <CardHeader>
                <CardTitle>Security</CardTitle>
              </CardHeader>
              <div className="space-y-6">
                <div className="p-4 bg-emerald-50 dark:bg-emerald-900/20 rounded-xl border border-emerald-200 dark:border-emerald-800">
                  <div className="flex items-center gap-3">
                    <Shield className="h-5 w-5 text-emerald-600 dark:text-emerald-400" />
                    <div>
                      <p className="text-sm font-medium text-emerald-800 dark:text-emerald-300">Account Secure</p>
                      <p className="text-xs text-emerald-600 dark:text-emerald-400">Your account is protected with a strong password</p>
                    </div>
                  </div>
                </div>

                <div>
                  <h4 className="text-sm font-semibold text-slate-900 dark:text-slate-100 mb-2 flex items-center gap-2">
                    <Key className="h-4 w-4" /> Change Password
                  </h4>
                  <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">
                    We'll send a secure password reset link to <span className="font-medium text-slate-700 dark:text-slate-300">{user?.email}</span>.
                  </p>
                  <Button
                    variant="outline"
                    leftIcon={<Mail className="h-4 w-4" />}
                    isLoading={isSendingReset}
                    onClick={handleSendResetEmail}
                  >
                    Send Password Reset Email
                  </Button>
                </div>

                <div className="border-t border-slate-100 dark:border-slate-700 pt-6">
                  <h4 className="text-sm font-semibold text-slate-900 dark:text-slate-100 mb-3">Account Info</h4>
                  <div className="space-y-3">
                    <div className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                      <div>
                        <p className="text-sm font-medium text-slate-900 dark:text-slate-100">Email</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400">{user?.email}</p>
                      </div>
                    </div>
                    <div className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                      <div>
                        <p className="text-sm font-medium text-slate-900 dark:text-slate-100">Role</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400">{user?.roles?.join(', ') ?? '—'}</p>
                      </div>
                    </div>
                    <div className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                      <div>
                        <p className="text-sm font-medium text-slate-900 dark:text-slate-100">Tenant ID</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400">{user?.tenantId ?? '—'}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

export default SettingsPage;
