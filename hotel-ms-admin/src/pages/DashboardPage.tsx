import { useState, useEffect, useCallback } from 'react';
import { Copy, Check, KeyRound, RefreshCw, Plus, X, Hotel, LogOut } from 'lucide-react';
import { useAdminStore } from '../lib/store';
import adminService, { type ProvisionResult, type TenantSummary } from '../services/admin.service';
import { cn } from '../lib/utils';

type PlanKey = 'Trial' | 'Monthly3' | 'Monthly6' | 'FiveYear' | 'Unlimited';

const PLANS: { key: PlanKey; label: string; duration: string }[] = [
  { key: 'Trial',     label: '30-Day Trial', duration: '30 days' },
  { key: 'Monthly3',  label: '3-Month',      duration: '3 months' },
  { key: 'Monthly6',  label: '6-Month',      duration: '6 months' },
  { key: 'FiveYear',  label: '5-Year',       duration: '5 years' },
  { key: 'Unlimited', label: 'Unlimited',    duration: 'Forever' },
];

function CopyButton({ text }: { text: string }) {
  const [copied, setCopied] = useState(false);
  const handle = () => {
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };
  return (
    <button
      onClick={handle}
      title="Copy"
      className="p-1.5 rounded-md text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors"
    >
      {copied ? <Check className="h-3.5 w-3.5 text-green-500" /> : <Copy className="h-3.5 w-3.5" />}
    </button>
  );
}

function StatusBadge({ t }: { t: TenantSummary }) {
  if (t.isUnlimited) return <span className="inline-flex px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-700">Unlimited</span>;
  if (t.isExpired)   return <span className="inline-flex px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700">Expired</span>;
  if (t.daysRemaining !== null && t.daysRemaining <= 14)
    return <span className="inline-flex px-2 py-0.5 rounded-full text-xs font-medium bg-amber-100 text-amber-700">{t.daysRemaining}d left</span>;
  return <span className="inline-flex px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-700">Active</span>;
}

