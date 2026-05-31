export interface Expense {
  id: number;
  travelPlanId: number;
  name: string;
  category: string;
  amount: number;
  date: string;
  description: string;
}

export interface CreateExpense {
  name: string;
  category: string;
  amount: number;
  date: string;
  description: string;
}