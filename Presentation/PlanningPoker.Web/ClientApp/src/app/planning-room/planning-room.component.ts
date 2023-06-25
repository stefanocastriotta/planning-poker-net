import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EstimateValue, PlanningRoom, PlanningRoomUser, ProductBacklogItem, ProductBacklogItemEstimate, ProductBacklogItemStatusEnum, RegisterEstimateResult } from '../app.entities';
import { switchMap, tap } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthorizeService } from 'src/api-authorization/authorize.service';
import { Profile, User } from 'oidc-client';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-planning-room',
  templateUrl: './planning-room.component.html',
  styleUrls: ['./planning-room.component.css']
})
export class PlanningRoomComponent {

  productBacklogItemCreation: FormGroup;
  fb: FormBuilder = new FormBuilder();
  private hubConnection: signalR.HubConnection | undefined;

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

        this.authorizeService.getAccessToken().subscribe(
          (res) => {
            this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.baseUrl}planningRoomHub?token=${res}`, { accessTokenFactory: () => res } as signalR.IHttpConnectionOptions)
            .configureLogging(signalR.LogLevel.Debug)
            .build();
       
            this.hubConnection.start()
            .then(() => this.hubConnection?.invoke('AddToGroup', this.planningRoom.id))
            .catch((err) => console.error(err.toString()));
        
            this.hubConnection.on('UserJoined', (user: PlanningRoomUser) => {
              this.planningRoom.planningRoomUsers.push(user);
            });

            this.hubConnection.on('ProductBacklogItemInserted', (productBacklogItem: ProductBacklogItem) => {
              this.planningRoom.productBacklogItem.push(productBacklogItem);
            });

            this.hubConnection.on('ProductBacklogItemUpdated', (updatedId: number, productBacklogItems: ProductBacklogItem[]) => {
              this.planningRoom.productBacklogItem = productBacklogItems;
              this.selectedProductBacklogItem = this.planningRoom.productBacklogItem.find(p => p.id === updatedId);
            });

            this.hubConnection.on('ProductBacklogItemEstimated', (estimate: ProductBacklogItemEstimate, productBacklogItem: ProductBacklogItem) => {
              if (productBacklogItem.statusId !== this.selectedProductBacklogItem?.statusId){
                this.selectedProductBacklogItem!.statusId = productBacklogItem.statusId;
                this.selectedProductBacklogItem!.status = productBacklogItem.status;
              }
              this.selectedProductBacklogItem?.productBacklogItemEstimate.push(estimate);
            });

          }
        )
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
    this.http.put<ProductBacklogItem[]>(`${this.baseUrl}api/productbacklogitem/${productBacklogItem.id}`, {...productBacklogItem, statusId: ProductBacklogItemStatusEnum.Processing, status: undefined},
    { headers:{ 'content-type': 'application/json'}   })
    .subscribe(res => {
      this.planningRoom.productBacklogItem = res;
      this.selectedProductBacklogItem = this.planningRoom.productBacklogItem.find(p => p.id === productBacklogItem.id);
    });
  }

  deleteProductBacklogItem(productBacklogItem: ProductBacklogItem){
    
  }

  cardSelected(estimateValue: EstimateValue):boolean{
    return this.selectedProductBacklogItem?.productBacklogItemEstimate.some(pe => pe.userId === this.currentUserId && pe.estimateValueId === estimateValue.id) ?? false;
  }

  estimated(user: PlanningRoomUser): boolean{
    return this.selectedProductBacklogItem?.productBacklogItemEstimate.some(pe => pe.userId === user.id) ?? false;
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
    if (!this.selectedProductBacklogItem?.productBacklogItemEstimate.some(pe => pe.userId === this.currentUserId)){
      this.http.post<RegisterEstimateResult>(`${this.baseUrl}api/productbacklogitemestimate`, 
        { estimateValueId: estimate.id,
          productBacklogItemId: this.selectedProductBacklogItem?.id
        } as ProductBacklogItemEstimate,
        { headers:{ 'content-type': 'application/json'}   }).subscribe(
          (res) => {
            if (res.productBacklogItem.statusId !== this.selectedProductBacklogItem?.statusId){
              this.selectedProductBacklogItem!.statusId = res.productBacklogItem.statusId;
              this.selectedProductBacklogItem!.status = res.productBacklogItem.status;
            }
            this.selectedProductBacklogItem?.productBacklogItemEstimate.push(res.estimate);
          }
        )
    }
  }

  public errorHandling = (control: string, error: string) => {
    return this.productBacklogItemCreation.controls[control].hasError(error);
  }
}
