export interface EstimateValueCategory{
    id: number;
    description: string;
    estimateValue: Array<EstimateValue>;
}

export interface EstimateValue{
    id: number;
    label: string;
    order: number;
    value: string;
}

export interface PlanningRoom{
    id: number;
    description: string;
    estimateValueCategoryId: number;
    estimateValueCategory: EstimateValueCategory;
    planningRoomUsers: PlanningRoomUser[];
    productBacklogItem: ProductBacklogItem[];
}

export interface PlanningRoomUser{
    id: string;
    userName: string;
}

export interface ProductBacklogItem{
    id: number;
    description: string;
    createDate: Date;
    status: ProductBacklogItemStatus;
    statusId: ProductBacklogItemStatusEnum;
    productBacklogItemEstimate: ProductBacklogItemEstimate[];
}

export interface ProductBacklogItemStatus{
    id: ProductBacklogItemStatusEnum;
    label: string;
    order: number;
    isActive: boolean;
}

export interface ProductBacklogItemEstimate{
    id: number;
    productBacklogItemId: number;
    userId: string;
    estimateValueId: number;
    createDate: Date;
}

export enum ProductBacklogItemStatusEnum{
    "Inserted" = 1,
    "Processing" = 2,
    "Completed" = 3
}