import { useState } from 'react';
import { Building2, Phone, Mail, Globe, Star, BedDouble, MapPin, Plus, Edit } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Modal } from '../components/ui/Modal';
import { Input } from '../components/ui/Input';
import type { Property } from '../types';

const MOCK_PROPERTIES: Property[] = [
  {
    id: '1',
    name: 'Grand Hotel Downtown',
    address: '123 Main Street',
    city: 'New York',
    country: 'United States',
    phoneNumber: '+1 212-555-0100',
    email: 'info@grandhotel.com',
    website: 'www.grandhotel.com',
    starRating: 5,
    totalRooms: 120,
    tenantId: 1,
    createdAt: new Date(Date.now() - 365 * 86400000).toISOString(),
  },
  {
    id: '2',
    name: 'Seaside Resort & Spa',
    address: '456 Ocean Drive',
    city: 'Miami Beach',
    country: 'United States',
    phoneNumber: '+1 305-555-0200',
    email: 'hello@seasideresort.com',
    website: 'www.seasideresort.com',
    starRating: 4,
    totalRooms: 80,
    tenantId: 1,
    createdAt: new Date(Date.now() - 180 * 86400000).toISOString(),
  },
];

function StarRating({ count }: { count: number }) {
  return (
    <div className="flex items-center gap-0.5">
      {Array.from({ length: 5 }).map((_, i) => (
        <Star
          key={i}
          className={`h-4 w-4 ${i < count ? 'text-amber-400 fill-amber-400' : 'text-slate-300 dark:text-slate-600'}`}
        />
      ))}
    </div>
  );
}

function PropertyCard({ property, onEdit }: { property: Property; onEdit: (p: Property) => void }) {
  return (
    <Card className="overflow-hidden" padding="none">
      {/* Header gradient */}
      <div className="h-32 bg-gradient-to-br from-indigo-500 via-indigo-600 to-violet-600 relative overflow-hidden">
        <div className="absolute inset-0 opacity-20">
          <div className="absolute top-4 right-4 w-24 h-24 border-2 border-white rounded-full" />
          <div className="absolute bottom-0 left-0 w-32 h-32 border-2 border-white rounded-full -translate-x-1/2 translate-y-1/2" />
        </div>
        <div className="absolute bottom-4 left-5 right-5 flex items-end justify-between">
          <div className="w-12 h-12 bg-white/20 backdrop-blur-sm rounded-xl flex items-center justify-center">
            <Building2 className="h-6 w-6 text-white" />
          </div>
          <Badge className="bg-white/20 text-white border border-white/30">
            {property.totalRooms} rooms
          </Badge>
        </div>
      </div>

      <div className="p-5">
        <div className="flex items-start justify-between mb-3">
          <div>
            <h3 className="font-bold text-slate-900 dark:text-slate-100 text-lg leading-tight">
              {property.name}
            </h3>
            <div className="mt-1">
              <StarRating count={property.starRating ?? 3} />
            </div>
          </div>
          <Button variant="ghost" size="icon" onClick={() => onEdit(property)}>
            <Edit className="h-4 w-4" />
          </Button>
        </div>

        <div className="space-y-2.5 text-sm">
          <div className="flex items-start gap-2 text-slate-600 dark:text-slate-400">
            <MapPin className="h-4 w-4 flex-shrink-0 mt-0.5" />
            <span>{property.address}, {property.city}, {property.country}</span>
          </div>
          {property.phoneNumber && (
            <div className="flex items-center gap-2 text-slate-600 dark:text-slate-400">
              <Phone className="h-4 w-4 flex-shrink-0" />
              <span>{property.phoneNumber}</span>
            </div>
          )}
          {property.email && (
            <div className="flex items-center gap-2 text-slate-600 dark:text-slate-400">
              <Mail className="h-4 w-4 flex-shrink-0" />
              <span>{property.email}</span>
            </div>
          )}
          {property.website && (
            <div className="flex items-center gap-2 text-indigo-600 dark:text-indigo-400">
              <Globe className="h-4 w-4 flex-shrink-0" />
              <a href={`https://${property.website}`} target="_blank" rel="noopener noreferrer" className="hover:underline">
                {property.website}
              </a>
            </div>
          )}
        </div>

        <div className="mt-4 pt-4 border-t border-slate-100 dark:border-slate-700 flex items-center justify-between">
          <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
            <BedDouble className="h-3.5 w-3.5" />
            <span>{property.totalRooms} total rooms</span>
          </div>
          <Badge variant="success" dot>Active</Badge>
        </div>
      </div>
    </Card>
  );
}

