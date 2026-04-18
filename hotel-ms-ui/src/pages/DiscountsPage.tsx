import { useState, useEffect, useCallback } from 'react';
import { Plus, Search, X, Pencil, Trash2, ToggleLeft, ToggleRight } from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { useToast } from '../lib/store';
import { discountService } from '../services/discount.service';
import type { Discount, CreateDiscountRequest } from '../types';
import { formatDate, formatCurrency } from '../lib/utils';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const discountSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  percentage: z.coerce.number().min(0).max(100).optional(),
  fixedAmount: z.coerce.number().min(0).optional(),
  isActive: z.boolean(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
});

type DiscountForm = z.infer<typeof discountSchema>;

export function DiscountsPage() {
  const [discounts, setDiscounts] = useState<Discount[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editing, setEditing] = useState<Discount | null>(null);
  const [isBusy, setIsBusy] = useState(false);
  const toast = useToast();

  const { register, handleSubmit, reset, formState: { errors } } = useForm<DiscountForm>({
    resolver: zodResolver(discountSchema),
    defaultValues: { isActive: true },
  });

  const fetchDiscounts = useCallback(async () => {
    setIsLoading(true);
    try {
      const result = await discountService.getAll();
      setDiscounts(result);
    } catch {
      toast.error('Failed to load discounts');
    } finally {
      setIsLoading(false);
    }
  }, [toast]);

  useEffect(() => { fetchDiscounts(); }, [fetchDiscounts]);

  const openCreate = () => {
    setEditing(null);
    reset({ isActive: true });
    setIsModalOpen(true);
  };

  const openEdit = (d: Discount) => {
    setEditing(d);
    reset({
      name: d.name,
      description: d.description ?? '',
      percentage: d.percentage,
      fixedAmount: d.fixedAmount,
      isActive: d.isActive,
      startDate: d.startDate ? d.startDate.slice(0, 10) : '',
      endDate: d.endDate ? d.endDate.slice(0, 10) : '',
    });
    setIsModalOpen(true);
  };

  const onSubmit = async (data: DiscountForm) => {
    setIsBusy(true);
    try {
      const payload: CreateDiscountRequest = {
        name: data.name,
        description: data.description || undefined,
        percentage: data.percentage,
        fixedAmount: data.fixedAmount,
        isActive: data.isActive,
        startDate: data.startDate || undefined,
        endDate: data.endDate || undefined,
      };
      if (editing) {
        await discountService.update(editing.id, payload);
        toast.success('Discount updated', data.name);
      } else {
        await discountService.create(payload);
        toast.success('Discount created', data.name);
      }
      setIsModalOpen(false);
      fetchDiscounts();
    } catch (err) {
      toast.error('Failed', err instanceof Error ? err.message : 'Operation failed');
    } finally {
      setIsBusy(false);
    }
  };

  const handleToggle = async (d: Discount) => {
    try {
      await discountService.update(d.id, { ...d, isActive: !d.isActive });
      setDiscounts((prev) => prev.map((x) => x.id === d.id ? { ...x, isActive: !x.isActive } : x));
      toast.success(d.isActive ? 'Discount deactivated' : 'Discount activated', d.name);
    } catch {
      toast.error('Failed to update discount');
    }
  };

  const handleDelete = async (d: Discount) => {
    if (!confirm(`Delete discount "${d.name}"? This cannot be undone.`)) return;
    try {
      await discountService.delete(d.id);
      setDiscounts((prev) => prev.filter((x) => x.id !== d.id));
      toast.success('Discount deleted', d.name);
    } catch {
      toast.error('Failed to delete discount');
    }
  };

  const filtered = discounts.filter((d) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return d.name.toLowerCase().includes(q) || (d.description ?? '').toLowerCase().includes(q);
  });

  const columns = [
    {
      key: 'name',
      header: 'Name',
      render: (d: Discount) => (
        <div>
          <p className="font-medium text-slate-800 dark:text-slate-200">{d.name}</p>
          {d.description && <p className="text-xs text-slate-500 dark:text-slate-400">{d.description}</p>}
        </div>
      ),
    },
    {
      key: 'value',
      header: 'Value',
      render: (d: Discount) => (
        <span className="font-semibold text-slate-900 dark:text-slate-100">
          {d.percentage ? `${d.percentage}%` : d.fixedAmount ? formatCurrency(d.fixedAmount) : '—'}
        </span>
      ),
    },
    {
      key: 'validity',
      header: 'Validity',
      render: (d: Discount) => (
        <span className="text-sm text-slate-600 dark:text-slate-400">
          {d.startDate ? formatDate(d.startDate) : '—'} → {d.endDate ? formatDate(d.endDate) : 'No end'}
        </span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (d: Discount) => (
        <Badge variant={d.isActive ? 'success' : 'default'} dot>
          {d.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (d: Discount) => (
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); handleToggle(d); }}>
            {d.isActive ? <ToggleRight className="h-4 w-4 text-emerald-500" /> : <ToggleLeft className="h-4 w-4 text-slate-400" />}
          </Button>
          <Button variant="ghost" size="sm" leftIcon={<Pencil className="h-3.5 w-3.5" />}
            onClick={(e) => { e.stopPropagation(); openEdit(d); }}>Edit</Button>
          <Button variant="ghost" size="sm" leftIcon={<Trash2 className="h-3.5 w-3.5 text-red-500" />}
            onClick={(e) => { e.stopPropagation(); handleDelete(d); }}
            className="text-red-500 hover:text-red-600">Delete</Button>
        </div>
      ),
    },
  ];

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Discounts</h2>
          <p className="page-subtitle">{discounts.length} discount{discounts.length !== 1 ? 's' : ''}</p>
        </div>
        <Button leftIcon={<Plus className="h-4 w-4" />} onClick={openCreate}>
          Add Discount
        </Button>
      </div>

      <Card className="mb-6" padding="sm">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search discounts..."
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
          emptyMessage="No discounts found"
          emptyDescription="Create discount codes to offer promotions to guests"
        />
      </Card>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editing ? 'Edit Discount' : 'New Discount'}
        size="md"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>Cancel</Button>
            <Button isLoading={isBusy} onClick={handleSubmit(onSubmit)}>
              {editing ? 'Save Changes' : 'Create Discount'}
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Name *</label>
            <input {...register('name')} className="w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400" placeholder="e.g. Summer Sale" />
            {errors.name && <p className="text-xs text-red-500 mt-1">{errors.name.message}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Description</label>
            <input {...register('description')} className="w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400" placeholder="Optional description" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Percentage (%)</label>
              <input {...register('percentage')} type="number" min="0" max="100" step="0.01" className="w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400" placeholder="0" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Fixed Amount</label>
              <input {...register('fixedAmount')} type="number" min="0" step="0.01" className="w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400" placeholder="0.00" />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Start Date</label>
              <input {...register('startDate')} type="date" className="w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">End Date</label>
              <input {...register('endDate')} type="date" className="w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400" />
            </div>
          </div>
          <div className="flex items-center gap-3">
            <input {...register('isActive')} type="checkbox" id="isActive" className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500" />
            <label htmlFor="isActive" className="text-sm font-medium text-slate-700 dark:text-slate-300">Active</label>
          </div>
        </div>
      </Modal>
    </div>
  );
}

export default DiscountsPage;
