import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { Roles } from './core/models';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'datetime',
    loadComponent: () => import('./shared/components/datetime-management/datetime-management.component').then(m => m.DateTimeManagementComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: [Roles.ADMINISTRATOR] },
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/administrator/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'attractions',
        loadComponent: () => import('./features/administrator/attractions/attractions-list/attractions-list.component').then(m => m.AttractionsListComponent)
      },
      {
        path: 'attractions/new',
        loadComponent: () => import('./features/administrator/attractions/attraction-form/attraction-form.component').then(m => m.AttractionFormComponent)
      },
      {
        path: 'attractions/edit/:id',
        loadComponent: () => import('./features/administrator/attractions/attraction-form/attraction-form.component').then(m => m.AttractionFormComponent)
      },
      {
        path: 'events',
        loadComponent: () => import('./features/administrator/events/events-list/events-list.component').then(m => m.EventsListComponent)
      },
      {
        path: 'events/new',
        loadComponent: () => import('./features/administrator/events/event-form/event-form.component').then(m => m.EventFormComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/administrator/users/users-list/users-list.component').then(m => m.UsersListComponent)
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./features/administrator/reports/reports.component').then(
            (m) => m.ReportsComponent
          ),
      },
      {
        path: 'rewards',
        loadComponent: () => import('./features/administrator/rewards/rewards-list.component').then(m => m.RewardsListComponent)
      },
      {
        path: 'rewards/create',
        loadComponent: () => import('./features/administrator/rewards/reward-form.component').then(m => m.RewardFormComponent)
      },
      {
        path: 'rewards/edit/:id',
        loadComponent: () => import('./features/administrator/rewards/reward-form.component').then(m => m.RewardFormComponent)
      },
      {
        path: 'maintenance/schedules',
        loadComponent: () => import('./features/administrator/maintenance/schedule-list.component').then(m => m.ScheduleListComponent)
      },
      {
        path: 'maintenance/schedules/create',
        loadComponent: () => import('./features/administrator/maintenance/schedule-form.component').then(m => m.ScheduleFormComponent)
      },
      {
        path: 'score-history',
        loadComponent: () => import('./features/administrator/score-history/all-score-history.component').then(m => m.AllScoreHistoryComponent)
      },
      {
        path: 'plugins',
        loadComponent: () => import('./features/administrator/plugins/plugin-list.component').then(m => m.PluginListComponent)
      },
      {
        path: 'strategy',
        redirectTo: 'plugins',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'operator',
    canActivate: [authGuard, roleGuard],
    data: { roles: [Roles.OPERATOR] },
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/operator/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'entry-exit',
        loadComponent: () => import('./features/operator/entry-exit/entry-exit.component').then(m => m.EntryExitComponent)
      },
      {
        path: 'incidents',
        loadComponent: () => import('./features/operator/incidents/incidents.component').then(m => m.IncidentsComponent)
      },
      {
        path: 'maintenance',
        loadComponent: () => import('./features/operator/maintenance/operator-maintenance.component').then(m => m.OperatorMaintenanceComponent)
      }
    ]
  },
  {
    path: 'visitor',
    canActivate: [authGuard, roleGuard],
    data: { roles: [Roles.VISITOR] },
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/visitor/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'tickets',
        loadComponent: () => import('./features/visitor/tickets/my-tickets/my-tickets.component').then(m => m.MyTicketsComponent)
      },
      {
        path: 'tickets/purchase',
        loadComponent: () => import('./features/visitor/tickets/purchase-ticket/purchase-ticket.component').then(m => m.PurchaseTicketComponent)
      },
      {
        path: 'attractions',
        loadComponent: () => import('./features/visitor/browse/attractions/attractions.component').then(m => m.AttractionsComponent)
      },
      {
        path: 'events',
        loadComponent: () => import('./features/visitor/browse/events/events.component').then(m => m.EventsComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/visitor/profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'rewards',
        loadComponent: () => import('./features/visitor/rewards/browse-rewards.component').then(m => m.BrowseRewardsComponent)
      },
      {
        path: 'my-redemptions',
        loadComponent: () => import('./features/visitor/rewards/my-redemptions.component').then(m => m.MyRedemptionsComponent)
      },
      {
        path: 'score-history',
        loadComponent: () => import('./features/visitor/score-history/score-history.component').then(m => m.ScoreHistoryComponent)
      }
    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/components/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
  },
  { path: '**', redirectTo: '/login' }
];
