export interface ShareToken {
  id: number;
  travelPlanId: number;
  token: string;
  accessType: string;
  expiresAt: string;
  createdAt?: string;
  isDeleted?: boolean;
}

export interface CreateShareToken {
  accessType: string;
  expiresAt: string; // ISO date string - matches backend CreateShareTokenDto.ExpiresAt
}