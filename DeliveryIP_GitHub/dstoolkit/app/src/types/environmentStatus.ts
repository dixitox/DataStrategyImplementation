export enum DeploymentStatus {
    None = "None",
    Requested = "Requested",
    Queued = "Queued",
    InProgress = "InProgress",
    Completed = "Completed",
    Failure = "Failure",
}
export interface EnvironmentStatus {
    sandbox: {
        isDeployable: boolean;
        isDeploying: boolean;
        lastStatus: DeploymentStatus;
    };
    production: {
        isDeployable: boolean;
        isDeploying: boolean;
        lastStatus: DeploymentStatus;
    };
}
