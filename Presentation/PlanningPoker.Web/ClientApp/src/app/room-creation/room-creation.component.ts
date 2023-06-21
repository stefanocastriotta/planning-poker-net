import { Component, Inject  } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { EstimateValueCategory, PlanningRoom } from '../app.entities';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-room-creation',
  templateUrl: './room-creation.component.html',
  styleUrls: ['./room-creation.component.css'],
})
export class RoomCreationComponent  {

roomCreation: FormGroup;
fb: FormBuilder = new FormBuilder();
estimateValueCategories$: Observable<Array<EstimateValueCategory>>;

  constructor(
    private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router
  ) { }

  ngOnInit(){
    this.roomCreation = this.fb.group({
      description: ['', Validators.required],
      estimateValueCategoryId: [null, Validators.required],
      newEstimateValueCategory: [null],
      newEstimateValueCategoryValues: [null]
    });
    this.estimateValueCategories$ = this.http.get<Array<EstimateValueCategory>>(this.baseUrl + 'api/estimateValue/categories');
    this.roomCreation.get('estimateValueCategoryId')?.valueChanges.subscribe(val => {
      if (val === -1) {
        this.roomCreation.controls['newEstimateValueCategory'].setValidators([Validators.required]);
        this.roomCreation.controls['newEstimateValueCategoryValues'].setValidators([Validators.required]);
      } else {
        this.roomCreation.controls['newEstimateValueCategory'].clearValidators();
        this.roomCreation.controls['newEstimateValueCategoryValues'].clearValidators();
      }
      this.roomCreation.controls['newEstimateValueCategory'].updateValueAndValidity();
      this.roomCreation.controls['newEstimateValueCategoryValues'].updateValueAndValidity();
    });
  }

  onSubmit() {
    this.http.post<PlanningRoom>(`${this.baseUrl}api/planningroom`, this.roomCreation.getRawValue(),
    { headers:{ 'content-type': 'application/json'}   }).subscribe(
     (res) => {
       this.router.navigate(['planning-room', res.id]);
     }
    )
 }

  public errorHandling = (control: string, error: string) => {
    return this.roomCreation.controls[control].hasError(error);
  }
}

