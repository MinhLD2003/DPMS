export type Purpose = {
    name: string;
    status: boolean;
}
export type ConsentDataViewModel = {
    dataSubjectId: string;
    email: string;
    consentMethod: number;
    consentIp: string;
    consentUserAgent: string;
    externalSystemName: string;
    isWithdrawn: boolean;
    consentDate: string;
    withdrawnDate: string;
    consentPurpose: Purpose[];
}
