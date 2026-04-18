import { useState, useEffect, useCallback } from 'react';
import { Plus, Clock, CheckCircle, AlertCircle, Loader2, BedDouble, User, RefreshCw, SkipForward } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '../components/ui/Card';
import { Badge } from '../components/ui/Badge';
import { Button } from '../components/ui/Button';
import { Modal } from '../components/ui/Modal';
import { Input } from '../components/ui/Input';
import { Select } from '../components/ui/Select';
import { useToast, useAuthStore } from '../lib/store';
import { housekeepingService } from '../services/housekeeping.service';
import { roomService } from '../services/room.service';
import { userService } from '../services/user.service';
import type { HousekeepingTask, HousekeepingTaskTrigger, Room, User } from '../types';
import { cn, formatDate } from '../lib/utils';

// Columns match backend HousekeepingTaskState enum exactly
const COLUMNS: { key: HousekeepingTask['status']; label: string; icon: React.ReactNode; color: string }[] = [
  { key: 'Pending',    label: 'Pending',     icon: <Clock className="h-4 w-4" />,       color: 'text-amber-500' },
  { key: 'InProgress', label: 'In Progress', icon: <Loader2 className="h-4 w-4" />,     color: 'text-blue-500' },
  { key: 'Done',       label: 'Done',        icon: <CheckCircle className="h-4 w-4" />, color: 'text-emerald-500' },
  { key: 'Skipped',    label: 'Skipped',     icon: <AlertCircle className="h-4 w-4" />, color: 'text-slate-400' },
];

// Which triggers are valid per status
const STATUS_TRIGGERS: Partial<Record<HousekeepingTask['status'], HousekeepingTaskTrigger[]>> = {
  Pending:    ['Start', 'Skip'],
  InProgress: ['Complete', 'Skip'],
};

const PRIORITY_COLORS: Record<string, string> = {
  Urgent: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
  High:   'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',
  Medium: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
  Low:    'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-400',
  Normal: 'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-400',
};

const PRIORITY_BAR: Record<string, string> = {
  Urgent: 'bg-red-500',
  High:   'bg-orange-500',
  Medium: 'bg-blue-500',
  Low:    'bg-slate-400',
  Normal: 'bg-slate-400',
};

interface TaskCardProps {
  task: HousekeepingTask;
  onMove: (taskId: number, trigger: HousekeepingTaskTrigger) => Promise<void>;
}

