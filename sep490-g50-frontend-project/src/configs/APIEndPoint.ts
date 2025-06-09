
const API_ENDPOINTS = {

  USERS: {
    GET_ALL: `/users`,
    GET_BY_ID: (id: string) => `/users/${id}`,
    UPDATE: (id: string) => `/users/${id}`,
    DELETE: (id: string) => `/users/${id}`,
  },

  TICKETS: {
    CREATE: `/IssueTicket`,
    GET_ALL: `/IssueTicket`,
    GET_BY_ID: (id: string) => `/IssueTicket/${id}`,
    UPDATE: (id: string) => `/IssueTicket/${id}`,
    DELETE: (id: string) => `/IssueTicket/${id}`,
    UPDATE_STATUS: (id: string) => `/IssueTicket/${id}/update-status`
  },
  SYSTEMS: {
    GET_ALL: `/systems`,
    GET_BY_ID: (id: string) => `/systems/${id}`,
    CREATE: `/systems`,
    UPDATE: (id: string) => `/systems/${id}`,
    DELETE: (id: string) => `/systems/${id}`,
  },
  PURPOSES: {
    GET_ALL: `/purpose`,
    GET_BY_ID: (id: string) => `/purpose/${id}`,
    CREATE: `/purpose`,
    UPDATE: (id: string) => `/purpose/${id}`,
    DELETE: (id: string) => `/purpose/${id}`,
  },
  FEATURES: {
    GET_ALL :`Feature`,
    GET_LIST_FEATURES:(id :string) =>`/Feature/get-list-features/${id}`,
    GET_BY_ID:(id :string) => `/Feature/${id}`,
    ADD_FEATURES_TO_GROUP: `/Feature/add-feature-to-group`
  },
  GROUPS: {
    GET_ALL :`Group`,
    GET_BY_ID:(id :string) => `Group/${id}`,
  }
};

export default API_ENDPOINTS;
