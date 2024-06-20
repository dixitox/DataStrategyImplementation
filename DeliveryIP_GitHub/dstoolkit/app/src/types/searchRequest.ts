import { SortCriteria } from "./sortCriteria";

export interface SearchRequest {
    search: string;
    filters: {
        industry?: string[];
        tags?: string[];
        assetType?: string[];
    };
    sortBy?: SortCriteria;
    pageSize?: number;
    page: number;
}
