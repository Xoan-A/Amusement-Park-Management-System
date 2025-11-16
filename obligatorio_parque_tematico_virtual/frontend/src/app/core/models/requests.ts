import { AttractionType, MembershipLevel, TicketType, MaintenanceType } from './enums';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterVisitorRequest {
  name: string;
  lastName: string;
  email: string;
  password: string;
  birthDate?: string;
  membershipLevel?: MembershipLevel;
}

export interface CreateUserRequest {
  name: string;
  lastName: string;
  email: string;
  password: string;
  birthDate?: string;
  membershipLevel?: MembershipLevel;
  roles: string[];
}

export interface ModifyUserRequest {
  name?: string;
  lastName?: string;
  email?: string;
  birthDate?: string;
  membershipLevel?: MembershipLevel;
}

export interface AddRolesRequest {
  role: string;
}

export interface AttractionRequest {
  name: string;
  description: string;
  type: AttractionType;
  minAge: number;
  maxCapacity: number;
  currentCapacity?: number;
}

export interface EventRequest {
  name: string;
  date: string;
  hour: number;
  maxCapacity: number;
  cost: number;
  attractionIds: string[];
}

export interface PurchaseTicketRequest {
  visitorId: string;
  visitDate: string;
  type: TicketType;
  eventId?: string;
}

export interface RegisterEntryRequest {
  enterDate: string;
  userId: string;
  qr?: string;
  nfc?: string;
  eventId?: string;
}

export interface RegisterExitRequest {
  userId: string;
  exitDate: string;
}

export interface IncidentRequest {
  incident: string;
}

export interface AttractionsVisitsRequest {
  startDate: string;
  endDate: string;
}

export interface SetDateTimeRequest {
  dateTime: string;
}

export interface StrategyRequest {
  strategyName: string;
  n?: number;
}

export interface RewardRequest {
  name: string;
  description: string;
  pointsCost: number;
  availableQuantity: number;
  requiredMembershipLevel?: MembershipLevel;
}

export interface RedeemRewardRequest {
  rewardId: string;
}

export interface MaintenanceScheduleRequest {
  attractionId: string;
  scheduledDate: string;
  maintenanceType: MaintenanceType;
  description: string;
}

export interface MaintenanceRecordRequest {
  attractionId: string;
  maintenanceScheduleId?: string;
  performedDate: string;
  maintenanceType: MaintenanceType;
  description: string;
  durationMinutes: number;
  notes?: string;
}

export interface UpdateStatusRequest {
  status: string;
}
