export enum AttractionType {
  RollerCoaster = 'RollerCoaster',
  Simulator = 'Simulator',
  Performance = 'Performance',
  InteractiveZone = 'InteractiveZone'
}

export enum MembershipLevel {
  Standard = 'Standard',
  Premium = 'Premium',
  VIP = 'VIP'
}

export enum TicketType {
  General = 0,
  EventSpecial = 1
}

export const Roles = {
  ADMINISTRATOR: 'Administrator',
  OPERATOR: 'Operator',
  VISITOR: 'Visitor'
} as const;

export type UserRole = typeof Roles[keyof typeof Roles];

export enum MaintenanceStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum MaintenanceType {
  Inspection = 'Inspection',
  Cleaning = 'Cleaning',
  Repair = 'Repair',
  SafetyCheck = 'SafetyCheck'
}

export enum ScoreOrigin {
  AttractionVisit = 'AttractionVisit',
  EventParticipation = 'EventParticipation',
  SpecialMission = 'SpecialMission',
  Redemption = 'Redemption',
  AdminAdjustment = 'AdminAdjustment',
  Other = 'Other'
}
