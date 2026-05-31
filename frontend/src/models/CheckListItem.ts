export interface ChecklistItem {
  id: number;
  travelPlanId: number;
  name: string;
  isCompleted: boolean;
}

export interface CreateChecklistItem {
  name: string;
}