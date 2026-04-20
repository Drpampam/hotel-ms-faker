import { useState, useEffect, useCallback } from 'react';
import { Key, CheckCircle, Clock, AlertTriangle, RefreshCw, Zap } from 'lucide-react';
import { useAuthStore, useToast } from '../lib/store';
import activationService, { type SubscriptionStatus } from '../services/activation.service';

const PLAN_LABELS: Record<string, { label: string; color: string }> = {
  Trial:     { label: 'Trial (30 days)',    color: 'text-amber-400' },
  Monthly3:  { label: '3-Month Plan',       color: 'text-indigo-400' },
  Monthly6:  { label: '6-Month Plan',       color: 'text-violet-400' },
  FiveYear:  { label: '5-Year Plan',        color: 'text-emerald-400' },
  Unlimited: { label: 'Unlimited',          color: 'text-cyan-400' },
};

export default function SubscriptionPage() {
  const { tenantId } = useAuthStore();
  const toast = useToast();

  const [status, setStatus] = useState<SubscriptionStatus | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [code, setCode] = useState('');
  const [isActivating, setIsActivating] = useState(false);
  const [activateError, setActivateError] = useState<string | null>(null);
  const [activated, setActivated] = useState(false);

  const loadStatus = useCallback(async () => {
    if (!tenantId) return;
    try {
      const s = await activationService.getStatus(tenantId);
      setStatus(s);
    } catch {
      // non-fatal
    } finally {
      setIsLoading(false);
    }
  }, [tenantId]);

  useEffect(() => { loadStatus(); }, [loadStatus]);

  const handleActivate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!code.trim() || !tenantId) return;
    setActivateError(null);
    setIsActivating(true);
    try {
      await activationService.renew(tenantId, code.trim());
      toast.success('Plan activated!', 'Your subscription has been upgraded successfully.');
      setActivated(true);
      setCode('');
      await loadStatus();
    } catch (err: unknown) {
      const msg = (err as Error)?.message || 'Invalid or expired activation code.';
      setActivateError(msg);
    } finally {
      setIsActivating(false);
    }
  };

  const planInfo = status?.planType ? PLAN_LABELS[status.planType] ?? { label: status.planLabel ?? status.planType, color: 'text-slate-300' } : null;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Subscription</h1>
        <p className="text-slate-500 dark:text-slate-400 text-sm mt-1">
          View your current plan and activate an upgrade code
        </p>
      </div>

      {/* Current status card */}
      <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
        <h2 className="text-sm font-semibold text-slate-600 dark:text-slate-400 uppercase tracking-wider mb-4">
          Current Plan
        </h2>

        {isLoading ? (
          <div className="flex items-center gap-3 text-slate-400">
            <RefreshCw className="h-4 w-4 animate-spin" />
            <span className="text-sm">Loading subscription info…</span>
          </div>
        ) : status ? (
          <div className="flex flex-col sm:flex-row sm:items-center gap-4">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-indigo-50 dark:bg-indigo-900/30 rounded-xl">
                <Zap className="h-5 w-5 text-indigo-600 dark:text-indigo-400" />
              </div>
              <div>
                <div className={`text-lg font-bold ${planInfo?.color ?? 'text-slate-900 dark:text-white'}`}>
                  {planInfo?.label ?? status.planLabel}
                </div>
                <div className="flex items-center gap-2 mt-0.5">
                  {status.isExpired ? (
                    <span className="flex items-center gap-1 text-xs text-red-500">
                      <AlertTriangle className="h-3 w-3" /> Expired
                    </span>
                  ) : status.isUnlimited ? (
                    <span className="flex items-center gap-1 text-xs text-emerald-500">
                      <CheckCircle className="h-3 w-3" /> Active · Unlimited
                    </span>
                  ) : (
                    <span className="flex items-center gap-1 text-xs text-slate-500 dark:text-slate-400">
                      <Clock className="h-3 w-3" />
                      {status.daysRemaining != null
                        ? `${status.daysRemaining} day${status.daysRemaining !== 1 ? 's' : ''} remaining`
                        : 'No expiry info'}
                    </span>
                  )}
                </div>
              </div>
            </div>

            {status.expiresAt && !status.isUnlimited && (
              <div className="sm:ml-auto text-sm text-slate-500 dark:text-slate-400">
                Expires: {new Date(status.expiresAt).toLocaleDateString()}
              </div>
            )}
          </div>
        ) : (
          <p className="text-sm text-slate-400">Unable to load subscription status.</p>
        )}
      </div>

      {/* Activation code entry */}
      {!status?.isUnlimited && (
        <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
          <h2 className="text-sm font-semibold text-slate-600 dark:text-slate-400 uppercase tracking-wider mb-1">
            Activate Plan
          </h2>
          <p className="text-sm text-slate-500 dark:text-slate-400 mb-5">
            Enter the activation code provided by your account manager to upgrade or renew your subscription.
          </p>

          {activated && (
            <div className="mb-4 p-3 bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800 rounded-xl flex items-center gap-2 text-emerald-700 dark:text-emerald-400 text-sm">
              <CheckCircle className="h-4 w-4 flex-shrink-0" />
              Plan activated successfully!
            </div>
          )}

          {activateError && (
            <div className="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl flex items-center gap-2 text-red-700 dark:text-red-400 text-sm">
              <AlertTriangle className="h-4 w-4 flex-shrink-0" />
              {activateError}
            </div>
          )}

          <form onSubmit={handleActivate} className="flex gap-3">
            <div className="relative flex-1">
              <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
                <Key className="h-4 w-4" />
              </div>
              <input
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder="XXXX-XXXX-XXXX-XXXX"
                required
                className="w-full pl-9 pr-4 py-2.5 bg-slate-50 dark:bg-slate-700 border border-slate-200 dark:border-slate-600 rounded-xl text-slate-900 dark:text-white placeholder-slate-400 text-sm font-mono focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500/50"
              />
            </div>
            <button
              type="submit"
              disabled={isActivating || !code.trim()}
              className="px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white font-semibold rounded-xl transition-colors text-sm whitespace-nowrap"
            >
              {isActivating ? 'Activating…' : 'Activate'}
            </button>
          </form>

          <p className="mt-3 text-xs text-slate-400 dark:text-slate-500">
            Your account manager provides activation codes when you purchase or renew a subscription plan.
          </p>
        </div>
      )}
    </div>
  );
}
