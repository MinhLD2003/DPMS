
export type FeatureViewModel = {
    id: string;
    featureName: string;
    url: string;
    description: string;
    httpMethod: number;
    parentId:string;
    children?: FeatureViewModel[]
}