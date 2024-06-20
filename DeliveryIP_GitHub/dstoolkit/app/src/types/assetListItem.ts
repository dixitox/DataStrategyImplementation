import { AssetType } from "./assetType";

export interface AssetListItem {
    id: string;
    partitionKey: string;
    name: string;
    assetType: AssetType[];
    tagline: string;
    industries: string[];
    tags: string[];
    order: number;
    importV1?: boolean;
    releasedOn: Date;
    authors: [
        {
            name: string;
            gitHubAlias?: string;
            gitHubAvatar?: string;
            linkedin?: string;
        }
    ];
    lastPushed?: Date;
    stargazers: number;
    subscribers: number;
    forks: number;
    lastChangedBy: {
        on: Date;
    };
}
