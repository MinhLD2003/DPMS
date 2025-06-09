
export enum UserStatus {
    Active = "Active",
    Inactive = "Inactive",
    Activated = "Activated",
    Deactivated = "Deactivated",
}
export enum NumberedUserStatus {
    // Active = "Active",
    // Inactive = "Inactive",
    Activated = 1,
    Deactivated = 0,
}
export enum TicketType {
    DPIA = "DPIA",
    Risk = "Risk",
    Violation = "Violation",
    System = "System",
}
export enum IssueTicketStatus {
    Pending = "Pending",
    Accept = "Accept",
    Reject = "Reject",
    Done = "Done"
}
export enum HttpMethodType {
    GET = "GET",
    POST = "POST",
    PUT = "PUT",
    DELETE = "DELETE",
    PATCH = "PATCH"
}

export enum FormStatus {
    Draft = 1,
    Activated = 2,
    Deactivated = 3
}
export enum FeatureStatus {
    Active = "Active",
    Inactive = "Inactive"
}
export enum FormType {
    FIC,
    //To be khong tinh yeu
}
export enum PolicyStatus {
    Active = "Active",
    Deactivated = "Deactivated",
    Draft = "Draft",
    Inactive = "Inactive",
}
export enum DSARType {
    Access,
    Delete,
}
export enum DSARStatus {
    Submitted,
    RequiredReponse,
    Completed,
    Rejected
}
export enum RiskCategory {
    Technical = 0,
    Organizational = 1,
    Scope = 2,
    Schedule = 3,
    Usability = 4,
    Communication = 5,
    Quality = 6,
}

export enum ResponseStrategy {
    Mitigate = 0,
    Prevent = 1,
    Transfer = 2,
    Acceptance = 3,
    Exploitation = 4,
}
export enum ExternalSystemStatus {
    WaitingForFIC = 1,
    WaitingForDPIA = 2,
    DPIACreated = 3,
    Activated = 4,
    Deactivated = 5
}
export enum ExternalSystemStatusText {
    WaitingForFIC = "WaitingForFIC",
    WaitingForDPIA = "WaitingForDPIA",
    DPIACreated = "DPIACreated",
    Activated = "Active",
    Deactivated = "Inactive"
}
