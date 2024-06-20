import React, { MouseEventHandler } from "react";
import { useTranslation } from "react-i18next";
import { RunFlags } from "../../types/runFlags";

export function Footer() {
    const { t } = useTranslation();

    return (
        <footer className="mt-20 w-full bg-neutral-50">
            {/* Parent is a full width container */}
            {/* Child is centered and has max width */}
            <ul className="_max-content-width mx-auto flex flex-wrap justify-end gap-6 whitespace-nowrap p-8 text-[12px] text-neutral-700 md:px-24">
                <li>
                    <a href="https://support.microsoft.com/contactus" target="_blank" rel="noreferrer">
                        {t("components.footer.contact")}
                    </a>
                </li>
                <li>
                    <a href="https://go.microsoft.com/fwlink/?LinkId=521839" target="_blank" rel="noreferrer">
                        {t("components.footer.privacy")}
                    </a>
                </li>
                {window.ENV?.RUN_FLAGS.includes(RunFlags.CookieConsentEnabled) ? (
                    <li>
                        <span
                            role="link"
                            onClick={() => window.WcpConsent.siteConsent.manageConsent()}
                            className="cursor-pointer hover:underline"
                            id="manage-cookies-link"
                        >
                            {t("components.footer.manage-cookies")}
                        </span>
                    </li>
                ) : null}
                <li>
                    <a href="https://privacy.microsoft.com/en-us/data-privacy-notice" target="_blank" rel="noreferrer">
                        {t("components.footer.global-privacy")}
                    </a>
                </li>
                <li>
                    <a href="https://go.microsoft.com/fwlink/?linkid=2196228" target="_blank" rel="noreferrer">
                        {t("components.footer.trademarks")}
                    </a>
                </li>
                <li>
                    <a href="https://go.microsoft.com/fwlink/?LinkID=206977" target="_blank" rel="noreferrer">
                        {t("components.footer.terms-of-use")}
                    </a>
                </li>
                <li>{t("components.footer.copyright")}</li>
            </ul>
        </footer>
    );
}