function TaskCard({ task, onMove }: TaskCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [busy, setBusy] = useState<HousekeepingTaskTrigger | null>(null);
  const triggers = STATUS_TRIGGERS[task.status] ?? [];
  const priority = task.priority ?? 'Normal';

  const fire = async (trigger: HousekeepingTaskTrigger) => {
    setBusy(trigger);
    await onMove(task.id, trigger);
    setBusy(null);
  };

  return (
    <div
      className={cn(
        'bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700',
        'shadow-sm hover:shadow-md transition-all duration-200 cursor-pointer overflow-hidden'
      )}
      onClick={() => setIsExpanded(!isExpanded)}
    >
      <div className={cn('h-1', PRIORITY_BAR[priority] ?? 'bg-slate-400')} />
      <div className="p-4">
        <div className="flex items-start justify-between gap-2 mb-2">
          <div>
            <div className="flex items-center gap-2 mb-1">
              <span className="font-mono text-xs bg-slate-100 dark:bg-slate-700 px-1.5 py-0.5 rounded text-slate-600 dark:text-slate-400">
                {task.roomNumber ?? `Room ${task.roomId}`}
              </span>
              <span className={cn('text-xs font-medium px-2 py-0.5 rounded-full', PRIORITY_COLORS[priority] ?? PRIORITY_COLORS.Normal)}>
                {priority}
              </span>
            </div>
            <p className="font-semibold text-slate-900 dark:text-slate-100 text-sm">{task.taskType ?? 'Cleaning'}</p>
          </div>
          <BedDouble className="h-4 w-4 text-slate-400 flex-shrink-0" />
        </div>

        {task.assignedToName && (
          <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400 mt-2">
            <User className="h-3.5 w-3.5" />
            <span>{task.assignedToName}</span>
          </div>
        )}

        {isExpanded && (
          <div className="mt-3 pt-3 border-t border-slate-100 dark:border-slate-700 space-y-2">
            {task.notes && (
              <p className="text-xs text-slate-600 dark:text-slate-400 bg-slate-50 dark:bg-slate-700/50 p-2 rounded-lg">
                {task.notes}
              </p>
            )}
            <p className="text-xs text-slate-400">Created: {formatDate(task.createdAt)}</p>
            {task.completedAt && (
              <p className="text-xs text-emerald-600 dark:text-emerald-400">
                Completed: {formatDate(task.completedAt)}
              </p>
            )}
          </div>
        )}

        {triggers.length > 0 && (
          <div className="flex gap-2 mt-3" onClick={(e) => e.stopPropagation()}>
            {triggers.map((trigger) => (
              <button
                key={trigger}
                disabled={busy !== null}
                onClick={() => fire(trigger)}
                className={cn(
                  'flex-1 py-1.5 rounded-lg text-xs font-medium transition-colors flex items-center justify-center gap-1',
                  trigger === 'Start'
                    ? 'bg-blue-50 hover:bg-blue-100 dark:bg-blue-900/20 dark:hover:bg-blue-900/30 text-blue-600 dark:text-blue-400'
                    : trigger === 'Complete'
                    ? 'bg-emerald-50 hover:bg-emerald-100 dark:bg-emerald-900/20 dark:hover:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400'
                    : 'bg-slate-100 hover:bg-slate-200 dark:bg-slate-700/50 dark:hover:bg-slate-700 text-slate-600 dark:text-slate-400',
                  busy !== null && 'opacity-50 cursor-not-allowed'
                )}
              >
                {trigger === 'Skip' && <SkipForward className="h-3 w-3" />}
                {busy === trigger ? '…' : trigger === 'Start' ? 'Start' : trigger === 'Complete' ? 'Complete' : 'Skip'}
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export function HousekeepingPage() {
  const [tasks, setTasks] = useState<HousekeepingTask[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [staffUsers, setStaffUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [newTask, setNewTask] = useState({
    roomId: '', taskType: 'Cleaning', priority: 'Normal', notes: '', scheduledAt: '', assignedToUserId: '',
  });
  const toast = useToast();
  const { tenantId } = useAuthStore();

  const loadData = useCallback(async () => {
    setIsLoading(true);
    try {
      const [taskData, roomData, userData] = await Promise.allSettled([
        housekeepingService.getTasks({ tenantId: tenantId ?? undefined }),
        roomService.getAll(),
        userService.getAll(),
      ]);
      if (taskData.status === 'fulfilled') setTasks(taskData.value);
      else toast.error('Failed to load tasks', 'Could not fetch housekeeping tasks');
      if (roomData.status === 'fulfilled') setRooms(roomData.value);
      if (userData.status === 'fulfilled') {
        setStaffUsers(userData.value.filter((u) =>
          u.userRoles?.some((r) => ['Housekeeping', 'FrontDesk', 'Admin', 'SuperAdmin'].includes(r.name))
        ));
      }
    } finally {
      setIsLoading(false);
    }
  }, [tenantId, toast]);

  useEffect(() => { loadData(); }, [loadData]);

  const moveTask = async (taskId: number, trigger: HousekeepingTaskTrigger) => {
    try {
      const updated = await housekeepingService.changeState(taskId, trigger);
      setTasks((prev) => prev.map((t) => (t.id === taskId ? updated : t)));
      toast.success('Task updated', `Task ${trigger === 'Complete' ? 'completed' : trigger === 'Skip' ? 'skipped' : 'started'}`);
    } catch (err) {
      toast.error('Update failed', err instanceof Error ? err.message : 'Could not update task state');
    }
  };

  const handleAddTask = async () => {
    if (!newTask.roomId) { toast.error('Validation', 'Please select a room'); return; }
    setIsSubmitting(true);
    try {
      const created = await housekeepingService.create({
        roomId: Number(newTask.roomId),
        taskType: newTask.taskType,
        priority: newTask.priority,
        notes: newTask.notes || undefined,
        scheduledAt: newTask.scheduledAt || undefined,
        assignedToUserId: newTask.assignedToUserId ? Number(newTask.assignedToUserId) : undefined,
        tenantId: tenantId ?? undefined,
      });
      setTasks((prev) => [created, ...prev]);
      toast.success('Task created', 'New housekeeping task added');
      setIsAddOpen(false);
      setNewTask({ roomId: '', taskType: 'Cleaning', priority: 'Normal', notes: '', scheduledAt: '', assignedToUserId: '' });
    } catch (err) {
      toast.error('Failed to create task', err instanceof Error ? err.message : 'Could not create housekeeping task');
    } finally {
      setIsSubmitting(false);
    }
  };

  const getColumnTasks = (status: HousekeepingTask['status']) => tasks.filter((t) => t.status === status);

  const stats = [
    { label: 'Pending',     count: getColumnTasks('Pending').length,    color: 'text-amber-500' },
    { label: 'In Progress', count: getColumnTasks('InProgress').length,  color: 'text-blue-500' },
    { label: 'Done Today',  count: getColumnTasks('Done').length,        color: 'text-emerald-500' },
    { label: 'Total Tasks', count: tasks.length,                         color: 'text-slate-700 dark:text-slate-300' },
  ];

  const roomOptions = [
    { value: '', label: 'Select a room…' },
    ...rooms.map((r) => ({ value: String(r.id), label: `Room ${r.roomNumber ?? r.number} — ${r.type ?? ''} (${r.roomState})` })),
  ];

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Housekeeping</h2>
          <p className="page-subtitle">Manage room cleaning and maintenance tasks</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" leftIcon={<RefreshCw className="h-4 w-4" />} onClick={loadData} isLoading={isLoading}>
            Refresh
          </Button>
          <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsAddOpen(true)}>
            Add Task
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-8">
        {stats.map((s) => (
          <div key={s.label} className="bg-white dark:bg-slate-800 rounded-xl border border-slate-100 dark:border-slate-700 p-4 shadow-sm">
            {isLoading ? (
              <div className="h-8 w-12 bg-slate-200 dark:bg-slate-700 rounded animate-pulse mb-1" />
            ) : (
              <p className={cn('text-3xl font-bold', s.color)}>{s.count}</p>
            )}
            <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{s.label}</p>
          </div>
        ))}
      </div>

      {/* Kanban Board */}
      {isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {COLUMNS.map((col) => (
            <div key={col.key} className="space-y-3">
              <div className="h-6 w-24 bg-slate-200 dark:bg-slate-700 rounded animate-pulse" />
              {[1, 2].map((i) => (
                <div key={i} className="h-28 bg-slate-200 dark:bg-slate-700 rounded-xl animate-pulse" />
              ))}
            </div>
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {COLUMNS.map((col) => {
            const colTasks = getColumnTasks(col.key);
            return (
              <div key={col.key} className="flex flex-col">
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center gap-2">
                    <span className={col.color}>{col.icon}</span>
                    <span className="text-sm font-semibold text-slate-700 dark:text-slate-300">{col.label}</span>
                  </div>
                  <span className="text-xs font-medium bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-400 px-2 py-0.5 rounded-full">
                    {colTasks.length}
                  </span>
                </div>
                <div className="flex flex-col gap-3 min-h-[200px]">
                  {colTasks.map((task) => (
                    <TaskCard key={task.id} task={task} onMove={moveTask} />
                  ))}
                  {colTasks.length === 0 && (
                    <div className="flex-1 flex items-center justify-center py-8 rounded-xl border-2 border-dashed border-slate-200 dark:border-slate-700">
                      <p className="text-sm text-slate-400 dark:text-slate-500">No tasks</p>
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Add Task Modal */}
      <Modal
        isOpen={isAddOpen}
        onClose={() => setIsAddOpen(false)}
        title="Add Housekeeping Task"
        size="md"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsAddOpen(false)}>Cancel</Button>
            <Button onClick={handleAddTask} isLoading={isSubmitting}>Add Task</Button>
          </>
        }
      >
        <div className="space-y-4">
          <Select
            label="Room"
            required
            value={newTask.roomId}
            onChange={(e) => setNewTask((p) => ({ ...p, roomId: e.target.value }))}
            options={roomOptions}
          />
          {rooms.length === 0 && (
            <p className="text-xs text-amber-500">No rooms loaded. Make sure rooms exist and the API is reachable.</p>
          )}
          <Select
            label="Task Type"
            value={newTask.taskType}
            onChange={(e) => setNewTask((p) => ({ ...p, taskType: e.target.value }))}
            options={[
              { value: 'Cleaning', label: 'Cleaning' },
              { value: 'Linen Change', label: 'Linen Change' },
              { value: 'Maintenance', label: 'Maintenance' },
              { value: 'Turndown', label: 'Turndown Service' },
              { value: 'Inspection', label: 'Room Inspection' },
              { value: 'Deep Clean', label: 'Deep Clean' },
            ]}
          />
          <Select
            label="Priority"
            value={newTask.priority}
            onChange={(e) => setNewTask((p) => ({ ...p, priority: e.target.value }))}
            options={[
              { value: 'Low', label: 'Low' },
              { value: 'Normal', label: 'Normal' },
              { value: 'High', label: 'High' },
              { value: 'Urgent', label: 'Urgent' },
            ]}
          />
          <Select
            label="Assign To"
            value={newTask.assignedToUserId}
            onChange={(e) => setNewTask((p) => ({ ...p, assignedToUserId: e.target.value }))}
            options={[
              { value: '', label: 'Unassigned' },
              ...staffUsers.map((u) => ({ value: String(u.id), label: u.fullName || `${u.firstName} ${u.lastName}`.trim() || u.email })),
            ]}
          />
          <Input
            label="Scheduled Date & Time"
            type="datetime-local"
            value={newTask.scheduledAt}
            onChange={(e) => setNewTask((p) => ({ ...p, scheduledAt: e.target.value }))}
          />
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">Notes</label>
            <textarea
              rows={3}
              value={newTask.notes}
              onChange={(e) => setNewTask((p) => ({ ...p, notes: e.target.value }))}
              placeholder="Task details or special instructions..."
              className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
            />
          </div>
        </div>
      </Modal>
    </div>
  );
}

export default HousekeepingPage;
