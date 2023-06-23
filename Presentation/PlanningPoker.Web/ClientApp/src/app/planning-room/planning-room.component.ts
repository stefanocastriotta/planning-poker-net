import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EstimateValue, PlanningRoom, PlanningRoomUser, ProductBacklogItem, ProductBacklogItemEstimate, ProductBacklogItemStatusEnum } from '../app.entities';
import { switchMap, tap } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthorizeService } from 'src/api-authorization/authorize.service';
import { Profile, User } from 'oidc-client';

@Component({
  selector: 'app-planning-room',
  templateUrl: './planning-room.component.html',
  styleUrls: ['./planning-room.component.css']
})
export class PlanningRoomComponent {

  productBacklogItemCreation: FormGroup;
  fb: FormBuilder = new FormBuilder();
  planningRoom: PlanningRoom;
  currentUserId: string;

  selectedProductBacklogItem: ProductBacklogItem | undefined;

  constructor(private route: ActivatedRoute, private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private authorizeService: AuthorizeService){}
 
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

    this.authorizeService.getUser().subscribe(
      (res) => {
        this.currentUserId = (res as Profile).sub;
      });

    this.productBacklogItemCreation = this.fb.group({
      description: ['', Validators.required]
    });
  }

  selectProductBacklogItem(productBacklogItem: ProductBacklogItem){
    this.http.put<ProductBacklogItem>(`${this.baseUrl}api/productbacklogitem/${productBacklogItem.id}`, {...productBacklogItem, statusId: ProductBacklogItemStatusEnum.Processing, status: undefined},
    { headers:{ 'content-type': 'application/json'}   }).pipe(
      switchMap(() => this.http.get<ProductBacklogItem[]>(`${this.baseUrl}api/planningroom/${this.planningRoom.id}/productBacklogItems`))
    ).subscribe(res => {
      this.planningRoom.productBacklogItem = res;
      this.selectedProductBacklogItem = this.planningRoom.productBacklogItem.find(p => p.id === productBacklogItem.id);
    });
  }

  deleteProductBacklogItem(productBacklogItem: ProductBacklogItem){
    
  }

  currentUserEstimated(){
    return this.selectedProductBacklogItem?.productBacklogItemEstimate.some(pe => pe.userId === this.currentUserId)??false;
  }

  estimated(user: PlanningRoomUser): boolean{
    return this.selectedProductBacklogItem?.productBacklogItemEstimate.some(pe => pe.userId === user.id)??false;
  }

  onSubmit() {
    this.http.post<ProductBacklogItem>(`${this.baseUrl}api/productbacklogitem`, 
      { ...this.productBacklogItemCreation.getRawValue(), 
        statusId: ProductBacklogItemStatusEnum.Inserted, 
        planningRoomId: this.planningRoom.id
      } as ProductBacklogItem,
      { headers:{ 'content-type': 'application/json'}   }).subscribe(
        (res) => {
          this.planningRoom.productBacklogItem.push(res);
        }
      )
  }

  
  get processingStatus(): number {
    return ProductBacklogItemStatusEnum.Processing
  }

  registerEstimate(estimate: EstimateValue) {
    this.http.post<ProductBacklogItemEstimate>(`${this.baseUrl}api/productbacklogitemestimate`, 
      { estimateValueId: estimate.id,
        productBacklogItemId: this.selectedProductBacklogItem?.id
      } as ProductBacklogItemEstimate,
      { headers:{ 'content-type': 'application/json'}   }).subscribe(
        (res) => {
          this.selectedProductBacklogItem?.productBacklogItemEstimate.push(res);
        }
      )
  }

  public errorHandling = (control: string, error: string) => {
    return this.productBacklogItemCreation.controls[control].hasError(error);
  }
}
