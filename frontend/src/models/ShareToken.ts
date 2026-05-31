export interface ShareToken {
  id: number;
  travelPlanId: number;
  token: string;
  accessType: string;
  expiresAt: string;
}

export interface CreateShareToken {
  accessType: string;
}