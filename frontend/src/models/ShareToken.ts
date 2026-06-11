export interface ShareToken {
  id: number;
  travelPlanId: number;
  token: string;
  accessType: string;
  expiresAt: string;
  createdAt?: string;    // OPCIONALNO
  isDeleted?: boolean;   // OPCIONALNO
}

export interface CreateShareToken {
  accessType: string;
  expiresInDays?: number;  // DODATO - opciono, backend ce koristiti default ako nije poslato
}