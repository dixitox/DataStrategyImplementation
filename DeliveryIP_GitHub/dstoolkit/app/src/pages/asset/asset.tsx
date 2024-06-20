import React, { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { AssetItem } from "../../types/assetItem";
import { httpClient } from "../../utils/httpClient/httpClient";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { useMsal } from "@azure/msal-react";
import { Spinner } from "@fluentui/react-components";
import { HeaderBar } from "../../components/headerBar/headerBar";
import { SearchBox } from "../../components/searchBox/searchBox";
import { AssetSummary } from "./assetSummary";
import { Auth } from "../../utils/auth/auth";
import { RedirectRequest } from "@azure/msal-browser";
import { Header } from "../../components/header/header";

export function Asset() {
    const { t } = useTranslation();
    const [searchParams, setSearchParams] = useSearchParams();
    const params = useParams();
    const { accounts, instance } = useMsal();
    const [asset, setAsset] = useState<AssetItem>();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const navigate = useNavigate();

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    useEffectOnce(() => {
        if (searchParams.has("login") && !accounts.length) {
            setIsLoading(true);
            setTimeout(() => {
                setSearchParams({});
                // Force login if querystring contains login param (for teams integration)
                instance.loginRedirect(Auth.getAuthenticationRequest() as RedirectRequest);
            }, 0);
        } else if (params.assetId) loadDataAsync(params.assetId);
    });

    useEffect(() => {
        // Restore original page title on exit
        return () => {
            document.title = t("common.title");
        };
    }, []);

    async function loadDataAsync(assetId: string) {
        setIsLoading(true);
        const res: AssetItem = await httpClient.get(`${window.ENV.API_URL}/assets/${assetId}`);
        document.title = res.name;
        setAsset(res);
        setIsLoading(false);
    }

    function onSearchChanged(searchValue: string): void {
        if (searchValue) navigate(`/search?q=${encodeURIComponent(searchValue)}`);
    }

    function renderStringWithUrls(str: string): React.ReactNode[] {
        if (!str) return [""];
        const urlRegex = /(https?:\/\/[^\s]+)/g;
        const parts = str.split(urlRegex);
        return parts.map((part, index) => {
            if (part.match(urlRegex)) {
                return (
                    <a key={index} href={part} target="_blank" rel="noopener noreferrer">
                        {part}
                    </a>
                );
            } else {
                return <span key={index}>{part}</span>;
            }
        });
    }

    return (
        <>
            <Header>
                <HeaderBar />
                <SearchBox
                    className="items-center justify-end"
                    labelClassName="font-semibold"
                    inputClassName="w-40"
                    onSearchChanged={onSearchChanged}
                    placeholder={t("common.search")}
                />
            </Header>

            <main className="grid grid-cols-1 gap-4 px-8 md:grid-cols-4 md:gap-x-12 md:px-24">
                {/* Left nav bar */}
                <div className="whitespace-nowrap md:sticky md:top-5 md:self-start">
                    <div className="w-fit border-t-4 py-6 text-sm font-bold uppercase">
                        {t("pages.asset.asset-content")}
                    </div>
                    <div className="text-base font-semibold leading-8">
                        <a href="#business-problem">
                            <div>{t("entities.asset.business-problem")}</div>
                        </a>
                        <a href="#business-value">
                            <div>{t("entities.asset.business-value")}</div>
                        </a>
                        <a href="#accelerator-description">
                            <div>{t("entities.asset.accelerator-description")}</div>
                        </a>
                        <a href="#modeling-approach">
                            <div>{t("entities.asset.modeling-approach-and-training")}</div>
                        </a>
                        <a href="#data">
                            <div>{t("entities.asset.data")}</div>
                        </a>
                        <a href="#architecture">
                            <div>{t("entities.asset.architecture")}</div>
                        </a>
                    </div>
                </div>

                <div className="[&>p>a]:text-neutral-550 col-span-3">
                    {isLoading && (
                        <div className="mt-16">
                            <Spinner size="extra-large" />
                        </div>
                    )}
                    {/* Values */}
                    {!isLoading && asset && (
                        <>
                            <AssetSummary asset={asset} />

                            <h4 id="business-problem" className="pt-10 pb-5">
                                {t("entities.asset.business-problem")}
                            </h4>
                            <p className="whitespace-pre-line">{renderStringWithUrls(asset.businessProblem)}</p>

                            <h4 id="business-value" className="pt-10 pb-5">
                                {t("entities.asset.business-value")}
                            </h4>
                            <p className="whitespace-pre-line">{renderStringWithUrls(asset.businessValue)}</p>

                            <h4 id="accelerator-description" className="pt-10 pb-5">
                                {t("entities.asset.accelerator-description")}
                            </h4>
                            <p className="whitespace-pre-line">{renderStringWithUrls(asset.description)}</p>

                            {asset.screenshot?.path && (
                                <img
                                    src={`${window.ENV.IMG_URL}/${asset.screenshot.path}`}
                                    alt={asset.screenshot.alternativeText || asset.name}
                                    className="m-auto h-auto w-auto object-cover pt-10 pb-5"
                                />
                            )}

                            <h4 id="modeling-approach" className="pt-10 pb-5">
                                {t("entities.asset.modeling-approach-and-training")}
                            </h4>
                            <p className="whitespace-pre-line">
                                {renderStringWithUrls(asset.modelingApproachAndTraining)}
                            </p>

                            <h4 id="data" className="pt-10 pb-5">
                                {t("entities.asset.data")}
                            </h4>
                            <p className="whitespace-pre-line">{renderStringWithUrls(asset.data)}</p>

                            <h4 id="architecture" className="pt-10 pb-5">
                                {t("entities.asset.architecture")}
                            </h4>
                            <p className="whitespace-pre-line">{renderStringWithUrls(asset.architecture)}</p>

                            <div className="hidden">
                                <h4 className="pt-10 pb-5">{t("entities.asset.ml-type")}</h4>
                                {asset.tags?.map((s, i) => (
                                    <li key={i}>{s}</li>
                                ))}
                            </div>
                        </>
                    )}
                </div>
            </main>
        </>
    );
}
