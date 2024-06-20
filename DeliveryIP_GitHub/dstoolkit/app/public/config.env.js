window.ENV = {
    ENVIRONMENT: "__environment__",
    API_URL: "__api_url__",
    IMG_URL: "__img_url__",
    APP_INSIGHTS_CS: "__app_insights_cs__",
    AUTH: {
        clientId: "__auth_client_id__",
        authority: "https://__auth_instance__/__auth_tenant_id__",
        b2cPolicies: undefined,
        cacheLocation: "localStorage",
        knownAuthorities: ["__auth_instance__"],
        resources: {
            api: {
                endpoint: "",
                scopes: ["__auth_scope__"],
            },
        },
    },
    ACR: {
        groupJoinUrl: "__acr_groupjoinurl__",
        name: "__acr_name__",
    },
    ANALYTICS_REPORT_URL: "__analytics_report_url__",
    DEPLOYMENT_STATUS_REFRESH_MS: __deployment_status_refresh_ms__,
    RUN_FLAGS: [__run_flags__],
};
