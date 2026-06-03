import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule
} from '@angular/forms';

import { CommonModule } from '@angular/common';

import { AuthService }
from '../../../services/auth';

@Component({
  selector:'app-profile',
  standalone:true,
  imports:[
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl:'./profile.html',
  styleUrl:'./profile.css'
})
export class ProfileComponent
implements OnInit{

  profileForm!:FormGroup;

  constructor(
    private fb:FormBuilder,
    private auth:AuthService
  ){}

  ngOnInit(){

    this.profileForm =
      this.fb.group({

        fullName:[''],

        preferredLanguage:['en'],

        memoryEnabled:[false]
      });

    this.loadProfile();
  }

  loadProfile(){

    this.auth
      .getProfile()
      .subscribe({

        next:(res:any)=>{

          this.profileForm
            .patchValue({

              fullName:
                res.fullName,

              preferredLanguage:
                res.preferredLanguage,

              memoryEnabled:
                res.memoryEnabled
            });
        }
      });
  }

  updateProfile(){

    this.auth
      .updateProfile(
        this.profileForm.value
      )
      .subscribe({

        next:()=>{

          alert(
            'Profile Updated'
          );
        }
      });
  }

  logout(){

    localStorage.removeItem(
      'token'
    );

    location.href='/login';
  }
}