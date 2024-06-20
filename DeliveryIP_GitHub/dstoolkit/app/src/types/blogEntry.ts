import { BlogEntryStatus } from "./blogEntryStatus";
import { BlogEntryType } from "./blogEntryType";

export interface BlogEntry {
    id: string;
    title: string;
    heroImage: {
        path: string;
        alternativeText: string;
        base64?: string;
    };
    introduction: string;
    createdBy: {
        on: Date;
        displayName: string;
        objectId: string;
        mail: string;
    };
    lastChangedBy: {
        on: Date;
        displayName: string;
        objectId: string;
        mail: string;
    };
    entryType: BlogEntryType;
    status: BlogEntryStatus;
    body?: string; // only returned on detail API
    authors: 
        {
            name: string;
            gitHubAlias?: string;
            gitHubAvatar?: string;
            linkedin?: string;
        }[]
    ;
}
