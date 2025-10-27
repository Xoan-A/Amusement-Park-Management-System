import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Roles } from '../../../core/models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent implements OnInit {
  currentUser: any = null;
  isCollapsed = true;

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  logout(): void {
    this.authService.logout();
  }

  toggleNavbar(): void {
    this.isCollapsed = !this.isCollapsed;
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
}
