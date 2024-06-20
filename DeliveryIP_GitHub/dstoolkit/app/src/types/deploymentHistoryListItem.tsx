

export interface DeploymentHistoryListItem {
    id: string;
    partitionKey: string;
    repositoryName: string;
    repositoryUrl: string
    environment: string;
    runId: string;
    lastUpdatedBy: {
        displayName: string;
        on: Date;
        objectId: string;
        mail: string;
    }


}