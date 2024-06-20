import { AssetListItem } from "./assetListItem";

export interface AssetItem extends AssetListItem {
    demoUrl?: string;
    acrRepositoryName?: string;
    armTemplate: string;
    workflowUrl?: string;
    requireHosting: boolean;
    businessProblem: string;
    businessValue: string;
    description: string;
    modelingApproachAndTraining: string;
    data: string;
    architecture: string;
    screenshot?: {
        path: string;
        alternativeText?: string;
        base64?: string;
    };
    repositoryUrl: string;
    enabled: boolean;
    createdBy?: {
        displayName: string;
        mail: string;
        objectId: string;
        on: Date;
    };
    comments?: string;
}
