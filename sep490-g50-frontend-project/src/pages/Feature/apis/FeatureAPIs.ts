import AxiosClient from "../../../configs/axiosConfig";
import { PagedResponse } from "../../../common/PagedResponse";
import { FilterParams } from "../../../common/FilterParams";
import { FeaturePutModel } from "../models/FeaturePutModel";
import { FeaturePostModel } from "../models/FeaturePostModel";
import { FeatureViewModel } from "../models/FeatureViewModel";
import { Children } from "react";



export const featureService = {
  async createFeature(featureData: FeaturePostModel) {
    try {
      const response = await AxiosClient.post('/Feature', featureData);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error("Error creating Feature:", error);
      return {
        success: false,
        error
      };
    }
  },

  async getFeatures(filterParams: FilterParams) {
    try {
      const response = await AxiosClient.get<PagedResponse<FeatureViewModel>>(
        '/Feature',
        { params: filterParams }
      );

      console.log("response", response);
      return {
        success: true,
        objectList: response.data.data,
        totalCount: response.data.totalRecords,
        pageNumber: response.data.pageNumber,
        pageSize: response.data.pageSize
      };
    } catch (error) {
      console.error("Error fetching Features:", error);
      return {
        success: false,
        objectList: [], // Always provide default values even in error case
        totalCount: 0,
        error
      };
    }
  },

  async getFeatureDetail(id: string) {
    try {
      const response = await AxiosClient.get<FeatureViewModel>(
        `/Feature/${id}`,
      );
      return {
        success: true,
        featureName: response.data.featureName,
        description: response.data.description,
        httpMethod: response.data.httpMethod,
        url: response.data.url,
        parentId: response.data.parentId,
        children: response.data.children
      };
    } catch (error) {
      console.error("Error fetching feature detail:", error);
      return {
        success: false,
        featureName: "",
        description: "",
        parentId:"",
        children:null,
        url: "",
        error,
      };
    }
  },

  async UpdateFeature(id: string, featureData: FeaturePutModel) {
    try {
      const featurePutData: FeaturePutModel = { ...featureData, id };
      const response = await AxiosClient.put<FeaturePutModel>(
        `/Feature/${id}`, featurePutData);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error("Error updating feature:", error);
      const errorMessage = "Failed to update feature";

      return {
        success: false,
        errorMessage
      };
    }
  },
};

export default featureService;