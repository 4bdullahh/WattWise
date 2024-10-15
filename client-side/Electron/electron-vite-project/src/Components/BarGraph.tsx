import React from 'react';
import { BarChart } from '@mui/x-charts/BarChart';

interface BarGraphProps {
  readings: { date: string; reading: number }[];
}

export default function BarGraph({ readings }: BarGraphProps) {
  // Extract the dates and readings from the readings array
  const dates = readings.map((reading) => reading.date);
  const values = readings.map((reading) => reading.reading);

  return (
    <BarChart
      series={[
        {
          data: values,  // Use the readings as data points
        },
      ]}
      height={290}
      xAxis={[{ data: dates, scaleType: 'band' }]}  // Use the dates as the x-axis labels
      margin={{ top: 10, bottom: 30, left: 40, right: 10 }}
    />
  );
}
