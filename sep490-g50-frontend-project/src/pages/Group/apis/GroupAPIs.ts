import AxiosClient from "../../../configs/axiosConfig";
import { PagedResponse } from "../../../common/PagedResponse";
import { GroupModel } from "../models/GroupModel";
import { FilterParams } from "../../../common/FilterParams";
import { PostGroupModel } from "../models/PostGroupModel";



export const groupService = {
  async createGroup(groupData: PostGroupModel) {
    try {
      const response = await AxiosClient.post('/Group', groupData);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error("Error creating Group:", error);
      return {
        success: false,
        error
      };
    }
  },

  async getGroups(filterParams: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<GroupModel>>(
        '/Group',
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
      console.error("Error fetching Groups:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        totalCount: 0,
        error
      };
    }
  },
  async getGroupDetail(id: string) {
    try {
      const response = await AxiosClient.get<GroupModel>(
        `/Group/${id}/detail`,
      );
      return {
        success: true,
        name: response.data.name,
        description: response.data.description,
      };
    } catch (error) {
      console.error("Error fetching group detail:", error);
      return {
        success: false,
        name: "",
        description: "",
        error
      };
    }
  },
  async updateGroup(id: string, data: { name: string; description: string }) {
    try {
      const response = await AxiosClient.put<GroupModel>(
        `/Group/${id}`,
        data
      );
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error("Error updating group:", error);
      const errorMessage = "Failed to update group";

      return {
        success: false,
        errorMessage
      };
    }
  },
  addUsersToGroup: async (groupId: string, userIds: string[]) => {
    try {
      await AxiosClient.post(`/Group/add-user-to-group?groupId=${groupId}`, userIds);
      return { success: true };
    } catch (error) {
      console.error('Failed to add users to group:', error);
      return { success: false, error };
    }
  }
};

export default groupService;