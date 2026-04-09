// Auth - matches backend LoginResponseDTO
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

// The backend returns token in the response header "Token", and user info in response body data
export interface LoginResponseData {
  email?: string;
  fullName?: string;
  picture?: string;
  roles?: string[];
}

// The backend BaseResponse<LoginResponseDTO>
export interface BackendLoginResponse {
  status: boolean;
  statusCode?: string;
  message?: string;
  data?: LoginResponseData;
}

export interface AuthUser {
  email: string;
  fullName: string;
  roles: string[];
  tenantId: number;
  picture?: string;
}

// Legacy alias kept so existing page code compiles
export interface AuthResponse {
  data: {
    token: string;
    refreshToken: string;
    user?: AuthUser;
  };
  message?: string;
  success?: boolean;
}

export interface AuthState {
  user: AuthUser | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

// Users - matches ApplicationUserDTO
export type UserRole = 'Admin' | 'SuperAdmin' | 'FrontDesk' | 'Housekeeping' | 'Guest' | 'Developer';

export interface User {
  // ApplicationUserDTO fields
  fullName?: string;
  status?: string;
  email: string;
  creationDate?: string;
  lastActiveDate?: string;
  lastModifiedDate?: string;
  userRoles?: Array<{ id: number; name: string }>;
  isActive?: boolean;
  phoneNumber?: string;
  shift?: string;
  department?: string;
  picture?: string;
  // Convenience shims used by the UI
  id: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  tenantId: number;
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber: string;
  role: UserRole;
  hotelName?: string;
  subscriptionPlanId?: number;
}

// Rooms - matches RoomResponseDTO
export type RoomStatus = 'Available' | 'Occupied' | 'Maintenance' | 'Cleaning';
export type RoomType = 'Standard' | 'Deluxe' | 'Suite' | 'Presidential' | 'Twin' | 'Double' | 'Single';

// RoomState enum values from Core.States (must match backend exactly)
export type RoomState =
  | 'Available'
  | 'Occupied'
  | 'Cleaning'
  | 'Maintenance';

// RoomTrigger enum values from Core.States (must match backend exactly)
export type RoomTrigger =
  | 'CheckIn'
  | 'CheckOut'
  | 'SetCleaning'
  | 'FinishCleaning'
  | 'SetMaintenance'
  | 'FinishMaintenance';

export interface Room {
  // Backend RoomResponseDTO fields
  id: number;
  number?: string;       // backend field name
  type?: string;
  capacity: number;
  pricePerNight: number;
  isAvailable: boolean;
  roomState: RoomState;
  propertyId: number;
  createdBy?: string;
  creationDate?: string;
  lastModifiedDate?: string;
  // UI shims
  roomNumber: string;    // mapped from number
  status: RoomStatus;    // derived from roomState
  floor?: number;
  description?: string;
  amenities?: string[];
  tenantId?: number;
  createdAt?: string;
}

export interface AddRoomRequest {
  propertyId: number;
  number: string;
  type: string;
  capacity: number;
  pricePerNight: number;
}

export interface UpdateRoomRequest {
  id: number;
  number?: string;
  type?: string;
  capacity?: number;
  pricePerNight?: number;
}

// Guests - matches GuestProfileResponseDTO
export interface Guest {
  // Backend fields
  id: number;
  userId: number;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  passportNumber?: string;
  nationality?: string;
  dateOfBirth?: string;
  preferredRoomType?: string;
  specialRequests?: string;
  loyaltyPoints: number;
  loyaltyTier?: string;
  tenantId?: number;
  creationDate?: string;
  // UI shims
  firstName: string;
  lastName: string;
  country?: string;
  address?: string;
  city?: string;
  idNumber?: string;
  idType?: string;
  createdAt: string;
  totalReservations?: number;
}

export interface CreateGuestProfileRequest {
  userId: number;
  passportNumber?: string;
  nationality?: string;
  dateOfBirth?: string;
  preferredRoomType?: string;
  specialRequests?: string;
  tenantId?: number;
}

// Keep legacy name for pages that use it
export interface CreateGuestRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  address?: string;
  city?: string;
  country?: string;
  idNumber?: string;
  idType?: string;
  nationality?: string;
}

// Reservations - matches ReservationResponseDTO
export type ReservationStatus =
  | 'Pending'
  | 'Confirmed'
  | 'CheckedIn'
  | 'CheckedOut'
  | 'Cancelled'
  | 'NoShow';

// ReservationState enum from backend
export type ReservationState =
  | 'Pending'
  | 'Confirmed'
  | 'CheckedIn'
  | 'CheckedOut'
  | 'Cancelled'
  | 'NoShow';

