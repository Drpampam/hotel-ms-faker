import { useEffect, useState } from 'react';
import { AlertTriangle, XCircle, RefreshCw } from 'lucide-react';
import { useAuthStore } from '../../lib/store';
import activationService, { type SubscriptionStatus } from '../../services/activation.service';

export function SubscriptionBanner() {
  const { tenantId } = useAuthStore();
  const [status, setStatus] = useState<SubscriptionStatus | null>(null);

  useEffect(() => {
    if (!tenantId || tenantId <= 0) return;
    activationService.getStatus(tenantId).then(setStatus).catch(() => null);
  }, [tenantId]);

  if (!status || status.isUnlimited || (status.daysRemaining !== null && status.daysRemaining > 30)) return null;

  if (status.isExpired) {
    return (
      <div className="bg-red-600 text-white px-4 py-2 flex items-center gap-3 text-sm font-medium">
        <XCircle className="w-4 h-4 shrink-0" />
        <span>Your subscription has expired. Contact your provider to renew.</span>
      </div>
    );
  }

  return (
    <div className="bg-amber-500 text-white px-4 py-2 flex items-center gap-3 text-sm font-medium">
      <AlertTriangle className="w-4 h-4 shrink-0" />
      <span>
        Your <strong>{status.planLabel}</strong> subscription expires in{' '}
        <strong>{status.daysRemaining} day{status.daysRemaining !== 1 ? 's' : ''}</strong>.
      </span>
      <a
        href="/settings"
        className="ml-auto flex items-center gap-1 underline underline-offset-2 hover:opacity-80"
      >
        <RefreshCw className="w-3 h-3" /> Renew
      </a>
    </div>
  );
}
