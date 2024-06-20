import { Telemetry } from "../../utils/telemetry/telemetry";
import { RunFlags } from "../../types/runFlags";
import "./cookiePolicyBanner.scss";

export function wcpCookieConsentInit() {    
    if (!window.ENV?.RUN_FLAGS.includes(RunFlags.CookieConsentEnabled)) return;

    window.WcpConsent && window.WcpConsent.init(navigator.language, "cookie-banner", null, null, window.WcpConsent.themes.light);

    const checkConsents = (categoryPreferences : any) => {
        if(categoryPreferences.Analytics)
            Telemetry.enableAppInsights()
        else
            Telemetry.disableAppInsights();
    };

    window.WcpConsent.onConsentChanged(checkConsents);
}

export function getCookiePolicyUserChoice(): boolean {
    return !window.ENV?.RUN_FLAGS.includes(RunFlags.CookieConsentEnabled) || (!window.WcpConsent?.wcpCookie?.consentId) || window.WcpConsent.siteConsent.getConsentFor("Analytics");
}

