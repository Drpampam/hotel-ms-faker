import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Search,
  X,
  UserCircle,
  BedDouble,
  CalendarCheck,
  Loader2,
  ArrowRight,
  CornerDownLeft,
} from 'lucide-react';
import { guestService } from '../../services/guest.service';
import { roomService } from '../../services/room.service';
import { reservationService } from '../../services/reservation.service';
import type { Guest, Room, Reservation } from '../../types';
import { formatCurrency } from '../../lib/utils';
import { cn } from '../../lib/utils';

interface SearchModalProps {
  isOpen: boolean;
  onClose: () => void;
}

type ResultType = 'guest' | 'room' | 'reservation';

interface Result {
  type: ResultType;
  id: number;
  title: string;
  meta: string;
  badge?: string;
  badgeColor?: string;
}

const STATUS_COLOR: Record<string, string> = {
  Available:  'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
  Occupied:   'bg-blue-100   text-blue-700   dark:bg-blue-900/30   dark:text-blue-400',
  Cleaning:   'bg-amber-100  text-amber-700  dark:bg-amber-900/30  dark:text-amber-400',
  Maintenance:'bg-red-100    text-red-700    dark:bg-red-900/30    dark:text-red-400',
  Pending:    'bg-amber-100  text-amber-700  dark:bg-amber-900/30  dark:text-amber-400',
  Confirmed:  'bg-blue-100   text-blue-700   dark:bg-blue-900/30   dark:text-blue-400',
  CheckedIn:  'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
  CheckedOut: 'bg-slate-100  text-slate-600  dark:bg-slate-700     dark:text-slate-300',
  Cancelled:  'bg-red-100    text-red-700    dark:bg-red-900/30    dark:text-red-400',
};

const ICON: Record<ResultType, React.ReactNode> = {
  guest:       <UserCircle  className="h-4 w-4 text-indigo-500 flex-shrink-0" />,
  room:        <BedDouble   className="h-4 w-4 text-purple-500 flex-shrink-0" />,
  reservation: <CalendarCheck className="h-4 w-4 text-blue-500 flex-shrink-0" />,
};

const SECTION_LABEL: Record<ResultType, string> = {
  guest: 'Guests',
  room: 'Rooms',
  reservation: 'Reservations',
};

const MAX_PER_TYPE = 5;

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const t = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(t);
  }, [value, delay]);
  return debounced;
}

