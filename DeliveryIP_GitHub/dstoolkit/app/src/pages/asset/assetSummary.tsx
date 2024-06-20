import React, { useState, useEffect, useRef } from "react";
import { Avatar, Button, Tooltip, ProgressBar } from "@fluentui/react-components";
import { Field } from "@fluentui/react-components";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { AssetItem } from "../../types/assetItem";
import { getIndustriesColors, getIndustryColor } from "../../utils/industryColors/industryColors";
import { GitHubIcon } from "../../assets/icons/gitHubLogoIcon";
import { WindowNewRegular, EditRegular } from "@fluentui/react-icons";
import { AzureIcon } from "../../assets/icons/azureIcon";
import { isPlatformProducer, isPlatformAdmin } from "../../utils/auth/roles";
import { RunFlags } from "../../types/runFlags";
import { httpClient } from "../../utils/httpClient/httpClient";
import { EnvironmentStatus, DeploymentStatus } from "../../types/environmentStatus";

export function AssetSummary({ asset }: { asset: AssetItem }) {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const { accounts } = useMsal();
    const [isProcessing, setIsProcessing] = useState<boolean>(false);
    const [environmentStatus, setEnvironmentStatus] = useState<EnvironmentStatus>();
    const isAdmin = isPlatformAdmin(accounts);
    const isProducer = isPlatformProducer(accounts);
    const showDeploymentsFeatures =
        window.ENV?.RUN_FLAGS.includes(RunFlags.WorkflowsEnabled) &&
        asset.workflowUrl &&
        asset.enabled &&
        (isProducer || isAdmin);

    useEffect(() => {
        loadDataAsync();
    }, [asset]);

    function canEdit(): boolean {
        if (accounts.length === 0) return false;

        return (isProducer && asset.createdBy?.objectId === accounts[0].localAccountId) || isAdmin;
    }

    function validationState(envStatus: any) {
        if (envStatus?.isDeploying) return "warning";
        if (envStatus?.lastStatus == DeploymentStatus.Completed) return "success";
        if (envStatus?.lastStatus == DeploymentStatus.Failure) return "error";
        return undefined;
    }

    const timerRef = useRef(0);
    async function loadDataAsync() {
        setIsProcessing(true);
        // Read the environment status if deployment workflows are enabled
        if (showDeploymentsFeatures) {
            const res: EnvironmentStatus = await httpClient.get(
                `${window.ENV.API_URL}/asset/${asset.id}/environmentsstatus`
            );
            setEnvironmentStatus(res);
            if (res.sandbox.isDeploying || res.production.isDeploying)
                timerRef.current = window.setTimeout(() => loadDataAsync(), window.ENV.DEPLOYMENT_STATUS_REFRESH_MS);
            else setIsProcessing(false);
        }
    }

    async function deploy(env: "sandbox" | "production") {
        setIsProcessing(true);
        await httpClient.post(`${window.ENV.API_URL}/asset/${asset.id}/workflowdispatch/${env}`);
        timerRef.current = window.setTimeout(() => loadDataAsync(), 1000);
    }

    return (
        <>
            <div className="flex flex-grow flex-col rounded-b-xl bg-white py-5 pl-7 pr-4 shadow-lg">
                <div className="-mt-5 -mr-4 -ml-7 h-1" style={{ background: getIndustriesColors(asset.industries) }} />
                <div className="flex flex-wrap items-center gap-y-3 pt-6 text-sm font-bold uppercase tracking-wider">
                    {asset.industries.map((industry, idx) => (
                        <div key={idx}>
                            {idx > 0 && <span className="mx-1">•</span>}
                            <span style={{ color: getIndustryColor(industry) }}>{industry}</span>
                        </div>
                    ))}
                </div>
                <h4 className="py-5">{asset.name}</h4>

                <div className="flex flex-wrap gap-2.5">
                    {canEdit() && (
                        <div>
                            <Button
                                appearance="secondary"
                                icon={<EditRegular />}
                                onClick={() => navigate(`/editor/${asset.id}`)}
                            >
                                {t("common.edit")}
                            </Button>
                        </div>
                    )}
                    {asset.repositoryUrl && (
                        <div>
                            <Button
                                appearance="primary"
                                as="a"
                                href={asset.repositoryUrl}
                                icon={<GitHubIcon className="h-4 w-4 fill-white" />}
                                target="_blank"
                            >
                                {t("pages.asset.source-button")}
                            </Button>
                        </div>
                    )}
                    {asset.demoUrl && (
                        <div>
                            <Button
                                appearance="primary"
                                as="a"
                                href={asset.demoUrl}
                                icon={<WindowNewRegular />}
                                target="_blank"
                            >
                                {t("entities.asset.demo")}
                            </Button>
                        </div>
                    )}
                    {asset.armTemplate && (
                        <div>
                            <Button
                                appearance="primary"
                                as="a"
                                href={asset.armTemplate}
                                icon={<AzureIcon className="h-4 w-4 fill-white" />}
                                target="_blank"
                            >
                                {t("entities.asset.deploy")}
                            </Button>
                        </div>
                    )}
                    {showDeploymentsFeatures && (
                        <div>
                            <Button
                                appearance="primary"
                                icon={<AzureIcon className="h-4 w-4 fill-white" />}
                                onClick={() => deploy("sandbox")}
                                disabled={isProcessing || !environmentStatus?.sandbox.isDeployable}
                            >
                                {t("pages.asset.deploy-to-sandbox")}
                            </Button>
                            {environmentStatus?.sandbox.lastStatus != DeploymentStatus.None && (
                                <Field
                                    style={{ marginTop: "10px" }}
                                    validationMessage={environmentStatus?.sandbox.lastStatus}
                                    validationState={validationState(environmentStatus?.sandbox)}
                                >
                                    <ProgressBar
                                        title="sandbox progress"
                                        value={environmentStatus?.sandbox.isDeploying ? undefined : 100}
                                        color={validationState(environmentStatus?.sandbox)}
                                    />
                                </Field>
                            )}
                        </div>
                    )}
                    {showDeploymentsFeatures && (
                        <div>
                            <Button
                                appearance="primary"
                                icon={<AzureIcon className="h-4 w-4 fill-white" />}
                                onClick={() => deploy("production")}
                                disabled={isProcessing || !environmentStatus?.production.isDeployable}
                            >
                                {t("pages.asset.deploy-to-prod")}
                            </Button>
                            {environmentStatus?.production.lastStatus != DeploymentStatus.None && (
                                <Field
                                    style={{ marginTop: "10px" }}
                                    validationMessage={environmentStatus?.production.lastStatus}
                                    validationState={validationState(environmentStatus?.production)}
                                >
                                    <ProgressBar
                                        title="production progress"
                                        value={environmentStatus?.production.isDeploying ? undefined : 100}
                                        color={validationState(environmentStatus?.production)}
                                    />
                                </Field>
                            )}
                        </div>
                    )}
                </div>
                <div className="mt-5 flex items-center border-y border-y-neutral-300 py-5 text-[12px] text-black">
                    <span className="mr-2">{t("entities.asset.author", { count: asset.authors.length })}:</span>
                    {asset.authors.map((author, idx) => (
                        <Tooltip key={idx} content={author.name || author.gitHubAlias || "-"} relationship="label">
                            <a
                                href={`https://github.com/${author.gitHubAlias}`}
                                target="_blank"
                                rel="noopener noreferrer"
                            >
                                <Avatar
                                    className="mr-1"
                                    size={24}
                                    name={author.name || author.gitHubAlias}
                                    color="colorful"
                                    image={author.gitHubAvatar ? { src: author.gitHubAvatar } : undefined}
                                />
                            </a>
                        </Tooltip>
                    ))}
                    {asset.authors.length === 1 && (
                        <span className="ml-1 font-normal text-neutral-700">{asset.authors[0].name}</span>
                    )}
                </div>
                <p className="pt-5">{asset.tagline}</p>
                <div className="flex flex-wrap items-center gap-y-3 pt-6 text-[12px] text-neutral-700">
                    <div className="mr-2.5 font-semibold text-black">{t("entities.asset.tags")}:</div>
                    {asset.tags.map((tag, idx) => (
                        <div key={idx} className="mr-2.5 rounded-sm bg-neutral-100 px-2 py-1">
                            {tag}
                        </div>
                    ))}
                </div>
            </div>
        </>
    );
}
