import React from "react";
import { useTranslation } from "react-i18next";
import { InfoPopover } from "../../components/infoPopover/infoPopover";

export function AcrInfo({ className }: { className?: string }) {
    const { t } = useTranslation();

    return (
        <InfoPopover className={className}>
            <div>
                <h4>{t("components.acr-info.title")}</h4>
                {t("components.acr-info.intro")
                    .split("\n")
                    .map((o, idx, a) => (
                        <p className="mt-2" key={idx}>
                            {o}
                            {idx === a.length - 1 ? (
                                <>
                                    <a
                                        href={window.ENV?.ACR.groupJoinUrl}
                                        target="_blank"
                                        rel="noreferrer"
                                        className="ml-1 inline-block"
                                    >
                                        {t("components.acr-info.intro-link")}
                                    </a>
                                    .
                                </>
                            ) : null}
                        </p>
                    ))}
                <p className="mt-2">{t("components.acr-info.body")}</p>

                <pre className="whitespace-pre-line pl-4">
                    <p className="pt-2">az login</p>
                    <p className="pt-2">
                        $TOKEN=$(az acr login --name &#34;{window.ENV?.ACR.name}&#34; --expose-token --output tsv
                        --query accessToken)
                    </p>
                    <p className="pt-2">
                        docker login {window.ENV?.ACR.name}.azurecr.io --username 00000000-0000-0000-0000-000000000000
                        --password $TOKEN
                    </p>
                    <p className="pt-2">
                        docker build . -t {window.ENV?.ACR.name}.azurecr.io/demos/&lt;your-image-name&gt;:latest
                    </p>
                    <p className="pt-2">
                        docker push {window.ENV?.ACR.name}.azurecr.io/demos/&lt;your-image-name&gt;:latest
                    </p>
                </pre>
                <p className="mt-4">{t("components.acr-info.footer")}</p>
            </div>
        </InfoPopover>
    );
}
