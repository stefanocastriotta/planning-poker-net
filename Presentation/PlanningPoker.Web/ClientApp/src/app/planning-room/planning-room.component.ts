import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PlanningRoom, ProductBacklogItem } from '../app.entities';
import { switchMap, tap } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-planning-room',
  templateUrl: './planning-room.component.html',
  styleUrls: ['./planning-room.component.css']
})
export class PlanningRoomComponent {

  productBacklogItemCreation: FormGroup;
  fb: FormBuilder = new FormBuilder();
  planningRoom: PlanningRoom;

  constructor(private route: ActivatedRoute, private http: HttpClient, @Inject('BASE_URL') private baseUrl: string){}
 
  ngOnInit() {
    this.route.params.subscribe(params => {

      this.http.post(`${this.baseUrl}api/planningroom/${params['id']}/registeruser`, {},
      { headers:{ 'content-type': 'application/json'}   } )
        .pipe(switchMap(() => this.http.get<PlanningRoom>(`${this.baseUrl}api/planningroom/${params['id']}`)
          .pipe(tap((planningRoom) => this.planningRoom = planningRoom)))
      )
      .subscribe()
    });

    this.productBacklogItemCreation = this.fb.group({
      description: ['', Validators.required]
    });
  }

  onSubmit() {
    this.http.post<ProductBacklogItem>(`${this.baseUrl}api/planningroom/${this.planningRoom.id}/addproductbacklogitem`, this.productBacklogItemCreation.getRawValue(),
    { headers:{ 'content-type': 'application/json'}   }).subscribe(
     (res) => {
       this.planningRoom.productBacklogItem.push(res);
     }
    )
  }

  public errorHandling = (control: string, error: string) => {
    return this.productBacklogItemCreation.controls[control].hasError(error);
  }
}
