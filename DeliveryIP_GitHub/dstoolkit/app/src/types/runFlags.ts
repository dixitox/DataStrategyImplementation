export enum RunFlags {
    DsToolkitNav = "dstoolkit-nav", // DsToolkit nav item
    NoFilters = "no-filters",
    NoFiltersIndustries = "no-filters-ind",
    NoFiltersMachineLearning = "no-filters-ml",
    NoFiltersAssetType = "no-filters-assettype",
    NoDemoHosting = "no-demo-hosting",
    WorkflowsEnabled = "workflows-enabled",
    AllowAnonymousAccess = "allow-anonymous-access", // anonymous users are considered as consumers
    LoggedInAsConsumer = "logged-in-as-consumer", // if anonymous users is disabled and this flat is enabled, all logged in users are considered consumer despite of their role
    BlogEnabled = "blog-enabled",
    CookieConsentEnabled = "cookies-consents-enabled"
}
