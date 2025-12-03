import type { Vehicle } from '../types/api';

interface VehicleDetailsProps {
  vehicle: Vehicle;
}

export function VehicleDetails({ vehicle }: VehicleDetailsProps) {
  return (
    <div>
      <div>
        <span>Vehicle Information</span>
      </div>

      <div>
        <div>
          <p>Registration Number</p>
          <div>
            <span>{vehicle.regnr}</span>
          </div>
        </div>

        <div>
          <p>Make & Model</p>
          <p>
            {vehicle.make} {vehicle.model}
          </p>
        </div>

        <div>
          <p>Model Year</p>
          <p>{vehicle.year}</p>
        </div>

        <div>
          <p>VIN</p>
          <p>{vehicle.vin}</p>
        </div>
      </div>
    </div>
  );
}
