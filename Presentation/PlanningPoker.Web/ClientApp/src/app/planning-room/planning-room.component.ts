import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PlanningRoom, PlanningRoomUser, ProductBacklogItem, ProductBacklogItemStatusEnum } from '../app.entities';
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

  selectedProductBacklogItem: ProductBacklogItem | undefined;

  constructor(private route: ActivatedRoute, private http: HttpClient, @Inject('BASE_URL') private baseUrl: string){}
 
  ngOnInit() {
    this.route.params.subscribe(params => {

      this.http.post(`${this.baseUrl}api/planningroom/${params['id']}/registeruser`, {},
      { headers:{ 'content-type': 'application/json'}   } )
        .pipe(switchMap(() => this.http.get<PlanningRoom>(`${this.baseUrl}api/planningroom/${params['id']}`)
          .pipe(tap((planningRoom) => this.planningRoom = planningRoom)))
      )
      .subscribe((res: PlanningRoom) => {
        this.selectedProductBacklogItem = res.productBacklogItem.sort(p => p.id).find(p => p.status.id === ProductBacklogItemStatusEnum.Processing);
      })
    });

    this.productBacklogItemCreation = this.fb.group({
      description: ['', Validators.required]
    });
  }

  selectProductBacklogItem(productBacklogItem: ProductBacklogItem){
    this.http.post<ProductBacklogItem>(`${this.baseUrl}api/planningroom/${this.planningRoom.id}/productbacklogitem`, {...productBacklogItem, statusId: ProductBacklogItemStatusEnum.Processing, status: null},
    { headers:{ 'content-type': 'application/json'}   }).subscribe(
     (res) => {
       let index = this.planningRoom.productBacklogItem.findIndex(item => item.id === productBacklogItem.id);
       this.planningRoom.productBacklogItem[index] = res;
       this.selectedProductBacklogItem = res;
     }
    )
  }

  deleteProductBacklogItem(productBacklogItem: ProductBacklogItem){
    
  }

  estimated(user: PlanningRoomUser): boolean{
    return this.selectedProductBacklogItem?.productBacklogItemEstimate.some(pe => pe.userId === user.id)??false;
  }

  onSubmit() {
    this.http.post<ProductBacklogItem>(`${this.baseUrl}api/planningroom/${this.planningRoom.id}/productbacklogitem`, this.productBacklogItemCreation.getRawValue(),
    { headers:{ 'content-type': 'application/json'}   }).subscribe(
     (res) => {
       this.planningRoom.productBacklogItem.push(res);
     }
    )
  }

  
  get processingStatus(): number {
    return ProductBacklogItemStatusEnum.Processing
  }

  public errorHandling = (control: string, error: string) => {
    return this.productBacklogItemCreation.controls[control].hasError(error);
  }
}