export function SearchModal({ isOpen, onClose }: SearchModalProps) {
  const navigate = useNavigate();
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  const [query, setQuery] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [dataLoaded, setDataLoaded] = useState(false);
  const [activeIndex, setActiveIndex] = useState(0);

  const debouncedQuery = useDebounce(query, 250);

  // Focus input when opened
  useEffect(() => {
    if (isOpen) {
      setQuery('');
      setActiveIndex(0);
      setTimeout(() => inputRef.current?.focus(), 50);
    }
  }, [isOpen]);

  // Load all data once on first open
  useEffect(() => {
    if (!isOpen || dataLoaded) return;
    setIsLoading(true);
    Promise.allSettled([
      guestService.getAll({ pageSize: 200 }),
      roomService.getAll({ pageSize: 200 }),
      reservationService.getAll({ pageSize: 200 }),
    ]).then(([g, r, res]) => {
      setGuests(g.status === 'fulfilled' ? g.value : []);
      setRooms(r.status === 'fulfilled' ? r.value : []);
      setReservations(res.status === 'fulfilled' ? res.value : []);
      setDataLoaded(true);
    }).finally(() => setIsLoading(false));
  }, [isOpen, dataLoaded]);

  // Build filtered results
  const results: Result[] = (() => {
    if (!debouncedQuery.trim()) return [];
    const q = debouncedQuery.toLowerCase();

    const guestResults: Result[] = guests
      .filter((g) => {
        const name = (g.fullName ?? `${g.firstName} ${g.lastName}`).toLowerCase();
        return name.includes(q) || (g.email ?? '').toLowerCase().includes(q) || (g.phoneNumber ?? '').includes(q);
      })
      .slice(0, MAX_PER_TYPE)
      .map((g) => ({
        type: 'guest' as const,
        id: g.id,
        title: g.fullName ?? `${g.firstName} ${g.lastName}`.trim() || '—',
        meta: [g.email, g.phoneNumber].filter(Boolean).join(' · ') || 'No contact info',
        badge: g.loyaltyTier,
        badgeColor: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
      }));

    const roomResults: Result[] = rooms
      .filter((r) => {
        const num = (r.roomNumber ?? r.number ?? '').toLowerCase();
        const type = (r.type ?? '').toLowerCase();
        return num.includes(q) || type.includes(q);
      })
      .slice(0, MAX_PER_TYPE)
      .map((r) => ({
        type: 'room' as const,
        id: r.id,
        title: `Room ${r.roomNumber ?? r.number}`,
        meta: `${r.type ?? '—'} · ${formatCurrency(r.pricePerNight)}/night`,
        badge: r.status,
        badgeColor: STATUS_COLOR[r.status] ?? '',
      }));

    const reservationResults: Result[] = reservations
      .filter((r) => {
        const ref = (r.reservationNumber ?? `RES-${r.id}`).toLowerCase();
        const guest = (r.guestName ?? '').toLowerCase();
        const room = (r.roomNumber ?? '').toLowerCase();
        return ref.includes(q) || guest.includes(q) || room.includes(q);
      })
      .slice(0, MAX_PER_TYPE)
      .map((r) => ({
        type: 'reservation' as const,
        id: r.id,
        title: r.guestName ?? `Reservation #${r.id}`,
        meta: `${r.reservationNumber ?? `RES-${r.id}`} · Room ${r.roomNumber ?? '—'} · ${formatCurrency(r.totalAmount ?? 0)}`,
        badge: r.status,
        badgeColor: STATUS_COLOR[r.status] ?? '',
      }));

    return [...guestResults, ...roomResults, ...reservationResults];
  })();

  // Reset active index when results change
  useEffect(() => { setActiveIndex(0); }, [results.length]);

  const handleSelect = useCallback((result: Result) => {
    onClose();
    setQuery('');
    switch (result.type) {
      case 'guest':       navigate('/guests');       break;
      case 'room':        navigate('/rooms');         break;
      case 'reservation': navigate('/reservations'); break;
    }
  }, [navigate, onClose]);

  // Keyboard navigation
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') { onClose(); return; }
    if (!results.length) return;
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setActiveIndex((i) => Math.min(i + 1, results.length - 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setActiveIndex((i) => Math.max(i - 1, 0));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (results[activeIndex]) handleSelect(results[activeIndex]);
    }
  };

  // Scroll active item into view
  useEffect(() => {
    const el = listRef.current?.querySelector(`[data-index="${activeIndex}"]`);
    el?.scrollIntoView({ block: 'nearest' });
  }, [activeIndex]);

  if (!isOpen) return null;

  // Group results for section headers
  const sections: ResultType[] = ['guest', 'room', 'reservation'];
  const grouped = sections
    .map((type) => ({ type, items: results.filter((r) => r.type === type) }))
    .filter((g) => g.items.length > 0);

  // Flat index map for keyboard nav across sections
  let flatIndex = 0;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/40 backdrop-blur-sm z-50"
        onClick={onClose}
      />

      {/* Panel */}
      <div className="fixed left-1/2 top-[12vh] -translate-x-1/2 w-full max-w-xl z-50 px-4">
        <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
          {/* Input row */}
          <div className="flex items-center gap-3 px-4 py-3 border-b border-slate-100 dark:border-slate-800">
            {isLoading
              ? <Loader2 className="h-5 w-5 text-slate-400 flex-shrink-0 animate-spin" />
              : <Search className="h-5 w-5 text-slate-400 flex-shrink-0" />
            }
            <input
              ref={inputRef}
              type="text"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Search guests, rooms, reservations..."
              className="flex-1 bg-transparent text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 outline-none"
            />
            {query && (
              <button onClick={() => setQuery('')} className="text-slate-400 hover:text-slate-600 transition-colors">
                <X className="h-4 w-4" />
              </button>
            )}
            <kbd className="hidden sm:flex items-center gap-1 text-xs text-slate-400 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded px-1.5 py-0.5">
              Esc
            </kbd>
          </div>

          {/* Results */}
          <div ref={listRef} className="max-h-[60vh] overflow-y-auto">
            {!debouncedQuery.trim() && (
              <div className="px-4 py-8 text-center">
                <Search className="h-8 w-8 text-slate-300 dark:text-slate-600 mx-auto mb-2" />
                <p className="text-sm text-slate-500 dark:text-slate-400">
                  Start typing to search across guests, rooms, and reservations
                </p>
              </div>
            )}

            {debouncedQuery.trim() && results.length === 0 && !isLoading && (
              <div className="px-4 py-8 text-center">
                <p className="text-sm text-slate-500 dark:text-slate-400">
                  No results for <span className="font-medium text-slate-700 dark:text-slate-300">"{debouncedQuery}"</span>
                </p>
              </div>
            )}

            {grouped.map(({ type, items }) => (
              <div key={type}>
                {/* Section header */}
                <div className="px-4 py-1.5 bg-slate-50 dark:bg-slate-800/50 border-b border-slate-100 dark:border-slate-800">
                  <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide flex items-center gap-1.5">
                    {ICON[type]} {SECTION_LABEL[type]}
                  </p>
                </div>

                {items.map((result) => {
                  const idx = flatIndex++;
                  return (
                    <button
                      key={`${result.type}-${result.id}`}
                      data-index={idx}
                      onClick={() => handleSelect(result)}
                      onMouseEnter={() => setActiveIndex(idx)}
                      className={cn(
                        'w-full flex items-center gap-3 px-4 py-3 text-left transition-colors border-b border-slate-50 dark:border-slate-800/50 last:border-0',
                        activeIndex === idx
                          ? 'bg-indigo-50 dark:bg-indigo-900/20'
                          : 'hover:bg-slate-50 dark:hover:bg-slate-800/40'
                      )}
                    >
                      <div className={cn(
                        'w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0',
                        type === 'guest'       ? 'bg-indigo-100 dark:bg-indigo-900/30' :
                        type === 'room'        ? 'bg-purple-100 dark:bg-purple-900/30' :
                                                 'bg-blue-100   dark:bg-blue-900/30'
                      )}>
                        {ICON[type]}
                      </div>

                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-slate-900 dark:text-slate-100 truncate">
                          {result.title}
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 truncate mt-0.5">
                          {result.meta}
                        </p>
                      </div>

                      {result.badge && (
                        <span className={cn('text-xs font-medium px-2 py-0.5 rounded-full flex-shrink-0', result.badgeColor)}>
                          {result.badge}
                        </span>
                      )}

                      <ArrowRight className={cn(
                        'h-4 w-4 flex-shrink-0 transition-opacity',
                        activeIndex === idx ? 'opacity-100 text-indigo-500' : 'opacity-0'
                      )} />
                    </button>
                  );
                })}
              </div>
            ))}
          </div>

          {/* Footer hint */}
          {results.length > 0 && (
            <div className="flex items-center gap-4 px-4 py-2 border-t border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-800/50">
              <span className="flex items-center gap-1 text-xs text-slate-400">
                <kbd className="bg-white dark:bg-slate-700 border border-slate-200 dark:border-slate-600 rounded px-1">↑↓</kbd> navigate
              </span>
              <span className="flex items-center gap-1 text-xs text-slate-400">
                <CornerDownLeft className="h-3 w-3" /> select
              </span>
              <span className="flex items-center gap-1 text-xs text-slate-400">
                <kbd className="bg-white dark:bg-slate-700 border border-slate-200 dark:border-slate-600 rounded px-1">Esc</kbd> close
              </span>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
