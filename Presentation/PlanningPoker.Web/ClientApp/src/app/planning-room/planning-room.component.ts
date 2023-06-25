import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EstimateValue, PlanningRoom, PlanningRoomUser, ProductBacklogItem, ProductBacklogItemEstimate, ProductBacklogItemStatusEnum, RegisterEstimateResult } from '../app.entities';
import { switchMap, tap } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthorizeService } from 'src/api-authorization/authorize.service';
import { Profile, User } from 'oidc-client';
import * as signalR from '@microsoft/signalr';
import { ChartData, ChartOptions, ChartType } from 'chart.js';

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

  public estimateChartData: ChartData<'doughnut'>;
  public estimateChartType: ChartType = 'doughnut';

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
              if (this.selectedProductBacklogItem?.statusId === ProductBacklogItemStatusEnum.Completed){
                this.setChart();
              }
            });

            this.hubConnection.on('ProductBacklogItemEstimated', (estimate: ProductBacklogItemEstimate, productBacklogItem: ProductBacklogItem) => {
              this.selectedProductBacklogItem?.productBacklogItemEstimate.push(estimate);
              if (productBacklogItem.statusId !== this.selectedProductBacklogItem?.statusId){
                this.selectedProductBacklogItem!.statusId = productBacklogItem.statusId;
                this.selectedProductBacklogItem!.status = productBacklogItem.status;
                this.setChart();
              }
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
    this.http.put<ProductBacklogItem[]>(`${this.baseUrl}api/productbacklogitem/${productBacklogItem.id}`, {...productBacklogItem, statusId: productBacklogItem.statusId === ProductBacklogItemStatusEnum.Completed ? productBacklogItem.statusId : ProductBacklogItemStatusEnum.Processing, status: undefined},
    { headers:{ 'content-type': 'application/json'}   })
    .subscribe(res => {
      this.planningRoom.productBacklogItem = res;
      this.selectedProductBacklogItem = this.planningRoom.productBacklogItem.find(p => p.id === productBacklogItem.id);
      if (this.selectedProductBacklogItem?.statusId === ProductBacklogItemStatusEnum.Completed){
        this.setChart();
      }
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

  get completedStatus(): number {
    return ProductBacklogItemStatusEnum.Completed
  }

  registerEstimate(estimate: EstimateValue) {
    if (!this.selectedProductBacklogItem?.productBacklogItemEstimate.some(pe => pe.userId === this.currentUserId)){
      this.http.post<RegisterEstimateResult>(`${this.baseUrl}api/productbacklogitemestimate`, 
        { estimateValueId: estimate.id,
          productBacklogItemId: this.selectedProductBacklogItem?.id
        } as ProductBacklogItemEstimate,
        { headers:{ 'content-type': 'application/json'}   }).subscribe(
          (res) => {
            this.selectedProductBacklogItem?.productBacklogItemEstimate.push(res.estimate);
            if (res.productBacklogItem.statusId !== this.selectedProductBacklogItem?.statusId){
              this.selectedProductBacklogItem!.statusId = res.productBacklogItem.statusId;
              this.selectedProductBacklogItem!.status = res.productBacklogItem.status;
              this.setChart();
            }
          }
        )
    }
  }

  public errorHandling = (control: string, error: string) => {
    return this.productBacklogItemCreation.controls[control].hasError(error);
  }

  private setChart(){
    let map: {[key: string]: number} = {};
    this.selectedProductBacklogItem?.productBacklogItemEstimate.forEach(element => {
      let ev = this.planningRoom.estimateValueCategory.estimateValue.find(e => e.id === element.estimateValueId);
      if (ev) {
        if (!map[ev?.label])
        {
          map[ev.label] = 1;
        }
        else{
          map[ev.label] += 1;
        }
      }
    })

    this.estimateChartData = {
      labels: Object.keys(map),
      datasets: [
        { data: Object.values(map) }
      ]
    }
  }
}

