import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Roles } from '../../../core/models';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit {
  currentUser: any = null;
  activeRole: string | null = null;
  availableRoles: string[] = [];
  isCollapsed = false;

  @Output() collapsedChange = new EventEmitter<boolean>();

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.availableRoles = this.authService.getAvailableRoles();
    });

    this.authService.activeRole$.subscribe(role => {
      this.activeRole = role;
    });
  }

  logout(): void {
    this.authService.logout();
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
    this.collapsedChange.emit(this.isCollapsed);
  }

  switchRole(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const newRole = selectElement.value;

    if (newRole && newRole !== this.activeRole) {
      this.authService.switchRole(newRole);
    }
  }

  get isAdmin(): boolean {
    return this.authService.isAdministrator();
  }

  get isOperator(): boolean {
    return this.authService.isOperator();
  }

  get isVisitor(): boolean {
    return this.authService.isVisitor();
  }

  get hasMultipleRoles(): boolean {
    return this.authService.hasMultipleRoles();
  }

  get Roles() {
    return Roles;
  }

  getRoleName(role: string): string {
    switch (role) {
      case Roles.ADMINISTRATOR:
        return 'Administrator';
      case Roles.OPERATOR:
        return 'Operator';
      case Roles.VISITOR:
        return 'Visitor';
      default:
        return role;
    }
  }
}
