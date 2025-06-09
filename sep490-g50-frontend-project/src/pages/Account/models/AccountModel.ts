export type AccountModel = {
id: string;
userName: string;
fullName: string;
email: string;
status: number;
createdAt: string;
groups: GroupVM[];


// lastModifiedAt: string;
// createdById: string;
// lastModifiedById: string;
};
export type GroupVM ={
    id: string;
    name: string;
    description: string;
}