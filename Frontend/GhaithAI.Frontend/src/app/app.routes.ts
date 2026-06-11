import { Routes } from '@angular/router';
import { LandingComponent } from '../app/landing/landing';
import { authGuard } from './guards/auth-guard';
import { adminGuard } from './guards/admin-guard';
import { ExerciseDetailsComponent } from './selfHelp/exercise-details-component/exercise-details-component';

export const routes: Routes = [

  { path: '', component: LandingComponent },

  {
    path: 'login',
    loadComponent: () => import('./auth/pages/login/login').then(m => m.LoginComponent)
  },
  {
  path: 'admin',
  canActivate: [adminGuard],
  loadComponent: () =>
    import('./auth/pages/admin-dashboard/admin-dashboard')
      .then(m => m.AdminDashboardComponent)
},

  {
    path: 'register',
    loadComponent: () => import('./auth/pages/register/register').then(m => m.RegisterComponent)
  },

  // ✅ Clinician Register
  {
    path: 'register-clinician',
    loadComponent: () =>
      import('./auth/pages/register-clinician/register-clinician')
        .then(m => m.RegisterClinicianComponent)
  },

  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./auth/pages/dashboard/dashboard').then(m => m.DashboardComponent)
  },

  {
    path: 'dashboard/self-help/exercise/:id',
    component: ExerciseDetailsComponent
  },

  {
    path: 'support',
    loadComponent: () => import('./support/layout').then(m => m.SupportLayout),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'chat', pathMatch: 'full' },
      { path: 'chat', loadComponent: () => import('./support/chat/page').then(m => m.ChatPage) },
      { path: 'chatsession', loadComponent: () => import('./support/chatsession/page').then(m => m.ChatSessionPage) },
      { path: 'crisis', loadComponent: () => import('./support/emergency/page').then(m => m.EmergencyPage) }
    ]
  },

  { path: '**', redirectTo: '' }
];
