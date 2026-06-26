import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  importProvidersFrom
} from '@angular/core';

import {
  provideRouter
} from '@angular/router';

import {
  provideHttpClient,
  withInterceptors
} from '@angular/common/http';

import {
  provideAnimations
} from '@angular/platform-browser/animations';

import {
  SocialAuthServiceConfig,
  GoogleLoginProvider,
  SocialLoginModule
} from '@abacritt/angularx-social-login';

import { BrowserAnimationsModule }
  from '@angular/platform-browser/animations';


import { NgxSpinnerModule }
  from 'ngx-spinner';

import { routes } from './app.routes';

import {
  authInterceptor
} from './interceptors/auth-interceptor';

import { errorInterceptor } from './interceptors/error.interceptor';
export const appConfig: ApplicationConfig = {

  providers: [

    provideBrowserGlobalErrorListeners(),

    provideRouter(routes),

    provideAnimations(),

    provideHttpClient(
      withInterceptors([
        authInterceptor,
        errorInterceptor
      ])
    ),

    importProvidersFrom(
      BrowserAnimationsModule,
      NgxSpinnerModule,
      SocialLoginModule
    ),

    {
      provide: 'SocialAuthServiceConfig' as any,

      useValue: {
        autoLogin: false,

        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,

            provider: new GoogleLoginProvider(
              '617722058922-o8hffsc1rv8gqos8shl57gogjig6uh7o.apps.googleusercontent.com'
            )
          }
        ],

        onError: (err: any) => {
          console.error(err);
        }
      }

    }


  ]
};
