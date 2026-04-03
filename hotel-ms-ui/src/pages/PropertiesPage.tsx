import { useState, useEffect, useCallback } from 'react';
import { useSlowConnection } from '../hooks/useSlowConnection';
import { Building2, MapPin, Plus, Edit, RefreshCw, Star, Save, X } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
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

const propertySchema = z.object({
  name: z.string().min(2, 'Property name must be at least 2 characters'),
  description: z.string().min(10, 'Description must be at least 10 characters'),
  street: z.string().min(3, 'Street address is required'),
  city: z.string().min(2, 'City is required'),
  state: z.string().min(2, 'State / Province is required'),
  country: z.string().min(2, 'Country is required'),
  zipCode: z.string().min(3, 'Zip / Postal code is required'),
  latitude: z.string().refine((v) => !isNaN(Number(v)), { message: 'Latitude must be a number' }).optional(),
  longitude: z.string().refine((v) => !isNaN(Number(v)), { message: 'Longitude must be a number' }).optional(),
});

type PropertyForm = z.infer<typeof propertySchema>;

function propertyToForm(p: Property): PropertyForm {
  return {
    name: p.name,
    description: p.description ?? '',
    street: p.address?.street ?? '',
    city: p.address?.city ?? p.city ?? '',
    state: p.address?.state ?? '',
    country: p.address?.country ?? p.country ?? '',
    zipCode: p.address?.zipCode ?? '',
    latitude: String(p.address?.latitude ?? 0),
    longitude: String(p.address?.longitude ?? 0),
  };
}

function PropertyFormFields({ register, errors }: {
  register: ReturnType<typeof useForm<PropertyForm>>['register'];
  errors: ReturnType<typeof useForm<PropertyForm>>['formState']['errors'];
}) {
  return (
    <div className="space-y-4">
      <Input label="Property Name" required placeholder="e.g. Grand Hotel Downtown" {...register('name')} error={errors.name?.message} />
      <Input label="Description" required placeholder="Brief description of the property" {...register('description')} error={errors.description?.message} />
      <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide pt-1">Address</p>
      <div className="grid grid-cols-2 gap-4">
        <div className="col-span-2">
          <Input label="Street" required placeholder="123 Main Street" {...register('street')} error={errors.street?.message} />
        </div>
        <Input label="City" required placeholder="City" {...register('city')} error={errors.city?.message} />
        <Input label="State / Province" required placeholder="State" {...register('state')} error={errors.state?.message} />
        <Input label="Country" required placeholder="Country" {...register('country')} error={errors.country?.message} />
        <Input label="Zip / Postal Code" required placeholder="100001" {...register('zipCode')} error={errors.zipCode?.message} />
        <Input label="Latitude" placeholder="0.000000" {...register('latitude')} error={errors.latitude?.message} />
        <Input label="Longitude" placeholder="0.000000" {...register('longitude')} error={errors.longitude?.message} />
      </div>
    </div>
  );
}

export function PropertiesPage() {
  const [properties, setProperties] = useState<Property[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [editProperty, setEditProperty] = useState<Property | null>(null);
  const toast = useToast();
  const { tenantId } = useAuthStore();
  useSlowConnection(isLoading);

  const addForm = useForm<PropertyForm>({ resolver: zodResolver(propertySchema) });
  const editForm = useForm<PropertyForm>({ resolver: zodResolver(propertySchema) });

  const loadProperties = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await propertyService.getAll({ tenantId: tenantId ?? undefined });
      setProperties(data);
    } catch (err) {
      toast.error('Failed to load properties', err instanceof Error ? err.message : 'Could not fetch properties');
    } finally {
      setIsLoading(false);
    }
  }, [tenantId, toast]);

  useEffect(() => { loadProperties(); }, [loadProperties]);

  const onAdd = addForm.handleSubmit(async (data) => {
    try {
      await propertyService.create({
        name: data.name,
        description: data.description,
        image: 'https://placehold.co/600x400',
        tenantId: tenantId ?? 1,
        address: {
          street: data.street, city: data.city, state: data.state,
          country: data.country, zipCode: data.zipCode,
          latitude: Number(data.latitude) || 0,
          longitude: Number(data.longitude) || 0,
        },
      });
      toast.success('Property added', `${data.name} has been created`);
      setIsAddOpen(false);
      addForm.reset();
      await loadProperties();
    } catch (err) {
      toast.error('Failed to create property', err instanceof Error ? err.message : 'Could not add property');
    }
  });

  const openEdit = (p: Property) => {
    setEditProperty(p);
    editForm.reset(propertyToForm(p));
  };

  const onEdit = editForm.handleSubmit(async (data) => {
    if (!editProperty) return;
    try {
      await propertyService.update({
        id: editProperty.id,
        name: data.name,
        description: data.description,
        image: editProperty.image ?? 'https://placehold.co/600x400',
        address: {
          street: data.street, city: data.city, state: data.state,
          country: data.country, zipCode: data.zipCode,
          latitude: Number(data.latitude) || 0,
          longitude: Number(data.longitude) || 0,
        },
      });
      toast.success('Property updated', `${data.name} has been updated`);
      setEditProperty(null);
      await loadProperties();
    } catch (err) {
      toast.error('Failed to update property', err instanceof Error ? err.message : 'Could not update property');
    }
  });

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
            <PropertyCard key={p.id} property={p} onEdit={openEdit} />
          ))}
        </div>
      )}

      {/* Add Modal */}
      <Modal
        isOpen={isAddOpen}
        onClose={() => { setIsAddOpen(false); addForm.reset(); }}
        title="Add New Property"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => { setIsAddOpen(false); addForm.reset(); }}>Cancel</Button>
            <Button onClick={onAdd} isLoading={addForm.formState.isSubmitting} leftIcon={<Plus className="h-4 w-4" />}>Save Property</Button>
          </>
        }
      >
        <form onSubmit={onAdd}>
          <PropertyFormFields register={addForm.register} errors={addForm.formState.errors} />
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal
        isOpen={!!editProperty}
        onClose={() => setEditProperty(null)}
        title="Edit Property"
        size="lg"
        footer={
          <>
            <Button variant="outline" leftIcon={<X className="h-4 w-4" />} onClick={() => setEditProperty(null)}>Cancel</Button>
            <Button onClick={onEdit} isLoading={editForm.formState.isSubmitting} leftIcon={<Save className="h-4 w-4" />}>Save Changes</Button>
          </>
        }
      >
        <form onSubmit={onEdit}>
          <PropertyFormFields register={editForm.register} errors={editForm.formState.errors} />
        </form>
      </Modal>
    </div>
  );
}

export default PropertiesPage;
