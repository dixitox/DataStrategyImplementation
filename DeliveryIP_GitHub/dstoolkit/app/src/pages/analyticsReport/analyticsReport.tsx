import React from "react";
import { useTranslation } from "react-i18next";
import { Header } from "../../components/header/header";
import { HeaderBar, NavLocation } from "../../components/headerBar/headerBar";

export function AnalyticsReport() {
    const { t } = useTranslation();
    return (
        <>
            <Header>
                <HeaderBar location={NavLocation.AnalyticsReport} />
                <h1>{t("pages.analytics-report.title")}</h1>
            </Header>
            <main className="flex flex-col px-8 md:px-24">
                <iframe
                    src={window.ENV.ANALYTICS_REPORT_URL}
                    title={t("pages.analytics-report.title")}
                    className="min-h-[640px] flex-grow shadow"
                    frameBorder={0}
                    allowFullScreen={true}
                ></iframe>
            </main>
        </>
    );
}
