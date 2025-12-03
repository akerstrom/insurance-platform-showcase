export type InsuranceType = 'Car' | 'Pet' | 'Health';

export const InsuranceType = {
  Car: 'Car' as const,
  Pet: 'Pet' as const,
  Health: 'Health' as const,
} as const;

export interface Vehicle {
  vin: string;
  regnr: string;
  make: string;
  model: string;
  year: number;
}

export interface CustomerInsurance {
  id: string;
  pid: string;
  type: InsuranceType;
  status: string;
  premium: number;
  vehicle: Vehicle | null;
}

export interface ApiError {
  error: string;
  message: string;
}
