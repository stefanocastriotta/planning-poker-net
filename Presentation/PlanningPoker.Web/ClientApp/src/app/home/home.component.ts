import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  constructor(private router: Router){}

  startForm: FormGroup;
  fb: FormBuilder = new FormBuilder();

  ngOnInit(){
    this.startForm = this.fb.group({
      roomId: ['', Validators.required]
    });
  }

  onSubmit() {
    this.router.navigate(['planning-room', this.startForm.value.roomId])
  }

  public errorHandling = (control: string, error: string) => {
    return this.startForm.controls[control].hasError(error);
  }
}
