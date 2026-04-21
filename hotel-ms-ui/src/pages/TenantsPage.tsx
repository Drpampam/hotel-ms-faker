import { useState, useEffect, useCallback } from 'react';
import { Copy, Check, KeyRound, RefreshCw, X, Building2, Mail, Calendar, Clock, ShieldCheck, Zap } from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Table, type Column } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { useToast } from '../lib/store';
import activationService, { type TenantSummary, type GenerateCodeResponse } from '../services/activation.service';
import { formatDate } from '../lib/utils';

const PLAN_OPTIONS = [
  { value: 'Trial',     label: '30-Day Trial' },
  { value: 'Monthly3',  label: '3-Month Plan' },
  { value: 'Monthly6',  label: '6-Month Plan' },
  { value: 'FiveYear',  label: '5-Year Plan' },
  { value: 'Unlimited', label: 'Unlimited' },
] as const;

type PlanValue = typeof PLAN_OPTIONS[number]['value'];

// ── Tenant detail slide-over ──────────────────────────────────────────────────
function TenantDetailPanel({ tenant, onClose, onRenew }: {
  tenant: TenantSummary;
  onClose: () => void;
  onRenew: (t: TenantSummary) => void;
}) {
  const planColor =
    tenant.isUnlimited ? 'text-cyan-600 dark:text-cyan-400' :
    tenant.isExpired   ? 'text-red-600 dark:text-red-400' :
    (tenant.daysRemaining ?? 999) <= 14 ? 'text-amber-600 dark:text-amber-400' :
    'text-emerald-600 dark:text-emerald-400';

  return (
    <div className="fixed inset-0 z-40 flex justify-end">
      {/* backdrop */}
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />

      {/* panel */}
      <div className="relative w-full max-w-md bg-white dark:bg-slate-900 shadow-2xl flex flex-col h-full animate-slide-right">
        {/* header */}
        <div className="flex items-start justify-between p-6 border-b border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 bg-indigo-100 dark:bg-indigo-900/40 rounded-xl">
              <Building2 className="h-5 w-5 text-indigo-600 dark:text-indigo-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{tenant.name}</h2>
              <p className="text-xs text-slate-500 dark:text-slate-400">Tenant ID: #{tenant.id}</p>
            </div>
          </div>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 transition-colors">
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* body */}
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          {/* Status banner */}
          <div className={`rounded-xl p-4 border ${
            tenant.isExpired
              ? 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800'
              : (tenant.daysRemaining ?? 999) <= 14 && !tenant.isUnlimited
              ? 'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800'
              : 'bg-emerald-50 dark:bg-emerald-900/20 border-emerald-200 dark:border-emerald-800'
          }`}>
            <div className="flex items-center gap-2 mb-1">
              <ShieldCheck className={`h-4 w-4 ${planColor}`} />
              <span className={`text-sm font-semibold ${planColor}`}>{tenant.planLabel}</span>
            </div>
            <p className="text-xs text-slate-500 dark:text-slate-400">
              {tenant.isUnlimited
                ? 'Unlimited access — no expiry'
                : tenant.isExpired
                ? 'Subscription has expired'
                : `${tenant.daysRemaining} day${tenant.daysRemaining !== 1 ? 's' : ''} remaining`}
            </p>
          </div>

          {/* Details grid */}
          <div className="space-y-4">
            <DetailRow icon={<Mail className="h-4 w-4" />} label="Admin Email" value={tenant.adminEmail} />
            <DetailRow icon={<Zap className="h-4 w-4" />}  label="Plan"        value={tenant.planLabel} />
            <DetailRow
              icon={<Calendar className="h-4 w-4" />}
              label="Onboarded"
              value={formatDate(tenant.createdAt)}
            />
            {!tenant.isUnlimited && (
              <DetailRow
                icon={<Clock className="h-4 w-4" />}
                label="Expires"
                value={tenant.expiresAt ? formatDate(tenant.expiresAt) : '—'}
              />
            )}
            <div className="flex items-center justify-between py-3 border-t border-slate-100 dark:border-slate-800">
              <span className="text-sm text-slate-500 dark:text-slate-400">Account Status</span>
              {tenant.isUnlimited
                ? <Badge variant="success">Unlimited</Badge>
                : tenant.isExpired
                ? <Badge variant="danger">Expired</Badge>
                : (tenant.daysRemaining ?? 999) <= 14
                ? <Badge variant="warning">{tenant.daysRemaining}d left</Badge>
                : <Badge variant="success">Active</Badge>
              }
            </div>
          </div>
        </div>

        {/* footer actions */}
        <div className="p-6 border-t border-slate-200 dark:border-slate-700 flex gap-3">
          <Button variant="secondary" className="flex-1" onClick={onClose}>Close</Button>
          <Button className="flex-1" onClick={() => { onClose(); onRenew(tenant); }}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Renew Plan
          </Button>
        </div>
      </div>
    </div>
  );
}

