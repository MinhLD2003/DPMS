export type TicketModel = {
    id: string;
    title: string;
    systemName: string;
    category: string;
    description: string;
    attachment: string[];
    createdAt: string;
    lastModifiedAt: string;
    createdById: string;
    lastModifiedById: string;
}