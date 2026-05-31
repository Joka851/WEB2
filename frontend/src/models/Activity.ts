export interface Activity {
  id: number;
  travelPlanId: number;
  name: string;
  date: string;
  time: string;
  location: string;
  description: string;
  estimatedCost: number;
  status: string;
}

export interface CreateActivity {
  name: string;
  date: string;
  time: string;
  location: string;
  description: string;
  estimatedCost: number;
  status: string;
}