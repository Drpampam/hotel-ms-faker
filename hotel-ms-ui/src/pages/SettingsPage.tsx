import { useState } from 'react';
import { User, Bell, Shield, Globe, Palette, Key, Save, Sun, Moon, Monitor } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Badge } from '../components/ui/Badge';
import { useAuthStore, useToast } from '../lib/store';
import { useTheme } from '../hooks/useTheme';
import { cn, getInitials } from '../lib/utils';

const TABS = [
  { id: 'profile', label: 'Profile', icon: <User className="h-4 w-4" /> },
  { id: 'notifications', label: 'Notifications', icon: <Bell className="h-4 w-4" /> },
  { id: 'security', label: 'Security', icon: <Shield className="h-4 w-4" /> },
  { id: 'appearance', label: 'Appearance', icon: <Palette className="h-4 w-4" /> },
  { id: 'tenant', label: 'Tenant Settings', icon: <Globe className="h-4 w-4" /> },
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
  const [activeTab, setActiveTab] = useState('profile');
  const { user } = useAuthStore();
  const { theme, setTheme } = useTheme();
  const toast = useToast();
  const [isSaving, setIsSaving] = useState(false);

  const [notifications, setNotifications] = useState({
    newReservations: true,
    checkInReminders: true,
    checkOutReminders: false,
    maintenanceAlerts: true,
    systemUpdates: false,
  });

  const handleSave = async () => {
    setIsSaving(true);
    await new Promise((r) => setTimeout(r, 800));
    setIsSaving(false);
    toast.success('Settings saved', 'Your preferences have been updated');
  };

  const nameParts = (user?.fullName ?? '').trim().split(/\s+/);
  const initials = user ? getInitials(nameParts[0] || 'U', nameParts.slice(1).join(' ') || 'S') : 'US';

  return (
    <div className="page-container">
      <div className="page-header">
        <h2 className="page-title">Settings</h2>
        <p className="page-subtitle">Manage your account and system preferences</p>
      </div>

      <div className="flex flex-col lg:flex-row gap-6">
        {/* Sidebar tabs */}
        <div className="lg:w-56 flex-shrink-0">
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
          {/* Profile Tab */}
          {activeTab === 'profile' && (
            <Card>
              <CardHeader>
                <CardTitle>Profile Information</CardTitle>
              </CardHeader>

              {/* Avatar */}
              <div className="flex items-center gap-5 mb-8 pb-6 border-b border-slate-100 dark:border-slate-700">
                <div className="relative">
                  <div className="w-20 h-20 rounded-2xl bg-indigo-600 flex items-center justify-center">
                    <span className="text-2xl font-bold text-white">{initials}</span>
                  </div>
                  <button className="absolute -bottom-1 -right-1 w-7 h-7 bg-white dark:bg-slate-700 border border-slate-200 dark:border-slate-600 rounded-full flex items-center justify-center shadow-sm hover:shadow-md transition-shadow">
                    <User className="h-3.5 w-3.5 text-slate-600 dark:text-slate-400" />
                  </button>
                </div>
                <div>
                  <h3 className="font-semibold text-slate-900 dark:text-slate-100">
                    {user?.fullName || user?.email || 'User'}
                  </h3>
                  <p className="text-sm text-slate-500 dark:text-slate-400">{user?.email}</p>
                  <div className="mt-1">
                    <Badge variant="primary">{user?.roles?.[0] ?? 'Admin'}</Badge>
                  </div>
                </div>
              </div>

              <div className="space-y-5">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <Input label="First Name" defaultValue={nameParts[0] ?? ''} placeholder="First name" />
                  <Input label="Last Name" defaultValue={nameParts.slice(1).join(' ') ?? ''} placeholder="Last name" />
                  <div className="sm:col-span-2">
                    <Input label="Email Address" type="email" defaultValue={user?.email ?? ''} />
                  </div>
                  <Input label="Phone Number" type="tel" placeholder="+1 555-0100" />
                  <Input label="Job Title" placeholder="e.g. Hotel Manager" />
                </div>

                <div className="flex justify-end">
                  <Button leftIcon={<Save className="h-4 w-4" />} isLoading={isSaving} onClick={handleSave}>
                    Save Changes
                  </Button>
                </div>
              </div>
            </Card>
          )}

          {/* Notifications Tab */}
          {activeTab === 'notifications' && (
            <Card>
              <CardHeader>
                <CardTitle>Notification Preferences</CardTitle>
              </CardHeader>
              <div className="space-y-5">
                <p className="text-sm text-slate-500 dark:text-slate-400">
                  Choose which notifications you'd like to receive
                </p>

                {[
                  { key: 'newReservations', label: 'New Reservations', desc: 'Get notified when a new reservation is created' },
                  { key: 'checkInReminders', label: 'Check-in Reminders', desc: 'Reminders for upcoming guest check-ins' },
                  { key: 'checkOutReminders', label: 'Check-out Reminders', desc: 'Reminders for scheduled guest check-outs' },
                  { key: 'maintenanceAlerts', label: 'Maintenance Alerts', desc: 'Alerts for room maintenance issues' },
                  { key: 'systemUpdates', label: 'System Updates', desc: 'News about system features and updates' },
                ].map((item) => (
                  <div key={item.key} className="flex items-center justify-between py-4 border-b border-slate-100 dark:border-slate-700 last:border-0">
                    <div>
                      <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{item.label}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{item.desc}</p>
                    </div>
                    <ToggleSwitch
                      checked={notifications[item.key as keyof typeof notifications]}
                      onChange={(val) => setNotifications((prev) => ({ ...prev, [item.key]: val }))}
                    />
                  </div>
                ))}

                <div className="flex justify-end">
                  <Button leftIcon={<Save className="h-4 w-4" />} isLoading={isSaving} onClick={handleSave}>
                    Save Preferences
                  </Button>
                </div>
              </div>
            </Card>
          )}

          {/* Security Tab */}
          {activeTab === 'security' && (
            <Card>
              <CardHeader>
                <CardTitle>Security Settings</CardTitle>
              </CardHeader>
              <div className="space-y-6">
                <div className="p-4 bg-emerald-50 dark:bg-emerald-900/20 rounded-xl border border-emerald-200 dark:border-emerald-800">
                  <div className="flex items-center gap-3">
                    <Shield className="h-5 w-5 text-emerald-600 dark:text-emerald-400" />
                    <div>
                      <p className="text-sm font-medium text-emerald-800 dark:text-emerald-300">Account Secure</p>
                      <p className="text-xs text-emerald-600 dark:text-emerald-400">Your account is protected</p>
                    </div>
                  </div>
                </div>

                <div>
                  <h4 className="text-sm font-semibold text-slate-900 dark:text-slate-100 mb-4 flex items-center gap-2">
                    <Key className="h-4 w-4" /> Change Password
                  </h4>
                  <div className="space-y-4">
                    <Input label="Current Password" type="password" placeholder="Enter current password" />
                    <Input label="New Password" type="password" placeholder="Min. 8 characters" />
                    <Input label="Confirm New Password" type="password" placeholder="Repeat new password" />
                  </div>
                </div>

                <div className="border-t border-slate-100 dark:border-slate-700 pt-6">
                  <h4 className="text-sm font-semibold text-slate-900 dark:text-slate-100 mb-4">Active Sessions</h4>
                  <div className="space-y-3">
                    {[
                      { device: 'Chrome on Windows', location: 'New York, US', isCurrent: true, lastSeen: 'Now' },
                      { device: 'Safari on iPhone', location: 'New York, US', isCurrent: false, lastSeen: '2 hours ago' },
                    ].map((session) => (
                      <div key={session.device} className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                        <div>
                          <p className="text-sm font-medium text-slate-900 dark:text-slate-100 flex items-center gap-2">
                            {session.device}
                            {session.isCurrent && <Badge variant="success" size="sm">Current</Badge>}
                          </p>
                          <p className="text-xs text-slate-500 dark:text-slate-400">
                            {session.location} · {session.lastSeen}
                          </p>
                        </div>
                        {!session.isCurrent && (
                          <button className="text-xs text-red-500 hover:text-red-600 font-medium">Revoke</button>
                        )}
                      </div>
                    ))}
                  </div>
                </div>

                <div className="flex justify-end">
                  <Button leftIcon={<Save className="h-4 w-4" />} isLoading={isSaving} onClick={handleSave}>
                    Update Password
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
                      { value: 'dark', label: 'Dark', icon: <Moon className="h-5 w-5" /> },
                      { value: 'system', label: 'System', icon: <Monitor className="h-5 w-5" /> },
                    ].map((t) => (
                      <button
                        key={t.value}
                        onClick={() => {
                          if (t.value !== 'system') setTheme(t.value as 'light' | 'dark');
                        }}
                        className={cn(
                          'flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all',
                          (theme === t.value || (t.value === 'system' && theme !== 'light' && theme !== 'dark'))
                            ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-600 dark:text-indigo-400'
                            : 'border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-400 hover:border-slate-300 dark:hover:border-slate-600'
                        )}
                      >
                        {t.icon}
                        <span className="text-sm font-medium">{t.label}</span>
                      </button>
                    ))}
                  </div>
                </div>

                <div className="border-t border-slate-100 dark:border-slate-700 pt-6">
                  <p className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Accent Color</p>
                  <div className="flex flex-wrap gap-3">
                    {[
                      { label: 'Indigo', bg: 'bg-indigo-500', active: true },
                      { label: 'Violet', bg: 'bg-violet-500', active: false },
                      { label: 'Blue', bg: 'bg-blue-500', active: false },
                      { label: 'Emerald', bg: 'bg-emerald-500', active: false },
                      { label: 'Rose', bg: 'bg-rose-500', active: false },
                    ].map((color) => (
                      <button
                        key={color.label}
                        title={color.label}
                        className={cn(
                          'w-8 h-8 rounded-full transition-transform hover:scale-110',
                          color.bg,
                          color.active && 'ring-2 ring-offset-2 ring-indigo-500'
                        )}
                      />
                    ))}
                  </div>
                </div>

                <div className="border-t border-slate-100 dark:border-slate-700 pt-6">
                  <p className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Sidebar</p>
                  <div className="space-y-3">
                    {[
                      { label: 'Compact Mode', desc: 'Collapse sidebar by default', key: 'compact' },
                      { label: 'Show Labels', desc: 'Always show navigation labels', key: 'labels' },
                    ].map((item) => (
                      <div key={item.key} className="flex items-center justify-between py-3 border-b border-slate-100 dark:border-slate-700 last:border-0">
                        <div>
                          <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{item.label}</p>
                          <p className="text-xs text-slate-500 dark:text-slate-400">{item.desc}</p>
                        </div>
                        <ToggleSwitch checked={false} onChange={() => {}} />
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </Card>
          )}

          {/* Tenant Settings Tab */}
          {activeTab === 'tenant' && (
            <Card>
              <CardHeader>
                <CardTitle>Tenant Settings</CardTitle>
              </CardHeader>
              <div className="space-y-5">
                <div className="p-4 bg-indigo-50 dark:bg-indigo-900/20 rounded-xl border border-indigo-100 dark:border-indigo-800">
                  <p className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 uppercase tracking-wide mb-1">Current Tenant</p>
                  <p className="text-lg font-bold text-indigo-700 dark:text-indigo-300">Tenant ID: {user?.tenantId ?? 1}</p>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <Input label="Hotel Name" defaultValue="Grand Hotel" placeholder="Your hotel name" />
                  <Input label="Hotel Email" type="email" defaultValue="info@hotel.com" placeholder="contact@hotel.com" />
                  <Input label="Phone Number" type="tel" placeholder="+1 555-0100" />
                  <Input label="Website" placeholder="www.yourhotel.com" />
                  <div className="sm:col-span-2">
                    <Input label="Address" placeholder="Full address" />
                  </div>
                  <Input label="City" placeholder="City" />
                  <Input label="Country" placeholder="Country" />
                  <Input label="Currency" defaultValue="USD" placeholder="e.g. USD, EUR" />
                  <Input label="Timezone" defaultValue="America/New_York" placeholder="Timezone" />
                </div>

                <div className="border-t border-slate-100 dark:border-slate-700 pt-5">
                  <p className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Hotel Policies</p>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <Input label="Check-in Time" type="time" defaultValue="14:00" />
                    <Input label="Check-out Time" type="time" defaultValue="11:00" />
                    <Input label="Late Check-out Fee" type="number" placeholder="50" />
                    <Input label="Cancellation Window (hours)" type="number" defaultValue="24" />
                  </div>
                </div>

                <div className="flex justify-end">
                  <Button leftIcon={<Save className="h-4 w-4" />} isLoading={isSaving} onClick={handleSave}>
                    Save Settings
                  </Button>
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
