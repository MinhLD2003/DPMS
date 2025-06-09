export type ConsentPostModel = {
    uniqueIdentifier: string,
    tokenString: string,
    consentPurposes: PurposeValue[],
}
export type PurposeValue = {
    purposeId: string,
    status: boolean,
}