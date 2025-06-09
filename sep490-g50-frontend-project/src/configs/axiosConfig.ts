import axios from "axios";
import { validateEnv } from "./env.config";
const AxiosClient = axios.create({
  baseURL: validateEnv().apiBaseUrl,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add a request interceptor
AxiosClient.interceptors.request.use(
  function (config) {
    // Do something before request is sent
    const accessToken = localStorage.getItem('jwt');
    if (accessToken !== "") {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  function (error) {
    // Do something with request error
    return Promise.reject(error);
  }
);

// Add a response interceptor
AxiosClient.interceptors.response.use(
  function (response) {
    return response;
  },
  function (error) {
    //const authToken = error.config?.headers?.Authorization || "No Token Found";

    const errorResponse = {
      message: error.message,
      apiErrorMessage: error.response?.data?.message,
      apiSingleErrorMessage: error.response,
      status: error.response?.status,
      data: error.response?.data,
    };
    console.error("Axios Error:", errorResponse);

    return Promise.reject(errorResponse);
  }
);

export default AxiosClient;
