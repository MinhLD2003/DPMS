import AxiosClient from "../../../configs/axiosConfig";
import { PagedResponse } from "../../../common/PagedResponse";
import { AccountModel } from "../models/AccountModel";
import { FilterParams } from "../../../common/FilterParams";
import { PostAccountModel } from "../models/PostAccountModel";
import { DeactivateModel } from "../models/DeactivateModel";



export const accountService = {
  async createAccount(accountData: PostAccountModel) {
    try {
      const response = await AxiosClient.post('/Account', accountData);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error("Error creating account:", error);
      return {
        success: false,
        error
      };
    }
  },

  async getAccounts(filterParams: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<AccountModel>>(
        '/Account',
        { params: filterParams }
      );
      return {
        success: true,
        objectList: response.data.data,
        totalRecords: response.data.totalRecords,
        pageNumber: response.data.pageNumber,
        pageSize: response.data.pageSize,
      };
    } catch (error) {
      console.error("Error fetching accounts:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        totalRecords: 0,
        error
      };
    }
  },
  async deactivate(payload: DeactivateModel) {
    try {
      const response = await AxiosClient.put(
        '/User/update-user-status', payload
      );
      return {
        success: true,
      };
    } catch (error) {
      return {
        success: false,
        error
      };
    }
  }

};

export default accountService;