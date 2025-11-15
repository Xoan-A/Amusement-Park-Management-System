import { AttractionType, MembershipLevel, TicketType, MaintenanceStatus, ScoreOrigin } from './enums';

export interface LoginResponse {
  token: string;
  id: string;
  email: string;
  roles: string[];
  name: string;
}

export interface RegisterResponse {
  id: string;
  email: string;
  message: string;
}

export interface MessageResponse {
  message: string;
}

export interface DateTimeResponse {
  currentDateTime: string;
}

export interface UserResponse {
  id: string;
  name: string;
  lastName: string;
  email: string;
  birthDate?: string;
  membershipLevel?: MembershipLevel;
  userRoles: string[];
  score: number;
}

export interface AttractionResponse {
  id: string;
  name: string;
  description: string;
  type: AttractionType;
  minAge: number;
  maxCapacity: number;
  currentCapacity: number;
  isActive: boolean;
  incidents?: string[];
}

export interface AllAttractionsResponse {
  attractions: AttractionResponse[];
}

export interface CreateAttractionResponse {
  id: string;
  message: string;
}

export interface CapacityResponse {
  attractionId: string;
  maxCapacity: number;
  currentCapacity: number;
  availableCapacity: number;
}

export interface EventResponse {
  id: string;
  name: string;
  date: string;
  hour: number;
  maxCapacity: number;
  currentCapacity: number;
  cost: number;
  attractions: AttractionResponse[];
}

export interface CreateEventResponse {
  id: string;
  message: string;
}

export interface TicketResponse {
  id: string;
  visitorId: string;
  visitorName: string;
  visitorLastName: string;
  purchaseDate: string;
  visitDate: string;
  type: TicketType;
  qrCode: string;
  eventId?: string;
}

export interface DateTimeResponse {
  currentDateTime: string;
}

export interface StrategyResponse {
  name: string;
}

export interface TopTenResponse {
  visitors: TopVisitor[];
}

export interface TopVisitor {
  visitorId: string;
  name: string;
  lastName: string;
  email: string;
  score: number;
}

export interface AttractionsVisitResponse {
  startDate: string;
  endDate: string;
  attractionVisits: AttractionVisitDetail[];
}

export interface AttractionVisitDetail {
  attractionId: string;
  attractionName: string;
  totalVisits: number;
  averageStayMinutes: number;
}

export interface RewardResponse {
  id: string;
  name: string;
  description: string;
  pointsCost: number;
  availableQuantity: number;
  requiredMembershipLevel?: MembershipLevel;
  isAvailable: boolean;
}

export interface AllRewardsResponse {
  rewards: RewardResponse[];
}

export interface CreateRewardResponse {
  id: string;
  message: string;
}

export interface RedemptionHistoryResponse {
  id: string;
  visitorId: string;
  rewardId: string;
  redeemedAt: string;
  pointsSpent: number;
  rewardName?: string;
  visitorName?: string;
}

export interface AllRedemptionsResponse {
  redemptions: RedemptionHistoryResponse[];
}

export interface MaintenanceScheduleResponse {
  id: string;
  attractionId: string;
  attractionName: string;
  scheduledDate: string;
  description: string;
  estimatedDuration: number;
  status: string;
  isOverdue: boolean;
}

export interface ScoreHistoryResponse {
  id: string;
  visitorId: string;
  visitorName?: string;
  points: number;
  origin: ScoreOrigin;
  strategyName: string;
  relatedEntityId?: string;
  createdAt: string;
}

export interface PluginResponse {
  name: string;
  description: string;
  author: string;
  version: string;
}
