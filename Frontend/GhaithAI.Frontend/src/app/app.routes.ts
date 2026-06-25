import { Routes } from '@angular/router';
import { LandingComponent } from '../app/landing/landing';
import { authGuard } from './guards/auth-guard';
import { adminGuard } from './guards/admin-guard';
import { clinicianGuard } from './guards/clinician-guard';
import { ExerciseDetailsComponent } from './selfHelp/exercise-details-component/exercise-details-component';

export const routes: Routes = [

  { path: '', component: LandingComponent },

  {
    path: 'login',
    loadComponent: () => import('./auth/pages/login/login').then(m => m.LoginComponent)
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

  // ✅ User Dashboard
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./auth/pages/dashboard/dashboard').then(m => m.DashboardComponent)
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

  // ✅ Clinician Dashboard
  {
    path: 'clinician-dashboard',
    canActivate: [clinicianGuard],
    loadComponent: () =>
      import('./auth/pages/clinician-dashboard/clinician-dashboard')
        .then(m => m.ClinicianDashboardComponent)
  },

  // ✅ Session Room - الدكتور واليوزر بيدخلوا هنا
  {
    path: 'session-room/:bookingId',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./auth/session-room.component/session-room.component')
        .then(m => m.SessionRoomComponent)
  },

  // ✅ Transcript Review
  {
    path: 'clinical-session/:sessionId/transcript',
    canActivate: [clinicianGuard],
    loadComponent: () =>
      import('./auth/transcript-viewer.component/transcript-viewer.component')
        .then(m => m.TranscriptViewerComponent)
  },

  // ✅ Admin
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./auth/pages/admin-dashboard/admin-dashboard')
        .then(m => m.AdminDashboardComponent),
    children: [
      {
        path: 'self-help',
        loadComponent: () =>
          import('./features/AdminSelfHelp/components/self-help-dashboard/self-help-dashboard')
            .then(m => m.SelfHelpDashboardComponent)
      },
      {
        path: 'dashboard-home',
        loadComponent: () =>
          import('./features/admin-dashboard-home/admin-dashboard-home')
            .then(m => m.AdminDashboardHome)
      }
    ]
  },

  {
    path: 'admin/doctor/:id',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./auth/pages/doctor-detail/doctor-detail')
        .then(m => m.DoctorDetailComponent)
  },

  // ✅ Support / Chat
  {
    path: 'support',
    loadComponent: () => import('./support/layout').then(m => m.SupportLayout),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'chat', pathMatch: 'full' },
      {
        path: 'chat',
        loadComponent: () => import('./support/chat/page').then(m => m.ChatPage)
      },
      {
        path: 'chatsession',
        loadComponent: () => import('./support/chatsession/page').then(m => m.ChatSessionPage)
      },
      {
        path: 'crisis',
        loadComponent: () => import('./support/emergency/page').then(m => m.EmergencyPage)
      }
    ]
  },

{
  path: 'payment/success',
  canActivate: [authGuard],
  loadComponent: () =>
    import('./auth/payment-success.component/payment-success.component')
      .then(m => m.PaymentSuccessComponent)
},
{
  path: 'payment/cancel',
  redirectTo: '/dashboard'
},
{
  path: 'clinical-session/:sessionId/report',
  canActivate: [authGuard],
  loadComponent: () =>
    import('./auth/session-report/session-report')
      .then(m => m.SessionReportComponent)
},
{
  path: 'session/:sessionId/report',
  canActivate: [authGuard],
  loadComponent: () =>
    import('./auth/session-report/session-report')
      .then(m => m.SessionReportComponent)
},

  { path: '**', redirectTo: '' }
];
