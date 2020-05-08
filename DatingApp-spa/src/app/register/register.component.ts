import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  model:any = {};
  @Output() cancelRegister = new EventEmitter();

  constructor(private auth:AuthService) { }

  ngOnInit() {
  }

  register(){
    this.auth.register(this.model)
      .subscribe(next => {
        console.log("registered successfully");
      },
      error => {
        console.log("error in registration"+error)
      })

  }

  cancel(){
    this.cancelRegister.emit(false);
    console.log("cancel");
  }
}
