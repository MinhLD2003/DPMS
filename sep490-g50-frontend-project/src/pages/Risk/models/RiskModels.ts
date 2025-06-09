export const categoryOptions = [
    'Technical', 'Organizational', 'Scope', 'Schedule', 'Usability', 'Communication', 'Quality'
];

export const strategyOptions = ['Mitigate', 'Prevent', 'Transfer', 'Acceptance', 'Exploitation'];

export const impactOptions = [1, 2, 4, 8, 16];
export const probabilityOptions = [1, 2, 3, 4, 5];
export type RiskAssessPutModel = {
    riskImpactAfterMitigation: number;
    riskProbabilityAfterMitigation: number;
}
export type RiskBeforeAssessModel = {
    id: string;
    riskImpact: number;
    riskProbability: number;
}