export interface ReservationExpense {
  id: number;
  reservationId: number;
  description: string;
  category?: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  createdBy?: string;
  creationDate?: string;
}

export interface AddReservationExpenseRequest {
  description: string;
  category?: string;
  quantity: number;
  unitPrice: number;
}

export interface Reservation {
  // Backend ReservationResponseDTO fields
  id: number;
  roomId: number;
  roomNumber?: string;
  roomType?: string;
  guestId: number;
  guestName?: string;
  guestEmail?: string;
  checkInDate: string;
  checkOutDate: string;
  nightsCount: number;
  totalPrice: number;
  expensesTotal: number;
  grandTotal: number;
  expenses: ReservationExpense[];
  status: ReservationStatus;
  specialRequests?: string;
  discountId?: number;
  createdBy?: string;
  creationDate?: string;
  lastModifiedDate?: string;
  // UI shims
  reservationNumber?: string;
  totalAmount: number;  // alias for totalPrice
  adults?: number;
  children?: number;
  guest?: Guest;
  room?: Room;
  tenantId?: number;
  createdAt: string;
}

export interface CreateReservationRequest {
  roomId: number;
  guestId: number;
  checkInDate: string;
  checkOutDate: string;
  discountId?: number;
  specialRequests?: string;
}

// Payments - matches PaymentResponseDTO
export type PaymentState = 'Pending' | 'Processing' | 'Completed' | 'Failed' | 'Refunded';
export type PaymentTrigger = 'Process' | 'Complete' | 'Fail' | 'Retry' | 'Refund';

export interface Payment {
  id: number;
  reservationId: number;
  paymentMethod: string;
  amount: number;
  isSuccessful: boolean;
  transactionId?: string;
  paymentDate: string;
  paymentState: PaymentState;
}

export interface CapturePaymentRequest {
  reservationId: number;
  paymentMethod: string;
  amount: number;
  transactionId?: string;
}

// Properties - matches PropertyResponseDTO
export interface PropertyAddress {
  street?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
}

export interface Property {
  id: number;
  name: string;
  description?: string;
  image?: string;
  creationDate?: string;
  lastModifiedDate?: string;
  address?: PropertyAddress;
  // UI shims
  city?: string;
  country?: string;
  phoneNumber?: string;
  email?: string;
  website?: string;
  starRating?: number;
  totalRooms?: number;
  tenantId?: number;
  createdAt: string;
}

// Housekeeping - matches HousekeepingTaskResponseDTO
export type HousekeepingTaskStatus = 'Pending' | 'InProgress' | 'Done' | 'Skipped';
export type HousekeepingTaskPriority = 'Low' | 'Medium' | 'High' | 'Urgent' | 'Normal';
// Must match backend HousekeepingTaskState enum exactly
export type HousekeepingTaskState =
  | 'Pending'
  | 'InProgress'
  | 'Done'
  | 'Skipped';
// Must match backend HousekeepingTaskTrigger enum exactly
export type HousekeepingTaskTrigger = 'Start' | 'Complete' | 'Skip';

export interface HousekeepingTask {
  // Backend fields
  id: number;
  roomId: number;
  roomNumber?: string;
  assignedToUserId?: number;
  assignedToName?: string;
  taskType?: string;
  priority?: string;
  notes?: string;
  state?: HousekeepingTaskState;
  availableTriggers?: HousekeepingTaskTrigger[];
  scheduledAt?: string;
  completedAt?: string;
  tenantId?: number;
  createdBy?: string;
  creationDate?: string;
  // UI shims
  status: HousekeepingTaskStatus;
  scheduledDate?: string;
  createdAt: string;
}

// Backend API wrappers
export interface ApiResponse<T> {
  status: boolean;
  statusCode?: string;
  message?: string;
  data: T;
}

export interface PageApiResponse<T> {
  status: boolean;
  statusCode?: string;
  message?: string;
  data: T;
  dataCount?: number;
  pageNumber: number;
  pageSize: number;
  totalPageCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Dashboard
export interface DashboardStats {
  totalRooms: number;
  availableRooms: number;
  occupiedRooms: number;
  activeReservations: number;
  totalGuests: number;
  revenueThisMonth: number;
  occupancyRate: number;
  checkInsToday: number;
  checkOutsToday: number;
}

export interface OccupancyDataPoint {
  date: string;
  reservations: number;
  checkIns: number;
  checkOuts: number;
}

// Theme
export type Theme = 'light' | 'dark';
