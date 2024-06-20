window.ENV = {
    ENVIRONMENT: "local",
    API_URL: "https://web-dstoolkit-api-we-dev.azurewebsites.net", // "http://localhost:4001" "https://localhost:7078",
    IMG_URL: "https://stdstoolkitwedev.blob.core.windows.net/images",
    APP_INSIGHTS_CS:
        "InstrumentationKey=7fb415bf-5d24-4c25-a375-c04ce11bde62;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/",
    AUTH: {
        clientId: "38e2cb9a-b594-4a01-a930-7b271c2cbe78",
        authority: "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47",
        b2cPolicies: undefined,
        cacheLocation: "localStorage",
        knownAuthorities: ["login.microsoftonline.com"],
        resources: {
            api: {
                endpoint: "",
                scopes: ["api://38e2cb9a-b594-4a01-a930-7b271c2cbe78/api.access"],
            },
        },
    },
    ACR: {
        groupJoinUrl:
            "https://idwebelements.microsoft.com/GroupManagement.aspx?Group=dstoolkit-acr-access-dev&Operation=join",
        name: "craigallerydev001",
    },
    ANALYTICS_REPORT_URL:
        "https://msit.powerbi.com/reportEmbed?reportId=3c61be50-bf25-40fc-9653-dae47fd90c7d&autoAuth=true&ctid=72f988bf-86f1-41af-91ab-2d7cd011db47",
    DEPLOYMENT_STATUS_REFRESH_MS: 3000,
    RUN_FLAGS: ["_allow-anonymous-access", "logged-in-as-consumer", "_no-filters", "_no-filters-ind", "no-filters-ml", "_no-filters-assettype", "_dstoolkit-nav", "_no-demo-hosting", "workflows-enabled", "blog-enabled", "cookies-consents-enabled"],
};
