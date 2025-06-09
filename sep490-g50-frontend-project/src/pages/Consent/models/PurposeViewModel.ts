export type PurposeViewModel = {
    id: string,
    name: string,
    description: string,
    status: string,
    createdAt: string,
    lastModifiedAt: string,
    createdById: string,
    lastModifiedById: string
}
export type SimplifiedPurposeViewModel = {
    id: string,
    name: string,
    //description: string,
    status?: string,
}
