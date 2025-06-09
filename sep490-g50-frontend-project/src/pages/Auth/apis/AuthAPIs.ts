import AxiosClient from "../../../configs/axiosConfig";
import { LoginPayload } from "../models/LoginPayload";

export interface LoginResponse {
  token: string;
}

export const authService = {
  login: async (credentials: LoginPayload): Promise<LoginResponse> => {
    try {
      const response = await AxiosClient.post<LoginResponse>(`/Auth/login`, credentials);
      return response.data;
    } catch (error) {
      console.error("Login failed:", error);
      throw error;
    }
  },

  // Add other auth-related API calls here
  forgotPassword: async (email: string) => {
    try {
      const response = await AxiosClient.post('/Forgotpassword/forgot-password', { email });
      return { success: true, data: response.data };
    } catch (error) {
      console.error("Forgot password request failed:", error);
      throw error;
    }
  },
  verify: async (token: string) => {
    try {
      const response = await AxiosClient.get(`/Forgotpassword/verify?token=${token}`);
      return { success: true, data: response.data };
    } catch (error) {
      console.error("Forgot password request failed:", error);
      throw error;
    }
  },
  setPassword: async (token: string, newPassword: string) => {
    try {
      const response = await AxiosClient.post(`/Forgotpassword/set-new-password`, { token, newPassword });
      return { success: true, data: response.data };
    } catch (error) {
      console.error("Forgot password request failed:", error);
      throw error;
    }
  },


  googleLogin: async (token: string) => {
    try {
      const response = await AxiosClient.post('/Auth/google-login', { token });
      return response.data;
    } catch (error) {
      console.error("Google login failed:", error);
      throw error;
    }
  }
};