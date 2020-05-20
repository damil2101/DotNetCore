import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker/public_api';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  user:User;
  @Output() cancelRegister = new EventEmitter();
  registerForm:FormGroup; //using reaactive forms
  bsConfig:Partial<BsDatepickerConfig>; // partial class makes all properties in class optional

  constructor(private auth:AuthService, private alertify:AlertifyService,private fb:FormBuilder,private router:Router) { }

  ngOnInit() {
    //creating form control using form group
    // this.registerForm = new FormGroup({
    //   username: new FormControl('',Validators.required),
    //   password:new FormControl('',[Validators.required,Validators.minLength(6),Validators.maxLength(12)]),
    //   confirmPassword : new FormControl('',Validators.required)
    // }, this.passwordMatchValidator);
    this.bsConfig = {
      containerClass:'theme-orange'
    };
    this.createRegisterForm();
  }
  passwordMatchValidator(g:FormGroup){
    return g.get('password').value === g.get('confirmPassword').value ? null : {'mismatch':true};
  }
//create form using form builder
  createRegisterForm(){
    this.registerForm = this.fb.group({
      gender:['male'],
      username:['',Validators.required],
      knownAs:['',Validators.required],
      DOB:[null,Validators.required],
      city:['',Validators.required],
      country:['',Validators.required],
      password:['',[Validators.required,Validators.minLength(6),Validators.maxLength(12)]],
      confirmPassword:['',Validators.required]
    },{
      validator:this.passwordMatchValidator});
  }
  register(){
    if(this.registerForm.valid){
      this.user = Object.assign({},this.registerForm.value);
      this.auth.register(this.user)
      .subscribe(next => {
        this.alertify.success("Registration successfull");
      },error => {
        this.alertify.error(error);
      },() => {
        this.auth.login(this.user).subscribe(()=>{
          this.router.navigate(['/members']);
        })
      })
    }

  }

  cancel(){
    this.cancelRegister.emit(false);
    this.alertify.message("dismissed");
  }
}
