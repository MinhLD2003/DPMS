import AxiosClient from "../../../configs/axiosConfig";
import { ProfileModel } from "../models/ProfileModel";



export const profileService = {

  async getProfile(id: string) {
    try {
      const response = await AxiosClient.get<ProfileModel>(
        `/profile/${id}`
      );
      return {
        success: true,
        data: response.data,
      };
    } catch (error) {
      console.error("Error fetching profile", error);
      return {
        success: false,
        error
      };
    }
  }
};

export default profileService;