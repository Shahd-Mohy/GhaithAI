import { Component } from '@angular/core';

import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  Router,
  RouterLink
} from '@angular/router';

import {
  SocialAuthService,
  GoogleLoginProvider,
  SocialUser
} from '@abacritt/angularx-social-login';

import { AuthService } from '../../../services/auth';
import { TokenService } from '../../../services/token';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {

  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private token: TokenService,
    // private socialAuth: SocialAuthService,
    private router: Router
  ) {

    this.loginForm = this.fb.group({

      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ],

      password: [
        '',
        Validators.required
      ]
    });
  }

  login(): void {

    if (this.loginForm.invalid)
      return;

    this.auth
      .login(this.loginForm.value)
      .subscribe({

        next: (res: any) => {

          this.token.saveToken(
            res.token
          );

          this.router.navigate([
            '/dashboard'
          ]);
        },

        error: (err) => {

          console.error(err);

          alert('Login Failed');
        }
      });
  }

  // googleLogin(): void {

  //   this.socialAuth
  //     .signIn(
  //       GoogleLoginProvider.PROVIDER_ID
  //     )
  //     .then((user: SocialUser) => {

  //       console.log(
  //         'Google User:',
  //         user
  //       );

  //       this.auth
  //         .googleLogin({

  //           idToken:
  //             user.idToken

  //         })
  //         .subscribe({

  //           next: (res: any) => {

  //             this.token.saveToken(
  //               res.token
  //             );

  //             this.router.navigate([
  //               '/profile'
  //             ]);
  //           },

  //           error: (err) => {

  //             console.error(err);

  //             alert(
  //               'Google Login Failed'
  //             );
  //           }
  //         });
  //     })
  //     .catch(error => {

  //       console.error(
  //         'Google Sign In Error',
  //         error
  //       );
  //     });
  // }
}