export function PropertiesPage() {
  const [properties] = useState<Property[]>(MOCK_PROPERTIES);
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [editProperty, setEditProperty] = useState<Property | null>(null);

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Properties</h2>
          <p className="page-subtitle">{properties.length} properties managed</p>
        </div>
        <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsAddOpen(true)}>
          Add Property
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
        {[
          { label: 'Total Properties', value: properties.length, icon: <Building2 className="h-5 w-5 text-indigo-600" />, bg: 'bg-indigo-50 dark:bg-indigo-900/20' },
          { label: 'Total Rooms', value: properties.reduce((s, p) => s + (p.totalRooms ?? 0), 0), icon: <BedDouble className="h-5 w-5 text-blue-600" />, bg: 'bg-blue-50 dark:bg-blue-900/20' },
          { label: 'Active Properties', value: properties.length, icon: <Star className="h-5 w-5 text-amber-500" />, bg: 'bg-amber-50 dark:bg-amber-900/20' },
        ].map((s) => (
          <div key={s.label} className="flex items-center gap-4 p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-100 dark:border-slate-700 shadow-sm">
            <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${s.bg}`}>
              {s.icon}
            </div>
            <div>
              <p className="text-xs text-slate-500 dark:text-slate-400">{s.label}</p>
              <p className="text-2xl font-bold text-slate-900 dark:text-slate-100">{s.value}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Properties grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
        {properties.map((p) => (
          <PropertyCard key={p.id} property={p} onEdit={setEditProperty} />
        ))}
      </div>

      {/* Add Property Modal */}
      <Modal
        isOpen={isAddOpen}
        onClose={() => setIsAddOpen(false)}
        title="Add New Property"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsAddOpen(false)}>Cancel</Button>
            <Button onClick={() => setIsAddOpen(false)}>Save Property</Button>
          </>
        }
      >
        <div className="space-y-4">
          <Input label="Property Name" required placeholder="e.g. Grand Hotel Downtown" />
          <div className="grid grid-cols-2 gap-4">
            <Input label="Address" placeholder="Street address" />
            <Input label="City" placeholder="City" />
            <Input label="Country" placeholder="Country" />
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">Star Rating</label>
              <select className="w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100">
                {[1, 2, 3, 4, 5].map((s) => <option key={s} value={s}>{s} Star{s > 1 ? 's' : ''}</option>)}
              </select>
            </div>
            <Input label="Phone Number" type="tel" placeholder="+1 555-0100" />
            <Input label="Email" type="email" placeholder="info@property.com" />
            <Input label="Website" placeholder="www.property.com" />
            <Input label="Total Rooms" type="number" min={1} placeholder="120" />
          </div>
        </div>
      </Modal>

      {/* Edit Property Modal */}
      <Modal
        isOpen={!!editProperty}
        onClose={() => setEditProperty(null)}
        title="Edit Property"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => setEditProperty(null)}>Cancel</Button>
            <Button onClick={() => setEditProperty(null)}>Save Changes</Button>
          </>
        }
      >
        {editProperty && (
          <div className="space-y-4">
            <Input label="Property Name" defaultValue={editProperty.name} />
            <div className="grid grid-cols-2 gap-4">
              <Input label="Address" defaultValue={editProperty.address} />
              <Input label="City" defaultValue={editProperty.city} />
              <Input label="Country" defaultValue={editProperty.country} />
              <Input label="Phone" type="tel" defaultValue={editProperty.phoneNumber} />
              <Input label="Email" type="email" defaultValue={editProperty.email} />
              <Input label="Website" defaultValue={editProperty.website} />
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

export default PropertiesPage;
