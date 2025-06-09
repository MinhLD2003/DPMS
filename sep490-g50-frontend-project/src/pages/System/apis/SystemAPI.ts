import AxiosClient from "../../../configs/axiosConfig";
import { PagedResponse } from "../../../common/PagedResponse";
import { SystemModel } from "../models/SystemModel";
import { FilterParams } from "../../../common/FilterParams";
import { PostSystemModel } from "../models/PostSystemModel";



export const systemService = {
  async createSystem(systemData: PostSystemModel) {
    try {
      const response = await AxiosClient.post('/ExternalSystem', systemData);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error("Error creating System:", error);
      return {
        success: false,
        error
      };
    }
  },

  async getSystems(filterParams?: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<SystemModel>>(
        '/ExternalSystem',
        { params: filterParams }
      );
      return {
        success: true,
        objectList: response.data.data,
        totalCount: response.data.totalRecords,
        pageNumber: response.data.pageNumber,
        pageSize: response.data.pageSize
      };
    } catch (error) {
      console.error("Error fetching Systems:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        totalCount: 0,
        error
      };
    }
  },
  // async getSystemDetail(id: string) {
  //   try {
  //     const response = await AxiosClient.get<SystemModel>(
  //       `/System/${id}/detail`,
  //     );
  //     return {
  //       success: true,
  //       name: response.data.name,
  //       description: response.data.description,
  //     };
  //   } catch (error) {
  //     console.error("Error fetching system detail:", error);
  //     return {
  //       success: false,
  //       name: "",
  //       description: "",
  //       error
  //     };
  //   }
  // },

};

export default systemService;