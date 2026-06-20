import { Routes } from '@angular/router';
import { LandingComponent } from '../app/landing/landing';
import { authGuard } from './guards/auth-guard';
import { adminGuard } from './guards/admin-guard';
import { clinicianGuard } from './guards/clinician-guard';
import { ExerciseDetailsComponent } from './selfHelp/exercise-details-component/exercise-details-component';
// import { ForgotPasswordComponent } from './pages/auth/forgot-password/forgot-password';
// import { ResetPasswordComponent } from './pages/auth/reset-password/reset-password';
import { ProfessionalsComponent } from './auth/pages/professionals/professionals';

export const routes: Routes = [

  { path: '', component: LandingComponent },

  {
    path: 'login',
    loadComponent: () => import('./auth/pages/login/login').then(m => m.LoginComponent)
  },

  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./auth/pages/admin-dashboard/admin-dashboard')
      .then(m => m.AdminDashboardComponent),
    children: [
      {
        path: 'self-help',
        loadComponent: () => import('./features/AdminSelfHelp/components/self-help-dashboard/self-help-dashboard')
          .then(m => m.SelfHelpDashboardComponent)
      },
      {
        path: 'dashboard-home',
        loadComponent: () => import('./features/admin-dashboard-home/admin-dashboard-home')
          .then(m => m.AdminDashboardHome)
      }
    ]
  },

  {
    path: 'register',
    loadComponent: () => import('./auth/pages/register/register').then(m => m.RegisterComponent)
  },

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
    path: 'clinician-dashboard',
    canActivate: [clinicianGuard],
    loadComponent: () => import('./auth/pages/clinician-dashboard/clinician-dashboard')
      .then(m => m.ClinicianDashboardComponent)
  },

  {
    path: 'dashboard/self-help/exercise/:id',
    component: ExerciseDetailsComponent
  },
  {
  path: 'dashboard/professionals',
  loadComponent: () =>
    import('./auth/pages/professionals/professionals')
      .then(m => m.ProfessionalsComponent)
},

  // ✅ Admin routes
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./auth/pages/admin-dashboard/admin-dashboard')
        .then(m => m.AdminDashboardComponent)
  },

  {
    path: 'admin/doctor/:id',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./auth/pages/doctor-detail/doctor-detail')
        .then(m => m.DoctorDetailComponent)
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

{
  path: 'forgot-password',
  loadComponent: () =>
    import('./auth/pages/forgot-password/forgot-password')
      .then(m => m.ForgotPasswordComponent)
},
{
  path: 'reset-password',
  loadComponent: () =>
    import('./auth/pages/reset-password/reset-password')
      .then(m => m.ResetPasswordComponent)
},

  { path: '**', redirectTo: '' }
];
