import { Injectable } from '@angular/core';
import { MembershipLevel } from '../models';

@Injectable({
  providedIn: 'root'
})
export class MembershipService {
  getBadgeClass(level: MembershipLevel): string {
    switch (level) {
      case MembershipLevel.VIP:
        return 'bg-warning text-dark';
      case MembershipLevel.Premium:
        return 'bg-primary';
      case MembershipLevel.Standard:
        return 'bg-secondary';
      default:
        return 'bg-secondary';
    }
  }

  getLevelName(level: MembershipLevel): string {
    switch (level) {
      case MembershipLevel.VIP:
        return 'VIP';
      case MembershipLevel.Premium:
        return 'Premium';
      case MembershipLevel.Standard:
        return 'Standard';
      default:
        return 'Unknown';
    }
  }
}
