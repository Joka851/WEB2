export interface TravelPlan {
  id: number;
  userId: number;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  budget: number;
  notes: string;
  createdAt: string;
  destinations?: any[];
  activities?: any[];
  checklistItems?: any[];
}

export interface CreateTravelPlan {
  userId: number;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  budget: number;
  notes: string;
}