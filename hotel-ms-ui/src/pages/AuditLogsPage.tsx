import { useState, useEffect, useCallback } from 'react';
import { Search, X, RefreshCw } from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Table } from '../components/ui/Table';
import { useToast } from '../lib/store';
import { auditLogService } from '../services/auditLog.service';
import type { AuditLog } from '../types';
import { formatDate } from '../lib/utils';

export function AuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const toast = useToast();

  const fetchLogs = useCallback(async () => {
    setIsLoading(true);
    try {
      const result = await auditLogService.getAll();
      setLogs(result);
    } catch {
      toast.error('Failed to load audit logs');
    } finally {
      setIsLoading(false);
    }
  }, [toast]);

  useEffect(() => { fetchLogs(); }, [fetchLogs]);

  const filtered = logs.filter((log) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (
      log.action.toLowerCase().includes(q) ||
      log.performedBy.toLowerCase().includes(q) ||
      log.performerEmail.toLowerCase().includes(q) ||
      (log.performedAgainst ?? '').toLowerCase().includes(q)
    );
  });

  const columns = [
    {
      key: 'action',
      header: 'Action',
      render: (log: AuditLog) => (
        <span className="font-mono text-xs bg-slate-100 dark:bg-slate-700 px-2 py-1 rounded-md">
          {log.action}
        </span>
      ),
    },
    {
      key: 'performedBy',
      header: 'Performed By',
      render: (log: AuditLog) => (
        <div>
          <p className="font-medium text-slate-800 dark:text-slate-200">{log.performedBy}</p>
          <p className="text-xs text-slate-500 dark:text-slate-400">{log.performerEmail}</p>
        </div>
      ),
    },
    {
      key: 'performedAgainst',
      header: 'Target',
      render: (log: AuditLog) => (
        <span className="text-sm text-slate-600 dark:text-slate-400">{log.performedAgainst ?? '—'}</span>
      ),
    },
    {
      key: 'ipAddress',
      header: 'IP Address',
      render: (log: AuditLog) => (
        <span className="font-mono text-xs text-slate-600 dark:text-slate-400">{log.ipAddress ?? '—'}</span>
      ),
    },
    {
      key: 'datePerformed',
      header: 'Date',
      render: (log: AuditLog) => formatDate(log.datePerformed),
    },
  ];

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Audit Logs</h2>
          <p className="page-subtitle">{logs.length} log entr{logs.length !== 1 ? 'ies' : 'y'}</p>
        </div>
        <Button variant="outline" leftIcon={<RefreshCw className="h-4 w-4" />} onClick={fetchLogs}>
          Refresh
        </Button>
      </div>

      <Card className="mb-6" padding="sm">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search by action, user, or target..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full h-10 pl-10 pr-10 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400"
          />
          {search && (
            <button onClick={() => setSearch('')} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600">
              <X className="h-4 w-4" />
            </button>
          )}
        </div>
      </Card>

      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No audit logs found"
          emptyDescription="System actions and changes will appear here"
        />
      </Card>
    </div>
  );
}

export default AuditLogsPage;
