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
    status: ProductBacklogItemStatus
}

export interface ProductBacklogItemStatus{
    id: number;
    label: string;
    order: number;
    isActive: boolean;
}