import { useState, useEffect, useCallback } from 'react';
import { Building2, MapPin, Plus, Edit, RefreshCw, Star } from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Modal } from '../components/ui/Modal';
import { Input } from '../components/ui/Input';
import { useToast, useAuthStore } from '../lib/store';
import { propertyService } from '../services/property.service';
import type { Property } from '../types';

function StarRating({ count }: { count?: number }) {
  const n = count ?? 3;
  return (
    <div className="flex items-center gap-0.5">
      {Array.from({ length: 5 }).map((_, i) => (
        <Star key={i} className={`h-4 w-4 ${i < n ? 'text-amber-400 fill-amber-400' : 'text-slate-300 dark:text-slate-600'}`} />
      ))}
    </div>
  );
}

function PropertyCard({ property, onEdit }: { property: Property; onEdit: (p: Property) => void }) {
  return (
    <Card className="overflow-hidden" padding="none">
      <div className="h-32 bg-gradient-to-br from-indigo-500 via-indigo-600 to-violet-600 relative overflow-hidden">
        <div className="absolute inset-0 opacity-20">
          <div className="absolute top-4 right-4 w-24 h-24 border-2 border-white rounded-full" />
          <div className="absolute bottom-0 left-0 w-32 h-32 border-2 border-white rounded-full -translate-x-1/2 translate-y-1/2" />
        </div>
        {property.image && (
          <img src={property.image} alt={property.name} className="absolute inset-0 w-full h-full object-cover opacity-30" />
        )}
        <div className="absolute bottom-4 left-5 right-5 flex items-end justify-between">
          <div className="w-12 h-12 bg-white/20 backdrop-blur-sm rounded-xl flex items-center justify-center">
            <Building2 className="h-6 w-6 text-white" />
          </div>
          <Badge className="bg-white/20 text-white border border-white/30">Active</Badge>
        </div>
      </div>

      <div className="p-5">
        <div className="flex items-start justify-between mb-3">
          <div>
            <h3 className="font-bold text-slate-900 dark:text-slate-100 text-lg leading-tight">{property.name}</h3>
            <StarRating />
          </div>
          <Button variant="ghost" size="icon" onClick={() => onEdit(property)}>
            <Edit className="h-4 w-4" />
          </Button>
        </div>

        {property.description && (
          <p className="text-sm text-slate-500 dark:text-slate-400 mb-3 line-clamp-2">{property.description}</p>
        )}

        <div className="space-y-2 text-sm">
          {(property.city || property.country) && (
            <div className="flex items-start gap-2 text-slate-600 dark:text-slate-400">
              <MapPin className="h-4 w-4 flex-shrink-0 mt-0.5" />
              <span>{[property.city, property.country].filter(Boolean).join(', ')}</span>
            </div>
          )}
        </div>

        <div className="mt-4 pt-4 border-t border-slate-100 dark:border-slate-700 flex items-center justify-between">
          <span className="text-xs text-slate-400">ID: {property.id}</span>
          <Badge variant="success" dot>Active</Badge>
        </div>
      </div>
    </Card>
  );
}

