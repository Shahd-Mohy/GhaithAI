import { Routes } from '@angular/router';

import { LoginComponent }
from './auth/pages/login/login';

import { RegisterComponent }
from './auth/pages/register/register';

import { ProfileComponent }
from './auth/pages/profile/profile';

import { DashboardComponent }
from './auth/pages/dashboard/dashboard';

import { authGuard }
from './guards/auth-guard';

import { LandingComponent }
from '../app/landing/landing';

export const routes: Routes = [

  {
    path:'',
    component: LandingComponent
  },

  {
    path:'login',
    loadComponent:() =>
      import('./auth/pages/login/login')
      .then(m => m.LoginComponent)
  },

  {
    path:'register',
    loadComponent:() =>
      import('./auth/pages/register/register')
      .then(m => m.RegisterComponent)
  },

  {
    path:'profile',
    canActivate:[authGuard],
    loadComponent:() =>
      import('./auth/pages/profile/profile')
      .then(m => m.ProfileComponent)
  },

  {
    path:'dashboard',
    loadComponent:() =>
      import('./auth/pages/dashboard/dashboard')
      .then(m => m.DashboardComponent)
  },

  {
    path:'**',
    redirectTo:''
  }
];
