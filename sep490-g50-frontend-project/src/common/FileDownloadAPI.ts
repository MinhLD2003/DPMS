import AxiosClient from "../configs/axiosConfig";

export const downloadFileFromApi = async (apiPath: string): Promise<Blob> => {
    const response = await AxiosClient.get(apiPath, {
      responseType: 'blob',
    });
  
    return new Blob([response.data]);
  };