function DetailRow({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
  return (
    <div className="flex items-center justify-between py-3 border-t border-slate-100 dark:border-slate-800 first:border-0 first:pt-0">
      <div className="flex items-center gap-2 text-slate-500 dark:text-slate-400">
        {icon}
        <span className="text-sm">{label}</span>
      </div>
      <span className="text-sm font-medium text-slate-900 dark:text-white">{value}</span>
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────
export default function TenantsPage() {
  const [tenants, setTenants] = useState<TenantSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedTenant, setSelectedTenant] = useState<TenantSummary | null>(null);
  const [isGenerateOpen, setIsGenerateOpen] = useState(false);
  const [isRenewOpen, setIsRenewOpen] = useState(false);
  const [renewTarget, setRenewTarget] = useState<TenantSummary | null>(null);
  const [email, setEmail] = useState('');
  const [planType, setPlanType] = useState<PlanValue>('Monthly3');
  const [renewCode, setRenewCode] = useState('');
  const [isBusy, setIsBusy] = useState(false);
  const [generated, setGenerated] = useState<GenerateCodeResponse | null>(null);
  const [copied, setCopied] = useState(false);
  const toast = useToast();

  const fetchTenants = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await activationService.getAllTenants();
      setTenants(data);
    } catch {
      toast.error('Failed to load tenants');
    } finally {
      setIsLoading(false);
    }
  }, [toast]);

  useEffect(() => { fetchTenants(); }, [fetchTenants]);

  const handleGenerate = async () => {
    if (!email.trim()) { toast.error('Email is required'); return; }
    setIsBusy(true);
    try {
      const result = await activationService.generateCode({ email: email.trim(), planType });
      setGenerated(result);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to generate code');
    } finally {
      setIsBusy(false);
    }
  };

  const handleCopy = () => {
    if (!generated) return;
    navigator.clipboard.writeText(generated.plaintextCode);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const closeGenerate = () => {
    setIsGenerateOpen(false);
    setGenerated(null);
    setEmail('');
    setPlanType('Monthly3');
  };

  const openRenew = (tenant: TenantSummary) => {
    setRenewTarget(tenant);
    setRenewCode('');
    setIsRenewOpen(true);
  };

  const openDetail = (tenant: TenantSummary) => setSelectedTenant(tenant);

  const handleRenew = async () => {
    if (!renewTarget || !renewCode.trim()) { toast.error('Activation code is required'); return; }
    setIsBusy(true);
    try {
      await activationService.renew(renewTarget.id, renewCode.trim());
      toast.success(`Subscription renewed for ${renewTarget.name}`);
      setIsRenewOpen(false);
      fetchTenants();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Renewal failed');
    } finally {
      setIsBusy(false);
    }
  };

  const statusBadge = (t: TenantSummary) => {
    if (t.isUnlimited) return <Badge variant="success">Unlimited</Badge>;
    if (t.isExpired)   return <Badge variant="danger">Expired</Badge>;
    if (t.daysRemaining !== null && t.daysRemaining <= 14)
      return <Badge variant="warning">{t.daysRemaining}d left</Badge>;
    return <Badge variant="success">Active</Badge>;
  };

  const columns: Column<TenantSummary>[] = [
    {
      key: 'name',
      header: 'Hotel / Tenant',
      render: (t) => (
        <span className="font-medium text-slate-900 dark:text-slate-100 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">
          {t.name}
        </span>
      ),
    },
    {
      key: 'adminEmail',
      header: 'Admin Email',
      render: (t) => <span className="text-slate-600 dark:text-slate-400">{t.adminEmail}</span>,
    },
    { key: 'planLabel', header: 'Plan', render: (t) => t.planLabel },
    { key: 'status',    header: 'Status',   render: statusBadge },
    {
      key: 'expiresAt',
      header: 'Expires',
      render: (t) => t.isUnlimited ? '—' : t.expiresAt ? formatDate(t.expiresAt) : '—',
    },
    { key: 'createdAt', header: 'Onboarded', render: (t) => formatDate(t.createdAt) },
    {
      key: 'actions',
      header: '',
      render: (t) => (
        <Button variant="ghost" size="sm" onClick={() => openRenew(t)}>
          <RefreshCw className="h-3.5 w-3.5 mr-1" />
          Renew
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">Tenants</h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">
            Generate activation codes and manage all onboarded hotels.
          </p>
        </div>
        <Button onClick={() => setIsGenerateOpen(true)}>
          <KeyRound className="h-4 w-4 mr-2" />
          Generate Activation Code
        </Button>
      </div>

      <Card>
        <Table<TenantSummary>
          data={tenants}
          columns={columns}
          keyField="id"
          isLoading={isLoading}
          emptyMessage="No tenants onboarded yet."
          onRowClick={openDetail}
        />
      </Card>

      {selectedTenant && (
        <TenantDetailPanel
          tenant={selectedTenant}
          onClose={() => setSelectedTenant(null)}
          onRenew={openRenew}
        />
      )}

      {/* Generate code modal */}
      <Modal isOpen={isGenerateOpen} onClose={closeGenerate} title="Generate Activation Code">
        {generated ? (
          <div className="space-y-5">
            <p className="text-sm text-slate-600 dark:text-slate-400">
              Activation code generated for <strong>{generated.boundToEmail}</strong> ({generated.planLabel}).
              Copy it and send it to the tenant — it is only shown once.
            </p>
            <div className="flex items-center gap-2">
              <code className="flex-1 bg-slate-100 dark:bg-slate-800 text-slate-900 dark:text-slate-100 text-base font-mono tracking-widest px-4 py-3 rounded-lg border border-slate-200 dark:border-slate-700 text-center">
                {generated.plaintextCode}
              </code>
              <Button variant="secondary" size="sm" onClick={handleCopy}>
                {copied ? <Check className="h-4 w-4 text-green-500" /> : <Copy className="h-4 w-4" />}
              </Button>
            </div>
            <div className="flex justify-end">
              <Button onClick={closeGenerate}>Done</Button>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                Tenant Admin Email
              </label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="owner@theirhotel.com"
                className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
              <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
                The code is bound to this email — the tenant must use the same email when activating.
              </p>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                Plan
              </label>
              <select
                value={planType}
                onChange={(e) => setPlanType(e.target.value as PlanValue)}
                className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              >
                {PLAN_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>{o.label}</option>
                ))}
              </select>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="secondary" onClick={closeGenerate}>Cancel</Button>
              <Button onClick={handleGenerate} disabled={isBusy}>
                {isBusy ? 'Generating…' : 'Generate Code'}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Renew subscription modal */}
      <Modal isOpen={isRenewOpen} onClose={() => setIsRenewOpen(false)} title={`Renew — ${renewTarget?.name ?? ''}`}>
        <div className="space-y-4">
          <p className="text-sm text-slate-600 dark:text-slate-400">
            Enter a new activation code to extend this tenant's subscription. The code must be bound to{' '}
            <strong>{renewTarget?.adminEmail}</strong>.
          </p>
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
              Activation Code
            </label>
            <input
              type="text"
              value={renewCode}
              onChange={(e) => setRenewCode(e.target.value)}
              placeholder="XXXX-XXXX-XXXX-XXXX"
              className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm font-mono tracking-widest text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setIsRenewOpen(false)}>Cancel</Button>
            <Button onClick={handleRenew} disabled={isBusy}>
              {isBusy ? 'Renewing…' : 'Renew Subscription'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
