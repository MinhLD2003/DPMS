import { Tag } from 'antd';

// Generate colors for matrix cells based on severity
export const getRiskColor = (severity: number): string => {
    if (severity >= 40) return '#f5222d'; // Red
    if (severity >= 20) return '#fa8c16'; // Orange
    if (severity >= 10) return '#fadb14'; // Yellow
    if (severity >= 3) return '#52c41a';  // Green
    return '#13c2c2';                     // Cyan - Very Low
};

// Then modify getCellColor to use getRiskColor
export const getCellColor = (impact: number, probability: number): string => {
    return getRiskColor(impact * probability);
};

export const getImpactColor = (i: number): string => {
    if (i == 16) return '#f5222d'; // Red
    if (i == 8) return '#fa8c16'; // Orange
    if (i == 4) return '#fadb14'; // Yellow
    if (i == 2) return '#52c41a';  // Green
    return '#13c2c2';                     // Cyan - Very Low
};

export const getProbabilityColor = (p: number): string => {
    if (p == 5) return '#f5222d'; // Red
    if (p == 4) return '#fa8c16'; // Orange
    if (p == 3) return '#fadb14'; // Yellow
    if (p == 2) return '#52c41a';  // Green
    return '#13c2c2';                     // Cyan - Very Low
};
export const getTextColor = (bgColor: string): string => {
    return ['#f5222d', '#fa8c16'].includes(bgColor) ? 'white' : 'black';
};
export const getPriorityLabel = (value: number): JSX.Element => {
    if (value >= 40) return <Tag color="red" > Extreme </Tag>;
    if (value >= 20) return <Tag color="orange" > High </Tag>;
    if (value >= 10) return <Tag color="gold" > Medium </Tag>;
    if (value >= 3) return <Tag color="green" > Low </Tag>;
    return <Tag color="cyan" > Negligible </Tag>;
};

export const getScoreStepPercent = (score: number) => {
    if (score >= 40) return 100;
    if (score >= 20) return 80;
    if (score >= 10) return 60;
    if (score >= 3) return 40;
    return 20;
};
export const getImpactStepPercent = (impact: number) => {
    if (impact == 16) return 100;
    if (impact == 8) return 80;
    if (impact == 4) return 60;
    if (impact == 2) return 40;
    return 20;
};
// Function to get tag color for strategy
export const getStrategyColor = (strategy: number) => {
    const colors = ['#228B22', '#32CD32', '#90EE90', '#98FF98', '#F0FFF0'];
    //return colors[strategy] || '#1890ff';
    return 'cyan';
};

// Function to get tag color for category
export const getCategoryColor = (category: number) => {
    const colors = ['#0429a3', '#4169E1', '#0000CD', '#1E90FF', '#87CEEB', '#87CEFA', '#B0E0E6'];
   // return colors[category] || '#1890ff';
    return 'blue';

};
export const getRowColor = (severity: number): string => {
    if (40 <= severity && severity <= 80) return 'hover:bg-red-100';
    if (20 <= severity && severity <= 32) return 'hover:bg-orange-100';
    if (10 <= severity && severity <= 16) return 'hover:bg-yellow-100';
    if (3 <= severity && severity <= 8) return 'hover:bg-green-100';
    return 'hover:bg-cyan-100';
}