import React from 'react';
import { CartesianGrid, Line, LineChart as RechartsLineChart, Tooltip, XAxis, YAxis, Legend } from 'recharts';

interface Dataset {
  name: string;
  data: { hour: string; value: number }[];
  color: string;
}

interface LineChartProps {
  datasets: Dataset[];
}

const LineChart: React.FC<LineChartProps> = ({ datasets }) => {
  const mergedData = datasets.reduce((acc, dataset) => {
    dataset.data.forEach(item => {
      const existing = acc.find(d => d.hour === item.hour);
      if (existing) {
        existing[dataset.name] = item.value;
      } else {
        const newItem: any = { hour: item.hour };
        newItem[dataset.name] = item.value;
        datasets.forEach(ds => {
          if (ds.name !== dataset.name) {
            newItem[ds.name] = 0;
          }
        });
        acc.push(newItem);
      }
    });
    return acc;
  }, [] as any[]);

  mergedData.sort((a, b) => a.hour.localeCompare(b.hour));

  return (
    <RechartsLineChart
      style={{ width: '100%', aspectRatio: 1.618, maxHeight: 500, margin: 'auto' }}
      responsive
      data={mergedData}
    >
      <CartesianGrid stroke="#eee" strokeDasharray="5 5" />
      <XAxis dataKey="hour" />
      <YAxis
        width="auto"
        domain={[0, 'dataMax']}
        label={{ value: '', angle: -90, position: 'insideLeft' }}
        tickFormatter={(value: number) => {
          if (value === 0) return '0 B';
          const k = 1024;
          const sizes = ['B', 'KB', 'MB', 'GB'];
          const i = Math.floor(Math.log(value) / Math.log(k));
          return `${(value / Math.pow(k, i)).toFixed(0)} ${sizes[i]}`;
        }}
      />
      {datasets.map((dataset, index) => (
        <Line
          key={index}
          type="monotone"
          dataKey={dataset.name}
          stroke={dataset.color}
          strokeWidth={2}
          dot={{ r: 4 }}
          name={dataset.name}
        />
      ))}
      <Tooltip
        formatter={(value: number | undefined) => {
          if (value === undefined) return ['--', ''];
          if (value === 0) return ['0 B', ''];
          const k = 1024;
          const sizes = ['B', 'KB', 'MB', 'GB'];
          const i = Math.floor(Math.log(value) / Math.log(k));
          return [`${(value / Math.pow(k, i)).toFixed(2)} ${sizes[i]}`, ''];
        }}
        labelFormatter={(label) => `时间: ${label}`}
      />
      <Legend />
    </RechartsLineChart>
  );
};

export default LineChart;