interface AddPropertyForm {
  name: string;
  description: string;
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

const EMPTY_FORM: AddPropertyForm = { name: '', description: '', street: '', city: '', state: '', country: '', zipCode: '' };

export function PropertiesPage() {
  const [properties, setProperties] = useState<Property[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [editProperty, setEditProperty] = useState<Property | null>(null);
  const [form, setForm] = useState<AddPropertyForm>(EMPTY_FORM);
  const toast = useToast();
  const { tenantId } = useAuthStore();

  const loadProperties = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await propertyService.getAll({ tenantId: tenantId ?? undefined });
      setProperties(data);
    } catch {
      toast.error('Failed to load', 'Could not fetch properties');
    } finally {
      setIsLoading(false);
    }
  }, [tenantId, toast]);

  useEffect(() => { loadProperties(); }, [loadProperties]);

  const handleAdd = async () => {
    if (!form.name.trim()) { toast.error('Validation', 'Property name is required'); return; }
    setIsSubmitting(true);
    try {
      const created = await propertyService.create({
        name: form.name,
        description: form.description,
        image: 'https://placehold.co/600x400',
        tenantId: tenantId ?? 1,
        address: { street: form.street, city: form.city, state: form.state, country: form.country, zipCode: form.zipCode },
      });
      setProperties((prev) => [created, ...prev]);
      toast.success('Property added', `${form.name} has been created`);
      setIsAddOpen(false);
      setForm(EMPTY_FORM);
    } catch {
      toast.error('Failed to create', 'Could not add property');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Properties</h2>
          <p className="page-subtitle">{properties.length} propert{properties.length === 1 ? 'y' : 'ies'} managed</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" leftIcon={<RefreshCw className="h-4 w-4" />} onClick={loadProperties} isLoading={isLoading}>
            Refresh
          </Button>
          <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsAddOpen(true)}>
            Add Property
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-8">
        {[
          { label: 'Total Properties', value: properties.length, icon: <Building2 className="h-5 w-5 text-indigo-600" />, bg: 'bg-indigo-50 dark:bg-indigo-900/20' },
          { label: 'Active Properties', value: properties.length, icon: <Star className="h-5 w-5 text-amber-500" />, bg: 'bg-amber-50 dark:bg-amber-900/20' },
        ].map((s) => (
          <div key={s.label} className="flex items-center gap-4 p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-100 dark:border-slate-700 shadow-sm">
            <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${s.bg}`}>{s.icon}</div>
            <div>
              <p className="text-xs text-slate-500 dark:text-slate-400">{s.label}</p>
              {isLoading ? <div className="h-7 w-8 bg-slate-200 dark:bg-slate-700 rounded animate-pulse mt-0.5" /> : (
                <p className="text-2xl font-bold text-slate-900 dark:text-slate-100">{s.value}</p>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Grid */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-64 bg-slate-200 dark:bg-slate-700 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : properties.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-slate-400">
          <Building2 className="h-12 w-12 mb-4 opacity-40" />
          <p className="text-lg font-medium">No properties yet</p>
          <p className="text-sm mt-1">Click "Add Property" to create your first property</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
          {properties.map((p) => (
            <PropertyCard key={p.id} property={p} onEdit={setEditProperty} />
          ))}
        </div>
      )}

      {/* Add Modal */}
      <Modal
        isOpen={isAddOpen}
        onClose={() => { setIsAddOpen(false); setForm(EMPTY_FORM); }}
        title="Add New Property"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => { setIsAddOpen(false); setForm(EMPTY_FORM); }}>Cancel</Button>
            <Button onClick={handleAdd} isLoading={isSubmitting}>Save Property</Button>
          </>
        }
      >
        <div className="space-y-4">
          <Input label="Property Name" required placeholder="e.g. Grand Hotel Downtown" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
          <Input label="Description" placeholder="Brief description" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          <div className="grid grid-cols-2 gap-4">
            <Input label="Street" placeholder="123 Main Street" value={form.street} onChange={(e) => setForm((f) => ({ ...f, street: e.target.value }))} />
            <Input label="City" placeholder="City" value={form.city} onChange={(e) => setForm((f) => ({ ...f, city: e.target.value }))} />
            <Input label="State" placeholder="State" value={form.state} onChange={(e) => setForm((f) => ({ ...f, state: e.target.value }))} />
            <Input label="Country" placeholder="Country" value={form.country} onChange={(e) => setForm((f) => ({ ...f, country: e.target.value }))} />
            <Input label="Zip Code" placeholder="100001" value={form.zipCode} onChange={(e) => setForm((f) => ({ ...f, zipCode: e.target.value }))} />
          </div>
        </div>
      </Modal>

      {/* Edit Modal (view-only for now) */}
      <Modal
        isOpen={!!editProperty}
        onClose={() => setEditProperty(null)}
        title="Property Details"
        size="md"
        footer={<Button variant="outline" onClick={() => setEditProperty(null)}>Close</Button>}
      >
        {editProperty && (
          <div className="space-y-3 text-sm">
            <div><span className="font-medium text-slate-700 dark:text-slate-300">Name:</span> <span className="text-slate-600 dark:text-slate-400">{editProperty.name}</span></div>
            {editProperty.description && <div><span className="font-medium text-slate-700 dark:text-slate-300">Description:</span> <span className="text-slate-600 dark:text-slate-400">{editProperty.description}</span></div>}
            {editProperty.city && <div><span className="font-medium text-slate-700 dark:text-slate-300">City:</span> <span className="text-slate-600 dark:text-slate-400">{editProperty.city}</span></div>}
            {editProperty.country && <div><span className="font-medium text-slate-700 dark:text-slate-300">Country:</span> <span className="text-slate-600 dark:text-slate-400">{editProperty.country}</span></div>}
          </div>
        )}
      </Modal>
    </div>
  );
}

export default PropertiesPage;
