import { AssetListItem } from "./assetListItem";

export interface AssetManageItem extends AssetListItem {
    enabled: boolean;
    createdBy: {
        displayName: string;
        mail: string;
        objectId: string;
        on: Date;
    };
}
