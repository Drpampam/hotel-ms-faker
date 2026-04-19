import { useState, useEffect, useCallback } from 'react';
import { Copy, Check, KeyRound, RefreshCw } from 'lucide-react';
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

export default function TenantsPage() {
  const [tenants, setTenants] = useState<TenantSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
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
      render: (t) => <span className="font-medium text-slate-900 dark:text-slate-100">{t.name}</span>,
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
        />
      </Card>

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
