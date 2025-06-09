import AxiosClient from "../../../configs/axiosConfig";
import { PagedResponse } from "../../../common/PagedResponse";
import { ConsentDataViewModel } from "../models/ConsentLogViewModel";
import { PurposeViewModel, SimplifiedPurposeViewModel } from "../models/PurposeViewModel";
import { FilterParams } from "../../../common/FilterParams";
import { ConsentItem } from "../models/ConsentBannerViewModel";
import { ConsentPostModel } from "../models/ConsentPostModel";
import PurposeList from "../modules/PurposeList";



export const consentService = {
  async getConsentLogs(filterParams: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<ConsentDataViewModel>>(
        '/Consent/consent-log',
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
      console.error("Error fetching purposes:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        totalCount: 0,
        error
      };
    }
  },
  async getSystemConsentLogs(systemId: string, filterParams: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<ConsentDataViewModel>>(
        `/Consent/consent-log/${systemId}`,
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
      console.error("Error fetching purposes:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        totalCount: 0,
        error
      };
    }
  },
  async postConsent(consentData: ConsentPostModel) {
    try {
      const response = await AxiosClient.post('/Consent', consentData);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error("Error posting consent:", error);
      return {
        success: false,
        error
      };
    }
  },
  async getConsentBanner(uniqueIdentifier: string, token: string) {
    try {
      const response = await AxiosClient.get<ConsentItem[]>(
        `/Consent/get-banner/${uniqueIdentifier}/${token}`,
      );
      return {
        success: true,
        objectList: response.data,
      };
    } catch (error) {
      console.error("Error fetching banner items:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        error
      };
    }
  },
  async getPurposes(filterParams?: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<SimplifiedPurposeViewModel>>(
        '/Purpose',
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
      console.error("Error fetching purposes:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        totalCount: 0,
        error
      };
    }
  },
  async getSystemPurposes(systemId: string, filterParams?: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<PurposeViewModel>>(
        `/ExternalSystem/${systemId}/purposes`,
        { params: filterParams }
      );
      console.log("Response data:", response.data);
      return {
        success: true,
        purposeList: response.data,
      };
    } catch (error) {
      console.error("Error fetching purposes:", error);
      return {
        success: false,
        purposeList: [], 
      };
    }
  },

  //   async getConsentDetail(id: string) {
  //     try {
  //       const response = await AxiosClient.get<ConsentModel>(
  //         `/Consent/${id}/detail`,
  //       );
  //       return {
  //         success: true,
  //         name: response.data.name,
  //         description: response.data.description,
  //       };
  //     } catch (error) {
  //       console.error("Error fetching consent detail:", error);
  //       return {
  //         success: false,
  //         name: "",
  //         description: "",
  //         error
  //       };
  //     }
  //   },


};

export default consentService;