function formatDate(d: string | null) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function DashboardPage() {
  const { user, logout } = useAdminStore();

  // Provision form state
  const [showForm, setShowForm] = useState(false);
  const [email, setEmail] = useState('');
  const [fullName, setFullName] = useState('');
  const [plan, setPlan] = useState<PlanKey>('Monthly3');
  const [isProvisioning, setIsProvisioning] = useState(false);
  const [provisionError, setProvisionError] = useState('');
  const [result, setResult] = useState<ProvisionResult | null>(null);

  // Tenants list
  const [tenants, setTenants] = useState<TenantSummary[]>([]);
  const [isLoadingTenants, setIsLoadingTenants] = useState(true);

  // Renew state
  const [renewTarget, setRenewTarget] = useState<TenantSummary | null>(null);
  const [renewPlan, setRenewPlan] = useState<PlanKey>('Monthly3');
  const [isRenewing, setIsRenewing] = useState(false);
  const [renewError, setRenewError] = useState('');

  const fetchTenants = useCallback(async () => {
    setIsLoadingTenants(true);
    try {
      setTenants(await adminService.getAllTenants());
    } catch {
      // non-fatal
    } finally {
      setIsLoadingTenants(false);
    }
  }, []);

  useEffect(() => { fetchTenants(); }, [fetchTenants]);

  const handleProvision = async () => {
    if (!email.trim()) { setProvisionError('Client email is required.'); return; }
    setProvisionError('');
    setIsProvisioning(true);
    try {
      const data = await adminService.provisionTenant({ email: email.trim(), fullName: fullName.trim() || undefined, planType: plan });
      setResult(data);
      fetchTenants();
    } catch (err) {
      setProvisionError(err instanceof Error ? err.message : 'Provision failed.');
    } finally {
      setIsProvisioning(false);
    }
  };

  const resetForm = () => {
    setShowForm(false);
    setResult(null);
    setEmail('');
    setFullName('');
    setPlan('Monthly3');
    setProvisionError('');
  };

  const handleRenew = async () => {
    if (!renewTarget) return;
    setRenewError('');
    setIsRenewing(true);
    try {
      await adminService.renewSubscription(renewTarget.id, renewPlan);
      setRenewTarget(null);
      fetchTenants();
    } catch (err) {
      setRenewError(err instanceof Error ? err.message : 'Renewal failed.');
    } finally {
      setIsRenewing(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Top nav */}
      <header className="bg-white border-b border-slate-200 sticky top-0 z-10">
        <div className="max-w-6xl mx-auto px-6 h-14 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <div className="w-7 h-7 bg-indigo-600 rounded-lg flex items-center justify-center">
              <Hotel className="h-4 w-4 text-white" />
            </div>
            <span className="font-bold text-slate-800">HotelMS <span className="text-indigo-600">Admin</span></span>
          </div>
          <div className="flex items-center gap-3">
            <span className="text-sm text-slate-500">{user?.email}</span>
            <button
              onClick={logout}
              className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-red-600 transition-colors"
            >
              <LogOut className="h-4 w-4" />
              Sign out
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-8 space-y-8">
        {/* Provision section */}
        <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
            <div>
              <h2 className="font-semibold text-slate-900">Provision New Tenant</h2>
              <p className="text-xs text-slate-500 mt-0.5">Register a client and generate their login credentials and activation code.</p>
            </div>
            {!showForm && (
              <button
                onClick={() => setShowForm(true)}
                className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-xl transition-colors"
              >
                <Plus className="h-4 w-4" />
                New tenant
              </button>
            )}
          </div>

          {showForm && !result && (
            <div className="px-6 py-6">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Client email <span className="text-red-500">*</span></label>
                  <input
                    type="email"
                    value={email}
                    onChange={(e) => { setEmail(e.target.value); setProvisionError(''); }}
                    placeholder="client@theirhotel.com"
                    className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Client full name <span className="text-slate-400 text-xs">(optional)</span></label>
                  <input
                    type="text"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    placeholder="Jane Smith"
                    className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                </div>
              </div>

              <div className="mb-5">
                <label className="block text-sm font-medium text-slate-700 mb-2">Subscription plan</label>
                <div className="flex flex-wrap gap-2">
                  {PLANS.map((p) => (
                    <button
                      key={p.key}
                      onClick={() => setPlan(p.key)}
                      className={cn(
                        'px-4 py-2 rounded-xl text-sm font-medium border-2 transition-all',
                        plan === p.key
                          ? 'border-indigo-600 bg-indigo-50 text-indigo-700'
                          : 'border-slate-200 text-slate-600 hover:border-slate-300'
                      )}
                    >
                      {p.label}
                      <span className="ml-1.5 text-xs opacity-60">{p.duration}</span>
                    </button>
                  ))}
                </div>
              </div>

              {provisionError && (
                <div className="mb-4 rounded-lg bg-red-50 border border-red-200 px-4 py-2.5 text-sm text-red-700">
                  {provisionError}
                </div>
              )}

              <div className="flex items-center gap-3">
                <button
                  onClick={handleProvision}
                  disabled={isProvisioning}
                  className="flex items-center gap-2 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white text-sm font-semibold rounded-xl transition-colors"
                >
                  {isProvisioning ? (
                    <><svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg> Generating credentials…</>
                  ) : (
                    <><KeyRound className="h-4 w-4" /> Generate credentials</>
                  )}
                </button>
                <button onClick={resetForm} className="px-4 py-2.5 text-sm text-slate-500 hover:text-slate-700 transition-colors">
                  Cancel
                </button>
              </div>
            </div>
          )}

          {result && (
            <div className="px-6 py-6">
              <div className="flex items-start gap-3 mb-5">
                <div className="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center flex-shrink-0">
                  <Check className="h-4 w-4 text-green-600" />
                </div>
                <div>
                  <p className="font-semibold text-slate-900">Credentials generated for {result.fullName}</p>
                  <p className="text-sm text-slate-500">Plan: {result.planLabel}</p>
                </div>
                <button onClick={resetForm} className="ml-auto p-1 text-slate-400 hover:text-slate-600">
                  <X className="h-4 w-4" />
                </button>
              </div>

              <div className="bg-slate-900 rounded-2xl p-5 space-y-4 text-sm font-mono">
                <div className="flex items-center justify-between">
                  <span className="text-slate-400 font-sans font-medium text-xs uppercase tracking-wide">Email (Login)</span>
                  <div className="flex items-center gap-2">
                    <span className="text-green-400">{result.email}</span>
                    <CopyButton text={result.email} />
                  </div>
                </div>
                <div className="border-t border-slate-700" />
                <div className="flex items-center justify-between">
                  <span className="text-slate-400 font-sans font-medium text-xs uppercase tracking-wide">Temporary Password</span>
                  <div className="flex items-center gap-2">
                    <span className="text-amber-400 tracking-widest">{result.tempPassword}</span>
                    <CopyButton text={result.tempPassword} />
                  </div>
                </div>
                <div className="border-t border-slate-700" />
                <div className="flex items-center justify-between">
                  <span className="text-slate-400 font-sans font-medium text-xs uppercase tracking-wide">Activation Code</span>
                  <div className="flex items-center gap-2">
                    <span className="text-indigo-400 tracking-widest">{result.activationCode}</span>
                    <CopyButton text={result.activationCode} />
                  </div>
                </div>
              </div>

              <div className="mt-4 rounded-lg bg-amber-50 border border-amber-200 px-4 py-3 text-xs text-amber-700">
                <strong>Send these to the client.</strong> They will log in to HotelMS with the email + temporary password, then enter the activation code to provision their workspace. The password should be changed on first login.
              </div>

              <button
                onClick={resetForm}
                className="mt-4 flex items-center gap-2 text-sm text-slate-500 hover:text-indigo-600 transition-colors"
              >
                <Plus className="h-4 w-4" />
                Provision another tenant
              </button>
            </div>
          )}
        </div>

        {/* Tenants list */}
        <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
            <div>
              <h2 className="font-semibold text-slate-900">All Tenants</h2>
              <p className="text-xs text-slate-500 mt-0.5">{tenants.length} tenant{tenants.length !== 1 ? 's' : ''} onboarded</p>
            </div>
            <button onClick={fetchTenants} className="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors">
              <RefreshCw className="h-4 w-4" />
            </button>
          </div>

          {isLoadingTenants ? (
            <div className="px-6 py-12 text-center text-slate-400 text-sm">Loading tenants…</div>
          ) : tenants.length === 0 ? (
            <div className="px-6 py-12 text-center text-slate-400 text-sm">No tenants onboarded yet.</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-100">
                    <th className="text-left px-6 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide">Tenant</th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide">Admin email</th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide">Plan</th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide">Status</th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide">Expires</th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide">Onboarded</th>
                    <th className="px-4 py-3" />
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {tenants.map((t) => (
                    <tr key={t.id} className="hover:bg-slate-50 transition-colors">
                      <td className="px-6 py-3.5 font-medium text-slate-900">
                        {t.name || <span className="text-slate-400 italic">Pending activation</span>}
                      </td>
                      <td className="px-4 py-3.5 text-slate-600">{t.adminEmail}</td>
                      <td className="px-4 py-3.5 text-slate-600">{t.planLabel}</td>
                      <td className="px-4 py-3.5"><StatusBadge t={t} /></td>
                      <td className="px-4 py-3.5 text-slate-600">{t.isUnlimited ? '—' : formatDate(t.expiresAt)}</td>
                      <td className="px-4 py-3.5 text-slate-600">{formatDate(t.createdAt)}</td>
                      <td className="px-4 py-3.5">
                        <button
                          onClick={() => { setRenewTarget(t); setRenewPlan('Monthly3'); setRenewError(''); }}
                          className="flex items-center gap-1.5 text-xs text-slate-500 hover:text-indigo-600 hover:bg-indigo-50 px-2.5 py-1.5 rounded-lg transition-colors"
                        >
                          <RefreshCw className="h-3 w-3" />
                          Renew
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </main>

      {/* Renew modal */}
      {renewTarget && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 px-4">
          <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-md p-6">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h3 className="font-semibold text-slate-900">Renew Subscription</h3>
                <p className="text-sm text-slate-500 mt-0.5">{renewTarget.name || renewTarget.adminEmail}</p>
              </div>
              <button onClick={() => setRenewTarget(null)} className="p-1 text-slate-400 hover:text-slate-600">
                <X className="h-4 w-4" />
              </button>
            </div>

            <p className="text-sm text-slate-600 mb-3">Select the new plan for this tenant:</p>
            <div className="grid grid-cols-1 gap-2 mb-4">
              {PLANS.map((p) => (
                <button
                  key={p.key}
                  onClick={() => setRenewPlan(p.key)}
                  className={cn(
                    'flex items-center justify-between px-4 py-2.5 rounded-xl border-2 text-sm transition-all',
                    renewPlan === p.key
                      ? 'border-indigo-600 bg-indigo-50 text-indigo-700 font-semibold'
                      : 'border-slate-200 text-slate-600 hover:border-slate-300'
                  )}
                >
                  <span>{p.label}</span>
                  <span className="text-xs opacity-60">{p.duration}</span>
                </button>
              ))}
            </div>

            {renewError && <p className="text-sm text-red-600 mb-3">{renewError}</p>}
            <div className="flex gap-3">
              <button
                onClick={() => setRenewTarget(null)}
                className="flex-1 py-2.5 text-sm text-slate-600 border border-slate-300 rounded-xl hover:bg-slate-50 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleRenew}
                disabled={isRenewing}
                className="flex-1 py-2.5 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 rounded-xl transition-colors"
              >
                {isRenewing ? 'Renewing…' : 'Renew Subscription'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
