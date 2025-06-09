export type SystemModel = {
    id: string;
    name: string;
    domain: string;
    description: string;
    status: string,
    createdAt: string,
    createdBy: string,
}

export type Group = {
    id: string;
    name: string;
    description: string;
  }
  
  
export type TeamMember = {
    key: string;
    id?: string;
    no: number;
    fullName: string;
    email: string;
    groups: Group[];
    status: number;
    lastTimeLogin: string;
  }
export type SystemData = {
    id: string;
    name: string;
    domain: string;
    description: string;
    status: string;
    createdAt: string;
    createdBy: string;
    lastModifiedAt: string;
    lastModifiedBy: string;
    hasApiKey: boolean;
    users: TeamMember[];
    groups: Group[];
}
export type SearchUser = {
    id: string;
    fullName: string;
    groupName: string;
    email: string;
  }
  export type SystemUpdateStatusModel ={
    systemId: string;
    status: number;
  }