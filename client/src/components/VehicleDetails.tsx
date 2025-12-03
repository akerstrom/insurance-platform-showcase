import { Car } from 'lucide-react';
import type { Vehicle } from '../types/api';

interface VehicleDetailsProps {
  vehicle: Vehicle;
}

export function VehicleDetails({ vehicle }: VehicleDetailsProps) {
  return (
    <div className="mt-4 rounded-lg border border-indigo-100 bg-indigo-50 p-4">
      <div className="mb-3 flex items-center gap-2">
        <Car className="h-4 w-4 text-indigo-600" />
        <span className="text-sm font-medium text-indigo-800">Vehicle Information</span>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <p className="text-xs uppercase tracking-wide text-slate-500">Registration Number</p>
          <div className="mt-1">
            <span className="inline-block rounded bg-white px-2 py-1 font-mono text-sm font-semibold text-slate-900">
              {vehicle.regnr}
            </span>
          </div>
        </div>

        <div>
          <p className="text-xs uppercase tracking-wide text-slate-500">Make & Model</p>
          <p className="mt-1 text-sm font-medium text-slate-900">
            {vehicle.make} {vehicle.model}
          </p>
        </div>

        <div>
          <p className="text-xs uppercase tracking-wide text-slate-500">Model Year</p>
          <p className="mt-1 text-sm font-medium text-slate-900">{vehicle.year}</p>
        </div>

        <div>
          <p className="text-xs uppercase tracking-wide text-slate-500">VIN</p>
          <p className="mt-1 font-mono text-xs text-slate-700">{vehicle.vin}</p>
        </div>
      </div>
    </div>
  );
}
